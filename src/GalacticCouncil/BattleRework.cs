// Battle rework - game glue (numbers and pure maths live in BattleCore, the design in docs/BATTLES_REWORK.md).
//
// Hooked by tools/Patcher/Patcher.cs (HookBattles):
//  * ServerGroundBattle.ComputeSpawnReport  -> ApplyCrewReserve : only the crew a ship can spare goes to the ground
//  * ServerGroundBattle.IsFinished          -> CheckSurrender   : at 0 morale the defender capitulates
//  * Ship.RefillManpower                    -> AllowedRefillFor : crews refill only in the rooted (capital-connected) network
// Driven from the existing Council turn hooks:
//  * CouncilGame.BeforeTurnEnd              -> PerTurn          : morale bookkeeping + under-crewed ship penalties
//  * CouncilGame.SerializeAttributes         -> the "BR1|" state part
//  * CouncilGame.AdjustOpponentProp          -> morale-aware attacker manpower limit bonus
using System;
using System.Collections.Generic;
using UnityEngine;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Simulation;

public static class BattleRework
{
    public static BattleCore Core = new BattleCore();

    /// <summary>Turned off (by the debug tab, or when a save predates the feature) nothing of the rework applies.</summary>
    public static bool Active { get { return Core != null && Core.Enabled; } }

    // ------------------------------------------------------------------ per-turn work

    const string CrewDescriptorType = "GalacticCouncilCrew";

    readonly static Dictionary<ulong, SysSnapshot> snapshots = new Dictionary<ulong, SysSnapshot>();
    readonly static Dictionary<ulong, CrewRecord> crew = new Dictionary<ulong, CrewRecord>();
    readonly static HashSet<ulong> militiaTemporary = new HashSet<ulong>();
    readonly static HashSet<ulong> militiaPermanent = new HashSet<ulong>();
    static int descriptorSeq;
    static float lastCut;                                                // the crew reserve taken from the last spawn report

    /// <summary>The crew the last spawn report withheld from the ground battle (debug readout).</summary>
    public static float LastCrewCut { get { return lastCut; } }

    struct SysSnapshot
    {
        public int Owner;              // empire index of the owner (-1 = none)
        public float Manpower;         // SystemManpowerStock, used to notice bombardment losses
        public int Morale;             // morale as of the last turn (0 = the defence is broken)
    }

    sealed class CrewRecord
    {
        public float Penalty = 1f;     // damage/movement multiplier currently written into the descriptor
        public int Seen;               // turn id of the last sighting (records of dead ships are dropped)
        public SimulationDescriptor Desc;
    }

    /// <summary>Called from CouncilGame's per-turn work (BeforeTurnEnd runs on the server and on the client).</summary>
    public static void PerTurn(Game game, int turn)
    {
        if (game == null || !Active) return;
        Core.LastTurn = turn;
        try { TickMorale(game, turn); } catch (Exception e) { Fail("TickMorale", e); }
        try { RefreshCrewPenalties(game, turn); } catch (Exception e) { Fail("RefreshCrewPenalties", e); }
        // the template catalogue (phase 2): idempotent, and the databases are certainly loaded at turn end
        try { BattleTemplates.RegisterVariantTemplates(); } catch (Exception e) { Fail("RegisterVariantTemplates", e); }
        try { BattleTemplates.RefreshHudOverlay(game, CouncilGame.LocalEmpire() as MajorEmpire); }
        catch (Exception e) { Fail("RefreshHudOverlay", e); }
    }

