// Hero rework: the skill content (redesign).
//  * every hero has three trees: the tree of his political party (instead of the "Universal" one), the tree of his race and the tree of
//    his class; all of them are written by hand for the role of the hero (admiral, governor, senator, dictator): see HeroTreeDesign.cs
//    and HeroTreeSpecs.cs. A dictator also has a tree of his faction and no class tree;
//  * the vanilla skills of these trees are not used any more; the personal skills of every hero stay as they are;
//  * everything is registered at run time in the databases of the game (skill definitions and their GUI elements), so saves find the
//    skills by name.
using System;
using System.Collections.Generic;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Gui;
using Amplitude.Unity.Simulation;
using Amplitude.Unity.Simulation.SimulationModifierDescriptors;
using Amplitude.Unity.Xml;

public static partial class HeroRework
{
    const string ContentSentinel = "HRContentEnd";
    const string GenericTreeName = "HeroSkillTreeGeneric";

    static readonly Dictionary<string, HeroSkillDefinition> contentSkills = new Dictionary<string, HeroSkillDefinition>();
    static readonly Dictionary<string, HeroSkillTreeDefinition> authoredTrees = new Dictionary<string, HeroSkillTreeDefinition>();
    static readonly Dictionary<int, string> iconSkillOfFlavor = new Dictionary<int, string>();
    static readonly Dictionary<string, string> masteryNames = new Dictionary<string, string>();
    static SimulationDescriptor completeDescriptor;
    static string anySkillName;
    static int contentGeneration;
    static bool inContent;

    static readonly HeroKind[] roleKinds = { HeroKind.Admiral, HeroKind.Governor, HeroKind.Senator, HeroKind.Senator };
    static readonly bool[] roleDictator = { false, false, false, true };
    static readonly string[] roleCodes = { "A", "G", "S", "D" };

    static bool Ru() { try { return CouncilGame.IsRussian(); } catch (Exception) { return false; } }

    // ---------------------------------------------------------------- registration

    /// <summary>Generates and registers every skill of the rework once. Cheap when everything is already there.</summary>
    public static void EnsureContent()
    {
        if (inContent || !Enabled) return;
        inContent = true;
        try
        {
            IDatabase<HeroSkillDefinition> db = Databases.GetDatabase<HeroSkillDefinition>(false);
            if (db == null || db.ContainsKey(new StaticString(ContentSentinel))) return;
            IDatabase<HeroSkillTreeDefinition> treeDb = Databases.GetDatabase<HeroSkillTreeDefinition>(false);
            if (treeDb == null) return;
            HeroSkillTreeDefinition[] vanilla = treeDb.GetValues();
            if (vanilla == null || vanilla.Length == 0) return;

            ResetContent();
            var bases = new List<HeroSkillDefinition>();
            var seen = new HashSet<string>();
            for (int i = 0; i < vanilla.Length; i++)
            {
                HeroSkillTreeDefinition tree = vanilla[i];
                if (tree == null || tree.Stages == null) continue;
                for (int s = 0; s < tree.Stages.Length; s++)
                {
                    HeroSkillTreeStage stage = tree.Stages[s];
                    if (stage == null || stage.Skills == null) continue;
                    for (int k = 0; k < stage.Skills.Length; k++) CollectSlotSkills(stage.Skills[k], bases, seen);
                }
            }
            for (int i = 0; i < bases.Count; i++) ScanBase(bases[i]);

            // the written trees: every key of every role
            for (int r = 0; r < roleCodes.Length; r++)
            {
                List<string> keys = HeroDesign.AllKeys(roleCodes[r][0]);
                for (int k = 0; k < keys.Count; k++)
                {
                    try
                    {
                        DesignTree tree = HeroDesign.Build(keys[k], roleCodes[r][0]);
                        if (tree != null) RegisterDesigned(db, tree);
                    }
                    catch (Exception e) { Fail("tree " + keys[k] + "|" + roleCodes[r], e); }
                }
            }

            var end = new HeroSkillDefinition();
            end.XmlSerializableName = ContentSentinel;
            end.SkillLevels = new HeroSkillDefinition.HeroSkillLevelDefinition[0];
            db.Add(end);
            contentGeneration++;
            try { UnityEngine.Debug.Log("[HeroRework] registered " + contentSkills.Count + " hero skills"); } catch (Exception) { }
        }
        catch (Exception e) { Fail("EnsureContent", e); }
        finally { inContent = false; }
    }

