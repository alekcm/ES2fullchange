// Galactic Council of the Academy - law registry (pure C#, no game dependencies).
using System;
using System.Collections.Generic;

public sealed class CouncilLawDef
{
    public const int KindCycle = 0;      // lasts until the next vote opens
    public const int KindInstant = 1;    // executed once when enacted
    public const int KindPermanent = 2;  // permanent stat change (while in the council)
    public const int KindMandate = 3;    // "M:<id>" question: elects the holder of a mandate (lasts until re-election)

    public string Id;
    public bool Targeted;       // true = "against/for one empire", false = whole council yes/no
    public int Kind;
    public int Benefit;         // +1 helps the target / the voters, -1 hurts the target (used by AI voting)
    public string Group;        // laws of one group are never offered in the same vote
    public bool Implemented;    // effect exists in this build; only implemented laws are offered
    public string Ru, En, RuDesc, EnDesc;

    public CouncilLawDef(string id, bool targeted, int kind, int benefit, string group, bool implemented,
        string ru, string en, string ruDesc, string enDesc)
    {
        Id = id; Targeted = targeted; Kind = kind; Benefit = benefit; Group = group; Implemented = implemented;
        Ru = ru; En = en; RuDesc = ruDesc; EnDesc = enDesc;
    }
}

public sealed class CouncilMandateDef
{
    public string Id;
    public string[] Categories;   // any of these task categories counts
    public int Need;              // number of DIFFERENT tasks needed
    public bool Victory;          // the supreme (victory) mandate: special requirements
    public string Ru, En, RuDesc, EnDesc, RuNeed, EnNeed;

    public CouncilMandateDef(string id, string[] cats, int need, bool victory, string ru, string en, string ruDesc, string enDesc, string ruNeed, string enNeed)
    {
        Id = id; Categories = cats; Need = need; Victory = victory;
        Ru = ru; En = en; RuDesc = ruDesc; EnDesc = enDesc; RuNeed = ruNeed; EnNeed = enNeed;
    }
}

public static class CouncilLaws
{
    public static readonly List<CouncilMandateDef> Mandates = BuildMandates();   // must be initialised before All
    public static readonly List<CouncilLawDef> All = Build();

    // Mandate ids (stable: stored in saves)
    public const string Chairman = "Chairman";
    public const string Marshal = "Marshal";
    public const string Quartermaster = "Quartermaster";
    public const string Eye = "Eye";
    public const string Supreme = "Supreme";
    public const string MandatePrefix = "M:";

    // task categories (the Academy assignment system is a stub for now: see CouncilGame.ReportTask)
    // Qualification achievements (the game layer reports them once the condition is seen in the game state)
    public const string CatCapital = "capital";        // Marshal: captured the capital of another player
    public const string CatSystemLevel = "syslevel";   // Quartermaster: a star system of level 4
    public const string CatExplore = "explore";        // Eye: 75% of the galaxy discovered
    public const string CatMastery = "mastery";        // Chairman: heroes that reached the Mastery rank (took the ultimate)
    public const int ExplorePercent = 75;
    public const int SystemLevelNeeded = 4;
    public const int MasteryHeroes = 5;

    public static CouncilMandateDef GetMandate(string id)
    {
        for (int i = 0; i < Mandates.Count; i++) if (Mandates[i].Id == id) return Mandates[i];
        return null;
    }

    public static int MandateIndex(string id)
    {
        for (int i = 0; i < Mandates.Count; i++) if (Mandates[i].Id == id) return i;
        return -1;
    }

