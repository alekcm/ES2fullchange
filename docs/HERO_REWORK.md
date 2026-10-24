# Hero rework

Status: Stages 1–2 are implemented. Nothing has been tested in the running game; checks so far are compilation, Mono JIT of every method (`tools/verify.sh`), unit tests (`tests/HeroKindsTests.cs`) and a smoke test on real simulation objects (`tools/Verify/HeroSmoke.cs`).

## Kinds (Stage 1)
The kind follows the vanilla hero class (`HeroDefinition.Class`):

| Class (ship design) | Kind |
|---|---|
| `HeroClassAdmiral` (Attacker) | Admiral |
| `HeroClassAdventurer` (Explorer) | Admiral — its tree is almost all ship skills |
| `HeroClassAdministrator` (Defender) | Governor |
| `HeroClassCorporate` (Supporter) | Senator |

A hero of an unknown (modded) class falls back to `GUID % 3`. AI-ruler heroes are not touched. Trees that keep no useful skill for the kind are dropped, so a kind only gets branches that exist for it.
The mapping lives in `HeroKinds.TryKindOfClass`.
The skill descriptors a hero provides are filtered by channel: Admiral → Ship, Governor → System, Senator → Senate. The Hero channel is kept for all.
Skill trees are rebuilt per kind (skills with no effect for the kind are dropped, stage thresholds are clamped to the points that remain).
Assigning a hero to the wrong job (`CheckHeroAssignableToFleet/ToSystem`, `Senator.UpdateRepresentativeHero`) is refused.
The hero title shows the kind (`GuiHero.get_Title`).

## Ultimate and automatic growth (Stage 2), admirals and governors only
* A hero's *ultimate* is any skill of the final stage (maximum `RequiredLevel`) of any of his ORIGINAL trees (`HeroDefinition.GetSkillTrees()`), because after kind filtering the final skill of a tree often belongs to another channel.
* The level at which it is taken is recorded (`HeroRework.ultimates`). From then on `Hero.availableSkillDefinitionNames` is emptied: the hero offers no more skills, specialization is fixed.
* Every hero level above that gives every learned skill an extra `100 / ultimateLevel` percent of its effect: scaled copies of the skill descriptors are appended to the lists returned by `Hero.Get*SkillDescriptors`.
  Scaling: Addition/Subtraction/Percent are linear, Multiplication scales the excess over 1, Binary(property × constant) scales the constant; other operations (Force, Min, Max…) and property-reference values are not scaled.
  Copies are new objects with fresh `ModifierId`, non-serializable, cached by (descriptor, bonus).
* A turn pass (`HeroRework.PerTurn`, from `CouncilGame.ClientBeginTurn`) refreshes a hero's effects (`Hero.OnSkillsUpdated`) when his level changed.
* Dummy heroes (skill editing window copies) never record an ultimate: only the entity registered in `IGameEntityRepositoryService` does.
* Persistence: the registry (`guid:level,…`) is appended to the `CouncilState` attribute after `@@HR@@`; old saves load with an empty registry (a hero already holding an ultimate is fixed at his current level on the first turn pass).

## Hooks
| Method | Hook |
|---|---|
| `Hero.Get{System,Ship,Senate,…}SkillDescriptors` (8) | `HeroRework.Filter*` (kind filter + auto bonus) |
| `Hero.get_SkillTrees` | `HeroRework.FilterTrees` |
| `Hero.ComputeAvailableSkills` (before each `ret`) | `HeroRework.AfterComputeAvailable` |
| `GuiHero.get_Title` | kind in the title |
| `CustomHackingHelper.CheckHeroAssignableToFleet/ToSystem` | kind check |
| `Senator.UpdateRepresentativeHero` | senator kind check |
| `CouncilGame.ClientBeginTurn` / `SerializeAttributes` | `HeroRework.PerTurn` / registry persistence |

## Not yet done
* Stage 3: senator level cap (~6–8 levels, empire-wide skills) and the dictatorship (one pre-chosen faction-leader senator, ideology locked while he rules, 30+ levels, ultimate that locks unlearned skills and adds levels to learned ones).
* Stage 4: party trees (replace "Universal"), unique per-kind race trees and the dictator tree — these need authored skill definitions plus matching `GuiElement` entries.

## Unverified assumptions
The refresh via `OnSkillsUpdated` on level change, acceptance of runtime descriptor copies by `SimulationObject`, `Hero.get_SkillTrees` being the only GUI path, the stage-threshold clamp, already-assigned mismatched heroes staying assigned without effect, the `FailureInfo` tooltip, and that `Hero.Level` is the quantity the user means by "level".