    static void TickMorale(Game game, int turn)
    {
        var repo = Services.GetService<IGroundBattleRepositoryService>();

        // 1. ground battles of this turn mark their system and carry the "citizens to the militia" tactic with them
        if (repo != null)
        {
            foreach (var gb in repo.GroundBattles)
            {
                if (gb == null) continue;
                ColonizedStarSystem cs;
                try { cs = gb.DefenderColonizedStarSystem ?? gb.TryRetrieveDefenderSystem(); }
                catch (Exception) { continue; }
                if (cs == null) continue;
                ulong id = cs.GUID;
                Core.State(id).LastBattleTurn = turn;
                var rep = gb.BattleReport;
                if (rep == null) continue;
                if (rep.TemporaryConvertedPopulations != null && rep.TemporaryConvertedPopulations.Count > 0) militiaTemporary.Add(id);
                if (rep.PermanentlyConvertedPopulations != null && rep.PermanentlyConvertedPopulations.Count > 0) militiaPermanent.Add(id);
            }
        }

        // 2. every colonised system of every living major empire, once per turn
        foreach (MajorEmpire emp in game.EnumerableMajorEmpires)
        {
            if (emp == null || emp.HasBeenEliminated) continue;
            var interior = emp.GetAgency<DepartmentOfTheInterior>();
            if (interior == null) continue;
            foreach (ColonizedStarSystem cs in interior.ColonizedStarSystems)
            {
                if (cs == null) continue;
                ulong id = cs.GUID;
                var st = Core.State(id);

                int owner = cs.Empire != null ? cs.Empire.Index : -1;
                SysSnapshot prev;
                bool known = snapshots.TryGetValue(id, out prev);
                float manpower = GetProp(cs, SimulationProperties.StarSystem.SystemManpowerStock);
                bool bombarded = known && manpower < prev.Manpower - 0.5f;         // the garrison bled out from orbit
                if (bombarded) st.LastRazeTurn = turn;
                bool besieged = IsBesieged(cs, owner);
                if (besieged) st.LastSiegeTurn = turn;
                if (known && prev.Owner != owner && owner >= 0)                    // the system just changed hands
                {
                    st.OccupationMarkTurn = turn;
                    st.Morale = Math.Min(st.Morale < 0 ? BattleCore.MoraleMax : st.Morale, BattleCore.OccupationCap(0));
                }

                bool battle = turn - st.LastBattleTurn <= 1;
                bool peaceful = !battle && !besieged && !bombarded;
                int current = st.Morale >= 0
                    ? st.Morale
                    : BattleCore.BaseMorale(GetProp(cs, SimulationProperties.StarSystem.Happiness), cs.IsHomeSystemOfCurrentOwner);
                int cap = BattleCore.OccupationCap(st.OccupationMarkTurn < 0 ? -1 : turn - st.OccupationMarkTurn);
                int morale = BattleCore.TickMorale(current, battle, besieged && !battle, bombarded, 0,
                                                    militiaTemporary.Contains(id), militiaPermanent.Contains(id),
                                                    peaceful, cap, st.Morale == 0);
                militiaTemporary.Remove(id);
                militiaPermanent.Remove(id);
                st.Morale = morale;
                if (morale != prev.Morale) st.LastChangeTurn = turn;                // cheap "something happened" marker

                snapshots[id] = new SysSnapshot { Owner = owner, Manpower = manpower, Morale = morale };
            }
        }
    }

    /// <summary>The game exposes no per-system "under siege" flag for the defender, so: a visible foreign fleet
    /// sits in orbit of the system while the owner's own fleets cannot contest it.</summary>
    static bool IsBesieged(ColonizedStarSystem cs, int ownerIndex)
    {
        try
        {
            var node = cs.Node;
            if (node == null) return false;
            foreach (Fleet f in node.DockedEntities)
            {
                if (f == null || f.Empire == null) continue;
                if (f.Empire.Index == ownerIndex) continue;
                if (f.IsInvisible) continue;
                return true;
            }
        }
        catch (Exception) { }
        return false;
    }

    // ------------------------------------------------------------------ crew reserve (assault manpower)

