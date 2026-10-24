// Galactic Council of the Academy - state machine, voting, tally, persistence.
// Pure C# (no game dependencies) so it can be unit-tested with Mono (see tests/).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public sealed class CouncilActiveLaw
{
    public string Id;
    public int Target;     // empire index, -1 for laws that apply to everyone
    public int StartTurn;
}

public sealed class CouncilResult
{
    public int Turn;
    public string LawId;
    public int Target;     // winning empire (targeted) or -1
    public bool Passed;
    public int Yes;        // global: votes "for"; targeted: votes of the winner
    public int No;         // global: votes "against"; targeted: votes of the runner-up
}

/// <summary>Deterministic xorshift RNG (identical on every machine).</summary>
public sealed class CouncilRng
{
    uint s;
    public CouncilRng(int seed) { s = (uint)(seed * 2654435761u + 0x9E3779B9u); if (s == 0) s = 0x1234567u; for (int i = 0; i < 4; i++) Next(); }
    public uint Next() { s ^= s << 13; s ^= s >> 17; s ^= s << 5; return s; }
    public int Range(int n) { return n <= 0 ? 0 : (int)(Next() % (uint)n); }
}

public sealed partial class CouncilCore
{
    public const int BaseVotes = 100;
    public const int VoteStep = 10;
    public const int MaxQuestions = 3;
    public const int BaseIntervalTurns = 20;
    public const int HistoryLimit = 12;

    public const int PhaseIdle = 0;
    public const int PhaseVoting = 1;

    public const int ActionNone = 0;
    public const int ActionTally = 1;
    public const int ActionOpen = 2;

    public bool Enabled;
    public bool Destroyed;
    public int FoundedTurn;
    public int VoteIndex;
    public int Phase;
    public int VoteOpenTurn;
    public int NextVoteTurn;
    public int IntervalTurns = BaseIntervalTurns;

    public List<string> Proposals = new List<string>();
    public List<string> LastProposals = new List<string>();
    public List<int> Members = new List<int>();
    public Dictionary<int, List<int[]>> Ballots = new Dictionary<int, List<int[]>>();   // empire -> [question, option, votes]
    public List<CouncilActiveLaw> Active = new List<CouncilActiveLaw>();
    public Dictionary<int, int> VoteBonus = new Dictionary<int, int>();
    public Dictionary<int, int> ColonyDelta = new Dictionary<int, int>();
    public List<CouncilResult> History = new List<CouncilResult>();

    // ------------------------------------------------------------------ queries

    public bool IsMember(int emp) { return Members.Contains(emp); }

    /// <summary>Votes of an empire: see CouncilVotes.cs for the sources.</summary>
    public int Budget(int emp)
    {
        int b = BaseVotes;
        int bonus;
        if (VoteBonus.TryGetValue(emp, out bonus)) b += bonus;
        b += PresenceBonusOf(emp);
        b += SessionVotesOf(emp);              // system levels, Republic, senator skills, minus the war penalty
        b += MandateVotesOf(emp);              // +50 for every ordinary mandate
        if (b < 0) b = 0;
        if (Holds(emp, CouncilLaws.Supreme)) b = b * 2 + SupremeMandateVotes;   // supreme mandate: doubles the votes, +100
        return b;
    }

    public int Spent(int emp)
    {
        List<int[]> b;
        if (!Ballots.TryGetValue(emp, out b)) return 0;
        int s = 0;
        for (int i = 0; i < b.Count; i++) s += b[i][2];
        return s;
    }

    public int GetVotes(int emp, int question, int option)
    {
        List<int[]> b;
        if (!Ballots.TryGetValue(emp, out b)) return 0;
        for (int i = 0; i < b.Count; i++) if (b[i][0] == question && b[i][1] == option) return b[i][2];
        return 0;
    }

    public bool HasVoted(int emp) { return Spent(emp) > 0; }

    public bool IsLawActive(string id)
    {
        for (int i = 0; i < Active.Count; i++) if (Active[i].Id == id) return true;
        return false;
    }

    public bool IsLawActiveFor(string id, int emp)
    {
        for (int i = 0; i < Active.Count; i++) if (Active[i].Id == id && (Active[i].Target == emp || Active[i].Target < 0)) return true;
        return false;
    }

    public int TurnsToNextVote(int turn) { return Math.Max(0, NextVoteTurn - turn); }

    public int GetColonyDelta(int emp) { int v; return ColonyDelta.TryGetValue(emp, out v) ? v : 0; }

    // ------------------------------------------------------------------ lifecycle

