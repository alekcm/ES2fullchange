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
| Шаблоны кораблей (каталог, подвиды, описания, выбор игрока, ИИ) | **спроектирован, не реализован** | §5 |
| Тултип с описаниями шаблонов при наведении на флот | спроектирован | §6 |
| Статистика в итогах боя | спроектирован | §7 |
| «Перемодернизация флота» в меню приказов | спроектирован | §8 |

Все числа — константы в `src/GalacticCouncil/BattleCore.cs`, они проверяются тестами `tests/BattleCoreTests.cs`
(47 проверок, все проходят).

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

## 5. Шаблоны кораблей (спроектировано)

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

## 6–8. UI-планы (не реализованы)

* **§6 Тултип на глобальной карте.** На наведении на флот: слева — состав и шаблоны *своего* выбранного флота,
  справа — шаблоны чужого. Точка входа — `GuiFleet`/`BattleGroupInfoPanel` (`AdvancedEncounterReportModalWindow`
  уже показывает такие панели в отчёте) или `GuiTooltipManager`: клонировать существующий `GuiTooltip` и
  заполнять текстом из реестра шаблонов (`BattleCore.Designs[ShipDesign GUID]`). Привязка «корабль → шаблон»
  решается реестром `BattleCore.Designs` (уже сериализуется), который заполняется в момент применения шаблона.
* **§7 Статистика в итогах боя.** Всё для этого есть в `Encounter.Report`: инструкции
  `EncounterReportInstruction_Hit { WeaponEncounterGUID, TargetEncounterGUID, TheoreticalDamages }`
  (у урона есть и разбивка по типам: `DamagePerSecondKinetic|Missile|Laser|Beam` на секции/модуле).
  План: пройтись по `EncounterReport.Instructions`, aggredировать по (ShipDesign → тип, модуль), и вывести
  4 строки («больше всех урона нанёс X», «чаще всего применялся модуль Y», «самый полезный — Z»,
  «больше всего получили типа U») в `AdvancedEncounterReportModalWindow` рядом с `PlayerTotalDamageLabel` —
  то есть шрифтом, иконками и стилями оригинального интерфейса, как и просили.
* **§8 Перемодернизация флота.** Новая сущность в меню приказов = `EntityActionDefinition` + `EntityAction`
  (`SiegeStarSystemActionDefinition` — готовый образец: `CanBeExecuted(fleet, system, ...)`,
  `PrerequisitesOnFleet`, `GetContenders`). Стоимость — `Cost ResourceName="EmpireMoney"`,
  базово `RefitDustBase = 50 × игровой этап × число кораблей`; `CheckContender` запрещает действие, если система
  не своя/не союзная (очевидно, кнопка активна только в дружественных системах).
  Применение — существующий `DepartmentOfDefense.RetrofitShipsProcessor` (он уже пересобирает корабли
  под новый `ShipDesign` и перечитывает `MaximumShipManpower`), а список целей — выбор шаблона из реестра §5.

## 9. Что дальше (порядок)

1. Капитуляция без боя при 0 морали вне сражения (нужен серверный путь: создать `GroundBattle` и сразу
   `OnAttackerVictory`, либо свой `GroundBattleOutcomeDefinition`) — требует проверки на живой игре.
2. Гейт пополнения гарнизона/цитадели (`RefreshManpowerRefill`) — вариант «B»: локальный пул системы,
   если сеть не связана; сейчас гейтится только имперский сток для кораблей.
3. Реестр шаблонов §5 (генерация + описания + подвиды), затем UI выбора из кнопки «Автодизайн».
4. Привязка «ShipDesign → шаблон» для тултипов §6 и перемодернизации §8.
5. Блок статистики §7.

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

## Известные ограничения

* Все хуки переделки используют только токены, уже присутствующие в зацепленных методах, — сборка не получает
  новых self-ссылок (`CheckSelfReferences: before=33 after=33`), переходы целы (`branch check: bad=0`).
* Состояние морали живёт в атрибуте `BattleState` рядом с `CouncilState` Совета (`CouncilGame.SerializeBattles`),
  поэтому переделка работает и в партиях без Совета. Старые сейвы грузятся с пустой моралью (она считается заново
  из настроения).
* Числа (`−6 / −4 / −10 / −8 / −25 / +5 / 0.75 / 500`) — балансировочные, их стоит прогнать на живой игре;
  меняются в одном месте — `BattleCore`.
