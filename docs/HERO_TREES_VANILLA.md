# Деревья навыков героев - ванильные данные Endless Space 2 и их роли в моде

Сгенерировано `tools/Dump/HeroTreesDump.cs` из XML ES2 (репозиторий Endless-Studio, более старая версия игры - в вашей сборке цифры могут отличаться).
Названия и описания - из русской локализации. Эффекты - технические имена свойств игры: `Prop +N`, `Prop +N%`, `Prop xN`, `@Путь` - к чему применяется.
Роль героя в моде: A - адмирал, G - губернатор, S - сенатор, D - диктатор. «kept» - ванильный эффект своего канала сохранён, «converted» - эффект чужого канала пересчитан в эффект роли (по таблице флейворов `HeroSkillData.cs`).
Порог этапа в очках навыков: ваниль 0/4/8/12; адмирал и губернатор так же; сенатор /2 (0/2/4/6); диктатор x1.5 (0/6/12/18). У сенатора один уровень навыка (сильный), у диктатора три (если у ванильного два).

Какие деревья получает герой (мод): общее дерево («Universal») заменено деревом партии (`HRTreeParty0N@код`, см. HERO_TREES_MOD.md); расовое и классовое деревья - те же слоты, но с навыками роли; диктатор вместо классового дерева получает дерево фракции (`HRTreeDict<Фракция>`).

## 1. Герои (63)

| Герой | Имя | Класс -> роль | Фракция (аффинити) | Политика -> партия | Деревья | Личные навыки | Скрыт |
|---|---|---|---|---|---|---|---|
| `HeroSophons01` | Фахеммн Натхарр / Fahemmn Nutharr | Administrator -> Губернатор | Sophons (Sophons) | 01 -> 1 Индустриалисты | Generic, Sophons, Administrator | `HeroSkill01Sophons01`, `HeroSkill02Sophons01` |  |
| `HeroSophons02` | Ханмхарла Джерзи / Hanmharla Gerzi | Adventurer -> Адмирал | Sophons (Sophons) | 05 -> 5 Религиозные | Generic, Sophons, Adventurer | `HeroSkill01Sophons02`, `HeroSkill02Sophons02` |  |
| `HeroSophons03` | Атхассам Рамоэзз / Uthassum Rhamoezz | Corporate -> Сенатор | Sophons (Sophons) | 03 -> 3 Пацифисты | Generic, Sophons, Corporate | `HeroSkill01Sophons03`, `HeroSkill02Sophons03` |  |
| `HeroSophons04` | Асароэнни Дзулмаран / Asaroenni Dzulmaran | Corporate -> Сенатор | Sophons (Sophons) | 02 -> 2 Учёные | Generic, Sophons, Corporate | `HeroSkill01Sophons04`, `HeroSkill02Sophons04` |  |
| `HeroSophons05` | Ариринн Турлеммоз / Ahririnn Turlemmoz | Corporate -> Сенатор | Sophons (Sophons) | 03 -> 3 Пацифисты | Generic, Sophons, Corporate | `HeroSkill01Sophons05`, `HeroSkill02Sophons05` |  |
| `HeroTerrans01` | Имма Наратья / Immah Naratya | Administrator -> Губернатор | Terrans (Terrans) | 04 -> 4 Экологи | Generic, Terrans, Administrator | `HeroSkill01Terrans01`, `HeroSkill02Terrans01` |  |
| `HeroTerrans02` | Доминик Хаванен / Dominik Havanen | Administrator -> Губернатор | Terrans (Terrans) | 06 -> 6 Милитаристы | Generic, Terrans, Administrator | `HeroSkill01Terrans02`, `HeroSkill02Terrans02` |  |
| `HeroTerrans03` | Олег Лагарин / Oleg Lagarin | Admiral -> Адмирал | Terrans (Terrans) | 02 -> 2 Учёные | Generic, Terrans, Admiral | `HeroSkill01Terrans03`, `HeroSkill02Terrans03` |  |
| `HeroTerrans04` | Дмитрий Ленко / Dmitri Lenko | Corporate -> Сенатор | Terrans (Terrans) | 01 -> 1 Индустриалисты | Generic, Terrans, Corporate | `HeroSkill01Terrans04`, `HeroSkill02Terrans04` |  |
| `HeroTerrans05` | Кинет Малду / Kinete Muldaur | Admiral -> Адмирал | Terrans (Terrans) | 05 -> 5 Религиозные | Generic, Terrans, Admiral | `HeroSkill01Terrans05`, `HeroSkill02Terrans05` |  |
| `HeroHoratio01` | Фарелла -ЕМ8 Марио 22 / Farella -EM8 Mario 22 | Administrator -> Губернатор | Horatio (Horatio) | 04 -> 4 Экологи | Generic, Horatio, Administrator | `HeroSkill01Horatio01`, `HeroSkill02Horatio01` |  |
| `HeroHoratio02` | Андро Камилл -Зет / Andro Camille -Z | Adventurer -> Адмирал | Horatio (Horatio) | 02 -> 2 Учёные | Generic, Horatio, Adventurer | `HeroSkill01Horatio02`, `HeroSkill02Horatio02` |  |
| `HeroHoratio03` | Саларио Домус -63 / Salario Domus -63 | Corporate -> Сенатор | Horatio (Horatio) | 06 -> 6 Милитаристы | Generic, Horatio, Corporate | `HeroSkill01Horatio03`, `HeroSkill02Horatio03` |  |
| `HeroHoratio04` | Рутиан -Ф11 Тигина / Ruthian -F11 Tighina | Corporate -> Сенатор | Horatio (Horatio) | 05 -> 5 Религиозные | Generic, Horatio, Corporate | `HeroSkill01Horatio04`, `HeroSkill02Horatio04` |  |
| `HeroTimeLords01` | е / e | Administrator -> Губернатор | TimeLords (TimeLords) | 01 -> 1 Индустриалисты | Generic, TimeLords, Administrator | `HeroSkill01TimeLords01`, `HeroSkill02TimeLords01` |  |
| `HeroTimeLords02` | pi | Administrator -> Губернатор | TimeLords (TimeLords) | 03 -> 3 Пацифисты | Generic, TimeLords, Administrator | `HeroSkill01TimeLords02`, `HeroSkill02TimeLords02` |  |
| `HeroTimeLords03` | sum | Admiral -> Адмирал | TimeLords (TimeLords) | 06 -> 6 Милитаристы | Generic, TimeLords, Admiral | `HeroSkill01TimeLords03`, `HeroSkill02TimeLords03` |  |
| `HeroTimeLords04` | derivate | Corporate -> Сенатор | TimeLords (TimeLords) | 05 -> 5 Религиозные | Generic, TimeLords, Corporate | `HeroSkill01TimeLords04`, `HeroSkill02TimeLords04` |  |
| `HeroUnfallen01` | Барамакса / Baramaxa | Administrator -> Губернатор | Unfallen (Unfallen) | 02 -> 2 Учёные | Generic, Unfallen, Administrator | `HeroSkill01Unfallen01`, `HeroSkill02Unfallen01` |  |
| `HeroUnfallen02` | Лунабарксес / Lunabarxes | Adventurer -> Адмирал | Unfallen (Unfallen) | 03 -> 3 Пацифисты | Generic, Unfallen, Adventurer | `HeroSkill01Unfallen02`, `HeroSkill02Unfallen02` |  |
| `HeroUnfallen03` | Састреллен / Thustrellen | Adventurer -> Адмирал | Unfallen (Unfallen) | 01 -> 1 Индустриалисты | Generic, Unfallen, Adventurer | `HeroSkill01Unfallen03`, `HeroSkill02Unfallen03` |  |
| `HeroUnfallen04` | Анаксагор / Anaxagoras | Admiral -> Адмирал | Unfallen (Unfallen) | 06 -> 6 Милитаристы | Generic, Unfallen, Admiral | `HeroSkill01Unfallen04`, `HeroSkill02Unfallen04` |  |
| `HeroCravers01` | Малк-Сорвиголова / Malk Unbound | Admiral -> Адмирал | Cravers (Cravers) | 05 -> 5 Религиозные | Generic, Cravers, Admiral | `HeroSkill01Cravers01`, `HeroSkill02Cravers01` |  |
| `HeroCravers02` | Ристо Либера / Risto Libera | Administrator -> Губернатор | Cravers (Cravers) | 01 -> 1 Индустриалисты | Generic, Cravers, Administrator | `HeroSkill01Cravers02`, `HeroSkill02Cravers02` |  |
| `HeroCravers03` | Крен Солутус / Kren Solutus | Admiral -> Адмирал | Cravers (Cravers) | 06 -> 6 Милитаристы | Generic, Cravers, Admiral | `HeroSkill01Cravers03`, `HeroSkill02Cravers03` |  |
| `HeroCravers04` | Оройо Нер Фрид / Oroyo Ner Freed | Adventurer -> Адмирал | Cravers (Cravers) | 03 -> 3 Пацифисты | Generic, Cravers, Adventurer | `HeroSkill01Cravers04`, `HeroSkill02Cravers04` |  |
| `HeroCraversPrime` | Корос Апогей / Koros Apogee | Administrator -> Губернатор | Cravers (Cravers) | 01 -> 1 Индустриалисты | Generic, Cravers, Administrator | `HeroSkill01CraversPrime`, `HeroSkill02CraversPrime` |  |
| `HeroVenetians01` | Иотар Вос-Ланселлум / Iotar Vos-Lancellum | Admiral -> Адмирал | Venetians (Venetians) | 04 -> 4 Экологи | Generic, Venetians, Admiral | `HeroSkill01Venetians01`, `HeroSkill02Venetians01` |  |
| `HeroVenetians02` | Колстя Вельм-Омокар / Kolstya Velm-Omokar | Adventurer -> Адмирал | Venetians (Venetians) | 02 -> 2 Учёные | Generic, Venetians, Adventurer | `HeroSkill01Venetians02`, `HeroSkill02Venetians02` |  |
| `HeroVenetians03` | Елчин Ордара-Арракьо / Yelchin Ordara-Arrakyo | Adventurer -> Адмирал | Venetians (Venetians) | 03 -> 3 Пацифисты | Generic, Venetians, Adventurer | `HeroSkill01Venetians03`, `HeroSkill02Venetians03` |  |
| `HeroVenetians04` | Дженестра Древ-Меос / Jenhaestra Drev-Meos | Corporate -> Сенатор | Venetians (Venetians) | 01 -> 1 Индустриалисты | Generic, Venetians, Corporate | `HeroSkill01Venetians04`, `HeroSkill02Venetians04` |  |
| `HeroVenetians05` | Брунем Берто'Ланселлум / Brunem Berto'Lancellum | Admiral -> Адмирал | Venetians (Venetians) | 05 -> 5 Религиозные | Generic, Venetians, Admiral | `HeroSkill01Venetians05`, `HeroSkill02Venetians05` |  |
| `HeroVampirilis01` | Энав Св. Льеаор / Enav St Lyeaor | Admiral -> Адмирал | Vampirilis (Vampirilis) | 04 -> 4 Экологи | Generic, Vampirilis, Admiral | `HeroSkill01Vampirilis01`, `HeroSkill02Vampirilis01` |  |
| `HeroVampirilis02` | Варб Св. Зуйейна / Varb St Zouieina | Admiral -> Адмирал | Vampirilis (Vampirilis) | 05 -> 5 Религиозные | Generic, Vampirilis, Admiral | `HeroSkill01Vampirilis02`, `HeroSkill02Vampirilis02` |  |
| `HeroVampirilis03` | Клев Св. Сейотия / Klev St Seiotia | Adventurer -> Адмирал | Vampirilis (Vampirilis) | 02 -> 2 Учёные | Generic, Vampirilis, Adventurer | `HeroSkill01Vampirilis03`, `HeroSkill02Vampirilis03` |  |
| `HeroVampirilis04` | Даро Св. Тайадж / Daro St Taiage | Corporate -> Сенатор | Vampirilis (Vampirilis) | 06 -> 6 Милитаристы | Generic, Vampirilis, Corporate | `HeroSkill01Vampirilis04`, `HeroSkill02Vampirilis04` |  |
| `HeroAmoeba01` | Куратор Ай 1.2 Гадо / Ay Seer 1.2Gado | Corporate -> Сенатор | Insectoids (Insectoids) | 04 -> 4 Экологи | Generic, MinorFaction02, Corporate | `HeroSkill01Amoeba01`, `HeroSkill02Amoeba01` |  |
| `HeroBenthys01` | Нелли "Движуха" Сира Нинпере / Nelly "Shaken" Sira Ninpere | Administrator -> Губернатор | Primitives (Primitives) | 05 -> 5 Религиозные | Generic, MinorFaction04, Administrator | `HeroSkill01Benthys01`, `HeroSkill02Benthys01` |  |
| `HeroBots01` | ЭПИСТИС "Мейдин" 20910 / EPISTIS "Madein" 20910 | Adventurer -> Адмирал | Advanced (Advanced) | 04 -> 4 Экологи | Generic, MinorFaction03, Adventurer | `HeroSkill01Bots01`, `HeroSkill02Bots01` |  |
| `HeroDeuyivans01` | Тинг Нейль / Ting Neuil | Corporate -> Сенатор | Insectoids (Insectoids) | 02 -> 2 Учёные | Generic, MinorFaction02, Corporate | `HeroSkill01Deuyivans01`, `HeroSkill02Deuyivans01` |  |
| `HeroHaroshems01` | Вуд Свез / Wud Svez | Administrator -> Губернатор | Pirates (Pirates) | 03 -> 3 Пацифисты | Generic, MinorFaction01, Administrator | `HeroSkill01Haroshems01`, `HeroSkill02Haroshems01` |  |
| `HeroHisshos01` | Фуша Кинкрест / Fusha Keencrest | Admiral -> Адмирал | Pirates (Pirates) | 06 -> 6 Милитаристы | Generic, MinorFaction01, Admiral | `HeroSkill01Hisshos01`, `HeroSkill02Hisshos01` |  |
| `HeroMavros01` | Крак "Кубический" Блейс / Krak "Cubed" Bleis | Admiral -> Адмирал | Advanced (Advanced) | 01 -> 1 Индустриалисты | Generic, MinorFaction03, Admiral | `HeroSkill01Mavros01`, `HeroSkill02Mavros01` |  |
| `HeroPilgrims01` | Мванга Мото / Mwanga Moto | Admiral -> Адмирал | Advanced (Advanced) | 05 -> 5 Религиозные | Generic, MinorFaction03, Admiral | `HeroSkill01Pilgrims01`, `HeroSkill02Pilgrims01` |  |
| `HeroZvali01` | Норн Д'Гирит / Norn D'Girit | Administrator -> Губернатор | Advanced (Advanced) | 02 -> 2 Учёные | Generic, MinorFaction03, Administrator | `HeroSkill01Zvali01`, `HeroSkill02Zvali01` |  |
| `HeroTikanans01` | Восьмая из Пятнадцатых Карвала / Eighth of Fifteenth of Karval | Admiral -> Адмирал | Insectoids (Insectoids) | 06 -> 6 Милитаристы | Generic, MinorFaction02, Admiral | `HeroSkill01Tikanans01`, `HeroSkill02Tikanans01` |  |
| `HeroGnashasts01` | Торвот Бзеч'Грул / Torvoth Bzech'Grul | Adventurer -> Адмирал | Primitives (Primitives) | 06 -> 6 Милитаристы | Generic, MinorFaction04, Adventurer | `HeroSkill01Gnashasts01`, `HeroSkill02Gnashasts01` |  |
| `HeroEyders01` | Скейлс Галена / Scales Galena | Corporate -> Сенатор | Pirates (Pirates) | 05 -> 5 Религиозные | Generic, MinorFaction01, Corporate | `HeroSkill01Eyders01`, `HeroSkill02Eyders01` |  |
| `HeroNiris01` | Эли Леандра / Eli Leandra | Administrator -> Губернатор | Primitives (Primitives) | 04 -> 4 Экологи | Generic, MinorFaction04, Administrator | `HeroSkill01Niris01`, `HeroSkill02Niris01` |  |
| `HeroBhagabas01` | 5748-А27П-ТЗР / 5748-A27P-TZR | Corporate -> Сенатор | Primitives (Primitives) | 03 -> 3 Пацифисты | Generic, MinorFaction04, Corporate | `HeroSkill01Bhagabas01`, `HeroSkill02Bhagabas01` |  |
| `HeroRemnant01` | Калин Фетт-Иска / Kalin Fett-Iska | Adventurer -> Адмирал | Insectoids (Insectoids) | 01 -> 1 Индустриалисты | Generic, MinorFaction02, Adventurer | `HeroSkill01Remnant01`, `HeroSkill02Remnant01` |  |
| `HeroPulsos01` | Каксалксиксокс Друз / Caxalxsixox Druse | Adventurer -> Адмирал | Pirates (Pirates) | 01 -> 1 Индустриалисты | Generic, MinorFaction01, Adventurer | `HeroSkill01Pulsos01`, `HeroSkill02Pulsos01` |  |
| `QuestHeroEndless01` | Реярин Сайус / Reyaryn Cious | Admiral -> Адмирал | Endless (Endless) | 05 -> 5 Религиозные | Generic, Endless, Admiral | `HeroSkill01QuestEndless01`, `HeroSkill02QuestEndless01` | да |
| `QuestHeroEndless02` | Тиах Жиллек / Tiaych Zhilleaq | Adventurer -> Адмирал | Endless (Endless) | 04 -> 4 Экологи | Generic, Endless, Adventurer | `HeroSkill01QuestEndless02`, `HeroSkill02QuestEndless02` | да |
| `HeroTerransQuest01` | Хадри Ленко / Hadri Lenko | Admiral -> Адмирал | Terrans (Terrans) | 06 -> 6 Милитаристы | Generic, Terrans, Admiral | `HeroSkill01TerransQuest01`, `HeroSkill02TerransQuest01` | да |
| `HeroTerransQuest02` | Лена Зелевас / Lena Zelevas | Corporate -> Сенатор | Terrans (Terrans) | 05 -> 5 Религиозные | Generic, Terrans, Corporate | `HeroSkill01TerransQuest02`, `HeroSkill02TerransQuest02` | да |
| `HeroTerransQuest03` | Петра Мандзукич / Petra Mandzukic | Adventurer -> Адмирал | Terrans (Terrans) | 02 -> 2 Учёные | Generic, Terrans, Adventurer | `HeroSkill01TerransQuest03`, `HeroSkill02TerransQuest03` | да |
| `HeroQuestHoratio04` | Пятый / Five | Admiral -> Адмирал | Horatio (Horatio) | 06 -> 6 Милитаристы | Generic, Horatio, Admiral | `HeroSkill01HoratioQuest04`, `HeroSkill02HoratioQuest04` | да |
| `HeroQuestVirus01` | Анакс-Джинк 2077 / Anax-Jink 2077 | Adventurer -> Адмирал | Pirates (Pirates) | 05 -> 5 Религиозные | Generic, MinorFaction01, Adventurer | `HeroSkill01QuestVirus01`, `HeroSkill02QuestVirus01` | да |
| `HeroPirate01` |  | Adventurer -> Адмирал | Pirates (Pirates) | 01 -> 1 Индустриалисты | Generic, MinorFaction01, Adventurer | `HeroSkill01Pirate01`, `HeroSkill02Pirate01` |  |
| `HeroGreenman01` |  | Corporate -> Сенатор | Primitives (Primitives) | 02 -> 2 Учёные | Generic, MinorFaction04, Corporate | `HeroSkill01Greenman01`, `HeroSkill02Greenman01` | да |
| `HeroGreenman02` |  | Admiral -> Адмирал | Primitives (Primitives) | 04 -> 4 Экологи | Generic, MinorFaction04, Admiral | `HeroSkill01Greenman02`, `HeroSkill02Greenman02` | да |
| `HeroAmoeba02` | Глия Лак.Адмнстр7Б2 / Glia Lak.Admstr7B2 | Administrator -> Губернатор | Insectoids (Insectoids) | 03 -> 3 Пацифисты | Generic, MinorFaction02, Administrator | `HeroSkill01Amoeba02`, `HeroSkill02Amoeba02` | да |

Партии (`Politics`): 1 = Индустриалисты (Industrialists), 2 = Учёные (Scientists), 3 = Пацифисты (Pacifists), 4 = Экологи (Ecologists), 5 = Религиозные (Religious), 6 = Милитаристы (Militarists).

## 2. Личные навыки героев (`<Skill>` в определении героя, у каждого героя свои два)

Личные навыки остаются у героя всегда, но в моде их эффекты фильтруются по каналу роли (адмирал - флот, губернатор - система, сенатор/диктатор - сенат); чужие каналы отбрасываются без пересчёта.

### HeroSophons01 - Фахеммн Натхарр / Fahemmn Nutharr (Губернатор)
- **`HeroSkill01Sophons01`** 
  - ур.1 [мастерство Command x1]: флот: FleetMaximumShield +0.05 @GarrisonFleet
- **`HeroSkill02Sophons01`** - Фахеммн Натхарр / Fahemmn Nutharr  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: SystemProduction Addition ($(AnomalyCount) Multiplication 10) @ColonizedStarSystem; BonusHeroSystemIndustry Addition ($(AnomalyCount) Multiplication 10) @ColonizedStarSystem (hidden)

### HeroSophons02 - Ханмхарла Джерзи / Hanmharla Gerzi (Адмирал)
- **`HeroSkill01Sophons02`** 
  - ур.1 [мастерство Curiosity x1]: флот: NetShipProbe +0.5 @GarrisonFleet/Ship,ShipRoleExploration
- **`HeroSkill02Sophons02`** - Ханмхарла Джерзи / Hanmharla Gerzi  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemResearch Addition (10 Multiplication $(OwnedAnomalies)) @ColonizedStarSystem; BonusHeroSystemScience Addition (10 Multiplication $(OwnedAnomalies)) @ColonizedStarSystem (hidden)

### HeroSophons03 - Атхассам Рамоэзз / Uthassum Rhamoezz (Сенатор)
- **`HeroSkill01Sophons03`** 
  - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet
- **`HeroSkill02Sophons03`** - Атхассам Рамоэзз / Uthassum Rhamoezz  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: PlanetRawDust +1 @ColonizedStarSystem/ColonizedPlanet; BonusHeroSystemDust +1 @ColonizedStarSystem/ColonizedPlanet (hidden)

### HeroSophons04 - Асароэнни Дзулмаран / Asaroenni Dzulmaran (Сенатор)
- **`HeroSkill01Sophons04`** 
  - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet
- **`HeroSkill02Sophons04`** - Асароэнни Дзулмаран / Asaroenni Dzulmaran  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: PlanetRawScience +1 @ColonizedStarSystem/ColonizedPlanet; BonusHeroSystemScience +1 @ColonizedStarSystem/ColonizedPlanet (hidden)

### HeroSophons05 - Ариринн Турлеммоз / Ahririnn Turlemmoz (Сенатор)
- **`HeroSkill01Sophons05`** 
  - ур.1 [мастерство Curiosity x1]: флот: MaximumShipProbeStock +1 @Ship
- **`HeroSkill02Sophons05`** - Ариринн Турлеммоз / Ahririnn Turlemmoz  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: PlanetRawPrestige +1 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetPrestige Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)

### HeroTerrans01 - Имма Наратья / Immah Naratya (Губернатор)
- **`HeroSkill01Terrans01`** 
  - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +1 @GarrisonFleet
- **`HeroSkill02Terrans01`** - Имма Наратья / Immah Naratya  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: SystemGrowth +7% @ColonizedStarSystem

### HeroTerrans02 - Доминик Хаванен / Dominik Havanen (Губернатор)
- **`HeroSkill01Terrans02`** 
  - ур.1 [мастерство Command x1]: флот: Damage +0.2 @Ship
- **`HeroSkill02Terrans02`** - Доминик Хаванен / Dominik Havanen  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Command x1]: система: GroundBattleDefenderTroopsMaxHealthMultiplier +0.05

### HeroTerrans03 - Олег Лагарин / Oleg Lagarin (Адмирал)
- **`HeroSkill01Terrans03`** 
  - ур.1 [мастерство Command x1]: флот: ShipDamage +0.05 @GarrisonFleet
- **`HeroSkill02Terrans03`** - Олег Лагарин / Oleg Lagarin  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemResearch +10% @ColonizedStarSystem

### HeroTerrans04 - Дмитрий Ленко / Dmitri Lenko (Сенатор)
- **`HeroSkill01Terrans04`** 
  - ур.1 [мастерство Command x1]: флот: ShipHullPlatingAbsorptionPercent +0.05 @GarrisonFleet
- **`HeroSkill02Terrans04`** - Дмитрий Ленко / Dmitri Lenko  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: SystemProduction +5% @ColonizedStarSystem

### HeroTerrans05 - Кинет Малду / Kinete Muldaur (Адмирал)
- **`HeroSkill01Terrans05`** 
  - ур.1 [мастерство Command x1]: флот: ShipDamage Addition (0.005 Multiplication $(CommandPoints)) @GarrisonFleet
