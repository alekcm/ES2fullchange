// Hero rework (stage 4), pure data and logic without game types: "flavors" of skill effects.
//  * every vanilla modifier is classified into a flavor (offense, production, science, ...) with a normalized magnitude;
//  * every flavor has one (or two) concrete modifiers for each channel: a fleet (admiral), a star system (governor) or the
//    whole empire through the senate (senator and dictator);
//  * so a skill written for one channel can be turned into the same skill for another channel:
//    "+10 production and +30% weapon damage" -> governor "+10 production and +10 food", admiral "+30% damage and +10% health".
// Also here: names of the generated skills, the themes of the political parties and of the faction dictators.
using System;
using System.Collections.Generic;

public enum Flavor
{
    Offense = 0, Defense, Mobility, Sustain, Upkeep, Production, Growth, Science, Money, Influence, Happiness, Manpower, Deposit, Experience, Diplomacy,
    Votes     // votes in the Galactic Council: a senator skill without a senate modifier (the Council reads it), a governor/admiral gets influence instead
}

public sealed class FlavorTarget
{
    public string Prop;
    public int Op;        // ModifierOperation: 1 Addition, 2 Subtraction, 3 Multiplication, 6 Percent
    public string Path;   // "" = no path
    public float Ref;     // value of the effect at magnitude 1 (negative when the good direction is a decrease)

    public FlavorTarget(string prop, int op, string path, float reference) { Prop = prop; Op = op; Path = path; Ref = reference; }
}

/// <summary>One planned level of an authored skill: the flavors it gives and how strong they are.</summary>
public sealed class PlannedSkill
{
    public string Id;                // stable id inside a tree: S0..S6
    public Flavor[] Flavors;
    public float[][] Magnitudes;     // [level][flavor index]
    public int StageIndex;
}

public sealed class PlannedStage
{
    public int RequiredLevel;
    public List<PlannedSkill> Skills = new List<PlannedSkill>();
}

public static class HeroFlavors
{
    public const string ShipPath = "./ClassGarrisonFleet";
    public const string ShipCrewPath = "./ClassGarrisonFleet/ClassShip";
    public const string SystemPath = "ClassColonizedStarSystem";
    public const string PlanetPath = "ClassColonizedStarSystem/ClassColonizedPlanet";
    public const string DepositPath = "ClassColonizedStarSystem/ClassPlanet/ClassResourceDeposit";
    public const string SenateFleet = "../ClassEmpire/ClassGarrisonFleet";
    public const string SenateSystem = "../ClassEmpire/ClassColonizedStarSystem";
    public const string SenatePlanet = "../ClassEmpire/ClassColonizedStarSystem/ClassColonizedPlanet";
    public const string SenateDeposit = "../ClassEmpire/ClassColonizedStarSystem/ClassPlanet/ClassResourceDeposit";
    public const string SenateEmpire = "../ClassEmpire";

    const int Add = 1, Sub = 2, Mul = 3, Pct = 6;

    public const int FlavorCount = 16;

    // ---------------------------------------------------------------- targets

    static FlavorTarget T(string prop, int op, string path, float reference) { return new FlavorTarget(prop, op, path, reference); }

