// Battle rework / phase 2 - the ship template catalogue, pure part (no game types, unit-tested by
// tests/TemplateCoreTests.cs; the game-side data gathering lives in BattleTemplates.cs).
//
// A template is NOT "hull + role": the vanilla ShipDesignTemplateDefinitions give one template per hull
// (85 of them), and inside that template the weapon slot offers several families with weights. The same
// "small attacker" hull can therefore end up kinetic, missile, laser or beam, and that choice - not the
// hull - is what the player actually feels. So the variant identity (name + description) below is derived
// from the properties of the modules the template equips: the AI range profile (Use short/medium/long),
// the damage type, what armour it prefers to punch through, and whether it carries flak.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>The weapon-module facts a variant is derived from (filled from the game's own ModuleDefinition data).</summary>
public sealed class TemplateModuleInfo
{
    public string Family;
    public string WeaponType;         // "Kinetic" | "Missile" | "Laser" | "Beam"
    public float RangeShort;          // AIBattleSituations Use/Short  (0 = cannot fight there, 1 = best there)
    public float RangeMedium;
    public float RangeLong;
    public float VsEnergyArmor;       // AIBattleSituations Against/EnergyDefense
    public float VsProjectileArmor;   // AIBattleSituations Against/ProjectileDefense
    public float VsKinetic;           // Against/Kinetic  (positive = good against that weapon)
    public float VsMissile;
    public float VsLaser;
    public float VsBeam;
    public float VsFighter;           // Against/Fighter (+ Bomber folded in by the loader)
    public bool HasFlak;              // the module carries a flak slave module (shoots missiles/fighters)
    public bool IsStrategic;          // a strategic-resource module: better, but needs the resource
    public float Dps;                 // DamagePerSecond of the module descriptor (0 = unknown)
    public float LongPenalty;         // ShipModule_Weapon:LongRangePenalty   (0 = no penalty at long range)
    public float ShortPenalty;        // ShipModule_Weapon:ShortRangePenalty
    public int BestLevel;             // highest module level available in the family
}

/// <summary>One template variant: the identity a design built from a (hull, role, weapon family) gets.</summary>
public sealed class TemplateVariant
{
    public string Key;                // "Assault" | "Sniper" | "Universal" | "Barrage"
    public string NameRu;
    public string NameEn;
    public string ProfileRu;         // "18 / 32 / 0 (S/M/L)" - the numbers the derivation was made of
    public string ArmorRu;            // "бьёт энергетическую броню, прячется от снарядной"
    public string DamageRu;         // "энергетический" | "снарядный"
    public string ExtraRu;           // "зенитки", "стратегический ресурс"
}

/// <summary>A catalogue entry: one buildable template (hull + role + weapon family) and what it becomes.</summary>
public sealed class BattleTemplate
{
    public string Id;                 // "<Hull>_<Role>_<Family>" - stable, used as the registered template name
    public string ParentId;           // the vanilla ShipDesignTemplateDefinition this was split from
    public string Hull;
    public string Role;
    public string Family;
    public TemplateVariant Variant;
    public TemplateModuleInfo Info;   // the weapon facts the variant was derived from
    public int VariantIndex;          // position in the hull's variant list (the cycle order the player sees)
    public string Description;        // the short text shown in the UI (already language-picked)
    public int Weight;                // catalogue order hint (T1 families first)
}

/// <summary>The maths of the catalogue: classification, naming, descriptions, matchup scores.</summary>
public static class TemplateCore
{
    public const string Assault = "Assault";
    public const string Sniper = "Sniper";
    public const string Universal = "Universal";
    public const string Barrage = "Barrage";

    // The game writes 0 / 0.5 / 1 into the AI range profile. "Primary" is where the AI wants to fight,
    // "open" is any band it is not helpless in - the two thresholds the classification below works with.
    public const float RangeBandOpen = 0.25f;
    public const float RangeBandPrimary = 0.75f;

