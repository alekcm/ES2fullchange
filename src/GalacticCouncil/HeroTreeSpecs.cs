// Hero rework (redesign): the written trees (one line per role) and the builder that turns a line into a tree.
using System;
using System.Collections.Generic;
using System.Text;

public static partial class HeroDesign
{
    static readonly Dictionary<string, string> specs = new Dictionary<string, string>();
    static readonly Dictionary<string, string> ultimateTitles = new Dictionary<string, string>();

    public static readonly string[] PartyKeys = { "Party01", "Party02", "Party03", "Party04", "Party05", "Party06" };
    public static readonly string[] RaceKeys = { "Sophons", "Cravers", "Terrans", "Venetians", "Vampirilis", "TimeLords", "Unfallen", "Endless", "Horatio", "Pirates", "Insectoids", "Advanced", "Primitives" };
    public static readonly string[] ClassKeys = { "Admiral", "Adventurer", "Administrator", "Corporate" };
    public static readonly string[] DictKeys = { "Sophons", "Cravers", "Terrans", "Venetians", "Vampirilis", "TimeLords", "Unfallen", "Endless", "Horatio", "Pirates", "Insectoids", "Advanced", "Primitives", "Other" };

    static void Spec(string key, char role, string line) { specs[key + "|" + role] = line; }
    static void Ult(string key, string ru, string en) { ultimateTitles[key] = ru + "|" + en; }