    static FlavorTarget[] ShipTargets(Flavor f)
    {
        switch (f)
        {
            case Flavor.Offense: return new[] { T("ShipDamage", Add, ShipPath, 0.2f) };
            case Flavor.Defense: return new[] { T("FleetMaximumHealth", Add, ShipPath, 0.2f) };
            case Flavor.Mobility: return new[] { T("ShipMaximumMovement", Add, ShipPath, 2f) };
            case Flavor.Sustain: return new[] { T("HealthRegenerationRatio", Add, ShipPath, 0.2f) };
            case Flavor.Upkeep: return new[] { T("GarrisonMoneyUpkeep", Pct, ShipPath, -0.2f) };
            case Flavor.Production: return new[] { T("ShipHullPlatingAbsorptionPercent", Add, ShipPath, 0.2f) };
            case Flavor.Growth: return new[] { T("FleetManpowerRefillRateModifier", Add, ShipPath, 0.4f) };
            case Flavor.Science: return new[] { T("VisionRange", Add, ShipPath, 2f) };
            case Flavor.Money: return new[] { T("GarrisonMoneyUpkeep", Pct, ShipPath, -0.3f) };
            case Flavor.Influence: case Flavor.Votes: return new[] { T("SiegePower", Pct, ShipPath, 0.2f) };
            case Flavor.Experience: return new[] { T("ExperiencePerTurn", Add, ShipPath, 8f) };
            case Flavor.Happiness: return new[] { T("FleetMaximumShield", Add, ShipPath, 0.25f) };
            case Flavor.Manpower: return new[] { T("MaximumShipManpower", Pct, ShipCrewPath, 0.25f) };
            case Flavor.Deposit: return new[] { T("Evasion", Add, ShipPath, 0.1f) };
            default: return new[] { T("ShipShieldPenetrationFactor", Add, ShipPath, 0.4f) };
        }
    }

    static FlavorTarget[] SystemTargets(Flavor f)
    {
        switch (f)
        {
            case Flavor.Offense: return new[] { T("GroundBattleDefenderTroopsMaxHealthMultiplier", Add, SystemPath, 0.2f) };
            case Flavor.Defense: return new[] { T("MaximumSystemManpower", Pct, "", 0.4f) };
            case Flavor.Mobility: return new[] { T("TradingRoutesBonus_Production", Pct, SystemPath, 0.4f) };
            case Flavor.Sustain: return new[] { T("MaximumSystemManpower", Add, "", 100f) };
            case Flavor.Upkeep: return new[] { T("SystemMoneyUpkeep", Pct, SystemPath, -0.2f) };
            case Flavor.Production: return new[] { T("SystemProduction", Add, SystemPath, 10f) };
            case Flavor.Growth: return new[] { T("SystemGrowth", Add, SystemPath, 20f) };
            case Flavor.Science: return new[] { T("SystemResearch", Add, SystemPath, 40f) };
            case Flavor.Money: return new[] { T("SystemMoney", Add, SystemPath, 10f) };
            case Flavor.Influence: case Flavor.Votes: return new[] { T("SystemEmpirePoint", Add, SystemPath, 20f) };
            case Flavor.Experience: return new[] { T("SystemResearch", Pct, SystemPath, 0.2f) };
            case Flavor.Happiness: return new[] { T("Happiness", Add, SystemPath, 10f) };
            case Flavor.Manpower: return new[] { T("NetSystemManpower", Add, SystemPath, 10f) };
            case Flavor.Deposit: return new[] { T("DepositValue", Add, DepositPath, 1f) };
            default: return new[] { T("TradingRoutesBonus_Growth", Pct, SystemPath, 0.4f) };
        }
    }

