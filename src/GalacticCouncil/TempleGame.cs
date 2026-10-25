// Endless Temples - glue between TempleCore and the game (science victory rework).
//
// Flow: when the game itself would make an Endless victory technology available to an empire, the technology
// stays locked (GetTechnologyState hook) and the empire instead learns the location of that technology's
// Temple of the Ancients - a special node shared by every empire. A Behemoth (Juggernaut hull) fitted with
// special-node analyzer / research modules must stay at the temple: +1 study turn per turn present,
// -0.5 per turn absent, 10 turns needed on normal speed (scaled by game speed). Then the technology becomes
// researchable for that empire and every other player hears the "radio hum".
//
// Alliance science victory is rewritten through the interpreter context: the summed score is replaced by the
// best single member's number of researched Endless technologies and the alliance size by 1, so the alliance
// wins exactly when one of its members has researched all four.
using System;
using System.Collections.Generic;
using System.Globalization;
using Amplitude;
using Amplitude.Unity.Game;
using Amplitude.Unity.Simulation;

public sealed class TempleMarkerInfo
{
    public ulong NodeGuid;
    public string Label;
}

public static class TempleGame
{
    public static TempleCore State = new TempleCore();

    /// <summary>While set, the GetTechnologyState hook does not block: the reveal check has to see the vanilla state.</summary>
    public static bool SuppressBlock;

    static Game currentGame;
    static bool stateLoadedFromSave;
    static int lastHandledTurn = int.MinValue;
    static StaticString[] techNames;      // lazy: StaticString interning needs the game registry
    static StaticString scienceVictoryName;
    static bool scienceVictoryNameInit;

    // ---------------------------------------------------------------- helpers

    static void Fail(string where, Exception e)
    {
        string msg = "Temples " + where + ": " + e.GetType().Name + ": " + e.Message;
        try { CouncilGame.LastError = msg; } catch (Exception) { }
        try { Diagnostics.LogWarning("[EndlessTemples] " + msg); } catch (Exception) { }
    }

    static void Log(string text)
    {
        try { Diagnostics.LogWarning("[EndlessTemples] " + text); } catch (Exception) { }
    }

    static string Tr(string ru, string en) { return CouncilGame.Tr(ru, en); }

    public static string TechDisplay(int tech)
    {
        var d = TempleDefs.All[tech];
        return CouncilGame.IsRussian() ? d.Ru : d.En;
    }

    static StaticString TechName(int tech)
    {
        if (techNames == null)
        {
            techNames = new StaticString[TempleCore.TechCount];
            for (int i = 0; i < techNames.Length; i++) techNames[i] = new StaticString(TempleDefs.All[i].TechName);
        }
        return techNames[tech];
    }

    static int TechIndex(ScienceConstructibleElement tech)
    {
        if (tech == null) return -1;
        var n = tech.Name;
        for (int i = 0; i < TempleCore.TechCount; i++) if (n == TechName(i)) return i;
        return -1;
    }

    static int ScaleTurns(int turns, Empire emp)
    {
        try { return Math.Max(1, CustomHackingHelper.ScaleTurns(turns, emp)); } catch (Exception) { return turns; }
    }

    static bool IsLocalEmpire(int index)
    {
        var local = CouncilGame.LocalEmpire();
        return local != null && local.Index == index;
    }

    static bool HasLocalEmpire()
    {
        return CouncilGame.LocalEmpire() != null;
    }

    // ---------------------------------------------------------------- persistence (piggybacks the CouncilState attribute)

    public static string SerializeState()
    {
        try { return State.Serialize(); } catch (Exception e) { Fail("SerializeState", e); return ""; }
    }

    public static void LoadState(string data)
    {
        State = TempleCore.Deserialize(data);
        stateLoadedFromSave = true;
        lastHandledTurn = int.MinValue;
    }

    // ---------------------------------------------------------------- hooks: technology state / unlock guard

    /// <summary>Return hook of DepartmentOfScience.GetTechnologyState(ScienceConstructibleElement):
    /// an Endless victory technology whose temple is not opened yet reports NotAvailable.</summary>
    public static ScienceConstructibleElement.State AdjustTechState(ScienceConstructibleElement.State value, DepartmentOfScience science, ScienceConstructibleElement tech)
    {
        try
        {
            if (SuppressBlock) return value;
            if (value != ScienceConstructibleElement.State.Available
                && value != ScienceConstructibleElement.State.Queued
                && value != ScienceConstructibleElement.State.InProgress) return value;
            if (science == null || tech == null || science.Empire == null) return value;
            int idx = TechIndex(tech);
            if (idx < 0) return value;
            if (State.IsOpened(science.Empire.Index, idx)) return value;
            return ScienceConstructibleElement.State.NotAvailable;
        }
        catch (Exception) { return value; }
    }

