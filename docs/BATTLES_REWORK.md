# Переделка боёв: экипажи, мораль, пополнение, шаблоны (этап 1)

Проект переделки космических и наземных сражений. Файл описывает **все** механики (включая те, что ещё не написаны),
и документирует то, что уже вшито в `Managed/Assembly-CSharp.dll`.

| Блок | Статус | Где |
|---|---|---|
| Наземный бой: резерв экипажа на кораблях | **реализован** (симуляция) | `BattleCore.ReserveFor` + `BattleRework.ApplyCrewReserve`, хук после `ServerGroundBattle.ComputeSpawnReport` |
| Наземный бой: штраф за недоукомплектованный экипаж | **реализован** | `BattleRework.RefreshCrewPenalties` (дескриптор `GalacticCouncilCrew` на корабль) |
| Мораль защитника + автокапитуляция при 0 | **реализован** (хук в бою) | `BattleCore.TickMorale`, `BattleRework.CheckSurrender`, хук в `ServerGroundBattle.IsFinished` |
| Пополнение manpower только из «связных» систем | **реализован** | `BattleRework.AllowedRefillFor`, хук в `Ship.RefillManpower` (1 место чтения) |
| Отладочная панель в God-Mod | **реализован** | `BattleDebug.DrawBattleTools` (вкладка «Дипломатия и империи») |
| Шаблоны кораблей: каталог, подвиды, описания, привязка проекта, кнопка «Автодизайн» | **реализован** (без отдельного окна выбора) | `TemplateCore` + `BattleTemplates`, хуки в `ShipDesignModalWindow.{OnAutoDesignCb,RefreshButtons}` |
| Шаблоны для ИИ (чтобы он брал корабли из подвида) | **решение: не трогаем** (у `Database<T>` нет публичного reorder) | §5.3 |
| Состав флотов в **нативном тултипе** строки флота | **реализован** (append в `FleetLine.FleetTooltip.Content`) | `BattleTemplates.RefreshFleetLineTooltip`, хук `FleetLine.Refresh`, §6 |
| Статистика в итогах боя («что в бою») | **реализован** (тултип цифры урона в отчёте) | `ReportCore` + `BattleReportStats`, хвост `AdvancedEncounterReportModalWindow.Refresh`, §7 |
| «Перемодернизация флота» | **реализован как инструмент God-Mod'а**; отдельный приказ — после проверки пути в игре | `BattleDebug.DrawRefit` + `BattleTemplates.RefitShip`, §8 |

Все числа — константы в `src/GalacticCouncil/BattleCore.cs`, они проверяются тестами `tests/BattleCoreTests.cs`
(62 проверки, все проходят). Чистые части второго этапа проверяются отдельно, без игры: `tests/TemplateCoreTests.cs`
(37 проверок, математика шаблонов) и `tests/ReportCoreTests.cs` (26 проверок, агрегация отчёта). Треть этапа —
это уже склейка с игрой: её можно подтвердить только запуском (план ниже).

---

## 1. Что уже делает ваниль (проверено по ассембли и по данным игры)

Это важно, потому что переделка сознательно **не дублирует** то, что уже считается игрой, а переписывает только нужные места.

**Manpower корабля.** `SimulationProperties/Ship`: `MaximumShipManpower`, `ShipManpowerStock`, `ShipManpowerRefillRate`,
`MaximumShipManpowerRefillRate`, `EmpireManpowerStock`, `MaximumEmpireManpowerStock`, `EmpireManpower`,
`BonusManpower`, `MaximumManpowerPerGroundBattleTurn`.
База `MaximumShipManpower` в `SimulationDescriptors[Battles/Ship]`:

```
<Property Name="MaximumShipManpower" BaseValue="0"/>
<BinaryModifier TargetProperty="MaximumShipManpower" Operation="Percent" Left="$(ShipStage)" Multiplication Right="0.5"/>
```

плюс надбавки класса корпуса (те же файлы, блок `ClassHulls`):

| Класс корпуса | надбавка |
|---|---|
| `ClassHullSmallCivilian` | +40 |
| `ClassHullSmallAttack`, `ClassHullSmallSupport` | +100 |
| `ClassHullMediumAttack`, `ClassHullMediumDefense` | +300 |
| `ClassHullLargeCarrier` | +900 |
| `ClassHullMothership` | +220 и ещё `100 × MothershipLevel` |
| `ClassHullCravers` (фракция) | +10 % |