    public void Found(int turn, IList<int> members, int interval)
    {
        Enabled = true; Destroyed = false;
        FoundedTurn = turn; VoteIndex = 0; Phase = PhaseIdle;
        IntervalTurns = Math.Max(1, interval);
        NextVoteTurn = turn;
        Members.Clear(); for (int i = 0; i < members.Count; i++) Members.Add(members[i]);
        Proposals.Clear(); LastProposals.Clear(); Ballots.Clear(); Active.Clear();
        VoteBonus.Clear(); ColonyDelta.Clear(); History.Clear();
        ResetMandates();
        VictoryTurns = Math.Max(1, (int)Math.Round(VictoryBaseTurns * IntervalTurns / (double)BaseIntervalTurns));
    }

    public void Disband()
    {
        Enabled = false; Destroyed = true; Phase = PhaseIdle;
        Proposals.Clear(); Ballots.Clear(); Active.Clear();
        ResetMandates();
    }

    public void RemoveMember(int emp)
    {
        Members.Remove(emp);
        Ballots.Remove(emp);
        VoteBonus.Remove(emp);
        PresenceBonus.Remove(emp);
        ColonyDelta.Remove(emp);
        for (int i = Active.Count - 1; i >= 0; i--) if (Active[i].Target == emp) Active.RemoveAt(i);
        RemoveHoldsOf(emp);
        MandateCandidates.Remove(emp);
    }

    /// <summary>What has to be done at the beginning of the given turn.</summary>
    public int NextAction(int turn)
    {
        if (!Enabled || Destroyed) return ActionNone;
        if (Phase == PhaseVoting) return turn >= VoteOpenTurn + 1 ? ActionTally : ActionNone;
        return turn >= NextVoteTurn ? ActionOpen : ActionNone;
    }

    /// <summary>Removes cycle laws; returns the removed ones so the caller can undo their effects.</summary>
    public List<CouncilActiveLaw> ExpireLaws()
    {
        var removed = new List<CouncilActiveLaw>(Active);
        Active.Clear();
        return removed;
    }

    public void OpenVote(int turn, IList<string> proposals)
    {
        LastProposals = new List<string>(Proposals);
        Proposals.Clear();
        for (int i = 0; i < proposals.Count && i < MaxQuestions; i++) Proposals.Add(proposals[i]);
        Ballots.Clear();
        Phase = PhaseVoting;
        VoteOpenTurn = turn;
        VoteIndex++;
        NextVoteTurn = turn + IntervalTurns;
    }

    // ------------------------------------------------------------------ proposals

    public List<string> PickProposals(int seed, int count, IList<string> forced)
    {
        var rng = new CouncilRng(seed);
        var res = new List<string>();
        var groups = new List<string>();
        if (forced != null)
            for (int i = 0; i < forced.Count && res.Count < count; i++)
            {
                var d = CouncilLaws.Get(forced[i]);
                if (d == null) continue;
                res.Add(d.Id); if (d.Group != null) groups.Add(d.Group);
            }
        var pool = new List<CouncilLawDef>();
        for (int i = 0; i < CouncilLaws.All.Count; i++)
        {
            var d = CouncilLaws.All[i];
            if (!d.Implemented) continue;
            if (d.Id == CouncilLaws.NoConfidence) continue;   // only offered by the victory-mandate rules
            if (res.Contains(d.Id)) continue;
            if (Members.Count == 0) continue;
            pool.Add(d);
        }
        // do not repeat last vote's laws if there is enough variety
        var fresh = new List<CouncilLawDef>();
        for (int i = 0; i < pool.Count; i++) if (!LastProposals.Contains(pool[i].Id)) fresh.Add(pool[i]);
        var src = fresh.Count >= count - res.Count ? fresh : pool;
        int guard = 0;
        while (res.Count < count && src.Count > 0 && guard++ < 200)
        {
            int k = rng.Range(src.Count);
            var d = src[k];
            if (d.Group != null && groups.Contains(d.Group)) { src.RemoveAt(k); continue; }
            res.Add(d.Id); if (d.Group != null) groups.Add(d.Group);
            src.RemoveAt(k);
        }
        return res;
    }

    // ------------------------------------------------------------------ ballots

