// Stage 4/5 glue: laws implemented through hooks found in the game code
// (simplified battle damage, ground-battle modifiers, trading company income, evacuation compensation,
//  forced peace, strategic presence, trade company reinvestment).
using System;
using System.Collections.Generic;
using UnityEngine;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Simulation;

public static partial class CouncilGame
{
    public const string LawDamageDown = "DamageDown";
    public const string LawEvac = "EvacCompensation";
    public const string LawPresence = "StrategicPresence";
    public const string LawTruce = "Truce";
    public const string LawReinvest = "TradeReinvest";
    public const string LawFort = "FortInspection";
    public const string LawCapital = "CapitalFortify";
    public const string LawAntitrust = "Antimonopoly";

    // ---------------------------------------------------------------- ship weapon damage -20% (simplified battle = every battle)

    static int GroupEmpire(EncounterGroup g)
    {
        if (g == null) return -1;
        var leader = g.Leader;
        if (leader == null || leader.Empire == null) return -1;
        return leader.Empire.Index;
    }

    /// <summary>End hook of ServerEncounterState_RunSimpleBattle.ComputeSimplifiedEffectiveDamagePercentage.</summary>
    public static void AdjustSimplifiedDamage(EncounterGroup attacker, EncounterGroup defender, ref float attackerPct, ref float defenderPct)
    {
        try
        {
            if (!Core.Enabled || !Core.IsLawActive(LawDamageDown)) return;
            int a = GroupEmpire(attacker), d = GroupEmpire(defender);
            if (a >= 0 && Core.IsLawActiveFor(LawDamageDown, a)) attackerPct *= 0.8f;
            if (d >= 0 && Core.IsLawActiveFor(LawDamageDown, d)) defenderPct *= 0.8f;
        }
        catch (Exception e) { Fail("AdjustSimplifiedDamage", e); }
    }

    // ---------------------------------------------------------------- ground battle: Fortification Inspection / Capital Reinforcement

    /// <summary>Return hook of ServerGroundBattle.GetColonizedStarSystemOpponentPropertyValue(system, property, opponentName).</summary>
    public static float AdjustOpponentProp(float value, ColonizedStarSystem system, StaticString property, string opponent)
    {
        try
        {
            if (!Core.Enabled || system == null || opponent != "Defender") return value;
            var owner = system.Empire;
            if (owner == null) return value;
            int e = owner.Index;
            bool fort = Core.IsLawActiveFor(LawFort, e);
            bool cap = Core.IsLawActiveFor(LawCapital, e) && system.IsHomeSystemOfCurrentOwner;
            if (!fort && !cap) return value;
            if (property == SimulationProperties.StarSystem.GroundBattleOpponentTroopsMaxHealthMultiplier)
            {
                if (fort) value *= 0.75f;      // weaker defending troops
                if (cap) value *= 1.5f;        // tougher home-system defenders
            }
            else if (fort && (property == SimulationProperties.StarSystem.GroundBattleBombardmentOpponentDamagesMin
                           || property == SimulationProperties.StarSystem.GroundBattleBombardmentOpponentDamagesMax))
            {
                value *= 1.5f;                 // siege losses of the garrison +50%
            }
        }
        catch (Exception e) { Fail("AdjustOpponentProp", e); }
        return value;
    }

    // ---------------------------------------------------------------- Antitrust Oversight

    /// <summary>Return hook of DepartmentOfCommerce.GetTradingCompanyIncomeMultiplier(company).</summary>
    public static float AdjustCompanyIncome(float value, TradingCompany company)
    {
        try
        {
            if (Core.Enabled && company != null && Core.IsLawActive(LawAntitrust) && Core.IsLawActiveFor(LawAntitrust, company.OwnedByEmpireIndex))
                return value * 0.5f;
        }
        catch (Exception e) { Fail("AdjustCompanyIncome", e); }
        return value;
    }

    static string lastAntitrustSig = "";

