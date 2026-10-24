# Деревья навыков, созданные модом (партии и диктаторы)

Сгенерировано `tools/Dump/HeroTreesDump.cs` из кода мода (`HeroSkillData.cs`: `PlanTree`, таблицы флейворов). Значения - ровно то, что регистрирует игра.

Устройство любого такого дерева: 7 навыков, 4 этапа. Этап 0: A, B. Этап 1: C, A+B. Этап 2: B+C, A+C. Этап 3 (ультимейт): A+B+C (у диктатора +счастье и людские резервы). A, B, C - три флейвора темы. Размер: этап x (1.0 / 1.5 / 2.0 / 2.5), один флейвор x1, два x0.7 каждый, три x0.55, пять x0.45. Адмирал/губернатор: 2 уровня (0.5, 1.0 от размера), сенатор: 1 уровень (1.0), диктатор: 3 уровня (0.5, 1.0, 1.5), ультимейт диктатора x1.5; сила диктатора x1.6.

Флейвор -> эффект (таблицы `ShipTargets` / `SystemTargets` / `SenateTargets`): значение = опорное число x магнитуда x сила. Флейвор `Votes` в сенате не имеет модификатора: голоса читает Совет.

Порог этапа (очки навыков): A/G 0/4/8/12; сенатор 0/2/4/6; диктатор 0/6/12/18.

## Опорные эффекты флейворов (магнитуда 1, сила 1)

| Флейвор | Адмирал (флот) | Губернатор (система) | Сенатор (вся империя) |
|---|---|---|---|
| Offense (Военные ассигнования) | ShipDamage +0.2 @GarrisonFleet | GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem | ShipDamage +0.1 @Empire/GarrisonFleet |
| Defense (Бронирование флотов) | FleetMaximumHealth +0.2 @GarrisonFleet | MaximumSystemManpower +40% | FleetMaximumHealth +0.1 @Empire/GarrisonFleet |
| Mobility (Имперская навигация) | ShipMaximumMovement +2 @GarrisonFleet | TradingRoutesBonus_Production +40% @ColonizedStarSystem | ShipMaximumMovement +1 @Empire/GarrisonFleet |
| Sustain (Службы ремонта) | HealthRegenerationRatio +0.2 @GarrisonFleet | MaximumSystemManpower +100 | HealthRegenerationRatio +0.05 @Empire/GarrisonFleet |
| Upkeep (Режим экономии) | GarrisonMoneyUpkeep -20% @GarrisonFleet | SystemMoneyUpkeep -20% @ColonizedStarSystem | SystemMoneyUpkeep -10% @Empire/ColonizedStarSystem |
| Production (Государственная индустрия) | ShipHullPlatingAbsorptionPercent +0.2 @GarrisonFleet | SystemProduction +10 @ColonizedStarSystem | PlanetRawIndustry +1 @Empire/ColonizedStarSystem/ColonizedPlanet |
| Growth (Аграрная реформа) | FleetManpowerRefillRateModifier +0.4 @GarrisonFleet | SystemGrowth +20 @ColonizedStarSystem | PlanetRawFood +1 @Empire/ColonizedStarSystem/ColonizedPlanet |
| Science (Научный комитет) | VisionRange +2 @GarrisonFleet | SystemResearch +40 @ColonizedStarSystem | PlanetRawScience +1 @Empire/ColonizedStarSystem/ColonizedPlanet |
| Money (Казначейство) | GarrisonMoneyUpkeep -30% @GarrisonFleet | SystemMoney +10 @ColonizedStarSystem | PlanetRawDust +1 @Empire/ColonizedStarSystem/ColonizedPlanet |
| Influence (Имперская идеология) | SiegePower +20% @GarrisonFleet | SystemEmpirePoint +20 @ColonizedStarSystem | PlanetRawPrestige +1 @Empire/ColonizedStarSystem/ColonizedPlanet |
| Happiness (Всеобщее благополучие) | FleetMaximumShield +0.25 @GarrisonFleet | Happiness +10 @ColonizedStarSystem | Happiness +5 @Empire/ColonizedStarSystem |
| Manpower (Всеобщая мобилизация) | MaximumShipManpower +25% @GarrisonFleet/Ship | NetSystemManpower +10 @ColonizedStarSystem | MaximumEmpireManpowerStock +25% @Empire |
| Deposit (Ресурсная политика) | Evasion +0.1 @GarrisonFleet | DepositValue +1 @ColonizedStarSystem/Planet/ResourceDeposit | DepositValue +0.5 @Empire/ColonizedStarSystem/Planet/ResourceDeposit |
| Experience (Образовательная реформа) | ExperiencePerTurn +8 @GarrisonFleet | SystemResearch +20% @ColonizedStarSystem | SystemResearch +10% @Empire/ColonizedStarSystem |
| Diplomacy (Дипломатический корпус) | ShipShieldPenetrationFactor +0.4 @GarrisonFleet | TradingRoutesBonus_Growth +40% @ColonizedStarSystem | PeacefulTreatyCostReduction -0.5 @Empire<br>AggressiveTreatyCostReduction -0.5 @Empire |
| Votes (Голоса в Совете) | SiegePower +20% @GarrisonFleet | SystemEmpirePoint +20 @ColonizedStarSystem | - (голоса Совета) |

## Партии (6 x 4 роли = 24 дерева)

### Партия 1: Индустриалисты / Industrialists - тема: Production, Money, Manpower

##### `HRTreeParty01@A` - роль Адмирал, канал Ship, мощность x1
- **Порог 0 очк.**
  - **`HRParty01S0@A`** «Бронепластины» / Plating  [Production]
    - ур.1 (магнитуда 0.5): ShipHullPlatingAbsorptionPercent +0.1 @GarrisonFleet  [Production]
    - ур.2 (магнитуда 1): ShipHullPlatingAbsorptionPercent +0.2 @GarrisonFleet  [Production]
  - **`HRParty01S1@A`** «Бережливое снабжение» / Frugal Supply  [Money]
    - ур.1 (магнитуда 0.5): GarrisonMoneyUpkeep -15% @GarrisonFleet  [Money]
    - ур.2 (магнитуда 1): GarrisonMoneyUpkeep -30% @GarrisonFleet  [Money]
- **Порог 4 очк.**
  - **`HRParty01S2@A`** «Набор экипажей» / Crew Recruitment  [Manpower]
    - ур.1 (магнитуда 0.75): MaximumShipManpower +18.8% @GarrisonFleet/Ship  [Manpower]
    - ур.2 (магнитуда 1.5): MaximumShipManpower +37.5% @GarrisonFleet/Ship  [Manpower]
  - **`HRParty01S3@A`** «Бронепластины / Бережливое снабжение» / Plating / Frugal Supply  [Production+Money]
    - ур.1 (магнитуда 0.525/0.525): ShipHullPlatingAbsorptionPercent +0.105 @GarrisonFleet  [Production]; GarrisonMoneyUpkeep -15.8% @GarrisonFleet  [Money]
    - ур.2 (магнитуда 1.05/1.05): ShipHullPlatingAbsorptionPercent +0.21 @GarrisonFleet  [Production]; GarrisonMoneyUpkeep -31.5% @GarrisonFleet  [Money]
- **Порог 8 очк.**
  - **`HRParty01S4@A`** «Бережливое снабжение / Набор экипажей» / Frugal Supply / Crew Recruitment  [Money+Manpower]
    - ур.1 (магнитуда 0.7/0.7): GarrisonMoneyUpkeep -21% @GarrisonFleet  [Money]; MaximumShipManpower +17.5% @GarrisonFleet/Ship  [Manpower]
    - ур.2 (магнитуда 1.4/1.4): GarrisonMoneyUpkeep -42% @GarrisonFleet  [Money]; MaximumShipManpower +35% @GarrisonFleet/Ship  [Manpower]
  - **`HRParty01S5@A`** «Бронепластины / Набор экипажей» / Plating / Crew Recruitment  [Production+Manpower]
    - ур.1 (магнитуда 0.7/0.7): ShipHullPlatingAbsorptionPercent +0.14 @GarrisonFleet  [Production]; MaximumShipManpower +17.5% @GarrisonFleet/Ship  [Manpower]
    - ур.2 (магнитуда 1.4/1.4): ShipHullPlatingAbsorptionPercent +0.28 @GarrisonFleet  [Production]; MaximumShipManpower +35% @GarrisonFleet/Ship  [Manpower]
- **Порог 12 очк.**
  - **`HRParty01S6@A`** «Вершина: Бронепластины» / Pinnacle: Plating  [Production+Money+Manpower]
    - ур.1 (магнитуда 1.375/1.375/1.375): ShipHullPlatingAbsorptionPercent +0.275 @GarrisonFleet  [Production]; GarrisonMoneyUpkeep -41.3% @GarrisonFleet  [Money]; MaximumShipManpower +34.4% @GarrisonFleet/Ship  [Manpower]

##### `HRTreeParty01@G` - роль Губернатор, канал System, мощность x1
- **Порог 0 очк.**
  - **`HRParty01S0@G`** «Индустриальный рост» / Industrial Drive  [Production]
    - ур.1 (магнитуда 0.5): SystemProduction +5 @ColonizedStarSystem  [Production]
    - ур.2 (магнитуда 1): SystemProduction +10 @ColonizedStarSystem  [Production]
  - **`HRParty01S1@G`** «Торговля и налоги» / Commerce and Taxes  [Money]
    - ур.1 (магнитуда 0.5): SystemMoney +5 @ColonizedStarSystem  [Money]
    - ур.2 (магнитуда 1): SystemMoney +10 @ColonizedStarSystem  [Money]
- **Порог 4 очк.**
  - **`HRParty01S2@G`** «Мобилизация» / Mobilization  [Manpower]
    - ур.1 (магнитуда 0.75): NetSystemManpower +8 @ColonizedStarSystem  [Manpower]
    - ур.2 (магнитуда 1.5): NetSystemManpower +15 @ColonizedStarSystem  [Manpower]
  - **`HRParty01S3@G`** «Индустриальный рост / Торговля и налоги» / Industrial Drive / Commerce and Taxes  [Production+Money]
    - ур.1 (магнитуда 0.525/0.525): SystemProduction +5 @ColonizedStarSystem  [Production]; SystemMoney +5 @ColonizedStarSystem  [Money]
    - ур.2 (магнитуда 1.05/1.05): SystemProduction +10 @ColonizedStarSystem  [Production]; SystemMoney +10 @ColonizedStarSystem  [Money]
- **Порог 8 очк.**
  - **`HRParty01S4@G`** «Торговля и налоги / Мобилизация» / Commerce and Taxes / Mobilization  [Money+Manpower]
    - ур.1 (магнитуда 0.7/0.7): SystemMoney +7 @ColonizedStarSystem  [Money]; NetSystemManpower +7 @ColonizedStarSystem  [Manpower]
    - ур.2 (магнитуда 1.4/1.4): SystemMoney +14 @ColonizedStarSystem  [Money]; NetSystemManpower +14 @ColonizedStarSystem  [Manpower]
  - **`HRParty01S5@G`** «Индустриальный рост / Мобилизация» / Industrial Drive / Mobilization  [Production+Manpower]
    - ур.1 (магнитуда 0.7/0.7): SystemProduction +7 @ColonizedStarSystem  [Production]; NetSystemManpower +7 @ColonizedStarSystem  [Manpower]
    - ур.2 (магнитуда 1.4/1.4): SystemProduction +14 @ColonizedStarSystem  [Production]; NetSystemManpower +14 @ColonizedStarSystem  [Manpower]
- **Порог 12 очк.**
  - **`HRParty01S6@G`** «Вершина: Индустриальный рост» / Pinnacle: Industrial Drive  [Production+Money+Manpower]
    - ур.1 (магнитуда 1.375/1.375/1.375): SystemProduction +14 @ColonizedStarSystem  [Production]; SystemMoney +14 @ColonizedStarSystem  [Money]; NetSystemManpower +14 @ColonizedStarSystem  [Manpower]

##### `HRTreeParty01@S` - роль Сенатор, канал Senate, мощность x1
- **Порог 0 очк.**
  - **`HRParty01S0@S`** «Государственная индустрия» / State Industry  [Production]
    - ур.1 (магнитуда 1): PlanetRawIndustry +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
  - **`HRParty01S1@S`** «Казначейство» / Treasury  [Money]
    - ур.1 (магнитуда 1): PlanetRawDust +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
- **Порог 2 очк.**
  - **`HRParty01S2@S`** «Всеобщая мобилизация» / National Mobilization  [Manpower]
    - ур.1 (магнитуда 1.5): MaximumEmpireManpowerStock +37.5% @Empire  [Manpower]
  - **`HRParty01S3@S`** «Государственная индустрия / Казначейство» / State Industry / Treasury  [Production+Money]
    - ур.1 (магнитуда 1.05/1.05): PlanetRawIndustry +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawDust +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
- **Порог 4 очк.**
  - **`HRParty01S4@S`** «Казначейство / Всеобщая мобилизация» / Treasury / National Mobilization  [Money+Manpower]
    - ур.1 (магнитуда 1.4/1.4): PlanetRawDust +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; MaximumEmpireManpowerStock +35% @Empire  [Manpower]
  - **`HRParty01S5@S`** «Государственная индустрия / Всеобщая мобилизация» / State Industry / National Mobilization  [Production+Manpower]
    - ур.1 (магнитуда 1.4/1.4): PlanetRawIndustry +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; MaximumEmpireManpowerStock +35% @Empire  [Manpower]
- **Порог 6 очк.**
  - **`HRParty01S6@S`** «Вершина: Государственная индустрия» / Pinnacle: State Industry  [Production+Money+Manpower]
    - ур.1 (магнитуда 1.375/1.375/1.375): PlanetRawIndustry +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawDust +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; MaximumEmpireManpowerStock +34.4% @Empire  [Manpower]

##### `HRTreeParty01@D` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRParty01S0@D`** «Государственная индустрия» / State Industry  [Production]
    - ур.1 (магнитуда 0.8): PlanetRawIndustry +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - ур.2 (магнитуда 1.6): PlanetRawIndustry +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - ур.3 (магнитуда 2.4): PlanetRawIndustry +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
  - **`HRParty01S1@D`** «Казначейство» / Treasury  [Money]
    - ур.1 (магнитуда 0.8): PlanetRawDust +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.2 (магнитуда 1.6): PlanetRawDust +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.3 (магнитуда 2.4): PlanetRawDust +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
- **Порог 6 очк.**
  - **`HRParty01S2@D`** «Всеобщая мобилизация» / National Mobilization  [Manpower]
    - ур.1 (магнитуда 1.2): MaximumEmpireManpowerStock +30% @Empire  [Manpower]
    - ур.2 (магнитуда 2.4): MaximumEmpireManpowerStock +60% @Empire  [Manpower]
    - ур.3 (магнитуда 3.6): MaximumEmpireManpowerStock +90% @Empire  [Manpower]
  - **`HRParty01S3@D`** «Государственная индустрия / Казначейство» / State Industry / Treasury  [Production+Money]
    - ур.1 (магнитуда 0.84/0.84): PlanetRawIndustry +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawDust +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.2 (магнитуда 1.68/1.68): PlanetRawIndustry +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawDust +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.3 (магнитуда 2.52/2.52): PlanetRawIndustry +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawDust +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