Модули экипажа (`ModuleDefinitions[Support]`, `Family="CrewBase"`) дают `LocalManpower` → `MaximumShipManpower`:
Crew I = 100, II = 200, III = 300 (Terrans 450, Cravers 400), IV = 400 (Cravers 600, стратегические 500/300 с вампир-абсорбцией).
Система: `MaximumSystemManpower` = 300 (`+1000` на столицу), гарнизон сдаёт излишки в империю.
Пополнение: `Ship.MaximumShipManpowerRefillRate = 25%` от максимума, `Garrison` наследует её по флоту;
фактическую раздачу делает `DepartmentOfDefense.RefreshManpowerRefill()`, которая делит `EmpireManpowerStock + NetEmpireManpower`
поровну на всех «manpower-refillable» (все корабли всех флотов/ангаров + колонизованные системы + цитадели),
а `Ship.RefillManpower()` забирает недостающее у империи через `DepartmentOfTheTreasury.TryTransferResources`.
Время от времени `TransferExcessManpowerToEmpire` возвращает излишек корабля обратно в сток.

**Штурм.** `GroundBattleSpawnReport::Instantiate(attacker, attackerDisplayed, defender, total[], fleet[], population[], prevTotal[], prevCounts[][])`:
защитнику в `OpponentInitManPowers[1]` кладётся `SystemManpowerStock` + `CitadelManpowerStock`, а атакующему
`OpponentInitManPowers[0]` = сумма `ShipManpowerStock` **всего флота** (свойство класса `ClassGarrisonFleet`),
поверх чего стоят лимиты `GroundBattleAttackerManpowerLimitFromEmpire`
(база = `Empire.MaximumManpowerPerGroundBattleTurn` = **500**) и `…FromFleets`
(сюда вкладывается `Fleet.AdditionalGroundBattleManpowerLimit`, есть также `Fleet.CannotContributeManpower`).
После боя `GroundBattle.GiveBackManpower` возвращает десант: защитнику — в гарнизон системы, атакующему —
в `ShipManpowerStock` кораблей того же флота, ровно до `MaximumShipManpower`.

**Тактика «жители в ополчение».** `GroundBattlePopulationToManpowerTemporaryConversionRate` (на бой) и
`…PermanentConversionRate` (навсегда). В отчёте боя это `GroundBattleBattleReport.TemporaryConvertedPopulations`
/ `PermanentlyConvertedPopulations` — переделка читает именно их.

**Сдача.** `ServerGroundBattle.ComputeBattleReport` читает свойство системы `GroundBattleDefenderSurrender`
и пишет `GroundBattleBattleReport.DefenderSurrendered` (открытое поле). Дальше движок: атакующий не несёт потерь,
фаза «выбор исхода» (`GroundBattleOutcomeCapture/Loot/Raze/…Definition`) упрощается.

**Настроение.** `ColonizedStarSystem`: `Happiness` (0..100), `CurrentHappinessStatus`
(пороги vanilla-набора: ≤15 бунт, 15–30, 30–70, 70–85, ≥85), `HappinessMalusFromPoliticsSupport`,
`TurnsUntilDecayIsOver`, `GetOwnership()`, `lastInvasionTurnPerEmpireIndex`.

## 2. Резерв экипажа (реализовано)

Правило: **на корабле всегда остаётся резерв**, в штурм идёт только то, что сверх него.

```
hullReserve = 40 | 100 | 300 | 900 | 220 (+100 × уровень матершипа)     // по классу корпуса
reserve     = min( hullReserve, 0.75 × MaximumShipManpower, ShipManpowerStock )
assault     = max( 0, ShipManpowerStock − reserve )
```

`hullReserve` — «сколько людей нужно именно такому корпусу, чтобы он вообще функционировал»; класс корпуса
`HullDefinition.Size`, а если он пуст (в датате Size не заполнен) — подстрока Small/Medium/Large/Carrier/
Juggernaut/Mothership в имени корпуса, иначе 100. `CrewReserveRatio = 0.75` — потолок: корабль никогда не
оставляет на борту больше 75 % экипажа, иначе крупные корпуса (авианосец на 900) не могли бы никого высадить.

Тот самый пример из задания: малый атакующий с 400 manpower (корпус 100 + два модуля экипажа по 100 + стадия)
→ резерв 300, в штурм идут 100. Поднял лимит модулем/навыком героя до 600 — резерв всё ещё 300 (или 450, если
корпус стал крупнее: работает потолок 75 %), а всё, что сверху, — десант. Кончились десантные отряды —
наступать нечем, ровно как сейчас: просто потому, что `assault = 0`.

Куда это вшито: в конец `ServerGroundBattle.ComputeSpawnReport`, сразу после `set_SpawnReport`, вставлен
`call BattleRework::ApplyCrewReserve(ServerGroundBattle)`. Наш код берёт `battle.DefenderNode.DockedEntities`
(флоты атакующего, `Fleet.CannotContributeManpower` уважается), суммирует резервы и правит `GroundBattleSpawnReport`:

* `OpponentInitManPowers[0]`, `OpponentTotalManPowers[0]`, `OpponentManPowerLimits[0]` (индекс 0 = атакующий);
* `OpponentTroopTypeInitCounts[0][тип]` — числа отрядов **пересчитываются** с тем же коэффициентом, потому что
  ванильный `GroundBattleSpawnReport::Instantiate` нарезает их из `OpponentInitManPowers`
  (`count = floor(Manpower × Weight/TotalWeight / TroopTypeManpowerCost<Type>)`).

