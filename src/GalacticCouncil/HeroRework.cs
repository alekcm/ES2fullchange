// Hero rework (stage 1): heroes are admirals, governors or senators.
//  * every effect of a hero skill is applied only through the channel of his kind (ship / system / senate);
//  * only skills that do something for the kind are offered in the skill trees of the hero;
//  * admirals command fleets only, governors govern systems only, only senators represent a party in the senate.
// AI ruler heroes of the Hacking Overhaul (CustomHackingHelper.IsAiHero) are never touched.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using Amplitude.Unity.Simulation.SimulationModifierDescriptors;
using UnityEngine;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Simulation;
using Amplitude.Unity.Xml;

public static partial class HeroRework
{
    public static bool Enabled = true;

    static void Fail(string where, Exception e)
    {
        try { UnityEngine.Debug.LogWarning("[HeroRework] " + where + ": " + e); } catch (Exception) { }
    }

    // ---------------------------------------------------------------- kind

    public static bool IsRegular(Hero hero)
    {
        try { return Enabled && hero != null && hero.Definition != null && !CustomHackingHelper.IsAiHero(hero); }
        catch (Exception) { return false; }
    }

    public static HeroKind KindOf(Hero hero)
    {
        string className = null;
        try { if (hero.Definition != null && hero.Definition.Class != null) className = hero.Definition.Class.Name.ToString(); }
        catch (Exception) { }
        return HeroKinds.KindOf(className, (ulong)hero.GUID);
    }

    // ---------------------------------------------------------------- effect channels (Hero.Get*SkillDescriptors, return hooks)

    static List<SimulationDescriptor> Filter(List<SimulationDescriptor> list, Hero hero, HeroChannel channel)
    {
        try
        {
            if (!IsRegular(hero)) return list;
            if (!HeroKinds.Allows(KindOf(hero), channel)) return new List<SimulationDescriptor>();
            // an empty list is normal for a ship hero whose skills are all special (no static modifiers):
            // the mirror skills below still have to be injected, so null/empty is replaced with a fresh list
            if (list == null) list = new List<SimulationDescriptor>();
            List<SimulationDescriptor> result = WithAutoBonus(list, hero);
            if (channel == HeroChannel.Ship) result = AddMirrorSkills(result, hero);
            return result;
        }
        catch (Exception e) { Fail("Filter", e); return list; }
    }

    public static List<SimulationDescriptor> FilterShip(List<SimulationDescriptor> list, Hero hero) { return Filter(list, hero, HeroChannel.Ship); }
    public static List<SimulationDescriptor> FilterSystem(List<SimulationDescriptor> list, Hero hero) { return Filter(list, hero, HeroChannel.System); }
    public static List<SimulationDescriptor> FilterSenate(List<SimulationDescriptor> list, Hero hero) { return Filter(list, hero, HeroChannel.Senate); }

    // ---------------------------------------------------------------- tech-mirrored skills (Seeker stealth / detection)
    // The skill level is a CAP for the level of the module the empire has actually researched: the hero ship
    // carries the same AntiVisionStrengthForModule / VisionStrength the best researched cloaker / anti-cloaker
    // module would give. When a technology is unlocked the mirror refreshes (OnTechnologyUnlocked below).

    // skill database name -> mirror special ("Stealth" / "Detect"), filled while the designed trees register
    static readonly Dictionary<string, string> mirrorSpecials = new Dictionary<string, string>();

    public static void RegisterMirrorSkill(string skillName, string special) { mirrorSpecials[skillName] = special; }

    static readonly Dictionary<string, string> moduleTechCache = new Dictionary<string, string>();
    static readonly Dictionary<string, SimulationDescriptor> mirrorCache = new Dictionary<string, SimulationDescriptor>();

    /// <summary>Adds the live mirror descriptors of the hero's Stealth / Detect skills (level capped by the
    /// researched technology) and keeps a rebuilt mothership at 0 command points.</summary>
    static List<SimulationDescriptor> AddMirrorSkills(List<SimulationDescriptor> list, Hero hero)
    {
        // Hot path: this runs inside hero descriptor refreshes. The common case (no mirror/mothership
        // skill on this hero) must not allocate: previously the list was copied unconditionally and the
        // technology check rescanned (and ToString'ed) every empire descriptor per module.
        if (mirrorSpecials.Count == 0 && mothershipSkills.Count == 0) return list;
        List<SimulationDescriptor> extra = null;
        var skills = hero.UnlockedSkills;
        if (skills != null && mirrorSpecials.Count > 0)
        {
            HashSet<string> descriptorNames = null;
            for (int i = 0; i < skills.Count; i++)
            {
                string special;
                if (skills[i] == null || skills[i].Definition == null) continue;
                if (!mirrorSpecials.TryGetValue(skills[i].Definition.Name.ToString(), out special)) continue;
                if (descriptorNames == null)
                {
                    var empire = hero.Empire;
                    if (empire == null) break;
                    descriptorNames = DescriptorNames(empire);
                }
                var names = descriptorNames;
                int level = Math.Min(skills[i].Level, HeroMirror.LevelOf(HeroMirror.TableOf(special), ModuleTech, delegate(string t) { return t != null && names.Contains(t); }));
                if (level <= 0) continue;
                SimulationDescriptor d = MirrorDescriptor(special, level);
                if (d != null) { if (extra == null) extra = new List<SimulationDescriptor>(); extra.Add(d); }
            }
        }
        // the mothership rebuild replaces the hero role with the carrier role on the ship; the skill keeps
        // it a free hero ship: 0 command points (the exact Force modifier of ShipRoleHero)
        if (mothershipSkills.Count > 0 && HasMothershipUlt(hero))
        {
            SimulationDescriptor cp = MirrorDescriptor("Mothership", 1);
            if (cp != null) { if (extra == null) extra = new List<SimulationDescriptor>(); extra.Add(cp); }
        }
        if (extra == null) return list;
        var result = new List<SimulationDescriptor>(list);
        result.AddRange(extra);
        return result;
    }