    // empire-wide: values are the "significant senator skill" size, about half of what a governor gets in one system
    static FlavorTarget[] SenateTargets(Flavor f)
    {
        switch (f)
        {
            case Flavor.Offense: return new[] { T("ShipDamage", Add, SenateFleet, 0.1f) };
            case Flavor.Defense: return new[] { T("FleetMaximumHealth", Add, SenateFleet, 0.1f) };
            case Flavor.Mobility: return new[] { T("ShipMaximumMovement", Add, SenateFleet, 1f) };
            case Flavor.Sustain: return new[] { T("HealthRegenerationRatio", Add, SenateFleet, 0.05f) };
            case Flavor.Upkeep: return new[] { T("SystemMoneyUpkeep", Pct, SenateSystem, -0.1f) };
            case Flavor.Production: return new[] { T("PlanetRawIndustry", Add, SenatePlanet, 1f) };
            case Flavor.Growth: return new[] { T("PlanetRawFood", Add, SenatePlanet, 1f) };
            case Flavor.Science: return new[] { T("PlanetRawScience", Add, SenatePlanet, 1f) };
            case Flavor.Money: return new[] { T("PlanetRawDust", Add, SenatePlanet, 1f) };
            case Flavor.Influence: return new[] { T("PlanetRawPrestige", Add, SenatePlanet, 1f) };
            case Flavor.Experience: return new[] { T("SystemResearch", Pct, SenateSystem, 0.1f) };
            case Flavor.Happiness: return new[] { T("Happiness", Add, SenateSystem, 5f) };
            case Flavor.Manpower: return new[] { T("MaximumEmpireManpowerStock", Pct, SenateEmpire, 0.25f) };
            case Flavor.Deposit: return new[] { T("DepositValue", Add, SenateDeposit, 0.5f) };
            case Flavor.Votes: return new FlavorTarget[0];   // no senate modifier: the votes are read by the Council (see VotesOfSkill)
            default: return new[] { T("PeacefulTreatyCostReduction", Add, SenateEmpire, -0.5f), T("AggressiveTreatyCostReduction", Add, SenateEmpire, -0.5f) };
        }
    }

    public static FlavorTarget[] TargetsOf(Flavor flavor, HeroChannel channel)
    {
        switch (channel)
        {
            case HeroChannel.Ship: return ShipTargets(flavor);
            case HeroChannel.System: return SystemTargets(flavor);
            case HeroChannel.Senate: return SenateTargets(flavor);
        }
        return new FlavorTarget[0];
    }

    public static HeroChannel ChannelOf(HeroKind kind)
    {
        return kind == HeroKind.Admiral ? HeroChannel.Ship : kind == HeroKind.Governor ? HeroChannel.System : HeroChannel.Senate;
    }

    public static float Round(float v)
    {
        float a = Math.Abs(v);
        if (a >= 5f) return (float)Math.Round(v);
        if (a >= 1f) return (float)Math.Round(v * 10f) / 10f;
        return (float)Math.Round(v * 1000f) / 1000f;
    }

    /// <summary>Value of a target modifier for a magnitude (1 = the usual size of a second-level skill) and a power factor.</summary>
    public static float ValueOf(FlavorTarget target, float magnitude, float power)
    {
        return Round(target.Ref * magnitude * power);
    }

    // ---------------------------------------------------------------- classification of vanilla modifiers

    static readonly float[] defaultAdd = { 0.2f, 0.2f, 2f, 0.2f, 0.2f, 10f, 20f, 40f, 10f, 20f, 10f, 100f, 1f, 8f, 0.5f, 20f };
    static readonly float[] defaultPercent = { 0.2f, 0.2f, 0.2f, 0.4f, 0.2f, 0.15f, 0.15f, 0.2f, 0.15f, 0.1f, 0.1f, 0.4f, 0.33f, 0.2f, 0.5f, 0.1f };

    static bool Has(string s, string part) { return s.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0; }