    /// <summary>Which variant a weapon family plays: only its range profile (plus the flak slave module) decides.
    /// Nothing but long range = sniper, nothing but short range = assault, any real spread = universal, and a
    /// weapon that carries flak is a barrage screen whatever its bands, because that is what the game makes it do.</summary>
    public static string Classify(TemplateModuleInfo w)
    {
        if (w == null) return Universal;
        if (w.HasFlak) return Barrage;
        bool s = w.RangeShort >= RangeBandPrimary, m = w.RangeMedium >= RangeBandPrimary, l = w.RangeLong >= RangeBandPrimary;
        if (l && !s && !m) return Sniper;
        if (s && !m && !l) return Assault;
        return Universal;
    }

    /// <summary>1 = the whole range band is open, 0 = nothing but one band. Used for the wording.</summary>
    public static int BandsUsed(TemplateModuleInfo w)
    {
        int n = 0;
        if (w.RangeShort >= RangeBandOpen) n++;
        if (w.RangeMedium >= RangeBandOpen) n++;
        if (w.RangeLong >= RangeBandOpen) n++;
        return n;
    }

    public static string NameRu(string variantKey, string hullSize)
    {
        string core;
        switch (variantKey)
        {
            case Sniper: core = "Снайпер"; break;
            case Barrage: core = "Заградительный огонь"; break;
            case Assault: core = "Штурмовик"; break;
            default: core = "Универсал"; break;
        }
        string size = SizeRu(hullSize);
        return size == null ? core : core + " (" + size + ")";
    }

    public static string NameEn(string variantKey, string hullSize)
    {
        string core;
        switch (variantKey)
        {
            case Sniper: core = "Sniper"; break;
            case Barrage: core = "Barrage screen"; break;
            case Assault: core = "Assault"; break;
            default: core = "Universal"; break;
        }
        string size = SizeEn(hullSize);
        return size == null ? core : core + " (" + size + ")";
    }

    static string SizeRu(string hullSize)
    {
        if (string.IsNullOrEmpty(hullSize)) return null;
        if (hullSize.IndexOf("Small", StringComparison.OrdinalIgnoreCase) >= 0) return "малый";
        if (hullSize.IndexOf("Medium", StringComparison.OrdinalIgnoreCase) >= 0) return "средний";
        if (hullSize.IndexOf("Large", StringComparison.OrdinalIgnoreCase) >= 0
            || hullSize.IndexOf("Carrier", StringComparison.OrdinalIgnoreCase) >= 0
            || hullSize.IndexOf("Juggernaut", StringComparison.OrdinalIgnoreCase) >= 0) return "тяжёлый";
        if (hullSize.IndexOf("Mothership", StringComparison.OrdinalIgnoreCase) >= 0) return "материнский корабль";
        return null;
    }

    static string SizeEn(string hullSize)
    {
        if (string.IsNullOrEmpty(hullSize)) return null;
        if (hullSize.IndexOf("Small", StringComparison.OrdinalIgnoreCase) >= 0) return "small";
        if (hullSize.IndexOf("Medium", StringComparison.OrdinalIgnoreCase) >= 0) return "medium";
        if (hullSize.IndexOf("Large", StringComparison.OrdinalIgnoreCase) >= 0
            || hullSize.IndexOf("Carrier", StringComparison.OrdinalIgnoreCase) >= 0
            || hullSize.IndexOf("Juggernaut", StringComparison.OrdinalIgnoreCase) >= 0) return "heavy";
        if (hullSize.IndexOf("Mothership", StringComparison.OrdinalIgnoreCase) >= 0) return "mothership";
        return null;
    }

    /// <summary>Which armour the family punches: Against/EnergyDefense &gt; Against/ProjectileDefense means the
    /// module is meant against shielded ships (and the game calls its damage projectile), and the other way
    /// round for the energy weapons, which shields absorb. Same two numbers, one label.</summary>
    public static string DamageKindRu(TemplateModuleInfo w)
    {
        if (w == null) return "";
        float e = w.VsEnergyArmor, p = w.VsProjectileArmor;
        if (e > p) return "по энергетической броне";
        if (p > e) return "по снарядной броне";
        return "безразлично к броне";
    }

    public static string ArmorRu(TemplateModuleInfo w)
    {
        if (w == null) return "";
        float e = w.VsEnergyArmor, p = w.VsProjectileArmor;
        if (e > p) return "пробивает энергетическую броню, бесполезен против снарядной";
        if (p > e) return "пробивает снарядную броню, бесполезен против энергетической";
        return "не зависит от типа брони";
    }

