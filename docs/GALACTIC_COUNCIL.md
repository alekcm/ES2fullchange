# Galactic Council (Stages 1-6 + votes rework)

Replaces the Academy "request resources" phase with a Council vote. Implemented as C# (`src/GalacticCouncil`),
compiled with Mono/Roslyn and merged into `Managed/Assembly-CSharp.dll` with Cecil (`tools/Patcher`).

## Rebuild
```
tools/setup.sh                 # downloads Mono+Roslyn into /tmp (once)
tools/build_patch.sh           # baseline (commit 6ae43ff) + Council -> Managed/Assembly-CSharp.dll
tests: csc tests/CouncilCoreTests.cs src/GalacticCouncil/CouncilLaws.cs src/GalacticCouncil/CouncilCore.cs
tools/Verify/Verify.cs         # loads the result in Mono and JIT-compiles every Council/hooked method
```

## Rhythm
Vote opens at the start of a turn, is tallied at the start of the next one, laws take effect that turn and last
until the next vote. Interval: 20 turns on normal speed (scaled by game speed). Each empire gets 3 random laws and
votes (100 base plus the sources below), spent in steps of 10. "All" laws: accept/reject. "One" laws: pick the target empire.

## Stage 1 laws (functional)
Merit (+30 votes), Expansion Mandate (+1 colony limit), Expansion Limit (-1), Dust payout per colony,
Replenishment (manpower x1.5), Exchange of Intelligence Methods (+4 sleepers), Non-Destabilization Pact
(no sleeper-based hacks), Academy Counterintelligence (no new backdoors).

Other laws from the spec are registered in `CouncilLaws` but `Implemented=false`, so they are never offered yet.

## Stage 2 (added)
Laws: Academic Mentoring (+3 levels to all active heroes of every member, human and AI, except the AI ruler heroes of the hacking mod; posted by the host), Market Deregulation
(x0.5 buy price of Strategic/Luxury resources), Letters of Marque (x0.5 buy price of ships), Right of Plunder (x2 loot of
`GroundBattleOutcomeLootDefinition.FillLoot`), Trade Restrictions (buy x2 / sell x0.5 for the target).

Mandates (`CouncilMandates.cs`, state saved as `GC2`): Chairman, Marshal, Quartermaster, Eye, Supreme Legate.
* Votes for a mandate (Mandates tab) come out of the same pool of votes; the winner replaces one law question of the next vote
  (at most one mandate question per vote) and the holder is chosen among the empires that qualify. Holding lasts until
  re-election, leaving the Council or the Academy's destruction.
* Chairman: replace one ordinary question once per cycle before anybody voted (Voting tab).
* Marshal: hires a warship group of the empire's own faction at the home system (upkeep paid by the empire).
* Quartermaster: once per cycle Dust + Science delivery scaled by stage and game length.
* Eye: +1 simultaneous hacking operation, +20 `MaximumEmpireProcessingPowerStock` (the "ПВ" reading is a guess).
* Supreme Legate: +100 votes (no doubling), the "Establishment of Absolute Power" vote is forced on the next vote and while the countdown runs; after 30 turns (scaled)
  `IVictoryManagementService.ForceRiseVictory` is called with the victory condition whose name contains `diplomat`/`senate`/...
* Leaving the Council: the Academy relation is pinned to -100 every turn (static `AcademyDiplomaticRelation.SetRelationPoints`).
* Qualification: see "Votes and qualification rework" below (the manual assignment stub was removed).

## Stage 3 (added): service squads
Marshal contingent, Council Escort (all members), Academy Squadron (target) and Covert Group (target; **not cloaked**) are
created with `OrderCreateFleetFromShipDesignName` at the home system (design = a warship picked from the faction's
`AbstractToShipDesignDictionary` by name heuristics; heavy for Marshal/Academy, light for Escort/Covert; size scales with the stage).
The fleet name carries a marker `[GC<tag><empire>]`; every turn `SyncSquads` records the ship GUIDs of marked fleets in the
Council state (`CouncilSquads.cs`, saved) and, when the mandate/law ended or the empire left, posts `OrderDisbandShips` (host only).
`ScrapShipsPreprocessor` and `SelloutTradableShipsPreprocessor` refuse orders that contain a squad ship.
Not verified in game: that the heuristic finds a design, that the fleet name is kept, that the host may post orders for AI empires.

## Stage 4: research, upkeep, infrastructure
* Research +-50% (Development / Economy & Trade / Military): `DepartmentOfTheTreasury.GetProductionCostWithBonus(context, provider, cost, float)`
  is the sink of all cost overloads; its returns go through `CouncilGame.AdjustProductionCost`. For a `TechnologyDefinition` paid in `EmpireResearch`
  the cost is divided by 1.5 (+50% speed) or multiplied by 2 (-50%). The branch comes from `TechnologyStage.TechnologyQuadrant`
  (fallback `CustomHackingHelper.MatchesQuadrant`).
