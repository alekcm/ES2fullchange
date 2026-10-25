// Hero rework: the special ultimates of the admirals that no simulation property can express.
//  * Salvage (Dust / science / influence for every destroyed enemy CP) - Industrialists, Sophons, Religious;
//  * Taking Captives (Cravers): after a victory one citizen of the defeated faction appears in the capital;
//  * Triumph (Terrans): after a victory the happiness of the whole empire rises for a while, the longer the more CP were destroyed.
// The hook is a postfix of GameClient.ApplyDamage(Encounter, damage, attacker, ...) which every client runs for an automatically resolved
// ("simplified") battle: destroyedShips are the ships of the damaged side, the attacker is the empire that dealt the damage. Manual
// (tactical) battles do not go through this method and are NOT covered.
using System;
using System.Collections.Generic;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Simulation;
using Amplitude.Unity.Simulation.SimulationModifierDescriptors;

public static partial class HeroRework
{
    public const int EuphoriaBaseTurns = 5, EuphoriaCpPerTurn = 10, EuphoriaMaxTurns = 40;

    // empire index -> { last turn of the effect, happiness }
    static readonly Dictionary<int, int[]> euphoria = new Dictionary<int, int[]>();
    static string euphoriaSignature = "";
    static readonly List<SimulationDescriptor> euphoriaDescriptors = new List<SimulationDescriptor>();
    static readonly List<ColonizedStarSystem> euphoriaTargets = new List<ColonizedStarSystem>();
    static int euphoriaSeq;

    // ---------------------------------------------------------------- battle hook

    /// <summary>What the admirals of a group win in one battle (pure data, tested without the game).</summary>
    public sealed class SpecialReward
    {
        public int Dust, Science, Influence, Prisoners, Happiness;
        public bool Any { get { return Dust > 0 || Science > 0 || Influence > 0 || Prisoners > 0 || Happiness > 0; } }
    }

    /// <summary>Adds the rewards of one hero skill (level, 1-based) for a battle in which destroyedCp command points were destroyed.</summary>
    public static void AddSkillReward(SpecialReward reward, DesignSkill skill, int level, int destroyedCp)
    {
        if (skill == null || reward == null || destroyedCp <= 0) return;
        reward.Dust += HeroDesign.AmountOf(skill, level, "A.LootDust") * destroyedCp;
        reward.Science += HeroDesign.AmountOf(skill, level, "A.LootSci") * destroyedCp;
        reward.Influence += HeroDesign.AmountOf(skill, level, "A.LootInfl") * destroyedCp;
        reward.Prisoners += HeroDesign.AmountOf(skill, level, "A.Prisoner");
        reward.Happiness = Math.Max(reward.Happiness, HeroDesign.AmountOf(skill, level, "A.Euphoria"));
    }

    /// <summary>Turns of the Triumph effect for the destroyed CP.</summary>
    public static int EuphoriaTurns(int destroyedCp)
    {
        return Math.Min(EuphoriaMaxTurns, EuphoriaBaseTurns + Math.Max(0, destroyedCp) / EuphoriaCpPerTurn);
    }

    /// <summary>Hook: end of GameClient.ApplyDamage (simplified battle), once per damaged side.</summary>
    public static void OnShipsDestroyed(Encounter encounter, Empire attacker, ref List<Ship> destroyed)
    {
        try
        {
            if (!Enabled || encounter == null || attacker == null || destroyed == null || destroyed.Count == 0) return;
            var winner = attacker as MajorEmpire;
            if (winner == null) return;

            int cp = 0;
            for (int i = 0; i < destroyed.Count; i++)
            {
                int one = 1;
                try { one = Math.Max(1, destroyed[i].CommandPoints); } catch (Exception) { }
                cp += one;
            }

            EncounterGroup mine = null;
            Empire victim = null;
            var groups = encounter.Groups;
            for (int i = 0; groups != null && i < groups.Length; i++)
            {
                EncounterGroup g = groups[i];
                if (g == null || g.Leader == null || g.Leader.Empire == null) continue;
                if (g.Leader.Empire.Index == attacker.Index) { if (mine == null) mine = g; }
                else if (victim == null) victim = g.Leader.Empire;
            }
            if (mine == null) return;

            // the admirals who fought: heroes assigned to the fleets of the group
            var seen = new HashSet<ulong>();
            var reward = new SpecialReward();
            Hero credited = null;
            var ships = mine.GetInitialSetupShips();
            for (int i = 0; ships != null && i < ships.Count; i++)
            {
                Fleet fleet = ships[i] == null ? null : ships[i].Garrison as Fleet;
                Hero hero = fleet == null ? null : fleet.AssignedHero;
                if (hero == null || !seen.Add((ulong)hero.GUID)) continue;
                var skills = hero.UnlockedSkills;
                if (skills == null) continue;
                var own = new SpecialReward();
                for (int s = 0; s < skills.Count; s++)
                {
                    HeroSkill sk = skills[s];
                    if (sk == null || sk.Definition == null) continue;
                    AddSkillReward(own, HeroDesign.FindSkill(sk.Definition.Name.ToString()), sk.Level, cp);
                }
                if (!own.Any) continue;
                reward.Dust += own.Dust; reward.Science += own.Science; reward.Influence += own.Influence;
                reward.Prisoners += own.Prisoners; reward.Happiness = Math.Max(reward.Happiness, own.Happiness);
                if (credited == null) credited = hero;
            }
            if (!reward.Any) return;
            GrantSpecialReward(winner, victim as MajorEmpire, reward, cp, credited);
        }
        catch (Exception e) { Fail("OnShipsDestroyed", e); }
    }