    /// <summary>Start hook of DepartmentOfScience.UnlockTechnology(ScienceConstructibleElement, ...):
    /// no side channel (trade, quest, hacking) grants an Endless technology before its temple is opened.
    /// Returns true when the unlock must be skipped.</summary>
    public static bool BlocksUnlock(DepartmentOfScience science, ScienceConstructibleElement tech)
    {
        try
        {
            if (science == null || tech == null || science.Empire == null) return false;
            int idx = TechIndex(tech);
            if (idx < 0) return false;
            if (State.IsOpened(science.Empire.Index, idx)) return false;
            if (science.IsResearched(tech.Name)) return false;   // vanilla early-outs anyway
            return true;
        }
        catch (Exception) { return false; }
    }

    // ---------------------------------------------------------------- hook: alliance science victory

    /// <summary>
    /// Second end-hook call of VictoryManager.SetupInterpreterContextForContender (see Patcher.HookVictoryContext).
    /// For an alliance contender of VictoryScience the vanilla alliance formula reads SummedMajorEmpiresScore and
    /// NumberOfMajorEmpiresInAlliance; replacing them with (best member's researched Endless technologies, 1)
    /// turns "the alliance's technologies add up" into "one member with all four wins for the whole alliance".
    /// </summary>
    public static void AdjustScienceAllianceVictory(VictoryManager manager, MajorEmpire[] empires, VictoryConditionDefinition definition)
    {
        try
        {
            if (manager == null || empires == null || empires.Length <= 1 || definition == null || manager.InterpreterContext == null) return;
            if (!scienceVictoryNameInit) { scienceVictoryName = new StaticString("VictoryScience"); scienceVictoryNameInit = true; }
            if (definition.Name != scienceVictoryName) return;
            int best = 0;
            for (int i = 0; i < empires.Length; i++)
            {
                var emp = empires[i];
                if (emp == null) continue;
                var sci = emp.GetAgency<DepartmentOfScience>();
                if (sci == null) continue;
                int count = 0;
                for (int t = 0; t < TempleCore.TechCount; t++) if (sci.IsResearched(TechName(t))) count++;
                if (count > best) best = count;
            }
            manager.InterpreterContext.Register(new StaticString("SummedMajorEmpiresScore"), (object)(float)best);
            manager.InterpreterContext.Register(new StaticString("NumberOfMajorEmpiresInAlliance"), (object)1);
        }
        catch (Exception e) { Fail("AdjustScienceAllianceVictory", e); }
    }

    // ---------------------------------------------------------------- turn processing

    /// <summary>Runs from CouncilGame.ClientBeginTurn (before the Council's own early returns: temples work
    /// whether or not the Council exists). Deterministic on every machine, like the rest of the mod.</summary>
    public static void PerTurn(Game game, int turn)
    {
        try
        {
            if (game == null || game.Galaxy == null) return;
            if (!ReferenceEquals(currentGame, game))
            {
                currentGame = game;
                lastHandledTurn = int.MinValue;
                if (stateLoadedFromSave) stateLoadedFromSave = false;
                else State = new TempleCore();   // new game in the same process without a save read
            }
            if (turn == lastHandledTurn) return;
            lastHandledTurn = turn;

            Dictionary<ulong, SpecialNode> nodeByGuid = null;
            foreach (MajorEmpire emp in game.EnumerableMajorEmpires)
            {
                if (emp == null || emp.HasBeenEliminated) continue;
                var sci = emp.GetAgency<DepartmentOfScience>();
                if (sci == null) continue;
                for (int tech = 0; tech < TempleCore.TechCount; tech++)
                {
                    try
                    {
                        if (State.IsOpened(emp.Index, tech)) continue;
                        if (sci.IsResearched(TechName(tech))) { State.MarkOpened(emp.Index, tech); continue; }

                        if (!State.IsRevealed(emp.Index, tech))
                        {
                            bool avail = false;
                            SuppressBlock = true;
                            try
                            {
                                var st = sci.GetTechnologyState(TempleDefs.All[tech].TechName);
                                avail = st == ScienceConstructibleElement.State.Available
                                     || st == ScienceConstructibleElement.State.Queued
                                     || st == ScienceConstructibleElement.State.InProgress;
                            }
                            finally { SuppressBlock = false; }
                            if (!avail) continue;
                            RevealFor(game, emp, tech, turn);
                        }

                        if (State.StateOf(emp.Index, tech) == TempleCore.Revealed)
                        {
                            ulong guid = State.TempleNode[tech];
                            if (nodeByGuid == null) nodeByGuid = BuildNodeMap(game);
                            SpecialNode node;
                            bool present = false;
                            if (nodeByGuid.TryGetValue(guid, out node) && node != null)
                                present = HasResearchBehemothAt(emp, node);
                            if (State.Tick(emp.Index, tech, present))
                                OnTempleOpened(game, emp, tech, node, false);
                        }

                        // A temple-locked technology must never sit in the research queue. New queue orders are
                        // already rejected because GetTechnologyState reports NotAvailable, but a save made
                        // before this mod (or a direct side channel) can contain such a construction. Remove it
                        // before end-turn research can mark it completed and call the blocked UnlockTechnology:
                        // vanilla ComputeUnlockResearches would dequeue/unregister it first, leaving a stale
                        // researches-in-progress entry and silently losing the invested science.
                        if (!State.IsOpened(emp.Index, tech))
                            CancelQueuedTempleResearch(sci, emp, tech);
                    }
                    catch (Exception e) { Fail("PerTurn tech " + tech + " emp " + emp.Index, e); }
                }
            }
        }
        catch (Exception e) { Fail("PerTurn", e); }
    }