    /// <summary>Flavor of a simulation property by its name; false when the property means nothing a skill could convert.</summary>
    public static bool FlavorOfProperty(string prop, out Flavor flavor)
    {
        flavor = Flavor.Offense;
        if (string.IsNullOrEmpty(prop)) return false;
        if (Has(prop, "TreatyCost") || Has(prop, "ElectionAction")) { flavor = Flavor.Diplomacy; return true; }
        if (Has(prop, "Upkeep")) { flavor = Flavor.Upkeep; return true; }
        if (Has(prop, "CostReduction")) { flavor = Flavor.Production; return true; }
        if (Has(prop, "Damage") || Has(prop, "Penetration") || Has(prop, "SiegePower") || Has(prop, "Attack")) { flavor = Flavor.Offense; return true; }
        if (Has(prop, "Regeneration") || Has(prop, "Refill")) { flavor = Flavor.Sustain; return true; }
        if (Has(prop, "Health") || Has(prop, "Shield") || Has(prop, "Absorption") || Has(prop, "Evasion") || Has(prop, "Armor") || Has(prop, "Defense")) { flavor = Flavor.Defense; return true; }
        if (Has(prop, "Movement") || Has(prop, "Vision") || Has(prop, "Probe") || Has(prop, "Speed") || Has(prop, "Range")) { flavor = Flavor.Mobility; return true; }
        if (Has(prop, "Manpower") || Has(prop, "Troops")) { flavor = Flavor.Manpower; return true; }
        if (Has(prop, "Experience")) { flavor = Flavor.Experience; return true; }
        if (Has(prop, "Growth") || Has(prop, "Food")) { flavor = Flavor.Growth; return true; }
        if (Has(prop, "Industry") || Has(prop, "Production") || Has(prop, "FIDSI")) { flavor = Flavor.Production; return true; }
        if (Has(prop, "Research") || Has(prop, "Science")) { flavor = Flavor.Science; return true; }
        if (Has(prop, "EmpirePoint") || Has(prop, "Prestige") || Has(prop, "Influence")) { flavor = Flavor.Influence; return true; }
        if (Has(prop, "Money") || Has(prop, "Dust") || Has(prop, "Trad")) { flavor = Flavor.Money; return true; }
        if (Has(prop, "Happiness") || Has(prop, "Approval")) { flavor = Flavor.Happiness; return true; }
        if (Has(prop, "Deposit")) { flavor = Flavor.Deposit; return true; }
        return false;
    }

    /// <summary>
    /// Flavor and magnitude (1 = a typical second-level vanilla skill) of one modifier. Only plain numbers can be converted:
    /// additions, subtractions, factors and percents. Penalties and unknown properties are dropped.
    /// </summary>
    public static bool Classify(string prop, int op, float value, HeroChannel source, out Flavor flavor, out float magnitude)
    {
        magnitude = 0f;
        if (!FlavorOfProperty(prop, out flavor)) return false;
        float signed;
        switch (op)
        {
            case Add: signed = value; break;
            case Sub: signed = -value; break;
            case Mul: signed = value - 1f; break;
            case Pct: signed = value; break;
            default: return false;
        }
        float reference = op == Pct || op == Mul ? defaultPercent[(int)flavor] : defaultAdd[(int)flavor];
        if (op != Pct && op != Mul && Has(prop, "PlanetRaw")) reference = flavor == Flavor.Production || flavor == Flavor.Growth || flavor == Flavor.Science || flavor == Flavor.Money || flavor == Flavor.Influence ? 1.5f : reference;
        if (op != Pct && op != Mul && Has(prop, "NetSystemManpower")) reference = 10f;   // per turn recruits, not the stock
        if (source == HeroChannel.Senate) reference *= 0.5f;   // senate effects are about half of the system ones
        bool absolute = flavor == Flavor.Upkeep || flavor == Flavor.Diplomacy || Has(prop, "Cost") || Has(prop, "Reduction") || Has(prop, "Penalt");
        float n = (absolute ? Math.Abs(signed) : signed) / reference;
        if (n < 0.01f) return false;
        magnitude = n;
        return true;
    }

    /// <summary>Flavor behind a hero mastery: the fallback when a skill had nothing convertible.</summary>
    public static Flavor FlavorOfMastery(string mastery, Flavor fallback)
    {
        if (mastery == null) return fallback;
        if (Has(mastery, "Labour")) return Flavor.Production;
        if (Has(mastery, "Wit")) return Flavor.Science;
        if (Has(mastery, "Command")) return Flavor.Offense;
        if (Has(mastery, "Curiosity")) return Flavor.Mobility;
        return fallback;
    }