    /// <summary>"хорош против X, плох против Y", from the Against/<weapon> values.</summary>
    public static string MatchupRu(TemplateModuleInfo w)
    {
        if (w == null) return "";
        var good = new List<string>();
        var bad = new List<string>();
        // "хорош против / плох против" wants the genitive - the words are the genitive forms of the weapon names
        Add(good, bad, w.VsKinetic, "кинетики");
        Add(good, bad, w.VsMissile, "ракет");
        Add(good, bad, w.VsLaser, "лазеров");
        Add(good, bad, w.VsBeam, "лучей");
        Add(good, bad, w.VsFighter, "истребителей и бомбардировщиков");
        var sb = new StringBuilder();
        if (good.Count > 0) sb.Append("хорош против ").Append(string.Join(", ", good.ToArray()));
        if (bad.Count > 0)
        {
            if (sb.Length > 0) sb.Append(", ");
            sb.Append("плох против ").Append(string.Join(", ", bad.ToArray()));
        }
        return sb.ToString();
    }

    static void Add(List<string> good, List<string> bad, float v, string what)
    {
        if (v >= 0.5f) good.Add(what);
        else if (v <= -0.5f) bad.Add(what);
    }

    /// <summary>Role label with no hull-size suffix (the "Роль — ..." part of the description).</summary>
    public static string VariantNameRu(string variantKey) { return NameRu(variantKey, null); }

    /// <summary>Range profile in the words the player uses ("держится в дальнем ряду").</summary>
    public static string PositionRu(TemplateModuleInfo w)
    {
        if (w == null) return "";
        switch (Classify(w))
        {
            case Sniper: return "держится в дальнем ряду, вблизи беззащитен";
            case Barrage: return "прикрывает флот на короткой дистанции, бьёт по всему, что подлетит";
            case Assault: return "бьёт в упор и на средней дистанции, далеко не дотягивается";
            default: return "одинаково бьёт на любой дистанции";
        }
    }

    /// <summary>The numbers the classification was made of - shown in the debug/tip text so the player can
    /// check that the label is not magic (and so a modder can spot a data change in the vanilla modules).</summary>
    public static string Profile(TemplateModuleInfo w)
    {
        if (w == null) return "";
        return Num(w.RangeShort) + " / " + Num(w.RangeMedium) + " / " + Num(w.RangeLong) + " (S/M/L)";
    }

    static string Num(float v) { return v.ToString("0.##", CultureInfo.InvariantCulture); }

    /// <summary>
    /// The short description of a variant, all of it derived from the equipped modules:
    /// "Корпус — малый атакующий. Роль — снайпер: держится в дальнем ряду, вблизи беззащитен.
    ///  N энергии, XX/YY/ZZ (S/M/L), пробивает снарядную броню; хорош против ракет, плох против кинетики."
    /// </summary>
    public static string Describe(string hullSize, string roleLabel, TemplateModuleInfo w, bool russian)
    {
        if (!russian) return DescribeEn(hullSize, roleLabel, w);
        var sb = new StringBuilder();
        string size = SizeRu(hullSize);
        sb.Append("Корпус — ");
        if (size != null) sb.Append(size).Append(' ');
        sb.Append(string.IsNullOrEmpty(roleLabel) ? "боевой корабль" : roleLabel).Append(". ");
        sb.Append("Роль — ").Append(NameRu(Classify(w), null)).Append(": ").Append(PositionRu(w)).Append(". ");
        if (w != null && w.Dps > 0f) sb.Append(Num(w.Dps)).Append(" урона в секунду (").Append(DamageKindRu(w)).Append("). ");
        sb.Append("Дистанции ").Append(Profile(w)).Append(". ");
        string armor = ArmorRu(w);
        if (armor.Length > 0) sb.Append(armor[0]).Append(char.ToUpperInvariant(armor[1])).Append(armor.Substring(2)).Append(". ");
        string mu = MatchupRu(w);
        if (mu.Length > 0) sb.Append(mu).Append(". ");
        var extra = new List<string>();
        if (w != null && w.HasFlak) extra.Add("есть зенитки");
        if (w != null && w.IsStrategic) extra.Add("нужен стратегический ресурс");
        if (w != null && w.LongPenalty > 0f && w.RangeLong >= RangeBandOpen) extra.Add("на дальней дистанции штраф " + Num(w.LongPenalty * 100f) + "%");
        if (extra.Count > 0) sb.Append(string.Join("; ", extra.ToArray())).Append(".");
        return sb.ToString();
    }