    static Dictionary<ulong, SpecialNode> BuildNodeMap(Game game)
    {
        var map = new Dictionary<ulong, SpecialNode>();
        try
        {
            foreach (SpecialNode sn in game.Galaxy.SpecialNodes) if (sn != null) map[sn.GUID] = sn;
        }
        catch (Exception e) { Fail("BuildNodeMap", e); }
        return map;
    }

    static SpecialNode ResolveNode(Game game, ulong guid)
    {
        if (guid == TempleCore.NodeUnassigned || guid == TempleCore.NodeNoneInGalaxy) return null;
        SpecialNode node;
        return BuildNodeMap(game).TryGetValue(guid, out node) ? node : null;
    }

    // ---------------------------------------------------------------- reveal & node choice

    static void RevealFor(Game game, MajorEmpire emp, int tech, int turn)
    {
        ulong guid = State.TempleNode[tech];
        if (guid == TempleCore.NodeUnassigned) guid = ChooseTempleNode(game, tech, emp, turn);
        int requiredHalf = 2 * ScaleTurns(TempleCore.BaseStudyTurns, emp);
        if (!State.Reveal(emp.Index, tech, guid, requiredHalf)) return;

        Log("turn " + turn + ": temple of " + TempleDefs.All[tech].TechName + " revealed to empire " + emp.Index
            + " at node " + (guid == TempleCore.NodeNoneInGalaxy ? "<none in galaxy>" : guid.ToString(CultureInfo.InvariantCulture)));

        if (guid == TempleCore.NodeNoneInGalaxy)
        {
            OnTempleOpened(game, emp, tech, null, true);
            return;
        }
        if (IsLocalEmpire(emp.Index))
        {
            var node = ResolveNode(game, guid);
            TempleUi.QueueMessage(
                Tr("Храм Древних найден", "Temple of the Ancients located"),
                Tr("Наши учёные расшифровали координаты храма технологии «", "Our scientists have deciphered the coordinates of the temple of \"")
                + TechDisplay(tech) + Tr("»: особый узел ", "\": the special node ") + NodeName(node)
                + " (" + NodeTypeName(node) + "). "
                + Tr("Отправьте туда Бегемота с модулями анализа особых узлов и удерживайте его у храма: изучение займёт ",
                     "Send a Behemoth fitted with special-node analyzer modules there and keep it at the temple: the study takes ")
                + TurnsText(State.RequiredTurns(emp.Index, tech))
                + Tr(" ходов (без Бегемота прогресс падает на 0.5 за ход). Точка отмечена на карте галактики.",
                     " turns (without a Behemoth the progress falls by 0.5 per turn). The point is marked on the galaxy map."));
        }
    }

