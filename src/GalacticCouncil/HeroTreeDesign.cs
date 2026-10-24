// Hero rework (redesign of all skill trees): explicit, hand-written trees. Pure data and logic without game types.
//  * a CONCEPT is one real effect (hull armor, evasion, vision range, science of a system, ...) with its modifiers, its explicit
//    round steps per tier and the number of levels it has for every role (admiral A, governor G, senator S, dictator D);
//  * a TREE is written as one line per role: "T1:Armor,Repair;T2:Kinetic;T3:Evasion;U:Armor,Repair,Kinetic,LootDust",
//    one skill per concept, several selectable levels per skill, one ultimate that combines the effects;
//  * values are whole numbers (percent never ends in decimals); level N of a skill gives N * step of its tier.
// The listing page (docs/HERO_TREES.html) is generated from the same data.
using System;
using System.Collections.Generic;
using System.Text;

public sealed class DesignMod
{
    public string Prop;
    public int Op;          // ModifierOperation: 1 Addition, 2 Subtraction, 3 Multiplication, 6 Percent
    public string Path;
    public float Scale;     // modifier value of one displayed unit
    public DesignMod(string prop, int op, string path, float scale) { Prop = prop; Op = op; Path = path; Scale = scale; }
}

public sealed class Concept
{
    public string Key;          // A.Armor, G.Prod, S.Votes
    public HeroChannel Channel;
    public string Ru, En;       // name of the effect
    public string UnitRu, UnitEn;
    public bool Reduces;        // shown with a minus: the good direction is a decrease
    public Flavor Icon;         // icon and mastery of the skill
    public string Group;        // one idea: two concepts of the same group never share a tree
    public int[] Steps;         // per tier 1..3: value of one level (displayed units)
    public int Levels;          // A, G
    public int LevelsS, LevelsD;// senator, dictator
    public int Ult, UltD;       // value of the ultimate (UltD: dictator)
    public int DictMul = 1;     // dictator step = step * DictMul
    public DesignMod[] Mods;
    public string Special;      // id of a hook handled in code (null = plain modifiers)
    public string NoteRu, NoteEn;

    public int LevelCount(char role) { return role == 'S' ? LevelsS : role == 'D' ? LevelsD : Levels; }

    public int Step(char role, int tier)
    {
        int s = Steps[Math.Max(1, Math.Min(Steps.Length, tier)) - 1];
        return role == 'D' ? s * DictMul : s;
    }

    public int Value(char role, int tier, int level) { return Step(role, tier) * level; }
    public int UltValue(char role) { return role == 'D' ? UltD : Ult; }

    public string Show(int value, bool ru)
    {
        string unit = ru ? UnitRu : UnitEn;
        return (Reduces ? "\u2212" : "+") + value + unit;
    }

    public string Name(bool ru) { return ru ? Ru : En; }
    public string Note(bool ru) { return ru ? NoteRu : NoteEn; }
}

public sealed class DesignSkill
{
    public string Name;             // skill name in the database
    public int Tier;                // 1..3, 4 = ultimate
    public Concept[] Concepts;
    public int[][] Values;          // [level][concept]
    public string TitleRu, TitleEn;
    public int Levels { get { return Values.Length; } }
}

public sealed class DesignStage
{
    public int RequiredLevel;
    public List<DesignSkill> Skills = new List<DesignSkill>();
}

public sealed class DesignTree
{
    public string Key;
    public char Role;
    public string TreeName;
    public string TitleRu, TitleEn;
    public List<DesignStage> Stages = new List<DesignStage>();
}

public static partial class HeroDesign
{
    public const string ShipPath = HeroFlavors.ShipPath;
    public const string ShipCrewPath = HeroFlavors.ShipCrewPath;
    public const string WeaponPath = "./ClassGarrisonFleet/ClassShip//ClassModuleWeapon";
    const int Add = 1, Pct = 6, Mul = 3;

    static readonly Dictionary<string, Concept> library = new Dictionary<string, Concept>();

    static DesignMod M(string prop, int op, string path, float scale) { return new DesignMod(prop, op, path, scale); }

    static Concept Def(HeroChannel ch, string key, string ru, string en, string unitRu, string unitEn, bool reduces, Flavor icon, string group,
                       int[] steps, int levels, int levelsS, int levelsD, int dictMul, int ult, int ultD, string noteRu, string noteEn, params DesignMod[] mods)
    {
        var c = new Concept();
        c.Channel = ch;
        c.Key = (ch == HeroChannel.Ship ? "A." : ch == HeroChannel.System ? "G." : "S.") + key;
        c.Ru = ru; c.En = en; c.UnitRu = unitRu; c.UnitEn = unitEn; c.Reduces = reduces; c.Icon = icon; c.Group = group;
        c.Steps = steps; c.Levels = levels; c.LevelsS = levelsS; c.LevelsD = levelsD; c.DictMul = dictMul; c.Ult = ult; c.UltD = ultD;
        c.NoteRu = noteRu; c.NoteEn = noteEn; c.Mods = mods;
        library[c.Key] = c;
        return c;
    }

