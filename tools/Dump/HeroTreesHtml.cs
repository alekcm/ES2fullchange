// HTML output of the hero tree listing: the same data as the Markdown dump, but in plain Russian sentences instead of property names.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

static partial class HeroTreesDump
{
    // ---------------------------------------------------------------- vocabulary

    sealed class PropInfo { public string Name; public bool Fraction; public PropInfo(string name, bool fraction) { Name = name; Fraction = fraction; } }

    static readonly Dictionary<string, PropInfo> props = BuildProps();

    static Dictionary<string, PropInfo> BuildProps()
    {
        var d = new Dictionary<string, PropInfo>();
        Action<string, string, bool> a = (k, n, f) => d[k] = new PropInfo(n, f);
        // fleet
        a("ShipDamage", "урон орудий", true); a("Damage", "урон", true); a("ShipDamageEnergy", "урон энергетического оружия", true); a("ShipDamageProjectile", "урон снарядного оружия", true);
        a("FleetMaximumHealth", "прочность кораблей", true); a("FleetMaximumShield", "мощность щитов", true); a("MaximumShield", "мощность щитов", true);
        a("FleetHullPlatingAbsorption", "броня корпуса", false); a("ShipHullPlatingAbsorptionPercent", "поглощение урона бронёй", true);
        a("ShipShieldAbsorptionPercent", "поглощение урона щитами", true); a("ShieldAbsorptionPercent", "поглощение урона щитами", true);
        a("ShipHullPlatingPenetrationFactor", "пробивание брони", true); a("ShipShieldPenetrationFactor", "пробивание щитов", true);
        a("Evasion", "уклонение", true); a("ExperiencePerTurn", "опыт флота за ход", false);
        a("FleetManpowerRefillRateModifier", "скорость пополнения экипажей", true); a("HealthRegenerationRatio", "самовосстановление кораблей за ход", true);
        a("GarrisonMoneyUpkeep", "содержание флота (Пыль)", true); a("MaximumShipManpower", "размер экипажа кораблей", true);
        a("MaximumShipProbeStock", "запас зондов", false); a("NetShipProbe", "зондов за ход", false);
        a("ShipMaximumMovement", "дальность хода", false); a("FreeMovementSpeed", "скорость движения вне боя", false);
        a("SiegePower", "осадная мощность", true); a("VisionRange", "дальность обзора", false);
        // hero
        a("Upkeep", "содержание героя", true); a("AssignmentCooldownBaseDuration", "перерыв между назначениями героя", true);
        a("CanBeAssignedOutOfTerritory", "героя можно назначать вне своей территории", false);
        // system / planets
        a("SystemGrowth", "еда системы (рост)", false); a("BonusHeroSystemFood", "еда системы", false); a("PlanetRawFood", "еда с каждой планеты", false); a("BonusHeroPlanetFood", "еда с планеты", false);
        a("PlanetPopulationFood", "еда с жителей", false);
        a("SystemProduction", "промышленность системы", false); a("BonusHeroSystemIndustry", "промышленность системы", false); a("PlanetRawIndustry", "промышленность с каждой планеты", false); a("BonusHeroPlanetIndustry", "промышленность с планеты", false);
        a("SystemResearch", "наука системы", false); a("BonusHeroSystemScience", "наука системы", false); a("PlanetRawScience", "наука с каждой планеты", false); a("PlanetPopulationScience", "наука с жителей", false);
        a("SystemMoney", "Пыль системы", false); a("BonusHeroSystemDust", "Пыль системы", false); a("PlanetRawDust", "Пыль с каждой планеты", false);
        a("SystemEmpirePoint", "влияние системы", false); a("BonusHeroSystemPrestige", "влияние системы", false); a("PlanetRawPrestige", "влияние с каждой планеты", false); a("BonusHeroPlanetPrestige", "влияние с планеты", false);
        a("EmpireEmpirePoint", "влияние империи", false);
        a("PlanetRawFIDSI", "все пять ресурсов (еда, промышленность, Пыль, наука, влияние) с каждой планеты", false); a("PlanetHeroFIDSIPercent", "все ресурсы планеты", true);
        a("Happiness", "счастье", false); a("DepositValue", "ценность залежей ресурсов", false);
        a("GroundBattleDefenderTroopsMaxHealthMultiplier", "прочность наземных войск-защитников", true); a("GroundBattleBombardmentAttackerDamages", "урон атакующим при наземном бое", false);
        a("MaximumSystemManpower", "резерв людей в системе (максимум)", false); a("NetSystemManpower", "прирост людей в системе за ход", false); a("MaximumEmpireManpowerStock", "общий запас людей империи", true);
        a("OverColonizationPenalties", "штрафы за перенаселённость колониями", true); a("OverPopulationPenalties", "штрафы за перенаселение", true);
        a("PlanetDepletionPerTurn", "истощение планеты за ход", false); a("RawMaximumPopulation", "максимум населения на планете (слоты)", false);
        a("BuildingProductionCostReduction", "стоимость построек", true); a("ShipProductionCostReduction", "стоимость кораблей", true); a("SystemMoneyUpkeep", "содержание систем (Пыль)", true);
        a("TradingRoutesBonus_Growth", "бонус торговых путей к еде", true); a("TradingRoutesBonus_Production", "бонус торговых путей к промышленности", true);
        a("TradingRouteIncomeModifier", "доход от торговых путей", true);
        a("PeacefulTreatyCostReduction", "цена мирных договоров", false); a("AggressiveTreatyCostReduction", "цена агрессивных договоров", false); a("ElectionActionCost", "цена выборных действий в сенате", false);
        return d;
    }