    static ulong ChooseTempleNode(Game game, int tech, MajorEmpire emp, int turn)
    {
        var neutral = new List<ulong>();
        var all = new List<ulong>();
        var neutralFresh = new List<ulong>();   // not already another technology's temple
        var allFresh = new List<ulong>();
        try
        {
            foreach (SpecialNode sn in game.Galaxy.SpecialNodes)
            {
                if (sn == null || sn.IsDestroyed) continue;
                var def = sn.SpecialNodeDefinition;
                if (def == null) continue;
                if (def.StartInvisible || def.IgnoreForGalaxyDiscovery) continue;
                if (!TempleDefs.NodeMatches(tech, def.Name.ToString(), def.Category.ToString())) continue;
                ulong g = sn.GUID;
                bool isNeutral = false;
                try { isNeutral = sn.SystemWhichInfluences == null; } catch (Exception) { }
                all.Add(g);
                if (isNeutral) neutral.Add(g);
                if (!State.IsTempleNode(g)) { allFresh.Add(g); if (isNeutral) neutralFresh.Add(g); }
            }
        }
        catch (Exception e) { Fail("ChooseTempleNode scan", e); }
        neutral.Sort(); all.Sort(); neutralFresh.Sort(); allFresh.Sort();
        int seed = unchecked(turn * 1103515245 + tech * 12345 + (emp != null ? emp.Index : 0) * 131 + all.Count * 17 + 99991);
        ulong pick = TempleCore.PickTemple(neutralFresh, allFresh, seed);
        if (pick == TempleCore.NodeNoneInGalaxy) pick = TempleCore.PickTemple(neutral, all, seed);
        return pick;
    }

    static void OnTempleOpened(Game game, MajorEmpire emp, int tech, SpecialNode node, bool noTempleInGalaxy)
    {
        Log("turn " + (game != null ? game.Turn : -1) + ": empire " + emp.Index + " opened the temple of "
            + TempleDefs.All[tech].TechName + (noTempleInGalaxy ? " (no temple node exists in this galaxy: opened directly)" : ""));
        try { TempleUi.EnsureHost(); } catch (Exception) { }
        if (IsLocalEmpire(emp.Index))
        {
            TempleUi.QueueMessage(
                Tr("Храм Древних открыт", "A Temple of the Ancients is open"),
                Tr("Храм Древних изучен! Технология «", "The Temple of the Ancients has been studied! The technology \"")
                + TechDisplay(tech) + Tr("» теперь доступна для исследования.", "\" can now be researched."));
        }
        else if (HasLocalEmpire())
        {
            TempleUi.QueueMessage(
                Tr("Радиогул", "A radio hum"),
                Tr("По вселенной прошёл неслыханный до селе радиогул. Наши шпионы докладывают, что ",
                   "An unheard-of radio hum has swept the universe. Our spies report that ")
                + CouncilGame.EmpireName(emp.Index)
                + Tr(" открыли один из Храмов Древних.", " have opened one of the Temples of the Ancients."));
        }
    }

    // ---------------------------------------------------------------- behemoth study check

    /// <summary>A research Behemoth: a Juggernaut-hull ship fitted with special-node analyzer / research modules
    /// (ModuleSupportJuggernautAnalyzer* / ModuleSupportJuggernautResearch* module definitions).</summary>
    public static bool IsResearchBehemoth(Ship ship)
    {
        try
        {
            if (ship == null || !ship.IsJuggernaut) return false;
            var sections = ship.Sections;
            if (sections == null) return false;
            for (int i = 0; i < sections.Length; i++)
            {
                var sec = sections[i];
                if (sec == null || sec.Modules == null) continue;
                for (int m = 0; m < sec.Modules.Count; m++)
                {
                    var mod = sec.Modules[m];
                    if (mod == null || mod.Definition == null) continue;
                    string name = mod.Definition.Name.ToString();
                    if (name != null && (name.IndexOf("JuggernautAnalyzer", StringComparison.OrdinalIgnoreCase) >= 0
                                      || name.IndexOf("JuggernautResearch", StringComparison.OrdinalIgnoreCase) >= 0))
                        return true;
                }
            }
        }
        catch (Exception) { }
        return false;
    }

    static bool FleetHasResearchBehemoth(Fleet f, int empireIndex)
    {
        try
        {
            if (f == null || f.IsDestroyed || f.IsMoving || f.EmpireIndex != empireIndex || f.Ships == null) return false;
            for (int i = 0; i < f.Ships.Count; i++) if (IsResearchBehemoth(f.Ships[i])) return true;
        }
        catch (Exception) { }
        return false;
    }

