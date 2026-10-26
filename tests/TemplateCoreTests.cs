// Standalone tests for the template catalogue maths (no game types): the variant classification is checked
// against the values the vanilla weapon modules actually carry (AIBattleSituations + SlaveModule/flak), so a
// data change upstream shows up here as a failing expectation rather than as a mislabelled template.
// Run:  source tools/env.sh; csc -out:/tmp/t/TemplateCoreTests.exe tests/TemplateCoreTests.cs src/GalacticCouncil/TemplateCore.cs && mono /tmp/t/TemplateCoreTests.exe
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

static class TemplateCoreTest
{
    static int fails;
    static void Check(bool c, string m) { if (!c) { fails++; Console.WriteLine("FAIL: " + m); } else Console.WriteLine("ok:   " + m); }

    static TemplateModuleInfo Weapon(string family, string type, float s, float m, float l,
                                     float vsEnergyArmor, float vsProjectileArmor,
                                     float vsKinetic, float vsMissile, float vsLaser, float vsBeam,
                                     bool flak)
    {
        var w = new TemplateModuleInfo
        {
            Family = family,
            WeaponType = type,
            RangeShort = s,
            RangeMedium = m,
            RangeLong = l,
            VsEnergyArmor = vsEnergyArmor,
            VsProjectileArmor = vsProjectileArmor,
            VsKinetic = vsKinetic,
            VsMissile = vsMissile,
            VsLaser = vsLaser,
            VsBeam = vsBeam,
            HasFlak = flak
        };
        return w;
    }