    static List<CouncilMandateDef> BuildMandates()
    {
        var m = new List<CouncilMandateDef>();
        m.Add(new CouncilMandateDef(Chairman, new[] { CatMastery }, MasteryHeroes, false,
            "Председатель Совета Академии", "Chairman of the Academy Council",
            "Раз в цикл может заменить одно обычное постановление новым до начала голосования. Не накапливается; нельзя убрать выборы мандатов и вотум недоверия; после начала подачи голосов повестку менять нельзя.",
            "Once per cycle may replace one ordinary resolution with a new one before voting starts. Does not accumulate; cannot remove mandate elections or the no-confidence vote; the agenda cannot change once votes are cast.",
            "Довести 5 героев до ранга Мастерства (взять ультимативный навык)", "Bring 5 heroes to the Mastery rank (take the ultimate skill)"));
        m.Add(new CouncilMandateDef(Marshal, new[] { CatCapital }, 1, false,
            "Маршал Академии", "Marshal of the Academy",
            "Позволяет нанять один ограниченный служебный контингент. Содержание оплачивает игрок, сила растёт с этапом партии; продать его или превратить в бесплатный постоянный флот нельзя; контракт заканчивается вместе с мандатом.",
            "Allows hiring one limited service contingent. The player pays its upkeep, its strength scales with the game stage; it cannot be sold or turned into a free permanent fleet; the contract ends with the mandate.",
            "Захватить столицу другого игрока", "Capture the capital of another player"));
        m.Add(new CouncilMandateDef(Quartermaster, new[] { CatSystemLevel }, 1, false,
            "Интендант Совета", "Council Quartermaster",
            "Раз в цикл (20 ходов) позволяет запросить у Академии существенную поставку ресурсов. Размер растёт с этапом партии и длиной игры; возврат не требуется.",
            "Once per cycle (20 turns) may request a substantial resource delivery from the Academy. Scales with game stage and game length; no repayment.",
            "Улучшить звёздную систему до 4-го уровня", "Upgrade a star system to level 4"));
        m.Add(new CouncilMandateDef(Eye, new[] { CatExplore }, 1, false,
            "Око Академии", "Eye of the Academy",
            "+1 одновременная операция взлома и +20 к максимуму вычислительной мощности (ПВ).",
            "+1 simultaneous hacking operation and +20 to maximum processing power.",
            "Открыть 75% галактики", "Discover 75% of the galaxy"));
        m.Add(new CouncilMandateDef(Supreme, new string[0], 0, true,
            "Верховный легат Академии", "Supreme Legate of the Academy",
            "Победный мандат: даёт владельцу +100 голосов. Удерживайте его 30 ходов (нормальная скорость) для победы. Следующее голосование гарантированно содержит голосование «Установление абсолютной власти», а пока идёт отсчёт, один из вопросов — всегда оно: мандат остаётся при 51% голосов «за» (67%, если владелец состоит в альянсе).",
            "Victory mandate: gives the holder +100 votes. Hold it for 30 turns (normal speed) to win. The next vote is guaranteed to contain the Establishment of Absolute Power vote, and while the countdown runs one question is always that: the mandate stays with 51% of the votes in favour (67% when the holder is a member of an alliance).",
            "Допуск к 2 обычным мандатам и удержание одного обычного мандата в течение полного цикла", "Qualify for 2 ordinary mandates and hold one ordinary mandate for a full cycle"));
        return m;
    }

    // Law ids (stable: they are stored in saves)
    public const string Merit = "Merit";
    public const string ExpansionMandate = "ExpansionMandate";
    public const string ExpansionLimit = "ExpansionLimit";
    public const string DustPayout = "DustPayout";
    public const string Replenishment = "Replenishment";
    public const string IntelExchange = "IntelExchange";
    public const string NoDestabilization = "NoDestabilization";
    public const string Counterintel = "Counterintel";
    public const string NoConfidence = "NoConfidence";

    public static CouncilLawDef Get(string id)
    {
        for (int i = 0; i < All.Count; i++) if (All[i].Id == id) return All[i];
        return null;
    }