- **Порог 12 очк.**
  - **`HRParty01S4@D`** «Казначейство / Всеобщая мобилизация» / Treasury / National Mobilization  [Money+Manpower]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawDust +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; MaximumEmpireManpowerStock +28% @Empire  [Manpower]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawDust +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; MaximumEmpireManpowerStock +56% @Empire  [Manpower]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawDust +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; MaximumEmpireManpowerStock +84% @Empire  [Manpower]
  - **`HRParty01S5@D`** «Государственная индустрия / Всеобщая мобилизация» / State Industry / National Mobilization  [Production+Manpower]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawIndustry +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; MaximumEmpireManpowerStock +28% @Empire  [Manpower]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawIndustry +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; MaximumEmpireManpowerStock +56% @Empire  [Manpower]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawIndustry +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; MaximumEmpireManpowerStock +84% @Empire  [Manpower]
- **Порог 18 очк.**
  - **`HRParty01S6@D`** «Вершина: Государственная индустрия» / Pinnacle: State Industry  [Production+Money+Manpower]
    - ур.1 (магнитуда 3.3/3.3/3.3): PlanetRawIndustry +3.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawDust +3.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; MaximumEmpireManpowerStock +82.5% @Empire  [Manpower]

### Партия 2: Учёные / Scientists - тема: Science, Experience, Influence

##### `HRTreeParty02@A` - роль Адмирал, канал Ship, мощность x1
- **Порог 0 очк.**
  - **`HRParty02S0@A`** «Дальняя разведка» / Long-Range Scouting  [Science]
    - ур.1 (магнитуда 0.5): VisionRange +1 @GarrisonFleet  [Science]
    - ур.2 (магнитуда 1): VisionRange +2 @GarrisonFleet  [Science]
  - **`HRParty02S1@A`** «Боевой опыт» / Combat Experience  [Experience]
    - ур.1 (магнитуда 0.5): ExperiencePerTurn +4 @GarrisonFleet  [Experience]
    - ур.2 (магнитуда 1): ExperiencePerTurn +8 @GarrisonFleet  [Experience]
- **Порог 4 очк.**
  - **`HRParty02S2@A`** «Осадное мастерство» / Siegecraft  [Influence]
    - ур.1 (магнитуда 0.75): SiegePower +15% @GarrisonFleet  [Influence]
    - ур.2 (магнитуда 1.5): SiegePower +30% @GarrisonFleet  [Influence]
  - **`HRParty02S3@A`** «Дальняя разведка / Боевой опыт» / Long-Range Scouting / Combat Experience  [Science+Experience]
    - ур.1 (магнитуда 0.525/0.525): VisionRange +1 @GarrisonFleet  [Science]; ExperiencePerTurn +4.2 @GarrisonFleet  [Experience]
    - ур.2 (магнитуда 1.05/1.05): VisionRange +2.1 @GarrisonFleet  [Science]; ExperiencePerTurn +8 @GarrisonFleet  [Experience]
- **Порог 8 очк.**
  - **`HRParty02S4@A`** «Боевой опыт / Осадное мастерство» / Combat Experience / Siegecraft  [Experience+Influence]
    - ур.1 (магнитуда 0.7/0.7): SiegePower +14% @GarrisonFleet  [Influence]; ExperiencePerTurn +6 @GarrisonFleet  [Experience]
    - ур.2 (магнитуда 1.4/1.4): SiegePower +28% @GarrisonFleet  [Influence]; ExperiencePerTurn +11 @GarrisonFleet  [Experience]
  - **`HRParty02S5@A`** «Дальняя разведка / Осадное мастерство» / Long-Range Scouting / Siegecraft  [Science+Influence]
    - ур.1 (магнитуда 0.7/0.7): VisionRange +1.4 @GarrisonFleet  [Science]; SiegePower +14% @GarrisonFleet  [Influence]
    - ур.2 (магнитуда 1.4/1.4): VisionRange +2.8 @GarrisonFleet  [Science]; SiegePower +28% @GarrisonFleet  [Influence]
- **Порог 12 очк.**
  - **`HRParty02S6@A`** «Вершина: Дальняя разведка» / Pinnacle: Long-Range Scouting  [Science+Experience+Influence]
    - ур.1 (магнитуда 1.375/1.375/1.375): VisionRange +2.8 @GarrisonFleet  [Science]; SiegePower +27.5% @GarrisonFleet  [Influence]; ExperiencePerTurn +11 @GarrisonFleet  [Experience]

##### `HRTreeParty02@G` - роль Губернатор, канал System, мощность x1
- **Порог 0 очк.**
  - **`HRParty02S0@G`** «Научные центры» / Research Centers  [Science]
    - ур.1 (магнитуда 0.5): SystemResearch +20 @ColonizedStarSystem  [Science]
    - ур.2 (магнитуда 1): SystemResearch +40 @ColonizedStarSystem  [Science]
  - **`HRParty02S1@G`** «Академические программы» / Academic Programs  [Experience]
    - ур.1 (магнитуда 0.5): SystemResearch +10% @ColonizedStarSystem  [Experience]
    - ур.2 (магнитуда 1): SystemResearch +20% @ColonizedStarSystem  [Experience]
- **Порог 4 очк.**
  - **`HRParty02S2@G`** «Престиж колоний» / Colonial Prestige  [Influence]
    - ур.1 (магнитуда 0.75): SystemEmpirePoint +15 @ColonizedStarSystem  [Influence]
    - ур.2 (магнитуда 1.5): SystemEmpirePoint +30 @ColonizedStarSystem  [Influence]
  - **`HRParty02S3@G`** «Научные центры / Академические программы» / Research Centers / Academic Programs  [Science+Experience]
    - ур.1 (магнитуда 0.525/0.525): SystemResearch +21 @ColonizedStarSystem  [Science]; SystemResearch +10.5% @ColonizedStarSystem  [Experience]
    - ур.2 (магнитуда 1.05/1.05): SystemResearch +42 @ColonizedStarSystem  [Science]; SystemResearch +21% @ColonizedStarSystem  [Experience]
- **Порог 8 очк.**
  - **`HRParty02S4@G`** «Академические программы / Престиж колоний» / Academic Programs / Colonial Prestige  [Experience+Influence]
    - ур.1 (магнитуда 0.7/0.7): SystemEmpirePoint +14 @ColonizedStarSystem  [Influence]; SystemResearch +14% @ColonizedStarSystem  [Experience]
    - ур.2 (магнитуда 1.4/1.4): SystemEmpirePoint +28 @ColonizedStarSystem  [Influence]; SystemResearch +28% @ColonizedStarSystem  [Experience]
  - **`HRParty02S5@G`** «Научные центры / Престиж колоний» / Research Centers / Colonial Prestige  [Science+Influence]
    - ур.1 (магнитуда 0.7/0.7): SystemResearch +28 @ColonizedStarSystem  [Science]; SystemEmpirePoint +14 @ColonizedStarSystem  [Influence]
    - ур.2 (магнитуда 1.4/1.4): SystemResearch +56 @ColonizedStarSystem  [Science]; SystemEmpirePoint +28 @ColonizedStarSystem  [Influence]
- **Порог 12 очк.**
  - **`HRParty02S6@G`** «Вершина: Научные центры» / Pinnacle: Research Centers  [Science+Experience+Influence]
    - ур.1 (магнитуда 1.375/1.375/1.375): SystemResearch +55 @ColonizedStarSystem  [Science]; SystemEmpirePoint +28 @ColonizedStarSystem  [Influence]; SystemResearch +27.5% @ColonizedStarSystem  [Experience]

##### `HRTreeParty02@S` - роль Сенатор, канал Senate, мощность x1
- **Порог 0 очк.**
  - **`HRParty02S0@S`** «Научный комитет» / Science Committee  [Science]
    - ур.1 (магнитуда 1): PlanetRawScience +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
  - **`HRParty02S1@S`** «Образовательная реформа» / Education Reform  [Experience]
    - ур.1 (магнитуда 1): SystemResearch +10% @Empire/ColonizedStarSystem  [Experience]
- **Порог 2 очк.**
  - **`HRParty02S2@S`** «Имперская идеология» / Imperial Ideology  [Influence]
    - ур.1 (магнитуда 1.5): PlanetRawPrestige +1.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
  - **`HRParty02S3@S`** «Научный комитет / Образовательная реформа» / Science Committee / Education Reform  [Science+Experience]
    - ур.1 (магнитуда 1.05/1.05): PlanetRawScience +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; SystemResearch +10.5% @Empire/ColonizedStarSystem  [Experience]
- **Порог 4 очк.**
  - **`HRParty02S4@S`** «Образовательная реформа / Имперская идеология» / Education Reform / Imperial Ideology  [Experience+Influence]
    - ур.1 (магнитуда 1.4/1.4): PlanetRawPrestige +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; SystemResearch +14% @Empire/ColonizedStarSystem  [Experience]
  - **`HRParty02S5@S`** «Научный комитет / Имперская идеология» / Science Committee / Imperial Ideology  [Science+Influence]
    - ур.1 (магнитуда 1.4/1.4): PlanetRawScience +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Порог 6 очк.**
  - **`HRParty02S6@S`** «Вершина: Научный комитет» / Pinnacle: Science Committee  [Science+Experience+Influence]
    - ур.1 (магнитуда 1.375/1.375/1.375): PlanetRawScience +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; SystemResearch +13.8% @Empire/ColonizedStarSystem  [Experience]

##### `HRTreeParty02@D` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRParty02S0@D`** «Научный комитет» / Science Committee  [Science]
    - ур.1 (магнитуда 0.8): PlanetRawScience +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.2 (магнитуда 1.6): PlanetRawScience +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.3 (магнитуда 2.4): PlanetRawScience +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
  - **`HRParty02S1@D`** «Образовательная реформа» / Education Reform  [Experience]
    - ур.1 (магнитуда 0.8): SystemResearch +8% @Empire/ColonizedStarSystem  [Experience]
    - ур.2 (магнитуда 1.6): SystemResearch +16% @Empire/ColonizedStarSystem  [Experience]
    - ур.3 (магнитуда 2.4): SystemResearch +24% @Empire/ColonizedStarSystem  [Experience]
- **Порог 6 очк.**
  - **`HRParty02S2@D`** «Имперская идеология» / Imperial Ideology  [Influence]
    - ур.1 (магнитуда 1.2): PlanetRawPrestige +1.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 2.4): PlanetRawPrestige +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 3.6): PlanetRawPrestige +3.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
  - **`HRParty02S3@D`** «Научный комитет / Образовательная реформа» / Science Committee / Education Reform  [Science+Experience]
    - ур.1 (магнитуда 0.84/0.84): PlanetRawScience +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; SystemResearch +8.4% @Empire/ColonizedStarSystem  [Experience]
    - ур.2 (магнитуда 1.68/1.68): PlanetRawScience +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; SystemResearch +16.8% @Empire/ColonizedStarSystem  [Experience]
    - ур.3 (магнитуда 2.52/2.52): PlanetRawScience +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; SystemResearch +25.2% @Empire/ColonizedStarSystem  [Experience]
- **Порог 12 очк.**
  - **`HRParty02S4@D`** «Образовательная реформа / Имперская идеология» / Education Reform / Imperial Ideology  [Experience+Influence]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawPrestige +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; SystemResearch +11.2% @Empire/ColonizedStarSystem  [Experience]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawPrestige +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; SystemResearch +22.4% @Empire/ColonizedStarSystem  [Experience]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawPrestige +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; SystemResearch +33.6% @Empire/ColonizedStarSystem  [Experience]
  - **`HRParty02S5@D`** «Научный комитет / Имперская идеология» / Science Committee / Imperial Ideology  [Science+Influence]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawScience +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawScience +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawScience +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Порог 18 очк.**
  - **`HRParty02S6@D`** «Вершина: Научный комитет» / Pinnacle: Science Committee  [Science+Experience+Influence]
    - ур.1 (магнитуда 3.3/3.3/3.3): PlanetRawScience +3.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +3.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; SystemResearch +33% @Empire/ColonizedStarSystem  [Experience]

### Партия 3: Пацифисты / Pacifists - тема: Happiness, Votes, Growth

##### `HRTreeParty03@A` - роль Адмирал, канал Ship, мощность x1
- **Порог 0 очк.**
  - **`HRParty03S0@A`** «Усиленные щиты» / Reinforced Shields  [Happiness]
    - ур.1 (магнитуда 0.5): FleetMaximumShield +0.125 @GarrisonFleet  [Happiness]
    - ур.2 (магнитуда 1): FleetMaximumShield +0.25 @GarrisonFleet  [Happiness]
  - **`HRParty03S1@A`** «Влияние в Совете» / Council Clout  [Votes]
    - ур.1 (магнитуда 0.5): SiegePower +10% @GarrisonFleet  [Votes]
    - ур.2 (магнитуда 1): SiegePower +20% @GarrisonFleet  [Votes]
- **Порог 4 очк.**
  - **`HRParty03S2@A`** «Резервные экипажи» / Reserve Crews  [Growth]
    - ур.1 (магнитуда 0.75): FleetManpowerRefillRateModifier +0.3 @GarrisonFleet  [Growth]
    - ур.2 (магнитуда 1.5): FleetManpowerRefillRateModifier +0.6 @GarrisonFleet  [Growth]
  - **`HRParty03S3@A`** «Усиленные щиты / Влияние в Совете» / Reinforced Shields / Council Clout  [Happiness+Votes]
    - ур.1 (магнитуда 0.525/0.525): FleetMaximumShield +0.131 @GarrisonFleet  [Happiness]; SiegePower +10.5% @GarrisonFleet  [Votes]
    - ур.2 (магнитуда 1.05/1.05): FleetMaximumShield +0.262 @GarrisonFleet  [Happiness]; SiegePower +21% @GarrisonFleet  [Votes]
- **Порог 8 очк.**
  - **`HRParty03S4@A`** «Влияние в Совете / Резервные экипажи» / Council Clout / Reserve Crews  [Votes+Growth]
    - ур.1 (магнитуда 0.7/0.7): FleetManpowerRefillRateModifier +0.28 @GarrisonFleet  [Growth]; SiegePower +14% @GarrisonFleet  [Votes]
    - ур.2 (магнитуда 1.4/1.4): FleetManpowerRefillRateModifier +0.56 @GarrisonFleet  [Growth]; SiegePower +28% @GarrisonFleet  [Votes]
  - **`HRParty03S5@A`** «Усиленные щиты / Резервные экипажи» / Reinforced Shields / Reserve Crews  [Happiness+Growth]
    - ур.1 (магнитуда 0.7/0.7): FleetManpowerRefillRateModifier +0.28 @GarrisonFleet  [Growth]; FleetMaximumShield +0.175 @GarrisonFleet  [Happiness]
    - ур.2 (магнитуда 1.4/1.4): FleetManpowerRefillRateModifier +0.56 @GarrisonFleet  [Growth]; FleetMaximumShield +0.35 @GarrisonFleet  [Happiness]
- **Порог 12 очк.**
  - **`HRParty03S6@A`** «Вершина: Усиленные щиты» / Pinnacle: Reinforced Shields  [Happiness+Votes+Growth]
    - ур.1 (магнитуда 1.375/1.375/1.375): FleetManpowerRefillRateModifier +0.55 @GarrisonFleet  [Growth]; FleetMaximumShield +0.344 @GarrisonFleet  [Happiness]; SiegePower +27.5% @GarrisonFleet  [Votes]

##### `HRTreeParty03@G` - роль Губернатор, канал System, мощность x1
- **Порог 0 очк.**
  - **`HRParty03S0@G`** «Общественный порядок» / Public Order  [Happiness]
    - ур.1 (магнитуда 0.5): Happiness +5 @ColonizedStarSystem  [Happiness]
    - ур.2 (магнитуда 1): Happiness +10 @ColonizedStarSystem  [Happiness]
  - **`HRParty03S1@G`** «Голос системы» / System Voice  [Votes]
    - ур.1 (магнитуда 0.5): SystemEmpirePoint +10 @ColonizedStarSystem  [Votes]
    - ур.2 (магнитуда 1): SystemEmpirePoint +20 @ColonizedStarSystem  [Votes]
