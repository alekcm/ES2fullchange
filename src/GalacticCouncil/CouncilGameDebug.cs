// Galactic Council - debug actions of the God-Mod menu (see CouncilDebug.cs for the window part).
// Every action travels as OrderCouncilVote(KindDebug, A = action, B = parameter) and is executed by every client,
// so the council state stays identical on all machines.
using System;
using System.Collections.Generic;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Event;
using Amplitude.Unity.Game;

public static partial class CouncilGame
{
    public const int DbgFound = 0;            // found the council right now (ignores the Academy discovery rules)
    public const int DbgOpenVote = 1;         // open the vote right now (only when no vote is running)
    public const int DbgTally = 2;            // close the running vote right now (AIs vote first)
    public const int DbgSoon = 3;             // the next vote opens at the start of the next turn
    public const int DbgRecalcVotes = 4;      // re-read the votes of the game (systems, Republic, senators) and subtract the war penalty
    public const int DbgAddVotes = 5;         // B = change of the votes of the empire (session votes)
    public const int DbgAddWar = 6;           // register one more declared war of the empire
    public const int DbgClearWars = 7;        // forget the declared wars of the empire
    public const int DbgAlliance = 8;         // B = -1 automatic, 0 holder is alone, 1 holder is in an alliance
    public const int DbgGrantQualification = 9;   // B = index in CouncilLaws.Mandates: report every condition of the mandate
    public const int DbgClearQualifications = 10; // forget every reported condition of the empire
    public const int DbgDetectNow = 11;       // read the conditions from the game right now
    public const int DbgGiveMandate = 12;     // B = index in CouncilLaws.Mandates
    public const int DbgTakeMandate = 13;     // B = index in CouncilLaws.Mandates
    public const int DbgAiVote = 14;          // every AI member casts its ballot now
    public const int DbgClearVoteBonus = 15;  // forget the manual vote changes of the empire

    public const int DbgOpenAcademy = 16;     // discover the Academy for everybody (any turn) and convene the Council

    static AcademyRequestsManager lastMgr;
    public static int AllianceOverride = -1;
    /// <summary>The Academy manager is seen at the first turn start after a load; "found" and "open vote now" need it.</summary>
    public static bool ManagerKnown { get { return Mgr() != null; } }

    /// <summary>The manager seen at the last turn start, or (right after a load) the one registered as the game service.</summary>
    static AcademyRequestsManager Mgr()
    {
        if (lastMgr != null) return lastMgr;
        try { lastMgr = Services.GetService<IAcademyRequestsService>() as AcademyRequestsManager; } catch (Exception) { }
        return lastMgr;
    }

    public static bool DebugSend(Empire empire, int action, int param)
    {
        return SendKind(empire, OrderCouncilVote.KindDebug, action, param);
    }

    static void DebugNote(string ru, string en)
    {
        try { CouncilUi.Toast(Tr(ru, en)); } catch (Exception) { }
    }