    static void LoadSpecs()
    {
        // ------------------------------------------------ parties: admirals (A), governors (G), senators (S; the dictator uses the S line)
        Spec("Party01", 'A', "T1:Armor;T2:Repair;T3:Kinetic;U:Armor,Repair,Kinetic,LootDust");
        Spec("Party02", 'A', "T1:Vision,Xp;T2:Energy,Shield;T3:ShieldReload;U:Energy,Shield,ShieldReload,Xp");
        Spec("Party03", 'A', "T1:Upkeep;T2:CrewDef;T3:ShieldAbs;U:Upkeep,CrewDef,ShieldAbs,Health");
        Spec("Party04", 'A', "T1:Evasion;T2:Regen;T3:Health;U:Evasion,Regen,Health,Move");
        Spec("Party05", 'A', "T1:Move;T2:CrewKill;T3:Accuracy;U:Move,CrewKill,Accuracy,LootInfl");
        Spec("Party06", 'A', "T1:Siege;T2:Crit;T3:Rate;U:Siege,Crit,Rate,Damage");
        Spec("Party01", 'G', "T1:Prod;T2:Deposit;T3:TradeProd;U:Prod,Deposit,TradeProd,Money");
        Spec("Party02", 'G', "T1:Sci;T2:Money;T3:Infl;U:Sci,Money,Infl,Happy");
        Spec("Party03", 'G', "T1:Happy;T2:Upkeep;T3:Food;U:Happy,Upkeep,Food,Infl");
        Spec("Party04", 'G', "T1:Food;T2:TradeFood;T3:Deposit;U:Food,TradeFood,Deposit,Happy");
        Spec("Party05", 'G', "T1:Infl;T2:Happy;T3:Garrison;U:Infl,Happy,Garrison,Food");
        Spec("Party06", 'G', "T1:Garrison;T2:Prod;T3:Money;U:Garrison,Prod,Money,Happy");
        Spec("Party01", 'S', "T1:Prod;T2:Hp;T3:Money;U:Prod,Hp,Money,Upkeep");
        Spec("Party02", 'S', "T1:Sci;T2:Vision;T3:Infl;U:Sci,Vision,Infl,Money");
        Spec("Party03", 'S', "T1:Happy;T2:Votes;T3:Diplo;U:Happy,Votes,Diplo,Upkeep");
        Spec("Party04", 'S', "T1:Food;T2:Happy;T3:Regen;U:Food,Happy,Regen,Upkeep");
        Spec("Party05", 'S', "T1:Infl;T2:Votes;T3:Happy;U:Infl,Votes,Happy,Diplo");
        Spec("Party06", 'S', "T1:Dmg;T2:Hp;T3:Move;U:Dmg,Hp,Move,Prod");
        Ult("Party01", "Промышленный джаггернаут", "Industrial Juggernaut");
        Ult("Party02", "Научное превосходство", "Scientific Supremacy");
        Ult("Party03", "Щит мира", "Shield of Peace");
        Ult("Party04", "Живой флот", "Living Fleet");
        Ult("Party05", "Священный поход", "Holy Crusade");
        Ult("Party06", "Тотальная война", "Total War");

        // ------------------------------------------------ races (vanilla race trees)
        Spec("RaceSophons", 'A', "T1:Vision,Move;T2:Energy;T3:Shield;U:Energy,Shield,Vision,LootSci");
        Spec("RaceCravers", 'A', "T1:Health,CrewKill;T2:Kinetic;T3:Siege;U:Prisoner,Health,Kinetic,CrewKill");
        Spec("RaceTerrans", 'A', "T1:Armor,Repair;T2:Energy;T3:ShieldReload;U:Euphoria,Armor,Energy,ShieldReload");
        Spec("RaceVenetians", 'A', "T1:Upkeep,LootDust;T2:Evasion;T3:Move;U:Upkeep,LootDust,Evasion,Move");
        Spec("RaceVampirilis", 'A', "T1:Regen,CrewKill;T2:Crit;T3:Evasion;U:Regen,CrewKill,Crit,LootInfl");
        Spec("RaceTimeLords", 'A', "T1:Move,Vision;T2:Rate;T3:Evasion;U:Rate,Move,Evasion,Xp");
        Spec("RaceUnfallen", 'A', "T1:Armor,ShieldAbs;T2:Health;T3:Repair;U:Armor,ShieldAbs,Health,Regen");
        Spec("RaceEndless", 'A', "T1:Xp,Pierce;T2:ShieldReload;T3:Accuracy;U:Xp,Pierce,ShieldReload,Energy");
        Spec("RaceHoratio", 'A', "T1:Move,Upkeep;T2:LootDust;T3:Crit;U:Move,Upkeep,LootDust,Crit");
        Spec("RacePirates", 'A', "T1:LootDust,Rate;T2:Crit;T3:Move;U:LootDust,Rate,Crit,Move");
        Spec("RaceInsectoids", 'A', "T1:Health,Regen;T2:Kinetic;T3:Move;U:Health,Regen,Kinetic,CrewKill");
        Spec("RaceAdvanced", 'A', "T1:Vision,Shield;T2:Energy;T3:Accuracy;U:Vision,Shield,Energy,Pierce");
        Spec("RacePrimitives", 'A', "T1:Armor,Evasion;T2:Kinetic;T3:Repair;U:Armor,Evasion,Kinetic,Repair");
        Spec("RaceSophons", 'G', "T1:Sci;T2:Food;T3:Infl;U:Sci,Food,Infl,Happy");
        Spec("RaceCravers", 'G', "T1:Garrison,Prod;T2:Infl;T3:Deposit;U:Garrison,Prod,Infl,Deposit");
        Spec("RaceTerrans", 'G', "T1:Prod,Money;T2:Happy;T3:TradeProd;U:Prod,Money,Happy,TradeProd");
        Spec("RaceVenetians", 'G', "T1:Money,Infl;T2:Upkeep;T3:TradeFood;U:Money,Infl,Upkeep,TradeFood");
        Spec("RaceVampirilis", 'G', "T1:Infl,Garrison;T2:Food;T3:Money;U:Infl,Garrison,Food,Money");
        Spec("RaceTimeLords", 'G', "T1:Sci,Infl;T2:Prod;T3:Deposit;U:Sci,Infl,Prod,Deposit");
        Spec("RaceUnfallen", 'G', "T1:Garrison,Happy;T2:Food;T3:Upkeep;U:Garrison,Happy,Food,Upkeep");
        Spec("RaceEndless", 'G', "T1:Sci,Prod;T2:Infl;T3:Deposit;U:Sci,Prod,Infl,Deposit");
        Spec("RaceHoratio", 'G', "T1:Money,Upkeep;T2:TradeProd;T3:Infl;U:Money,Upkeep,TradeProd,Infl");
        Spec("RacePirates", 'G', "T1:Money,Garrison;T2:Prod;T3:Upkeep;U:Money,Garrison,Prod,Upkeep");
        Spec("RaceInsectoids", 'G', "T1:Food,Garrison;T2:Prod;T3:Happy;U:Food,Garrison,Prod,Happy");
        Spec("RaceAdvanced", 'G', "T1:Sci,Prod;T2:Money;T3:Happy;U:Sci,Prod,Money,Happy");
        Spec("RacePrimitives", 'G', "T1:Deposit,Food;T2:Garrison;T3:Happy;U:Deposit,Food,Garrison,Happy");
        Spec("RaceSophons", 'S', "T1:Sci;T2:Vision;T3:Food;U:Sci,Vision,Food,Infl");
        Spec("RaceCravers", 'S', "T1:Dmg;T2:Hp;T3:Prod;U:Dmg,Hp,Prod,Move");
        Spec("RaceTerrans", 'S', "T1:Prod;T2:Money;T3:Happy;U:Prod,Money,Happy,Infl");
        Spec("RaceVenetians", 'S', "T1:Money;T2:Votes;T3:Diplo;U:Money,Votes,Diplo,Upkeep");
        Spec("RaceVampirilis", 'S', "T1:Infl;T2:Regen;T3:Dmg;U:Infl,Regen,Dmg,Happy");
        Spec("RaceTimeLords", 'S', "T1:Sci;T2:Move;T3:Infl;U:Sci,Move,Infl,Vision");
        Spec("RaceUnfallen", 'S', "T1:Hp;T2:Happy;T3:Food;U:Hp,Happy,Food,Regen");
        Spec("RaceEndless", 'S', "T1:Sci;T2:Prod;T3:Infl;U:Sci,Prod,Infl,Money");
        Spec("RaceHoratio", 'S', "T1:Money;T2:Diplo;T3:Move;U:Money,Diplo,Move,Upkeep");
        Spec("RacePirates", 'S', "T1:Dmg;T2:Money;T3:Move;U:Dmg,Money,Move,Upkeep");
        Spec("RaceInsectoids", 'S', "T1:Food;T2:Regen;T3:Hp;U:Food,Regen,Hp,Prod");
        Spec("RaceAdvanced", 'S', "T1:Sci;T2:Prod;T3:Vision;U:Sci,Prod,Vision,Money");
        Spec("RacePrimitives", 'S', "T1:Food;T2:Hp;T3:Happy;U:Food,Hp,Happy,Regen");
        Ult("RaceSophons", "Охотники за знаниями", "Knowledge Hunters");
        Ult("RaceCravers", "Пленение побеждённых", "Taking Captives");
        Ult("RaceTerrans", "Триумф Объединённой Империи", "Triumph of the United Empire");
        Ult("RaceVenetians", "Золотой флот", "Golden Fleet");
        Ult("RaceVampirilis", "Кровавая жатва", "Blood Harvest");
        Ult("RaceTimeLords", "Хозяева времени", "Masters of Time");
        Ult("RaceUnfallen", "Несокрушимые", "The Unbreakable");
        Ult("RaceEndless", "Наследие Эндлесс", "Endless Legacy");
        Ult("RaceHoratio", "Удача Горация", "Horatio's Luck");
        Ult("RacePirates", "Королева пиратов", "Pirate Queen");
        Ult("RaceInsectoids", "Рой", "The Swarm");
        Ult("RaceAdvanced", "Передовая технология", "Advanced Technology");
        Ult("RacePrimitives", "Дикая сила", "Primal Strength");

        // ------------------------------------------------ classes
        // Guardian: the combat line of the hero's PERSONAL ship (empty path = the ship itself).
        // Seeker: utility line - virtual support modules serving the whole fleet through the hero's ship.
        Spec("ClassAdmiral", 'A', "T1:ShipDmg;T2:ShipHp;T3:ShipCrit;U:ShipDmg,ShipHp,ShipCrit,ShipCrewKill");
        Spec("ClassAdventurer", 'A', "T1:Move;T2:Repair;T3:Vision;U:Move,Repair,Vision,Crew");
        Spec("ClassAdministrator", 'G', "T1:Happy;T2:Prod;T3:Upkeep;U:Happy,Prod,Upkeep,Infl");
        Spec("ClassCorporate", 'S', "T1:Money;T2:Upkeep;T3:Prod;U:Money,Upkeep,Prod,Infl");
        Ult("ClassAdmiral", "Личный флагман", "Personal Flagship");
        Ult("ClassAdventurer", "Материнский корабль", "Mothership");
        Ult("ClassAdministrator", "Образцовый управитель", "Model Administrator");
        Ult("ClassCorporate", "Корпоративный магнат", "Corporate Magnate");

        // ------------------------------------------------ dictator trees of the factions (written for the dictator, two skills in each tier)
        Spec("DictSophons", 'D', "T1:Sci,Food;T2:Vision,Infl;T3:Money,Happy;U:Sci,Food,Infl,Happy");
        Spec("DictCravers", 'D', "T1:Dmg,Hp;T2:Prod,Move;T3:Happy,Upkeep;U:Dmg,Hp,Prod,Happy");
        Spec("DictTerrans", 'D', "T1:Prod,Money;T2:Happy,Infl;T3:Upkeep,Hp;U:Prod,Money,Happy,Infl");
        Spec("DictVenetians", 'D', "T1:Money,Votes;T2:Diplo,Infl;T3:Upkeep,Happy;U:Money,Votes,Diplo,Infl");
        Spec("DictVampirilis", 'D', "T1:Infl,Regen;T2:Dmg,Happy;T3:Food,Hp;U:Infl,Regen,Dmg,Happy");
        Spec("DictTimeLords", 'D', "T1:Sci,Move;T2:Vision,Infl;T3:Regen,Prod;U:Sci,Move,Vision,Infl");
        Spec("DictUnfallen", 'D', "T1:Hp,Happy;T2:Food,Regen;T3:Upkeep,Infl;U:Hp,Happy,Food,Regen");
        Spec("DictEndless", 'D', "T1:Sci,Prod;T2:Infl,Money;T3:Vision,Happy;U:Sci,Prod,Infl,Money");
        Spec("DictHoratio", 'D', "T1:Money,Diplo;T2:Move,Upkeep;T3:Infl,Votes;U:Money,Diplo,Move,Votes");
        Spec("DictPirates", 'D', "T1:Dmg,Money;T2:Move,Upkeep;T3:Hp,Prod;U:Dmg,Money,Move,Prod");
        Spec("DictInsectoids", 'D', "T1:Food,Regen;T2:Hp,Prod;T3:Happy,Upkeep;U:Food,Regen,Hp,Prod");
        Spec("DictAdvanced", 'D', "T1:Sci,Prod;T2:Money,Vision;T3:Infl,Happy;U:Sci,Prod,Money,Vision");
        Spec("DictPrimitives", 'D', "T1:Food,Hp;T2:Happy,Regen;T3:Upkeep,Infl;U:Food,Hp,Happy,Regen");
        Spec("DictOther", 'D', "T1:Prod,Sci;T2:Money,Infl;T3:Happy,Upkeep;U:Prod,Sci,Money,Infl");
        Ult("DictSophons", "Эра знаний", "Age of Knowledge");
        Ult("DictCravers", "Железная диктатура", "Iron Dictatorship");
        Ult("DictTerrans", "Золотой век Земли", "Golden Age of Earth");
        Ult("DictVenetians", "Республика купцов", "Merchant Republic");
        Ult("DictVampirilis", "Вечная власть", "Eternal Rule");
        Ult("DictTimeLords", "Власть над временем", "Dominion over Time");
        Ult("DictUnfallen", "Непоколебимый порядок", "Unshakable Order");
        Ult("DictEndless", "Возрождение Эндлесс", "Endless Rebirth");
        Ult("DictHoratio", "Империя Горация", "Horatio's Empire");
        Ult("DictPirates", "Пиратское королевство", "Pirate Kingdom");
        Ult("DictInsectoids", "Единый улей", "One Hive");
        Ult("DictAdvanced", "Технократия", "Technocracy");
        Ult("DictPrimitives", "Племенной союз", "Tribal Union");
        Ult("DictOther", "Единая власть", "United Rule");
    }