    static void ResetContent()
    {
        contentSkills.Clear(); authoredTrees.Clear(); iconSkillOfFlavor.Clear(); masteryNames.Clear();
        completeDescriptor = null; anySkillName = null;
        treeCache.Clear(); ultimateSets.Clear();
    }

    static void CollectSlotSkills(HeroSkillTreeSkill slot, List<HeroSkillDefinition> list, HashSet<string> seen)
    {
        if (slot == null) return;
        HeroSkillDefinition d = slot.SkillDefinition;
        if (d != null && seen.Add(d.Name.ToString())) list.Add(d);
        HeroSkillDefinition[] req = slot.RequiredSkills;
        if (req != null)
            for (int i = 0; i < req.Length; i++)
                if (req[i] != null && seen.Add(req[i].Name.ToString())) list.Add(req[i]);
        if (slot.Alternatives != null)
            for (int i = 0; i < slot.Alternatives.Length; i++) CollectSlotSkills(slot.Alternatives[i], list, seen);
    }

    // ---------------------------------------------------------------- scanning of the vanilla skills

    static SimulationDescriptor[][] ShipArrays(HeroSkillDefinition.HeroSkillLevelDefinition l)
    {
        return new[] { l.ShipDescriptors, l.OnOrbitDescriptors, l.OnOwnedOrbitDescriptors, l.OnHostileOrbitDescriptors, l.OnNeutralOrbitDescriptors, l.OnFriendlyOrbitDescriptors };
    }

    static void ScanBase(HeroSkillDefinition skill)
    {
        if (anySkillName == null) anySkillName = skill.Name.ToString();
        HeroSkillDefinition.HeroSkillLevelDefinition[] levels = skill.SkillLevels;
        if (levels == null || levels.Length == 0) return;
        HeroSkillDefinition.HeroSkillLevelDefinition last = levels[levels.Length - 1];
        if (last == null) return;
        if (completeDescriptor == null && last.HeroDescriptors != null)
            for (int i = 0; i < last.HeroDescriptors.Length; i++)
                if (last.HeroDescriptors[i] != null && last.HeroDescriptors[i].Name.ToString() == "HeroSkillComplete") completeDescriptor = last.HeroDescriptors[i];
        for (int l = 0; l < levels.Length; l++)
        {
            if (levels[l] == null || levels[l].MasteryLevels == null) continue;
            foreach (var m in levels[l].MasteryLevels)
            {
                string n = m.MasteryName.ToString();
                string[] parts = { "Labour", "Wit", "Command", "Curiosity" };
                for (int p = 0; p < parts.Length; p++)
                    if (n.IndexOf(parts[p], StringComparison.OrdinalIgnoreCase) >= 0 && !masteryNames.ContainsKey(parts[p])) masteryNames[parts[p]] = n;
            }
        }
        var acc = new Dictionary<int, float>();
        Accumulate(last.ShipDescriptors, HeroChannel.Ship, acc);
        Accumulate(last.SystemDescriptors, HeroChannel.System, acc);
        Accumulate(last.SenateDescriptors, HeroChannel.Senate, acc);
        List<Flavor> top = TopFlavors(acc, 1);
        if (top.Count > 0 && !iconSkillOfFlavor.ContainsKey((int)top[0])) iconSkillOfFlavor[(int)top[0]] = skill.Name.ToString();
    }

    /// <summary>Adds up flavors of the plain numeric modifiers of the descriptors.</summary>
    static void Accumulate(SimulationDescriptor[] descriptors, HeroChannel channel, Dictionary<int, float> acc)
    {
        if (descriptors == null) return;
        for (int d = 0; d < descriptors.Length; d++)
        {
            SimulationDescriptor desc = descriptors[d];
            if (desc == null || desc.SimulationModifierDescriptors == null) continue;
            SimulationModifierDescriptor[] mods = desc.SimulationModifierDescriptors;
            for (int m = 0; m < mods.Length; m++)
            {
                var single = mods[m] as SingleSimulationModifierDescriptor;
                if (single == null || !single.IsValueFloat) continue;
                Flavor flavor; float magnitude;
                if (!HeroFlavors.Classify(single.TargetPropertyName.ToString(), (int)single.Operation, single.ValueAsFloat, channel, out flavor, out magnitude)) continue;
                float old;
                acc.TryGetValue((int)flavor, out old);
                acc[(int)flavor] = old + magnitude;
            }
        }
    }

