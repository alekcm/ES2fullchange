# Galactic Council (Stages 1-6)

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
100 votes, spent in steps of 10. "All" laws: accept/reject. "One" laws: pick the target empire.

## Stage 1 laws (functional)
Merit (+30 votes), Expansion Mandate (+1 colony limit), Expansion Limit (-1), Dust payout per colony,
Replenishment (manpower x1.5), Exchange of Intelligence Methods (+4 sleepers), Non-Destabilization Pact
(no sleeper-based hacks), Academy Counterintelligence (no new backdoors).

Other laws from the spec are registered in `CouncilLaws` but `Implemented=false`, so they are never offered yet.

## Stage 2 (added)
Laws: Academic Mentoring (+3 levels to all active heroes of non-AI members, posted by the host), Market Deregulation
(x0.5 buy price of Strategic/Luxury resources), Letters of Marque (x0.5 buy price of ships), Right of Plunder (x2 loot of
`GroundBattleOutcomeLootDefinition.FillLoot`), Trade Restrictions (buy x2 / sell x0.5 for the target).

Mandates (`CouncilMandates.cs`, state saved as `GC2`): Chairman, Marshal, Quartermaster, Eye, Supreme Legate.
* Votes for a mandate (Mandates tab) come out of the same 100 votes; the winner replaces one law question of the next vote
  (at most one mandate question per vote) and the holder is chosen among the empires that qualify. Holding lasts until
  re-election, leaving the Council or the Academy's destruction.
* Chairman: replace one ordinary question once per cycle before anybody voted (Voting tab).
* Marshal: hires a warship group of the empire's own faction at the home system (upkeep paid by the empire).
* Quartermaster: once per cycle Dust + Science delivery scaled by stage and game length.
* Eye: +1 simultaneous hacking operation, +20 `MaximumEmpireProcessingPowerStock` (the "ПВ" reading is a guess).
* Supreme Legate: x2 votes, No-Confidence forced on the next vote and while the countdown runs; after 30 turns (scaled)
  `IVictoryManagementService.ForceRiseVictory` is called with the victory condition whose name contains `diplomat`/`senate`/...
* Leaving the Council: the Academy relation is pinned to -100 every turn (static `AcademyDiplomaticRelation.SetRelationPoints`).
* Assignments are a **stub**: six buttons in the Mandates tab count one finished assignment of a category.

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

## Hooks in the game assembly
| Where | What |
|---|---|
| `AcademyRequestsManager.ClientBeginTurn` | body replaced by `CouncilGame.ClientBeginTurn(this)` |
| `AcademyRequestsManager.SerializeAttributes` | appends `CouncilGame.SerializeAttributes` (attribute `CouncilState`, save version >= 7) |
| `DepartmentOfTheTreasury` | new order processor `CouncilVote` (`OrderCouncilVote`) |
| `GameClient.SpawnHackingOperationBackdoorProcessor` | exits early while Counterintel is active |
| `OverhaulOutcomeDefinition.CheckHackingEmpireAndTarget`, `CustomHackingHelper.BuildPromptElement` | sleeper count goes through `CouncilGame.SleepersForHack` |
| `AcademyModalWindow.OnBeginShow/OnBeginHide` | shows/hides the "Council" launcher button |
| `Tradable.GetValue`, `TradableResource.GetValue` (every `ret`) | value goes through `CouncilGame.AdjustTradableValue` |
| `Marketplace.ITradingManagementService.GetTradablePrice` (every `ret`) | price goes through `CouncilGame.AdjustTradePrice` |
| `GroundBattleOutcomeLootDefinition.FillLoot` (end) | `CouncilGame.AdjustLoot` |
| `DepartmentOfTheTreasury.GetProductionCostWithBonus(..., Single)` (every `ret`) | value goes through `CouncilGame.AdjustProductionCost` |
| `ServerEncounterState_RunSimpleBattle.ComputeSimplifiedEffectiveDamagePercentage` (end) | `CouncilGame.AdjustSimplifiedDamage` |
| `ServerGroundBattle.GetColonizedStarSystemOpponentPropertyValue` (every `ret`) | value goes through `CouncilGame.AdjustOpponentProp` |
| `DepartmentOfCommerce.GetTradingCompanyIncomeMultiplier` (every `ret`) | value goes through `CouncilGame.AdjustCompanyIncome` |
| `DepartmentOfTheInterior.<DecolonizeProcessor>.MoveNext` (before `RemoveColonizedStarSystemAndPlanets`) | `CouncilGame.OnDecolonize` |
| `VictoryManager.SetupInterpreterContextForContender` (end) | `CouncilGame.AdjustVictoryContext` |
| `DepartmentOfDefense.ScrapShipsPreprocessor`, `DepartmentOfCommerce.SelloutTradableShipsPreprocessor` (start) | return false when a ship belongs to a council squad |

## UI
An IMGUI window (no Unity prefabs exist in this repo): a "Council" button appears while the Academy window is open;
tabs Vote / Laws / Mandates. Toasts announce vote start and results.