    // ---------------------------------------------------------------- tree keys and names

    public static string TreeKeyOfVanilla(string vanillaTreeName)
    {
        const string prefix = "HeroSkillTree";
        if (vanillaTreeName == null || !vanillaTreeName.StartsWith(prefix, StringComparison.Ordinal)) return null;
        string rest = vanillaTreeName.Substring(prefix.Length);
        if (rest == "Generic") return null;
        if (rest == "MinorFaction01") rest = "Pirates";
        else if (rest == "MinorFaction02") rest = "Insectoids";
        else if (rest == "MinorFaction03") rest = "Advanced";
        else if (rest == "MinorFaction04") rest = "Primitives";
        if (Array.IndexOf(ClassKeys, rest) >= 0) return "Class" + rest;
        if (Array.IndexOf(RaceKeys, rest) >= 0) return "Race" + rest;
        return null;
    }

    public static string TreeName(string key, char role)
    {
        return "HRTree" + key + (key.StartsWith("Dict", StringComparison.Ordinal) ? "" : "@" + role);
    }

    public static bool HasSpec(string key, char role) { return specs.ContainsKey(SpecKey(key, role)); }

    static string SpecKey(string key, char role)
    {
        char r = role == 'D' && !key.StartsWith("Dict", StringComparison.Ordinal) ? 'S' : role;
        return key + "|" + r;
    }

