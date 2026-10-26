// Battle rework - pure state and numbers (no game dependencies, unit-testable, see tests/BattleCoreTests.cs).
//
// Two systems live here:
//  * crew reserve: a ship only sends to the ground the part of its manpower it can do without,
//  * defender morale: a colonised system fights as long as its morale holds, at 0 it capitulates.
//
// Everything is stored per star system / per ship by GUID and serialised into the mod's own
// "CouncilState" attribute (see BattleRework / CouncilGame.SerializeAttributes), format "BR1|...".
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>One tracked star system: its morale and the events that ate it.</summary>
public sealed class BattleSystemState
{
    public int Morale = -1;          // 0..MoraleMax, -1 = not computed yet (fall back to the base formula)
    public int LastBattleTurn = -1;  // a ground battle is running / happened this turn
    public int LastSiegeTurn = -1;   // the system was besieged (blockaded) at the end of this turn
    public int LastRazeTurn = -1;    // the system was bombarded (obliterator / razing) at the end of this turn
    public int OccupationMarkTurn = -1;  // turn the system changed owner (captured, not colonised)
    public int LastChangeTurn = -1;      // last turn anything happened to the morale

    public bool Attacked(int turn) { return turn - LastBattleTurn <= 1; }
    public bool Besieged(int turn) { return turn - LastSiegeTurn <= 1; }
    public bool Bombarded(int turn) { return turn - LastRazeTurn <= 1; }
}

/// <summary>A ship design produced (or re-applied) from a combat template: hull + role + weapon family.</summary>
public sealed class BattleTemplateBinding
{
    public string TemplateId;    // "ShipDesignTemplateDefinition" name, e.g. "HullSmall01Sophons_SmallAttacker_Missile"
    public string Hull;          // HullDefinition name
    public string Role;          // ShipRoleDefinition name
    public string WeaponFamily;  // ModuleDefinition.Family of the weapon the design was built around
    public string DesignName;    // ShipDesign.Name at bind time (display only, the design may be renamed)
}

public sealed class BattleCore
{
    // ---------------------------------------------------------------- numbers (all tunable from here)

    public const int MoraleMax = 100;

    // base morale of a population: it does not start at "happiness", it starts at "what the
    // population is willing to die for", which is a fraction of its mood.
    public const int MoraleBase = 20;
    public const float MoralePerHappiness = 0.8f;      // +0.8 per point of system happiness (0..100)
    public const int MoraleHomeSystemBonus = 15;       // the capital and the home systems fight for real
    public const int MoraleOccupiedCap = 12;           // a freshly captured system never recovers above this
    public const int OccupationMoraleTurns = 6;        // ... until this many turns after the capture

    // losses
    public const int MoralePerBattleTurn = 6;          // every turn a ground battle is resolved on the system
    public const int MoralePerSiegeTurn = 4;           // garrison being ground down by a blockade / siege
    public const int MoralePerRazeTurn = 10;           // orbital bombardment (obliterator, razing)
    public const int MoralePerDefenderLossPercent = 0; // reserved: 0 = casualties alone do not break the defence
    public const int MoraleMilitiaTemporary = 8;       // "call up the citizens" for this battle only
    public const int MoraleMilitiaPermanent = 25;      // and for good when the conscripts are kept

    // recovery, per peaceful turn
    public const int MoraleRegen = 5;
    public const int MoraleRegenOccupied = 2;          // an occupied system calms down slower than it is defended

    // crew reserve: the share of a ship's manpower that never leaves the decks, and the hull cap of it
    public const float CrewReserveRatio = 0.75f;
    public const int CrewReserveSmall = 40;
    public const int CrewReserveMedium = 100;
    public const int CrewReserveLarge = 300;
    public const int CrewReserveMothership = 220;
    public const int CrewReserveMothershipPerLevel = 100;

    // a ship below its reserve is under-crewed: it fights worse (BattleRework registers the descriptor)
    public const float LowCrewDamagePenalty = 0.4f;    // -40% damage at 0 crew, linear in between
    public const float LowCrewMovementPenalty = 0.5f;  // -50% movement at 0 crew

    // refill gate: a fleet only refills its crews in a system that still answers to the capital
    public const bool RefillRequiresRootedNetwork = true;

    // refit: swapping the templates of a fleet in a friendly system
    public const float RefitDustBase = 50f;             // per ship, times the game stage
    public const int RefitTurnsLock = 1;                // the fleet cannot fight/be reordered for N turns

    // ---------------------------------------------------------------- state

    public readonly Dictionary<ulong, BattleSystemState> Systems = new Dictionary<ulong, BattleSystemState>();
    public readonly Dictionary<ulong, BattleTemplateBinding> Designs = new Dictionary<ulong, BattleTemplateBinding>();
    // per hull: the variant the player last chose (the game's own auto-design of that hull uses it, and the AI
    // keeps its vanilla behaviour until the design generator is taught to read this)
    public readonly Dictionary<string, string> HullVariantChoice = new Dictionary<string, string>(StringComparer.Ordinal);
    public bool Enabled = true;
    public int LastTurn = -1;