Подрез никогда не опускается ниже `Σ count × TroopTypeManpowerCost<Type>` (`BattleRework.MinimumNeeded`) —
то есть отряды, которые уже нарезаны, обязаны остаться обеспеченными, иначе бой стал бы нерешаемым.

Отдельная мелочь: недоукомплектованный корабль (после вычета резерва `stock < reserve`) получает дескриптор
типа `GalacticCouncilCrew` с `DamagePerSecond` и `MaximumMovement` × `1 − 0.4 × (1 − stock/reserve)`
(`RefreshCrewPenalties`, раз в ход, пересоздаётся только при изменении > 2 %; при `ratio = 1` снимается,
при гибели корабля дескриптор удаляется из датабазы). Это тот самый «дебафф за снижение экипажа», которого
в ваниле нет как класса (есть только `DamageBonusFromManpower`/`MaximumDamageBonusFromManpower = 0.2` —
бонус за полный экипаж).

## 3. Мораль защитника (реализовано)

```
base   = 20 + 0.8 × Happiness            (потолок 100 только для домашних систем: +15 и до 100)
battle = −6 за ход штурма        siege = −4 за ход блокады/осады        bombard = −10 за ход бомбардировки
militia(временная) = −8          militia(постоянная) = −25             regen = +5 за спокойный ход (+2 в оккупации)
```

Мораль нигде не падает ниже 0 и не поднимается выше 100; `IsCollapsed(morale) ⇔ morale ≤ 0` →
в бою включается `GroundBattleBattleReport.DefenderSurrendered` — хук `ServerGroundBattle.IsFinished →
BattleRework.CheckSurrender` оборачивает все `ret` этого метода, поэтому сдача срабатывает и при штурме,
и при бомбардировке (во втором случае — на следующем обращении движка к бою/системе).

Счётчики хода ведёт `BattleRework.PerTurn(game, game.Turn)`, вызываемый из
`CouncilGameMandates.BeforeTurnEnd` **до** проверки `Core.Enabled` (то есть работает и при выключенном Совете):

* «был бой» — системы, упомянутые в `IGroundBattleRepositoryService.GroundBattles` этого хода
  (`BattleSystemState.LastBattleTurn`), заодно оттуда же берутся `Temporary/PermanentlyConvertedPopulations`
  → флаги ополчения;
* «бомбардировка» — `SystemManpowerStock` системы упал по сравнению со снапшотом прошлого хода;
* «осада» — в `StarSystemNode.DockedEntities` стоит видимый чужой флот (`IsBesieged`);
* смена владельца по снапшоту → `OccupationMarkTurn = turn`, и мораль сразу режется потолком оккупации.

**Оккупация — это фича.** Ваниль даёт свежезахваченной системе штраф к настроению на несколько ходов,
настроение → 0, значит мораль = пол. `OccupationCap(0) = 12`, линейно до 100 за 6 ходов
(`OccupationMoraleTurns`), поэтому вернувшийся в течение пары ходов бывший владелец берёт систему
**без боя**. Никаких «костылей» вроде «в первые 2 хода мораль не считается» специально не делается.

Что пока не делается (следующий этап, см. §4): «мораль кончилась вне боя» → добровольная капитуляция
системы. Сейчас мораль вне боя только копится/падает; `DefenderSurrendered` включается, когда движок
считает бой. Причина: `SiegeStarSystemAction` — клиентский, а прямой вызов
`GroundBattle::Finish(Finished)` из мода мог бы оставить машину состояний серверного боя в полупроходе.

## 4. Пополнение экипажей только из «своих» систем (реализовано)

Правило из задания: сток manpower империи **не работает** для системы, не соединённой цепочкой своих
систем со столицей. Реализация — подмена источника пополнения в `Ship.RefillManpower()` (патчер проходит по `Ship` и `Garrison`
и чинит ровно одно место — `Ship`, у систем и цитаделей пополнение идёт через общий `RefreshManpowerRefill`):

```
ldarg.0; callvirt Empire::get_Empire; ldsfld Empire::EmpireManpowerStock; callvirt GetPropertyValue
→ ldarg.0; call BattleRework::AllowedRefillFor(Ship)
```

`AllowedRefillFor` возвращает `EmpireManpowerStock` только если система, где стоит корабль,
в корневой сети империи — `DepartmentOfTheInterior.IsNodeConnectedToRootNetwork(node)` (это ровно та
же проверка, по которой игра считает системы «изолированными»: цепочка `SpaceRoots` до `HomeSystemNode`
с учётом `RootingIsolatedRadius`). Корабль в ангаре/в переход — поведение ванильное.

Следствие из задания: враг, захвативший систему на другом конце галактики с потопленным флотом, **не** лечит
его экипажи из своего стока — лечится только локальный гарнизонный пул системы (`SystemManpowerStock`,
`MaximumSystemManpower = 300`), пока сеть не восстановится. То же распространяется на гарнизон/цитадель —
пока только на корабли; `Garrison`/`Citadel` пополняются общим `RefreshManpowerRefill`, который гейтится
в варианте «B» (см. §9, «что дальше»).

