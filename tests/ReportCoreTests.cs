// Standalone tests for the battle-report aggregation (docs/BATTLES_REWORK.md §7): per-design merge, the four rows,
// the "most useful" formula and the text that the game's own label shows.
// Run:  source tools/env.sh; csc -out:/tmp/t/ReportCoreTests.exe tests/ReportCoreTests.cs src/GalacticCouncil/ReportCore.cs && mono /tmp/t/ReportCoreTests.exe
using System;
using System.Collections.Generic;

static class ReportCoreTest
{
    static int fails;
    static void Check(bool c, string m) { if (!c) { fails++; Console.WriteLine("FAIL: " + m); } else Console.WriteLine("ok:   " + m); }

    static ReportShipStat Ship(string name, string template, float dealt, float received, int shots, int hits,
                               params object[] guns)
    {
        var s = new ReportShipStat { Name = name, Template = template, Dealt = dealt, Received = received, Shots = shots, Hits = hits };
        for (int i = 0; i + 2 < guns.Length; i += 3)
            s.Weapons.Add(new ReportWeaponStat { Module = (string)guns[i], DamageType = (string)guns[i + 1], Dealt = (float)guns[i + 2] });
        return s;
    }

    static ReportWeaponStat Gun(string module, string type, float dealt, int shots, int hits)
    {
        return new ReportWeaponStat { Module = module, DamageType = type, Dealt = dealt, Shots = shots, Hits = hits };
    }

    static int Main()
    {
        return Aggregate();
    }

    static int Aggregate()
    {
        // ---------------- the empty case: nothing to show
        var empty = ReportSummary.Build(null);
        Check(!empty.HasData && empty.ToText(true).Length == 0, "no ships -> no block at all");
        Check(!ReportSummary.Build(new List<ReportShipStat>()).HasData, "an empty list is the same");

        // ---------------- two cruisers of the same design are one line ("our cruisers did the damage")
        var a = Ship("Крейсер", "Снайпер (средний)", 400f, 100f, 10, 8);
        a.Weapons.Add(Gun("ModuleWeaponMissile2", "Missile", 250f, 6, 5));
        a.Weapons.Add(Gun("ModuleWeaponKinetic1", "Kinetic", 150f, 4, 3));
        var b = Ship("Крейсер", "Снайпер (средний)", 300f, 50f, 8, 4);
        b.Weapons.Add(Gun("ModuleWeaponMissile2", "Missile", 300f, 8, 4));
        var c = Ship("Эсминец", "Заградительный огонь (малый)", 500f, 400f, 20, 9);
        c.Weapons.Add(Gun("ModuleWeaponKinetic2", "Kinetic", 500f, 20, 9));

        // a and b are ours (two cruisers of one design), c is the enemy that shot at us with kinetics
        var sum = ReportSummary.Build(new List<ReportShipStat> { a, b }, new List<ReportShipStat> { c });
        Check(sum.HasData, "data is there");
        Check(sum.TopShip == "Крейсер" && Math.Abs(sum.TopShipDamage - 700f) < 0.01f,
              "the merged design deals the most damage (700 > 500): " + sum.TopShip + " " + sum.TopShipDamage);
        Check(sum.TopModule == "ModuleWeaponMissile2" && sum.TopModuleShots == 14 && Math.Abs(sum.TopModuleDamage - 550f) < 0.01f,
              "the most used module is the merged missile (14 salvos, 550): " + sum.TopModule + " " + sum.TopModuleShots);
        Check(sum.MostUseful == "Крейсер", "most useful = damage - half the damage taken: cruiser 700-75 vs destroyer 500-200 -> " + sum.MostUseful);
        Check(sum.TopDamageType == "Kinetic" && Math.Abs(sum.TopDamageTypeAmount - 500f) < 0.01f,
              "the type we took the most of is what THEY shot with (kinetic 500): " + sum.TopDamageType + " " + sum.TopDamageTypeAmount);

        string text = sum.ToText(true);
        Console.WriteLine("  ru block:\n" + Indent(text));
        Check(text.StartsWith("Больше всех урона: "), "line 1 is the damage row");
        Check(text.Contains("Чаще всего стреляли: ModuleWeaponMissile2"), "line 2 is the module row");
        Check(text.Contains("Самый полезный корабль: Крейсер"), "line 3 is the usefulness row");
        Check(text.Contains("Больше всего получили: Кинетика"), "line 4 names the damage type in Russian: " + Last(text));
        Check(CountLines(text) == 4, "four lines, no more: " + CountLines(text));
        string en = sum.ToText(false);
        Console.WriteLine("  en block:\n" + Indent(en));
        Check(en.Contains("Most damage dealt by: ") && en.Contains("Mostly damaged by: Kinetic"), "the english wording");

        // ---------------- a battle with no shots: the block must not lie
        var dry = ReportSummary.Build(new List<ReportShipStat> { Ship("Транспорт", "", 0f, 250f, 0, 0) }, null);
        Check(dry.HasData, "the transport is still data");
        Check(dry.TopShip == "Транспорт" && dry.TopShipDamage == 0f, "and it is the 'top' with zero damage (nothing else exists)");
        Check(dry.TopModule == null, "no module row without guns");
        Check(dry.MostUseful == null, "nobody was useful here");
        Console.WriteLine("  dry block:\n" + Indent(dry.ToText(true)));
        Check(!dry.ToText(true).Contains("Чаще всего"), "the missing rows are simply absent");

        // ---------------- details block (the tooltip): one line per ship, with its template
        string det = ReportSummary.Details(new List<ReportShipStat> { a, c }, true);
        Console.WriteLine("  details:\n" + Indent(det));
        Check(det.Contains("Крейсер [Снайпер (средний)]"), "the template name is shown next to the design");
        Check(det.Contains("8/10 попаданий"), "and the hit ratio");
        Check(CountLines(det) == 2, "one line per ship");

        // ---------------- the "used most" row must not be won by a single lucky salvo
        var x = Ship("A", "", 1000f, 0f, 1, 1);
        x.Weapons.Add(Gun("ОдноСчастливоеОрудие", "Beam", 1000f, 1, 1));
        var y = Ship("B", "", 300f, 0f, 30, 28);
        y.Weapons.Add(Gun("РабочаяЛошадка", "Laser", 300f, 30, 28));
        var byShots = ReportSummary.Build(new List<ReportShipStat> { x, y }, null);
        Check(byShots.TopModule == "РабочаяЛошадка", "'most used' counts salvos, not damage: " + byShots.TopModule);
        Check(byShots.TopShip == "A", "but the damage row still goes to the big hit: " + byShots.TopShip);
        Check(Math.Abs(byShots.MostUsefulScore - 1000f) < 0.01f, "and 'most useful' too");
        var oneSided = ReportSummary.Build(new List<ReportShipStat> { Ship("A", "", 10f, 90f, 0, 0) }, null);
        Check(oneSided.MostUseful == null, "a ship that only absorbed damage is not 'most useful'");
        Check(oneSided.TopShip == "A", "but it still shows up in the damage row");

        Console.WriteLine(fails == 0 ? "ALL OK" : fails + " FAILURES");
        return fails == 0 ? 0 : 1;
    }

    static int CountLines(string s) { return string.IsNullOrEmpty(s) ? 0 : s.Split('\n').Length; }
    static string Last(string s) { int i = s.LastIndexOf('\n'); return i < 0 ? s : s.Substring(i + 1); }
    static string Indent(string s) { return "    " + (s ?? "").Replace("\n", "\n    "); }
}
