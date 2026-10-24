using System;
using System.Collections.Generic;
using System.Text;

// Tests of the admiral class trees ("hero's own ship" redesign of Guardian and Seeker) and of the
// library invariants the trees rely on. Pure data - the running game is not needed.
//   run: csc -codepage:65001 -nostdlib -langversion:6 -out:/tmp/build/HeroClassTreeTests.exe $REFS
//        tests/HeroClassTreeTests.cs $(find src/GalacticCouncil -name '*.cs')
//        cp /tmp/build/HeroClassTreeTests.exe $MANAGED/ && (cd $MANAGED && mono HeroClassTreeTests.exe)
static class HeroClassTreeTests
{
    static int bad;
    static void Check(bool ok, string what) { if (!ok) { bad++; Console.WriteLine("FAIL: " + what); } }

    static string ModText(DesignMod m)
    {
        var ops = new Dictionary<int, string> { { 1, "Add" }, { 2, "Sub" }, { 3, "Mul" }, { 6, "Pct" } };
        string op; if (!ops.TryGetValue(m.Op, out op)) op = "op" + m.Op;
        return m.Prop + " " + op + " scale " + m.Scale.ToString("0.####") + " path '" + (m.Path ?? "<null>") + "'";
    }

    static void CheckMod(string conceptKey, string prop, int op, string path, float scale)
    {
        Concept c = HeroDesign.Get(HeroChannel.Ship, conceptKey);
        Check(c != null, "concept A." + conceptKey + " exists");
        if (c == null) return;
        Check(c.Mods != null && c.Mods.Length == 1, "A." + conceptKey + " has exactly one modifier");
        if (c.Mods == null || c.Mods.Length != 1) return;
        CheckModAt(c.Mods[0], "A." + conceptKey, prop, op, path, scale);
    }

    static void CheckModAt(DesignMod m, string what, string prop, int op, string path, float scale)
    {
        Check(m.Prop == prop, what + " modifies " + prop + " (got " + m.Prop + ")");
        Check(m.Op == op, what + " operation " + op + " (got " + m.Op + ")");
        Check(m.Path == path, what + " path '" + path + "' (got '" + (m.Path ?? "<null>") + "')");
        Check(Math.Abs(m.Scale - scale) < 0.0001f, what + " scale " + scale + " (got " + m.Scale + ")");
    }

    static DesignSkill UltOf(DesignTree t)
    {
        Check(t.Stages.Count == 4, "tree has 4 stages (got " + t.Stages.Count + ")");
        Check(t.Stages[3].Skills.Count == 1, "stage 4 holds one ultimate");
        return t.Stages[3].Skills[0];
    }

    static DesignSkill TierOf(DesignTree t, int tier)
    {
        var stage = t.Stages[tier - 1];
        Check(stage.Skills.Count == 1, "stage " + tier + " holds one skill (got " + stage.Skills.Count + ")");
        return stage.Skills[0];
    }

    static void CheckStages(DesignTree t, int a, int b, int c, int d)
    {
        Check(t.Stages[0].RequiredLevel == a && t.Stages[1].RequiredLevel == b && t.Stages[2].RequiredLevel == c && t.Stages[3].RequiredLevel == d,
            "stage thresholds " + a + "/" + b + "/" + c + "/" + d + " (got " + t.Stages[0].RequiredLevel + "/" + t.Stages[1].RequiredLevel + "/" + t.Stages[2].RequiredLevel + "/" + t.Stages[3].RequiredLevel + ")");
    }

