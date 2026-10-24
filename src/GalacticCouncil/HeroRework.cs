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
            if (list == null || list.Count == 0 || !IsRegular(hero)) return list;
            if (!HeroKinds.Allows(KindOf(hero), channel)) return new List<SimulationDescriptor>();
            return WithAutoBonus(list, hero);
        }
        catch (Exception e) { Fail("Filter", e); return list; }
    }

    public static List<SimulationDescriptor> FilterShip(List<SimulationDescriptor> list, Hero hero) { return Filter(list, hero, HeroChannel.Ship); }
    public static List<SimulationDescriptor> FilterSystem(List<SimulationDescriptor> list, Hero hero) { return Filter(list, hero, HeroChannel.System); }
    public static List<SimulationDescriptor> FilterSenate(List<SimulationDescriptor> list, Hero hero) { return Filter(list, hero, HeroChannel.Senate); }

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
