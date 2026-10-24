using System;

static class HeroKindsTests
{
    static int bad;
    static void Check(bool ok, string what) { if (!ok) { bad++; Console.WriteLine("FAIL: " + what); } }

    static int Main()
    {
        int[] count = new int[3];
        for (ulong g = 1; g <= 3000; g++) count[(int)HeroKinds.KindOfGuid(g)]++;
        Check(count[0] == 1000 && count[1] == 1000 && count[2] == 1000, "kinds are evenly spread over sequential ids");
        Check(HeroKinds.KindOfGuid(7) == HeroKinds.KindOfGuid(7), "kind is stable");

        foreach (HeroKind k in new[] { HeroKind.Admiral, HeroKind.Governor, HeroKind.Senator })
        {
            Check(HeroKinds.Allows(k, HeroChannel.Hero), k + " keeps hero-level effects");
            int channels = 0;
            foreach (HeroChannel c in new[] { HeroChannel.System, HeroChannel.Ship, HeroChannel.Senate }) if (HeroKinds.Allows(k, c)) channels++;
            Check(channels == 1, k + " uses exactly one outside channel");
        }
        Check(HeroKinds.Allows(HeroKind.Admiral, HeroChannel.Ship) && !HeroKinds.Allows(HeroKind.Admiral, HeroChannel.System), "admiral = ship");
        Check(HeroKinds.Allows(HeroKind.Governor, HeroChannel.System) && !HeroKinds.Allows(HeroKind.Governor, HeroChannel.Ship), "governor = system");
        Check(HeroKinds.Allows(HeroKind.Senator, HeroChannel.Senate) && !HeroKinds.Allows(HeroKind.Senator, HeroChannel.System), "senator = senate");

        Check(HeroKinds.ClampRequiredLevel(12, 20) == 12, "enough points below: unchanged");
        Check(HeroKinds.ClampRequiredLevel(12, 5) == 4, "few points below: lowered to 80%");
        Check(HeroKinds.ClampRequiredLevel(0, 0) == 0, "stage zero stays zero");
        Check(HeroKinds.Name(HeroKind.Governor, true) == "Губернатор" && HeroKinds.Name(HeroKind.Senator, false) == "Senator", "names");

        // ultimate auto bonus: +(100/ultimate level)% per hero level
        Check(Math.Abs(HeroKinds.AutoBonusFraction(12, 15) - 0.25f) < 1e-5f, "ult at 12, level 15 = +25%");
        Check(Math.Abs(HeroKinds.AutoBonusFraction(10, 20) - 1f) < 1e-5f, "ult at 10, level 20 = +100%");
        Check(HeroKinds.AutoBonusFraction(12, 12) == 0f && HeroKinds.AutoBonusFraction(12, 3) == 0f, "no bonus at or below the ultimate level");
        Check(HeroKinds.AutoBonusFraction(0, 2) == 1f, "degenerate ultimate level is treated as 1");
        float sc;
        Check(HeroKinds.ScaleModifierValue(1, 10f, 0.5f, out sc) && Math.Abs(sc - 5f) < 1e-5f, "addition scales linearly");
        Check(HeroKinds.ScaleModifierValue(6, 0.3f, 0.5f, out sc) && Math.Abs(sc - 0.15f) < 1e-5f, "percent scales linearly");
        Check(HeroKinds.ScaleModifierValue(3, 1.3f, 0.5f, out sc) && Math.Abs(sc - 1.15f) < 1e-5f, "multiplication scales the excess over 1");
        Check(!HeroKinds.ScaleModifierValue(0, 1f, 0.5f, out sc) && !HeroKinds.ScaleModifierValue(7, 1f, 0.5f, out sc), "force and minimum are not scaled");
        var map = new System.Collections.Generic.Dictionary<ulong, int>(); map[5UL] = 12; map[18446744073709551000UL] = 9;
        var back = HeroKinds.ParseUltimates(HeroKinds.SerializeUltimates(map));
        Check(back.Count == 2 && back[5UL] == 12 && back[18446744073709551000UL] == 9, "ultimate registry round trip");
        Check(HeroKinds.ParseUltimates("junk,3:x,:4,7:0,8:2").Count == 1, "garbage entries ignored");
        Check(HeroKinds.ParseUltimates(null).Count == 0 && HeroKinds.SerializeUltimates(new System.Collections.Generic.Dictionary<ulong, int>()) == "", "empty registry");
        Check(HeroKinds.SerializeUltimates(map).IndexOf('|') < 0 && HeroKinds.SerializeUltimates(map).IndexOf("@@") < 0, "no council separators in the token");

        Console.WriteLine(bad == 0 ? "ALL PASSED" : bad + " FAILED");
        return bad;
    }
}