* Infrastructure Program: same hook, `StarSystemImprovementDefinition` that is not a wonder, paid in `SystemProduction`, target empire: cost x0.75.
* Warship upkeep +100% / ship upkeep -50%: an `Addition` modifier on `EmpireMoneyUpkeep` (class `ClassEmpire`), recomputed each turn from
  `Ship.GarrisonMoneyUpkeep` over all fleets (warship = not civilian, not academy), rounded to 5.

## Stage 5: battle, economy, diplomacy laws
* Ship weapon damage -20%: the game only has automatic battles. `ServerEncounterState_RunSimpleBattle.ComputeSimplifiedEffectiveDamagePercentage`
  (end) -> `CouncilGame.AdjustSimplifiedDamage`: the target's effective damage percentage x0.8 (group leader's empire).
* Fortification Inspection / Capital Reinforcement: `ServerGroundBattle.GetColonizedStarSystemOpponentPropertyValue` (return) ->
  `AdjustOpponentProp`. For the defending opponent of the target's system: troop max health x0.75 (inspection) / x1.5 (capital, home system of the owner).
  (Stage 6: the bombardment multiplier was removed, the siege acts on the garrison, see below.)
* Antitrust Oversight: `DepartmentOfCommerce.GetTradingCompanyIncomeMultiplier` (return) -> `AdjustCompanyIncome`: x0.5 for the target's companies;
  companies are marked `IncomeDirty` when the law set changes.
* Trading Company Reinvestment: every turn the systems that hold a company HQ/subsidiary of a member get a descriptor
  (`SystemProduction` x1.25); removed when the law ends. (Stage 6: moved to the shared system-effects descriptor, see below.)
* Evacuation compensation: `DepartmentOfTheInterior.DecolonizeProcessor` calls `CouncilGame.OnDecolonize(order, system)` before the system is removed.
  Voluntary evacuation into a colony ship pays an extra `3 x improvement costs + 250 x stage` Dust.
* Academic Truce: forced peace with every war of the target (`SetDiplomaticRelationState(Truce)` both ways, then
  `IDiplomaticWarRepositoryControl.DestroyWarInstance`, the same sequence as `ForceTruce.ClientAccept`, without tribute/war points).
* Strategic Presence: when the law is enacted `PresenceMarks` are drawn (5..12 random systems, deterministic seed); when the next session opens
  each member gets +10 votes per marked system it holds (colony or outpost), max +50; marks are consumed. Shown in the Laws tab. State is saved.

## Stage 6: garrison, Academy home world
Garrison = the system manpower (`SimulationProperties/StarSystem`: `SystemManpowerStock`, `SystemManpowerRefillRate`, `SystemManpowerLoss`, ...).
* `CouncilGame.RefreshSystemEffects` (every turn) keeps ONE descriptor per affected `ColonizedStarSystem` (type `GalacticCouncilSys`, modifiers with
  path `ClassStarSystem`, the same pattern as `CustomHackingHelper.ApplyPostOverhaulGodModEffects`; `Multiplication` operation):
  * Fortification Inspection: `SystemManpowerLoss` x1.5 in every system of the target (siege losses of the garrison, ships in orbit);
  * Capital Reinforcement: `SystemManpowerRefillRate` and `MaximumSystemManpowerRefillRate` x2 in the target's home system (garrison recovery);
  * Trading Company Reinvestment: `SystemProduction` x1.25.
  The descriptors are rebuilt only when the signature (system + flags) changes and are removed when the law ends or the Council is disbanded.
* Empire-wide descriptors now use their own type `GalacticCouncilEmp` (previously the class name `ClassEmpire`).
* Academy home world = half of the capitals: `VictoryManager.SetupInterpreterContextForContender` (end) -> `CouncilGame.AdjustVictoryContext(manager, contender.Empires)`.
  The victory formulas read `NumberOfHealthyHomeSystems` from the interpreter context. For a contender that holds the Academy home world (colony state
  at `Galaxy.AcademyHomePosition`) the value becomes `max(1, healthy - players / 2)`; for the others the true value is registered again (the context is shared).
  Assumption: the military victory formula uses `NumberOfHealthyHomeSystems` as the number of capitals that must be captured.