- **Порог 4 очк.**
  - **`HRParty03S2@G`** «Агрокультура» / Agriculture  [Growth]
    - ур.1 (магнитуда 0.75): SystemGrowth +15 @ColonizedStarSystem  [Growth]
    - ур.2 (магнитуда 1.5): SystemGrowth +30 @ColonizedStarSystem  [Growth]
  - **`HRParty03S3@G`** «Общественный порядок / Голос системы» / Public Order / System Voice  [Happiness+Votes]
    - ур.1 (магнитуда 0.525/0.525): Happiness +5 @ColonizedStarSystem  [Happiness]; SystemEmpirePoint +10 @ColonizedStarSystem  [Votes]
    - ур.2 (магнитуда 1.05/1.05): Happiness +10 @ColonizedStarSystem  [Happiness]; SystemEmpirePoint +21 @ColonizedStarSystem  [Votes]
- **Порог 8 очк.**
  - **`HRParty03S4@G`** «Голос системы / Агрокультура» / System Voice / Agriculture  [Votes+Growth]
    - ур.1 (магнитуда 0.7/0.7): SystemGrowth +14 @ColonizedStarSystem  [Growth]; SystemEmpirePoint +14 @ColonizedStarSystem  [Votes]
    - ур.2 (магнитуда 1.4/1.4): SystemGrowth +28 @ColonizedStarSystem  [Growth]; SystemEmpirePoint +28 @ColonizedStarSystem  [Votes]
  - **`HRParty03S5@G`** «Общественный порядок / Агрокультура» / Public Order / Agriculture  [Happiness+Growth]
    - ур.1 (магнитуда 0.7/0.7): SystemGrowth +14 @ColonizedStarSystem  [Growth]; Happiness +7 @ColonizedStarSystem  [Happiness]
    - ур.2 (магнитуда 1.4/1.4): SystemGrowth +28 @ColonizedStarSystem  [Growth]; Happiness +14 @ColonizedStarSystem  [Happiness]
- **Порог 12 очк.**
  - **`HRParty03S6@G`** «Вершина: Общественный порядок» / Pinnacle: Public Order  [Happiness+Votes+Growth]
    - ур.1 (магнитуда 1.375/1.375/1.375): SystemGrowth +28 @ColonizedStarSystem  [Growth]; Happiness +14 @ColonizedStarSystem  [Happiness]; SystemEmpirePoint +28 @ColonizedStarSystem  [Votes]