    public static string KeyTitle(string key, bool ru)
    {
        if (key.StartsWith("Party", StringComparison.Ordinal)) return HeroFlavors.PartyName(int.Parse(key.Substring(5)), ru);
        if (key.StartsWith("Race", StringComparison.Ordinal)) return HeroFlavors.FactionName(key.Substring(4), ru);
        if (key.StartsWith("Dict", StringComparison.Ordinal)) return HeroFlavors.FactionName(key.Substring(4), ru);
        string c = key.Substring(5);
        switch (c)
        {
            case "Admiral": return ru ? "Адмирал" : "Admiral";
            case "Adventurer": return ru ? "Авантюрист" : "Adventurer";
            case "Administrator": return ru ? "Администратор" : "Administrator";
            default: return ru ? "Корпорат" : "Corporate";
        }
    }

    public static string UltimateTitle(string key, bool ru)
    {
        string t;
        if (!ultimateTitles.TryGetValue(key, out t)) return ru ? "Вершина" : "Pinnacle";
        int cut = t.IndexOf('|');
        return ru ? t.Substring(0, cut) : t.Substring(cut + 1);
    }

    public static List<string> AllKeys(char role)
    {
        var keys = new List<string>();
        if (role == 'D')
        {
            for (int i = 0; i < PartyKeys.Length; i++) keys.Add(PartyKeys[i]);
            for (int i = 0; i < RaceKeys.Length; i++) keys.Add("Race" + RaceKeys[i]);
            for (int i = 0; i < DictKeys.Length; i++) keys.Add("Dict" + DictKeys[i]);
            return keys;
        }
        for (int i = 0; i < PartyKeys.Length; i++) keys.Add(PartyKeys[i]);
        for (int i = 0; i < RaceKeys.Length; i++) keys.Add("Race" + RaceKeys[i]);
        for (int i = 0; i < ClassKeys.Length; i++) if (HasSpec("Class" + ClassKeys[i], role)) keys.Add("Class" + ClassKeys[i]);
        return keys;
    }