    static bool HasResearchBehemothAt(MajorEmpire emp, SpecialNode node)
    {
        if (emp == null || node == null) return false;

        // Authoritative vanilla notion of "in orbit here": Fleet.SetPosition docks the fleet into the
        // GameNode returned for its NodePosition (for a temple that object is the SpecialNode itself,
        // which is how its own docked-fleet descriptors/effect multiplier find the fleet).
        try
        {
            var docked = node.DockedEntities;
            if (docked != null)
                for (int i = 0; i < docked.Count; i++)
                    if (FleetHasResearchBehemoth(docked[i], emp.Index)) return true;
        }
        catch (Exception) { }

        // Fallback for any transient docking-list mismatch: a non-moving empire fleet whose logical
        // node position is exactly the temple node's position is at the temple.
        var defense = emp.GetAgency<DepartmentOfDefense>();
        if (defense == null || defense.Fleets == null) return false;
        foreach (Fleet f in defense.Fleets)
        {
            if (f == null || f.NodePosition != node.NodePosition) continue;
            if (FleetHasResearchBehemoth(f, emp.Index)) return true;
        }
        return false;
    }

    // Removes a temple-locked Endless technology from the research queue exactly like the vanilla
    // CancelResearchProcessor does: refund paid instant costs, remove the Construction from the queue and
    // notify with Cancelled. The Construction deliberately stays registered / in researches-in-progress
    // (again exactly like vanilla cancel), so accumulated non-instant science is preserved and can be
    // resumed by queueing the technology again after its temple opens.
    static void CancelQueuedTempleResearch(DepartmentOfScience sci, MajorEmpire emp, int tech)
    {
        try
        {
            if (sci == null || emp == null || sci.ResearchQueue == null || sci.ResearchQueue.PendingConstructions == null) return;
            var pending = new List<Construction>(sci.ResearchQueue.PendingConstructions);
            for (int i = 0; i < pending.Count; i++)
            {
                var c = pending[i];
                if (c == null || c.ConstructibleElement == null || c.ConstructibleElement.Name != TechName(tech)) continue;
                if (!RefundInstantConstructionCosts(emp, c))
                {
                    Log("could not refund instant costs; leaving queued temple-locked technology "
                        + TempleDefs.All[tech].TechName + " of empire " + emp.Index + " in the research queue");
                    continue;
                }
                sci.ResearchQueue.Remove(c);
                sci.OnResearchQueueChanged(c, ConstructionChangeEventAction.Cancelled);
                Log("removed queued temple-locked technology " + TempleDefs.All[tech].TechName
                    + " of empire " + emp.Index + " from the research queue until its temple is opened");
            }
        }
        catch (Exception e) { Fail("CancelQueuedTempleResearch emp " + (emp != null ? emp.Index : -1) + " tech " + tech, e); }
    }

    static bool RefundInstantConstructionCosts(MajorEmpire emp, Construction c)
    {
        var costs = c.Costs;
        var stocks = c.CurrentStocks;
        if (costs == null || stocks == null || costs.Length == 0) return true;
        var treasury = emp.GetAgency<DepartmentOfTheTreasury>();
        int n = Math.Min(costs.Length, stocks.Length);
        for (int i = 0; i < n; i++)
        {
            var cost = costs[i];
            var stock = stocks[i];
            if (cost == null || stock == null || stock.Stock <= 0f || !cost.Instant) continue;
            if (treasury == null || emp.SimulationObject == null) return false;
            if (!treasury.TryTransferResources(emp.SimulationObject, stock.ResourceName, stock.Stock, true)) return false;
        }
        return true;
    }

    // ---------------------------------------------------------------- names & UI data

    public static string NodeName(SpecialNode node)
    {
        try
        {
            if (node == null) return "?";
            string n = node.LocalizedName;
            if (!string.IsNullOrEmpty(n)) return n;
            return node.Name.ToString();
        }
        catch (Exception) { return "?"; }
    }

    public static string NodeTypeName(SpecialNode node)
    {
        try
        {
            if (node == null || node.SpecialNodeDefinition == null) return "";
            string dn = node.SpecialNodeDefinition.Name.ToString();
            return NodeTypeName(dn);
        }
        catch (Exception) { return ""; }
    }

