// Hero rework, pure logic (no game types): kinds of heroes and the effect channels each kind may use.
using System;

public enum HeroKind { Admiral = 0, Governor = 1, Senator = 2 }

/// <summary>Where a skill effect is applied: the hero itself, the star system he governs, the fleet he commands or the senate.</summary>
public enum HeroChannel { Hero = 0, System = 1, Ship = 2, Senate = 3 }

public static class HeroKinds
{
    /// <summary>Kind by the vanilla hero class (HeroClassDefinition name). Attacker and explorer classes fight with fleets, administrators govern, corporates sit in the senate.</summary>
    public static bool TryKindOfClass(string className, out HeroKind kind)
    {
        kind = HeroKind.Admiral;
        if (className == null) return false;
        if (className.IndexOf("Admiral", StringComparison.OrdinalIgnoreCase) >= 0 || className.IndexOf("Adventurer", StringComparison.OrdinalIgnoreCase) >= 0) { kind = HeroKind.Admiral; return true; }
        if (className.IndexOf("Administrator", StringComparison.OrdinalIgnoreCase) >= 0) { kind = HeroKind.Governor; return true; }
        if (className.IndexOf("Corporate", StringComparison.OrdinalIgnoreCase) >= 0) { kind = HeroKind.Senator; return true; }
        return false;
    }

    /// <summary>Fallback for heroes of an unknown (modded) class: spread by entity id, so nothing is left without a role.</summary>
    public static HeroKind KindOfGuid(ulong guid)
    {
        return (HeroKind)(int)(guid % 3UL);
    }

    public static HeroKind KindOf(string className, ulong guid)
    {
        HeroKind k;
        return TryKindOfClass(className, out k) ? k : KindOfGuid(guid);
    }

    /// <summary>Admirals only use ship effects, governors only system effects, senators only senate effects. Effects on the hero himself are universal.</summary>
    public static bool Allows(HeroKind kind, HeroChannel channel)
    {
        switch (channel)
        {
            case HeroChannel.Hero: return true;
            case HeroChannel.System: return kind == HeroKind.Governor;
            case HeroChannel.Ship: return kind == HeroKind.Admiral;
            case HeroChannel.Senate: return kind == HeroKind.Senator;
        }
        return false;
    }

    public static string Name(HeroKind kind, bool russian)
    {
        switch (kind)
        {
            case HeroKind.Admiral: return russian ? "Адмирал" : "Admiral";
            case HeroKind.Governor: return russian ? "Губернатор" : "Governor";
            default: return russian ? "Сенатор" : "Senator";
        }
    }

    /// <summary>Skill points a stage can really ask for: stage thresholds are lowered when filtering left too few skills below them.</summary>
    public static int ClampRequiredLevel(int required, int availableBelow)
    {
        if (required <= 0 || availableBelow >= required) return required;
        int v = (int)Math.Floor(availableBelow * 0.8);
        return v < 0 ? 0 : v;
    }

    /// <summary>Fraction of its base effect every learned skill gains per hero level after the ultimate: (100 / level the ultimate was taken at) percent.</summary>
    public static float AutoBonusFraction(int ultimateLevel, int heroLevel)
    {
        if (ultimateLevel < 1) ultimateLevel = 1;
        int steps = heroLevel - ultimateLevel;
        if (steps <= 0) return 0f;
        return steps / (float)ultimateLevel;
    }

    /// <summary>Scales one modifier value for an operation. Returns false when the operation cannot be scaled (no bonus).</summary>
    public static bool ScaleModifierValue(int operation, float value, float fraction, out float scaled)
    {
        // ModifierOperation: 1 Addition, 2 Subtraction, 3 Multiplication, 6 Percent
        scaled = value;
        switch (operation)
        {
            case 1:
            case 2:
            case 6:
                scaled = value * fraction;
                return true;
            case 3:
                scaled = 1f + (value - 1f) * fraction;
                return true;
        }
        return false;
    }

    // ---- ultimate registry persistence: "guid:level,guid:level"

    public static string SerializeUltimates(System.Collections.Generic.Dictionary<ulong, int> map)
    {
        var sb = new System.Text.StringBuilder();
        var keys = new System.Collections.Generic.List<ulong>(map.Keys);
        keys.Sort();
        foreach (ulong k in keys)
        {
            if (sb.Length > 0) sb.Append(',');
            sb.Append(k).Append(':').Append(map[k]);
        }
        return sb.ToString();
    }

    public static System.Collections.Generic.Dictionary<ulong, int> ParseUltimates(string text)
    {
        var map = new System.Collections.Generic.Dictionary<ulong, int>();
        if (string.IsNullOrEmpty(text)) return map;
        foreach (string part in text.Split(','))
        {
            int c = part.IndexOf(':');
            if (c <= 0) continue;
            ulong g; int lvl;
            if (ulong.TryParse(part.Substring(0, c), out g) && int.TryParse(part.Substring(c + 1), out lvl) && lvl > 0) map[g] = lvl;
        }
        return map;
    }

