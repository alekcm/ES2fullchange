// Endless Temples: pure-logic tests.
// Run: csc tests/TempleCoreTests.cs src/GalacticCouncil/TempleCore.cs && mono <exe>
using System;
using System.Collections.Generic;

static class TempleTests
{
    static int fails;
    static void Check(bool c, string m) { if (!c) { fails++; Console.WriteLine("FAIL: " + m); } else Console.WriteLine("ok:   " + m); }

    static int Main()
    {
        // ---- definitions / node pools
        Check(TempleDefs.IndexOfTech("TechnologyDefinitionEmpireDevelopmentVictory") == TempleDefs.Power, "Power tech name");
        Check(TempleDefs.IndexOfTech("TechnologyDefinitionScienceAndExplorationVictory") == TempleDefs.Genius, "Genius tech name");
        Check(TempleDefs.IndexOfTech("TechnologyDefinitionMilitaryVictory") == TempleDefs.Might, "Might tech name");
        Check(TempleDefs.IndexOfTech("TechnologyDefinitionEconomyAndTradeVictory") == TempleDefs.Wealth, "Wealth tech name");
        Check(TempleDefs.NodeMatches(TempleDefs.Power, "SpecialNodeBlackHole", ""), "Power: black hole");
        Check(TempleDefs.NodeMatches(TempleDefs.Power, "SpecialNodeRejuvenationField", ""), "Power: rejuvenation field");
        Check(!TempleDefs.NodeMatches(TempleDefs.Power, "SpecialNodeAsteroidField", ""), "Power: no asteroid field");
        Check(TempleDefs.NodeMatches(TempleDefs.Genius, "SpecialNodeSolarNebula", ""), "Genius: solar nebula");
        Check(!TempleDefs.NodeMatches(TempleDefs.Genius, "", "CategorySpecialNodeSpaceClouds"), "Genius: no space clouds");
        Check(TempleDefs.NodeMatches(TempleDefs.Might, "", "CategorySpecialNodeSpaceClouds"), "Might: space clouds via category");
        Check(TempleDefs.NodeMatches(TempleDefs.Wealth, "SpecialNodeCollapsingStar", ""), "Wealth: collapsing star");
        Check(!TempleDefs.NodeMatches(TempleDefs.Wealth, "SpecialNodeNeutronStar", ""), "Wealth: no neutron star");

        // ---- reveal assigns the shared node once
        var c = new TempleCore();
        Check(c.StateOf(0, TempleDefs.Power) == TempleCore.Hidden, "hidden at start");
        Check(c.Reveal(0, TempleDefs.Power, 4242ul, 20), "reveal to empire 0");
        Check(c.TempleNode[TempleDefs.Power] == 4242ul, "node recorded for everybody");
        Check(!c.Reveal(0, TempleDefs.Power, 7777ul, 20), "second reveal of same empire is a no-op");
        Check(c.Reveal(1, TempleDefs.Power, 7777ul, 20), "reveal to empire 1");
        Check(c.TempleNode[TempleDefs.Power] == 4242ul, "empire 1 gets the same temple node");
        Check(c.IsRevealed(1, TempleDefs.Power) && !c.IsOpened(1, TempleDefs.Power), "revealed but not opened");
        Check(c.IsTempleNode(4242ul) && !c.IsTempleNode(7777ul), "IsTempleNode");

        // ---- study: +1 per turn present, -0.5 absent, opens at the requirement
        bool opened = false;
        for (int i = 0; i < 9; i++) opened |= c.Tick(0, TempleDefs.Power, true);
        Check(!opened && c.ProgressHalf(0, TempleDefs.Power) == 18, "9 turns present = 18 halves, not open yet");
        opened = c.Tick(0, TempleDefs.Power, false);
        Check(!opened && c.ProgressHalf(0, TempleDefs.Power) == 17, "one absent turn decays by half a turn");
        Check(!c.Tick(0, TempleDefs.Power, true) && c.ProgressHalf(0, TempleDefs.Power) == 19, "back to study: 19");
        Check(c.Tick(0, TempleDefs.Power, true), "opens exactly when reaching 20 halves (10 turns)");
        Check(c.IsOpened(0, TempleDefs.Power) && !c.Tick(0, TempleDefs.Power, true), "opening reported once");
        Check(!c.IsOpened(1, TempleDefs.Power), "the other empire still has to study the same temple");

        // decay never goes below zero; hidden techs do not tick
        var c2 = new TempleCore();
        c2.Reveal(3, TempleDefs.Might, 100ul, 20);
        c2.Tick(3, TempleDefs.Might, false);
        Check(c2.ProgressHalf(3, TempleDefs.Might) == 0, "decay clamps at zero");
        Check(!c2.Tick(3, TempleDefs.Wealth, true) && c2.ProgressHalf(3, TempleDefs.Wealth) == 0, "hidden temple does not tick");

        // scaled requirement is honoured
        var c3 = new TempleCore();
        c3.Reveal(0, TempleDefs.Genius, 55ul, 40);   // 20 turns on slow speed
        for (int i = 0; i < 19; i++) c3.Tick(0, TempleDefs.Genius, true);
        Check(!c3.IsOpened(0, TempleDefs.Genius), "scaled: 19 turns not enough");
        Check(c3.Tick(0, TempleDefs.Genius, true), "scaled: opens at 20 turns");

        // no suitable node in the galaxy: opens directly (no softlock)
        var c4 = new TempleCore();
        Check(c4.Reveal(2, TempleDefs.Wealth, TempleCore.NodeNoneInGalaxy, 20), "reveal with no node");
        Check(c4.IsOpened(2, TempleDefs.Wealth), "no temple in galaxy -> opened at reveal");
        Check(c4.TempleNode[TempleDefs.Wealth] == TempleCore.NodeNoneInGalaxy, "none recorded");

        // ---- node picking: neutral preferred, deterministic
        var neutral = new List<ulong> { 10ul, 20ul, 30ul };
        var all = new List<ulong> { 10ul, 20ul, 30ul, 40ul };
        ulong p1 = TempleCore.PickTemple(neutral, all, 12345);
        ulong p2 = TempleCore.PickTemple(neutral, all, 12345);
        Check(p1 == p2 && neutral.Contains(p1), "pick is deterministic and neutral");
        Check(TempleCore.PickTemple(new List<ulong>(), all, 7) != TempleCore.NodeNoneInGalaxy, "falls back to all candidates");
        Check(TempleCore.PickTemple(new List<ulong>(), new List<ulong>(), 7) == TempleCore.NodeNoneInGalaxy, "no candidates -> none");

        // ---- serialization roundtrip
        var c5 = new TempleCore();
        c5.Reveal(0, TempleDefs.Power, 4242ul, 20);
        c5.Reveal(1, TempleDefs.Power, 4242ul, 30);
        c5.Tick(0, TempleDefs.Power, true);
        c5.Tick(0, TempleDefs.Power, false);
        c5.Reveal(1, TempleDefs.Wealth, TempleCore.NodeNoneInGalaxy, 20);
        string s = c5.Serialize();
        var r = TempleCore.Deserialize(s);
        Check(r.Serialize() == s, "serialize roundtrip exact");
        Check(r.TempleNode[TempleDefs.Power] == 4242ul && r.TempleNode[TempleDefs.Wealth] == TempleCore.NodeNoneInGalaxy, "nodes restored");
        Check(r.ProgressHalf(0, TempleDefs.Power) == 1, "progress restored (2-1)");
        Check(r.RequiredHalf(1, TempleDefs.Power) == 30, "per-empire requirement restored");
        Check(r.IsOpened(1, TempleDefs.Wealth) && !r.IsOpened(0, TempleDefs.Power), "states restored");
        Check(TempleCore.Deserialize("").Serialize() == new TempleCore().Serialize(), "empty data -> fresh core");
        Check(TempleCore.Deserialize("garbage;;").TempleNode[0] == TempleCore.NodeUnassigned, "garbage tolerated");

        // MarkOpened (already-researched tech from an old save)
        var c6 = new TempleCore();
        c6.MarkOpened(5, TempleDefs.Might);
        Check(c6.IsOpened(5, TempleDefs.Might), "MarkOpened");

        Console.WriteLine(fails == 0 ? "ALL OK" : fails + " FAILURES");
        return fails;
    }
}
