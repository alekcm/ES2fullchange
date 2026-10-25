// Galactic Council of the Academy - mandates, supreme (victory) mandate, no-confidence, leaving the council.
// Pure C# (no game dependencies), part of CouncilCore.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public sealed class CouncilMandateHold
{
    public string Id;
    public int Emp;
    public int Since;     // vote index at which the mandate was won
}

public sealed partial class CouncilCore
{
    public const int MandateQBase = 100;       // ballot entries with question >= 100 are votes for putting mandate (q-100) on the next vote
    public const int VictoryBaseTurns = 30;

    public List<CouncilMandateHold> Holders = new List<CouncilMandateHold>();
    public Dictionary<int, List<string>> Tasks = new Dictionary<int, List<string>>();   // empire -> "category~taskId"
    public string PendingMandate = "";                       // mandate that won the mandate vote and goes on the next vote
    public List<int> MandateCandidates = new List<int>();    // empires that may be elected in the current mandate question
    public int SupremeSince = -1;                            // turn the supreme mandate was won (-1: nobody holds it)
    public int VictoryTurns = VictoryBaseTurns;
    public List<int> Outcasts = new List<int>();             // empires that left the council (the Academy is at war with them)
    public int ChairmanUsedVote = -1;
    public Dictionary<int, int> LastSupplyTurn = new Dictionary<int, int>();
    public List<int> MarshalHired = new List<int>();

    void ResetMandates()
    {
        Holders.Clear(); Tasks.Clear(); PendingMandate = ""; MandateCandidates.Clear();
        SupremeSince = -1; Outcasts.Clear(); ChairmanUsedVote = -1; LastSupplyTurn.Clear(); MarshalHired.Clear();
        ResetPresence();
        ResetVotes();
    }

    // ------------------------------------------------------------------ achievements: the game layer reports every qualification condition once it is seen in the game state

    public bool ReportTask(int emp, string category, string taskId)
    {
        if (!IsMember(emp)) return false;
        string key = category + "~" + taskId;
        List<string> l;
        if (!Tasks.TryGetValue(emp, out l)) { l = new List<string>(); Tasks[emp] = l; }
        if (l.Contains(key)) return false;
        l.Add(key);
        return true;
    }

    public bool HasTask(int emp, string category, string taskId)
    {
        List<string> l;
        return Tasks.TryGetValue(emp, out l) && l.Contains(category + "~" + taskId);
    }

    public int TaskCount(int emp, string[] categories)
    {
        List<string> l;
        if (!Tasks.TryGetValue(emp, out l)) return 0;
        int n = 0;
        for (int i = 0; i < l.Count; i++)
        {
            string cat = l[i].Substring(0, l[i].IndexOf('~'));
            for (int c = 0; c < categories.Length; c++) if (categories[c] == cat) { n++; break; }
        }
        return n;
    }

    // ------------------------------------------------------------------ qualification

    public bool QualifiesOrdinary(int emp, CouncilMandateDef d)
    {
        return d != null && !d.Victory && TaskCount(emp, d.Categories) >= d.Need;
    }

    public int OrdinaryQualifiedCount(int emp)
    {
        int n = 0;
        for (int i = 0; i < CouncilLaws.Mandates.Count; i++) if (QualifiesOrdinary(emp, CouncilLaws.Mandates[i])) n++;
        return n;
    }

    /// <summary>True when the empire holds an ordinary mandate that survived at least one full cycle.</summary>
    public bool HeldOrdinaryForFullCycle(int emp)
    {
        for (int i = 0; i < Holders.Count; i++)
            if (Holders[i].Emp == emp && Holders[i].Id != CouncilLaws.Supreme && Holders[i].Since < VoteIndex) return true;
        return false;
    }

    public bool Qualifies(int emp, string mandateId)
    {
        if (!IsMember(emp)) return false;
        var d = CouncilLaws.GetMandate(mandateId);
        if (d == null) return false;
        if (d.Victory) return OrdinaryQualifiedCount(emp) >= 2 && HeldOrdinaryForFullCycle(emp);
        return QualifiesOrdinary(emp, d);
    }

    // ------------------------------------------------------------------ holders

    public bool Holds(int emp, string id)
    {
        for (int i = 0; i < Holders.Count; i++) if (Holders[i].Emp == emp && Holders[i].Id == id) return true;
        return false;
    }