    /// <summary>Validates and replaces the whole ballot of an empire. Entries: [question, option, votes].</summary>
    public bool TrySetBallot(int emp, IList<int[]> entries)
    {
        if (!Enabled || Phase != PhaseVoting || !IsMember(emp)) return false;
        var clean = new List<int[]>();
        int sum = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            int q = entries[i][0], opt = entries[i][1], v = entries[i][2];
            if (v == 0) continue;
            if (v < 0 || v % VoteStep != 0) return false;
            if (q >= MandateQBase)
            {
                // votes for putting a mandate on the NEXT vote
                if (opt != 0 || q - MandateQBase >= CouncilLaws.Mandates.Count) return false;
                if (!Qualifies(emp, CouncilLaws.Mandates[q - MandateQBase].Id)) return false;
                bool merged2 = false;
                for (int j = 0; j < clean.Count; j++)
                    if (clean[j][0] == q && clean[j][1] == opt) { clean[j][2] += v; merged2 = true; break; }
                if (!merged2) clean.Add(new[] { q, opt, v });
                sum += v;
                continue;
            }
            if (q < 0 || q >= Proposals.Count) return false;
            var d = CouncilLaws.Get(Proposals[q]);
            if (d == null) return false;
            if (d.Kind == CouncilLawDef.KindMandate) { if (!MandateCandidates.Contains(opt)) return false; }
            else if (d.Targeted) { if (!IsMember(opt)) return false; }
            else if (opt != 0 && opt != 1) return false;
            bool merged = false;
            for (int j = 0; j < clean.Count; j++)
                if (clean[j][0] == q && clean[j][1] == opt) { clean[j][2] += v; merged = true; break; }
            if (!merged) clean.Add(new[] { q, opt, v });
            sum += v;
        }
        if (sum > Budget(emp)) return false;
        Ballots[emp] = clean;
        return true;
    }

    // ------------------------------------------------------------------ tally

    /// <summary>Counts the votes of the open session, enacts the winning laws and closes the vote.</summary>
    public List<CouncilActiveLaw> Tally(int turn)
    {
        var enacted = new List<CouncilActiveLaw>();
        if (Phase != PhaseVoting) return enacted;
        for (int q = 0; q < Proposals.Count; q++)
        {
            var d = CouncilLaws.Get(Proposals[q]);
            if (d == null) continue;
            var res = new CouncilResult { Turn = turn, LawId = d.Id, Target = -1 };
            if (!d.Targeted)
            {
                int yes = 0, no = 0;
                foreach (var kv in Ballots)
                    for (int i = 0; i < kv.Value.Count; i++)
                        if (kv.Value[i][0] == q) { if (kv.Value[i][1] == 0) yes += kv.Value[i][2]; else no += kv.Value[i][2]; }
                res.Yes = yes; res.No = no;
                res.Passed = yes > no;
                if (res.Passed) enacted.Add(new CouncilActiveLaw { Id = d.Id, Target = -1, StartTurn = turn });
            }
            else
            {
                var perTarget = new Dictionary<int, int>();
                foreach (var kv in Ballots)
                    for (int i = 0; i < kv.Value.Count; i++)
                        if (kv.Value[i][0] == q)
                        {
                            int t = kv.Value[i][1], cur;
                            if (!IsMember(t) || !IsMember(kv.Key)) continue;
                            perTarget.TryGetValue(t, out cur);
                            perTarget[t] = cur + kv.Value[i][2];
                        }
                int best = -1, bestV = 0, second = 0;
                foreach (var kv in perTarget)
                {
                    if (kv.Value > bestV) { second = bestV; bestV = kv.Value; best = kv.Key; }
                    else if (kv.Value > second) second = kv.Value;
                }
                bool tie = best >= 0 && second == bestV;
                res.Yes = bestV; res.No = second;
                res.Passed = best >= 0 && bestV > 0 && !tie;
                if (res.Passed && d.Kind == CouncilLawDef.KindMandate)
                {
                    res.Target = best;
                    SetHolder(d.Id.Substring(CouncilLaws.MandatePrefix.Length), best, turn);
                    enacted.Add(new CouncilActiveLaw { Id = d.Id, Target = best, StartTurn = turn });
                }
                else if (res.Passed) { res.Target = best; enacted.Add(new CouncilActiveLaw { Id = d.Id, Target = best, StartTurn = turn }); }
            }
            History.Add(res);
        }
        TallyMandateVotes(turn);
        while (History.Count > HistoryLimit) History.RemoveAt(0);

        // apply to state: cycle laws stay in Active, permanent ones change stats, instant ones are only reported
        for (int i = 0; i < enacted.Count; i++)
        {
            var d = CouncilLaws.Get(enacted[i].Id);
            if (d == null) continue;
            if (d.Kind == CouncilLawDef.KindCycle) Active.Add(enacted[i]);
            else if (d.Kind == CouncilLawDef.KindPermanent) ApplyPermanent(d.Id, enacted[i].Target);
            else if (d.Id == CouncilLaws.NoConfidence) RevokeMandate(CouncilLaws.Supreme);
        }
        Phase = PhaseIdle;
        Proposals.Clear();
        Ballots.Clear();
        PresenceBonus.Clear();
        MandateCandidates.Clear();
        return enacted;
    }

    void ApplyPermanent(string id, int target)
    {
        int cur;
        if (id == CouncilLaws.Merit) { VoteBonus.TryGetValue(target, out cur); VoteBonus[target] = cur + 30; }
        else if (id == CouncilLaws.ExpansionMandate) { ColonyDelta.TryGetValue(target, out cur); ColonyDelta[target] = cur + 1; }
        else if (id == CouncilLaws.ExpansionLimit) { ColonyDelta.TryGetValue(target, out cur); ColonyDelta[target] = cur - 1; }
    }

    // ------------------------------------------------------------------ AI ballots

    /// <summary>
    /// Deterministic AI ballot. scoreOf(emp) lets the caller rank empires (strongest rival is the preferred
    /// victim of harmful laws).
    /// </summary>
    public List<int[]> BuildAiBallot(int emp, int seed, Func<int, int> scoreOf)
    {
        var rng = new CouncilRng(seed ^ (emp * 7919));
        int n = Proposals.Count;
        var list = new List<int[]>();
        if (n == 0) return list;
        int budget = Budget(emp);
        // spend 70..100% of the budget in 10-vote chunks, weights pseudo-random
        int chunks = budget / VoteStep;
        chunks = (int)(chunks * (70 + rng.Range(31)) / 100);
        // sometimes back a mandate the empire qualifies for (it goes to the NEXT vote)
        var qualified = new List<int>();
        for (int mi = 0; mi < CouncilLaws.Mandates.Count; mi++)
            if (Qualifies(emp, CouncilLaws.Mandates[mi].Id)) qualified.Add(mi);
        if (qualified.Count > 0 && rng.Range(100) < 70)
        {
            int mc = Math.Max(1, Math.Min(chunks / 3, 1 + rng.Range(3)));
            list.Add(new[] { MandateQBase + qualified[rng.Range(qualified.Count)], 0, mc * VoteStep });
            chunks -= mc;
        }
        var weights = new int[n];
        for (int q = 0; q < n; q++) weights[q] = 1 + rng.Range(4);
        int wsum = 0; for (int q = 0; q < n; q++) wsum += weights[q];
        for (int q = 0; q < n; q++)
        {
            int c = chunks * weights[q] / wsum;
            if (c <= 0) continue;
            var d = CouncilLaws.Get(Proposals[q]);
            if (d == null) continue;
            int opt;
            if (d.Kind == CouncilLawDef.KindMandate)
            {
                if (MandateCandidates.Count == 0) continue;
                opt = MandateCandidates.Contains(emp) ? emp : MandateCandidates[rng.Range(MandateCandidates.Count)];
            }
            else if (d.Targeted)
            {
                opt = emp;
                if (d.Benefit < 0)
                {
                    int bestScore = int.MinValue; opt = -1;
                    for (int i = 0; i < Members.Count; i++)
                    {
                        if (Members[i] == emp) continue;
                        int sc = scoreOf != null ? scoreOf(Members[i]) : 0;
                        if (sc > bestScore) { bestScore = sc; opt = Members[i]; }
                    }
                    if (opt < 0) continue;     // nobody else to blame
                }
            }
            else
            {
                int chanceYes = d.Benefit > 0 ? 80 : (d.Benefit < 0 ? 20 : 50);
                opt = rng.Range(100) < chanceYes ? 0 : 1;
            }
            list.Add(new[] { q, opt, c * VoteStep });
        }
        return list;
    }

    // ------------------------------------------------------------------ persistence

    static string I(int v) { return v.ToString(CultureInfo.InvariantCulture); }

    public string Serialize()
    {
        var sb = new StringBuilder();
        sb.Append("GC2|").Append(Enabled ? 1 : 0).Append('|').Append(Destroyed ? 1 : 0).Append('|')
          .Append(I(FoundedTurn)).Append('|').Append(I(VoteIndex)).Append('|').Append(I(Phase)).Append('|')
          .Append(I(VoteOpenTurn)).Append('|').Append(I(NextVoteTurn)).Append('|').Append(I(IntervalTurns)).Append('|');
        sb.Append(string.Join(",", Proposals.ToArray())).Append('|');
        sb.Append(string.Join(",", LastProposals.ToArray())).Append('|');
        var mem = new string[Members.Count]; for (int i = 0; i < mem.Length; i++) mem[i] = I(Members[i]);
        sb.Append(string.Join(",", mem)).Append('|');
        // ballots: emp@q:opt:v,q:opt:v;emp@...
        var parts = new List<string>();
        foreach (var kv in Ballots)
        {
            var e = new List<string>();
            for (int i = 0; i < kv.Value.Count; i++) e.Add(I(kv.Value[i][0]) + ":" + I(kv.Value[i][1]) + ":" + I(kv.Value[i][2]));
            parts.Add(I(kv.Key) + "@" + string.Join(",", e.ToArray()));
        }
        sb.Append(string.Join(";", parts.ToArray())).Append('|');
        parts.Clear();
        for (int i = 0; i < Active.Count; i++) parts.Add(Active[i].Id + ":" + I(Active[i].Target) + ":" + I(Active[i].StartTurn));
        sb.Append(string.Join(";", parts.ToArray())).Append('|');
        parts.Clear();
        foreach (var kv in VoteBonus) parts.Add(I(kv.Key) + ":" + I(kv.Value));
        sb.Append(string.Join(";", parts.ToArray())).Append('|');
        parts.Clear();
        foreach (var kv in ColonyDelta) parts.Add(I(kv.Key) + ":" + I(kv.Value));
        sb.Append(string.Join(";", parts.ToArray())).Append('|');
        parts.Clear();
        for (int i = 0; i < History.Count; i++)
        {
            var h = History[i];
            parts.Add(I(h.Turn) + ":" + h.LawId.Replace(':', '~') + ":" + I(h.Target) + ":" + (h.Passed ? "1" : "0") + ":" + I(h.Yes) + ":" + I(h.No));
        }
        sb.Append(string.Join(";", parts.ToArray())).Append('|');
        AppendMandateState(sb);
        return sb.ToString();
    }

    static int P(string s) { return int.Parse(s, CultureInfo.InvariantCulture); }
    static string[] Split(string s, char c) { return string.IsNullOrEmpty(s) ? new string[0] : s.Split(c); }

    /// <summary>Returns a fresh state; an empty/invalid string gives a disabled council.</summary>
    public static CouncilCore Deserialize(string data)
    {
        var c = new CouncilCore();
        if (string.IsNullOrEmpty(data)) return c;
        try
        {
            var f = data.Split('|');
            if (f.Length < 15 || (f[0] != "GC1" && f[0] != "GC2")) return c;
            var n = new CouncilCore();
            n.Enabled = f[1] == "1"; n.Destroyed = f[2] == "1";
            n.FoundedTurn = P(f[3]); n.VoteIndex = P(f[4]); n.Phase = P(f[5]);
            n.VoteOpenTurn = P(f[6]); n.NextVoteTurn = P(f[7]); n.IntervalTurns = Math.Max(1, P(f[8]));
            foreach (var s in Split(f[9], ',')) n.Proposals.Add(s);
            foreach (var s in Split(f[10], ',')) n.LastProposals.Add(s);
            foreach (var s in Split(f[11], ',')) n.Members.Add(P(s));
            foreach (var rec in Split(f[12], ';'))
            {
                var ab = rec.Split('@');
                var list = new List<int[]>();
                foreach (var e in Split(ab[1], ','))
                {
                    var t = e.Split(':');
                    list.Add(new[] { P(t[0]), P(t[1]), P(t[2]) });
                }
                n.Ballots[P(ab[0])] = list;
            }
            foreach (var rec in Split(f[13], ';'))
            {
                var t = rec.Split(':');
                n.Active.Add(new CouncilActiveLaw { Id = t[0], Target = P(t[1]), StartTurn = P(t[2]) });
            }
            foreach (var rec in Split(f[14], ';')) { var t = rec.Split(':'); n.VoteBonus[P(t[0])] = P(t[1]); }
            if (f.Length > 15) foreach (var rec in Split(f[15], ';')) { var t = rec.Split(':'); n.ColonyDelta[P(t[0])] = P(t[1]); }
            if (f.Length > 16)
                foreach (var rec in Split(f[16], ';'))
                {
                    var t = rec.Split(':');
                    n.History.Add(new CouncilResult { Turn = P(t[0]), LawId = t[1].Replace('~', ':'), Target = P(t[2]), Passed = t[3] == "1", Yes = P(t[4]), No = P(t[5]) });
                }
            if (f[0] == "GC2") n.ReadMandateState(f);
            else n.VictoryTurns = Math.Max(1, (int)Math.Round(VictoryBaseTurns * n.IntervalTurns / (double)BaseIntervalTurns));
            return n;
        }
        catch (Exception)
        {
            return c;
        }
    }
}