##### `HRTreeParty03@S` - роль Сенатор, канал Senate, мощность x1
- **Порог 0 очк.**
  - **`HRParty03S0@S`** «Всеобщее благополучие» / General Welfare  [Happiness]
    - ур.1 (магнитуда 1): Happiness +5 @Empire/ColonizedStarSystem  [Happiness]
  - **`HRParty03S1@S`** «Голоса в Совете» / Council Votes  [Votes]
    - ур.1 (магнитуда 1): (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +6 (+1 за каждые 4 системы)
- **Порог 2 очк.**
  - **`HRParty03S2@S`** «Аграрная реформа» / Agrarian Reform  [Growth]
    - ур.1 (магнитуда 1.5): PlanetRawFood +1.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
  - **`HRParty03S3@S`** «Всеобщее благополучие / Голоса в Совете» / General Welfare / Council Votes  [Happiness+Votes]
    - ур.1 (магнитуда 1.05/1.05): Happiness +5 @Empire/ColonizedStarSystem  [Happiness]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +6 (+1 за каждые 4 системы)
- **Порог 4 очк.**
  - **`HRParty03S4@S`** «Голоса в Совете / Аграрная реформа» / Council Votes / Agrarian Reform  [Votes+Growth]
    - ур.1 (магнитуда 1.4/1.4): PlanetRawFood +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +8 (+1.4 за каждые 4 системы)
  - **`HRParty03S5@S`** «Всеобщее благополучие / Аграрная реформа» / General Welfare / Agrarian Reform  [Happiness+Growth]
    - ур.1 (магнитуда 1.4/1.4): PlanetRawFood +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; Happiness +7 @Empire/ColonizedStarSystem  [Happiness]
- **Порог 6 очк.**
  - **`HRParty03S6@S`** «Вершина: Всеобщее благополучие» / Pinnacle: General Welfare  [Happiness+Votes+Growth]
    - ур.1 (магнитуда 1.375/1.375/1.375): PlanetRawFood +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; Happiness +7 @Empire/ColonizedStarSystem  [Happiness]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +8 (+1.4 за каждые 4 системы)

##### `HRTreeParty03@D` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRParty03S0@D`** «Всеобщее благополучие» / General Welfare  [Happiness]
    - ур.1 (магнитуда 0.8): Happiness +4 @Empire/ColonizedStarSystem  [Happiness]
    - ур.2 (магнитуда 1.6): Happiness +8 @Empire/ColonizedStarSystem  [Happiness]
    - ур.3 (магнитуда 2.4): Happiness +12 @Empire/ColonizedStarSystem  [Happiness]
  - **`HRParty03S1@D`** «Голоса в Совете» / Council Votes  [Votes]
    - ур.1 (магнитуда 0.8): (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +5 (+0.8 за каждые 4 системы)
    - ур.2 (магнитуда 1.6): (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +10 (+1.6 за каждые 4 системы)
    - ур.3 (магнитуда 2.4): (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +14 (+2.4 за каждые 4 системы)
- **Порог 6 очк.**
  - **`HRParty03S2@D`** «Аграрная реформа» / Agrarian Reform  [Growth]
    - ур.1 (магнитуда 1.2): PlanetRawFood +1.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.2 (магнитуда 2.4): PlanetRawFood +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.3 (магнитуда 3.6): PlanetRawFood +3.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
  - **`HRParty03S3@D`** «Всеобщее благополучие / Голоса в Совете» / General Welfare / Council Votes  [Happiness+Votes]
    - ур.1 (магнитуда 0.84/0.84): Happiness +4.2 @Empire/ColonizedStarSystem  [Happiness]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +5 (+0.84 за каждые 4 системы)
    - ур.2 (магнитуда 1.68/1.68): Happiness +8 @Empire/ColonizedStarSystem  [Happiness]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +10 (+1.7 за каждые 4 системы)
    - ур.3 (магнитуда 2.52/2.52): Happiness +13 @Empire/ColonizedStarSystem  [Happiness]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +15 (+2.5 за каждые 4 системы)
- **Порог 12 очк.**
  - **`HRParty03S4@D`** «Голоса в Совете / Аграрная реформа» / Council Votes / Agrarian Reform  [Votes+Growth]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawFood +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +7 (+1.1 за каждые 4 системы)
    - ур.2 (магнитуда 2.24/2.24): PlanetRawFood +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +13 (+2.2 за каждые 4 системы)
    - ур.3 (магнитуда 3.36/3.36): PlanetRawFood +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +20 (+3.4 за каждые 4 системы)
  - **`HRParty03S5@D`** «Всеобщее благополучие / Аграрная реформа» / General Welfare / Agrarian Reform  [Happiness+Growth]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawFood +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; Happiness +6 @Empire/ColonizedStarSystem  [Happiness]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawFood +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; Happiness +11 @Empire/ColonizedStarSystem  [Happiness]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawFood +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; Happiness +17 @Empire/ColonizedStarSystem  [Happiness]
- **Порог 18 очк.**
  - **`HRParty03S6@D`** «Вершина: Всеобщее благополучие» / Pinnacle: General Welfare  [Happiness+Votes+Growth]
    - ур.1 (магнитуда 3.3/3.3/3.3): PlanetRawFood +3.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; Happiness +16 @Empire/ColonizedStarSystem  [Happiness]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +20 (+3.3 за каждые 4 системы)

### Партия 4: Экологи / Ecologists - тема: Growth, Deposit, Sustain

##### `HRTreeParty04@A` - роль Адмирал, канал Ship, мощность x1
- **Порог 0 очк.**
  - **`HRParty04S0@A`** «Резервные экипажи» / Reserve Crews  [Growth]
    - ур.1 (магнитуда 0.5): FleetManpowerRefillRateModifier +0.2 @GarrisonFleet  [Growth]
    - ур.2 (магнитуда 1): FleetManpowerRefillRateModifier +0.4 @GarrisonFleet  [Growth]
  - **`HRParty04S1@A`** «Уклонение» / Evasive Tactics  [Deposit]
    - ур.1 (магнитуда 0.5): Evasion +0.05 @GarrisonFleet  [Deposit]
    - ур.2 (магнитуда 1): Evasion +0.1 @GarrisonFleet  [Deposit]
- **Порог 4 очк.**
  - **`HRParty04S2@A`** «Полевой ремонт» / Field Repairs  [Sustain]
    - ур.1 (магнитуда 0.75): HealthRegenerationRatio +0.15 @GarrisonFleet  [Sustain]
    - ур.2 (магнитуда 1.5): HealthRegenerationRatio +0.3 @GarrisonFleet  [Sustain]
  - **`HRParty04S3@A`** «Резервные экипажи / Уклонение» / Reserve Crews / Evasive Tactics  [Growth+Deposit]
    - ур.1 (магнитуда 0.525/0.525): FleetManpowerRefillRateModifier +0.21 @GarrisonFleet  [Growth]; Evasion +0.052 @GarrisonFleet  [Deposit]
    - ур.2 (магнитуда 1.05/1.05): FleetManpowerRefillRateModifier +0.42 @GarrisonFleet  [Growth]; Evasion +0.105 @GarrisonFleet  [Deposit]
- **Порог 8 очк.**
  - **`HRParty04S4@A`** «Уклонение / Полевой ремонт» / Evasive Tactics / Field Repairs  [Deposit+Sustain]
    - ур.1 (магнитуда 0.7/0.7): HealthRegenerationRatio +0.14 @GarrisonFleet  [Sustain]; Evasion +0.07 @GarrisonFleet  [Deposit]
    - ур.2 (магнитуда 1.4/1.4): HealthRegenerationRatio +0.28 @GarrisonFleet  [Sustain]; Evasion +0.14 @GarrisonFleet  [Deposit]
  - **`HRParty04S5@A`** «Резервные экипажи / Полевой ремонт» / Reserve Crews / Field Repairs  [Growth+Sustain]
    - ур.1 (магнитуда 0.7/0.7): HealthRegenerationRatio +0.14 @GarrisonFleet  [Sustain]; FleetManpowerRefillRateModifier +0.28 @GarrisonFleet  [Growth]
    - ур.2 (магнитуда 1.4/1.4): HealthRegenerationRatio +0.28 @GarrisonFleet  [Sustain]; FleetManpowerRefillRateModifier +0.56 @GarrisonFleet  [Growth]
- **Порог 12 очк.**
  - **`HRParty04S6@A`** «Вершина: Резервные экипажи» / Pinnacle: Reserve Crews  [Growth+Deposit+Sustain]
    - ур.1 (магнитуда 1.375/1.375/1.375): HealthRegenerationRatio +0.275 @GarrisonFleet  [Sustain]; FleetManpowerRefillRateModifier +0.55 @GarrisonFleet  [Growth]; Evasion +0.138 @GarrisonFleet  [Deposit]

##### `HRTreeParty04@G` - роль Губернатор, канал System, мощность x1
- **Порог 0 очк.**
  - **`HRParty04S0@G`** «Агрокультура» / Agriculture  [Growth]
    - ур.1 (магнитуда 0.5): SystemGrowth +10 @ColonizedStarSystem  [Growth]
    - ур.2 (магнитуда 1): SystemGrowth +20 @ColonizedStarSystem  [Growth]
  - **`HRParty04S1@G`** «Добыча ресурсов» / Resource Extraction  [Deposit]
    - ур.1 (магнитуда 0.5): DepositValue +0.5 @ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - ур.2 (магнитуда 1): DepositValue +1 @ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
- **Порог 4 очк.**
  - **`HRParty04S2@G`** «Пополнение войск» / Troop Replenishment  [Sustain]
    - ур.1 (магнитуда 0.75): MaximumSystemManpower +75  [Sustain]
    - ур.2 (магнитуда 1.5): MaximumSystemManpower +150  [Sustain]
  - **`HRParty04S3@G`** «Агрокультура / Добыча ресурсов» / Agriculture / Resource Extraction  [Growth+Deposit]
    - ур.1 (магнитуда 0.525/0.525): SystemGrowth +10 @ColonizedStarSystem  [Growth]; DepositValue +0.525 @ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - ур.2 (магнитуда 1.05/1.05): SystemGrowth +21 @ColonizedStarSystem  [Growth]; DepositValue +1 @ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
- **Порог 8 очк.**
  - **`HRParty04S4@G`** «Добыча ресурсов / Пополнение войск» / Resource Extraction / Troop Replenishment  [Deposit+Sustain]
    - ур.1 (магнитуда 0.7/0.7): MaximumSystemManpower +70  [Sustain]; DepositValue +0.7 @ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - ур.2 (магнитуда 1.4/1.4): MaximumSystemManpower +140  [Sustain]; DepositValue +1.4 @ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
  - **`HRParty04S5@G`** «Агрокультура / Пополнение войск» / Agriculture / Troop Replenishment  [Growth+Sustain]
    - ур.1 (магнитуда 0.7/0.7): MaximumSystemManpower +70  [Sustain]; SystemGrowth +14 @ColonizedStarSystem  [Growth]
    - ур.2 (магнитуда 1.4/1.4): MaximumSystemManpower +140  [Sustain]; SystemGrowth +28 @ColonizedStarSystem  [Growth]
- **Порог 12 очк.**
  - **`HRParty04S6@G`** «Вершина: Агрокультура» / Pinnacle: Agriculture  [Growth+Deposit+Sustain]
    - ур.1 (магнитуда 1.375/1.375/1.375): MaximumSystemManpower +138  [Sustain]; SystemGrowth +28 @ColonizedStarSystem  [Growth]; DepositValue +1.4 @ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]

##### `HRTreeParty04@S` - роль Сенатор, канал Senate, мощность x1
- **Порог 0 очк.**
  - **`HRParty04S0@S`** «Аграрная реформа» / Agrarian Reform  [Growth]
    - ур.1 (магнитуда 1): PlanetRawFood +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
  - **`HRParty04S1@S`** «Ресурсная политика» / Resource Policy  [Deposit]
    - ур.1 (магнитуда 1): DepositValue +0.5 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
- **Порог 2 очк.**
  - **`HRParty04S2@S`** «Службы ремонта» / Repair Corps  [Sustain]
    - ур.1 (магнитуда 1.5): HealthRegenerationRatio +0.075 @Empire/GarrisonFleet  [Sustain]
  - **`HRParty04S3@S`** «Аграрная реформа / Ресурсная политика» / Agrarian Reform / Resource Policy  [Growth+Deposit]
    - ур.1 (магнитуда 1.05/1.05): PlanetRawFood +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; DepositValue +0.525 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
- **Порог 4 очк.**
  - **`HRParty04S4@S`** «Ресурсная политика / Службы ремонта» / Resource Policy / Repair Corps  [Deposit+Sustain]
    - ур.1 (магнитуда 1.4/1.4): HealthRegenerationRatio +0.07 @Empire/GarrisonFleet  [Sustain]; DepositValue +0.7 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
  - **`HRParty04S5@S`** «Аграрная реформа / Службы ремонта» / Agrarian Reform / Repair Corps  [Growth+Sustain]
    - ур.1 (магнитуда 1.4/1.4): HealthRegenerationRatio +0.07 @Empire/GarrisonFleet  [Sustain]; PlanetRawFood +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
- **Порог 6 очк.**
  - **`HRParty04S6@S`** «Вершина: Аграрная реформа» / Pinnacle: Agrarian Reform  [Growth+Deposit+Sustain]
    - ур.1 (магнитуда 1.375/1.375/1.375): HealthRegenerationRatio +0.069 @Empire/GarrisonFleet  [Sustain]; PlanetRawFood +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; DepositValue +0.688 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]

##### `HRTreeParty04@D` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRParty04S0@D`** «Аграрная реформа» / Agrarian Reform  [Growth]
    - ур.1 (магнитуда 0.8): PlanetRawFood +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.2 (магнитуда 1.6): PlanetRawFood +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.3 (магнитуда 2.4): PlanetRawFood +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
  - **`HRParty04S1@D`** «Ресурсная политика» / Resource Policy  [Deposit]
    - ур.1 (магнитуда 0.8): DepositValue +0.4 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - ур.2 (магнитуда 1.6): DepositValue +0.8 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - ур.3 (магнитуда 2.4): DepositValue +1.2 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
- **Порог 6 очк.**
  - **`HRParty04S2@D`** «Службы ремонта» / Repair Corps  [Sustain]
    - ур.1 (магнитуда 1.2): HealthRegenerationRatio +0.06 @Empire/GarrisonFleet  [Sustain]
    - ур.2 (магнитуда 2.4): HealthRegenerationRatio +0.12 @Empire/GarrisonFleet  [Sustain]
    - ур.3 (магнитуда 3.6): HealthRegenerationRatio +0.18 @Empire/GarrisonFleet  [Sustain]
  - **`HRParty04S3@D`** «Аграрная реформа / Ресурсная политика» / Agrarian Reform / Resource Policy  [Growth+Deposit]
    - ур.1 (магнитуда 0.84/0.84): PlanetRawFood +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; DepositValue +0.42 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - ур.2 (магнитуда 1.68/1.68): PlanetRawFood +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; DepositValue +0.84 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - ур.3 (магнитуда 2.52/2.52): PlanetRawFood +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; DepositValue +1.3 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
- **Порог 12 очк.**
  - **`HRParty04S4@D`** «Ресурсная политика / Службы ремонта» / Resource Policy / Repair Corps  [Deposit+Sustain]
    - ур.1 (магнитуда 1.12/1.12): HealthRegenerationRatio +0.056 @Empire/GarrisonFleet  [Sustain]; DepositValue +0.56 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - ур.2 (магнитуда 2.24/2.24): HealthRegenerationRatio +0.112 @Empire/GarrisonFleet  [Sustain]; DepositValue +1.1 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - ур.3 (магнитуда 3.36/3.36): HealthRegenerationRatio +0.168 @Empire/GarrisonFleet  [Sustain]; DepositValue +1.7 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
  - **`HRParty04S5@D`** «Аграрная реформа / Службы ремонта» / Agrarian Reform / Repair Corps  [Growth+Sustain]
    - ур.1 (магнитуда 1.12/1.12): HealthRegenerationRatio +0.056 @Empire/GarrisonFleet  [Sustain]; PlanetRawFood +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.2 (магнитуда 2.24/2.24): HealthRegenerationRatio +0.112 @Empire/GarrisonFleet  [Sustain]; PlanetRawFood +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.3 (магнитуда 3.36/3.36): HealthRegenerationRatio +0.168 @Empire/GarrisonFleet  [Sustain]; PlanetRawFood +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
- **Порог 18 очк.**
  - **`HRParty04S6@D`** «Вершина: Аграрная реформа» / Pinnacle: Agrarian Reform  [Growth+Deposit+Sustain]
    - ур.1 (магнитуда 3.3/3.3/3.3): HealthRegenerationRatio +0.165 @Empire/GarrisonFleet  [Sustain]; PlanetRawFood +3.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; DepositValue +1.6 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]

### Партия 5: Религиозные / Religious - тема: Happiness, Influence, Votes

##### `HRTreeParty05@A` - роль Адмирал, канал Ship, мощность x1
- **Порог 0 очк.**
  - **`HRParty05S0@A`** «Усиленные щиты» / Reinforced Shields  [Happiness]
    - ур.1 (магнитуда 0.5): FleetMaximumShield +0.125 @GarrisonFleet  [Happiness]
    - ур.2 (магнитуда 1): FleetMaximumShield +0.25 @GarrisonFleet  [Happiness]
  - **`HRParty05S1@A`** «Осадное мастерство» / Siegecraft  [Influence]
    - ур.1 (магнитуда 0.5): SiegePower +10% @GarrisonFleet  [Influence]
    - ур.2 (магнитуда 1): SiegePower +20% @GarrisonFleet  [Influence]
- **Порог 4 очк.**
  - **`HRParty05S2@A`** «Влияние в Совете» / Council Clout  [Votes]
    - ур.1 (магнитуда 0.75): SiegePower +15% @GarrisonFleet  [Votes]
    - ур.2 (магнитуда 1.5): SiegePower +30% @GarrisonFleet  [Votes]
  - **`HRParty05S3@A`** «Усиленные щиты / Осадное мастерство» / Reinforced Shields / Siegecraft  [Happiness+Influence]
    - ур.1 (магнитуда 0.525/0.525): SiegePower +10.5% @GarrisonFleet  [Influence]; FleetMaximumShield +0.131 @GarrisonFleet  [Happiness]
    - ур.2 (магнитуда 1.05/1.05): SiegePower +21% @GarrisonFleet  [Influence]; FleetMaximumShield +0.262 @GarrisonFleet  [Happiness]
- **Порог 8 очк.**
  - **`HRParty05S4@A`** «Осадное мастерство / Влияние в Совете» / Siegecraft / Council Clout  [Influence+Votes]
    - ур.1 (магнитуда 0.7/0.7): SiegePower +14% @GarrisonFleet  [Influence]; SiegePower +14% @GarrisonFleet  [Votes]
    - ур.2 (магнитуда 1.4/1.4): SiegePower +28% @GarrisonFleet  [Influence]; SiegePower +28% @GarrisonFleet  [Votes]
  - **`HRParty05S5@A`** «Усиленные щиты / Влияние в Совете» / Reinforced Shields / Council Clout  [Happiness+Votes]
    - ур.1 (магнитуда 0.7/0.7): FleetMaximumShield +0.175 @GarrisonFleet  [Happiness]; SiegePower +14% @GarrisonFleet  [Votes]
    - ур.2 (магнитуда 1.4/1.4): FleetMaximumShield +0.35 @GarrisonFleet  [Happiness]; SiegePower +28% @GarrisonFleet  [Votes]
- **Порог 12 очк.**
  - **`HRParty05S6@A`** «Вершина: Усиленные щиты» / Pinnacle: Reinforced Shields  [Happiness+Influence+Votes]
    - ур.1 (магнитуда 1.375/1.375/1.375): SiegePower +27.5% @GarrisonFleet  [Influence]; FleetMaximumShield +0.344 @GarrisonFleet  [Happiness]; SiegePower +27.5% @GarrisonFleet  [Votes]

##### `HRTreeParty05@G` - роль Губернатор, канал System, мощность x1
- **Порог 0 очк.**
  - **`HRParty05S0@G`** «Общественный порядок» / Public Order  [Happiness]
    - ур.1 (магнитуда 0.5): Happiness +5 @ColonizedStarSystem  [Happiness]
    - ур.2 (магнитуда 1): Happiness +10 @ColonizedStarSystem  [Happiness]
  - **`HRParty05S1@G`** «Престиж колоний» / Colonial Prestige  [Influence]
    - ур.1 (магнитуда 0.5): SystemEmpirePoint +10 @ColonizedStarSystem  [Influence]
    - ур.2 (магнитуда 1): SystemEmpirePoint +20 @ColonizedStarSystem  [Influence]
- **Порог 4 очк.**
  - **`HRParty05S2@G`** «Голос системы» / System Voice  [Votes]
    - ур.1 (магнитуда 0.75): SystemEmpirePoint +15 @ColonizedStarSystem  [Votes]
    - ур.2 (магнитуда 1.5): SystemEmpirePoint +30 @ColonizedStarSystem  [Votes]
  - **`HRParty05S3@G`** «Общественный порядок / Престиж колоний» / Public Order / Colonial Prestige  [Happiness+Influence]
    - ур.1 (магнитуда 0.525/0.525): SystemEmpirePoint +10 @ColonizedStarSystem  [Influence]; Happiness +5 @ColonizedStarSystem  [Happiness]
    - ур.2 (магнитуда 1.05/1.05): SystemEmpirePoint +21 @ColonizedStarSystem  [Influence]; Happiness +10 @ColonizedStarSystem  [Happiness]
- **Порог 8 очк.**
  - **`HRParty05S4@G`** «Престиж колоний / Голос системы» / Colonial Prestige / System Voice  [Influence+Votes]
    - ур.1 (магнитуда 0.7/0.7): SystemEmpirePoint +14 @ColonizedStarSystem  [Influence]; SystemEmpirePoint +14 @ColonizedStarSystem  [Votes]
    - ур.2 (магнитуда 1.4/1.4): SystemEmpirePoint +28 @ColonizedStarSystem  [Influence]; SystemEmpirePoint +28 @ColonizedStarSystem  [Votes]
  - **`HRParty05S5@G`** «Общественный порядок / Голос системы» / Public Order / System Voice  [Happiness+Votes]
    - ур.1 (магнитуда 0.7/0.7): Happiness +7 @ColonizedStarSystem  [Happiness]; SystemEmpirePoint +14 @ColonizedStarSystem  [Votes]
    - ур.2 (магнитуда 1.4/1.4): Happiness +14 @ColonizedStarSystem  [Happiness]; SystemEmpirePoint +28 @ColonizedStarSystem  [Votes]
- **Порог 12 очк.**
  - **`HRParty05S6@G`** «Вершина: Общественный порядок» / Pinnacle: Public Order  [Happiness+Influence+Votes]
    - ур.1 (магнитуда 1.375/1.375/1.375): SystemEmpirePoint +28 @ColonizedStarSystem  [Influence]; Happiness +14 @ColonizedStarSystem  [Happiness]; SystemEmpirePoint +28 @ColonizedStarSystem  [Votes]

##### `HRTreeParty05@S` - роль Сенатор, канал Senate, мощность x1
- **Порог 0 очк.**
  - **`HRParty05S0@S`** «Всеобщее благополучие» / General Welfare  [Happiness]
    - ур.1 (магнитуда 1): Happiness +5 @Empire/ColonizedStarSystem  [Happiness]
  - **`HRParty05S1@S`** «Имперская идеология» / Imperial Ideology  [Influence]
    - ур.1 (магнитуда 1): PlanetRawPrestige +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Порог 2 очк.**
  - **`HRParty05S2@S`** «Голоса в Совете» / Council Votes  [Votes]
    - ур.1 (магнитуда 1.5): (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +9 (+1.5 за каждые 4 системы)
  - **`HRParty05S3@S`** «Всеобщее благополучие / Имперская идеология» / General Welfare / Imperial Ideology  [Happiness+Influence]
    - ур.1 (магнитуда 1.05/1.05): PlanetRawPrestige +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; Happiness +5 @Empire/ColonizedStarSystem  [Happiness]
- **Порог 4 очк.**
  - **`HRParty05S4@S`** «Имперская идеология / Голоса в Совете» / Imperial Ideology / Council Votes  [Influence+Votes]
    - ур.1 (магнитуда 1.4/1.4): PlanetRawPrestige +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +8 (+1.4 за каждые 4 системы)
  - **`HRParty05S5@S`** «Всеобщее благополучие / Голоса в Совете» / General Welfare / Council Votes  [Happiness+Votes]
    - ур.1 (магнитуда 1.4/1.4): Happiness +7 @Empire/ColonizedStarSystem  [Happiness]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +8 (+1.4 за каждые 4 системы)
- **Порог 6 очк.**
  - **`HRParty05S6@S`** «Вершина: Всеобщее благополучие» / Pinnacle: General Welfare  [Happiness+Influence+Votes]
    - ур.1 (магнитуда 1.375/1.375/1.375): PlanetRawPrestige +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; Happiness +7 @Empire/ColonizedStarSystem  [Happiness]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +8 (+1.4 за каждые 4 системы)

##### `HRTreeParty05@D` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRParty05S0@D`** «Всеобщее благополучие» / General Welfare  [Happiness]
    - ур.1 (магнитуда 0.8): Happiness +4 @Empire/ColonizedStarSystem  [Happiness]
    - ур.2 (магнитуда 1.6): Happiness +8 @Empire/ColonizedStarSystem  [Happiness]
    - ур.3 (магнитуда 2.4): Happiness +12 @Empire/ColonizedStarSystem  [Happiness]
  - **`HRParty05S1@D`** «Имперская идеология» / Imperial Ideology  [Influence]
    - ур.1 (магнитуда 0.8): PlanetRawPrestige +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 1.6): PlanetRawPrestige +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 2.4): PlanetRawPrestige +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Порог 6 очк.**
  - **`HRParty05S2@D`** «Голоса в Совете» / Council Votes  [Votes]
    - ур.1 (магнитуда 1.2): (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +7 (+1.2 за каждые 4 системы)
    - ур.2 (магнитуда 2.4): (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +14 (+2.4 за каждые 4 системы)
    - ур.3 (магнитуда 3.6): (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +22 (+3.6 за каждые 4 системы)
  - **`HRParty05S3@D`** «Всеобщее благополучие / Имперская идеология» / General Welfare / Imperial Ideology  [Happiness+Influence]
    - ур.1 (магнитуда 0.84/0.84): PlanetRawPrestige +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; Happiness +4.2 @Empire/ColonizedStarSystem  [Happiness]
    - ур.2 (магнитуда 1.68/1.68): PlanetRawPrestige +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; Happiness +8 @Empire/ColonizedStarSystem  [Happiness]
    - ур.3 (магнитуда 2.52/2.52): PlanetRawPrestige +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; Happiness +13 @Empire/ColonizedStarSystem  [Happiness]
- **Порог 12 очк.**
  - **`HRParty05S4@D`** «Имперская идеология / Голоса в Совете» / Imperial Ideology / Council Votes  [Influence+Votes]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawPrestige +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +7 (+1.1 за каждые 4 системы)
    - ур.2 (магнитуда 2.24/2.24): PlanetRawPrestige +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +13 (+2.2 за каждые 4 системы)
    - ур.3 (магнитуда 3.36/3.36): PlanetRawPrestige +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +20 (+3.4 за каждые 4 системы)
  - **`HRParty05S5@D`** «Всеобщее благополучие / Голоса в Совете» / General Welfare / Council Votes  [Happiness+Votes]
    - ур.1 (магнитуда 1.12/1.12): Happiness +6 @Empire/ColonizedStarSystem  [Happiness]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +7 (+1.1 за каждые 4 системы)
    - ур.2 (магнитуда 2.24/2.24): Happiness +11 @Empire/ColonizedStarSystem  [Happiness]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +13 (+2.2 за каждые 4 системы)
    - ур.3 (магнитуда 3.36/3.36): Happiness +17 @Empire/ColonizedStarSystem  [Happiness]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +20 (+3.4 за каждые 4 системы)
- **Порог 18 очк.**
  - **`HRParty05S6@D`** «Вершина: Всеобщее благополучие» / Pinnacle: General Welfare  [Happiness+Influence+Votes]
    - ур.1 (магнитуда 3.3/3.3/3.3): PlanetRawPrestige +3.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; Happiness +16 @Empire/ColonizedStarSystem  [Happiness]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +20 (+3.3 за каждые 4 системы)

### Партия 6: Милитаристы / Militarists - тема: Offense, Defense, Manpower

##### `HRTreeParty06@A` - роль Адмирал, канал Ship, мощность x1
- **Порог 0 очк.**
  - **`HRParty06S0@A`** «Огневая мощь» / Firepower  [Offense]
    - ур.1 (магнитуда 0.5): ShipDamage +0.1 @GarrisonFleet  [Offense]
    - ур.2 (магнитуда 1): ShipDamage +0.2 @GarrisonFleet  [Offense]
  - **`HRParty06S1@A`** «Крепкие корпуса» / Hardened Hulls  [Defense]
    - ур.1 (магнитуда 0.5): FleetMaximumHealth +0.1 @GarrisonFleet  [Defense]
    - ур.2 (магнитуда 1): FleetMaximumHealth +0.2 @GarrisonFleet  [Defense]
- **Порог 4 очк.**
  - **`HRParty06S2@A`** «Набор экипажей» / Crew Recruitment  [Manpower]
    - ур.1 (магнитуда 0.75): MaximumShipManpower +18.8% @GarrisonFleet/Ship  [Manpower]
    - ур.2 (магнитуда 1.5): MaximumShipManpower +37.5% @GarrisonFleet/Ship  [Manpower]
  - **`HRParty06S3@A`** «Огневая мощь / Крепкие корпуса» / Firepower / Hardened Hulls  [Offense+Defense]
    - ур.1 (магнитуда 0.525/0.525): ShipDamage +0.105 @GarrisonFleet  [Offense]; FleetMaximumHealth +0.105 @GarrisonFleet  [Defense]
    - ур.2 (магнитуда 1.05/1.05): ShipDamage +0.21 @GarrisonFleet  [Offense]; FleetMaximumHealth +0.21 @GarrisonFleet  [Defense]
- **Порог 8 очк.**
  - **`HRParty06S4@A`** «Крепкие корпуса / Набор экипажей» / Hardened Hulls / Crew Recruitment  [Defense+Manpower]
    - ур.1 (магнитуда 0.7/0.7): FleetMaximumHealth +0.14 @GarrisonFleet  [Defense]; MaximumShipManpower +17.5% @GarrisonFleet/Ship  [Manpower]
    - ур.2 (магнитуда 1.4/1.4): FleetMaximumHealth +0.28 @GarrisonFleet  [Defense]; MaximumShipManpower +35% @GarrisonFleet/Ship  [Manpower]
  - **`HRParty06S5@A`** «Огневая мощь / Набор экипажей» / Firepower / Crew Recruitment  [Offense+Manpower]
    - ур.1 (магнитуда 0.7/0.7): ShipDamage +0.14 @GarrisonFleet  [Offense]; MaximumShipManpower +17.5% @GarrisonFleet/Ship  [Manpower]
    - ур.2 (магнитуда 1.4/1.4): ShipDamage +0.28 @GarrisonFleet  [Offense]; MaximumShipManpower +35% @GarrisonFleet/Ship  [Manpower]
- **Порог 12 очк.**
  - **`HRParty06S6@A`** «Вершина: Огневая мощь» / Pinnacle: Firepower  [Offense+Defense+Manpower]
    - ур.1 (магнитуда 1.375/1.375/1.375): ShipDamage +0.275 @GarrisonFleet  [Offense]; FleetMaximumHealth +0.275 @GarrisonFleet  [Defense]; MaximumShipManpower +34.4% @GarrisonFleet/Ship  [Manpower]

##### `HRTreeParty06@G` - роль Губернатор, канал System, мощность x1
- **Порог 0 очк.**
  - **`HRParty06S0@G`** «Гарнизонная выучка» / Garrison Training  [Offense]
    - ур.1 (магнитуда 0.5): GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @ColonizedStarSystem  [Offense]
    - ур.2 (магнитуда 1): GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem  [Offense]
  - **`HRParty06S1@G`** «Укреплённые гарнизоны» / Fortified Garrisons  [Defense]
    - ур.1 (магнитуда 0.5): MaximumSystemManpower +20%  [Defense]
    - ур.2 (магнитуда 1): MaximumSystemManpower +40%  [Defense]
- **Порог 4 очк.**
  - **`HRParty06S2@G`** «Мобилизация» / Mobilization  [Manpower]
    - ур.1 (магнитуда 0.75): NetSystemManpower +8 @ColonizedStarSystem  [Manpower]
    - ур.2 (магнитуда 1.5): NetSystemManpower +15 @ColonizedStarSystem  [Manpower]
  - **`HRParty06S3@G`** «Гарнизонная выучка / Укреплённые гарнизоны» / Garrison Training / Fortified Garrisons  [Offense+Defense]
    - ур.1 (магнитуда 0.525/0.525): GroundBattleDefenderTroopsMaxHealthMultiplier +0.105 @ColonizedStarSystem  [Offense]; MaximumSystemManpower +21%  [Defense]
    - ур.2 (магнитуда 1.05/1.05): GroundBattleDefenderTroopsMaxHealthMultiplier +0.21 @ColonizedStarSystem  [Offense]; MaximumSystemManpower +42%  [Defense]
- **Порог 8 очк.**
  - **`HRParty06S4@G`** «Укреплённые гарнизоны / Мобилизация» / Fortified Garrisons / Mobilization  [Defense+Manpower]
    - ур.1 (магнитуда 0.7/0.7): MaximumSystemManpower +28%  [Defense]; NetSystemManpower +7 @ColonizedStarSystem  [Manpower]
    - ур.2 (магнитуда 1.4/1.4): MaximumSystemManpower +56%  [Defense]; NetSystemManpower +14 @ColonizedStarSystem  [Manpower]
  - **`HRParty06S5@G`** «Гарнизонная выучка / Мобилизация» / Garrison Training / Mobilization  [Offense+Manpower]
    - ур.1 (магнитуда 0.7/0.7): GroundBattleDefenderTroopsMaxHealthMultiplier +0.14 @ColonizedStarSystem  [Offense]; NetSystemManpower +7 @ColonizedStarSystem  [Manpower]
    - ур.2 (магнитуда 1.4/1.4): GroundBattleDefenderTroopsMaxHealthMultiplier +0.28 @ColonizedStarSystem  [Offense]; NetSystemManpower +14 @ColonizedStarSystem  [Manpower]
- **Порог 12 очк.**
  - **`HRParty06S6@G`** «Вершина: Гарнизонная выучка» / Pinnacle: Garrison Training  [Offense+Defense+Manpower]
    - ур.1 (магнитуда 1.375/1.375/1.375): GroundBattleDefenderTroopsMaxHealthMultiplier +0.275 @ColonizedStarSystem  [Offense]; MaximumSystemManpower +55%  [Defense]; NetSystemManpower +14 @ColonizedStarSystem  [Manpower]

##### `HRTreeParty06@S` - роль Сенатор, канал Senate, мощность x1
- **Порог 0 очк.**
  - **`HRParty06S0@S`** «Военные ассигнования» / Military Appropriations  [Offense]
    - ур.1 (магнитуда 1): ShipDamage +0.1 @Empire/GarrisonFleet  [Offense]
  - **`HRParty06S1@S`** «Бронирование флотов» / Fleet Armor Programs  [Defense]
    - ур.1 (магнитуда 1): FleetMaximumHealth +0.1 @Empire/GarrisonFleet  [Defense]
- **Порог 2 очк.**
  - **`HRParty06S2@S`** «Всеобщая мобилизация» / National Mobilization  [Manpower]
    - ур.1 (магнитуда 1.5): MaximumEmpireManpowerStock +37.5% @Empire  [Manpower]
  - **`HRParty06S3@S`** «Военные ассигнования / Бронирование флотов» / Military Appropriations / Fleet Armor Programs  [Offense+Defense]
    - ур.1 (магнитуда 1.05/1.05): ShipDamage +0.105 @Empire/GarrisonFleet  [Offense]; FleetMaximumHealth +0.105 @Empire/GarrisonFleet  [Defense]
- **Порог 4 очк.**
  - **`HRParty06S4@S`** «Бронирование флотов / Всеобщая мобилизация» / Fleet Armor Programs / National Mobilization  [Defense+Manpower]
    - ур.1 (магнитуда 1.4/1.4): FleetMaximumHealth +0.14 @Empire/GarrisonFleet  [Defense]; MaximumEmpireManpowerStock +35% @Empire  [Manpower]
  - **`HRParty06S5@S`** «Военные ассигнования / Всеобщая мобилизация» / Military Appropriations / National Mobilization  [Offense+Manpower]
    - ур.1 (магнитуда 1.4/1.4): ShipDamage +0.14 @Empire/GarrisonFleet  [Offense]; MaximumEmpireManpowerStock +35% @Empire  [Manpower]
- **Порог 6 очк.**
  - **`HRParty06S6@S`** «Вершина: Военные ассигнования» / Pinnacle: Military Appropriations  [Offense+Defense+Manpower]
    - ур.1 (магнитуда 1.375/1.375/1.375): ShipDamage +0.138 @Empire/GarrisonFleet  [Offense]; FleetMaximumHealth +0.138 @Empire/GarrisonFleet  [Defense]; MaximumEmpireManpowerStock +34.4% @Empire  [Manpower]

##### `HRTreeParty06@D` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRParty06S0@D`** «Военные ассигнования» / Military Appropriations  [Offense]
    - ур.1 (магнитуда 0.8): ShipDamage +0.08 @Empire/GarrisonFleet  [Offense]
    - ур.2 (магнитуда 1.6): ShipDamage +0.16 @Empire/GarrisonFleet  [Offense]
    - ур.3 (магнитуда 2.4): ShipDamage +0.24 @Empire/GarrisonFleet  [Offense]
  - **`HRParty06S1@D`** «Бронирование флотов» / Fleet Armor Programs  [Defense]
    - ур.1 (магнитуда 0.8): FleetMaximumHealth +0.08 @Empire/GarrisonFleet  [Defense]
    - ур.2 (магнитуда 1.6): FleetMaximumHealth +0.16 @Empire/GarrisonFleet  [Defense]
    - ур.3 (магнитуда 2.4): FleetMaximumHealth +0.24 @Empire/GarrisonFleet  [Defense]
- **Порог 6 очк.**
  - **`HRParty06S2@D`** «Всеобщая мобилизация» / National Mobilization  [Manpower]
    - ур.1 (магнитуда 1.2): MaximumEmpireManpowerStock +30% @Empire  [Manpower]
    - ур.2 (магнитуда 2.4): MaximumEmpireManpowerStock +60% @Empire  [Manpower]
    - ур.3 (магнитуда 3.6): MaximumEmpireManpowerStock +90% @Empire  [Manpower]
  - **`HRParty06S3@D`** «Военные ассигнования / Бронирование флотов» / Military Appropriations / Fleet Armor Programs  [Offense+Defense]
    - ур.1 (магнитуда 0.84/0.84): ShipDamage +0.084 @Empire/GarrisonFleet  [Offense]; FleetMaximumHealth +0.084 @Empire/GarrisonFleet  [Defense]
    - ур.2 (магнитуда 1.68/1.68): ShipDamage +0.168 @Empire/GarrisonFleet  [Offense]; FleetMaximumHealth +0.168 @Empire/GarrisonFleet  [Defense]
    - ур.3 (магнитуда 2.52/2.52): ShipDamage +0.252 @Empire/GarrisonFleet  [Offense]; FleetMaximumHealth +0.252 @Empire/GarrisonFleet  [Defense]
- **Порог 12 очк.**
  - **`HRParty06S4@D`** «Бронирование флотов / Всеобщая мобилизация» / Fleet Armor Programs / National Mobilization  [Defense+Manpower]
    - ур.1 (магнитуда 1.12/1.12): FleetMaximumHealth +0.112 @Empire/GarrisonFleet  [Defense]; MaximumEmpireManpowerStock +28% @Empire  [Manpower]
    - ур.2 (магнитуда 2.24/2.24): FleetMaximumHealth +0.224 @Empire/GarrisonFleet  [Defense]; MaximumEmpireManpowerStock +56% @Empire  [Manpower]
    - ур.3 (магнитуда 3.36/3.36): FleetMaximumHealth +0.336 @Empire/GarrisonFleet  [Defense]; MaximumEmpireManpowerStock +84% @Empire  [Manpower]
  - **`HRParty06S5@D`** «Военные ассигнования / Всеобщая мобилизация» / Military Appropriations / National Mobilization  [Offense+Manpower]
    - ур.1 (магнитуда 1.12/1.12): ShipDamage +0.112 @Empire/GarrisonFleet  [Offense]; MaximumEmpireManpowerStock +28% @Empire  [Manpower]
    - ур.2 (магнитуда 2.24/2.24): ShipDamage +0.224 @Empire/GarrisonFleet  [Offense]; MaximumEmpireManpowerStock +56% @Empire  [Manpower]
    - ур.3 (магнитуда 3.36/3.36): ShipDamage +0.336 @Empire/GarrisonFleet  [Offense]; MaximumEmpireManpowerStock +84% @Empire  [Manpower]
- **Порог 18 очк.**
  - **`HRParty06S6@D`** «Вершина: Военные ассигнования» / Pinnacle: Military Appropriations  [Offense+Defense+Manpower]
    - ур.1 (магнитуда 3.3/3.3/3.3): ShipDamage +0.33 @Empire/GarrisonFleet  [Offense]; FleetMaximumHealth +0.33 @Empire/GarrisonFleet  [Defense]; MaximumEmpireManpowerStock +82.5% @Empire  [Manpower]

## Деревья диктаторов фракций (14)

### Софоны / Sophons (`HRTreeDictSophons`) - тема: Science, Growth, Influence

##### `HRTreeDictSophons` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRDictSophonsS0`** «Научный комитет» / Science Committee  [Science]
    - ур.1 (магнитуда 0.8): PlanetRawScience +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.2 (магнитуда 1.6): PlanetRawScience +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.3 (магнитуда 2.4): PlanetRawScience +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
  - **`HRDictSophonsS1`** «Аграрная реформа» / Agrarian Reform  [Growth]
    - ур.1 (магнитуда 0.8): PlanetRawFood +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.2 (магнитуда 1.6): PlanetRawFood +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.3 (магнитуда 2.4): PlanetRawFood +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
- **Порог 6 очк.**
  - **`HRDictSophonsS2`** «Имперская идеология» / Imperial Ideology  [Influence]
    - ур.1 (магнитуда 1.2): PlanetRawPrestige +1.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 2.4): PlanetRawPrestige +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 3.6): PlanetRawPrestige +3.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
  - **`HRDictSophonsS3`** «Научный комитет / Аграрная реформа» / Science Committee / Agrarian Reform  [Science+Growth]
    - ур.1 (магнитуда 0.84/0.84): PlanetRawFood +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; PlanetRawScience +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.2 (магнитуда 1.68/1.68): PlanetRawFood +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; PlanetRawScience +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.3 (магнитуда 2.52/2.52): PlanetRawFood +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; PlanetRawScience +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
- **Порог 12 очк.**
  - **`HRDictSophonsS4`** «Аграрная реформа / Имперская идеология» / Agrarian Reform / Imperial Ideology  [Growth+Influence]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawFood +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; PlanetRawPrestige +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawFood +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; PlanetRawPrestige +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawFood +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; PlanetRawPrestige +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
  - **`HRDictSophonsS5`** «Научный комитет / Имперская идеология» / Science Committee / Imperial Ideology  [Science+Influence]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawScience +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawScience +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawScience +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Порог 18 очк.**
  - **`HRDictSophonsS6`** «Вершина: Научный комитет» / Pinnacle: Science Committee  [Science+Growth+Influence+Happiness+Manpower]
    - ур.1 (магнитуда 2.7/2.7/2.7/2.7/2.7): PlanetRawFood +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; PlanetRawScience +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; Happiness +14 @Empire/ColonizedStarSystem  [Happiness]; MaximumEmpireManpowerStock +67.5% @Empire  [Manpower]

### Краверы / Cravers (`HRTreeDictCravers`) - тема: Offense, Manpower, Production

##### `HRTreeDictCravers` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRDictCraversS0`** «Военные ассигнования» / Military Appropriations  [Offense]
    - ур.1 (магнитуда 0.8): ShipDamage +0.08 @Empire/GarrisonFleet  [Offense]
    - ур.2 (магнитуда 1.6): ShipDamage +0.16 @Empire/GarrisonFleet  [Offense]
    - ур.3 (магнитуда 2.4): ShipDamage +0.24 @Empire/GarrisonFleet  [Offense]
  - **`HRDictCraversS1`** «Всеобщая мобилизация» / National Mobilization  [Manpower]
    - ур.1 (магнитуда 0.8): MaximumEmpireManpowerStock +20% @Empire  [Manpower]
    - ур.2 (магнитуда 1.6): MaximumEmpireManpowerStock +40% @Empire  [Manpower]
    - ур.3 (магнитуда 2.4): MaximumEmpireManpowerStock +60% @Empire  [Manpower]
- **Порог 6 очк.**
  - **`HRDictCraversS2`** «Государственная индустрия» / State Industry  [Production]
    - ур.1 (магнитуда 1.2): PlanetRawIndustry +1.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - ур.2 (магнитуда 2.4): PlanetRawIndustry +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - ур.3 (магнитуда 3.6): PlanetRawIndustry +3.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
  - **`HRDictCraversS3`** «Военные ассигнования / Всеобщая мобилизация» / Military Appropriations / National Mobilization  [Offense+Manpower]
    - ур.1 (магнитуда 0.84/0.84): ShipDamage +0.084 @Empire/GarrisonFleet  [Offense]; MaximumEmpireManpowerStock +21% @Empire  [Manpower]
    - ур.2 (магнитуда 1.68/1.68): ShipDamage +0.168 @Empire/GarrisonFleet  [Offense]; MaximumEmpireManpowerStock +42% @Empire  [Manpower]
    - ур.3 (магнитуда 2.52/2.52): ShipDamage +0.252 @Empire/GarrisonFleet  [Offense]; MaximumEmpireManpowerStock +63% @Empire  [Manpower]
- **Порог 12 очк.**
  - **`HRDictCraversS4`** «Всеобщая мобилизация / Государственная индустрия» / National Mobilization / State Industry  [Manpower+Production]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawIndustry +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; MaximumEmpireManpowerStock +28% @Empire  [Manpower]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawIndustry +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; MaximumEmpireManpowerStock +56% @Empire  [Manpower]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawIndustry +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; MaximumEmpireManpowerStock +84% @Empire  [Manpower]
  - **`HRDictCraversS5`** «Военные ассигнования / Государственная индустрия» / Military Appropriations / State Industry  [Offense+Production]
    - ур.1 (магнитуда 1.12/1.12): ShipDamage +0.112 @Empire/GarrisonFleet  [Offense]; PlanetRawIndustry +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - ур.2 (магнитуда 2.24/2.24): ShipDamage +0.224 @Empire/GarrisonFleet  [Offense]; PlanetRawIndustry +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - ур.3 (магнитуда 3.36/3.36): ShipDamage +0.336 @Empire/GarrisonFleet  [Offense]; PlanetRawIndustry +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
- **Порог 18 очк.**
  - **`HRDictCraversS6`** «Вершина: Военные ассигнования» / Pinnacle: Military Appropriations  [Offense+Manpower+Production+Happiness+Manpower]
    - ур.1 (магнитуда 2.7/2.7/2.7/2.7/2.7): ShipDamage +0.27 @Empire/GarrisonFleet  [Offense]; PlanetRawIndustry +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; Happiness +14 @Empire/ColonizedStarSystem  [Happiness]; MaximumEmpireManpowerStock +140% @Empire  [Manpower]

### Земляне / Terrans (`HRTreeDictTerrans`) - тема: Production, Money, Influence

##### `HRTreeDictTerrans` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRDictTerransS0`** «Государственная индустрия» / State Industry  [Production]
    - ур.1 (магнитуда 0.8): PlanetRawIndustry +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - ур.2 (магнитуда 1.6): PlanetRawIndustry +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - ур.3 (магнитуда 2.4): PlanetRawIndustry +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
  - **`HRDictTerransS1`** «Казначейство» / Treasury  [Money]
    - ур.1 (магнитуда 0.8): PlanetRawDust +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.2 (магнитуда 1.6): PlanetRawDust +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.3 (магнитуда 2.4): PlanetRawDust +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
- **Порог 6 очк.**
  - **`HRDictTerransS2`** «Имперская идеология» / Imperial Ideology  [Influence]
    - ур.1 (магнитуда 1.2): PlanetRawPrestige +1.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 2.4): PlanetRawPrestige +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 3.6): PlanetRawPrestige +3.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
  - **`HRDictTerransS3`** «Государственная индустрия / Казначейство» / State Industry / Treasury  [Production+Money]
    - ур.1 (магнитуда 0.84/0.84): PlanetRawIndustry +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawDust +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.2 (магнитуда 1.68/1.68): PlanetRawIndustry +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawDust +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.3 (магнитуда 2.52/2.52): PlanetRawIndustry +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawDust +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