    public static string DescribeEn(string hullSize, string roleLabel, TemplateModuleInfo w)
    {
        var sb = new StringBuilder();
        string size = SizeEn(hullSize);
        sb.Append("Hull: ");
        if (size != null) sb.Append(size).Append(' ');
        sb.Append(string.IsNullOrEmpty(roleLabel) ? "warship" : roleLabel).Append(". ");
        sb.Append("Role: ").Append(NameEn(Classify(w), null)).Append(" - ").Append(PositionRu(w)).Append(". ");
        if (w != null && w.Dps > 0f) sb.Append(Num(w.Dps)).Append(" damage per second. ");
        sb.Append("Range profile ").Append(Profile(w)).Append(".");
        return sb.ToString();
    }

    /// <summary>The id a variant template is registered under (the vanilla names are "<Hull>_<Role>").</summary>
    public static string TemplateId(string hull, string role, string family)
    {
        return hull + "_" + role + "_" + family;
    }

    /// <summary>Short chip label of a weapon family for tooltips/panels.</summary>
    public static string FamilyLabel(string family)
    {
        if (string.IsNullOrEmpty(family)) return "";
        if (family.IndexOf("Kinetic", StringComparison.Ordinal) >= 0) return CouncilGame_IsRu() ? "кинетика" : "Kinetic";
        if (family.IndexOf("Missile", StringComparison.Ordinal) >= 0) return CouncilGame_IsRu() ? "ракеты" : "Missiles";
        if (family.IndexOf("Laser", StringComparison.Ordinal) >= 0) return CouncilGame_IsRu() ? "лазеры" : "Lasers";
        if (family.IndexOf("Beam", StringComparison.Ordinal) >= 0) return CouncilGame_IsRu() ? "лучи" : "Beams";
        return family;
    }
    // TemplateCore is pure - the language comes from the caller through this tiny indirection, set once at catalog build.
    static Func<bool> russianProvider;
    public static void SetLanguageProvider(Func<bool> provider) { russianProvider = provider; }
    static bool CouncilGame_IsRu() { return russianProvider != null && russianProvider(); }

    /// <summary>1-2 sentences describing what a ship actually is, from ITS OWN range efficiency (the game's
    /// "ОБЩАЯ МЕТКОСТЬ" numbers, not the template's nominal bands) plus its AA/armor traits. This is what the
    /// ship tooltip shows without Alt; deliberately short (docs §6 - the player asked for the whole truth in
    /// one line, not for the catalogue entry reprinted).</summary>
    public static string ShortProfileText(float effLong, float effMedium, float effShort, bool hasFlak, string armorBias, bool russian)
    {
        var sb = new System.Text.StringBuilder();
        float hi = Max(effLong, effMedium, effShort), lo = Min(effLong, effMedium, effShort);
        string range;
        if (hi < 0.35f) range = null;                                    // no real guns / all ranges bad: nothing to say
        else if (hi - lo <= 0.25f) range = russian ? "Универсален на любой дистанции" : "Universal at any range";
        else if (effShort >= effLong + 0.25f) range = russian ? "Опасен вблизи, на дистанции промахивается" : "Deadly up close, misses at range";
        else if (effLong >= effShort + 0.25f) range = russian ? "Бьёт издалека, в упор беспомощен" : "Strikes from afar, helpless at zero range";
        else if (effMedium >= hi - 0.05f && effMedium >= effLong + 0.2f && effMedium >= effShort + 0.2f) range = russian ? "Силён на средней дистанции" : "Strongest at medium range";
        else range = russian ? "Держит средние и ближние дистанции" : "Handles medium and short ranges";
        string trait = null;
        if (hasFlak) trait = russian ? "прикрывает флот от ракет и истребителей" : "screens the fleet from missiles and fighters";
        else if (armorBias == "energy") trait = russian ? "пробивает энергетическую броню" : "punches through energy armor";
        else if (armorBias == "projectile") trait = russian ? "пробивает снарядную броню" : "punches through projectile armor";
        if (trait != null) sb.Append(range == null ? Cap(trait) + "." : range + ", " + trait + ".");
        else if (range != null) sb.Append(range).Append('.');
        return sb.Length > 0 ? sb.ToString() : (russian ? "Без выраженного профиля." : "No pronounced profile.");
    }

