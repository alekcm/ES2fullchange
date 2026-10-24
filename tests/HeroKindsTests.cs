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

        Console.WriteLine(bad == 0 ? "ALL PASSED" : bad + " FAILED");
        return bad;
    }
}