    public static string MasteryOfFlavor(Flavor f)
    {
        switch (f)
        {
            case Flavor.Production: case Flavor.Growth: case Flavor.Manpower: case Flavor.Deposit: return "HeroMasteryLabour";
            case Flavor.Science: case Flavor.Money: case Flavor.Influence: case Flavor.Happiness: case Flavor.Diplomacy: case Flavor.Votes: return "HeroMasteryWit";
            case Flavor.Offense: case Flavor.Defense: return "HeroMasteryCommand";
            default: return "HeroMasteryCuriosity";
        }
    }

    // ---------------------------------------------------------------- names (ru|en)

    static readonly string[] shipNouns =
    {
        "Огневая мощь|Firepower", "Крепкие корпуса|Hardened Hulls", "Манёвренность|Maneuverability", "Полевой ремонт|Field Repairs",
        "Экономия флота|Fleet Economy", "Бронепластины|Plating", "Резервные экипажи|Reserve Crews", "Дальняя разведка|Long-Range Scouting",
        "Бережливое снабжение|Frugal Supply", "Осадное мастерство|Siegecraft", "Усиленные щиты|Reinforced Shields", "Набор экипажей|Crew Recruitment",
        "Уклонение|Evasive Tactics", "Боевой опыт|Combat Experience", "Пробивание щитов|Shield Breaking", "Влияние в Совете|Council Clout"
    };
    static readonly string[] systemNouns =
    {
        "Гарнизонная выучка|Garrison Training", "Укреплённые гарнизоны|Fortified Garrisons", "Торговые пути|Trade Routes", "Пополнение войск|Troop Replenishment",
        "Бережливое управление|Frugal Administration", "Индустриальный рост|Industrial Drive", "Агрокультура|Agriculture", "Научные центры|Research Centers",
        "Торговля и налоги|Commerce and Taxes", "Престиж колоний|Colonial Prestige", "Общественный порядок|Public Order", "Мобилизация|Mobilization",
        "Добыча ресурсов|Resource Extraction", "Академические программы|Academic Programs", "Торговые связи|Trade Ties", "Голос системы|System Voice"
    };
    static readonly string[] senateNouns =
    {
        "Военные ассигнования|Military Appropriations", "Бронирование флотов|Fleet Armor Programs", "Имперская навигация|Imperial Navigation", "Службы ремонта|Repair Corps",
        "Режим экономии|Austerity", "Государственная индустрия|State Industry", "Аграрная реформа|Agrarian Reform", "Научный комитет|Science Committee",
        "Казначейство|Treasury", "Имперская идеология|Imperial Ideology", "Всеобщее благополучие|General Welfare", "Всеобщая мобилизация|National Mobilization",
        "Ресурсная политика|Resource Policy", "Образовательная реформа|Education Reform", "Дипломатический корпус|Diplomatic Corps", "Голоса в Совете|Council Votes"
    };

    // the three arrays above are written in the order of the Flavor enum

    static string Pick(string pair, bool russian)
    {
        int cut = pair.IndexOf('|');
        return russian ? pair.Substring(0, cut) : pair.Substring(cut + 1);
    }

    public static string Noun(Flavor flavor, HeroChannel channel, bool russian)
    {
        string[] table = channel == HeroChannel.Ship ? shipNouns : channel == HeroChannel.System ? systemNouns : senateNouns;
        return Pick(table[(int)flavor], russian);
    }

    static readonly string[] roman = { "I", "II", "III", "IV", "V" };
    public static string Roman(int i) { return roman[Math.Max(0, Math.Min(roman.Length - 1, i))]; }

    // ---------------------------------------------------------------- parties

    public static int PartyIndex(string politics)
    {
        if (string.IsNullOrEmpty(politics)) return 0;
        int end = politics.Length;
        int start = end;
        while (start > 0 && char.IsDigit(politics[start - 1])) start--;
        int n;
        if (start == end || end - start > 2 || !int.TryParse(politics.Substring(start), out n)) return 0;
        return n >= 1 && n <= 6 ? n : 0;
    }

    static readonly string[] partyNames =
    {
        "Индустриалисты|Industrialists", "Учёные|Scientists", "Пацифисты|Pacifists", "Экологи|Ecologists", "Религиозные|Religious", "Милитаристы|Militarists"
    };

