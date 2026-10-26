// Battle rework / phase 2 - the "what happened" block inside the game's own battle report (docs/BATTLES_REWORK.md §7).
//
// The numbers come from the encounter's own per-entity statistics - the game already records them for the report
// screen (SimulationProperties.EncounterShip: DamageApplied/DamageReceived/TotalShotSent/TotalHitSent, and the same
// pair per weapon module), so nothing is recomputed from the instruction stream: we only group what the game knows.
// The text is rendered with the window's own widgets: it is appended to the tooltip of the report's damage number
// (PlayerTotalDamageTooltip, an AgeTooltip of the original window) and, when there is room, put into the report
// panel's own content table. No IMGUI, no separate window - the widgets are the game's.
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Gui;
using Amplitude.Unity.Simulation;

public static class BattleReportStats
{
    // The game's own property names for the per-entity battle statistics.
    static readonly StaticString PropDamageApplied = SimulationProperties.EncounterShip.DamageApplied;
    static readonly StaticString PropDamageReceived = SimulationProperties.EncounterShip.DamageReceived;
    static readonly StaticString PropShotsSent = SimulationProperties.EncounterShip.TotalShotSent;
    static readonly StaticString PropHitsSent = SimulationProperties.EncounterShip.TotalHitSent;
    static readonly StaticString PropModuleDamage = SimulationProperties.EncounterModule_Weapon.DamageApplied;
    static readonly StaticString PropModuleShots = SimulationProperties.EncounterModule_Weapon.TotalShot;

    readonly static Dictionary<ulong, string> lastText = new Dictionary<ulong, string>();     // per encounter: the block, computed once

    /// <summary>Hook of AdvancedEncounterReportModalWindow.Refresh (post-call): the report screen is bound, so the
    /// aggregation is added to the tooltip of the game's own damage number. Cheap: the result is cached per encounter.</summary>
    public static void Refresh(AdvancedEncounterReportModalWindow window)
    {
        try
        {
            if (window == null) return;
            Encounter encounter = window.Encounter;
            if (encounter == null) return;
            EncounterGroup mine = window.PlayerEncounterGroup;
            if (mine == null) return;

            ulong id = encounter.GUID.ToUInt64Safe();
            string text;
            if (!lastText.TryGetValue(id, out text))
            {
                text = BuildText(encounter, mine);
                lastText[id] = text;
                if (lastText.Count > 64) lastText.Clear();     // per-session cache of finished reports, no need to grow
            }
            if (string.IsNullOrEmpty(text)) return;

            var tooltip = window.PlayerTotalDamageTooltip;
            if (tooltip != null)
            {
                string current = TooltipContent(tooltip);
                if (current == null || current.IndexOf("═══", StringComparison.Ordinal) < 0)
                {
                    string block = (CouncilGame.IsRussian() ? "═══ Что в бою ═══" : "═══ Battle notes ═══") + "\n" + text;
                    string ourTpl = BattleTemplates.FleetSummaryOf(mine);
                    if (!string.IsNullOrEmpty(ourTpl))
                        block += "\n" + (CouncilGame.IsRussian() ? "наши шаблоны:" : "our templates:") + "\n" + ourTpl;
                    EncounterGroup enemyGroup = null;
                    try { enemyGroup = encounter.GetEnemyGroup(mine.Index); } catch (Exception) { }
                    string theirTpl = BattleTemplates.FleetSummaryOf(enemyGroup);
                    if (!string.IsNullOrEmpty(theirTpl))
                        block += "\n" + (CouncilGame.IsRussian() ? "шаблоны противника:" : "enemy templates:") + "\n" + theirTpl;
                    SetTooltipContent(tooltip, string.IsNullOrEmpty(current) ? block : current + "\n" + block);
                }
            }
        }
        catch (Exception e) { Fail("Refresh", e); }
    }

    // ------------------------------------------------------------------ the aggregation

    static string BuildText(Encounter encounter, EncounterGroup mine)
    {
        List<ReportShipStat> ours = ShipsOf(mine);
        EncounterGroup theirs = null;
        try { theirs = encounter.GetEnemyGroup(mine.Index); } catch (Exception) { }
        List<ReportShipStat> enemy = theirs != null ? ShipsOf(theirs) : null;
        if (ours.Count == 0 && (enemy == null || enemy.Count == 0)) return "";
        ReportSummary sum = ReportSummary.Build(ours, enemy);
        string block = sum.ToText(CouncilGame.IsRussian());
        string details = ReportSummary.Details(ours, CouncilGame.IsRussian());
        if (!string.IsNullOrEmpty(details)) block = block + "\n" + details;
        return block;
    }

