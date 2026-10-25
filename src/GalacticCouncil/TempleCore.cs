// Endless Temples - science victory rework: pure state/logic (no game dependencies, unit-testable like CouncilCore).
//
// The four Endless victory technologies stay locked until the empire "opens" the matching Temple of the
// Ancients: once the game itself would make the technology available, a temple (a special node in the galaxy) is
// revealed to that empire; a research Behemoth must then study it there. The temple node of each technology is
// chosen once (by the first empire that reveals it) and is then the same for everybody.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>Static data of one Endless victory technology and its temple.</summary>
public sealed class TempleDef
{
    public string TechName;        // TechnologyDefinition name in the game data
    public string Ru;              // display name (from the game localization)
    public string En;
    public string[] NodeTokens;    // substrings matched against SpecialNodeDefinition Name/Category

    public TempleDef(string tech, string ru, string en, string[] tokens)
    {
        TechName = tech; Ru = ru; En = en; NodeTokens = tokens;
    }
}

public static class TempleDefs
{
    public const int Power = 0;    // Власть Вечных / Power of the Endless (Empire Development)
    public const int Genius = 1;   // Гений Вечных / Genius of the Endless (Science and Exploration)
    public const int Might = 2;    // Могущество Вечных / Might of the Endless (Military)
    public const int Wealth = 3;   // Достояние Вечных / Wealth of the Endless (Economy and Trade)

    public static readonly TempleDef[] All =
    {
        new TempleDef("TechnologyDefinitionEmpireDevelopmentVictory", "Власть Вечных", "Power of the Endless",
            new[] { "BlackHole", "NeutronStar", "SpaceClouds", "Rejuvenation" }),
        new TempleDef("TechnologyDefinitionScienceAndExplorationVictory", "Гений Вечных", "Genius of the Endless",
            new[] { "BlackHole", "NeutronStar", "AsteroidField", "SolarNebula" }),
        new TempleDef("TechnologyDefinitionMilitaryVictory", "Могущество Вечных", "Might of the Endless",
            new[] { "CollapsingStar", "SpaceClouds", "Rejuvenation" }),
        new TempleDef("TechnologyDefinitionEconomyAndTradeVictory", "Достояние Вечных", "Wealth of the Endless",
            new[] { "CollapsingStar", "AsteroidField", "SolarNebula" }),
    };

    public static int IndexOfTech(string techName)
    {
        for (int i = 0; i < All.Length; i++) if (All[i].TechName == techName) return i;
        return -1;
    }