    // ---- senators (not the dictator) get a few levels only; the dictator is limited by the vanilla rule alone

    public const int SenatorLevelMax = 8;           // level 1 -> 8 = seven level-ups: "6-8 levels over the whole game"
    public const float DictatorUltimateBoost = 0f;    // the dictator has five levels per skill and a stronger ultimate of his own: no extra multiplier

    // ---- passive experience: senators sit in the senate and never see a battle or a construction site, so they grow with time

    public const int SenatorXpTurnsPerLevel = 10;    // a senator gets a level every ~10 turns (until SenatorLevelMax)
    public const int DictatorXpTurnsPerLevel = 6;    // the dictator grows faster: he is the ruler of the faction

    /// <summary>Experience a senator hero gains each turn: the share of the experience his next level asks for.</summary>
    public static float PassiveExperience(float nextLevelExperience, bool dictator)
    {
        if (nextLevelExperience <= 0f) return 0f;
        return nextLevelExperience / (dictator ? DictatorXpTurnsPerLevel : SenatorXpTurnsPerLevel);
    }

    /// <summary>Skill points shown as unspent: a hero who took his ultimate has nothing left to spend them on, so later levels do not ask for them.</summary>
    public static int VisibleSkillPoints(int points, bool fixedByUltimate)
    {
        return fixedByUltimate ? 0 : points;
    }

    // ---- name of the definition of a dictator hero: HeroDefinitionGCDictator_<faction>_<politics>

    public const string DictatorDefinitionPrefix = "HeroDefinitionGCDictator_";

    public static string DictatorDefinitionName(string faction, string politics)
    {
        return DictatorDefinitionPrefix + faction + "_" + politics;
    }

    public static bool IsDictatorDefinitionName(string name)
    {
        return name != null && name.StartsWith(DictatorDefinitionPrefix, StringComparison.Ordinal);
    }

    /// <summary>The level cap to put on a hero's level-up rule: negative = leave the rule alone.</summary>
    public static int LevelCapFor(HeroKind kind, bool dictator, int ruleLevelMax)
    {
        if (kind != HeroKind.Senator || dictator) return -1;
        if (ruleLevelMax >= 0 && ruleLevelMax <= SenatorLevelMax) return -1;
        return SenatorLevelMax;
    }

    /// <summary>Total share of its base effect that the learned skills of a hero get on top: fixed boost of the dictator plus the per level growth after the ultimate.</summary>
    public static float TotalBonusFraction(int ultimateLevel, int heroLevel, bool dictator)
    {
        return AutoBonusFraction(ultimateLevel, heroLevel) + (dictator ? DictatorUltimateBoost : 0f);
    }

    // ---- dictatorship registry persistence: "empireIndex=politics=senatorGuid,..."

    public static string SerializeDictators(System.Collections.Generic.Dictionary<int, System.Collections.Generic.KeyValuePair<string, ulong>> map)
    {
        var sb = new System.Text.StringBuilder();
        var keys = new System.Collections.Generic.List<int>(map.Keys);
        keys.Sort();
        foreach (int k in keys)
        {
            if (sb.Length > 0) sb.Append(',');
            sb.Append(k).Append('=').Append(map[k].Key).Append('=').Append(map[k].Value);
        }
        return sb.ToString();
    }

    public static System.Collections.Generic.Dictionary<int, System.Collections.Generic.KeyValuePair<string, ulong>> ParseDictators(string text)
    {
        var map = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.KeyValuePair<string, ulong>>();
        if (string.IsNullOrEmpty(text)) return map;
        foreach (string part in text.Split(','))
        {
            string[] f = part.Split('=');
            int e; ulong g;
            if (f.Length != 3 || f[1].Length == 0 || !int.TryParse(f[0], out e) || !ulong.TryParse(f[2], out g)) continue;
            map[e] = new System.Collections.Generic.KeyValuePair<string, ulong>(f[1], g);
        }
        return map;
    }

    /// <summary>The persisted hero state is "ultimates;dictators"; older saves have only the first part.</summary>
    public static string JoinState(string ultimates, string dictators) { return ultimates + ";" + dictators; }

    public static void SplitState(string state, out string ultimates, out string dictators)
    {
        ultimates = ""; dictators = "";
        if (string.IsNullOrEmpty(state)) return;
        int cut = state.IndexOf(';');
        if (cut < 0) { ultimates = state; return; }
        ultimates = state.Substring(0, cut);
        dictators = state.Substring(cut + 1);
    }
}
