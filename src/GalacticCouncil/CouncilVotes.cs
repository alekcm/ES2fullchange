// Galactic Council: where the votes of an empire come from (pure C#, no game types, part of CouncilCore).
//   base 100 + Merit law + Strategic Presence                        (CouncilCore.Budget)
//   + session snapshot of the game state taken when the vote opens:   star system levels, Republic, Empire Development technology stage, senator hero skills,
//                                                                     minus the penalty for declared wars
//   + 50 votes for every ordinary mandate held, 100 for the Supreme one (and nothing is doubled)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public sealed partial class CouncilCore
{
    public const int RepublicVotes = 40;          // empire under the Republic government
    public const int MandateVotes = 50;           // each ordinary mandate
    public const int SupremeMandateVotes = 100;   // the victory mandate (simply +100, nothing is doubled)
    public const int KeepPercentSolo = 51;        // confirmation of the victory mandate: share of the votes needed to keep it
    public const int KeepPercentAlliance = 67;    // the same when the holder is a member of an alliance
    public const int WarPenaltyVotes = 20;        // per declared war ...
    // ... for IntervalTurns turns (one vote cycle, 20 turns at normal speed)

    /// <summary>Extra votes taken from the game when the session opened (system levels, Republic, hero skills, minus wars).</summary>
    public Dictionary<int, int> SessionVotes = new Dictionary<int, int>();
    /// <summary>Turns at which the empire declared a war (empire -> turns), for the war penalty.</summary>
    public Dictionary<int, List<int>> WarsDeclared = new Dictionary<int, List<int>>();

    /// <summary>Set by the game layer before the tally and every turn: the holder of the victory mandate is a member of an alliance.</summary>
    public bool SupremeInAlliance;

    public int KeepPercentNeeded { get { return SupremeInAlliance ? KeepPercentAlliance : KeepPercentSolo; } }

    /// <summary>The victory mandate stays when the "keep" votes are at least 51% of all votes cast (67% for an allied holder). Nobody voted: it stays.</summary>
    public bool MandateKept(int keep, int remove)
    {
        long total = (long)keep + remove;
        return total == 0 || keep * 100L >= KeepPercentNeeded * total;
    }

    /// <summary>Votes for the stage reached in the Empire Development technology branch: stage 3 gives 10, stage 4 gives 20, stage 5 gives 30. The stages do not add up (like the system levels).</summary>
    public static int TechStageVotes(int stage)
    {
        if (stage >= 5) return 30;
        if (stage == 4) return 20;
        if (stage == 3) return 10;
        return 0;
    }

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
        if (session != 0) sb.Append(ru ? "; системы, технологии, Республика, сенаторы, войны " : "; systems, technologies, Republic, senators, wars ").Append(session > 0 ? "+" : "").Append(session);
        if (mand != 0) sb.Append(ru ? "; мандаты +" : "; mandates +").Append(mand);
        int bought = BoughtVotesOf(emp);
        if (bought > 0) sb.Append(ru ? "; куплено на сессии +" : "; bought this session +").Append(bought);
        if (supreme) sb.Append(ru ? "; Верховный легат +" : "; Supreme Legate +").Append(SupremeMandateVotes);
        int wars = RecentWars(emp, turn);
        if (wars > 0) sb.Append(ru ? "; штраф за объявленные войны (" + wars + "): −" : "; penalty for declared wars (" + wars + "): -").Append(wars * WarPenaltyVotes);
        return sb.ToString();
    }

    void ResetVotes() { SessionVotes.Clear(); WarsDeclared.Clear(); Buys.Clear(); TerrorFleets.Clear(); }

    // ------------------------------------------------------------------ votes bought during the session (senator class line)
    // The corporate class tree of the senators offers four ACTIVE ways to add 10 votes during an open session:
    // Dust (bribe), Influence (pressure), happiness of the systems (civic) and an immobilised fleet (terror).
    // The stacks are bought through OrderCouncilVote.KindBuyVotes; the game layer (CouncilGameBuys) pays the
    // price and refreshes the penalties. Everything is wiped when the NEXT session opens - the votes of the old
    // one stop counting and the happiness / fleet penalties end exactly there.

    public const int BuyMethodCount = 4;         // 0 bribe (Dust), 1 pressure (Influence), 2 civic (happiness), 3 terror (a frozen fleet)
    public const int BuyVotesPerStack = 10;      // votes of one purchase
    public const int BuyBoostStacks = 2;         // the class ultimate: extra purchases for every method
    public const int DustPerStack = 250;         // base price of one bribed stack before inflation
    public const int InfluencePerStack = 100;    // base price of one pressured stack before inflation
    public const int CivicHappinessPerStack = 5; // happiness every system loses per civic purchase
    public const int TerrorFleetCp = 4;          // command points a fleet needs to be held "in fear"

    /// <summary>Purchases of the current session: empire -> stacks bought per method (missing = none).</summary>
    public Dictionary<int, int[]> Buys = new Dictionary<int, int[]>();
    /// <summary>Fleets held "in fear": empire -> GUIDs; one per terror purchase, freed when the next session opens.</summary>
    public Dictionary<int, List<ulong>> TerrorFleets = new Dictionary<int, List<ulong>>();

    static readonly int[] NoBuys = new int[BuyMethodCount];

    /// <summary>Copies of the purchases of an empire (missing entries are all zero).</summary>
    public int[] BuysOf(int emp)
    {
        int[] v;
        return Buys.TryGetValue(emp, out v) && v != null && v.Length == BuyMethodCount ? v : NoBuys;
    }

    int[] OwnedBuys(int emp)
    {
        int[] v;
        if (!Buys.TryGetValue(emp, out v) || v == null || v.Length != BuyMethodCount) { v = new int[BuyMethodCount]; Buys[emp] = v; }
        return v;
    }

    public int BoughtStacks(int emp, int method) { return BuysOf(emp)[method]; }

    /// <summary>Votes the empire bought this session (10 per purchase, all methods together).</summary>
    public int BoughtVotesOf(int emp)
    {
        int[] v = BuysOf(emp);
        return BuyVotesPerStack * (v[0] + v[1] + v[2] + v[3]);
    }

    public bool IsTerrorFleet(int emp, ulong guid)
    {
        List<ulong> l;
        return TerrorFleets.TryGetValue(emp, out l) && l != null && l.Contains(guid);
    }

    /// <summary>Price of one purchased stack with inflation: the base price of the method x game stage x (1 + half of the purchases already made).</summary>
    public static int BuyStackCost(int method, int stack, int stage)
    {
        if (method != 0 && method != 1) return 0;
        int b = method == 0 ? DustPerStack : InfluencePerStack;
        float f = b * Math.Max(1, stage) * (1f + 0.5f * Math.Max(0, stack - 1));
        return (int)(Math.Round(f / 10f, MidpointRounding.AwayFromZero) * 10f);
    }

    public bool CanBuyVotes(int emp, int method, int limit)
    {
        if (!Enabled || Phase != PhaseVoting || !IsMember(emp)) return false;
        if (method < 0 || method >= BuyMethodCount) return false;
        return BuysOf(emp)[method] < limit;
    }

    /// <summary>Registers one purchased stack; the game layer has already collected the price.</summary>
    public bool TryBuyVotes(int emp, int method, int limit)
    {
        if (!CanBuyVotes(emp, method, limit)) return false;
        OwnedBuys(emp)[method]++;
        return true;
    }

    /// <summary>One terror purchase: the stack and the immobilised fleet register together (the fleet is the price).</summary>
    public bool TryBuyTerror(int emp, int limit, ulong guid)
    {
        if (!CanBuyVotes(emp, 3, limit) || guid == 0ul || IsTerrorFleet(emp, guid)) return false;
        OwnedBuys(emp)[3]++;
        List<ulong> l;
        if (!TerrorFleets.TryGetValue(emp, out l) || l == null) { l = new List<ulong>(); TerrorFleets[emp] = l; }
        l.Add(guid);
        return true;
    }

    // ------------------------------------------------------------------ persistence (two fields after the presence state)

    void AppendVoteState(StringBuilder sb)
    {
        var parts = new List<string>();
        foreach (var kv in SessionVotes) parts.Add(Iv(kv.Key) + ":" + Iv(kv.Value));
        sb.Append('|').Append(string.Join(";", parts.ToArray())).Append('|');
        parts.Clear();
        foreach (var kv in WarsDeclared) parts.Add(Iv(kv.Key) + "@" + IntList(kv.Value));
        sb.Append(string.Join(";", parts.ToArray()));
        AppendBuyState(sb);
    }

    void AppendBuyState(StringBuilder sb)
    {
        var parts = new List<string>();
        foreach (var kv in Buys)
            if (kv.Value != null)
            {
                var n = new string[kv.Value.Length];
                for (int i = 0; i < n.Length; i++) n[i] = Iv(kv.Value[i]);
                parts.Add(Iv(kv.Key) + ":" + string.Join(",", n));
            }
        sb.Append('|').Append(string.Join(";", parts.ToArray())).Append('|');
        parts.Clear();
        foreach (var kv in TerrorFleets)
            if (kv.Value != null && kv.Value.Count > 0)
            {
                var n = new string[kv.Value.Count];
                for (int i = 0; i < n.Length; i++) n[i] = kv.Value[i].ToString(CultureInfo.InvariantCulture);
                parts.Add(Iv(kv.Key) + "@" + string.Join(",", n));
            }
        sb.Append(string.Join(";", parts.ToArray()));
    }

    void ReadBuyState(string buys, string fleets)
    {
        foreach (var rec in SplitStr(buys, ';'))
        {
            var ab = rec.Split(':');
            var t = SplitStr(ab[1], ',');
            var v = new int[Math.Min(t.Length, BuyMethodCount)];
            for (int i = 0; i < v.Length; i++) v[i] = int.Parse(t[i], CultureInfo.InvariantCulture);
            Buys[int.Parse(ab[0], CultureInfo.InvariantCulture)] = v;
        }
        foreach (var rec in SplitStr(fleets, ';'))
        {
            var ab = rec.Split('@');
            var l = new List<ulong>();
            foreach (var g in SplitStr(ab[1], ','))
            {
                ulong u;
                if (ulong.TryParse(g, NumberStyles.Integer, CultureInfo.InvariantCulture, out u) && u != 0ul) l.Add(u);
            }
            TerrorFleets[int.Parse(ab[0], CultureInfo.InvariantCulture)] = l;
        }
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