    static List<CouncilLawDef> Build()
    {
        var l = new List<CouncilLawDef>();
        const int C = CouncilLawDef.KindCycle, I = CouncilLawDef.KindInstant, P = CouncilLawDef.KindPermanent;

        // ---------------- applies to everyone (yes / no) ----------------
        l.Add(new CouncilLawDef("ResearchDevUp", false, C, 1, "resDev", true, "Ускоренное развитие империи", "Accelerated Empire Development",
            "Исследования ветки «Развитие империи» ускоряются на 50%.", "Empire Development research is 50% faster."));
        l.Add(new CouncilLawDef("ResearchDevDown", false, C, -1, "resDev", true, "Замедленное развитие империи", "Stalled Empire Development",
            "Исследования ветки «Развитие империи» замедляются на 50%.", "Empire Development research is 50% slower."));
        l.Add(new CouncilLawDef("ResearchEcoUp", false, C, 1, "resEco", true, "Ускоренная экономика и торговля", "Accelerated Economy & Trade",
            "Исследования ветки «Экономика и торговля» ускоряются на 50%.", "Economy & Trade research is 50% faster."));
        l.Add(new CouncilLawDef("ResearchEcoDown", false, C, -1, "resEco", true, "Замедленная экономика и торговля", "Stalled Economy & Trade",
            "Исследования ветки «Экономика и торговля» замедляются на 50%.", "Economy & Trade research is 50% slower."));
        l.Add(new CouncilLawDef("ResearchMilUp", false, C, 1, "resMil", true, "Ускоренные военные исследования", "Accelerated Military Research",
            "Исследования военной ветки ускоряются на 50%.", "Military research is 50% faster."));
        l.Add(new CouncilLawDef("ResearchMilDown", false, C, -1, "resMil", true, "Замедленные военные исследования", "Stalled Military Research",
            "Исследования военной ветки замедляются на 50%.", "Military research is 50% slower."));
        l.Add(new CouncilLawDef("WarshipUpkeepUp", false, C, -1, "upkeep", true, "Военные расходы", "War Expenditure",
            "Содержание боевых кораблей в Пыли повышается на 100%.", "Warship upkeep in Dust is +100%."));
        l.Add(new CouncilLawDef("EvacCompensation", false, C, 1, "evac", true, "Компенсация за эвакуацию", "Evacuation Compensation",
            "Сверхсущественное повышение компенсации Пылью за эвакуацию системы.", "Greatly increased Dust compensation for evacuating a system."));
        l.Add(new CouncilLawDef("EscortSquads", false, C, 1, "squads", true, "Эскортные эскадры", "Escort Squadrons",
            "Небольшие временные эскортные эскадры всем членам Совета.", "Small temporary escort squadrons for every council member."));
        l.Add(new CouncilLawDef("Mentoring", false, I, 1, "mentor", true, "Академическое менторство", "Academic Mentoring",
            "Все герои-губернаторы, не управляемые ИИ, моментально получают +3 уровня.", "All non-AI hero governors instantly gain +3 levels."));
        l.Add(new CouncilLawDef("StrategicPresence", false, C, 1, "presence", true, "Программа стратегического присутствия", "Strategic Presence Program",
            "Дополнительные голоса на следующем заседании за удержание заранее отмеченных систем.", "Extra votes at the next session for holding pre-marked systems."));
        l.Add(new CouncilLawDef(NoConfidence, false, I, 0, "noconf", true, "Установление абсолютной власти", "Establishment of Absolute Power",
            "Голосование за то, чтобы оставить Верховного легата. «За» — оставить, «против» — снять. Мандат остаётся, если «за» набрано не менее 51% голосов (не менее 67%, если владелец состоит в альянсе); иначе он отзывается и отсчёт победы прекращается.", "A vote to keep the Supreme Legate. \"For\" keeps the mandate, \"against\" removes it. The mandate stays with at least 51% of the votes in favour (at least 67% when the holder is a member of an alliance); otherwise it is revoked and the victory countdown stops."));
        l.Add(new CouncilLawDef(Counterintel, false, C, 0, "backdoor", true, "Контрразведка от Академии", "Academy Counter-Intelligence",
            "Запрещает установку новых бэкдоров.", "Installing new backdoors is forbidden."));
        l.Add(new CouncilLawDef(IntelExchange, false, C, 0, "sleepers", true, "Обмен разведывательными методиками", "Intelligence Method Exchange",
            "Для всех взломов считается, что в системе на 4 спящих больше.", "For all hacks, the system counts as having 4 more sleepers."));
        l.Add(new CouncilLawDef(NoDestabilization, false, C, 0, "sleepers", true, "Пакт о недопустимости дестабилизации", "Anti-Destabilization Pact",
            "Любые взломы, требующие наличия спящих, недопустимы.", "Any hack that requires sleepers is forbidden."));
        l.Add(new CouncilLawDef("Deregulation", false, C, 1, "market", true, "Дерегулирование рынка", "Market Deregulation",
            "Существенное снижение цен на рынке при покупке стратегических ресурсов и роскоши.", "Large discount on strategic and luxury resources at the Market."));
        l.Add(new CouncilLawDef("Privateering", false, C, 1, "market2", true, "Разрешение на каперство", "Letters of Marque",
            "Существенное снижение цен на рынке при покупке военных кораблей.", "Large discount when buying warships at the Market."));
        l.Add(new CouncilLawDef("TradeReinvest", false, C, 1, "tc", true, "Реинвестиции торговых компаний", "Trading Company Reinvestment",
            "Временный бонус к Промышленности в системах со штаб-квартирами или филиалами торговых компаний.", "Temporary Industry bonus in systems with trading company HQs or subsidiaries."));

        // ---------------- aimed at one empire (vote for an empire) ----------------
        l.Add(new CouncilLawDef("DamageDown", true, C, -1, "dmg", true, "Ограничение вооружений", "Arms Limitation",
            "Урон корабельного вооружения империи снижается на 20%.", "The empire's ship weapon damage is reduced by 20%."));
        l.Add(new CouncilLawDef(DustPayout, true, I, 1, "payout", true, "Выплата Академии", "Academy Payout",
            "Разовая выплата Пыли за каждую контролируемую колонизированную систему. Размер выплаты растёт с этапом партии.",
            "One-time Dust payment for every controlled colonized system. Scales with the stage of the game."));
        l.Add(new CouncilLawDef("UpkeepDown", true, C, 1, "upkeepT", true, "Льготы на содержание флота", "Fleet Upkeep Relief",
            "Расходы на содержание кораблей в Пыли снижаются на 50%.", "Ship upkeep in Dust is reduced by 50%."));
        l.Add(new CouncilLawDef("PillageX2", true, C, 1, "pillage", true, "Право на разграбление", "Right of Plunder",
            "Добыча от действия «Разграбить систему» повышена в 2 раза.", "Loot from the 'Pillage System' action is doubled."));
        l.Add(new CouncilLawDef("FortInspection", true, C, -1, "fort", true, "Инспекция укреплений", "Fortification Inspection",
            "Ослабление обороняющихся наземных войск и увеличение потерь гарнизона от осады на 50%.", "Defending ground troops are weakened and siege garrison losses are +50%."));
        l.Add(new CouncilLawDef("TradeRestrictions", true, C, -1, "trade", true, "Торговые ограничения", "Trade Restrictions",
            "Покупки и продажи ресурсов на Рынке для выбранной империи в 2 раза менее выгодны.", "Market purchases and sales are twice as unfavourable for the chosen empire."));
        l.Add(new CouncilLawDef("Truce", true, I, 1, "truce", true, "Академическое перемирие", "Academic Truce",
            "Принудительно заключает мир со всеми, с кем фракция сейчас воюет (устанавливается перемирие).", "Forces peace (a truce) with everyone the faction is currently at war with."));
        l.Add(new CouncilLawDef(Replenishment, true, C, 1, "replen", true, "Программа пополнения", "Replenishment Program",
            "Увеличение производства людских ресурсов (+50%) на время действия закона.", "Manpower production is increased by 50% while the law is in force."));
        l.Add(new CouncilLawDef(ExpansionMandate, true, P, 1, "colony", true, "Мандат на экспансию", "Expansion Mandate",
            "Навсегда даёт +1 к колонизационному лимиту, пока империя состоит в Совете.", "Permanently grants +1 colonization limit while the empire stays in the council."));
        l.Add(new CouncilLawDef("Infrastructure", true, C, 1, "infra", true, "Инфраструктурная программа", "Infrastructure Program",
            "Снижение промышленной стоимости системных улучшений (кроме галактических чудес).", "Lower Industry cost of system improvements (except galactic wonders)."));
        l.Add(new CouncilLawDef("AcademySquad", true, C, 1, "squad", true, "Академическая эскадра", "Academy Squadron",
            "Временная академическая эскадра; состав зависит от этапа партии. Возвращается по окончании закона.", "A temporary Academy squadron scaled to the game stage. Returned when the law ends."));
        l.Add(new CouncilLawDef("CapitalFortify", true, C, 1, "capital", true, "Укрепление столицы", "Capital Fortification",
            "Увеличение живучести защитников домашней системы и ускоренное восстановление гарнизона вне осады.", "Home-system defenders are tougher and recover faster outside of sieges."));
        l.Add(new CouncilLawDef(ExpansionLimit, true, P, -1, "colony", true, "Ограничение экспансии", "Expansion Restriction",
            "Империя навсегда теряет 1 колонизационный лимит, пока состоит в Совете.", "The empire permanently loses 1 colonization limit while in the council."));
        l.Add(new CouncilLawDef("StealthOps", true, C, 1, "stealth", true, "Лицензия на скрытые операции", "Covert Operations License",
            "Небольшая специализированная рейдерская группа с маскировкой.", "A small specialized cloaked raider group."));
        l.Add(new CouncilLawDef(Merit, true, P, 1, "votes", true, "Признание заслуг", "Recognition of Merit",
            "Выбранная фракция навсегда получает +30 голосов.", "The chosen faction permanently gains +30 votes."));
        l.Add(new CouncilLawDef("Antimonopoly", true, C, -1, "tcT", true, "Антимонопольный надзор", "Antitrust Oversight",
            "Доходы торговых компаний выбранной фракции снижаются вдвое на время действия закона.", "Trading company income of the chosen faction is halved while the law is in force."));
        for (int i = 0; i < Mandates.Count; i++)
        {
            var md = Mandates[i];
            l.Add(new CouncilLawDef(MandatePrefix + md.Id, true, KindMandateConst, 1, "mandate", false, md.Ru, md.En, md.RuDesc, md.EnDesc));
        }
        return l;
    }

    const int KindMandateConst = CouncilLawDef.KindMandate;
}