    /// <summary>Does a special node (by definition name / category) belong to the temple pool of this technology?</summary>
    public static bool NodeMatches(int tech, string defName, string category)
    {
        if (tech < 0 || tech >= All.Length) return false;
        var tokens = All[tech].NodeTokens;
        for (int i = 0; i < tokens.Length; i++)
        {
            if (!string.IsNullOrEmpty(defName) && defName.IndexOf(tokens[i], StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (!string.IsNullOrEmpty(category) && category.IndexOf(tokens[i], StringComparison.OrdinalIgnoreCase) >= 0) return true;
        }
        return false;
    }
}

/// <summary>Per-empire temple progress.</summary>
public sealed class TempleEmpireState
{
    public byte[] State = new byte[TempleCore.TechCount];        // TempleCore.Hidden / Revealed / Opened
    public int[] ProgressHalf = new int[TempleCore.TechCount];   // study progress in half-turns (+2 per turn present, -1 per turn absent)
    public int[] RequiredHalf = new int[TempleCore.TechCount];   // required half-turns fixed at reveal (scaled by game speed); 0 = use default
}

public sealed class TempleCore
{
    public const int TechCount = 4;
    public const int BaseStudyTurns = 10;          // on normal speed; scaled by game speed at reveal
    public const int GainHalfPerTurn = 2;          // +1 turn of study per turn with a research Behemoth at the temple
    public const int DecayHalfPerTurn = 1;         // -0.5 turns per turn without one

    public const int Hidden = 0;
    public const int Revealed = 1;
    public const int Opened = 2;

    public const ulong NodeUnassigned = 0ul;
    public const ulong NodeNoneInGalaxy = ulong.MaxValue;   // no suitable special node exists: the study step is skipped

    /// <summary>Temple node GUID per technology: shared by every empire, assigned at the first reveal.</summary>
    public readonly ulong[] TempleNode = new ulong[TechCount];

    public readonly Dictionary<int, TempleEmpireState> Empires = new Dictionary<int, TempleEmpireState>();

    TempleEmpireState Of(int emp)
    {
        TempleEmpireState s;
        if (!Empires.TryGetValue(emp, out s) || s == null) { s = new TempleEmpireState(); Empires[emp] = s; }
        return s;
    }

    public int StateOf(int emp, int tech)
    {
        if (tech < 0 || tech >= TechCount) return Hidden;
        TempleEmpireState s;
        return Empires.TryGetValue(emp, out s) && s != null ? s.State[tech] : Hidden;
    }

    public bool IsRevealed(int emp, int tech) { return StateOf(emp, tech) >= Revealed; }
    public bool IsOpened(int emp, int tech) { return StateOf(emp, tech) >= Opened; }

    public int ProgressHalf(int emp, int tech)
    {
        if (tech < 0 || tech >= TechCount) return 0;
        TempleEmpireState s;
        return Empires.TryGetValue(emp, out s) && s != null ? s.ProgressHalf[tech] : 0;
    }

    public int RequiredHalf(int emp, int tech)
    {
        if (tech < 0 || tech >= TechCount) return BaseStudyTurns * 2;
        TempleEmpireState s;
        if (Empires.TryGetValue(emp, out s) && s != null && s.RequiredHalf[tech] > 0) return s.RequiredHalf[tech];
        return BaseStudyTurns * 2;
    }

    /// <summary>Progress as "turns studied" (can be x.5 while decaying).</summary>
    public float ProgressTurns(int emp, int tech) { return ProgressHalf(emp, tech) / 2f; }
    public float RequiredTurns(int emp, int tech) { return RequiredHalf(emp, tech) / 2f; }

    /// <summary>Fixes the temple node of a technology for everybody (no-op once assigned).</summary>
    public bool AssignTemple(int tech, ulong nodeGuid)
    {
        if (tech < 0 || tech >= TechCount || TempleNode[tech] != NodeUnassigned) return false;
        TempleNode[tech] = nodeGuid;
        return true;
    }

    public bool IsTempleNode(ulong nodeGuid)
    {
        if (nodeGuid == NodeUnassigned || nodeGuid == NodeNoneInGalaxy) return false;
        for (int i = 0; i < TechCount; i++) if (TempleNode[i] == nodeGuid) return true;
        return false;
    }

    /// <summary>
    /// Reveals the temple of a technology to an empire. Assigns the shared node if needed (use NodeNoneInGalaxy
    /// when the galaxy has no suitable special node). Returns true when this call revealed it; when there is no
    /// temple node in the galaxy the technology is opened right away (and the caller should report it differently).
    /// </summary>
    public bool Reveal(int emp, int tech, ulong nodeGuid, int requiredHalfTurns)
    {
        if (tech < 0 || tech >= TechCount) return false;
        AssignTemple(tech, nodeGuid);
        var s = Of(emp);
        if (s.State[tech] >= Revealed) return false;
        if (requiredHalfTurns > 0) s.RequiredHalf[tech] = requiredHalfTurns;
        s.ProgressHalf[tech] = 0;
        s.State[tech] = TempleNode[tech] == NodeNoneInGalaxy ? (byte)Opened : (byte)Revealed;
        return true;
    }

    /// <summary>Marks as opened directly (technology already researched, e.g. a save from before this mod).</summary>
    public void MarkOpened(int emp, int tech)
    {
        if (tech < 0 || tech >= TechCount) return;
        var s = Of(emp);
        s.State[tech] = (byte)Opened;
        s.ProgressHalf[tech] = Math.Max(s.ProgressHalf[tech], RequiredHalf(emp, tech));
    }

    /// <summary>
    /// One turn of temple study for an empire: with a research Behemoth at the temple the progress grows
    /// by one turn, without one it falls by half a turn (never below zero). The Behemoth may be replaced
    /// by another one - only presence matters. Returns true exactly on the turn the temple opens.
    /// </summary>
    public bool Tick(int emp, int tech, bool behemothPresent)
    {
        if (tech < 0 || tech >= TechCount) return false;
        var s = Of(emp);
        if (s.State[tech] != Revealed) return false;
        if (behemothPresent) s.ProgressHalf[tech] += GainHalfPerTurn;
        else s.ProgressHalf[tech] = Math.Max(0, s.ProgressHalf[tech] - DecayHalfPerTurn);
        if (s.ProgressHalf[tech] >= RequiredHalf(emp, tech))
        {
            s.State[tech] = (byte)Opened;
            return true;
        }
        return false;
    }

    /// <summary>How many of the four Endless technologies the empire has researched count-wise (opened temples are not enough: the caller passes the researched flags).</summary>
    public static int CountOpened(TempleCore core, int emp)
    {
        int n = 0;
        for (int i = 0; i < TechCount; i++) if (core != null && core.IsOpened(emp, i)) n++;
        return n;
    }

    // ---------------------------------------------------------------- deterministic temple choice

    /// <summary>
    /// Picks a temple node GUID: neutral candidates first (not inside anyone's influence),
    /// then any candidates; deterministic for a given seed on every machine (inputs must be sorted by GUID).
    /// Returns NodeNoneInGalaxy when there are no candidates at all.
    /// </summary>
    public static ulong PickTemple(List<ulong> neutralSorted, List<ulong> allSorted, int seed)
    {
        var pool = neutralSorted != null && neutralSorted.Count > 0 ? neutralSorted : allSorted;
        if (pool == null || pool.Count == 0) return NodeNoneInGalaxy;
        // deterministic xorshift, same on every machine (standalone copy: TempleCore must stay testable alone)
        uint s = (uint)(seed * 2654435761u + 0x9E3779B9u);
        if (s == 0) s = 0x1234567u;
        for (int i = 0; i < 4; i++) { s ^= s << 13; s ^= s >> 17; s ^= s << 5; }
        s ^= s << 13; s ^= s >> 17; s ^= s << 5;
        return pool[(int)(s % (uint)pool.Count)];
    }

    // ---------------------------------------------------------------- persistence

    public string Serialize()
    {
        var sb = new StringBuilder();
        sb.Append("TM1");
        for (int i = 0; i < TechCount; i++) sb.Append(';').Append(TempleNode[i].ToString(CultureInfo.InvariantCulture));
        var emps = new List<int>(Empires.Keys);
        emps.Sort();
        for (int e = 0; e < emps.Count; e++)
        {
            var s = Empires[emps[e]];
            if (s == null) continue;
            bool any = false;
            for (int i = 0; i < TechCount; i++) if (s.State[i] != Hidden || s.ProgressHalf[i] != 0) { any = true; break; }
            if (!any) continue;
            sb.Append(';').Append(emps[e].ToString(CultureInfo.InvariantCulture)).Append(':');
            for (int i = 0; i < TechCount; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(((int)s.State[i]).ToString(CultureInfo.InvariantCulture)).Append('.')
                  .Append(s.ProgressHalf[i].ToString(CultureInfo.InvariantCulture)).Append('.')
                  .Append(s.RequiredHalf[i].ToString(CultureInfo.InvariantCulture));
            }
        }
        return sb.ToString();
    }

    public static TempleCore Deserialize(string data)
    {
        var core = new TempleCore();
        if (string.IsNullOrEmpty(data)) return core;
        try
        {
            var parts = data.Split(';');
            if (parts.Length < 1 + TechCount || parts[0] != "TM1") return core;
            for (int i = 0; i < TechCount; i++)
            {
                ulong g;
                if (ulong.TryParse(parts[1 + i], NumberStyles.Integer, CultureInfo.InvariantCulture, out g)) core.TempleNode[i] = g;
            }
            for (int p = 1 + TechCount; p < parts.Length; p++)
            {
                var kv = parts[p].Split(':');
                if (kv.Length != 2) continue;
                int emp;
                if (!int.TryParse(kv[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out emp)) continue;
                var entries = kv[1].Split(',');
                if (entries.Length != TechCount) continue;
                var s = core.Of(emp);
                for (int i = 0; i < TechCount; i++)
                {
                    var nums = entries[i].Split('.');
                    int st, pr, rq;
                    if (nums.Length == 3
                        && int.TryParse(nums[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out st)
                        && int.TryParse(nums[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out pr)
                        && int.TryParse(nums[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out rq))
                    {
                        s.State[i] = (byte)Math.Max(0, Math.Min(2, st));
                        s.ProgressHalf[i] = Math.Max(0, pr);
                        s.RequiredHalf[i] = Math.Max(0, rq);
                    }
                }
            }
        }
        catch (Exception) { return new TempleCore(); }
        return core;
    }
}