    /// <summary>Hook in ServerGroundBattle.ComputeSpawnReport, right after the vanilla body built the spawn report:
    /// a ship only sends to the ground the manpower it can spare, i.e. everything above the reserve its hull and
    /// crew need to keep operating. The report's initial/total attacker manpower is lowered by that reserve, and
    /// never below what the troops it already rolled actually need (an army cannot be smaller than one unit).</summary>
    public static void ApplyCrewReserve(ServerGroundBattle battle)
    {
        try
        {
            var report = battle != null ? battle.SpawnReport : null;
            if (report == null || !Active) return;
            int side = 0;                                                   // 0 = attacker, 1 = defender (the garrison)
            var node = battle.DefenderNode;
            var attacker = battle.AttackerEmpire;
            if (node == null || attacker == null) return;
            float reserve = 0f;
            foreach (Fleet f in node.DockedEntities)
            {
                if (f == null || f.Empire == null) continue;
                if (f.Empire.Index != attacker.Index) continue;             // allies contribute through their own fleets
                if (f.CannotContributeManpower) continue;
                var ships = f.Ships;
                if (ships == null) continue;
                for (int i = 0; i < ships.Count; i++)
                {
                    var s = ships[i];
                    if (s == null) continue;
                    float max = GetShipProp(s, SimulationProperties.Ship.MaximumShipManpower);
                    float stock = GetShipProp(s, SimulationProperties.Ship.ShipManpowerStock);
                    if (stock <= 0f) continue;
                    reserve += BattleCore.ReserveFor(max, stock, HullReserve(s));
                }
            }
            if (reserve <= 0f) return;
            var init = report.OpponentInitManPowers;
            var total = report.OpponentTotalManPowers;
            if (init == null || init.Length <= side) return;
            float before = init[side];
            float cut = reserve < before ? reserve : before;
            if (cut <= 0f) return;
            // The vanilla spawn sizes the pre-rolled unit counts from the attacker's initial manpower, and it never
            // lets it drop under what those counts consume. The same floor is applied here with the real per-unit
            // manpower cost, and the counts are trimmed by the same ratio so the report stays self-consistent.
            float floor_ = MinimumNeeded(attacker, report, side);
            float after = before - cut;
            if (after < floor_) after = floor_;
            if (after < 0f) after = 0f;
            if (after >= before) return;
            float ratio = before > 0f ? after / before : 0f;
            TrimCounts(report, side, ratio);
            init[side] = after;
            if (total != null && side < total.Length)
            {
                float nt = Math.Min(total[side], after);
                total[side] = nt < floor_ ? floor_ : nt;
            }
            if (report.OpponentManPowerLimits != null && side < report.OpponentManPowerLimits.Length)
            {
                float lim = report.OpponentManPowerLimits[side];
                report.OpponentManPowerLimits[side] = lim < cut ? lim : lim - cut;
            }
            lastCut = cut;                                                   // read by the pre-battle panel / the debug tab
        }
        catch (Exception e) { Fail("ApplyCrewReserve", e); }
    }

    /// <summary>The manpower the already rolled attacker troop counts consume: the battle must stay solvable,
    /// so the reserve never cuts below it. Cost per unit = the vanilla "TroopTypeManpowerCost<Type>" property.</summary>
    static float MinimumNeeded(Empire empire, GroundBattleSpawnReport report, int side)
    {
        try
        {
            var counts = report.OpponentTroopTypeInitCounts;
            if (counts == null || side >= counts.Length) return 0f;
            var perType = counts[side];
            if (perType == null) return 0f;
            float sum = 0f;
            for (int i = 0; i < perType.Length; i++)
            {
                if (perType[i] <= 0) continue;
                sum += perType[i] * TroopUnitCost(empire, i);
            }
            return sum;
        }
        catch (Exception) { return 0f; }
    }

    static readonly float[] fallbackTroopCost = new float[] { 20f, 40f, 10f };   // infantry / tank / plane, ballpark

    static float TroopUnitCost(Empire empire, int troopTypeIndex)
    {
        try
        {
            float c = empire.GetPropertyValue(TroopCostProperty(troopTypeIndex));
            if (c > 0f) return c;
        }
        catch (Exception) { }
        return fallbackTroopCost[troopTypeIndex >= 0 && troopTypeIndex < fallbackTroopCost.Length ? troopTypeIndex : 0];
    }

    static readonly StaticString[] troopCostProperties = new StaticString[3];