    static List<Flavor> TopFlavors(Dictionary<int, float> acc, int max)
    {
        var result = new List<Flavor>();
        var used = new HashSet<int>();
        float best0 = -1f;
        for (int round = 0; round < max; round++)
        {
            int bestKey = -1; float best = 0f;
            foreach (var kv in acc)
                if (!used.Contains(kv.Key) && kv.Value > best) { best = kv.Value; bestKey = kv.Key; }
            if (bestKey < 0) break;
            if (best0 < 0f) best0 = best;
            if (best < best0 * 0.4f) break;
            used.Add(bestKey);
            result.Add((Flavor)bestKey);
        }
        return result;
    }

    // ---------------------------------------------------------------- generated descriptors

    static string DescriptorType(HeroChannel channel)
    {
        return channel == HeroChannel.Ship ? "HeroSkillShip" : channel == HeroChannel.System ? "HeroSkillSystem" : "HeroSkillSenator";
    }

    static HeroSkillDefinition.MasteryLevel[] Masteries(string part)
    {
        string name;
        if (!masteryNames.TryGetValue(part, out name)) return new HeroSkillDefinition.MasteryLevel[0];
        var m = new HeroSkillDefinition.MasteryLevel();
        m.MasteryName = new StaticString(name);
        m.Levels = 1;
        return new[] { m };
    }

    static string MasteryPartOf(Flavor f)
    {
        string full = HeroFlavors.MasteryOfFlavor(f);
        string[] parts = { "Labour", "Wit", "Command", "Curiosity" };
        for (int i = 0; i < parts.Length; i++) if (full.IndexOf(parts[i], StringComparison.Ordinal) >= 0) return parts[i];
        return "Curiosity";
    }

    static HeroSkillDefinition.HeroSkillLevelDefinition EmptyLevel()
    {
        var level = new HeroSkillDefinition.HeroSkillLevelDefinition();
        level.heroDescriptors = new SimulationDescriptor[0];
        level.senateDescriptors = new SimulationDescriptor[0];
        level.shipDescriptors = new SimulationDescriptor[0];
        level.systemDescriptors = new SimulationDescriptor[0];
        level.onOrbitDescriptors = new SimulationDescriptor[0];
        level.onOwnedOrbitDescriptors = new SimulationDescriptor[0];
        level.onHostileOrbitDescriptors = new SimulationDescriptor[0];
        level.onNeutralOrbitDescriptors = new SimulationDescriptor[0];
        level.onFriendlyOrbitDescriptors = new SimulationDescriptor[0];
        level.encounterPlayDefinitions = new EncounterPlayDefinition[0];
        level.MasteryLevels = new HeroSkillDefinition.MasteryLevel[0];
        level.Name = "";
        level.PostDeserialization();
        return level;
    }

    static void RegisterGui(string name, string templateName, string title, string description)
    {
        IDatabase<GuiElement> gdb = Gui.GuiElementsDatabase;
        if (gdb == null || gdb.ContainsKey(new StaticString(name))) return;
        GuiElement template = null;
        if (templateName != null) gdb.TryGetValue(new StaticString(templateName), out template);
        GuiElement element = template != null ? (GuiElement)template.Clone() : new GuiElement();
        string key = "HRX_" + name.Replace('@', '_');
        CustomHackingHelper.RegisterLoc(key + "_T", title);
        CustomHackingHelper.RegisterLoc(key + "_D", description);
        element.XmlSerializableName = name;
        element.Title = "%" + key + "_T";
        element.Description = "%" + key + "_D";
        gdb.Add(element);
    }

    // ---------------------------------------------------------------- the written trees

    static string KindWord(string key, bool ru)
    {
        if (key.StartsWith("Party", StringComparison.Ordinal)) return ru ? "Партия" : "Party";
        if (key.StartsWith("Race", StringComparison.Ordinal)) return ru ? "Раса" : "Race";
        if (key.StartsWith("Class", StringComparison.Ordinal)) return ru ? "Класс" : "Class";
        return ru ? "Диктатура" : "Dictatorship";
    }

    static string RoleName(char role, bool ru)
    {
        if (role == 'D') return ru ? "Диктатор" : "Dictator";
        return HeroKinds.Name(role == 'A' ? HeroKind.Admiral : role == 'G' ? HeroKind.Governor : HeroKind.Senator, ru);
    }