## Votes and qualification rework
**Sources of votes** (`CouncilVotes.cs`, pure; `CouncilGameVotes.cs`, game glue). `Budget(emp)` =
(100 base + Merit law + Strategic Presence + *session votes*) + 50 per ordinary mandate held; the Supreme Legate simply gets +100 (the victory mandate gives 100, not 50; nothing is
doubled). The budget never goes below 0.
* *Session votes* are read from the game when a vote opens (like Strategic Presence) and saved in `CouncilState`
  (fields 30-31 of the `GC2` string, fields 32-34 hold the votes *bought* during the session, older saves load without them):
  * every star system: level 2 -> +5, level 3 -> +10, level 4 -> +20 (one value per system, **not cumulative**;
    summed over all systems; `ColonizedStarSystem.Level` = `SimulationProperties.StarSystem.SystemLevel`);
  * Republic government: +40 (`Senate.Government.Definition.Name` contains `Republic`);
  * Empire Development technology branch (`DepartmentOfScience.GetCurrentStageNumber`, the highest unlocked `TechnologyStageDefinition` of a quadrant whose name contains `EmpireDevelopment`): stage T3 +10, T4 +20, T5 +30; like the system levels the stages do not add up (`CouncilCore.TechStageVotes`);
  * skills of the heroes that represent the senators (see below);
  * minus 20 for every war the empire declared during the last `IntervalTurns` turns (20 at normal speed).
* **War penalty:** hook `GameClient.<CreateWarInstanceProcessor>.MoveNext`, right after `CreateWarInstance` returns,
  calls `CouncilGame.OnWarCreated(war)`; the empire of `war.AttackerGroup.FirstWarEmpire` (the declarer; allies that join are
  not punished) gets the war recorded with the turn. Wars of the AI count too. Penalties stack, each lasts one cycle.
* **Senator skills** (`HeroRework.CouncilVotesOf`): the passive votes of an empire come from the learned skills of every
  `Senator.RepresentativeHero`. They live **only in the senator class tree** (`HRTreeClassCorporate@S`): "Party Discipline"
  +5 votes per level (T2), "Political Machine" +10 per level (T3) and the ultimate "The Voice of the Nation" +40. The party
  and race trees of the senators carry no votes; the dictator keeps his own vote skills (`HRTreeDictVenetians`,
  `HRTreeDictHoratio`). Admirals and governors get influence instead of votes.
* **Bought votes** (see the next section): votes purchased during the open session are part of the budget until it ends.
* The vote tab shows where the votes come from (`BudgetText`).

**Confirmation of the victory mandate** (was "No Confidence", law id `NoConfidence` kept for saves): the vote is about
*keeping* the Supreme Legate. "For" keeps him, "against" removes him; he stays with at least **51%** of the votes in favour
(solo) or **67%** when he is a member of an alliance (the others need only 34% to remove him). Nobody voted: he stays.
The alliance flag is read from `IAllianceRepositoryService.FindAllianceOf` (two or more empires) every turn and just before
the tally. AI empires: the holder always votes to keep, the others vote to remove with 85% probability.

**Qualification of the mandates** (task categories are now achievements reported once by `CouncilGame.DetectQualifications`
every turn, for every member; the progress is saved in the old `Tasks` field):
| Mandate | Condition | How it is read |
|---|---|---|
| Eye of the Academy | 75% of the galaxy discovered | `DepartmentOfTheInterior.GetNodesExplorationProgressRatio(EntityExploration.State.Identified)` (the count the game uses for its discovery milestone) |
| Council Quartermaster | a star system of level 4 | any owned `ColonizedStarSystem.Level >= 4` |
| Marshal of the Academy | capture the capital of another player | an owned system whose `StarSystemNode.IsHomeSystem` with `HomeSystemEmpireIndex` of another major empire |
| Chairman | 5 heroes of the Mastery rank | `HeroRework.IsFixed(hero)` (took the ultimate) for the empire's heroes; each hero counted once, even if he dies later |
| Supreme Legate | unchanged: qualify for 2 ordinary mandates and hold one for a full cycle | |
The achievements stay once earned. The UI shows the progress (for example `62% / 75%`).

## Buying votes during a session (the senator class line, 2026-10-25)

The class tree of the senators (`HRTreeClassCorporate@S`) is entirely about Council votes and holds no system buffs.
Tier 1 carries five **active** skills: while a vote is open, each skill level is one purchase of **10 votes**, paid the way
the method describes. The buttons appear in the Council window (vote tab, cards below the questions) as soon as the empire's
representing senators know at least one of the skills. The IMGUI fallback window (`CouncilUiImgui`) has the same five
purchases in a compact form at the bottom of the vote tab, with a fleet picker (a selection grid) for terror.

| Method (skill) | Price of one purchase |
|---|---|
| Bribing the Electorate | **Dust**: 250 x game stage x (1 + 0.5 x purchases already made) |
| Crushing Authority | **Influence**: 100 x game stage x (1 + 0.5 x purchases already made) |
| Compulsory Turnout | **-5 happiness on every system** of the empire, until the *next* vote opens |
| Keeping Them in Fear | one fleet of **4+ command points immobilised** (cannot move), until the *next* vote opens |
| Military Support | **-25% of the current and the maximum Manpower** of the empire, until the *next* vote opens |