    public int HolderOf(string id)
    {
        for (int i = 0; i < Holders.Count; i++) if (Holders[i].Id == id) return Holders[i].Emp;
        return -1;
    }

    public int SupremeHolder { get { return HolderOf(CouncilLaws.Supreme); } }

    public void SetHolder(string id, int emp, int turn)
    {
        RevokeMandate(id);
        Holders.Add(new CouncilMandateHold { Id = id, Emp = emp, Since = VoteIndex });
        if (id == CouncilLaws.Supreme) SupremeSince = turn;
    }

    public void RevokeMandate(string id)
    {
        for (int i = Holders.Count - 1; i >= 0; i--) if (Holders[i].Id == id) Holders.RemoveAt(i);
        if (id == CouncilLaws.Supreme) SupremeSince = -1;
        if (id == CouncilLaws.Marshal) MarshalHired.Clear();
    }

    void RemoveHoldsOf(int emp)
    {
        var ids = new List<string>();
        for (int i = 0; i < Holders.Count; i++) if (Holders[i].Emp == emp) ids.Add(Holders[i].Id);
        for (int i = 0; i < ids.Count; i++) RevokeMandate(ids[i]);
        MarshalHired.Remove(emp);
        LastSupplyTurn.Remove(emp);
        Tasks.Remove(emp);
    }

    // ------------------------------------------------------------------ leaving the council

    public bool IsOutcast(int emp) { return Outcasts.Contains(emp); }

    /// <summary>An empire leaves the council for good (the caller makes the Academy declare war on it).</summary>
    public bool Leave(int emp)
    {
        if (!Enabled || !IsMember(emp)) return false;
        RemoveMember(emp);
        if (!Outcasts.Contains(emp)) Outcasts.Add(emp);
        return true;
    }

    // ------------------------------------------------------------------ agenda of the next vote

    /// <summary>Forced questions (no-confidence, mandate election) + random ordinary laws.</summary>
    public List<string> BuildProposals(int seed, int count)
    {
        var forced = new List<string>();
        MandateCandidates.Clear();
        if (SupremeHolder >= 0) forced.Add(CouncilLaws.NoConfidence);
        if (!string.IsNullOrEmpty(PendingMandate) && CouncilLaws.GetMandate(PendingMandate) != null)
        {
            for (int i = 0; i < Members.Count; i++)
                if (Qualifies(Members[i], PendingMandate)) MandateCandidates.Add(Members[i]);
            if (MandateCandidates.Count > 0) forced.Add(CouncilLaws.MandatePrefix + PendingMandate);
        }
        PendingMandate = "";
        return PickProposals(seed, count, forced);
    }

    void TallyMandateVotes(int turn)
    {
        var sums = new Dictionary<int, int>();
        foreach (var kv in Ballots)
            for (int i = 0; i < kv.Value.Count; i++)
            {
                int q = kv.Value[i][0];
                if (q < MandateQBase) continue;
                int cur; sums.TryGetValue(q - MandateQBase, out cur);
                sums[q - MandateQBase] = cur + kv.Value[i][2];
            }
        int best = -1, bestV = 0;
        var tied = new List<int>();
        for (int m = 0; m < CouncilLaws.Mandates.Count; m++)
        {
            int v; if (!sums.TryGetValue(m, out v) || v <= 0) continue;
            if (v > bestV) { bestV = v; tied.Clear(); tied.Add(m); }
            else if (v == bestV) tied.Add(m);
        }
        if (tied.Count > 0) best = tied[tied.Count == 1 ? 0 : new CouncilRng(VoteIndex * 31 + turn).Range(tied.Count)];
        if (best < 0) { PendingMandate = ""; return; }
        PendingMandate = CouncilLaws.Mandates[best].Id;
        History.Add(new CouncilResult { Turn = turn, LawId = "MV:" + PendingMandate, Target = -1, Passed = true, Yes = bestV, No = 0 });
    }

    // ------------------------------------------------------------------ mandate powers

    public bool CanChairmanReplace(int emp)
    {
        return Enabled && Phase == PhaseVoting && Holds(emp, CouncilLaws.Chairman) && ChairmanUsedVote != VoteIndex && Ballots.Count == 0;
    }