- **Порог 12 очк.**
  - **`HRDictTerransS4`** «Казначейство / Имперская идеология» / Treasury / Imperial Ideology  [Money+Influence]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawDust +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; PlanetRawPrestige +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawDust +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; PlanetRawPrestige +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawDust +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; PlanetRawPrestige +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
  - **`HRDictTerransS5`** «Государственная индустрия / Имперская идеология» / State Industry / Imperial Ideology  [Production+Influence]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawIndustry +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawPrestige +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawIndustry +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawPrestige +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawIndustry +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawPrestige +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Порог 18 очк.**
  - **`HRDictTerransS6`** «Вершина: Государственная индустрия» / Pinnacle: State Industry  [Production+Money+Influence+Happiness+Manpower]
    - ур.1 (магнитуда 2.7/2.7/2.7/2.7/2.7): PlanetRawIndustry +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawDust +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; PlanetRawPrestige +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; Happiness +14 @Empire/ColonizedStarSystem  [Happiness]; MaximumEmpireManpowerStock +67.5% @Empire  [Manpower]

### Венецианцы / Venetians (`HRTreeDictVenetians`) - тема: Money, Influence, Votes

##### `HRTreeDictVenetians` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRDictVenetiansS0`** «Казначейство» / Treasury  [Money]
    - ур.1 (магнитуда 0.8): PlanetRawDust +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.2 (магнитуда 1.6): PlanetRawDust +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.3 (магнитуда 2.4): PlanetRawDust +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
  - **`HRDictVenetiansS1`** «Имперская идеология» / Imperial Ideology  [Influence]
    - ур.1 (магнитуда 0.8): PlanetRawPrestige +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 1.6): PlanetRawPrestige +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 2.4): PlanetRawPrestige +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Порог 6 очк.**
  - **`HRDictVenetiansS2`** «Голоса в Совете» / Council Votes  [Votes]
    - ур.1 (магнитуда 1.2): (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +7 (+1.2 за каждые 4 системы)
    - ур.2 (магнитуда 2.4): (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +14 (+2.4 за каждые 4 системы)
    - ур.3 (магнитуда 3.6): (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +22 (+3.6 за каждые 4 системы)
  - **`HRDictVenetiansS3`** «Казначейство / Имперская идеология» / Treasury / Imperial Ideology  [Money+Influence]
    - ур.1 (магнитуда 0.84/0.84): PlanetRawDust +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; PlanetRawPrestige +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 1.68/1.68): PlanetRawDust +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; PlanetRawPrestige +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 2.52/2.52): PlanetRawDust +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; PlanetRawPrestige +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Порог 12 очк.**
  - **`HRDictVenetiansS4`** «Имперская идеология / Голоса в Совете» / Imperial Ideology / Council Votes  [Influence+Votes]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawPrestige +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +7 (+1.1 за каждые 4 системы)
    - ур.2 (магнитуда 2.24/2.24): PlanetRawPrestige +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +13 (+2.2 за каждые 4 системы)
    - ур.3 (магнитуда 3.36/3.36): PlanetRawPrestige +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +20 (+3.4 за каждые 4 системы)
  - **`HRDictVenetiansS5`** «Казначейство / Голоса в Совете» / Treasury / Council Votes  [Money+Votes]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawDust +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +7 (+1.1 за каждые 4 системы)
    - ур.2 (магнитуда 2.24/2.24): PlanetRawDust +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +13 (+2.2 за каждые 4 системы)
    - ур.3 (магнитуда 3.36/3.36): PlanetRawDust +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +20 (+3.4 за каждые 4 системы)
- **Порог 18 очк.**
  - **`HRDictVenetiansS6`** «Вершина: Казначейство» / Pinnacle: Treasury  [Money+Influence+Votes+Happiness+Manpower]
    - ур.1 (магнитуда 2.7/2.7/2.7/2.7/2.7): PlanetRawDust +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; PlanetRawPrestige +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; Happiness +14 @Empire/ColonizedStarSystem  [Happiness]; MaximumEmpireManpowerStock +67.5% @Empire  [Manpower]; (Council votes: read by the Council, see HERO_TREES_MOD.md); голоса Совета: +16 (+2.7 за каждые 4 системы)

### Вампирилис / Vampirilis (`HRTreeDictVampirilis`) - тема: Influence, Offense, Sustain

##### `HRTreeDictVampirilis` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRDictVampirilisS0`** «Имперская идеология» / Imperial Ideology  [Influence]
    - ур.1 (магнитуда 0.8): PlanetRawPrestige +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 1.6): PlanetRawPrestige +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 2.4): PlanetRawPrestige +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
  - **`HRDictVampirilisS1`** «Военные ассигнования» / Military Appropriations  [Offense]
    - ур.1 (магнитуда 0.8): ShipDamage +0.08 @Empire/GarrisonFleet  [Offense]
    - ур.2 (магнитуда 1.6): ShipDamage +0.16 @Empire/GarrisonFleet  [Offense]
    - ур.3 (магнитуда 2.4): ShipDamage +0.24 @Empire/GarrisonFleet  [Offense]
