# Как устроен взлом в Endless Space 2 (разбор кода из `Managed/`)

Исходников игры в репозитории нет — есть только скомпилированные сборки.
Весь разбор ниже получен чтением IL-кода `Assembly-CSharp.dll` и `Assembly-CSharp-firstpass.dll`
(метаданные + дизассемблер на базе `dnfile`/`dncil`). Имена методов и свойств приведены точно так,
как они лежат в сборке, — по ним можно искать в dnSpy/ILSpy.

## 1. Из чего состоит операция взлома

```
HackingOperation                      ← сама операция (не MonoBehaviour, обычный игровой объект)
├── GUID                : GameEntityGUID
├── EmpireIndex         : int
├── Start               : IHackingOperationStart     (колония, из которой идёт взлом)
├── Target              : IHackingOperationTarget    (система / узел / материнский корабль)
├── TargetSystem        : ColonizedStarSystem
├── Steps               : HackingOperationStep[]     (чередование Move- и Node-шагов)
├── CurrentStepIndex    : int
├── SelectedOutcomeDefinitionName : StaticString     (выбранный исход)
├── SimulationObject    : SimulationObject           (симуляционные свойства операции)
└── IsFinalized         : bool
```

Шаги бывают двух видов:

* `MoveHackingOperationStep` — перемещение по линии между узлами;
* `NodeHackingOperationStep` — «прогрызание» самого узла (в том числе целевой системы).

У каждого шага есть `Stock` (накопленный прогресс) и два ключевых метода:

```csharp
float GetSpeedMultiplier(Empire empire, HackingOperation operation)
float GetRequiredProgress(Empire empire, HackingOperation operation)
```

## 2. Где считается прогресс за ход

`HackingOperation.ClientEndTurn()` — главный цикл (пересказ IL):

```csharp
Empire empire = Start.Empire;
float net = empire.GetPropertyValue(NetProgressProperty);   // очки взлома империи за ход
while (currentStepIndex < Steps.Length)
{
    HackingOperationStep step = Steps[currentStepIndex];
    float speed = step.GetSpeedMultiplier(empire, this);
    if (speed <= 0f) break;                                 // ← операция стоит на месте
    float required = step.GetRequiredProgress(empire, null);
    if (step.Stock + net * speed >= required) { ...шаг закончен, currentStepIndex++... }
    else { step.Stock += net * speed; net = 0f; }
}
if (currentStepIndex == Steps.Length) { ...операция завершена, выбирается исход... }
```

Отсюда два важных факта, на которых стоит весь мод:

1. **Множитель скорости <= 0 — это штатная ситуация.** Игра просто не набирает прогресс
   (`break`), ничего не ломается. `NodeHackingOperationStep.GetRemainingTurns` в этом случае
   возвращает `int.MaxValue`, то есть интерфейс показывает «никогда».
2. **Операция завершается ровно в тот момент, когда пройден последний шаг.** Если последний шаг
   не заканчивается, операция живёт вечно и остаётся подключённой к системе.

Сам множитель для узла берётся из симуляционного свойства операции:

```csharp
// NodeHackingOperationStep.GetSpeedMultiplier
if (operation != null)
    return operation.SimulationObject.GetPropertyValue(SimulationProperties.HackingOperation.HackingNodeSpeed);
```

(есть ещё `HackingWarpSpeed` и `HackingNonWarpSpeed` для шагов движения, и набор множителей
`GetSpeedMultiplier_FromColony/FromBackdoors/FromGhosts/FromTimeBubbles/FromPrograms`).

## 3. Исходы (то, что игрок выбирает в ванили)

Исходы — это data-driven объекты `HackingOperationOutcome*Definition` (XML-дататейблы), у каждого:

```csharp
bool IsValid(Empire empire, IHackingOperationTarget target)
void ServerApplyEffects(HackingOperation operation, PlayerController controller, StaticString parameter)
void ClientApplyEffects(HackingOperation operation, StaticString parameter)
```

Полезные примеры (из них и взяты «правильные» способы делать эффекты):

