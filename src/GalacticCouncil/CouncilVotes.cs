// Galactic Council: where the votes of an empire come from (pure C#, no game types, part of CouncilCore).
//   base 100 + Merit law + Strategic Presence                        (CouncilCore.Budget)
//   + session snapshot of the game state taken when the vote opens:   star system levels, Republic, senator hero skills,
//                                                                     minus the penalty for declared wars
//   + 50 votes for every ordinary mandate held, 100 for the Supreme one (which also doubles everything else)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public sealed partial class CouncilCore
{
    public const int RepublicVotes = 40;          // empire under the Republic government
    public const int MandateVotes = 50;           // each ordinary mandate
    public const int SupremeMandateVotes = 100;   // the victory mandate (on top of the doubling)
    public const int WarPenaltyVotes = 20;        // per declared war ...
    // ... for IntervalTurns turns (one vote cycle, 20 turns at normal speed)

    /// <summary>Extra votes taken from the game when the session opened (system levels, Republic, hero skills, minus wars).</summary>
    public Dictionary<int, int> SessionVotes = new Dictionary<int, int>();
    /// <summary>Turns at which the empire declared a war (empire -> turns), for the war penalty.</summary>
    public Dictionary<int, List<int>> WarsDeclared = new Dictionary<int, List<int>>();

    /// <summary>Votes of one star system: level 2 gives 5, level 3 gives 10, level 4 gives 20. The levels do not add up.</summary>
    public static int SystemLevelVotes(int level)
    {
        if (level >= 4) return 20;
        if (level == 3) return 10;
        if (level == 2) return 5;
        return 0;
    }

    public int SessionVotesOf(int emp) { int v; return SessionVotes.TryGetValue(emp, out v) ? v : 0; }

    /// <summary>Votes given by the mandates the empire holds now (the Supreme one: 100 instead of 50).</summary>
    public int MandateVotesOf(int emp)
    {
        int v = 0;
        for (int i = 0; i < Holders.Count; i++)
            if (Holders[i].Emp == emp && Holders[i].Id != CouncilLaws.Supreme) v += MandateVotes;
        return v;
    }

    public void RecordWarDeclared(int emp, int turn)
    {
        List<int> l;
        if (!WarsDeclared.TryGetValue(emp, out l)) { l = new List<int>(); WarsDeclared[emp] = l; }
        l.Add(turn);
        if (l.Count > 40) l.RemoveRange(0, l.Count - 40);
    }

    /// <summary>Wars the empire declared during the last IntervalTurns turns (every one costs 20 votes).</summary>
    public int RecentWars(int emp, int turn)
    {
        List<int> l;
        if (!WarsDeclared.TryGetValue(emp, out l)) return 0;
        int n = 0;
        for (int i = 0; i < l.Count; i++) if (turn - l[i] < IntervalTurns && turn >= l[i]) n++;
        return n;
    }

    public int WarPenaltyOf(int emp, int turn) { return RecentWars(emp, turn) * WarPenaltyVotes; }

    /// <summary>Called when a session opens: gameVotes holds the positive votes read from the game; the war penalty is subtracted here.</summary>
    public void ApplySessionVotes(Dictionary<int, int> gameVotes, int turn)
    {
        SessionVotes.Clear();
        for (int i = 0; i < Members.Count; i++)
        {
            int e = Members[i];
            int v; gameVotes.TryGetValue(e, out v);
            v -= WarPenaltyOf(e, turn);
            if (v != 0) SessionVotes[e] = v;
        }
        // old wars no longer matter
        foreach (var kv in WarsDeclared) kv.Value.RemoveAll(delegate (int t) { return turn - t >= IntervalTurns * 2; });
    }

    /// <summary>One line that explains where the votes of an empire come from.</summary>
    public string BudgetText(int emp, int turn, bool ru)
    {
        int bonus; VoteBonus.TryGetValue(emp, out bonus);
        int law = bonus + PresenceBonusOf(emp);
        int session = SessionVotesOf(emp);
        int mand = MandateVotesOf(emp);
        bool supreme = Holds(emp, CouncilLaws.Supreme);
        var sb = new StringBuilder();
        sb.Append(ru ? "База " : "Base ").Append(BaseVotes);
        if (law != 0) sb.Append(ru ? "; законы и присутствие " : "; laws and presence ").Append(law > 0 ? "+" : "").Append(law);
        if (session != 0) sb.Append(ru ? "; системы, Республика, сенаторы, войны " : "; systems, Republic, senators, wars ").Append(session > 0 ? "+" : "").Append(session);
        if (mand != 0) sb.Append(ru ? "; мандаты +" : "; mandates +").Append(mand);
        if (supreme) sb.Append(ru ? "; Верховный легат: ×2 и +" : "; Supreme Legate: x2 and +").Append(SupremeMandateVotes);
        int wars = RecentWars(emp, turn);
        if (wars > 0) sb.Append(ru ? "; штраф за объявленные войны (" + wars + "): −" : "; penalty for declared wars (" + wars + "): -").Append(wars * WarPenaltyVotes);
        return sb.ToString();
    }

    void ResetVotes() { SessionVotes.Clear(); WarsDeclared.Clear(); }

    // ------------------------------------------------------------------ persistence (two fields after the presence state)

    void AppendVoteState(StringBuilder sb)
    {
        var parts = new List<string>();
        foreach (var kv in SessionVotes) parts.Add(Iv(kv.Key) + ":" + Iv(kv.Value));
        sb.Append('|').Append(string.Join(";", parts.ToArray())).Append('|');
        parts.Clear();
        foreach (var kv in WarsDeclared) parts.Add(Iv(kv.Key) + "@" + IntList(kv.Value));
        sb.Append(string.Join(";", parts.ToArray()));
    }

    void ReadVoteState(string sessionVotes, string wars)
    {
        foreach (var rec in SplitStr(sessionVotes, ';'))
        {
            var t = rec.Split(':');
            SessionVotes[int.Parse(t[0], CultureInfo.InvariantCulture)] = int.Parse(t[1], CultureInfo.InvariantCulture);
        }
        foreach (var rec in SplitStr(wars, ';'))
        {
            var ab = rec.Split('@');
            WarsDeclared[int.Parse(ab[0], CultureInfo.InvariantCulture)] = ab.Length > 1 ? ParseIntList(ab[1]) : new List<int>();
        }
    }
}