    /// <summary>Replaces ordinary question q with an ordinary law (never mandate elections or no-confidence).</summary>
    public bool ChairmanReplace(int emp, int q, string lawId)
    {
        if (!CanChairmanReplace(emp)) return false;
        if (q < 0 || q >= Proposals.Count) return false;
        var old = CouncilLaws.Get(Proposals[q]);
        var nw = CouncilLaws.Get(lawId);
        if (old == null || nw == null) return false;
        if (old.Kind == CouncilLawDef.KindMandate || old.Id == CouncilLaws.NoConfidence) return false;
        if (nw.Kind == CouncilLawDef.KindMandate || nw.Id == CouncilLaws.NoConfidence || !nw.Implemented) return false;
        for (int i = 0; i < Proposals.Count; i++)
        {
            if (i == q) continue;
            if (Proposals[i] == lawId) return false;
            var o = CouncilLaws.Get(Proposals[i]);
            if (o != null && o.Group != null && o.Group == nw.Group) return false;
        }
        Proposals[q] = nw.Id;
        ChairmanUsedVote = VoteIndex;
        History.Add(new CouncilResult { Turn = VoteOpenTurn, LawId = "CH:" + old.Id + ">" + nw.Id, Target = emp, Passed = true });
        while (History.Count > HistoryLimit) History.RemoveAt(0);
        return true;
    }

    // ---- mandate powers of AI empires (nobody presses the buttons for them)

    public const int AiMarshalStockPerStage = 150;   // Dust in the treasury per game stage before an AI hires the contingent
    public const int AiMarshalNetPerShip = 25;       // Dust of net income per contingent ship it must keep after the hire

    public static int MarshalShips(int stage) { return Math.Min(8, 2 + Math.Max(0, stage)); }

    /// <summary>The Marshal contingent is paid by the holder: an AI hires it only when it can carry the upkeep (positive net income with a margin, and a stock).</summary>
    public static bool AiCanAffordMarshal(float stock, float net, int stage)
    {
        return net >= AiMarshalNetPerShip * MarshalShips(stage) && stock >= AiMarshalStockPerStage * Math.Max(1, stage);
    }

    /// <summary>An AI Chairman uses his right at once: one ordinary question is replaced by a random ordinary law that does not hurt anyone.</summary>
    public bool TryAiChairman(int emp, int seed)
    {
        if (!CanChairmanReplace(emp)) return false;
        var rng = new CouncilRng(seed ^ (emp * 7919));
        var slots = new List<int>();
        for (int q = 0; q < Proposals.Count; q++)
        {
            var d = CouncilLaws.Get(Proposals[q]);
            if (d != null && d.Kind != CouncilLawDef.KindMandate && d.Id != CouncilLaws.NoConfidence) slots.Add(q);
        }
        var laws = new List<CouncilLawDef>();
        for (int i = 0; i < CouncilLaws.All.Count; i++)
        {
            var nd = CouncilLaws.All[i];
            if (!nd.Implemented || nd.Kind == CouncilLawDef.KindMandate || nd.Id == CouncilLaws.NoConfidence || nd.Benefit < 0) continue;
            if (Proposals.Contains(nd.Id)) continue;
            laws.Add(nd);
        }
        if (slots.Count == 0 || laws.Count == 0) return false;
        for (int attempt = 0; attempt < 24; attempt++)
            if (ChairmanReplace(emp, slots[rng.Range(slots.Count)], laws[rng.Range(laws.Count)].Id)) return true;
        return false;
    }

    public bool CanUseQuartermaster(int emp, int turn)
    {
        if (!Enabled || !Holds(emp, CouncilLaws.Quartermaster)) return false;
        int last;
        return !LastSupplyTurn.TryGetValue(emp, out last) || turn - last >= IntervalTurns;
    }

    public int TurnsToQuartermaster(int emp, int turn)
    {
        int last;
        if (!LastSupplyTurn.TryGetValue(emp, out last)) return 0;
        return Math.Max(0, IntervalTurns - (turn - last));
    }

    public bool TryQuartermaster(int emp, int turn)
    {
        if (!CanUseQuartermaster(emp, turn)) return false;
        LastSupplyTurn[emp] = turn;
        return true;
    }

    public bool CanHireMarshal(int emp)
    {
        return Enabled && Holds(emp, CouncilLaws.Marshal) && !MarshalHired.Contains(emp);
    }

    public bool TryHireMarshal(int emp)
    {
        if (!CanHireMarshal(emp)) return false;
        MarshalHired.Add(emp);
        return true;
    }

    // ------------------------------------------------------------------ victory