| Класс исхода | Что делает | Как именно |
|---|---|---|
| `HackingOperationOutcomeTransferResourcesDefinition` | кража/передача ресурсов | `DepartmentOfTheTreasury.TryTransferResources(simulationObject, resourceName, value, true)` |
| `HackingOperationOutcomeSpawnTraitorsDefinition` | внедрение спящих | `controller.PostOrder(new OrderSpawnTraitorPopulation(empireIndex, system.GUID))` |
| `HackingOperationOutcomeMinorRelationTemporaryEffectDefinition` | отношения с малой фракцией | временный симуляционный дескриптор на отношении |

Для отношений есть и прямой приказ: `OrderSetMinorRelationPoints(empireIndex, minorEmpireIndex, points)`,
который обрабатывает `DepartmentOfForeignAffairs.SetMinorRelationPointsProcessor`
(текущее значение читается через `minorEmpire.GetAgency<DepartmentOfForeignAffairs>().GetMinorRelationPoints(majorEmpire)`).

## 4. Нужные симуляционные свойства и ресурсы

| Что нужно | Имя |
|---|---|
| наука системы за ход | `SimulationProperties.StarSystem.NetSystemResearch` |
| пыль системы за ход | `SimulationProperties.StarSystem.NetSystemMoney` |
| влияние системы за ход | `SimulationProperties.StarSystem.NetSystemEmpirePoint` |
| ресурс «наука» империи | `DepartmentOfTheTreasury.Resources.EmpireResearch` |
| ресурс «пыль» империи | `DepartmentOfTheTreasury.Resources.EmpireMoney` |
| ресурс «влияние» империи | `DepartmentOfTheTreasury.Resources.EmpireEmpirePoint` |
| скорость взлома узла | `SimulationProperties.HackingOperation.HackingNodeSpeed` |

Значение берётся через `SimulationObjectWrapper.GetPropertyValue(StaticString)`
(у `ColonizedStarSystem` и `Empire` он есть, оба — наследники `SimulationObjectWrapper`).

## 5. Почему мод сделан плагином, а не XML-модом или правкой DLL

* **XML-мод не тянет.** В XML можно описать новый исход, но нельзя сделать эффект, который капает
  каждый ход, пока операция висит, и нельзя обнулить скорость взлома в зависимости от выбора игрока.
* **Правка `Assembly-CSharp.dll`** работает, но файл слетает на каждом обновлении игры,
  ломает проверку целостности файлов Steam и требует бэкапа.
* **BepInEx + Harmony** патчит те же самые методы, но в памяти: файлы игры не трогаются,
  мод снимается удалением одного файла.

## 6. Точки, за которые цепляется мод

| Метод | Тип патча | Что делает |
|---|---|---|
| `NodeHackingOperationStep.GetSpeedMultiplier` | postfix | `0` для пассивных режимов (когда операция подключена к цели) |
| `MoveHackingOperationStep.GetSpeedMultiplier` | postfix | то же для шагов движения (нужно для режима `FreezeImmediately`) |
| `HackingManager.ClientBeginTurn` | postfix | применить эффекты режимов (раз в ход, защёлка по номеру хода) |
| `HackingManager.ClientEndTurn` | postfix | запасная точка той же защёлки |
| `HackingManager.StartHackingOperation` | postfix | показать окно выбора режима сразу после запуска |
| `HackingOperation.ClientBeginTurn` | prefix | не дать применить ванильный исход к «припаркованной» операции |

«Подключён к системе» в терминах кода — это `CurrentStepIndex >= Steps.Length - 1`,
то есть текущий шаг — узел целевой системы: линия доведена до системы, но внутрь взлом ещё не вошёл.

## 7. Инструменты разбора

В каталоге `tools/` лежат скрипты на Python, которыми делался разбор (нужен `pip install dnfile dncil`):

```bash
python3 tools/ildump.py types Hacking              # список типов по маске
python3 tools/ildump.py type HackingOperation      # поля и методы с сигнатурами
python3 tools/ildump.py il  HackingOperation ClientEndTurn   # дизассемблер IL
ASM=Managed/Assembly-CSharp-firstpass.dll python3 tools/ildump.py type Amplitude.Unity.Game.Empire
```