    public static string PartyName(int party, bool russian) { return Pick(partyNames[party - 1], russian); }

    static readonly Flavor[][] partyThemes =
    {
        new[] { Flavor.Production, Flavor.Money, Flavor.Manpower },
        new[] { Flavor.Science, Flavor.Experience, Flavor.Influence },
        new[] { Flavor.Happiness, Flavor.Votes, Flavor.Growth },       // pacifists: votes in the Council
        new[] { Flavor.Growth, Flavor.Deposit, Flavor.Sustain },
        new[] { Flavor.Happiness, Flavor.Influence, Flavor.Votes },    // religious: votes in the Council
        new[] { Flavor.Offense, Flavor.Defense, Flavor.Manpower }
    };

    public static Flavor[] PartyTheme(int party) { return partyThemes[party - 1]; }

    // ---------------------------------------------------------------- factions (dictators)

    public static readonly string[] FactionKeys =
    {
        "Sophons", "Cravers", "Terrans", "Venetians", "Vampirilis", "TimeLords", "Unfallen", "Endless", "Horatio", "Insectoids", "Pirates", "Primitives", "Advanced", "Other"
    };

    static readonly string[] factionNames =
    {
        "Софоны|Sophons", "Краверы|Cravers", "Земляне|Terrans", "Венецианцы|Venetians", "Вампирилис|Vampirilis", "Повелители времени|Time Lords", "Падшие|Unfallen",
        "Эндлесс|Endless", "Гораций|Horatio", "Инсектоиды|Insectoids", "Пираты|Pirates", "Примитивы|Primitives", "Продвинутые|Advanced", "Фракция|Faction"
    };

    static readonly Flavor[][] factionThemes =
    {
        new[] { Flavor.Science, Flavor.Growth, Flavor.Influence },          // Sophons: science, food
        new[] { Flavor.Offense, Flavor.Manpower, Flavor.Production },       // Cravers: war
        new[] { Flavor.Production, Flavor.Money, Flavor.Influence },        // Terrans
        new[] { Flavor.Money, Flavor.Influence, Flavor.Votes },            // Venetians: trade and the votes of the Council
        new[] { Flavor.Influence, Flavor.Offense, Flavor.Sustain },         // Vampirilis
        new[] { Flavor.Science, Flavor.Mobility, Flavor.Experience },       // Time Lords
        new[] { Flavor.Defense, Flavor.Growth, Flavor.Happiness },          // Unfallen
        new[] { Flavor.Science, Flavor.Influence, Flavor.Production },      // Endless
        new[] { Flavor.Money, Flavor.Mobility, Flavor.Diplomacy },          // Horatio
        new[] { Flavor.Growth, Flavor.Manpower, Flavor.Sustain },           // Insectoids
        new[] { Flavor.Offense, Flavor.Money, Flavor.Mobility },            // Pirates
        new[] { Flavor.Deposit, Flavor.Growth, Flavor.Defense },            // Primitives
        new[] { Flavor.Science, Flavor.Production, Flavor.Experience },     // Advanced
        new[] { Flavor.Production, Flavor.Science, Flavor.Influence }
    };

    /// <summary>Faction key of a hero affinity name (for example HeroAffinitySophons); "Other" for anything unknown.</summary>
    public static string FactionKey(string affinity)
    {
        if (!string.IsNullOrEmpty(affinity))
            for (int i = 0; i < FactionKeys.Length - 1; i++)
                if (Has(affinity, FactionKeys[i])) return FactionKeys[i];
        return "Other";
    }

    static int FactionIndex(string key)
    {
        for (int i = 0; i < FactionKeys.Length; i++) if (FactionKeys[i] == key) return i;
        return FactionKeys.Length - 1;
    }

