# Galactic Council (Stages 1-3)

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

Still not implemented (registered, `Implemented=false`, never offered): research +-50%, warship upkeep laws, weapon damage -20%,
evacuation compensation, escort/Academy squadrons, Strategic Presence, Fortification Inspection, Capital Reinforcement,
Infrastructure, Antitrust, Trade Company Reinvestment, Academic Truce, Academy-home-world 50% capital rule.
Academic Truce: `SetDiplomaticRelationState` handles the relation, but nothing found destroys the `WarInstance`, so it was left out.

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
| `DepartmentOfDefense.ScrapShipsPreprocessor`, `DepartmentOfCommerce.SelloutTradableShipsPreprocessor` (start) | return false when a ship belongs to a council squad |

## UI
An IMGUI window (no Unity prefabs exist in this repo): a "Council" button appears while the Academy window is open;
tabs Vote / Laws / Mandates. Toasts announce vote start and results.