    public int VictoryTurnsLeft(int turn)
    {
        if (SupremeSince < 0 || SupremeHolder < 0) return -1;
        return Math.Max(0, VictoryTurns - (turn - SupremeSince));
    }

    /// <summary>The empire that has held the supreme mandate long enough, or -1.</summary>
    public int CheckVictory(int turn)
    {
        if (!Enabled) return -1;
        int h = SupremeHolder;
        if (h < 0 || !IsMember(h) || SupremeSince < 0) return -1;
        return turn - SupremeSince >= VictoryTurns ? h : -1;
    }

    // ------------------------------------------------------------------ persistence of the mandate state

    static string Iv(int v) { return v.ToString(CultureInfo.InvariantCulture); }

    void AppendMandateState(StringBuilder sb)
    {
        var parts = new List<string>();
        foreach (var kv in Tasks) parts.Add(Iv(kv.Key) + "@" + string.Join(",", kv.Value.ToArray()));
        sb.Append(string.Join(";", parts.ToArray())).Append('|');
        parts.Clear();
        for (int i = 0; i < Holders.Count; i++) parts.Add(Holders[i].Id + ":" + Iv(Holders[i].Emp) + ":" + Iv(Holders[i].Since));
        sb.Append(string.Join(";", parts.ToArray())).Append('|');
        sb.Append(PendingMandate ?? "").Append('|');
        sb.Append(IntList(MandateCandidates)).Append('|');
        sb.Append(Iv(SupremeSince)).Append('|').Append(Iv(VictoryTurns)).Append('|');
        sb.Append(IntList(Outcasts)).Append('|');
        sb.Append(Iv(ChairmanUsedVote)).Append('|');
        parts.Clear();
        foreach (var kv in LastSupplyTurn) parts.Add(Iv(kv.Key) + ":" + Iv(kv.Value));
        sb.Append(string.Join(";", parts.ToArray())).Append('|');
        sb.Append(IntList(MarshalHired)).Append('|');
        AppendSquadState(sb);
        AppendPresenceState(sb);
        AppendVoteState(sb);
    }

    static string IntList(List<int> l)
    {
        var a = new string[l.Count];
        for (int i = 0; i < a.Length; i++) a[i] = Iv(l[i]);
        return string.Join(",", a);
    }

    static List<int> ParseIntList(string s)
    {
        var l = new List<int>();
        foreach (var x in SplitStr(s, ',')) l.Add(int.Parse(x, CultureInfo.InvariantCulture));
        return l;
    }

    static string[] SplitStr(string s, char c) { return string.IsNullOrEmpty(s) ? new string[0] : s.Split(c); }

    // f = fields of the serialized state, mandate fields start at index 17
    void ReadMandateState(string[] f)
    {
        if (f.Length < 27) return;
        foreach (var rec in SplitStr(f[17], ';'))
        {
            var ab = rec.Split('@');
            var l = new List<string>(SplitStr(ab[1], ','));
            Tasks[int.Parse(ab[0], CultureInfo.InvariantCulture)] = l;
        }
        foreach (var rec in SplitStr(f[18], ';'))
        {
            var t = rec.Split(':');
            Holders.Add(new CouncilMandateHold { Id = t[0], Emp = int.Parse(t[1], CultureInfo.InvariantCulture), Since = int.Parse(t[2], CultureInfo.InvariantCulture) });
        }
        PendingMandate = f[19];
        MandateCandidates = ParseIntList(f[20]);
        SupremeSince = int.Parse(f[21], CultureInfo.InvariantCulture);
        VictoryTurns = Math.Max(1, int.Parse(f[22], CultureInfo.InvariantCulture));
        Outcasts = ParseIntList(f[23]);
        ChairmanUsedVote = int.Parse(f[24], CultureInfo.InvariantCulture);
        foreach (var rec in SplitStr(f[25], ';'))
        {
            var t = rec.Split(':');
            LastSupplyTurn[int.Parse(t[0], CultureInfo.InvariantCulture)] = int.Parse(t[1], CultureInfo.InvariantCulture);
        }
        MarshalHired = ParseIntList(f[26]);
        if (f.Length > 27) ReadSquadState(f[27]);
        if (f.Length > 29) ReadPresenceState(f[28], f[29]);
        if (f.Length > 31) ReadVoteState(f[30], f[31]);
        if (f.Length > 33) ReadBuyState(f[32], f[33], f.Length > 34 ? f[34] : null);
    }
}