## 5. Шаблоны кораблей (реализовано ядро)

Хорошая новость: игра уже хранит шаблоны как дата-таблицу — `ShipDesignTemplateDefinition`:

```
<ShipDesignTemplateDefinition Name="..." Hull="HullSmall01Sophons" ShipRole="ShipRoleSmallAttacker">
  <TemplateSlotBinding SlotName="CoreSection.Weapon01">
      <TemplateModule Family="LaserBase" Weight="4"/>
      <TemplateModule Family="BeamBase"  Weight="3"/>
  </TemplateSlotBinding> ...
```

`ShipDesign.FillShipDesignFromTemplate()` выбирает шаблон по паре (Hull, Role) и на каждый слот
ставит лучший **доступный по технологиям** модуль семейства с весом `Weight × Level`
(`TryGetBestAvailableModuleOfFamily`) — то есть «автоматически улучшается с новыми открытиями» уже ванильная
механика. Её вызывает одна-единственная кнопка: `ShipDesignModalWindow.OnAutoDesignCb →
ShipDesignEditionPanel.AutoDesign → ShipDesign.AutoDesign()` (`UpgradeModulesAlreadyInShipDesign` +
`FillShipDesignFromPattern` + `FillShipDesignFromTemplate`), с описанием `%ShipDesignAutoDesignDescription`.

План мода — не изобретать новый формат, а **развернуть одну пару (Hull, Role) в каталог шаблонов по оружию**:

* Родители — 4 базовых семейства оружия T1: `KineticBase` (короткая дистанция, пробивает энергощиты,
  флак 42/0.25 по истребителям), `MissileBase` (дальний, 180 урона, плох вблизи), `LaserBase`
  (средний, 32, хорош против снарядной брони), `BeamBase` (все дистанции, 80, режет и кинетику, и лазеры,
  но боится ракет). Плюс «стратегические» ветки `*Strategic1/3/5` и `*Strategic2/4/6` — это и есть
  естественные подвиды: кинетика/ракеты 1-3-5 (снарядная линия) и лазер/луч 2-4-6 (энергетическая линия).
* Для каждой пары «Hull × ShipRole × семейство» генерируется шаблон с именем вида
  `HullSmall01Sophons_SmallAttacker_LaserBase`, с подвидами по дистанционному профилю:

  | роль | KineticBase | MissileBase | LaserBase | BeamBase |
  |---|---|---|---|---|
  | SmallAttacker | «Штурмовик» (S, флак) | «Заградительный огонь» (L, 0 вблизи) | «Универсал» (M) | «Снайпер» (все S/M/L, больно крупным) |
  | MediumAttacker | «Таран» | «Осадный» | «Линейный» | «Линкор» |
  | Carrier/Squadron | «Прикрытие» | «Носитель» | — | — |

  Дистанционный профиль берётся прямо из `<AIBattleSituations Situation="Short|Medium|Long" Mode="Use">`
  модулей шаблона, тип брони — из `Mode="Against" EnergyDefense|ProjectileDefense`:
  **название и описание выводятся из фактических свойств модулей**, как и просили, а не пишутся руками.
* Описание — 2 строки, генерируется из тех же данных (пример формулировки в задании: «Корпус — Атакующий.
  Роль — снайпер. Держится в дальнем ряду, наносит много энергетического урона крупным целям»); хранится
  как `GuiElement`/локализация, добавленная рантаймом (`Databases.GetDatabase<GuiElement>().Add(...)`
  и `LocalizationDatatableElement` — тот же путь, которым `HeroContent`/`HeroDictatorHero` добавляют свои тексты).
* ИИ: существующий `AutoDesign` ничего не теряет — модинг добавляет шаблоны в ту же БД, а вариант для ИИ
  выбирается по тем же весам `AIBattleSituations` против состава противника (в §9 — отдельным шагом).
* Игрок: выбор шаблона = запуск `ShipDesign` созданного из шаблона; ограничение отсутствует, править можно всё.

Технически регистрация — тот же проверенный путь, что у «материнского корабля» героя
(`HeroRework.RegisterMothershipDefinition`): `db.Add(new ShipDesignDefinition{...})` +
`DepartmentOfDefense.ServerAddShipDesign(name)` + `slot.BindModule(...)`. Для шаблонов вместо ручного
лэйаута достаточно `ShipDesign.AutoDesign()`, потому что шаблон уже в БД.

### 5.1 Как это устроено сейчас

`TemplateCore.cs` (чистая математика, тесты `tests/TemplateCoreTests.cs`, 37 проверок) +
`BattleTemplates.cs` (работа с живыми базами игры):