    /// <summary>Snapshot of the empire's descriptor names (one pass, reused for every technology check).</summary>
    static HashSet<string> DescriptorNames(Empire empire)
    {
        var set = new HashSet<string>();
        try
        {
            SimulationDescriptor[] descriptors = empire.GetSimulationDescriptors();
            if (descriptors != null)
                for (int i = 0; i < descriptors.Length; i++)
                    if (descriptors[i] != null) set.Add(descriptors[i].Name.ToString());
        }
        catch (Exception e) { Fail("DescriptorNames", e); }
        return set;
    }

    /// <summary>Best mirror level the empire's researched technologies allow for the special.</summary>
    public static int MirrorTechLevel(string special, Empire empire)
    {
        if (empire == null) return 0;
        var names = DescriptorNames(empire);
        return HeroMirror.LevelOf(HeroMirror.TableOf(special), ModuleTech, delegate(string t) { return t != null && names.Contains(t); });
    }

    /// <summary>Technology descriptor the module requires (the TechnologyPrerequisite of the module), cached; null when unknown.</summary>
    static string ModuleTech(string moduleName)
    {
        string cached;
        if (moduleTechCache.TryGetValue(moduleName, out cached)) return cached;
        string tech = null;
        try
        {
            IDatabase<SupportModuleDefinition> db = Databases.GetDatabase<SupportModuleDefinition>(false);
            SupportModuleDefinition module;
            if (db != null && db.TryGetValue(new StaticString(moduleName), out module) && module != null)
            {
                Prerequisite[] prereqs = module.Prerequisites;
                if (prereqs != null)
                    for (int i = 0; i < prereqs.Length; i++)
                    {
                        var tp = prereqs[i] as TechnologyPrerequisite;
                        if (tp == null) continue;
                        string content = tp.PrerequisiteContent;
                        if (!string.IsNullOrEmpty(content)) { tech = content; break; }
                    }
            }
        }
        catch (Exception e) { Fail("ModuleTech " + moduleName, e); }
        moduleTechCache[moduleName] = tech;
        return tech;
    }

    /// <summary>The empire carries the technology descriptor of a researched technology.</summary>
    public static bool EmpireHasDescriptor(Empire empire, string name)
    {
        try
        {
            SimulationDescriptor[] descriptors = empire.GetSimulationDescriptors();
            if (descriptors == null) return false;
            for (int i = 0; i < descriptors.Length; i++)
                if (descriptors[i] != null && descriptors[i].Name.ToString() == name) return true;
        }
        catch (Exception e) { Fail("EmpireHasDescriptor", e); }
        return false;
    }

    /// <summary>The live mirror descriptor of a special at a level (cached): the modifier pair of the vanilla module, on the hero ship.</summary>
    static SimulationDescriptor MirrorDescriptor(string special, int level)
    {
        string key = special + level;
        SimulationDescriptor cached;
        if (mirrorCache.TryGetValue(key, out cached)) return cached;
        var mods = new List<SimulationModifierDescriptor>();
        if (special == "Stealth")
            mods.Add(CustomHackingHelper.MakeModifier("AntiVisionStrengthForModule", SimulationModifierDescriptor.ModifierOperation.Addition, level, ""));
        else if (special == "Detect")
        {
            mods.Add(CustomHackingHelper.MakeModifier("VisionStrength", SimulationModifierDescriptor.ModifierOperation.Addition, level, ""));
            if (level >= 2) mods.Add(CustomHackingHelper.MakeModifier("MaximumShipDetectionProbeStock", SimulationModifierDescriptor.ModifierOperation.Addition, level, ""));
        }
        else if (special == "Mothership")
            mods.Add(CustomHackingHelper.MakeModifier("CommandPoints", SimulationModifierDescriptor.ModifierOperation.Force, 0f, "ClassShip"));
        SimulationDescriptor result = null;
        try { result = CustomHackingHelper.CreateAndRegisterDescriptor(null, "HRA_Mirror_" + special + "_" + level, "HeroSkillShip", null, mods.ToArray()); }
        catch (Exception e) { Fail("MirrorDescriptor", e); }
        mirrorCache[key] = result;
        return result;
    }

    // ---------------------------------------------------------------- mothership (the Seeker ultimate: the hull swap)
    // The learned ultimate rebuilds the hero ship as a mothership: a large carrier hull of the empire's
    // faction, laid out with bomber squadrons and support instead of personal weapons. The design is a real
    // registered ShipDesign (survives save/load), the swap goes through the game's own hero-ship binding
    // path (DepartmentOfDefense.ApplyShipDesignToShip -> BindShipDesign: sections and descriptors rebuilt).