* **Limits** (`HeroRework.BuyLimitOf`): the sum of the skill levels over the senators that represent the empire's parties,
  **+2 per method** when one of them holds the class ultimate ("The Vote Headquarters"). All five methods are independent;
  a fully developed senator with the ultimate can buy up to 6 x 10 votes per method during one session.
* **Inflation** (`CouncilCore.BuyStackCost`): the price scales with the game stage (1..6, `CouncilGame.GameStage`) and every
  purchase made this session makes the next one of the same method 50% more expensive; prices are rounded to tens.
* The purchases are final: they add to `Budget` for the rest of the session and cannot be undone (a native confirmation
  dialog asks before paying). When the *next* session opens, everything is wiped: the bought votes stop counting and the
  happiness / fleet / Manpower penalties end (`CouncilCore.OpenVote`).
* Orders: `OrderCouncilVote.KindBuyVotes` (A = method, `Fleet` = the immobilised fleet for terror) - validated by
  `CouncilGame.BuyPreprocessor` (phase, membership, limit, stock, fleet ownership and size, a non-empty Manpower stock
  for war) and applied once by `BuyProcessor`; the state is saved in `CouncilState` fields 32-34.
* The penalties are live descriptors refreshed every turn (`CouncilGame.RefreshBuyPenalties`, the Euphoria pattern):
  one happiness descriptor per strength on every colonised system of the buying empire, one movement descriptor
  (`ShipMaximumMovement` -999) per frozen fleet, one capacity descriptor (`MaximumEmpireManpowerStock`, -25% per purchase,
  capped at -90%) on the buying empire. The Manpower taken by the war purchases is collected from the stock through the
  treasury (`EmpireManpower`, the path the game itself uses) and given back when the purchases end (`ManpowerTaken`).
* **AI** (`CouncilGameAi.AiBuyVotes`, at the end of the voting turn before the tally): AI empires bribe and pressure while
  they can keep a reserve of 4x the price; they never use the civic, terror or war purchases.

## Endless Temples (science victory)
Full design and implementation notes: [`ENDLESS_TEMPLES.md`](ENDLESS_TEMPLES.md).

The four Endless victory technologies stay locked after their normal availability condition until the empire studies the
matching Temple of the Ancients. Each technology has one shared temple node, fixed at the first reveal (neutral special
nodes are preferred); another empire is shown that node only when it qualifies itself. Study requires a stationary
Juggernaut/Behemoth with `JuggernautAnalyzer` or `JuggernautResearch` modules for 10 turns on standard speed (scaled by
game speed), with progress decaying by 0.5 per absent turn. The `TM1` temple state is stored after `@@TM@@` in
`CouncilState`. For an alliance `VictoryScience` contender, `TempleGame.AdjustScienceAllianceVictory` replaces the
summed score and alliance size by the best member's researched Endless-technology count and 1, so one member with all
four wins for the alliance. AI empires use the same temple rules and have not yet been taught to study temples.

