using System;
using System.Collections.Generic;

// Tests of the "variant 2b" Academy offer: the pure parts of the pair pick (HeroAcademy.PickPair / StableHash).
// The dialog and processor parts need the running game; the deterministic pick must not.
static class HeroAcademyTests
{
    static int bad;
    static void Check(bool ok, string what) { if (!ok) { bad++; Console.WriteLine("FAIL: " + what); } }

    static int Main()
    {
        // ---- determinism: the same candidates and seed give the same pair on every machine
        var pool = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H" };
        var p1 = HeroAcademy.PickPair(pool, 12345);
        var p2 = HeroAcademy.PickPair(pool, 12345);
        Check(p1.Count == 2 && p2.Count == 2, "a pool of many gives a pair");
        Check(p1[0] == p2[0] && p1[1] == p2[1], "the same seed picks the same pair");
        Check(p1[0] != p1[1], "the two candidates differ");
        Check(pool.Count == 8, "the input pool is not modified");

        // ---- the pair always comes out of the candidates
        bool fromPool = true;
        for (int seed = 0; seed < 200; seed++)
        {
            var pair = HeroAcademy.PickPair(pool, seed);
            if (pair.Count != 2 || !pool.Contains(pair[0]) || !pool.Contains(pair[1]) || pair[0] == pair[1]) fromPool = false;
        }
        Check(fromPool, "every seed picks two different candidates out of the pool");

        // ---- randomness: different seeds really change the pair (not the same fixed couple every time)
        var seen = new HashSet<string>();
        for (int seed = 0; seed < 200; seed++)
        {
            var pair = HeroAcademy.PickPair(pool, seed);
            seen.Add(pair[0] + "," + pair[1]);
        }
        Check(seen.Count >= 20, "many different pairs appear over the seeds (got " + seen.Count + ")");

        // ---- the pool order does not matter (the machine-local list order must not change the pick)
        var rotated = new List<string> { "E", "F", "G", "H", "A", "B", "C", "D" };
        bool orderIndependent = true;
        for (int seed = 0; seed < 100; seed++)
        {
            var a = HeroAcademy.PickPair(pool, seed);
            var b = HeroAcademy.PickPair(rotated, seed);
            if (a[0] != b[0] || a[1] != b[1]) orderIndependent = false;
        }
        Check(orderIndependent, "the pick does not depend on the order of the pool");

        // ---- small pools: one candidate gives a "pair" of one, an empty pool gives nothing
        var one = HeroAcademy.PickPair(new List<string> { "Only" }, 77);
        Check(one.Count == 1 && one[0] == "Only", "a single candidate is offered alone");
        Check(HeroAcademy.PickPair(new List<string>(), 77).Count == 0, "no candidates -> no pair");
        var two = HeroAcademy.PickPair(new List<string> { "X", "Y" }, 5);
        Check(two.Count == 2 && ((two[0] == "X" && two[1] == "Y") || (two[0] == "Y" && two[1] == "X")), "two candidates give the two");

        // ---- StableHash: stable for the same name, different across names, never throws
        Check(HeroAcademy.StableHash("Klev") == HeroAcademy.StableHash("Klev"), "hash is stable");
        Check(HeroAcademy.StableHash("Klev") != HeroAcademy.StableHash("Kren"), "names hash differently");
        Check(HeroAcademy.StableHash(null) == HeroAcademy.StableHash(null), "null does not throw");
        ulong h = HeroAcademy.StableHash("HeroClassWhatever");
        Check((HeroKind)(int)(h % 3UL) == HeroKinds.KindOfGuid(h), "KindOfGuid(StableHash) matches the spread rule");

        // ---- the kind of an unknown class name must be stable too (spread by name, not by machine)
        HeroKind k1 = HeroKinds.KindOfGuid(HeroAcademy.StableHash("ModClass"));
        HeroKind k2 = HeroKinds.KindOfGuid(HeroAcademy.StableHash("ModClass"));
        Check(k1 == k2, "an unknown class keeps its kind");

        Console.WriteLine(bad == 0 ? "HeroAcademyTests: OK" : "HeroAcademyTests: " + bad + " FAILED");
        return bad == 0 ? 0 : 1;
    }
}