    /// <summary>"TroopTypeManpowerCostInfantry|Tank|Plane" - the same per-type property the vanilla spawn reads
    /// (it format-compiles SimulationProperties.Empire::TroopTypeManpowerCost with the unit type).</summary>
    static StaticString TroopCostProperty(int troopTypeIndex)
    {
        int slot = troopTypeIndex >= 0 && troopTypeIndex < troopCostProperties.Length ? troopTypeIndex : 0;
        if (troopCostProperties[slot] != null) return troopCostProperties[slot];
        string suffix = slot == 1 ? "Tank" : slot == 2 ? "Plane" : "Infantry";
        StaticString value = "TroopTypeManpowerCost" + suffix;
        troopCostProperties[slot] = value;
        return value;
    }

    /// <summary>Shrinks the pre-rolled counts of one side by the manpower ratio (a 0.5 ratio halves the army).</summary>
    static void TrimCounts(GroundBattleSpawnReport report, int side, float ratio)
    {
        try
        {
            var counts = report.OpponentTroopTypeInitCounts;
            if (counts == null || side >= counts.Length || counts[side] == null) return;
            for (int i = 0; i < counts[side].Length; i++)
            {
                int c = counts[side][i];
                if (c <= 0) continue;
                int n = (int)Math.Floor(c * ratio);
                counts[side][i] = n;
            }
        }
        catch (Exception e) { Fail("TrimCounts", e); }
    }

    /// <summary>Reserve of a ship, i.e. what stays on board (0..current crew).</summary>
    public static int ReserveOf(Ship ship)
    {
        if (ship == null) return 0;
        try
        {
            float max = GetShipProp(ship, SimulationProperties.Ship.MaximumShipManpower);
            float stock = GetShipProp(ship, SimulationProperties.Ship.ShipManpowerStock);
            return BattleCore.ReserveFor(max, stock, HullReserve(ship));
        }
        catch (Exception) { return 0; }
    }

    /// <summary>The hull part of the reserve. Vanilla writes the per-hull-class manpower into MaximumShipManpower
    /// (small 40 / small attack + support 100 / medium 300 / large carrier 900 / mothership 220 + 100 x level),
    /// which is exactly the crew a hull of that size needs to operate - so we reuse those numbers.</summary>
    public static int HullReserve(Ship ship)
    {
        try
        {
            var design = ship.ShipDesign;
            var hull = design != null ? design.Hull : null;
            string tag = null;
            try { if (hull != null && hull.Size != null && hull.Size.ToString().Length > 0) tag = hull.Size.ToString(); } catch (Exception) { }
            if ((tag == null || tag.Length == 0) && hull != null && hull.Name != null) tag = hull.Name.ToString();
            return BattleCore.HullReserve(tag, ship.IsJuggernaut, 0);
        }
        catch (Exception) { return BattleCore.CrewReserveMedium; }
    }

    /// <summary>Under-crewed ships hit weaker and move slower. One descriptor per ship, refreshed when the
    /// crew ratio moves by more than 2% (descriptors are cheap to keep, expensive to rebuild every turn).</summary>
    static void RefreshCrewPenalties(Game game, int turn)
    {
        foreach (var k in new List<ulong>(crew.Keys)) { var c = crew[k]; c.Seen = -1; }
        foreach (MajorEmpire emp in game.EnumerableMajorEmpires)
        {
            if (emp == null || emp.HasBeenEliminated) continue;
            var defense = emp.GetAgency<DepartmentOfDefense>();
            if (defense == null) continue;
            foreach (Fleet f in defense.Fleets)
            {
                if (f == null) continue;
                var ships = f.Ships;
                if (ships == null) continue;
                for (int i = 0; i < ships.Count; i++)
                {
                    var s = ships[i];
                    if (s == null) continue;
                    float stock = GetProp(s, SimulationProperties.Ship.ShipManpowerStock);
                    float max = GetProp(s, SimulationProperties.Ship.MaximumShipManpower);
                    int reserve = BattleCore.ReserveFor(max, stock, HullReserve(s));
                    float ratio = BattleCore.CrewRatio(stock, reserve);
                    float penalty = 1f - (1f - ratio) * BattleCore.LowCrewDamagePenalty;
                    ulong id = s.GUID;
                    CrewRecord rec;
                    if (!crew.TryGetValue(id, out rec)) { rec = new CrewRecord { Penalty = 1f }; crew[id] = rec; }
                    rec.Seen = turn;
                    if (ratio >= 1f)
                    {
                        if (rec.Penalty < 1f) ClearCrewDescriptor(s, rec);
                        continue;
                    }
                    if (Math.Abs(rec.Penalty - penalty) < 0.02f) continue;
                    ApplyCrewDescriptor(s, rec, penalty);
                }
            }
        }
        foreach (var id in new List<ulong>(crew.Keys))
        {
            var c = crew[id];
            if (c.Seen != turn)
            {
                // the ship is gone: drop the record, and its registered descriptor with it (the descriptor
                // database is global, an orphaned one would leak into every later save)
                if (c.Desc != null) { try { Databases.GetDatabase<SimulationDescriptor>(false).Remove(c.Desc); } catch (Exception) { } }
                crew.Remove(id);
            }
        }
    }