    static readonly Dictionary<string, string> vars = new Dictionary<string, string>
    {
        { "Population", "каждого жителя" }, { "MajorPopulationCount", "каждого жителя основной расы" }, { "AnomalyCount", "каждую аномалию в системе" },
        { "OwnedPlanets", "каждую колонизированную планету" }, { "OwnedTeemingPlanets", "каждую изобильную планету" }, { "OwnedMeagerPlanets", "каждую скудную планету" },
        { "CommandPoints", "каждое очко командования героя" }, { "AssignedHeroLevel", "каждый уровень героя" }, { "NumberOfTradingCompanies", "каждую торговую компанию" },
        { "OwnedStrategicDeposits", "каждую стратегическую залежь" }, { "CuriositiesDiscovered", "каждую найденную диковину" }, { "SystemLevel", "каждый уровень системы" }
    };

    static readonly Dictionary<Flavor, string> flavorRu = new Dictionary<Flavor, string>
    {
        { Flavor.Offense, "Атака" }, { Flavor.Defense, "Защита" }, { Flavor.Mobility, "Мобильность" }, { Flavor.Sustain, "Самовосстановление" }, { Flavor.Upkeep, "Экономия содержания" },
        { Flavor.Production, "Производство" }, { Flavor.Growth, "Рост и еда" }, { Flavor.Science, "Наука" }, { Flavor.Money, "Деньги (Пыль)" }, { Flavor.Influence, "Влияние" },
        { Flavor.Happiness, "Счастье" }, { Flavor.Manpower, "Людские резервы" }, { Flavor.Deposit, "Залежи ресурсов" }, { Flavor.Experience, "Опыт" }, { Flavor.Diplomacy, "Дипломатия" },
        { Flavor.Votes, "Голоса в Совете" }
    };

    // ---------------------------------------------------------------- phrases