1. **Каталог** строится из самих же датат — ничего не захардкожено:
   `Databases.GetDatabase<ModuleDefinition>()` даёт все `WeaponModule`, у которых есть
   `AIBattleSituations` (это отфильтровывает истребители/AOE); `Databases.GetDatabase<ShipDesignTemplateDefinition>()`
   даёт 85 ванильных шаблонов (по одному на корпус), и для каждого мы разбираем его `TemplateSlotBindings`:
   какие семейства оружия вообще могут встать в слоты этого корпуса. Итого: **корпус × роль × семейство оружия**.
2. **Подвид (вариант)** выводится из свойств этих модулей (`TemplateCore.Classify`):
   * `HasFlakSlave` (у модуля есть `SlaveModule` «Flack…») → «Заградительный огонь» (кинетыка T1 — она);
   * иначе по AI-профилю дистанций `Use/Short|Medium|Long`: только `Long` ≥ 0.75 → «Снайпер» (ракеты);
     только `Short` → «Штурмовик»; всё остальное → «Универсал» (лазер 0.5/1/0.5 и луч 1/1/1 — оба они).
   Дamage type и броня берутся из `Against/EnergyDefense` и `Against/ProjectileDefense`; DPS и штрафы
   дистанций — из дескрипторов модуля (`DamagePerSecond`, `Short/LongRangePenalty`) тем же обходом, что у
   `HeroContent.Accumulate`.
3. **Описание** собирает `TemplateCore.Describe`: «Корпус — малый атакующий. Роль — снайпер: держится в дальнем
   ряду, вблизи беззащитен. N урона в секунду (по энергетической броне). Дистанции 0 / 0.5 / 1 (S/M/L). …»
   — то есть имя и текст следуют за данными модулей, как и требовалось, а не за ролью.
4. **Регистрация**: `RegisterVariantTemplates()` (вызывается из `BattleRework.PerTurn`) добавляет в ту же
   `ShipDesignTemplateDefinition` запись на вариант — копия родительского шаблона, у которого в слотах `*Weapon*`
   осталось только семейство варианта. Значит ванильный `FillShipDesignFromTemplate` (это и `AutoDesign()` игрока,
   и генерация ИИ) **автоматически** получает «улучшение по мере открытия технологий»: он и так берёт
   `Weight × ModuleDefinition.Level` лучшего доступного модуля семейства.
5. **Привязка проекта**: `BattleCore.HullVariantChoice[hull]` — выбранный вариант корпуса; `BattleCore.Designs`
   — привязка конкретного `ShipDesign` (для тултипов и «перемодернизации»). Всё это уезжает в 5-й сегмент
   `BR1|`-строки (`BattleState`), старые сейвы грузятся без него.
6. **Кнопка «Автодизайн»** (реальная кнопка оригинального окна проекта корабля):
   * `IL` в начале `ShipDesignModalWindow.OnAutoDesignCb` → `BattleTemplates.AutoDesignCb(window)`: если для
     корпуса выбран вариант — заполняем пустые слоты оружием этого семейства (`FillFromVariant`, повторяющий
     правило ванили: только пустые слоты, `CanModuleBeBound`, проверка доступности технологий) и помечаем проект;
     дальше ванильное тело делает своё `ShipDesign.AutoDesign()`.
   * `IL` в начале `ShipDesignModalWindow.RefreshButtons` → `BattleTemplates.SyncDesignWindow(window)`: подпись
     кнопки становится «Автодизайн: Снайпер», а её тултип — коротким описанием варианта. Без выбранного варианта
     подпись возвращается к оригинальной.
   * Выбор варианта пока делается в God-Mod (блок «Шаблоны кораблей»: список проектов империи, варианты корпуса,
     кнопки + описание) — отдельное нативное окно выбора это следующий шаг.

### 5.2 Что с приоритетом варианта (решение)

У `Database<T>` нет публичной вставки в начало, а `FillShipDesignFromTemplate` (путь ИИ) берёт первое совпадение,
то есть ванильный родительский шаблон. Переопределить это можно только перестановкой в самом списке базы
(`Remove` + `Add`) — на объектах, на которые уже ссылаются сохранённые проекты. Риск (порванная ссылка шаблона у
живого проекта) несоразмерен выгоде, поэтому **решение: ИИ остаётся на ванильных шаблонах**, игрок получает
вариант своей кнопкой «Автодизайн», а его выбор переживает перезагрузку (привязка в `BattleRework.Core.Designs`).

### 5.3 Итог по §5

Реализовано: каталог, подвиды, описания, регистрация в БД, кнопка «Автодизайн» как точка входа, подпись/тултип
кнопки, привязка проекта в сейве,HUD-строка состава (§6) и рефит (§8). Не делается сознательно: приоритет для ИИ
(§5.2) и отдельное нативное окно выбора — выбор уже показан на кнопке, дубль окна только запутывает.

## 6–8. UI: план, что сделано и что сознательно не делается