    static readonly int[] S1 = { 1, 1, 1 };

    static HeroDesign()
    {
        const HeroChannel A = HeroChannel.Ship, G = HeroChannel.System, S = HeroChannel.Senate;
        // ---------------------------------------------------------------- admiral (fleet)
        Def(A, "Armor", "Броня корпуса", "Hull Armor", "%", "%", false, Flavor.Production, "armor", new[] { 2, 3, 4 }, 5, 0, 0, 1, 30, 0,
            "Обшивка корпусов поглощает больше урона.", "Hull plating absorbs more damage.", M("ShipHullPlatingAbsorptionPercent", Add, ShipPath, 0.01f));
        Def(A, "Repair", "Ремонт корпуса вне боя", "Out-of-Battle Hull Repair", " ед. корпуса в ход", " hull per turn", false, Flavor.Sustain, "repair", new[] { 10, 20, 30 }, 5, 0, 0, 1, 200, 0,
            "Каждый корабль чинит фиксированное число очков корпуса каждый ход.", "Every ship repairs a fixed amount of hull each turn.", M("FlatHealthRegeneration", Add, ShipCrewPath, 1f));
        Def(A, "Regen", "Самовосстановление кораблей", "Ship Self-Repair", "% в ход", "% per turn", false, Flavor.Sustain, "regen", new[] { 1, 2, 3 }, 5, 0, 0, 1, 15, 0,
            "Каждый ход корабли восстанавливают долю своего корпуса.", "Every turn ships restore a share of their hull.", M("HealthRegenerationRatio", Add, ShipPath, 0.01f));
        Def(A, "Kinetic", "Урон кинетического оружия", "Kinetic Weapon Damage", "%", "%", false, Flavor.Offense, "dmgk", new[] { 2, 3, 4 }, 5, 0, 0, 1, 30, 0,
            "Сильнее бьют пушки и ракеты.", "Cannons and missiles hit harder.", M("ShipDamageKinetic", Add, ShipPath, 0.01f));
        Def(A, "Energy", "Урон энергетического оружия", "Energy Weapon Damage", "%", "%", false, Flavor.Offense, "dmge", new[] { 2, 3, 4 }, 5, 0, 0, 1, 30, 0,
            "Сильнее бьют лазеры и лучи.", "Lasers and beams hit harder.", M("ShipDamageEnergy", Add, ShipPath, 0.01f));
        Def(A, "Damage", "Урон всего оружия", "All Weapon Damage", "%", "%", false, Flavor.Offense, "dmg", new[] { 1, 2, 3 }, 5, 0, 0, 1, 20, 0,
            "Урон всех видов оружия флота.", "Damage of every weapon of the fleet.", M("ShipDamage", Add, ShipPath, 0.01f));
        Def(A, "Crit", "Шанс критического попадания", "Critical Hit Chance", "%", "%", false, Flavor.Offense, "crit", new[] { 1, 2, 3 }, 5, 0, 0, 1, 15, 0,
            "Выстрелы чаще наносят критический урон.", "Shots land critical hits more often.", M("ShipCriticalHitChance", Add, ShipPath, 0.01f));
        Def(A, "Accuracy", "Меткость", "Accuracy", "%", "%", false, Flavor.Offense, "acc", new[] { 2, 3, 4 }, 5, 0, 0, 1, 25, 0,
            "Выстрелы реже уходят в молоко.", "Fewer shots miss.", M("ShipAccuracy", Add, ShipPath, 0.01f));
        Def(A, "Rate", "Скорострельность", "Rate of Fire", "% времени перезарядки", "% cooldown", true, Flavor.Offense, "rate", new[] { 1, 2, 3 }, 5, 0, 0, 1, 15, 0,
            "Оружие перезаряжается быстрее.", "Weapons reload faster.", M("Cooldown", Pct, WeaponPath, -0.01f));
        Def(A, "Siege", "Бонус к осаде", "Siege Bonus", "%", "%", false, Flavor.Influence, "siege", new[] { 4, 6, 8 }, 5, 0, 0, 1, 50, 0,
            "Флот быстрее сбивает оборону осаждаемой системы.", "The fleet wears down the defense of a besieged system faster.", M("SiegePower", Pct, ShipPath, 0.01f));
        Def(A, "Evasion", "Бонус к уклонению", "Evasion Bonus", "%", "%", false, Flavor.Deposit, "evasion", new[] { 1, 2, 3 }, 5, 0, 0, 1, 15, 0,
            "Корабли чаще уходят от вражеских выстрелов.", "Ships dodge enemy shots more often.", M("Evasion", Add, ShipPath, 0.01f));
        Def(A, "Vision", "Дальность обзора", "Vision Range", "", "", false, Flavor.Science, "vision", S1, 4, 0, 0, 1, 6, 0,
            "Флот видит дальше.", "The fleet sees farther.", M("VisionRange", Add, ShipPath, 1f));
        Def(A, "Move", "Дальность хода", "Movement Range", "", "", false, Flavor.Mobility, "move", S1, 3, 0, 0, 1, 4, 0,
            "Флот проходит больше за ход.", "The fleet travels more per turn.", M("ShipMaximumMovement", Add, ShipPath, 1f));
        Def(A, "Xp", "Опыт флота за ход", "Fleet Experience per Turn", "", "", false, Flavor.Experience, "xp", new[] { 2, 3, 4 }, 5, 0, 0, 1, 25, 0,
            "Корабли набирают опыт каждый ход, даже без боёв.", "Ships gain experience every turn, even without battles.", M("ExperiencePerTurn", Add, ShipPath, 1f));
        Def(A, "Shield", "Прочность щитов", "Shield Health", "%", "%", false, Flavor.Happiness, "shield", new[] { 2, 3, 4 }, 5, 0, 0, 1, 30, 0,
            "Щиты кораблей выдерживают больше урона.", "Ship shields take more damage.", M("FleetMaximumShield", Add, ShipPath, 0.01f));
        Def(A, "ShieldReload", "Восстановление щита каждую фазу", "Shield Reload Each Phase", " ед. щита в фазу", " shield per phase", false, Flavor.Happiness, "reload", new[] { 10, 15, 20 }, 5, 0, 0, 1, 150, 0,
            "Щит быстрее восстанавливается во время боя.", "The shield recovers faster during a battle.", M("ShieldReloadPerPhase", Add, ShipCrewPath, 1f));
        Def(A, "Upkeep", "Содержание флота", "Fleet Upkeep", "% Пыли", "% Dust", true, Flavor.Upkeep, "upkeep", new[] { 3, 4, 5 }, 5, 0, 0, 1, 40, 0,
            "Флот стоит дешевле в Пыли.", "The fleet costs less Dust.", M("GarrisonMoneyUpkeep", Pct, ShipPath, -0.01f));
        Def(A, "CrewDef", "Защита экипажа от потерь", "Crew Loss Protection", "% на корабль", "% per ship", false, Flavor.Defense, "crewdef", S1, 5, 0, 0, 1, 5, 0,
            "Меньше экипажа гибнет при попаданиях по вашим кораблям. Считается на каждый корабль и суммируется по флоту (потери не опускаются ниже 5%).",
            "Fewer crew die when your ships are hit. Counted per ship and summed over the fleet (losses never drop below 5%).", M("ManpowerDamageOverTimeDefense", Add, ShipCrewPath, 0.01f));
        Def(A, "CrewKill", "Уничтожение экипажа врага", "Enemy Crew Destruction", "%", "%", false, Flavor.Offense, "crewkill", new[] { 6, 8, 10 }, 5, 0, 0, 1, 60, 0,
            "Ваш флот убивает больше вражеского экипажа.", "Your fleet kills more enemy crew.", M("ManpowerDamageOverTime", Pct, ShipCrewPath, 0.01f));
        Def(A, "Health", "Прочность корпусов", "Hull Durability", "%", "%", false, Flavor.Defense, "hp", new[] { 2, 3, 4 }, 5, 0, 0, 1, 30, 0,
            "Больше здоровья у каждого корабля.", "More health on every ship.", M("FleetMaximumHealth", Add, ShipPath, 0.01f));
        Def(A, "ShieldAbs", "Поглощение щитами", "Shield Absorption", "%", "%", false, Flavor.Happiness, "abs", new[] { 2, 3, 4 }, 5, 0, 0, 1, 30, 0,
            "Щиты поглощают большую долю урона.", "Shields absorb a larger share of damage.", M("ShipShieldAbsorptionPercent", Add, ShipPath, 0.01f));
        Def(A, "Pierce", "Пробивание щитов", "Shield Penetration", "%", "%", false, Flavor.Offense, "pierce", new[] { 2, 3, 4 }, 5, 0, 0, 1, 25, 0,
            "Выстрелы лучше проходят сквозь вражеские щиты.", "Shots pass enemy shields more easily.", M("ShipShieldPenetrationFactor", Add, ShipPath, 0.01f));
        Def(A, "LootDust", "Пыль за каждое уничтоженное КО", "Dust per Destroyed CP", " Пыли", " Dust", false, Flavor.Money, "loot", new[] { 1, 1, 2 }, 5, 0, 0, 1, 8, 0,
            "Разбор обломков: за каждое уничтоженное КО (командное очко) врага вы получаете Пыль. Работает в автоматических боях.", "Salvage: Dust for every destroyed enemy CP (command point). Works in auto-resolved battles.");
        Def(A, "LootSci", "Наука за каждое уничтоженное КО", "Science per Destroyed CP", " науки", " science", false, Flavor.Science, "loot", new[] { 1, 2, 2 }, 5, 0, 0, 1, 10, 0,
            "Разбор обломков: за каждое уничтоженное КО врага вы получаете очки науки. Работает в автоматических боях.", "Salvage: science for every destroyed enemy CP. Works in auto-resolved battles.");
        Def(A, "LootInfl", "Влияние за каждое уничтоженное КО", "Influence per Destroyed CP", " влияния", " influence", false, Flavor.Influence, "loot", S1, 5, 0, 0, 1, 5, 0,
            "Слава победителя: за каждое уничтоженное КО врага вы получаете Влияние. Работает в автоматических боях.", "Glory of victory: Influence for every destroyed enemy CP. Works in auto-resolved battles.");
        Concept pr = Def(A, "Prisoner", "Пленник", "Captive", " жителя за победу", " citizen per victory", false, Flavor.Manpower, "special", S1, 1, 0, 0, 1, 1, 0,
            "После победы над вражеским флотом в вашей столице появляется один житель фракции побеждённого флота (автоматические бои; пираты и малые фракции жителей не дают).",
            "After a victory over an enemy fleet one citizen of the defeated fleet's faction appears in your capital.");
        pr.Special = "Prisoner";
        Concept eu = Def(A, "Euphoria", "Триумф", "Triumph", " счастья", " happiness", false, Flavor.Happiness, "special", S1, 1, 0, 0, 1, 10, 0,
            "За каждую выигранную битву счастье во всей империи временно растёт; чем больше КО уничтожено, тем дольше это длится (5 ходов плюс 1 ход за каждые 10 КО). Работает в автоматических боях.",
            "For every battle won the happiness of the whole empire rises for a while; the more CP destroyed, the longer it lasts (5 turns plus 1 turn per 10 CP).");
        eu.Special = "Euphoria";
        Get(A, "LootDust").Special = "Loot"; Get(A, "LootSci").Special = "Loot"; Get(A, "LootInfl").Special = "Loot";

        // ---------------------------------------------------------------- admiral (his own ship): class trees of the admirals
        // Ship-skill descriptors are carried by the hero's own ship (HeroShip.RefreshHeroDescriptors adds them
        // to the ship itself), so an empty path targets the hero's ship only, while the fleet is reached through
        // it (HeroFlavors.ShipPath / ShipCrewPath resolve from the ship). Guardian powers up his personal
        // warship; Seeker carries virtual support modules that serve the whole fleet.
        Def(A, "ShipDmg", "Урон орудий корабля героя", "Hero Ship Weapon Damage", "%", "%", false, Flavor.Offense, "dmg", new[] { 8, 10, 12 }, 5, 0, 0, 1, 100, 0,
            "Только корабль героя: установленные на нём орудия бьют сильнее. Проценты умножают урон модулей, так что выигрыш растёт вместе с военными технологиями. Масштаб ванильных навыков адмирала: один навык там даёт до +40% урона.",
            "The hero's own ship only: the guns installed on it hit harder. The percentages multiply module damage, so the gain grows with your military tech. The scale follows vanilla admiral skills: a single vanilla skill grants up to +40% damage.",
            M("Damage", Add, "", 0.01f));
        Def(A, "ShipHp", "Прочность корабля героя", "Hero Ship Hull", "%", "%", false, Flavor.Defense, "hp", new[] { 10, 15, 20 }, 5, 0, 0, 1, 150, 0,
            "Только корабль героя: его корпус выдерживает больше урона. Масштаб ванильных навыков на один корабль: до +80% за навык.",
            "The hero's own ship only: its hull takes more punishment. The scale follows vanilla per-ship skills: up to +80% in one skill.",
            M("MaximumHealth", Pct, "", 0.01f));
        Def(A, "ShipCrit", "Шанс крита корабля героя", "Hero Ship Critical Hit Chance", "%", "%", false, Flavor.Offense, "crit", new[] { 2, 3, 4 }, 5, 0, 0, 1, 25, 0,
            "Только корабль героя: его выстрелы чаще наносят двойной урон.",
            "The hero's own ship only: its shots deal double damage more often.",
            M("CriticalHitChance", Add, "", 0.01f));
        Def(A, "ShipCrewKill", "Выкашивание экипажей", "Crew Culling", "%", "%", false, Flavor.Offense, "crewkill", new[] { 10, 15, 20 }, 5, 0, 0, 1, 100, 0,
            "Только корабль героя: его выстрелы выкашивают экипажи вражеских кораблей, ослабляя их абордаж и оборону.",
            "The hero's own ship only: its shots mow down enemy crews, weakening their boarding and defense.",
            M("ManpowerDamageOverTime", Pct, "", 0.01f));
        Def(A, "Crew", "Запасные экипажи", "Reserve Crews", "%", "%", false, Flavor.Manpower, "crew", new[] { 5, 6, 8 }, 5, 0, 0, 1, 40, 0,
            "Корабли флота героя берут на борт больше экипажа: больше солдат для абордажа, обороны и осады.",
            "The ships of the hero's fleet take more crew aboard: more soldiers for boarding, defense and sieges.",
            M("MaximumShipManpower", Pct, ShipCrewPath, 0.01f));
        // Library only (no tree references it): the vanilla jammer mechanic (ModuleSupportJammer lowers the
        // ship's TargetingWeightBonus, so enemy guns pick the hero's ship as a target less often). It lost
        // its Seeker slot to the flak barrage; the concept stays available for later designs.
        Def(A, "Jammer", "Генератор помех", "Jammer", " к весу цели", " target weight", true, Flavor.Deposit, "evasion", new[] { 2, 3, 4 }, 5, 0, 0, 1, 30, 0,
            "Только корабль героя: вражеские орудия реже выбирают его целью. Механика ванильного модуля «Глушитель» (вес цели), ванильные модули дают −10/−20.",
            "The hero's own ship only: enemy guns pick it as a target less often. The vanilla Jammer module mechanic (target weight); vanilla modules give -10/-20.",
            M("TargetingWeightBonus", Add, "", -1f));
        // Seeker T2: fleet flak - the exact modifier pair of the vanilla "Flak enhancer" support module:
        // FlakEfficiency on the kinetic guns of every ship (their anti-missile fire) and +4 damage per point
        // on the flak guns (the flak salvo damage equals the module's Damage property).
        Def(A, "Flak", "Зенитный огонь флота", "Fleet Flak Barrage", " ед. зенитного усиления", " flak power", false, Flavor.Defense, "flak", new[] { 1, 2, 3 }, 5, 0, 0, 1, 25, 0,
            "Кинетические орудия всех кораблей флота героя сбивают вражеские ракеты и истребители эффективнее, а зенитные орудия бьют сильнее (+4 урона за единицу). Повторяет эффект ванильного модуля «Улучшение зениток», но на весь флот.",
            "The kinetic guns of every ship in the hero's fleet shoot down enemy missiles and fighters better, and flak guns hit harder (+4 damage per point). Mirrors the vanilla Flak Enhancer module, fleet-wide.",
            M("FlakEfficiency", Add, "./ClassGarrisonFleet/ClassShip/ClassSection/ClassModuleWeapon,ClassModuleWeaponKinetic", 1f),
            M("Damage", Add, "./ClassGarrisonFleet/ClassShip/ClassSection/ClassModuleWeapon,ClassModuleWeaponFlack", 4f));
        // Seeker T3: tech-mirrored stealth. No static modifiers: the skill level works as a CAP for the best
        // researched cloaking module (ModuleSupportInvisibilityLevel1/2, Ambush, FullCombat -> the tech
        // descriptors they require), and the hero ship carries the same AntiVisionStrengthForModule as the
        // module would give. HeroRework injects the descriptor live (AddMirrorSkills).
        Concept stealth = Def(A, "Stealth", "Маскировка флота", "Fleet Cloaking", " ур. маскировки", " cloaking lvl.", false, Flavor.Science, "stealth", S1, 3, 0, 0, 1, 3, 0,
            "Корабль героя несёт генератор маскировки: уровень равен лучшему изученному модулю маскировки империи, но не выше уровня навыка. Изучите новую технологию маскировки - и навык сам станет сильнее (уровень 3 = маскировка на всю битву). Флот получает действие «Скрытность».",
            "The hero's ship carries a cloaking generator: its level equals the best cloaking module the empire has researched, capped by the skill level. Research a better cloaking technology and the skill upgrades itself (level 3 = cloaked through the whole battle). The fleet gains the Cloak action.");
        stealth.Special = "Stealth";
        // Seeker T3: tech-mirrored detection. Same mirroring of the anti-cloaker modules
        // (ModuleSupportInvisibilityDetectionLevel1/2/3): the fleet sees cloaked enemies at or below its
        // detection level; from level 2 the hero ship also carries detection probes (probe stock = level).
        Concept detect = Def(A, "Detect", "Сеть обнаружения", "Detection Grid", " ур. обнаружения", " detection lvl.", false, Flavor.Science, "detect", S1, 3, 0, 0, 1, 3, 0,
            "Корабль героя несёт детектор невидимости: уровень равен лучшему изученному модулю обнаружения империи, но не выше уровня навыка. Флот видит замаскированных врагов своего уровня и ниже; со 2-го уровня корабль ещё и несёт зонды обнаружения (запас = уровню).",
            "The hero's ship carries an invisibility detector: its level equals the best detection module the empire has researched, capped by the skill level. The fleet sees cloaked enemies of its level and below; from level 2 the ship also carries detection probes (stock = level).");
        detect.Special = "Detect";
        // Seeker ultimate: the hero ship is rebuilt as a mothership (a new hull with a support-carrier
        // module layout). The hull swap itself is the special; no passive bonuses.
        Concept mothership = Def(A, "Mothership", "Материнский корабль", "Mothership", "", "", false, Flavor.Mobility, "special", S1, 1, 0, 0, 1, 1, 0,
            "Корабль героя перерождается в материнский корабль - летающую базу флота с новой раскладкой модулей: больше поддержки, эскадрилий и дальнобойных средств, меньше личного оружия.",
            "The hero's ship is reborn as a mothership - a flying fleet base with a new module layout: more support, squadrons and long-range tools, less personal weaponry.");
        mothership.Special = "Mothership";

        // ---------------------------------------------------------------- governor (star system)
        const string SP = HeroFlavors.SystemPath;
        Def(G, "Prod", "Промышленность", "Industry", "", "", false, Flavor.Production, "prod", new[] { 2, 3, 4 }, 5, 0, 0, 1, 40, 0,
            "Производство системы.", "Production of the system.", M("SystemProduction", Add, SP, 1f));
        Def(G, "Food", "Рост населения", "Population Growth", "", "", false, Flavor.Growth, "food", new[] { 3, 4, 5 }, 5, 0, 0, 1, 40, 0,
            "Еда (рост) системы.", "Food (growth) of the system.", M("SystemGrowth", Add, SP, 1f));
        Def(G, "Sci", "Исследования", "Research", "", "", false, Flavor.Science, "sci", new[] { 4, 6, 8 }, 5, 0, 0, 1, 80, 0,
            "Наука системы.", "Science of the system.", M("SystemResearch", Add, SP, 1f));
        Def(G, "Money", "Доходы", "Revenue", "", "", false, Flavor.Money, "money", new[] { 2, 3, 4 }, 5, 0, 0, 1, 30, 0,
            "Пыль системы.", "Dust of the system.", M("SystemMoney", Add, SP, 1f));
        Def(G, "Infl", "Влияние системы", "System Influence", "", "", false, Flavor.Influence, "infl", new[] { 2, 3, 4 }, 5, 0, 0, 1, 25, 0,
            "Влияние системы.", "Influence of the system.", M("SystemEmpirePoint", Add, SP, 1f));
        Def(G, "Happy", "Общественный порядок", "Public Order", "", "", false, Flavor.Happiness, "happy", new[] { 2, 3, 4 }, 5, 0, 0, 1, 25, 0,
            "Счастье в системе.", "Happiness in the system.", M("Happiness", Add, SP, 1f));
        Def(G, "Upkeep", "Бережливое управление", "Frugal Administration", "% Пыли", "% Dust", true, Flavor.Upkeep, "upkeep", new[] { 3, 4, 5 }, 5, 0, 0, 1, 40, 0,
            "Система стоит дешевле в Пыли.", "The system costs less Dust.", M("SystemMoneyUpkeep", Pct, SP, -0.01f));
        Def(G, "Garrison", "Гарнизон", "Garrison", "%", "%", false, Flavor.Offense, "garrison", new[] { 4, 6, 8 }, 5, 0, 0, 1, 50, 0,
            "Наземные защитники системы крепче.", "The ground defenders of the system are sturdier.", M("GroundBattleDefenderTroopsMaxHealthMultiplier", Add, SP, 0.01f));
        Def(G, "Deposit", "Добыча ресурсов", "Resource Extraction", "", "", false, Flavor.Deposit, "deposit", S1, 3, 0, 0, 1, 3, 0,
            "Каждое месторождение системы даёт больше.", "Every deposit of the system yields more.", M("DepositValue", Add, HeroFlavors.DepositPath, 1f));
        Def(G, "TradeProd", "Торговые пути: производство", "Trade Routes: Industry", "%", "%", false, Flavor.Mobility, "tradep", new[] { 4, 6, 8 }, 5, 0, 0, 1, 50, 0,
            "Торговые пути системы дают больше производства.", "Trade routes of the system give more production.", M("TradingRoutesBonus_Production", Pct, SP, 0.01f));
        Def(G, "TradeFood", "Торговые пути: рост", "Trade Routes: Growth", "%", "%", false, Flavor.Mobility, "tradef", new[] { 4, 6, 8 }, 5, 0, 0, 1, 50, 0,
            "Торговые пути системы дают больше еды.", "Trade routes of the system give more food.", M("TradingRoutesBonus_Growth", Pct, SP, 0.01f));

        // ---------------------------------------------------------------- senator (whole empire through the senate)
        const string SS = HeroFlavors.SenateSystem, SF = HeroFlavors.SenateFleet, SE = HeroFlavors.SenateEmpire;
        Def(S, "Prod", "Государственная индустрия", "State Industry", "%", "%", false, Flavor.Production, "prod", new[] { 2, 3, 4 }, 0, 3, 5, 1, 15, 30,
            "Производство во всех системах империи.", "Production in every system of the empire.", M("SystemProduction", Pct, SS, 0.01f));
        Def(S, "Food", "Аграрная реформа", "Agrarian Reform", "%", "%", false, Flavor.Growth, "food", new[] { 2, 3, 4 }, 0, 3, 5, 1, 15, 30,
            "Рост во всех системах империи.", "Growth in every system of the empire.", M("SystemGrowth", Pct, SS, 0.01f));
        Def(S, "Sci", "Научный комитет", "Science Committee", "%", "%", false, Flavor.Science, "sci", new[] { 2, 3, 4 }, 0, 3, 5, 1, 15, 30,
            "Наука во всех системах империи.", "Science in every system of the empire.", M("SystemResearch", Pct, SS, 0.01f));
        Def(S, "Money", "Казначейство", "Treasury", "%", "%", false, Flavor.Money, "money", new[] { 2, 3, 4 }, 0, 3, 5, 1, 15, 30,
            "Пыль во всех системах империи.", "Dust in every system of the empire.", M("SystemMoney", Pct, SS, 0.01f));
        Def(S, "Infl", "Имперская идеология", "Imperial Ideology", "%", "%", false, Flavor.Influence, "infl", new[] { 2, 3, 4 }, 0, 3, 5, 1, 15, 30,
            "Влияние во всех системах империи.", "Influence in every system of the empire.", M("SystemEmpirePoint", Pct, SS, 0.01f));
        Def(S, "Happy", "Всеобщее благополучие", "General Welfare", "", "", false, Flavor.Happiness, "happy", new[] { 1, 2, 2 }, 0, 3, 5, 1, 8, 12,
            "Счастье во всех системах империи.", "Happiness in every system of the empire.", M("Happiness", Add, SS, 1f));
        Def(S, "Upkeep", "Режим экономии", "Austerity", "% Пыли", "% Dust", true, Flavor.Upkeep, "upkeep", new[] { 2, 3, 3 }, 0, 3, 5, 1, 12, 20,
            "Системы империи стоят дешевле в Пыли.", "Systems of the empire cost less Dust.", M("SystemMoneyUpkeep", Pct, SS, -0.01f));
        Def(S, "Dmg", "Военные ассигнования", "Military Appropriations", "%", "%", false, Flavor.Offense, "dmg", new[] { 1, 2, 3 }, 0, 3, 5, 1, 10, 20,
            "Урон оружия всех флотов империи.", "Weapon damage of every fleet of the empire.", M("ShipDamage", Add, SF, 0.01f));
        Def(S, "Hp", "Бронирование флотов", "Fleet Armor Programs", "%", "%", false, Flavor.Defense, "hp", new[] { 1, 2, 3 }, 0, 3, 5, 1, 10, 20,
            "Прочность корпусов всех флотов империи.", "Hull durability of every fleet of the empire.", M("FleetMaximumHealth", Add, SF, 0.01f));
        Def(S, "Regen", "Службы ремонта", "Repair Corps", "% в ход", "% per turn", false, Flavor.Sustain, "regen", S1, 0, 3, 5, 1, 5, 8,
            "Все флоты империи сами чинят корпуса каждый ход.", "Every fleet of the empire repairs itself each turn.", M("HealthRegenerationRatio", Add, SF, 0.01f));
        Def(S, "Move", "Имперская навигация", "Imperial Navigation", "", "", false, Flavor.Mobility, "move", S1, 0, 1, 2, 1, 2, 3,
            "Дальность хода всех флотов империи.", "Movement range of every fleet of the empire.", M("ShipMaximumMovement", Add, SF, 1f));
        Def(S, "Vision", "Разведывательная сеть", "Intelligence Network", "", "", false, Flavor.Science, "vision", S1, 0, 1, 2, 1, 2, 3,
            "Дальность обзора всех флотов империи.", "Vision range of every fleet of the empire.", M("VisionRange", Add, SF, 1f));
        Def(S, "Diplo", "Дипломатический корпус", "Diplomatic Corps", "% стоимости договоров", "% treaty cost", true, Flavor.Diplomacy, "diplo", new[] { 5, 5, 5 }, 0, 3, 5, 1, 30, 50,
            "Мирные и агрессивные договоры стоят дешевле.", "Peaceful and aggressive treaties cost less.",
            M("PeacefulTreatyCostReduction", Add, SE, -0.01f), M("AggressiveTreatyCostReduction", Add, SE, -0.01f));
        Def(S, "Votes", "Голоса в Галактическом Совете", "Galactic Council Votes", " голосов", " votes", false, Flavor.Votes, "votes", new[] { 3, 4, 5 }, 0, 3, 5, 1, 18, 40,
            "Дополнительные голоса вашей империи в Галактическом Совете, пока герой представляет партию в Сенате.",
            "Extra votes of your empire in the Galactic Council while the hero represents his party in the Senate.");
        LoadSpecs();
    }