- **`HeroSkill02Terrans05`**   (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: GroundBattleDefenderTroopsMaxHealthMultiplier +0.05

### HeroHoratio01 - Фарелла -ЕМ8 Марио 22 / Farella -EM8 Mario 22 (Губернатор)
- **`HeroSkill01Horatio01`** 
  - ур.1 [мастерство Command x1]: флот: ShipMaximumMovement +1 @GarrisonFleet
- **`HeroSkill02Horatio01`** - Фарелла -ЕМ8 Марио 22 / Farella -EM8 Mario 22  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: PlanetPopulationFood Addition (1 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; BonusHeroPlanetFood Addition (1 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet (hidden)

### HeroHoratio02 - Андро Камилл -Зет / Andro Camille -Z (Адмирал)
- **`HeroSkill01Horatio02`** 
  - ур.1 [мастерство Command x1]: флот: MaximumShield Addition (10 Multiplication $(CuriositiesDiscovered)) @GarrisonFleet; MaximumShield Addition (10 Multiplication $(CuriositiesDiscovered)) @GarrisonFleet/Ship (hidden)
- **`HeroSkill02Horatio02`** - Андро Камилл -Зет / Andro Camille -Z  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: PlanetPopulationScience Addition (1 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; BonusHeroSystemScience Addition (1 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet (hidden)

### HeroHoratio03 - Саларио Домус -63 / Salario Domus -63 (Сенатор)
- **`HeroSkill01Horatio03`** 
  - ур.1 [мастерство Command x1]: флот: ShipDamage +0.05 @GarrisonFleet
- **`HeroSkill02Horatio03`** - Саларио Домус -63 / Salario Domus -63  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: ShipProductionCostReduction -0.05

### HeroHoratio04 - Рутиан -Ф11 Тигина / Ruthian -F11 Tighina (Сенатор)
- **`HeroSkill01Horatio04`** 
  - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet
- **`HeroSkill02Horatio04`** - Рутиан -Ф11 Тигина / Ruthian -F11 Tighina  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: Happiness Addition (1 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem

### HeroTimeLords01 - е / e (Губернатор)
- **`HeroSkill01TimeLords01`** 
  - ур.1 [мастерство Command x1]: флот: ShipShieldPenetrationFactor +0.1 @GarrisonFleet
- **`HeroSkill02TimeLords01`** - e  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemResearch Addition (10 Multiplication $(OwnedAnomalies)) @ColonizedStarSystem; BonusHeroSystemScience Addition (10 Multiplication $(OwnedAnomalies)) @ColonizedStarSystem (hidden)

### HeroTimeLords02 - pi (Губернатор)
- **`HeroSkill01TimeLords02`** 
  - ур.1 [мастерство Command x1]: флот: FleetMaximumShield +0.05 @GarrisonFleet
- **`HeroSkill02TimeLords02`** - pi  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: PlanetRawIndustry +1 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetIndustry Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)

### HeroTimeLords03 - sum (Адмирал)
- **`HeroSkill01TimeLords03`** 
  - ур.1 [мастерство Command x1]: флот: ShipHullPlatingPenetrationFactor +0.1 @GarrisonFleet
- **`HeroSkill02TimeLords03`** - sum  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: ShipProductionCostReduction -0.05

### HeroTimeLords04 - derivate (Сенатор)
- **`HeroSkill01TimeLords04`** 
  - ур.1 [мастерство Command x1]: флот: ShipShieldAbsorptionPercent +0.1 @GarrisonFleet
- **`HeroSkill02TimeLords04`** - derivate  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: PlanetRawScience +1 @ColonizedStarSystem/ColonizedPlanet; BonusHeroSystemScience +1 @ColonizedStarSystem/ColonizedPlanet (hidden)

### HeroUnfallen01 - Барамакса / Baramaxa (Губернатор)
- **`HeroSkill01Unfallen01`** 
  - ур.1 [мастерство Curiosity x1]: флот: ExperiencePerTurn +4 @GarrisonFleet; VisionRange +1 @GarrisonFleet
- **`HeroSkill02Unfallen01`** - Анаксагор / Anaxagoras  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: SystemGrowth +10 @ColonizedStarSystem; SystemGrowth Addition (1 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemFood +10 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (1 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)

### HeroUnfallen02 - Лунабарксес / Lunabarxes (Адмирал)
- **`HeroSkill01Unfallen02`** 
  - ур.1 [мастерство Command x1]: флот: HealthRegenerationRatio +0.05 @GarrisonFleet
- **`HeroSkill02Unfallen02`** - Лунабарксес / Lunabarxes  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemMoney +15 @ColonizedStarSystem; BonusHeroSystemDust +15 @ColonizedStarSystem (hidden)

### HeroUnfallen03 - Састреллен / Thustrellen (Адмирал)
- **`HeroSkill01Unfallen03`** 
  - ур.1 [мастерство Command x1]: флот: Evasion +0.1 @GarrisonFleet
- **`HeroSkill02Unfallen03`** - Састреллен / Thustrellen  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: Happiness +5

### HeroUnfallen04 - Анаксагор / Anaxagoras (Адмирал)
- **`HeroSkill01Unfallen04`** 
  - ур.1 [мастерство Command x1]: флот: ShipDamage +0.05 @GarrisonFleet
- **`HeroSkill02Unfallen04`** - Барамакса / Baramaxa  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: NetSystemManpower +10 @ColonizedStarSystem

### HeroCravers01 - Малк-Сорвиголова / Malk Unbound (Адмирал)
- **`HeroSkill01Cravers01`** 
  - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet
- **`HeroSkill02Cravers01`** - Малк-Сорвиголова / Malk Unbound  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: PlanetRawIndustry +1 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetIndustry Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)

### HeroCravers02 - Ристо Либера / Risto Libera (Губернатор)
- **`HeroSkill01Cravers02`** 
  - ур.1 [мастерство Command x1]: флот: Damage +0.2 @Ship
- **`HeroSkill02Cravers02`** - Ристо Либера / Risto Libera  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: ShipProductionCostReduction -0.05

### HeroCravers03 - Крен Солутус / Kren Solutus (Адмирал)
- **`HeroSkill01Cravers03`** 
  - ур.1 [мастерство Command x1]: флот: Damage +0.2 @Ship
- **`HeroSkill02Cravers03`** - Крен Солутус / Kren Solutus  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: PlanetRawPrestige +1 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetPrestige Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)

### HeroCravers04 - Оройо Нер Фрид / Oroyo Ner Freed (Адмирал)
- **`HeroSkill01Cravers04`** 
  - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +1 @GarrisonFleet
- **`HeroSkill02Cravers04`** - Оройо Нер Фрид / Oroyo Ner Freed  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +5% @ColonizedStarSystem

### HeroCraversPrime - Корос Апогей / Koros Apogee (Губернатор)
- **`HeroSkill01CraversPrime`**   (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: PlanetRawIndustry +1 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetIndustry Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
- **`HeroSkill02CraversPrime`** 
  - ур.1 [мастерство Curiosity x1]: флот: ExperiencePerTurn +4 @GarrisonFleet; VisionRange +1 @GarrisonFleet

### HeroVenetians01 - Иотар Вос-Ланселлум / Iotar Vos-Lancellum (Адмирал)
- **`HeroSkill01Venetians01`** 
  - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet
- **`HeroSkill02Venetians01`** - Дженестра Древ-Меос / Jenhaestra’Drev’Meos  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: ShipProductionCostReduction -0.05

### HeroVenetians02 - Колстя Вельм-Омокар / Kolstya Velm-Omokar (Адмирал)
- **`HeroSkill01Venetians02`** 
  - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +1 @GarrisonFleet
- **`HeroSkill02Venetians02`** - Иотар Вос-Ланселлум / Iotar’Vos’Lancellum  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: BuildingProductionCostReduction -0.05 @ColonizedStarSystem

### HeroVenetians03 - Елчин Ордара-Арракьо / Yelchin Ordara-Arrakyo (Адмирал)
- **`HeroSkill01Venetians03`** 
  - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +1 @GarrisonFleet
- **`HeroSkill02Venetians03`** - Елчин Ордара-Арракьо / Yelchin’Ordara’Arrakyo  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemMoney Addition ($(AnomalyCount) Multiplication 10) @ColonizedStarSystem; BonusHeroSystemDust Addition ($(AnomalyCount) Multiplication 10) @ColonizedStarSystem (hidden)

### HeroVenetians04 - Дженестра Древ-Меос / Jenhaestra Drev-Meos (Сенатор)
- **`HeroSkill01Venetians04`** 
  - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet
- **`HeroSkill02Venetians04`** - Колстя Вельм-Омокар / Kolstya’Velm’Omokar  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: PlanetRawDust +1 @ColonizedStarSystem/ColonizedPlanet; BonusHeroSystemDust +1 @ColonizedStarSystem/ColonizedPlanet (hidden)

### HeroVenetians05 - Брунем Берто'Ланселлум / Brunem Berto'Lancellum (Адмирал)
- **`HeroSkill01Venetians05`** 
  - ур.1 [мастерство Command x1]: флот: Evasion +0.1 @GarrisonFleet
- **`HeroSkill02Venetians05`**   (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: флот: GarrisonMoneyUpkeep -10% @GarrisonFleet

### HeroVampirilis01 - Энав Св. Льеаор / Enav St Lyeaor (Адмирал)
- **`HeroSkill01Vampirilis01`** 
  - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet
- **`HeroSkill02Vampirilis01`** - Энав Св. Льеаор / Enav St Lyeaor  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: SystemProduction +5 @ColonizedStarSystem; SystemGrowth Addition (10 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; BonusHeroSystemIndustry +5 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (10 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)

### HeroVampirilis02 - Варб Св. Зуйейна / Varb St Zouieina (Адмирал)
- **`HeroSkill01Vampirilis02`** 
  - ур.1 [мастерство Command x1]: флот: ShipDamage +0.05 @GarrisonFleet
- **`HeroSkill02Vampirilis02`** - Варб Св. Зуйейна / Varb St Zouieina  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +10 @ColonizedStarSystem; BonusHeroSystemPrestige +10 @ColonizedStarSystem (hidden)

### HeroVampirilis03 - Клев Св. Сейотия / Klev St Seiotia (Адмирал)
- **`HeroSkill01Vampirilis03`** 
  - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +1 @GarrisonFleet
- **`HeroSkill02Vampirilis03`** - Клев Св. Сейотия / Klev St Seiotia  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemResearch Addition (10 Multiplication $(OwnedAnomalies)) @ColonizedStarSystem; BonusHeroSystemScience Addition (10 Multiplication $(OwnedAnomalies)) @ColonizedStarSystem (hidden)

### HeroVampirilis04 - Даро Св. Тайадж / Daro St Taiage (Сенатор)
- **`HeroSkill01Vampirilis04`** 
  - ур.1 [мастерство Command x1]: флот: Damage +0.2 @Ship
- **`HeroSkill02Vampirilis04`** - Даро Св. Тайадж / Daro St Taiage  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: ShipProductionCostReduction -0.05

### HeroAmoeba01 - Куратор Ай 1.2 Гадо / Ay Seer 1.2Gado (Сенатор)
- **`HeroSkill01Amoeba01`** 
  - ур.1 [мастерство Command x1]: флот: HealthRegenerationRatio +0.05 @GarrisonFleet
- **`HeroSkill02Amoeba01`** - Куратор Ай 1.2 Гадо / Ay Seer 1.2Gado  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: PlanetRawPrestige +1 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetPrestige Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)

### HeroBenthys01 - Нелли "Движуха" Сира Нинпере / Nelly "Shaken" Sira Ninpere (Губернатор)
- **`HeroSkill01Benthys01`** 
  - ур.1 [мастерство Curiosity x1]: флот: NetShipProbe +0.5 @GarrisonFleet/Ship,ShipRoleExploration
- **`HeroSkill02Benthys01`** - Нелли "Движуха" Сира Нинпере / Nelly “Shaken” Sira Ninpere  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: Happiness +5

### HeroBots01 - ЭПИСТИС "Мейдин" 20910 / EPISTIS "Madein" 20910 (Адмирал)
- **`HeroSkill01Bots01`** 
  - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet
- **`HeroSkill02Bots01`** - ЭПИСТИС "Мейдин" 20910 / EPISTIS "Madein" 20910  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: SystemGrowth Addition (10 Multiplication $(AnomalyCount)) @ColonizedStarSystem; BonusHeroSystemFood Addition (10 Multiplication $(AnomalyCount)) @ColonizedStarSystem (hidden)

### HeroDeuyivans01 - Тинг Нейль / Ting Neuil (Сенатор)
- **`HeroSkill01Deuyivans01`** 
  - ур.1 [мастерство Command x1]: флот: MaximumShield +20% @Ship
- **`HeroSkill02Deuyivans01`** - Тинг Нейль / Ting Neuil  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemResearch +15 @ColonizedStarSystem; BonusHeroSystemScience +15 @ColonizedStarSystem (hidden)

### HeroHaroshems01 - Вуд Свез / Wud Svez (Губернатор)
- **`HeroSkill01Haroshems01`** 
  - ур.1 [мастерство Command x1]: флот: FleetMaximumShield +0.05 @GarrisonFleet
- **`HeroSkill02Haroshems01`** - Вуд Свез / Wud Svez  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: BuildingProductionCostReduction -0.05 @ColonizedStarSystem

### HeroHisshos01 - Фуша Кинкрест / Fusha Keencrest (Адмирал)
- **`HeroSkill01Hisshos01`** 
  - ур.1 [мастерство Command x1]: флот: ShipDamageProjectile +0.1 @GarrisonFleet
- **`HeroSkill02Hisshos01`** - Фуша Кинкрест / Fusha Keencrest  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: SystemProduction +15 @ColonizedStarSystem; BonusHeroSystemIndustry +15 @ColonizedStarSystem (hidden)

### HeroMavros01 - Крак "Кубический" Блейс / Krak "Cubed" Bleis (Адмирал)
- **`HeroSkill01Mavros01`** 
  - ур.1 [мастерство Command x1]: флот: ShipDamageEnergy +0.1 @GarrisonFleet
- **`HeroSkill02Mavros01`** - Крак "Кубический" Блейс / Krak "Cubed" Bleis  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: ShipProductionCostReduction -0.05

### HeroPilgrims01 - Мванга Мото / Mwanga Moto (Адмирал)
- **`HeroSkill01Pilgrims01`** 
  - ур.1 [мастерство Command x1]: флот: NetShipProbe +0.5 @GarrisonFleet/Ship,ShipRoleExploration
- **`HeroSkill02Pilgrims01`** - Мванга Мото / Mwanga Moto  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemResearch Addition (10 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; BonusHeroSystemScience Addition (10 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)

### HeroZvali01 - Норн Д'Гирит / Norn D'Girit (Губернатор)
- **`HeroSkill01Zvali01`** 
  - ур.1 [мастерство Command x1]: флот: Evasion +0.1 @GarrisonFleet
- **`HeroSkill02Zvali01`** - Норн Д'Гирит / Norn D'Girit  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: Happiness Addition (10 Multiplication $(SystemLevel)) @ColonizedStarSystem

### HeroTikanans01 - Восьмая из Пятнадцатых Карвала / Eighth of Fifteenth of Karval (Адмирал)
- **`HeroSkill01Tikanans01`** 
  - ур.1 [мастерство Command x1]: флот: GarrisonMoneyUpkeep -10% @GarrisonFleet
- **`HeroSkill02Tikanans01`** - Восьмая из Пятнадцатых Карвала / Eighth of Fifteenth of Karval  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: NetSystemManpower +10 @ColonizedStarSystem

### HeroGnashasts01 - Торвот Бзеч'Грул / Torvoth Bzech'Grul (Адмирал)
- **`HeroSkill01Gnashasts01`** 
  - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet
- **`HeroSkill02Gnashasts01`** - Торвот Бзеч'Грул / Torvoth Bzech'Grul  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: GroundBattleBombardmentAttackerDamages +500 @ColonizedStarSystem

### HeroEyders01 - Скейлс Галена / Scales Galena (Сенатор)
- **`HeroSkill01Eyders01`** 
  - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +1 @GarrisonFleet
- **`HeroSkill02Eyders01`** - Скейлс Галена / Scales Galena  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemMoney +15 @ColonizedStarSystem; BonusHeroSystemDust +15 @ColonizedStarSystem (hidden)

### HeroNiris01 - Эли Леандра / Eli Leandra (Губернатор)
- **`HeroSkill01Niris01`** 
  - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet
- **`HeroSkill02Niris01`**   (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: Happiness +5

### HeroBhagabas01 - 5748-А27П-ТЗР / 5748-A27P-TZR (Сенатор)
- **`HeroSkill01Bhagabas01`** 
  - ур.1 [мастерство Command x1]: флот: HealthRegenerationRatio +0.05 @GarrisonFleet
- **`HeroSkill02Bhagabas01`**   (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +5% @ColonizedStarSystem

### HeroRemnant01 - Калин Фетт-Иска / Kalin Fett-Iska (Адмирал)
- **`HeroSkill01Remnant01`** 
  - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +1 @GarrisonFleet
- **`HeroSkill02Remnant01`**   (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: PlanetRawFIDSI +0.5 @ColonizedStarSystem/ColonizedPlanet; BonusHeroSystemFood Addition (0.5 Multiplication $(Population)) @ColonizedStarSystem (hidden); BonusHeroSystemIndustry Addition (0.5 Multiplication $(Population)) @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (0.5 Multiplication $(Population)) @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (0.5 Multiplication $(Population)) @ColonizedStarSystem (hidden); BonusHeroSystemPrestige Addition (0.5 Multiplication $(Population)) @ColonizedStarSystem (hidden)

### HeroPulsos01 - Каксалксиксокс Друз / Caxalxsixox Druse (Адмирал)
- **`HeroSkill01Pulsos01`** 
  - ур.1 [мастерство Command x1]: флот: MaximumShield Addition (10 Multiplication $(CuriositiesDiscovered)) @GarrisonFleet; MaximumShield Addition (10 Multiplication $(CuriositiesDiscovered)) @GarrisonFleet/Ship (hidden)
- **`HeroSkill02Pulsos01`**   (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemResearch Addition (10 Multiplication $(OwnedAnomalies)) @ColonizedStarSystem; BonusHeroSystemScience Addition (10 Multiplication $(OwnedAnomalies)) @ColonizedStarSystem (hidden)

### QuestHeroEndless01 - Реярин Сайус / Reyaryn Cious (Адмирал)
- **`HeroSkill01QuestEndless01`** - Реярин Сиус / Reyaryn Cious
  - ур.1 [мастерство Command x1]: флот: Damage +0.2 @Ship
- **`HeroSkill02QuestEndless01`** - Реярин Сиус / Reyaryn Cious  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: PlanetRawIndustry +1 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetIndustry Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)

### QuestHeroEndless02 - Тиах Жиллек / Tiaych Zhilleaq (Адмирал)
- **`HeroSkill01QuestEndless02`** - Тиах Жиллек / Tiaych Zhilleaq
  - ур.1 [мастерство Command x1]: флот: FreeMovementSpeed x1.5 @GarrisonFleet; VisionRange +1 @GarrisonFleet
- **`HeroSkill02QuestEndless02`** - Тиах Жиллек / Tiaych Zhilleaq  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemResearch Addition (10 Multiplication $(OwnedAnomalies)) @ColonizedStarSystem; BonusHeroSystemScience Addition (10 Multiplication $(OwnedAnomalies)) @ColonizedStarSystem (hidden)

### HeroTerransQuest01 - Хадри Ленко / Hadri Lenko (Адмирал)
- **`HeroSkill01TerransQuest01`**   (tags: ColonizedStarSystem)
  - ур.1 [мастерство Command x1]: флот: MaximumShipManpower +10% @GarrisonFleet/Ship
- **`HeroSkill02TerransQuest01`**   (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: ShipProductionCostReduction -0.05

### HeroTerransQuest02 - Лена Зелевас / Lena Zelevas (Сенатор)
- **`HeroSkill01TerransQuest02`** 
  - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet
- **`HeroSkill02TerransQuest02`**   (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +5% @ColonizedStarSystem

### HeroTerransQuest03 - Петра Мандзукич / Petra Mandzukic (Адмирал)
- **`HeroSkill01TerransQuest03`** 
  - ур.1 [мастерство Command x1]: флот: MaximumShipProbeStock +1 @Ship
- **`HeroSkill02TerransQuest03`**   (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemResearch +15 @ColonizedStarSystem; BonusHeroSystemScience +15 @ColonizedStarSystem (hidden)

### HeroQuestHoratio04 - Пятый / Five (Адмирал)
- **`HeroSkill01HoratioQuest04`** 
  - ур.1 [мастерство Command x1]: флот: MaximumShipProbeStock +1 @Ship
- **`HeroSkill02HoratioQuest04`**   (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemResearch +15 @ColonizedStarSystem; BonusHeroSystemScience +15 @ColonizedStarSystem (hidden)

### HeroQuestVirus01 - Анакс-Джинк 2077 / Anax-Jink 2077 (Адмирал)
- **`HeroSkill01QuestVirus01`** - Анакс-Джинк 2077 / Anax-Jink 2077
  - ур.1 [мастерство Command x1]: флот: MaximumShield Addition (10 Multiplication $(CuriositiesDiscovered)) @GarrisonFleet; MaximumShield Addition (10 Multiplication $(CuriositiesDiscovered)) @GarrisonFleet/Ship (hidden)
- **`HeroSkill02QuestVirus01`** - Анакс-Джинк 2077 / Anax-Jink 2077  (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: PlanetRawScience +1 @ColonizedStarSystem/ColonizedPlanet; BonusHeroSystemScience +1 @ColonizedStarSystem/ColonizedPlanet (hidden)

### HeroPirate01 -  (Адмирал)
- **`HeroSkill01Pirate01`** 
  - ур.1 [мастерство Command x1]: флот: Damage +10% @GarrisonFleet/Ship/Section/ModuleSquadron
- **`HeroSkill02Pirate01`**   (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: SystemProduction +5% @ColonizedStarSystem

### HeroGreenman01 -  (Сенатор)
- **`HeroSkill01Greenman01`** 
  - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +1 @GarrisonFleet
- **`HeroSkill02Greenman01`**   (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: SystemResearch +15 @ColonizedStarSystem; BonusHeroSystemScience +15 @ColonizedStarSystem (hidden)

### HeroGreenman02 -  (Адмирал)
- **`HeroSkill01Greenman02`** 
  - ур.1 [мастерство Command x1]: флот: MaximumShipManpower +10% @GarrisonFleet/Ship
- **`HeroSkill02Greenman02`**   (tags: ColonizedStarSystem)
  - ур.1 [мастерство Labour x1]: система: SystemProduction +15 @ColonizedStarSystem; BonusHeroSystemIndustry +15 @ColonizedStarSystem (hidden)

### HeroAmoeba02 - Глия Лак.Адмнстр7Б2 / Glia Lak.Admstr7B2 (Губернатор)
- **`HeroSkill01Amoeba02`** 
  - ур.1 [мастерство Command x1]: флот: GarrisonMoneyUpkeep -10% @GarrisonFleet
- **`HeroSkill02Amoeba02`**   (tags: ColonizedStarSystem)
  - ур.1 [мастерство Wit x1]: система: PlanetRawDust +1 @ColonizedStarSystem/ColonizedPlanet; BonusHeroSystemDust +1 @ColonizedStarSystem/ColonizedPlanet (hidden)

## 3. Деревья (18)

### HeroSkillTreeGeneric - Универсальные навыки / Universal skills
Эта ветка содержит навыки, доступные всем Героям.

Героев с этим деревом: 63

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchGeneric`** - Эксперт по оптимизации производства / Optimal Operations Expert  (tags: ColonizedStarSystem)
    - описание: После многих лет изучения оптимальных производственных процессов в различных отраслях, Герой обретает концентрированное знание технологических процессов, способных обеспечить максимальную производительность в промышленности и сельском хозяйстве данной планетной системы.
    - ур.1 [мастерство Labour x1]: система: PlanetRawFood +1 @ColonizedStarSystem/ColonizedPlanet; PlanetRawIndustry +1 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetFood Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetIndustry Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - ур.2 [мастерство Labour x2]: система: PlanetRawFood +2 @ColonizedStarSystem/ColonizedPlanet; PlanetRawIndustry +2 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetFood Addition (2 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetIndustry Addition (2 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - роль Адмирал (A): «Резервные экипажи / Бронепластины» / Reserve Crews / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.133 @GarrisonFleet  [Production]; converted: FleetManpowerRefillRateModifier +0.267 @GarrisonFleet  [Growth]
      - ур.2: converted: ShipHullPlatingAbsorptionPercent +0.267 @GarrisonFleet  [Production]; converted: FleetManpowerRefillRateModifier +0.533 @GarrisonFleet  [Growth]
    - роль Губернатор (G): «Агрокультура / Индустриальный рост» / Agriculture / Industrial Drive
      - ур.1: kept: PlanetRawFood +1 @ColonizedStarSystem/ColonizedPlanet; kept: PlanetRawIndustry +1 @ColonizedStarSystem/ColonizedPlanet; kept: BonusHeroPlanetFood Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); kept: BonusHeroPlanetIndustry Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
      - ур.2: kept: PlanetRawFood +2 @ColonizedStarSystem/ColonizedPlanet; kept: PlanetRawIndustry +2 @ColonizedStarSystem/ColonizedPlanet; kept: BonusHeroPlanetFood Addition (2 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); kept: BonusHeroPlanetIndustry Addition (2 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - роль Сенатор (S): «Аграрная реформа / Государственная индустрия» / Agrarian Reform / State Industry
      - ур.1: converted: PlanetRawIndustry +1.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: PlanetRawFood +1.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - роль Диктатор (D): «Аграрная реформа / Государственная индустрия» / Agrarian Reform / State Industry
      - ур.1: converted: PlanetRawIndustry +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: PlanetRawFood +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.2: converted: PlanetRawIndustry +2.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: PlanetRawFood +2.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.3: converted: PlanetRawIndustry +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: PlanetRawFood +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
  - **`HeroSkill02BranchGeneric`** - Космический изгнанник / Cosmic Castaway
    - описание: Во время перелета Герой уводит маленькие разведгруппы от главного флота. Выполнение этой операции дает флоту дополнительное Поле Обзора и готовит членов экипажей к спецоперациям в глубоком космосе.
    - ур.1 [мастерство Curiosity x1]: флот: ExperiencePerTurn +4 @GarrisonFleet; VisionRange +1 @GarrisonFleet
    - ур.2 [мастерство Curiosity x2]: флот: ExperiencePerTurn +8 @GarrisonFleet; VisionRange +2 @GarrisonFleet
    - роль Адмирал (A): «Боевой опыт / Манёвренность» / Combat Experience / Maneuverability
      - ур.1: kept: ExperiencePerTurn +4 @GarrisonFleet; kept: VisionRange +1 @GarrisonFleet
      - ур.2: kept: ExperiencePerTurn +8 @GarrisonFleet; kept: VisionRange +2 @GarrisonFleet
    - роль Губернатор (G): «Академические программы / Торговые пути» / Academic Programs / Trade Routes
      - ур.1: converted: TradingRoutesBonus_Production +20% @ColonizedStarSystem  [Mobility]; converted: SystemResearch +10% @ColonizedStarSystem  [Experience]
      - ур.2: converted: TradingRoutesBonus_Production +40% @ColonizedStarSystem  [Mobility]; converted: SystemResearch +20% @ColonizedStarSystem  [Experience]
    - роль Сенатор (S): «Образовательная реформа / Имперская навигация» / Education Reform / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +1 @Empire/GarrisonFleet  [Mobility]; converted: SystemResearch +10% @Empire/ColonizedStarSystem  [Experience]
    - роль Диктатор (D): «Образовательная реформа / Имперская навигация» / Education Reform / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +0.8 @Empire/GarrisonFleet  [Mobility]; converted: SystemResearch +8% @Empire/ColonizedStarSystem  [Experience]
      - ур.2: converted: ShipMaximumMovement +1.6 @Empire/GarrisonFleet  [Mobility]; converted: SystemResearch +16% @Empire/ColonizedStarSystem  [Experience]
      - ур.3: converted: ShipMaximumMovement +2.4 @Empire/GarrisonFleet  [Mobility]; converted: SystemResearch +24% @Empire/ColonizedStarSystem  [Experience]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchGeneric`** - Оптимизация руководства / Transformational Leadership  (tags: ColonizedStarSystem)
    - описание: Герой сосредоточен на построении позитивных отношений с правителями данной системы, формируя глобальное видение того, каким образом их совместная работа может способствовать процветанию империи.
    - ур.1 [мастерство Wit x1]: система: Happiness +10
    - ур.2 [мастерство Wit x2]: система: Happiness +20
    - роль Адмирал (A): «Усиленные щиты» / Reinforced Shields
      - ур.1: converted: FleetMaximumShield +0.25 @GarrisonFleet  [Happiness]
      - ур.2: converted: FleetMaximumShield +0.5 @GarrisonFleet  [Happiness]
    - роль Губернатор (G): «Общественный порядок» / Public Order
      - ур.1: kept: Happiness +10
      - ур.2: kept: Happiness +20
    - роль Сенатор (S): «Всеобщее благополучие» / General Welfare
      - ур.1: converted: Happiness +10 @Empire/ColonizedStarSystem  [Happiness]
    - роль Диктатор (D): «Всеобщее благополучие» / General Welfare
      - ур.1: converted: Happiness +8 @Empire/ColonizedStarSystem  [Happiness]
      - ур.2: converted: Happiness +16 @Empire/ColonizedStarSystem  [Happiness]
      - ур.3: converted: Happiness +24 @Empire/ColonizedStarSystem  [Happiness]
  - **`HeroSkill04BranchGeneric`** - Гений вербовки / Conscription Genius  (tags: ColonizedStarSystem)
    - описание: Когда войска несут масштабные потери в кровопролитных войнах, правителям бывает непросто рекрутировать новых бойцов. Однако благодаря умно организованным пропагандистским кампаниям, использующим тщательно подобранные лозунги и рекламные материалы, Герой способен улучшить показатели армейского набора.
    - ур.1 [мастерство Labour x1]: система: MaximumSystemManpower +100 | сенат: MaximumEmpireManpowerStock +25% @Empire
    - ур.2 [мастерство Labour x2]: система: MaximumSystemManpower +200 | сенат: MaximumEmpireManpowerStock +25% @Empire
    - роль Адмирал (A): «Набор экипажей» / Crew Recruitment
      - ур.1: converted: MaximumShipManpower +56.2% @GarrisonFleet/Ship  [Manpower]
      - ур.2: converted: MaximumShipManpower +81.2% @GarrisonFleet/Ship  [Manpower]
    - роль Губернатор (G): «Мобилизация» / Mobilization
      - ур.1: kept: MaximumSystemManpower +100; converted: NetSystemManpower +12 @ColonizedStarSystem  [Manpower]
      - ур.2: kept: MaximumSystemManpower +200; converted: NetSystemManpower +12 @ColonizedStarSystem  [Manpower]
    - роль Сенатор (S): «Всеобщая мобилизация» / National Mobilization
      - ур.1: kept: MaximumEmpireManpowerStock +25% @Empire; converted: MaximumEmpireManpowerStock +50% @Empire  [Manpower]
    - роль Диктатор (D): «Всеобщая мобилизация» / National Mobilization
      - ур.1: kept: MaximumEmpireManpowerStock +25% @Empire  (x1.6 for the dictator); converted: MaximumEmpireManpowerStock +40% @Empire  [Manpower]
      - ур.2: kept: MaximumEmpireManpowerStock +25% @Empire  (x1.6 for the dictator); converted: MaximumEmpireManpowerStock +80% @Empire  [Manpower]
      - ур.3: kept: MaximumEmpireManpowerStock +25% @Empire  (x2.4 for the dictator); converted: MaximumEmpireManpowerStock +120% @Empire  [Manpower]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchGeneric`** - Катализация Пылью / Dust Catalyzation
    - описание: Случайная встреча в глубоком космосе со скоплением неконцентрированной Пыли изменяет временное поле Героев, сдвигая их в несколько иную плоскость существования. Затем Герои быстро возвращаются в нормальную временную структуру, но остаточные эффекты этой аномалии повышают их отдачу и снижают потребность в отдыхе.
    - ур.1 [мастерство Curiosity x1]: система: Upkeep -25% @ColonizedStarSystem/HeroAssignedToSystem | герой: AssignmentCooldownBaseDuration -25%
    - ур.2 [мастерство Curiosity x2]: система: Upkeep -50% @ColonizedStarSystem/HeroAssignedToSystem | герой: AssignmentCooldownBaseDuration -50%
    - роль Адмирал (A): «Экономия флота» / Fleet Economy
      - ур.1: converted: GarrisonMoneyUpkeep -25% @GarrisonFleet  [Upkeep]; hero: AssignmentCooldownBaseDuration -25%
      - ур.2: converted: GarrisonMoneyUpkeep -50% @GarrisonFleet  [Upkeep]; hero: AssignmentCooldownBaseDuration -50%
    - роль Губернатор (G): «Бережливое управление» / Frugal Administration
      - ур.1: kept: Upkeep -25% @ColonizedStarSystem/HeroAssignedToSystem; hero: AssignmentCooldownBaseDuration -25%
      - ур.2: kept: Upkeep -50% @ColonizedStarSystem/HeroAssignedToSystem; hero: AssignmentCooldownBaseDuration -50%
    - роль Сенатор (S): «Режим экономии» / Austerity
      - ур.1: converted: SystemMoneyUpkeep -25% @Empire/ColonizedStarSystem  [Upkeep]; hero: AssignmentCooldownBaseDuration -50%
    - роль Диктатор (D): «Режим экономии» / Austerity
      - ур.1: converted: SystemMoneyUpkeep -20% @Empire/ColonizedStarSystem  [Upkeep]; hero: AssignmentCooldownBaseDuration -25%
      - ур.2: converted: SystemMoneyUpkeep -40% @Empire/ColonizedStarSystem  [Upkeep]; hero: AssignmentCooldownBaseDuration -50%
      - ур.3: converted: SystemMoneyUpkeep -60% @Empire/ColonizedStarSystem  [Upkeep]; hero: AssignmentCooldownBaseDuration -50%
  - **`HeroSkill06BranchGeneric`** - Оружие на заказ / Bespoke Arms
    - описание: Обладая глубокими познаниями о системах вооружения со всех уголков галактики - от люмерийских торпедных технологий до оружия с фазированными лучами с Мавроса - Герой может получить дополнительные бонусы при производстве оружия.
    - ур.1 [мастерство Command x1]: флот: Damage +0.8 @Ship
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: Damage +0.8 @Ship
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.8 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.4 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.64 @Empire/GarrisonFleet  [Offense]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchGeneric`** - Трансцендентное мировоззрение / Transcendental Outlook
    - описание: Герои, конечно, открывают впечатляющие перспективы развития и созидают стратегии, способные изменить судьбы целых империй, но обходятся они очень недешево. Огромные кортежи, множество резиденций и пунктуально составленные списки предъявляемых требований влетают в копеечку. К счастью, обретение этого навыка позволяет Герою отринуть эти мирские заботы, и тогда стоимость его содержания убывает до нуля.
    - ур.1 [мастерство Curiosity x1]: флот: Upkeep x0 @.GarrisonFleet/Hero,HeroAssignedToFleet | герой: Upkeep -10% @Empire/ColonizedStarSystem/Hero; Upkeep -10% @Empire/GarrisonFleet/Hero (hidden)
    - роль Адмирал (A): «Экономия флота» / Fleet Economy
      - ур.1: kept: Upkeep x0 @.GarrisonFleet/Hero,HeroAssignedToFleet; hero: Upkeep -10% @Empire/ColonizedStarSystem/Hero; hero: Upkeep -10% @Empire/GarrisonFleet/Hero (hidden)
    - роль Губернатор (G): «Бережливое управление» / Frugal Administration
      - ур.1: converted: SystemMoneyUpkeep -100% @ColonizedStarSystem  [Upkeep]; hero: Upkeep -10% @Empire/ColonizedStarSystem/Hero; hero: Upkeep -10% @Empire/GarrisonFleet/Hero (hidden)
    - роль Сенатор (S): «Режим экономии» / Austerity
      - ур.1: converted: SystemMoneyUpkeep -50% @Empire/ColonizedStarSystem  [Upkeep]; hero: Upkeep -10% @Empire/ColonizedStarSystem/Hero; hero: Upkeep -10% @Empire/GarrisonFleet/Hero (hidden)
    - роль Диктатор (D): «Режим экономии» / Austerity
      - ур.1: converted: SystemMoneyUpkeep -80% @Empire/ColonizedStarSystem  [Upkeep]; hero: Upkeep -10% @Empire/ColonizedStarSystem/Hero; hero: Upkeep -10% @Empire/GarrisonFleet/Hero (hidden)

### HeroSkillTreeAdmiral - Навыки Стража / Guardian skills
Этот блок содержит полный набор специальных навыков Стража.

Героев с этим деревом: 18 (HeroTerrans03, HeroTerrans05, HeroTimeLords03, HeroUnfallen04, HeroCravers01, HeroCravers03, HeroVenetians01, HeroVenetians05, HeroVampirilis01, HeroVampirilis02, HeroHisshos01, HeroMavros01, HeroPilgrims01, HeroTikanans01, QuestHeroEndless01, HeroTerransQuest01, HeroQuestHoratio04, HeroGreenman02)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchAdmiral`** - Фанат эффективности / Efficiency Aficionado  (tags: ColonizedStarSystem)
    - описание: Герой демонстрирует маниакальное стремление оптимизировать методологию принятия решений. Индустриальные комплексы и работники под его руководством извлекают выгоду (порой не слишком охотно) из этого постоянного шквала новых блестящих идей.
    - ур.1 [мастерство Labour x1]: система: SystemProduction +5 @ColonizedStarSystem; SystemProduction Addition (5 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; BonusHeroSystemFood +5 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (5 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemProduction +10 @ColonizedStarSystem; SystemProduction Addition (10 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; BonusHeroSystemFood +10 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (10 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Бронепластины / Резервные экипажи» / Plating / Reserve Crews
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.1 @GarrisonFleet  [Production]; converted: FleetManpowerRefillRateModifier +0.1 @GarrisonFleet  [Growth]
      - ур.2: converted: ShipHullPlatingAbsorptionPercent +0.2 @GarrisonFleet  [Production]; converted: FleetManpowerRefillRateModifier +0.2 @GarrisonFleet  [Growth]
    - роль Губернатор (G): «Индустриальный рост / Агрокультура» / Industrial Drive / Agriculture
      - ур.1: kept: SystemProduction +5 @ColonizedStarSystem; kept: SystemProduction Addition (5 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemFood +5 @ColonizedStarSystem (hidden); kept: BonusHeroSystemFood Addition (5 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemProduction +10 @ColonizedStarSystem; kept: SystemProduction Addition (10 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemFood +10 @ColonizedStarSystem (hidden); kept: BonusHeroSystemFood Addition (10 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Государственная индустрия / Аграрная реформа» / State Industry / Agrarian Reform
      - ур.1: converted: PlanetRawIndustry +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: PlanetRawFood +0.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - роль Диктатор (D): «Государственная индустрия / Аграрная реформа» / State Industry / Agrarian Reform
      - ур.1: converted: PlanetRawIndustry +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: PlanetRawFood +0.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.2: converted: PlanetRawIndustry +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: PlanetRawFood +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.3: converted: PlanetRawIndustry +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: PlanetRawFood +1.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
  - **`HeroSkill02BranchAdmiral`** - В строгом соответствии с нормативами / By The Book
    - описание: Герой лично контролирует основные аспекты строительства флота, в то время как инженеры и рабочие в доках неустанно заботятся о том, чтобы их корабли были максимально подготовлены к наихудшему, что может ожидать их в глубинах космоса.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: FleetMaximumHealth +0.2 @GarrisonFleet
    - роль Адмирал (A): «Крепкие корпуса» / Hardened Hulls
      - ур.1: kept: FleetMaximumHealth +0.1 @GarrisonFleet
      - ур.2: kept: FleetMaximumHealth +0.2 @GarrisonFleet
    - роль Губернатор (G): «Укреплённые гарнизоны» / Fortified Garrisons
      - ур.1: converted: MaximumSystemManpower +20%  [Defense]
      - ур.2: converted: MaximumSystemManpower +40%  [Defense]
    - роль Сенатор (S): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.1 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.08 @Empire/GarrisonFleet  [Defense]
      - ур.2: converted: FleetMaximumHealth +0.16 @Empire/GarrisonFleet  [Defense]
      - ур.3: converted: FleetMaximumHealth +0.24 @Empire/GarrisonFleet  [Defense]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchAdmiral`** - Документы, пожалуйста / Papers Please
    - описание: Хранение детальных сведений о каждом члене экипажа в составе руководимого им флота позволяет Герою быстро и эффективно проводить мобилизацию даже в тех случаях, когда число бойцов на борту превышает обычное.
    - ур.1 [мастерство Curiosity x1]: флот: FleetManpowerRefillRateModifier +0.4 @GarrisonFleet | сенат: PeacefulTreatyCostReduction -0.5 @Empire; AggressiveTreatyCostReduction -0.5 @Empire
    - роль Адмирал (A): «Пробивание щитов / Полевой ремонт» / Shield Breaking / Field Repairs
      - ур.1: kept: FleetManpowerRefillRateModifier +0.4 @GarrisonFleet; converted: ShipShieldPenetrationFactor +1.6 @GarrisonFleet  [Diplomacy]
    - роль Губернатор (G): «Торговые связи / Пополнение войск» / Trade Ties / Troop Replenishment
      - ур.1: converted: MaximumSystemManpower +200  [Sustain]; converted: TradingRoutesBonus_Growth +160% @ColonizedStarSystem  [Diplomacy]
    - роль Сенатор (S): «Дипломатический корпус / Службы ремонта» / Diplomatic Corps / Repair Corps
      - ур.1: kept: PeacefulTreatyCostReduction -0.5 @Empire; kept: AggressiveTreatyCostReduction -0.5 @Empire; converted: HealthRegenerationRatio +0.1 @Empire/GarrisonFleet  [Sustain]
    - роль Диктатор (D): «Дипломатический корпус / Службы ремонта» / Diplomatic Corps / Repair Corps
      - ур.1: kept: PeacefulTreatyCostReduction -0.5 @Empire  (x1.6 for the dictator); kept: AggressiveTreatyCostReduction -0.5 @Empire  (x1.6 for the dictator); converted: HealthRegenerationRatio +0.16 @Empire/GarrisonFleet  [Sustain]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill04BranchAdmiral`** - Глас народа / Vox Populi  (tags: ColonizedStarSystem)
    - описание: Герой старается регулярно посещать региональные верфи и промзоны, стимулируя этим их производительность и создавая атмосферу дружеского соревнования.
    - ур.1 [мастерство Labour x1]: система: SystemProduction +15% @ColonizedStarSystem
    - ур.2 [мастерство Labour x2]: система: SystemProduction +30% @ColonizedStarSystem
    - роль Адмирал (A): «Бронепластины» / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.2 @GarrisonFleet  [Production]
      - ур.2: converted: ShipHullPlatingAbsorptionPercent +0.4 @GarrisonFleet  [Production]
    - роль Губернатор (G): «Индустриальный рост» / Industrial Drive
      - ур.1: kept: SystemProduction +15% @ColonizedStarSystem
      - ур.2: kept: SystemProduction +30% @ColonizedStarSystem
    - роль Сенатор (S): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - роль Диктатор (D): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
      - ур.2: converted: PlanetRawIndustry +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
      - ур.3: converted: PlanetRawIndustry +4.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
  - **`HeroSkill05BranchAdmiral`** - Железная дисциплина / Iron Discipline
    - описание: Во время обороны, Герой способен воодушевить экипажи судов лучше защищать корабли, наполняя своих подчиненных стальной решимостью и сосредоточенностью.
    - ур.1 [мастерство Command x1]: флот: ShipHullPlatingAbsorptionPercent +0.2 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipHullPlatingAbsorptionPercent +0.4 @GarrisonFleet
    - роль Адмирал (A): «Крепкие корпуса» / Hardened Hulls
      - ур.1: kept: ShipHullPlatingAbsorptionPercent +0.2 @GarrisonFleet
      - ур.2: kept: ShipHullPlatingAbsorptionPercent +0.4 @GarrisonFleet
    - роль Губернатор (G): «Укреплённые гарнизоны» / Fortified Garrisons
      - ур.1: converted: MaximumSystemManpower +40%  [Defense]
      - ур.2: converted: MaximumSystemManpower +80%  [Defense]
    - роль Сенатор (S): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.2 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.16 @Empire/GarrisonFleet  [Defense]
      - ур.2: converted: FleetMaximumHealth +0.32 @Empire/GarrisonFleet  [Defense]
      - ур.3: converted: FleetMaximumHealth +0.48 @Empire/GarrisonFleet  [Defense]
  - **`HeroSkill06BranchAdmiral`** - Пострелят. система наведения на цель / Post-Relativistic Targeting
    - описание: Используя теоретические разработки на основе пострелятивистской математики, Герой способен просчитывать неконвенциональные стратегии ведения огня, извлекая выгоду из использования ноль-гравитационных сценариев и повышая общую огневую мощь Флота.
    - ур.1 [мастерство Command x1]: флот: ShipDamage +0.2 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipDamage +0.4 @GarrisonFleet
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: ShipDamage +0.2 @GarrisonFleet
      - ур.2: kept: ShipDamage +0.4 @GarrisonFleet
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem  [Offense]
      - ур.2: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.4 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.2 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.16 @Empire/GarrisonFleet  [Offense]
      - ур.2: converted: ShipDamage +0.32 @Empire/GarrisonFleet  [Offense]
      - ур.3: converted: ShipDamage +0.48 @Empire/GarrisonFleet  [Offense]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchAdmiral`** - Гроссмейстер военного дела / Battlefield Grandmaster
    - описание: Владея практически безошибочным пониманием военной стратегии, Герой способен увеличивать насколько необходимо наступательные и оборонительные возможности флота.
    - ур.1 [мастерство Command x1]: флот: ShipDamage +0.25 @GarrisonFleet; FleetMaximumShield +0.33 @GarrisonFleet
    - роль Адмирал (A): «Крепкие корпуса / Огневая мощь» / Hardened Hulls / Firepower
      - ур.1: kept: ShipDamage +0.25 @GarrisonFleet; kept: FleetMaximumShield +0.33 @GarrisonFleet
    - роль Губернатор (G): «Укреплённые гарнизоны / Гарнизонная выучка» / Fortified Garrisons / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.25 @ColonizedStarSystem  [Offense]; converted: MaximumSystemManpower +66%  [Defense]
    - роль Сенатор (S): «Бронирование флотов / Военные ассигнования» / Fleet Armor Programs / Military Appropriations
      - ур.1: converted: ShipDamage +0.125 @Empire/GarrisonFleet  [Offense]; converted: FleetMaximumHealth +0.165 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов / Военные ассигнования» / Fleet Armor Programs / Military Appropriations
      - ур.1: converted: ShipDamage +0.2 @Empire/GarrisonFleet  [Offense]; converted: FleetMaximumHealth +0.264 @Empire/GarrisonFleet  [Defense]

### HeroSkillTreeAdventurer - Навыки Искателя / Seeker skills
Этот блок содержит полный набор специальных навыков Искателя.

Героев с этим деревом: 16 (HeroSophons02, HeroHoratio02, HeroUnfallen02, HeroUnfallen03, HeroCravers04, HeroVenetians02, HeroVenetians03, HeroVampirilis03, HeroBots01, HeroGnashasts01, HeroRemnant01, HeroPulsos01, QuestHeroEndless02, HeroTerransQuest03, HeroQuestVirus01, HeroPirate01)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchAdventurer`** - Извлекая пользу из безграничной фантазии / Blue Sky Speculator  (tags: ColonizedStarSystem)
    - описание: Иногда, наибольшие дивиденды может принести ставка на неизвестное. Герой стимулирует корпоративное финансирование научных исследований в своей системе, результатом чего оказывается увеличение показателя Науки.
    - ур.1 [мастерство Wit x1]: система: SystemResearch +15 @ColonizedStarSystem; BonusHeroSystemScience +15 @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemResearch +30 @ColonizedStarSystem; BonusHeroSystemScience +30 @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Дальняя разведка» / Long-Range Scouting
      - ур.1: converted: VisionRange +1.5 @GarrisonFleet  [Science]
      - ур.2: converted: VisionRange +3 @GarrisonFleet  [Science]
    - роль Губернатор (G): «Научные центры» / Research Centers
      - ур.1: kept: SystemResearch +15 @ColonizedStarSystem; kept: BonusHeroSystemScience +15 @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemResearch +30 @ColonizedStarSystem; kept: BonusHeroSystemScience +30 @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +1.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - роль Диктатор (D): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +1.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
      - ур.2: converted: PlanetRawScience +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
      - ур.3: converted: PlanetRawScience +3.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
  - **`HeroSkill02BranchAdventurer`** - Улучшенная астронавигация / Enhanced Astronavigation
    - описание: Улучшенная астронавигация. Даже через тысячи лет после эпохи парусов и секстантов, ориентация по звездам представляет собой ценный навык. Отличное знание рисунка созвездий наделяет флот Героя усовершенствованными возможностями Видения и Исследования.
    - ур.1 [мастерство Curiosity x1]: флот: FreeMovementSpeed x1.5 @GarrisonFleet; VisionRange +1 @GarrisonFleet
    - ур.2 [мастерство Curiosity x2]: флот: FreeMovementSpeed x2 @GarrisonFleet; VisionRange +2 @GarrisonFleet
    - роль Адмирал (A): «Манёвренность» / Maneuverability
      - ур.1: kept: FreeMovementSpeed x1.5 @GarrisonFleet; kept: VisionRange +1 @GarrisonFleet
      - ур.2: kept: FreeMovementSpeed x2 @GarrisonFleet; kept: VisionRange +2 @GarrisonFleet
    - роль Губернатор (G): «Торговые пути» / Trade Routes
      - ур.1: converted: TradingRoutesBonus_Production +120% @ColonizedStarSystem  [Mobility]
      - ур.2: converted: TradingRoutesBonus_Production +240% @ColonizedStarSystem  [Mobility]
    - роль Сенатор (S): «Имперская навигация» / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +6 @Empire/GarrisonFleet  [Mobility]
    - роль Диктатор (D): «Имперская навигация» / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +4.8 @Empire/GarrisonFleet  [Mobility]
      - ур.2: converted: ShipMaximumMovement +10 @Empire/GarrisonFleet  [Mobility]
      - ур.3: converted: ShipMaximumMovement +14 @Empire/GarrisonFleet  [Mobility]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchAdventurer`** - Личное ополчение / Personal Militia
    - описание: Герой набирает разношерстную команду из бывших солдат, чокнутых ученых и лиц без определенных занятий, задачей которой является укрепление наземной обороны в планетной системе. Когда оно не занято ведением партизанской войны, это подразделение работает с простонародьем путем распространения сказаний и застольных песен, прославляющих деянья героя.
    - ур.1 [мастерство Wit x1]: система: PlanetRawPrestige +1 @ColonizedStarSystem/ColonizedPlanet; GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @ColonizedStarSystem; BonusHeroSystemPrestige +1 @ColonizedStarSystem/ColonizedPlanet (hidden)
    - ур.2 [мастерство Wit x2]: система: PlanetRawPrestige +2 @ColonizedStarSystem/ColonizedPlanet; GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem; BonusHeroSystemPrestige +2 @ColonizedStarSystem/ColonizedPlanet (hidden)
    - роль Адмирал (A): «Осадное мастерство / Крепкие корпуса» / Siegecraft / Hardened Hulls
      - ур.1: converted: FleetMaximumHealth +0.1 @GarrisonFleet  [Defense]; converted: SiegePower +14.3% @GarrisonFleet  [Influence]
      - ур.2: converted: FleetMaximumHealth +0.2 @GarrisonFleet  [Defense]; converted: SiegePower +28.7% @GarrisonFleet  [Influence]
    - роль Губернатор (G): «Престиж колоний / Укреплённые гарнизоны» / Colonial Prestige / Fortified Garrisons
      - ур.1: kept: PlanetRawPrestige +1 @ColonizedStarSystem/ColonizedPlanet; kept: GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @ColonizedStarSystem; kept: BonusHeroSystemPrestige +1 @ColonizedStarSystem/ColonizedPlanet (hidden)
      - ур.2: kept: PlanetRawPrestige +2 @ColonizedStarSystem/ColonizedPlanet; kept: GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem; kept: BonusHeroSystemPrestige +2 @ColonizedStarSystem/ColonizedPlanet (hidden)
    - роль Сенатор (S): «Имперская идеология / Бронирование флотов» / Imperial Ideology / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.1 @Empire/GarrisonFleet  [Defense]; converted: PlanetRawPrestige +1.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - роль Диктатор (D): «Имперская идеология / Бронирование флотов» / Imperial Ideology / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.08 @Empire/GarrisonFleet  [Defense]; converted: PlanetRawPrestige +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
      - ур.2: converted: FleetMaximumHealth +0.16 @Empire/GarrisonFleet  [Defense]; converted: PlanetRawPrestige +2.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
      - ур.3: converted: FleetMaximumHealth +0.24 @Empire/GarrisonFleet  [Defense]; converted: PlanetRawPrestige +3.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
  - **`HeroSkill04BranchAdventurer`** - Программа астроподготовки / Startrekking Instruction
    - описание: Десятилетия звездоплавания наделили Героя опытом преодоления всевозможных проблем - от преодоления червячных переходов до прохождения вблизи газовых гигантов. Сейчас Герой способен превратить эти бесценные интуиции в тренировочную программу для всех командиров имперского космофлота, существенно повысив тем самым его скоростные характеристики.
    - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +2 @GarrisonFleet; MaximumShipProbeStock +1 @Ship | сенат: ShipMaximumMovement +1 @Empire/GarrisonFleet; MaximumShipProbeStock +1 @Empire/GarrisonFleet/Ship,ShipRoleHero
    - ур.2 [мастерство Curiosity x2]: флот: ShipMaximumMovement +4 @GarrisonFleet; MaximumShipProbeStock +2 @Ship | сенат: ShipMaximumMovement +1 @Empire/GarrisonFleet; MaximumShipProbeStock +1 @Empire/GarrisonFleet/Ship,ShipRoleHero
    - роль Адмирал (A): «Манёвренность» / Maneuverability
      - ур.1: kept: ShipMaximumMovement +2 @GarrisonFleet; kept: MaximumShipProbeStock +1 @Ship; converted: ShipMaximumMovement +4 @GarrisonFleet  [Mobility]
      - ур.2: kept: ShipMaximumMovement +4 @GarrisonFleet; kept: MaximumShipProbeStock +2 @Ship; converted: ShipMaximumMovement +4 @GarrisonFleet  [Mobility]
    - роль Губернатор (G): «Торговые пути» / Trade Routes
      - ур.1: converted: TradingRoutesBonus_Production +140% @ColonizedStarSystem  [Mobility]
      - ур.2: converted: TradingRoutesBonus_Production +200% @ColonizedStarSystem  [Mobility]
    - роль Сенатор (S): «Имперская навигация» / Imperial Navigation
      - ур.1: kept: ShipMaximumMovement +1 @Empire/GarrisonFleet; kept: MaximumShipProbeStock +1 @Empire/GarrisonFleet/Ship,ShipRoleHero; converted: ShipMaximumMovement +3 @Empire/GarrisonFleet  [Mobility]
    - роль Диктатор (D): «Имперская навигация» / Imperial Navigation
      - ур.1: kept: ShipMaximumMovement +1 @Empire/GarrisonFleet  (x1.6 for the dictator); kept: MaximumShipProbeStock +1 @Empire/GarrisonFleet/Ship,ShipRoleHero  (x1.6 for the dictator); converted: ShipMaximumMovement +2.4 @Empire/GarrisonFleet  [Mobility]
      - ур.2: kept: ShipMaximumMovement +1 @Empire/GarrisonFleet  (x1.6 for the dictator); kept: MaximumShipProbeStock +1 @Empire/GarrisonFleet/Ship,ShipRoleHero  (x1.6 for the dictator); converted: ShipMaximumMovement +4.8 @Empire/GarrisonFleet  [Mobility]
      - ур.3: kept: ShipMaximumMovement +1 @Empire/GarrisonFleet  (x2.4 for the dictator); kept: MaximumShipProbeStock +1 @Empire/GarrisonFleet/Ship,ShipRoleHero  (x2.4 for the dictator); converted: ShipMaximumMovement +7 @Empire/GarrisonFleet  [Mobility]
  - **`HeroSkill05BranchAdventurer`** - Пострелят. система наведения на цель / Post-Relativistic Targeting
    - описание: Используя теоретические разработки на основе пострелятивистской математики, Герой способен просчитывать неконвенциональные стратегии ведения огня, извлекая выгоду из использования ноль-гравитационных сценариев и повышая общую огневую мощь Флота.
    - ур.1 [мастерство Command x1]: флот: ShipDamage +0.1 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipDamage +0.2 @GarrisonFleet
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: ShipDamage +0.1 @GarrisonFleet
      - ур.2: kept: ShipDamage +0.2 @GarrisonFleet
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @ColonizedStarSystem  [Offense]
      - ур.2: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.1 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.08 @Empire/GarrisonFleet  [Offense]
      - ур.2: converted: ShipDamage +0.16 @Empire/GarrisonFleet  [Offense]
      - ур.3: converted: ShipDamage +0.24 @Empire/GarrisonFleet  [Offense]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill06BranchAdventurer`** - Щит Ушедших / Shield of the Lost
    - описание: Герой обнаруживает в недрах древнейшего строения, относящегося еще к эпохе до появления Вечных, загадочный объект, давно утративший силу. Однако после контакта с насыщенным Пылью телом Героя, он реактивируется, обнаруживая способность перенаправлять кинетическую энергию, уменьшая Ущерб Кораблю.
    - ур.1 [мастерство Command x1]: флот: ShieldAbsorptionPercent +0.4 @Ship
    - ур.2 [мастерство Command x2]: флот: ShieldAbsorptionPercent +0.8 @Ship
    - роль Адмирал (A): «Крепкие корпуса» / Hardened Hulls
      - ур.1: kept: ShieldAbsorptionPercent +0.4 @Ship
      - ур.2: kept: ShieldAbsorptionPercent +0.8 @Ship
    - роль Губернатор (G): «Укреплённые гарнизоны» / Fortified Garrisons
      - ур.1: converted: MaximumSystemManpower +80%  [Defense]
      - ур.2: converted: MaximumSystemManpower +160%  [Defense]
    - роль Сенатор (S): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.4 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.32 @Empire/GarrisonFleet  [Defense]
      - ур.2: converted: FleetMaximumHealth +0.64 @Empire/GarrisonFleet  [Defense]
      - ур.3: converted: FleetMaximumHealth +0.96 @Empire/GarrisonFleet  [Defense]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchAdventurer`** - Преодоление бюрократизма / Red Tape Scyther
    - описание: Густая чащоба бюрократических норм, регулирующих процедуру распределения Героев, для Героя ничто. Применяя словесный напор, фальсификацию, а иногда и шантаж, Герой может получить назначение в ранее закрытые для него зоны и быть переназначен вдвое быстрее, чем положено по процедуре.
    - ур.1 [мастерство Curiosity x1]: герой: AssignmentCooldownBaseDuration -25% @Hero; CanBeAssignedOutOfTerritory Force 1 @Hero (hidden)
    - роль Адмирал (A): «Манёвренность» / Maneuverability
      - ур.1: converted: ShipMaximumMovement +2 @GarrisonFleet  [Mobility]; hero: AssignmentCooldownBaseDuration -25% @Hero; hero: CanBeAssignedOutOfTerritory Force 1 @Hero (hidden)
    - роль Губернатор (G): «Торговые пути» / Trade Routes
      - ур.1: converted: TradingRoutesBonus_Production +40% @ColonizedStarSystem  [Mobility]; hero: AssignmentCooldownBaseDuration -25% @Hero; hero: CanBeAssignedOutOfTerritory Force 1 @Hero (hidden)
    - роль Сенатор (S): «Имперская навигация» / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +1 @Empire/GarrisonFleet  [Mobility]; hero: AssignmentCooldownBaseDuration -25% @Hero; hero: CanBeAssignedOutOfTerritory Force 1 @Hero (hidden)
    - роль Диктатор (D): «Имперская навигация» / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +1.6 @Empire/GarrisonFleet  [Mobility]; hero: AssignmentCooldownBaseDuration -25% @Hero; hero: CanBeAssignedOutOfTerritory Force 1 @Hero (hidden)

### HeroSkillTreeAdministrator - Навыки Куратора / Overseer skills
Эта ветка содержит специфические навыки Куратора.

Героев с этим деревом: 14 (HeroSophons01, HeroTerrans01, HeroTerrans02, HeroHoratio01, HeroTimeLords01, HeroTimeLords02, HeroUnfallen01, HeroCravers02, HeroCraversPrime, HeroBenthys01, HeroHaroshems01, HeroZvali01, HeroNiris01, HeroAmoeba02)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchAdministrator`** - Агрологистика / Farming Logistics  (tags: ColonizedStarSystem)
    - описание: Оптимизация технологий выращивания растений и сезонного севооборота, не говоря уже об усовершенствовании методов работы на аквафермах, позволяет значительно увеличить производство Пищи в системах, управляемых этим Героем.
    - ур.1 [мастерство Labour x1]: система: SystemGrowth +10 @ColonizedStarSystem; SystemGrowth Addition (10 Multiplication $(OwnedTeemingPlanets)) @ColonizedStarSystem; BonusHeroSystemFood +10 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (10 Multiplication $(OwnedTeemingPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemGrowth +20 @ColonizedStarSystem; SystemGrowth Addition (20 Multiplication $(OwnedTeemingPlanets)) @ColonizedStarSystem; BonusHeroSystemFood +20 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (20 Multiplication $(OwnedTeemingPlanets)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Резервные экипажи» / Reserve Crews
      - ур.1: converted: FleetManpowerRefillRateModifier +0.4 @GarrisonFleet  [Growth]
      - ур.2: converted: FleetManpowerRefillRateModifier +0.8 @GarrisonFleet  [Growth]
    - роль Губернатор (G): «Агрокультура» / Agriculture
      - ур.1: kept: SystemGrowth +10 @ColonizedStarSystem; kept: SystemGrowth Addition (10 Multiplication $(OwnedTeemingPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemFood +10 @ColonizedStarSystem (hidden); kept: BonusHeroSystemFood Addition (10 Multiplication $(OwnedTeemingPlanets)) @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemGrowth +20 @ColonizedStarSystem; kept: SystemGrowth Addition (20 Multiplication $(OwnedTeemingPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemFood +20 @ColonizedStarSystem (hidden); kept: BonusHeroSystemFood Addition (20 Multiplication $(OwnedTeemingPlanets)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Аграрная реформа» / Agrarian Reform
      - ур.1: converted: PlanetRawFood +2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - роль Диктатор (D): «Аграрная реформа» / Agrarian Reform
      - ур.1: converted: PlanetRawFood +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.2: converted: PlanetRawFood +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.3: converted: PlanetRawFood +4.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
  - **`HeroSkill02BranchAdministrator`** - Документы, пожалуйста / Papers Please  (tags: ColonizedStarSystem)
    - описание: Хранение детальных сведений о каждом члене экипажа в составе руководимого им флота позволяет Герою быстро и эффективно проводить мобилизацию даже в тех случаях, когда число бойцов на борту превышает обычное.
    - ур.1 [мастерство Command x1]: флот: FleetManpowerRefillRateModifier +0.2 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: FleetManpowerRefillRateModifier +0.4 @GarrisonFleet
    - роль Адмирал (A): «Полевой ремонт» / Field Repairs
      - ур.1: kept: FleetManpowerRefillRateModifier +0.2 @GarrisonFleet
      - ур.2: kept: FleetManpowerRefillRateModifier +0.4 @GarrisonFleet
    - роль Губернатор (G): «Пополнение войск» / Troop Replenishment
      - ур.1: converted: MaximumSystemManpower +100  [Sustain]
      - ур.2: converted: MaximumSystemManpower +200  [Sustain]
    - роль Сенатор (S): «Службы ремонта» / Repair Corps
      - ур.1: converted: HealthRegenerationRatio +0.1 @Empire/GarrisonFleet  [Sustain]
    - роль Диктатор (D): «Службы ремонта» / Repair Corps
      - ур.1: converted: HealthRegenerationRatio +0.08 @Empire/GarrisonFleet  [Sustain]
      - ур.2: converted: HealthRegenerationRatio +0.16 @Empire/GarrisonFleet  [Sustain]
      - ур.3: converted: HealthRegenerationRatio +0.24 @Empire/GarrisonFleet  [Sustain]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchAdministrator`** - Фанат эффективности / Efficiency Aficionado  (tags: ColonizedStarSystem)
    - описание: Герой демонстрирует маниакальное стремление оптимизировать методологию принятия решений. Индустриальные комплексы и работники под его руководством извлекают выгоду (порой не слишком охотно) из этого постоянного шквала новых блестящих идей.
    - ур.1 [мастерство Labour x1]: система: SystemProduction +10 @ColonizedStarSystem; SystemProduction Addition (10 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; BonusHeroSystemFood +10 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (10 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemProduction +20 @ColonizedStarSystem; SystemProduction Addition (20 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; BonusHeroSystemFood +20 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (20 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Бронепластины / Резервные экипажи» / Plating / Reserve Crews
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.2 @GarrisonFleet  [Production]; converted: FleetManpowerRefillRateModifier +0.2 @GarrisonFleet  [Growth]
      - ур.2: converted: ShipHullPlatingAbsorptionPercent +0.4 @GarrisonFleet  [Production]; converted: FleetManpowerRefillRateModifier +0.4 @GarrisonFleet  [Growth]
    - роль Губернатор (G): «Индустриальный рост / Агрокультура» / Industrial Drive / Agriculture
      - ур.1: kept: SystemProduction +10 @ColonizedStarSystem; kept: SystemProduction Addition (10 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemFood +10 @ColonizedStarSystem (hidden); kept: BonusHeroSystemFood Addition (10 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemProduction +20 @ColonizedStarSystem; kept: SystemProduction Addition (20 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemFood +20 @ColonizedStarSystem (hidden); kept: BonusHeroSystemFood Addition (20 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Государственная индустрия / Аграрная реформа» / State Industry / Agrarian Reform
      - ур.1: converted: PlanetRawIndustry +2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: PlanetRawFood +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - роль Диктатор (D): «Государственная индустрия / Аграрная реформа» / State Industry / Agrarian Reform
      - ур.1: converted: PlanetRawIndustry +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: PlanetRawFood +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.2: converted: PlanetRawIndustry +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: PlanetRawFood +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.3: converted: PlanetRawIndustry +4.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: PlanetRawFood +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
  - **`HeroSkill04BranchAdministrator`** - Канцелярская правка / Clerical Corrections  (tags: ColonizedStarSystem)
    - описание: Меткий глаз иногда в состоянии углядеть неувязки даже в самых запутанных комплексах данных. С течением времени это приводит к периодическому открытию "забытых" авуаров, что выражается в общем приросте доступных активов.
    - ур.1 [мастерство Labour x1]: система: DepositValue +1 @ColonizedStarSystem/Planet/ResourceDeposit | сенат: DepositValue +0.5 @Empire/ColonizedStarSystem/Planet/ResourceDeposit
    - ур.2 [мастерство Labour x2]: система: DepositValue +1 @ColonizedStarSystem/Planet/ResourceDeposit; Happiness Addition (5 Multiplication $(OwnedLuxuryDeposits)) @ColonizedStarSystem | сенат: DepositValue +0.5 @Empire/ColonizedStarSystem/Planet/ResourceDeposit
    - роль Адмирал (A): «Уклонение» / Evasive Tactics
      - ур.1: converted: Evasion +0.2 @GarrisonFleet  [Deposit]
      - ур.2: converted: Evasion +0.2 @GarrisonFleet  [Deposit]
    - роль Губернатор (G): «Добыча ресурсов» / Resource Extraction
      - ур.1: kept: DepositValue +1 @ColonizedStarSystem/Planet/ResourceDeposit; converted: DepositValue +1 @ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
      - ур.2: kept: DepositValue +1 @ColonizedStarSystem/Planet/ResourceDeposit; kept: Happiness Addition (5 Multiplication $(OwnedLuxuryDeposits)) @ColonizedStarSystem; converted: DepositValue +1 @ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - роль Сенатор (S): «Ресурсная политика» / Resource Policy
      - ур.1: kept: DepositValue +0.5 @Empire/ColonizedStarSystem/Planet/ResourceDeposit; converted: DepositValue +0.5 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - роль Диктатор (D): «Ресурсная политика» / Resource Policy
      - ур.1: kept: DepositValue +0.5 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  (x1.6 for the dictator); converted: DepositValue +0.8 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
      - ур.2: kept: DepositValue +0.5 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  (x1.6 for the dictator); converted: DepositValue +0.8 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
      - ур.3: kept: DepositValue +0.5 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  (x2.4 for the dictator); converted: DepositValue +1.2 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchAdministrator`** - Регулятор поведения масс / Crowd Controller  (tags: ColonizedStarSystem)
    - описание: Герой-Куратор правит железной рукой, будь то на корабле или на планете. Когда все работает как часы, жизнь становится несколько менее хаотичной даже в перенаселенных метрополиях самых обширных империй.
    - ур.1 [мастерство Wit x1]: система: OverPopulationPenalties -20% @ColonizedStarSystem; OverColonizationPenalties -20% @ColonizedStarSystem
    - ур.2 [мастерство Wit x2]: система: OverPopulationPenalties -40% @ColonizedStarSystem; OverColonizationPenalties -40% @ColonizedStarSystem
    - роль Адмирал (A): «Дальняя разведка» / Long-Range Scouting
      - ур.1: converted: VisionRange +1 @GarrisonFleet  [Science]
      - ур.2: converted: VisionRange +2 @GarrisonFleet  [Science]
    - роль Губернатор (G): «Научные центры» / Research Centers
      - ур.1: kept: OverPopulationPenalties -20% @ColonizedStarSystem; kept: OverColonizationPenalties -20% @ColonizedStarSystem; converted: SystemResearch +20 @ColonizedStarSystem  [Science]
      - ур.2: kept: OverPopulationPenalties -40% @ColonizedStarSystem; kept: OverColonizationPenalties -40% @ColonizedStarSystem; converted: SystemResearch +40 @ColonizedStarSystem  [Science]
    - роль Сенатор (S): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - роль Диктатор (D): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +0.533 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
      - ур.2: converted: PlanetRawScience +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
      - ур.3: converted: PlanetRawScience +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
  - **`HeroSkill06BranchAdministrator`** - Проводник провидца / Prescient Guide
    - описание: С помощью Пыли Герой может проецировать свое сознание в саму ткань кораблей, что позволяет флоту выбирать наилучший курс и повышает силу щита.
    - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +1 @GarrisonFleet; FleetMaximumShield +0.15 @GarrisonFleet
    - ур.2 [мастерство Curiosity x2]: флот: ShipMaximumMovement +2 @GarrisonFleet; FleetMaximumShield +0.3 @GarrisonFleet
    - роль Адмирал (A): «Крепкие корпуса / Манёвренность» / Hardened Hulls / Maneuverability
      - ур.1: kept: ShipMaximumMovement +1 @GarrisonFleet; kept: FleetMaximumShield +0.15 @GarrisonFleet
      - ур.2: kept: ShipMaximumMovement +2 @GarrisonFleet; kept: FleetMaximumShield +0.3 @GarrisonFleet
    - роль Губернатор (G): «Укреплённые гарнизоны / Торговые пути» / Fortified Garrisons / Trade Routes
      - ур.1: converted: MaximumSystemManpower +30%  [Defense]; converted: TradingRoutesBonus_Production +20% @ColonizedStarSystem  [Mobility]
      - ур.2: converted: MaximumSystemManpower +60%  [Defense]; converted: TradingRoutesBonus_Production +40% @ColonizedStarSystem  [Mobility]
    - роль Сенатор (S): «Бронирование флотов / Имперская навигация» / Fleet Armor Programs / Imperial Navigation
      - ур.1: converted: FleetMaximumHealth +0.15 @Empire/GarrisonFleet  [Defense]; converted: ShipMaximumMovement +1 @Empire/GarrisonFleet  [Mobility]
    - роль Диктатор (D): «Бронирование флотов / Имперская навигация» / Fleet Armor Programs / Imperial Navigation
      - ур.1: converted: FleetMaximumHealth +0.12 @Empire/GarrisonFleet  [Defense]; converted: ShipMaximumMovement +0.8 @Empire/GarrisonFleet  [Mobility]
      - ур.2: converted: FleetMaximumHealth +0.24 @Empire/GarrisonFleet  [Defense]; converted: ShipMaximumMovement +1.6 @Empire/GarrisonFleet  [Mobility]
      - ур.3: converted: FleetMaximumHealth +0.36 @Empire/GarrisonFleet  [Defense]; converted: ShipMaximumMovement +2.4 @Empire/GarrisonFleet  [Mobility]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchAdministrator`** - Бухгалтер / Bean Counter  (tags: ColonizedStarSystem)
    - описание: Настаивая на необходимости жесткого финансового контроля, Герой обеспечивает рост промышленного и сельскохозяйственного производства, а также добычи Пыли, поскольку работники не могут сливать на черный рынок избытки продукции.
    - ур.1 [мастерство Labour x1]: система: SystemProduction +16% @ColonizedStarSystem; SystemGrowth +16% @ColonizedStarSystem; SystemMoney +16% @ColonizedStarSystem
    - роль Адмирал (A): «Бронепластины / Резервные экипажи» / Plating / Reserve Crews
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.213 @GarrisonFleet  [Production]; converted: FleetManpowerRefillRateModifier +0.427 @GarrisonFleet  [Growth]; converted: GarrisonMoneyUpkeep -32% @GarrisonFleet  [Money]
    - роль Губернатор (G): «Индустриальный рост / Агрокультура» / Industrial Drive / Agriculture
      - ур.1: kept: SystemProduction +16% @ColonizedStarSystem; kept: SystemGrowth +16% @ColonizedStarSystem; kept: SystemMoney +16% @ColonizedStarSystem
    - роль Сенатор (S): «Государственная индустрия / Аграрная реформа» / State Industry / Agrarian Reform
      - ур.1: converted: PlanetRawIndustry +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: PlanetRawFood +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; converted: PlanetRawDust +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - роль Диктатор (D): «Государственная индустрия / Аграрная реформа» / State Industry / Agrarian Reform
      - ур.1: converted: PlanetRawIndustry +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: PlanetRawFood +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; converted: PlanetRawDust +1.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]

### HeroSkillTreeCorporate - Навыки Советника / Counselor skills
Эта ветка содержит специфические навыки Советника.

Героев с этим деревом: 15 (HeroSophons03, HeroSophons04, HeroSophons05, HeroTerrans04, HeroHoratio03, HeroHoratio04, HeroTimeLords04, HeroVenetians04, HeroVampirilis04, HeroAmoeba01, HeroDeuyivans01, HeroEyders01, HeroBhagabas01, HeroTerransQuest02, HeroGreenman01)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchCorporate`** - Человек со средствами / Person of Means  (tags: ColonizedStarSystem)
    - описание: Успешный финансист, Герой также вкладывает значительные личные средства в экономическое развитие конкретных систем.
    - ур.1 [мастерство Wit x1]: система: SystemMoney +5 @ColonizedStarSystem; SystemMoney Addition (5 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem; BonusHeroSystemDust +5 @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (5 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemMoney +10 @ColonizedStarSystem; SystemMoney Addition (10 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem; BonusHeroSystemDust +10 @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (10 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Бережливое снабжение» / Frugal Supply
      - ур.1: converted: GarrisonMoneyUpkeep -30% @GarrisonFleet  [Money]
      - ур.2: converted: GarrisonMoneyUpkeep -60% @GarrisonFleet  [Money]
    - роль Губернатор (G): «Торговля и налоги» / Commerce and Taxes
      - ур.1: kept: SystemMoney +5 @ColonizedStarSystem; kept: SystemMoney Addition (5 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemDust +5 @ColonizedStarSystem (hidden); kept: BonusHeroSystemDust Addition (5 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemMoney +10 @ColonizedStarSystem; kept: SystemMoney Addition (10 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemDust +10 @ColonizedStarSystem (hidden); kept: BonusHeroSystemDust Addition (10 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Казначейство» / Treasury
      - ур.1: converted: PlanetRawDust +2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - роль Диктатор (D): «Казначейство» / Treasury
      - ур.1: converted: PlanetRawDust +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
      - ур.2: converted: PlanetRawDust +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
      - ур.3: converted: PlanetRawDust +4.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
  - **`HeroSkill02BranchCorporate`** - Пострелят. система наведения на цель / Post-Relativistic Targeting
    - описание: Используя теоретические разработки на основе пострелятивистской математики, Герой способен просчитывать неконвенциональные стратегии ведения огня, извлекая выгоду из использования ноль-гравитационных сценариев и повышая общую огневую мощь Флота.
    - ур.1 [мастерство Command x1]: флот: ShipDamage +0.05 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipDamage +0.1 @GarrisonFleet
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: ShipDamage +0.05 @GarrisonFleet
      - ур.2: kept: ShipDamage +0.1 @GarrisonFleet
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.05 @ColonizedStarSystem  [Offense]
      - ур.2: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.05 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.04 @Empire/GarrisonFleet  [Offense]
      - ур.2: converted: ShipDamage +0.08 @Empire/GarrisonFleet  [Offense]
      - ур.3: converted: ShipDamage +0.12 @Empire/GarrisonFleet  [Offense]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchCorporate`** - Извлекая пользу из безграничной фантазии / Blue Sky Speculator  (tags: ColonizedStarSystem)
    - описание: Иногда, наибольшие дивиденды может принести ставка на неизвестное. Герой стимулирует корпоративное финансирование научных исследований в своей системе, результатом чего оказывается увеличение показателя Науки.
    - ур.1 [мастерство Wit x1]: система: SystemResearch +30 @ColonizedStarSystem; BonusHeroSystemScience +30 @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemResearch +60 @ColonizedStarSystem; BonusHeroSystemScience +60 @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Дальняя разведка» / Long-Range Scouting
      - ур.1: converted: VisionRange +3 @GarrisonFleet  [Science]
      - ур.2: converted: VisionRange +6 @GarrisonFleet  [Science]
    - роль Губернатор (G): «Научные центры» / Research Centers
      - ур.1: kept: SystemResearch +30 @ColonizedStarSystem; kept: BonusHeroSystemScience +30 @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemResearch +60 @ColonizedStarSystem; kept: BonusHeroSystemScience +60 @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - роль Диктатор (D): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
      - ур.2: converted: PlanetRawScience +4.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
      - ур.3: converted: PlanetRawScience +7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
  - **`HeroSkill04BranchCorporate`** - Неликвидные активы / Non-Liquid Assets
    - описание: Внешний вид кораблей Героя-Советника всегда безупречен. Используя определенную часть своих средств на укрепление переборок и отражающие покрытия, Герой увеличивает тем самым также и эффективность флота в бою.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet; ShipHullPlatingAbsorptionPercent +0.1 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: FleetMaximumHealth +0.2 @GarrisonFleet; ShipHullPlatingAbsorptionPercent +0.2 @GarrisonFleet
    - роль Адмирал (A): «Крепкие корпуса» / Hardened Hulls
      - ур.1: kept: FleetMaximumHealth +0.1 @GarrisonFleet; kept: ShipHullPlatingAbsorptionPercent +0.1 @GarrisonFleet
      - ур.2: kept: FleetMaximumHealth +0.2 @GarrisonFleet; kept: ShipHullPlatingAbsorptionPercent +0.2 @GarrisonFleet
    - роль Губернатор (G): «Укреплённые гарнизоны» / Fortified Garrisons
      - ур.1: converted: MaximumSystemManpower +40%  [Defense]
      - ур.2: converted: MaximumSystemManpower +80%  [Defense]
    - роль Сенатор (S): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.2 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.16 @Empire/GarrisonFleet  [Defense]
      - ур.2: converted: FleetMaximumHealth +0.32 @Empire/GarrisonFleet  [Defense]
      - ур.3: converted: FleetMaximumHealth +0.48 @Empire/GarrisonFleet  [Defense]
  - **`HeroSkill05BranchCorporate`** - Эксперт по пиару / Public Relations Expert  (tags: ColonizedStarSystem)
    - описание: Герой - умелый оратор и притягательная личность. Улучшает репутацию своей империи всюду, куда попадает, или куда доходят его выверенные послания.
    - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +20 @ColonizedStarSystem; SystemEmpirePoint +5% @ColonizedStarSystem; BonusHeroSystemPrestige +20 @ColonizedStarSystem (hidden) | сенат: EmpireEmpirePoint Addition (50 Multiplication $(NumberOfTradingCompanies)) @Empire; BonusHeroSystemPrestige Addition (50 Multiplication $(NumberOfTradingCompanies)) @Empire (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemEmpirePoint +40 @ColonizedStarSystem; SystemEmpirePoint +10% @ColonizedStarSystem; BonusHeroSystemPrestige +40 @ColonizedStarSystem (hidden) | сенат: EmpireEmpirePoint Addition (50 Multiplication $(NumberOfTradingCompanies)) @Empire; BonusHeroSystemPrestige Addition (50 Multiplication $(NumberOfTradingCompanies)) @Empire (hidden)
    - роль Адмирал (A): «Осадное мастерство» / Siegecraft
      - ур.1: converted: SiegePower +50% @GarrisonFleet  [Influence]
      - ур.2: converted: SiegePower +100% @GarrisonFleet  [Influence]
    - роль Губернатор (G): «Престиж колоний» / Colonial Prestige
      - ур.1: kept: SystemEmpirePoint +20 @ColonizedStarSystem; kept: SystemEmpirePoint +5% @ColonizedStarSystem; kept: BonusHeroSystemPrestige +20 @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemEmpirePoint +40 @ColonizedStarSystem; kept: SystemEmpirePoint +10% @ColonizedStarSystem; kept: BonusHeroSystemPrestige +40 @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Имперская идеология» / Imperial Ideology
      - ур.1: kept: EmpireEmpirePoint Addition (50 Multiplication $(NumberOfTradingCompanies)) @Empire; kept: BonusHeroSystemPrestige Addition (50 Multiplication $(NumberOfTradingCompanies)) @Empire (hidden); converted: PlanetRawPrestige +5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - роль Диктатор (D): «Имперская идеология» / Imperial Ideology
      - ур.1: kept: EmpireEmpirePoint Addition (50 Multiplication $(NumberOfTradingCompanies)) @Empire  (x1.6 for the dictator); kept: BonusHeroSystemPrestige Addition (50 Multiplication $(NumberOfTradingCompanies)) @Empire (hidden)  (x1.6 for the dictator); converted: PlanetRawPrestige +4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
      - ур.2: kept: EmpireEmpirePoint Addition (50 Multiplication $(NumberOfTradingCompanies)) @Empire  (x1.6 for the dictator); kept: BonusHeroSystemPrestige Addition (50 Multiplication $(NumberOfTradingCompanies)) @Empire (hidden)  (x1.6 for the dictator); converted: PlanetRawPrestige +8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
      - ур.3: kept: EmpireEmpirePoint Addition (50 Multiplication $(NumberOfTradingCompanies)) @Empire  (x2.4 for the dictator); kept: BonusHeroSystemPrestige Addition (50 Multiplication $(NumberOfTradingCompanies)) @Empire (hidden)  (x2.4 for the dictator); converted: PlanetRawPrestige +12 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill06BranchCorporate`** - Сенсорные системы / Sensitive Systems
    - описание: Глубоко проникая в физические и математические аспекты распространения волн, Герой развивает и совершенствует сенсорные системы дальнего радиуса действия. Это не только увеличивает поле доступного восприятия, но и помогает избегать контактов со всякого рода космическими объектами, удешевляя тем самым обслуживание кораблей.
    - ур.1 [мастерство Curiosity x1]: флот: VisionRange +4 @GarrisonFleet; GarrisonMoneyUpkeep -20% @GarrisonFleet
    - ур.2 [мастерство Curiosity x2]: флот: VisionRange +8 @GarrisonFleet; GarrisonMoneyUpkeep -40% @GarrisonFleet
    - роль Адмирал (A): «Манёвренность / Экономия флота» / Maneuverability / Fleet Economy
      - ур.1: kept: VisionRange +4 @GarrisonFleet; kept: GarrisonMoneyUpkeep -20% @GarrisonFleet
      - ур.2: kept: VisionRange +8 @GarrisonFleet; kept: GarrisonMoneyUpkeep -40% @GarrisonFleet
    - роль Губернатор (G): «Торговые пути / Бережливое управление» / Trade Routes / Frugal Administration
      - ур.1: converted: TradingRoutesBonus_Production +80% @ColonizedStarSystem  [Mobility]; converted: SystemMoneyUpkeep -20% @ColonizedStarSystem  [Upkeep]
      - ур.2: converted: TradingRoutesBonus_Production +160% @ColonizedStarSystem  [Mobility]; converted: SystemMoneyUpkeep -40% @ColonizedStarSystem  [Upkeep]
    - роль Сенатор (S): «Имперская навигация / Режим экономии» / Imperial Navigation / Austerity
      - ур.1: converted: ShipMaximumMovement +4 @Empire/GarrisonFleet  [Mobility]; converted: SystemMoneyUpkeep -20% @Empire/ColonizedStarSystem  [Upkeep]
    - роль Диктатор (D): «Имперская навигация / Режим экономии» / Imperial Navigation / Austerity
      - ур.1: converted: ShipMaximumMovement +3.2 @Empire/GarrisonFleet  [Mobility]; converted: SystemMoneyUpkeep -16% @Empire/ColonizedStarSystem  [Upkeep]
      - ур.2: converted: ShipMaximumMovement +6 @Empire/GarrisonFleet  [Mobility]; converted: SystemMoneyUpkeep -32% @Empire/ColonizedStarSystem  [Upkeep]
      - ур.3: converted: ShipMaximumMovement +10 @Empire/GarrisonFleet  [Mobility]; converted: SystemMoneyUpkeep -48% @Empire/ColonizedStarSystem  [Upkeep]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchCorporate`** - Неукоснительно радуясь / The Joy Initiative  (tags: ColonizedStarSystem)
    - описание: Одобрение - показатель, не очень-то поддающийся измерению. Когда системой командует могущественный Герой-Советник, кто может сказать, происходит ли Одобрение населения от истинной любви или от боязни последствий?
    - ур.1 [мастерство Wit x1]: система: Happiness Force 100 @ColonizedStarSystem; SystemMoneyUpkeep -20% @ColonizedStarSystem
    - роль Адмирал (A): «Экономия флота» / Fleet Economy
      - ур.1: converted: GarrisonMoneyUpkeep -20% @GarrisonFleet  [Upkeep]
    - роль Губернатор (G): «Бережливое управление» / Frugal Administration
      - ур.1: kept: Happiness Force 100 @ColonizedStarSystem; kept: SystemMoneyUpkeep -20% @ColonizedStarSystem
    - роль Сенатор (S): «Режим экономии» / Austerity
      - ур.1: converted: SystemMoneyUpkeep -10% @Empire/ColonizedStarSystem  [Upkeep]
    - роль Диктатор (D): «Режим экономии» / Austerity
      - ур.1: converted: SystemMoneyUpkeep -16% @Empire/ColonizedStarSystem  [Upkeep]

### HeroSkillTreeSophons - Навыки Софонов / Sophon skills
Эта ветка содержит специфические навыки Софонов.

Героев с этим деревом: 5 (HeroSophons01, HeroSophons02, HeroSophons03, HeroSophons04, HeroSophons05)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchSophons`** - Академическое рецензирование / Peer Review  (tags: ColonizedStarSystem)
    - описание: Герой проявляет интерес к научным исследованиям в подвластной ему системе, внося свой вклад в обеспечение должного уровня функционирования научно-образовательной системы.
    - ур.1 [мастерство Wit x1]: система: SystemResearch +10 @ColonizedStarSystem; BonusHeroSystemScience +10 @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemResearch +10 @ColonizedStarSystem; SystemResearch Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemScience +10 @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Дальняя разведка» / Long-Range Scouting
      - ур.1: converted: VisionRange +1 @GarrisonFleet  [Science]
      - ур.2: converted: VisionRange +1 @GarrisonFleet  [Science]
    - роль Губернатор (G): «Научные центры» / Research Centers
      - ур.1: kept: SystemResearch +10 @ColonizedStarSystem; kept: BonusHeroSystemScience +10 @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemResearch +10 @ColonizedStarSystem; kept: SystemResearch Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; kept: BonusHeroSystemScience +10 @ColonizedStarSystem (hidden); kept: BonusHeroSystemScience Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +0.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - роль Диктатор (D): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
      - ур.2: converted: PlanetRawScience +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
      - ур.3: converted: PlanetRawScience +1.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
  - **`HeroSkill02BranchSophons`** - Галактическое чутье / Galactic Sense
    - описание: Чувствуя Пыль, плывущую в пространстве Галактики, Герой способен "видеть" далеко за пределами возможностей физических устройств, увеличивая дальность обзора и полета своего флота.
    - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +1 @GarrisonFleet; VisionRange +1 @GarrisonFleet
    - ур.2 [мастерство Curiosity x2]: флот: ShipMaximumMovement +2 @GarrisonFleet; VisionRange +2 @GarrisonFleet
    - роль Адмирал (A): «Манёвренность» / Maneuverability
      - ур.1: kept: ShipMaximumMovement +1 @GarrisonFleet; kept: VisionRange +1 @GarrisonFleet
      - ур.2: kept: ShipMaximumMovement +2 @GarrisonFleet; kept: VisionRange +2 @GarrisonFleet
    - роль Губернатор (G): «Торговые пути» / Trade Routes
      - ур.1: converted: TradingRoutesBonus_Production +40% @ColonizedStarSystem  [Mobility]
      - ур.2: converted: TradingRoutesBonus_Production +80% @ColonizedStarSystem  [Mobility]
    - роль Сенатор (S): «Имперская навигация» / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +2 @Empire/GarrisonFleet  [Mobility]
    - роль Диктатор (D): «Имперская навигация» / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +1.6 @Empire/GarrisonFleet  [Mobility]
      - ур.2: converted: ShipMaximumMovement +3.2 @Empire/GarrisonFleet  [Mobility]
      - ур.3: converted: ShipMaximumMovement +4.8 @Empire/GarrisonFleet  [Mobility]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchSophons`** - Жгучий свет / The Burning Light
    - описание: Тщательно проанализировав критические энергетические потребности флота, Герой может перенастроить системы энергетического вооружения таким образом, чтобы они производили больше мощности, тем самым увеличивая урон от них.
    - ур.1 [мастерство Command x1]: флот: ShipDamageEnergy +0.2 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipDamageEnergy +0.4 @GarrisonFleet
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: ShipDamageEnergy +0.2 @GarrisonFleet
      - ур.2: kept: ShipDamageEnergy +0.4 @GarrisonFleet
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem  [Offense]
      - ур.2: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.4 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.2 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.16 @Empire/GarrisonFleet  [Offense]
      - ур.2: converted: ShipDamage +0.32 @Empire/GarrisonFleet  [Offense]
      - ур.3: converted: ShipDamage +0.48 @Empire/GarrisonFleet  [Offense]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill04BranchSophons`** - Прикладной нонконформист / Applied Nonconformist  (tags: ColonizedStarSystem)
    - описание: Смело идти туда, где раньше не ступала нога Софона. Софон, направленный в систему, полную аномалий, проведет несколько лет, исследуя все ее особенности. Интерес и одобрение населения будут увеличиваться, пока продолжается его работа.
    - ур.1 [мастерство Wit x1]: система: Happiness Addition (8 Multiplication $(AnomalyCount)) @ColonizedStarSystem | сенат: Happiness +5 @Empire/ColonizedStarSystem
    - ур.2 [мастерство Wit x2]: система: Happiness Addition (16 Multiplication $(AnomalyCount)) @ColonizedStarSystem | сенат: Happiness +5 @Empire/ColonizedStarSystem
    - роль Адмирал (A): «Усиленные щиты» / Reinforced Shields
      - ур.1: converted: FleetMaximumShield +0.25 @GarrisonFleet  [Happiness]
      - ур.2: converted: FleetMaximumShield +0.25 @GarrisonFleet  [Happiness]
    - роль Губернатор (G): «Общественный порядок» / Public Order
      - ур.1: kept: Happiness Addition (8 Multiplication $(AnomalyCount)) @ColonizedStarSystem; converted: Happiness +10 @ColonizedStarSystem  [Happiness]
      - ур.2: kept: Happiness Addition (16 Multiplication $(AnomalyCount)) @ColonizedStarSystem; converted: Happiness +10 @ColonizedStarSystem  [Happiness]
    - роль Сенатор (S): «Всеобщее благополучие» / General Welfare
      - ур.1: kept: Happiness +5 @Empire/ColonizedStarSystem
    - роль Диктатор (D): «Всеобщее благополучие» / General Welfare
      - ур.1: kept: Happiness +5 @Empire/ColonizedStarSystem  (x1.6 for the dictator)
      - ур.2: kept: Happiness +5 @Empire/ColonizedStarSystem  (x1.6 for the dictator)
      - ур.3: kept: Happiness +5 @Empire/ColonizedStarSystem  (x2.4 for the dictator)
  - **`HeroSkill05BranchSophons`** - Небесная механика / Celestial Mechanics
    - описание: Используя свое "ощущение" влияния Пыли на звездные струны, Герой лучше может направлять свой флот по путям наименьшего сопротивления, увеличивая дальность его полета.
    - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +6 @GarrisonFleet
    - роль Адмирал (A): «Манёвренность» / Maneuverability
      - ур.1: kept: ShipMaximumMovement +6 @GarrisonFleet
    - роль Губернатор (G): «Торговые пути» / Trade Routes
      - ур.1: converted: TradingRoutesBonus_Production +120% @ColonizedStarSystem  [Mobility]
    - роль Сенатор (S): «Имперская навигация» / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +3 @Empire/GarrisonFleet  [Mobility]
    - роль Диктатор (D): «Имперская навигация» / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +4.8 @Empire/GarrisonFleet  [Mobility]
  - **`HeroSkill06BranchSophons`** - Миниатюрные зонды / Slim Probe Model
    - описание: Полностью трансформировав всю конструкцию зонда, Герой разрабатывает улучшенную модель, которая вдвое меньше размером, потребляет вчетверо меньше энергии и при этом в десять раз симпатичнее. Результат? Запас зондов увеличивается вдвое и, с учетом прорыва в энергоэффективности, щиты тоже становятся лучше!
    - ур.1 [мастерство Curiosity x1]: флот: FleetMaximumShield +0.2 @GarrisonFleet; MaximumShipProbeStock +2 @GarrisonFleet/Ship,ShipRoleExploration
    - ур.2 [мастерство Curiosity x2]: флот: FleetMaximumShield +0.4 @GarrisonFleet; MaximumShipProbeStock +4 @GarrisonFleet/Ship,ShipRoleExploration
    - роль Адмирал (A): «Крепкие корпуса / Манёвренность» / Hardened Hulls / Maneuverability
      - ур.1: kept: FleetMaximumShield +0.2 @GarrisonFleet; kept: MaximumShipProbeStock +2 @GarrisonFleet/Ship,ShipRoleExploration
      - ур.2: kept: FleetMaximumShield +0.4 @GarrisonFleet; kept: MaximumShipProbeStock +4 @GarrisonFleet/Ship,ShipRoleExploration
    - роль Губернатор (G): «Укреплённые гарнизоны / Торговые пути» / Fortified Garrisons / Trade Routes
      - ур.1: converted: MaximumSystemManpower +40%  [Defense]; converted: TradingRoutesBonus_Production +40% @ColonizedStarSystem  [Mobility]
      - ур.2: converted: MaximumSystemManpower +80%  [Defense]; converted: TradingRoutesBonus_Production +80% @ColonizedStarSystem  [Mobility]
    - роль Сенатор (S): «Бронирование флотов / Имперская навигация» / Fleet Armor Programs / Imperial Navigation
      - ур.1: converted: FleetMaximumHealth +0.2 @Empire/GarrisonFleet  [Defense]; converted: ShipMaximumMovement +2 @Empire/GarrisonFleet  [Mobility]
    - роль Диктатор (D): «Бронирование флотов / Имперская навигация» / Fleet Armor Programs / Imperial Navigation
      - ур.1: converted: FleetMaximumHealth +0.16 @Empire/GarrisonFleet  [Defense]; converted: ShipMaximumMovement +1.6 @Empire/GarrisonFleet  [Mobility]
      - ур.2: converted: FleetMaximumHealth +0.32 @Empire/GarrisonFleet  [Defense]; converted: ShipMaximumMovement +3.2 @Empire/GarrisonFleet  [Mobility]
      - ур.3: converted: FleetMaximumHealth +0.48 @Empire/GarrisonFleet  [Defense]; converted: ShipMaximumMovement +4.8 @Empire/GarrisonFleet  [Mobility]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchSophons`** - Гениократ / Geniocrat  (tags: ColonizedStarSystem)
    - описание: Герой принимает меры против стагнации в политической и академической сферах, привнося определенную долю своей организаторской логики в функционирование данной системы, отчего показатель Науки буквально взлетает.
    - ур.1 [мастерство Wit x1]: система: SystemResearch +60% @ColonizedStarSystem
    - роль Адмирал (A): «Дальняя разведка» / Long-Range Scouting
      - ур.1: converted: VisionRange +6 @GarrisonFleet  [Science]
    - роль Губернатор (G): «Научные центры» / Research Centers
      - ур.1: kept: SystemResearch +60% @ColonizedStarSystem
    - роль Сенатор (S): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - роль Диктатор (D): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +4.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]

### HeroSkillTreeCravers - Навыки Пожирателей / Craver skills
Эта ветка содержит специфические навыки Пожирателей.

Героев с этим деревом: 5 (HeroCravers01, HeroCravers02, HeroCravers03, HeroCravers04, HeroCraversPrime)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchCravers`** - Эффект грубой силы / Blunt Force Trauma
    - описание: Демонстрируя свойственную Пожирателям нестандартность мышления, Герой разграбляет руины давно исчезнувшей цивилизации и крадет для своего личного флота оружие, позволяющее повысить причиняемый Кинетический ущерб и ущерб, наносимый Снарядами.
    - ур.1 [мастерство Command x1]: флот: ShipDamageProjectile +0.1 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipDamageProjectile +0.2 @GarrisonFleet
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: ShipDamageProjectile +0.1 @GarrisonFleet
      - ур.2: kept: ShipDamageProjectile +0.2 @GarrisonFleet
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @ColonizedStarSystem  [Offense]
      - ур.2: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.1 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.08 @Empire/GarrisonFleet  [Offense]
      - ур.2: converted: ShipDamage +0.16 @Empire/GarrisonFleet  [Offense]
      - ур.3: converted: ShipDamage +0.24 @Empire/GarrisonFleet  [Offense]
  - **`HeroSkill02BranchCravers`** - Принудительный труд / Forced Labor  (tags: ColonizedStarSystem)
    - описание: Герой отдает приказ о захвате и содержании в неволе представителей других рас, которых силой принуждают работать на заводах и фермах. Их отпустят и вознаградят за тяжкий труд… но не прежде, чем из планет, на которых их эксплуатируют, будут дочиста выкачаны все ресурсы...
    - ур.1 [мастерство Labour x1]: система: SystemProduction +5 @ColonizedStarSystem; SystemGrowth Addition (10 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; BonusHeroSystemIndustry +5 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (10 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemProduction +10 @ColonizedStarSystem; SystemGrowth Addition (20 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; BonusHeroSystemIndustry +10 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (20 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Бронепластины» / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.2 @GarrisonFleet  [Production]
      - ур.2: converted: ShipHullPlatingAbsorptionPercent +0.4 @GarrisonFleet  [Production]
    - роль Губернатор (G): «Индустриальный рост» / Industrial Drive
      - ур.1: kept: SystemProduction +5 @ColonizedStarSystem; kept: SystemGrowth Addition (10 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemIndustry +5 @ColonizedStarSystem (hidden); kept: BonusHeroSystemFood Addition (10 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemProduction +10 @ColonizedStarSystem; kept: SystemGrowth Addition (20 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemIndustry +10 @ColonizedStarSystem (hidden); kept: BonusHeroSystemFood Addition (20 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - роль Диктатор (D): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
      - ур.2: converted: PlanetRawIndustry +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
      - ур.3: converted: PlanetRawIndustry +4.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchCravers`** - Адаптация к условиям глубокого космоса / Deep Space Adaptation
    - описание: Члены экипажей кораблей, входящих во флот Героя, проходят программу спецподготовки, чтобы адаптировать их физические возможности к перемещениям в глубоком космосе. Это повышает их эффективность при обслуживании корабельных систем в экстремальных условиях опасных районов.
    - ур.1 [мастерство Curiosity x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet; MaximumShipProbeStock +1 @GarrisonFleet/Ship,ShipRoleExploration
    - ур.2 [мастерство Curiosity x2]: флот: FleetMaximumHealth +0.2 @GarrisonFleet; MaximumShipProbeStock +2 @GarrisonFleet/Ship,ShipRoleExploration
    - роль Адмирал (A): «Крепкие корпуса / Манёвренность» / Hardened Hulls / Maneuverability
      - ур.1: kept: FleetMaximumHealth +0.1 @GarrisonFleet; kept: MaximumShipProbeStock +1 @GarrisonFleet/Ship,ShipRoleExploration
      - ур.2: kept: FleetMaximumHealth +0.2 @GarrisonFleet; kept: MaximumShipProbeStock +2 @GarrisonFleet/Ship,ShipRoleExploration
    - роль Губернатор (G): «Укреплённые гарнизоны / Торговые пути» / Fortified Garrisons / Trade Routes
      - ур.1: converted: MaximumSystemManpower +20%  [Defense]; converted: TradingRoutesBonus_Production +20% @ColonizedStarSystem  [Mobility]
      - ур.2: converted: MaximumSystemManpower +40%  [Defense]; converted: TradingRoutesBonus_Production +40% @ColonizedStarSystem  [Mobility]
    - роль Сенатор (S): «Бронирование флотов / Имперская навигация» / Fleet Armor Programs / Imperial Navigation
      - ур.1: converted: FleetMaximumHealth +0.1 @Empire/GarrisonFleet  [Defense]; converted: ShipMaximumMovement +1 @Empire/GarrisonFleet  [Mobility]
    - роль Диктатор (D): «Бронирование флотов / Имперская навигация» / Fleet Armor Programs / Imperial Navigation
      - ур.1: converted: FleetMaximumHealth +0.08 @Empire/GarrisonFleet  [Defense]; converted: ShipMaximumMovement +0.8 @Empire/GarrisonFleet  [Mobility]
      - ур.2: converted: FleetMaximumHealth +0.16 @Empire/GarrisonFleet  [Defense]; converted: ShipMaximumMovement +1.6 @Empire/GarrisonFleet  [Mobility]
      - ур.3: converted: FleetMaximumHealth +0.24 @Empire/GarrisonFleet  [Defense]; converted: ShipMaximumMovement +2.4 @Empire/GarrisonFleet  [Mobility]
  - **`HeroSkill04BranchCravers`** - Счастливый случай / Lucky Windfall
    - описание: Организовав общесистемную лотерею с огромными выплатами, Герой получает колоссальные возможности для воздействия на общественное мнение.
    - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +10% @ColonizedStarSystem
    - роль Адмирал (A): «Осадное мастерство» / Siegecraft
      - ур.1: converted: SiegePower +20% @GarrisonFleet  [Influence]
    - роль Губернатор (G): «Престиж колоний» / Colonial Prestige
      - ур.1: kept: SystemEmpirePoint +10% @ColonizedStarSystem
    - роль Сенатор (S): «Имперская идеология» / Imperial Ideology
      - ур.1: converted: PlanetRawPrestige +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - роль Диктатор (D): «Имперская идеология» / Imperial Ideology
      - ур.1: converted: PlanetRawPrestige +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchCravers`** - Запах крови / Blood Scent
    - описание: Опыт, полученный в сотне битв, наделяет Героя мгновенной реакцией, основанной на интуитивной способности находить слабые места вражеского флота, что усиливает поражающую мощь его кораблей.
    - ур.1 [мастерство Command x1]: флот: ShipHullPlatingPenetrationFactor +0.4 @GarrisonFleet
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: ShipHullPlatingPenetrationFactor +0.4 @GarrisonFleet
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.4 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.2 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.32 @Empire/GarrisonFleet  [Offense]
  - **`HeroSkill06BranchCravers`** - Системы туннельной защиты / Tunneling Defense  (tags: ColonizedStarSystem)
    - описание: Герой в прямом смысле вгрызается в недра, создавая накануне битвы сеть подземных тоннелей. Обеспечив достижение успеха подобной стратегией, раса Пожирателей затем применяет ее по всей империи.
    - ур.1 [мастерство Labour x1]: система: GroundBattleDefenderTroopsMaxHealthMultiplier +0.25 | сенат: GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @Empire/ColonizedStarSystem
    - ур.2 [мастерство Labour x2]: система: GroundBattleDefenderTroopsMaxHealthMultiplier +0.5 | сенат: GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @Empire/ColonizedStarSystem
    - роль Адмирал (A): «Крепкие корпуса» / Hardened Hulls
      - ур.1: converted: FleetMaximumHealth +0.45 @GarrisonFleet  [Defense]
      - ур.2: converted: FleetMaximumHealth +0.7 @GarrisonFleet  [Defense]
    - роль Губернатор (G): «Укреплённые гарнизоны» / Fortified Garrisons
      - ур.1: kept: GroundBattleDefenderTroopsMaxHealthMultiplier +0.25; converted: MaximumSystemManpower +40%  [Defense]
      - ур.2: kept: GroundBattleDefenderTroopsMaxHealthMultiplier +0.5; converted: MaximumSystemManpower +40%  [Defense]
    - роль Сенатор (S): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: kept: GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @Empire/ColonizedStarSystem; converted: FleetMaximumHealth +0.25 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: kept: GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @Empire/ColonizedStarSystem  (x1.6 for the dictator); converted: FleetMaximumHealth +0.2 @Empire/GarrisonFleet  [Defense]
      - ур.2: kept: GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @Empire/ColonizedStarSystem  (x1.6 for the dictator); converted: FleetMaximumHealth +0.4 @Empire/GarrisonFleet  [Defense]
      - ур.3: kept: GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @Empire/ColonizedStarSystem  (x2.4 for the dictator); converted: FleetMaximumHealth +0.6 @Empire/GarrisonFleet  [Defense]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchCravers`** - Бесконечное потребление / Endless Consumption  (tags: ColonizedStarSystem)
    - описание: В качестве парадоксального поворота эволюции, Герой запускает спровоцированную Пылью реакцию, которая увеличивает производство ресурсов в системе, управляемой Пожирателями, но также и ускоряет процесс их истощения.
    - ур.1 [мастерство Labour x1]: система: PlanetDepletionPerTurn +8 @ColonizedStarSystem/Planet; PlanetHeroFIDSIPercent +0.5 @ColonizedStarSystem/ColonizedPlanet,!PlanetDepletionMalus
    - роль Адмирал (A): «Бронепластины» / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.01 @GarrisonFleet  [Production]
    - роль Губернатор (G): «Индустриальный рост» / Industrial Drive
      - ур.1: kept: PlanetDepletionPerTurn +8 @ColonizedStarSystem/Planet; kept: PlanetHeroFIDSIPercent +0.5 @ColonizedStarSystem/ColonizedPlanet,!PlanetDepletionMalus
    - роль Сенатор (S): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +0.05 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - роль Диктатор (D): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +0.08 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]

### HeroSkillTreeTerrans - Навыки Единой Империи / United Empire Skills
Эта ветка включает навыки, специфические для Единой Империи.

Героев с этим деревом: 8 (HeroTerrans01, HeroTerrans02, HeroTerrans03, HeroTerrans04, HeroTerrans05, HeroTerransQuest01, HeroTerransQuest02, HeroTerransQuest03)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchTerrans`** - Защитный строй / Protective Formations
    - описание: Во время битвы Герой выстраивает флот таким образом, что корабли становятся менее восприимчивы к артиллерийскому огню.
    - ур.1 [мастерство Command x1]: флот: ShipShieldAbsorptionPercent +0.1 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipShieldAbsorptionPercent +0.2 @GarrisonFleet
    - роль Адмирал (A): «Крепкие корпуса» / Hardened Hulls
      - ур.1: kept: ShipShieldAbsorptionPercent +0.1 @GarrisonFleet
      - ур.2: kept: ShipShieldAbsorptionPercent +0.2 @GarrisonFleet
    - роль Губернатор (G): «Укреплённые гарнизоны» / Fortified Garrisons
      - ур.1: converted: MaximumSystemManpower +20%  [Defense]
      - ур.2: converted: MaximumSystemManpower +40%  [Defense]
    - роль Сенатор (S): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.1 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.08 @Empire/GarrisonFleet  [Defense]
      - ур.2: converted: FleetMaximumHealth +0.16 @Empire/GarrisonFleet  [Defense]
      - ур.3: converted: FleetMaximumHealth +0.24 @Empire/GarrisonFleet  [Defense]
  - **`HeroSkill02BranchTerrans`** - Воодушевляющий оратор / Rousing Orator  (tags: ColonizedStarSystem)
    - описание: Герой может формировать и использовать общественное мнение, с легкостью воодушевляя людей во время массовых митингов с помощью провокационных лозунгов и эмоциональных аргументов.
    - ур.1 [мастерство Wit x1]: система: PlanetRawPrestige +1 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetPrestige Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - ур.2 [мастерство Wit x2]: система: PlanetRawPrestige +2 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - роль Адмирал (A): «Осадное мастерство» / Siegecraft
      - ур.1: converted: SiegePower +13.3% @GarrisonFleet  [Influence]
      - ур.2: converted: SiegePower +26.7% @GarrisonFleet  [Influence]
    - роль Губернатор (G): «Престиж колоний» / Colonial Prestige
      - ур.1: kept: PlanetRawPrestige +1 @ColonizedStarSystem/ColonizedPlanet; kept: BonusHeroPlanetPrestige Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
      - ур.2: kept: PlanetRawPrestige +2 @ColonizedStarSystem/ColonizedPlanet; kept: BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - роль Сенатор (S): «Имперская идеология» / Imperial Ideology
      - ур.1: converted: PlanetRawPrestige +1.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - роль Диктатор (D): «Имперская идеология» / Imperial Ideology
      - ур.1: converted: PlanetRawPrestige +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
      - ур.2: converted: PlanetRawPrestige +2.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
      - ур.3: converted: PlanetRawPrestige +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchTerrans`** - Кровь и земля / Blood and Land  (tags: ColonizedStarSystem)
    - описание: Как истинный патриот, Герой пропагандирует представление о том, что личность каждого человека глубоко связана с его родиной, тем самым значительно повышая количество призывников.
    - ур.1 [мастерство Labour x1]: система: MaximumSystemManpower +20%
    - ур.2 [мастерство Labour x2]: система: MaximumSystemManpower +40%
    - роль Адмирал (A): «Набор экипажей» / Crew Recruitment
      - ур.1: converted: MaximumShipManpower +12.5% @GarrisonFleet/Ship  [Manpower]
      - ур.2: converted: MaximumShipManpower +25% @GarrisonFleet/Ship  [Manpower]
    - роль Губернатор (G): «Мобилизация» / Mobilization
      - ур.1: kept: MaximumSystemManpower +20%
      - ур.2: kept: MaximumSystemManpower +40%
    - роль Сенатор (S): «Всеобщая мобилизация» / National Mobilization
      - ур.1: converted: MaximumEmpireManpowerStock +25% @Empire  [Manpower]
    - роль Диктатор (D): «Всеобщая мобилизация» / National Mobilization
      - ур.1: converted: MaximumEmpireManpowerStock +20% @Empire  [Manpower]
      - ур.2: converted: MaximumEmpireManpowerStock +40% @Empire  [Manpower]
      - ур.3: converted: MaximumEmpireManpowerStock +60% @Empire  [Manpower]
  - **`HeroSkill04BranchTerrans`** - Дух первенства / Pioneering Spirit  (tags: ColonizedStarSystem)
    - описание: Привнося дух первопроходцев в систему, куда он назначен, Герой вдохновляет население работать больше и дольше, увеличивая объемы промышленного производства. Этот эффект распространяется на пограничные миры.
    - ур.1 [мастерство Labour x1]: система: SystemProduction +10 @ColonizedStarSystem; SystemProduction Addition (10 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem; BonusHeroSystemIndustry +10 @ColonizedStarSystem (hidden); BonusHeroSystemIndustry Addition (10 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemProduction +20 @ColonizedStarSystem; SystemProduction Addition (20 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem; BonusHeroSystemIndustry +20 @ColonizedStarSystem (hidden); BonusHeroSystemIndustry Addition (10 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Бронепластины» / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.4 @GarrisonFleet  [Production]
      - ур.2: converted: ShipHullPlatingAbsorptionPercent +0.8 @GarrisonFleet  [Production]
    - роль Губернатор (G): «Индустриальный рост» / Industrial Drive
      - ур.1: kept: SystemProduction +10 @ColonizedStarSystem; kept: SystemProduction Addition (10 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemIndustry +10 @ColonizedStarSystem (hidden); kept: BonusHeroSystemIndustry Addition (10 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemProduction +20 @ColonizedStarSystem; kept: SystemProduction Addition (20 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemIndustry +20 @ColonizedStarSystem (hidden); kept: BonusHeroSystemIndustry Addition (10 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - роль Диктатор (D): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
      - ур.2: converted: PlanetRawIndustry +6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
      - ур.3: converted: PlanetRawIndustry +10 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchTerrans`** - Военный стержень / Military Spine
    - описание: Обладая способностью определять наиболее способных командиров на флоте, Герой может делегировать тактические решения, увеличивая возможности наступления.
    - ур.1 [мастерство Command x1]: флот: ShipDamage Addition (0.015 Multiplication $(CommandPoints)) @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipDamage Addition (0.02 Multiplication $(CommandPoints)) @GarrisonFleet
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: ShipDamage Addition (0.015 Multiplication $(CommandPoints)) @GarrisonFleet; converted: ShipDamage +0.1 @GarrisonFleet  [Offense]
      - ур.2: kept: ShipDamage Addition (0.02 Multiplication $(CommandPoints)) @GarrisonFleet; converted: ShipDamage +0.2 @GarrisonFleet  [Offense]
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @ColonizedStarSystem  [Offense]
      - ур.2: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.1 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.053 @Empire/GarrisonFleet  [Offense]
      - ур.2: converted: ShipDamage +0.107 @Empire/GarrisonFleet  [Offense]
      - ур.3: converted: ShipDamage +0.24 @Empire/GarrisonFleet  [Offense]
  - **`HeroSkill06BranchTerrans`** - Метод старателя / Prospector's Touch  (tags: ColonizedStarSystem)
    - описание: Герой разведывает драгоценные минералы, словно ищейка, идущая по следу своей добычи. Во многом этому способствуют тщательный анализ геологии, климата и химии, но и сверхъестественное чувство Пыли, безусловно, играет не последнюю роль.
    - ур.1 [мастерство Labour x1]: система: DepositValue +2 @ColonizedStarSystem/Planet/ResourceDeposit,ResourceTypeStrategic
    - роль Адмирал (A): «Уклонение» / Evasive Tactics
      - ур.1: converted: Evasion +0.2 @GarrisonFleet  [Deposit]
    - роль Губернатор (G): «Добыча ресурсов» / Resource Extraction
      - ур.1: kept: DepositValue +2 @ColonizedStarSystem/Planet/ResourceDeposit,ResourceTypeStrategic
    - роль Сенатор (S): «Ресурсная политика» / Resource Policy
      - ур.1: converted: DepositValue +1 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - роль Диктатор (D): «Ресурсная политика» / Resource Policy
      - ур.1: converted: DepositValue +1.6 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchTerrans`** - Титан промышленности / Titan of Industry  (tags: ColonizedStarSystem)
    - описание: Герой - живая легенда. Он посещает промышленные объекты по всей империи и берется за любую работу: от мастера литейного цеха до директора по производству. После его отъезда квалификация и мотивация всех работников возрастает.
    - ур.1 [мастерство Labour x1]: система: SystemProduction Percent (0.02 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem | сенат: SystemProduction Percent (0.005 Multiplication $(AssignedHeroLevel)) @Empire/ColonizedStarSystem
    - роль Адмирал (A): «Бронепластины» / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.2 @GarrisonFleet  [Production]
    - роль Губернатор (G): «Индустриальный рост» / Industrial Drive
      - ур.1: kept: SystemProduction Percent (0.02 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; converted: SystemProduction +10 @ColonizedStarSystem  [Production]
    - роль Сенатор (S): «Государственная индустрия» / State Industry
      - ур.1: kept: SystemProduction Percent (0.005 Multiplication $(AssignedHeroLevel)) @Empire/ColonizedStarSystem; converted: PlanetRawIndustry +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - роль Диктатор (D): «Государственная индустрия» / State Industry
      - ур.1: kept: SystemProduction Percent (0.005 Multiplication $(AssignedHeroLevel)) @Empire/ColonizedStarSystem  (x1.6 for the dictator); converted: PlanetRawIndustry +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]

### HeroSkillTreeHoratio - Навыки Горацио / Horatio Skills
Эта ветка содержит специфические навыки Горацио.

Героев с этим деревом: 5 (HeroHoratio01, HeroHoratio02, HeroHoratio03, HeroHoratio04, HeroQuestHoratio04)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchHoratio`** - Клонированные продукты / Clone Wares  (tags: ColonizedStarSystem)
    - описание: Герой Горацио поддерживает монокультурное сельское хозяйство и за счет единообразия повышает уровень Пищи и Одобрения.
    - ур.1 [мастерство Labour x1]: система: Happiness +5 @ColonizedStarSystem; SystemGrowth +5% @ColonizedStarSystem
    - ур.2 [мастерство Labour x2]: система: Happiness +10 @ColonizedStarSystem; SystemGrowth +10% @ColonizedStarSystem
    - роль Адмирал (A): «Усиленные щиты / Резервные экипажи» / Reinforced Shields / Reserve Crews
      - ур.1: converted: FleetManpowerRefillRateModifier +0.133 @GarrisonFleet  [Growth]; converted: FleetMaximumShield +0.125 @GarrisonFleet  [Happiness]
      - ур.2: converted: FleetManpowerRefillRateModifier +0.267 @GarrisonFleet  [Growth]; converted: FleetMaximumShield +0.25 @GarrisonFleet  [Happiness]
    - роль Губернатор (G): «Общественный порядок / Агрокультура» / Public Order / Agriculture
      - ур.1: kept: Happiness +5 @ColonizedStarSystem; kept: SystemGrowth +5% @ColonizedStarSystem
      - ур.2: kept: Happiness +10 @ColonizedStarSystem; kept: SystemGrowth +10% @ColonizedStarSystem
    - роль Сенатор (S): «Всеобщее благополучие / Аграрная реформа» / General Welfare / Agrarian Reform
      - ур.1: converted: PlanetRawFood +0.667 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; converted: Happiness +5 @Empire/ColonizedStarSystem  [Happiness]
    - роль Диктатор (D): «Всеобщее благополучие / Аграрная реформа» / General Welfare / Agrarian Reform
      - ур.1: converted: PlanetRawFood +0.533 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; converted: Happiness +4 @Empire/ColonizedStarSystem  [Happiness]
      - ур.2: converted: PlanetRawFood +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; converted: Happiness +8 @Empire/ColonizedStarSystem  [Happiness]
      - ур.3: converted: PlanetRawFood +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]; converted: Happiness +12 @Empire/ColonizedStarSystem  [Happiness]
  - **`HeroSkill02BranchHoratio`** - Любознательность Горацио / Horatic Curiosity
    - описание: Как наследник Горацио, Герой стремится развивать исследовательскую деятельность, направляя все излишки ресурсов флота на поддержание сканирующих систем и изучение космических струн.
    - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +1 @GarrisonFleet; VisionRange +20% @GarrisonFleet
    - роль Адмирал (A): «Манёвренность» / Maneuverability
      - ур.1: kept: ShipMaximumMovement +1 @GarrisonFleet; kept: VisionRange +20% @GarrisonFleet
    - роль Губернатор (G): «Торговые пути» / Trade Routes
      - ур.1: converted: TradingRoutesBonus_Production +60% @ColonizedStarSystem  [Mobility]
    - роль Сенатор (S): «Имперская навигация» / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +1.5 @Empire/GarrisonFleet  [Mobility]
    - роль Диктатор (D): «Имперская навигация» / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +2.4 @Empire/GarrisonFleet  [Mobility]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchHoratio`** - Счастливый случай / Lucky Windfall  (tags: ColonizedStarSystem)
    - описание: Организовав общесистемную лотерею с огромными выплатами, Герой получает колоссальные возможности для воздействия на общественное мнение.
    - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +10% @ColonizedStarSystem
    - ур.2 [мастерство Wit x1]: система: SystemEmpirePoint +20% @ColonizedStarSystem
    - роль Адмирал (A): «Осадное мастерство» / Siegecraft
      - ур.1: converted: SiegePower +20% @GarrisonFleet  [Influence]
      - ур.2: converted: SiegePower +40% @GarrisonFleet  [Influence]
    - роль Губернатор (G): «Престиж колоний» / Colonial Prestige
      - ур.1: kept: SystemEmpirePoint +10% @ColonizedStarSystem
      - ур.2: kept: SystemEmpirePoint +20% @ColonizedStarSystem
    - роль Сенатор (S): «Имперская идеология» / Imperial Ideology
      - ур.1: converted: PlanetRawPrestige +2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - роль Диктатор (D): «Имперская идеология» / Imperial Ideology
      - ур.1: converted: PlanetRawPrestige +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
      - ур.2: converted: PlanetRawPrestige +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
      - ур.3: converted: PlanetRawPrestige +4.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill04BranchHoratio`** - Плодородные сады / Fecund Gardens  (tags: ColonizedStarSystem)
    - описание: Пропагандируя социальную философию уверенности в собственных силах и доказывая это на личном примере, Герой воодушевляет население своей системы выращивать собственные овощи, что способствует увеличению объемов производства продуктов питания.
    - ур.1 [мастерство Labour x1]: система: PlanetRawFood +4 @ColonizedStarSystem/ColonizedPlanet; PlanetPopulationFood Addition (4 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; BonusHeroPlanetFood Addition (4 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetFood Addition (4 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - ур.2 [мастерство Labour x1]: система: PlanetRawFood +8 @ColonizedStarSystem/ColonizedPlanet; PlanetPopulationFood Addition (8 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; BonusHeroPlanetFood Addition (8 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetFood Addition (8 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - роль Адмирал (A): «Резервные экипажи» / Reserve Crews
      - ур.1: converted: FleetManpowerRefillRateModifier +1.1 @GarrisonFleet  [Growth]
      - ур.2: converted: FleetManpowerRefillRateModifier +2.1 @GarrisonFleet  [Growth]
    - роль Губернатор (G): «Агрокультура» / Agriculture
      - ур.1: kept: PlanetRawFood +4 @ColonizedStarSystem/ColonizedPlanet; kept: PlanetPopulationFood Addition (4 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; kept: BonusHeroPlanetFood Addition (4 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); kept: BonusHeroPlanetFood Addition (4 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
      - ур.2: kept: PlanetRawFood +8 @ColonizedStarSystem/ColonizedPlanet; kept: PlanetPopulationFood Addition (8 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; kept: BonusHeroPlanetFood Addition (8 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); kept: BonusHeroPlanetFood Addition (8 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - роль Сенатор (S): «Аграрная реформа» / Agrarian Reform
      - ур.1: converted: PlanetRawFood +5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - роль Диктатор (D): «Аграрная реформа» / Agrarian Reform
      - ур.1: converted: PlanetRawFood +4.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.2: converted: PlanetRawFood +9 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.3: converted: PlanetRawFood +13 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
  - **`HeroSkill05BranchHoratio`** - Рынки тщеславия / Vanity Markets  (tags: ColonizedStarSystem)
    - описание: Герой Горацио создает общество, зацикленное на атрибутах богатства: предметах роскоши, ювелирных изделиях и деликатесах. Это искусственно повышает стоимость всех Ресурсов Роскоши.
    - ур.1 [мастерство Wit x1]: система: DepositValue +2 @ColonizedStarSystem/Planet/ResourceDeposit,ResourceTypeLuxury
    - ур.2 [мастерство Wit x1]: система: DepositValue +4 @ColonizedStarSystem/Planet/ResourceDeposit,ResourceTypeLuxury
    - роль Адмирал (A): «Уклонение» / Evasive Tactics
      - ур.1: converted: Evasion +0.2 @GarrisonFleet  [Deposit]
      - ур.2: converted: Evasion +0.4 @GarrisonFleet  [Deposit]
    - роль Губернатор (G): «Добыча ресурсов» / Resource Extraction
      - ур.1: kept: DepositValue +2 @ColonizedStarSystem/Planet/ResourceDeposit,ResourceTypeLuxury
      - ур.2: kept: DepositValue +4 @ColonizedStarSystem/Planet/ResourceDeposit,ResourceTypeLuxury
    - роль Сенатор (S): «Ресурсная политика» / Resource Policy
      - ур.1: converted: DepositValue +2 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - роль Диктатор (D): «Ресурсная политика» / Resource Policy
      - ур.1: converted: DepositValue +1.6 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
      - ур.2: converted: DepositValue +3.2 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
      - ур.3: converted: DepositValue +4.8 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
  - **`HeroSkill06BranchHoratio`** - Защитный строй / Protective Formations
    - описание: Во время битвы Герой выстраивает флот таким образом, что корабли становятся менее восприимчивы к артиллерийскому огню.
    - ур.1 [мастерство Command x1]: флот: ShipShieldAbsorptionPercent +0.4 @GarrisonFleet
    - ур.2 [мастерство Command x1]: флот: ShipShieldAbsorptionPercent +0.8 @GarrisonFleet
    - роль Адмирал (A): «Крепкие корпуса» / Hardened Hulls
      - ур.1: kept: ShipShieldAbsorptionPercent +0.4 @GarrisonFleet
      - ур.2: kept: ShipShieldAbsorptionPercent +0.8 @GarrisonFleet
    - роль Губернатор (G): «Укреплённые гарнизоны» / Fortified Garrisons
      - ур.1: converted: MaximumSystemManpower +80%  [Defense]
      - ур.2: converted: MaximumSystemManpower +160%  [Defense]
    - роль Сенатор (S): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.4 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.32 @Empire/GarrisonFleet  [Defense]
      - ур.2: converted: FleetMaximumHealth +0.64 @Empire/GarrisonFleet  [Defense]
      - ур.3: converted: FleetMaximumHealth +0.96 @Empire/GarrisonFleet  [Defense]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchHoratio`** - Вне конкуренции / Restricted Supplier  (tags: ColonizedStarSystem)
    - описание: Быстро захватывая контроль над Ресурсами Роскоши в Системе, Герой Горацио искусственно поднимает их цену выше настоящей рыночной стоимости.
    - ур.1 [мастерство Wit x1]: система: DepositValue +33% @ColonizedStarSystem/Planet/ResourceDeposit,ResourceTypeLuxury; PlanetRawIndustry +2 @ColonizedStarSystem,HappinessStatusStarSystem4/Planet | сенат: DepositValue +33% @Empire/ColonizedStarSystem/Planet/ResourceDeposit,ResourceTypeLuxury
    - роль Адмирал (A): «Уклонение / Бронепластины» / Evasive Tactics / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.267 @GarrisonFleet  [Production]; converted: Evasion +0.3 @GarrisonFleet  [Deposit]
    - роль Губернатор (G): «Добыча ресурсов / Индустриальный рост» / Resource Extraction / Industrial Drive
      - ур.1: kept: DepositValue +33% @ColonizedStarSystem/Planet/ResourceDeposit,ResourceTypeLuxury; kept: PlanetRawIndustry +2 @ColonizedStarSystem,HappinessStatusStarSystem4/Planet; converted: DepositValue +2 @ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - роль Сенатор (S): «Ресурсная политика / Государственная индустрия» / Resource Policy / State Industry
      - ур.1: kept: DepositValue +33% @Empire/ColonizedStarSystem/Planet/ResourceDeposit,ResourceTypeLuxury; converted: PlanetRawIndustry +1.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: DepositValue +0.5 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]
    - роль Диктатор (D): «Ресурсная политика / Государственная индустрия» / Resource Policy / State Industry
      - ур.1: kept: DepositValue +33% @Empire/ColonizedStarSystem/Planet/ResourceDeposit,ResourceTypeLuxury  (x1.6 for the dictator); converted: PlanetRawIndustry +2.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: DepositValue +0.8 @Empire/ColonizedStarSystem/Planet/ResourceDeposit  [Deposit]

### HeroSkillTreeVenetians - Навыки Люмерис / Lumeris skills
Эта ветка содержит специфические навыки Люмерис.

Героев с этим деревом: 5 (HeroVenetians01, HeroVenetians02, HeroVenetians03, HeroVenetians04, HeroVenetians05)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchVenetians`** - Бонус для Семейства Меос / Meos Family Favor  (tags: ColonizedStarSystem)
    - описание: Герой заключает сделку с кланом Меос, который в обмен на оказываемую Герою политическую поддержку получает доступ к производству Пыли в данной Системе.
    - ур.1 [мастерство Wit x1]: система: SystemMoney +5 @ColonizedStarSystem; SystemMoney Addition (5 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; BonusHeroSystemDust +5 @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (5 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemMoney +10 @ColonizedStarSystem; SystemMoney Addition (10 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; BonusHeroSystemDust +10 @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (10 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Бережливое снабжение» / Frugal Supply
      - ур.1: converted: GarrisonMoneyUpkeep -30% @GarrisonFleet  [Money]
      - ур.2: converted: GarrisonMoneyUpkeep -60% @GarrisonFleet  [Money]
    - роль Губернатор (G): «Торговля и налоги» / Commerce and Taxes
      - ур.1: kept: SystemMoney +5 @ColonizedStarSystem; kept: SystemMoney Addition (5 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemDust +5 @ColonizedStarSystem (hidden); kept: BonusHeroSystemDust Addition (5 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemMoney +10 @ColonizedStarSystem; kept: SystemMoney Addition (10 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemDust +10 @ColonizedStarSystem (hidden); kept: BonusHeroSystemDust Addition (10 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Казначейство» / Treasury
      - ур.1: converted: PlanetRawDust +2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - роль Диктатор (D): «Казначейство» / Treasury
      - ур.1: converted: PlanetRawDust +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
      - ур.2: converted: PlanetRawDust +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
      - ур.3: converted: PlanetRawDust +4.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
  - **`HeroSkill02BranchVenetians`** - Промышленная деятельность клана Омокар / Omokar Industrial Operations
    - описание: Элитная команда из числа отколовшихся от клана Омокар инженеров присоединяется к флоту Героя, чтобы усовершенствовать характеристики корпуса и брони его судов.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.05 @GarrisonFleet; FleetMaximumShield +0.05 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: FleetMaximumHealth +0.1 @GarrisonFleet; FleetMaximumShield +0.1 @GarrisonFleet
    - роль Адмирал (A): «Крепкие корпуса» / Hardened Hulls
      - ур.1: kept: FleetMaximumHealth +0.05 @GarrisonFleet; kept: FleetMaximumShield +0.05 @GarrisonFleet
      - ур.2: kept: FleetMaximumHealth +0.1 @GarrisonFleet; kept: FleetMaximumShield +0.1 @GarrisonFleet
    - роль Губернатор (G): «Укреплённые гарнизоны» / Fortified Garrisons
      - ур.1: converted: MaximumSystemManpower +20%  [Defense]
      - ур.2: converted: MaximumSystemManpower +40%  [Defense]
    - роль Сенатор (S): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.1 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.08 @Empire/GarrisonFleet  [Defense]
      - ур.2: converted: FleetMaximumHealth +0.16 @Empire/GarrisonFleet  [Defense]
      - ур.3: converted: FleetMaximumHealth +0.24 @Empire/GarrisonFleet  [Defense]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchVenetians`** - Счастливый случай / Lucky Windfall  (tags: ColonizedStarSystem)
    - описание: Организовав общесистемную лотерею с огромными выплатами, Герой получает колоссальные возможности для воздействия на общественное мнение.
    - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +10% @ColonizedStarSystem | сенат: TradingRouteIncomeModifier +10% @Empire/ColonizedStarSystem
    - роль Адмирал (A): «Бережливое снабжение / Осадное мастерство» / Frugal Supply / Siegecraft
      - ур.1: converted: GarrisonMoneyUpkeep -40% @GarrisonFleet  [Money]; converted: SiegePower +20% @GarrisonFleet  [Influence]
    - роль Губернатор (G): «Торговля и налоги / Престиж колоний» / Commerce and Taxes / Colonial Prestige
      - ур.1: kept: SystemEmpirePoint +10% @ColonizedStarSystem; converted: SystemMoney +13 @ColonizedStarSystem  [Money]
    - роль Сенатор (S): «Казначейство / Имперская идеология» / Treasury / Imperial Ideology
      - ур.1: kept: TradingRouteIncomeModifier +10% @Empire/ColonizedStarSystem; converted: PlanetRawPrestige +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - роль Диктатор (D): «Казначейство / Имперская идеология» / Treasury / Imperial Ideology
      - ур.1: kept: TradingRouteIncomeModifier +10% @Empire/ColonizedStarSystem  (x1.6 for the dictator); converted: PlanetRawPrestige +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill04BranchVenetians`** - Личные связи / Personal Networking  (tags: ColonizedStarSystem)
    - описание: Герой настойчиво трудится для обеспечения полной занятости населения в своей Системе, используя финансы клана Меос и предприятия клана Омокар, чтобы снизить процент безработицы почти до нуля. Обеспеченное работой население создает в Системе промышленный бум.
    - ур.1 [мастерство Labour x1]: система: PlanetRawIndustry +3 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetIndustry Addition (3 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - ур.2 [мастерство Labour x2]: система: PlanetRawIndustry +6 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetIndustry Addition (6 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - роль Адмирал (A): «Бронепластины» / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.4 @GarrisonFleet  [Production]
      - ур.2: converted: ShipHullPlatingAbsorptionPercent +0.8 @GarrisonFleet  [Production]
    - роль Губернатор (G): «Индустриальный рост» / Industrial Drive
      - ур.1: kept: PlanetRawIndustry +3 @ColonizedStarSystem/ColonizedPlanet; kept: BonusHeroPlanetIndustry Addition (3 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
      - ур.2: kept: PlanetRawIndustry +6 @ColonizedStarSystem/ColonizedPlanet; kept: BonusHeroPlanetIndustry Addition (6 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - роль Сенатор (S): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - роль Диктатор (D): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
      - ур.2: converted: PlanetRawIndustry +6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
      - ур.3: converted: PlanetRawIndustry +10 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
  - **`HeroSkill05BranchVenetians`** - Реклама туристического направления / Destination Marketing  (tags: ColonizedStarSystem)
    - описание: Герой работает над позиционированием выбранной Системы в качестве центра власти и перспективного для люмерийских Семейств туристического направления. Подкупленные ученые из клана Арракьо распространяют научные доказательства в пользу этого, и рост инвестиций и иммиграции стимулирует влияние расы Люмерис в данной Системе.
    - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +20 @ColonizedStarSystem; SystemMoney Addition (40 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; BonusHeroSystemPrestige +20 @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (40 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemEmpirePoint +40 @ColonizedStarSystem; SystemMoney Addition (80 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; BonusHeroSystemPrestige +40 @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (80 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Осадное мастерство» / Siegecraft
      - ур.1: converted: SiegePower +40% @GarrisonFleet  [Influence]
      - ур.2: converted: SiegePower +80% @GarrisonFleet  [Influence]
    - роль Губернатор (G): «Престиж колоний» / Colonial Prestige
      - ур.1: kept: SystemEmpirePoint +20 @ColonizedStarSystem; kept: SystemMoney Addition (40 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemPrestige +20 @ColonizedStarSystem (hidden); kept: BonusHeroSystemDust Addition (40 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemEmpirePoint +40 @ColonizedStarSystem; kept: SystemMoney Addition (80 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemPrestige +40 @ColonizedStarSystem (hidden); kept: BonusHeroSystemDust Addition (80 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Имперская идеология» / Imperial Ideology
      - ур.1: converted: PlanetRawPrestige +4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - роль Диктатор (D): «Имперская идеология» / Imperial Ideology
      - ур.1: converted: PlanetRawPrestige +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
      - ур.2: converted: PlanetRawPrestige +6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
      - ур.3: converted: PlanetRawPrestige +10 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
  - **`HeroSkill06BranchVenetians`** - Черный рынок на флоте / Fleet Black Market
    - описание: Позволяя развиваться на подвластных ему кораблях хорошо контролируемому черному рынку, Герой достигает одновременно ускоренной реконструкции зондов и сокращения расходов на техобслуживание.
    - ур.1 [мастерство Curiosity x1]: флот: NetShipProbe +0.5 @GarrisonFleet/Ship,ShipRoleExploration; GarrisonMoneyUpkeep -20% @GarrisonFleet
    - ур.2 [мастерство Curiosity x2]: флот: NetShipProbe +0.5 @GarrisonFleet/Ship,ShipRoleExploration; GarrisonMoneyUpkeep -40% @GarrisonFleet
    - роль Адмирал (A): «Экономия флота» / Fleet Economy
      - ур.1: kept: NetShipProbe +0.5 @GarrisonFleet/Ship,ShipRoleExploration; kept: GarrisonMoneyUpkeep -20% @GarrisonFleet
      - ур.2: kept: NetShipProbe +0.5 @GarrisonFleet/Ship,ShipRoleExploration; kept: GarrisonMoneyUpkeep -40% @GarrisonFleet
    - роль Губернатор (G): «Бережливое управление» / Frugal Administration
      - ур.1: converted: TradingRoutesBonus_Production +10% @ColonizedStarSystem  [Mobility]; converted: SystemMoneyUpkeep -20% @ColonizedStarSystem  [Upkeep]
      - ур.2: converted: TradingRoutesBonus_Production +10% @ColonizedStarSystem  [Mobility]; converted: SystemMoneyUpkeep -40% @ColonizedStarSystem  [Upkeep]
    - роль Сенатор (S): «Режим экономии» / Austerity
      - ур.1: converted: ShipMaximumMovement +0.25 @Empire/GarrisonFleet  [Mobility]; converted: SystemMoneyUpkeep -20% @Empire/ColonizedStarSystem  [Upkeep]
    - роль Диктатор (D): «Режим экономии» / Austerity
      - ур.1: converted: ShipMaximumMovement +0.4 @Empire/GarrisonFleet  [Mobility]; converted: SystemMoneyUpkeep -16% @Empire/ColonizedStarSystem  [Upkeep]
      - ур.2: converted: ShipMaximumMovement +0.4 @Empire/GarrisonFleet  [Mobility]; converted: SystemMoneyUpkeep -32% @Empire/ColonizedStarSystem  [Upkeep]
      - ур.3: converted: ShipMaximumMovement +0.6 @Empire/GarrisonFleet  [Mobility]; converted: SystemMoneyUpkeep -48% @Empire/ColonizedStarSystem  [Upkeep]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchVenetians`** - Люмерийская монополия / Lumerian Monopoly  (tags: ColonizedStarSystem)
    - описание: Герой поднимается на высший уровень в иерархии своего клана, добиваясь полного контроля над импортом и экспортом товаров в данной системе. Выгодное местечко.
    - ур.1 [мастерство Wit x1]: система: TradingRoutesBonus_Growth +40% @ColonizedStarSystem; TradingRoutesBonus_Production +40% @ColonizedStarSystem
    - роль Адмирал (A): «Резервные экипажи / Бронепластины» / Reserve Crews / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.533 @GarrisonFleet  [Production]; converted: FleetManpowerRefillRateModifier +1.1 @GarrisonFleet  [Growth]
    - роль Губернатор (G): «Агрокультура / Индустриальный рост» / Agriculture / Industrial Drive
      - ур.1: kept: TradingRoutesBonus_Growth +40% @ColonizedStarSystem; kept: TradingRoutesBonus_Production +40% @ColonizedStarSystem
    - роль Сенатор (S): «Аграрная реформа / Государственная индустрия» / Agrarian Reform / State Industry
      - ур.1: converted: PlanetRawIndustry +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: PlanetRawFood +2.7 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - роль Диктатор (D): «Аграрная реформа / Государственная индустрия» / Agrarian Reform / State Industry
      - ур.1: converted: PlanetRawIndustry +4.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]; converted: PlanetRawFood +4.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]

### HeroSkillTreeVampirilis - Навыки Водяни / Vodyani Skills
Эта ветка содержит специфические навыки Водяни.

Героев с этим деревом: 4 (HeroVampirilis01, HeroVampirilis02, HeroVampirilis03, HeroVampirilis04)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchVampirilis`** - Пища аскета / Ascetic Sustenance  (tags: ColonizedStarSystem)
    - описание: Герой начинает питаться простой и скромной пищей, подавая пример и вдохновляя свое окружение делать то же самое. Сельское хозяйство сосредотачивается на производстве легко выращиваемой продукций, что приводит к повышению урожайности.
    - ур.1 [мастерство Labour x1]: система: SystemGrowth +10 @ColonizedStarSystem; SystemGrowth Addition (1 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemFood +10 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (1 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemGrowth +20 @ColonizedStarSystem; SystemGrowth Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemFood +20 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Резервные экипажи» / Reserve Crews
      - ур.1: converted: FleetManpowerRefillRateModifier +0.4 @GarrisonFleet  [Growth]
      - ур.2: converted: FleetManpowerRefillRateModifier +0.8 @GarrisonFleet  [Growth]
    - роль Губернатор (G): «Агрокультура» / Agriculture
      - ур.1: kept: SystemGrowth +10 @ColonizedStarSystem; kept: SystemGrowth Addition (1 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; kept: BonusHeroSystemFood +10 @ColonizedStarSystem (hidden); kept: BonusHeroSystemFood Addition (1 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemGrowth +20 @ColonizedStarSystem; kept: SystemGrowth Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; kept: BonusHeroSystemFood +20 @ColonizedStarSystem (hidden); kept: BonusHeroSystemFood Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Аграрная реформа» / Agrarian Reform
      - ур.1: converted: PlanetRawFood +2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - роль Диктатор (D): «Аграрная реформа» / Agrarian Reform
      - ур.1: converted: PlanetRawFood +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.2: converted: PlanetRawFood +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.3: converted: PlanetRawFood +4.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
  - **`HeroSkill02BranchVampirilis`** - Проекция брони / Mantle Projection
    - описание: Благодаря мастерству субстанции, Герой может проецировать часть своего личного Пылевого поля в щитовые системы корабля, повышая его защиту.
    - ур.1 [мастерство Command x1]: флот: MaximumShield +20% @Ship
    - ур.2 [мастерство Command x2]: флот: MaximumShield +40% @Ship
    - роль Адмирал (A): «Крепкие корпуса» / Hardened Hulls
      - ур.1: kept: MaximumShield +20% @Ship
      - ур.2: kept: MaximumShield +40% @Ship
    - роль Губернатор (G): «Укреплённые гарнизоны» / Fortified Garrisons
      - ур.1: converted: MaximumSystemManpower +40%  [Defense]
      - ур.2: converted: MaximumSystemManpower +80%  [Defense]
    - роль Сенатор (S): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.2 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.16 @Empire/GarrisonFleet  [Defense]
      - ур.2: converted: FleetMaximumHealth +0.32 @Empire/GarrisonFleet  [Defense]
      - ур.3: converted: FleetMaximumHealth +0.48 @Empire/GarrisonFleet  [Defense]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchVampirilis`** - Скопление пыли / Inundation of Dust  (tags: ColonizedStarSystem)
    - описание: Тщательно изучив писания Вечных и проведя всевозможные эксперименты, Герой многое узнает о сложностях добычи Пыли и получает возможность значительно повысить объемы получаемой магической субстанции в Системе, в которую он назначен.
    - ур.1 [мастерство Wit x1]: система: SystemMoney +10% @ColonizedStarSystem
    - ур.2 [мастерство Wit x2]: система: SystemMoney +20% @ColonizedStarSystem
    - роль Адмирал (A): «Бережливое снабжение» / Frugal Supply
      - ур.1: converted: GarrisonMoneyUpkeep -20% @GarrisonFleet  [Money]
      - ур.2: converted: GarrisonMoneyUpkeep -40% @GarrisonFleet  [Money]
    - роль Губернатор (G): «Торговля и налоги» / Commerce and Taxes
      - ур.1: kept: SystemMoney +10% @ColonizedStarSystem
      - ур.2: kept: SystemMoney +20% @ColonizedStarSystem
    - роль Сенатор (S): «Казначейство» / Treasury
      - ур.1: converted: PlanetRawDust +1.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - роль Диктатор (D): «Казначейство» / Treasury
      - ур.1: converted: PlanetRawDust +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
      - ур.2: converted: PlanetRawDust +2.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
      - ур.3: converted: PlanetRawDust +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
  - **`HeroSkill04BranchVampirilis`** - Жгучий свет / The Burning Light
    - описание: Тщательно проанализировав критические энергетические потребности флота, Герой может перенастроить системы энергетического вооружения таким образом, чтобы они производили больше мощности, тем самым увеличивая урон от них.
    - ур.1 [мастерство Command x1]: флот: ShipDamageEnergy +0.2 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipDamageEnergy +0.4 @GarrisonFleet
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: ShipDamageEnergy +0.2 @GarrisonFleet
      - ур.2: kept: ShipDamageEnergy +0.4 @GarrisonFleet
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem  [Offense]
      - ур.2: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.4 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.2 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.16 @Empire/GarrisonFleet  [Offense]
      - ур.2: converted: ShipDamage +0.32 @Empire/GarrisonFleet  [Offense]
      - ур.3: converted: ShipDamage +0.48 @Empire/GarrisonFleet  [Offense]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchVampirilis`** - Небесная механика / Celestial Mechanics
    - описание: Используя свое "ощущение" влияния Пыли на звездные струны, Герой лучше может направлять свой флот по путям наименьшего сопротивления, увеличивая дальность его полета.
    - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +6 @GarrisonFleet
    - роль Адмирал (A): «Манёвренность» / Maneuverability
      - ур.1: kept: ShipMaximumMovement +6 @GarrisonFleet
    - роль Губернатор (G): «Торговые пути» / Trade Routes
      - ур.1: converted: TradingRoutesBonus_Production +120% @ColonizedStarSystem  [Mobility]
    - роль Сенатор (S): «Имперская навигация» / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +3 @Empire/GarrisonFleet  [Mobility]
    - роль Диктатор (D): «Имперская навигация» / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +4.8 @Empire/GarrisonFleet  [Mobility]
  - **`HeroSkill06BranchVampirilis`** - Непогрешимый авторитет / Infallible Authority  (tags: ColonizedStarSystem)
    - описание: Являясь проводником воли Виртуалов, имея сильный характер и благодаря непоколебимой вере, Герой Водяни способен объединить всю империю крайне эмоциональными речами.
    - ур.1 [мастерство Labour x1]: система: PlanetRawFIDSI +2 @ColonizedStarSystem/ColonizedPlanet; BonusHeroSystemFood Addition (2 Multiplication $(Population)) @ColonizedStarSystem (hidden); BonusHeroSystemIndustry Addition (2 Multiplication $(Population)) @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (2 Multiplication $(Population)) @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (2 Multiplication $(Population)) @ColonizedStarSystem (hidden); BonusHeroSystemPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem (hidden) | сенат: PlanetRawFIDSI +0.5 @Empire/ColonizedStarSystem/ColonizedPlanet
    - роль Адмирал (A): «Бронепластины» / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.4 @GarrisonFleet  [Production]
    - роль Губернатор (G): «Индустриальный рост» / Industrial Drive
      - ур.1: kept: PlanetRawFIDSI +2 @ColonizedStarSystem/ColonizedPlanet; kept: BonusHeroSystemFood Addition (2 Multiplication $(Population)) @ColonizedStarSystem (hidden); kept: BonusHeroSystemIndustry Addition (2 Multiplication $(Population)) @ColonizedStarSystem (hidden); kept: BonusHeroSystemDust Addition (2 Multiplication $(Population)) @ColonizedStarSystem (hidden); kept: BonusHeroSystemScience Addition (2 Multiplication $(Population)) @ColonizedStarSystem (hidden); kept: BonusHeroSystemPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem (hidden); converted: SystemProduction +7 @ColonizedStarSystem  [Production]
    - роль Сенатор (S): «Государственная индустрия» / State Industry
      - ур.1: kept: PlanetRawFIDSI +0.5 @Empire/ColonizedStarSystem/ColonizedPlanet; converted: PlanetRawIndustry +1.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - роль Диктатор (D): «Государственная индустрия» / State Industry
      - ур.1: kept: PlanetRawFIDSI +0.5 @Empire/ColonizedStarSystem/ColonizedPlanet  (x1.6 for the dictator); converted: PlanetRawIndustry +2.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchVampirilis`** - Праведность посвященных / Righteousness of the Ordained
    - описание: Во времена битвы, преисполненный праведной верой, герой Водяни наполняет свое ближайшее окружение такой же верой, значительно повышая мощь и силу своего личного корабля.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.25 @GarrisonFleet; Damage +0.8 @Ship
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: FleetMaximumHealth +0.25 @GarrisonFleet; kept: Damage +0.8 @Ship
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.8 @ColonizedStarSystem  [Offense]; converted: MaximumSystemManpower +50%  [Defense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.4 @Empire/GarrisonFleet  [Offense]; converted: FleetMaximumHealth +0.125 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.64 @Empire/GarrisonFleet  [Offense]; converted: FleetMaximumHealth +0.2 @Empire/GarrisonFleet  [Defense]

### HeroSkillTreeTimeLords - Навыки Властелинов Времени / Riftborn Skills
Эта ветка содержит навыки Властелинов Времени

Героев с этим деревом: 4 (HeroTimeLords01, HeroTimeLords02, HeroTimeLords03, HeroTimeLords04)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchTimeLords`** - Скопление пыли / Inundation of Dust  (tags: ColonizedStarSystem)
    - описание: Тщательно изучив писания Вечных и проведя всевозможные эксперименты, Герой многое узнает о сложностях добычи Пыли и получает возможность значительно повысить объемы получаемой магической субстанции в Системе, в которую он назначен.
    - ур.1 [мастерство Wit x1]: система: SystemMoney +5% @ColonizedStarSystem
    - ур.2 [мастерство Wit x2]: система: SystemMoney +10% @ColonizedStarSystem
    - роль Адмирал (A): «Бережливое снабжение» / Frugal Supply
      - ур.1: converted: GarrisonMoneyUpkeep -10% @GarrisonFleet  [Money]
      - ур.2: converted: GarrisonMoneyUpkeep -20% @GarrisonFleet  [Money]
    - роль Губернатор (G): «Торговля и налоги» / Commerce and Taxes
      - ур.1: kept: SystemMoney +5% @ColonizedStarSystem
      - ур.2: kept: SystemMoney +10% @ColonizedStarSystem
    - роль Сенатор (S): «Казначейство» / Treasury
      - ур.1: converted: PlanetRawDust +0.667 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
    - роль Диктатор (D): «Казначейство» / Treasury
      - ур.1: converted: PlanetRawDust +0.533 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
      - ур.2: converted: PlanetRawDust +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
      - ур.3: converted: PlanetRawDust +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Money]
  - **`HeroSkill02BranchTimeLords`** - Гений механики / Mechanical Genius  (tags: ColonizedStarSystem)
    - описание: Благодаря тому, что промышленные базы становятся все более автоматизированными, понимание Героем машин может обеспечить увеличение объемов производства по всей Системе.
    - ур.1 [мастерство Labour x1]: система: SystemProduction +15 @ColonizedStarSystem; BonusHeroSystemIndustry +15 @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemProduction +30 @ColonizedStarSystem; BonusHeroSystemIndustry +30 @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Бронепластины» / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.6 @GarrisonFleet  [Production]
      - ур.2: converted: ShipHullPlatingAbsorptionPercent +1.2 @GarrisonFleet  [Production]
    - роль Губернатор (G): «Индустриальный рост» / Industrial Drive
      - ур.1: kept: SystemProduction +15 @ColonizedStarSystem; kept: BonusHeroSystemIndustry +15 @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemProduction +30 @ColonizedStarSystem; kept: BonusHeroSystemIndustry +30 @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - роль Диктатор (D): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +4.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
      - ур.2: converted: PlanetRawIndustry +10 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
      - ур.3: converted: PlanetRawIndustry +14 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchTimeLords`** - Тайные знания / Arcane Knowledge  (tags: ColonizedStarSystem)
    - описание: Общаясь с эхом Пыли, найденными на планетах, Герой может погрузиться в историю и получить утраченные знания.
    - ур.1 [мастерство Wit x1]: система: SystemResearch +20 @ColonizedStarSystem; SystemResearch Addition (20 Multiplication $(AnomalyCount)) @ColonizedStarSystem; BonusHeroSystemScience +20 @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (20 Multiplication $(AnomalyCount)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Дальняя разведка» / Long-Range Scouting
      - ур.1: converted: VisionRange +2 @GarrisonFleet  [Science]
    - роль Губернатор (G): «Научные центры» / Research Centers
      - ур.1: kept: SystemResearch +20 @ColonizedStarSystem; kept: SystemResearch Addition (20 Multiplication $(AnomalyCount)) @ColonizedStarSystem; kept: BonusHeroSystemScience +20 @ColonizedStarSystem (hidden); kept: BonusHeroSystemScience Addition (20 Multiplication $(AnomalyCount)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - роль Диктатор (D): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
  - **`HeroSkill04BranchTimeLords`** - Интеграция оборонных ИИ-ресурсов / Defensive AI Integration
    - описание: Специализированная система Искусственного Интеллекта, разрабатываемая группой волонтеров под руководством Героя с целью оптимизации тактических компьютерных расчетов. Ее высокоскоростные характеристики позволяют повысить оборонительный потенциал космофлота, увеличивая коэффициент Выживаемости в бою.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumShield +0.1 @GarrisonFleet
    - роль Адмирал (A): «Крепкие корпуса» / Hardened Hulls
      - ур.1: kept: FleetMaximumShield +0.1 @GarrisonFleet
    - роль Губернатор (G): «Укреплённые гарнизоны» / Fortified Garrisons
      - ур.1: converted: MaximumSystemManpower +20%  [Defense]
    - роль Сенатор (S): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.05 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.08 @Empire/GarrisonFleet  [Defense]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchTimeLords`** - Академическое рецензирование / Peer Review  (tags: ColonizedStarSystem)
    - описание: Герой проявляет интерес к научным исследованиям в подвластной ему системе, внося свой вклад в обеспечение должного уровня функционирования научно-образовательной системы.
    - ур.1 [мастерство Wit x1]: система: SystemResearch +40 @ColonizedStarSystem; SystemResearch Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemScience +40 @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemResearch +80 @ColonizedStarSystem; SystemResearch Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemScience +80 @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Дальняя разведка» / Long-Range Scouting
      - ур.1: converted: VisionRange +4 @GarrisonFleet  [Science]
      - ур.2: converted: VisionRange +8 @GarrisonFleet  [Science]
    - роль Губернатор (G): «Научные центры» / Research Centers
      - ур.1: kept: SystemResearch +40 @ColonizedStarSystem; kept: SystemResearch Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; kept: BonusHeroSystemScience +40 @ColonizedStarSystem (hidden); kept: BonusHeroSystemScience Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemResearch +80 @ColonizedStarSystem; kept: SystemResearch Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; kept: BonusHeroSystemScience +80 @ColonizedStarSystem (hidden); kept: BonusHeroSystemScience Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - роль Диктатор (D): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
      - ур.2: converted: PlanetRawScience +6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
      - ур.3: converted: PlanetRawScience +10 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
  - **`HeroSkill06BranchTimeLords`** - На пределе / Red Zone Overkill
    - описание: Управляя вооружением, Герой увеличивает его максимальную мощность тем самым увеличивая пробивную способность. Для ремонта и замены использованных запчастей после боя потребуется дополнительное время, но экипажи относятся к этому спокойно, учитывая их повышенную эффективность.
    - ур.1 [мастерство Command x1]: флот: ShipShieldPenetrationFactor +0.4 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipShieldPenetrationFactor +0.8 @GarrisonFleet
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: ShipShieldPenetrationFactor +0.4 @GarrisonFleet
      - ур.2: kept: ShipShieldPenetrationFactor +0.8 @GarrisonFleet
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.4 @ColonizedStarSystem  [Offense]
      - ур.2: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.8 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.4 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.32 @Empire/GarrisonFleet  [Offense]
      - ур.2: converted: ShipDamage +0.64 @Empire/GarrisonFleet  [Offense]
      - ур.3: converted: ShipDamage +0.96 @Empire/GarrisonFleet  [Offense]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchTimeLords`** - Технические инкубаторы / Tech Hubs  (tags: ColonizedStarSystem)
    - описание: Научные инкубаторы, размещенные вблизи аномалий, дают толчок развитию творческого машиностроения и повышают объемы промышленного производства.
    - ур.1 [мастерство Labour x1]: система: SystemProduction Addition ($(AnomalyCount) Multiplication 50) @ColonizedStarSystem; BonusHeroSystemIndustry Addition ($(AnomalyCount) Multiplication 50) @ColonizedStarSystem (hidden) | сенат: SystemProduction Addition ($(AnomalyCount) Multiplication 10) @Empire/ColonizedStarSystem; BonusHeroSystemIndustry Addition ($(AnomalyCount) Multiplication 10) @Empire/ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Бронепластины» / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.2 @GarrisonFleet  [Production]
    - роль Губернатор (G): «Индустриальный рост» / Industrial Drive
      - ур.1: kept: SystemProduction Addition ($(AnomalyCount) Multiplication 50) @ColonizedStarSystem; kept: BonusHeroSystemIndustry Addition ($(AnomalyCount) Multiplication 50) @ColonizedStarSystem (hidden); converted: SystemProduction +10 @ColonizedStarSystem  [Production]
    - роль Сенатор (S): «Государственная индустрия» / State Industry
      - ур.1: kept: SystemProduction Addition ($(AnomalyCount) Multiplication 10) @Empire/ColonizedStarSystem; kept: BonusHeroSystemIndustry Addition ($(AnomalyCount) Multiplication 10) @Empire/ColonizedStarSystem (hidden); converted: PlanetRawIndustry +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - роль Диктатор (D): «Государственная индустрия» / State Industry
      - ур.1: kept: SystemProduction Addition ($(AnomalyCount) Multiplication 10) @Empire/ColonizedStarSystem  (x1.6 for the dictator); kept: BonusHeroSystemIndustry Addition ($(AnomalyCount) Multiplication 10) @Empire/ColonizedStarSystem (hidden)  (x1.6 for the dictator); converted: PlanetRawIndustry +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]

### HeroSkillTreeUnfallen - 

Героев с этим деревом: 4 (HeroUnfallen01, HeroUnfallen02, HeroUnfallen03, HeroUnfallen04)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchUnfallen`** 
    - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth Addition (0.005 Multiplication $(CommandPoints)) @GarrisonFleet
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: FleetMaximumHealth Addition (0.005 Multiplication $(CommandPoints)) @GarrisonFleet; converted: ShipDamage +0.2 @GarrisonFleet  [Offense]
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.1 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.16 @Empire/GarrisonFleet  [Offense]
  - **`HeroSkill02BranchUnfallen`**   (tags: ColonizedStarSystem)
    - ур.1 [мастерство Labour x1]: система: SystemGrowth +7% @ColonizedStarSystem
    - ур.2 [мастерство Labour x2]: система: SystemGrowth +15% @ColonizedStarSystem
    - роль Адмирал (A): «Резервные экипажи» / Reserve Crews
      - ур.1: converted: FleetManpowerRefillRateModifier +0.187 @GarrisonFleet  [Growth]
      - ур.2: converted: FleetManpowerRefillRateModifier +0.4 @GarrisonFleet  [Growth]
    - роль Губернатор (G): «Агрокультура» / Agriculture
      - ур.1: kept: SystemGrowth +7% @ColonizedStarSystem
      - ур.2: kept: SystemGrowth +15% @ColonizedStarSystem
    - роль Сенатор (S): «Аграрная реформа» / Agrarian Reform
      - ур.1: converted: PlanetRawFood +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - роль Диктатор (D): «Аграрная реформа» / Agrarian Reform
      - ур.1: converted: PlanetRawFood +0.747 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.2: converted: PlanetRawFood +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.3: converted: PlanetRawFood +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchUnfallen`** 
    - ур.1 [мастерство Command x1]: флот: ShipShieldAbsorptionPercent +0.2 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipShieldAbsorptionPercent +0.4 @GarrisonFleet
    - роль Адмирал (A): «Крепкие корпуса» / Hardened Hulls
      - ур.1: kept: ShipShieldAbsorptionPercent +0.2 @GarrisonFleet
      - ур.2: kept: ShipShieldAbsorptionPercent +0.4 @GarrisonFleet
    - роль Губернатор (G): «Укреплённые гарнизоны» / Fortified Garrisons
      - ур.1: converted: MaximumSystemManpower +40%  [Defense]
      - ур.2: converted: MaximumSystemManpower +80%  [Defense]
    - роль Сенатор (S): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.2 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.16 @Empire/GarrisonFleet  [Defense]
      - ур.2: converted: FleetMaximumHealth +0.32 @Empire/GarrisonFleet  [Defense]
      - ур.3: converted: FleetMaximumHealth +0.48 @Empire/GarrisonFleet  [Defense]
  - **`HeroSkill04BranchUnfallen`**   (tags: ColonizedStarSystem)
    - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +10 @ColonizedStarSystem; SystemEmpirePoint Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemPrestige +10 @ColonizedStarSystem (hidden); BonusHeroSystemPrestige Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemEmpirePoint +20 @ColonizedStarSystem; SystemEmpirePoint Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemPrestige +20 @ColonizedStarSystem (hidden); BonusHeroSystemPrestige Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Осадное мастерство» / Siegecraft
      - ур.1: converted: SiegePower +20% @GarrisonFleet  [Influence]
      - ур.2: converted: SiegePower +40% @GarrisonFleet  [Influence]
    - роль Губернатор (G): «Престиж колоний» / Colonial Prestige
      - ур.1: kept: SystemEmpirePoint +10 @ColonizedStarSystem; kept: SystemEmpirePoint Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; kept: BonusHeroSystemPrestige +10 @ColonizedStarSystem (hidden); kept: BonusHeroSystemPrestige Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemEmpirePoint +20 @ColonizedStarSystem; kept: SystemEmpirePoint Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; kept: BonusHeroSystemPrestige +20 @ColonizedStarSystem (hidden); kept: BonusHeroSystemPrestige Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Имперская идеология» / Imperial Ideology
      - ур.1: converted: PlanetRawPrestige +2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - роль Диктатор (D): «Имперская идеология» / Imperial Ideology
      - ур.1: converted: PlanetRawPrestige +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
      - ур.2: converted: PlanetRawPrestige +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
      - ур.3: converted: PlanetRawPrestige +4.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchUnfallen`**   (tags: ColonizedStarSystem)
    - ур.1 [мастерство Wit x1]: система: PlanetRawPrestige +2 @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet; PlanetRawPrestige +2 @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden); PlanetRawPrestige +2 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet (hidden); BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden)
    - ур.2 [мастерство Wit x2]: система: PlanetRawPrestige +4 @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet; PlanetRawPrestige +4 @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden); PlanetRawPrestige +4 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetPrestige Addition (4 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetPrestige Addition (4 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet (hidden); BonusHeroPlanetPrestige Addition (4 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden)
    - роль Адмирал (A): «Осадное мастерство» / Siegecraft
      - ур.1: converted: SiegePower +80% @GarrisonFleet  [Influence]
      - ур.2: converted: SiegePower +160% @GarrisonFleet  [Influence]
    - роль Губернатор (G): «Престиж колоний» / Colonial Prestige
      - ур.1: kept: PlanetRawPrestige +2 @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet; kept: PlanetRawPrestige +2 @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden); kept: PlanetRawPrestige +2 @ColonizedStarSystem/ColonizedPlanet; kept: BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); kept: BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet (hidden); kept: BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden)
      - ур.2: kept: PlanetRawPrestige +4 @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet; kept: PlanetRawPrestige +4 @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden); kept: PlanetRawPrestige +4 @ColonizedStarSystem/ColonizedPlanet; kept: BonusHeroPlanetPrestige Addition (4 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); kept: BonusHeroPlanetPrestige Addition (4 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet (hidden); kept: BonusHeroPlanetPrestige Addition (4 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden)
    - роль Сенатор (S): «Имперская идеология» / Imperial Ideology
      - ур.1: converted: PlanetRawPrestige +8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - роль Диктатор (D): «Имперская идеология» / Imperial Ideology
      - ур.1: converted: PlanetRawPrestige +6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
      - ур.2: converted: PlanetRawPrestige +13 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
      - ур.3: converted: PlanetRawPrestige +19 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
  - **`HeroSkill06BranchUnfallen`**   (tags: ColonizedStarSystem)
    - ур.1 [мастерство Labour x1]: система: PlanetRawFood +4 @ColonizedStarSystem/ColonizedPlanet; PlanetPopulationFood Addition (4 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; BonusHeroPlanetFood Addition (4 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetFood Addition (4 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - ур.2 [мастерство Labour x2]: система: PlanetRawFood +8 @ColonizedStarSystem/ColonizedPlanet; PlanetPopulationFood Addition (8 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; BonusHeroPlanetFood Addition (8 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetFood Addition (8 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - роль Адмирал (A): «Резервные экипажи» / Reserve Crews
      - ур.1: converted: FleetManpowerRefillRateModifier +1.1 @GarrisonFleet  [Growth]
      - ур.2: converted: FleetManpowerRefillRateModifier +2.1 @GarrisonFleet  [Growth]
    - роль Губернатор (G): «Агрокультура» / Agriculture
      - ур.1: kept: PlanetRawFood +4 @ColonizedStarSystem/ColonizedPlanet; kept: PlanetPopulationFood Addition (4 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; kept: BonusHeroPlanetFood Addition (4 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); kept: BonusHeroPlanetFood Addition (4 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
      - ур.2: kept: PlanetRawFood +8 @ColonizedStarSystem/ColonizedPlanet; kept: PlanetPopulationFood Addition (8 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; kept: BonusHeroPlanetFood Addition (8 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); kept: BonusHeroPlanetFood Addition (8 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - роль Сенатор (S): «Аграрная реформа» / Agrarian Reform
      - ур.1: converted: PlanetRawFood +5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - роль Диктатор (D): «Аграрная реформа» / Agrarian Reform
      - ур.1: converted: PlanetRawFood +4.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.2: converted: PlanetRawFood +9 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.3: converted: PlanetRawFood +13 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchUnfallen`** 
    - ур.1 [мастерство Labour x1]: флот: HealthRegenerationRatio +0.4 @GarrisonFleet | сенат: HealthRegenerationRatio +0.05 @Empire/GarrisonFleet
    - роль Адмирал (A): «Полевой ремонт» / Field Repairs
      - ур.1: kept: HealthRegenerationRatio +0.4 @GarrisonFleet; converted: HealthRegenerationRatio +0.1 @GarrisonFleet  [Sustain]
    - роль Губернатор (G): «Пополнение войск» / Troop Replenishment
      - ур.1: converted: MaximumSystemManpower +250  [Sustain]
    - роль Сенатор (S): «Службы ремонта» / Repair Corps
      - ур.1: kept: HealthRegenerationRatio +0.05 @Empire/GarrisonFleet; converted: HealthRegenerationRatio +0.1 @Empire/GarrisonFleet  [Sustain]
    - роль Диктатор (D): «Службы ремонта» / Repair Corps
      - ур.1: kept: HealthRegenerationRatio +0.05 @Empire/GarrisonFleet  (x1.6 for the dictator); converted: HealthRegenerationRatio +0.16 @Empire/GarrisonFleet  [Sustain]

### HeroSkillTreeMinorFaction01 - Навыки Добытчиков / Scavenger skills
Эта ветка содержит специфические навыки малых цивилизаций Добытчиков.

Героев с этим деревом: 6 (HeroHaroshems01, HeroHisshos01, HeroEyders01, HeroPulsos01, HeroQuestVirus01, HeroPirate01)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchMinorFaction01`** - Грязная работа / Common Touch  (tags: ColonizedStarSystem)
    - описание: Герой не стыдится замарать руки даже на самой грязной работе, поэтому им восхищается большая часть населения Системы, которой он управляет. В результате поддержка населения значительно увеличивается.
    - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +5 @ColonizedStarSystem; SystemEmpirePoint Addition (1 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemPrestige +5 @ColonizedStarSystem (hidden); BonusHeroSystemPrestige Addition (1 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemEmpirePoint +10 @ColonizedStarSystem; SystemEmpirePoint Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemPrestige +10 @ColonizedStarSystem (hidden); BonusHeroSystemPrestige Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Осадное мастерство» / Siegecraft
      - ур.1: converted: SiegePower +10% @GarrisonFleet  [Influence]
      - ур.2: converted: SiegePower +20% @GarrisonFleet  [Influence]
    - роль Губернатор (G): «Престиж колоний» / Colonial Prestige
      - ур.1: kept: SystemEmpirePoint +5 @ColonizedStarSystem; kept: SystemEmpirePoint Addition (1 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; kept: BonusHeroSystemPrestige +5 @ColonizedStarSystem (hidden); kept: BonusHeroSystemPrestige Addition (1 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemEmpirePoint +10 @ColonizedStarSystem; kept: SystemEmpirePoint Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; kept: BonusHeroSystemPrestige +10 @ColonizedStarSystem (hidden); kept: BonusHeroSystemPrestige Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Имперская идеология» / Imperial Ideology
      - ур.1: converted: PlanetRawPrestige +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - роль Диктатор (D): «Имперская идеология» / Imperial Ideology
      - ур.1: converted: PlanetRawPrestige +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
      - ур.2: converted: PlanetRawPrestige +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
      - ур.3: converted: PlanetRawPrestige +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill02BranchMinorFaction01`** - Черный рынок на флоте / Fleet Black Market
    - описание: Позволяя развиваться на подвластных ему кораблях хорошо контролируемому черному рынку, Герой достигает одновременно ускоренной реконструкции зондов и сокращения расходов на техобслуживание.
    - ур.1 [мастерство Curiosity x1]: флот: NetShipProbe +0.5 @GarrisonFleet/Ship,ShipRoleExploration
    - ур.2 [мастерство Curiosity x2]: флот: NetShipProbe +0.5 @GarrisonFleet/Ship,ShipRoleExploration; GarrisonMoneyUpkeep -20% @GarrisonFleet
    - роль Адмирал (A): «Экономия флота» / Fleet Economy
      - ур.1: kept: NetShipProbe +0.5 @GarrisonFleet/Ship,ShipRoleExploration
      - ур.2: kept: NetShipProbe +0.5 @GarrisonFleet/Ship,ShipRoleExploration; kept: GarrisonMoneyUpkeep -20% @GarrisonFleet
    - роль Губернатор (G): «Бережливое управление» / Frugal Administration
      - ур.1: converted: TradingRoutesBonus_Production +10% @ColonizedStarSystem  [Mobility]
      - ур.2: converted: TradingRoutesBonus_Production +10% @ColonizedStarSystem  [Mobility]; converted: SystemMoneyUpkeep -20% @ColonizedStarSystem  [Upkeep]
    - роль Сенатор (S): «Режим экономии» / Austerity
      - ур.1: converted: ShipMaximumMovement +0.25 @Empire/GarrisonFleet  [Mobility]; converted: SystemMoneyUpkeep -10% @Empire/ColonizedStarSystem  [Upkeep]
    - роль Диктатор (D): «Режим экономии» / Austerity
      - ур.1: converted: ShipMaximumMovement +0.4 @Empire/GarrisonFleet  [Mobility]
      - ур.2: converted: ShipMaximumMovement +0.4 @Empire/GarrisonFleet  [Mobility]; converted: SystemMoneyUpkeep -16% @Empire/ColonizedStarSystem  [Upkeep]
      - ур.3: converted: ShipMaximumMovement +0.6 @Empire/GarrisonFleet  [Mobility]; converted: SystemMoneyUpkeep -24% @Empire/ColonizedStarSystem  [Upkeep]
  - **`HeroSkill03BranchMinorFaction01`** - Запах крови / Blood Scent
    - описание: Опыт, полученный в сотне битв, наделяет Героя мгновенной реакцией, основанной на интуитивной способности находить слабые места вражеского флота, что усиливает поражающую мощь его кораблей.
    - ур.1 [мастерство Command x1]: флот: ShipHullPlatingPenetrationFactor +0.2 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipHullPlatingPenetrationFactor +0.4 @GarrisonFleet
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: ShipHullPlatingPenetrationFactor +0.2 @GarrisonFleet
      - ур.2: kept: ShipHullPlatingPenetrationFactor +0.4 @GarrisonFleet
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem  [Offense]
      - ур.2: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.4 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.2 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.16 @Empire/GarrisonFleet  [Offense]
      - ур.2: converted: ShipDamage +0.32 @Empire/GarrisonFleet  [Offense]
      - ур.3: converted: ShipDamage +0.48 @Empire/GarrisonFleet  [Offense]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill04BranchMinorFaction01`** - Фокусы с пахучими удобрениями / Steaming Fertilizer Tricks  (tags: ColonizedStarSystem)
    - описание: Большое количество набегов на заболоченные планеты с чрезвычайно простыми методами ведения сельского хозяйства дало Герою богатое, даже ароматное, понимание того, как получить максимальную пользу от навоза копытных животных. Распространение этого ноу-хау по всей Системе Героя может существенно повысить урожайность даже на самых современных сельскохозяйственных планетах.
    - ур.1 [мастерство Labour x1]: система: SystemGrowth +30% @ColonizedStarSystem
    - ур.2 [мастерство Labour x2]: система: SystemGrowth +60% @ColonizedStarSystem
    - роль Адмирал (A): «Резервные экипажи» / Reserve Crews
      - ур.1: converted: FleetManpowerRefillRateModifier +0.8 @GarrisonFleet  [Growth]
      - ур.2: converted: FleetManpowerRefillRateModifier +1.6 @GarrisonFleet  [Growth]
    - роль Губернатор (G): «Агрокультура» / Agriculture
      - ур.1: kept: SystemGrowth +30% @ColonizedStarSystem
      - ур.2: kept: SystemGrowth +60% @ColonizedStarSystem
    - роль Сенатор (S): «Аграрная реформа» / Agrarian Reform
      - ур.1: converted: PlanetRawFood +4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - роль Диктатор (D): «Аграрная реформа» / Agrarian Reform
      - ур.1: converted: PlanetRawFood +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.2: converted: PlanetRawFood +6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.3: converted: PlanetRawFood +10 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
  - **`HeroSkill05BranchMinorFaction01`** - Мусорная экономика / Scrapyard Economics
    - описание: Герой хорошо разбирается в переработке отходов для снижения затрат на строительные проекты. С помощью этого навыка Герой создает сеть гигантских скоординированных складов в масштабе империи, что помогает сэкономить при производстве кораблей по всей империи.
    - ур.1 [мастерство Labour x1]: система: ShipProductionCostReduction -0.1 | сенат: ShipProductionCostReduction -0.05 @Empire/ColonizedStarSystem
    - роль Адмирал (A): «Бронепластины» / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.004 @GarrisonFleet  [Production]
    - роль Губернатор (G): «Индустриальный рост» / Industrial Drive
      - ур.1: kept: ShipProductionCostReduction -0.1; converted: SystemProduction +0.1 @ColonizedStarSystem  [Production]
    - роль Сенатор (S): «Государственная индустрия» / State Industry
      - ур.1: kept: ShipProductionCostReduction -0.05 @Empire/ColonizedStarSystem; converted: PlanetRawIndustry +0.01 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - роль Диктатор (D): «Государственная индустрия» / State Industry
      - ур.1: kept: ShipProductionCostReduction -0.05 @Empire/ColonizedStarSystem  (x1.6 for the dictator); converted: PlanetRawIndustry +0.016 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
  - **`HeroSkill06BranchMinorFaction01`** - Лоскутная защита / Patchwork Defenses
    - описание: Опыт сотен боев - от стычек в астероидных шахтах до масштабных сражений флотов - позволил Герою понять, как получить преимущество в обороне. Перераспределение пластин корпуса и усиление кораблей в тех местах, куда скорее всего будет нанесен удар, и ослабление брони в местах, куда попадание маловероятно, позволяет Герою улучшить оборону.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet; FleetHullPlatingAbsorption +40 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: FleetMaximumHealth +0.2 @GarrisonFleet; FleetHullPlatingAbsorption +80 @GarrisonFleet
    - роль Адмирал (A): «Крепкие корпуса» / Hardened Hulls
      - ур.1: kept: FleetMaximumHealth +0.1 @GarrisonFleet; kept: FleetHullPlatingAbsorption +40 @GarrisonFleet
      - ур.2: kept: FleetMaximumHealth +0.2 @GarrisonFleet; kept: FleetHullPlatingAbsorption +80 @GarrisonFleet
    - роль Губернатор (G): «Укреплённые гарнизоны» / Fortified Garrisons
      - ур.1: converted: MaximumSystemManpower +8000%  [Defense]
      - ур.2: converted: MaximumSystemManpower +16000%  [Defense]
    - роль Сенатор (S): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +40 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +32 @Empire/GarrisonFleet  [Defense]
      - ур.2: converted: FleetMaximumHealth +64 @Empire/GarrisonFleet  [Defense]
      - ур.3: converted: FleetMaximumHealth +96 @Empire/GarrisonFleet  [Defense]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchMinorFaction01`** - Мастер-отступник / Renegade Master  (tags: ColonizedStarSystem)
    - описание: Изучение загадочных культурных традиций малых цивилизаций, таких как ритуалы общения с духами Эпистиса, делает Героя легендой в глазах меньшинств, населяющих Систему.
    - ур.1 [мастерство Wit x1]: система: Happiness Addition (5 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem; SystemGrowth Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem; SystemProduction Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem; SystemMoney Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem; SystemResearch Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem; BonusHeroSystemFood Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem (hidden); BonusHeroSystemIndustry Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Дальняя разведка» / Long-Range Scouting
      - ур.1: converted: VisionRange +2 @GarrisonFleet  [Science]
    - роль Губернатор (G): «Научные центры» / Research Centers
      - ур.1: kept: Happiness Addition (5 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem; kept: SystemGrowth Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem; kept: SystemProduction Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem; kept: SystemMoney Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem; kept: SystemResearch Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem; kept: BonusHeroSystemFood Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem (hidden); kept: BonusHeroSystemIndustry Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem (hidden); kept: BonusHeroSystemDust Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem (hidden); kept: BonusHeroSystemScience Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem (hidden); converted: SystemResearch +40 @ColonizedStarSystem  [Science]
    - роль Сенатор (S): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - роль Диктатор (D): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]

### HeroSkillTreeMinorFaction02 - Эусоциальные навыки / Eusocial skills
Эта ветвь содержит навыки социальных малых цивилизаций.

Героев с этим деревом: 5 (HeroAmoeba01, HeroDeuyivans01, HeroTikanans01, HeroRemnant01, HeroAmoeba02)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchMinorFaction02`** - Разум роя / Hive Mind  (tags: ColonizedStarSystem)
    - описание: Являясь частью суперорганизма, Герой тонко чувствует потребности и желания населения и легко получает общественную поддержку.
    - ур.1 [мастерство Wit x1]: система: Happiness Addition (1 Multiplication $(Population)) @ColonizedStarSystem
    - ур.2 [мастерство Wit x2]: система: Happiness Addition (2 Multiplication $(Population)) @ColonizedStarSystem
    - роль Адмирал (A): «Дальняя разведка» / Long-Range Scouting
      - ур.1: converted: VisionRange +1 @GarrisonFleet  [Science]
      - ур.2: converted: VisionRange +2 @GarrisonFleet  [Science]
    - роль Губернатор (G): «Научные центры» / Research Centers
      - ур.1: kept: Happiness Addition (1 Multiplication $(Population)) @ColonizedStarSystem; converted: SystemResearch +20 @ColonizedStarSystem  [Science]
      - ур.2: kept: Happiness Addition (2 Multiplication $(Population)) @ColonizedStarSystem; converted: SystemResearch +40 @ColonizedStarSystem  [Science]
    - роль Сенатор (S): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - роль Диктатор (D): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +0.533 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
      - ур.2: converted: PlanetRawScience +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
      - ур.3: converted: PlanetRawScience +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
  - **`HeroSkill02BranchMinorFaction02`** - Укрепленный панцирь / Hardened Carapace
    - описание: Полагаясь на рассредоточенную иерархию на флоте, Герой может позволить экипажам быть более автономными, что повышает общую обороноспособность судов.
    - ур.1 [мастерство Command x1]: флот: FleetHullPlatingAbsorption +30 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: FleetHullPlatingAbsorption +60 @GarrisonFleet
    - роль Адмирал (A): «Крепкие корпуса» / Hardened Hulls
      - ур.1: kept: FleetHullPlatingAbsorption +30 @GarrisonFleet
      - ур.2: kept: FleetHullPlatingAbsorption +60 @GarrisonFleet
    - роль Губернатор (G): «Укреплённые гарнизоны» / Fortified Garrisons
      - ур.1: converted: MaximumSystemManpower +6000%  [Defense]
      - ур.2: converted: MaximumSystemManpower +12000%  [Defense]
    - роль Сенатор (S): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +30 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +24 @Empire/GarrisonFleet  [Defense]
      - ур.2: converted: FleetMaximumHealth +48 @Empire/GarrisonFleet  [Defense]
      - ур.3: converted: FleetMaximumHealth +72 @Empire/GarrisonFleet  [Defense]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchMinorFaction02`** - Принудительный труд / Forced Labor  (tags: ColonizedStarSystem)
    - описание: Герой отдает приказ о захвате и содержании в неволе представителей других рас, которых силой принуждают работать на заводах и фермах. Их отпустят и вознаградят за тяжкий труд… но не прежде, чем из планет, на которых их эксплуатируют, будут дочиста выкачаны все ресурсы...
    - ур.1 [мастерство Labour x1]: система: SystemProduction +10 @ColonizedStarSystem; SystemGrowth Addition (20 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; BonusHeroSystemIndustry +10 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (20 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemProduction +20 @ColonizedStarSystem; SystemGrowth Addition (40 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; BonusHeroSystemIndustry +20 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (40 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Бронепластины» / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.4 @GarrisonFleet  [Production]
      - ур.2: converted: ShipHullPlatingAbsorptionPercent +0.8 @GarrisonFleet  [Production]
    - роль Губернатор (G): «Индустриальный рост» / Industrial Drive
      - ур.1: kept: SystemProduction +10 @ColonizedStarSystem; kept: SystemGrowth Addition (20 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemIndustry +10 @ColonizedStarSystem (hidden); kept: BonusHeroSystemFood Addition (20 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemProduction +20 @ColonizedStarSystem; kept: SystemGrowth Addition (40 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemIndustry +20 @ColonizedStarSystem (hidden); kept: BonusHeroSystemFood Addition (40 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - роль Диктатор (D): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
      - ур.2: converted: PlanetRawIndustry +6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
      - ур.3: converted: PlanetRawIndustry +10 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
  - **`HeroSkill04BranchMinorFaction02`** - Нулевая толерантность / Zero Tolerance  (tags: ColonizedStarSystem)
    - описание: Для эусоциального вида здоровье всего суперорганизма имеет первостепенное значение. Слабые особи отвергаются. Таким образом, все сухопутные войска Героя находятся в идеальной форме.
    - ур.1 [мастерство Labour x1]: система: GroundBattleDefenderTroopsMaxHealthMultiplier Addition (0.01 Multiplication $(Population)) @ColonizedStarSystem
    - ур.2 [мастерство Labour x2]: система: GroundBattleDefenderTroopsMaxHealthMultiplier Addition (0.02 Multiplication $(Population)) @ColonizedStarSystem
    - роль Адмирал (A): «Бронепластины» / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +0.1 @GarrisonFleet  [Production]
      - ур.2: converted: ShipHullPlatingAbsorptionPercent +0.2 @GarrisonFleet  [Production]
    - роль Губернатор (G): «Индустриальный рост» / Industrial Drive
      - ур.1: kept: GroundBattleDefenderTroopsMaxHealthMultiplier Addition (0.01 Multiplication $(Population)) @ColonizedStarSystem; converted: SystemProduction +5 @ColonizedStarSystem  [Production]
      - ур.2: kept: GroundBattleDefenderTroopsMaxHealthMultiplier Addition (0.02 Multiplication $(Population)) @ColonizedStarSystem; converted: SystemProduction +10 @ColonizedStarSystem  [Production]
    - роль Сенатор (S): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - роль Диктатор (D): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +0.533 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
      - ур.2: converted: PlanetRawIndustry +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
      - ур.3: converted: PlanetRawIndustry +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchMinorFaction02`** - Воодушевляющий оратор / Rousing Orator  (tags: ColonizedStarSystem)
    - описание: Герой может формировать и использовать общественное мнение, с легкостью воодушевляя людей во время массовых митингов с помощью провокационных лозунгов и эмоциональных аргументов.
    - ур.1 [мастерство Wit x1]: система: PlanetRawPrestige +2 @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet; PlanetRawPrestige +2 @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden); PlanetRawPrestige +2 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet (hidden); BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden) | сенат: ElectionActionCost -0.5 @Empire/Senate
    - роль Адмирал (A): «Осадное мастерство / Пробивание щитов» / Siegecraft / Shield Breaking
      - ур.1: converted: SiegePower +80% @GarrisonFleet  [Influence]; converted: ShipShieldPenetrationFactor +0.8 @GarrisonFleet  [Diplomacy]
    - роль Губернатор (G): «Престиж колоний / Торговые связи» / Colonial Prestige / Trade Ties
      - ур.1: kept: PlanetRawPrestige +2 @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet; kept: PlanetRawPrestige +2 @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden); kept: PlanetRawPrestige +2 @ColonizedStarSystem/ColonizedPlanet; kept: BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); kept: BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet (hidden); kept: BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden); converted: TradingRoutesBonus_Growth +80% @ColonizedStarSystem  [Diplomacy]
    - роль Сенатор (S): «Имперская идеология / Дипломатический корпус» / Imperial Ideology / Diplomatic Corps
      - ур.1: kept: ElectionActionCost -0.5 @Empire/Senate; converted: PlanetRawPrestige +4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
    - роль Диктатор (D): «Имперская идеология / Дипломатический корпус» / Imperial Ideology / Diplomatic Corps
      - ур.1: kept: ElectionActionCost -0.5 @Empire/Senate  (x1.6 for the dictator); converted: PlanetRawPrestige +6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Influence]
  - **`HeroSkill06BranchMinorFaction02`** - Образцовый хозяин / Ultra Houseproud  (tags: ColonizedStarSystem)
    - описание: Герой вдохновляет население гордиться общественными местами и сооружениями. Это приводит к снижению стоимости улучшений.
    - ур.1 [мастерство Labour x1]: система: SystemMoneyUpkeep -40% @ColonizedStarSystem
    - роль Адмирал (A): «Экономия флота» / Fleet Economy
      - ур.1: converted: GarrisonMoneyUpkeep -40% @GarrisonFleet  [Upkeep]
    - роль Губернатор (G): «Бережливое управление» / Frugal Administration
      - ур.1: kept: SystemMoneyUpkeep -40% @ColonizedStarSystem
    - роль Сенатор (S): «Режим экономии» / Austerity
      - ур.1: converted: SystemMoneyUpkeep -20% @Empire/ColonizedStarSystem  [Upkeep]
    - роль Диктатор (D): «Режим экономии» / Austerity
      - ур.1: converted: SystemMoneyUpkeep -32% @Empire/ColonizedStarSystem  [Upkeep]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchMinorFaction02`** - Легендарный авторитет / Legendary Authority
    - описание: Герой управляет находящимися под его командованием флотами настолько идеально, что любое несовершенство непростительно. Здоровье флота значительно увеличивается.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth Addition (0.02 Multiplication $(CommandPoints)) @GarrisonFleet
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: FleetMaximumHealth Addition (0.02 Multiplication $(CommandPoints)) @GarrisonFleet; converted: ShipDamage +0.2 @GarrisonFleet  [Offense]
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.1 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.16 @Empire/GarrisonFleet  [Offense]

### HeroSkillTreeMinorFaction03 - Технологические навыки / Technologist skills
Эта ветка содержит специфические навыки малых Технологических цивилизаций.

Героев с этим деревом: 4 (HeroBots01, HeroMavros01, HeroPilgrims01, HeroZvali01)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchMinorFaction03`** - Запах крови / Blood Scent
    - описание: Опыт, полученный в сотне битв, наделяет Героя мгновенной реакцией, основанной на интуитивной способности находить слабые места вражеского флота, что усиливает поражающую мощь его кораблей.
    - ур.1 [мастерство Command x1]: флот: ShipHullPlatingPenetrationFactor +0.1 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipHullPlatingPenetrationFactor +0.2 @GarrisonFleet
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: ShipHullPlatingPenetrationFactor +0.1 @GarrisonFleet
      - ур.2: kept: ShipHullPlatingPenetrationFactor +0.2 @GarrisonFleet
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @ColonizedStarSystem  [Offense]
      - ур.2: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.1 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.08 @Empire/GarrisonFleet  [Offense]
      - ур.2: converted: ShipDamage +0.16 @Empire/GarrisonFleet  [Offense]
      - ур.3: converted: ShipDamage +0.24 @Empire/GarrisonFleet  [Offense]
  - **`HeroSkill02BranchMinorFaction03`** - Мощные сканеры / Overclocked Scanners
    - описание: Обладая знаниями о том, как улучшить разрешение снимков дальнего космоса, Герой может увеличить диапазон обзора назначенного ему флота.
    - ур.1 [мастерство Curiosity x1]: флот: VisionRange +3 @GarrisonFleet
    - ур.2 [мастерство Curiosity x2]: флот: VisionRange +6 @GarrisonFleet
    - роль Адмирал (A): «Манёвренность» / Maneuverability
      - ур.1: kept: VisionRange +3 @GarrisonFleet
      - ур.2: kept: VisionRange +6 @GarrisonFleet
    - роль Губернатор (G): «Торговые пути» / Trade Routes
      - ур.1: converted: TradingRoutesBonus_Production +60% @ColonizedStarSystem  [Mobility]
      - ур.2: converted: TradingRoutesBonus_Production +120% @ColonizedStarSystem  [Mobility]
    - роль Сенатор (S): «Имперская навигация» / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +3 @Empire/GarrisonFleet  [Mobility]
    - роль Диктатор (D): «Имперская навигация» / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +2.4 @Empire/GarrisonFleet  [Mobility]
      - ур.2: converted: ShipMaximumMovement +4.8 @Empire/GarrisonFleet  [Mobility]
      - ур.3: converted: ShipMaximumMovement +7 @Empire/GarrisonFleet  [Mobility]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchMinorFaction03`** - Проекция брони / Mantle Projection
    - описание: Благодаря мастерству субстанции, Герой может проецировать часть своего личного Пылевого поля в щитовые системы корабля, повышая его защиту.
    - ур.1 [мастерство Command x1]: флот: MaximumShield +40% @Ship
    - ур.2 [мастерство Command x2]: флот: MaximumShield +80% @Ship
    - роль Адмирал (A): «Крепкие корпуса» / Hardened Hulls
      - ур.1: kept: MaximumShield +40% @Ship
      - ур.2: kept: MaximumShield +80% @Ship
    - роль Губернатор (G): «Укреплённые гарнизоны» / Fortified Garrisons
      - ур.1: converted: MaximumSystemManpower +80%  [Defense]
      - ур.2: converted: MaximumSystemManpower +160%  [Defense]
    - роль Сенатор (S): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.4 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.32 @Empire/GarrisonFleet  [Defense]
      - ур.2: converted: FleetMaximumHealth +0.64 @Empire/GarrisonFleet  [Defense]
      - ур.3: converted: FleetMaximumHealth +0.96 @Empire/GarrisonFleet  [Defense]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill04BranchMinorFaction03`** - Интеграция оборонных ИИ-ресурсов / Defensive AI Integration
    - описание: Специализированная система Искусственного Интеллекта, разрабатываемая группой волонтеров под руководством Героя с целью оптимизации тактических компьютерных расчетов. Ее высокоскоростные характеристики позволяют повысить оборонительный потенциал космофлота, увеличивая коэффициент Выживаемости в бою.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumShield +0.25 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: FleetMaximumShield +0.5 @GarrisonFleet
    - роль Адмирал (A): «Крепкие корпуса» / Hardened Hulls
      - ур.1: kept: FleetMaximumShield +0.25 @GarrisonFleet
      - ур.2: kept: FleetMaximumShield +0.5 @GarrisonFleet
    - роль Губернатор (G): «Укреплённые гарнизоны» / Fortified Garrisons
      - ур.1: converted: MaximumSystemManpower +50%  [Defense]
      - ур.2: converted: MaximumSystemManpower +100%  [Defense]
    - роль Сенатор (S): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.25 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.2 @Empire/GarrisonFleet  [Defense]
      - ур.2: converted: FleetMaximumHealth +0.4 @Empire/GarrisonFleet  [Defense]
      - ур.3: converted: FleetMaximumHealth +0.6 @Empire/GarrisonFleet  [Defense]
  - **`HeroSkill05BranchMinorFaction03`** - Гениократ / Geniocrat  (tags: ColonizedStarSystem)
    - описание: Герой принимает меры против стагнации в политической и академической сферах, привнося определенную долю своей организаторской логики в функционирование данной системы, отчего показатель Науки буквально взлетает.
    - ур.1 [мастерство Wit x1]: система: SystemResearch +30% @ColonizedStarSystem
    - роль Адмирал (A): «Дальняя разведка» / Long-Range Scouting
      - ур.1: converted: VisionRange +3 @GarrisonFleet  [Science]
    - роль Губернатор (G): «Научные центры» / Research Centers
      - ур.1: kept: SystemResearch +30% @ColonizedStarSystem
    - роль Сенатор (S): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +1.5 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - роль Диктатор (D): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
  - **`HeroSkill06BranchMinorFaction03`** - Руководство экспедициями / Expedition Management  (tags: ColonizedStarSystem)
    - описание: Герой лично руководит экспедициями в пределах Системы, применяя свой искусственно повышенный интеллект для обнаружения хорошо сохранившихся развалин на холодных планетах.
    - ур.1 [мастерство Wit x1]: система: SystemResearch +40 @ColonizedStarSystem; SystemResearch Addition (40 Multiplication $(OwnedColdPlanets)) @ColonizedStarSystem; BonusHeroSystemScience +40 @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (40 Multiplication $(OwnedColdPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemResearch +80 @ColonizedStarSystem; SystemResearch Addition (80 Multiplication $(OwnedColdPlanets)) @ColonizedStarSystem; BonusHeroSystemScience +80 @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (80 Multiplication $(OwnedColdPlanets)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Дальняя разведка» / Long-Range Scouting
      - ур.1: converted: VisionRange +4 @GarrisonFleet  [Science]
      - ур.2: converted: VisionRange +8 @GarrisonFleet  [Science]
    - роль Губернатор (G): «Научные центры» / Research Centers
      - ур.1: kept: SystemResearch +40 @ColonizedStarSystem; kept: SystemResearch Addition (40 Multiplication $(OwnedColdPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemScience +40 @ColonizedStarSystem (hidden); kept: BonusHeroSystemScience Addition (40 Multiplication $(OwnedColdPlanets)) @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemResearch +80 @ColonizedStarSystem; kept: SystemResearch Addition (80 Multiplication $(OwnedColdPlanets)) @ColonizedStarSystem; kept: BonusHeroSystemScience +80 @ColonizedStarSystem (hidden); kept: BonusHeroSystemScience Addition (80 Multiplication $(OwnedColdPlanets)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - роль Диктатор (D): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
      - ур.2: converted: PlanetRawScience +6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
      - ур.3: converted: PlanetRawScience +10 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchMinorFaction03`** - На пределе / Red Zone Overkill
    - описание: Управляя вооружением, Герой увеличивает его максимальную мощность тем самым увеличивая пробивную способность. Для ремонта и замены использованных запчастей после боя потребуется дополнительное время, но экипажи относятся к этому спокойно, учитывая их повышенную эффективность.
    - ур.1 [мастерство Command x1]: флот: ShipShieldPenetrationFactor +0.8 @GarrisonFleet | сенат: ShipShieldPenetrationFactor +0.1 @Empire/GarrisonFleet
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: ShipShieldPenetrationFactor +0.8 @GarrisonFleet; converted: ShipDamage +0.2 @GarrisonFleet  [Offense]
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +1 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: kept: ShipShieldPenetrationFactor +0.1 @Empire/GarrisonFleet; converted: ShipDamage +0.4 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: kept: ShipShieldPenetrationFactor +0.1 @Empire/GarrisonFleet  (x1.6 for the dictator); converted: ShipDamage +0.64 @Empire/GarrisonFleet  [Offense]

### HeroSkillTreeMinorFaction04 - Первобытные Навыки / Primitive skills
Эта ветка содержит специфические навыки малых первобытных цивилизаций.

Героев с этим деревом: 6 (HeroBenthys01, HeroGnashasts01, HeroNiris01, HeroBhagabas01, HeroGreenman01, HeroGreenman02)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchMinorFaction04`** - Плодородные сады / Fecund Gardens  (tags: ColonizedStarSystem)
    - описание: Пропагандируя социальную философию уверенности в собственных силах и доказывая это на личном примере, Герой воодушевляет население своей системы выращивать собственные овощи, что способствует увеличению объемов производства продуктов питания.
    - ур.1 [мастерство Labour x1]: система: PlanetRawFood +1 @ColonizedStarSystem/ColonizedPlanet; PlanetPopulationFood Addition (1 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; BonusHeroPlanetFood Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetFood Addition (1 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - ур.2 [мастерство Labour x2]: система: PlanetRawFood +2 @ColonizedStarSystem/ColonizedPlanet; PlanetPopulationFood Addition (2 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; BonusHeroPlanetFood Addition (2 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetFood Addition (2 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - роль Адмирал (A): «Резервные экипажи» / Reserve Crews
      - ур.1: converted: FleetManpowerRefillRateModifier +0.267 @GarrisonFleet  [Growth]
      - ур.2: converted: FleetManpowerRefillRateModifier +0.533 @GarrisonFleet  [Growth]
    - роль Губернатор (G): «Агрокультура» / Agriculture
      - ур.1: kept: PlanetRawFood +1 @ColonizedStarSystem/ColonizedPlanet; kept: PlanetPopulationFood Addition (1 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; kept: BonusHeroPlanetFood Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); kept: BonusHeroPlanetFood Addition (1 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
      - ур.2: kept: PlanetRawFood +2 @ColonizedStarSystem/ColonizedPlanet; kept: PlanetPopulationFood Addition (2 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; kept: BonusHeroPlanetFood Addition (2 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); kept: BonusHeroPlanetFood Addition (2 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - роль Сенатор (S): «Аграрная реформа» / Agrarian Reform
      - ур.1: converted: PlanetRawFood +1.3 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - роль Диктатор (D): «Аграрная реформа» / Agrarian Reform
      - ур.1: converted: PlanetRawFood +1.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.2: converted: PlanetRawFood +2.1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.3: converted: PlanetRawFood +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
  - **`HeroSkill02BranchMinorFaction04`** - Военный стержень / Military Spine
    - описание: Обладая способностью определять наиболее способных командиров на флоте, Герой может делегировать тактические решения, увеличивая возможности наступления.
    - ур.1 [мастерство Command x1]: флот: ShipDamage Addition (0.005 Multiplication $(CommandPoints)) @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipDamage Addition (0.01 Multiplication $(CommandPoints)) @GarrisonFleet
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: ShipDamage Addition (0.005 Multiplication $(CommandPoints)) @GarrisonFleet; converted: ShipDamage +0.1 @GarrisonFleet  [Offense]
      - ур.2: kept: ShipDamage Addition (0.01 Multiplication $(CommandPoints)) @GarrisonFleet; converted: ShipDamage +0.2 @GarrisonFleet  [Offense]
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @ColonizedStarSystem  [Offense]
      - ур.2: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.1 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.053 @Empire/GarrisonFleet  [Offense]
      - ур.2: converted: ShipDamage +0.107 @Empire/GarrisonFleet  [Offense]
      - ур.3: converted: ShipDamage +0.24 @Empire/GarrisonFleet  [Offense]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchMinorFaction04`** - Пища аскета / Ascetic Sustenance  (tags: ColonizedStarSystem)
    - описание: Герой начинает питаться простой и скромной пищей, подавая пример и вдохновляя свое окружение делать то же самое. Сельское хозяйство сосредотачивается на производстве легко выращиваемой продукций, что приводит к повышению урожайности.
    - ур.1 [мастерство Labour x1]: система: SystemGrowth +20 @ColonizedStarSystem; SystemGrowth Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemFood +20 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemGrowth +40 @ColonizedStarSystem; SystemGrowth Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemFood +40 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Резервные экипажи» / Reserve Crews
      - ур.1: converted: FleetManpowerRefillRateModifier +0.8 @GarrisonFleet  [Growth]
      - ур.2: converted: FleetManpowerRefillRateModifier +1.6 @GarrisonFleet  [Growth]
    - роль Губернатор (G): «Агрокультура» / Agriculture
      - ур.1: kept: SystemGrowth +20 @ColonizedStarSystem; kept: SystemGrowth Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; kept: BonusHeroSystemFood +20 @ColonizedStarSystem (hidden); kept: BonusHeroSystemFood Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemGrowth +40 @ColonizedStarSystem; kept: SystemGrowth Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; kept: BonusHeroSystemFood +40 @ColonizedStarSystem (hidden); kept: BonusHeroSystemFood Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Аграрная реформа» / Agrarian Reform
      - ур.1: converted: PlanetRawFood +4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
    - роль Диктатор (D): «Аграрная реформа» / Agrarian Reform
      - ур.1: converted: PlanetRawFood +3.2 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.2: converted: PlanetRawFood +6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
      - ур.3: converted: PlanetRawFood +10 @Empire/ColonizedStarSystem/ColonizedPlanet  [Growth]
  - **`HeroSkill04BranchMinorFaction04`** - Гений механики / Mechanical Genius  (tags: ColonizedStarSystem)
    - описание: Благодаря тому, что промышленные базы становятся все более автоматизированными, понимание Героем машин может обеспечить увеличение объемов производства по всей Системе.
    - ур.1 [мастерство Labour x1]: система: SystemProduction +30 @ColonizedStarSystem; BonusHeroSystemIndustry +30 @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Бронепластины» / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +1.2 @GarrisonFleet  [Production]
    - роль Губернатор (G): «Индустриальный рост» / Industrial Drive
      - ур.1: kept: SystemProduction +30 @ColonizedStarSystem; kept: BonusHeroSystemIndustry +30 @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - роль Диктатор (D): «Государственная индустрия» / State Industry
      - ур.1: converted: PlanetRawIndustry +10 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchMinorFaction04`** - Кровь и земля / Blood and Land  (tags: ColonizedStarSystem)
    - описание: Как истинный патриот, Герой пропагандирует представление о том, что личность каждого человека глубоко связана с его родиной, тем самым значительно повышая количество призывников.
    - ур.1 [мастерство Labour x1]: система: MaximumSystemManpower +40%
    - ур.2 [мастерство Labour x2]: система: MaximumSystemManpower +80%
    - роль Адмирал (A): «Набор экипажей» / Crew Recruitment
      - ур.1: converted: MaximumShipManpower +25% @GarrisonFleet/Ship  [Manpower]
      - ур.2: converted: MaximumShipManpower +50% @GarrisonFleet/Ship  [Manpower]
    - роль Губернатор (G): «Мобилизация» / Mobilization
      - ур.1: kept: MaximumSystemManpower +40%
      - ур.2: kept: MaximumSystemManpower +80%
    - роль Сенатор (S): «Всеобщая мобилизация» / National Mobilization
      - ур.1: converted: MaximumEmpireManpowerStock +50% @Empire  [Manpower]
    - роль Диктатор (D): «Всеобщая мобилизация» / National Mobilization
      - ур.1: converted: MaximumEmpireManpowerStock +40% @Empire  [Manpower]
      - ур.2: converted: MaximumEmpireManpowerStock +80% @Empire  [Manpower]
      - ур.3: converted: MaximumEmpireManpowerStock +120% @Empire  [Manpower]
  - **`HeroSkill06BranchMinorFaction04`** - Авторитарное лидерство / Authoritarian Leadership  (tags: ColonizedStarSystem)
    - описание: Тотальное господство воли Героя обусловливает повышенную отдачу и энтузиазм по всей социальной вертикали сверху донизу. Возрастающий энтузиазм ведет к увеличению трудовых ресурсов на строительстве флота, а сказания о деяниях Героя усиливают общественную поддержку финансирования военных программ.
    - ур.1 [мастерство Command x1]: флот: MaximumShipManpower +25% @GarrisonFleet/Ship; SiegePower +10% @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: MaximumShipManpower +50% @GarrisonFleet/Ship; SiegePower +20% @GarrisonFleet
    - роль Адмирал (A): «Набор экипажей / Огневая мощь» / Crew Recruitment / Firepower
      - ур.1: kept: MaximumShipManpower +25% @GarrisonFleet/Ship; kept: SiegePower +10% @GarrisonFleet
      - ур.2: kept: MaximumShipManpower +50% @GarrisonFleet/Ship; kept: SiegePower +20% @GarrisonFleet
    - роль Губернатор (G): «Мобилизация / Гарнизонная выучка» / Mobilization / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @ColonizedStarSystem  [Offense]; converted: NetSystemManpower +6 @ColonizedStarSystem  [Manpower]
      - ур.2: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem  [Offense]; converted: NetSystemManpower +12 @ColonizedStarSystem  [Manpower]
    - роль Сенатор (S): «Всеобщая мобилизация / Военные ассигнования» / National Mobilization / Military Appropriations
      - ур.1: converted: ShipDamage +0.1 @Empire/GarrisonFleet  [Offense]; converted: MaximumEmpireManpowerStock +31.2% @Empire  [Manpower]
    - роль Диктатор (D): «Всеобщая мобилизация / Военные ассигнования» / National Mobilization / Military Appropriations
      - ур.1: converted: ShipDamage +0.08 @Empire/GarrisonFleet  [Offense]; converted: MaximumEmpireManpowerStock +25% @Empire  [Manpower]
      - ур.2: converted: ShipDamage +0.16 @Empire/GarrisonFleet  [Offense]; converted: MaximumEmpireManpowerStock +50% @Empire  [Manpower]
      - ур.3: converted: ShipDamage +0.24 @Empire/GarrisonFleet  [Offense]; converted: MaximumEmpireManpowerStock +75% @Empire  [Manpower]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchMinorFaction04`** - Личные связи / Personal Networking  (tags: ColonizedStarSystem)
    - описание: Герой настойчиво трудится для обеспечения полной занятости населения в своей Системе, используя финансы клана Меос и предприятия клана Омокар, чтобы снизить процент безработицы почти до нуля. Обеспеченное работой население создает в Системе промышленный бум.
    - ур.1 [мастерство Labour x1]: система: PlanetRawIndustry +6 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetIndustry Addition (6 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden) | сенат: PlanetRawIndustry +1 @Empire/ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetIndustry Addition (1 Multiplication $(Population)) @Empire/ColonizedStarSystem/ColonizedPlanet (hidden)
    - роль Адмирал (A): «Бронепластины» / Plating
      - ур.1: converted: ShipHullPlatingAbsorptionPercent +1.1 @GarrisonFleet  [Production]
    - роль Губернатор (G): «Индустриальный рост» / Industrial Drive
      - ур.1: kept: PlanetRawIndustry +6 @ColonizedStarSystem/ColonizedPlanet; kept: BonusHeroPlanetIndustry Addition (6 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); converted: SystemProduction +13 @ColonizedStarSystem  [Production]
    - роль Сенатор (S): «Государственная индустрия» / State Industry
      - ур.1: kept: PlanetRawIndustry +1 @Empire/ColonizedStarSystem/ColonizedPlanet; kept: BonusHeroPlanetIndustry Addition (1 Multiplication $(Population)) @Empire/ColonizedStarSystem/ColonizedPlanet (hidden); converted: PlanetRawIndustry +4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]
    - роль Диктатор (D): «Государственная индустрия» / State Industry
      - ур.1: kept: PlanetRawIndustry +1 @Empire/ColonizedStarSystem/ColonizedPlanet  (x1.6 for the dictator); kept: BonusHeroPlanetIndustry Addition (1 Multiplication $(Population)) @Empire/ColonizedStarSystem/ColonizedPlanet (hidden)  (x1.6 for the dictator); converted: PlanetRawIndustry +6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Production]

### HeroSkillTreeEndless - Навыки Вечных / Endless Skills
Эта ветка содержит специфические навыки Вечных.

Героев с этим деревом: 2 (QuestHeroEndless01, QuestHeroEndless02)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchEndless`** - Тайные знания / Arcane Knowledge  (tags: ColonizedStarSystem)
    - описание: Общаясь с эхом Пыли, найденными на планетах, Герой может погрузиться в историю и получить утраченные знания.
    - ур.1 [мастерство Labour x1]: система: SystemResearch +10 @ColonizedStarSystem; SystemResearch Addition (10 Multiplication $(AnomalyCount)) @ColonizedStarSystem; BonusHeroSystemScience +10 @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (10 Multiplication $(AnomalyCount)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemResearch +20 @ColonizedStarSystem; SystemResearch Addition (20 Multiplication $(AnomalyCount)) @ColonizedStarSystem; BonusHeroSystemScience +20 @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (20 Multiplication $(AnomalyCount)) @ColonizedStarSystem (hidden)
    - роль Адмирал (A): «Дальняя разведка» / Long-Range Scouting
      - ур.1: converted: VisionRange +1 @GarrisonFleet  [Science]
      - ур.2: converted: VisionRange +2 @GarrisonFleet  [Science]
    - роль Губернатор (G): «Научные центры» / Research Centers
      - ур.1: kept: SystemResearch +10 @ColonizedStarSystem; kept: SystemResearch Addition (10 Multiplication $(AnomalyCount)) @ColonizedStarSystem; kept: BonusHeroSystemScience +10 @ColonizedStarSystem (hidden); kept: BonusHeroSystemScience Addition (10 Multiplication $(AnomalyCount)) @ColonizedStarSystem (hidden)
      - ур.2: kept: SystemResearch +20 @ColonizedStarSystem; kept: SystemResearch Addition (20 Multiplication $(AnomalyCount)) @ColonizedStarSystem; kept: BonusHeroSystemScience +20 @ColonizedStarSystem (hidden); kept: BonusHeroSystemScience Addition (20 Multiplication $(AnomalyCount)) @ColonizedStarSystem (hidden)
    - роль Сенатор (S): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +1 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
    - роль Диктатор (D): «Научный комитет» / Science Committee
      - ур.1: converted: PlanetRawScience +0.8 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
      - ур.2: converted: PlanetRawScience +1.6 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
      - ур.3: converted: PlanetRawScience +2.4 @Empire/ColonizedStarSystem/ColonizedPlanet  [Science]
  - **`HeroSkill02BranchEndless`** - Галактическое чутье / Galactic Sense
    - описание: Чувствуя Пыль, плывущую в пространстве Галактики, Герой способен "видеть" далеко за пределами возможностей физических устройств, увеличивая дальность обзора и полета своего флота.
    - ур.1 [мастерство Command x1]: флот: ShipMaximumMovement +1 @GarrisonFleet; VisionRange +1 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipMaximumMovement +2 @GarrisonFleet; VisionRange +2 @GarrisonFleet
    - роль Адмирал (A): «Манёвренность» / Maneuverability
      - ур.1: kept: ShipMaximumMovement +1 @GarrisonFleet; kept: VisionRange +1 @GarrisonFleet
      - ур.2: kept: ShipMaximumMovement +2 @GarrisonFleet; kept: VisionRange +2 @GarrisonFleet
    - роль Губернатор (G): «Торговые пути» / Trade Routes
      - ур.1: converted: TradingRoutesBonus_Production +40% @ColonizedStarSystem  [Mobility]
      - ур.2: converted: TradingRoutesBonus_Production +80% @ColonizedStarSystem  [Mobility]
    - роль Сенатор (S): «Имперская навигация» / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +2 @Empire/GarrisonFleet  [Mobility]
    - роль Диктатор (D): «Имперская навигация» / Imperial Navigation
      - ур.1: converted: ShipMaximumMovement +1.6 @Empire/GarrisonFleet  [Mobility]
      - ур.2: converted: ShipMaximumMovement +3.2 @Empire/GarrisonFleet  [Mobility]
      - ур.3: converted: ShipMaximumMovement +4.8 @Empire/GarrisonFleet  [Mobility]
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchEndless`** - Смажьте колеса / Grease the Wheels
    - описание: Бюджетные навыки обычно в большом почете на любом космофлоте. Искусное манипулирование финансовыми ресурсами позволяет Герою обеспечивать улучшенное снабжение и техобслуживание двигателей искривленного пространства и щитовых систем.
    - ур.1 [мастерство Command x1]: флот: GarrisonMoneyUpkeep -20% @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: GarrisonMoneyUpkeep -40% @GarrisonFleet
    - роль Адмирал (A): «Экономия флота» / Fleet Economy
      - ур.1: kept: GarrisonMoneyUpkeep -20% @GarrisonFleet
      - ур.2: kept: GarrisonMoneyUpkeep -40% @GarrisonFleet
    - роль Губернатор (G): «Бережливое управление» / Frugal Administration
      - ур.1: converted: SystemMoneyUpkeep -20% @ColonizedStarSystem  [Upkeep]
      - ур.2: converted: SystemMoneyUpkeep -40% @ColonizedStarSystem  [Upkeep]
    - роль Сенатор (S): «Режим экономии» / Austerity
      - ур.1: converted: SystemMoneyUpkeep -20% @Empire/ColonizedStarSystem  [Upkeep]
    - роль Диктатор (D): «Режим экономии» / Austerity
      - ур.1: converted: SystemMoneyUpkeep -16% @Empire/ColonizedStarSystem  [Upkeep]
      - ур.2: converted: SystemMoneyUpkeep -32% @Empire/ColonizedStarSystem  [Upkeep]
      - ур.3: converted: SystemMoneyUpkeep -48% @Empire/ColonizedStarSystem  [Upkeep]
  - **`HeroSkill04BranchEndless`** - Интеграция оборонных ИИ-ресурсов / Defensive AI Integration
    - описание: Специализированная система Искусственного Интеллекта, разрабатываемая группой волонтеров под руководством Героя с целью оптимизации тактических компьютерных расчетов. Ее высокоскоростные характеристики позволяют повысить оборонительный потенциал космофлота, увеличивая коэффициент Выживаемости в бою.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumShield +0.1 @GarrisonFleet
    - роль Адмирал (A): «Крепкие корпуса» / Hardened Hulls
      - ур.1: kept: FleetMaximumShield +0.1 @GarrisonFleet
    - роль Губернатор (G): «Укреплённые гарнизоны» / Fortified Garrisons
      - ур.1: converted: MaximumSystemManpower +20%  [Defense]
    - роль Сенатор (S): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.05 @Empire/GarrisonFleet  [Defense]
    - роль Диктатор (D): «Бронирование флотов» / Fleet Armor Programs
      - ур.1: converted: FleetMaximumHealth +0.08 @Empire/GarrisonFleet  [Defense]
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchEndless`** - Хронозаряды / Chrono Ordnance
    - описание: То ли эта способность связана с многомерной физикой, то ли это мистический навык управления Пылью, но герой может заставить орудия флота проскочить вперед на пару единиц Планка. Это помогает обойти защиту вражеских щитов.
    - ур.1 [мастерство Command x1]: флот: ShipShieldPenetrationFactor +0.4 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipShieldPenetrationFactor +0.8 @GarrisonFleet
    - роль Адмирал (A): «Огневая мощь» / Firepower
      - ур.1: kept: ShipShieldPenetrationFactor +0.4 @GarrisonFleet
      - ур.2: kept: ShipShieldPenetrationFactor +0.8 @GarrisonFleet
    - роль Губернатор (G): «Гарнизонная выучка» / Garrison Training
      - ур.1: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.4 @ColonizedStarSystem  [Offense]
      - ур.2: converted: GroundBattleDefenderTroopsMaxHealthMultiplier +0.8 @ColonizedStarSystem  [Offense]
    - роль Сенатор (S): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.4 @Empire/GarrisonFleet  [Offense]
    - роль Диктатор (D): «Военные ассигнования» / Military Appropriations
      - ур.1: converted: ShipDamage +0.32 @Empire/GarrisonFleet  [Offense]
      - ур.2: converted: ShipDamage +0.64 @Empire/GarrisonFleet  [Offense]
      - ур.3: converted: ShipDamage +0.96 @Empire/GarrisonFleet  [Offense]
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill06BranchEndless`** - Бесконечная харизма / Endless Charisma  (tags: ColonizedStarSystem)
    - описание: Так как Герой - это древнее существо, рожденное в другую эпоху, он пользуется глубоким, неизменным уважением всех, кто ему служит. Бойцы с удовольствием переносят трудности, чтобы приблизиться к такой внушающий благоговейный ужас фигуре.
    - ур.1 [мастерство Command x1]: флот: MaximumShipManpower +40% @GarrisonFleet/Ship; FleetManpowerRefillRateModifier +0.4 @GarrisonFleet
    - роль Адмирал (A): «Полевой ремонт / Набор экипажей» / Field Repairs / Crew Recruitment
      - ур.1: kept: MaximumShipManpower +40% @GarrisonFleet/Ship; kept: FleetManpowerRefillRateModifier +0.4 @GarrisonFleet
    - роль Губернатор (G): «Пополнение войск / Мобилизация» / Troop Replenishment / Mobilization
      - ур.1: converted: MaximumSystemManpower +200  [Sustain]; converted: NetSystemManpower +10 @ColonizedStarSystem  [Manpower]
    - роль Сенатор (S): «Службы ремонта / Всеобщая мобилизация» / Repair Corps / National Mobilization
      - ур.1: converted: HealthRegenerationRatio +0.1 @Empire/GarrisonFleet  [Sustain]; converted: MaximumEmpireManpowerStock +25% @Empire  [Manpower]
    - роль Диктатор (D): «Службы ремонта / Всеобщая мобилизация» / Repair Corps / National Mobilization
      - ур.1: converted: HealthRegenerationRatio +0.16 @Empire/GarrisonFleet  [Sustain]; converted: MaximumEmpireManpowerStock +40% @Empire  [Manpower]