* **§6 Тултип на глобальной карте.** Точка входа найдена: `GameOverlayTooltipPanel` (public, `GuiWindow`) — панель
  наведения на карте; у неё есть `ContentTable`/`Label`/`PanelFeaturesTable` и `OnParentShow/OnParentHide`, т.е.
  достаточно своего `AgePrimitiveLabel` внутри `ContentTable` (текст даёт `BattleTemplates.SummarizeFleet(fleet, includeOwn)`).
* **§8.0** Для «перемодернизации» в игре уже есть готовый слой: `EntityActionDefinition` (public, виртуальные
  `CanBeExecuted/BuildEntityAction/GetAvailableContext/GetEntityActionDefinition(name)`, `Costs`, `Prerequisites`,
  `EntityFilters`) и живой образец `SpecializeJuggernautRetrofitShipActionDefinition : SpecializeJuggernautShipActionDefinition`
  (у него есть `ShipDesignDefinitionName` и `GetShipDesign(Ship)`) + `ShipsManagementPanel.RetrofitTooltipData`
  (`GetFinalCostsString(ICostResolutionContext)`, `GetFullTitle(AgePrimitiveLabel, bool)`) — то есть и подсказка о
  стоимости, и место, куда она выводится. Наш приказ = `EntityActionDefinition` из БД (тот же путь, что у
  `OrderCouncilVote`/`OrderAcademyHeroKind`), `GetShipDesign` которого возвращает проект выбранного варианта.
* **§6 Состав флотов — только нативный тултип.** Точка, найденная по скриншоту: блок «Обороняющиеся флоты /
  1-й НАСТОЙЧИВЫЕ флоты / 4 кораблей…» на карте — это `FleetLine`, у которого есть **собственный `FleetTooltip`
  (AgeTooltip)**, заполняемый в его же `Refresh` (`GuiGarrison` → контент). Хук `hook: FleetLine.Refresh ends with
  BattleTemplates.RefreshFleetLineTooltip (returns: 2)` дописывает под всей этой информацией блок
  «═══ Шаблоны ═══»: по строке «N× <вариант> [<семейство>]» + короткое описание варианта. Тот же `FleetLine`
  используется и в панелях системы, так что один хук покрывает все места, где перечислен флот.
  **Свой label в HUD убран** (первая версия показывала «Fleet#1041; 1 x ShipDesign#416» — сырые внутренние имена
  и отдельный виджет; по требованию игрока никаких отдельных подписей не будет, только тултипы/панели игры,
  а внутренние имена ещё и фильтруются: `BattleTemplates.DesignName` не отдаёт строки вида «ShipDesign#…»).
  В God-Mod'е тот же текст доступен как кнопка «предпросмотр тултипов».
* **§7 Статистика в итогах боя.** Всё для этого есть в `Encounter.Report`: инструкции
  `EncounterReportInstruction_Hit { WeaponEncounterGUID, TargetEncounterGUID, TheoreticalDamages }`
  (у урона есть и разбивка по типам: `DamagePerSecondKinetic|Missile|Laser|Beam` на секции/модуле).
  План: пройтись по `EncounterReport.Instructions`, aggredировать по (ShipDesign → тип, модуль), и вывести
  4 строки («больше всех урона нанёс X», «чаще всего применялся модуль Y», «самый полезный — Z»,
  «больше всего получили типа U») в `AdvancedEncounterReportModalWindow` рядом с `PlayerTotalDamageLabel` —
  то есть шрифтом, иконками и стилями оригинального интерфейса, как и просили.
  *Реализовано иначе и проще:* инструкции отчёта (`EncounterReportInstruction_Hit`) хранят GUID'ы и требуют
  `IGalaxyEncounterEntityRepository`, который после боя уже мог умереть; вместо них берутся готовые счётчики
  энкаунтера, а текст дописывается в тултип `PlayerTotalDamageTooltip` (см. заметку «фаза 2» в конце файла).
* **§8 Перемодернизация флота.** Новая сущность в меню приказов = `EntityActionDefinition` + `EntityAction`
  (`SiegeStarSystemActionDefinition` — готовый образец: `CanBeExecuted(fleet, system, ...)`,
  `PrerequisitesOnFleet`, `GetContenders`). Стоимость — `Cost ResourceName="EmpireMoney"`,
  базово `RefitDustBase = 50 × игровой этап × число кораблей`; `CheckContender` запрещает действие, если система
  не своя/не союзная (очевидно, кнопка активна только в дружественных системах). **Сейчас этого не сделано:**
  путь рефита сначала проверяется в игре через God-Mod (`BattleDebug.DrawRefit`), и только потом стоит заводить
  приказ с `Costs`/`Prerequisites` (стоимость в `Dust` — это новый баланс, который надо на чём-то проверять).
  Применение — существующий `DepartmentOfDefense.RetrofitShipsProcessor` (он уже пересобирает корабли
  под новый `ShipDesign` и перечитывает `MaximumShipManpower`), а список целей — выбор шаблона из реестра §5.

## 9. Что дальше (порядок)

1. Капитуляция без боя при 0 морали вне сражения (нужен серверный путь: создать `GroundBattle` и сразу
   `OnAttackerVictory`, либо свой `GroundBattleOutcomeDefinition`) — требует проверки на живой игре.
2. **Гейт пополнения гарнизона/цитадели — отложен сознательно.** Единственное место, где это можно вставить, —
   приватный `DepartmentOfDefense.RefreshManpowerRefill()` (279 инструкций, перебирает `HangarsByGameEntityGUID`
   и пишет `LocalManpowerStock`), и вызывается он из `GameClientState_Turn_{Begin,End}_RefreshManpowerRefill`
   (корутины). Одиночная IL-правка в такое тело без живой игры — гарантированный риск сломать пополнение у всех;
   вместо этого эффект 0 морали уже есть: автокапитуляция в бою (`CheckSurrender`) и потеря контроля над системой.
   Если всё же нужно — правильный путь: свой `PostTurn`-проход, который для «разорванных» систем откатывает
   `ColonizedStarSystem.SetLocalManpowerStock(GetLocalManpowerStock() − дельта)` с капом в 25%/ход.
3. ~~Реестр шаблонов §5~~ — сделано ядро (каталог, подвиды, описания, регистрация, кнопка «Автодизайн», привязка в сейв).
   **Решение по приоритету для ИИ: оставляем как есть.** Вариант — это копия `ShipDesignTemplateDefinition`,
   добавленная в `Database<T>.Add` (то есть в хвост списка), и перестановка элементов базы — единственный способ
   заставить `FillShipDesignFromTemplate` prefer вариант; у базы нет публичного reorder, а `Remove`+`Add` на
   уже сохранённых объектах может порвать ссылки проекта на шаблон. Риск несоразмерен выгоде: ИИ остаётся на
   ванильных шаблонах, игрок получает вариант своей кнопкой «Автодизайн». Проверять в игре.
   Отдельное нативное окно выбора варианта тоже отложено: выбор уже виден на кнопке `AutoDesignButton`
   (`SyncDesignWindow`) и в тултипе, а дублировать окно — лишний UI.
4. ~~Привязка «ShipDesign → шаблон»~~ — есть (`BattleRework.Core.Designs` + `BattleTemplates.Match`), ею пользуются
   тултип оверлея (HUD-строка `RefreshHudOverlay`), окно проекта и перемодернизация.
5. ~~Блок статистики §7~~ — `ReportCore` (26 проверок) + `BattleReportStats` (хук `AdvancedEncounterReportModalWindow.Refresh`).
   Осталось: проверить в игре, что `SimulationObjectWrapper.GetPropertyValue` отдаёт эти свойства после боя
   (свойства есть в `SimulationProperties.EncounterShip/EncounterModule_Weapon`), и что клона label'а панели
   достаточно для второй копии текста.
6. **Перемодернизация §8** — сделана как инструмент God-Mod'а (`BattleDebug.DrawRefit` →
   `BattleTemplates.RefitShip` = `SetChoice(design, variant, rememberForHull:false)` +
   `DepartmentOfDefense.ApplyShipDesignToShip`, fallback `Ship.RetrofitShipDesign`). Порядок: проект принадлежит
   империи, поэтому рефит одного корабля трогает все корабли на этом проекте — строка это и показывает
   («кораблей на этом проекте: N»). Автоматического «лучшего против врага» выбора нет намеренно.
   Отдельный приказ из `EntityActionDefinition` — только после того, как путь рефита подтверждён в игре
   (нужны ли доки, стоимость, `CannotBeModified`).