    static void GrantSpecialReward(MajorEmpire winner, MajorEmpire victim, SpecialReward reward, int cp, Hero hero)
    {
        var lines = new List<string>();
        var treasury = winner.GetAgency<DepartmentOfTheTreasury>();
        if (treasury != null && winner.SimulationObject != null)
        {
            if (reward.Dust > 0 && treasury.TryTransferResources(winner.SimulationObject, DepartmentOfTheTreasury.Resources.EmpireMoney, reward.Dust, false))
                lines.Add(CouncilGame.Tr("+" + reward.Dust + " Пыли", "+" + reward.Dust + " Dust"));
            if (reward.Science > 0 && treasury.TryTransferResources(winner.SimulationObject, DepartmentOfTheTreasury.Resources.EmpireResearch, reward.Science, false))
                lines.Add(CouncilGame.Tr("+" + reward.Science + " науки", "+" + reward.Science + " science"));
            if (reward.Influence > 0 && treasury.TryTransferResources(winner.SimulationObject, DepartmentOfTheTreasury.Resources.EmpireEmpirePoint, reward.Influence, false))
                lines.Add(CouncilGame.Tr("+" + reward.Influence + " влияния", "+" + reward.Influence + " influence"));
        }
        if (reward.Prisoners > 0 && victim != null && TakeCaptive(winner, victim, reward.Prisoners))
            lines.Add(CouncilGame.Tr("пленник из фракции «" + CouncilGame.EmpireName(victim.Index) + "» в столице", "a captive of \"" + CouncilGame.EmpireName(victim.Index) + "\" in the capital"));
        if (reward.Happiness > 0)
        {
            int turns = EuphoriaTurns(cp);
            int now = CouncilGame.CurTurn();
            int[] cur;
            int last = now + turns;
            if (euphoria.TryGetValue(winner.Index, out cur) && cur[0] >= now) last = Math.Min(now + EuphoriaMaxTurns, cur[0] + turns);
            euphoria[winner.Index] = new[] { last, Math.Max(reward.Happiness, cur == null ? 0 : cur[1]) };
            RefreshEuphoria(CouncilGame.GetGame());
            lines.Add(CouncilGame.Tr("триумф: +" + reward.Happiness + " счастья до хода " + last, "triumph: +" + reward.Happiness + " happiness until turn " + last));
        }
        var local = CouncilGame.GetMajor(0);
        if (lines.Count > 0 && local != null && local.Index == winner.Index)
            CouncilUi.Toast(CouncilGame.Tr("Победа" + (hero == null ? "" : " адмирала «" + hero.LocalizedName + "»") + " (уничтожено КО: " + cp + "): " + string.Join("; ", lines.ToArray()),
                                           "Victory" + (hero == null ? "" : " of admiral \"" + hero.LocalizedName + "\"") + " (CP destroyed: " + cp + "): " + string.Join("; ", lines.ToArray())));
    }

    // ---------------------------------------------------------------- captives

    static bool TakeCaptive(MajorEmpire winner, MajorEmpire victim, int count)
    {
        try
        {
            if (victim.Faction == null) return false;
            string affinity = victim.Faction.GetMajorPopulationAffinityName();
            if (string.IsNullOrEmpty(affinity)) return false;
            var interior = winner.GetAgency<DepartmentOfTheInterior>();
            if (interior == null || interior.ColonizedStarSystems == null) return false;
            // the capital first, then any other system with room
            ColonizedStarSystem home = null;
            foreach (ColonizedStarSystem cs in interior.ColonizedStarSystems) if (cs != null && cs.IsHomeSystemOfCurrentOwner) { home = cs; break; }
            if (home != null && interior.AddPopulationOnStarSystem(home, new StaticString(affinity), count, null, true, false, null)) return true;
            foreach (ColonizedStarSystem cs in interior.ColonizedStarSystems)
            {
                if (cs == null || ReferenceEquals(cs, home)) continue;
                if (interior.AddPopulationOnStarSystem(cs, new StaticString(affinity), count, null, true, false, null)) return true;
            }
        }
        catch (Exception e) { Fail("TakeCaptive", e); }
        return false;
    }