- **Порог 6 очк.**
  - **`HRDictVampirilisS2`** «Службы ремонта» / Repair Corps  [Sustain]
    - ур.1 (магнитуда 1.2): HealthRegenerationRatio +0.06 @Empire/GarrisonFleet  [Sustain]
    - ур.2 (магнитуда 2.4): HealthRegenerationRatio +0.12 @Empire/GarrisonFleet  [Sustain]
    - ур.3 (магнитуда 3.6): HealthRegenerationRatio +0.18 @Empire/GarrisonFleet  [Sustain]
  - **`HRDictVampirilisS3`** «Имперская идеология / Военные ассигнования» / Imperial Ideology / Military Appropriations  [Influence+Offense]
    - ур.1 (магнитуда 0.84/0.84): ShipDamage +0.084 @Empire/GarrisonFleet  [Offense]; PlanetRawPrestige +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 1.68/1.68): ShipDamage +0.168 @Empire/GarrisonFleet  [Offense]; PlanetRawPrestige +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 2.52/2.52): ShipDamage +0.252 @Empire/GarrisonFleet  [Offense]; PlanetRawPrestige +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Порог 12 очк.**
  - **`HRDictVampirilisS4`** «Военные ассигнования / Службы ремонта» / Military Appropriations / Repair Corps  [Offense+Sustain]
    - ур.1 (магнитуда 1.12/1.12): ShipDamage +0.112 @Empire/GarrisonFleet  [Offense]; HealthRegenerationRatio +0.056 @Empire/GarrisonFleet  [Sustain]
    - ур.2 (магнитуда 2.24/2.24): ShipDamage +0.224 @Empire/GarrisonFleet  [Offense]; HealthRegenerationRatio +0.112 @Empire/GarrisonFleet  [Sustain]
    - ур.3 (магнитуда 3.36/3.36): ShipDamage +0.336 @Empire/GarrisonFleet  [Offense]; HealthRegenerationRatio +0.168 @Empire/GarrisonFleet  [Sustain]
  - **`HRDictVampirilisS5`** «Имперская идеология / Службы ремонта» / Imperial Ideology / Repair Corps  [Influence+Sustain]
    - ур.1 (магнитуда 1.12/1.12): HealthRegenerationRatio +0.056 @Empire/GarrisonFleet  [Sustain]; PlanetRawPrestige +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 2.24/2.24): HealthRegenerationRatio +0.112 @Empire/GarrisonFleet  [Sustain]; PlanetRawPrestige +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 3.36/3.36): HealthRegenerationRatio +0.168 @Empire/GarrisonFleet  [Sustain]; PlanetRawPrestige +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Порог 18 очк.**
  - **`HRDictVampirilisS6`** «Вершина: Имперская идеология» / Pinnacle: Imperial Ideology  [Influence+Offense+Sustain+Happiness+Manpower]
    - ур.1 (магнитуда 2.7/2.7/2.7/2.7/2.7): ShipDamage +0.27 @Empire/GarrisonFleet  [Offense]; HealthRegenerationRatio +0.135 @Empire/GarrisonFleet  [Sustain]; PlanetRawPrestige +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; Happiness +14 @Empire/ColonizedStarSystem  [Happiness]; MaximumEmpireManpowerStock +67.5% @Empire  [Manpower]

### Повелители времени / Time Lords (`HRTreeDictTimeLords`) - тема: Science, Mobility, Experience

##### `HRTreeDictTimeLords` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRDictTimeLordsS0`** «Научный комитет» / Science Committee  [Science]
    - ур.1 (магнитуда 0.8): PlanetRawScience +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.2 (магнитуда 1.6): PlanetRawScience +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.3 (магнитуда 2.4): PlanetRawScience +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
  - **`HRDictTimeLordsS1`** «Имперская навигация» / Imperial Navigation  [Mobility]
    - ур.1 (магнитуда 0.8): ShipMaximumMovement +0.8 @Empire/GarrisonFleet  [Mobility]
    - ур.2 (магнитуда 1.6): ShipMaximumMovement +1.6 @Empire/GarrisonFleet  [Mobility]
    - ур.3 (магнитуда 2.4): ShipMaximumMovement +2.4 @Empire/GarrisonFleet  [Mobility]
