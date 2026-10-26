// Battle rework / phase 2 - the "what happened" block of the game's own battle report (docs/BATTLES_REWORK.md §7).
// The pure part: what the aggregation means and how it is worded. All four rows come out of the same numbers, and
// the text is written to be dropped into the game's own label - the window's font/size/colour are inherited by the
// clone the UI side makes (BattleReportStats), so only the words are made here.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>One weapon of one ship: which module it was, what type of damage it dealt and how much.</summary>
public sealed class ReportWeaponStat
{
    public string Module;         // the module's display name
    public string DamageType;     // "Kinetic" | "Missile" | "Laser" | "Beam" (read from the module definition)
    public float Dealt;
    public int Shots;
    public int Hits;
}

/// <summary>One ship of the battle: what it dealt, what it took, and what its guns were.</summary>
public sealed class ReportShipStat
{
    public string Name;           // the ship design's display name
    public string Template = "";  // the template variant it was built from ("" when it has none)
    public float Dealt;
    public float Received;
    public int Shots;
    public int Hits;
    public readonly List<ReportWeaponStat> Weapons = new List<ReportWeaponStat>();

    /// <summary>"Most useful": the damage it put in, minus half the damage it swallowed - so a tanky ship that
    /// only stood there being shot at does not win the row, and a ship that died doing damage still counts.</summary>
    public float Usefulness { get { return Dealt - Received * 0.5f; } }
}

/// <summary>The four rows of the report block, aggregated from the ships that fought.</summary>
public sealed class ReportSummary
{
    public string TopShip;              // which ship type dealt the most damage
    public float TopShipDamage;
    public string TopModule;            // which module was used most (and how much it dealt)
    public float TopModuleDamage;
    public int TopModuleShots;
    public string MostUseful;           // who was the most useful overall
    public float MostUsefulScore;
    public string TopDamageType;        // which damage type the player took the most of
    public float TopDamageTypeAmount;
    public bool HasData;                // false -> nothing to show (the block stays hidden)

    /// <summary>Aggregate the rows. Ships of the same design are merged, because the player thinks in designs
    /// ("our cruisers did the damage", not "ship #4 did"), and the damage-type row is read from the *other* side's
    /// guns: "what did we take the most of" is a question about what was shot at us.</summary>
    public static ReportSummary Build(List<ReportShipStat> ours, List<ReportShipStat> theirs)
    {
        var s = new ReportSummary();
        var byDesign = Merge(ours, ref s.HasData);
        var byModule = new Dictionary<string, ReportWeaponStat>(StringComparer.Ordinal);
        if (ours != null)
            foreach (var ship in ours)
            {
                if (ship == null || ship.Weapons == null) continue;
                for (int i = 0; i < ship.Weapons.Count; i++)
                {
                    var w = ship.Weapons[i];
                    if (w == null || string.IsNullOrEmpty(w.Module)) continue;
                    ReportWeaponStat mw;
                    if (!byModule.TryGetValue(w.Module, out mw))
                    {
                        mw = new ReportWeaponStat { Module = w.Module, DamageType = w.DamageType };
                        byModule[w.Module] = mw;
                    }
                    mw.Dealt += w.Dealt;
                    mw.Shots += w.Shots;
                    mw.Hits += w.Hits;
                }
            }

        foreach (var kv in byDesign)
        {
            if (s.TopShip == null || kv.Value.Dealt > s.TopShipDamage) { s.TopShip = kv.Value.Name; s.TopShipDamage = kv.Value.Dealt; }
            // "most useful" only means something when somebody actually did more than stand there being shot at
            if (kv.Value.Usefulness <= 0f) continue;
            if (s.MostUseful == null || kv.Value.Usefulness > s.MostUsefulScore) { s.MostUseful = kv.Value.Name; s.MostUsefulScore = kv.Value.Usefulness; }
        }
        foreach (var kv in byModule)
        {
            // "used most" = the module the fleet actually shot with (salvos), its damage is the tie-breaker
            bool better = s.TopModule == null || kv.Value.Shots > s.TopModuleShots
                          || (kv.Value.Shots == s.TopModuleShots && kv.Value.Dealt > s.TopModuleDamage);
            if (better) { s.TopModule = kv.Value.Module; s.TopModuleShots = kv.Value.Shots; s.TopModuleDamage = kv.Value.Dealt; }
        }
        // the type split of what we took: the enemy's guns, weighted by what they dealt to us
        var byType = new Dictionary<string, float>(StringComparer.Ordinal);
        var theirModules = MergeModules(theirs);
        foreach (var kv in theirModules)
        {
            if (string.IsNullOrEmpty(kv.Value.DamageType)) continue;
            float old;
            byType.TryGetValue(kv.Value.DamageType, out old);
            byType[kv.Value.DamageType] = old + kv.Value.Dealt;
        }
        foreach (var kv in byType)
            if (s.TopDamageType == null || kv.Value > s.TopDamageTypeAmount) { s.TopDamageType = kv.Key; s.TopDamageTypeAmount = kv.Value; }
        return s;
    }