    public static Flavor[] FactionTheme(string key) { return factionThemes[FactionIndex(key)]; }
    public static string FactionName(string key, bool russian) { return Pick(factionNames[FactionIndex(key)], russian); }

    // ---------------------------------------------------------------- authored trees (party and dictator)

    public const int StageCount = 4;
    static readonly float[] tierOfStage = { 1.0f, 1.5f, 2.0f, 2.5f };

    /// <summary>Skill points a hero must have spent before a stage opens (vanilla: 0 / 4 / 8 / 12).</summary>
    public static int StageThreshold(int stage, HeroKind kind, bool dictator)
    {
        int vanilla = stage * 4;
        if (dictator) return stage * 6;        // 0 / 6 / 12 / 18: the dictator has thirty levels to spend
        if (kind == HeroKind.Senator) return stage * 2;   // 0 / 2 / 4 / 6: a senator has seven level-ups
        return vanilla;
    }

    /// <summary>
    /// The seven skills of an authored tree (two at each of the first three stages, one ultimate):
    /// stage 0: A, B; stage 1: C, A+B; stage 2: B+C, A+C; stage 3: the ultimate A+B+C (+ authority for the dictator).
    /// Admirals and governors get two levels per skill, a senator one (the strong one), the dictator three.
    /// </summary>
    public static List<PlannedStage> PlanTree(Flavor[] theme, HeroKind kind, bool dictator, bool authority)
    {
        Flavor a = theme[0], b = theme[1], c = theme[2];
        var combos = new Flavor[][]
        {
            new[] { a }, new[] { b }, new[] { c }, new[] { a, b }, new[] { b, c }, new[] { a, c },
            authority ? new[] { a, b, c, Flavor.Happiness, Flavor.Manpower } : new[] { a, b, c }
        };
        int[] stageOf = { 0, 0, 1, 1, 2, 2, 3 };
        var stages = new List<PlannedStage>();
        for (int s = 0; s < StageCount; s++) stages.Add(new PlannedStage { RequiredLevel = StageThreshold(s, kind, dictator) });
        for (int i = 0; i < combos.Length; i++)
        {
            int s = stageOf[i];
            float tier = tierOfStage[s];
            float per = combos[i].Length == 1 ? 1f : (combos[i].Length == 2 ? 0.7f : (combos[i].Length == 3 ? 0.55f : 0.45f));
            bool ultimate = s == StageCount - 1;
            float[] levelFactor;
            if (ultimate) levelFactor = new[] { dictator ? 1.5f : 1f };
            else if (dictator) levelFactor = new[] { 0.5f, 1f, 1.5f };
            else if (kind == HeroKind.Senator) levelFactor = new[] { 1f };
            else levelFactor = new[] { 0.5f, 1f };
            var skill = new PlannedSkill { Id = "S" + i, Flavors = combos[i], StageIndex = s, Magnitudes = new float[levelFactor.Length][] };
            for (int l = 0; l < levelFactor.Length; l++)
            {
                skill.Magnitudes[l] = new float[combos[i].Length];
                for (int f = 0; f < combos[i].Length; f++) skill.Magnitudes[l][f] = tier * per * levelFactor[l];
            }
            stages[s].Skills.Add(skill);
        }
        return stages;
    }

    /// <summary>Title of an authored skill: the nouns of its flavors in the channel of the kind.</summary>
    public static string SkillTitle(Flavor[] flavors, HeroChannel channel, bool russian)
    {
        if (flavors.Length == 1) return Noun(flavors[0], channel, russian);
        if (flavors.Length == 2) return Noun(flavors[0], channel, russian) + " / " + Noun(flavors[1], channel, russian);
        return russian ? "Вершина: " + Noun(flavors[0], channel, russian) : "Pinnacle: " + Noun(flavors[0], channel, russian);
    }

    public static string KindCode(HeroKind kind, bool dictator)
    {
        if (dictator) return "D";
        return kind == HeroKind.Admiral ? "A" : kind == HeroKind.Governor ? "G" : "S";
    }

