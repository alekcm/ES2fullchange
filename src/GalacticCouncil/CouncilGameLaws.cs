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

    // ---------------------------------------------------------------- ground battle: weaker / tougher defending troops

    /// <summary>Return hook of ServerGroundBattle.GetColonizedStarSystemOpponentPropertyValue(system, property, opponentName).</summary>
    public static float AdjustOpponentProp(float value, ColonizedStarSystem system, StaticString property, string opponent)
    {
        try
        {
            if (!Core.Enabled || system == null || opponent != "Defender") return value;
            if (property != SimulationProperties.StarSystem.GroundBattleOpponentTroopsMaxHealthMultiplier) return value;
            var owner = system.Empire;
            if (owner == null) return value;
            int e = owner.Index;
            if (Core.IsLawActiveFor(LawFort, e)) value *= 0.75f;                                                  // weaker defending troops
            if (Core.IsLawActiveFor(LawCapital, e) && system.IsHomeSystemOfCurrentOwner) value *= 1.5f;           // tougher home-system defenders
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

    // ---------------------------------------------------------------- per-system effects
    // Trading Company Reinvestment: SystemProduction x1.25 in systems with a company HQ / subsidiary of a member.
    // Fortification Inspection: SystemManpowerLoss (garrison siege losses) x1.5 in every system of the target.
    // Capital Reinforcement: SystemManpowerRefillRate (garrison recovery) x2 in the target's home system.
    // Same mechanism as the god-mode code of the mod: a descriptor of its own type holding modifiers whose path is the class of the target.

    static readonly List<SimulationDescriptor> sysDescriptors = new List<SimulationDescriptor>();
    static readonly List<ColonizedStarSystem> sysTargets = new List<ColonizedStarSystem>();
    static string sysSig = "";

    static void ClearSystemEffects()
    {
        for (int i = 0; i < sysDescriptors.Count; i++)
        {
            try { sysTargets[i].SimulationObject.RemoveDescriptor(sysDescriptors[i]); sysTargets[i].Refresh(false); } catch (Exception) { }
        }
        sysDescriptors.Clear();
        sysTargets.Clear();
        sysSig = "";
    }

    const int FxReinvest = 1, FxFort = 2, FxCapital = 4;

    static void RefreshSystemEffects(Game game)
    {
        if (!Core.Enabled) { if (sysDescriptors.Count > 0) ClearSystemEffects(); return; }
        bool reinvest = Core.IsLawActive(LawReinvest), fort = Core.IsLawActive(LawFort), cap = Core.IsLawActive(LawCapital);
        if (!reinvest && !fort && !cap) { if (sysDescriptors.Count > 0) ClearSystemEffects(); return; }

        var flags = new Dictionary<ulong, int>();
        var systems = new Dictionary<ulong, ColonizedStarSystem>();
        var repo = Services.GetService<IColonizedStarSystemRepositoryService>();
        foreach (MajorEmpire e in game.EnumerableMajorEmpires)
        {
            if (e.HasBeenEliminated) continue;
            if (reinvest && repo != null && Core.IsMember(e.Index))
            {
                var com = e.GetAgency<DepartmentOfCommerce>();
                if (com != null)
                    foreach (TradingCompanyStructure st in com.EnumerableTradingCompanyStructures)
                    {
                        if (st == null || st.IsDisabled || st.IsLost) continue;
                        ColonizedStarSystem cs;
                        if (!repo.TryGetValue(e, st.NodePosition, out cs) || cs == null) continue;
                        Mark(flags, systems, cs, FxReinvest);
                    }
            }
            bool f = fort && Core.IsLawActiveFor(LawFort, e.Index);
            bool c = cap && Core.IsLawActiveFor(LawCapital, e.Index);
            if (!f && !c) continue;
            var interior = e.GetAgency<DepartmentOfTheInterior>();
            if (interior == null) continue;
            foreach (ColonizedStarSystem cs in interior.ColonizedStarSystems)
            {
                if (cs == null) continue;
                if (f) Mark(flags, systems, cs, FxFort);
                if (c && cs.IsHomeSystemOfCurrentOwner) Mark(flags, systems, cs, FxCapital);
            }
        }
        var ids = new List<ulong>(flags.Keys);
        ids.Sort();
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < ids.Count; i++) sb.Append(ids[i]).Append(':').Append(flags[ids[i]]).Append(',');
        string sig = sb.ToString();
        if (sig == sysSig) return;
        ClearSystemEffects();
        sysSig = sig;
        for (int i = 0; i < ids.Count; i++)
        {
            var cs = systems[ids[i]];
            if (cs.SimulationObject == null) continue;
            int fl = flags[ids[i]];
            var mods = new List<SimulationModifierDescriptor>();
            if ((fl & FxReinvest) != 0) mods.Add(SysMod(SimulationProperties.StarSystem.SystemProduction, 1.25f));
            if ((fl & FxFort) != 0) mods.Add(SysMod(SimulationProperties.StarSystem.SystemManpowerLoss, 1.5f));
            if ((fl & FxCapital) != 0)
            {
                mods.Add(SysMod(SimulationProperties.StarSystem.SystemManpowerRefillRate, 2f));
                mods.Add(SysMod(SimulationProperties.StarSystem.MaximumSystemManpowerRefillRate, 2f));
            }
            var desc = CustomHackingHelper.CreateAndRegisterDescriptor(null, "GalacticCouncilSys_" + (++descSeq), "GalacticCouncilSys", null, mods.ToArray());
            cs.SimulationObject.AddDescriptor(desc);
            sysDescriptors.Add(desc);
            sysTargets.Add(cs);
            cs.Refresh(false);
        }
    }

    static void Mark(Dictionary<ulong, int> flags, Dictionary<ulong, ColonizedStarSystem> systems, ColonizedStarSystem cs, int flag)
    {
        ulong id = cs.GUID;
        int cur; flags.TryGetValue(id, out cur);
        flags[id] = cur | flag;
        systems[id] = cs;
    }

    static SimulationModifierDescriptor SysMod(StaticString property, float factor)
    {
        return CustomHackingHelper.MakeModifier(property.ToString(), SimulationModifierDescriptor.ModifierOperation.Multiplication, factor, "ClassStarSystem");
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

    // ---------------------------------------------------------------- Academy home world counts as half of the capitals needed for a military victory

    /// <summary>
    /// End hook of VictoryManager.SetupInterpreterContextForContender(contender): the victory formulas read the number of capitals
    /// that still exist from the interpreter context (NumberOfHealthyHomeSystems). For a contender that holds the Academy home world the
    /// number is lowered by (number of players / 2), i.e. the Academy home world counts for half of the players' capitals.
    /// </summary>
    public static void AdjustVictoryContext(VictoryManager manager, MajorEmpire[] empires)
    {
        try
        {
            var game = GetGame();
            if (game == null || game.Galaxy == null || manager == null || empires == null || manager.InterpreterContext == null) return;
            int healthy = 0, players = 0;
            foreach (MajorEmpire e in game.EnumerableMajorEmpires)
            {
                players++;
                var interior = e.GetAgency<DepartmentOfTheInterior>();
                var home = interior != null ? interior.HomeSystemNode : null;
                if (home == null || !home.IsDestroyed) healthy++;
            }
            float value = healthy;
            if (HoldsAcademyHome(game, empires)) value = Math.Max(1f, healthy - players / 2);
            manager.InterpreterContext.Register(new StaticString("NumberOfHealthyHomeSystems"), (object)value);
        }
        catch (Exception e) { Fail("AdjustVictoryContext", e); }
    }

    static bool HoldsAcademyHome(Game game, MajorEmpire[] empires)
    {
        var repo = Services.GetService<IColonizedStarSystemRepositoryService>();
        if (repo == null) return false;
        foreach (ColonizedStarSystem cs in repo.GetValues(game.Galaxy.AcademyHomePosition))
        {
            if (cs == null || cs.State != StarSystemState.Colony) continue;
            var holder = cs.Empire as MajorEmpire;
            if (holder == null) continue;
            for (int i = 0; i < empires.Length; i++) if (empires[i] != null && empires[i].Index == holder.Index) return true;
        }
        return false;
    }
}