    public static Concept Get(HeroChannel channel, string key)
    {
        Concept c;
        string prefix = channel == HeroChannel.Ship ? "A." : channel == HeroChannel.System ? "G." : "S.";
        return library.TryGetValue(prefix + key, out c) ? c : null;
    }

    public static List<Concept> AllConcepts() { return new List<Concept>(library.Values); }


    public static HeroChannel ChannelOfRole(char role) { return role == 'A' ? HeroChannel.Ship : role == 'G' ? HeroChannel.System : HeroChannel.Senate; }
}

/// <summary>
/// Tech-mirrored specials (Seeker stealth / detection): the skill level is only a CAP - the live level equals
/// the best module the empire has actually researched. Pure data and logic, no game types: the tests check it
/// directly, HeroRework wires it to the empire's researched technologies.
/// </summary>
public static class HeroMirror
{
    /// <summary>One vanilla module the skill mirrors: its name, the technology descriptor it needs (verified
    /// fallback for when the module is missing at runtime) and the mirror level it carries.</summary>
    public sealed class Source
    {
        public readonly string Module, FallbackTech;
        public readonly int Level;
        public Source(string module, string fallbackTech, int level) { Module = module; FallbackTech = fallbackTech; Level = level; }
    }

    // Cloaker I/II/III of the military tree (Penumbra): the cloak survives longer each level.
    public static readonly Source[] Stealth =
    {
        new Source("ModuleSupportInvisibilityLevel1",     "TechnologyArmyCap1",  1),
        new Source("ModuleSupportInvisibilityLevel2",     "TechnologyPlayDeck1", 2),
        new Source("ModuleSupportInvisibilityAmbush",     "TechnologyPlayDeck3", 3),
        new Source("ModuleSupportInvisibilityFullCombat", "TechnologyPlayDeck5", 3),
    };