    static void ApplyCrewDescriptor(Ship ship, CrewRecord rec, float penalty)
    {
        try
        {
            var sim = ship.SimulationObject;
            if (sim == null) return;
            if (rec.Desc != null) { try { sim.RemoveDescriptor(rec.Desc); } catch (Exception) { } rec.Desc = null; }
            var mods = new List<SimulationModifierDescriptor>();
            mods.Add(Mod(SimulationProperties.Ship.DamagePerSecond, penalty));
            mods.Add(Mod(SimulationProperties.Ship.MaximumMovement, penalty));
            var d = CustomHackingHelper.CreateAndRegisterDescriptor(null, "GCBattleCrew_" + (++descriptorSeq),
                                                                    CrewDescriptorType, null, mods.ToArray());
            sim.AddDescriptor(d);
            rec.Desc = d;
            rec.Penalty = penalty;
            sim.Refresh();
        }
        catch (Exception e) { Fail("ApplyCrewDescriptor", e); }
    }

    static void ClearCrewDescriptor(Ship ship, CrewRecord rec)
    {
        try
        {
            if (rec.Desc != null && ship.SimulationObject != null) ship.SimulationObject.RemoveDescriptor(rec.Desc);
            if (ship.SimulationObject != null) ship.SimulationObject.RemoveDescriptorsByType(new StaticString(CrewDescriptorType));
        }
        catch (Exception) { }
        rec.Desc = null;
        rec.Penalty = 1f;
    }

    static SimulationModifierDescriptor Mod(StaticString property, float value)
    {
        return CustomHackingHelper.MakeModifier(property.ToString(), SimulationModifierDescriptor.ModifierOperation.Multiplication,
                                                value, "ClassShip");
    }

    // ------------------------------------------------------------------ surrender

    /// <summary>Hook of ServerGroundBattle.IsFinished (return): the defence capitulates as soon as its morale is gone.</summary>
    public static bool CheckSurrender(bool finished, ServerGroundBattle battle)
    {
        try
        {
            if (finished || battle == null || !Active) return finished;
            var cs = battle.TryRetrieveDefenderSystem();
            if (cs == null) return finished;
            if (!BattleCore.IsCollapsed(MoraleOf(cs))) return finished;
            var rep = battle.BattleReport;
            if (rep != null) rep.DefenderSurrendered = true;
            return true;
        }
        catch (Exception e) { Fail("CheckSurrender", e); return finished; }
    }

    /// <summary>Morale of a system: the tracked value, or the mood-derived base value while it was never attacked.
    /// A system captured this session stays under the occupation cap, which is why a counter-attack the next
    /// couple of turns takes it back without a fight - by design, see docs/BATTLES_REWORK.md.</summary>
    public static int MoraleOf(ColonizedStarSystem cs)
    {
        if (cs == null) return BattleCore.MoraleMax;
        try
        {
            ulong id = cs.GUID;
            BattleSystemState st = Core.HasState(id) ? Core.State(id) : null;
            int morale = st != null && st.Morale >= 0
                ? st.Morale
                : BattleCore.BaseMorale(GetProp(cs, SimulationProperties.StarSystem.Happiness), cs.IsHomeSystemOfCurrentOwner);
            int cap = BattleCore.OccupationCap(st == null || st.OccupationMarkTurn < 0 ? -1 : Core.LastTurn - st.OccupationMarkTurn);
            return morale > cap ? cap : morale;
        }
        catch (Exception) { return BattleCore.MoraleMax; }
    }