7. **Ховер вражеского флота на карте** — публичной точки расширения нет (`GameOverlayTooltipPanel` занимается только
   failure infos), поэтому своя строка в HUD показывает состав наших флотов; вражеская колонка требует хука
   `Update`/`OnMouseEnter` оверлея и намеренно не делалась.

## Проверка в игре (что смотреть после сборки)

1. **Экипажи/мораль.** Наземный бой за систему в 2 хода: в отчёте у атакующего должно быть видно, что пехоты
   пришло меньше стокa (резерв `floor(min(hull, 0.75×max, stock))`), а после победного хода мораль защитника
   падает на 6 и восстанавливается на 5 (в оккупированной системе — только на 2). При 0 морали — сдача без боя
   на следующем бою в этой системе.
2. **Панель.** Вкладка «Дипломатия и империи» в God-Mod: блок «ПЕРЕДЕЛКА БОЁВ» (состояние, резерв по флотам),
   «Шаблоны» (каталог, выбор варианта для корпуса) и «ПЕРЕВОРУЖЕНИЕ ФЛОТОВ» (строки рефита + `→ вариант`).
3. **Шаблоны.** Открыть корабельный конструктор → «Автодизайн»: кнопка должна подписаться «Автодизайн: <вариант>»,
   после нажатия пустые оружейные слоты заполнены модулями этого семейства; тултип кнопки — описание варианта.