    // Anti-Cloaker I/II/III: detection levels 1-3; level 2+ also carry detection probes.
    public static readonly Source[] Detect =
    {
        new Source("ModuleSupportInvisibilityDetectionLevel1", "TechnologyPlayDeck2", 1),
        new Source("ModuleSupportInvisibilityDetectionLevel2", null, 2),
        new Source("ModuleSupportInvisibilityDetectionLevel3", null, 3),
        // alternate module naming used by some versions of the game
        new Source("ModuleInvisibilityDetectionProbeLevel2",   null, 2),
        new Source("ModuleInvisibilityDetectionProbeLevel3",   null, 3),
    };

    public static Source[] TableOf(string special) { return special == "Stealth" ? Stealth : special == "Detect" ? Detect : null; }

    /// <summary>Best mirror level the empire's research allows: for every source module the technology it
    /// requires (moduleTech resolves it at runtime, the fallback is the verified name) must be researched.</summary>
    public static int LevelOf(Source[] table, Func<string, string> moduleTech, Func<string, bool> researched)
    {
        if (table == null) return 0;
        int level = 0;
        for (int i = 0; i < table.Length; i++)
        {
            string tech = moduleTech(table[i].Module) ?? table[i].FallbackTech;
            if (tech == null || table[i].Level <= level) continue;
            if (researched(tech)) level = table[i].Level;
        }
        return level;
    }