    static void RegisterDesigned(IDatabase<HeroSkillDefinition> db, DesignTree tree)
    {
        char role = tree.Role;
        HeroChannel channel = HeroDesign.ChannelOfRole(role);
        var stages = new List<HeroSkillTreeStage>();
        for (int s = 0; s < tree.Stages.Count; s++)
        {
            var slots = new List<HeroSkillTreeSkill>();
            for (int k = 0; k < tree.Stages[s].Skills.Count; k++)
            {
                DesignSkill sk = tree.Stages[s].Skills[k];
                HeroSkillDefinition def;
                if (!db.TryGetValue(new StaticString(sk.Name), out def))
                {
                    var levels = new HeroSkillDefinition.HeroSkillLevelDefinition[sk.Levels];
                    for (int l = 0; l < levels.Length; l++)
                    {
                        var level = EmptyLevel();
                        var mods = new List<SimulationModifierDescriptor>();
                        for (int c = 0; c < sk.Concepts.Length; c++)
                        {
                            DesignMod[] cm = sk.Concepts[c].Mods;
                            for (int m = 0; m < cm.Length; m++)
                            {
                                float value = HeroDesign.ModValue(cm[m], sk.Values[l][c]);
                                if (value == 0f) continue;
                                mods.Add(CustomHackingHelper.MakeModifier(cm[m].Prop, (SimulationModifierDescriptor.ModifierOperation)cm[m].Op, value, cm[m].Path));
                            }
                        }
                        var list = new List<SimulationDescriptor>();
                        if (mods.Count > 0)
                            list.Add(CustomHackingHelper.CreateAndRegisterDescriptor(null, "HRA_" + sk.Name + "_" + l, DescriptorType(channel), null, mods.ToArray()));
                        if (channel == HeroChannel.System) level.systemDescriptors = list.ToArray();
                        else if (channel == HeroChannel.Senate) level.senateDescriptors = list.ToArray();
                        else level.shipDescriptors = list.ToArray();
                        if (l == levels.Length - 1 && completeDescriptor != null) level.heroDescriptors = new[] { completeDescriptor };
                        level.MasteryLevels = Masteries(MasteryPartOf(sk.Concepts[0].Icon));
                        levels[l] = level;
                    }
                    def = new HeroSkillDefinition();
                    def.XmlSerializableName = sk.Name;
                    def.SkillLevels = levels;
                    def.Cost = 0;
                    def.HeroSkillTreeReference = tree.TreeName;
                    try { def.PostDeserialization(); } catch (Exception) { }
                    db.Add(def);

                    bool ru = Ru();
                    string desc = HeroDesign.Describe(sk, ru) + " " + CouncilGame.Tr(
                        KindWord(tree.Key, true) + ": " + HeroDesign.KeyTitle(tree.Key, true) + ". Роль: " + RoleName(role, true) + ".",
                        KindWord(tree.Key, false) + ": " + HeroDesign.KeyTitle(tree.Key, false) + ". Role: " + RoleName(role, false) + ".");
                    RegisterGui(sk.Name, IconTemplateFor(sk.Concepts[0].Icon), ru ? sk.TitleRu : sk.TitleEn, desc);
                }
                contentSkills[sk.Name] = def;
                // live mirrors: the Stealth / Detect skills carry no static descriptors, their effect is
                // injected by HeroRework.AddMirrorSkills (capped by the researched technology);
                // the Mothership ultimate swaps the hull - HeroRework.TryMothership rebuilds the ship
                if (sk.Tier < 4 && sk.Concepts.Length == 1)
                {
                    string special = sk.Concepts[0].Special;
                    if (special == "Stealth" || special == "Detect") HeroRework.RegisterMirrorSkill(sk.Name, special);
                }
                if (sk.Tier == 4 && sk.Concepts.Length == 1 && sk.Concepts[0].Special == "Mothership")
                    HeroRework.RegisterMothershipSkill(sk.Name);
                var slot = new HeroSkillTreeSkill();
                slot.skillDefinition = def;
                slot.SkillDefinitionReference = new XmlNamedReference(sk.Name);
                slot.RequiredSkillsReferences = new XmlNamedReference[0];
                slots.Add(slot);
            }
            var stage = new HeroSkillTreeStage();
            stage.RequiredLevel = tree.Stages[s].RequiredLevel;
            stage.Skills = slots.ToArray();
            stages.Add(stage);
        }
        var result = new HeroSkillTreeDefinition();
        result.XmlSerializableName = tree.TreeName;
        result.Stages = stages.ToArray();
        authoredTrees[tree.TreeName] = result;

        string title = tree.Key.StartsWith("Dict", StringComparison.Ordinal)
            ? CouncilGame.Tr("Диктатор: " + HeroDesign.KeyTitle(tree.Key, true), "Dictator: " + HeroDesign.KeyTitle(tree.Key, false))
            : CouncilGame.Tr(KindWord(tree.Key, true) + ": " + HeroDesign.KeyTitle(tree.Key, true) + " (" + RoleName(role, true) + ")",
                             KindWord(tree.Key, false) + ": " + HeroDesign.KeyTitle(tree.Key, false) + " (" + RoleName(role, false) + ")");
        RegisterGui(tree.TreeName, GenericTreeName, title, title);
    }

