// God-Mod menu: Galactic Council and hero testing tools (IMGUI). The functions are called at the end of two tabs of the God-Mod window
// (CustomHackingHelper.DrawTabTechLawsHeroes and DrawTabDiplomacyAndEmpires) - see Patcher.HookDebugTabs.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Simulation;

public static class CouncilDebug
{
    static bool councilOpen, heroOpen;
    static int recruitFilter;               // 0 all, 1 admirals, 2 governors, 3 senators
    static int recruitLevel = 1;
    static Vector2 recruitScroll, heroScroll;
    static string status = "";

    static string T(string ru, string en) { return CouncilGame.Tr(ru, en); }
    static bool Btn(string text) { return GUILayout.Button(text, GUILayout.ExpandWidth(false)); }
    static void Say(string s) { status = s; try { CouncilUi.Toast(s); } catch (Exception) { } }

    // ============================================================ Council

    public static void DrawCouncilTools(Game game, MajorEmpire player)
    {
        try
        {
            GUILayout.Space(10);
            councilOpen = GUILayout.Toggle(councilOpen, T("══ ОТЛАДКА: Галактический совет ══", "══ DEBUG: Galactic Council ══"));
            if (!councilOpen || player == null) return;
            var core = CouncilGame.Core;
            if (!CouncilGame.ManagerKnown) GUILayout.Label(T("Менеджер Академии ещё не виден (после загрузки он появится в начале следующего хода): «Созвать» и «Открыть голосование» пока ничего не сделают.", "The Academy manager is not seen yet (after a load it appears at the start of the next turn): \"Convene\" and \"Open the vote\" do nothing for now."));
            int emp = player.Index;
            int turn = game.Turn;
            GUILayout.Label(T("Управляется империя: ", "Controlled empire: ") + CouncilGame.EmpireName(emp) + T("  (смена игрока — «Играть за эту империю» выше)", "  (to switch use \"Play As\" above)"));

            GUILayout.BeginHorizontal();
            if (Btn(T("Открыть Академию (с любого хода)", "Open the Academy (any turn)"))) CouncilGame.DebugSend(player, CouncilGame.DbgOpenAcademy, 0);
            GUILayout.Label(T("  открывает Академию для всех, снимает задержку первого запроса и созывает Совет", "  discovers the Academy for everybody, removes the first-request delay and convenes the Council"));
            GUILayout.EndHorizontal();
            if (CouncilGame.TextsStatus.Length > 0) GUILayout.Label(T("Пометки «даёт голоса» в текстах игры: ", "\"Gives votes\" notes in game texts: ") + CouncilGame.TextsStatus);
            if (!core.Enabled)
            {
                GUILayout.Label(core.Destroyed ? T("Совет распущен (Академия была уничтожена).", "The Council is disbanded (the Academy was eliminated).")
                                               : T("Совет ещё не созван: нужно, чтобы Академия была открыта, и прошло достаточно ходов.", "The Council is not convened yet: the Academy must be discovered and enough turns must pass."));
                if (Btn(T("Созвать Совет сейчас", "Convene the Council now"))) CouncilGame.DebugSend(player, CouncilGame.DbgFound, 0);
                return;
            }

            // ---- state
            bool voting = core.Phase == CouncilCore.PhaseVoting;
            GUILayout.Label(T("Фаза: ", "Phase: ") + (voting ? T("ИДЁТ ГОЛОСОВАНИЕ", "VOTING") : T("ожидание", "idle"))
                + T("   голосование №", "   vote #") + core.VoteIndex
                + T("   до следующего: ", "   next in: ") + core.TurnsToNextVote(turn) + T(" ход.", " turn(s)")
                + T("   участников: ", "   members: ") + core.Members.Count);
            GUILayout.Label(T("Голоса ", "Votes ") + CouncilGame.EmpireName(emp) + ": " + core.Budget(emp) + T(" (потрачено ", " (spent ") + core.Spent(emp) + ")");
            GUILayout.Label("  " + core.BudgetText(emp, turn, CouncilGame.IsRussian()));
            string holder = core.SupremeHolder >= 0 ? CouncilGame.EmpireName(core.SupremeHolder) : T("нет", "none");
            GUILayout.Label(T("Верховный легат: ", "Supreme Legate: ") + holder
                + T("   порог удержания: ", "   keep threshold: ") + core.KeepPercentNeeded + "%"
                + T("   альянс: ", "   alliance: ") + (core.SupremeInAlliance ? T("да", "yes") : T("нет", "no"))
                + (CouncilGame.AllianceOverride >= 0 ? T(" (принудительно)", " (forced)") : T(" (авто)", " (auto)")));

            // ---- vote flow
            GUILayout.BeginHorizontal();
            if (!voting && Btn(T("Открыть голосование сейчас", "Open the vote now"))) CouncilGame.DebugSend(player, CouncilGame.DbgOpenVote, 0);
            if (!voting && Btn(T("Голосование со следующего хода", "Vote at next turn"))) CouncilGame.DebugSend(player, CouncilGame.DbgSoon, 0);
            if (voting && Btn(T("ИИ голосуют сейчас", "AIs vote now"))) CouncilGame.DebugSend(player, CouncilGame.DbgAiVote, 0);
            if (voting && Btn(T("Подвести итоги сейчас", "Tally now"))) CouncilGame.DebugSend(player, CouncilGame.DbgTally, 0);
            if (Btn(T("Окно Совета", "Council window"))) CouncilUi.Visible = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (Btn(T("Выдать класс сенатора (покупки голосов)", "Give the senator class (vote purchases)"))) CouncilGame.DebugSend(player, CouncilGame.DbgGiveBuySkills, 0);
            GUILayout.EndHorizontal();

            // ---- votes
            GUILayout.Label(T("Голоса:", "Votes:"));
            GUILayout.BeginHorizontal();
            if (Btn("+10")) CouncilGame.DebugSend(player, CouncilGame.DbgAddVotes, 10);
            if (Btn("+100")) CouncilGame.DebugSend(player, CouncilGame.DbgAddVotes, 100);
            if (Btn("-100")) CouncilGame.DebugSend(player, CouncilGame.DbgAddVotes, -100);
            if (Btn(T("Пересчитать из игры", "Recompute from game"))) CouncilGame.DebugSend(player, CouncilGame.DbgRecalcVotes, 0);
            if (Btn(T("Сбросить ручные правки", "Reset manual changes"))) CouncilGame.DebugSend(player, CouncilGame.DbgClearVoteBonus, 0);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (Btn(T("+ война (−20 голосов)", "+ declared war (-20 votes)"))) CouncilGame.DebugSend(player, CouncilGame.DbgAddWar, 0);
            if (Btn(T("Забыть войны", "Forget wars"))) CouncilGame.DebugSend(player, CouncilGame.DbgClearWars, 0);
            GUILayout.Label(T("   Альянс легата:", "   Legate alliance:"), GUILayout.ExpandWidth(false));
            if (Btn(T("авто", "auto"))) CouncilGame.DebugSend(player, CouncilGame.DbgAlliance, -1);
            if (Btn(T("один (51%)", "alone (51%)"))) CouncilGame.DebugSend(player, CouncilGame.DbgAlliance, 0);
            if (Btn(T("в альянсе (67%)", "allied (67%)"))) CouncilGame.DebugSend(player, CouncilGame.DbgAlliance, 1);
            GUILayout.EndHorizontal();

            // ---- mandates
            GUILayout.Label(T("Мандаты (условие / владелец):", "Mandates (condition / holder):"));
            for (int i = 0; i < CouncilLaws.Mandates.Count; i++)
            {
                var d = CouncilLaws.Mandates[i];
                int h = core.HolderOf(d.Id);
                string prog;
                if (d.Victory) prog = T("квалиф. обычных ", "ordinary qualified ") + core.OrdinaryQualifiedCount(emp) + "/2, "
                                      + (core.HeldOrdinaryForFullCycle(emp) ? T("цикл выдержан", "full cycle held") : T("цикл не выдержан", "no full cycle"));
                else prog = core.TaskCount(emp, d.Categories) + "/" + d.Need;
                GUILayout.BeginHorizontal();
                GUILayout.Label((CouncilGame.IsRussian() ? d.Ru : d.En) + "  [" + prog + "]  →  " + (h >= 0 ? CouncilGame.EmpireName(h) : "—"), GUILayout.Width(520));
                if (Btn(d.Victory ? T("Дать условия (2 мандата)", "Grant conditions (2 mandates)") : T("Засчитать условие", "Grant condition"))) CouncilGame.DebugSend(player, CouncilGame.DbgGrantQualification, i);
                if (Btn(T("Дать мне", "Give to me"))) CouncilGame.DebugSend(player, CouncilGame.DbgGiveMandate, i);
                if (Btn(T("Отнять", "Take away"))) CouncilGame.DebugSend(player, CouncilGame.DbgTakeMandate, i);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            if (Btn(T("Прочитать условия из игры", "Detect conditions from game"))) CouncilGame.DebugSend(player, CouncilGame.DbgDetectNow, 0);
            if (Btn(T("Сбросить условия империи", "Clear conditions"))) CouncilGame.DebugSend(player, CouncilGame.DbgClearQualifications, 0);
            GUILayout.EndHorizontal();
            GUILayout.Label(T("Проверка «Установления абсолютной власти»: «Дать мне» Верховного легата → «Открыть голосование» → проголосовать → «Подвести итоги».",
                              "To test \"Establishment of Absolute Power\": give yourself the Supreme Legate, open the vote, vote, tally."));

            // ---- history
            if (core.History.Count > 0)
            {
                GUILayout.Label(T("Последние итоги:", "Last results:"));
                int from = Math.Max(0, core.History.Count - 6);
                for (int i = core.History.Count - 1; i >= from; i--)
                {
                    var r = core.History[i];
                    GUILayout.Label("  " + HistoryLine(r));
                }
            }
            if (status.Length > 0) GUILayout.Label(status);
        }
        catch (Exception e) { GUILayout.Label("Council debug: " + e.Message); }
    }

    static string HistoryLine(CouncilResult r)
    {
        try
        {
            var l = CouncilLaws.Get(r.LawId);
            string name = l == null ? r.LawId : (CouncilGame.IsRussian() ? l.Ru : l.En);
            return name + ": " + (r.Passed ? T("принято", "passed") : T("отклонено", "rejected")) + " (" + r.Yes + " / " + r.No + ")";
        }
        catch (Exception) { return "?"; }
    }

    // ============================================================ Heroes

    public static void DrawHeroTools(Game game, MajorEmpire player)
    {
        try
        {
            GUILayout.Space(10);
            heroOpen = GUILayout.Toggle(heroOpen, T("══ ОТЛАДКА: герои (уровни, получение, мастерство) ══", "══ DEBUG: heroes (levels, recruiting, mastery) ══"));
            if (!heroOpen || player == null) return;
            var edu = player.GetAgency<DepartmentOfEducation>();
            if (edu == null) { GUILayout.Label("DepartmentOfEducation: n/a"); return; }
            bool ru = CouncilGame.IsRussian();

            // ---- academy offer (variant 2b) switch
            HeroRework.AcademyOfferEnabled = GUILayout.Toggle(HeroRework.AcademyOfferEnabled,
                T("Выбор типа героя при заполнении шкалы Академии (Адмирал/Губернатор/Сенатор, затем два кандидата)",
                  "Pick the hero kind when the Academy gauge fills (Admiral/Governor/Senator, then two candidates)"));

            // ---- own heroes
            GUILayout.Label(T("Герои империи ", "Heroes of ") + CouncilGame.EmpireName(player.Index) + ":");
            var list = new List<Hero>();
            for (int i = 0; i < edu.ActiveHeroes.Count; i++)
            {
                var hero = edu.ActiveHeroes[i];
                if (hero != null && hero.Definition != null && HeroRework.IsRegular(hero)) list.Add(hero);
            }
            if (list.Count == 0) GUILayout.Label(T("  героев нет — наймите ниже", "  no heroes - recruit one below"));
            for (int i = 0; i < list.Count; i++)
            {
                var hero = list[i];
                string kind = HeroKinds.Name(HeroRework.KindOf(hero), ru) + (HeroRework.IsDictator(hero) ? (ru ? ", диктатор" : ", dictator") : "");
                string mastery = HeroRework.HoldsUltimate(hero) ? T("Мастерство ✓", "Mastery ✓") : (HeroRework.DebugHasUltimateSkill(hero) ? T("Мастерства нет", "no Mastery") : T("нет ультимата", "no ultimate"));
                GUILayout.BeginHorizontal();
                GUILayout.Label(hero.LocalizedName + "  [" + kind + "]  " + T("ур. ", "lvl ") + hero.Level
                    + "  XP " + (int)hero.Experience + "/" + (int)hero.NextLevelRequestedExperience
                    + "  " + T("очки ", "points ") + hero.AvailableSkillPoints + "/" + hero.SkillPoints + "  " + mastery, GUILayout.Width(560));
                if (Btn("+1")) Level(hero, 1, game);
                if (Btn("+5")) Level(hero, 5, game);
                if (Btn("+20")) Level(hero, 20, game);
                if (Btn(T("Мастерство", "Mastery"))) Say(HeroRework.DebugGrantUltimate(hero, game.Turn));
                if (Btn(T("+5 очков", "+5 pts"))) { HeroRework.DebugAddSkillPoints(hero, 5); }
                GUILayout.EndHorizontal();
            }
            if (list.Count > 0)
            {
                GUILayout.BeginHorizontal();
                if (Btn(T("Всем +1 ур.", "All +1 lvl"))) for (int i = 0; i < list.Count; i++) Level(list[i], 1, game);
                if (Btn(T("Всем +5 ур.", "All +5 lvl"))) for (int i = 0; i < list.Count; i++) Level(list[i], 5, game);
                if (Btn(T("Всем мастерство", "All Mastery"))) for (int i = 0; i < list.Count; i++) HeroRework.DebugGrantUltimate(list[i], game.Turn);
                GUILayout.EndHorizontal();
            }

            // ---- hero gauge (natural way to get heroes)
            DrawGauges(player, edu);

            // ---- recruit
            GUILayout.Label(T("Нанять героя сразу (минуя шкалу):", "Recruit a hero directly (skips the gauge):"));
            GUILayout.BeginHorizontal();
            GUILayout.Label(T("Тип:", "Kind:"), GUILayout.ExpandWidth(false));
            string[] kinds = { T("все", "all"), HeroKinds.Name(HeroKind.Admiral, ru), HeroKinds.Name(HeroKind.Governor, ru), HeroKinds.Name(HeroKind.Senator, ru) };
            for (int k = 0; k < kinds.Length; k++) if (GUILayout.Toggle(recruitFilter == k, kinds[k], GUI.skin.button, GUILayout.ExpandWidth(false))) recruitFilter = k;
            GUILayout.Label(T("   Уровень найма:", "   Hire level:"), GUILayout.ExpandWidth(false));
            int[] levels = { 1, 5, 10, 20 };
            for (int k = 0; k < levels.Length; k++) if (GUILayout.Toggle(recruitLevel == levels[k], levels[k].ToString(), GUI.skin.button, GUILayout.ExpandWidth(false))) recruitLevel = levels[k];
            GUILayout.EndHorizontal();

            var owned = OwnedDefinitions(game);
            var defs = Databases.GetDatabase<HeroDefinition>(false).GetValues();
            var shown = new List<HeroDefinition>();
            for (int i = 0; i < defs.Length; i++)
            {
                var d = defs[i];
                if (d == null) continue;
                int kindIndex = KindIndexOf(d);
                if (recruitFilter != 0 && kindIndex != recruitFilter) continue;
                shown.Add(d);
            }
            GUILayout.BeginHorizontal();
            if (Btn(T("Нанять случайного свободного", "Recruit a random free hero")))
            {
                var free = new List<HeroDefinition>();
                for (int i = 0; i < shown.Count; i++) if (!owned.Contains(shown[i].Name.ToString())) free.Add(shown[i]);
                if (free.Count == 0) Say(T("Свободных героев этого типа нет", "No free heroes of this kind"));
                else Recruit(player, free[UnityEngine.Random.Range(0, free.Count)]);
            }
            GUILayout.Label(T("  показано: ", "  shown: ") + shown.Count, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            recruitScroll = GUILayout.BeginScrollView(recruitScroll, GUILayout.Height(220));
            for (int i = 0; i < shown.Count; i++)
            {
                var d = shown[i];
                bool isOwned = owned.Contains(d.Name.ToString());
                GUILayout.BeginHorizontal();
                int ki = KindIndexOf(d);
                GUILayout.Label(d.LocalizedName + "  [" + (ki == 1 ? kinds[1] : ki == 2 ? kinds[2] : ki == 3 ? kinds[3] : "?") + ", " + ClassName(d) + "]", GUILayout.Width(430));
                if (isOwned) GUILayout.Label(T("уже в игре", "already in play"));
                else if (Btn(T("Нанять", "Recruit"))) Recruit(player, d);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            if (status.Length > 0) GUILayout.Label(status);
        }
        catch (Exception e) { GUILayout.Label("Hero debug: " + e.Message); }
    }

    static void Level(Hero hero, int n, Game game)
    {
        int gained = HeroRework.DebugAddLevels(hero, n);
        Say(hero.LocalizedName + ": " + (gained > 0 ? "+" + gained + T(" ур., теперь ", " lvl, now ") + hero.Level : T("уровень не изменился (достигнут максимум?)", "level unchanged (maximum reached?)")));
    }

    static void DrawGauges(MajorEmpire player, DepartmentOfEducation edu)
    {
        GUILayout.Label(T("Шкала героев (так игрок получает героев в обычной игре):", "Hero gauge (how a player normally gets heroes):"));
        int pending = edu.PendingHeroChoices == null ? 0 : edu.PendingHeroChoices.Count;
        var gauges = edu.HeroUnlockGauges;
        GUILayout.Label("  " + T("ожидающих выборов героя: ", "pending hero choices: ") + pending
            + T("   найм открыт: ", "   recruitment unlocked: ") + (edu.HeroRecruitmentUnlocked ? T("да", "yes") : T("нет", "no")));
        if (gauges == null) return;
        for (int g = 0; g < gauges.Length; g++)
        {
            var gauge = gauges[g];
            if (gauge == null) continue;
            GUILayout.Label("  " + T("Шкала #", "Gauge #") + g + ": " + gauge.GetPoints().ToString("0.#") + T(" очков", " points") + (gauge.IsOnHold ? T(" (на паузе)", " (on hold)") : ""));
            if (gauge.Classes == null) continue;
            for (int c = 0; c < gauge.Classes.Length; c++)
            {
                var cls = gauge.Classes[c];
                if (cls == null) continue;
                GUILayout.BeginHorizontal();
                GUILayout.Label("    " + cls.UnlockedClass.ToString() + ": " + cls.GetPointsStock().ToString("0.#"), GUILayout.Width(300));
                if (Btn("+100")) AddGaugePoints(player, cls, 100f);
                if (Btn("+1000")) AddGaugePoints(player, cls, 1000f);
                if (Btn("+10000")) AddGaugePoints(player, cls, 10000f);
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.Label("  " + T("Когда шкала заполнится, на конце хода появится выбор героя класса с наибольшим счётом.", "When the gauge fills, a hero choice of the leading class appears at the end of the turn."));
    }

    static void AddGaugePoints(MajorEmpire player, HeroUnlockClassGauge cls, float points)
    {
        try
        {
            var pc = player.PlayerControllers;
            if (pc == null || pc.Client == null) return;
            pc.Client.PostOrder(new OrderAddHeroUnlockGaugeClassPoints(player.Index, cls.HeroUnlockPointsResourceName, points));
            Say(T("Очки шкалы отправлены: +", "Gauge points sent: +") + points);
        }
        catch (Exception e) { Say("gauge: " + e.Message); }
    }

    static HashSet<string> OwnedDefinitions(Game game)
    {
        var set = new HashSet<string>();
        foreach (MajorEmpire e in game.EnumerableMajorEmpires)
        {
            var edu = e.GetAgency<DepartmentOfEducation>();
            if (edu == null) continue;
            for (int i = 0; i < edu.ActiveHeroes.Count; i++)
                if (edu.ActiveHeroes[i] != null && edu.ActiveHeroes[i].Definition != null) set.Add(edu.ActiveHeroes[i].Definition.Name.ToString());
        }
        return set;
    }

    static string ClassName(HeroDefinition d)
    {
        try { return d.Class != null ? d.Class.Name.ToString().Replace("HeroClass", "") : "?"; } catch (Exception) { return "?"; }
    }

    /// <summary>1 admiral, 2 governor, 3 senator, 0 unknown class.</summary>
    static int KindIndexOf(HeroDefinition d)
    {
        try
        {
            HeroKind kind;
            if (d.Class != null && HeroKinds.TryKindOfClass(d.Class.Name.ToString(), out kind))
                return kind == HeroKind.Admiral ? 1 : kind == HeroKind.Governor ? 2 : 3;
        }
        catch (Exception) { }
        return 0;
    }

    static void Recruit(MajorEmpire player, HeroDefinition def)
    {
        try
        {
            var pc = player.PlayerControllers;
            if (pc == null || pc.Client == null) { Say(T("Нет клиента игрока", "No player client")); return; }
            var order = new OrderRecruitHero(player.Index, def);
            order.HeroLevel = recruitLevel;
            order.GaugeIndex = -1;
            pc.Client.PostOrder(order);
            Say(T("Заказ найма отправлен: ", "Recruit order sent: ") + def.LocalizedName + T(", ур. ", ", lvl ") + recruitLevel);
        }
        catch (Exception e) { Say("recruit: " + e.Message); }
    }
}
