// Galactic Council of the Academy - glue between CouncilCore and the game.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Amplitude;
using Amplitude.Serialization;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Simulation;

public static partial class CouncilGame
{
    public static CouncilCore Core = new CouncilCore();
    public static string LastError = "";

    static readonly List<SimulationDescriptor> appliedDescriptors = new List<SimulationDescriptor>();
    static readonly List<SimulationObject> appliedTargets = new List<SimulationObject>();
    static string appliedSignature = "";
    static int lastHandledTurn = int.MinValue;

    // ---------------------------------------------------------------- helpers

    static void Fail(string where, Exception e)
    {
        LastError = where + ": " + e.GetType().Name + ": " + e.Message;
        try { Debug.LogWarning("[GalacticCouncil] " + LastError); } catch (Exception) { }
    }

    public static Game GetGame()
    {
        try
        {
            var svc = Services.GetService<IGameService>();
            return svc != null ? svc.Game as Game : null;
        }
        catch (Exception) { return null; }
    }

    public static Empire LocalEmpire()
    {
        try { return CustomHackingHelper.GetSafePlayerEmpire(); } catch (Exception) { return null; }
    }

    public static MajorEmpire GetMajor(int index)
    {
        var g = GetGame();
        if (g == null) return null;
        foreach (MajorEmpire e in g.EnumerableMajorEmpires) if (e.Index == index) return e;
        return null;
    }

    public static string EmpireName(int index)
    {
        try
        {
            var e = GetMajor(index);
            if (e == null) return "#" + index;
            string n = e.LocalizedName;
            return string.IsNullOrEmpty(n) ? ("#" + index) : n;
        }
        catch (Exception) { return "#" + index; }
    }

    /// <summary>Tests without the game engine set this (0 English, 1 Russian): the Unity call below cannot run outside the game.</summary>
    public static int LanguageOverride = -1;