    static string IconTemplateFor(Flavor flavor)
    {
        string name;
        if (iconSkillOfFlavor.TryGetValue((int)flavor, out name)) return name;
        return anySkillName;
    }

    // ---------------------------------------------------------------- trees of a hero

    static string ClassTreeName(Hero hero)
    {
        try
        {
            string n = hero.Definition.Class.Name.ToString();
            return "HeroSkillTree" + (n.StartsWith("HeroClass") ? n.Substring("HeroClass".Length) : n);
        }
        catch (Exception) { return null; }
    }

    /// <summary>The trees of a hero: the written trees of his role instead of the vanilla ones (party tree instead of the universal one); null when the content is not registered.</summary>
    static HeroSkillTreeDefinition[] BuildHeroTrees(HeroSkillTreeDefinition[] original, Hero hero, HeroKind kind, bool dictator)
    {
        if (contentSkills.Count == 0) return null;
        char role = dictator ? 'D' : HeroFlavors.KindCode(kind, false)[0];
        string classTree = ClassTreeName(hero);
        int party = 0;
        string faction = "Other";
        try { party = HeroFlavors.PartyIndex(hero.Definition.Politics.ToString()); } catch (Exception) { }
        try { faction = HeroFlavors.FactionKey(hero.Definition.Affinity.ToString()); } catch (Exception) { }

        var result = new List<HeroSkillTreeDefinition>();
        for (int i = 0; i < original.Length; i++)
        {
            HeroSkillTreeDefinition tree = original[i];
            if (tree == null || tree.Stages == null) continue;
            string name = tree.Name.ToString();
            HeroSkillTreeDefinition replacement;
            if (name == GenericTreeName)
            {
                if (party > 0 && authoredTrees.TryGetValue(HeroDesign.TreeName("Party" + party.ToString("00"), role), out replacement)) result.Add(replacement);
                continue;
            }
            if (dictator && classTree != null && name == classTree) continue;   // the dictator has a tree of his own instead of the class tree
            string key = HeroDesign.TreeKeyOfVanilla(name);
            if (key != null && authoredTrees.TryGetValue(HeroDesign.TreeName(key, role), out replacement)) result.Add(replacement);
        }
        if (dictator)
        {
            HeroSkillTreeDefinition own;
            if (authoredTrees.TryGetValue(HeroDesign.TreeName("Dict" + faction, 'D'), out own)) result.Add(own);
        }
        return result.Count == 0 ? null : result.ToArray();
    }

    // ---------------------------------------------------------------- votes of the Galactic Council

    /// <summary>Council votes of one hero (the sum of his learned skills that give votes).</summary>
    public static int CouncilVotesOf(Hero hero)
    {
        int total = 0;
        var skills = hero == null ? null : hero.UnlockedSkills;
        if (skills == null) return 0;
        for (int i = 0; i < skills.Count; i++)
        {
            HeroSkill sk = skills[i];
            if (sk == null || sk.Definition == null) continue;
            total += HeroDesign.VotesOfSkill(sk.Definition.Name.ToString(), sk.Level);
        }
        return total;
    }

    /// <summary>Council votes an empire gets from the heroes that represent its political parties in the senate.</summary>
    public static int CouncilVotesOf(Empire empire)
    {
        if (!Enabled || empire == null) return 0;
        Senate senate = SenateOf(empire);
        if (senate == null || senate.Senators == null) return 0;
        int total = 0;
        foreach (Senator s in senate.Senators)
            if (s != null && s.RepresentativeHero != null) total += CouncilVotesOf(s.RepresentativeHero);
        return total;
    }
}
