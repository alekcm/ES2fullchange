// Hero rework (stage 4): the skill content.
//  * every vanilla skill is rebuilt for each role (admiral, governor, senator, dictator): effects of the role are kept, effects of the
//    other channels are converted into the equivalent effect of the role (see HeroSkillData), so no skill is wasted;
//  * the "Universal" tree is replaced by the tree of the political party of the hero, the race tree and the class tree keep their
//    structure but hold the skills of the role: an admiral Sophon and a governor Sophon get different Sophon trees;
//  * a dictator gets the senate version of the race tree, his party tree and a unique tree of his faction.
// All of it is generated at run time from the live skill database and registered in the databases of the game
// (skill definitions and their GUI elements), so saves find the skills by name.
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
    static readonly Dictionary<string, HeroSkillTreeDefinition> variantTrees = new Dictionary<string, HeroSkillTreeDefinition>();
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

            for (int i = 0; i < bases.Count; i++)
                for (int r = 0; r < roleCodes.Length; r++)
                {
                    try { RegisterVariant(db, bases[i], roleKinds[r], roleDictator[r]); }
                    catch (Exception e) { Fail("variant " + bases[i].Name, e); }
                }

            for (int party = 1; party <= 6; party++)
                for (int r = 0; r < roleCodes.Length; r++)
                {
                    try { RegisterAuthored(db, PartyTreeName(party, roleCodes[r]), "HRParty" + party.ToString("00"), HeroFlavors.PartyTheme(party),
                                           roleKinds[r], roleDictator[r], false,
                                           HeroFlavors.PartyName(party, true), HeroFlavors.PartyName(party, false), null); }
                    catch (Exception e) { Fail("party tree " + party, e); }
                }
            for (int f = 0; f < HeroFlavors.FactionKeys.Length; f++)
            {
                string key = HeroFlavors.FactionKeys[f];
                try { RegisterAuthored(db, DictatorTreeName(key), "HRDict" + key, HeroFlavors.FactionTheme(key), HeroKind.Senator, true, true,
                                       HeroFlavors.FactionName(key, true), HeroFlavors.FactionName(key, false), key); }
                catch (Exception e) { Fail("dictator tree " + key, e); }
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
        contentSkills.Clear(); variantTrees.Clear(); authoredTrees.Clear(); iconSkillOfFlavor.Clear(); masteryNames.Clear();
        completeDescriptor = null; anySkillName = null;
        treeCache.Clear(); ultimateSets.Clear();
    }

    static string PartyTreeName(int party, string code) { return "HRTreeParty" + party.ToString("00") + "@" + code; }
    static string DictatorTreeName(string faction) { return "HRTreeDict" + faction; }

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

    static SimulationDescriptor MakeConverted(string name, HeroChannel channel, Dictionary<int, float> magnitudes, float power)
    {
        if (magnitudes.Count == 0) return null;
        var mods = new List<SimulationModifierDescriptor>();
        foreach (var kv in magnitudes)
        {
            FlavorTarget[] targets = HeroFlavors.TargetsOf((Flavor)kv.Key, channel);
            for (int t = 0; t < targets.Length; t++)
            {
                float value = HeroFlavors.ValueOf(targets[t], kv.Value, power);
                if (value == 0f) continue;
                mods.Add(CustomHackingHelper.MakeModifier(targets[t].Prop, (SimulationModifierDescriptor.ModifierOperation)targets[t].Op, value, targets[t].Path));
            }
        }
        if (mods.Count == 0) return null;
        return CustomHackingHelper.CreateAndRegisterDescriptor(null, name, DescriptorType(channel), null, mods.ToArray());
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

    static SimulationDescriptor[] ExcludeComplete(SimulationDescriptor[] source)
    {
        if (source == null) return new SimulationDescriptor[0];
        var list = new List<SimulationDescriptor>();
        for (int i = 0; i < source.Length; i++)
            if (source[i] != null && source[i].Name.ToString() != "HeroSkillComplete") list.Add(source[i]);
        return list.ToArray();
    }

    static SimulationDescriptor[] Concat(SimulationDescriptor[] a, List<SimulationDescriptor> b)
    {
        var list = new List<SimulationDescriptor>();
        if (a != null) for (int i = 0; i < a.Length; i++) if (a[i] != null) list.Add(a[i]);
        list.AddRange(b);
        return list.ToArray();
    }

    // ---------------------------------------------------------------- variants of vanilla skills

    static void RegisterVariant(IDatabase<HeroSkillDefinition> db, HeroSkillDefinition src, HeroKind kind, bool dictator)
    {
        string code = HeroFlavors.KindCode(kind, dictator);
        string name = src.Name.ToString() + "@" + code;
        HeroSkillDefinition existing;
        if (db.TryGetValue(new StaticString(name), out existing)) { contentSkills[name] = existing; return; }

        HeroChannel channel = HeroFlavors.ChannelOf(kind);
        float power = HeroFlavors.PowerOf(dictator);
        HeroSkillDefinition.HeroSkillLevelDefinition[] srcLevels = src.SkillLevels;
        int n = srcLevels == null ? 0 : srcLevels.Length;
        int count = n == 0 ? 0 : (kind == HeroKind.Senator && !dictator ? 1 : (dictator && n == 2 ? 3 : n));
        var levels = new HeroSkillDefinition.HeroSkillLevelDefinition[count];
        var finalFlavors = new Dictionary<int, float>();
        for (int j = 0; j < count; j++)
        {
            int sj = (kind == HeroKind.Senator && !dictator) ? n - 1 : Math.Min(j, n - 1);
            HeroSkillDefinition.HeroSkillLevelDefinition s = srcLevels[sj];
            float levelScale = (dictator && j >= n) ? 1.5f : 1f;
            float factor = power * levelScale;
            var level = EmptyLevel();
            var all = new Dictionary<int, float>();
            var converted = new Dictionary<int, float>();

            // own channel: the vanilla descriptors, strengthened for the dictator
            var extra = new List<SimulationDescriptor>();
            SimulationDescriptor[] own = channel == HeroChannel.System ? s.SystemDescriptors : channel == HeroChannel.Senate ? s.SenateDescriptors : s.ShipDescriptors;
            Accumulate(own, channel, all);
            if (channel == HeroChannel.Ship)
            {
                SimulationDescriptor[][] ship = ShipArrays(s);
                for (int a = 1; a < ship.Length; a++) Accumulate(ship[a], channel, all);
            }
            if (factor > 1.001f && own != null)
                for (int d = 0; d < own.Length; d++)
                {
                    SimulationDescriptor copy = ScaledCopy(own[d], factor - 1f, (int)Math.Round((factor - 1f) * 1000f) + 7000000);
                    if (copy != null) extra.Add(copy);
                }

            // foreign channels: converted into the effects of this channel
            if (channel != HeroChannel.Ship)
            {
                SimulationDescriptor[][] ship = ShipArrays(s);
                for (int a = 0; a < ship.Length; a++) Accumulate(ship[a], HeroChannel.Ship, converted);
            }
            if (channel != HeroChannel.System) Accumulate(s.SystemDescriptors, HeroChannel.System, converted);
            if (channel != HeroChannel.Senate) Accumulate(s.SenateDescriptors, HeroChannel.Senate, converted);
            if (converted.Count == 0 && all.Count == 0)
            {
                // nothing to carry over: a modest effect of the theme of the skill, so the level is never empty
                Flavor fallback = Flavor.Production;
                if (s.MasteryLevels != null && s.MasteryLevels.Length > 0) fallback = HeroFlavors.FlavorOfMastery(s.MasteryLevels[0].MasteryName.ToString(), fallback);
                converted[(int)fallback] = (j + 1f) / count;
            }
            foreach (var kv in converted)
            {
                float old;
                all.TryGetValue(kv.Key, out old);
                all[kv.Key] = old + kv.Value;
            }
            var convScaled = new Dictionary<int, float>();
            foreach (var kv in converted) convScaled[kv.Key] = kv.Value * levelScale;
            SimulationDescriptor conv = MakeConverted("HRC_" + name + "_" + j, channel, convScaled, power);
            if (conv != null) extra.Add(conv);

            if (channel == HeroChannel.System) level.systemDescriptors = Concat(own, extra);
            else if (channel == HeroChannel.Senate) level.senateDescriptors = Concat(own, extra);
            else
            {
                level.shipDescriptors = Concat(own, extra);
                level.onOrbitDescriptors = Concat(s.OnOrbitDescriptors, new List<SimulationDescriptor>());
                level.onOwnedOrbitDescriptors = Concat(s.OnOwnedOrbitDescriptors, new List<SimulationDescriptor>());
                level.onHostileOrbitDescriptors = Concat(s.OnHostileOrbitDescriptors, new List<SimulationDescriptor>());
                level.onNeutralOrbitDescriptors = Concat(s.OnNeutralOrbitDescriptors, new List<SimulationDescriptor>());
                level.onFriendlyOrbitDescriptors = Concat(s.OnFriendlyOrbitDescriptors, new List<SimulationDescriptor>());
                if (s.EncounterPlayDefinitions != null) level.encounterPlayDefinitions = s.EncounterPlayDefinitions;
            }
            // the skill is complete on its last level only
            level.heroDescriptors = j == count - 1 ? (s.HeroDescriptors ?? new SimulationDescriptor[0]) : ExcludeComplete(s.HeroDescriptors);
            if (j == count - 1 && !HasComplete(level.heroDescriptors) && completeDescriptor != null)
                level.heroDescriptors = Concat(level.heroDescriptors, new List<SimulationDescriptor> { completeDescriptor });
            level.MasteryLevels = s.MasteryLevels ?? new HeroSkillDefinition.MasteryLevel[0];
            level.DefaultClass = s.DefaultClass;
            level.Name = s.Name ?? "";
            levels[j] = level;
            if (j == count - 1) finalFlavors = all;
        }

        var def = new HeroSkillDefinition();
        def.XmlSerializableName = name;
        def.SkillLevels = levels;
        def.Cost = src.Cost;
        def.HeroSkillTreeReference = src.HeroSkillTreeReference;
        def.tags = src.tags;
        try { def.PostDeserialization(); } catch (Exception) { }
        db.Add(def);
        contentSkills[name] = def;

        // GUI element: the icons of the original, the name and the description of the new skill
        List<Flavor> top = TopFlavors(finalFlavors, 2);
        string title, description;
        var tr = new List<string>();
        for (int i = 0; i < top.Count; i++) tr.Add(HeroFlavors.Noun(top[i], channel, Ru()));
        if (top.Count == 0) title = Ru() ? "Навык" : "Skill";
        else title = HeroFlavors.SkillTitle(top.ToArray(), channel, Ru());
        string roleName = dictator ? CouncilGame.Tr("Диктатор", "Dictator") : HeroKinds.Name(kind, Ru());
        string baseTitle = null;
        try { GuiElement baseEl; if (Gui.GuiElementsDatabase.TryGetValue(src.Name, out baseEl)) baseTitle = Gui.Localize(baseEl.Title); } catch (Exception) { }
        description = CouncilGame.Tr("Версия навыка для роли «" + roleName + "»" + (baseTitle != null ? " на основе «" + baseTitle + "»" : "") + ". Эффекты пересчитаны под роль героя.",
                                     "Skill version for the role \"" + roleName + "\"" + (baseTitle != null ? " based on \"" + baseTitle + "\"" : "") + ". Effects are recalculated for the hero role.");
        RegisterGui(name, src.Name.ToString(), title, description);
    }

    static bool HasComplete(SimulationDescriptor[] a)
    {
        if (a == null) return false;
        for (int i = 0; i < a.Length; i++) if (a[i] != null && a[i].Name.ToString() == "HeroSkillComplete") return true;
        return false;
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

    // ---------------------------------------------------------------- authored trees (party, dictator)

    static void RegisterAuthored(IDatabase<HeroSkillDefinition> db, string treeName, string idPrefix, Flavor[] theme, HeroKind kind, bool dictator, bool authority,
                                 string titleRu, string titleEn, string faction)
    {
        string code = HeroFlavors.KindCode(kind, dictator);
        HeroChannel channel = HeroFlavors.ChannelOf(kind);
        float power = HeroFlavors.PowerOf(dictator);
        List<PlannedStage> plan = HeroFlavors.PlanTree(theme, kind, dictator, authority);
        var stages = new List<HeroSkillTreeStage>();
        for (int s = 0; s < plan.Count; s++)
        {
            var slots = new List<HeroSkillTreeSkill>();
            for (int k = 0; k < plan[s].Skills.Count; k++)
            {
                PlannedSkill ps = plan[s].Skills[k];
                string name = idPrefix + ps.Id + (faction != null ? "" : "@" + code);
                HeroSkillDefinition def;
                if (!db.TryGetValue(new StaticString(name), out def))
                {
                    var levels = new HeroSkillDefinition.HeroSkillLevelDefinition[ps.Magnitudes.Length];
                    for (int l = 0; l < levels.Length; l++)
                    {
                        var level = EmptyLevel();
                        var mags = new Dictionary<int, float>();
                        for (int f = 0; f < ps.Flavors.Length; f++)
                        {
                            float old;
                            mags.TryGetValue((int)ps.Flavors[f], out old);
                            mags[(int)ps.Flavors[f]] = old + ps.Magnitudes[l][f];
                        }
                        SimulationDescriptor d = MakeConverted("HRA_" + name + "_" + l, channel, mags, power);
                        var list = new List<SimulationDescriptor>();
                        if (d != null) list.Add(d);
                        if (channel == HeroChannel.System) level.systemDescriptors = list.ToArray();
                        else if (channel == HeroChannel.Senate) level.senateDescriptors = list.ToArray();
                        else level.shipDescriptors = list.ToArray();
                        if (l == levels.Length - 1 && completeDescriptor != null) level.heroDescriptors = new[] { completeDescriptor };
                        level.MasteryLevels = Masteries(MasteryPartOf(ps.Flavors[0]));
                        levels[l] = level;
                    }
                    def = new HeroSkillDefinition();
                    def.XmlSerializableName = name;
                    def.SkillLevels = levels;
                    def.Cost = 0;
                    def.HeroSkillTreeReference = treeName;
                    try { def.PostDeserialization(); } catch (Exception) { }
                    db.Add(def);

                    string title = HeroFlavors.SkillTitle(ps.Flavors, channel, Ru());
                    string tag = faction != null ? HeroFlavors.FactionName(faction, Ru()) : HeroFlavors.PartyName(PartyOfPrefix(idPrefix), Ru());
                    string desc = CouncilGame.Tr(
                        (faction != null ? "Диктатура, " : "Партия, ") + tag + ". Особый навык роли «" + (dictator ? "Диктатор" : HeroKinds.Name(kind, true)) + "».",
                        (faction != null ? "Dictatorship, " : "Party, ") + tag + ". A skill of the role \"" + (dictator ? "Dictator" : HeroKinds.Name(kind, false)) + "\".");
                    float[] vm = HeroFlavors.VoteMagnitudes(name);
                    if (vm != null)
                    {
                        string ru = "", en = "";
                        for (int l = 0; l < vm.Length; l++)
                        {
                            string part = "+" + Math.Round(vm[l] * HeroFlavors.VotesFlatPerMagnitude) + " (+" + HeroFlavors.Round(vm[l] * HeroFlavors.VotesPerFourSystems) + ")";
                            ru += (l > 0 ? ", " : "") + part; en += (l > 0 ? ", " : "") + part;
                        }
                        desc += CouncilGame.Tr(" Голоса в Галактическом Совете по уровням: " + ru + " (в скобках — за каждые 4 колонизированные системы). Действует, пока герой представляет партию в Сенате.",
                                               " Galactic Council votes by level: " + en + " (in brackets: per every 4 colonized systems). Works while the hero represents his party in the Senate.");
                    }
                    string icon = IconTemplateFor(ps.Flavors[0]);
                    RegisterGui(name, icon, title, desc);
                }
                contentSkills[name] = def;
                var slot = new HeroSkillTreeSkill();
                slot.skillDefinition = def;
                slot.SkillDefinitionReference = new XmlNamedReference(name);
                slot.RequiredSkillsReferences = new XmlNamedReference[0];
                slots.Add(slot);
            }
            var stage = new HeroSkillTreeStage();
            stage.RequiredLevel = plan[s].RequiredLevel;
            stage.Skills = slots.ToArray();
            stages.Add(stage);
        }
        var tree = new HeroSkillTreeDefinition();
        tree.XmlSerializableName = treeName;
        tree.Stages = stages.ToArray();
        authoredTrees[treeName] = tree;

        string treeTitle = faction != null
            ? CouncilGame.Tr("Диктатор: " + titleRu, "Dictator: " + titleEn)
            : CouncilGame.Tr("Партия: " + titleRu + " (" + HeroKinds.Name(kind, true) + ")", "Party: " + titleEn + " (" + HeroKinds.Name(kind, false) + ")");
        if (faction == null && dictator) treeTitle = CouncilGame.Tr("Партия: " + titleRu + " (диктатор)", "Party: " + titleEn + " (dictator)");
        RegisterGui(treeName, GenericTreeName, treeTitle, treeTitle);
    }

    static int PartyOfPrefix(string prefix)
    {
        int p = HeroFlavors.PartyIndex(prefix);
        return p < 1 ? 1 : p;
    }

    static string IconTemplateFor(Flavor flavor)
    {
        string name;
        if (iconSkillOfFlavor.TryGetValue((int)flavor, out name)) return name;
        return anySkillName;
    }

    // ---------------------------------------------------------------- trees of a hero

    static int ScaleThreshold(int required, HeroKind kind, bool dictator)
    {
        if (dictator) return (required * 3 + 1) / 2;
        if (kind == HeroKind.Senator) return required / 2;
        return required;
    }

    static HeroSkillTreeSkill VariantSlot(HeroSkillTreeSkill slot, string code)
    {
        if (slot == null || slot.SkillDefinition == null) return null;
        HeroSkillDefinition variant;
        if (!contentSkills.TryGetValue(slot.SkillDefinition.Name.ToString() + "@" + code, out variant)) return null;
        var copy = (HeroSkillTreeSkill)CloneObject(slot);
        copy.skillDefinition = variant;
        copy.SkillDefinitionReference = new XmlNamedReference(variant.Name.ToString());
        if (slot.RequiredSkillsReferences != null)
        {
            var refs = new List<XmlNamedReference>();
            for (int i = 0; i < slot.RequiredSkillsReferences.Length; i++)
            {
                XmlNamedReference r = slot.RequiredSkillsReferences[i];
                if (r != null) refs.Add(new XmlNamedReference(r.Name.ToString() + "@" + code));
            }
            copy.RequiredSkillsReferences = refs.ToArray();
        }
        copy.requiredSkills = null;
        if (slot.Alternatives != null)
        {
            var alts = new List<HeroSkillTreeSkill>();
            for (int i = 0; i < slot.Alternatives.Length; i++)
            {
                HeroSkillTreeSkill a = VariantSlot(slot.Alternatives[i], code);
                if (a != null) alts.Add(a);
            }
            copy.Alternatives = alts.ToArray();
        }
        return copy;
    }

    static HeroSkillTreeDefinition VariantTree(HeroSkillTreeDefinition tree, HeroKind kind, bool dictator)
    {
        string code = HeroFlavors.KindCode(kind, dictator);
        string key = tree.Name.ToString() + "|" + code;
        HeroSkillTreeDefinition cached;
        if (variantTrees.TryGetValue(key, out cached)) return cached;
        HeroSkillTreeDefinition result = null;
        if (tree.Stages != null)
        {
            var stages = new List<HeroSkillTreeStage>();
            for (int s = 0; s < tree.Stages.Length; s++)
            {
                HeroSkillTreeStage st = tree.Stages[s];
                if (st == null || st.Skills == null) continue;
                var slots = new List<HeroSkillTreeSkill>();
                for (int k = 0; k < st.Skills.Length; k++)
                {
                    HeroSkillTreeSkill slot = VariantSlot(st.Skills[k], code);
                    if (slot != null) slots.Add(slot);
                }
                if (slots.Count == 0) continue;
                var sc = (HeroSkillTreeStage)CloneObject(st);
                sc.Skills = slots.ToArray();
                sc.RequiredLevel = ScaleThreshold(st.RequiredLevel, kind, dictator);
                stages.Add(sc);
            }
            if (stages.Count > 0)
            {
                result = (HeroSkillTreeDefinition)CloneObject(tree);
                result.Stages = stages.ToArray();
            }
        }
        variantTrees[key] = result;
        return result;
    }

    static string ClassTreeName(Hero hero)
    {
        try
        {
            string n = hero.Definition.Class.Name.ToString();
            return "HeroSkillTree" + (n.StartsWith("HeroClass") ? n.Substring("HeroClass".Length) : n);
        }
        catch (Exception) { return null; }
    }

    /// <summary>The trees of a hero: race and class of his role, the party tree instead of the universal one; null when the content is not registered.</summary>
    static HeroSkillTreeDefinition[] BuildHeroTrees(HeroSkillTreeDefinition[] original, Hero hero, HeroKind kind, bool dictator)
    {
        if (contentSkills.Count == 0) return null;
        string code = HeroFlavors.KindCode(kind, dictator);
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
            if (name == GenericTreeName && party > 0 && authoredTrees.TryGetValue(PartyTreeName(party, code), out replacement)) { result.Add(replacement); continue; }
            if (dictator && classTree != null && name == classTree) continue;   // the dictator has a tree of his own instead of the class tree
            HeroSkillTreeDefinition variant = VariantTree(tree, kind, dictator);
            if (variant != null) result.Add(variant);
        }
        if (dictator)
        {
            HeroSkillTreeDefinition own;
            if (authoredTrees.TryGetValue(DictatorTreeName(faction), out own)) result.Add(own);
        }
        return result.Count == 0 ? null : result.ToArray();
    }

    // ---------------------------------------------------------------- votes of the Galactic Council

    /// <summary>Council votes of one hero (the sum of his learned skills that give votes).</summary>
    public static int CouncilVotesOf(Hero hero, int systems)
    {
        int total = 0;
        var skills = hero == null ? null : hero.UnlockedSkills;
        if (skills == null) return 0;
        for (int i = 0; i < skills.Count; i++)
        {
            HeroSkill sk = skills[i];
            if (sk == null || sk.Definition == null) continue;
            total += HeroFlavors.VotesOfSkill(sk.Definition.Name.ToString(), sk.Level, systems);
        }
        return total;
    }

    /// <summary>Council votes an empire gets from the heroes that represent its political parties in the senate.</summary>
    public static int CouncilVotesOf(Empire empire)
    {
        if (!Enabled || empire == null) return 0;
        Senate senate = SenateOf(empire);
        if (senate == null || senate.Senators == null) return 0;
        var interior = empire.GetAgency<DepartmentOfTheInterior>();
        int systems = interior == null || interior.ColonizedStarSystems == null ? 0 : interior.ColonizedStarSystems.Count;
        int total = 0;
        foreach (Senator s in senate.Senators)
            if (s != null && s.RepresentativeHero != null) total += CouncilVotesOf(s.RepresentativeHero, systems);
        return total;
    }
}
