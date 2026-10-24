// Hero rework (stage 1): heroes are admirals, governors or senators.
//  * every effect of a hero skill is applied only through the channel of his kind (ship / system / senate);
//  * only skills that do something for the kind are offered in the skill trees of the hero;
//  * admirals command fleets only, governors govern systems only, only senators represent a party in the senate.
// AI ruler heroes of the Hacking Overhaul (CustomHackingHelper.IsAiHero) are never touched.
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Simulation;

public static class HeroRework
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
        return HeroKinds.KindOfGuid((ulong)hero.GUID);
    }

    // ---------------------------------------------------------------- effect channels (Hero.Get*SkillDescriptors, return hooks)

    static List<SimulationDescriptor> Filter(List<SimulationDescriptor> list, Hero hero, HeroChannel channel)
    {
        try
        {
            if (list == null || list.Count == 0 || !IsRegular(hero)) return list;
            if (HeroKinds.Allows(KindOf(hero), channel)) return list;
            return new List<SimulationDescriptor>();
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
                if (best == null || h.CompareTo_ForSenate(best) > 0) best = h;
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
            return title + "  [" + HeroKinds.Name(KindOf(gui.Hero), CouncilGame.IsRussian()) + "]";
        }
        catch (Exception) { return title; }
    }

    // ---------------------------------------------------------------- skill trees (Hero.get_SkillTrees, return hook)

    sealed class TreeCache
    {
        public HeroSkillTreeDefinition[] Original;
        public HeroSkillTreeDefinition[] Filtered;
    }

    static readonly Dictionary<ulong, TreeCache> treeCache = new Dictionary<ulong, TreeCache>();
    static readonly MethodInfo memberwiseClone = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

    static object CloneObject(object source)
    {
        return memberwiseClone.Invoke(source, null);
    }

    public static HeroSkillTreeDefinition[] TreesFor(HeroSkillTreeDefinition[] original, Hero hero)
    {
        try
        {
            if (original == null || original.Length == 0 || !IsRegular(hero)) return original;
            ulong id = (ulong)hero.GUID;
            TreeCache c;
            if (treeCache.TryGetValue(id, out c) && ReferenceEquals(c.Original, original)) return c.Filtered;
            c = new TreeCache { Original = original, Filtered = BuildTrees(original, KindOf(hero)) };
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
}
