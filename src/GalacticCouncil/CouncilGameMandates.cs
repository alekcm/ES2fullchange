// Stage 2 glue: mandates, victory, leaving the council, supply, market and loot hooks.
using System;
using System.Collections.Generic;
using UnityEngine;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Simulation;

public static partial class CouncilGame
{
    static bool victoryRaised;
    static string lastHoldersSig = null;

    public static int CurTurn()
    {
        var g = GetGame();
        return g != null ? g.Turn : 0;
    }

    // ---------------------------------------------------------------- per-turn work

    static void PerTurn(Game game, int turn)
    {
        try { CheckVictoryNow(game, turn); } catch (Exception e) { Fail("CheckVictory", e); }
        try { PressOutcasts(game); } catch (Exception e) { Fail("PressOutcasts", e); }
        try { EndMarshalContracts(); } catch (Exception e) { Fail("EndMarshalContracts", e); }
        try { AnnounceHolders(); } catch (Exception e) { Fail("AnnounceHolders", e); }
    }

    static void CheckVictoryNow(Game game, int turn)
    {
        if (victoryRaised) return;
        int winner = Core.CheckVictory(turn);
        if (winner < 0) return;
        var emp = GetMajor(winner);
        if (emp == null) return;
        var def = FindDiplomaticVictory();
        if (def == null)
        {
            CouncilUi.Toast(Tr("Победа Верховного легата достигнута, но условие победы не найдено в данных игры",
                               "The Supreme Legate has won, but no matching victory condition was found in the game data"));
            Fail("Victory", new Exception("no diplomatic victory condition found"));
            victoryRaised = true;
            return;
        }
        victoryRaised = true;
        var svc = Services.GetService<IVictoryManagementService>();
        if (svc != null) svc.ForceRiseVictory(def, emp);
    }

    static VictoryConditionDefinition FindDiplomaticVictory()
    {
        var db = Databases.GetDatabase<VictoryConditionDefinition>(false);
        if (db == null) return null;
        VictoryConditionDefinition best = null;
        string[] keys = { "diplomat", "senate", "academy", "council", "peace", "cooperat", "unif" };
        for (int k = 0; k < keys.Length && best == null; k++)
        {
            foreach (var d in db.GetValues())
            {
                if (d == null) continue;
                string n = d.Name.ToString().ToLowerInvariant();
                if (n.Contains(keys[k])) { best = d; break; }
            }
        }
        return best;
    }

    /// <summary>The Academy is at war with every empire that left the council: its relation points stay at the minimum.</summary>
    static void PressOutcasts(Game game)
    {
        for (int i = 0; i < Core.Outcasts.Count; i++)
        {
            var emp = GetMajor(Core.Outcasts[i]);
            if (emp == null || emp.HasBeenEliminated) continue;
            AcademyDiplomaticRelation.SetRelationPoints(emp, -100f);
        }
    }

    static void EndMarshalContracts()
    {
        // the contract ends with the mandate (it can be hired again only after a new election)
        for (int i = Core.MarshalHired.Count - 1; i >= 0; i--)
            if (!Core.Holds(Core.MarshalHired[i], CouncilLaws.Marshal)) Core.MarshalHired.RemoveAt(i);
    }