    static string Cap(string v) { return string.IsNullOrEmpty(v) ? v : char.ToUpper(v[0]) + v.Substring(1); }
    static float Max(float a, float b, float c) { return a > b ? (a > c ? a : c) : (b > c ? b : c); }
    static float Min(float a, float b, float c) { return a < b ? (a < c ? a : c) : (b < c ? b : c); }

    /// <summary>Family part of a template id ("HullSmall01Terrans_SmallAttacker_KineticBase" -> "KineticBase").</summary>
    public static string FamilyOfId(string templateId)
    {
        if (string.IsNullOrEmpty(templateId)) return null;
        int i = templateId.LastIndexOf('_');
        return i < 0 || i == templateId.Length - 1 ? null : templateId.Substring(i + 1);
    }

    /// <summary>How well one variant plays against a target family: the sum of the target's Against values
    /// (the game's own AIBattleSituations, Mode=Against), used to offer the player the variant that answers
    /// the enemy fleet he is about to fight, and - later - for the AI's design choice.</summary>
    public static float ScoreAgainst(TemplateModuleInfo mine, TemplateModuleInfo hostile)
    {
        if (mine == null || hostile == null) return 0f;
        float v = 0f;
        v += Signed(mine.VsKinetic, hostile.WeaponType, "Kinetic");
        v += Signed(mine.VsMissile, hostile.WeaponType, "Missile");
        v += Signed(mine.VsLaser, hostile.WeaponType, "Laser");
        v += Signed(mine.VsBeam, hostile.WeaponType, "Beam");
        if (mine.HasFlak) v += 0.5f * (Band(hostile.WeaponType, "Missile") + Band(hostile.WeaponType, "Kinetic"));
        v += BandsUsed(mine) >= 2 && BandsUsed(hostile) <= 1 ? 0.25f : 0f;   // a screen weapon beats a one-band fleet
        return v;
    }

    static float Signed(float value, string weaponType, string matches)
    {
        return Band(weaponType, matches) * value;
    }

    static float Band(string weaponType, string matches)
    {
        return string.Equals(weaponType, matches, StringComparison.OrdinalIgnoreCase) ? 1f : 0f;
    }

    /// <summary>How well my weapon fares against an enemy that fields the given armour type ("EnergyDefense" /
    /// "ProjectileDefense"): the game's own Against/&lt;armour&gt; value of the module. Kept out of ScoreAgainst,
    /// because which armour the enemy bought is the player's call, not something to be guessed.</summary>
    public static float ArmorScore(TemplateModuleInfo mine, string enemyArmorSituation)
    {
        if (mine == null || string.IsNullOrEmpty(enemyArmorSituation)) return 0f;
        if (enemyArmorSituation == "EnergyDefense") return mine.VsEnergyArmor;
        if (enemyArmorSituation == "ProjectileDefense") return mine.VsProjectileArmor;
        return 0f;
    }

    /// <summary>Pick the best variant for a hostile fleet out of a catalogue (0-based index, -1 if empty).</summary>
    public static int BestAgainst(List<BattleTemplate> catalogue, TemplateModuleInfo hostile, string hull, string role)
    {
        if (catalogue == null) return -1;
        int best = -1;
        float bestScore = float.MinValue;
        for (int i = 0; i < catalogue.Count; i++)
        {
            var t = catalogue[i];
            if (t == null) continue;
            if (hull != null && t.Hull != hull) continue;
            if (role != null && t.Role != role) continue;
            float s = ScoreAgainst(t.Info, hostile);
            if (s > bestScore) { bestScore = s; best = i; }
        }
        return best;
    }
}
