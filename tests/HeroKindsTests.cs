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
        Check(HeroKinds.DictatorUltimateBoost == 0f && HeroKinds.TotalBonusFraction(12, 12, true) == 0f, "no extra multiplier for the dictator (he has five levels per skill)");
        Check(Math.Abs(HeroKinds.TotalBonusFraction(10, 20, true) - 1f) < 1e-5f, "growth after the ultimate");
        Check(HeroKinds.TotalBonusFraction(12, 12, false) == 0f, "no bonus for others at the ultimate");
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
        // stage thresholds
        Check(HeroFlavors.StageThreshold(3, HeroKind.Admiral, false) == 12 && HeroFlavors.StageThreshold(3, HeroKind.Senator, false) == 6 && HeroFlavors.StageThreshold(3, HeroKind.Senator, true) == 18, "stage thresholds");
        Check(HeroFlavors.KindCode(HeroKind.Admiral, false) == "A" && HeroFlavors.KindCode(HeroKind.Senator, true) == "D" && HeroFlavors.KindCode(HeroKind.Governor, false) == "G" && HeroFlavors.KindCode(HeroKind.Senator, false) == "S", "kind codes");
        HeroKind pk; bool pd;
        Check(HeroFlavors.TryParseKindCode("D", out pk, out pd) && pd && !HeroFlavors.TryParseKindCode("X", out pk, out pd), "parse kind code");

        // ---- the written trees (HeroTreeDesign / HeroTreeSpecs)
        var roles = new[] { 'A', 'G', 'S', 'D' };
        int treesTotal = 0, skillsTotal = 0;
        foreach (char role in roles)
        {
            var keys = HeroDesign.AllKeys(role);
            Check(keys.Count >= 20, "keys for role " + role + ": " + keys.Count);
            foreach (string key in keys)
            {
                DesignTree tree = HeroDesign.Build(key, role);
                Check(tree != null, "tree " + key + "|" + role);
                if (tree == null) continue;
                treesTotal++;
                Check(tree.Stages.Count == 4, "four stages in " + tree.TreeName);
                int prev = -1;
                var usedConcepts = new System.Collections.Generic.HashSet<string>();
                var usedGroups = new System.Collections.Generic.HashSet<string>();
                var names = new System.Collections.Generic.HashSet<string>();
                for (int si = 0; si < tree.Stages.Count; si++)
                {
                    var st = tree.Stages[si];
                    Check(st.RequiredLevel > prev, "thresholds grow in " + tree.TreeName); prev = st.RequiredLevel;
                    Check(st.RequiredLevel == HeroFlavors.StageThreshold(si, role == 'A' ? HeroKind.Admiral : role == 'G' ? HeroKind.Governor : HeroKind.Senator, role == 'D'), "threshold of stage " + si + " in " + tree.TreeName);
                    if (si == 3) Check(st.Skills.Count == 1 && st.Skills[0].Levels == 1, "single one-level ultimate in " + tree.TreeName);
                    else Check(st.Skills.Count >= 1 && st.Skills.Count <= 3, "skills per stage in " + tree.TreeName);
                    foreach (DesignSkill sk in st.Skills)
                    {
                        skillsTotal++;
                        Check(names.Add(sk.Name), "unique skill name " + sk.Name);
                        Check(sk.Concepts.Length >= 1 && sk.Values.Length >= 1 && sk.TitleRu.Length > 0 && sk.TitleEn.Length > 0, "skill has content " + sk.Name);
                        Check(HeroDesign.Describe(sk, true).Length > 0 && HeroDesign.Describe(sk, false).Length > 0, "description of " + sk.Name);
                        if (si < 3 && role != 'D')
                            Check(sk.Levels >= (role == 'S' ? 1 : 3) && sk.Levels <= 8, "levels of " + sk.Name + ": " + sk.Levels);
                        if (si < 3 && role == 'D') Check(sk.Levels >= 2 && sk.Levels <= 8, "dictator levels of " + sk.Name);
                        for (int c = 0; c < sk.Concepts.Length; c++)
                        {
                            if (si < 3)
                            {
                                Check(usedConcepts.Add(sk.Concepts[c].Key), "concept " + sk.Concepts[c].Key + " is used once in " + tree.TreeName);
                                if (sk.Concepts[c].Group != null) Check(usedGroups.Add(sk.Concepts[c].Group), "idea group " + sk.Concepts[c].Group + " is used once in " + tree.TreeName);
                            }
                            for (int l = 0; l < sk.Levels; l++) Check(sk.Values[l][c] > 0, "positive value in " + sk.Name);
                            // no fractional percentages are shown
                            for (int l = 0; l < sk.Levels; l++) { string t = HeroDesign.LevelText(sk, l, true); Check(!System.Text.RegularExpressions.Regex.IsMatch(t, @"\d[.,]\d"), "round numbers in " + sk.Name + ": " + t); }
                        }
                        // values grow with the level
                        for (int l = 1; l < sk.Levels; l++) Check(sk.Values[l][0] > sk.Values[l - 1][0], "levels grow in " + sk.Name);
                    }
                }
                Check(HeroDesign.UltimateTitle(key, true).Length > 0, "ultimate title " + key);
                Check(HeroDesign.TreeName(key, role) == tree.TreeName, "tree name " + tree.TreeName);
            }
        }
        Check(treesTotal >= 80 && skillsTotal >= 400, "trees " + treesTotal + ", skills " + skillsTotal);
        Check(HeroDesign.TreeKeyOfVanilla("HeroSkillTreeSophons") == "RaceSophons" && HeroDesign.TreeKeyOfVanilla("HeroSkillTreeAdmiral") == "ClassAdmiral" && HeroDesign.TreeKeyOfVanilla("HeroSkillTreeMinorFaction01") == "RacePirates", "vanilla tree key");
        Check(HeroDesign.TreeKeyOfVanilla("HeroSkillTreeGeneric") == null && HeroDesign.TreeKeyOfVanilla("Nonsense") == null, "unknown tree keys");

        // ---- votes of the Galactic Council given by senator and dictator skills
        int voteSkills = 0;
        foreach (char role in new[] { 'S', 'D' })
            foreach (string key in HeroDesign.AllKeys(role))
            {
                DesignTree tree = HeroDesign.Build(key, role);
                foreach (var st in tree.Stages) foreach (var sk in st.Skills)
                {
                    int[] v = HeroDesign.VotesByLevel(sk.Name);
                    if (v == null) continue;
                    voteSkills++;
                    Check(v.Length == sk.Levels && v[0] > 0 && HeroDesign.VotesOfSkill(sk.Name, 1) == v[0] && HeroDesign.VotesOfSkill(sk.Name, sk.Levels + 5) == v[v.Length - 1] && HeroDesign.VotesOfSkill(sk.Name, 0) == 0, "votes table of " + sk.Name);
                    for (int l = 1; l < v.Length; l++) Check(v[l] > v[l - 1], "votes grow in " + sk.Name);
                }
            }
        Check(voteSkills >= 12, "skills that give votes: " + voteSkills);
        Check(HeroDesign.VotesOfSkill("HeroSkillSomething", 1) == 0 && HeroDesign.VotesOfSkill(null, 1) == 0, "vanilla skills give no votes");
        Check(HeroDesign.VotesByLevel("HRParty03_0@A") == null, "admiral skills give no votes");

        // ---- special ultimates
        Check(HeroDesign.SpecialOf(HeroDesign.Build("RaceCravers", 'A').Stages[3].Skills[0].Name) == "Prisoner", "Cravers admiral ultimate: prisoner");
        Check(HeroDesign.SpecialOf(HeroDesign.Build("RaceTerrans", 'A').Stages[3].Skills[0].Name) == "Euphoria", "Terrans admiral ultimate: euphoria");
        bool sci = false; foreach (var cc in HeroDesign.Build("RaceSophons", 'A').Stages[3].Skills[0].Concepts) if (cc.Key == "A.LootSci") sci = true;
        Check(sci, "Sophons admiral ultimate: science for kills");
        Check(HeroDesign.SpecialOf("HRRaceSophons_6@A") == null && HeroDesign.SpecialOf(null) == null, "no hook for ordinary skills");
        // each concept has a way to be described and a unit
        foreach (var cpt in HeroDesign.AllConcepts()) Check(cpt.Name(true).Length > 0 && cpt.Name(false).Length > 0, "concept name " + cpt.Key);

        // ---- passive experience, visible skill points, dictator hero definitions
        Check(Math.Abs(HeroKinds.PassiveExperience(100f, false) - 10f) < 1e-4f && Math.Abs(HeroKinds.PassiveExperience(120f, true) - 20f) < 1e-4f, "passive xp: 10 turns per level for a senator, 6 for the dictator");
        Check(HeroKinds.PassiveExperience(0f, false) == 0f && HeroKinds.PassiveExperience(-5f, true) == 0f, "no passive xp when the next level asks for nothing");
        Check(HeroKinds.PassiveExperience(100f, true) > HeroKinds.PassiveExperience(100f, false), "the dictator levels faster");
        Check(HeroKinds.VisibleSkillPoints(3, true) == 0 && HeroKinds.VisibleSkillPoints(3, false) == 3 && HeroKinds.VisibleSkillPoints(0, false) == 0, "fixed heroes show no unspent points");
        Check(HeroKinds.DictatorDefinitionName("Sophons", "HeroPolitics03") == "HeroDefinitionGCDictator_Sophons_HeroPolitics03", "dictator definition name");
        Check(HeroKinds.IsDictatorDefinitionName(HeroKinds.DictatorDefinitionName("Other", "P")) && !HeroKinds.IsDictatorDefinitionName("HeroDefinitionHOPeacefulAI_1") && !HeroKinds.IsDictatorDefinitionName(null), "dictator definition name check");

        Console.WriteLine(bad == 0 ? "ALL PASSED" : bad + " FAILED");
        return bad;
    }
}