    static Dictionary<string, ReportShipStat> Merge(List<ReportShipStat> ships, ref bool hasData)
    {
        var byDesign = new Dictionary<string, ReportShipStat>(StringComparer.Ordinal);
        if (ships == null) return byDesign;
        foreach (var ship in ships)
        {
            if (ship == null) continue;
            hasData = true;
            string key = string.IsNullOrEmpty(ship.Name) ? "?" : ship.Name;
            ReportShipStat agg;
            if (!byDesign.TryGetValue(key, out agg))
            {
                agg = new ReportShipStat { Name = key, Template = ship.Template ?? "" };
                byDesign[key] = agg;
            }
            agg.Dealt += ship.Dealt;
            agg.Received += ship.Received;
            agg.Shots += ship.Shots;
            agg.Hits += ship.Hits;
            if (ship.Weapons != null)
                for (int i = 0; i < ship.Weapons.Count; i++)
                    if (ship.Weapons[i] != null) agg.Weapons.Add(ship.Weapons[i]);
        }
        return byDesign;
    }

    static Dictionary<string, ReportWeaponStat> MergeModules(List<ReportShipStat> ships)
    {
        var byModule = new Dictionary<string, ReportWeaponStat>(StringComparer.Ordinal);
        if (ships == null) return byModule;
        foreach (var ship in ships)
        {
            if (ship == null || ship.Weapons == null) continue;
            for (int i = 0; i < ship.Weapons.Count; i++)
            {
                var w = ship.Weapons[i];
                if (w == null || string.IsNullOrEmpty(w.Module)) continue;
                ReportWeaponStat mw;
                if (!byModule.TryGetValue(w.Module, out mw))
                {
                    mw = new ReportWeaponStat { Module = w.Module, DamageType = w.DamageType };
                    byModule[w.Module] = mw;
                }
                mw.Dealt += w.Dealt;
                mw.Shots += w.Shots;
                mw.Hits += w.Hits;
            }
        }
        return byModule;
    }

    /// <summary>Convenience: the rows for one side only (the damage-type row then reads that same side's guns).</summary>
    public static ReportSummary Build(List<ReportShipStat> ships) { return Build(ships, ships); }

    static readonly string[] RuTypes = { "Кинетика", "Ракеты", "Лазеры", "Лучи" };
    static readonly string[] EnTypes = { "Kinetic", "Missile", "Laser", "Beam" };

    static string TypeLabel(string type, bool russian)
    {
        if (string.IsNullOrEmpty(type)) return "";
        string[] table = russian ? RuTypes : EnTypes;
        for (int i = 0; i < EnTypes.Length; i++)
            if (string.Equals(EnTypes[i], type, StringComparison.OrdinalIgnoreCase)) return table[i];
        return russian ? char.ToUpperInvariant(type[0]) + type.Substring(1) : type;
    }

    static string Num(float v)
    {
        return (v < 0f ? 0f : v).ToString("0", CultureInfo.InvariantCulture);
    }

    /// <summary>The four lines, as one label's text (the game's label does not do columns, so every row is its own
    /// line, prefixed with the same short lead-in words a vanilla report line uses).</summary>
    public string ToText(bool russian)
    {
        if (!HasData) return "";
        var sb = new StringBuilder();
        if (russian)
        {
            if (TopShip != null) sb.Append("Больше всех урона: ").Append(TopShip).Append(" (").Append(Num(TopShipDamage)).Append(")\n");
            if (TopModule != null) sb.Append("Чаще всего стреляли: ").Append(TopModule).Append(" (").Append(TopModuleShots).Append(" залпов, ").Append(Num(TopModuleDamage)).Append(" урона)\n");
            if (MostUseful != null) sb.Append("Самый полезный корабль: ").Append(MostUseful).Append("\n");
            if (TopDamageType != null) sb.Append("Больше всего получили: ").Append(TypeLabel(TopDamageType, true)).Append(" (").Append(Num(TopDamageTypeAmount)).Append(")");
        }
        else
        {
            if (TopShip != null) sb.Append("Most damage dealt by: ").Append(TopShip).Append(" (").Append(Num(TopShipDamage)).Append(")\n");
            if (TopModule != null) sb.Append("Most used module: ").Append(TopModule).Append(" (").Append(TopModuleShots).Append(" salvos, ").Append(Num(TopModuleDamage)).Append(" damage)\n");
            if (MostUseful != null) sb.Append("Most useful ship: ").Append(MostUseful).Append("\n");
            if (TopDamageType != null) sb.Append("Mostly damaged by: ").Append(TypeLabel(TopDamageType, false)).Append(" (").Append(Num(TopDamageTypeAmount)).Append(")");
        }
        return sb.ToString().TrimEnd('\n');
    }

    /// <summary>Per-ship lines (the tooltip of the block): name, what it dealt/took and its guns' types.</summary>
    public static string Details(List<ReportShipStat> ships, bool russian)
    {
        if (ships == null || ships.Count == 0) return "";
        var sb = new StringBuilder();
        foreach (var s in ships)
        {
            if (s == null) continue;
            sb.Append(s.Name);
            if (!string.IsNullOrEmpty(s.Template)) sb.Append(" [").Append(s.Template).Append("]");
            sb.Append(": -").Append(Num(s.Dealt)).Append(russian ? " нанесено, +" : " dealt, +").Append(Num(s.Received))
              .Append(russian ? " получено" : " received");
            if (s.Shots > 0)
            {
                sb.Append(" (").Append(s.Hits).Append('/').Append(s.Shots);
                sb.Append(russian ? " попаданий" : " hits");
                sb.Append(')');
            }
            sb.Append('\n');
        }
        return sb.ToString().TrimEnd('\n');
    }
}