    /// <summary>The hero ship carries the modifier pair of the vanilla module of this level (HeroRework turns it into a descriptor).</summary>
    public static string[] ModifiersOf(string special, int level)
    {
        if (special == "Stealth") return new[] { "AntiVisionStrengthForModule +" + level };
        if (special == "Detect")
            return level >= 2
                ? new[] { "VisionStrength +" + level, "MaximumShipDetectionProbeStock +" + level }
                : new[] { "VisionStrength +" + level };
        if (special == "Mothership") return new[] { "CommandPoints Force 0" };
        return new string[0];
    }
}

/// <summary>
/// The Seeker ultimate: the hero ship is REBUILT as a mothership - a large carrier hull of the empire's
/// faction with a support-carrier module layout (squadrons and support instead of personal weapons).
/// Pure data and logic, no game types: the tests check the plan, HeroRework wires it to the game.
/// </summary>
public static class HeroMothership
{
    /// <summary>The design role: the large carrier hulls carry layouts for it (not for ShipRoleHero), and it
    /// buffs squadrons (+10% health/damage). The hero ship keeps its 0 command points through the skill
    /// descriptor (HeroMirror.ModifiersOf("Mothership")), whatever hull it wears.</summary>
    public const string RoleName = "ShipRoleCarrier";

    /// <summary>The large carrier hull line, one per major faction (HullLarge01Terrans, ...Cravers, ...).</summary>
    public const string HullPrefix = "HullLarge01";
    public const string HullFallback = "HullLarge01Terrans";

