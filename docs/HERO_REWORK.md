# Hero rework

Status: Stages 1–3 are implemented. Nothing has been tested in the running game; checks so far are compilation, Mono JIT of every method (`tools/verify.sh`), unit tests (`tests/HeroKindsTests.cs`) and a smoke test on real simulation objects (`tools/Verify/HeroSmoke.cs`).

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

## Senators and the dictatorship (Stage 3)
* **Level cap.** A senator hero (not the dictator) stops at hero level `HeroKinds.SenatorLevelMax` = 8 (seven level-ups). Done by giving the hero a clone of his `LevelUpRuleDefinition` with `LevelMax = 8` at the start of `Hero.UpdateLevel(bool)` (`HeroRework.BeforeUpdateLevel`). Heroes already above the cap keep their level.
* **Official ideology** is the politics of the prime senator (`Senate.MainPolitics`); the prime senator is the one with the highest `Support`.
* **Dictatorship** is detected by the senate simulation tags `GovernmentDictatorship` / `GovernmentDictatorshipCravers` (the same ones `Senate.OnLoad` uses).
  * When a dictatorship starts, its leader is the prime senator at that moment. This is the "chosen in advance" senator: the player prepares him with the elections before switching. The senate is trimmed to this one senator (`RemoveSenator`).
  * While the leader rules, the ideology is locked against external factors: `Senate.ParseElectionResult` (start hook `LockElection`) rewrites the candidate list to the leader only, and `Senate.ServerTriggerRebellion` (start hook `BlockRebellion`) does nothing, so unrest and rebellion-causing events cannot overthrow the government and replace the ideology. System-level riots still happen.
  * The player is the only one who can end it: by changing the government (abdication, `OrderSwapGovernment`) or when the government changes for another reason the turn pass forgets the record (`UpdateDictatorships`).
  * The record (`empireIndex=politics=senatorGuid`) is persisted after the ultimates, split by `;`.
* **Dictator hero** = the representative hero of the leader senator (a senator kind hero, class Corporate). He has no level cap (only the vanilla rule), and the same ultimate rule as admirals/governors: after his ultimate no new skills are offered, every learned skill gets `+DictatorUltimateBoost` (x2.75 effect, "2 levels become 5–6") plus the per-level growth `100 / ultimateLevel` percent. The title shows `[Диктатор]`.

## Hooks
| Method | Hook |
|---|---|
| `Hero.Get{System,Ship,Senate,…}SkillDescriptors` (8) | `HeroRework.Filter*` (kind filter + auto bonus) |
| `Hero.get_SkillTrees` | `HeroRework.FilterTrees` |
| `Hero.ComputeAvailableSkills` (before each `ret`) | `HeroRework.AfterComputeAvailable` |
| `GuiHero.get_Title` | kind in the title |
| `CustomHackingHelper.CheckHeroAssignableToFleet/ToSystem` | kind check |
| `Senator.UpdateRepresentativeHero` | senator kind check |
| `Hero.UpdateLevel(bool)` (start) | `HeroRework.BeforeUpdateLevel` (senator level cap) |
| `Senate.ParseElectionResult` (start) | `HeroRework.LockElection` |
| `Senate.ServerTriggerRebellion` (start) | `HeroRework.BlockRebellion` |
| `CouncilGame.ClientBeginTurn` / `SerializeAttributes` | `HeroRework.PerTurn` / registry persistence |

## Not yet done
* Stage 4: party trees (replace "Universal"), unique per-kind race trees and the dictator tree, authored senator skills (a senator has only 3 useful vanilla skills for 7 level-ups), stronger faction-specific laws and abilities of the leader — these need authored skill definitions plus matching `GuiElement` entries.

## Unverified assumptions
The refresh via `OnSkillsUpdated` on level change, acceptance of runtime descriptor copies by `SimulationObject`, `Hero.get_SkillTrees` being the only GUI path, the stage-threshold clamp, already-assigned mismatched heroes staying assigned without effect, the `FailureInfo` tooltip, and that `Hero.Level` is the quantity the user means by "level".
Stage 3: that the dictatorship tags are present on the senate object under every dictatorship government, that skipping `ServerTriggerRebellion` (also its per-system rebellions caused by the empire rebellion) is acceptable, that vanilla dictatorships may have several senators, that `LevelMax` on a cloned rule is honoured by `UpdateLevel`/GUI and that the starting hero level is 1, and the prime-senator-at-start choice of the leader.