    static void DebugProcess(OrderCouncilVote o)
    {
        try
        {
            int emp = o.EmpireIndex;
            int b = o.B;
            var game = GetGame();
            if (game == null) return;
            int turn = game.Turn;
            switch (o.A)
            {
                case DbgOpenAcademy:
                    DebugOpenAcademy(emp, game, turn);
                    break;
                case DbgFound:
                    if (Core.Enabled) { DebugNote("Совет уже создан", "The Council already exists"); break; }
                    if (Mgr() == null || Mgr().AcademyEmpire == null) { DebugNote("В этой партии нет Академии: Совет создать нельзя", "There is no Academy in this game: the Council cannot be created"); break; }
                    if (AcademyIsGone(Mgr())) { DebugNote("Академия уничтожена: Совет создать нельзя", "The Academy is eliminated: the Council cannot be created"); break; }
                    FoundCouncil(Mgr(), game, turn);
                    break;
                case DbgOpenVote:
                    if (!Core.Enabled) { DebugNote("Совет ещё не создан", "The Council does not exist yet"); break; }
                    if (Core.Phase != CouncilCore.PhaseIdle) { DebugNote("Голосование уже идёт", "A vote is already running"); break; }
                    if (Mgr() == null) { DebugNote("Менеджер Академии недоступен", "The Academy manager is not available"); break; }
                    DoOpen(Mgr(), game, turn);
                    break;
                case DbgTally:
                    if (Core.Enabled && Core.Phase == CouncilCore.PhaseVoting) { DoTally(game, turn); RefreshEffects(game); }
                    break;
                case DbgSoon:
                    if (Core.Enabled && Core.Phase == CouncilCore.PhaseIdle) Core.NextVoteTurn = turn + 1;
                    break;
                case DbgRecalcVotes:
                    if (Core.Enabled) ApplySessionVotes(game, turn);
                    break;
                case DbgAddVotes:
                    if (Core.Enabled && Core.IsMember(emp)) Core.SessionVotes[emp] = Core.SessionVotesOf(emp) + b;
                    break;
                case DbgClearVoteBonus:
                    if (Core.Enabled) { Core.SessionVotes.Remove(emp); Core.VoteBonus.Remove(emp); }
                    break;
                case DbgAddWar:
                    if (Core.Enabled && Core.IsMember(emp)) { Core.RecordWarDeclared(emp, turn); Core.SessionVotes[emp] = Core.SessionVotesOf(emp) - CouncilCore.WarPenaltyVotes; }
                    break;
                case DbgClearWars:
                    if (Core.Enabled)
                    {
                        int wars = Core.RecentWars(emp, turn);
                        Core.WarsDeclared.Remove(emp);
                        if (wars > 0) Core.SessionVotes[emp] = Core.SessionVotesOf(emp) + wars * CouncilCore.WarPenaltyVotes;
                    }
                    break;
                case DbgAlliance:
                    AllianceOverride = b;
                    RefreshAllianceFlag();
                    break;
                case DbgGrantQualification:
                    if (Core.Enabled && b >= 0 && b < CouncilLaws.Mandates.Count)
                    {
                        var d = CouncilLaws.Mandates[b];
                        if (d.Victory)
                        {
                            // the victory mandate needs two ordinary qualifications: grant the first ones that are missing
                            for (int i = 0; i < CouncilLaws.Mandates.Count; i++)
                                if (!CouncilLaws.Mandates[i].Victory && !Core.QualifiesOrdinary(emp, CouncilLaws.Mandates[i]) && Core.OrdinaryQualifiedCount(emp) < 2)
                                    GrantConditions(emp, CouncilLaws.Mandates[i]);
                        }
                        else GrantConditions(emp, d);
                    }
                    break;
                case DbgClearQualifications:
                    if (Core.Enabled) Core.Tasks.Remove(emp);
                    break;
                case DbgDetectNow:
                    if (Core.Enabled) DetectQualifications(game);
                    break;
                case DbgGiveMandate:
                    if (Core.Enabled && Core.IsMember(emp) && b >= 0 && b < CouncilLaws.Mandates.Count)
                    {
                        Core.SetHolder(CouncilLaws.Mandates[b].Id, emp, turn);
                        RefreshAllianceFlag();
                        RefreshEffects(game);
                    }
                    break;
                case DbgTakeMandate:
                    if (Core.Enabled && b >= 0 && b < CouncilLaws.Mandates.Count)
                    {
                        Core.RevokeMandate(CouncilLaws.Mandates[b].Id);
                        RefreshAllianceFlag();
                        RefreshEffects(game);
                    }
                    break;
                case DbgAiVote:
                    if (Core.Enabled && Core.Phase == CouncilCore.PhaseVoting)
                    {
                        foreach (MajorEmpire e in game.EnumerableMajorEmpires)
                        {
                            if (!Core.IsMember(e.Index) || !e.IsControlledByAI || Core.HasVoted(e.Index)) continue;
                            int aiSeed = unchecked(Core.VoteOpenTurn * 1103515245 + Core.VoteIndex * 12345 + e.Index * 131 + 17);
                            Core.TrySetBallot(e.Index, Core.BuildAiBallot(e.Index, aiSeed, ScoreOf));
                        }
                    }
                    break;
            }
        }
        catch (Exception e) { Fail("DebugProcess", e); }
    }

    /// <summary>Does what the first exploration of the Academy system does: the Academy is discovered (event for every empire), the first-request delay is removed, then the Council is convened.</summary>
    static void DebugOpenAcademy(int emp, Game game, int turn)
    {
        var me = GetMajor(emp);
        var mgr = Mgr();
        if (me == null || mgr == null || mgr.AcademyEmpire == null) { DebugNote("В этой партии нет Академии", "There is no Academy in this game"); return; }
        if (AcademyIsGone(mgr)) { DebugNote("Академия уничтожена", "The Academy is eliminated"); return; }
        try
        {
            var edu = me.GetAgency<DepartmentOfEducation>();
            var academy = edu == null ? null : edu.Academy;
            var events = Services.GetService<IEventService>();
            if (academy != null && events != null && academy.WorldAcademy != null)
            {
                if (!academy.HasBeenDiscovered)
                    foreach (MajorEmpire e in game.EnumerableMajorEmpires)
                        events.Notify(new EventAcademyDiscovered(e, me, academy.WorldAcademy));
            }
        }
        catch (Exception e) { Fail("DebugOpenAcademy event", e); }
        try
        {
            if (mgr.DiscoveredTurn == -1) mgr.DiscoveredTurn = Math.Max(0, turn);
            mgr.MinTurnsForFirstRequest = 0f;
        }
        catch (Exception e) { Fail("DebugOpenAcademy manager", e); }
        if (!Core.Enabled) FoundCouncil(mgr, game, turn);
        DebugNote("Академия открыта", "The Academy is open");
    }

    /// <summary>Reports as many different (fake) conditions as the mandate needs.</summary>
    static void GrantConditions(int emp, CouncilMandateDef d)
    {
        for (int i = 0; i < d.Need; i++) Core.ReportTask(emp, d.Categories[0], "debug" + i);
    }
}