    // basic-tier vanilla modules - the same tier the vanilla hero designs are built from
    public const string SquadronModule = "ModuleSquadronBomber1";                       // the bomber wing
    public const string SupportBoosterModule = "ModuleSupportSquadronDefenseEnhancer1Strategic3";  // one per ship: squadron defense
    public const string SupportModule = "ModuleSupportEngine2";                          // plain engines
    public const string ShieldModule = "ModuleDefenseShield1";
    public const string PlatingModule = "ModuleDefenseHullPlating1";

    public static string HullNameFor(string factionName) { return HullPrefix + factionName; }
    public static string DesignNameFor(string hullName) { return "HRMothershipShip_" + hullName; }

    /// <summary>The module a slot of the carrier hull receives (null = the slot stays empty): every
    /// squadron-capable slot carries a bomber wing; large weapon mounts and pure weapon slots stay empty
    /// (the mothership is a flying base, not a battleship); defense-capable slots alternate shield and
    /// plating; the first pure support slot carries the squadron booster, the others engines.</summary>
    public static string ModuleFor(string[] restrictedCategories, bool isLargeSlot, int supportSlotsFilled, int defenseSlotsFilled)
    {
        if (restrictedCategories == null || restrictedCategories.Length == 0 || isLargeSlot) return null;
        bool squadron = false, support = false, defense = false;
        for (int i = 0; i < restrictedCategories.Length; i++)
        {
            if (restrictedCategories[i] == "Squadron") squadron = true;
            else if (restrictedCategories[i] == "Support") support = true;
            else if (restrictedCategories[i] == "Defense") defense = true;
        }
        if (squadron) return SquadronModule;
        if (defense) return (defenseSlotsFilled % 2 == 0) ? ShieldModule : PlatingModule;
        if (support) return supportSlotsFilled == 0 ? SupportBoosterModule : SupportModule;
        return null;
    }
}