    static string NodeTypeName(string defName)
    {
        if (defName == null) return "";
        if (defName.IndexOf("BlackHole", StringComparison.OrdinalIgnoreCase) >= 0) return Tr("чёрная дыра", "Black Hole");
        if (defName.IndexOf("NeutronStar", StringComparison.OrdinalIgnoreCase) >= 0) return Tr("нейтронная звезда", "Neutron Star");
        if (defName.IndexOf("SpaceClouds", StringComparison.OrdinalIgnoreCase) >= 0) return Tr("туманное облако", "Nebular Clouds");
        if (defName.IndexOf("Rejuvenation", StringComparison.OrdinalIgnoreCase) >= 0) return Tr("поле реювенации", "Rejuvenation Field");
        if (defName.IndexOf("AsteroidField", StringComparison.OrdinalIgnoreCase) >= 0) return Tr("астероидное поле", "Asteroid Field");
        if (defName.IndexOf("SolarNebula", StringComparison.OrdinalIgnoreCase) >= 0) return Tr("солнечная туманность", "Solar Nebula");
        if (defName.IndexOf("CollapsingStar", StringComparison.OrdinalIgnoreCase) >= 0) return Tr("коллапсирующая звезда", "Collapsing Star");
        return defName;
    }

    static string TurnsText(float turns)
    {
        return turns.ToString("0.#", CultureInfo.InvariantCulture);
    }

    /// <summary>Markers for the galaxy map: temples revealed to the local player and not opened yet.</summary>
    public static List<TempleMarkerInfo> LocalMarkers()
    {
        var res = new List<TempleMarkerInfo>();
        try
        {
            var me = CouncilGame.LocalEmpire();
            if (me == null) return res;
            for (int tech = 0; tech < TempleCore.TechCount; tech++)
            {
                if (State.StateOf(me.Index, tech) != TempleCore.Revealed) continue;
                ulong guid = State.TempleNode[tech];
                if (guid == TempleCore.NodeUnassigned || guid == TempleCore.NodeNoneInGalaxy) continue;
                res.Add(new TempleMarkerInfo
                {
                    NodeGuid = guid,
                    Label = Tr("Храм Древних: ", "Temple of the Ancients: ") + TechDisplay(tech)
                        + " (" + TurnsText(State.ProgressTurns(me.Index, tech)) + "/" + TurnsText(State.RequiredTurns(me.Index, tech)) + ")"
                });
            }
        }
        catch (Exception e) { Fail("LocalMarkers", e); }
        return res;
    }

    /// <summary>Status lines for the God-Mod debug tab.</summary>
    public static List<string> DebugLines(int empIndex)
    {
        var res = new List<string>();
        try
        {
            var game = CouncilGame.GetGame();
            for (int tech = 0; tech < TempleCore.TechCount; tech++)
            {
                ulong guid = State.TempleNode[tech];
                string where = guid == TempleCore.NodeUnassigned ? Tr("узел ещё не выбран", "node not chosen yet")
                    : guid == TempleCore.NodeNoneInGalaxy ? Tr("нет подходящего узла в галактике", "no suitable node in the galaxy")
                    : NodeName(game != null ? ResolveNode(game, guid) : null);
                int st = State.StateOf(empIndex, tech);
                string state = st == TempleCore.Opened ? Tr("открыт", "opened")
                    : st == TempleCore.Revealed ? Tr("изучение ", "study ") + TurnsText(State.ProgressTurns(empIndex, tech)) + "/" + TurnsText(State.RequiredTurns(empIndex, tech))
                    : Tr("скрыт", "hidden");
                res.Add(TechDisplay(tech) + " — " + state + " (" + where + ")");
            }
        }
        catch (Exception e) { Fail("DebugLines", e); }
        return res;
    }

    // ---------------------------------------------------------------- debug (God-Mod menu, synced via OrderCouncilVote KindDebug)

    public static void DebugReveal(int empIndex, int tech)
    {
        try
        {
            var game = CouncilGame.GetGame();
            var emp = CouncilGame.GetMajor(empIndex);
            if (game == null || emp == null || tech < 0 || tech >= TempleCore.TechCount) return;
            if (State.IsOpened(empIndex, tech)) return;
            RevealFor(game, emp, tech, game.Turn);
        }
        catch (Exception e) { Fail("DebugReveal", e); }
    }

    public static void DebugOpen(int empIndex, int tech)
    {
        try
        {
            var game = CouncilGame.GetGame();
            var emp = CouncilGame.GetMajor(empIndex);
            if (game == null || emp == null || tech < 0 || tech >= TempleCore.TechCount) return;
            if (!State.IsRevealed(empIndex, tech)) RevealFor(game, emp, tech, game.Turn);
            if (!State.IsOpened(empIndex, tech))
            {
                State.MarkOpened(empIndex, tech);
                OnTempleOpened(game, emp, tech, ResolveNode(game, State.TempleNode[tech]), false);
            }
        }
        catch (Exception e) { Fail("DebugOpen", e); }
    }
}