    public BattleSystemState State(ulong systemId)
    {
        BattleSystemState s;
        if (!Systems.TryGetValue(systemId, out s)) { s = new BattleSystemState(); Systems[systemId] = s; }
        return s;
    }

    /// <summary>True when a system has been fought over at all (only then do we keep morale for it):
    /// an untouched empire does not need 400 per-system records.</summary>
    public bool HasState(ulong systemId)
    {
        BattleSystemState s;
        return Systems.TryGetValue(systemId, out s) && s.Morale >= 0;
    }

    // ---------------------------------------------------------------- morale maths

    /// <summary>Morale of a system that has never been recorded: the population's mood turned into will to fight.</summary>
    public static int BaseMorale(float happiness, bool homeSystem)
    {
        if (happiness < 0f) happiness = 0f;              // a rebellious population is still not volunteering to die
        float v = MoraleBase + happiness * MoralePerHappiness;
        if (homeSystem) v += MoraleHomeSystemBonus;
        if (v > MoraleMax - (homeSystem ? 0 : MoraleHomeSystemBonus)) v = MoraleMax - (homeSystem ? 0 : MoraleHomeSystemBonus);
        return Clamp((int)Math.Round(v));     // only the home systems may reach the ceiling
    }

    /// <summary>Cap applied while the system is still reeling from its capture (vanilla keeps the happiness
    /// malus for a few turns, we keep the morale low for the same time). Reaching 0 means: capitulates at once.</summary>
    public static int OccupationCap(int turnsSinceCapture)
    {
        if (turnsSinceCapture < 0 || turnsSinceCapture > OccupationMoraleTurns) return MoraleMax;
        // 12 -> 100 over the occupation window, so a counter-attack the next turn is free and by turn ~5 it fights
        float t = Math.Max(0, turnsSinceCapture) / (float)OccupationMoraleTurns;
        return Clamp((int)Math.Round(MoraleOccupiedCap + (MoraleMax - MoraleOccupiedCap) * t));
    }

    /// <summary>One turn of bookkeeping for a system. Returns the new morale. Deltas are applied to the
    /// recorded value (or to the base value the first time the system is touched).</summary>
    public static int TickMorale(int current, bool battle, bool besieged, bool bombarded,
                                int defenderLossPercent, bool militiaTemporary, bool militiaPermanent,
                                bool peaceful, int occupationCap, bool capitaledAlready)
    {
        int v = current;
        if (battle) v -= MoralePerBattleTurn;
        if (besieged) v -= MoralePerSiegeTurn;
        if (bombarded) v -= MoralePerRazeTurn;
        if (MoralePerDefenderLossPercent > 0 && defenderLossPercent > 0)
            v -= defenderLossPercent * MoralePerDefenderLossPercent;
        if (militiaTemporary) v -= MoraleMilitiaTemporary;
        if (militiaPermanent) v -= MoraleMilitiaPermanent;
        if (peaceful && !capitaledAlready) v += (occupationCap < MoraleMax ? MoraleRegenOccupied : MoraleRegen);
        if (occupationCap < v) v = occupationCap;         // the occupation malus holds the morale down
        return Clamp(v);
    }

    /// <summary>A defence at 0 morale is over: it capitulates, in a battle as well as under bombardment.</summary>
    public static bool IsCollapsed(int morale) { return morale <= 0; }

    static int Clamp(int v) { return v < 0 ? 0 : (v > MoraleMax ? MoraleMax : v); }

    // ---------------------------------------------------------------- crew reserve maths

