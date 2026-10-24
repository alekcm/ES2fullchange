// Hero rework: the faction dictator hero and the passive experience of senators.
//  * a player who has a Dictatorship (or starts with one, or has the authoritarian rule of the Cravers) is given a faction dictator:
//    a hidden senator hero of the faction whose politics are the politics of the leader senator, so he represents the leader in the senate;
//  * when the dictatorship ends (or the leader changes party) the hero is removed again;
//  * the definitions are generated at run time for every (faction, politics) pair, registered in the hero database (and the GUI
//    database) before a save is read, and copied from a vanilla senator (class Corporate) hero of the faction;
//  * senator heroes (and the dictator) gain experience with time: a level every ~10 turns (6 for the dictator).
using System;
using System.Collections.Generic;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Gui;
using Amplitude.Unity.Xml;

public static partial class HeroRework
{
    const int DictatorStartLevel = 1;
    const int DictatorOrderRetryTurns = 3;

    static readonly Dictionary<int, int> dictatorOrderTurn = new Dictionary<int, int>();
    static string lastDictatorDefinition;
    static bool registeringDictatorDefinitions;
    static int passiveXpTurn = int.MinValue;

    public static bool IsDictatorDefinition(HeroDefinition def)
    {
        try { return def != null && HeroKinds.IsDictatorDefinitionName(def.Name.ToString()); }
        catch (Exception) { return false; }
    }

    // ---------------------------------------------------------------- definitions

    static string ClassNameOf(HeroDefinition d)
    {
        try { return d.Class != null ? d.Class.Name.ToString() : null; }
        catch (Exception) { return null; }
    }

