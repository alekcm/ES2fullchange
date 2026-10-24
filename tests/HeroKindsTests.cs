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

        // ---- stage 4: flavors
        Flavor fl; float mg;
        Check(HeroFlavors.Classify("SystemProduction", 1, 10f, HeroChannel.System, out fl, out mg) && fl == Flavor.Production && Math.Abs(mg - 1f) < 1e-4f, "production +10 is magnitude 1");
        Check(HeroFlavors.Classify("ShipDamage", 1, 0.4f, HeroChannel.Ship, out fl, out mg) && fl == Flavor.Offense && Math.Abs(mg - 2f) < 1e-4f, "damage +40% is magnitude 2");
        Check(HeroFlavors.Classify("SystemMoneyUpkeep", 6, -0.2f, HeroChannel.System, out fl, out mg) && fl == Flavor.Upkeep && mg > 0.9f, "upkeep reduction counts as a positive effect");
        Check(HeroFlavors.Classify("HealthRegenerationRatio", 1, 0.05f, HeroChannel.Senate, out fl, out mg) && fl == Flavor.Sustain && Math.Abs(mg - 0.5f) < 1e-4f, "senate effects count double");
        Check(!HeroFlavors.Classify("SomethingUnknown", 1, 5f, HeroChannel.System, out fl, out mg), "unknown property is dropped");
        Check(!HeroFlavors.Classify("SystemProduction", 0, 5f, HeroChannel.System, out fl, out mg), "forced values are not converted");
        Check(!HeroFlavors.Classify("SystemProduction", 1, -5f, HeroChannel.System, out fl, out mg), "penalties are not converted");
        Check(HeroFlavors.Classify("ShipMaximumMovement", 1, 2f, HeroChannel.Ship, out fl, out mg) && fl == Flavor.Mobility, "movement is mobility");
        Check(HeroFlavors.Classify("MaximumEmpireManpowerStock", 6, 0.25f, HeroChannel.Senate, out fl, out mg) && fl == Flavor.Manpower, "empire manpower is manpower");
        Check(HeroFlavors.Classify("PeacefulTreatyCostReduction", 1, -0.5f, HeroChannel.Senate, out fl, out mg) && fl == Flavor.Diplomacy && mg > 1.9f, "treaty cost reduction is diplomacy");
        Check(HeroFlavors.Classify("TradingRoutesBonus_Growth", 6, 0.4f, HeroChannel.System, out fl, out mg) && fl == Flavor.Growth, "trade route growth is growth");
        for (int fi = 0; fi < HeroFlavors.FlavorCount; fi++)
            foreach (HeroChannel ch in new[] { HeroChannel.Ship, HeroChannel.System, HeroChannel.Senate })
            {
                var targets = HeroFlavors.TargetsOf((Flavor)fi, ch);
                Check(targets.Length >= 1 || ((Flavor)fi == Flavor.Votes && ch == HeroChannel.Senate), "target for " + (Flavor)fi + "/" + ch);
                foreach (var t in targets) Check(t.Prop.Length > 0 && t.Ref != 0f && (t.Op == 1 || t.Op == 6), "valid target " + t.Prop);
                Check(HeroFlavors.Noun((Flavor)fi, ch, true).Length > 0 && HeroFlavors.Noun((Flavor)fi, ch, false).Length > 0, "noun " + (Flavor)fi + "/" + ch);
            }
        Check(HeroFlavors.TargetsOf(Flavor.Offense, HeroChannel.Hero).Length == 0, "no target for the hero channel");
        // round trip: a magnitude converted to a channel and classified back gives the same flavor and size
        for (int fi = 0; fi < HeroFlavors.FlavorCount; fi++)
        {
            if ((Flavor)fi == Flavor.Mobility || (Flavor)fi == Flavor.Diplomacy) continue;   // trade-route targets are named after what they boost
            var tg = HeroFlavors.TargetsOf((Flavor)fi, HeroChannel.System)[0];
            float v = HeroFlavors.ValueOf(tg, 1f, 1f);
            Flavor fb; float m2;
            bool ok = HeroFlavors.Classify(tg.Prop, tg.Op, v, HeroChannel.System, out fb, out m2);
            Check(ok && Math.Abs(m2 - 1f) < 0.06f || !ok, "system round trip of " + (Flavor)fi + " gives " + m2);
        }
        Check(HeroFlavors.PartyIndex("HeroPolitics03") == 3 && HeroFlavors.PartyIndex("Politics06") == 6 && HeroFlavors.PartyIndex("HeroPolitics09") == 0 && HeroFlavors.PartyIndex("") == 0 && HeroFlavors.PartyIndex(null) == 0, "party index");
        for (int pi = 1; pi <= 6; pi++) Check(HeroFlavors.PartyTheme(pi).Length == 3 && HeroFlavors.PartyName(pi, true).Length > 0 && HeroFlavors.PartyName(pi, false).Length > 0, "party " + pi);
        Check(HeroFlavors.FactionKey("HeroAffinitySophons") == "Sophons" && HeroFlavors.FactionKey("HeroAffinityTimeLords") == "TimeLords" && HeroFlavors.FactionKey("???") == "Other" && HeroFlavors.FactionKey(null) == "Other", "faction key");
        foreach (string fk in HeroFlavors.FactionKeys) Check(HeroFlavors.FactionTheme(fk).Length == 3 && HeroFlavors.FactionName(fk, true).Length > 0, "faction " + fk);
        // trees
        foreach (HeroKind k in new[] { HeroKind.Admiral, HeroKind.Governor, HeroKind.Senator })
            foreach (bool dict in new[] { false, true })
            {
                if (dict && k != HeroKind.Senator) continue;
                var plan = HeroFlavors.PlanTree(HeroFlavors.PartyTheme(1), k, dict, false);
                int skills = 0, points = 0, prev = -1;
                foreach (var st in plan) { Check(st.RequiredLevel > prev, "thresholds grow"); prev = st.RequiredLevel; foreach (var sk in st.Skills) { skills++; points += sk.Magnitudes.Length; } }
                Check(plan.Count == 4 && skills == 7, "seven skills in four stages for " + k + dict);
                Check(plan[3].Skills.Count == 1 && plan[3].Skills[0].Magnitudes.Length == 1, "single ultimate");
                if (k == HeroKind.Senator && !dict) Check(plan[0].Skills[0].Magnitudes.Length == 1 && plan[3].RequiredLevel == 6, "senator skills have one level and open early");
                if (dict) Check(plan[0].Skills[0].Magnitudes.Length == 3 && plan[3].RequiredLevel == 18 && points >= 19, "dictator skills have three levels");
                if (k != HeroKind.Senator) Check(plan[0].Skills[0].Magnitudes.Length == 2 && plan[3].RequiredLevel == 12 && points == 13, "vanilla thresholds and levels for admirals and governors");
                // the points a hero has before the last stage are enough to open it
                int below = 0; for (int s = 0; s < 3; s++) foreach (var sk in plan[s].Skills) below += sk.Magnitudes.Length;
                Check(below >= plan[3].RequiredLevel, "last stage can be opened for " + k + dict);
            }
        var auth = HeroFlavors.PlanTree(HeroFlavors.FactionTheme("Sophons"), HeroKind.Senator, true, true);
        Check(auth[3].Skills[0].Flavors.Length == 5, "dictator ultimate adds authority");
        Check(HeroFlavors.KindCode(HeroKind.Admiral, false) == "A" && HeroFlavors.KindCode(HeroKind.Senator, true) == "D" && HeroFlavors.KindCode(HeroKind.Governor, false) == "G" && HeroFlavors.KindCode(HeroKind.Senator, false) == "S", "kind codes");
        HeroKind pk; bool pd;
        Check(HeroFlavors.TryParseKindCode("D", out pk, out pd) && pd && !HeroFlavors.TryParseKindCode("X", out pk, out pd), "parse kind code");
        Check(HeroFlavors.DictatorPower > 1f, "dictator is stronger");

        // ---- votes of the Galactic Council given by senator skills
        Check(HeroFlavors.TargetsOf(Flavor.Votes, HeroChannel.Senate).Length == 0, "votes have no senate modifier (the Council reads them)");
        Check(HeroFlavors.TargetsOf(Flavor.Votes, HeroChannel.System).Length == 1 && HeroFlavors.TargetsOf(Flavor.Votes, HeroChannel.Ship).Length == 1, "governors and admirals get influence instead of votes");
        float[] vm = HeroFlavors.VoteMagnitudes("HRParty03S1@S");
        Check(vm != null && vm.Length == 1 && Math.Abs(vm[0] - 1f) < 1e-4f, "pacifist senator skill S1 is votes with magnitude 1");
        Check(HeroFlavors.VotesOfSkill("HRParty03S1@S", 1, 0) == 6 && HeroFlavors.VotesOfSkill("HRParty03S1@S", 1, 8) == 8 && HeroFlavors.VotesOfSkill("HRParty03S1@S", 1, 3) == 6, "votes: 6 flat + 1 per four systems");
        Check(HeroFlavors.VoteMagnitudes("HRParty03S0@S") == null && HeroFlavors.VoteMagnitudes("HRParty01S1@S") == null, "other skills and other parties give no votes");
        Check(HeroFlavors.VoteMagnitudes("HRParty03S1@G") == null && HeroFlavors.VoteMagnitudes("HRParty03S1@A") == null, "governors and admirals give no council votes");
        Check(HeroFlavors.VoteMagnitudes("HRParty05S2@S") != null, "religious party has a votes skill");
        var dv = HeroFlavors.VoteMagnitudes("HRParty03S1@D");
        Check(dv != null && dv.Length == 3 && HeroFlavors.VotesOfSkill("HRParty03S1@D", 3, 0) > HeroFlavors.VotesOfSkill("HRParty03S1@S", 1, 0) && HeroFlavors.VotesOfSkill("HRParty03S1@D", 9, 0) == HeroFlavors.VotesOfSkill("HRParty03S1@D", 3, 0), "dictator version is stronger and level is clamped");
        Check(HeroFlavors.VoteMagnitudes("HRDictVenetiansS2") != null && HeroFlavors.VoteMagnitudes("HRDictVenetiansS6") != null && HeroFlavors.VoteMagnitudes("HRDictSophonsS0") == null, "venetian dictator skills give votes");
        Check(HeroFlavors.VoteMagnitudes(null) == null && HeroFlavors.VoteMagnitudes("HeroSkillSomething") == null && HeroFlavors.VoteMagnitudes("HRParty") == null && HeroFlavors.VoteMagnitudes("HRPartyXXS0@S") == null && HeroFlavors.VoteMagnitudes("HRDict") == null, "garbage names give no votes");
        Check(HeroFlavors.VotesOfSkill("HeroSkillSomething", 1, 100) == 0, "vanilla skills give no votes");
        int voteSkills = 0;
        for (int pi = 1; pi <= 6; pi++)
            for (int si = 0; si < 7; si++) if (HeroFlavors.VoteMagnitudes("HRParty0" + pi + "S" + si + "@S") != null) voteSkills++;
        Check(voteSkills >= 6, "at least some senator skills give votes (" + voteSkills + ")");

        Console.WriteLine(bad == 0 ? "ALL PASSED" : bad + " FAILED");
        return bad;
    }
}
