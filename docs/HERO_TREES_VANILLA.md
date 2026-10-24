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
  - **`HeroSkill02BranchGeneric`** - Космический изгнанник / Cosmic Castaway
    - описание: Во время перелета Герой уводит маленькие разведгруппы от главного флота. Выполнение этой операции дает флоту дополнительное Поле Обзора и готовит членов экипажей к спецоперациям в глубоком космосе.
    - ур.1 [мастерство Curiosity x1]: флот: ExperiencePerTurn +4 @GarrisonFleet; VisionRange +1 @GarrisonFleet
    - ур.2 [мастерство Curiosity x2]: флот: ExperiencePerTurn +8 @GarrisonFleet; VisionRange +2 @GarrisonFleet
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchGeneric`** - Оптимизация руководства / Transformational Leadership  (tags: ColonizedStarSystem)
    - описание: Герой сосредоточен на построении позитивных отношений с правителями данной системы, формируя глобальное видение того, каким образом их совместная работа может способствовать процветанию империи.
    - ур.1 [мастерство Wit x1]: система: Happiness +10
    - ур.2 [мастерство Wit x2]: система: Happiness +20
  - **`HeroSkill04BranchGeneric`** - Гений вербовки / Conscription Genius  (tags: ColonizedStarSystem)
    - описание: Когда войска несут масштабные потери в кровопролитных войнах, правителям бывает непросто рекрутировать новых бойцов. Однако благодаря умно организованным пропагандистским кампаниям, использующим тщательно подобранные лозунги и рекламные материалы, Герой способен улучшить показатели армейского набора.
    - ур.1 [мастерство Labour x1]: система: MaximumSystemManpower +100 | сенат: MaximumEmpireManpowerStock +25% @Empire
    - ур.2 [мастерство Labour x2]: система: MaximumSystemManpower +200 | сенат: MaximumEmpireManpowerStock +25% @Empire
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchGeneric`** - Катализация Пылью / Dust Catalyzation
    - описание: Случайная встреча в глубоком космосе со скоплением неконцентрированной Пыли изменяет временное поле Героев, сдвигая их в несколько иную плоскость существования. Затем Герои быстро возвращаются в нормальную временную структуру, но остаточные эффекты этой аномалии повышают их отдачу и снижают потребность в отдыхе.
    - ур.1 [мастерство Curiosity x1]: система: Upkeep -25% @ColonizedStarSystem/HeroAssignedToSystem | герой: AssignmentCooldownBaseDuration -25%
    - ур.2 [мастерство Curiosity x2]: система: Upkeep -50% @ColonizedStarSystem/HeroAssignedToSystem | герой: AssignmentCooldownBaseDuration -50%
  - **`HeroSkill06BranchGeneric`** - Оружие на заказ / Bespoke Arms
    - описание: Обладая глубокими познаниями о системах вооружения со всех уголков галактики - от люмерийских торпедных технологий до оружия с фазированными лучами с Мавроса - Герой может получить дополнительные бонусы при производстве оружия.
    - ур.1 [мастерство Command x1]: флот: Damage +0.8 @Ship
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchGeneric`** - Трансцендентное мировоззрение / Transcendental Outlook
    - описание: Герои, конечно, открывают впечатляющие перспективы развития и созидают стратегии, способные изменить судьбы целых империй, но обходятся они очень недешево. Огромные кортежи, множество резиденций и пунктуально составленные списки предъявляемых требований влетают в копеечку. К счастью, обретение этого навыка позволяет Герою отринуть эти мирские заботы, и тогда стоимость его содержания убывает до нуля.
    - ур.1 [мастерство Curiosity x1]: флот: Upkeep x0 @.GarrisonFleet/Hero,HeroAssignedToFleet | герой: Upkeep -10% @Empire/ColonizedStarSystem/Hero; Upkeep -10% @Empire/GarrisonFleet/Hero (hidden)

### HeroSkillTreeAdmiral - Навыки Стража / Guardian skills
Этот блок содержит полный набор специальных навыков Стража.

Героев с этим деревом: 18 (HeroTerrans03, HeroTerrans05, HeroTimeLords03, HeroUnfallen04, HeroCravers01, HeroCravers03, HeroVenetians01, HeroVenetians05, HeroVampirilis01, HeroVampirilis02, HeroHisshos01, HeroMavros01, HeroPilgrims01, HeroTikanans01, QuestHeroEndless01, HeroTerransQuest01, HeroQuestHoratio04, HeroGreenman02)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchAdmiral`** - Фанат эффективности / Efficiency Aficionado  (tags: ColonizedStarSystem)
    - описание: Герой демонстрирует маниакальное стремление оптимизировать методологию принятия решений. Индустриальные комплексы и работники под его руководством извлекают выгоду (порой не слишком охотно) из этого постоянного шквала новых блестящих идей.
    - ур.1 [мастерство Labour x1]: система: SystemProduction +5 @ColonizedStarSystem; SystemProduction Addition (5 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; BonusHeroSystemFood +5 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (5 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemProduction +10 @ColonizedStarSystem; SystemProduction Addition (10 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; BonusHeroSystemFood +10 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (10 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
  - **`HeroSkill02BranchAdmiral`** - В строгом соответствии с нормативами / By The Book
    - описание: Герой лично контролирует основные аспекты строительства флота, в то время как инженеры и рабочие в доках неустанно заботятся о том, чтобы их корабли были максимально подготовлены к наихудшему, что может ожидать их в глубинах космоса.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: FleetMaximumHealth +0.2 @GarrisonFleet
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchAdmiral`** - Документы, пожалуйста / Papers Please
    - описание: Хранение детальных сведений о каждом члене экипажа в составе руководимого им флота позволяет Герою быстро и эффективно проводить мобилизацию даже в тех случаях, когда число бойцов на борту превышает обычное.
    - ур.1 [мастерство Curiosity x1]: флот: FleetManpowerRefillRateModifier +0.4 @GarrisonFleet | сенат: PeacefulTreatyCostReduction -0.5 @Empire; AggressiveTreatyCostReduction -0.5 @Empire
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill04BranchAdmiral`** - Глас народа / Vox Populi  (tags: ColonizedStarSystem)
    - описание: Герой старается регулярно посещать региональные верфи и промзоны, стимулируя этим их производительность и создавая атмосферу дружеского соревнования.
    - ур.1 [мастерство Labour x1]: система: SystemProduction +15% @ColonizedStarSystem
    - ур.2 [мастерство Labour x2]: система: SystemProduction +30% @ColonizedStarSystem
  - **`HeroSkill05BranchAdmiral`** - Железная дисциплина / Iron Discipline
    - описание: Во время обороны, Герой способен воодушевить экипажи судов лучше защищать корабли, наполняя своих подчиненных стальной решимостью и сосредоточенностью.
    - ур.1 [мастерство Command x1]: флот: ShipHullPlatingAbsorptionPercent +0.2 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipHullPlatingAbsorptionPercent +0.4 @GarrisonFleet
  - **`HeroSkill06BranchAdmiral`** - Пострелят. система наведения на цель / Post-Relativistic Targeting
    - описание: Используя теоретические разработки на основе пострелятивистской математики, Герой способен просчитывать неконвенциональные стратегии ведения огня, извлекая выгоду из использования ноль-гравитационных сценариев и повышая общую огневую мощь Флота.
    - ур.1 [мастерство Command x1]: флот: ShipDamage +0.2 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipDamage +0.4 @GarrisonFleet
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchAdmiral`** - Гроссмейстер военного дела / Battlefield Grandmaster
    - описание: Владея практически безошибочным пониманием военной стратегии, Герой способен увеличивать насколько необходимо наступательные и оборонительные возможности флота.
    - ур.1 [мастерство Command x1]: флот: ShipDamage +0.25 @GarrisonFleet; FleetMaximumShield +0.33 @GarrisonFleet

### HeroSkillTreeAdventurer - Навыки Искателя / Seeker skills
Этот блок содержит полный набор специальных навыков Искателя.

Героев с этим деревом: 16 (HeroSophons02, HeroHoratio02, HeroUnfallen02, HeroUnfallen03, HeroCravers04, HeroVenetians02, HeroVenetians03, HeroVampirilis03, HeroBots01, HeroGnashasts01, HeroRemnant01, HeroPulsos01, QuestHeroEndless02, HeroTerransQuest03, HeroQuestVirus01, HeroPirate01)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchAdventurer`** - Извлекая пользу из безграничной фантазии / Blue Sky Speculator  (tags: ColonizedStarSystem)
    - описание: Иногда, наибольшие дивиденды может принести ставка на неизвестное. Герой стимулирует корпоративное финансирование научных исследований в своей системе, результатом чего оказывается увеличение показателя Науки.
    - ур.1 [мастерство Wit x1]: система: SystemResearch +15 @ColonizedStarSystem; BonusHeroSystemScience +15 @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemResearch +30 @ColonizedStarSystem; BonusHeroSystemScience +30 @ColonizedStarSystem (hidden)
  - **`HeroSkill02BranchAdventurer`** - Улучшенная астронавигация / Enhanced Astronavigation
    - описание: Улучшенная астронавигация. Даже через тысячи лет после эпохи парусов и секстантов, ориентация по звездам представляет собой ценный навык. Отличное знание рисунка созвездий наделяет флот Героя усовершенствованными возможностями Видения и Исследования.
    - ур.1 [мастерство Curiosity x1]: флот: FreeMovementSpeed x1.5 @GarrisonFleet; VisionRange +1 @GarrisonFleet
    - ур.2 [мастерство Curiosity x2]: флот: FreeMovementSpeed x2 @GarrisonFleet; VisionRange +2 @GarrisonFleet
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchAdventurer`** - Личное ополчение / Personal Militia
    - описание: Герой набирает разношерстную команду из бывших солдат, чокнутых ученых и лиц без определенных занятий, задачей которой является укрепление наземной обороны в планетной системе. Когда оно не занято ведением партизанской войны, это подразделение работает с простонародьем путем распространения сказаний и застольных песен, прославляющих деянья героя.
    - ур.1 [мастерство Wit x1]: система: PlanetRawPrestige +1 @ColonizedStarSystem/ColonizedPlanet; GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @ColonizedStarSystem; BonusHeroSystemPrestige +1 @ColonizedStarSystem/ColonizedPlanet (hidden)
    - ур.2 [мастерство Wit x2]: система: PlanetRawPrestige +2 @ColonizedStarSystem/ColonizedPlanet; GroundBattleDefenderTroopsMaxHealthMultiplier +0.2 @ColonizedStarSystem; BonusHeroSystemPrestige +2 @ColonizedStarSystem/ColonizedPlanet (hidden)
  - **`HeroSkill04BranchAdventurer`** - Программа астроподготовки / Startrekking Instruction
    - описание: Десятилетия звездоплавания наделили Героя опытом преодоления всевозможных проблем - от преодоления червячных переходов до прохождения вблизи газовых гигантов. Сейчас Герой способен превратить эти бесценные интуиции в тренировочную программу для всех командиров имперского космофлота, существенно повысив тем самым его скоростные характеристики.
    - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +2 @GarrisonFleet; MaximumShipProbeStock +1 @Ship | сенат: ShipMaximumMovement +1 @Empire/GarrisonFleet; MaximumShipProbeStock +1 @Empire/GarrisonFleet/Ship,ShipRoleHero
    - ур.2 [мастерство Curiosity x2]: флот: ShipMaximumMovement +4 @GarrisonFleet; MaximumShipProbeStock +2 @Ship | сенат: ShipMaximumMovement +1 @Empire/GarrisonFleet; MaximumShipProbeStock +1 @Empire/GarrisonFleet/Ship,ShipRoleHero
  - **`HeroSkill05BranchAdventurer`** - Пострелят. система наведения на цель / Post-Relativistic Targeting
    - описание: Используя теоретические разработки на основе пострелятивистской математики, Герой способен просчитывать неконвенциональные стратегии ведения огня, извлекая выгоду из использования ноль-гравитационных сценариев и повышая общую огневую мощь Флота.
    - ур.1 [мастерство Command x1]: флот: ShipDamage +0.1 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipDamage +0.2 @GarrisonFleet
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill06BranchAdventurer`** - Щит Ушедших / Shield of the Lost
    - описание: Герой обнаруживает в недрах древнейшего строения, относящегося еще к эпохе до появления Вечных, загадочный объект, давно утративший силу. Однако после контакта с насыщенным Пылью телом Героя, он реактивируется, обнаруживая способность перенаправлять кинетическую энергию, уменьшая Ущерб Кораблю.
    - ур.1 [мастерство Command x1]: флот: ShieldAbsorptionPercent +0.4 @Ship
    - ур.2 [мастерство Command x2]: флот: ShieldAbsorptionPercent +0.8 @Ship
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchAdventurer`** - Преодоление бюрократизма / Red Tape Scyther
    - описание: Густая чащоба бюрократических норм, регулирующих процедуру распределения Героев, для Героя ничто. Применяя словесный напор, фальсификацию, а иногда и шантаж, Герой может получить назначение в ранее закрытые для него зоны и быть переназначен вдвое быстрее, чем положено по процедуре.
    - ур.1 [мастерство Curiosity x1]: герой: AssignmentCooldownBaseDuration -25% @Hero; CanBeAssignedOutOfTerritory Force 1 @Hero (hidden)

### HeroSkillTreeAdministrator - Навыки Куратора / Overseer skills
Эта ветка содержит специфические навыки Куратора.

Героев с этим деревом: 14 (HeroSophons01, HeroTerrans01, HeroTerrans02, HeroHoratio01, HeroTimeLords01, HeroTimeLords02, HeroUnfallen01, HeroCravers02, HeroCraversPrime, HeroBenthys01, HeroHaroshems01, HeroZvali01, HeroNiris01, HeroAmoeba02)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchAdministrator`** - Агрологистика / Farming Logistics  (tags: ColonizedStarSystem)
    - описание: Оптимизация технологий выращивания растений и сезонного севооборота, не говоря уже об усовершенствовании методов работы на аквафермах, позволяет значительно увеличить производство Пищи в системах, управляемых этим Героем.
    - ур.1 [мастерство Labour x1]: система: SystemGrowth +10 @ColonizedStarSystem; SystemGrowth Addition (10 Multiplication $(OwnedTeemingPlanets)) @ColonizedStarSystem; BonusHeroSystemFood +10 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (10 Multiplication $(OwnedTeemingPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemGrowth +20 @ColonizedStarSystem; SystemGrowth Addition (20 Multiplication $(OwnedTeemingPlanets)) @ColonizedStarSystem; BonusHeroSystemFood +20 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (20 Multiplication $(OwnedTeemingPlanets)) @ColonizedStarSystem (hidden)
  - **`HeroSkill02BranchAdministrator`** - Документы, пожалуйста / Papers Please  (tags: ColonizedStarSystem)
    - описание: Хранение детальных сведений о каждом члене экипажа в составе руководимого им флота позволяет Герою быстро и эффективно проводить мобилизацию даже в тех случаях, когда число бойцов на борту превышает обычное.
    - ур.1 [мастерство Command x1]: флот: FleetManpowerRefillRateModifier +0.2 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: FleetManpowerRefillRateModifier +0.4 @GarrisonFleet
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchAdministrator`** - Фанат эффективности / Efficiency Aficionado  (tags: ColonizedStarSystem)
    - описание: Герой демонстрирует маниакальное стремление оптимизировать методологию принятия решений. Индустриальные комплексы и работники под его руководством извлекают выгоду (порой не слишком охотно) из этого постоянного шквала новых блестящих идей.
    - ур.1 [мастерство Labour x1]: система: SystemProduction +10 @ColonizedStarSystem; SystemProduction Addition (10 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; BonusHeroSystemFood +10 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (10 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemProduction +20 @ColonizedStarSystem; SystemProduction Addition (20 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; BonusHeroSystemFood +20 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (20 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
  - **`HeroSkill04BranchAdministrator`** - Канцелярская правка / Clerical Corrections  (tags: ColonizedStarSystem)
    - описание: Меткий глаз иногда в состоянии углядеть неувязки даже в самых запутанных комплексах данных. С течением времени это приводит к периодическому открытию "забытых" авуаров, что выражается в общем приросте доступных активов.
    - ур.1 [мастерство Labour x1]: система: DepositValue +1 @ColonizedStarSystem/Planet/ResourceDeposit | сенат: DepositValue +0.5 @Empire/ColonizedStarSystem/Planet/ResourceDeposit
    - ур.2 [мастерство Labour x2]: система: DepositValue +1 @ColonizedStarSystem/Planet/ResourceDeposit; Happiness Addition (5 Multiplication $(OwnedLuxuryDeposits)) @ColonizedStarSystem | сенат: DepositValue +0.5 @Empire/ColonizedStarSystem/Planet/ResourceDeposit
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchAdministrator`** - Регулятор поведения масс / Crowd Controller  (tags: ColonizedStarSystem)
    - описание: Герой-Куратор правит железной рукой, будь то на корабле или на планете. Когда все работает как часы, жизнь становится несколько менее хаотичной даже в перенаселенных метрополиях самых обширных империй.
    - ур.1 [мастерство Wit x1]: система: OverPopulationPenalties -20% @ColonizedStarSystem; OverColonizationPenalties -20% @ColonizedStarSystem
    - ур.2 [мастерство Wit x2]: система: OverPopulationPenalties -40% @ColonizedStarSystem; OverColonizationPenalties -40% @ColonizedStarSystem
  - **`HeroSkill06BranchAdministrator`** - Проводник провидца / Prescient Guide
    - описание: С помощью Пыли Герой может проецировать свое сознание в саму ткань кораблей, что позволяет флоту выбирать наилучший курс и повышает силу щита.
    - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +1 @GarrisonFleet; FleetMaximumShield +0.15 @GarrisonFleet
    - ур.2 [мастерство Curiosity x2]: флот: ShipMaximumMovement +2 @GarrisonFleet; FleetMaximumShield +0.3 @GarrisonFleet
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchAdministrator`** - Бухгалтер / Bean Counter  (tags: ColonizedStarSystem)
    - описание: Настаивая на необходимости жесткого финансового контроля, Герой обеспечивает рост промышленного и сельскохозяйственного производства, а также добычи Пыли, поскольку работники не могут сливать на черный рынок избытки продукции.
    - ур.1 [мастерство Labour x1]: система: SystemProduction +16% @ColonizedStarSystem; SystemGrowth +16% @ColonizedStarSystem; SystemMoney +16% @ColonizedStarSystem

### HeroSkillTreeCorporate - Навыки Советника / Counselor skills
Эта ветка содержит специфические навыки Советника.

Героев с этим деревом: 15 (HeroSophons03, HeroSophons04, HeroSophons05, HeroTerrans04, HeroHoratio03, HeroHoratio04, HeroTimeLords04, HeroVenetians04, HeroVampirilis04, HeroAmoeba01, HeroDeuyivans01, HeroEyders01, HeroBhagabas01, HeroTerransQuest02, HeroGreenman01)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchCorporate`** - Человек со средствами / Person of Means  (tags: ColonizedStarSystem)
    - описание: Успешный финансист, Герой также вкладывает значительные личные средства в экономическое развитие конкретных систем.
    - ур.1 [мастерство Wit x1]: система: SystemMoney +5 @ColonizedStarSystem; SystemMoney Addition (5 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem; BonusHeroSystemDust +5 @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (5 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemMoney +10 @ColonizedStarSystem; SystemMoney Addition (10 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem; BonusHeroSystemDust +10 @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (10 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem (hidden)
  - **`HeroSkill02BranchCorporate`** - Пострелят. система наведения на цель / Post-Relativistic Targeting
    - описание: Используя теоретические разработки на основе пострелятивистской математики, Герой способен просчитывать неконвенциональные стратегии ведения огня, извлекая выгоду из использования ноль-гравитационных сценариев и повышая общую огневую мощь Флота.
    - ур.1 [мастерство Command x1]: флот: ShipDamage +0.05 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipDamage +0.1 @GarrisonFleet
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchCorporate`** - Извлекая пользу из безграничной фантазии / Blue Sky Speculator  (tags: ColonizedStarSystem)
    - описание: Иногда, наибольшие дивиденды может принести ставка на неизвестное. Герой стимулирует корпоративное финансирование научных исследований в своей системе, результатом чего оказывается увеличение показателя Науки.
    - ур.1 [мастерство Wit x1]: система: SystemResearch +30 @ColonizedStarSystem; BonusHeroSystemScience +30 @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemResearch +60 @ColonizedStarSystem; BonusHeroSystemScience +60 @ColonizedStarSystem (hidden)
  - **`HeroSkill04BranchCorporate`** - Неликвидные активы / Non-Liquid Assets
    - описание: Внешний вид кораблей Героя-Советника всегда безупречен. Используя определенную часть своих средств на укрепление переборок и отражающие покрытия, Герой увеличивает тем самым также и эффективность флота в бою.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet; ShipHullPlatingAbsorptionPercent +0.1 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: FleetMaximumHealth +0.2 @GarrisonFleet; ShipHullPlatingAbsorptionPercent +0.2 @GarrisonFleet
  - **`HeroSkill05BranchCorporate`** - Эксперт по пиару / Public Relations Expert  (tags: ColonizedStarSystem)
    - описание: Герой - умелый оратор и притягательная личность. Улучшает репутацию своей империи всюду, куда попадает, или куда доходят его выверенные послания.
    - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +20 @ColonizedStarSystem; SystemEmpirePoint +5% @ColonizedStarSystem; BonusHeroSystemPrestige +20 @ColonizedStarSystem (hidden) | сенат: EmpireEmpirePoint Addition (50 Multiplication $(NumberOfTradingCompanies)) @Empire; BonusHeroSystemPrestige Addition (50 Multiplication $(NumberOfTradingCompanies)) @Empire (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemEmpirePoint +40 @ColonizedStarSystem; SystemEmpirePoint +10% @ColonizedStarSystem; BonusHeroSystemPrestige +40 @ColonizedStarSystem (hidden) | сенат: EmpireEmpirePoint Addition (50 Multiplication $(NumberOfTradingCompanies)) @Empire; BonusHeroSystemPrestige Addition (50 Multiplication $(NumberOfTradingCompanies)) @Empire (hidden)
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill06BranchCorporate`** - Сенсорные системы / Sensitive Systems
    - описание: Глубоко проникая в физические и математические аспекты распространения волн, Герой развивает и совершенствует сенсорные системы дальнего радиуса действия. Это не только увеличивает поле доступного восприятия, но и помогает избегать контактов со всякого рода космическими объектами, удешевляя тем самым обслуживание кораблей.
    - ур.1 [мастерство Curiosity x1]: флот: VisionRange +4 @GarrisonFleet; GarrisonMoneyUpkeep -20% @GarrisonFleet
    - ур.2 [мастерство Curiosity x2]: флот: VisionRange +8 @GarrisonFleet; GarrisonMoneyUpkeep -40% @GarrisonFleet
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchCorporate`** - Неукоснительно радуясь / The Joy Initiative  (tags: ColonizedStarSystem)
    - описание: Одобрение - показатель, не очень-то поддающийся измерению. Когда системой командует могущественный Герой-Советник, кто может сказать, происходит ли Одобрение населения от истинной любви или от боязни последствий?
    - ур.1 [мастерство Wit x1]: система: Happiness Force 100 @ColonizedStarSystem; SystemMoneyUpkeep -20% @ColonizedStarSystem

### HeroSkillTreeSophons - Навыки Софонов / Sophon skills
Эта ветка содержит специфические навыки Софонов.

Героев с этим деревом: 5 (HeroSophons01, HeroSophons02, HeroSophons03, HeroSophons04, HeroSophons05)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchSophons`** - Академическое рецензирование / Peer Review  (tags: ColonizedStarSystem)
    - описание: Герой проявляет интерес к научным исследованиям в подвластной ему системе, внося свой вклад в обеспечение должного уровня функционирования научно-образовательной системы.
    - ур.1 [мастерство Wit x1]: система: SystemResearch +10 @ColonizedStarSystem; BonusHeroSystemScience +10 @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemResearch +10 @ColonizedStarSystem; SystemResearch Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemScience +10 @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
  - **`HeroSkill02BranchSophons`** - Галактическое чутье / Galactic Sense
    - описание: Чувствуя Пыль, плывущую в пространстве Галактики, Герой способен "видеть" далеко за пределами возможностей физических устройств, увеличивая дальность обзора и полета своего флота.
    - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +1 @GarrisonFleet; VisionRange +1 @GarrisonFleet
    - ур.2 [мастерство Curiosity x2]: флот: ShipMaximumMovement +2 @GarrisonFleet; VisionRange +2 @GarrisonFleet
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchSophons`** - Жгучий свет / The Burning Light
    - описание: Тщательно проанализировав критические энергетические потребности флота, Герой может перенастроить системы энергетического вооружения таким образом, чтобы они производили больше мощности, тем самым увеличивая урон от них.
    - ур.1 [мастерство Command x1]: флот: ShipDamageEnergy +0.2 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipDamageEnergy +0.4 @GarrisonFleet
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill04BranchSophons`** - Прикладной нонконформист / Applied Nonconformist  (tags: ColonizedStarSystem)
    - описание: Смело идти туда, где раньше не ступала нога Софона. Софон, направленный в систему, полную аномалий, проведет несколько лет, исследуя все ее особенности. Интерес и одобрение населения будут увеличиваться, пока продолжается его работа.
    - ур.1 [мастерство Wit x1]: система: Happiness Addition (8 Multiplication $(AnomalyCount)) @ColonizedStarSystem | сенат: Happiness +5 @Empire/ColonizedStarSystem
    - ур.2 [мастерство Wit x2]: система: Happiness Addition (16 Multiplication $(AnomalyCount)) @ColonizedStarSystem | сенат: Happiness +5 @Empire/ColonizedStarSystem
  - **`HeroSkill05BranchSophons`** - Небесная механика / Celestial Mechanics
    - описание: Используя свое "ощущение" влияния Пыли на звездные струны, Герой лучше может направлять свой флот по путям наименьшего сопротивления, увеличивая дальность его полета.
    - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +6 @GarrisonFleet
  - **`HeroSkill06BranchSophons`** - Миниатюрные зонды / Slim Probe Model
    - описание: Полностью трансформировав всю конструкцию зонда, Герой разрабатывает улучшенную модель, которая вдвое меньше размером, потребляет вчетверо меньше энергии и при этом в десять раз симпатичнее. Результат? Запас зондов увеличивается вдвое и, с учетом прорыва в энергоэффективности, щиты тоже становятся лучше!
    - ур.1 [мастерство Curiosity x1]: флот: FleetMaximumShield +0.2 @GarrisonFleet; MaximumShipProbeStock +2 @GarrisonFleet/Ship,ShipRoleExploration
    - ур.2 [мастерство Curiosity x2]: флот: FleetMaximumShield +0.4 @GarrisonFleet; MaximumShipProbeStock +4 @GarrisonFleet/Ship,ShipRoleExploration
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchSophons`** - Гениократ / Geniocrat  (tags: ColonizedStarSystem)
    - описание: Герой принимает меры против стагнации в политической и академической сферах, привнося определенную долю своей организаторской логики в функционирование данной системы, отчего показатель Науки буквально взлетает.
    - ур.1 [мастерство Wit x1]: система: SystemResearch +60% @ColonizedStarSystem

### HeroSkillTreeCravers - Навыки Пожирателей / Craver skills
Эта ветка содержит специфические навыки Пожирателей.

Героев с этим деревом: 5 (HeroCravers01, HeroCravers02, HeroCravers03, HeroCravers04, HeroCraversPrime)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchCravers`** - Эффект грубой силы / Blunt Force Trauma
    - описание: Демонстрируя свойственную Пожирателям нестандартность мышления, Герой разграбляет руины давно исчезнувшей цивилизации и крадет для своего личного флота оружие, позволяющее повысить причиняемый Кинетический ущерб и ущерб, наносимый Снарядами.
    - ур.1 [мастерство Command x1]: флот: ShipDamageProjectile +0.1 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipDamageProjectile +0.2 @GarrisonFleet
  - **`HeroSkill02BranchCravers`** - Принудительный труд / Forced Labor  (tags: ColonizedStarSystem)
    - описание: Герой отдает приказ о захвате и содержании в неволе представителей других рас, которых силой принуждают работать на заводах и фермах. Их отпустят и вознаградят за тяжкий труд… но не прежде, чем из планет, на которых их эксплуатируют, будут дочиста выкачаны все ресурсы...
    - ур.1 [мастерство Labour x1]: система: SystemProduction +5 @ColonizedStarSystem; SystemGrowth Addition (10 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; BonusHeroSystemIndustry +5 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (10 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemProduction +10 @ColonizedStarSystem; SystemGrowth Addition (20 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; BonusHeroSystemIndustry +10 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (20 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchCravers`** - Адаптация к условиям глубокого космоса / Deep Space Adaptation
    - описание: Члены экипажей кораблей, входящих во флот Героя, проходят программу спецподготовки, чтобы адаптировать их физические возможности к перемещениям в глубоком космосе. Это повышает их эффективность при обслуживании корабельных систем в экстремальных условиях опасных районов.
    - ур.1 [мастерство Curiosity x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet; MaximumShipProbeStock +1 @GarrisonFleet/Ship,ShipRoleExploration
    - ур.2 [мастерство Curiosity x2]: флот: FleetMaximumHealth +0.2 @GarrisonFleet; MaximumShipProbeStock +2 @GarrisonFleet/Ship,ShipRoleExploration
  - **`HeroSkill04BranchCravers`** - Счастливый случай / Lucky Windfall
    - описание: Организовав общесистемную лотерею с огромными выплатами, Герой получает колоссальные возможности для воздействия на общественное мнение.
    - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +10% @ColonizedStarSystem
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchCravers`** - Запах крови / Blood Scent
    - описание: Опыт, полученный в сотне битв, наделяет Героя мгновенной реакцией, основанной на интуитивной способности находить слабые места вражеского флота, что усиливает поражающую мощь его кораблей.
    - ур.1 [мастерство Command x1]: флот: ShipHullPlatingPenetrationFactor +0.4 @GarrisonFleet
  - **`HeroSkill06BranchCravers`** - Системы туннельной защиты / Tunneling Defense  (tags: ColonizedStarSystem)
    - описание: Герой в прямом смысле вгрызается в недра, создавая накануне битвы сеть подземных тоннелей. Обеспечив достижение успеха подобной стратегией, раса Пожирателей затем применяет ее по всей империи.
    - ур.1 [мастерство Labour x1]: система: GroundBattleDefenderTroopsMaxHealthMultiplier +0.25 | сенат: GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @Empire/ColonizedStarSystem
    - ур.2 [мастерство Labour x2]: система: GroundBattleDefenderTroopsMaxHealthMultiplier +0.5 | сенат: GroundBattleDefenderTroopsMaxHealthMultiplier +0.1 @Empire/ColonizedStarSystem
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchCravers`** - Бесконечное потребление / Endless Consumption  (tags: ColonizedStarSystem)
    - описание: В качестве парадоксального поворота эволюции, Герой запускает спровоцированную Пылью реакцию, которая увеличивает производство ресурсов в системе, управляемой Пожирателями, но также и ускоряет процесс их истощения.
    - ур.1 [мастерство Labour x1]: система: PlanetDepletionPerTurn +8 @ColonizedStarSystem/Planet; PlanetHeroFIDSIPercent +0.5 @ColonizedStarSystem/ColonizedPlanet,!PlanetDepletionMalus

### HeroSkillTreeTerrans - Навыки Единой Империи / United Empire Skills
Эта ветка включает навыки, специфические для Единой Империи.

Героев с этим деревом: 8 (HeroTerrans01, HeroTerrans02, HeroTerrans03, HeroTerrans04, HeroTerrans05, HeroTerransQuest01, HeroTerransQuest02, HeroTerransQuest03)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchTerrans`** - Защитный строй / Protective Formations
    - описание: Во время битвы Герой выстраивает флот таким образом, что корабли становятся менее восприимчивы к артиллерийскому огню.
    - ур.1 [мастерство Command x1]: флот: ShipShieldAbsorptionPercent +0.1 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipShieldAbsorptionPercent +0.2 @GarrisonFleet
  - **`HeroSkill02BranchTerrans`** - Воодушевляющий оратор / Rousing Orator  (tags: ColonizedStarSystem)
    - описание: Герой может формировать и использовать общественное мнение, с легкостью воодушевляя людей во время массовых митингов с помощью провокационных лозунгов и эмоциональных аргументов.
    - ур.1 [мастерство Wit x1]: система: PlanetRawPrestige +1 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetPrestige Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - ур.2 [мастерство Wit x2]: система: PlanetRawPrestige +2 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchTerrans`** - Кровь и земля / Blood and Land  (tags: ColonizedStarSystem)
    - описание: Как истинный патриот, Герой пропагандирует представление о том, что личность каждого человека глубоко связана с его родиной, тем самым значительно повышая количество призывников.
    - ур.1 [мастерство Labour x1]: система: MaximumSystemManpower +20%
    - ур.2 [мастерство Labour x2]: система: MaximumSystemManpower +40%
  - **`HeroSkill04BranchTerrans`** - Дух первенства / Pioneering Spirit  (tags: ColonizedStarSystem)
    - описание: Привнося дух первопроходцев в систему, куда он назначен, Герой вдохновляет население работать больше и дольше, увеличивая объемы промышленного производства. Этот эффект распространяется на пограничные миры.
    - ур.1 [мастерство Labour x1]: система: SystemProduction +10 @ColonizedStarSystem; SystemProduction Addition (10 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem; BonusHeroSystemIndustry +10 @ColonizedStarSystem (hidden); BonusHeroSystemIndustry Addition (10 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemProduction +20 @ColonizedStarSystem; SystemProduction Addition (20 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem; BonusHeroSystemIndustry +20 @ColonizedStarSystem (hidden); BonusHeroSystemIndustry Addition (10 Multiplication $(OwnedMeagerPlanets)) @ColonizedStarSystem (hidden)
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchTerrans`** - Военный стержень / Military Spine
    - описание: Обладая способностью определять наиболее способных командиров на флоте, Герой может делегировать тактические решения, увеличивая возможности наступления.
    - ур.1 [мастерство Command x1]: флот: ShipDamage Addition (0.015 Multiplication $(CommandPoints)) @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipDamage Addition (0.02 Multiplication $(CommandPoints)) @GarrisonFleet
  - **`HeroSkill06BranchTerrans`** - Метод старателя / Prospector's Touch  (tags: ColonizedStarSystem)
    - описание: Герой разведывает драгоценные минералы, словно ищейка, идущая по следу своей добычи. Во многом этому способствуют тщательный анализ геологии, климата и химии, но и сверхъестественное чувство Пыли, безусловно, играет не последнюю роль.
    - ур.1 [мастерство Labour x1]: система: DepositValue +2 @ColonizedStarSystem/Planet/ResourceDeposit,ResourceTypeStrategic
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchTerrans`** - Титан промышленности / Titan of Industry  (tags: ColonizedStarSystem)
    - описание: Герой - живая легенда. Он посещает промышленные объекты по всей империи и берется за любую работу: от мастера литейного цеха до директора по производству. После его отъезда квалификация и мотивация всех работников возрастает.
    - ур.1 [мастерство Labour x1]: система: SystemProduction Percent (0.02 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem | сенат: SystemProduction Percent (0.005 Multiplication $(AssignedHeroLevel)) @Empire/ColonizedStarSystem

### HeroSkillTreeHoratio - Навыки Горацио / Horatio Skills
Эта ветка содержит специфические навыки Горацио.

Героев с этим деревом: 5 (HeroHoratio01, HeroHoratio02, HeroHoratio03, HeroHoratio04, HeroQuestHoratio04)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchHoratio`** - Клонированные продукты / Clone Wares  (tags: ColonizedStarSystem)
    - описание: Герой Горацио поддерживает монокультурное сельское хозяйство и за счет единообразия повышает уровень Пищи и Одобрения.
    - ур.1 [мастерство Labour x1]: система: Happiness +5 @ColonizedStarSystem; SystemGrowth +5% @ColonizedStarSystem
    - ур.2 [мастерство Labour x2]: система: Happiness +10 @ColonizedStarSystem; SystemGrowth +10% @ColonizedStarSystem
  - **`HeroSkill02BranchHoratio`** - Любознательность Горацио / Horatic Curiosity
    - описание: Как наследник Горацио, Герой стремится развивать исследовательскую деятельность, направляя все излишки ресурсов флота на поддержание сканирующих систем и изучение космических струн.
    - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +1 @GarrisonFleet; VisionRange +20% @GarrisonFleet
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchHoratio`** - Счастливый случай / Lucky Windfall  (tags: ColonizedStarSystem)
    - описание: Организовав общесистемную лотерею с огромными выплатами, Герой получает колоссальные возможности для воздействия на общественное мнение.
    - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +10% @ColonizedStarSystem
    - ур.2 [мастерство Wit x1]: система: SystemEmpirePoint +20% @ColonizedStarSystem
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill04BranchHoratio`** - Плодородные сады / Fecund Gardens  (tags: ColonizedStarSystem)
    - описание: Пропагандируя социальную философию уверенности в собственных силах и доказывая это на личном примере, Герой воодушевляет население своей системы выращивать собственные овощи, что способствует увеличению объемов производства продуктов питания.
    - ур.1 [мастерство Labour x1]: система: PlanetRawFood +4 @ColonizedStarSystem/ColonizedPlanet; PlanetPopulationFood Addition (4 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; BonusHeroPlanetFood Addition (4 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetFood Addition (4 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - ур.2 [мастерство Labour x1]: система: PlanetRawFood +8 @ColonizedStarSystem/ColonizedPlanet; PlanetPopulationFood Addition (8 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; BonusHeroPlanetFood Addition (8 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetFood Addition (8 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
  - **`HeroSkill05BranchHoratio`** - Рынки тщеславия / Vanity Markets  (tags: ColonizedStarSystem)
    - описание: Герой Горацио создает общество, зацикленное на атрибутах богатства: предметах роскоши, ювелирных изделиях и деликатесах. Это искусственно повышает стоимость всех Ресурсов Роскоши.
    - ур.1 [мастерство Wit x1]: система: DepositValue +2 @ColonizedStarSystem/Planet/ResourceDeposit,ResourceTypeLuxury
    - ур.2 [мастерство Wit x1]: система: DepositValue +4 @ColonizedStarSystem/Planet/ResourceDeposit,ResourceTypeLuxury
  - **`HeroSkill06BranchHoratio`** - Защитный строй / Protective Formations
    - описание: Во время битвы Герой выстраивает флот таким образом, что корабли становятся менее восприимчивы к артиллерийскому огню.
    - ур.1 [мастерство Command x1]: флот: ShipShieldAbsorptionPercent +0.4 @GarrisonFleet
    - ур.2 [мастерство Command x1]: флот: ShipShieldAbsorptionPercent +0.8 @GarrisonFleet
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchHoratio`** - Вне конкуренции / Restricted Supplier  (tags: ColonizedStarSystem)
    - описание: Быстро захватывая контроль над Ресурсами Роскоши в Системе, Герой Горацио искусственно поднимает их цену выше настоящей рыночной стоимости.
    - ур.1 [мастерство Wit x1]: система: DepositValue +33% @ColonizedStarSystem/Planet/ResourceDeposit,ResourceTypeLuxury; PlanetRawIndustry +2 @ColonizedStarSystem,HappinessStatusStarSystem4/Planet | сенат: DepositValue +33% @Empire/ColonizedStarSystem/Planet/ResourceDeposit,ResourceTypeLuxury

### HeroSkillTreeVenetians - Навыки Люмерис / Lumeris skills
Эта ветка содержит специфические навыки Люмерис.

Героев с этим деревом: 5 (HeroVenetians01, HeroVenetians02, HeroVenetians03, HeroVenetians04, HeroVenetians05)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchVenetians`** - Бонус для Семейства Меос / Meos Family Favor  (tags: ColonizedStarSystem)
    - описание: Герой заключает сделку с кланом Меос, который в обмен на оказываемую Герою политическую поддержку получает доступ к производству Пыли в данной Системе.
    - ур.1 [мастерство Wit x1]: система: SystemMoney +5 @ColonizedStarSystem; SystemMoney Addition (5 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; BonusHeroSystemDust +5 @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (5 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemMoney +10 @ColonizedStarSystem; SystemMoney Addition (10 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; BonusHeroSystemDust +10 @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (10 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
  - **`HeroSkill02BranchVenetians`** - Промышленная деятельность клана Омокар / Omokar Industrial Operations
    - описание: Элитная команда из числа отколовшихся от клана Омокар инженеров присоединяется к флоту Героя, чтобы усовершенствовать характеристики корпуса и брони его судов.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.05 @GarrisonFleet; FleetMaximumShield +0.05 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: FleetMaximumHealth +0.1 @GarrisonFleet; FleetMaximumShield +0.1 @GarrisonFleet
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchVenetians`** - Счастливый случай / Lucky Windfall  (tags: ColonizedStarSystem)
    - описание: Организовав общесистемную лотерею с огромными выплатами, Герой получает колоссальные возможности для воздействия на общественное мнение.
    - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +10% @ColonizedStarSystem | сенат: TradingRouteIncomeModifier +10% @Empire/ColonizedStarSystem
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill04BranchVenetians`** - Личные связи / Personal Networking  (tags: ColonizedStarSystem)
    - описание: Герой настойчиво трудится для обеспечения полной занятости населения в своей Системе, используя финансы клана Меос и предприятия клана Омокар, чтобы снизить процент безработицы почти до нуля. Обеспеченное работой население создает в Системе промышленный бум.
    - ур.1 [мастерство Labour x1]: система: PlanetRawIndustry +3 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetIndustry Addition (3 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - ур.2 [мастерство Labour x2]: система: PlanetRawIndustry +6 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetIndustry Addition (6 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden)
  - **`HeroSkill05BranchVenetians`** - Реклама туристического направления / Destination Marketing  (tags: ColonizedStarSystem)
    - описание: Герой работает над позиционированием выбранной Системы в качестве центра власти и перспективного для люмерийских Семейств туристического направления. Подкупленные ученые из клана Арракьо распространяют научные доказательства в пользу этого, и рост инвестиций и иммиграции стимулирует влияние расы Люмерис в данной Системе.
    - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +20 @ColonizedStarSystem; SystemMoney Addition (40 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; BonusHeroSystemPrestige +20 @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (40 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemEmpirePoint +40 @ColonizedStarSystem; SystemMoney Addition (80 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem; BonusHeroSystemPrestige +40 @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (80 Multiplication $(OwnedHotPlanets)) @ColonizedStarSystem (hidden)
  - **`HeroSkill06BranchVenetians`** - Черный рынок на флоте / Fleet Black Market
    - описание: Позволяя развиваться на подвластных ему кораблях хорошо контролируемому черному рынку, Герой достигает одновременно ускоренной реконструкции зондов и сокращения расходов на техобслуживание.
    - ур.1 [мастерство Curiosity x1]: флот: NetShipProbe +0.5 @GarrisonFleet/Ship,ShipRoleExploration; GarrisonMoneyUpkeep -20% @GarrisonFleet
    - ур.2 [мастерство Curiosity x2]: флот: NetShipProbe +0.5 @GarrisonFleet/Ship,ShipRoleExploration; GarrisonMoneyUpkeep -40% @GarrisonFleet
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchVenetians`** - Люмерийская монополия / Lumerian Monopoly  (tags: ColonizedStarSystem)
    - описание: Герой поднимается на высший уровень в иерархии своего клана, добиваясь полного контроля над импортом и экспортом товаров в данной системе. Выгодное местечко.
    - ур.1 [мастерство Wit x1]: система: TradingRoutesBonus_Growth +40% @ColonizedStarSystem; TradingRoutesBonus_Production +40% @ColonizedStarSystem

### HeroSkillTreeVampirilis - Навыки Водяни / Vodyani Skills
Эта ветка содержит специфические навыки Водяни.

Героев с этим деревом: 4 (HeroVampirilis01, HeroVampirilis02, HeroVampirilis03, HeroVampirilis04)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchVampirilis`** - Пища аскета / Ascetic Sustenance  (tags: ColonizedStarSystem)
    - описание: Герой начинает питаться простой и скромной пищей, подавая пример и вдохновляя свое окружение делать то же самое. Сельское хозяйство сосредотачивается на производстве легко выращиваемой продукций, что приводит к повышению урожайности.
    - ур.1 [мастерство Labour x1]: система: SystemGrowth +10 @ColonizedStarSystem; SystemGrowth Addition (1 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemFood +10 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (1 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemGrowth +20 @ColonizedStarSystem; SystemGrowth Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemFood +20 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
  - **`HeroSkill02BranchVampirilis`** - Проекция брони / Mantle Projection
    - описание: Благодаря мастерству субстанции, Герой может проецировать часть своего личного Пылевого поля в щитовые системы корабля, повышая его защиту.
    - ур.1 [мастерство Command x1]: флот: MaximumShield +20% @Ship
    - ур.2 [мастерство Command x2]: флот: MaximumShield +40% @Ship
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchVampirilis`** - Скопление пыли / Inundation of Dust  (tags: ColonizedStarSystem)
    - описание: Тщательно изучив писания Вечных и проведя всевозможные эксперименты, Герой многое узнает о сложностях добычи Пыли и получает возможность значительно повысить объемы получаемой магической субстанции в Системе, в которую он назначен.
    - ур.1 [мастерство Wit x1]: система: SystemMoney +10% @ColonizedStarSystem
    - ур.2 [мастерство Wit x2]: система: SystemMoney +20% @ColonizedStarSystem
  - **`HeroSkill04BranchVampirilis`** - Жгучий свет / The Burning Light
    - описание: Тщательно проанализировав критические энергетические потребности флота, Герой может перенастроить системы энергетического вооружения таким образом, чтобы они производили больше мощности, тем самым увеличивая урон от них.
    - ур.1 [мастерство Command x1]: флот: ShipDamageEnergy +0.2 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipDamageEnergy +0.4 @GarrisonFleet
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchVampirilis`** - Небесная механика / Celestial Mechanics
    - описание: Используя свое "ощущение" влияния Пыли на звездные струны, Герой лучше может направлять свой флот по путям наименьшего сопротивления, увеличивая дальность его полета.
    - ур.1 [мастерство Curiosity x1]: флот: ShipMaximumMovement +6 @GarrisonFleet
  - **`HeroSkill06BranchVampirilis`** - Непогрешимый авторитет / Infallible Authority  (tags: ColonizedStarSystem)
    - описание: Являясь проводником воли Виртуалов, имея сильный характер и благодаря непоколебимой вере, Герой Водяни способен объединить всю империю крайне эмоциональными речами.
    - ур.1 [мастерство Labour x1]: система: PlanetRawFIDSI +2 @ColonizedStarSystem/ColonizedPlanet; BonusHeroSystemFood Addition (2 Multiplication $(Population)) @ColonizedStarSystem (hidden); BonusHeroSystemIndustry Addition (2 Multiplication $(Population)) @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (2 Multiplication $(Population)) @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (2 Multiplication $(Population)) @ColonizedStarSystem (hidden); BonusHeroSystemPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem (hidden) | сенат: PlanetRawFIDSI +0.5 @Empire/ColonizedStarSystem/ColonizedPlanet
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchVampirilis`** - Праведность посвященных / Righteousness of the Ordained
    - описание: Во времена битвы, преисполненный праведной верой, герой Водяни наполняет свое ближайшее окружение такой же верой, значительно повышая мощь и силу своего личного корабля.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.25 @GarrisonFleet; Damage +0.8 @Ship

### HeroSkillTreeTimeLords - Навыки Властелинов Времени / Riftborn Skills
Эта ветка содержит навыки Властелинов Времени

Героев с этим деревом: 4 (HeroTimeLords01, HeroTimeLords02, HeroTimeLords03, HeroTimeLords04)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchTimeLords`** - Скопление пыли / Inundation of Dust  (tags: ColonizedStarSystem)
    - описание: Тщательно изучив писания Вечных и проведя всевозможные эксперименты, Герой многое узнает о сложностях добычи Пыли и получает возможность значительно повысить объемы получаемой магической субстанции в Системе, в которую он назначен.
    - ур.1 [мастерство Wit x1]: система: SystemMoney +5% @ColonizedStarSystem
    - ур.2 [мастерство Wit x2]: система: SystemMoney +10% @ColonizedStarSystem
  - **`HeroSkill02BranchTimeLords`** - Гений механики / Mechanical Genius  (tags: ColonizedStarSystem)
    - описание: Благодаря тому, что промышленные базы становятся все более автоматизированными, понимание Героем машин может обеспечить увеличение объемов производства по всей Системе.
    - ур.1 [мастерство Labour x1]: система: SystemProduction +15 @ColonizedStarSystem; BonusHeroSystemIndustry +15 @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemProduction +30 @ColonizedStarSystem; BonusHeroSystemIndustry +30 @ColonizedStarSystem (hidden)
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchTimeLords`** - Тайные знания / Arcane Knowledge  (tags: ColonizedStarSystem)
    - описание: Общаясь с эхом Пыли, найденными на планетах, Герой может погрузиться в историю и получить утраченные знания.
    - ур.1 [мастерство Wit x1]: система: SystemResearch +20 @ColonizedStarSystem; SystemResearch Addition (20 Multiplication $(AnomalyCount)) @ColonizedStarSystem; BonusHeroSystemScience +20 @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (20 Multiplication $(AnomalyCount)) @ColonizedStarSystem (hidden)
  - **`HeroSkill04BranchTimeLords`** - Интеграция оборонных ИИ-ресурсов / Defensive AI Integration
    - описание: Специализированная система Искусственного Интеллекта, разрабатываемая группой волонтеров под руководством Героя с целью оптимизации тактических компьютерных расчетов. Ее высокоскоростные характеристики позволяют повысить оборонительный потенциал космофлота, увеличивая коэффициент Выживаемости в бою.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumShield +0.1 @GarrisonFleet
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchTimeLords`** - Академическое рецензирование / Peer Review  (tags: ColonizedStarSystem)
    - описание: Герой проявляет интерес к научным исследованиям в подвластной ему системе, внося свой вклад в обеспечение должного уровня функционирования научно-образовательной системы.
    - ур.1 [мастерство Wit x1]: система: SystemResearch +40 @ColonizedStarSystem; SystemResearch Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemScience +40 @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemResearch +80 @ColonizedStarSystem; SystemResearch Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemScience +80 @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
  - **`HeroSkill06BranchTimeLords`** - На пределе / Red Zone Overkill
    - описание: Управляя вооружением, Герой увеличивает его максимальную мощность тем самым увеличивая пробивную способность. Для ремонта и замены использованных запчастей после боя потребуется дополнительное время, но экипажи относятся к этому спокойно, учитывая их повышенную эффективность.
    - ур.1 [мастерство Command x1]: флот: ShipShieldPenetrationFactor +0.4 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipShieldPenetrationFactor +0.8 @GarrisonFleet
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchTimeLords`** - Технические инкубаторы / Tech Hubs  (tags: ColonizedStarSystem)
    - описание: Научные инкубаторы, размещенные вблизи аномалий, дают толчок развитию творческого машиностроения и повышают объемы промышленного производства.
    - ур.1 [мастерство Labour x1]: система: SystemProduction Addition ($(AnomalyCount) Multiplication 50) @ColonizedStarSystem; BonusHeroSystemIndustry Addition ($(AnomalyCount) Multiplication 50) @ColonizedStarSystem (hidden) | сенат: SystemProduction Addition ($(AnomalyCount) Multiplication 10) @Empire/ColonizedStarSystem; BonusHeroSystemIndustry Addition ($(AnomalyCount) Multiplication 10) @Empire/ColonizedStarSystem (hidden)

### HeroSkillTreeUnfallen - 

Героев с этим деревом: 4 (HeroUnfallen01, HeroUnfallen02, HeroUnfallen03, HeroUnfallen04)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchUnfallen`** 
    - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth Addition (0.005 Multiplication $(CommandPoints)) @GarrisonFleet
  - **`HeroSkill02BranchUnfallen`**   (tags: ColonizedStarSystem)
    - ур.1 [мастерство Labour x1]: система: SystemGrowth +7% @ColonizedStarSystem
    - ур.2 [мастерство Labour x2]: система: SystemGrowth +15% @ColonizedStarSystem
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchUnfallen`** 
    - ур.1 [мастерство Command x1]: флот: ShipShieldAbsorptionPercent +0.2 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipShieldAbsorptionPercent +0.4 @GarrisonFleet
  - **`HeroSkill04BranchUnfallen`**   (tags: ColonizedStarSystem)
    - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +10 @ColonizedStarSystem; SystemEmpirePoint Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemPrestige +10 @ColonizedStarSystem (hidden); BonusHeroSystemPrestige Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemEmpirePoint +20 @ColonizedStarSystem; SystemEmpirePoint Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemPrestige +20 @ColonizedStarSystem (hidden); BonusHeroSystemPrestige Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchUnfallen`**   (tags: ColonizedStarSystem)
    - ур.1 [мастерство Wit x1]: система: PlanetRawPrestige +2 @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet; PlanetRawPrestige +2 @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden); PlanetRawPrestige +2 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet (hidden); BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden)
    - ур.2 [мастерство Wit x2]: система: PlanetRawPrestige +4 @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet; PlanetRawPrestige +4 @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden); PlanetRawPrestige +4 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetPrestige Addition (4 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetPrestige Addition (4 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet (hidden); BonusHeroPlanetPrestige Addition (4 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden)
  - **`HeroSkill06BranchUnfallen`**   (tags: ColonizedStarSystem)
    - ур.1 [мастерство Labour x1]: система: PlanetRawFood +4 @ColonizedStarSystem/ColonizedPlanet; PlanetPopulationFood Addition (4 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; BonusHeroPlanetFood Addition (4 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetFood Addition (4 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - ур.2 [мастерство Labour x2]: система: PlanetRawFood +8 @ColonizedStarSystem/ColonizedPlanet; PlanetPopulationFood Addition (8 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; BonusHeroPlanetFood Addition (8 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetFood Addition (8 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchUnfallen`** 
    - ур.1 [мастерство Labour x1]: флот: HealthRegenerationRatio +0.4 @GarrisonFleet | сенат: HealthRegenerationRatio +0.05 @Empire/GarrisonFleet

### HeroSkillTreeMinorFaction01 - Навыки Добытчиков / Scavenger skills
Эта ветка содержит специфические навыки малых цивилизаций Добытчиков.

Героев с этим деревом: 6 (HeroHaroshems01, HeroHisshos01, HeroEyders01, HeroPulsos01, HeroQuestVirus01, HeroPirate01)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchMinorFaction01`** - Грязная работа / Common Touch  (tags: ColonizedStarSystem)
    - описание: Герой не стыдится замарать руки даже на самой грязной работе, поэтому им восхищается большая часть населения Системы, которой он управляет. В результате поддержка населения значительно увеличивается.
    - ур.1 [мастерство Wit x1]: система: SystemEmpirePoint +5 @ColonizedStarSystem; SystemEmpirePoint Addition (1 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemPrestige +5 @ColonizedStarSystem (hidden); BonusHeroSystemPrestige Addition (1 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemEmpirePoint +10 @ColonizedStarSystem; SystemEmpirePoint Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemPrestige +10 @ColonizedStarSystem (hidden); BonusHeroSystemPrestige Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill02BranchMinorFaction01`** - Черный рынок на флоте / Fleet Black Market
    - описание: Позволяя развиваться на подвластных ему кораблях хорошо контролируемому черному рынку, Герой достигает одновременно ускоренной реконструкции зондов и сокращения расходов на техобслуживание.
    - ур.1 [мастерство Curiosity x1]: флот: NetShipProbe +0.5 @GarrisonFleet/Ship,ShipRoleExploration
    - ур.2 [мастерство Curiosity x2]: флот: NetShipProbe +0.5 @GarrisonFleet/Ship,ShipRoleExploration; GarrisonMoneyUpkeep -20% @GarrisonFleet
  - **`HeroSkill03BranchMinorFaction01`** - Запах крови / Blood Scent
    - описание: Опыт, полученный в сотне битв, наделяет Героя мгновенной реакцией, основанной на интуитивной способности находить слабые места вражеского флота, что усиливает поражающую мощь его кораблей.
    - ур.1 [мастерство Command x1]: флот: ShipHullPlatingPenetrationFactor +0.2 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipHullPlatingPenetrationFactor +0.4 @GarrisonFleet
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill04BranchMinorFaction01`** - Фокусы с пахучими удобрениями / Steaming Fertilizer Tricks  (tags: ColonizedStarSystem)
    - описание: Большое количество набегов на заболоченные планеты с чрезвычайно простыми методами ведения сельского хозяйства дало Герою богатое, даже ароматное, понимание того, как получить максимальную пользу от навоза копытных животных. Распространение этого ноу-хау по всей Системе Героя может существенно повысить урожайность даже на самых современных сельскохозяйственных планетах.
    - ур.1 [мастерство Labour x1]: система: SystemGrowth +30% @ColonizedStarSystem
    - ур.2 [мастерство Labour x2]: система: SystemGrowth +60% @ColonizedStarSystem
  - **`HeroSkill05BranchMinorFaction01`** - Мусорная экономика / Scrapyard Economics
    - описание: Герой хорошо разбирается в переработке отходов для снижения затрат на строительные проекты. С помощью этого навыка Герой создает сеть гигантских скоординированных складов в масштабе империи, что помогает сэкономить при производстве кораблей по всей империи.
    - ур.1 [мастерство Labour x1]: система: ShipProductionCostReduction -0.1 | сенат: ShipProductionCostReduction -0.05 @Empire/ColonizedStarSystem
  - **`HeroSkill06BranchMinorFaction01`** - Лоскутная защита / Patchwork Defenses
    - описание: Опыт сотен боев - от стычек в астероидных шахтах до масштабных сражений флотов - позволил Герою понять, как получить преимущество в обороне. Перераспределение пластин корпуса и усиление кораблей в тех местах, куда скорее всего будет нанесен удар, и ослабление брони в местах, куда попадание маловероятно, позволяет Герою улучшить оборону.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth +0.1 @GarrisonFleet; FleetHullPlatingAbsorption +40 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: FleetMaximumHealth +0.2 @GarrisonFleet; FleetHullPlatingAbsorption +80 @GarrisonFleet
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchMinorFaction01`** - Мастер-отступник / Renegade Master  (tags: ColonizedStarSystem)
    - описание: Изучение загадочных культурных традиций малых цивилизаций, таких как ритуалы общения с духами Эпистиса, делает Героя легендой в глазах меньшинств, населяющих Систему.
    - ур.1 [мастерство Wit x1]: система: Happiness Addition (5 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem; SystemGrowth Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem; SystemProduction Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem; SystemMoney Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem; SystemResearch Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem; BonusHeroSystemFood Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem (hidden); BonusHeroSystemIndustry Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem (hidden); BonusHeroSystemDust Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (2 Multiplication $(NonMajorPopulationCount)) @ColonizedStarSystem (hidden)

### HeroSkillTreeMinorFaction02 - Эусоциальные навыки / Eusocial skills
Эта ветвь содержит навыки социальных малых цивилизаций.

Героев с этим деревом: 5 (HeroAmoeba01, HeroDeuyivans01, HeroTikanans01, HeroRemnant01, HeroAmoeba02)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchMinorFaction02`** - Разум роя / Hive Mind  (tags: ColonizedStarSystem)
    - описание: Являясь частью суперорганизма, Герой тонко чувствует потребности и желания населения и легко получает общественную поддержку.
    - ур.1 [мастерство Wit x1]: система: Happiness Addition (1 Multiplication $(Population)) @ColonizedStarSystem
    - ур.2 [мастерство Wit x2]: система: Happiness Addition (2 Multiplication $(Population)) @ColonizedStarSystem
  - **`HeroSkill02BranchMinorFaction02`** - Укрепленный панцирь / Hardened Carapace
    - описание: Полагаясь на рассредоточенную иерархию на флоте, Герой может позволить экипажам быть более автономными, что повышает общую обороноспособность судов.
    - ур.1 [мастерство Command x1]: флот: FleetHullPlatingAbsorption +30 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: FleetHullPlatingAbsorption +60 @GarrisonFleet
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchMinorFaction02`** - Принудительный труд / Forced Labor  (tags: ColonizedStarSystem)
    - описание: Герой отдает приказ о захвате и содержании в неволе представителей других рас, которых силой принуждают работать на заводах и фермах. Их отпустят и вознаградят за тяжкий труд… но не прежде, чем из планет, на которых их эксплуатируют, будут дочиста выкачаны все ресурсы...
    - ур.1 [мастерство Labour x1]: система: SystemProduction +10 @ColonizedStarSystem; SystemGrowth Addition (20 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; BonusHeroSystemIndustry +10 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (20 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemProduction +20 @ColonizedStarSystem; SystemGrowth Addition (40 Multiplication $(OwnedPlanets)) @ColonizedStarSystem; BonusHeroSystemIndustry +20 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (40 Multiplication $(OwnedPlanets)) @ColonizedStarSystem (hidden)
  - **`HeroSkill04BranchMinorFaction02`** - Нулевая толерантность / Zero Tolerance  (tags: ColonizedStarSystem)
    - описание: Для эусоциального вида здоровье всего суперорганизма имеет первостепенное значение. Слабые особи отвергаются. Таким образом, все сухопутные войска Героя находятся в идеальной форме.
    - ур.1 [мастерство Labour x1]: система: GroundBattleDefenderTroopsMaxHealthMultiplier Addition (0.01 Multiplication $(Population)) @ColonizedStarSystem
    - ур.2 [мастерство Labour x2]: система: GroundBattleDefenderTroopsMaxHealthMultiplier Addition (0.02 Multiplication $(Population)) @ColonizedStarSystem
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchMinorFaction02`** - Воодушевляющий оратор / Rousing Orator  (tags: ColonizedStarSystem)
    - описание: Герой может формировать и использовать общественное мнение, с легкостью воодушевляя людей во время массовых митингов с помощью провокационных лозунгов и эмоциональных аргументов.
    - ур.1 [мастерство Wit x1]: система: PlanetRawPrestige +2 @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet; PlanetRawPrestige +2 @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden); PlanetRawPrestige +2 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem3/ColonizedPlanet (hidden); BonusHeroPlanetPrestige Addition (2 Multiplication $(Population)) @ColonizedStarSystem,HappinessStatusStarSystem4/ColonizedPlanet (hidden) | сенат: ElectionActionCost -0.5 @Empire/Senate
  - **`HeroSkill06BranchMinorFaction02`** - Образцовый хозяин / Ultra Houseproud  (tags: ColonizedStarSystem)
    - описание: Герой вдохновляет население гордиться общественными местами и сооружениями. Это приводит к снижению стоимости улучшений.
    - ур.1 [мастерство Labour x1]: система: SystemMoneyUpkeep -40% @ColonizedStarSystem
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchMinorFaction02`** - Легендарный авторитет / Legendary Authority
    - описание: Герой управляет находящимися под его командованием флотами настолько идеально, что любое несовершенство непростительно. Здоровье флота значительно увеличивается.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumHealth Addition (0.02 Multiplication $(CommandPoints)) @GarrisonFleet

### HeroSkillTreeMinorFaction03 - Технологические навыки / Technologist skills
Эта ветка содержит специфические навыки малых Технологических цивилизаций.

Героев с этим деревом: 4 (HeroBots01, HeroMavros01, HeroPilgrims01, HeroZvali01)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchMinorFaction03`** - Запах крови / Blood Scent
    - описание: Опыт, полученный в сотне битв, наделяет Героя мгновенной реакцией, основанной на интуитивной способности находить слабые места вражеского флота, что усиливает поражающую мощь его кораблей.
    - ур.1 [мастерство Command x1]: флот: ShipHullPlatingPenetrationFactor +0.1 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipHullPlatingPenetrationFactor +0.2 @GarrisonFleet
  - **`HeroSkill02BranchMinorFaction03`** - Мощные сканеры / Overclocked Scanners
    - описание: Обладая знаниями о том, как улучшить разрешение снимков дальнего космоса, Герой может увеличить диапазон обзора назначенного ему флота.
    - ур.1 [мастерство Curiosity x1]: флот: VisionRange +3 @GarrisonFleet
    - ур.2 [мастерство Curiosity x2]: флот: VisionRange +6 @GarrisonFleet
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchMinorFaction03`** - Проекция брони / Mantle Projection
    - описание: Благодаря мастерству субстанции, Герой может проецировать часть своего личного Пылевого поля в щитовые системы корабля, повышая его защиту.
    - ур.1 [мастерство Command x1]: флот: MaximumShield +40% @Ship
    - ур.2 [мастерство Command x2]: флот: MaximumShield +80% @Ship
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill04BranchMinorFaction03`** - Интеграция оборонных ИИ-ресурсов / Defensive AI Integration
    - описание: Специализированная система Искусственного Интеллекта, разрабатываемая группой волонтеров под руководством Героя с целью оптимизации тактических компьютерных расчетов. Ее высокоскоростные характеристики позволяют повысить оборонительный потенциал космофлота, увеличивая коэффициент Выживаемости в бою.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumShield +0.25 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: FleetMaximumShield +0.5 @GarrisonFleet
  - **`HeroSkill05BranchMinorFaction03`** - Гениократ / Geniocrat  (tags: ColonizedStarSystem)
    - описание: Герой принимает меры против стагнации в политической и академической сферах, привнося определенную долю своей организаторской логики в функционирование данной системы, отчего показатель Науки буквально взлетает.
    - ур.1 [мастерство Wit x1]: система: SystemResearch +30% @ColonizedStarSystem
  - **`HeroSkill06BranchMinorFaction03`** - Руководство экспедициями / Expedition Management  (tags: ColonizedStarSystem)
    - описание: Герой лично руководит экспедициями в пределах Системы, применяя свой искусственно повышенный интеллект для обнаружения хорошо сохранившихся развалин на холодных планетах.
    - ур.1 [мастерство Wit x1]: система: SystemResearch +40 @ColonizedStarSystem; SystemResearch Addition (40 Multiplication $(OwnedColdPlanets)) @ColonizedStarSystem; BonusHeroSystemScience +40 @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (40 Multiplication $(OwnedColdPlanets)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Wit x2]: система: SystemResearch +80 @ColonizedStarSystem; SystemResearch Addition (80 Multiplication $(OwnedColdPlanets)) @ColonizedStarSystem; BonusHeroSystemScience +80 @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (80 Multiplication $(OwnedColdPlanets)) @ColonizedStarSystem (hidden)
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchMinorFaction03`** - На пределе / Red Zone Overkill
    - описание: Управляя вооружением, Герой увеличивает его максимальную мощность тем самым увеличивая пробивную способность. Для ремонта и замены использованных запчастей после боя потребуется дополнительное время, но экипажи относятся к этому спокойно, учитывая их повышенную эффективность.
    - ур.1 [мастерство Command x1]: флот: ShipShieldPenetrationFactor +0.8 @GarrisonFleet | сенат: ShipShieldPenetrationFactor +0.1 @Empire/GarrisonFleet

### HeroSkillTreeMinorFaction04 - Первобытные Навыки / Primitive skills
Эта ветка содержит специфические навыки малых первобытных цивилизаций.

Героев с этим деревом: 6 (HeroBenthys01, HeroGnashasts01, HeroNiris01, HeroBhagabas01, HeroGreenman01, HeroGreenman02)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchMinorFaction04`** - Плодородные сады / Fecund Gardens  (tags: ColonizedStarSystem)
    - описание: Пропагандируя социальную философию уверенности в собственных силах и доказывая это на личном примере, Герой воодушевляет население своей системы выращивать собственные овощи, что способствует увеличению объемов производства продуктов питания.
    - ур.1 [мастерство Labour x1]: система: PlanetRawFood +1 @ColonizedStarSystem/ColonizedPlanet; PlanetPopulationFood Addition (1 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; BonusHeroPlanetFood Addition (1 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetFood Addition (1 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
    - ур.2 [мастерство Labour x2]: система: PlanetRawFood +2 @ColonizedStarSystem/ColonizedPlanet; PlanetPopulationFood Addition (2 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/Planet; BonusHeroPlanetFood Addition (2 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden); BonusHeroPlanetFood Addition (2 Multiplication $(MajorPopulationCount)) @ColonizedStarSystem/ColonizedPlanet (hidden)
  - **`HeroSkill02BranchMinorFaction04`** - Военный стержень / Military Spine
    - описание: Обладая способностью определять наиболее способных командиров на флоте, Герой может делегировать тактические решения, увеличивая возможности наступления.
    - ур.1 [мастерство Command x1]: флот: ShipDamage Addition (0.005 Multiplication $(CommandPoints)) @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipDamage Addition (0.01 Multiplication $(CommandPoints)) @GarrisonFleet
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchMinorFaction04`** - Пища аскета / Ascetic Sustenance  (tags: ColonizedStarSystem)
    - описание: Герой начинает питаться простой и скромной пищей, подавая пример и вдохновляя свое окружение делать то же самое. Сельское хозяйство сосредотачивается на производстве легко выращиваемой продукций, что приводит к повышению урожайности.
    - ур.1 [мастерство Labour x1]: система: SystemGrowth +20 @ColonizedStarSystem; SystemGrowth Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemFood +20 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (2 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemGrowth +40 @ColonizedStarSystem; SystemGrowth Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem; BonusHeroSystemFood +40 @ColonizedStarSystem (hidden); BonusHeroSystemFood Addition (4 Multiplication $(AssignedHeroLevel)) @ColonizedStarSystem (hidden)
  - **`HeroSkill04BranchMinorFaction04`** - Гений механики / Mechanical Genius  (tags: ColonizedStarSystem)
    - описание: Благодаря тому, что промышленные базы становятся все более автоматизированными, понимание Героем машин может обеспечить увеличение объемов производства по всей Системе.
    - ур.1 [мастерство Labour x1]: система: SystemProduction +30 @ColonizedStarSystem; BonusHeroSystemIndustry +30 @ColonizedStarSystem (hidden)
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchMinorFaction04`** - Кровь и земля / Blood and Land  (tags: ColonizedStarSystem)
    - описание: Как истинный патриот, Герой пропагандирует представление о том, что личность каждого человека глубоко связана с его родиной, тем самым значительно повышая количество призывников.
    - ур.1 [мастерство Labour x1]: система: MaximumSystemManpower +40%
    - ур.2 [мастерство Labour x2]: система: MaximumSystemManpower +80%
  - **`HeroSkill06BranchMinorFaction04`** - Авторитарное лидерство / Authoritarian Leadership  (tags: ColonizedStarSystem)
    - описание: Тотальное господство воли Героя обусловливает повышенную отдачу и энтузиазм по всей социальной вертикали сверху донизу. Возрастающий энтузиазм ведет к увеличению трудовых ресурсов на строительстве флота, а сказания о деяниях Героя усиливают общественную поддержку финансирования военных программ.
    - ур.1 [мастерство Command x1]: флот: MaximumShipManpower +25% @GarrisonFleet/Ship; SiegePower +10% @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: MaximumShipManpower +50% @GarrisonFleet/Ship; SiegePower +20% @GarrisonFleet
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill07BranchMinorFaction04`** - Личные связи / Personal Networking  (tags: ColonizedStarSystem)
    - описание: Герой настойчиво трудится для обеспечения полной занятости населения в своей Системе, используя финансы клана Меос и предприятия клана Омокар, чтобы снизить процент безработицы почти до нуля. Обеспеченное работой население создает в Системе промышленный бум.
    - ур.1 [мастерство Labour x1]: система: PlanetRawIndustry +6 @ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetIndustry Addition (6 Multiplication $(Population)) @ColonizedStarSystem/ColonizedPlanet (hidden) | сенат: PlanetRawIndustry +1 @Empire/ColonizedStarSystem/ColonizedPlanet; BonusHeroPlanetIndustry Addition (1 Multiplication $(Population)) @Empire/ColonizedStarSystem/ColonizedPlanet (hidden)

### HeroSkillTreeEndless - Навыки Вечных / Endless Skills
Эта ветка содержит специфические навыки Вечных.

Героев с этим деревом: 2 (QuestHeroEndless01, QuestHeroEndless02)

- **Этап, порог 0 очк.** (в моде: A/G 0, S 0, D 0)
  - **`HeroSkill01BranchEndless`** - Тайные знания / Arcane Knowledge  (tags: ColonizedStarSystem)
    - описание: Общаясь с эхом Пыли, найденными на планетах, Герой может погрузиться в историю и получить утраченные знания.
    - ур.1 [мастерство Labour x1]: система: SystemResearch +10 @ColonizedStarSystem; SystemResearch Addition (10 Multiplication $(AnomalyCount)) @ColonizedStarSystem; BonusHeroSystemScience +10 @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (10 Multiplication $(AnomalyCount)) @ColonizedStarSystem (hidden)
    - ур.2 [мастерство Labour x2]: система: SystemResearch +20 @ColonizedStarSystem; SystemResearch Addition (20 Multiplication $(AnomalyCount)) @ColonizedStarSystem; BonusHeroSystemScience +20 @ColonizedStarSystem (hidden); BonusHeroSystemScience Addition (20 Multiplication $(AnomalyCount)) @ColonizedStarSystem (hidden)
  - **`HeroSkill02BranchEndless`** - Галактическое чутье / Galactic Sense
    - описание: Чувствуя Пыль, плывущую в пространстве Галактики, Герой способен "видеть" далеко за пределами возможностей физических устройств, увеличивая дальность обзора и полета своего флота.
    - ур.1 [мастерство Command x1]: флот: ShipMaximumMovement +1 @GarrisonFleet; VisionRange +1 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipMaximumMovement +2 @GarrisonFleet; VisionRange +2 @GarrisonFleet
- **Этап, порог 4 очк.** (в моде: A/G 4, S 2, D 6)
  - **`HeroSkill03BranchEndless`** - Смажьте колеса / Grease the Wheels
    - описание: Бюджетные навыки обычно в большом почете на любом космофлоте. Искусное манипулирование финансовыми ресурсами позволяет Герою обеспечивать улучшенное снабжение и техобслуживание двигателей искривленного пространства и щитовых систем.
    - ур.1 [мастерство Command x1]: флот: GarrisonMoneyUpkeep -20% @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: GarrisonMoneyUpkeep -40% @GarrisonFleet
  - **`HeroSkill04BranchEndless`** - Интеграция оборонных ИИ-ресурсов / Defensive AI Integration
    - описание: Специализированная система Искусственного Интеллекта, разрабатываемая группой волонтеров под руководством Героя с целью оптимизации тактических компьютерных расчетов. Ее высокоскоростные характеристики позволяют повысить оборонительный потенциал космофлота, увеличивая коэффициент Выживаемости в бою.
    - ур.1 [мастерство Command x1]: флот: FleetMaximumShield +0.1 @GarrisonFleet
- **Этап, порог 8 очк.** (в моде: A/G 8, S 4, D 12)
  - **`HeroSkill05BranchEndless`** - Хронозаряды / Chrono Ordnance
    - описание: То ли эта способность связана с многомерной физикой, то ли это мистический навык управления Пылью, но герой может заставить орудия флота проскочить вперед на пару единиц Планка. Это помогает обойти защиту вражеских щитов.
    - ур.1 [мастерство Command x1]: флот: ShipShieldPenetrationFactor +0.4 @GarrisonFleet
    - ур.2 [мастерство Command x2]: флот: ShipShieldPenetrationFactor +0.8 @GarrisonFleet
- **Этап, порог 12 очк.** (в моде: A/G 12, S 6, D 18)
  - **`HeroSkill06BranchEndless`** - Бесконечная харизма / Endless Charisma  (tags: ColonizedStarSystem)
    - описание: Так как Герой - это древнее существо, рожденное в другую эпоху, он пользуется глубоким, неизменным уважением всех, кто ему служит. Бойцы с удовольствием переносят трудности, чтобы приблизиться к такой внушающий благоговейный ужас фигуре.
    - ур.1 [мастерство Command x1]: флот: MaximumShipManpower +40% @GarrisonFleet/Ship; FleetManpowerRefillRateModifier +0.4 @GarrisonFleet