    // ---------------------------------------------------------------- building

    static HeroKind KindOfRole(char role) { return role == 'A' ? HeroKind.Admiral : role == 'G' ? HeroKind.Governor : HeroKind.Senator; }

    /// <summary>The tree of a key for a role (A, G, S, D); null when there is none.</summary>
    public static DesignTree Build(string key, char role)
    {
        string line;
        if (!specs.TryGetValue(SpecKey(key, role), out line)) return null;
        var tree = new DesignTree();
        tree.Key = key; tree.Role = role; tree.TreeName = TreeName(key, role);
        string[] stagesText = line.Split(';');
        var byTier = new List<string[]>();
        string[] ult = null;
        for (int i = 0; i < stagesText.Length; i++)
        {
            int colon = stagesText[i].IndexOf(':');
            string head = stagesText[i].Substring(0, colon);
            string[] names = stagesText[i].Substring(colon + 1).Split(',');
            if (head == "U") ult = names; else byTier.Add(names);
        }
        HeroChannel ch = ChannelOfRole(role);
        HeroKind kind = KindOfRole(role);
        bool dict = role == 'D';
        int index = 0;
        for (int t = 0; t < 4; t++)
        {
            var stage = new DesignStage();
            stage.RequiredLevel = HeroFlavors.StageThreshold(t, kind, dict);
            string[] names = t < 3 ? (t < byTier.Count ? byTier[t] : new string[0]) : ult;
            if (t == 3)
            {
                var skill = new DesignSkill();
                skill.Tier = 4;
                skill.Concepts = new Concept[names.Length];
                var values = new int[1][];
                values[0] = new int[names.Length];
                for (int c = 0; c < names.Length; c++)
                {
                    Concept cc = Get(ch, names[c]);
                    if (cc == null) throw new InvalidOperationException("unknown concept " + names[c] + " in " + key + "|" + role);
                    skill.Concepts[c] = cc;
                    values[0][c] = cc.UltValue(role);
                }
                skill.Values = values;
                index++;
                skill.Name = SkillName(key, index, role);
                skill.TitleRu = UltimateTitle(key, true); skill.TitleEn = UltimateTitle(key, false);
                stage.Skills.Add(skill);
            }
            else
            {
                for (int n = 0; n < names.Length; n++)
                {
                    Concept cc = Get(ch, names[n]);
                    if (cc == null) throw new InvalidOperationException("unknown concept " + names[n] + " in " + key + "|" + role);
                    var skill = new DesignSkill();
                    skill.Tier = t + 1;
                    skill.Concepts = new[] { cc };
                    int levels = cc.LevelCount(role);
                    skill.Values = new int[levels][];
                    for (int l = 0; l < levels; l++) skill.Values[l] = new[] { cc.Value(role, t + 1, l + 1) };
                    index++;
                    skill.Name = SkillName(key, index, role);
                    skill.TitleRu = cc.Ru; skill.TitleEn = cc.En;
                    stage.Skills.Add(skill);
                }
            }
            tree.Stages.Add(stage);
        }
        return tree;
    }

