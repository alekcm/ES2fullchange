# Endless Temples — science-victory rework

The four Endless victory technologies no longer become researchable merely because the game has made them available.
An empire must first study the matching **Temple of the Ancients** at a special node. Opening a temple only removes the
additional lock: the technology itself still has to be researched normally afterwards.

## Gameplay flow

1. `TempleGame.PerTurn` runs from `CouncilGame.ClientBeginTurn`, before the Galactic Council's own early returns, so the
   temples work whether or not the Council exists or is enabled.
2. For every surviving major empire, the game asks `DepartmentOfScience.GetTechnologyState` with the temple block
   temporarily suppressed (`TempleGame.SuppressBlock`). If the vanilla state is `Available`, `Queued` or `InProgress`,
   the empire has met the game's normal condition and the temple is revealed to that empire instead.
3. The first empire to reveal a given technology fixes that technology's temple node for the whole game. Other empires
   receive no marker or reveal message merely because somebody else revealed it; when they qualify later, their temple
   is at the same already-recorded node.
4. A qualifying research Behemoth must remain at the temple. Progress is per empire: several empires study the same
   shared node independently, but one empire's opening does not open the technology for another.
5. Study takes **10 turns on standard speed**, scaled at reveal through `CustomHackingHelper.ScaleTurns`. Progress is
   stored in half-turns: **+1 turn** for each turn with a qualifying Behemoth present and **−0.5 turns** for each turn
   without one, never below zero. The ship may be replaced by another qualifying Behemoth; only presence matters.
6. When the requirement is reached, the temple opens for that empire and its technology becomes researchable. Every
   other player receives the radio-hum message quoted below.
7. If the galaxy contains no suitable special node for a technology, `TempleCore.NodeNoneInGalaxy` is recorded and the
   temple opens immediately at reveal rather than making a science victory impossible.

An Endless technology that was already researched in a save from before this rework is recognised in `PerTurn` and its
temple is marked opened without a study.

## Technologies and temple nodes

| Technology | Game definition | Eligible special-node types |
|---|---|---|
| Власть Вечных / Power of the Endless | `TechnologyDefinitionEmpireDevelopmentVictory` | Black Hole, Neutron Star, Nebular Clouds, Rejuvenation Field |
| Гений Вечных / Genius of the Endless | `TechnologyDefinitionScienceAndExplorationVictory` | Black Hole, Neutron Star, Asteroid Field, Solar Nebula |
| Могущество Вечных / Might of the Endless | `TechnologyDefinitionMilitaryVictory` | Collapsing Star, Nebular Clouds, Rejuvenation Field |
| Достояние Вечных / Wealth of the Endless | `TechnologyDefinitionEconomyAndTradeVictory` | Collapsing Star, Asteroid Field, Solar Nebula |

Matching is deliberately based on case-insensitive substrings in `SpecialNodeDefinition.Name` or `.Category`
(`TempleDefs.NodeMatches`). The implementation tokens are:

- Power: `BlackHole`, `NeutronStar`, `SpaceClouds`, `Rejuvenation`
- Genius: `BlackHole`, `NeutronStar`, `AsteroidField`, `SolarNebula`
- Might: `CollapsingStar`, `SpaceClouds`, `Rejuvenation`
- Wealth: `CollapsingStar`, `AsteroidField`, `SolarNebula`

### Node choice

`TempleGame.ChooseTempleNode` scans `game.Galaxy.SpecialNodes` and skips destroyed nodes, nodes without a definition,
and definitions with `StartInvisible` or `IgnoreForGalaxyDiscovery`. A node is neutral when
`SpecialNode.SystemWhichInfluences == null`. Candidate GUIDs are sorted and passed to the deterministic xorshift in
`TempleCore.PickTemple`, so every machine chooses the same node.

The preference order is:

1. neutral nodes that are not already another technology's temple;
2. any eligible nodes that are not already another technology's temple;
3. neutral eligible nodes, including an already-used temple node;
4. any eligible node;
5. `NodeNoneInGalaxy` when there are no candidates.

## What counts as a research Behemoth?

The temple checks fleets docked at the `SpecialNode` first (`GameNode.DockedEntities`). As a fallback it also checks
the empire's fleets whose `NodePosition` is exactly the temple node's position. A fleet qualifies only when it belongs
to the studying empire, is neither destroyed nor moving, and contains a ship for which:

- `Ship.IsJuggernaut` is true (the Behemoth hull's internal name); and
- at least one fitted module definition name contains `JuggernautAnalyzer` or `JuggernautResearch`.

Both the special-node analyzer family and the Juggernaut research-module family are therefore accepted.

## Technology lock and pre-existing research queues

`Patcher.HookTemples` installs two guards:

- every return of `DepartmentOfScience.GetTechnologyState(ScienceConstructibleElement)` passes through
  `TempleGame.AdjustTechState`: an unopened Endless technology reports `NotAvailable` instead of the vanilla
  `Available`, `Queued` or `InProgress` state;
- the element overload of `DepartmentOfScience.UnlockTechnology` starts with `TempleGame.BlocksUnlock`, so another
  unlock channel cannot grant the technology before its temple opens.

New queue orders are consequently rejected by the vanilla research flow. A save made before the rework can nevertheless
already contain a queued Endless technology. While its temple remains unopened, `TempleGame.PerTurn` calls
`CancelQueuedTempleResearch`. That helper follows the vanilla cancellation subset: it refunds positive instant-cost
stocks through `DepartmentOfTheTreasury.TryTransferResources`, calls `ResearchQueue.Remove`, and raises
`OnResearchQueueChanged(..., Cancelled)`. The construction deliberately remains registered/in researches-in-progress,
again as with vanilla cancellation, so accumulated non-instant science is preserved rather than discarded. If an
instant-cost refund fails, the construction is left queued.

## Alliance science victory

The alliance rule is changed from a summed score to the best single member. At the end of
`VictoryManager.SetupInterpreterContextForContender`, the patch first calls the existing
`CouncilGame.AdjustVictoryContext` and then
`TempleGame.AdjustScienceAllianceVictory(manager, contender.Empires, contender.Definition)`.

For a contender with more than one empire and definition `VictoryScience`, the second call registers:

- `SummedMajorEmpiresScore` = the largest number of the four Endless technologies researched by any one member;
- `NumberOfMajorEmpiresInAlliance` = `1`.

The vanilla formula therefore evaluates the best member rather than adding partial progress across the alliance. If
at least one member has researched all four, the whole alliance achieves the science victory. Single-empire contenders
and other victory definitions are unchanged.

## Messages and galaxy marker

`TempleUi` is independent of the Council window and of the intentionally silent Council toast sink. It owns a persistent
Unity host named `__ES2EndlessTemplesUI__`, queues dismissible messages, and displays a galaxy-map label for each temple
that is revealed to the local empire but not opened yet. `TempleGame.LocalMarkers` supplies the technology and current
progress; `TempleUi` finds the matching `GalaxySpecialNode` scene object and projects its position with
`Camera.main.WorldToScreenPoint`.

The reveal message names the special node and its type, tells the player to bring a research Behemoth there, and shows
the scaled number of study turns. On completion the opening empire gets its own confirmation. Every other local player
gets, with the empire name substituted:

> «По вселенной прошёл неслыханный до селе радиогул. Наши шпионы докладывают, что %название фракции% открыли один из Храмов Древних.»

The wording **«до селе»** is preserved literally as requested.

## Persistence and determinism

`TempleCore` is pure C# with no game dependencies. It stores the shared `TempleNode[4]` GUIDs and, per empire and
technology, a `Hidden`/`Revealed`/`Opened` state, study progress in half-turns, and the scaled requirement fixed at
reveal. Its invariant-culture `TM1` serialization is appended to the existing save attribute `CouncilState` after the
separator `@@TM@@`; saves without that segment load a fresh temple state.

All ordinary state transitions happen in the deterministic beginning-of-turn processing. `TempleGame.PerTurn` has its
own game/turn guards, including protection against stale state when a new game starts in the same process.

## AI status

AI empires currently follow exactly the same lock and study rules as humans. There is deliberately no AI exemption and
no automatic opening: teaching the AI to send and keep a research Behemoth at a temple is a later task. Until then the
vanilla AI is not expected to complete this path to a science victory.

## Implementation map

| File | Responsibility |
|---|---|
| `src/GalacticCouncil/TempleCore.cs` | Technology/node definitions, pure temple state, study arithmetic, deterministic node picking, `TM1` serialization |
| `src/GalacticCouncil/TempleGame.cs` | Game glue: reveal and node choice, turn processing, Behemoth presence, queue cleanup, victory-context adjustment, UI/debug data |
| `src/GalacticCouncil/TempleUi.cs` | Persistent message queue and galaxy-map temple markers |
| `src/GalacticCouncil/CouncilGame.cs` | Calls `TempleGame.PerTurn`; serializes the `@@TM@@` segment in `CouncilState` |
| `src/GalacticCouncil/CouncilUi.cs` | Ensures the `TempleUi` host exists |
| `src/GalacticCouncil/CouncilDebug.cs`, `CouncilGameDebug.cs` | God-Mod temple status and synchronised reveal/open tools |
| `tools/Patcher/Patcher.cs` | `HookTemples` (`AdjustTechState`/`BlocksUnlock`) and the temple call in `HookVictoryContext` |
| `tests/TempleCoreTests.cs` | Standalone tests for definitions, shared nodes, study/decay, scaled requirements, fallback, picking and serialization |

## Debug tools

At the top of the God-Mod **Diplomacy** tab, independently of whether the Galactic Council exists,
`CouncilDebug.DrawTempleTools` shows each technology as hidden, studying with progress, or opened, together with its
temple node. **Reveal temple** and **Open temple** buttons are sent as synchronised
`OrderCouncilVote(KindDebug, ...)` actions:

- `DbgTempleReveal = 18`;
- `DbgTempleOpen = 19`;

parameter B is the technology index. `DebugOpen` reveals the temple first when necessary, then marks it opened and
produces the normal opening/radio-hum messages; it does not directly grant the technology.

## Tests and rebuild

After the repository's normal Mono/reference-assembly setup:

```bash
source tools/env.sh
mkdir -p /tmp/build
csc -codepage:65001 -out:/tmp/build/TempleCoreTests.exe \
  tests/TempleCoreTests.cs src/GalacticCouncil/TempleCore.cs
mono /tmp/build/TempleCoreTests.exe

bash tools/build_council.sh
bash tools/build_patch.sh
bash tools/verify.sh 20
```

`build_patch.sh` writes the merged result to `Managed/Assembly-CSharp.dll`; `verify.sh` Mono-JITs the patched assembly
as an IL sanity check without running the game.
