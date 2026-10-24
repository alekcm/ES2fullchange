// Hero rework - debug actions of the God-Mod menu (the window part is in CouncilDebug.cs).
// Level and skill changes are applied to the local simulation like the older God-Mod hero buttons (they are not sent as orders);
// recruiting goes through the normal OrderRecruitHero, so it is synchronized.
using System;
using System.Collections.Generic;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using System.ComponentModel;
using Amplitude.Unity.Simulation;

public static partial class HeroRework
{
    /// <summary>Raises the level of the hero by up to n levels using the normal experience path (the level-up rule and the senator cap still apply). Returns the gained levels.</summary>
    public static int DebugAddLevels(Hero hero, int n)
    {
        int start = hero.Level;
        for (int i = 0; i < n; i++)
        {
            float need = hero.NextLevelRequestedExperience - hero.Experience;
            if (need <= 0f) need = 1f;
            int before = hero.Level;
            hero.SetExperience(hero.Experience + need);
            if (hero.Level <= before) break;       // the level-up rule refuses (maximum level reached)
        }
        return hero.Level - start;
    }

    public static void DebugAddSkillPoints(Hero hero, int points)
    {
        float cur = hero.SimulationObject.GetPropertyBaseValue(SimulationProperties.Hero.SkillPoints);
        hero.SimulationObject.SetPropertyBaseValue(SimulationProperties.Hero.SkillPoints, cur + points);
        hero.Refresh(true);
    }

    /// <summary>Learns an ultimate skill (last stage of the trees of the hero) for free: this is the Mastery rank.</summary>
    public static string DebugGrantUltimate(Hero hero, int turn)
    {
        try
        {
            bool ru = CouncilGame.IsRussian();
            var set = UltimatesOf(hero);
            if (set.Count == 0) return ru ? "У этого героя нет ультимативного навыка" : "This hero has no ultimate skill";
            if (HoldsUltimate(hero)) return ru ? "Ультимативный навык уже изучен" : "The ultimate skill is already learned";
            HeroSkillDefinition pick = null;
            foreach (var d in set) { pick = d; break; }
            hero.UnlockSkill(pick, 1, true, false, turn);
            hero.ComputeAvailableSkills();
            var skills = hero.UnlockedSkills;
            if (skills != null && skills.Count > 0) hero.OnSkillsUpdated(CollectionChangeAction.Refresh, skills[0]);
            return (ru ? "Изучен ультимативный навык: " : "Ultimate skill learned: ") + pick.Name;
        }
        catch (Exception e) { Fail("DebugGrantUltimate", e); return "error: " + e.Message; }
    }

    public static bool DebugHasUltimateSkill(Hero hero)
    {
        try { return UltimatesOf(hero).Count > 0; } catch (Exception) { return false; }
    }
}