4. **Отчёт.** После космического боя открыть «Подробный отчёт»: в тултипе цифры урона — блок «Что в бою»
   (частый модуль, полезный корабль, тип брони противника, чем ответить) и построчно, из какого шаблона корабли.
   Если блок пустой — смотреть `Player.log` на предмет `[BattleReportStats]` (свойства энкаунтера могли не
   читаться после уничтожения сцены: тогда источник меняется на снимок, снятый в `Encounter_StateChanged`).
5. **HUD.** Строка состава флотов внизу слева (`GCCTemplateOverlay`) появляется при показе оверлея и в конце хода;
   если её нет — в логе нет ошибок, просто у оверлея не нашёлся донор-label (это не критично).

## Сборка и проверка

```bash
tools/setup.sh                      # Mono+Roslyn в /tmp (один раз)
tools/mkref.sh                      # /tmp/ref/Assembly-CSharp.dll (publicized) для компиляции мода
bash tools/build_council.sh         # компиляция src/GalacticCouncil (ловит все ошибки API)
bash tools/build_patch.sh           # пересборка Managed/Assembly-CSharp.dll (+хуки)
bash tools/verify.sh 5              # Mono-JIT каждого метода: "jitted ok=6503 fail=0"
# тесты математики:
source tools/env.sh && mkdir -p /tmp/t && csc -codepage:65001 -nostdlib -langversion:6 \
  -out:/tmp/t/BattleCoreTests.exe -r:$MANAGED/mscorlib.dll -r:$MANAGED/System.dll \
  tests/BattleCoreTests.cs src/GalacticCouncil/BattleCore.cs && mono /tmp/t/BattleCoreTests.exe
```

Отключить переделку на время отладки: `PATCH_SKIP=battles bash tools/build_patch.sh` (в игре — галка
в God-Mod, блок «ПЕРЕДЕЛКА БОЁВ»: там же видно резерв каждого флота, мораль всех систем,
`LastCrewCut` последнего отчёта рассадки, кнопки «выключить переделку» и «сбросить мораль»).

* Пополнение гарнизона в «разорванных» системах не гейтится (см. §9.2): гейтится только имперский сток, из которого
  корабли берут экипажи (`Ship.RefillManpower` → `BattleRework.AllowedRefillFor`).
* Пополнение manpower в отчёте боя не учитывает мораль — мораль влияет на сдачу, а не на экономику хода.

## Известные ограничения

* Все хуки переделки используют только токены, уже присутствующие в зацепленных методах, — сборка не получает
  новых self-ссылок (`CheckSelfReferences: before=33 after=33`), переходы целы (`branch check: bad=0`).
* Состояние морали живёт в атрибуте `BattleState` рядом с `CouncilState` Совета (`CouncilGame.SerializeBattles`),
  поэтому переделка работает и в партиях без Совета. Старые сейвы грузятся с пустой моралью (она считается заново
  из настроения).
* Числа (`−6 / −4 / −10 / −8 / −25 / +5 / 0.75 / 500`) — балансировочные, их стоит прогнать на живой игре;
  меняются в одном месте — `BattleCore`.
> **Второй этап (готово, в игре не проверено):** в `AdvancedEncounterReportModalWindow.Refresh` к тултипу
> собственной цифры урона отчёта дописывается блок «Что в бою»: самый частый ваш модуль, самый полезный корабль,
> по какой броне вас били, чем ответить (тексты — `ReportCore`, только арифметика/строки; 26 проверок
> `tests/ReportCoreTests.cs`). Числа берутся из статистики энкаунтера (`SimulationProperties.EncounterShip/
> EncounterModule_Weapon`: `DamageApplied/DamageReceived/TotalShotSent/TotalHitSent`) через
> `SimulationObjectWrapper.GetPropertyValue` — nothing is re-derived from the report instruction stream, the
> game already records all of it; `GalaxyEncounterEntity*` (live objects of the battle scene) are not used on
> purpose: the report is shown after the battle scene is gone. Строка «шаблон» — из `BattleTemplates.Match`
> (то есть отчёт также показывает, из какого шаблона собран вражеский флот).
> **Состав — в нативном тултипе строки флота** (см. §6 выше): отдельный label не создаётся вообще.
> В отчёте, кроме строк `ReportCore`, дописываются «наши шаблоны:» и «шаблоны противника:»
> (`BattleTemplates.FleetSummaryOf(EncounterGroup)`) — тем же тултипом, тем же виджетом игры.
> Если после космического боя в логе есть `[BattleReportStats]` — значит свойства энкаунтера не читаются
> после уничтожения боевой сцены; план «Б»: снимать снимок статистики в `Encounter_StateChanged` и показывать
> из него. Если окна подробного отчёта вообще нет (после наземного боя) — это уже не тултип: нужен фрагмент
> Player.log вокруг `AdvancedEncounterReportModalWindow`, пока правки отчёта целиком обёрнуты в try/catch и
> не могут «съесть» окно.