- **Порог 6 очк.**
  - **`HRDictTimeLordsS2`** «Образовательная реформа» / Education Reform  [Experience]
    - ур.1 (магнитуда 1.2): SystemResearch +12% @Empire/ColonizedStarSystem  [Experience]
    - ур.2 (магнитуда 2.4): SystemResearch +24% @Empire/ColonizedStarSystem  [Experience]
    - ур.3 (магнитуда 3.6): SystemResearch +36% @Empire/ColonizedStarSystem  [Experience]
  - **`HRDictTimeLordsS3`** «Научный комитет / Имперская навигация» / Science Committee / Imperial Navigation  [Science+Mobility]
    - ур.1 (магнитуда 0.84/0.84): ShipMaximumMovement +0.84 @Empire/GarrisonFleet  [Mobility]; PlanetRawScience +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.2 (магнитуда 1.68/1.68): ShipMaximumMovement +1.7 @Empire/GarrisonFleet  [Mobility]; PlanetRawScience +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.3 (магнитуда 2.52/2.52): ShipMaximumMovement +2.5 @Empire/GarrisonFleet  [Mobility]; PlanetRawScience +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
- **Порог 12 очк.**
  - **`HRDictTimeLordsS4`** «Имперская навигация / Образовательная реформа» / Imperial Navigation / Education Reform  [Mobility+Experience]
    - ур.1 (магнитуда 1.12/1.12): ShipMaximumMovement +1.1 @Empire/GarrisonFleet  [Mobility]; SystemResearch +11.2% @Empire/ColonizedStarSystem  [Experience]
    - ур.2 (магнитуда 2.24/2.24): ShipMaximumMovement +2.2 @Empire/GarrisonFleet  [Mobility]; SystemResearch +22.4% @Empire/ColonizedStarSystem  [Experience]
    - ур.3 (магнитуда 3.36/3.36): ShipMaximumMovement +3.4 @Empire/GarrisonFleet  [Mobility]; SystemResearch +33.6% @Empire/ColonizedStarSystem  [Experience]
  - **`HRDictTimeLordsS5`** «Научный комитет / Образовательная реформа» / Science Committee / Education Reform  [Science+Experience]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawScience +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; SystemResearch +11.2% @Empire/ColonizedStarSystem  [Experience]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawScience +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; SystemResearch +22.4% @Empire/ColonizedStarSystem  [Experience]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawScience +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; SystemResearch +33.6% @Empire/ColonizedStarSystem  [Experience]
- **Порог 18 очк.**
  - **`HRDictTimeLordsS6`** «Вершина: Научный комитет» / Pinnacle: Science Committee  [Science+Mobility+Experience+Happiness+Manpower]
    - ур.1 (магнитуда 2.7/2.7/2.7/2.7/2.7): ShipMaximumMovement +2.7 @Empire/GarrisonFleet  [Mobility]; PlanetRawScience +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; Happiness +14 @Empire/ColonizedStarSystem  [Happiness]; MaximumEmpireManpowerStock +67.5% @Empire  [Manpower]; SystemResearch +27% @Empire/ColonizedStarSystem  [Experience]

### Падшие / Unfallen (`HRTreeDictUnfallen`) - тема: Defense, Growth, Happiness

##### `HRTreeDictUnfallen` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRDictUnfallenS0`** «Бронирование флотов» / Fleet Armor Programs  [Defense]
    - ур.1 (магнитуда 0.8): FleetMaximumHealth +0.08 @Empire/GarrisonFleet  [Defense]
    - ур.2 (магнитуда 1.6): FleetMaximumHealth +0.16 @Empire/GarrisonFleet  [Defense]
    - ур.3 (магнитуда 2.4): FleetMaximumHealth +0.24 @Empire/GarrisonFleet  [Defense]
  - **`HRDictUnfallenS1`** «Аграрная реформа» / Agrarian Reform  [Growth]
    - ур.1 (магнитуда 0.8): PlanetRawFood +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.2 (магнитуда 1.6): PlanetRawFood +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.3 (магнитуда 2.4): PlanetRawFood +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
- **Порог 6 очк.**
  - **`HRDictUnfallenS2`** «Всеобщее благополучие» / General Welfare  [Happiness]
    - ур.1 (магнитуда 1.2): Happiness +6 @Empire/ColonizedStarSystem  [Happiness]
    - ур.2 (магнитуда 2.4): Happiness +12 @Empire/ColonizedStarSystem  [Happiness]
    - ур.3 (магнитуда 3.6): Happiness +18 @Empire/ColonizedStarSystem  [Happiness]
  - **`HRDictUnfallenS3`** «Бронирование флотов / Аграрная реформа» / Fleet Armor Programs / Agrarian Reform  [Defense+Growth]
    - ур.1 (магнитуда 0.84/0.84): FleetMaximumHealth +0.084 @Empire/GarrisonFleet  [Defense]; PlanetRawFood +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.2 (магнитуда 1.68/1.68): FleetMaximumHealth +0.168 @Empire/GarrisonFleet  [Defense]; PlanetRawFood +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.3 (магнитуда 2.52/2.52): FleetMaximumHealth +0.252 @Empire/GarrisonFleet  [Defense]; PlanetRawFood +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
- **Порог 12 очк.**
  - **`HRDictUnfallenS4`** «Аграрная реформа / Всеобщее благополучие» / Agrarian Reform / General Welfare  [Growth+Happiness]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawFood +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; Happiness +6 @Empire/ColonizedStarSystem  [Happiness]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawFood +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; Happiness +11 @Empire/ColonizedStarSystem  [Happiness]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawFood +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; Happiness +17 @Empire/ColonizedStarSystem  [Happiness]
  - **`HRDictUnfallenS5`** «Бронирование флотов / Всеобщее благополучие» / Fleet Armor Programs / General Welfare  [Defense+Happiness]
    - ур.1 (магнитуда 1.12/1.12): FleetMaximumHealth +0.112 @Empire/GarrisonFleet  [Defense]; Happiness +6 @Empire/ColonizedStarSystem  [Happiness]
    - ур.2 (магнитуда 2.24/2.24): FleetMaximumHealth +0.224 @Empire/GarrisonFleet  [Defense]; Happiness +11 @Empire/ColonizedStarSystem  [Happiness]
    - ур.3 (магнитуда 3.36/3.36): FleetMaximumHealth +0.336 @Empire/GarrisonFleet  [Defense]; Happiness +17 @Empire/ColonizedStarSystem  [Happiness]
- **Порог 18 очк.**
  - **`HRDictUnfallenS6`** «Вершина: Бронирование флотов» / Pinnacle: Fleet Armor Programs  [Defense+Growth+Happiness+Happiness+Manpower]
    - ур.1 (магнитуда 2.7/2.7/2.7/2.7/2.7): FleetMaximumHealth +0.27 @Empire/GarrisonFleet  [Defense]; PlanetRawFood +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; Happiness +27 @Empire/ColonizedStarSystem  [Happiness]; MaximumEmpireManpowerStock +67.5% @Empire  [Manpower]

### Эндлесс / Endless (`HRTreeDictEndless`) - тема: Science, Influence, Production

##### `HRTreeDictEndless` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRDictEndlessS0`** «Научный комитет» / Science Committee  [Science]
    - ур.1 (магнитуда 0.8): PlanetRawScience +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.2 (магнитуда 1.6): PlanetRawScience +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.3 (магнитуда 2.4): PlanetRawScience +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
  - **`HRDictEndlessS1`** «Имперская идеология» / Imperial Ideology  [Influence]
    - ур.1 (магнитуда 0.8): PlanetRawPrestige +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 1.6): PlanetRawPrestige +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 2.4): PlanetRawPrestige +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Порог 6 очк.**
  - **`HRDictEndlessS2`** «Государственная индустрия» / State Industry  [Production]
    - ур.1 (магнитуда 1.2): PlanetRawIndustry +1.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - ур.2 (магнитуда 2.4): PlanetRawIndustry +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - ур.3 (магнитуда 3.6): PlanetRawIndustry +3.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
  - **`HRDictEndlessS3`** «Научный комитет / Имперская идеология» / Science Committee / Imperial Ideology  [Science+Influence]
    - ур.1 (магнитуда 0.84/0.84): PlanetRawScience +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 1.68/1.68): PlanetRawScience +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 2.52/2.52): PlanetRawScience +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Порог 12 очк.**
  - **`HRDictEndlessS4`** «Имперская идеология / Государственная индустрия» / Imperial Ideology / State Industry  [Influence+Production]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawIndustry +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawPrestige +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawIndustry +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawPrestige +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawIndustry +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawPrestige +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
  - **`HRDictEndlessS5`** «Научный комитет / Государственная индустрия» / Science Committee / State Industry  [Science+Production]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawIndustry +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawScience +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawIndustry +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawScience +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawIndustry +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawScience +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
- **Порог 18 очк.**
  - **`HRDictEndlessS6`** «Вершина: Научный комитет» / Pinnacle: Science Committee  [Science+Influence+Production+Happiness+Manpower]
    - ур.1 (магнитуда 2.7/2.7/2.7/2.7/2.7): PlanetRawIndustry +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawScience +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; Happiness +14 @Empire/ColonizedStarSystem  [Happiness]; MaximumEmpireManpowerStock +67.5% @Empire  [Manpower]

### Гораций / Horatio (`HRTreeDictHoratio`) - тема: Money, Mobility, Diplomacy

##### `HRTreeDictHoratio` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRDictHoratioS0`** «Казначейство» / Treasury  [Money]
    - ур.1 (магнитуда 0.8): PlanetRawDust +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.2 (магнитуда 1.6): PlanetRawDust +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.3 (магнитуда 2.4): PlanetRawDust +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
  - **`HRDictHoratioS1`** «Имперская навигация» / Imperial Navigation  [Mobility]
    - ур.1 (магнитуда 0.8): ShipMaximumMovement +0.8 @Empire/GarrisonFleet  [Mobility]
    - ур.2 (магнитуда 1.6): ShipMaximumMovement +1.6 @Empire/GarrisonFleet  [Mobility]
    - ур.3 (магнитуда 2.4): ShipMaximumMovement +2.4 @Empire/GarrisonFleet  [Mobility]
- **Порог 6 очк.**
  - **`HRDictHoratioS2`** «Дипломатический корпус» / Diplomatic Corps  [Diplomacy]
    - ур.1 (магнитуда 1.2): PeacefulTreatyCostReduction -0.6 @Empire  [Diplomacy]; AggressiveTreatyCostReduction -0.6 @Empire  [Diplomacy]
    - ур.2 (магнитуда 2.4): PeacefulTreatyCostReduction -1.2 @Empire  [Diplomacy]; AggressiveTreatyCostReduction -1.2 @Empire  [Diplomacy]
    - ур.3 (магнитуда 3.6): PeacefulTreatyCostReduction -1.8 @Empire  [Diplomacy]; AggressiveTreatyCostReduction -1.8 @Empire  [Diplomacy]
  - **`HRDictHoratioS3`** «Казначейство / Имперская навигация» / Treasury / Imperial Navigation  [Money+Mobility]
    - ур.1 (магнитуда 0.84/0.84): ShipMaximumMovement +0.84 @Empire/GarrisonFleet  [Mobility]; PlanetRawDust +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.2 (магнитуда 1.68/1.68): ShipMaximumMovement +1.7 @Empire/GarrisonFleet  [Mobility]; PlanetRawDust +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.3 (магнитуда 2.52/2.52): ShipMaximumMovement +2.5 @Empire/GarrisonFleet  [Mobility]; PlanetRawDust +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
- **Порог 12 очк.**
  - **`HRDictHoratioS4`** «Имперская навигация / Дипломатический корпус» / Imperial Navigation / Diplomatic Corps  [Mobility+Diplomacy]
    - ур.1 (магнитуда 1.12/1.12): ShipMaximumMovement +1.1 @Empire/GarrisonFleet  [Mobility]; PeacefulTreatyCostReduction -0.56 @Empire  [Diplomacy]; AggressiveTreatyCostReduction -0.56 @Empire  [Diplomacy]
    - ур.2 (магнитуда 2.24/2.24): ShipMaximumMovement +2.2 @Empire/GarrisonFleet  [Mobility]; PeacefulTreatyCostReduction -1.1 @Empire  [Diplomacy]; AggressiveTreatyCostReduction -1.1 @Empire  [Diplomacy]
    - ур.3 (магнитуда 3.36/3.36): ShipMaximumMovement +3.4 @Empire/GarrisonFleet  [Mobility]; PeacefulTreatyCostReduction -1.7 @Empire  [Diplomacy]; AggressiveTreatyCostReduction -1.7 @Empire  [Diplomacy]
  - **`HRDictHoratioS5`** «Казначейство / Дипломатический корпус» / Treasury / Diplomatic Corps  [Money+Diplomacy]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawDust +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; PeacefulTreatyCostReduction -0.56 @Empire  [Diplomacy]; AggressiveTreatyCostReduction -0.56 @Empire  [Diplomacy]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawDust +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; PeacefulTreatyCostReduction -1.1 @Empire  [Diplomacy]; AggressiveTreatyCostReduction -1.1 @Empire  [Diplomacy]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawDust +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; PeacefulTreatyCostReduction -1.7 @Empire  [Diplomacy]; AggressiveTreatyCostReduction -1.7 @Empire  [Diplomacy]
- **Порог 18 очк.**
  - **`HRDictHoratioS6`** «Вершина: Казначейство» / Pinnacle: Treasury  [Money+Mobility+Diplomacy+Happiness+Manpower]
    - ур.1 (магнитуда 2.7/2.7/2.7/2.7/2.7): ShipMaximumMovement +2.7 @Empire/GarrisonFleet  [Mobility]; PlanetRawDust +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; Happiness +14 @Empire/ColonizedStarSystem  [Happiness]; MaximumEmpireManpowerStock +67.5% @Empire  [Manpower]; PeacefulTreatyCostReduction -1.4 @Empire  [Diplomacy]; AggressiveTreatyCostReduction -1.4 @Empire  [Diplomacy]

### Инсектоиды / Insectoids (`HRTreeDictInsectoids`) - тема: Growth, Manpower, Sustain

##### `HRTreeDictInsectoids` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRDictInsectoidsS0`** «Аграрная реформа» / Agrarian Reform  [Growth]
    - ур.1 (магнитуда 0.8): PlanetRawFood +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.2 (магнитуда 1.6): PlanetRawFood +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.3 (магнитуда 2.4): PlanetRawFood +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
  - **`HRDictInsectoidsS1`** «Всеобщая мобилизация» / National Mobilization  [Manpower]
    - ур.1 (магнитуда 0.8): MaximumEmpireManpowerStock +20% @Empire  [Manpower]
    - ур.2 (магнитуда 1.6): MaximumEmpireManpowerStock +40% @Empire  [Manpower]
    - ур.3 (магнитуда 2.4): MaximumEmpireManpowerStock +60% @Empire  [Manpower]
- **Порог 6 очк.**
  - **`HRDictInsectoidsS2`** «Службы ремонта» / Repair Corps  [Sustain]
    - ур.1 (магнитуда 1.2): HealthRegenerationRatio +0.06 @Empire/GarrisonFleet  [Sustain]
    - ур.2 (магнитуда 2.4): HealthRegenerationRatio +0.12 @Empire/GarrisonFleet  [Sustain]
    - ур.3 (магнитуда 3.6): HealthRegenerationRatio +0.18 @Empire/GarrisonFleet  [Sustain]
  - **`HRDictInsectoidsS3`** «Аграрная реформа / Всеобщая мобилизация» / Agrarian Reform / National Mobilization  [Growth+Manpower]
    - ур.1 (магнитуда 0.84/0.84): PlanetRawFood +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; MaximumEmpireManpowerStock +21% @Empire  [Manpower]
    - ур.2 (магнитуда 1.68/1.68): PlanetRawFood +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; MaximumEmpireManpowerStock +42% @Empire  [Manpower]
    - ур.3 (магнитуда 2.52/2.52): PlanetRawFood +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; MaximumEmpireManpowerStock +63% @Empire  [Manpower]
