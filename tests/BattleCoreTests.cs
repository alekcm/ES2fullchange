// Standalone tests for the battle rework maths (no game types): morale model, crew reserve, persistence.
// Run:  source tools/env.sh; csc -out:/tmp/t/BattleCoreTests.exe tests/BattleCoreTests.cs src/GalacticCouncil/BattleCore.cs && mono /tmp/t/BattleCoreTests.exe
using System;
using System.Collections.Generic;

static class BattleCoreTest
{
    static int fails;
    static void Check(bool c, string m) { if (!c) { fails++; Console.WriteLine("FAIL: " + m); } else Console.WriteLine("ok:   " + m); }

    static int Main()
    {
        // ---------------- crew reserve
        // the case from the design: a ship with 400 manpower, whose hull+role class puts 300 into MaximumShipManpower
        int max = 400, stock = 400, hull = 300;
        int reserve = BattleCore.ReserveFor(max, stock, hull);
        Check(reserve == 300, "400-crew ship keeps 300 aboard, reserve=" + reserve);
        Check(BattleCore.AssaultFor(stock, reserve) == 100f, "and lands 100");

        // the extra capacity bought with modules/hero skills is assault force, the reserve stays the same
        reserve = BattleCore.ReserveFor(600, 600, 200);
        Check(reserve == 200, "reserve does not grow with the cap when the hull does not, reserve=" + reserve);
        Check(BattleCore.AssaultFor(600, reserve) == 400f, "the whole surplus lands");
        reserve = BattleCore.ReserveFor(600, 600, 900);                          // large carrier hull
        Check(reserve == 450, "but never more than 75% of the crew: 600 -> 450, reserve=" + reserve);
        Check(BattleCore.AssaultFor(600, reserve) == 150f, "the carrier still keeps a landing party of 150");

        // damaged crews: the reserve cannot exceed what is left, so a burning ship lands nobody
        reserve = BattleCore.ReserveFor(400, 120, 300);
        Check(reserve == 120, "a ship below its reserve keeps everything it has, reserve=" + reserve);
        Check(BattleCore.AssaultFor(120, reserve) == 0f, "and sends nobody to the ground");
        Check(BattleCore.ReserveFor(400, 0, 300) == 0, "an empty hull has no reserve either");

        // hull table: the size word of the hull (vanilla writes the same numbers per hull class into MaximumShipManpower:
        // small civilian 40, small attack/support 100, medium 300, large carrier 900, mothership 220 + 100 per level)
        Check(BattleCore.HullReserve("ShipSizeSmall", false, 0) == BattleCore.CrewReserveSmall, "small hull -> the small reserve");
        Check(BattleCore.HullReserve("ShipSizeMedium", false, 0) == BattleCore.CrewReserveMedium, "medium hull -> 100");
        Check(BattleCore.HullReserve("ShipSizeLarge", false, 0) == BattleCore.CrewReserveLarge, "large hull -> 300");
        Check(BattleCore.HullReserve("HullSmall01Sophons", false, 0) == BattleCore.CrewReserveSmall, "a hull name works as well as the size");
        Check(BattleCore.HullReserve("HullJuggernaut", false, 0) == BattleCore.CrewReserveLarge, "juggernauts count as large");
        Check(BattleCore.HullReserve("ShipSizeMothership", true, 3) == BattleCore.CrewReserveMothership + 300, "mothership 220 + 100 per level");
        Check(BattleCore.HullReserve(null, false, 0) == BattleCore.CrewReserveMedium, "unknown hull falls back to the medium reserve");

        // under-crewed penalty curve
        Check(BattleCore.CrewRatio(400, 300) == 1f, "full crew = no penalty");
        Check(Math.Abs(BattleCore.CrewRatio(150, 300) - 0.5f) < 1e-5f, "half crewed");
        Check(BattleCore.CrewRatio(0, 300) == 0f, "no crew");
        Check(BattleCore.CrewRatio(10, 0) == 1f, "a hull without a reserve is never penalised");

        // ---------------- morale: the base value follows the population's mood
        Check(BattleCore.BaseMorale(100, true) == 100, "happy capital: morale 100");
        Check(BattleCore.BaseMorale(100, false) == 85, "happy colony: 20 + 0.8*100");
        Check(BattleCore.BaseMorale(80, false) == 84, "an ordinary colony stays under the ceiling");
        Check(BattleCore.BaseMorale(100, true) > BattleCore.BaseMorale(100, false), "only a home system can reach the full 100");
        Check(BattleCore.BaseMorale(50, false) == 60, "content colony: 60");
        Check(BattleCore.BaseMorale(0, false) == 20, "miserable colony: still 20 of civic duty");
        Check(BattleCore.BaseMorale(-30, false) == 20, "rebellion does not push it below the floor");

        // ---------------- morale: occupation. A captured system has its happiness driven to 0 by the vanilla
        // occupation malus, so its morale is at the floor: a counter-attack in the next turns takes it back
        // without a fight. That is intended (see docs/BATTLES_REWORK.md), it is not patched around.
        Check(BattleCore.OccupationCap(0) == 12, "the turn after the capture the morale is capped at 12");
        Check(BattleCore.OccupationCap(3) == 56, "it recovers linearly: 3 turns -> 56");
        Check(BattleCore.OccupationCap(BattleCore.OccupationMoraleTurns) == 100, "and is free after the occupation window");
        Check(BattleCore.OccupationCap(-1) == 100, "never captured: no cap");
        int m = BattleCore.TickMorale(90, false, false, false, 0, false, false, true, BattleCore.OccupationCap(0), false);
        Check(m == 12, "a content population that is occupied cannot recover above the cap: " + m);

        // ---------------- morale: the grind
        m = BattleCore.TickMorale(100, true, false, false, 0, false, false, false, 100, false);
        Check(m == 94, "one battle turn: -6");
        m = BattleCore.TickMorale(100, false, false, true, 0, false, false, false, 100, false);
        Check(m == 90, "one bombardment turn: -10");
        m = BattleCore.TickMorale(100, true, true, true, 0, false, false, false, 100, false);
        Check(m == 80, "assault + siege + bombardment together: -20");
        m = BattleCore.TickMorale(100, false, false, false, 0, true, false, true, 100, false);
        Check(m == 97, "a militia call-up costs 8, a peaceful turn gives 5 back");
        m = BattleCore.TickMorale(100, false, false, false, 0, false, true, false, 100, false);
        Check(m == 75, "conscripting the citizens for good is a 25-point blow");
        m = BattleCore.TickMorale(4, false, false, true, 0, false, false, false, 100, false);
        Check(m == 0, "the last dregs of morale collapse under bombardment");
        Check(BattleCore.IsCollapsed(0) && !BattleCore.IsCollapsed(1), "capitulation exactly at 0");
        m = BattleCore.TickMorale(0, false, false, false, 0, false, false, true, 100, true);
        Check(m == 0, "a capitulated system does not recover while it lies defeated");

        // ---------------- the 2-turn counter-attack scenario from the design
        var core = new BattleCore();
        var st = core.State(1234);
        st.Morale = BattleCore.BaseMorale(100, true);                  // the owner's capital, morale 100
        st.Morale = BattleCore.TickMorale(st.Morale, true, false, false, 0, false, false, false, 100, false);   // lost one assault turn
        st.OccupationMarkTurn = 40;                                    // the enemy takes it on turn 40
        st.Morale = Math.Min(st.Morale, BattleCore.OccupationCap(0));
        Check(st.Morale == 12, "after the capture: 12");
        st.Morale = BattleCore.TickMorale(st.Morale, false, false, false, 0, false, false, true, BattleCore.OccupationCap(1), false);
        Check(st.Morale == 14, "one quiet turn of occupation: 14");
        Check(!BattleCore.IsCollapsed(st.Morale), "but it is not surrendered yet");
        st.OccupationMarkTurn = 40;
        int turn = 41;
        int cap = BattleCore.OccupationCap(turn - st.OccupationMarkTurn);
        Check(cap == 27, "the cap lifts quickly: " + cap);

        // ---------------- persistence
        core = new BattleCore();
        var a = core.State(1001); a.Morale = 42; a.LastBattleTurn = 7; a.LastRazeTurn = 6; a.OccupationMarkTurn = 3;
        var b = core.State(1002); b.Morale = 0; b.LastSiegeTurn = 9;
        core.Designs[2001] = new BattleTemplateBinding { TemplateId = "HullSmall01Sophons_SmallAttacker_Beam", Hull = "HullSmall01Sophons", Role = "ShipRoleSmallAttacker", WeaponFamily = "BeamBase", DesignName = "Snipers" };
        core.LastTurn = 12;
        string data = core.Serialize();
        var back = BattleCore.Deserialize(data);
        Check(back.Enabled == core.Enabled, "enabled flag round-trips");
        Check(back.LastTurn == 12, "turn round-trips");
        Check(back.Systems[1001].Morale == 42 && back.Systems[1001].LastBattleTurn == 7 && back.Systems[1001].LastRazeTurn == 6, "system morale round-trips");
        Check(back.Systems[1002].Morale == 0 && back.Systems[1002].Besieged(9), "second system round-trips");
        BattleTemplateBinding binding;
        Check(back.Designs.TryGetValue(2001, out binding) && binding.WeaponFamily == "BeamBase" && binding.DesignName == "Snipers", "template binding round-trips (including the name with no separator damage)");
        core.Designs[2002] = new BattleTemplateBinding { TemplateId = "a:b;c", Hull = "h", Role = "r", WeaponFamily = "f", DesignName = "odd:name;split" };
        var back2 = BattleCore.Deserialize(core.Serialize());
        BattleTemplateBinding binding2;
        Check(back2.Designs.TryGetValue(2002, out binding2) && binding2.TemplateId == "a:b;c" && binding2.DesignName == "odd:name;split", "separators inside the strings survive");
        // the per-hull template choice (phase 2): travels in the 5th segment, older saves simply have none
        core.HullVariantChoice["HullSmall01Sophons"] = "HullSmall01Sophons_ShipRoleSmallAttacker_LaserBase";
        core.HullVariantChoice["HullLarge00Terrans"] = "HullLarge00Terrans_ShipRoleCarrier_MissileBase";
        var back3 = BattleCore.Deserialize(core.Serialize());
        string chosen;
        Check(back3.HullVariantChoice.TryGetValue("HullSmall01Sophons", out chosen)
              && chosen == "HullSmall01Sophons_ShipRoleSmallAttacker_LaserBase", "the hull's template choice round-trips");
        Check(back3.Designs.Count == 2 && back3.Systems.Count == 2, "and nothing else is lost when it is added");
        // the shape of a save written before the template phase existed: header + systems + designs, no 5th part
        var old = new BattleCore();
        old.LastTurn = 4;
        old.Systems[7] = new BattleSystemState { Morale = 25, LastSiegeTurn = 3 };
        old.Designs[9] = new BattleTemplateBinding { TemplateId = "t", Hull = "h", Role = "r", WeaponFamily = "f", DesignName = "n" };
        string legacy = old.Serialize();
        legacy = legacy.Substring(0, legacy.LastIndexOf('|'));                       // cut the choice segment off
        var back4 = BattleCore.Deserialize(legacy);
        Check(back4.HullVariantChoice.Count == 0, "a save without the choice segment still loads");
        Check(back4.Systems[7].Morale == 25 && back4.Designs[9].WeaponFamily == "f", "and keeps what it did have");

        Check(BattleCore.Deserialize("").Systems.Count == 0, "an old save (no data) gives a fresh, harmless state");
        Check(BattleCore.Deserialize("garbage|x|y|z").Systems.Count == 0, "broken data does not throw");
        Check(BattleCore.Deserialize("BR1|1|5|1001:30:1:2:3:4").Systems[1001].Morale == 30, "field-by-field parse");
        Check(core.Serialize() == core.Serialize(), "serialisation is stable (sorted keys)");

        // ---------------- the knobs of the design must stay inside sane ranges
        Check(BattleCore.MoraleMax == 100 && BattleCore.MoraleBase < BattleCore.MoraleMax, "morale scale");
        Check(BattleCore.CrewReserveRatio > 0.5f && BattleCore.CrewReserveRatio < 1f, "a reserve of 60..100% is a siege mechanic, not a blocker");
        Check(BattleCore.LowCrewDamagePenalty > 0f && BattleCore.LowCrewDamagePenalty <= 1f, "the under-crew penalty is a fraction");
        Check(BattleCore.MoraleMilitiaPermanent > BattleCore.MoraleMilitiaTemporary, "keeping the militia hurts more than calling them up once");

        Console.WriteLine(fails == 0 ? "ALL OK" : fails + " FAILURES");
        return fails == 0 ? 0 : 1;
    }
}
