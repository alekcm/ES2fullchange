// God-Mod debug block for the battle rework (docs/BATTLES_REWORK.md): it shows what the two simulation mechanics
// actually did - how much crew each fleet keeps aboard, and the current morale of every colonised system.
// Called from CouncilDebug.DrawCouncilTools, so it works with or without the Council (see Patcher.HookDebugTabs).
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Amplitude;
using Amplitude.Unity.Game;
using Amplitude.Unity.Simulation;

public static class BattleDebug
{
    static bool open;
    static bool showAll;
    static Vector2 templateScroll;
    static bool refitOpen;
    static Vector2 refitScroll;
    static string refitInfo = "";

    static string T(string ru, string en) { return CouncilGame.Tr(ru, en); }
    static void Say(string s) { try { CouncilUi.Toast(s); } catch (Exception) { } }

    public static void DrawBattleTools(Game game, MajorEmpire player)
    {
        try
        {
            GUILayout.Space(10);
            open = GUILayout.Toggle(open, T("══ ПЕРЕДЕЛКА БОЁВ: экипажи и мораль ══", "══ BATTLE REWORK: crews and morale ══"));
            if (!open) return;
            GUILayout.Label(T("Состояние: ", "State: ") + (BattleRework.Active ? T("включено", "on") : T("ВЫКЛЮЧЕНО", "OFF"))
                            + T("   сохранённых систем: ", "   tracked systems: ") + BattleRework.Core.Systems.Count);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(T(BattleRework.Active ? "Выключить переделку" : "Включить переделку",
                                   BattleRework.Active ? "Disable the rework" : "Enable the rework"), GUILayout.ExpandWidth(false)))
            {
                BattleRework.Core.Enabled = !BattleRework.Core.Enabled;
                BattleRework.ResetRuntime();
            }
            if (GUILayout.Button(T("Сбросить мораль", "Reset morale"), GUILayout.ExpandWidth(false))) BattleRework.Core.Systems.Clear();
            showAll = GUILayout.Toggle(showAll, T("показать все системы", "show every system"));
            GUILayout.EndHorizontal();

            GUILayout.Label(T("Резерв экипажа (флоты)", "Crew reserve (fleets)")
                            + (BattleRework.LastCrewCut > 0f ? T("   последний срез: ", "   last cut: ") + (int)BattleRework.LastCrewCut : ""));
            if (player != null)
            {
                var defense = player.GetAgency<DepartmentOfDefense>();
                if (defense != null && defense.Fleets != null)
                {
                    int shown = 0;
                    foreach (Fleet f in defense.Fleets)
                    {
                        if (f == null || f.Ships == null || f.Ships.Count == 0) continue;
                        float stock = 0f, max = 0f; int reserve = 0;
                        for (int i = 0; i < f.Ships.Count; i++)
                        {
                            var s = f.Ships[i];
                            if (s == null) continue;
                            stock += BattleRework.ShipManpower(s, true);
                            max += BattleRework.ShipManpower(s, false);
                            reserve += BattleRework.ReserveOf(s);
                        }
                        GUILayout.Label("  " + FleetName(f) + T(": экипаж ", ": crew ") + (int)stock + "/" + (int)max
                                        + T(", на бой ", ", to the ground ") + (int)(stock - reserve)
                                        + T(" (резерв ", " (reserve ") + reserve + ")");
                        if (++shown >= (showAll ? 100 : 8)) break;
                    }
                    if (shown == 0) GUILayout.Label(T("  нет флотов", "  no fleets"));
                }
            }

            if (BattleRework.SpaceBattlesEnabled) DrawTemplates();     // space battles are vanilla: no template/refit tools

            GUILayout.Label(T("Мораль систем", "System morale"));
            if (player != null)
            {
                var interior = player.GetAgency<DepartmentOfTheInterior>();
                if (interior != null)
                {
                    int shown = 0, collapsed = 0;
                    foreach (ColonizedStarSystem cs in interior.ColonizedStarSystems)
                    {
                        if (cs == null) continue;
                        int morale = BattleRework.MoraleOf(cs);
                        if (morale <= 0) collapsed++;
                        if (morale >= 100 && !showAll) continue;      // the quiet systems are only listed on demand
                        var st = BattleRework.Core.HasState(cs.GUID) ? BattleRework.Core.State(cs.GUID) : null;
                        GUILayout.Label("  " + SystemName(cs) + ": " + morale + "/100"
                                        + T("   настроение ", "   happiness ") + (int)BattleRework.SystemHappiness(cs)
                                        + (st != null && st.OccupationMarkTurn >= 0 ? T("   оккупация с хода ", "   occupied since turn ") + st.OccupationMarkTurn : "")
                                        + (st != null && st.LastBattleTurn >= 0 ? T("   бой в ходу ", "   battle turn ") + st.LastBattleTurn : "")
                                        + (st != null && st.LastRazeTurn >= 0 ? T("   бомбардировка в ходу ", "   bombarded turn ") + st.LastRazeTurn : ""));
                        if (++shown >= (showAll ? 100 : 12)) break;
                    }
                    if (shown == 0) GUILayout.Label(T("  все системы при полном морале (100)", "  every system is at full morale (100)"));
                    if (collapsed > 0) GUILayout.Label(T("  !!! капитулируют без боя: ", "  !!! capitulating without a fight: ") + collapsed);
                }
            }
        }
        catch (Exception e)
        {
            GUILayout.Label("[BattleDebug] " + e.GetType().Name + ": " + e.Message);
        }
    }

    // ---------------------------------------------------------------- templates (phase 2)

    static void DrawTemplates()
    {
        GUILayout.Label(T("Шаблоны кораблей (каталог)", "Ship templates (catalogue)"));
        GUILayout.Label("  " + BattleTemplates.Status);
        if (BattleTemplates.Count == 0)
        {
            if (GUILayout.Button(T("Построить каталог сейчас", "Build the catalogue now"), GUILayout.ExpandWidth(false))) BattleTemplates.BuildCatalog();
            return;
        }
        if (GUILayout.Button(T("Зарегистрировать шаблоны в базу игры", "Register the templates in the game database"), GUILayout.ExpandWidth(false)))
        {
            int added = BattleTemplates.RegisterVariantTemplates();
            Say(T("добавлено шаблонов: ", "templates added: ") + added);
        }
        var game = CouncilGame.GetGame();
        var player = CouncilGame.LocalEmpire() as MajorEmpire;
        var defense = player != null ? player.GetAgency<DepartmentOfDefense>() : null;
        var designs = defense != null ? defense.AvailableShipDesigns : null;
        if (designs == null || designs.Count == 0) { GUILayout.Label(T("  нет проектов кораблей", "  no ship designs")); return; }
        templateScroll = GUILayout.BeginScrollView(templateScroll, GUILayout.Height(190f));
        for (int d = 0; d < designs.Count; d++)
        {
            var design = designs[d];
            if (design == null || design.Hull == null) continue;
            string hull = design.Hull.Name.ToString();
            var variants = BattleTemplates.Variants(hull);
            if (variants.Count == 0) continue;
            string current = BattleTemplates.ChoiceForHull(hull);
            var bound = BattleTemplates.BoundOf(design);
            GUILayout.BeginHorizontal();
            GUILayout.Label((design.UserName ?? design.Name.ToString()) + T("  [", "  [") + hull + "]  "
                            + T("шаблон проекта: ", "design template: ") + (bound != null ? bound.Id : T("нет", "none")));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            for (int v = 0; v < variants.Count; v++)
            {
                var t = variants[v];
                string label = (CouncilGame.IsRussian() ? t.Variant.NameRu : t.Variant.NameEn);
                GUI.enabled = t.Id != current;
                if (GUILayout.Button(label, GUILayout.ExpandWidth(false)))
                {
                    BattleTemplates.SetChoice(design, t.Id);
                    Say(T("для корпуса ", "hull set to: ") + hull + " -> " + label);
                }
                GUI.enabled = true;
            }
            if (!string.IsNullOrEmpty(current))
            {
                if (GUILayout.Button(T("сбросить", "clear"), GUILayout.ExpandWidth(false)))
                {
                    BattleTemplates.SetChoiceForHull(hull, null);
                    Say(T("шаблон корпуса сброшен: ", "hull template cleared: ") + hull);
                }
            }
            GUILayout.EndHorizontal();
            var cur = BattleTemplates.ById(current);
            if (cur != null) GUILayout.Label("     " + cur.Description);
        }
        GUILayout.EndScrollView();
        DrawRefit(game, player);
    }

    /// <summary>docs/BATTLES_REWORK.md §8: refit the ships of an existing fleet to a template variant. The design is
    /// the empire's (not the ship's), so one row shows how many ships share it; the actual work is
    /// BattleTemplates.RefitShip (SetChoice + the game's own ApplyShipDesignToShip). This is the God-Mod panel
    /// version of the feature: the playtest has to show whether the vanilla refit path needs more than that
    /// (costs, a docked requirement), and only then does it get its own order + native window.</summary>
    static void DrawRefit(Game game, MajorEmpire player)
    {
        var defense = player != null ? player.GetAgency<DepartmentOfDefense>() : null;
        if (defense == null || defense.Fleets == null) return;
        GUILayout.Space(6);
        refitOpen = GUILayout.Toggle(refitOpen, T("══ ПЕРЕВОРУЖЕНИЕ ФЛОТОВ ══", "══ FLEET REFIT ══"));
        if (!refitOpen) return;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(T("лучший шаблон для всего флота", "best template for each hull"), GUILayout.ExpandWidth(false)))
        {
            int n = 0;
            foreach (Fleet f in defense.Fleets)
            {
                if (f == null || f.Ships == null) continue;
                for (int i = 0; i < f.Ships.Count; i++) if (RefitBest(f.Ships[i])) n++;
            }
            refitInfo = T("переснаряжено кораблей: ", "ships refitted: ") + n;
        }
        if (GUILayout.Button(T("предпросмотр тултипов", "tooltip preview"), GUILayout.ExpandWidth(false)))
        {
            var sb = new System.Text.StringBuilder();
            foreach (Fleet f in defense.Fleets)
            {
                if (f == null) continue;
                string gsum = BattleTemplates.GarrisonSummary(f, true);
                if (string.IsNullOrEmpty(gsum)) continue;
                if (sb.Length > 0) sb.Append('\n');
                sb.Append(FleetName(f)).Append(": ").Append(gsum.Replace("\n", " · "));
            }
            refitInfo = sb.Length > 0 ? sb.ToString() : T("пусто: шаблоны не заданы", "empty: no templates chosen");
        }
        GUILayout.EndHorizontal();
        if (!string.IsNullOrEmpty(refitInfo)) GUILayout.Label(refitInfo);
        refitScroll = GUILayout.BeginScrollView(refitScroll, GUILayout.Height(200f));
        foreach (Fleet f in defense.Fleets)
        {
            if (f == null || f.Ships == null || f.Ships.Count == 0) continue;
            GUILayout.Label(T("Флот: ", "Fleet: ") + FleetName(f) + "  (" + f.Ships.Count + T(" кораблей", " ships") + ")");
            for (int i = 0; i < f.Ships.Count; i++)
            {
                Ship ship = f.Ships[i];
                if (ship == null) continue;
                GUILayout.BeginHorizontal();
                GUILayout.Label(BattleTemplates.ShipLine(ship));
                string hull = ship.ShipDesign != null && ship.ShipDesign.Hull != null ? ship.ShipDesign.Hull.Name.ToString() : null;
                var variants = string.IsNullOrEmpty(hull) ? null : BattleTemplates.Variants(hull);
                for (int v = 0; variants != null && v < variants.Count; v++)
                {
                    var t = variants[v];
                    if (GUILayout.Button(T("→ ", "-> ") + (CouncilGame.IsRussian() ? t.Variant.NameRu : t.Variant.NameEn), GUILayout.ExpandWidth(false)))
                        refitInfo = BattleTemplates.RefitShip(ship, t, false);
                }
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndScrollView();
    }

    /// <summary>Refit one ship to the variant it (or its hull) is set to: the design's own binding first, then the
    /// hull default the player picked with the design window's Auto-design button. There is no automatic "best
    /// against the enemy" pick here on purpose - choosing the counter is the player's call (docs §8) and a
    /// background auto-refit of every fleet is the kind of surprise that makes a save unplayable.</summary>
    static bool RefitBest(Ship ship)
    {
        try
        {
            if (ship == null || ship.ShipDesign == null || ship.ShipDesign.Hull == null) return false;
            string hull = ship.ShipDesign.Hull.Name.ToString();
            var variants = BattleTemplates.Variants(hull);
            if (variants.Count == 0) return false;
            BattleTemplate pick = BattleTemplates.Match(ship.ShipDesign);
            if (pick == null)
            {
                string current = BattleTemplates.ChoiceForHull(hull);
                if (!string.IsNullOrEmpty(current)) pick = BattleTemplates.ById(current);
            }
            if (pick == null) return false;      // nothing chosen: no blind auto-refit
            return !string.IsNullOrEmpty(BattleTemplates.RefitShip(ship, pick, false));
        }
        catch (Exception) { return false; }
    }

    static string FleetName(Fleet f)
    {
        try { return string.IsNullOrEmpty(f.UserName) ? f.Name.ToString() : f.UserName; }
        catch (Exception) { return "?"; }
    }

    static string SystemName(ColonizedStarSystem cs)
    {
        try { return cs.Name + " [" + cs.GUID + "]"; }
        catch (Exception) { return "?"; }
    }
}
