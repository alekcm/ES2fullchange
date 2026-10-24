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
        HeroKind hk;
        Check(HeroKinds.TryKindOfClass("HeroClassAdmiral", out hk) && hk == HeroKind.Admiral, "admiral class -> admiral");
        Check(HeroKinds.TryKindOfClass("HeroClassAdventurer", out hk) && hk == HeroKind.Admiral, "adventurer (explorer) class -> admiral");
        Check(HeroKinds.TryKindOfClass("HeroClassAdministrator", out hk) && hk == HeroKind.Governor, "administrator class -> governor");
        Check(HeroKinds.TryKindOfClass("HeroClassCorporate", out hk) && hk == HeroKind.Senator, "corporate class -> senator");
        Check(!HeroKinds.TryKindOfClass("HeroClassSomethingNew", out hk) && !HeroKinds.TryKindOfClass(null, out hk), "unknown class is not mapped");
        Check(HeroKinds.KindOf("HeroClassCorporate", 3UL) == HeroKind.Senator && HeroKinds.KindOf("Mod", 4UL) == HeroKinds.KindOfGuid(4UL), "class wins, unknown falls back to the id");

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

        // senator level cap and dictator
        Check(HeroKinds.LevelCapFor(HeroKind.Senator, false, -1) == HeroKinds.SenatorLevelMax, "senator without a rule cap gets the senator cap");
        Check(HeroKinds.LevelCapFor(HeroKind.Senator, false, 50) == HeroKinds.SenatorLevelMax, "a larger rule cap is lowered");
        Check(HeroKinds.LevelCapFor(HeroKind.Senator, false, 5) == -1, "a lower rule cap is kept");
        Check(HeroKinds.LevelCapFor(HeroKind.Senator, true, -1) == -1, "the dictator is not capped");
        Check(HeroKinds.LevelCapFor(HeroKind.Admiral, false, -1) == -1 && HeroKinds.LevelCapFor(HeroKind.Governor, false, 60) == -1, "admirals and governors are not capped");
        Check(HeroKinds.SenatorLevelMax >= 7 && HeroKinds.SenatorLevelMax <= 9, "senator cap is about 6-8 level-ups");
        Check(Math.Abs(HeroKinds.TotalBonusFraction(12, 12, true) - HeroKinds.DictatorUltimateBoost) < 1e-5f, "dictator boost at the ultimate");
        Check(Math.Abs(HeroKinds.TotalBonusFraction(10, 20, true) - (HeroKinds.DictatorUltimateBoost + 1f)) < 1e-5f, "dictator boost plus growth");
        Check(HeroKinds.TotalBonusFraction(12, 12, false) == 0f, "no bonus for others at the ultimate");
        Check(1f + HeroKinds.DictatorUltimateBoost >= 2.5f && 1f + HeroKinds.DictatorUltimateBoost <= 3f, "2 skill levels become about 5-6");
        var dmap = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.KeyValuePair<string, ulong>>();
        dmap[0] = new System.Collections.Generic.KeyValuePair<string, ulong>("Politics03", 77UL);
        dmap[3] = new System.Collections.Generic.KeyValuePair<string, ulong>("Politics08", 18446744073709551000UL);
        var dback = HeroKinds.ParseDictators(HeroKinds.SerializeDictators(dmap));
        Check(dback.Count == 2 && dback[0].Key == "Politics03" && dback[0].Value == 77UL && dback[3].Value == 18446744073709551000UL, "dictator registry round trip");
        Check(HeroKinds.ParseDictators("x=y,1=2,a=b=c,1==5").Count == 0 && HeroKinds.ParseDictators(null).Count == 0, "garbage dictator entries ignored");
        string u, d;
        HeroKinds.SplitState(HeroKinds.JoinState("5:12,6:9", "0=P=1"), out u, out d);
        Check(u == "5:12,6:9" && d == "0=P=1", "state round trip");
        HeroKinds.SplitState("5:12", out u, out d);
        Check(u == "5:12" && d == "", "old save state has only ultimates");
        HeroKinds.SplitState("", out u, out d);
        Check(u == "" && d == "", "empty state");
        HeroKinds.SplitState(HeroKinds.JoinState("", ""), out u, out d);
        Check(u == "" && d == "", "empty parts");
        string joined = HeroKinds.JoinState(HeroKinds.SerializeUltimates(map), HeroKinds.SerializeDictators(dmap));
        Check(joined.IndexOf("@@") < 0, "state does not contain the council separator");

        Console.WriteLine(bad == 0 ? "ALL PASSED" : bad + " FAILED");
        return bad;
    }
}