    static string H(string s) { return (s ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;"); }

    static string Nf(float v)
    {
        float a = Math.Abs(v);
        string s = a >= 100f ? Math.Round(a).ToString("0", inv) : a >= 10f ? Math.Round(a, 1).ToString("0.#", inv) : Math.Round(a, 2).ToString("0.##", inv);
        return s;
    }

    static string Sg(float v) { return (v < -0.0001f ? "−" : "+") + Nf(v); }

    static string ScopeOf(string path, HeroChannel channel)
    {
        if (string.IsNullOrEmpty(path))
            return channel == HeroChannel.Ship ? "флот героя" : channel == HeroChannel.System ? "система героя" : channel == HeroChannel.Senate ? "вся империя" : "";
        bool emp = path.Contains("ClassEmpire") || path.StartsWith("Empire");
        if (path.Contains("ClassGarrisonFleet") || path.Contains("GarrisonFleet"))
        {
            bool crew = path.Contains("ClassShip") || path.EndsWith("/Ship");
            return emp ? (crew ? "корабли всех флотов империи" : "все флоты империи") : (crew ? "корабли флота героя" : "флот героя");
        }
        if (path.Contains("ResourceDeposit")) return emp ? "залежи во всех системах империи" : "залежи ресурсов в системе героя";
        if (path.Contains("ColonizedPlanet")) return emp ? "по всей империи" : "в системе героя";
        if (path.Contains("ColonizedStarSystem")) return emp ? "каждая система империи" : "система героя";
        if (emp) return "вся империя";
        return path;
    }

    static string Phrase(string prop, int op, float v, string path, HeroChannel channel, float scale)
    {
        PropInfo info;
        if (!props.TryGetValue(prop, out info)) info = new PropInfo(prop, false);
        float x = v * scale;
        string val;
        switch (op)
        {
            case 1: val = info.Fraction ? Sg(x * 100f) + "%" : Sg(x); break;
            case 2: val = info.Fraction ? Sg(-x * 100f) + "%" : Sg(-x); break;
            case 3: val = "×" + Nf(1f + (v - 1f) * scale); break;
            case 6: val = Sg(x * 100f) + "%"; break;
            case 7: val = "= " + Nf(v); break;
            default: val = Sg(x); break;
        }
        if (prop == "CanBeAssignedOutOfTerritory") return "<b>" + H(info.Name) + "</b>";
        string scope = ScopeOf(path, channel);
        return "<b>" + H(val) + "</b> " + H(info.Name) + (scope.Length > 0 ? " <span class=\"sc\">— " + H(scope) + "</span>" : "");
    }

    static int OpOf(string op)
    {
        switch (op) { case "Addition": return 1; case "Subtraction": return 2; case "Multiplication": return 3; case "Percent": return 6; case "Force": return 7; }
        return 0;
    }

    static List<string> HumanOwn(List<string> descriptors, HeroChannel channel, float scale)
    {
        var list = new List<string>();
        foreach (string d in descriptors)
        {
            List<Mod> mods;
            if (d == "HeroSkillComplete" || !desc.TryGetValue(d, out mods)) continue;
            foreach (Mod m in mods)
            {
                if (m.Prop == "CompleteSkill" || m.Hidden) continue;
                PropInfo info;
                if (m.Binary)
                {
                    string left = m.Left ?? "", right = m.Right ?? "";
                    string variable = left.StartsWith("$(") ? left : right, number = left.StartsWith("$(") ? right : left;
                    string name = variable.Length > 3 ? variable.Substring(2, variable.Length - 3) : variable;
                    float nv;
                    if (!float.TryParse(number, NumberStyles.Float, inv, out nv)) { list.Add(H(m.Prop + " (сложная формула)")); continue; }
                    if (!props.TryGetValue(m.Prop, out info)) info = new PropInfo(m.Prop, false);
                    string who; if (!vars.TryGetValue(name, out who)) who = "«" + name + "»";
                    bool pct = m.Op == "Percent";
                    string val = pct ? Sg(nv * scale * 100f) + "%" : Sg(nv * scale);
                    string scopeText = ScopeOf(m.Path, channel);
                    list.Add("<b>" + H(val) + "</b> " + H(info.Name) + " <span class=\"sc\">за " + H(who) + (scopeText.Length > 0 ? " — " + H(scopeText) : "") + "</span>");
                    continue;
                }
                float v;
                if (!float.TryParse(m.Value, NumberStyles.Float, inv, out v)) continue;
                list.Add(Phrase(m.Prop, OpOf(m.Op), v, m.Path, channel, scale));
            }
        }
        return list;
    }

    // ---------------------------------------------------------------- html pieces

    static string Chips(IEnumerable<Flavor> flavors)
    {
        return string.Concat(flavors.Select(f => "<span class=\"chip f" + f + "\">" + H(flavorRu[f]) + "</span>"));
    }

    static string Lines(List<string> lines)
    {
        if (lines.Count == 0) return "<div class=\"none\">нет числовых эффектов</div>";
        return "<ul>" + string.Concat(lines.Select(l => "<li>" + l + "</li>")) + "</ul>";
    }

    static string Needs(int stageIndex, int required, HeroKind kind, bool dictator)
    {
        return required == 0 ? "доступно сразу" : "после " + Scale(required, kind, dictator) + " потраченных очков навыков";
    }

    static string RoleClass(int r) { return new[] { "rA", "rG", "rS", "rD" }[r]; }

    static readonly string[] roleShort = { "Адмирал", "Губернатор", "Сенатор", "Диктатор" };
    static readonly string[] roleHint =
    {
        "флот: влияет на корабли одного флота",
        "система: влияет на одну систему, где он назначен",
        "сенатор: влияет на всю империю, один уровень навыка",
        "диктатор: влияет на всю империю, пять уровней у каждого навыка и особая вершина"
    };

    static string DesignTreeHtml(string key, char role)
    {
        DesignTree tree = HeroDesign.Build(key, role);
        if (tree == null || (role == 'D' && key.StartsWith("Class", StringComparison.Ordinal))) return "";
        int r = "AGSD".IndexOf(role);
        var sb = new StringBuilder();
        sb.Append("<div class=\"rv " + RoleClass(r) + "\"><div class=\"rolehead role" + r + "\">" + roleShort[r] + " <small>" + H(roleHint[r]) + "</small></div>");
        for (int s = 0; s < tree.Stages.Count; s++)
        {
            DesignStage st = tree.Stages[s];
            sb.Append("<div class=\"stage\"><div class=\"stagehead\">Ступень " + (s + 1) + (s == 3 ? " — вершина" : "") + " <small>(" + H(st.RequiredLevel == 0 ? "доступно сразу" : "после " + st.RequiredLevel + " потраченных очков навыков") + ")</small></div><div class=\"cards\">");
            foreach (DesignSkill sk in st.Skills)
            {
                sb.Append("<div class=\"card\"><div class=\"ct\">" + H(sk.TitleRu) + "</div><div class=\"cen\">" + H(sk.TitleEn) + "</div><div class=\"id\">" + H(sk.Name) + "</div>");
                if (sk.Tier < 4) sb.Append("<div class=\"sc\">" + H(sk.Concepts[0].Note(true)) + "</div>");
                for (int l = 0; l < sk.Levels; l++)
                {
                    sb.Append("<div class=\"lvl\"><span class=\"ln\">" + (sk.Levels > 1 ? "Уровень " + (l + 1) : "Эффекты") + "</span><ul>");
                    for (int c = 0; c < sk.Concepts.Length; c++)
                        sb.Append("<li><b>" + H(sk.Concepts[c].Show(sk.Values[l][c], true)) + "</b> " + H(sk.Concepts[c].Name(true)) + "</li>");
                    sb.Append("</ul></div>");
                }
                if (sk.Tier == 4)
                    for (int c = 0; c < sk.Concepts.Length; c++)
                        if (sk.Concepts[c].Special != null) sb.Append("<div class=\"sc\"><b>" + H(sk.Concepts[c].Name(true)) + ":</b> " + H(sk.Concepts[c].Note(true)) + "</div>");
                sb.Append("</div>");
            }
            sb.Append("</div></div>");
        }
        sb.Append("</div>");
        return sb.ToString();
    }

    static string DesignGroupHtml(string id, string title, string note, string prefix)
    {
        var sb = new StringBuilder();
        sb.Append("<h2 id=\"" + id + "\">" + H(title) + "</h2><div class=\"note\">" + H(note) + "</div>");
        var keys = new List<string>();
        foreach (char role in new[] { 'A', 'G', 'S', 'D' })
            foreach (string k in HeroDesign.AllKeys(role))
                if (k.StartsWith(prefix, StringComparison.Ordinal) && !keys.Contains(k)) keys.Add(k);
        foreach (string key in keys)
        {
            sb.Append("<h3>" + H(HeroDesign.KeyTitle(key, true)) + " <small>(" + H(HeroDesign.KeyTitle(key, false)) + ")</small></h3>");
            foreach (char role in new[] { 'A', 'G', 'S', 'D' }) sb.Append(DesignTreeHtml(key, role));
        }
        return sb.ToString();
    }

    static string VanillaSkillCard(string name, bool alt)
    {
        Skill sk;
        var sb = new StringBuilder();
        if (!skills.TryGetValue(name, out sk)) return "<div class=\"card\">" + H(name) + " (нет определения)</div>";
        string ru0 = TitleOf(name, true), en0 = TitleOf(name, false);
        sb.Append("<div class=\"card vcard\"><div class=\"ct\">" + H(ru0 ?? name) + "</div><div class=\"cen\">" + H(en0 ?? "") + "</div><div class=\"id\">" + H(name) + "</div>");
        string d = DescOf(name, true);
        if (!string.IsNullOrEmpty(d)) sb.Append("<div class=\"flav\">" + H(d) + "</div>");
        sb.Append("<div class=\"orig\"><div class=\"origh\">Как в оригинальной игре</div>");
        for (int i = 0; i < sk.Levels.Count; i++)
        {
            Level l = sk.Levels[i];
            var lines = new List<string>();
            lines.AddRange(HumanOwn(l.System, HeroChannel.System, 1f)); lines.AddRange(HumanOwn(l.Ship, HeroChannel.Ship, 1f));
            lines.AddRange(HumanOwn(l.Senate, HeroChannel.Senate, 1f)); lines.AddRange(HumanOwn(l.Hero, HeroChannel.Hero, 1f));
            sb.Append("<div class=\"lvl\"><span class=\"ln\">Уровень " + (i + 1) + "</span>" + Lines(lines) + "</div>");
        }
        sb.Append("</div>");
        sb.Append("</div>");
        return sb.ToString();
    }

    static string HeroSkillHtml(string skillName)
    {
        Skill sk;
        if (!skills.TryGetValue(skillName, out sk)) return H(skillName);
        var sb = new StringBuilder();
        sb.Append("<div class=\"hs\"><b>" + H(TitleOf(skillName, true) ?? skillName) + "</b>");
        for (int i = 0; i < sk.Levels.Count; i++)
        {
            Level l = sk.Levels[i];
            var lines = new List<string>();
            lines.AddRange(HumanOwn(l.System, HeroChannel.System, 1f)); lines.AddRange(HumanOwn(l.Ship, HeroChannel.Ship, 1f));
            lines.AddRange(HumanOwn(l.Senate, HeroChannel.Senate, 1f)); lines.AddRange(HumanOwn(l.Hero, HeroChannel.Hero, 1f));
            if (lines.Count > 0) sb.Append("<div class=\"lvl\"><span class=\"ln\">ур." + (i + 1) + "</span>" + string.Join("; ", lines) + "</div>");
        }
        sb.Append("</div>");
        return sb.ToString();
    }

    static string KindRu(string cls)
    {
        HeroKind k;
        if (!HeroKinds.TryKindOfClass(cls, out k)) return "?";
        return k == HeroKind.Admiral ? "Адмирал" : k == HeroKind.Governor ? "Губернатор" : "Сенатор";
    }

    static string ClassRu(string cls)
    {
        return TitleOf(cls, true) ?? cls.Replace("HeroClass", "");
    }

    // ---------------------------------------------------------------- page

    const string Css = @"
body{font:15px/1.45 Segoe UI,Arial,sans-serif;margin:0;background:#12151c;color:#dde3ee}
a{color:#7fc4ff}
header{padding:18px 28px;background:#1b2130;border-bottom:1px solid #2c3550}
h1{margin:0 0 4px;font-size:24px}
.bar{position:sticky;top:0;z-index:10;background:#1b2130;border-bottom:1px solid #2c3550;padding:8px 28px;display:flex;flex-wrap:wrap;gap:6px;align-items:center}
.bar button,.bar a.nav{background:#273049;color:#dde3ee;border:1px solid #3b4870;border-radius:6px;padding:5px 11px;cursor:pointer;font-size:14px;text-decoration:none}
.bar button.on{background:#3f6fd6;border-color:#6d97f2}
.bar .sep{width:18px}
main{padding:10px 28px 60px;max-width:1500px}
h2{border-bottom:2px solid #34406a;padding-bottom:4px;margin-top:42px}
h3{margin:28px 0 6px;color:#9cc6ff}
h4{margin:20px 0 6px;color:#ffd58a}
.note{background:#1c2436;border-left:4px solid #5b82e0;padding:8px 14px;margin:10px 0;border-radius:4px}
.warn{background:#2c2418;border-left:4px solid #e0a64a}
.stage{margin:10px 0}.stagehead{font-weight:600;color:#c9d6f0;margin:8px 0 4px}.stagehead small{font-weight:400;color:#8c9ab8}
.cards{display:flex;flex-wrap:wrap;gap:10px}
.card{background:#1a2030;border:1px solid #2f3b5e;border-radius:8px;padding:9px 12px;width:330px;box-sizing:border-box}
.card.vcard{width:420px}
.ct{font-weight:700;font-size:16px;color:#fff}.cen{font-size:12px;color:#8c9ab8;margin-bottom:3px}.id{font:11px monospace;color:#66728e;margin:2px 0 4px;word-break:break-all}
.flav{font-size:12.5px;color:#9aa6c0;font-style:italic;margin:4px 0}
.lvl{margin:5px 0}.ln{display:inline-block;background:#2b3858;border-radius:4px;padding:0 6px;font-size:12px;margin-bottom:2px}
.lvl ul,.card ul{margin:2px 0 2px 0;padding-left:18px}.lvl li{margin:1px 0}
.sc{color:#8c9ab8;font-size:12.5px}.none{color:#79839c;font-style:italic;font-size:13px}
.chip{display:inline-block;border-radius:10px;padding:0 8px;margin:1px 3px 1px 0;font-size:12px;background:#35405f;color:#fff}
.fOffense,.fDefense,.fManpower{background:#8a3b3b}.fMobility,.fExperience,.fScience{background:#3b6a8a}.fProduction,.fUpkeep,.fMoney{background:#8a6a2c}.fGrowth,.fSustain,.fDeposit{background:#3e7a47}.fInfluence,.fHappiness,.fVotes,.fDiplomacy{background:#6a4a8a}
.rolehead{font-weight:700;padding:5px 10px;border-radius:6px;margin:8px 0 4px}.rolehead small{font-weight:400;opacity:.8}
.role0{background:#4a2a2a}.role1{background:#2a4a33}.role2{background:#2a3a5a}.role3{background:#5a3a1f}
details.roles{margin-top:6px}details summary{cursor:pointer;color:#7fc4ff}
.orig{border-top:1px dashed #34406a;margin-top:6px;padding-top:4px}.origh{font-size:12px;color:#8c9ab8}
body[data-role=A] .rv:not(.rA),body[data-role=G] .rv:not(.rG),body[data-role=S] .rv:not(.rS),body[data-role=D] .rv:not(.rD){display:none}
table{border-collapse:collapse;width:100%;font-size:13.5px}th,td{border:1px solid #2f3b5e;padding:4px 8px;vertical-align:top;text-align:left}th{background:#202a44;position:sticky;top:44px}
.hs{margin-bottom:6px}
.ideas table td,.ideas table th{font-size:14px}.ideas code{background:#222b44;padding:0 4px;border-radius:3px}
.ideas h1{font-size:20px}.ideas h2{font-size:19px}.ideas h3{font-size:17px}
.themes span{margin-right:8px}
";

    const string Js = @"
function setRole(r){document.body.setAttribute('data-role',r);
 document.querySelectorAll('.bar button.role').forEach(function(b){b.classList.toggle('on',b.getAttribute('data-r')===r)});
 document.querySelectorAll('details.roles').forEach(function(d){d.open=(r!=='all')});}
";

    static void WriteHtml(string path)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html lang=\"ru\"><head><meta charset=\"utf-8\"><title>Деревья навыков героев</title><style>" + Css + "</style></head><body data-role=\"all\">");
        sb.Append("<header><h1>Деревья навыков героев — понятным языком</h1><div>Эндлесс Спейс 2 + мод «Галактический Совет». Всё, что умеют навыки всех героев, написано обычными фразами.</div></header>");
        sb.Append("<div class=\"bar\"><b>Показывать роль:</b>" +
                  "<button class=\"role on\" data-r=\"all\" onclick=\"setRole('all')\">Все</button><button class=\"role\" data-r=\"A\" onclick=\"setRole('A')\">Адмирал</button>" +
                  "<button class=\"role\" data-r=\"G\" onclick=\"setRole('G')\">Губернатор</button><button class=\"role\" data-r=\"S\" onclick=\"setRole('S')\">Сенатор</button>" +
                  "<button class=\"role\" data-r=\"D\" onclick=\"setRole('D')\">Диктатор</button><span class=\"sep\"></span>" +
                  "<a class=\"nav\" href=\"#how\">Как читать</a><a class=\"nav\" href=\"#ideas\">Идеи</a><a class=\"nav\" href=\"#parties\">Партии</a><a class=\"nav\" href=\"#races\">Расы</a><a class=\"nav\" href=\"#classes\">Классы</a><a class=\"nav\" href=\"#factions\">Диктаторы фракций</a><a class=\"nav\" href=\"#vanilla\">Ванильные деревья</a><a class=\"nav\" href=\"#heroes\">Герои</a></div>");
        sb.Append("<main>");

        sb.Append("<h2 id=\"how\">Как читать</h2>");
        sb.Append("<div class=\"note\"><b>Роли героя.</b> Один и тот же герой в моде работает в одной из ролей, и от роли зависит, <i>где</i> действует навык: " +
                  "<b>Адмирал</b> — на один флот; <b>Губернатор</b> — на одну систему; <b>Сенатор</b> — на всю империю (один уровень каждого навыка); " +
                  "<b>Диктатор</b> — на всю империю, но с пятью уровнями у каждого навыка. Роль определяется классом героя: Адмирал и Авантюрист → адмирал, Администратор → губернатор, Корпоративный → сенатор.</div>");
        sb.Append("<div class=\"note\"><b>Дерево.</b> У каждого дерева 4 ступени: три ступени с одиночными навыками и «вершина» с несколькими эффектами сразу. Один и тот же эффект в дереве не повторяется. Следующая ступень открывается, когда герой потратил определённое число очков навыков " +
                  "(адмирал и губернатор: 4 / 8 / 12; сенатор: 2 / 4 / 6; диктатор: 6 / 12 / 18). Если у навыка несколько уровней, каждый следующий уровень сильнее.</div>");
        sb.Append("<div class=\"note\"><b>Партия и фракция.</b> Партия героя (Индустриалисты, Учёные, Пацифисты, Экологи, Религиозные, Милитаристы) заменяет в моде общее дерево героя на партийное, а расовое и классовое деревья тоже написаны модом. " +
                  "Диктатор получает дерево своей фракции вместо классового дерева.</div>");
        sb.Append("<div class=\"note warn\"><b>Оговорки.</b> Деревья мода реализованы, но в игре ещё не проверялись; соответствие эффектов игровым свойствам (например, «восстановление корпуса вне боя») подобрано по данным игры и требует проверки. Числа в ванильном разделе в процентах — моя трактовка игровых значений. " +
                  "Ванильные данные взяты из более старой версии игры, в вашей сборке цифры и названия могут отличаться. Техническое имя навыка (серый мелкий шрифт) нужно, чтобы найти его в коде.</div>");

        sb.Append("<h2 id=\"ideas\">Идеи: что могли бы делать партии и фракции</h2><div class=\"ideas\">@@IDEAS@@</div>");

        sb.Append(DesignGroupHtml("parties", "Партии: деревья навыков мода (6 партий × 4 роли)", "Эти деревья создаёт мод. Партийное дерево заменяет ванильное «Универсальное» у героев соответствующей партии. Кнопки роли сверху оставляют на странице одну роль.", "Party"));
        sb.Append(DesignGroupHtml("races", "Расы: деревья навыков мода", "Расовое дерево героя (по фракции, откуда он родом) заменяет ванильное расовое. Для каждой роли своё дерево.", "Race"));
        sb.Append(DesignGroupHtml("classes", "Классы: деревья навыков мода", "Классовое дерево заменяет ванильное. Адмирал и Авантюрист — адмиральские деревья, Администратор — губернаторское, Корпорат — сенаторское. Диктатор классового дерева не имеет.", "Class"));
        sb.Append(DesignGroupHtml("factions", "Деревья диктаторов по фракциям", "Диктатор — герой, представляющий в сенате правителя империи при диктатуре. Вместо классового дерева он получает дерево своей фракции: по два навыка на ступень и особая вершина.", "Dict"));

        // ---- vanilla
        sb.Append("<h2 id=\"vanilla\">Ванильные деревья навыков (для сравнения)</h2>");
        sb.Append("<div class=\"note\">Это деревья оригинальной игры. В моде они больше не используются: вместо них действуют деревья мода из разделов выше. Личные навыки героев остаются как в игре.</div>");
        foreach (Tree t in trees)
        {
            var users = heroes.Where(h => h.Trees.Contains(t.Name)).ToList();
            sb.Append("<h3>" + H(TitleOf(t.Name, true) ?? t.Name) + " <small>(" + H(t.Name) + ")</small></h3>");
            string td = DescOf(t.Name, true);
            sb.Append("<div class=\"sc\">" + H(td ?? "") + " Героев с этим деревом: " + users.Count + ".</div>");
            int idx = 0;
            foreach (Stage st in t.Stages)
            {
                idx++;
                sb.Append("<div class=\"stage\"><div class=\"stagehead\">Ступень " + idx + (idx == t.Stages.Count ? " — вершина" : "") + " <small>(" + (st.Level == 0 ? "доступно сразу" : "от " + st.Level + " очков навыков у адмирала и губернатора, " + st.Level / 2 + " у сенатора, " + (st.Level * 3 + 1) / 2 + " у диктатора") + ")</small></div><div class=\"cards\">");
                foreach (Slot sl in st.Slots) sb.Append(VanillaSkillCard(sl.Skill, false));
                sb.Append("</div></div>");
            }
        }

        // ---- heroes
        sb.Append("<h2 id=\"heroes\">Герои (" + heroes.Count + ")</h2>");
        sb.Append("<div class=\"note\">У каждого героя есть два личных навыка (они есть всегда, но в моде действуют только те эффекты, что подходят роли героя) и набор деревьев: партийное (вместо универсального), расовое и классовое.</div>");
        sb.Append("<table><tr><th>Герой</th><th>Роль в моде</th><th>Класс</th><th>Фракция</th><th>Партия</th><th>Деревья</th><th>Личные навыки</th></tr>");
        foreach (Hero h in heroes)
        {
            int party = HeroFlavors.PartyIndex(h.Politics);
            string tr = string.Join(", ", h.Trees.Select(x => x == "HeroSkillTreeGeneric" ? (party > 0 ? "Партийное («" + HeroFlavors.PartyName(party, true) + "»)" : "Универсальное") : (TitleOf(x, true) ?? x)));
            sb.Append("<tr><td><b>" + H(TitleOf(h.Name, true) ?? h.Name) + "</b><div class=\"id\">" + H(h.Name) + (h.Hidden ? " (скрыт)" : "") + "</div></td><td>" + KindRu(h.Class) + "</td><td>" + H(ClassRu(h.Class)) + "</td><td>" +
                      H(HeroFlavors.FactionName(HeroFlavors.FactionKey(h.Affinity), true)) + "</td><td>" + (party > 0 ? H(HeroFlavors.PartyName(party, true)) : "—") + "</td><td>" + H(tr) + "</td><td>" +
                      string.Concat(h.Skills.Select(HeroSkillHtml)) + "</td></tr>");
        }
        sb.Append("</table>");
        sb.Append("</main><script>" + Js + "</script></body></html>");
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }
}
