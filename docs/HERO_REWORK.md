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

## Skill content (Stage 4)
All skills are generated at run time from the live skill database (`HeroContent.cs`, pure data in `HeroSkillData.cs`) and registered in the game databases together with `GuiElement`s and localization (`CustomHackingHelper.RegisterLoc`), so no game XML is shipped.

* **Roles** (code in the skill name): `@A` admiral, `@G` governor, `@S` senator, `@D` dictator. Every skill of every vanilla tree exists in all four versions: `<VanillaName>@A` etc.
* **Conversion.** Each vanilla modifier is classified into a *flavor* (offense, defense, mobility, sustain, upkeep, production, growth, science, money, influence, happiness, manpower, deposit, experience, diplomacy, council votes) with a magnitude (1 = a typical second-level skill). The effects of the role's own channel are kept; the effects of the other channels become the flavor's effect in the role's channel, so no skill is wasted: "+10 production / +30% damage" gives the governor "+10 production / +0.2 garrison health" and the admiral "+20% hull plating / +30% damage". Penalties, forced values and non-numeric modifiers are not converted; a level that would stay empty gets a modest effect by its mastery.
* **Senator version:** one level (the strongest vanilla level), stage thresholds halved (0/2/4/6 points: seven level-ups reach the last stage), effects empire-wide through the senate channel.
* **Dictator version:** senate channel, 3 levels for two-level skills, effects x`DictatorPower` (1.6), thresholds x1.5 (0/6/12/18).
* **Trees of a hero** (`BuildHeroTrees`): his race tree and class tree with the skills of his role (an admiral Sophon and a governor Sophon have different Sophon trees), and the **party tree** (`HRTreeParty0N@code`) instead of "Universal". Party themes: 1 Industrialists (production, money, manpower), 2 Scientists (science, experience, influence), 3 Pacifists (happiness, **council votes**, growth), 4 Ecologists (growth, deposits, sustain), 5 Religious (happiness, influence, **council votes**), 6 Militarists (offense, defense, manpower). Seven skills per tree: stage 0: A, B; stage 1: C, A+B; stage 2: B+C, A+C; stage 3: ultimate A+B+C.
* **Dictator** gets the senate version of his race tree, the dictator version of his party tree and a **unique faction tree** (`HRTreeDict<Faction>`) instead of the class tree. Faction themes: Sophons science/food/influence, Cravers war/manpower/industry, Terrans industry/money/influence, Venetians money/influence/council votes, Vampirilis influence/offense/sustain, Time Lords science/mobility/experience, Unfallen defense/food/happiness, Endless science/influence/industry, and so on; the tree ultimate also gives happiness and manpower. A hero who becomes or stops being the dictator gets his skills recomputed.
* **Ultimate** = a skill of the last stage of the hero's *own* (new) trees.
* `HeroSkill.Load` (start) calls `HeroRework.EnsureContent()` so saves find the skills by name.
* **Council votes** (`Flavor.Votes`): senator and dictator skills of this flavor have no senate modifier; the Galactic Council reads the learned skills (see `docs/GALACTIC_COUNCIL.md`). Governors and admirals get influence instead.
* Magnitudes are balance guesses in `HeroSkillData.cs` (target tables and `Classify` reference values): tune there.