    static int Main()
    {
        // ================= Guardian (ClassAdmiral@A): the line of the hero's PERSONAL warship.
        // Ship-skill descriptors live on the hero's own ship, so an empty path means "this ship only".
        DesignTree guardian = HeroDesign.Build("ClassAdmiral", 'A');
        Check(guardian != null, "ClassAdmiral@A builds");
        if (guardian != null)
        {
            CheckStages(guardian, 0, 4, 8, 12);
            Check(TierOf(guardian, 1).Concepts[0].Key == "A.ShipDmg", "stage 1 is the hero ship weapon damage");
            Check(TierOf(guardian, 2).Concepts[0].Key == "A.ShipHp", "stage 2 is the hero ship hull");
            Check(TierOf(guardian, 3).Concepts[0].Key == "A.ShipCrit", "stage 3 is the hero ship critical chance");

            DesignSkill u = UltOf(guardian);
            var keys = new List<string>(); foreach (Concept cc in u.Concepts) keys.Add(cc.Key);
            Check(keys.Count == 4, "the ultimate combines four effects");
            Check(keys.Contains("A.ShipDmg") && keys.Contains("A.ShipHp") && keys.Contains("A.ShipCrit") && keys.Contains("A.ShipCrewKill"),
                "the ultimate = ship damage + hull + crit + crew culling (got " + string.Join(",", keys.ToArray()) + ")");
            Check(u.TitleRu == "Личный флагман" && u.TitleEn == "Personal Flagship", "the Guardian ultimate is the personal flagship");
            Check(u.Concepts.Length == u.Values[0].Length, "every ultimate concept has a value");
        }

        // the descriptors: vanilla-proven properties, empty path = the hero's own ship
        CheckMod("ShipDmg", "Damage", 1, "", 0.01f);                    // HeroSkillShip22 uses TargetProperty="Damage" on ships
        CheckMod("ShipHp", "MaximumHealth", 6, "", 0.01f);              // ship-level max health, Percent like the module descriptors
        CheckMod("ShipCrit", "CriticalHitChance", 1, "", 0.01f);        // weapon modules add flat crit on the ship
        CheckMod("ShipCrewKill", "ManpowerDamageOverTime", 6, "", 0.01f);
        CheckMod("Crew", "MaximumShipManpower", 6, HeroFlavors.ShipCrewPath, 0.01f);

        // the numbers follow the vanilla scale: one skill = up to +40% (vanilla HeroSkillShip18_4 = ShipDamage +0.40),
        // per-ship values go higher (vanilla HeroSkillShip22_3 = Damage +0.80 per ship)
        Concept cd = HeroDesign.Get(HeroChannel.Ship, "ShipDmg");
        Concept ch = HeroDesign.Get(HeroChannel.Ship, "ShipHp");
        Concept cc2 = HeroDesign.Get(HeroChannel.Ship, "ShipCrit");
        Concept ck = HeroDesign.Get(HeroChannel.Ship, "ShipCrewKill");
        Check(cd != null && cd.Value('A', 1, 5) == 40, "tier 1 weapon damage reaches the vanilla +40% at level 5 (got " + (cd == null ? "?" : cd.Value('A', 1, 5).ToString()) + ")");
        Check(ch != null && ch.Value('A', 1, 5) >= 40, "tier 1 hull is at least +40% at level 5");
        Check(cd != null && cd.Ult >= 80, "the ultimate ship damage is at least +80% (got " + (cd == null ? "?" : cd.Ult.ToString()) + ")");
        Check(ch != null && ch.Ult >= 100, "the ultimate hull is at least +100%");
        Check(cc2 != null && cc2.Ult >= 10, "the ultimate crit is at least +10%");
        Check(ck != null && ck.Ult >= 30, "the ultimate crew culling is at least +30%");

        // ================= Seeker (ClassAdventurer@A): vision first, fleet flak, mirrored stealth and detection.
        DesignTree seeker = HeroDesign.Build("ClassAdventurer", 'A');
        Check(seeker != null, "ClassAdventurer@A builds");
        if (seeker != null)
        {
            CheckStages(seeker, 0, 4, 8, 12);
            Check(seeker.Stages[0].Skills.Count == 2, "stage 1 holds the movement and vision skills (got " + seeker.Stages[0].Skills.Count + ")");
            var tier1 = new List<string>(); foreach (DesignSkill sk in seeker.Stages[0].Skills) tier1.Add(sk.Concepts[0].Key);
            Check(tier1.Contains("A.Move") && tier1.Contains("A.Vision"), "stage 1 = movement + vision (got " + string.Join(",", tier1.ToArray()) + ")");
            Check(TierOf(seeker, 2).Concepts[0].Key == "A.Flak", "stage 2 is the fleet flak");
            var tier3 = new List<string>(); foreach (DesignSkill sk in seeker.Stages[2].Skills) tier3.Add(sk.Concepts[0].Key);
            Check(tier3.Count == 2 && tier3.Contains("A.Stealth") && tier3.Contains("A.Detect"),
                "stage 3 = stealth + detection mirrors (got " + string.Join(",", tier3.ToArray()) + ")");

            // no jammer anywhere in the tree (the concept stays in the library)
            bool hasJammer = false;
            foreach (DesignStage st in seeker.Stages)
                foreach (DesignSkill sk in st.Skills)
                    foreach (Concept c in sk.Concepts)
                        if (c.Key == "A.Jammer") hasJammer = true;
            Check(!hasJammer, "the Seeker tree no longer carries the jammer (flak displaced it)");

            DesignSkill u2 = UltOf(seeker);
            Check(u2.Concepts.Length == 1 && u2.Concepts[0].Key == "A.Mothership",
                "the Seeker ultimate is the mothership hull swap alone (got " + u2.Concepts.Length + " concepts)");
            Check(u2.Concepts[0].Special == "Mothership" && u2.Concepts[0].Levels == 1,
                "the mothership is a single-level special");
            Check(u2.TitleRu == "Материнский корабль" && u2.TitleEn == "Mothership", "the Seeker ultimate is the mothership");

            // the mirrors: specials with no static modifiers, three levels, no cheap tech bypass
            Concept stl = HeroDesign.Get(HeroChannel.Ship, "Stealth");
            Concept dtc = HeroDesign.Get(HeroChannel.Ship, "Detect");
            Check(stl != null && stl.Special == "Stealth" && stl.Levels == 3, "A.Stealth is a 3-level special");
            Check(dtc != null && dtc.Special == "Detect" && dtc.Levels == 3, "A.Detect is a 3-level special");
            Check((stl == null || stl.Mods == null || stl.Mods.Length == 0) && (dtc == null || dtc.Mods == null || dtc.Mods.Length == 0),
                "the mirrors carry no static modifiers (the live level comes from the researched technology)");
            DesignSkill stlSkill = null, dtcSkill = null;
            foreach (DesignSkill sk in seeker.Stages[2].Skills) { if (sk.Concepts[0].Key == "A.Stealth") stlSkill = sk; if (sk.Concepts[0].Key == "A.Detect") dtcSkill = sk; }
            if (stlSkill != null) Check(stlSkill.Values[stlSkill.Values.Length - 1][0] == 3, "stealth skill levels display up to 3");
            if (dtcSkill != null) Check(dtcSkill.Values[dtcSkill.Values.Length - 1][0] == 3, "detection skill levels display up to 3");

            bool hasRepair = false;
            foreach (DesignStage st in seeker.Stages)
                foreach (DesignSkill sk in st.Skills)
                    foreach (Concept c in sk.Concepts)
                        if (c.Key == "A.Repair") hasRepair = true;
            Check(!hasRepair, "the Seeker class tree has no hull repair (it stays in the party/race trees)");
        }

        // ================= tech mirrors (HeroMirror): the skill level caps the best researched module
        {
            // module -> tech resolution as the game does it (TechnologyPrerequisite of the module)
            var techOf = new Dictionary<string, string>();
            techOf["ModuleSupportInvisibilityLevel1"] = "TechnologyArmyCap1";
            techOf["ModuleSupportInvisibilityDetectionLevel1"] = "TechnologyPlayDeck2";
            techOf["ModuleSupportInvisibilityDetectionLevel2"] = "TechnologyAntimatterChanneling";
            Func<string, string> moduleTech = delegate(string m) { string t; return techOf.TryGetValue(m, out t) ? t : null; };
            var researched = new List<string>();
            Func<string, bool> isResearched = delegate(string t) { return researched.Contains(t); };

            Check(HeroMirror.LevelOf(HeroMirror.Stealth, moduleTech, isResearched) == 0, "stealth mirror is 0 without the technology");
            Check(HeroMirror.LevelOf(HeroMirror.Detect, moduleTech, isResearched) == 0, "detection mirror is 0 without the technology");
            researched.Add("TechnologyArmyCap1");
            Check(HeroMirror.LevelOf(HeroMirror.Stealth, moduleTech, isResearched) == 1, "cloaker I researched -> stealth mirror 1");
            researched.Add("TechnologyPlayDeck1");
            Check(HeroMirror.LevelOf(HeroMirror.Stealth, moduleTech, isResearched) == 2, "cloaker II researched -> stealth mirror 2");
            researched.Add("TechnologyPlayDeck5");
            Check(HeroMirror.LevelOf(HeroMirror.Stealth, moduleTech, isResearched) == 3, "full-combat cloak researched -> stealth mirror 3");
            researched.Add("TechnologyPlayDeck2");
            Check(HeroMirror.LevelOf(HeroMirror.Detect, moduleTech, isResearched) == 1, "anti-cloaker I researched -> detection mirror 1");
            researched.Add("TechnologyAntimatterChanneling");
            Check(HeroMirror.LevelOf(HeroMirror.Detect, moduleTech, isResearched) == 2, "anti-cloaker II researched -> detection mirror 2");

            // fallback names are used when the module is not found at runtime (some game versions rename modules)
            Func<string, string> noModule = delegate(string m) { return null; };
            researched.Clear();
            researched.Add("TechnologyPlayDeck3");
            Check(HeroMirror.LevelOf(HeroMirror.Stealth, noModule, isResearched) == 3, "ambush cloak falls back to the verified tech name");
            researched.Clear();
            researched.Add("TechnologyAntimatterChanneling");
            Check(HeroMirror.LevelOf(HeroMirror.Detect, noModule, isResearched) == 0, "no fallback tech name for anti-cloaker II - mirror stays 0 without the module");
            Check(HeroMirror.LevelOf(null, noModule, isResearched) == 0, "unknown special mirrors nothing");

            // the modifier pair of the vanilla module, on the hero ship
            Check(string.Join(";", HeroMirror.ModifiersOf("Stealth", 3)) == "AntiVisionStrengthForModule +3", "stealth mirror = the cloak level on the ship");
            Check(string.Join(";", HeroMirror.ModifiersOf("Detect", 1)) == "VisionStrength +1", "detection 1 = vision strength only");
            Check(string.Join(";", HeroMirror.ModifiersOf("Detect", 2)) == "VisionStrength +2;MaximumShipDetectionProbeStock +2", "detection 2+ also carries probe stock");
        }

        // the fleet effects resolve from the hero's ship: vanilla hero-skill paths (./ClassGarrisonFleet, +/ClassShip)
        CheckMod("Move", "ShipMaximumMovement", 1, HeroFlavors.ShipPath, 1f);
        CheckMod("Vision", "VisionRange", 1, HeroFlavors.ShipPath, 1f);
        // the jammer stays in the library (the vanilla ModuleSupportJammer mechanic, self-targeted on the hero's ship)
        CheckMod("Jammer", "TargetingWeightBonus", 1, "", -1f);
        // the flak: the vanilla Flak Enhancer pair - FlakEfficiency on the fleet's kinetic guns,
        // +4 damage per point on the fleet's flak guns
        {
            Concept flak = HeroDesign.Get(HeroChannel.Ship, "Flak");
            Check(flak != null, "concept A.Flak exists");
            if (flak != null && flak.Mods != null && flak.Mods.Length == 2)
            {
                CheckModAt(flak.Mods[0], "A.Flak eff", "FlakEfficiency", 1, "./ClassGarrisonFleet/ClassShip/ClassSection/ClassModuleWeapon,ClassModuleWeaponKinetic", 1f);
                CheckModAt(flak.Mods[1], "A.Flak dmg", "Damage", 1, "./ClassGarrisonFleet/ClassShip/ClassSection/ClassModuleWeapon,ClassModuleWeaponFlack", 4f);
            }
            else Check(false, "A.Flak has exactly two modifiers (got " + (flak == null || flak.Mods == null ? "?" : flak.Mods.Length.ToString()) + ")");
        }

        // ================= library invariants the game relies on
        // every concept of every tree of every role must build and produce a positive number for each level
        int skillsChecked = 0;
        foreach (char role in new[] { 'A', 'G', 'S', 'D' })
        {
            foreach (string key in HeroDesign.AllKeys(role))
            {
                DesignTree t = null;
                try { t = HeroDesign.Build(key, role); }
                catch (Exception e) { Check(false, "tree " + key + "@" + role + " throws: " + e.Message); }
                if (t == null) { Check(false, "tree " + key + "@" + role + " is missing"); continue; }
                foreach (DesignStage st in t.Stages)
                    foreach (DesignSkill sk in st.Skills)
                    {
                        skillsChecked++;
                        Check(sk.Name != null && sk.Name.StartsWith("HR", StringComparison.Ordinal), "skill name of " + key + "@" + role + " is set");
                        for (int lvl = 0; lvl < sk.Levels; lvl++)
                        {
                            for (int c = 0; c < sk.Concepts.Length; c++)
                            {
                                int v = sk.Values[lvl][c];
                                Check(v > 0, sk.Name + " level " + (lvl + 1) + " effect '" + sk.Concepts[c].Ru + "' is positive (got " + v + ")");
                                Check(sk.Concepts[c].Mods != null && sk.Concepts[c].Mods.Length > 0 || sk.Concepts[c].Special != null || sk.Concepts[c].Key == "S.Votes",
                                    sk.Name + " effect '" + sk.Concepts[c].Ru + "' has modifiers, a special hook or counted votes");
                                foreach (DesignMod m in sk.Concepts[c].Mods)
                                    Check(m.Path == null || m.Path.Length == 0 || m.Path.StartsWith("./", StringComparison.Ordinal) || m.Path.StartsWith("../", StringComparison.Ordinal) || m.Path.StartsWith("Class", StringComparison.Ordinal),
                                        "path '" + m.Path + "' of " + sk.Concepts[c].Key + " is an anchored path or the empty self path");
                            }
                        }
                    }
            }
        }
        Check(skillsChecked > 200, "the whole library was walked (checked " + skillsChecked + " skills)");

        // the tier skills of the class trees do not repeat a concept (the ultimate may combine them)
        foreach (string key in new[] { "ClassAdmiral", "ClassAdventurer" })
        {
            DesignTree t = HeroDesign.Build(key, 'A');
            if (t == null) continue;
            var seen = new HashSet<string>();
            bool noRepeat = true;
            for (int s = 0; s < 3; s++)
                foreach (DesignSkill sk in t.Stages[s].Skills)
                    foreach (Concept c in sk.Concepts)
                        if (!seen.Add(c.Key)) noRepeat = false;
            Check(noRepeat, "no repeating concept in the tiers of " + key + "@A");
        }

        Console.WriteLine(bad == 0 ? "HeroClassTreeTests: all OK (" + skillsChecked + " skills)" : "HeroClassTreeTests: " + bad + " FAILURES");
        return bad == 0 ? 0 : 1;
    }
}