    // ---------------------------------------------------------------- triumph (temporary happiness)

    static SimulationDescriptor MakeEuphoriaDescriptor(int happiness)
    {
        var mod = CustomHackingHelper.MakeModifier("Happiness", SimulationModifierDescriptor.ModifierOperation.Addition, happiness, "ClassStarSystem");
        return CustomHackingHelper.CreateAndRegisterDescriptor(null, "HeroTriumph_" + (++euphoriaSeq), "HeroTriumph", null, new[] { mod });
    }

    static void ClearEuphoria()
    {
        for (int i = 0; i < euphoriaDescriptors.Count; i++)
        {
            try { euphoriaTargets[i].SimulationObject.RemoveDescriptor(euphoriaDescriptors[i]); euphoriaTargets[i].Refresh(false); } catch (Exception) { }
        }
        euphoriaDescriptors.Clear();
        euphoriaTargets.Clear();
        euphoriaSignature = "";
    }

    /// <summary>Applies / removes the temporary happiness descriptors; called every turn and after a victory.</summary>
    public static void RefreshEuphoria(Game game)
    {
        try
        {
            if (game == null) return;
            int now = CouncilGame.CurTurn();
            var gone = new List<int>();
            foreach (var kv in euphoria) if (kv.Value[0] < now) gone.Add(kv.Key);
            for (int i = 0; i < gone.Count; i++) euphoria.Remove(gone[i]);
            if (euphoria.Count == 0)
            {
                if (euphoriaDescriptors.Count > 0) ClearEuphoria();
                euphoriaSignature = "";
                return;
            }

            var wanted = new List<ColonizedStarSystem>();
            var strength = new List<int>();
            var sb = new System.Text.StringBuilder();
            foreach (var kv in euphoria)
            {
                var emp = CouncilGame.GetMajor(kv.Key);
                if (emp == null || emp.HasBeenEliminated) continue;
                var interior = emp.GetAgency<DepartmentOfTheInterior>();
                if (interior == null || interior.ColonizedStarSystems == null) continue;
                foreach (ColonizedStarSystem cs in interior.ColonizedStarSystems)
                {
                    if (cs == null || cs.SimulationObject == null) continue;
                    wanted.Add(cs); strength.Add(kv.Value[1]);
                    sb.Append(cs.GUID).Append(':').Append(kv.Value[1]).Append(',');
                }
            }
            string sig = sb.ToString();
            if (sig == euphoriaSignature) return;
            ClearEuphoria();
            euphoriaSignature = sig;
            var byStrength = new Dictionary<int, SimulationDescriptor>();
            for (int i = 0; i < wanted.Count; i++)
            {
                SimulationDescriptor d;
                if (!byStrength.TryGetValue(strength[i], out d)) byStrength[strength[i]] = d = MakeEuphoriaDescriptor(strength[i]);
                wanted[i].SimulationObject.AddDescriptor(d);
                euphoriaDescriptors.Add(d);
                euphoriaTargets.Add(wanted[i]);
                wanted[i].Refresh(false);
            }
        }
        catch (Exception e) { Fail("RefreshEuphoria", e); }
    }

    static void ResetSpecials()
    {
        euphoria.Clear();
        euphoriaDescriptors.Clear();
        euphoriaTargets.Clear();
        euphoriaSignature = "";
    }

    // ---------------------------------------------------------------- state of the triumph in saved games

    static string SerializeSpecials()
    {
        var sb = new System.Text.StringBuilder();
        var keys = new List<int>(euphoria.Keys);
        keys.Sort();
        for (int i = 0; i < keys.Count; i++)
        {
            if (sb.Length > 0) sb.Append(',');
            sb.Append(keys[i]).Append(':').Append(euphoria[keys[i]][0]).Append(':').Append(euphoria[keys[i]][1]);
        }
        return sb.ToString();
    }

    static void ParseSpecials(string text)
    {
        ResetSpecials();
        if (string.IsNullOrEmpty(text)) return;
        string[] parts = text.Split(',');
        for (int i = 0; i < parts.Length; i++)
        {
            string[] f = parts[i].Split(':');
            int idx, last, power;
            if (f.Length == 3 && int.TryParse(f[0], out idx) && int.TryParse(f[1], out last) && int.TryParse(f[2], out power)) euphoria[idx] = new[] { last, power };
        }
    }
}