    static int Main()
    {
        // ---------------- the four vanilla T1 weapon families, values copied from ModuleDefinitions[Weapon].xml
        // Kinetic: Use 1/0/0, Against EnergyDefense +1 / ProjectileDefense -1, Missile +1, Laser -0.25, Beam -0.5,
        //          Fighter +0.25, Bomber +0.25, and a flak slave module (ModuleWeaponFlack1).
        var kinetic = Weapon("KineticBase", "Kinetic", 1f, 0f, 0f, 1f, -1f, 0f, 1f, -0.25f, -0.5f, true);
        // Missile: Use 0/0.5/1, Against EnergyDefense +1 / ProjectileDefense -1, Kinetic -1, Laser +0.75, Beam +1
        var missile = Weapon("MissileBase", "Missile", 0f, 0.5f, 1f, 1f, -1f, -1f, 0f, 0.75f, 1f, false);
        // Laser: Use 0.5/1/0.5, Against EnergyDefense -1 / ProjectileDefense +1, Kinetic +0.75, Missile -0.5, Beam +0.5
        var laser = Weapon("LaserBase", "Laser", 0.5f, 1f, 0.5f, -1f, 1f, 0.75f, -0.5f, 0f, 0.5f, false);
        // Beam: Use 1/1/1, Against EnergyDefense -1 / ProjectileDefense +1, Kinetic +0.5, Missile -0.75, Laser -0.75
        var beam = Weapon("BeamBase", "Beam", 1f, 1f, 1f, -1f, 1f, 0.5f, -0.75f, -0.75f, 0f, false);

        Check(TemplateCore.Classify(kinetic) == TemplateCore.Barrage, "kinetic (flak on board) -> заградительный огонь");
        Check(TemplateCore.Classify(Weapon("KineticBase", "Kinetic", 1f, 0f, 0f, 1f, -1f, 0f, 1f, -0.25f, -0.5f, false))
              == TemplateCore.Assault, "the same kinetic without its flak module -> штурмовик");
        Check(TemplateCore.Classify(missile) == TemplateCore.Sniper, "missile (0/0.5/1: only the far band is primary) -> снайпер");
        Check(TemplateCore.Classify(laser) == TemplateCore.Universal, "laser (0.5/1/0.5: an open profile) -> универсал");
        Check(TemplateCore.Classify(beam) == TemplateCore.Universal, "beam (1/1/1) -> универсал");

        // the user's rule: the variant follows the modules, not the hull/role - the same attacker hull with
        // another weapon module is another template
        Check(TemplateCore.Classify(Weapon("MissileBase", "Missile", 1f, 0f, 1f, 1f, -1f, 0f, 0f, 0f, 0f, false))
              == TemplateCore.Universal, "primary at both ends but not in the middle is still a usable-at-everything weapon");
        Check(TemplateCore.Classify(Weapon("MissileBase", "Missile", 0f, 0.5f, 1f, 1f, -1f, 0f, 0f, 0f, 0f, true))
              == TemplateCore.Barrage, "flak wins over the profile: any weapon with a flak module is a screen");
        Check(TemplateCore.Classify(Weapon("MissileStrategic1", "Missile", 1f, 1f, 1f, 1f, -1f, 0f, 0f, 0f, 0f, false))
              == TemplateCore.Universal, "a strategic missile with an open profile is judged by its profile, not by its name");

        // ---------------- damage type and armour preference
        Check(TemplateCore.DamageKindRu(kinetic) == "по энергетической броне", "kinetic (Against/EnergyDefense +1) is labelled as an anti-shield weapon");
        Check(TemplateCore.DamageKindRu(laser) == "по снарядной броне", "laser (Against/ProjectileDefense +1) is labelled as an anti-hull weapon");
        Check(TemplateCore.ArmorRu(kinetic).StartsWith("пробивает энергетическую"), "the armour line reads from the same two numbers");
        Check(TemplateCore.ArmorScore(kinetic, "EnergyDefense") > 0f && TemplateCore.ArmorScore(kinetic, "ProjectileDefense") < 0f,
              "the armour answer is scored separately (the player knows what the enemy bought)");
        Check(TemplateCore.ArmorScore(missile, "weird") == 0f, "an unknown armour name scores nothing");

        // ---------------- the wording
        string d = TemplateCore.Describe("HullSmall01Terrans", "атакующий", missile, true);
        Console.WriteLine("  missile description:\n    " + d);
        Check(d.Contains("Снайпер") || d.Contains("снайпер"), "the description carries the role word");
        Check(d.Contains("дальн"), "and says where it stands");
        Check(d.Contains("снарядн") || d.Contains("энергетич"), "and which armour it wants");
        Check(TemplateCore.Describe(null, null, beam, true).Contains("Универсал"), "a beam variant is called universal");
        Check(TemplateCore.Describe("HullLarge00Terrans", "атакующий", kinetic, true).Contains("тяжёлый"), "the hull size enters the first line");
        Check(TemplateCore.DescribeEn("HullSmall01Terrans", "attacker", missile).Contains("Sniper"), "the English text works too");
        Check(TemplateCore.Profile(missile) == "0 / 0.5 / 1 (S/M/L)", "the numbers behind the label are shown: " + TemplateCore.Profile(missile));
        Check(TemplateCore.MatchupRu(missile).Contains("плох против кинетики"), "the missile line names its bad matchup: " + TemplateCore.MatchupRu(missile));
        Check(TemplateCore.MatchupRu(laser).Contains("хорош против кинетики"), "and the laser line names its good one: " + TemplateCore.MatchupRu(laser));

        // ---------------- matchup scoring (what to answer an enemy fleet with)
        var catalogue = new List<BattleTemplate>();
        foreach (var w in new[] { kinetic, missile, laser, beam })
            catalogue.Add(new BattleTemplate
            {
                Id = TemplateCore.TemplateId("HullSmall01Terrans", "ShipRoleSmallAttacker", w.Family),
                Hull = "HullSmall01Terrans",
                Role = "ShipRoleSmallAttacker",
                Family = w.Family,
                Info = w
            });
        int vsKinetic = TemplateCore.BestAgainst(catalogue, kinetic, "HullSmall01Terrans", "ShipRoleSmallAttacker");
        Check(vsKinetic >= 0 && catalogue[vsKinetic].Family == "LaserBase",
              "against a kinetic fleet the catalogue answers with lasers (kinetic: Laser -0.25): " + catalogue[vsKinetic].Family);
        Check(TemplateCore.ScoreAgainst(laser, kinetic) > TemplateCore.ScoreAgainst(beam, kinetic),
              "and a beam scores below a laser there (its profile is open, but it is hurt by kinetics too)");
        int vsBeams = TemplateCore.BestAgainst(catalogue, beam, "HullSmall01Terrans", null);
        Check(vsBeams >= 0 && catalogue[vsBeams].Family == "MissileBase",
              "against a beam fleet it answers with missiles (missile: Beam +1): " + catalogue[vsBeams].Family);
        Check(TemplateCore.BestAgainst(null, beam, null, null) < 0, "an empty catalogue answers nothing");
        Check(TemplateCore.BestAgainst(catalogue, kinetic, "HullNothing", null) < 0, "a hull outside the catalogue answers nothing");
        Check(TemplateCore.BestAgainst(new List<BattleTemplate>(), beam, null, null) < 0, "a list without entries too");

        // ---------------- ids
        string id = TemplateCore.TemplateId("HullSmall01Terrans", "ShipRoleSmallAttacker", "MissileBase");
        Check(id == "HullSmall01Terrans_ShipRoleSmallAttacker_MissileBase", "template id format");
        Check(TemplateCore.FamilyOfId(id) == "MissileBase", "the family is recoverable from the id");
        Check(TemplateCore.FamilyOfId("NoUnderscore") == null, "a bad id yields no family");
        Check(TemplateCore.FamilyOfId(null) == null, "null too");

        // ---------------- ShortProfileText: the ship tooltip one-liner from the game's own accuracy numbers
        Check(TemplateCore.ShortProfileText(0.1f, 0.5f, 0.95f, false, null, true) == "Опасен вблизи, на дистанции промахивается.",
              "short-range hunter (10/50/95 is NOT a universal)");
        Check(TemplateCore.ShortProfileText(0.9f, 0.85f, 0.8f, false, null, true) == "Универсален на любой дистанции.",
              "all bands high and close = universal");
        Check(TemplateCore.ShortProfileText(1f, 0.5f, 0f, false, null, true) == "Бьёт издалека, в упор беспомощен.",
              "sniper");
        Check(TemplateCore.ShortProfileText(0.1f, 0.5f, 0.95f, true, "energy", true) == "Опасен вблизи, на дистанции промахивается, прикрывает флот от ракет и истребителей.",
              "flak wins the second clause over armor bias");
        Check(TemplateCore.ShortProfileText(0.5f, 0.5f, 0.5f, false, "projectile", true) == "Универсален на любой дистанции, пробивает снарядную броню.",
              "flat profile + armor");
        Check(TemplateCore.ShortProfileText(0f, 0f, 0f, false, null, false) == "No pronounced profile.",
              "empty data is honest, not 'universal'");
        Check(TemplateCore.ShortProfileText(1f, 0.6f, 0.2f, false, null, false) == "Strikes from afar, helpless at zero range.",
              "english line");

        // ---------------- the real game data, when it is fetched next to the repo (tools/fetch script)
        int fromData = CheckVanillaData();
        if (fromData == 0) Console.WriteLine("note: no /tmp/es2d/modweap.xml - the data cross-check was skipped");

        Console.WriteLine(fails == 0 ? "ALL OK" : fails + " FAILURES");
        return fails == 0 ? 0 : 1;
    }