## Hooks in the game assembly
| Where | What |
|---|---|
| `AcademyRequestsManager.ClientBeginTurn` | `CouncilGame.ClientBeginTurn(this)` is called first, then the **vanilla body runs unchanged** (the Academy's own Dormant → Request → Role cycle and leading table keep working next to the Council; replacing the body froze Academy diplomacy in "deliberating") |
| `GameClientState_LaunchGame.Begin`, `GameServerState_LaunchGame.Begin` | `CouncilGame.EnsureEarlyContent()` is called first: all one-time content generation (reworked hero skills, dictator hero definitions, council vote texts, static tables) runs under the session **loading screen** instead of lazily during the first turns; self-guarded, so later launches pay a few dictionary lookups |
| `AcademyRequestsManager.SerializeAttributes` | appends `CouncilGame.SerializeAttributes` (attribute `CouncilState`, save version >= 7) |
| `DepartmentOfTheTreasury` | new order processor `CouncilVote` (`OrderCouncilVote`) |
| `GameClient.SpawnHackingOperationBackdoorProcessor` | exits early while Counterintel is active |
| `OverhaulOutcomeDefinition.CheckHackingEmpireAndTarget`, `CustomHackingHelper.BuildPromptElement` | sleeper count goes through `CouncilGame.SleepersForHack` |
| `AcademyModalWindow.OnEndShow/OnEndHide` | at end-show: (re)creates the "Council" launcher and fronts the Council window while a vote is open and uncast; at end-hide: hides the Council with it. **End**-hooks on purpose: showing the Council modal from inside the Academy's `OnBeginShow` (before its base call) left the Academy window itself half-shown |
| `Tradable.GetValue`, `TradableResource.GetValue` (every `ret`) | value goes through `CouncilGame.AdjustTradableValue` |
| `Marketplace.ITradingManagementService.GetTradablePrice` (every `ret`) | price goes through `CouncilGame.AdjustTradePrice` |
| `GroundBattleOutcomeLootDefinition.FillLoot` (end) | `CouncilGame.AdjustLoot` |
| `DepartmentOfTheTreasury.GetProductionCostWithBonus(..., Single)` (every `ret`) | value goes through `CouncilGame.AdjustProductionCost` |
| `ServerEncounterState_RunSimpleBattle.ComputeSimplifiedEffectiveDamagePercentage` (end) | `CouncilGame.AdjustSimplifiedDamage` |
| `ServerGroundBattle.GetColonizedStarSystemOpponentPropertyValue` (every `ret`) | value goes through `CouncilGame.AdjustOpponentProp` |
| `DepartmentOfCommerce.GetTradingCompanyIncomeMultiplier` (every `ret`) | value goes through `CouncilGame.AdjustCompanyIncome` |
| `DepartmentOfTheInterior.<DecolonizeProcessor>.MoveNext` (before `RemoveColonizedStarSystemAndPlanets`) | `CouncilGame.OnDecolonize` |
| `DepartmentOfScience.GetTechnologyState(ScienceConstructibleElement)` (every `ret`) | state goes through `TempleGame.AdjustTechState`: an Endless victory technology whose temple is not opened reports `NotAvailable` |
| `DepartmentOfScience.UnlockTechnology(ScienceConstructibleElement, ...)` (start) | returns at once when `TempleGame.BlocksUnlock` reports that the technology's temple is not opened |
| `VictoryManager.SetupInterpreterContextForContender` (end) | `CouncilGame.AdjustVictoryContext(manager, contender.Empires)`, then `TempleGame.AdjustScienceAllianceVictory(manager, contender.Empires, contender.Definition)` |
| `CustomHackingHelper.DrawTabDiplomacyAndEmpires` (end) | `CouncilDebug.DrawCouncilTools`, then `BattleDebug.DrawBattleTools` (crew reserve + morale readout, works without the Council) |
| `CouncilGame.SerializeAttributes` (ret) | appends the battle rework state: `CouncilGame.SerializeBattles` -> attribute `BattleState` -> `BattleRework.LoadState` |
| `ShipDesignModalWindow.OnAutoDesignCb` (start) | `BattleTemplates.AutoDesignCb(window)`: lay the hull out from the template variant the player chose for it, then the vanilla `ShipDesign.AutoDesign()` finishes the rest (battle rework, docs/BATTLES_REWORK.md §5) |
| `AdvancedEncounterReportModalWindow.Refresh` | хвост метода | `BattleReportStats.Refresh(window)` | what happened блок отчёта → тултип цифры урона (фаза 2); numbers from `Encounter.Groups[].Flotillas[].Ships[]` statistics |
| `FleetLine.Refresh` | хвост метода (2 return) | `BattleTemplates.RefreshFleetLineTooltip(line)` | состав флота — в нативном `FleetTooltip` строки флота (карта и панели систем), отдельных label'ов нет (фаза 2) |
| `ShipDesignModalWindow.RefreshButtons` (start) | `BattleTemplates.SyncDesignWindow(window)`: the game's Auto-design button label/tooltip shows the current variant; vanilla text when none is chosen |
| `GameClient.<CreateWarInstanceProcessor>.MoveNext` (after `CreateWarInstance`) | `CouncilGame.OnWarCreated` (war penalty) |
| `DepartmentOfDefense.ScrapShipsPreprocessor`, `DepartmentOfCommerce.SelloutTradableShipsPreprocessor` (start) | return false when a ship belongs to a council squad |

## UI
Native Endless Space 2 interface (no Unity prefabs needed — the game's own windows are reused at run time):

* **The Council window** (`src/GalacticCouncil/CouncilUiNative.cs`) is a run-time clone of the game's laws screen
  (`LawsManagementModalWindow`): the whole prefab hierarchy (modal frame, background, buttons, sounds, animations) is
  cloned with `Object.Instantiate`, the window component is swapped for `CouncilModalWindow : GuiModalWindow`, and the
  widget references (tab strip, card table, details pane, action buttons) are copied from the clone. The window is
  registered in the GuiManager window list, so ESC/right-click closing, the modal dimming and the input blocking behave
  exactly like for a vanilla window (the few non-public `GuiWindow` members needed for that — `Initialize`, `Loaded`,
  `GameReady`, `guiWindowsFromBackToFront` — live in Assembly-CSharp-firstpass and are reached through reflection).
* Four tabs built from the laws-screen tab prefab: **Voting** (the ballot card + one card per question; option cards
  "→ Empire" appear under the selected targeted/mandate question; the chairman's replacement card), **Laws** (active
  laws, where the votes come from, marked systems), **Mandates** (each mandate with progress and holder actions:
  hire the contingent, request a delivery, leave the Council with a native confirmation), **Results** (past votes).
* Voting is native-style: select a question card → its options appear as cards below; in the details pane the two
  buttons add votes (**left click +10, right click −10**), the second button resets the question; "Cast votes" submits
  the whole ballot. The same ±10 steppers sit on mandate cards.
* A **HUD launcher button** (a clone of the laws-screen vote button, pinned to the top-right of the game overlay)
  opens the window any time; it is tinted orange while a vote is open and not yet cast.
* There are **no autonomous Council popups or toast notifications**. Existing `Toast` call sites are silent
  compatibility sinks; state and results are read in the window itself. User-triggered **confirmations** still
  go through `GuiManager.ShowMessage` (native Validate/Cancel message box).
* The window does not pop up by itself when a vote opens; the HUD launcher is highlighted instead.

If the native construction fails (another game version, missing widgets), the mod automatically falls back to the old
IMGUI window (`CouncilUiImgui` in `src/GalacticCouncil/CouncilUi.cs`): a "Council" button appears while the Academy
window is open, with the same tabs Vote / Laws / Mandates / Results. The shared ballot state (`CouncilUi.Pending`)
is common to both UIs, so a vote begun in one can be finished in the other.

Build hardening (2026-10-25, found after the IMGUI fallback was seen in play): the window build no longer assumes the
donor clone is active — `GuiPanel.Awake` caches the panel's `AgeTransform` (a plain backing-field getter), and on an
inactive clone `AddComponent` never runs `Awake`, so `win.AgeTransform` was null and the build died at the AGE-graph
step, permanently condemning the Council to IMGUI. The clone is now activated before `AddComponent`, the AgeTransform
is written through its backing field if `Awake` still did not run (`WinPriv.SetAgeTransform`), the first content
`Rebuild()` inside `Setup` and the post-build HUD launcher can no longer fail the construction, and an IMGUI window
that is already open is handed over to the native one. Every build phase is named in the failure line
(`Galactic Council: native UI failed at '<step>', falling back to IMGUI.` + full exception in `Diagnostics.html`;
successful builds log `[GalacticCouncil] native window ready: ...`).

### "Gives votes" in the interface
* Council window, Voting tab: the toggle "Where the votes come from" lists every source with its rule and the live value for the player's empire (`CouncilGame.VoteSourceLines`).
* Game texts (`CouncilGameTexts.cs`, once per turn, again after a language change): a note is appended to the description of the Republic government ("Galactic Council: the Republic gives +40 votes") and of the Empire Development stages T3/T4/T5. The keys are found at run time by scanning the `GuiElement` database (names `Government*Republic*`, `TechnologyStage[Definition]EmpireDevelopment3..5`); a stage without a description gets a new one. The God-Mod Council section prints how many texts were found.
* Senator skills with votes already say so in their descriptions; mandate cards show "Votes: +50 / +100".

## Debug tools (God-Mod menu)

The tools are appended to the END of two existing God-Mod tabs (hook `Patcher.HookDebugTabs`; code in `CouncilDebug.cs`, `CouncilGameDebug.cs`, `HeroDebug.cs`).
Both sections are folded by default (a toggle line opens them).

**Endless Temples tools** are drawn at the TOP of the Diplomacy tab by `CouncilDebug.DrawTempleTools`, independently of
the Council state: for the currently controlled empire they show the hidden/studying/opened state, progress and temple
node of all four Endless technologies, with "Reveal temple" / "Open temple" buttons. The buttons send synchronised
`OrderCouncilVote(KindDebug, ...)` actions (`DbgTempleReveal = 18`, `DbgTempleOpen = 19`; parameter B is the technology
index). See [`ENDLESS_TEMPLES.md`](ENDLESS_TEMPLES.md).

**Tab "Diplomacy and empires" → "DEBUG: Galactic Council"** (acts on the empire you currently control; switch with "Play As"):
- state: phase, vote number, turns to the next vote, votes of the empire with the breakdown, Supreme Legate, keep threshold, alliance flag;
- "Open the Academy (any turn)" - the same as exploring the Academy system for the first time (`EventAcademyDiscovered` for every empire, `DiscoveredTurn`, `MinTurnsForFirstRequest = 0`) and then the Council is convened, so votes can be tested from turn 1;
- "Convene the Council now", "Open the vote now", "Vote at next turn", "AIs vote now", "Tally now", "Council window";
- "Give the senator class (vote purchases)" - every senator that represents a party of the current empire gets the whole
  `ClassCorporate` tree (the five purchases, the passive votes and the ultimate) at full level, free (`OrderUnlockHeroSkill`
  with `IsFree`), so the purchases can be tested without growing a senator;
- votes: +10 / +100 / −100, "Recompute from game", "Reset manual changes", "+ declared war (−20)", "Forget wars";
- alliance of the Supreme Legate: auto / alone (51%) / allied (67%) - this is how both keep thresholds of "Establishment of Absolute Power" are tested;
- every mandate: progress, holder, "Grant condition" (the Supreme Legate button grants two ordinary qualifications), "Give to me", "Take away";
- "Detect conditions from game", "Clear conditions", last results.
All actions are `OrderCouncilVote(KindDebug, action, param)`, executed by every client, so the state stays in sync. The Academy manager is taken from the game service (`IAcademyRequestsService`) when no turn has started since the load. "Convene" works only when the game has an Academy that is not eliminated; debug failures do not open popups (use the God-Mod status/state lines and Diagnostics).
Natural founding (without the button): the Academy must be discovered (`DiscoveredTurn`), the turn must be greater than that turn and at least the original `MinTurnsForFirstRequest` (captured before the delay is removed for talkability: as soon as the Academy is discovered, `CouncilGame.MakeAcademyTalkable` zeroes the delay so the vanilla cycle enters Request on the next turn-begin and the Academy can be talked to at once; the Council's own founding still waits for the original delay).

Recipe for the keep-vote: "Give to me" on the Supreme Legate → "Open the vote now" → vote (Absolute Power: option 0 = keep, 1 = remove) → "Tally now".

**Tab "Tech / laws / heroes" → "DEBUG: heroes"**:
- every regular hero of the empire: kind (+ dictator), level, XP, skill points, Mastery state; buttons +1 / +5 / +20 levels (the normal XP path: level-up rule and the senator cap still apply), "Mastery" (learns an ultimate skill for free), "+5 points"; the same for all heroes;
- hero gauge: pending hero choices, points per gauge and class, "+100 / +1000 / +10000" (the vanilla cheating order `OrderAddHeroUnlockGaugeClassPoints`; the choice appears at the end of the turn);
- direct recruiting: filter by kind (by hero class), hire level 1 / 5 / 10 / 20, list of all definitions ("already in play" is disabled), "random free hero" (`OrderRecruitHero`, synchronized).
Level, points and Mastery changes are applied to the local simulation (like the older God-Mod hero buttons), they are not orders.

## Playtest fixes (log analysis, 2026-10-24)
* **Hack target could not be chosen** — `CustomHackingHelper.GetValidModesForTarget` (from the hacking-fix baseline `6ae43ff`) had a
  broken `brfalse.s` at IL_0075 (target >127 bytes away, unresolved), so the game's Mono threw `InvalidProgramException` whenever the
  UI asked for the hack modes of a system: the system could only be used as a route node. `Patcher.RepairHackingModes` points the
  branch at the final `ldloc.0; ret` and uses the long form. `Patcher.CheckBranches` now fails the build if any method in the
  assembly has a branch/switch operand that does not resolve (Mono 6 in `verify.sh` does not notice that kind of damage).
* **Research queue ETA ignored the research laws** — queued items are costed through their `Construction` wrapper
  (`ICostProvider`), not through the `TechnologyDefinition`, so `AdjustProductionCost` did not recognise them. It now looks through
  `Construction.ConstructibleElement` (also fixes the Infrastructure Program estimate for queued improvements).
* **An AI empire died on turn 8 (bankruptcy)** — the vanilla quest `QuestDefeatByBailif` ("no money for the bailiff = defeat")
  eliminates an empire whose dust is still <= 0 after the bailiff has sold everything. Log: a 4-ship heavy squad (`Juggernauts_Battleship`)
  was created for the empire at the start of turn 7, then at the turn end the bailiff scrapped ships/improvements and
  `OrderMarkEmpireForElimination` followed. Squads granted by laws (Escort, Academy Squadron, Covert Group) are now free of upkeep
  (`CouncilCore.IsGiftSquadShip`, subtracted in `UpkeepDelta`); the hired Marshal contingent still costs upkeep.
  A squad created during the turn is only discovered at the next `ClientBeginTurn`, so `CouncilGame.BeforeTurnEnd` (discover squads +
  `RefreshEffects`) is called at the start of state 0 of `DepartmentOfTheTreasury.GameServerState_ComputeBailiff` and
  `GameClientState_Turn_End_CollectResources` (iterator `MoveNext`), i.e. before the bailiff check and before resources are collected.
  The same applies to the "Warship upkeep +100%" law: it can still push a poor AI empire into bankruptcy (not changed).

## Performance (audit 2026-10-25)
* **Diagnostics were compiled out.** `Amplitude.Diagnostics.Log/LogWarning/LogError` are `[Conditional("ENABLE_DIAGNOSTICS")]`, and `build_council.sh` never defined the symbol — every mod log line (build stamp, `Perf` stall reports, the whole `ModPerf` per-turn bucket line, temple/hero diagnostics) silently vanished at compile time while the timing code around them still ran. The build now compiles with `-define:ENABLE_DIAGNOSTICS`; look for one `[GalacticCouncil][perf] turn N: HeroFilter=.. ProdCost=.. TradeValue=.. TradePrice=.. CompanyIncome=.. CouncilClientBeginTurn=.. BeforeTurnEnd=.. UiElements=.. (ms, per-call avg/max/count)` line per turn in the game's `Diagnostics.html`, plus a `ClientBeginTurn total=.. hero=.. temple=.. texts=.. detect=.. tally/open/ai=.. effects/buys=..` breakdown whenever a begin-turn pass exceeds 50 ms.
* **`RefreshEffects` is per-empire now.** It runs at `ClientBeginTurn` and twice inside the turn-end states (`BeforeTurnEnd`). Previously one global signature included every member's upkeep delta, so any member's fleet change recreated the empire descriptor and `emp.Refresh(false)` for **all** members — and the `ClearEffects` on that path also wiped the per-system law effects, refreshing every affected `ColonizedStarSystem` and forcing their rebuild next turn. Now each empire has its own signature (`appliedFx`); unchanged empires cost one signature comparison, only the changed empire is rebuilt/refreshed, system effects are untouched here (`ClearSystemEffects` only on disband), and `UpkeepDelta` is computed once per empire per pass instead of twice.
* **Temple node map cached.** `TempleGame.BuildNodeMap` is memoized per game+turn; `ResolveNode` (also called from the God-Mod temple tab lines every IMGUI event) no longer rebuilds the GUID dictionary per call.
* **One-time generation runs on the loading screen.** `CouncilGame.EnsureEarlyContent` is prepended to `GameClientState_LaunchGame.Begin` / `GameServerState_LaunchGame.Begin` (the states that show the session loading screen) and runs `HeroRework.EnsureContent` (all reworked hero skills), `EnsureDictatorDefinitions`, `EnsureVoteTexts`, `EnsureDevQuadrants` and the static constructors of `CouncilLaws`/`HeroDesign` before the map becomes interactive. Before this, the hero content was only triggered lazily by `HeroSkill.Load`/`TreesFor` — and a NEW game has no heroes at generation time, so the generation hitched the first hero/academy action with the map already on screen. Every part is self-guarded (dictionary lookups once done; runs over 25 ms are logged as `one-time content generated on the loading screen in ..ms`), and the old lazy hooks remain as safety nets (e.g. after a database reload when the mod set changes in the main menu).
* **Known remaining costs (measured via the perf line, not guessed).** The first turn after launch still pays Mono JIT of the cloned mod methods on their first execution, and windows build their Age UI on first open (user-initiated). The ES2 simulation itself is single-threaded and deterministic — mod hooks cannot be moved to background threads; the lever is caching/early-outs and avoiding `Refresh(false)` cascades, as above.

## Mandate powers of AI empires (single player against computers)
The buttons of the Council window are for humans; for an AI the same powers are run by `CouncilGame.AiMandateActions` (`CouncilGameAi.cs`) on every machine right after the tally/opening step of the turn, so the state stays identical:
* **Chairman**: right after a vote opens (nobody has voted yet) `CouncilCore.TryAiChairman` replaces one ordinary question by a random implemented ordinary law that does not hurt anyone (`Benefit >= 0`), once per vote, seed from the vote number.
* **Quartermaster**: the delivery is taken as soon as it is available (once per cycle).
* **Marshal**: the contingent is hired once per mandate, but only when the AI can pay for it: net Dust income >= 25 per contingent ship and a stock >= 150 x stage (`AiCanAffordMarshal`); otherwise it is retried every turn. This guard exists because the upkeep is paid by the holder and a bankruptcy eliminates an empire.
* **Leaving the Council** is never done by the AI.
* Mentoring now also levels the heroes of AI members.
Unverified in game: that an AI treasury returns a sensible net income at the start of the turn, the thresholds above, that the host posts the contingent for an AI empire.

## Hacking popup hidden
`NotificationMinorAndPirateHackingEnabled` (the only vanilla notification bound to `EventMinorAndPirateHackingUnlocked`, raised once per empire by `HackingManager.ClientBeginTurn` at the PvE hacking unlock turn) now returns false from `Bind` (`Patcher.HideHackingNotice`), so `GuiNotificationManager.RecordEventForEmpire` never creates it. The event itself is still raised because the AI listens to it. Not game-tested; if another hacking popup remains, its title is needed to find the event.

## Hacking end-turn warning removed
`DepartmentOfIntelligence.EndTurnValidator` (the check run by the "End turn" button) used to raise `EventHackingOperationLeftUnassigned` and refuse the first press while an empire had a free hacking slot and a possible target (re-armed every 5 turns). `Patcher.DisableHackingEndTurnWarning` makes it return true at once; the other validators (research, construction, heroes, treasury, ...) are untouched. Not game-tested.