    static List<ReportShipStat> ShipsOf(EncounterGroup group)
    {
        var list = new List<ReportShipStat>();
        if (group == null) return list;
        EncounterFlotilla[] flotillas = null;
        try { flotillas = group.Flotillas; } catch (Exception) { }
        if (flotillas == null) return list;
        for (int f = 0; f < flotillas.Length; f++)
        {
            EncounterShip[] ships = flotillas[f] != null ? flotillas[f].Ships : null;
            if (ships == null) continue;
            for (int s = 0; s < ships.Length; s++)
            {
                EncounterShip ship = ships[s];
                if (ship == null) continue;
                var st = new ReportShipStat
                {
                    Name = ShipName(ship),
                    Template = TemplateName(ship),
                    Dealt = Value(ship, PropDamageApplied),
                    Received = Value(ship, PropDamageReceived),
                    Shots = (int)Value(ship, PropShotsSent),
                    Hits = (int)Value(ship, PropHitsSent)
                };
                EncounterShipSection[] sections = null;
                try { sections = ship.Sections; } catch (Exception) { }
                for (int c = 0; sections != null && c < sections.Length; c++)
                {
                    EncounterModule[] mods = sections[c] != null ? sections[c].Modules : null;
                    for (int k = 0; mods != null && k < mods.Length; k++)
                    {
                        var weapon = mods[k] as EncounterModule_Weapon;
                        if (weapon == null) continue;
                        ModuleDefinition def = weapon.ModuleDefinition;
                        if (def == null) continue;
                        st.Weapons.Add(new ReportWeaponStat
                        {
                            Module = ModuleName(def),
                            DamageType = WeaponTypeOf(def),
                            Dealt = Value(weapon, PropModuleDamage),
                            Shots = (int)Value(weapon, PropModuleShots)
                        });
                    }
                }
                list.Add(st);
            }
        }
        return list;
    }

    static float Value(EncounterEntity entity, StaticString property)
    {
        try
        {
            var sim = entity != null ? entity.SimulationObject : null;
            if (sim == null) return 0f;
            return sim.GetPropertyValue(property);
        }
        catch (Exception) { return 0f; }
    }

    static string ShipName(EncounterShip ship)
    {
        try
        {
            ShipDesign design = ship.ShipDesign;
            if (design != null)
            {
                string n = design.LocalizedName;
                if (!string.IsNullOrEmpty(n)) return n;
                if (!string.IsNullOrEmpty(design.UserName)) return design.UserName;
                if (!StaticString.IsNullOrEmpty(design.Name)) return design.Name.ToString();
            }
            if (!StaticString.IsNullOrEmpty(ship.Name)) return ship.Name.ToString();
        }
        catch (Exception) { }
        return "?";
    }

    /// <summary>The template variant the design was built from ("" when it has none) - read through the catalogue,
    /// so a battle report also shows which template the enemy fleet was built from.</summary>
    static string TemplateName(EncounterShip ship)
    {
        try
        {
            ShipDesign design = ship.ShipDesign;
            if (design == null) return "";
            BattleTemplate t = BattleTemplates.Match(design);
            if (t == null || t.Variant == null) return "";
            return CouncilGame.IsRussian() ? t.Variant.NameRu : t.Variant.NameEn;
        }
        catch (Exception) { return ""; }
    }

    static string ModuleName(ModuleDefinition def)
    {
        try
        {
            string n = null;
            var prop = def.GetType().GetProperty("LocalizedName");
            if (prop != null) n = prop.GetValue(def, null) as string;
            if (string.IsNullOrEmpty(n)) n = def.Name.ToString();
            return string.IsNullOrEmpty(n) ? "?" : n;
        }
        catch (Exception) { return def.Name.ToString(); }
    }

    static string WeaponTypeOf(ModuleDefinition def)
    {
        try
        {
            var weapon = def as WeaponModuleDefinition;
            if (weapon != null) return weapon.Type.ToString();
        }
        catch (Exception) { }
        return "";
    }

    // ------------------------------------------------------------------ the game's tooltip

    static string TooltipContent(AgeTooltip tooltip)
    {
        try { return tooltip.Content; }
        catch (Exception) { return null; }
    }

    static void SetTooltipContent(AgeTooltip tooltip, string text)
    {
        try { tooltip.Content = text; }
        catch (Exception) { }
    }

    static void Fail(string where, Exception e)
    {
        try { Debug.LogWarning("[BattleReportStats] " + where + ": " + e.GetType().Name + ": " + e.Message); }
        catch (Exception) { }
    }
}

/// <summary>GameEntityGUID is a struct whose only accessor is the IConvertible one (which wants a format provider),
/// so the mod reads its private field - the same thing the game's serializer does. Shared by the battle-rework
/// files, because they all need the plain ulong identity of a game entity.</summary>
static class GameGuid
{
    static readonly FieldInfo guidField = typeof(GameEntityGUID).GetField("guid", BindingFlags.Instance | BindingFlags.NonPublic);

    public static ulong ToUInt64Safe(this GameEntityGUID g)
    {
        try
        {
            if (guidField == null) return 0UL;
            object raw = guidField.GetValue(g);
            return raw is ulong ? (ulong)raw : 0UL;
        }
        catch (Exception) { return 0UL; }
    }
}
