// Hero rework, pure logic (no game types): kinds of heroes and the effect channels each kind may use.
using System;

public enum HeroKind { Admiral = 0, Governor = 1, Senator = 2 }

/// <summary>Where a skill effect is applied: the hero itself, the star system he governs, the fleet he commands or the senate.</summary>
public enum HeroChannel { Hero = 0, System = 1, Ship = 2, Senate = 3 }

public static class HeroKinds
{
    /// <summary>The kind of a hero is fixed for his whole life and derived from his entity id: about a third of every kind, no saved state needed.</summary>
    public static HeroKind KindOfGuid(ulong guid)
    {
        return (HeroKind)(int)(guid % 3UL);
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
}