    public static string SkillName(string key, int index, char role)
    {
        return "HR" + key + "_" + index + (key.StartsWith("Dict", StringComparison.Ordinal) ? "" : "@" + role);
    }

    public static float ModValue(DesignMod m, int value) { return (float)Math.Round(value * m.Scale, 4); }

    // ---------------------------------------------------------------- texts

    /// <summary>Effects of one level of a skill: "Armor +2%, Repair +10".</summary>
    public static string LevelText(DesignSkill s, int level, bool ru)
    {
        var sb = new StringBuilder();
        for (int c = 0; c < s.Concepts.Length; c++)
        {
            if (c > 0) sb.Append("; ");
            Concept cc = s.Concepts[c];
            sb.Append(cc.Name(ru)).Append(' ').Append(cc.Show(s.Values[level][c], ru));
        }
        return sb.ToString();
    }

    /// <summary>Values of a one-effect skill by level: "+2% / +4% / +6%".</summary>
    public static string LevelsLine(DesignSkill s, bool ru)
    {
        var sb = new StringBuilder();
        for (int l = 0; l < s.Levels; l++)
        {
            if (l > 0) sb.Append(" / ");
            sb.Append(s.Concepts[0].Show(s.Values[l][0], ru));
        }
        return sb.ToString();
    }

    public static string Describe(DesignSkill s, bool ru)
    {
        var sb = new StringBuilder();
        if (s.Tier == 4)
        {
            sb.Append(ru ? "Вершина дерева. " : "Pinnacle of the tree. ");
            sb.Append(ru ? "Эффекты: " : "Effects: ");
            sb.Append(LevelText(s, 0, ru)).Append('.');
            for (int c = 0; c < s.Concepts.Length; c++)
                if (s.Concepts[c].Special != null) sb.Append(' ').Append(s.Concepts[c].Note(ru));
            return sb.ToString();
        }
        sb.Append(s.Concepts[0].Note(ru));
        sb.Append(ru ? " По уровням: " : " By level: ").Append(LevelsLine(s, ru)).Append('.');
        return sb.ToString();
    }

    // ---------------------------------------------------------------- votes of the Galactic Council

    static Dictionary<string, int[]> voteTable;