    public static bool TryParseKindCode(string code, out HeroKind kind, out bool dictator)
    {
        kind = HeroKind.Senator; dictator = false;
        switch (code)
        {
            case "A": kind = HeroKind.Admiral; return true;
            case "G": kind = HeroKind.Governor; return true;
            case "S": return true;
            case "D": dictator = true; return true;
        }
        return false;
    }

    // ---------------------------------------------------------------- votes of the Galactic Council

    public const float VotesFlatPerMagnitude = 6f;        // votes of a skill of magnitude 1 ...
    public const float VotesPerFourSystems = 1f;          // ... plus this many for every four colonized systems

    static readonly Dictionary<string, float[]> voteCache = new Dictionary<string, float[]>();

    /// <summary>
    /// Magnitude of the Council votes of a senator or dictator skill by level (null: the skill gives no votes).
    /// Everything is derived from the name of the generated skill (HRParty01S0@S, HRDictSophonsS6), so it works for saved games.
    /// </summary>
    public static float[] VoteMagnitudes(string skillName)
    {
        float[] r;
        if (skillName == null) return null;
        if (voteCache.TryGetValue(skillName, out r)) return r;
        r = ParseVotes(skillName);
        voteCache[skillName] = r;
        return r;
    }

    static float[] ParseVotes(string name)
    {
        Flavor[] theme; HeroKind kind = HeroKind.Senator; bool dictator = false, authority = false;
        string sid;
        if (name.StartsWith("HRParty", StringComparison.Ordinal))
        {
            int party, at = name.IndexOf('@');
            if (name.Length < 12 || at < 10 || !int.TryParse(name.Substring(7, 2), out party) || party < 1 || party > 6) return null;
            sid = name.Substring(9, at - 9);
            if (!TryParseKindCode(name.Substring(at + 1), out kind, out dictator)) return null;
            theme = PartyTheme(party);
        }
        else if (name.StartsWith("HRDict", StringComparison.Ordinal))
        {
            string rest = name.Substring(6);
            int s = rest.LastIndexOf('S');
            if (s < 1) return null;
            sid = rest.Substring(s);
            theme = FactionTheme(rest.Substring(0, s));
            dictator = true; authority = true;
        }
        else return null;
        if (kind != HeroKind.Senator) return null;           // only senators and dictators sit in the senate
        var plan = PlanTree(theme, kind, dictator, authority);
        float power = PowerOf(dictator);
        for (int st = 0; st < plan.Count; st++)
            for (int k = 0; k < plan[st].Skills.Count; k++)
            {
                PlannedSkill ps = plan[st].Skills[k];
                if (ps.Id != sid) continue;
                var res = new float[ps.Magnitudes.Length];
                float total = 0f;
                for (int l = 0; l < res.Length; l++)
                {
                    for (int f = 0; f < ps.Flavors.Length; f++) if (ps.Flavors[f] == Flavor.Votes) res[l] += ps.Magnitudes[l][f] * power;
                    total += res[l];
                }
                return total > 0f ? res : null;
            }
        return null;
    }

    /// <summary>Votes of one learned skill level: a flat part plus a part per four colonized systems of the empire.</summary>
    public static int VotesOfSkill(string skillName, int level, int systems)
    {
        float[] m = VoteMagnitudes(skillName);
        if (m == null || m.Length == 0) return 0;
        int idx = Math.Max(0, Math.Min(m.Length - 1, level - 1));
        return (int)Math.Round(m[idx] * (VotesFlatPerMagnitude + VotesPerFourSystems * (systems / 4)));
    }

    /// <summary>Strength of the senate effects of the dictator compared with a senator: "stronger faction-specialised laws".</summary>
    public const float DictatorPower = 1.6f;

    /// <summary>Power factor of a skill of a kind: the dictator is stronger than a senator.</summary>
    public static float PowerOf(bool dictator) { return dictator ? DictatorPower : 1f; }
}
