// Galactic Council glue: votes read from the game (system levels, Republic, senator skills, wars) and the real
// qualification conditions of the mandates (explored galaxy, level 4 system, captured capital, heroes of the Mastery rank).
using System;
using System.Collections.Generic;
using UnityEngine;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Simulation;

public static partial class CouncilGame
{
    // ---------------------------------------------------------------- votes taken when a session opens

    /// <summary>Votes of every member from the game state: levels of its star systems, Republic, skills of its senators.</summary>
    static Dictionary<int, int> GameVotes(Game game)
    {
        var map = new Dictionary<int, int>();
        foreach (MajorEmpire e in game.EnumerableMajorEmpires)
        {
            if (e.HasBeenEliminated || !Core.IsMember(e.Index)) continue;
            int v = 0;
            try { v += SystemLevelVotes(e); } catch (Exception ex) { Fail("SystemLevelVotes", ex); }
            try { if (IsRepublic(e)) v += CouncilCore.RepublicVotes; } catch (Exception ex) { Fail("IsRepublic", ex); }
            try { v += HeroRework.CouncilVotesOf(e); } catch (Exception ex) { Fail("SenatorVotes", ex); }
            map[e.Index] = v;
        }
        return map;
    }

    /// <summary>Sum over the star systems: level 2 gives 5 votes, 3 gives 10, 4 gives 20 (not cumulative).</summary>
    public static int SystemLevelVotes(Empire e)
    {
        var interior = e.GetAgency<DepartmentOfTheInterior>();
        if (interior == null) return 0;
        int v = 0;
        foreach (ColonizedStarSystem cs in interior.ColonizedStarSystems)
            if (cs != null && cs.SimulationObject != null) v += CouncilCore.SystemLevelVotes(cs.Level);
        return v;
    }

    public static bool IsRepublic(Empire e)
    {
        var domestic = e.GetAgency<DepartmentOfDomesticAffairs>();
        var senate = domestic == null ? null : domestic.Senate;
        if (senate == null || senate.Government == null || senate.Government.Definition == null) return false;
        return senate.Government.Definition.Name.ToString().IndexOf("Republic", StringComparison.Ordinal) >= 0;
    }

    /// <summary>Called when a session opens (before the proposals are built).</summary>
    static void ApplySessionVotes(Game game, int turn)
    {
        Core.ApplySessionVotes(GameVotes(game), turn);
    }

    // ---------------------------------------------------------------- alliance of the victory-mandate holder

    /// <summary>The holder of the victory mandate needs 67% instead of 51% to stay when he is a member of an alliance (two or more empires).</summary>
    static void RefreshAllianceFlag()
    {
        bool inAlliance = false;
        try
        {
            int holder = Core.SupremeHolder;
            var emp = holder >= 0 ? GetMajor(holder) : null;
            var repo = emp == null ? null : Services.GetService<IAllianceRepositoryService>();
            if (repo != null)
            {
                Alliance a = repo.FindAllianceOf(emp);
                inAlliance = a != null && a.EmpireCount >= 2;
            }
        }
        catch (Exception e) { Fail("RefreshAllianceFlag", e); }
        Core.SupremeInAlliance = inAlliance;
    }

    // ---------------------------------------------------------------- war declarations

    /// <summary>Hook: GameClient.CreateWarInstanceProcessor, right after the war instance has been created.</summary>
    public static void OnWarCreated(WarInstance war)
    {
        try
        {
            if (!Core.Enabled || war == null || war.AttackerGroup == null) return;
            WarEmpire declarer = war.AttackerGroup.FirstWarEmpire;       // the empire that declared the war; allies are not punished
            if (declarer == null || !Core.IsMember(declarer.EmpireIndex)) return;
            Core.RecordWarDeclared(declarer.EmpireIndex, CurTurn());
            var local = LocalEmpire();
            if (local != null && local.Index == declarer.EmpireIndex)
                CouncilUi.Toast(Tr("Объявлена война: −" + CouncilCore.WarPenaltyVotes + " голосов в Совете на " + Core.IntervalTurns + " ходов",
                                   "War declared: -" + CouncilCore.WarPenaltyVotes + " Council votes for " + Core.IntervalTurns + " turns"));
        }
        catch (Exception e) { Fail("OnWarCreated", e); }
    }

    // ---------------------------------------------------------------- qualification of the mandates

    static void DetectQualifications(Game game)
    {
        if (!Core.Enabled) return;
        foreach (MajorEmpire e in game.EnumerableMajorEmpires)
        {
            if (e.HasBeenEliminated || !Core.IsMember(e.Index)) continue;
            try { DetectFor(e); } catch (Exception ex) { Fail("DetectQualifications", ex); }
        }
    }