## Playtest follow-ups (2026-10-24)
* **Dictator hero is given.** While a *human* empire has a dictatorship (including a start with one and the Cravers' authoritarian rule), `UpdateDictatorships` calls `SyncDictatorHero`: if the empire has no hero whose definition name starts with `HeroDefinitionGCDictator_`, the client of the player posts a normal `OrderRecruitHero` (level 1, once per 3 turns at most). When the dictatorship ends, or the leader senator has another politics, the hero is removed with `DepartmentOfEducation.RemoveHero` (on every client, the representative links of the senators are cleared first and recomputed after). AI empires are not touched.
  * Definitions are generated at run time, one per (faction, politics): hidden, `IgnoreInstanceNumber`, class/ship/level rule of a vanilla Corporate hero, affinity of a hero of the faction (fallback `Other` = any Corporate hero), race tree of that faction hero, politics = the politics of the leader, so `Senator.UpdateRepresentative` picks him (a dictator definition always wins over a regular hero of the same party). GUI element copied from the template, title "Диктатор: <фракция>".
  * Registered by `HeroRework.EnsureDictatorDefinitions` at the start of `DepartmentOfEducation.SerializeElements` (before a save reads its heroes) and again on demand.
  * The faction of the empire is read from `Faction.Name`, `GetAffinityMappingName()` and `GetAffinityGameplayName()` through `HeroFlavors.FactionKey`; an unknown faction uses the `Other` set (the log line `[HeroRework] dictator hero definitions: N` is written once per registration).
* **No skill points after the ultimate.** `Hero.get_AvailableSkillPoints` is wrapped by `HeroRework.AdjustAvailablePoints`: 0 for a hero whose ultimate is registered (admiral, governor, dictator, senator with ultimate). This removes the "assign skills" prompt, the level-up window and the points counter; the per-level percentage growth of the learned skills is unchanged.
* **Passive experience of senators.** Each turn every senator kind hero of a human empire gets `NextLevelRequestedExperience / 10` (`/ 6` for the dictator) through `Hero.SetExperience`; the level cap (8 for senators) and the level-up rule decide whether it counts. Constants: `HeroKinds.SenatorXpTurnsPerLevel`, `DictatorXpTurnsPerLevel`.

## Hooks
| Method | Hook |
|---|---|
| `Hero.Get{System,Ship,Senate,…}SkillDescriptors` (8) | `HeroRework.Filter*` (kind filter + auto bonus) |
| `Hero.get_SkillTrees` | `HeroRework.TreesFor` (role trees) |
| `HeroSkill.Load` (start) | `HeroRework.EnsureContent` |
| `Hero.ComputeAvailableSkills` (before each `ret`) | `HeroRework.AfterComputeAvailable` |
| `GuiHero.get_Title` | kind in the title |
| `CustomHackingHelper.CheckHeroAssignableToFleet/ToSystem` | kind check |
| `Senator.UpdateRepresentativeHero` | senator kind check |
| `Hero.UpdateLevel(bool)` (start) | `HeroRework.BeforeUpdateLevel` (senator level cap) |
| `Senate.ParseElectionResult` (start) | `HeroRework.LockElection` |
| `Senate.ServerTriggerRebellion` (start) | `HeroRework.BlockRebellion` |
| `Hero.get_AvailableSkillPoints` (return) | `HeroRework.AdjustAvailablePoints` |
| `DepartmentOfEducation.SerializeElements` (start) | `HeroRework.EnsureDictatorDefinitions` |
| `CouncilGame.ClientBeginTurn` / `SerializeAttributes` | `HeroRework.PerTurn` / registry persistence |

## Not yet done
Nothing of the original list. Known limits: skills already learned in a save made before this build stay vanilla (they keep working, filtered by role) but are not part of the new trees; texts of generated skills are registered in the language active at start.

## Unverified assumptions
The refresh via `OnSkillsUpdated` on level change, acceptance of runtime descriptor copies by `SimulationObject`, `Hero.get_SkillTrees` being the only GUI path, the stage-threshold clamp, already-assigned mismatched heroes staying assigned without effect, the `FailureInfo` tooltip, and that `Hero.Level` is the quantity the user means by "level".
Stage 3: that the dictatorship tags are present on the senate object under every dictatorship government, that skipping `ServerTriggerRebellion` (also its per-system rebellions caused by the empire rebellion) is acceptable, that vanilla dictatorships may have several senators, that `LevelMax` on a cloned rule is honoured by `UpdateLevel`/GUI and that the starting hero level is 1, and the prime-senator-at-start choice of the leader.
Stage 4: that runtime-registered skills, `GuiElement`s and descriptors are accepted by the game (the registration was only exercised against a synthetic database in a plain Mono process, not in the game), that `GuiElement.Clone()` keeps the icons, that paths such as `../ClassEmpire/ClassGarrisonFleet` accept the fleet properties used for senators (`ShipDamage`, `FleetMaximumHealth`, ...), that the senate descriptors reach the senator of the dictator, that `HeroSkillTreeStage.RequiredLevel` is compared with hero-wide spent skill points (it is, in `ComputeAvailableSkills`), and that all magnitudes are balanced.
Follow-ups: that a client-posted `OrderRecruitHero` for a hidden runtime definition is accepted and processed by the game, that `Touch`/`Add` of the definitions before `SerializeElements` is early enough for a load, that the faction names resolve to a faction key, that removing a hero with `RemoveHero` on every client is consistent (no order exists for it), that the senators' representative refresh picks the new hero, and the pace of the passive experience (10 / 6 turns per level) - none of it has been tried in the game.