    static void BuildVoteTable()
    {
        var table = new Dictionary<string, int[]>();
        char[] roles = { 'S', 'D' };
        for (int r = 0; r < roles.Length; r++)
        {
            List<string> keys = AllKeys(roles[r]);
            for (int k = 0; k < keys.Count; k++)
            {
                DesignTree tree = Build(keys[k], roles[r]);
                if (tree == null) continue;
                for (int s = 0; s < tree.Stages.Count; s++)
                    for (int i = 0; i < tree.Stages[s].Skills.Count; i++)
                    {
                        DesignSkill sk = tree.Stages[s].Skills[i];
                        for (int c = 0; c < sk.Concepts.Length; c++)
                        {
                            if (sk.Concepts[c].Key != "S.Votes") continue;
                            var v = new int[sk.Levels];
                            for (int l = 0; l < v.Length; l++) v[l] = sk.Values[l][c];
                            table[sk.Name] = v;
                        }
                    }
            }
        }
        voteTable = table;
    }

    /// <summary>Council votes by level of a skill (null: it gives none). Works from the name, so saved games find it.</summary>
    public static int[] VotesByLevel(string skillName)
    {
        if (skillName == null || !skillName.StartsWith("HR", StringComparison.Ordinal)) return null;
        if (voteTable == null) BuildVoteTable();
        int[] v;
        return voteTable.TryGetValue(skillName, out v) ? v : null;
    }

    public static int VotesOfSkill(string skillName, int level)
    {
        int[] v = VotesByLevel(skillName);
        if (v == null || v.Length == 0 || level <= 0) return 0;
        return v[Math.Max(0, Math.Min(v.Length - 1, level - 1))];
    }

    // ---------------------------------------------------------------- lookups by skill name (hooks)

    static Dictionary<string, DesignSkill> skillIndex;

    /// <summary>The written skill with this database name (null for vanilla skills).</summary>
    public static DesignSkill FindSkill(string skillName)
    {
        if (skillName == null || !skillName.StartsWith("HR", StringComparison.Ordinal)) return null;
        if (skillIndex == null)
        {
            var index = new Dictionary<string, DesignSkill>();
            foreach (char role in new[] { 'A', 'G', 'S', 'D' })
            {
                List<string> keys = AllKeys(role);
                for (int k = 0; k < keys.Count; k++)
                {
                    DesignTree tree = Build(keys[k], role);
                    if (tree == null) continue;
                    for (int s = 0; s < tree.Stages.Count; s++)
                        for (int i = 0; i < tree.Stages[s].Skills.Count; i++) index[tree.Stages[s].Skills[i].Name] = tree.Stages[s].Skills[i];
                }
            }
            skillIndex = index;
        }
        DesignSkill result;
        return skillIndex.TryGetValue(skillName, out result) ? result : null;
    }

    /// <summary>Value of a concept (e.g. "A.LootSci") in a skill at a level (1-based); 0 when the skill has none.</summary>
    public static int AmountOf(DesignSkill skill, int level, string conceptKey)
    {
        if (skill == null || level <= 0) return 0;
        int l = Math.Min(skill.Levels, level) - 1;
        for (int c = 0; c < skill.Concepts.Length; c++)
            if (skill.Concepts[c].Key == conceptKey) return skill.Values[l][c];
        return 0;
    }

    /// <summary>Id of the hook of an ultimate (Prisoner, Euphoria) by skill name; null for ordinary skills.</summary>
    public static string SpecialOf(string skillName)
    {
        if (skillName == null || !skillName.StartsWith("HRRace", StringComparison.Ordinal) || !skillName.EndsWith("@A", StringComparison.Ordinal)) return null;
        if (skillName.StartsWith("HRRaceCravers_", StringComparison.Ordinal)) return IsUltimateIndex(skillName) ? "Prisoner" : null;
        if (skillName.StartsWith("HRRaceTerrans_", StringComparison.Ordinal)) return IsUltimateIndex(skillName) ? "Euphoria" : null;
        return null;
    }

    static bool IsUltimateIndex(string name)
    {
        int us = name.IndexOf('_'), at = name.IndexOf('@');
        int n;
        if (us < 0 || at < us || !int.TryParse(name.Substring(us + 1, at - us - 1), out n)) return false;
        string key = name.Substring(2, us - 2);
        DesignTree t = Build(key, 'A');
        if (t == null) return false;
        DesignSkill last = t.Stages[3].Skills[0];
        return last.Name == name;
    }
}
