# Galactic Council (Stage 1)

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
Not done yet (Stage 2): research +-50%, Mentorship, mandates (+ their votes/UI), victory by Supreme Legate,
leaving the Council, squadrons, market/trading laws, etc.

## Hooks in the game assembly
| Where | What |
|---|---|
| `AcademyRequestsManager.ClientBeginTurn` | body replaced by `CouncilGame.ClientBeginTurn(this)` |
| `AcademyRequestsManager.SerializeAttributes` | appends `CouncilGame.SerializeAttributes` (attribute `CouncilState`, save version >= 7) |
| `DepartmentOfTheTreasury` | new order processor `CouncilVote` (`OrderCouncilVote`) |
| `GameClient.SpawnHackingOperationBackdoorProcessor` | exits early while Counterintel is active |
| `OverhaulOutcomeDefinition.CheckHackingEmpireAndTarget`, `CustomHackingHelper.BuildPromptElement` | sleeper count goes through `CouncilGame.SleepersForHack` |
| `AcademyModalWindow.OnBeginShow/OnBeginHide` | shows/hides the "Council" launcher button |

## UI
An IMGUI window (no Unity prefabs exist in this repo): a "Council" button appears while the Academy window is open;
tabs Vote / Laws / Mandates (stub). Toasts announce vote start and results.
