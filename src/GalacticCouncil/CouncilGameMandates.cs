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
        try { RefreshCompanyIncome(game); } catch (Exception e) { Fail("RefreshCompanyIncome", e); }
        try { RefreshSystemEffects(game); } catch (Exception e) { Fail("RefreshSystemEffects", e); }
        try { SyncSquads(game); } catch (Exception e) { Fail("SyncSquads", e); }
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
        if (o == null) return false;
        if (o.Kind == OrderCouncilVote.KindDebug) return true;      // God-Mod menu: must work before the Council exists ("convene now")
        if (!Core.Enabled) return false;
        int emp = o.EmpireIndex;
        switch (o.Kind)
        {
            case OrderCouncilVote.KindBallot: return Core.Phase == CouncilCore.PhaseVoting;
            case OrderCouncilVote.KindChairmanReplace: return Core.CanChairmanReplace(emp);
            case OrderCouncilVote.KindMarshalHire: return Core.CanHireMarshal(emp);
            case OrderCouncilVote.KindQuartermaster: return Core.CanUseQuartermaster(emp, CurTurn());
            case OrderCouncilVote.KindReportTask: return false;     // the manual assignment stub is gone: conditions are read from the game
            case OrderCouncilVote.KindLeave: return Core.IsMember(emp);
            case OrderCouncilVote.KindDebug: return true;
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
            case OrderCouncilVote.KindDebug:
                DebugProcess(o);
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

    static readonly string[] HeavyKeys = { "cruiser", "destroyer", "frigate", "corvette", "battle", "war", "combat", "attack", "line" };
    static readonly string[] LightKeys = { "corvette", "frigate", "destroyer", "cruiser", "battle", "war", "combat", "attack", "line" };
    static readonly string[] NotWarKeys = { "scout", "colon", "outpost", "constr", "transport", "cargo", "mother", "explor", "probe", "settl", "trade", "hero", "builder", "miner" };

    /// <summary>Warship design (by name) from the empire's faction table abstract -> design, or null.</summary>
    static string PickWarDesign(Empire emp, bool heavy)
    {
        var map = emp.Faction.AbstractToShipDesignDictionary;
        var keys = heavy ? HeavyKeys : LightKeys;
        for (int pass = 0; pass < keys.Length; pass++)
        {
            foreach (var kv in map)
            {
                string k = kv.Key.ToLowerInvariant();
                bool bad = false;
                for (int n = 0; n < NotWarKeys.Length; n++) if (k.Contains(NotWarKeys[n])) { bad = true; break; }
                if (!bad && k.Contains(keys[pass])) return kv.Value;
            }
        }
        return null;
    }

    /// <summary>The controller that may post server-side orders (only the host has one).</summary>
    static PlayerController ServerController(Empire emp)
    {
        try { if (emp != null && emp.PlayerControllers != null && emp.PlayerControllers.Server != null) return emp.PlayerControllers.Server; } catch (Exception) { }
        var g = GetGame();
        if (g == null) return null;
        foreach (MajorEmpire e in g.EnumerableMajorEmpires)
            if (e.PlayerControllers != null && e.PlayerControllers.Server != null) return e.PlayerControllers.Server;
        return null;
    }

    static string SquadMarker(string tag, int emp) { return " [GC" + tag + emp + "]"; }

    static string SquadLabel(string tag)
    {
        switch (tag)
        {
            case CouncilCore.TagMarshal: return Tr("Контингент Маршала", "Marshal Contingent");
            case CouncilCore.TagEscort: return Tr("Эскорт Совета", "Council Escort");
            case CouncilCore.TagAcademy: return Tr("Эскадра Академии", "Academy Squadron");
            default: return Tr("Скрытая группа", "Covert Group");
        }
    }

    /// <summary>Creates a service squad at the empire's home system. Only the host actually posts the order.
    /// The fleet carries a marker in its name so every machine can find (and later recall) the ships.</summary>
    static void SpawnSquad(int empIndex, string tag, int count, bool heavy)
    {
        try
        {
            var emp = GetMajor(empIndex);
            var server = ServerController(emp);
            if (emp == null || server == null) return;
            var interior = emp.GetAgency<DepartmentOfTheInterior>();
            if (interior == null || interior.HomeSystemNode == null) return;
            string design = PickWarDesign(emp, heavy);
            if (design == null) { Fail("SpawnSquad", new Exception("no warship design among " + emp.Faction.AbstractToShipDesignDictionary.Count + " abstract designs")); return; }
            var dict = new Dictionary<StaticString, int>();
            dict[new StaticString(design)] = Math.Max(1, count);
            var order = new OrderCreateFleetFromShipDesignName(empIndex, interior.HomeSystemNode.NodePosition, dict);
            order.FleetName = new StaticString(SquadLabel(tag) + SquadMarker(tag, empIndex));
            server.PostOrder(order);
        }
        catch (Exception e) { Fail("SpawnSquad", e); }
    }

    static void SpawnContingent(int empIndex)
    {
        var emp = GetMajor(empIndex);
        if (emp == null) return;
        SpawnSquad(empIndex, CouncilCore.TagMarshal, Math.Min(8, 2 + GameStage(GetGame(), emp)), true);
    }

    static void SpawnForLaw(Game game, CouncilActiveLaw law)
    {
        Empire any = null;
        foreach (MajorEmpire e in game.EnumerableMajorEmpires) { any = e; break; }
        int stage = GameStage(game, any);
        if (law.Id == CouncilCore.LawEscort)
        {
            for (int i = 0; i < Core.Members.Count; i++) SpawnSquad(Core.Members[i], CouncilCore.TagEscort, 1 + stage / 3, false);
        }
        else if (law.Id == CouncilCore.LawAcademySquad && law.Target >= 0)
            SpawnSquad(law.Target, CouncilCore.TagAcademy, Math.Min(9, 3 + stage), true);
        else if (law.Id == CouncilCore.LawStealth && law.Target >= 0)
            SpawnSquad(law.Target, CouncilCore.TagStealth, 2 + stage / 3, false);
    }

    static bool TryParseMarker(string name, out string tag, out int emp)
    {
        tag = null; emp = -1;
        if (string.IsNullOrEmpty(name) || !name.EndsWith("]")) return false;
        int i = name.LastIndexOf("[GC");
        if (i < 0 || name.Length - i < 6) return false;
        tag = name.Substring(i + 3, 1);
        return int.TryParse(name.Substring(i + 4, name.Length - i - 5), out emp);
    }

    /// <summary>Finds freshly created squad fleets by their name marker, and recalls squads whose contract/law has ended.</summary>
    static void SyncSquads(Game game)
    {
        // 1. discovery
        foreach (MajorEmpire e in game.EnumerableMajorEmpires)
        {
            var def = e.GetAgency<DepartmentOfDefense>();
            if (def == null) continue;
            foreach (Fleet f in def.Fleets)
            {
                string tag; int owner;
                if (!TryParseMarker(f.LocalizedName, out tag, out owner)) continue;
                if (owner != e.Index || !Core.IsMember(owner)) continue;
                var ids = new List<ulong>();
                foreach (Ship s in f.Ships) ids.Add(s.GUID);
                if (ids.Count > 0) Core.AddSquadShips(tag, owner, ids);
            }
        }
        // 2. recall
        var expired = Core.ExpiredSquads();
        for (int i = 0; i < expired.Count; i++)
        {
            var sq = expired[i];
            var emp = GetMajor(sq.Emp);
            var alive = new List<ulong>();
            if (emp != null)
            {
                var def = emp.GetAgency<DepartmentOfDefense>();
                if (def != null)
                    foreach (Fleet f in def.Fleets)
                        foreach (Ship s in f.Ships) if (sq.Ships.Contains(s.GUID)) alive.Add(s.GUID);
            }
            if (alive.Count == 0) { Core.RemoveSquad(sq); continue; }
            var server = ServerController(emp);
            if (server != null) server.PostOrder(new OrderDisbandShips(sq.Emp, alive.ToArray()));
        }
    }

    /// <summary>True when any of the ships belongs to a council squad (such ships cannot be sold or scrapped).</summary>
    public static bool BlocksShips(GameEntityGUID[] ships)
    {
        try
        {
            if (ships == null || Core.Squads.Count == 0) return false;
            for (int i = 0; i < ships.Length; i++) if (Core.IsSquadShip(ships[i])) return true;
        }
        catch (Exception) { }
        return false;
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

    // ---------------------------------------------------------------- research speed and improvement cost (cost hook)

    static readonly Dictionary<TechnologyDefinition, string> quadrantCache = new Dictionary<TechnologyDefinition, string>();

    /// <summary>dev / eco / mil / "" - the research branch (quadrant) of a technology.</summary>
    static string QuadrantOf(TechnologyDefinition td)
    {
        string q;
        if (quadrantCache.TryGetValue(td, out q)) return q;
        q = "";
        try
        {
            var stage = td.TechnologyStage;
            string n = (stage != null && stage.TechnologyQuadrant != null) ? stage.TechnologyQuadrant.Name.ToString() : "";
            if (n.IndexOf("EmpireDevelopment", StringComparison.OrdinalIgnoreCase) >= 0) q = "dev";
            else if (n.IndexOf("Economy", StringComparison.OrdinalIgnoreCase) >= 0) q = "eco";
            else if (n.IndexOf("Military", StringComparison.OrdinalIgnoreCase) >= 0) q = "mil";
            else if (n.Length == 0)
            {
                // fallback: the helper the hacking code already uses to classify technologies
                if (CustomHackingHelper.MatchesQuadrant(td, "EmpireDevelopment")) q = "dev";
                else if (CustomHackingHelper.MatchesQuadrant(td, "Economy")) q = "eco";
                else if (CustomHackingHelper.MatchesQuadrant(td, "Military")) q = "mil";
            }
        }
        catch (Exception) { }
        quadrantCache[td] = q;
        return q;
    }

    /// <summary>Return hook of DepartmentOfTheTreasury.GetProductionCostWithBonus(context, provider, cost, float):
    /// research +-50% speed per branch (as a cost factor) and the Infrastructure Program (cheaper improvements).</summary>
    public static float AdjustProductionCost(float value, ICostResolutionContext context, ICostProvider provider, ICost cost)
    {
        try
        {
            if (!Core.Enabled || provider == null || cost == null) return value;
            var td = provider as TechnologyDefinition;
            if (td != null)
            {
                if (cost.ResourceName != DepartmentOfTheTreasury.Resources.EmpireResearch) return value;
                string q = QuadrantOf(td);
                if (q.Length == 0) return value;
                string up = q == "dev" ? "ResearchDevUp" : q == "eco" ? "ResearchEcoUp" : "ResearchMilUp";
                string down = q == "dev" ? "ResearchDevDown" : q == "eco" ? "ResearchEcoDown" : "ResearchMilDown";
                if (Core.IsLawActive(up)) return value / 1.5f;      // +50% speed
                if (Core.IsLawActive(down)) return value * 2f;      // -50% speed
                return value;
            }
            var imp = provider as StarSystemImprovementDefinition;
            if (imp != null && !imp.IsWonder && context != null && context.Empire != null
                && cost.ResourceName == DepartmentOfTheTreasury.Resources.SystemProduction
                && Core.IsLawActiveFor("Infrastructure", context.Empire.Index))
                return value * 0.75f;
        }
        catch (Exception) { }
        return value;
    }

    // ---------------------------------------------------------------- fleet upkeep laws

    /// <summary>Dust change of EmpireMoneyUpkeep caused by the upkeep laws (rounded to 5 to avoid constant re-registration).</summary>
    static float UpkeepDelta(MajorEmpire emp)
    {
        bool up = Core.IsLawActive("WarshipUpkeepUp");
        bool down = Core.IsLawActiveFor("UpkeepDown", emp.Index);
        if (!up && !down) return 0f;
        var def = emp.GetAgency<DepartmentOfDefense>();
        if (def == null) return 0f;
        float war = 0f, all = 0f;
        foreach (Fleet f in def.Fleets)
            foreach (Ship s in f.Ships)
            {
                float u = s.GarrisonMoneyUpkeep;
                all += u;
                if (!s.IsCivilianShip && !s.IsAcademyShip) war += u;
            }
        float d = 0f;
        if (up) d += war;              // warship upkeep +100%
        if (down) d -= 0.5f * all;     // ship upkeep -50%
        return (float)Math.Round(d / 5f) * 5f;
    }
}