    /// <summary>Reserve of a hull, from its size class (the numbers the vanilla hull classes put into MaximumShipManpower).</summary>
    public static int HullReserve(string hullSize, bool mothership, int mothershipLevel)
    {
        int baseReserve;
        if (hullSize == null) baseReserve = CrewReserveMedium;
        else if (hullSize.IndexOf("Mothership", StringComparison.OrdinalIgnoreCase) >= 0) baseReserve = CrewReserveMothership;
        else if (hullSize.IndexOf("Small", StringComparison.OrdinalIgnoreCase) >= 0) baseReserve = CrewReserveSmall;
        else if (hullSize.IndexOf("Large", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 hullSize.IndexOf("Carrier", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 hullSize.IndexOf("Juggernaut", StringComparison.OrdinalIgnoreCase) >= 0) baseReserve = CrewReserveLarge;
        else baseReserve = CrewReserveMedium;
        if (mothership) baseReserve += CrewReserveMothershipPerLevel * Math.Max(0, mothershipLevel);
        return baseReserve;
    }

    /// <summary>What stays on board: the hull reserve, never more than 75% of the crew the ship actually has,
    /// and never more than the ship has. Extra manpower from crew modules / hero skills above it is assault force.</summary>
    public static int ReserveFor(float maxManpower, float stock, int hullReserve)
    {
        if (stock <= 0f) return 0;
        float byRatio = maxManpower * CrewReserveRatio;
        float r = hullReserve < byRatio ? hullReserve : byRatio;
        if (r > stock) r = stock;
        if (r < 0f) r = 0f;
        return (int)Math.Floor(r);
    }

    /// <summary>Manpower a ship can put on the ground.</summary>
    public static float AssaultFor(float stock, int reserve)
    {
        float v = stock - reserve;
        return v < 0f ? 0f : v;
    }

    /// <summary>0..1, how well crewed the ship is against its own reserve (1 = at or above the reserve).</summary>
    public static float CrewRatio(float stock, int reserve)
    {
        if (reserve <= 0) return 1f;
        float v = stock / (float)reserve;
        return v > 1f ? 1f : (v < 0f ? 0f : v);
    }

    // ---------------------------------------------------------------- persistence

    public string Serialize()
    {
        var sb = new StringBuilder();
        sb.Append("BR1|").Append(Enabled ? 1 : 0).Append('|').Append(I(LastTurn)).Append('|');
        var parts = new List<string>();
        foreach (var kv in Systems)
        {
            var s = kv.Value;
            parts.Add(kv.Key + ":" + I(s.Morale) + ":" + I(s.LastBattleTurn) + ":" + I(s.LastSiegeTurn)
                      + ":" + I(s.LastRazeTurn) + ":" + I(s.OccupationMarkTurn));
        }
        parts.Sort();
        sb.Append(string.Join(";", parts.ToArray())).Append('|');
        parts.Clear();
        foreach (var kv in Designs)
        {
            var b = kv.Value;
            if (b == null || string.IsNullOrEmpty(b.TemplateId)) continue;
            parts.Add(kv.Key + ":" + Esc(b.TemplateId) + ":" + Esc(b.Hull) + ":" + Esc(b.Role) + ":" + Esc(b.WeaponFamily) + ":" + Esc(b.DesignName));
        }
        parts.Sort();
        sb.Append(string.Join(";", parts.ToArray())).Append('|');
        parts.Clear();
        foreach (var kv in HullVariantChoice)
        {
            if (string.IsNullOrEmpty(kv.Key) || string.IsNullOrEmpty(kv.Value)) continue;
            parts.Add(Esc(kv.Key) + ":" + Esc(kv.Value));
        }
        parts.Sort();
        sb.Append(string.Join(";", parts.ToArray()));
        return sb.ToString();
    }

    public static BattleCore Deserialize(string data)
    {
        var b = new BattleCore();
        if (string.IsNullOrEmpty(data)) return b;
        try
        {
            var f = data.Split('|');
            if (f.Length < 4 || f[0] != "BR1") return b;
            b.Enabled = f[1] == "1";
            b.LastTurn = P(f[2]);
            foreach (var rec in Split(f[3], ';'))
            {
                var t = rec.Split(':');
                if (t.Length < 6) continue;
                var s = new BattleSystemState
                {
                    Morale = P(t[1]),
                    LastBattleTurn = P(t[2]),
                    LastSiegeTurn = P(t[3]),
                    LastRazeTurn = P(t[4]),
                    OccupationMarkTurn = P(t[5])
                };
                ulong id;
                if (ulong.TryParse(t[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out id)) b.Systems[id] = s;
            }
            if (f.Length >= 6)
            {
                foreach (var rec in Split(f[5], ';'))
                {
                    var t = rec.Split(':');
                    if (t.Length < 2) continue;
                    string hull = Unesc(t[0]);
                    if (hull.Length > 0) b.HullVariantChoice[hull] = Unesc(t[1]);
                }
            }
            if (f.Length >= 5)
            {
                foreach (var rec in Split(f[4], ';'))
                {
                    var t = rec.Split(':');
                    if (t.Length < 5) continue;   // the design name is optional (older records)
                    ulong id;
                    if (!ulong.TryParse(t[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out id)) continue;
                    b.Designs[id] = new BattleTemplateBinding
                    {
                        TemplateId = Unesc(t[1]),
                        Hull = Unesc(t[2]),
                        Role = Unesc(t[3]),
                        WeaponFamily = Unesc(t[4]),
                        DesignName = t.Length > 5 ? Unesc(t[5]) : ""
                    };
                }
            }
        }
        catch (Exception) { return b; }
        return b;
    }

    static string I(int v) { return v.ToString(CultureInfo.InvariantCulture); }
    static int P(string s) { int v; return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v) ? v : 0; }
    static string[] Split(string s, char c) { return string.IsNullOrEmpty(s) ? new string[0] : s.Split(c); }
    static string Esc(string s) { return string.IsNullOrEmpty(s) ? "" : s.Replace(":", "~").Replace(";", "`"); }
    static string Unesc(string s) { return string.IsNullOrEmpty(s) ? "" : s.Replace('~', ':').Replace('`', ';'); }
}