    public static bool IsRussian()
    {
        if (LanguageOverride >= 0) return LanguageOverride == 1;
        return SystemIsRussian();
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    static bool SystemIsRussian()
    {
        try { return UnityEngine.Application.systemLanguage == SystemLanguage.Russian || UnityEngine.Application.systemLanguage == SystemLanguage.Ukrainian || UnityEngine.Application.systemLanguage == SystemLanguage.Belarusian; }
        catch (Exception) { return false; }
    }

    public static string LawName(CouncilLawDef d) { return IsRussian() ? d.Ru : d.En; }
    public static string LawDesc(CouncilLawDef d) { return IsRussian() ? d.RuDesc : d.EnDesc; }
    public static string Tr(string ru, string en) { return IsRussian() ? ru : en; }

    /// <summary>Game stage 1..6 (used by laws whose strength scales with the stage of the game).</summary>
    public static int GameStage(Game game, Empire anyEmpire)
    {
        int step = 40;
        try { step = Math.Max(1, CustomHackingHelper.ScaleTurns(40, anyEmpire)); } catch (Exception) { }
        int st = 1 + (game != null ? game.Turn : 0) / step;
        return Math.Max(1, Math.Min(6, st));
    }

    // ---------------------------------------------------------------- persistence (hooked into AcademyRequestsManager.SerializeAttributes)

    public static void SerializeAttributes(IAttributesSerializer serializer, IVersionProvider versionProvider)
    {
        try
        {
            if (serializer == null || versionProvider == null) return;
            if (versionProvider.GetVersionNumber() < 7)
            {
                if (serializer.Mode == Serializer.SerializationMode.Read) ResetRuntime(new CouncilCore());
                return;
            }
            string current = "";
            if (serializer.Mode == Serializer.SerializationMode.Write) current = Core.Serialize() + HeroSeparator + HeroRework.Serialize();
            string loaded = serializer.SerializeAttribute<string>("CouncilState", current ?? "");
            if (serializer.Mode == Serializer.SerializationMode.Read)
            {
                string heroPart = "";
                int cut = loaded == null ? -1 : loaded.IndexOf(HeroSeparator, StringComparison.Ordinal);
                if (cut >= 0) { heroPart = loaded.Substring(cut + HeroSeparator.Length); loaded = loaded.Substring(0, cut); }
                ResetRuntime(CouncilCore.Deserialize(loaded));
                HeroRework.Deserialize(heroPart);
            }
        }
        catch (Exception e) { Fail("SerializeAttributes", e); }
    }

    const string HeroSeparator = "@@HR@@";

    static void ResetRuntime(CouncilCore fresh)
    {
        Core = fresh;
        appliedDescriptors.Clear();
        appliedTargets.Clear();
        appliedSignature = "";
        lastHandledTurn = int.MinValue;
    }

    // ---------------------------------------------------------------- turn hook (replaces AcademyRequestsManager.ClientBeginTurn)

    public static void ClientBeginTurn(AcademyRequestsManager mgr)
    {
        try
        {
            if (mgr == null || mgr.AcademyEmpire == null) return;
            lastMgr = mgr;
            var game = GetGame();
            if (game == null) return;
            int turn = game.Turn;
            CouncilUi.EnsureHost();
            HeroRework.PerTurn(game);
            EnsureVoteTexts();

            if (Core.Enabled && AcademyIsGone(mgr))
            {
                DisbandCouncil(game, turn);
                return;
            }
            if (!Core.Enabled && !Core.Destroyed)
            {
                if (mgr.DiscoveredTurn != -1 && turn > mgr.DiscoveredTurn && turn >= mgr.MinTurnsForFirstRequest)
                    FoundCouncil(mgr, game, turn);
            }
            if (!Core.Enabled) return;

            if (turn == lastHandledTurn) return;     // never process a turn twice
            lastHandledTurn = turn;

            PruneMembers(game);
            RefreshAllianceFlag();
            DetectQualifications(game);
            if (Core.NextAction(turn) == CouncilCore.ActionTally) DoTally(game, turn);
            if (Core.NextAction(turn) == CouncilCore.ActionOpen) DoOpen(mgr, game, turn);
            try { AiMandateActions(game, turn); } catch (Exception ex) { Fail("AiMandateActions", ex); }
            PerTurn(game, turn);
            RefreshEffects(game);
        }
        catch (Exception e) { Fail("ClientBeginTurn", e); }
    }

    static bool AcademyIsGone(AcademyRequestsManager mgr)
    {
        try
        {
            var a = mgr.AcademyEmpire;
            if (a == null) return true;
            return a.HasBeenEliminated;
        }
        catch (Exception) { return false; }
    }

    static void FoundCouncil(AcademyRequestsManager mgr, Game game, int turn)
    {
        var members = new List<int>();
        Empire any = null;
        foreach (MajorEmpire e in game.EnumerableMajorEmpires)
        {
            if (e.HasBeenEliminated) continue;
            members.Add(e.Index);
            if (any == null) any = e;
        }
        if (members.Count == 0) return;
        int interval = CouncilCore.BaseIntervalTurns;
        try { interval = CustomHackingHelper.ScaleTurns(CouncilCore.BaseIntervalTurns, any); } catch (Exception) { }
        Core = new CouncilCore();
        Core.Found(turn, members, interval);
        // the old resource-demand cycle is replaced by the council
        try { mgr.CurrentState = RequestState.Dormant; mgr.ShowAlert = false; } catch (Exception) { }
        CouncilUi.Toast(Tr("Академия созвала Галактический совет", "The Academy has convened the Galactic Council"));
    }

    static void DisbandCouncil(Game game, int turn)
    {
        Core.Disband();
        ClearEffects(game);
        CouncilUi.Toast(Tr("Академия уничтожена — Галактический совет распущен, все законы аннулированы",
                           "The Academy is destroyed - the Galactic Council is dissolved and all laws are void"));
    }

    static void PruneMembers(Game game)
    {
        var alive = new List<int>();
        foreach (MajorEmpire e in game.EnumerableMajorEmpires) if (!e.HasBeenEliminated) alive.Add(e.Index);
        for (int i = Core.Members.Count - 1; i >= 0; i--)
            if (!alive.Contains(Core.Members[i])) Core.RemoveMember(Core.Members[i]);
    }

    // ---------------------------------------------------------------- vote lifecycle

    static void DoOpen(AcademyRequestsManager mgr, Game game, int turn)
    {
        var expired = Core.ExpireLaws();
        try { ApplyPresence(game); } catch (Exception e) { Fail("ApplyPresence", e); }
        try { ApplySessionVotes(game, turn); } catch (Exception e) { Fail("ApplySessionVotes", e); }
        int seed = unchecked(turn * 1103515245 + Core.VoteIndex * 12345 + mgr.AcademyEmpire.Index * 31 + 7);
        var props = Core.BuildProposals(seed, CouncilCore.MaxQuestions);
        if (props.Count == 0) { Core.NextVoteTurn = turn + Core.IntervalTurns; return; }
        Core.OpenVote(turn, props);
        CouncilUi.OnVoteOpened(expired.Count);
    }

    static int ScoreOf(int empIndex)
    {
        try { var e = GetMajor(empIndex); return e != null ? (int)e.GlobalScore : 0; } catch (Exception) { return 0; }
    }

    static void DoTally(Game game, int turn)
    {
        // AI empires vote at the end of the voting turn, deterministically (same result on every machine)
        foreach (MajorEmpire e in game.EnumerableMajorEmpires)
        {
            if (!Core.IsMember(e.Index) || !e.IsControlledByAI || Core.HasVoted(e.Index)) continue;
            int aiSeed = unchecked(Core.VoteOpenTurn * 1103515245 + Core.VoteIndex * 12345 + e.Index * 131 + 17);
            Core.TrySetBallot(e.Index, Core.BuildAiBallot(e.Index, aiSeed, ScoreOf));
        }
        RefreshAllianceFlag();
        var enacted = Core.Tally(turn);
        for (int i = 0; i < enacted.Count; i++)
        {
            try { ApplyInstant(game, enacted[i]); } catch (Exception e) { Fail("ApplyInstant " + enacted[i].Id, e); }
        }
        CouncilUi.OnVoteTallied(enacted.Count);
    }

    // ---------------------------------------------------------------- effects

    static void ApplyInstant(Game game, CouncilActiveLaw law)
    {
        if (law.Id == "Mentoring") { ApplyMentoring(game); return; }
        if (law.Id == LawTruce) { ForcePeace(game, law.Target); return; }
        if (law.Id == LawPresence) { MarkPresenceSystems(game, game.Turn); return; }
        if (law.Id == CouncilCore.LawEscort || law.Id == CouncilCore.LawAcademySquad || law.Id == CouncilCore.LawStealth) { SpawnForLaw(game, law); return; }
        if (law.Id == CouncilLaws.DustPayout)
        {
            var emp = GetMajor(law.Target);
            if (emp == null) return;
            int systems = (int)emp.GetPropertyValue(SimulationProperties.Empire.SystemCount);
            int stage = GameStage(game, emp);
            float amount = systems * 30f * stage;
            if (amount <= 0f) return;
            var treasury = emp.GetAgency<DepartmentOfTheTreasury>();
            if (treasury != null) treasury.TryTransferResources(emp.SimulationObject, new StaticString("EmpireMoney"), amount, false);
        }
    }

    static string DesiredSignature()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < Core.Members.Count; i++)
        {
            int e = Core.Members[i];
            int colony = Core.GetColonyDelta(e);
            bool repl = Core.IsLawActiveFor(CouncilLaws.Replenishment, e) && !LawIsGlobal(CouncilLaws.Replenishment);
            bool eye = Core.Holds(e, CouncilLaws.Eye);
            float upk = 0f;
            try { var me = GetMajor(e); if (me != null) upk = UpkeepDelta(me); } catch (Exception) { }
            if (colony != 0 || repl || eye || upk != 0f) sb.Append(e).Append(':').Append(colony).Append(':').Append(repl ? 1 : 0).Append(':').Append(eye ? 1 : 0).Append(':').Append(upk).Append(';');
        }
        return sb.ToString();
    }

    static bool LawIsGlobal(string id) { var d = CouncilLaws.Get(id); return d != null && !d.Targeted; }

    /// <summary>Descriptor-based effects (colonization limit, manpower). Re-applied after load / whenever the state changes.</summary>
    static void RefreshEffects(Game game)
    {
        string sig = DesiredSignature();
        if (sig == appliedSignature) return;
        ClearEffects(game);
        appliedSignature = sig;
        for (int i = 0; i < Core.Members.Count; i++)
        {
            int idx = Core.Members[i];
            var emp = GetMajor(idx);
            if (emp == null || emp.SimulationObject == null) continue;
            var mods = new List<SimulationModifierDescriptor>();
            int colony = Core.GetColonyDelta(idx);
            if (colony != 0)
                mods.Add(CustomHackingHelper.MakeModifier(SimulationProperties.Empire.OverColonizationThreshold.ToString(),
                    SimulationModifierDescriptor.ModifierOperation.Addition, colony, "ClassEmpire"));
            if (Core.IsLawActiveFor(CouncilLaws.Replenishment, idx))
                mods.Add(CustomHackingHelper.MakeModifier(SimulationProperties.Empire.NetEmpireManpower.ToString(),
                    SimulationModifierDescriptor.ModifierOperation.Multiplication, 1.5f, "ClassEmpire"));
            float upkeep = UpkeepDelta(emp);
            if (upkeep != 0f)
                mods.Add(CustomHackingHelper.MakeModifier(SimulationProperties.Empire.EmpireMoneyUpkeep.ToString(),
                    SimulationModifierDescriptor.ModifierOperation.Addition, upkeep, "ClassEmpire"));
            if (Core.Holds(idx, CouncilLaws.Eye))
            {
                // Eye of the Academy: +1 simultaneous hacking operation, +20 to the processing power cap
                mods.Add(CustomHackingHelper.MakeModifier(SimulationProperties.Empire.MaximumHackingOperationsCount.ToString(),
                    SimulationModifierDescriptor.ModifierOperation.Addition, 1f, "ClassEmpire"));
                mods.Add(CustomHackingHelper.MakeModifier(SimulationProperties.Empire.MaximumEmpireProcessingPowerStock.ToString(),
                    SimulationModifierDescriptor.ModifierOperation.Addition, 20f, "ClassEmpire"));
            }
            if (mods.Count == 0) continue;
            var desc = CustomHackingHelper.CreateAndRegisterDescriptor(null, "GalacticCouncil_" + idx + "_" + (++descSeq), "GalacticCouncilEmp", null, mods.ToArray());
            emp.SimulationObject.AddDescriptor(desc);
            appliedDescriptors.Add(desc);
            appliedTargets.Add(emp.SimulationObject);
            emp.Refresh(false);
        }
    }

    static int descSeq;

    static void ClearEffects(Game game)
    {
        try { ClearSystemEffects(); } catch (Exception) { }
        for (int i = 0; i < appliedDescriptors.Count; i++)
        {
            try { appliedTargets[i].RemoveDescriptor(appliedDescriptors[i]); } catch (Exception) { }
        }
        appliedDescriptors.Clear();
        appliedTargets.Clear();
        appliedSignature = "";
    }

    // ---------------------------------------------------------------- queries used by hooks in other game classes

    public static bool BlocksNewBackdoor()
    {
        try { return Core.Enabled && Core.IsLawActive(CouncilLaws.Counterintel); } catch (Exception) { return false; }
    }

    /// <summary>Applies sleeper-related council laws to the number of sleepers a hack sees on a system.</summary>
    public static int AdjustSleepers(int raw, Empire hacker)
    {
        try
        {
            if (!Core.Enabled) return raw;
            if (Core.IsLawActive(CouncilLaws.NoDestabilization)) return 0;
            if (Core.IsLawActive(CouncilLaws.IntelExchange)) return raw + 4;
        }
        catch (Exception) { }
        return raw;
    }

    /// <summary>Drop-in replacement for CustomHackingHelper.GetHackerSleeperCountOnSystem at the "can this hack run" call sites.</summary>
    public static int SleepersForHack(Empire hackerEmpire, ColonizedStarSystem system)
    {
        return AdjustSleepers(CustomHackingHelper.GetHackerSleeperCountOnSystem(hackerEmpire, system), hackerEmpire);
    }

    // ---------------------------------------------------------------- voting (UI side)

    /// <summary>Applies the ballot locally and posts the order so every machine ends up with the same ballot.</summary>
    public static bool CastBallot(Empire empire, List<int[]> entries)
    {
        try
        {
            if (empire == null) return false;
            if (!Core.TrySetBallot(empire.Index, entries)) return false;
            var pc = empire.PlayerControllers;
            if (pc != null && pc.Client != null)
                pc.Client.PostOrder(new OrderCouncilVote(empire.Index, entries));
            return true;
        }
        catch (Exception e) { Fail("CastBallot", e); return false; }
    }

    public static bool VotePreprocessor(OrderCouncilVote order)
    {
        try { return ActionPreprocessor(order); } catch (Exception e) { Fail("VotePreprocessor", e); return false; }
    }

    public static System.Collections.IEnumerator VoteProcessor(OrderCouncilVote order)
    {
        try { ActionProcess(order); }
        catch (Exception e) { Fail("VoteProcessor", e); }
        yield break;
    }

    static void ApplyMentoring(Game game)
    {
        var guids = new List<GameEntityGUID>();
        foreach (MajorEmpire e in game.EnumerableMajorEmpires)
        {
            if (!Core.IsMember(e.Index) || e.HasBeenEliminated) continue;   // human and AI members alike
            var edu = e.GetAgency<DepartmentOfEducation>();
            if (edu == null) continue;
            foreach (Hero h in edu.ActiveHeroes)
                if (!CustomHackingHelper.IsAiHero(h)) guids.Add(h.GUID);   // the AI ruler heroes of the hacking mod keep their own progression
        }
        var server = ServerController(null);
        if (server == null || guids.Count == 0) return;
        server.PostOrder(new OrderAddLevelsToHeroes(guids.ToArray(), 3f));
    }
}