    static readonly HashSet<string> mothershipSkills = new HashSet<string>();
    static readonly Dictionary<ulong, string> mothershipHulls = new Dictionary<ulong, string>();
    static readonly HashSet<string> mothershipDefinitions = new HashSet<string>();
    static readonly Dictionary<string, ModuleDefinition> moduleCache = new Dictionary<string, ModuleDefinition>();

    public static void RegisterMothershipSkill(string skillName) { mothershipSkills.Add(skillName); }

    static bool HasMothershipUlt(Hero hero)
    {
        if (mothershipSkills.Count == 0) return false;
        var skills = hero.UnlockedSkills;
        if (skills == null) return false;
        for (int i = 0; i < skills.Count; i++)
            if (skills[i] != null && skills[i].Definition != null && mothershipSkills.Contains(skills[i].Definition.Name.ToString())) return true;
        return false;
    }

    /// <summary>Turn pass: rebuild the ship of a Seeker who has learned the Mothership ultimate (once - the design name guards the repeat).</summary>
    public static void TryMothership(Hero hero)
    {
        try
        {
            if (mothershipSkills.Count == 0 || !IsRegular(hero) || !HasMothershipUlt(hero)) return;
            HeroShip ship = hero.HeroShip;
            Empire empire = hero.Empire;
            if (ship == null || empire == null) return;
            string hullName = MothershipHullOf(empire);
            string designName = HeroMothership.DesignNameFor(hullName);
            ShipDesign current = ship.ShipDesign;
            if (current != null && current.DefaultName.ToString() == designName) return;   // already rebuilt
            ShipDesign design = MothershipDesign(empire, hullName, designName);
            if (design == null || !design.IsValid) return;
            DepartmentOfDefense.ApplyShipDesignToShip(ship, design);
            ship.RefreshHeroDescriptors();
            Garrison garrison = ship.Garrison;
            if (garrison != null) garrison.Refresh(false);
        }
        catch (Exception e) { Fail("TryMothership", e); }
    }

    /// <summary>The large carrier hull of the empire's faction (Terrans hull as a safe fallback: retrofitting does not check build prerequisites).</summary>
    static string MothershipHullOf(Empire empire)
    {
        ulong id = (ulong)empire.GUID;
        string cached;
        if (mothershipHulls.TryGetValue(id, out cached)) return cached;
        string name = null;
        try
        {
            string faction = empire.Faction != null ? empire.Faction.Name.ToString() : null;
            if (!string.IsNullOrEmpty(faction) && HullExists(HeroMothership.HullNameFor(faction)))
                name = HeroMothership.HullNameFor(faction);
        }
        catch (Exception) { }
        if (name == null) name = HeroMothership.HullFallback;
        mothershipHulls[id] = name;
        return name;
    }

    static bool HullExists(string hullName)
    {
        try
        {
            HullDefinition hull;
            IDatabase<HullDefinition> db = Databases.GetDatabase<HullDefinition>(false);
            return db != null && db.TryGetValue(new StaticString(hullName), out hull) && hull != null;
        }
        catch (Exception) { return false; }
    }

    /// <summary>The mothership design of the empire: an existing one is reused, otherwise the definition is
    /// registered, the game creates the design (registered entity, save-safe) and the layout is filled with the carrier plan.</summary>
    static ShipDesign MothershipDesign(Empire empire, string hullName, string designName)
    {
        DepartmentOfDefense defense = empire.GetAgency<DepartmentOfDefense>();
        if (defense == null) return null;
        List<ShipDesign> available = defense.AvailableShipDesigns;
        ShipDesign design = available != null ? DepartmentOfDefense.GetShipDesignByDefinitionName(available, new StaticString(designName)) : null;
        if (design == null)
        {
            if (!mothershipDefinitions.Contains(designName))
            {
                if (!RegisterMothershipDefinition(designName, hullName)) return null;
                mothershipDefinitions.Add(designName);
            }
            defense.ServerAddShipDesign(new StaticString(designName));
            available = defense.AvailableShipDesigns;
            design = available != null ? DepartmentOfDefense.GetShipDesignByDefinitionName(available, new StaticString(designName)) : null;
            if (design == null) return null;
            BuildMothershipLayout(design);
        }
        return design;
    }

    /// <summary>A hidden non-editable hero-design-style definition on the carrier hull (like the vanilla ShipDesignHero* designs).</summary>
    static bool RegisterMothershipDefinition(string designName, string hullName)
    {
        try
        {
            IDatabase<ShipDesignDefinition> db = Databases.GetDatabase<ShipDesignDefinition>(false);
            if (db == null) return false;
            if (db.ContainsKey(new StaticString(designName))) return true;
            var def = new ShipDesignDefinition();
            def.XmlSerializableName = designName;
            def.Hidden = true;              // never shows up in the player's design list
            def.Editable = false;
            def.MustBePreloaded = true;     // the vanilla hero designs are preloaded too
            var tags = new Tags();
            tags.AddTag("HideUpkeep");
            def.Tags = tags;
            def.HullReference = new XmlNamedReference(hullName);
            def.ShipRoleReference = new XmlNamedReference(HeroMothership.RoleName);
            def.SlotBindings = new SlotBindingDefinition[0];
            db.Add(def);
            return true;
        }
        catch (Exception e) { Fail("RegisterMothershipDefinition", e); return false; }
    }