    // ------------------------------------------------------------------ manpower refill gate

    /// <summary>Hook replacing the read of EmpireManpowerStock inside Ship.RefillManpower: a ship may only draw
    /// from the imperial stock while it sits in the rooted network (a chain of its own systems back to the capital).
    /// Elsewhere nothing arrives from the empire, so a fleet cut off on the far side of the galaxy - or a fleet of
    /// a freshly conquered system - has to wait for the local pools.</summary>
    public static float AllowedRefillFor(Ship ship)
    {
        try
        {
            var emp = ship != null ? ship.Empire : null;
            if (emp == null || !Active || !BattleCore.RefillRequiresRootedNetwork) return StockOf(emp);
            return IsInRootedNetwork(ship) ? StockOf(emp) : 0f;
        }
        catch (Exception e) { Fail("AllowedRefillFor", e); return ship != null ? StockOf(ship.Empire) : 0f; }
    }

    static float StockOf(Empire emp)
    {
        if (emp == null) return 0f;
        return GetProp(emp, SimulationProperties.Empire.EmpireManpowerStock);
    }

    static bool IsInRootedNetwork(Ship ship)
    {
        var garrison = ship.Garrison;
        var node = garrison != null ? garrison.GetGameNode() : null;
        if (node == null) return true;                 // in a hangar / in transit: no system to judge, keep vanilla
        var emp = ship.Empire;
        if (emp == null) return true;
        var interior = emp.GetAgency<DepartmentOfTheInterior>();
        if (interior == null || !interior.CanUseRoots()) return true;
        try { return interior.IsNodeConnectedToRootNetwork(node); }
        catch (Exception) { return true; }
    }

    // ------------------------------------------------------------------ state

    public static string Serialize()
    {
        try { return Core != null ? Core.Serialize() : ""; }
        catch (Exception e) { Fail("Serialize", e); return ""; }
    }

    public static void LoadState(string data)
    {
        try
        {
            Core = BattleCore.Deserialize(data ?? "");
            snapshots.Clear();
            crew.Clear();
            militiaTemporary.Clear();
            militiaPermanent.Clear();
            BattleTemplates.ResetHudOverlay();
        }
        catch (Exception e) { Fail("LoadState", e); }
    }

    /// <summary>Reset for a new game / a reload: the rework is per-session state, never persisted config.</summary>
    public static void ResetRuntime()
    {
        snapshots.Clear();
        crew.Clear();
        militiaTemporary.Clear();
        militiaPermanent.Clear();
        BattleTemplates.ResetHudOverlay();       // the HUD label is a cloned widget of the GUI, dead after a reload
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>Current crew (true) or total crew capacity (false) of a ship - also read by the debug block.</summary>
    public static float ShipManpower(Ship ship, bool stock)
    {
        return GetShipProp(ship, stock ? SimulationProperties.Ship.ShipManpowerStock : SimulationProperties.Ship.MaximumShipManpower);
    }

    public static float SystemHappiness(ColonizedStarSystem cs)
    {
        return GetProp(cs, SimulationProperties.StarSystem.Happiness);
    }

    static float GetShipProp(Ship ship, StaticString property)
    {
        try
        {
            var sim = ship != null ? ship.SimulationObject : null;
            if (sim == null) return 0f;
            return sim.GetPropertyValue(property);
        }
        catch (Exception) { return 0f; }
    }

    static float GetProp(SimulationObjectWrapper wrapper, StaticString property)
    {
        if (wrapper == null) return 0f;
        var sim = wrapper.SimulationObject;
        if (sim == null) return 0f;
        try { return sim.GetPropertyValue(property); }
        catch (Exception) { return 0f; }
    }

    static void Fail(string where, Exception e)
    {
        try { Debug.LogWarning("[BattleRework] " + where + ": " + e.GetType().Name + ": " + e.Message); }
        catch (Exception) { }
    }
}