    static bool IsCorporateDefinition(HeroDefinition d)
    {
        string c = ClassNameOf(d);
        return c != null && c.IndexOf("Corporate", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    static string AffinityKeyOf(HeroDefinition d)
    {
        try { return HeroFlavors.FactionKey(d.Affinity.ToString()); }
        catch (Exception) { return "Other"; }
    }

    static bool IsEarlier(HeroDefinition a, HeroDefinition b)
    {
        return b == null || string.CompareOrdinal(a.Name.ToString(), b.Name.ToString()) < 0;
    }

    static void AddUnique(List<string> list, string value)
    {
        for (int i = 0; i < list.Count; i++) if (list[i] == value) return;
        list.Add(value);
    }

    /// <summary>Hero.get_SkillTrees of a template with the race tree of the faction hero: universal and class trees stay those of the template.</summary>
    static XmlNamedReference[] TreeReferences(HeroDefinition corporate, HeroDefinition factionHero)
    {
        XmlNamedReference[] own = corporate.SkillTreesReferences;
        if (own == null) return null;
        if (factionHero == null || ReferenceEquals(factionHero, corporate) || factionHero.SkillTreesReferences == null) return own;
        string ownClass = TreeNameOfClass(ClassNameOf(corporate));
        string heroClass = TreeNameOfClass(ClassNameOf(factionHero));
        XmlNamedReference race = null;
        XmlNamedReference[] theirs = factionHero.SkillTreesReferences;
        for (int i = 0; i < theirs.Length; i++)
        {
            string n = theirs[i] == null ? null : theirs[i].Name.ToString();
            if (n != null && n != GenericTreeName && n != heroClass) { race = theirs[i]; break; }
        }
        if (race == null) return own;
        var result = new List<XmlNamedReference>();
        bool replaced = false;
        for (int i = 0; i < own.Length; i++)
        {
            string n = own[i] == null ? null : own[i].Name.ToString();
            if (n != null && n != GenericTreeName && n != ownClass && !replaced) { result.Add(race); replaced = true; }
            else result.Add(own[i]);
        }
        if (!replaced) result.Add(race);
        return result.ToArray();
    }

    static string TreeNameOfClass(string className)
    {
        if (className == null) return null;
        return "HeroSkillTree" + (className.StartsWith("HeroClass") ? className.Substring("HeroClass".Length) : className);
    }

    /// <summary>Registers the dictator hero definitions (and their GUI elements) once; cheap when they are already there. Called before a save is read.</summary>
    public static void EnsureDictatorDefinitions()
    {
        if (!Enabled || registeringDictatorDefinitions) return;
        registeringDictatorDefinitions = true;
        try
        {
            IDatabase<HeroDefinition> db = Databases.GetDatabase<HeroDefinition>(false);
            if (db == null) return;
            if (lastDictatorDefinition != null && db.ContainsKey(new StaticString(lastDictatorDefinition))) return;
            HeroDefinition[] all = db.GetValues();
            if (all == null || all.Length == 0) return;

            var keys = new List<string>();
            var politics = new List<string>();
            var corporateOf = new Dictionary<string, HeroDefinition>();
            var anyOf = new Dictionary<string, HeroDefinition>();
            HeroDefinition anyCorporate = null;
            for (int i = 0; i < all.Length; i++)
            {
                HeroDefinition d = all[i];
                if (d == null || d.Hidden || IsDictatorDefinition(d)) continue;
                string key = AffinityKeyOf(d);
                AddUnique(keys, key);
                string p = null;
                try { p = d.Politics.ToString(); } catch (Exception) { }
                if (!string.IsNullOrEmpty(p)) AddUnique(politics, p);
                HeroDefinition cur;
                if (!anyOf.TryGetValue(key, out cur) || IsEarlier(d, cur)) anyOf[key] = d;
                if (IsCorporateDefinition(d))
                {
                    if (!corporateOf.TryGetValue(key, out cur) || IsEarlier(d, cur)) corporateOf[key] = d;
                    if (anyCorporate == null || IsEarlier(d, anyCorporate)) anyCorporate = d;
                }
            }
            if (anyCorporate == null || politics.Count == 0) return;
            AddUnique(keys, "Other");

            int made = 0;
            for (int k = 0; k < keys.Count; k++)
            {
                string key = keys[k];
                HeroDefinition corporate, factionHero;
                if (!corporateOf.TryGetValue(key, out corporate)) corporate = anyCorporate;
                if (!anyOf.TryGetValue(key, out factionHero)) factionHero = corporate;
                for (int p = 0; p < politics.Count; p++)
                {
                    try
                    {
                        string name = HeroKinds.DictatorDefinitionName(key, politics[p]);
                        if (!db.ContainsKey(new StaticString(name))) RegisterDictatorDefinition(db, name, key, politics[p], corporate, factionHero);
                        lastDictatorDefinition = name;
                        made++;
                    }
                    catch (Exception e) { Fail("dictator definition " + key + "/" + politics[p], e); }
                }
            }
            try { UnityEngine.Debug.Log("[HeroRework] dictator hero definitions: " + made); } catch (Exception) { }
        }
        catch (Exception e) { Fail("EnsureDictatorDefinitions", e); }
        finally { registeringDictatorDefinitions = false; }
    }

    static void RegisterDictatorDefinition(IDatabase<HeroDefinition> db, string name, string faction, string politics, HeroDefinition corporate, HeroDefinition factionHero)
    {
        var def = new HeroDefinition();
        CustomHackingHelper.SetPrivatePropOrField(def, "Name", new StaticString(name));
        CustomHackingHelper.SetPrivatePropOrField(def, "Hidden", true);
        CustomHackingHelper.SetPrivatePropOrField(def, "IgnoreInstanceNumber", true);
        CustomHackingHelper.SetPrivatePropOrField(def, "AffinityReference", factionHero.AffinityReference);
        CustomHackingHelper.SetPrivatePropOrField(def, "ClassReference", corporate.ClassReference);
        CustomHackingHelper.SetPrivatePropOrField(def, "PoliticsReference", new XmlNamedReference(politics));
        CustomHackingHelper.SetPrivatePropOrField(def, "ShipDesignReference", corporate.ShipDesignReference);
        CustomHackingHelper.SetPrivatePropOrField(def, "LevelUpRuleReference", corporate.LevelUpRuleReference);
        CustomHackingHelper.SetPrivatePropOrField(def, "SkillTreesReferences", TreeReferences(corporate, factionHero));
        db.Touch(def);

        IDatabase<GuiElement> gdb = Gui.GuiElementsDatabase;
        if (gdb == null || gdb.ContainsKey(new StaticString(name))) return;
        GuiElement template = null;
        gdb.TryGetValue(corporate.Name, out template);
        GuiElement element = template != null ? (GuiElement)template.Clone() : new HeroGuiElement();
        string key = "HRX_" + name;
        string factionName = HeroFlavors.FactionName(faction, Ru());
        CustomHackingHelper.RegisterLoc(key + "_T", Ru() ? "Диктатор: " + factionName : factionName + " Dictator");
        CustomHackingHelper.RegisterLoc(key + "_D", Ru()
            ? "Лидер фракции при диктатуре. Представляет правящую партию в сенате, не ограничен в уровне и быстрее набирает опыт."
            : "Faction leader under a dictatorship. Represents the ruling party in the senate, has no level cap and gains experience faster.");
        element.XmlSerializableName = name;
        element.Title = "%" + key + "_T";
        element.Description = "%" + key + "_D";
        gdb.Add(element);
    }

    // ---------------------------------------------------------------- faction of an empire

    static string FactionKeyOf(Empire empire)
    {
        string result = "Other";
        try
        {
            Faction f = empire.Faction;
            if (f == null) return result;
            string[] candidates = new string[3];
            try { candidates[0] = f.Name.ToString(); } catch (Exception) { }
            try { candidates[1] = f.GetAffinityMappingName(); } catch (Exception) { }
            try { candidates[2] = f.GetAffinityGameplayName(); } catch (Exception) { }
            for (int i = 0; i < candidates.Length; i++)
            {
                string key = HeroFlavors.FactionKey(candidates[i]);
                if (key != "Other") return key;
            }
        }
        catch (Exception) { }
        return result;
    }

    static HeroDefinition FindDictatorDefinition(Empire empire, string politics)
    {
        IDatabase<HeroDefinition> db = Databases.GetDatabase<HeroDefinition>(false);
        if (db == null) return null;
        string[] keys = { FactionKeyOf(empire), "Other" };
        for (int i = 0; i < keys.Length; i++)
        {
            HeroDefinition d;
            if (db.TryGetValue(new StaticString(HeroKinds.DictatorDefinitionName(keys[i], politics)), out d)) return d;
        }
        HeroDefinition[] all = db.GetValues();
        if (all != null)
            for (int i = 0; i < all.Length; i++)
                if (IsDictatorDefinition(all[i]) && all[i].Politics.ToString() == politics) return all[i];
        return null;
    }

    // ---------------------------------------------------------------- the hero follows the dictatorship

    /// <summary>The dictator hero of an empire (null when it has none).</summary>
    static Hero FindDictatorHero(DepartmentOfEducation education)
    {
        var heroes = education.ActiveHeroes;
        for (int i = 0; i < heroes.Count; i++)
            if (heroes[i] != null && IsDictatorDefinition(heroes[i].Definition)) return heroes[i];
        return null;
    }

    /// <summary>Gives the human empire a dictator hero while it has a dictatorship (rec != null) and takes it away when it ends.</summary>
    static void SyncDictatorHero(MajorEmpire empire, DictatorRecord rec)
    {
        try
        {
            if (empire == null || empire.IsControlledByAI) return;
            var education = empire.GetAgency<DepartmentOfEducation>();
            if (education == null) return;
            string want = rec == null ? null : rec.Politics;
            Hero keep = null;
            var remove = new List<Hero>();
            var heroes = education.ActiveHeroes;
            for (int i = 0; i < heroes.Count; i++)
            {
                Hero h = heroes[i];
                if (h == null || !IsDictatorDefinition(h.Definition)) continue;
                if (want != null && keep == null && h.Definition.Politics.ToString() == want) keep = h;
                else remove.Add(h);
            }
            for (int i = 0; i < remove.Count; i++) RemoveDictatorHero(empire, education, remove[i]);
            if (want == null) { dictatorOrderTurn.Remove(empire.Index); return; }
            if (keep != null) return;
            RecruitDictatorHero(empire, want);
        }
        catch (Exception e) { Fail("SyncDictatorHero", e); }
    }

    static void RecruitDictatorHero(MajorEmpire empire, string politics)
    {
        int last;
        if (dictatorOrderTurn.TryGetValue(empire.Index, out last) && lastTurn >= last && lastTurn - last < DictatorOrderRetryTurns) return;
        var pc = empire.PlayerControllers;
        if (pc == null || pc.Client == null) return;   // only the client of the player posts it (once)
        EnsureDictatorDefinitions();
        HeroDefinition def = FindDictatorDefinition(empire, politics);
        if (def == null) { Fail("RecruitDictatorHero", new Exception("no dictator definition for " + politics)); return; }
        var order = new OrderRecruitHero(empire.Index, def);
        order.HeroLevel = DictatorStartLevel;
        order.GaugeIndex = -1;
        pc.Client.PostOrder(order);
        dictatorOrderTurn[empire.Index] = lastTurn;
        try
        {
            CouncilUi.Toast(CouncilGame.Tr("Диктатура: фракция получила героя-диктатора («" + def.LocalizedName + "»).",
                                           "Dictatorship: the faction was given a dictator hero (\"" + def.LocalizedName + "\")."));
        }
        catch (Exception) { }
    }

    static void RemoveDictatorHero(MajorEmpire empire, DepartmentOfEducation education, Hero hero)
    {
        try
        {
            Senate senate = SenateOf(empire);
            var represented = new List<Senator>();
            if (senate != null && senate.Senators != null)
                foreach (Senator s in senate.Senators)
                    if (s != null && ReferenceEquals(s.RepresentativeHero, hero)) represented.Add(s);
            for (int i = 0; i < represented.Count; i++)
            {
                represented[i].RemoveAllHeroDescriptors();
                represented[i].RepresentativeHero = null;
            }
            ulong id = (ulong)hero.GUID;
            string name = hero.LocalizedName;
            education.RemoveHero(hero);
            ultimates.Remove(id); appliedSteps.Remove(id); dictatorState.Remove(id); treeCache.Remove(id); dictatorHeroes.Remove(id);
            for (int i = 0; i < represented.Count; i++) represented[i].UpdateRepresentativeHero();
            var local = CouncilGame.GetMajor(0);
            if (local != null && local.Index == empire.Index)
                CouncilUi.Toast(CouncilGame.Tr("Диктатура закончилась: герой «" + name + "» покинул фракцию.", "The dictatorship is over: the hero \"" + name + "\" has left the faction."));
        }
        catch (Exception e) { Fail("RemoveDictatorHero", e); }
    }

    // ---------------------------------------------------------------- passive experience

    /// <summary>One turn of experience for a senator hero: the level cap and the level-up rule decide whether it counts.</summary>
    static void GrantPassiveExperience(Hero hero)
    {
        try
        {
            BeforeUpdateLevel(hero);
            LevelUpRuleDefinition rule = hero.LevelUpRule;
            if (rule == null || !rule.CanLevelUp(hero.Level)) return;
            float add = HeroKinds.PassiveExperience(hero.NextLevelRequestedExperience, IsDictator(hero));
            if (add <= 0f) return;
            hero.SetExperience(hero.Experience + add);
        }
        catch (Exception e) { Fail("GrantPassiveExperience", e); }
    }

    /// <summary>Hero.get_AvailableSkillPoints return hook: a hero who took his ultimate has no skills left to learn, so the points are not offered.</summary>
    public static int AdjustAvailablePoints(int points, Hero hero)
    {
        try { return points > 0 && IsFixed(hero) ? HeroKinds.VisibleSkillPoints(points, true) : points; }
        catch (Exception) { return points; }
    }
}