- **Порог 12 очк.**
  - **`HRDictInsectoidsS4`** «Всеобщая мобилизация / Службы ремонта» / National Mobilization / Repair Corps  [Manpower+Sustain]
    - ур.1 (магнитуда 1.12/1.12): HealthRegenerationRatio +0.056 @Empire/GarrisonFleet  [Sustain]; MaximumEmpireManpowerStock +28% @Empire  [Manpower]
    - ур.2 (магнитуда 2.24/2.24): HealthRegenerationRatio +0.112 @Empire/GarrisonFleet  [Sustain]; MaximumEmpireManpowerStock +56% @Empire  [Manpower]
    - ур.3 (магнитуда 3.36/3.36): HealthRegenerationRatio +0.168 @Empire/GarrisonFleet  [Sustain]; MaximumEmpireManpowerStock +84% @Empire  [Manpower]
  - **`HRDictInsectoidsS5`** «Аграрная реформа / Службы ремонта» / Agrarian Reform / Repair Corps  [Growth+Sustain]
    - ур.1 (магнитуда 1.12/1.12): HealthRegenerationRatio +0.056 @Empire/GarrisonFleet  [Sustain]; PlanetRawFood +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.2 (магнитуда 2.24/2.24): HealthRegenerationRatio +0.112 @Empire/GarrisonFleet  [Sustain]; PlanetRawFood +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.3 (магнитуда 3.36/3.36): HealthRegenerationRatio +0.168 @Empire/GarrisonFleet  [Sustain]; PlanetRawFood +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
- **Порог 18 очк.**
  - **`HRDictInsectoidsS6`** «Вершина: Аграрная реформа» / Pinnacle: Agrarian Reform  [Growth+Manpower+Sustain+Happiness+Manpower]
    - ур.1 (магнитуда 2.7/2.7/2.7/2.7/2.7): HealthRegenerationRatio +0.135 @Empire/GarrisonFleet  [Sustain]; PlanetRawFood +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; Happiness +14 @Empire/ColonizedStarSystem  [Happiness]; MaximumEmpireManpowerStock +140% @Empire  [Manpower]

### Пираты / Pirates (`HRTreeDictPirates`) - тема: Offense, Money, Mobility

##### `HRTreeDictPirates` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRDictPiratesS0`** «Военные ассигнования» / Military Appropriations  [Offense]
    - ур.1 (магнитуда 0.8): ShipDamage +0.08 @Empire/GarrisonFleet  [Offense]
    - ур.2 (магнитуда 1.6): ShipDamage +0.16 @Empire/GarrisonFleet  [Offense]
    - ур.3 (магнитуда 2.4): ShipDamage +0.24 @Empire/GarrisonFleet  [Offense]
  - **`HRDictPiratesS1`** «Казначейство» / Treasury  [Money]
    - ур.1 (магнитуда 0.8): PlanetRawDust +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.2 (магнитуда 1.6): PlanetRawDust +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.3 (магнитуда 2.4): PlanetRawDust +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
- **Порог 6 очк.**
  - **`HRDictPiratesS2`** «Имперская навигация» / Imperial Navigation  [Mobility]
    - ур.1 (магнитуда 1.2): ShipMaximumMovement +1.2 @Empire/GarrisonFleet  [Mobility]
    - ур.2 (магнитуда 2.4): ShipMaximumMovement +2.4 @Empire/GarrisonFleet  [Mobility]
    - ур.3 (магнитуда 3.6): ShipMaximumMovement +3.6 @Empire/GarrisonFleet  [Mobility]
  - **`HRDictPiratesS3`** «Военные ассигнования / Казначейство» / Military Appropriations / Treasury  [Offense+Money]
    - ур.1 (магнитуда 0.84/0.84): ShipDamage +0.084 @Empire/GarrisonFleet  [Offense]; PlanetRawDust +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.2 (магнитуда 1.68/1.68): ShipDamage +0.168 @Empire/GarrisonFleet  [Offense]; PlanetRawDust +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.3 (магнитуда 2.52/2.52): ShipDamage +0.252 @Empire/GarrisonFleet  [Offense]; PlanetRawDust +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
- **Порог 12 очк.**
  - **`HRDictPiratesS4`** «Казначейство / Имперская навигация» / Treasury / Imperial Navigation  [Money+Mobility]
    - ур.1 (магнитуда 1.12/1.12): ShipMaximumMovement +1.1 @Empire/GarrisonFleet  [Mobility]; PlanetRawDust +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.2 (магнитуда 2.24/2.24): ShipMaximumMovement +2.2 @Empire/GarrisonFleet  [Mobility]; PlanetRawDust +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - ур.3 (магнитуда 3.36/3.36): ShipMaximumMovement +3.4 @Empire/GarrisonFleet  [Mobility]; PlanetRawDust +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
  - **`HRDictPiratesS5`** «Военные ассигнования / Имперская навигация» / Military Appropriations / Imperial Navigation  [Offense+Mobility]
    - ур.1 (магнитуда 1.12/1.12): ShipDamage +0.112 @Empire/GarrisonFleet  [Offense]; ShipMaximumMovement +1.1 @Empire/GarrisonFleet  [Mobility]
    - ур.2 (магнитуда 2.24/2.24): ShipDamage +0.224 @Empire/GarrisonFleet  [Offense]; ShipMaximumMovement +2.2 @Empire/GarrisonFleet  [Mobility]
    - ур.3 (магнитуда 3.36/3.36): ShipDamage +0.336 @Empire/GarrisonFleet  [Offense]; ShipMaximumMovement +3.4 @Empire/GarrisonFleet  [Mobility]
- **Порог 18 очк.**
  - **`HRDictPiratesS6`** «Вершина: Военные ассигнования» / Pinnacle: Military Appropriations  [Offense+Money+Mobility+Happiness+Manpower]
    - ур.1 (магнитуда 2.7/2.7/2.7/2.7/2.7): ShipDamage +0.27 @Empire/GarrisonFleet  [Offense]; ShipMaximumMovement +2.7 @Empire/GarrisonFleet  [Mobility]; PlanetRawDust +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]; Happiness +14 @Empire/ColonizedStarSystem  [Happiness]; MaximumEmpireManpowerStock +67.5% @Empire  [Manpower]

### Примитивы / Primitives (`HRTreeDictPrimitives`) - тема: Deposit, Growth, Defense

##### `HRTreeDictPrimitives` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRDictPrimitivesS0`** «Ресурсная политика» / Resource Policy  [Deposit]
    - ур.1 (магнитуда 0.8): DepositValue +0.4 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - ур.2 (магнитуда 1.6): DepositValue +0.8 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - ур.3 (магнитуда 2.4): DepositValue +1.2 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
  - **`HRDictPrimitivesS1`** «Аграрная реформа» / Agrarian Reform  [Growth]
    - ур.1 (магнитуда 0.8): PlanetRawFood +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.2 (магнитуда 1.6): PlanetRawFood +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.3 (магнитуда 2.4): PlanetRawFood +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
- **Порог 6 очк.**
  - **`HRDictPrimitivesS2`** «Бронирование флотов» / Fleet Armor Programs  [Defense]
    - ур.1 (магнитуда 1.2): FleetMaximumHealth +0.12 @Empire/GarrisonFleet  [Defense]
    - ур.2 (магнитуда 2.4): FleetMaximumHealth +0.24 @Empire/GarrisonFleet  [Defense]
    - ур.3 (магнитуда 3.6): FleetMaximumHealth +0.36 @Empire/GarrisonFleet  [Defense]
  - **`HRDictPrimitivesS3`** «Ресурсная политика / Аграрная реформа» / Resource Policy / Agrarian Reform  [Deposit+Growth]
    - ур.1 (магнитуда 0.84/0.84): PlanetRawFood +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; DepositValue +0.42 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - ур.2 (магнитуда 1.68/1.68): PlanetRawFood +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; DepositValue +0.84 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - ур.3 (магнитуда 2.52/2.52): PlanetRawFood +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; DepositValue +1.3 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
- **Порог 12 очк.**
  - **`HRDictPrimitivesS4`** «Аграрная реформа / Бронирование флотов» / Agrarian Reform / Fleet Armor Programs  [Growth+Defense]
    - ур.1 (магнитуда 1.12/1.12): FleetMaximumHealth +0.112 @Empire/GarrisonFleet  [Defense]; PlanetRawFood +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.2 (магнитуда 2.24/2.24): FleetMaximumHealth +0.224 @Empire/GarrisonFleet  [Defense]; PlanetRawFood +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - ур.3 (магнитуда 3.36/3.36): FleetMaximumHealth +0.336 @Empire/GarrisonFleet  [Defense]; PlanetRawFood +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
  - **`HRDictPrimitivesS5`** «Ресурсная политика / Бронирование флотов» / Resource Policy / Fleet Armor Programs  [Deposit+Defense]
    - ур.1 (магнитуда 1.12/1.12): FleetMaximumHealth +0.112 @Empire/GarrisonFleet  [Defense]; DepositValue +0.56 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - ур.2 (магнитуда 2.24/2.24): FleetMaximumHealth +0.224 @Empire/GarrisonFleet  [Defense]; DepositValue +1.1 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - ур.3 (магнитуда 3.36/3.36): FleetMaximumHealth +0.336 @Empire/GarrisonFleet  [Defense]; DepositValue +1.7 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
- **Порог 18 очк.**
  - **`HRDictPrimitivesS6`** «Вершина: Ресурсная политика» / Pinnacle: Resource Policy  [Deposit+Growth+Defense+Happiness+Manpower]
    - ур.1 (магнитуда 2.7/2.7/2.7/2.7/2.7): FleetMaximumHealth +0.27 @Empire/GarrisonFleet  [Defense]; PlanetRawFood +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; Happiness +14 @Empire/ColonizedStarSystem  [Happiness]; MaximumEmpireManpowerStock +67.5% @Empire  [Manpower]; DepositValue +1.4 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]

### Продвинутые / Advanced (`HRTreeDictAdvanced`) - тема: Science, Production, Experience

##### `HRTreeDictAdvanced` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRDictAdvancedS0`** «Научный комитет» / Science Committee  [Science]
    - ур.1 (магнитуда 0.8): PlanetRawScience +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.2 (магнитуда 1.6): PlanetRawScience +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.3 (магнитуда 2.4): PlanetRawScience +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
  - **`HRDictAdvancedS1`** «Государственная индустрия» / State Industry  [Production]
    - ур.1 (магнитуда 0.8): PlanetRawIndustry +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - ур.2 (магнитуда 1.6): PlanetRawIndustry +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - ур.3 (магнитуда 2.4): PlanetRawIndustry +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
- **Порог 6 очк.**
  - **`HRDictAdvancedS2`** «Образовательная реформа» / Education Reform  [Experience]
    - ур.1 (магнитуда 1.2): SystemResearch +12% @Empire/ColonizedStarSystem  [Experience]
    - ур.2 (магнитуда 2.4): SystemResearch +24% @Empire/ColonizedStarSystem  [Experience]
    - ур.3 (магнитуда 3.6): SystemResearch +36% @Empire/ColonizedStarSystem  [Experience]
  - **`HRDictAdvancedS3`** «Научный комитет / Государственная индустрия» / Science Committee / State Industry  [Science+Production]
    - ур.1 (магнитуда 0.84/0.84): PlanetRawIndustry +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawScience +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.2 (магнитуда 1.68/1.68): PlanetRawIndustry +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawScience +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.3 (магнитуда 2.52/2.52): PlanetRawIndustry +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawScience +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
- **Порог 12 очк.**
  - **`HRDictAdvancedS4`** «Государственная индустрия / Образовательная реформа» / State Industry / Education Reform  [Production+Experience]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawIndustry +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; SystemResearch +11.2% @Empire/ColonizedStarSystem  [Experience]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawIndustry +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; SystemResearch +22.4% @Empire/ColonizedStarSystem  [Experience]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawIndustry +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; SystemResearch +33.6% @Empire/ColonizedStarSystem  [Experience]
  - **`HRDictAdvancedS5`** «Научный комитет / Образовательная реформа» / Science Committee / Education Reform  [Science+Experience]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawScience +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; SystemResearch +11.2% @Empire/ColonizedStarSystem  [Experience]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawScience +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; SystemResearch +22.4% @Empire/ColonizedStarSystem  [Experience]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawScience +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; SystemResearch +33.6% @Empire/ColonizedStarSystem  [Experience]
- **Порог 18 очк.**
  - **`HRDictAdvancedS6`** «Вершина: Научный комитет» / Pinnacle: Science Committee  [Science+Production+Experience+Happiness+Manpower]
    - ур.1 (магнитуда 2.7/2.7/2.7/2.7/2.7): PlanetRawIndustry +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawScience +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; Happiness +14 @Empire/ColonizedStarSystem  [Happiness]; MaximumEmpireManpowerStock +67.5% @Empire  [Manpower]; SystemResearch +27% @Empire/ColonizedStarSystem  [Experience]

### Фракция / Faction (`HRTreeDictOther`) - тема: Production, Science, Influence

##### `HRTreeDictOther` - роль диктатор, канал Senate, мощность x1.6
- **Порог 0 очк.**
  - **`HRDictOtherS0`** «Государственная индустрия» / State Industry  [Production]
    - ур.1 (магнитуда 0.8): PlanetRawIndustry +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - ур.2 (магнитуда 1.6): PlanetRawIndustry +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - ур.3 (магнитуда 2.4): PlanetRawIndustry +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
  - **`HRDictOtherS1`** «Научный комитет» / Science Committee  [Science]
    - ур.1 (магнитуда 0.8): PlanetRawScience +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.2 (магнитуда 1.6): PlanetRawScience +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.3 (магнитуда 2.4): PlanetRawScience +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
- **Порог 6 очк.**
  - **`HRDictOtherS2`** «Имперская идеология» / Imperial Ideology  [Influence]
    - ур.1 (магнитуда 1.2): PlanetRawPrestige +1.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 2.4): PlanetRawPrestige +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 3.6): PlanetRawPrestige +3.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
  - **`HRDictOtherS3`** «Государственная индустрия / Научный комитет» / State Industry / Science Committee  [Production+Science]
    - ур.1 (магнитуда 0.84/0.84): PlanetRawIndustry +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawScience +0.84 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.2 (магнитуда 1.68/1.68): PlanetRawIndustry +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawScience +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - ур.3 (магнитуда 2.52/2.52): PlanetRawIndustry +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawScience +2.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
- **Порог 12 очк.**
  - **`HRDictOtherS4`** «Научный комитет / Имперская идеология» / Science Committee / Imperial Ideology  [Science+Influence]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawScience +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawScience +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawScience +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
  - **`HRDictOtherS5`** «Государственная индустрия / Имперская идеология» / State Industry / Imperial Ideology  [Production+Influence]
    - ур.1 (магнитуда 1.12/1.12): PlanetRawIndustry +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawPrestige +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.2 (магнитуда 2.24/2.24): PlanetRawIndustry +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawPrestige +2.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - ур.3 (магнитуда 3.36/3.36): PlanetRawIndustry +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawPrestige +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Порог 18 очк.**
  - **`HRDictOtherS6`** «Вершина: Государственная индустрия» / Pinnacle: State Industry  [Production+Science+Influence+Happiness+Manpower]
    - ур.1 (магнитуда 2.7/2.7/2.7/2.7/2.7): PlanetRawIndustry +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; PlanetRawScience +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]; PlanetRawPrestige +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]; Happiness +14 @Empire/ColonizedStarSystem  [Happiness]; MaximumEmpireManpowerStock +67.5% @Empire  [Manpower]