    /// <summary>Parse the vanilla weapon modules and re-run the classification on them: it must reproduce the
    /// four labels above for the four T1 families (the file is the one the fetch script downloads).</summary>
    static int CheckVanillaData()
    {
        string path = "/tmp/es2d/modweap.xml";
        if (!File.Exists(path)) return 0;
        string xml = File.ReadAllText(path, Encoding.UTF8);
        var byFamily = new Dictionary<string, TemplateModuleInfo>();
        foreach (Match mm in Regex.Matches(xml, @"<WeaponModule\s+Name=""(?<n>[^""]+)""[^>]*?Type=""(?<t>[^""]+)""[^>]*?Family=""(?<f>[^""]+)""[^>]*?>", RegexOptions.Singleline))
        {
            string fam = mm.Groups["f"].Value;
            int at = mm.Index;
            int end = xml.IndexOf("</WeaponModule>", at);
            if (end < 0) continue;
            string body = xml.Substring(at, end - at);
            var info = new TemplateModuleInfo
            {
                Family = fam,
                WeaponType = mm.Groups["t"].Value,
                HasFlak = body.Contains("ModuleWeaponFlack") || body.Contains("Flack"),
                IsStrategic = fam.Contains("Strategic")
            };
            foreach (Match s in Regex.Matches(body, @"<AIBattleSituations Situation=""(?<k>[^""]+)"" Mode=""(?<m>[^""]+)"" Value=""(?<v>-?[\d.]+)""")
                .Cast<Match>())
            {
                float v;
                if (!float.TryParse(s.Groups["v"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out v)) continue;
                string k = s.Groups["k"].Value, mode = s.Groups["m"].Value;
                if (mode == "Use")
                {
                    if (k == "Short") info.RangeShort = v;
                    else if (k == "Medium") info.RangeMedium = v;
                    else if (k == "Long") info.RangeLong = v;
                }
                else
                {
                    if (k == "EnergyDefense") info.VsEnergyArmor = v;
                    else if (k == "ProjectileDefense") info.VsProjectileArmor = v;
                    else if (k == "Kinetic") info.VsKinetic = v;
                    else if (k == "Missile") info.VsMissile = v;
                    else if (k == "Laser") info.VsLaser = v;
                    else if (k == "Beam") info.VsBeam = v;
                    else if (k == "Fighter" || k == "Bomber") info.VsFighter = Math.Max(info.VsFighter, v);
                }
            }
            if (!byFamily.ContainsKey(fam)) byFamily[fam] = info;
        }
        TemplateModuleInfo k2, m2, l2, b2;
        byFamily.TryGetValue("KineticBase", out k2);
        byFamily.TryGetValue("MissileBase", out m2);
        byFamily.TryGetValue("LaserBase", out l2);
        byFamily.TryGetValue("BeamBase", out b2);
        bool have = k2 != null && m2 != null && l2 != null && b2 != null;
        if (!have) { Console.WriteLine("note: the weapon file does not carry all four T1 families"); return 0; }
        Check(TemplateCore.Classify(k2) == TemplateCore.Barrage, "vanilla KineticBase -> Barrage (" + TemplateCore.Profile(k2) + ")");
        Check(TemplateCore.Classify(m2) == TemplateCore.Sniper, "vanilla MissileBase -> Sniper (" + TemplateCore.Profile(m2) + ")");
        Check(TemplateCore.Classify(l2) == TemplateCore.Universal, "vanilla LaserBase -> Universal (" + TemplateCore.Profile(l2) + ")");
        Check(TemplateCore.Classify(b2) == TemplateCore.Universal, "vanilla BeamBase -> Universal (" + TemplateCore.Profile(b2) + ")");
        Check(k2.HasFlak, "kinetic carries the flak slave module in the data");
        return 1;
    }
}