    static void RefreshCompanyIncome(Game game)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < Core.Active.Count; i++)
            if (Core.Active[i].Id == LawAntitrust) sb.Append(Core.Active[i].Target).Append(';');
        string sig = sb.ToString();
        if (sig == lastAntitrustSig) return;
        lastAntitrustSig = sig;
        // incomes are cached on the companies: force a recomputation
        foreach (MajorEmpire e in game.EnumerableMajorEmpires)
        {
            var com = e.GetAgency<DepartmentOfCommerce>();
            if (com == null) continue;
            foreach (TradingCompany tc in com.TradingCompanies) tc.IncomeDirty = true;
        }
    }

    // ---------------------------------------------------------------- Trading Company Reinvestment

    static readonly List<SimulationDescriptor> reinvestDescriptors = new List<SimulationDescriptor>();
    static readonly List<ColonizedStarSystem> reinvestTargets = new List<ColonizedStarSystem>();
    static string reinvestSig = "";

    static void ClearReinvest()
    {
        for (int i = 0; i < reinvestDescriptors.Count; i++)
        {
            try { reinvestTargets[i].SimulationObject.RemoveDescriptor(reinvestDescriptors[i]); reinvestTargets[i].Refresh(false); } catch (Exception) { }
        }
        reinvestDescriptors.Clear();
        reinvestTargets.Clear();
        reinvestSig = "";
    }

    static void RefreshReinvest(Game game)
    {
        bool on = Core.Enabled && Core.IsLawActive(LawReinvest);
        if (!on) { if (reinvestDescriptors.Count > 0) ClearReinvest(); return; }
        var repo = Services.GetService<IColonizedStarSystemRepositoryService>();
        if (repo == null) return;
        var systems = new List<ColonizedStarSystem>();
        var ids = new List<ulong>();
        foreach (MajorEmpire e in game.EnumerableMajorEmpires)
        {
            if (!Core.IsMember(e.Index) || e.HasBeenEliminated) continue;
            var com = e.GetAgency<DepartmentOfCommerce>();
            if (com == null) continue;
            foreach (TradingCompanyStructure st in com.EnumerableTradingCompanyStructures)
            {
                if (st == null || st.IsDisabled || st.IsLost) continue;
                ColonizedStarSystem cs;
                if (!repo.TryGetValue(e, st.NodePosition, out cs) || cs == null) continue;
                ulong id = cs.GUID;
                if (ids.Contains(id)) continue;
                ids.Add(id); systems.Add(cs);
            }
        }
        ids.Sort();
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < ids.Count; i++) sb.Append(ids[i]).Append(',');
        string sig = sb.ToString();
        if (sig == reinvestSig) return;
        ClearReinvest();
        reinvestSig = sig;
        for (int i = 0; i < systems.Count; i++)
        {
            var cs = systems[i];
            if (cs.SimulationObject == null) continue;
            var mod = CustomHackingHelper.MakeModifier(SimulationProperties.StarSystem.SystemProduction.ToString(),
                SimulationModifierDescriptor.ModifierOperation.Multiplication, 1.25f, "ClassColonizedStarSystem");
            var desc = CustomHackingHelper.CreateAndRegisterDescriptor(null, "GalacticCouncil_Reinvest_" + (++descSeq), "ClassColonizedStarSystem", null,
                new SimulationModifierDescriptor[] { mod });
            cs.SimulationObject.AddDescriptor(desc);
            reinvestDescriptors.Add(desc);
            reinvestTargets.Add(cs);
            cs.Refresh(false);
        }
    }

    // ---------------------------------------------------------------- evacuation compensation

    /// <summary>Called just before DepartmentOfTheInterior.DecolonizeProcessor removes the system.</summary>
    public static void OnDecolonize(OrderDecolonize order, ColonizedStarSystem system)
    {
        try
        {
            if (!Core.Enabled || !Core.IsLawActive(LawEvac) || order == null || system == null) return;
            if (!order.SpawnColonyShip || order.EmpireThatCapturedTheOutpostIndex >= 0) return;   // only voluntary evacuation into a colony ship
            var emp = system.Empire;
            if (emp == null) return;
            float cost = 0f;
            var list = system.GetDestructibleImprovementsOnSystem();
            if (list != null) for (int i = 0; i < list.Count; i++) cost += list[i].Cost;
            var game = GetGame();
            int stage = game != null ? GameStage(game, emp) : 1;
            float amount = 3f * cost + 250f * Math.Max(1, stage);
            var treasury = emp.GetAgency<DepartmentOfTheTreasury>();
            if (treasury != null && amount > 0f)
            {
                treasury.TryTransferResources(emp.SimulationObject, DepartmentOfTheTreasury.Resources.EmpireMoney, amount, false);
                if (emp.Index == LocalIndex())
                    CouncilUi.Toast(Tr("Совет выплатил компенсацию за эвакуацию: ", "The Council paid evacuation compensation: ") + (int)amount + " " + Tr("Пыли", "Dust"));
            }
        }
        catch (Exception e) { Fail("OnDecolonize", e); }
    }

    static int LocalIndex()
    {
        var e = LocalEmpire();
        return e != null ? e.Index : -1;
    }

    // ---------------------------------------------------------------- Academic Truce: forced peace

    static void ForcePeace(Game game, int empIndex)
    {
        var emp = GetMajor(empIndex);
        if (emp == null || emp.HasBeenEliminated) return;
        var wars = Services.GetService<IDiplomaticWarRepositoryService>();
        var ctl = wars as IDiplomaticWarRepositoryControl;
        if (wars == null || ctl == null) return;
        var found = new List<WarInstance>();
        foreach (MajorEmpire other in game.EnumerableMajorEmpires)
        {
            if (other.Index == empIndex || other.HasBeenEliminated) continue;
            WarInstance w;
            if (wars.TryGetWarInstanceOpposingEmpires(emp, other, out w) && w != null && !found.Contains(w)) found.Add(w);
        }
        int ended = 0;
        for (int i = 0; i < found.Count; i++)
        {
            var war = found[i];
            var att = new List<WarEmpire>(war.AttackerGroup.WarEmpires);
            var def = new List<WarEmpire>(war.DefenderGroup.WarEmpires);
            for (int a = 0; a < att.Count; a++)
                for (int d = 0; d < def.Count; d++)
                {
                    var ea = att[a].Empire; var ed = def[d].Empire;
                    if (ea == null || ed == null) continue;
                    var fa = ea.GetAgency<DepartmentOfForeignAffairs>();
                    var fd = ed.GetAgency<DepartmentOfForeignAffairs>();
                    if (fa != null) fa.SetDiplomaticRelationState(ed, DiplomaticRelationState.Names.Major.Truce, false, null);
                    if (fd != null) fd.SetDiplomaticRelationState(ea, DiplomaticRelationState.Names.Major.Truce, false, null);
                }
            ctl.DestroyWarInstance(war, true);
            ended++;
        }
        if (ended > 0)
            CouncilUi.Toast(EmpireName(empIndex) + Tr(": Академия принудительно заключила мир (войн прекращено: ", ": the Academy forced peace (wars ended: ") + ended + ")");
    }

    // ---------------------------------------------------------------- Strategic Presence Program

    static Dictionary<ulong, StarSystemNode> NodeMap(Game game)
    {
        var map = new Dictionary<ulong, StarSystemNode>();
        if (game == null || game.Galaxy == null) return map;
        foreach (StarSystemNode n in game.Galaxy.StarSystemNodes)
        {
            ulong id = n.GUID;
            map[id] = n;
        }
        return map;
    }

    /// <summary>Marks random systems at the end of the vote; they count at the next session.</summary>
    static void MarkPresenceSystems(Game game, int turn)
    {
        var cand = new List<ulong>();
        foreach (StarSystemNode n in game.Galaxy.StarSystemNodes)
        {
            if (n.IsDestroyed || n.IsHomeSystem || n.IsQuestNode) continue;
            ulong id = n.GUID;
            cand.Add(id);
        }
        cand.Sort();
        int seed = unchecked(turn * 7919 + Core.VoteIndex * 104729 + cand.Count * 13 + 3);
        var idx = CouncilCore.PickPresenceIndices(cand.Count, Core.Members.Count, seed);
        var marks = new List<ulong>();
        for (int i = 0; i < idx.Count; i++) marks.Add(cand[idx[i]]);
        Core.SetPresenceMarks(marks);
        CouncilUi.Toast(Tr("Стратегическое присутствие: отмечено систем — ", "Strategic Presence: systems marked - ") + marks.Count);
    }

    /// <summary>Empire that holds the system (colony first, then outpost), or -1.</summary>
    static int OwnerOfNode(IColonizedStarSystemRepositoryService repo, StarSystemNode node)
    {
        int outpost = -1;
        foreach (ColonizedStarSystem cs in repo.GetValues(node.NodePosition))
        {
            if (cs == null || cs.Empire == null) continue;
            if (cs.State == StarSystemState.Colony) return cs.Empire.Index;
            if (cs.State == StarSystemState.Outpost && outpost < 0) outpost = cs.Empire.Index;
        }
        return outpost;
    }

    /// <summary>Called when a session opens: marked systems that are held turn into extra votes.</summary>
    static void ApplyPresence(Game game)
    {
        var repo = Services.GetService<IColonizedStarSystemRepositoryService>();
        if (repo == null || Core.PresenceMarks.Count == 0) { Core.ApplyPresenceBonus(delegate (ulong g) { return -1; }); return; }
        var map = NodeMap(game);
        Core.ApplyPresenceBonus(delegate (ulong g)
        {
            StarSystemNode n;
            return map.TryGetValue(g, out n) ? OwnerOfNode(repo, n) : -1;
        });
    }

    /// <summary>Lines for the UI: marked systems and who holds them now.</summary>
    public static List<string> PresenceLines()
    {
        var res = new List<string>();
        try
        {
            var game = GetGame();
            var repo = Services.GetService<IColonizedStarSystemRepositoryService>();
            if (game == null || repo == null) return res;
            var map = NodeMap(game);
            for (int i = 0; i < Core.PresenceMarks.Count; i++)
            {
                StarSystemNode n;
                if (!map.TryGetValue(Core.PresenceMarks[i], out n)) continue;
                int o = OwnerOfNode(repo, n);
                res.Add(n.LocalizedName + " — " + (o >= 0 ? EmpireName(o) : Tr("свободна", "unclaimed")));
            }
        }
        catch (Exception e) { Fail("PresenceLines", e); }
        return res;
    }
}