    /// <summary>Fills the empty slots of the carrier hull with the mothership plan (HeroMothership.ModuleFor).</summary>
    static void BuildMothershipLayout(ShipDesign design)
    {
        design.BuildShipDesignLayout(false);
        List<Slot> slots = design.Layout.Slots;
        int supportIndex = 0, defenseIndex = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            Slot slot = slots[i];
            if (slot == null || slot.Definition == null) continue;
            ModuleCategory[] categories = slot.Definition.RestrictedModuleCategories;
            string[] names = categories == null ? new string[0] : Array.ConvertAll(categories, delegate(ModuleCategory c) { return c.ToString(); });
            string moduleName = HeroMothership.ModuleFor(names, slot.Definition.IsLargeSlot, supportIndex, defenseIndex);
            ModuleDefinition module = moduleName != null ? ModuleByName(moduleName) : null;
            slot.BindModule(module, false, true);   // a missing module clears the slot, never breaks the ship
            if (module == null) continue;
            if (module.Category == ModuleCategory.Support) supportIndex++;
            else if (module.Category == ModuleCategory.Defense) defenseIndex++;
        }
    }

    static ModuleDefinition ModuleByName(string name)
    {
        ModuleDefinition cached;
        if (moduleCache.TryGetValue(name, out cached)) return cached;
        ModuleDefinition module = null;
        try
        {
            IDatabase<ModuleDefinition> db = Databases.GetDatabase<ModuleDefinition>(false);
            if (db != null) db.TryGetValue(new StaticString(name), out module);
        }
        catch (Exception e) { Fail("ModuleByName " + name, e); }
        moduleCache[name] = module;
        return module;
    }

    /// <summary>End hook of DepartmentOfScience.UnlockTechnology: the mirror skills may rise, refresh every ship hero of the empire.</summary>
    public static void OnTechnologyUnlocked(DepartmentOfScience science)
    {
        try
        {
            if (mirrorSpecials.Count == 0 || science == null || science.Empire == null) return;
            DepartmentOfEducation education = science.Empire.GetAgency<DepartmentOfEducation>();
            if (education == null || education.ActiveHeroes == null) return;
            var heroes = education.ActiveHeroes;
            for (int i = 0; i < heroes.Count; i++)
            {
                Hero hero = heroes[i];
                if (hero == null || hero.HeroShip == null) continue;
                hero.HeroShip.RefreshHeroDescriptors();
            }
        }
        catch (Exception e) { Fail("OnTechnologyUnlocked", e); }
    }

    // ---------------------------------------------------------------- assignments (CustomHackingHelper.CheckHeroAssignableToFleet / ToSystem, start hook)

    /// <summary>Returns false when the hero may not be assigned; failureInfo is then filled.</summary>
    public static bool AllowAssign(Hero hero, ref FailureInfo failureInfo, bool toFleet)
    {
        try
        {
            if (!IsRegular(hero)) return true;
            HeroKind kind = KindOf(hero);
            if (toFleet ? kind == HeroKind.Admiral : kind == HeroKind.Governor) return true;
            failureInfo = new FailureInfo(ConstructionFlags.Disable);
            return false;
        }
        catch (Exception e) { Fail("AllowAssign", e); return true; }
    }

    // ---------------------------------------------------------------- senate representative (Senator.UpdateRepresentativeHero, start hook)

    /// <summary>Replacement of Senator.UpdateRepresentativeHero: only senators (and the AI ruler heroes) can represent a party. Returns false to run the original.</summary>
    public static bool UpdateRepresentative(Senator senator)
    {
        try
        {
            if (!Enabled || senator == null || senator.Empire == null || senator.Politics == null) return false;
            var education = senator.Empire.GetAgency<DepartmentOfEducation>();
            if (education == null) return false;
            var heroes = education.ActiveHeroes;
            Hero best = null;
            for (int i = 0; i < heroes.Count; i++)
            {
                Hero h = heroes[i];
                if (h == null || h.Definition == null) continue;
                if (h.Definition.Politics != senator.Politics.Name) continue;
                if (IsRegular(h) && KindOf(h) != HeroKind.Senator) continue;
                // the dictator hero of a dictatorship always represents the leader
                bool dict = IsDictatorDefinition(h.Definition), bestDict = best != null && IsDictatorDefinition(best.Definition);
                if (best == null || (dict && !bestDict) || (dict == bestDict && h.CompareTo_ForSenate(best) > 0)) best = h;
            }
            senator.RemoveAllHeroDescriptors();
            senator.RepresentativeHero = best;
            if (best != null) senator.RefreshHeroDescriptors();
            return true;
        }
        catch (Exception e) { Fail("UpdateRepresentative", e); return false; }
    }

    // ---------------------------------------------------------------- hero window title

    public static string TitleWithKind(string title, GuiHero gui)
    {
        try
        {
            if (gui == null || gui.Hero == null || !IsRegular(gui.Hero)) return title;
            string kind = HeroKinds.Name(KindOf(gui.Hero), CouncilGame.IsRussian());
            if (IsDictator(gui.Hero)) kind = CouncilGame.Tr("Диктатор", "Dictator");
            return title + "  [" + kind + "]";
        }
        catch (Exception) { return title; }
    }

    // ---------------------------------------------------------------- skill trees (Hero.get_SkillTrees, return hook)

    sealed class TreeCache
    {
        public HeroSkillTreeDefinition[] Original;
        public HeroSkillTreeDefinition[] Filtered;
        public bool Dictator;
        public int Generation;
    }

    static readonly Dictionary<ulong, TreeCache> treeCache = new Dictionary<ulong, TreeCache>();
    static readonly MethodInfo memberwiseClone = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

    static object CloneObject(object source)
    {
        return memberwiseClone.Invoke(source, null);
    }

    static bool SameTrees(HeroSkillTreeDefinition[] a, HeroSkillTreeDefinition[] b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null || a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
            if (!ReferenceEquals(a[i], b[i]) && (a[i] == null || b[i] == null || a[i].Name != b[i].Name)) return false;
        return true;
    }

    /// <summary>Hero.get_SkillTrees return hook: the trees of the role of the hero (see HeroContent), the filtered vanilla trees when the content is missing.</summary>
    public static HeroSkillTreeDefinition[] TreesFor(HeroSkillTreeDefinition[] original, Hero hero)
    {
        try
        {
            if (original == null || original.Length == 0 || !IsRegular(hero)) return original;
            ulong id = (ulong)hero.GUID;
            bool dictator = IsDictator(hero);
            TreeCache c;
            if (treeCache.TryGetValue(id, out c) && c.Dictator == dictator && c.Generation == contentGeneration && SameTrees(c.Original, original)) return c.Filtered;
            EnsureContent();
            HeroKind kind = KindOf(hero);
            HeroSkillTreeDefinition[] built = null;
            try { built = BuildHeroTrees(original, hero, kind, dictator); }
            catch (Exception e) { Fail("BuildHeroTrees", e); }
            if (built == null) built = BuildTrees(original, kind);
            c = new TreeCache { Original = original, Filtered = built, Dictator = dictator, Generation = contentGeneration };
            treeCache[id] = c;
            return c.Filtered;
        }
        catch (Exception e) { Fail("TreesFor", e); return original; }
    }

    static bool NotEmpty(SimulationDescriptor[] a)
    {
        return a != null && a.Length > 0;
    }

    static bool HasHeroEffect(SimulationDescriptor[] a)
    {
        if (a == null) return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != null && a[i].Name.ToString() != "HeroSkillComplete") return true;
        return false;
    }

    /// <summary>True when a level of the skill has an effect in a channel the kind is allowed to use.</summary>
    public static bool SkillHasEffectFor(HeroSkillDefinition skill, HeroKind kind)
    {
        if (skill == null || skill.SkillLevels == null) return false;
        var levels = skill.SkillLevels;
        for (int i = 0; i < levels.Length; i++)
        {
            var l = levels[i];
            if (l == null) continue;
            if (HasHeroEffect(l.HeroDescriptors)) return true;
            if (HeroKinds.Allows(kind, HeroChannel.System) && NotEmpty(l.SystemDescriptors)) return true;
            if (HeroKinds.Allows(kind, HeroChannel.Senate) && NotEmpty(l.SenateDescriptors)) return true;
            if (HeroKinds.Allows(kind, HeroChannel.Ship) &&
                (NotEmpty(l.ShipDescriptors) || NotEmpty(l.OnOrbitDescriptors) || NotEmpty(l.OnOwnedOrbitDescriptors) ||
                 NotEmpty(l.OnHostileOrbitDescriptors) || NotEmpty(l.OnNeutralOrbitDescriptors) || NotEmpty(l.OnFriendlyOrbitDescriptors))) return true;
        }
        return false;
    }

    static bool SlotRelevant(HeroSkillTreeSkill slot, HeroKind kind)
    {
        if (slot == null) return false;
        if (SkillHasEffectFor(slot.SkillDefinition, kind)) return true;
        if (slot.Alternatives != null)
            for (int i = 0; i < slot.Alternatives.Length; i++)
                if (SlotRelevant(slot.Alternatives[i], kind)) return true;
        return false;
    }

    static int PointsOf(HeroSkillTreeSkill slot)
    {
        var def = slot != null ? slot.SkillDefinition : null;
        if (def == null || def.SkillLevels == null) return 0;
        return def.SkillLevels.Length * Math.Max(1, def.Cost);
    }

    static HeroSkillTreeDefinition[] BuildTrees(HeroSkillTreeDefinition[] original, HeroKind kind)
    {
        // 1. skills that do something for the kind, plus every skill they require
        var keep = new HashSet<HeroSkillDefinition>();
        foreach (var tree in original)
        {
            if (tree == null || tree.Stages == null) continue;
            foreach (var st in tree.Stages)
            {
                if (st == null || st.Skills == null) continue;
                foreach (var slot in st.Skills)
                    if (SlotRelevant(slot, kind) && slot.SkillDefinition != null) keep.Add(slot.SkillDefinition);
            }
        }
        bool grew = true;
        while (grew)
        {
            grew = false;
            foreach (var tree in original)
            {
                if (tree == null || tree.Stages == null) continue;
                foreach (var st in tree.Stages)
                {
                    if (st == null || st.Skills == null) continue;
                    foreach (var slot in st.Skills)
                    {
                        if (slot == null || slot.SkillDefinition == null || !keep.Contains(slot.SkillDefinition) || slot.RequiredSkills == null) continue;
                        foreach (var req in slot.RequiredSkills)
                            if (req != null && keep.Add(req)) grew = true;
                    }
                }
            }
        }

        // 2. filtered copies of the trees
        var result = new List<HeroSkillTreeDefinition>();
        foreach (var tree in original)
        {
            if (tree == null || tree.Stages == null) { result.Add(tree); continue; }
            var copy = (HeroSkillTreeDefinition)CloneObject(tree);
            var stages = new List<HeroSkillTreeStage>();
            foreach (var st in tree.Stages)
            {
                if (st == null || st.Skills == null) { stages.Add(st); continue; }
                var slots = new List<HeroSkillTreeSkill>();
                foreach (var slot in st.Skills)
                    if (slot != null && slot.SkillDefinition != null && keep.Contains(slot.SkillDefinition)) slots.Add(slot);
                if (slots.Count == 0) continue;
                var sc = (HeroSkillTreeStage)CloneObject(st);
                sc.Skills = slots.ToArray();
                stages.Add(sc);
            }
            if (stages.Count == 0) continue;   // nothing of this tree is useful for the kind: no empty tree
            copy.Stages = stages.ToArray();
            result.Add(copy);
        }

        // 3. stage thresholds are skill points spent by the hero: never ask for more than the filtered skills below can give
        var newLevels = new List<KeyValuePair<HeroSkillTreeStage, int>>();
        foreach (var tree in result)
        {
            if (tree == null || tree.Stages == null) continue;
            foreach (var st in tree.Stages)
            {
                if (st == null || st.RequiredLevel <= 0) continue;
                int below = 0;
                foreach (var t2 in result)
                {
                    if (t2 == null || t2.Stages == null) continue;
                    foreach (var s2 in t2.Stages)
                    {
                        if (s2 == null || s2.Skills == null || s2.RequiredLevel >= st.RequiredLevel) continue;
                        foreach (var slot in s2.Skills) below += PointsOf(slot);
                    }
                }
                newLevels.Add(new KeyValuePair<HeroSkillTreeStage, int>(st, HeroKinds.ClampRequiredLevel(st.RequiredLevel, below)));
            }
        }
        foreach (var kv in newLevels) kv.Key.RequiredLevel = kv.Value;
        return result.ToArray();
    }

    // ---------------------------------------------------------------- ultimate fixes the specialization (admirals and governors)
    // The first skill taken from the last stage of any of the hero's trees is his ultimate: the hero stops choosing skills,
    // and every hero level after it adds (100 / level the ultimate was taken at) percent to every skill he learned.

    static Dictionary<ulong, int> ultimates = new Dictionary<ulong, int>();
    static readonly Dictionary<ulong, int> appliedSteps = new Dictionary<ulong, int>();
    static readonly Dictionary<string, HashSet<HeroSkillDefinition>> ultimateSets = new Dictionary<string, HashSet<HeroSkillDefinition>>();
    static readonly Dictionary<string, SimulationDescriptor> bonusCache = new Dictionary<string, SimulationDescriptor>();
    static int lastTurn = int.MinValue;
    static int descriptorSeq;

    public static bool Fixable(Hero hero)
    {
        if (!IsRegular(hero)) return false;
        // senators can always pick new skills, except the dictator and a senator who already took an ultimate
        return KindOf(hero) != HeroKind.Senator || IsDictator(hero) || ultimates.ContainsKey((ulong)hero.GUID);
    }

    public static bool IsFixed(Hero hero)
    {
        try { return Fixable(hero) && ultimates.ContainsKey((ulong)hero.GUID); }
        catch (Exception) { return false; }
    }

    public static int UltimateLevel(Hero hero)
    {
        int v;
        return ultimates.TryGetValue((ulong)hero.GUID, out v) ? v : 0;
    }

    public static string Serialize() { return HeroKinds.JoinState(HeroKinds.SerializeUltimates(ultimates), HeroKinds.SerializeDictators(DictatorsForSave())) + "~" + SerializeSpecials(); }

    public static void Deserialize(string text)
    {
        string ult, dict, specials = "";
        if (text != null)
        {
            int tilde = text.IndexOf('~');
            if (tilde >= 0) { specials = text.Substring(tilde + 1); text = text.Substring(0, tilde); }
        }
        ParseSpecials(specials);
        HeroKinds.SplitState(text, out ult, out dict);
        ultimates = HeroKinds.ParseUltimates(ult);
        LoadDictators(HeroKinds.ParseDictators(dict));
        appliedSteps.Clear();
        bonusCache.Clear();
        ultimateSets.Clear();
        lastTurn = int.MinValue;
    }

    /// <summary>Skills of the last stage of the trees the hero really has (his role decides the trees): taking one of them is the ultimate.</summary>
    static HashSet<HeroSkillDefinition> UltimatesOf(Hero hero)
    {
        string key = hero.Definition.Name.ToString() + "|" + HeroFlavors.KindCode(KindOf(hero), IsDictator(hero)) + "|" + contentGeneration;
        HashSet<HeroSkillDefinition> set;
        if (ultimateSets.TryGetValue(key, out set)) return set;
        set = new HashSet<HeroSkillDefinition>();
        var trees = TreesFor(hero.Definition.GetSkillTrees(), hero);
        if (trees != null)
            foreach (var tree in trees)
            {
                if (tree == null || tree.Stages == null || tree.Stages.Length < 2) continue;
                int top = 0;
                foreach (var st in tree.Stages) if (st != null && st.RequiredLevel > top) top = st.RequiredLevel;
                foreach (var st in tree.Stages)
                {
                    if (st == null || st.Skills == null || st.RequiredLevel != top) continue;
                    foreach (var slot in st.Skills)
                    {
                        if (slot != null && slot.SkillDefinition != null) set.Add(slot.SkillDefinition);
                        if (slot != null && slot.Alternatives != null)
                            foreach (var alt in slot.Alternatives) if (alt != null && alt.SkillDefinition != null) set.Add(alt.SkillDefinition);
                    }
                }
            }
        ultimateSets[key] = set;
        return set;
    }

    public static bool HoldsUltimate(Hero hero)
    {
        var set = UltimatesOf(hero);
        if (set.Count == 0) return false;
        var skills = hero.UnlockedSkills;
        if (skills == null) return false;
        for (int i = 0; i < skills.Count; i++)
            if (skills[i] != null && skills[i].Definition != null && set.Contains(skills[i].Definition)) return true;
        return false;
    }

    /// <summary>Dummy heroes are copies used by the skill editing window: only the entity the game really owns may fix a specialization.</summary>
    static bool IsRealHero(Hero hero)
    {
        var repository = Services.GetService<IGameEntityRepositoryService>();
        IGameEntity found;
        return repository != null && repository.TryGetValue(hero.GUID, out found) && ReferenceEquals(found, hero);
    }

    /// <summary>End hook of Hero.ComputeAvailableSkills: detects the ultimate and, once taken, offers no more skills.</summary>
    public static void AfterComputeAvailable(Hero hero)
    {
        try
        {
            if (!Fixable(hero)) return;
            ulong id = (ulong)hero.GUID;
            if (!ultimates.ContainsKey(id))
            {
                if (!HoldsUltimate(hero) || !IsRealHero(hero)) return;
                ultimates[id] = Math.Max(1, hero.Level);
                try { CouncilUi.Toast(CouncilGame.Tr("Герой " + hero.LocalizedName + " выбрал ультимативный навык: специализация закреплена на уровне " + ultimates[id] + ".",
                                                     "Hero " + hero.LocalizedName + " took the ultimate skill: specialization fixed at level " + ultimates[id] + ".")); } catch (Exception) { }
            }
            hero.availableSkillDefinitionNames.Clear();
        }
        catch (Exception e) { Fail("AfterComputeAvailable", e); }
    }

    // ---- scaled copies of the descriptors of the learned skills

    static List<SimulationDescriptor> WithAutoBonus(List<SimulationDescriptor> list, Hero hero)
    {
        if (!Fixable(hero)) return list;
        int ult;
        if (!ultimates.TryGetValue((ulong)hero.GUID, out ult)) return list;
        float fraction = HeroKinds.TotalBonusFraction(ult, hero.Level, IsDictator(hero));
        if (fraction <= 0f) return list;
        var result = new List<SimulationDescriptor>(list);
        int percent = (int)Math.Round(fraction * 1000f);
        for (int i = 0; i < list.Count; i++)
        {
            var copy = ScaledCopy(list[i], fraction, percent);
            if (copy != null) result.Add(copy);
        }
        return result;
    }

    static SimulationDescriptor ScaledCopy(SimulationDescriptor source, float fraction, int key)
    {
        if (source == null || source.SimulationModifierDescriptors == null || source.SimulationModifierDescriptors.Length == 0) return null;
        string cacheKey = source.Name.ToString() + "|" + key;
        SimulationDescriptor cached;
        if (bonusCache.TryGetValue(cacheKey, out cached)) return cached;
        var mods = new List<SimulationModifierDescriptor>();
        foreach (var m in source.SimulationModifierDescriptors)
        {
            var scaled = ScaledModifier(m, fraction);
            if (scaled != null) mods.Add(scaled);
        }
        SimulationDescriptor result = null;
        if (mods.Count > 0)
            result = CustomHackingHelper.CreateAndRegisterDescriptor(null, source.Name.ToString() + "@HR" + key + "_" + (++descriptorSeq), source.Type.ToString(), null, mods.ToArray());
        bonusCache[cacheKey] = result;
        return result;
    }

    // fields of the Amplitude simulation classes are private and their assembly is not publicized: reflection, like CustomHackingHelper does
    static void SetField(object target, string name, object value)
    {
        for (Type t = target.GetType(); t != null; t = t.BaseType)
        {
            FieldInfo f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (f != null) { f.SetValue(target, value); return; }
        }
        throw new MissingFieldException(target.GetType().Name, name);
    }

    /// <summary>The clone gets the id a freshly constructed modifier received, so the simulation never sees two modifiers with one id.</summary>
    static void FreshId(SimulationModifierDescriptor clone, SimulationModifierDescriptor fresh)
    {
        FieldInfo f = typeof(SimulationModifierDescriptor).GetField("<ModifierId>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        f.SetValue(clone, f.GetValue(fresh));
    }

    static SimulationModifierDescriptor ScaledModifier(SimulationModifierDescriptor m, float fraction)
    {
        if (m == null) return null;
        float scaled;
        var single = m as SingleSimulationModifierDescriptor;
        if (single != null)
        {
            if (!single.IsValueFloat || !HeroKinds.ScaleModifierValue((int)single.Operation, single.ValueAsFloat, fraction, out scaled)) return null;
            var copy = (SingleSimulationModifierDescriptor)memberwiseClone.Invoke(single, null);
            SetField(copy, "value", scaled);
            FreshId(copy, new SingleSimulationModifierDescriptor());
            return copy;
        }
        var count = m as CountSimulationModifierDescriptor;
        if (count != null)
        {
            if (!count.IsValueFloat || !HeroKinds.ScaleModifierValue((int)count.Operation, count.ValueAsFloat, fraction, out scaled)) return null;
            var copy = (CountSimulationModifierDescriptor)memberwiseClone.Invoke(count, null);
            SetField(copy, "value", scaled);
            FreshId(copy, new CountSimulationModifierDescriptor());
            return copy;
        }
        var binary = m as BinarySimulationModifierDescriptor;
        if (binary != null)
        {
            // property * constant: scale the constant; the operation of the whole modifier decides how
            bool rightIsConstant = binary.IsRightValueFloat, leftIsConstant = binary.IsLeftValueFloat;
            if (rightIsConstant == leftIsConstant || (int)binary.BinaryOperation != 3) return null;
            int outer = (int)binary.Operation;
            if (outer != 1 && outer != 2 && outer != 6) return null;
            if (!HeroKinds.ScaleModifierValue((int)binary.Operation, rightIsConstant ? binary.RightValueAsFloat : binary.LeftValueAsFloat, fraction, out scaled)) return null;
            var copy = (BinarySimulationModifierDescriptor)memberwiseClone.Invoke(binary, null);
            SetField(copy, rightIsConstant ? "rightValue" : "leftValue", scaled);
            FreshId(copy, new BinarySimulationModifierDescriptor());
            return copy;
        }
        return null;
    }

    // ---------------------------------------------------------------- per turn

    static readonly Dictionary<ulong, bool> dictatorState = new Dictionary<ulong, bool>();

    /// <summary>A hero who became (or stopped being) the dictator has other skill trees: recompute what he can learn.</summary>
    static void RefreshOnRoleChange(Hero hero)
    {
        try
        {
            if (!IsRegular(hero)) return;
            ulong id = (ulong)hero.GUID;
            bool now = IsDictator(hero), was;
            dictatorState.TryGetValue(id, out was);
            if (now == was) return;
            dictatorState[id] = now;
            hero.ComputeAvailableSkills();
            var skills = hero.UnlockedSkills;
            if (skills != null && skills.Count > 0) hero.OnSkillsUpdated(CollectionChangeAction.Refresh, skills[0]);
        }
        catch (Exception e) { Fail("RefreshOnRoleChange", e); }
    }

    /// <summary>Turn pass: forget state of a finished game, refresh the effects of fixed heroes whose level changed.</summary>
    public static void PerTurn(Game game)
    {
        try
        {
            if (!Enabled || game == null) return;
            int turn = game.Turn;
            if (turn < lastTurn) { ultimates.Clear(); appliedSteps.Clear(); bonusCache.Clear(); ultimateSets.Clear(); dictatorState.Clear(); ClearDictators(); ResetSpecials(); }
            if (turn == lastTurn) return;   // ClientBeginTurn fires per controller; never redo a turn
            lastTurn = turn;
            RefreshEuphoria(game);
            UpdateDictatorships(game);
            bool xpDue = turn != passiveXpTurn;
            passiveXpTurn = turn;
            foreach (MajorEmpire empire in game.EnumerableMajorEmpires)
            {
                var education = empire.GetAgency<DepartmentOfEducation>();
                if (education == null) continue;
                var heroes = education.ActiveHeroes;
                for (int i = 0; i < heroes.Count; i++)
                {
                    Hero hero = heroes[i];
                    if (xpDue && IsRegular(hero) && KindOf(hero) == HeroKind.Senator) GrantPassiveExperience(hero);
                    RefreshOnRoleChange(hero);
                    TryMothership(hero);
                    if (!Fixable(hero)) continue;
                    AfterComputeAvailable(hero);
                    ulong id = (ulong)hero.GUID;
                    int ult;
                    if (!ultimates.TryGetValue(id, out ult)) continue;
                    int steps = Math.Max(0, hero.Level - ult) * 2 + (IsDictator(hero) ? 1 : 0);
                    int before;
                    appliedSteps.TryGetValue(id, out before);
                    if (steps == before) continue;
                    appliedSteps[id] = steps;
                    var skills = hero.UnlockedSkills;
                    if (skills != null && skills.Count > 0) hero.OnSkillsUpdated(CollectionChangeAction.Refresh, skills[0]);
                }
            }
        }
        catch (Exception e) { Fail("PerTurn", e); }
    }
}