    static void Achieved(MajorEmpire e, string category, string id, string mandateId)
    {
        if (!Core.ReportTask(e.Index, category, id)) return;
        var local = LocalEmpire();
        if (local == null || local.Index != e.Index) return;
        var d = CouncilLaws.GetMandate(mandateId);
        if (d == null) return;
        if (Core.TaskCount(e.Index, d.Categories) < d.Need)
        {
            CouncilUi.Toast(Tr("Мандат «" + d.Ru + "»: ", "Mandate \"" + d.En + "\": ") + Core.TaskCount(e.Index, d.Categories) + "/" + d.Need);
            return;
        }
        CouncilUi.Toast(Tr("Выполнено условие мандата «" + d.Ru + "»", "Mandate requirement met: \"" + d.En + "\""));
    }

    static void DetectFor(MajorEmpire e)
    {
        var interior = e.GetAgency<DepartmentOfTheInterior>();
        if (interior == null) return;
        int emp = e.Index;

        // Eye of the Academy: 75% of the galaxy discovered
        if (!Core.HasTask(emp, CouncilLaws.CatExplore, "75") && ExploredPercent(interior) >= CouncilLaws.ExplorePercent)
            Achieved(e, CouncilLaws.CatExplore, "75", CouncilLaws.Eye);

        bool needLevel = !Core.HasTask(emp, CouncilLaws.CatSystemLevel, "L4");
        bool needCapital = !Core.HasTask(emp, CouncilLaws.CatCapital, "any");
        if (needLevel || needCapital)
            foreach (ColonizedStarSystem cs in interior.ColonizedStarSystems)
            {
                if (cs == null || cs.SimulationObject == null) continue;
                // Quartermaster: a star system of level 4
                if (needLevel && cs.Level >= CouncilLaws.SystemLevelNeeded) { needLevel = false; Achieved(e, CouncilLaws.CatSystemLevel, "L4", CouncilLaws.Quartermaster); }
                // Marshal: the capital of another player is in our hands
                if (needCapital && CapturedCapitalOf(cs, emp) >= 0) { needCapital = false; Achieved(e, CouncilLaws.CatCapital, "any", CouncilLaws.Marshal); }
            }

        // Chairman: heroes of the Mastery rank (took the ultimate skill; they only grow the skills they have)
        var education = e.GetAgency<DepartmentOfEducation>();
        if (education != null && education.ActiveHeroes != null)
            for (int i = 0; i < education.ActiveHeroes.Count; i++)
            {
                Hero h = education.ActiveHeroes[i];
                if (h == null || !HeroRework.IsFixed(h)) continue;
                Achieved(e, CouncilLaws.CatMastery, "H" + (ulong)h.GUID, CouncilLaws.Chairman);
            }
    }

    /// <summary>Index of the player whose capital this star system is when it belongs to somebody else, otherwise -1.</summary>
    static int CapturedCapitalOf(ColonizedStarSystem cs, int owner)
    {
        StarSystemNode node = cs.Node;
        if (node == null || !node.IsHomeSystem) return -1;
        int home = node.HomeSystemEmpireIndex;
        if (home == owner || home < 0 || GetMajor(home) == null) return -1;
        return home;
    }

    /// <summary>Percent of the galaxy nodes the empire has identified (the same count the game uses for its discovery milestones).</summary>
    public static float ExploredPercent(DepartmentOfTheInterior interior)
    {
        return interior.GetNodesExplorationProgressRatio(EntityExploration.State.Identified) * 100f;
    }

    // ---------------------------------------------------------------- progress text for the UI (cached: IMGUI asks every frame)

    static float progressStamp = -10f;
    static int progressEmp = -1;
    static float progressExplored;
    static int progressLevel;

    static void RefreshProgress(int emp)
    {
        if (progressEmp == emp && Time.realtimeSinceStartup - progressStamp < 2f) return;
        progressEmp = emp; progressStamp = Time.realtimeSinceStartup;
        progressExplored = 0f; progressLevel = 1;
        try
        {
            var e = GetMajor(emp);
            var interior = e == null ? null : e.GetAgency<DepartmentOfTheInterior>();
            if (interior == null) return;
            progressExplored = ExploredPercent(interior);
            foreach (ColonizedStarSystem cs in interior.ColonizedStarSystems)
                if (cs != null && cs.SimulationObject != null) progressLevel = Math.Max(progressLevel, cs.Level);
        }
        catch (Exception ex) { Fail("RefreshProgress", ex); }
    }

    /// <summary>Progress of a mandate requirement of an empire, for example "62% / 75%".</summary>
    public static string ProgressText(int emp, CouncilMandateDef d)
    {
        if (d.Victory) return "";
        int have = Core.TaskCount(emp, d.Categories);
        string tail = have + "/" + d.Need;
        if (have >= d.Need) return tail;
        RefreshProgress(emp);
        if (d.Id == CouncilLaws.Eye) return Mathf.FloorToInt(progressExplored) + "% / " + CouncilLaws.ExplorePercent + "%";
        if (d.Id == CouncilLaws.Quartermaster) return Tr("лучшая система: уровень ", "best system: level ") + progressLevel + " / " + CouncilLaws.SystemLevelNeeded;
        return tail;
    }
}