    static void AnnounceHolders()
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < Core.Holders.Count; i++) sb.Append(Core.Holders[i].Id).Append('=').Append(Core.Holders[i].Emp).Append(';');
        string sig = sb.ToString();
        if (lastHoldersSig == null) { lastHoldersSig = sig; return; }
        if (sig == lastHoldersSig) return;
        string old = lastHoldersSig;
        lastHoldersSig = sig;
        for (int i = 0; i < Core.Holders.Count; i++)
        {
            string tok = Core.Holders[i].Id + "=" + Core.Holders[i].Emp + ";";
            if (old.Contains(tok)) continue;
            var d = CouncilLaws.GetMandate(Core.Holders[i].Id);
            if (d == null) continue;
            CouncilUi.Toast(EmpireName(Core.Holders[i].Emp) + Tr(" получает мандат: ", " receives the mandate: ") + (IsRussian() ? d.Ru : d.En));
        }
    }

    // ---------------------------------------------------------------- orders from the UI

    public static bool SendKind(Empire empire, int kind, int a, int b)
    {
        try
        {
            if (empire == null) return false;
            var pc = empire.PlayerControllers;
            if (pc == null || pc.Client == null) return false;
            pc.Client.PostOrder(new OrderCouncilVote(empire.Index, kind, a, b));
            return true;
        }
        catch (Exception e) { Fail("SendKind", e); return false; }
    }

    static bool ActionPreprocessor(OrderCouncilVote o)
    {
        if (o == null || !Core.Enabled) return false;
        int emp = o.EmpireIndex;
        switch (o.Kind)
        {
            case OrderCouncilVote.KindBallot: return Core.Phase == CouncilCore.PhaseVoting;
            case OrderCouncilVote.KindChairmanReplace: return Core.CanChairmanReplace(emp);
            case OrderCouncilVote.KindMarshalHire: return Core.CanHireMarshal(emp);
            case OrderCouncilVote.KindQuartermaster: return Core.CanUseQuartermaster(emp, CurTurn());
            case OrderCouncilVote.KindReportTask: return Core.IsMember(emp);
            case OrderCouncilVote.KindLeave: return Core.IsMember(emp);
        }
        return false;
    }

    static void ActionProcess(OrderCouncilVote o)
    {
        int emp = o.EmpireIndex;
        int turn = CurTurn();
        switch (o.Kind)
        {
            case OrderCouncilVote.KindBallot:
                Core.TrySetBallot(emp, o.Entries);
                break;
            case OrderCouncilVote.KindChairmanReplace:
                if (o.B >= 0 && o.B < CouncilLaws.All.Count) Core.ChairmanReplace(emp, o.A, CouncilLaws.All[o.B].Id);
                break;
            case OrderCouncilVote.KindMarshalHire:
                if (Core.TryHireMarshal(emp)) SpawnContingent(emp);
                break;
            case OrderCouncilVote.KindQuartermaster:
                if (Core.TryQuartermaster(emp, turn)) GrantSupply(emp);
                break;
            case OrderCouncilVote.KindReportTask:
                if (o.A >= 0 && o.A < TaskCategories.Length) Core.ReportTask(emp, TaskCategories[o.A], "T" + o.B);
                break;
            case OrderCouncilVote.KindLeave:
                if (Core.Leave(emp))
                {
                    var me = GetMajor(emp);
                    if (me != null) AcademyDiplomaticRelation.SetRelationPoints(me, -100f);
                    CouncilUi.Toast(EmpireName(emp) + Tr(" покидает Совет — Академия объявляет войну", " leaves the Council - the Academy declares war"));
                }
                break;
        }
    }

    public static readonly string[] TaskCategories = {
        CouncilLaws.CatDiplomatic, CouncilLaws.CatMilitary, CouncilLaws.CatDefense,
        CouncilLaws.CatSupply, CouncilLaws.CatIntel, CouncilLaws.CatResearch };

    // ---------------------------------------------------------------- mandate powers

    static float GameLengthScale(Empire any)
    {
        try { return Math.Max(0.5f, CustomHackingHelper.ScaleTurns(20, any) / 20f); } catch (Exception) { return 1f; }
    }

    /// <summary>Council Quartermaster: a big delivery of Dust and Science, no repayment. Scales with the stage and game length.</summary>
    static void GrantSupply(int empIndex)
    {
        var emp = GetMajor(empIndex);
        if (emp == null) return;
        int stage = GameStage(GetGame(), emp);
        float scale = GameLengthScale(emp);
        var treasury = emp.GetAgency<DepartmentOfTheTreasury>();
        if (treasury == null) return;
        try { treasury.TryTransferResources(emp.SimulationObject, DepartmentOfTheTreasury.Resources.EmpireMoney, 400f * stage * scale, false); }
        catch (Exception e) { Fail("Supply Dust", e); }
        try { treasury.TryTransferResources(emp.SimulationObject, DepartmentOfTheTreasury.Resources.EmpireResearch, 150f * stage * scale, false); }
        catch (Exception e) { Fail("Supply Science", e); }
        CouncilUi.Toast(EmpireName(empIndex) + Tr(": Интендант получил поставку от Академии", ": the Quartermaster received a delivery from the Academy"));
    }

    static readonly string[] WarKeys = { "cruiser", "destroyer", "frigate", "corvette", "battle", "war", "combat", "attack", "line" };
    static readonly string[] NotWarKeys = { "scout", "colon", "outpost", "constr", "transport", "cargo", "mother", "explor", "probe", "settl", "trade", "hero", "builder", "miner" };

    /// <summary>Marshal's limited service contingent: warships of the empire's own faction at the home system. Strength scales with the stage.
    /// Only the host posts the creation order (all machines then receive it through the normal order pipeline).</summary>
    static void SpawnContingent(int empIndex)
    {
        try
        {
            var emp = GetMajor(empIndex);
            if (emp == null || emp.PlayerControllers == null || emp.PlayerControllers.Server == null) return;
            var interior = emp.GetAgency<DepartmentOfTheInterior>();
            if (interior == null || interior.HomeSystemNode == null) return;
            var map = emp.Faction.AbstractToShipDesignDictionary;
            string design = null;
            for (int pass = 0; pass < WarKeys.Length && design == null; pass++)
            {
                foreach (var kv in map)
                {
                    string k = kv.Key.ToLowerInvariant();
                    bool bad = false;
                    for (int n = 0; n < NotWarKeys.Length; n++) if (k.Contains(NotWarKeys[n])) { bad = true; break; }
                    if (!bad && k.Contains(WarKeys[pass])) { design = kv.Value; break; }
                }
            }
            if (design == null) { Fail("SpawnContingent", new Exception("no warship design among " + map.Count + " abstract designs")); return; }
            int count = Math.Min(8, 2 + GameStage(GetGame(), emp));
            var dict = new Dictionary<StaticString, int>();
            dict[new StaticString(design)] = count;
            emp.PlayerControllers.Server.PostOrder(new OrderCreateFleetFromShipDesignName(empIndex, interior.HomeSystemNode.NodePosition, dict));
        }
        catch (Exception e) { Fail("SpawnContingent", e); }
    }

    // ---------------------------------------------------------------- market and loot hooks

    static bool IsPremiumResource(StaticString name)
    {
        var db = Databases.GetDatabase<ResourceDefinition>(false);
        ResourceDefinition def;
        if (db == null || !db.TryGetValue(name, out def) || def == null) return false;
        string t = def.ResourceType.ToString();
        return t == "Strategic" || t == "Luxury" || t == "SystemStrategic" || t == "SystemLuxury";
    }

    /// <summary>Tradable.GetValue / TradableResource.GetValue return hook (purchase price of a lot).</summary>
    public static float AdjustTradableValue(float value, Tradable tradable, TradableTransactionType type)
    {
        try
        {
            if (!Core.Enabled || type != TradableTransactionType.Buyout || tradable == null) return value;
            var res = tradable as TradableResource;
            if (res != null)
            {
                if (Core.IsLawActive("Deregulation") && IsPremiumResource(res.ResourceName)) return value * 0.5f;
            }
            else if (tradable is TradableShip)
            {
                if (Core.IsLawActive("Privateering")) return value * 0.5f;
            }
        }
        catch (Exception) { }
        return value;
    }

    /// <summary>ITradingManagementService.GetTradablePrice return hook: Trade Restrictions makes the target's trades twice as bad.</summary>
    public static float AdjustTradePrice(float price, TradableTransactionType type, Empire empire)
    {
        try
        {
            if (!Core.Enabled || empire == null || !Core.IsLawActiveFor("TradeRestrictions", empire.Index)) return price;
            return type == TradableTransactionType.Buyout ? price * 2f : price * 0.5f;
        }
        catch (Exception) { return price; }
    }

    /// <summary>GroundBattleOutcomeLootDefinition.FillLoot end hook: the Right of Plunder doubles the attacker's loot.</summary>
    public static void AdjustLoot(GroundBattle battle, List<GroundBattleOutcomeLootDefinition.EvaluatedLootItem> loot)
    {
        try
        {
            if (!Core.Enabled || battle == null || loot == null || battle.AttackerEmpire == null) return;
            if (!Core.IsLawActiveFor("PillageX2", battle.AttackerEmpire.Index)) return;
            for (int i = 0; i < loot.Count; i++)
            {
                var it = loot[i];
                it.Amount *= 2f;
                loot[i] = it;
            }
        }
        catch (Exception) { }
    }
}
