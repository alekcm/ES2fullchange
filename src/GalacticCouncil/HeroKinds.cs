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
}
