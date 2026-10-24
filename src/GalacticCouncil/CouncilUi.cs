// Galactic Council of the Academy - IMGUI window (no Unity prefabs/assets are needed).
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Amplitude.Unity.Game;

public class CouncilUiBehaviour : MonoBehaviour
{
    void OnGUI()
    {
        try { CouncilUi.Draw(); } catch (Exception e) { CouncilUi.DrawFailed(e); }
    }
}

public static class CouncilUi
{
    public static bool Visible;
    static int tab;                       // 0 = vote, 1 = laws, 2 = mandates
    static Rect windowRect = new Rect(0, 0, 1000, 660);
    static bool rectInit;
    static GameObject host;
    static string toast = "";
    static float toastUntil;
    static int shownVoteIndex = -1;
    static Dictionary<int, int> pending = new Dictionary<int, int>();   // key = question*1000 + option
    static Vector2 scroll;
    static string uiError = "";
    static GUIStyle titleStyle, boxStyle, smallStyle, wrapStyle, goodStyle, badStyle;

    // ------------------------------------------------------------ hooks

    public static void EnsureHost()
    {
        try
        {
            if (host != null) return;
            var found = GameObject.Find("__ES2GalacticCouncilUI__");
            if (found == null)
            {
                found = new GameObject("__ES2GalacticCouncilUI__");
                UnityEngine.Object.DontDestroyOnLoad(found);
            }
            if (found.GetComponent<CouncilUiBehaviour>() == null) found.AddComponent<CouncilUiBehaviour>();
            host = found;
        }
        catch (Exception) { }
    }

    public static void Toast(string text)
    {
        toast = text;
        toastUntil = Time.realtimeSinceStartup + 12f;
    }

    public static void OnVoteOpened(int expiredLaws)
    {
        var c = CouncilGame.Core;
        Toast(CouncilGame.Tr("Галактический совет: открыто голосование. Итоги будут подведены в начале следующего хода.",
                              "Galactic Council: voting is open. Results will be counted at the start of next turn."));
        // pop the window up once for human players
        var me = CouncilGame.LocalEmpire();
        if (me != null && c.IsMember(me.Index)) { Visible = true; tab = 0; }
    }

    public static void OnVoteTallied(int enacted)
    {
        Toast(CouncilGame.Tr("Галактический совет: подведены итоги голосования (принято законов: " + enacted + ").",
                              "Galactic Council: votes counted (laws enacted: " + enacted + ")."));
    }

    /// <summary>Called from AcademyModalWindow.OnBeginShow so the existing Academy button opens the council.</summary>
    public static void OnAcademyWindowShown()
    {
        try { if (CouncilGame.Core.Enabled) { Visible = true; } } catch (Exception) { }
    }

    public static void OnAcademyWindowHidden()
    {
        try { Visible = false; } catch (Exception) { }
    }

    public static void DrawFailed(Exception e)
    {
        uiError = e.GetType().Name + ": " + e.Message;
    }

    // ------------------------------------------------------------ drawing

    static void InitStyles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, wordWrap = true };
        boxStyle = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, padding = new RectOffset(8, 8, 8, 8) };
        smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, wordWrap = true };
        wrapStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
        goodStyle = new GUIStyle(wrapStyle); goodStyle.normal.textColor = new Color(0.55f, 1f, 0.55f);
        badStyle = new GUIStyle(wrapStyle); badStyle.normal.textColor = new Color(1f, 0.6f, 0.5f);
    }

    public static void Draw()
    {
        var c = CouncilGame.Core;
        if (!c.Enabled) return;
        var me = CouncilGame.LocalEmpire();
        if (me == null) return;
        InitStyles();

        float sc = Mathf.Max(1f, Screen.height / 1080f);
        var oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(sc, sc, 1f));
        float sw = Screen.width / sc, sh = Screen.height / sc;
        if (!rectInit) { windowRect = new Rect((sw - 1000) / 2, (sh - 660) / 2, 1000, 660); rectInit = true; }

        // small always-visible launcher
        bool voting = c.Phase == CouncilCore.PhaseVoting && c.IsMember(me.Index) && !c.HasVoted(me.Index);
        string label = CouncilGame.Tr("Галактический совет", "Galactic Council") + (voting ? "  (!)" : "");
        var oldColor = GUI.backgroundColor;
        if (voting) GUI.backgroundColor = new Color(1f, 0.8f, 0.2f);
        if (GUI.Button(new Rect(sw - 222, 4, 210, 26), label)) Visible = !Visible;
        GUI.backgroundColor = oldColor;

        if (Time.realtimeSinceStartup < toastUntil && !string.IsNullOrEmpty(toast))
        {
            var r = new Rect((sw - 640) / 2, 40, 640, 34);
            GUI.Box(r, toast);
        }

        if (Visible)
        {
            if (shownVoteIndex != c.VoteIndex) { shownVoteIndex = c.VoteIndex; LoadPendingFromCore(me.Index); }
            windowRect = GUI.Window(0x6C0DE, windowRect, delegate (int id) { DrawWindow(me); }, CouncilGame.Tr("Галактический совет Академии", "Galactic Council of the Academy"));
        }
        GUI.matrix = oldMatrix;
    }

    static void LoadPendingFromCore(int emp)
    {
        pending.Clear();
        var c = CouncilGame.Core;
        for (int q = 0; q < c.Proposals.Count; q++)
        {
            for (int opt = 0; opt < 16; opt++)
            {
                int v = c.GetVotes(emp, q, opt);
                if (v > 0) pending[q * 1000 + opt] = v;
            }
        }
        for (int m = 0; m < CouncilLaws.Mandates.Count; m++)
        {
            int v = c.GetVotes(emp, CouncilCore.MandateQBase + m, 0);
            if (v > 0) pending[(CouncilCore.MandateQBase + m) * 1000] = v;
        }
    }

    static int PendingTotal() { int s = 0; foreach (var kv in pending) s += kv.Value; return s; }
    static int Pend(int q, int opt) { int v; return pending.TryGetValue(q * 1000 + opt, out v) ? v : 0; }

    static void DrawWindow(Empire me)
    {
        var c = CouncilGame.Core;
        GUILayout.BeginHorizontal();
        string[] tabs = { CouncilGame.Tr("Голосование", "Voting"), CouncilGame.Tr("Законы", "Laws"), CouncilGame.Tr("Мандаты", "Mandates") };
        for (int i = 0; i < tabs.Length; i++)
        {
            bool on = tab == i;
            if (GUILayout.Toggle(on, tabs[i], GUI.skin.button, GUILayout.Height(28)) && !on) tab = i;
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("X", GUILayout.Width(32), GUILayout.Height(28))) Visible = false;
        GUILayout.EndHorizontal();
        GUILayout.Space(6);

        if (!c.IsMember(me.Index))
            GUILayout.Label(CouncilGame.Tr("Ваша империя не состоит в Совете.", "Your empire is not a council member."), badStyle);

        if (tab == 0) DrawVoteTab(me);
        else if (tab == 1) DrawLawsTab(me);
        else DrawMandatesTab(me);

        if (!string.IsNullOrEmpty(uiError)) GUILayout.Label("UI error: " + uiError, badStyle);
        GUI.DragWindow(new Rect(0, 0, 10000, 22));
    }

    static void DrawVoteTab(Empire me)
    {
        var c = CouncilGame.Core;
        var game = CouncilGame.GetGame();
        int turn = game != null ? game.Turn : 0;

        if (c.Phase != CouncilCore.PhaseVoting)
        {
            GUILayout.Label(CouncilGame.Tr("Сейчас голосование не проводится.", "No vote is in progress."), titleStyle);
            GUILayout.Label(CouncilGame.Tr("Следующее голосование откроется через " + c.TurnsToNextVote(turn) + " ход. (ход " + c.NextVoteTurn + ")",
                                           "Next vote opens in " + c.TurnsToNextVote(turn) + " turn(s) (turn " + c.NextVoteTurn + ")."), wrapStyle);
            GUILayout.Space(8);
            DrawHistory();
            return;
        }

        int budget = c.Budget(me.Index);
        int left = budget - PendingTotal();
        GUILayout.Label(CouncilGame.Tr("Голосование открыто. Итоги будут подведены в начале следующего хода; законы действуют до следующего голосования.",
            "Voting is open. Results are counted at the start of next turn; laws last until the next vote."), wrapStyle);
        GUILayout.Label(CouncilGame.Tr("Ваши голоса: ", "Your votes: ") + budget + "  (" + c.BudgetText(me.Index, CouncilGame.CurTurn(), CouncilGame.IsRussian()) + ")", smallStyle);

        GUILayout.BeginHorizontal();
        for (int q = 0; q < c.Proposals.Count; q++)
        {
            var d = CouncilLaws.Get(c.Proposals[q]);
            if (d == null) continue;
            GUILayout.BeginVertical(boxStyle, GUILayout.Width(310), GUILayout.MinHeight(380));
            GUILayout.Label((q + 1) + ". " + CouncilGame.LawName(d), titleStyle);
            GUILayout.Label(d.Targeted ? CouncilGame.Tr("Закон против/для одной империи — голосуйте за империю", "Aimed at one empire - vote for an empire")
                                       : CouncilGame.Tr("Закон для всех — «за» или «против»", "Applies to everyone - vote for or against"), smallStyle);
            GUILayout.Label(CouncilGame.LawDesc(d), wrapStyle);
            GUILayout.Space(6);
            if (d.Kind == CouncilLawDef.KindMandate)
            {
                GUILayout.Label(CouncilGame.Tr("Выберите империю, которая получит мандат (из соответствующих требованиям). Мандат действует до переизбрания, выхода из Совета или уничтожения Академии.",
                                               "Choose the empire that gets the mandate (among those that qualify). It lasts until re-election, leaving the Council or the Academy's destruction."), smallStyle);
                for (int i = 0; i < c.MandateCandidates.Count; i++)
                {
                    int e = c.MandateCandidates[i];
                    DrawVoteRow(q, e, CouncilGame.EmpireName(e) + (e == me.Index ? CouncilGame.Tr(" (вы)", " (you)") : ""), ref left);
                }
            }
            else if (!d.Targeted)
            {
                DrawVoteRow(q, 0, CouncilGame.Tr("Принять закон", "Pass"), ref left);
                DrawVoteRow(q, 1, CouncilGame.Tr("Не принимать", "Reject"), ref left);
            }
            else
            {
                for (int i = 0; i < c.Members.Count; i++)
                {
                    int e = c.Members[i];
                    string nm = CouncilGame.EmpireName(e) + (e == me.Index ? CouncilGame.Tr(" (вы)", " (you)") : "");
                    DrawVoteRow(q, e, nm, ref left);
                }
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();

        DrawChairmanPanel(me);

        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.Label(CouncilGame.Tr("Осталось голосов: ", "Votes left: ") + left + " / " + budget, titleStyle, GUILayout.Width(300));
        GUILayout.FlexibleSpace();
        bool member = c.IsMember(me.Index);
        GUI.enabled = member;
        if (GUILayout.Button(CouncilGame.Tr("Сбросить", "Reset"), GUILayout.Width(120), GUILayout.Height(30))) pending.Clear();
        if (GUILayout.Button(CouncilGame.Tr("Отдать голоса", "Cast votes"), GUILayout.Width(170), GUILayout.Height(30)))
        {
            var entries = new List<int[]>();
            foreach (var kv in pending) if (kv.Value > 0) entries.Add(new[] { kv.Key / 1000, kv.Key % 1000, kv.Value });
            if (CouncilGame.CastBallot(me, entries)) Toast(CouncilGame.Tr("Ваши голоса учтены", "Your votes are recorded"));
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        if (c.HasVoted(me.Index))
            GUILayout.Label(CouncilGame.Tr("Ваши голоса приняты (потрачено " + c.Spent(me.Index) + "). Можно изменить до конца хода.",
                                           "Your votes are in (" + c.Spent(me.Index) + " spent). You can change them until the turn ends."), goodStyle);
    }

    static int chairQ = -1;
    static Vector2 chairScroll;

    /// <summary>Chairman of the Council: once per cycle replace one ordinary question before anybody has voted.</summary>
    static void DrawChairmanPanel(Empire me)
    {
        var c = CouncilGame.Core;
        if (!c.Holds(me.Index, CouncilLaws.Chairman)) return;
        GUILayout.BeginVertical(boxStyle);
        if (!c.CanChairmanReplace(me.Index))
        {
            GUILayout.Label(CouncilGame.Tr("Председатель Совета: право замены вопроса уже использовано или голосование началось.",
                                           "Council Chairman: the replacement right is used up or voting has already started."), smallStyle);
            GUILayout.EndVertical();
            return;
        }
        GUILayout.Label(CouncilGame.Tr("Председатель Совета: можно один раз заменить обычный вопрос другим (пока никто не проголосовал).",
                                       "Council Chairman: you may replace one ordinary question once (until anybody has voted)."), smallStyle);
        GUILayout.BeginHorizontal();
        for (int q = 0; q < c.Proposals.Count; q++)
        {
            var d = CouncilLaws.Get(c.Proposals[q]);
            if (d == null || d.Kind == CouncilLawDef.KindMandate || d.Id == CouncilLaws.NoConfidence) continue;
            bool on = chairQ == q;
            if (GUILayout.Toggle(on, CouncilGame.Tr("Заменить вопрос ", "Replace question ") + (q + 1), GUI.skin.button, GUILayout.Width(180)) && !on) chairQ = q;
        }
        GUILayout.EndHorizontal();
        if (chairQ >= 0 && chairQ < c.Proposals.Count)
        {
            chairScroll = GUILayout.BeginScrollView(chairScroll, GUILayout.Height(90));
            for (int i = 0; i < CouncilLaws.All.Count; i++)
            {
                var nd = CouncilLaws.All[i];
                if (!nd.Implemented || nd.Kind == CouncilLawDef.KindMandate || nd.Id == CouncilLaws.NoConfidence) continue;
                if (c.Proposals.Contains(nd.Id)) continue;
                if (GUILayout.Button(CouncilGame.LawName(nd) + (nd.Targeted ? CouncilGame.Tr("  [империя]", "  [empire]") : "")))
                {
                    CouncilGame.SendKind(me, OrderCouncilVote.KindChairmanReplace, chairQ, i);
                    chairQ = -1;
                    break;
                }
            }
            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();
    }

    static void DrawVoteRow(int q, int opt, string label, ref int left)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, wrapStyle, GUILayout.Width(170));
        int v = Pend(q, opt);
        GUI.enabled = v > 0;
        if (GUILayout.Button("-", GUILayout.Width(28))) { v -= CouncilCore.VoteStep; SetPend(q, opt, v); left += CouncilCore.VoteStep; }
        GUI.enabled = true;
        GUILayout.Label(v.ToString(), wrapStyle, GUILayout.Width(40));
        GUI.enabled = left >= CouncilCore.VoteStep;
        if (GUILayout.Button("+", GUILayout.Width(28))) { v += CouncilCore.VoteStep; SetPend(q, opt, v); left -= CouncilCore.VoteStep; }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
    }

    static void SetPend(int q, int opt, int v)
    {
        if (v <= 0) pending.Remove(q * 1000 + opt); else pending[q * 1000 + opt] = v;
    }

    static string MandateName(string id)
    {
        var m = CouncilLaws.GetMandate(id);
        return m == null ? id : (CouncilGame.IsRussian() ? m.Ru : m.En);
    }

    static string DescribeResult(CouncilResult r)
    {
        if (r.LawId.StartsWith("MV:"))
            return "T" + r.Turn + ": " + CouncilGame.Tr("Мандат на следующее голосование: ", "Mandate on the next vote: ") + MandateName(r.LawId.Substring(3)) + " (" + r.Yes + ")";
        if (r.LawId.StartsWith("CH:"))
        {
            string[] ids = r.LawId.Substring(3).Split('>');
            var o = ids.Length > 1 ? CouncilLaws.Get(ids[0]) : null;
            var n = ids.Length > 1 ? CouncilLaws.Get(ids[1]) : null;
            return "T" + r.Turn + ": " + CouncilGame.Tr("Председатель (", "Chairman (") + CouncilGame.EmpireName(r.Target) + CouncilGame.Tr(") заменил «", ") replaced '") +
                (o != null ? CouncilGame.LawName(o) : "?") + CouncilGame.Tr("» на «", "' with '") + (n != null ? CouncilGame.LawName(n) : "?") + "'";
        }
        var d = CouncilLaws.Get(r.LawId);
        string name = d != null ? CouncilGame.LawName(d) : r.LawId;
        if (d != null && !d.Targeted)
            return "T" + r.Turn + ": " + name + " — " + (r.Passed ? CouncilGame.Tr("принят", "passed") : CouncilGame.Tr("отклонён", "rejected")) + " (" + r.Yes + " : " + r.No + ")";
        string tgt = r.Passed ? CouncilGame.EmpireName(r.Target) : CouncilGame.Tr("не выбрана (ничья или нет голосов)", "none (tie or no votes)");
        return "T" + r.Turn + ": " + name + " → " + tgt + " (" + r.Yes + " / " + r.No + ")";
    }

    static void DrawHistory()
    {
        var c = CouncilGame.Core;
        GUILayout.Label(CouncilGame.Tr("Итоги прошлых голосований", "Previous results"), titleStyle);
        scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(380));
        for (int i = c.History.Count - 1; i >= 0; i--)
            GUILayout.Label(DescribeResult(c.History[i]), c.History[i].Passed ? goodStyle : badStyle);
        if (c.History.Count == 0) GUILayout.Label(CouncilGame.Tr("Пока нет.", "Nothing yet."), wrapStyle);
        GUILayout.EndScrollView();
    }

    static void DrawLawsTab(Empire me)
    {
        var c = CouncilGame.Core;
        GUILayout.Label(CouncilGame.Tr("Действующие законы", "Laws in force"), titleStyle);
        if (c.Active.Count == 0) GUILayout.Label(CouncilGame.Tr("Нет действующих законов.", "No laws in force."), wrapStyle);
        for (int i = 0; i < c.Active.Count; i++)
        {
            var a = c.Active[i];
            var d = CouncilLaws.Get(a.Id);
            if (d == null) continue;
            string who = a.Target >= 0 ? " → " + CouncilGame.EmpireName(a.Target) : "";
            GUILayout.Label("• " + CouncilGame.LawName(d) + who, goodStyle);
            GUILayout.Label("   " + CouncilGame.LawDesc(d), smallStyle);
        }
        GUILayout.Space(8);
        GUILayout.Label(CouncilGame.Tr("Постоянные эффекты", "Permanent effects"), titleStyle);
        bool any = false;
        for (int i = 0; i < c.Members.Count; i++)
        {
            int e = c.Members[i];
            int vb = c.Budget(e) - CouncilCore.BaseVotes, cd = c.GetColonyDelta(e);
            if (vb == 0 && cd == 0) continue;
            any = true;
            GUILayout.Label("• " + CouncilGame.EmpireName(e) + ": " + CouncilGame.Tr("голосов ", "votes ") + c.Budget(e) +
                (cd != 0 ? CouncilGame.Tr(", колонизационный лимит ", ", colonization limit ") + (cd > 0 ? "+" : "") + cd : ""), wrapStyle);
        }
        if (!any) GUILayout.Label(CouncilGame.Tr("Нет.", "None."), wrapStyle);
        var marks = CouncilGame.PresenceLines();
        if (marks.Count > 0)
        {
            GUILayout.Space(8);
            GUILayout.Label(CouncilGame.Tr("Отмеченные системы (стратегическое присутствие)", "Marked systems (Strategic Presence)"), titleStyle);
            GUILayout.Label(CouncilGame.Tr("На следующем заседании каждая удерживаемая отмеченная система даёт +10 голосов (максимум +50).",
                "At the next session every held marked system gives +10 votes (max +50)."), smallStyle);
            for (int i = 0; i < marks.Count; i++) GUILayout.Label("• " + marks[i], wrapStyle);
        }
        GUILayout.Space(8);
        DrawHistory();
    }

    static bool confirmLeave;

    static void DrawMandatesTab(Empire me)
    {
        var c = CouncilGame.Core;
        var game = CouncilGame.GetGame();
        int turn = game != null ? game.Turn : 0;
        int budget = c.Budget(me.Index);
        int left = budget - PendingTotal();
        bool member = c.IsMember(me.Index);
        bool voting = c.Phase == CouncilCore.PhaseVoting && member;

        GUILayout.Label(CouncilGame.Tr("Мандаты Совета", "Council mandates"), titleStyle);
        GUILayout.Label(CouncilGame.Tr(
            "Голоса за мандат отдаются здесь, из того же запаса голосов. Мандат с наибольшим числом голосов попадёт в следующее голосование вместо одного закона; там выбирается его владелец. Мандат действует до переизбрания, выхода владельца из Совета или уничтожения Академии.",
            "Votes for a mandate are cast here from the same pool of votes. The mandate with the most votes goes onto the next vote instead of one law, where its holder is chosen. A mandate lasts until re-election, the holder leaving the Council or the Academy's destruction."), smallStyle);
        if (!string.IsNullOrEmpty(c.PendingMandate))
            GUILayout.Label(CouncilGame.Tr("На следующем голосовании: ", "On the next vote: ") + MandateName(c.PendingMandate), goodStyle);
        int vl = c.VictoryTurnsLeft(turn);
        if (vl >= 0)
            GUILayout.Label(CouncilGame.Tr("Верховный легат: ", "Supreme Legate: ") + CouncilGame.EmpireName(c.SupremeHolder) +
                CouncilGame.Tr(" — до победы осталось ходов: ", " - turns to victory: ") + vl, vl <= 5 ? badStyle : goodStyle);
        if (voting) GUILayout.Label(CouncilGame.Tr("Осталось голосов: ", "Votes left: ") + left + " / " + budget, wrapStyle);

        scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(400));
        for (int m = 0; m < CouncilLaws.Mandates.Count; m++)
        {
            var d = CouncilLaws.Mandates[m];
            GUILayout.BeginVertical(boxStyle);
            int holder = c.HolderOf(d.Id);
            GUILayout.Label((CouncilGame.IsRussian() ? d.Ru : d.En) + "   [" + (holder >= 0 ? CouncilGame.EmpireName(holder) : CouncilGame.Tr("не назначен", "vacant")) + "]", titleStyle);
            GUILayout.Label(CouncilGame.IsRussian() ? d.RuDesc : d.EnDesc, smallStyle);
            GUILayout.Label(d.Victory ? CouncilGame.Tr("Голоса: ×2 и ещё +" + CouncilCore.SupremeMandateVotes, "Votes: x2 and +" + CouncilCore.SupremeMandateVotes + " more")
                                      : CouncilGame.Tr("Голоса: +" + CouncilCore.MandateVotes + " владельцу", "Votes: +" + CouncilCore.MandateVotes + " to the holder"), smallStyle);
            bool q = member && c.Qualifies(me.Index, d.Id);
            string prog = d.Victory ? "" : "  (" + CouncilGame.ProgressText(me.Index, d) + ")";
            GUILayout.Label(CouncilGame.Tr("Требование: ", "Requirement: ") + (CouncilGame.IsRussian() ? d.RuNeed : d.EnNeed) + prog + " — " +
                            (q ? CouncilGame.Tr("выполнено", "met") : CouncilGame.Tr("не выполнено", "not met")), q ? goodStyle : smallStyle);
            if (voting && q)
                DrawVoteRow(CouncilCore.MandateQBase + m, 0, CouncilGame.Tr("Голосов за мандат", "Votes for mandate"), ref left);

            if (c.Holds(me.Index, d.Id))
            {
                if (d.Id == CouncilLaws.Chairman)
                    GUILayout.Label(CouncilGame.Tr("Ваше право: заменить вопрос — на вкладке «Голосование».", "Your power: replace a question - see the Voting tab."), goodStyle);
                else if (d.Id == CouncilLaws.Marshal)
                {
                    if (c.CanHireMarshal(me.Index))
                    {
                        if (GUILayout.Button(CouncilGame.Tr("Нанять контингент Академии (содержание за ваш счёт)", "Hire the Academy contingent (you pay the upkeep)"), GUILayout.Height(26)))
                            CouncilGame.SendKind(me, OrderCouncilVote.KindMarshalHire, 0, 0);
                    }
                    else GUILayout.Label(CouncilGame.Tr("Контингент уже нанят на этот срок мандата.", "The contingent is already hired for this term."), smallStyle);
                }
                else if (d.Id == CouncilLaws.Quartermaster)
                {
                    if (c.CanUseQuartermaster(me.Index, turn))
                    {
                        if (GUILayout.Button(CouncilGame.Tr("Запросить поставку у Академии", "Request a delivery from the Academy"), GUILayout.Height(26)))
                            CouncilGame.SendKind(me, OrderCouncilVote.KindQuartermaster, 0, 0);
                    }
                    else GUILayout.Label(CouncilGame.Tr("Следующая поставка через ходов: ", "Next delivery in turns: ") + c.TurnsToQuartermaster(me.Index, turn), smallStyle);
                }
                else if (d.Id == CouncilLaws.Eye)
                    GUILayout.Label(CouncilGame.Tr("Действует: +1 одновременная операция взлома, +20 к лимиту вычислительной мощности.", "Active: +1 simultaneous hacking operation, +20 processing-power cap."), goodStyle);
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndScrollView();


        if (member)
        {
            GUILayout.BeginHorizontal();
            if (!confirmLeave)
            {
                if (GUILayout.Button(CouncilGame.Tr("Выйти из Совета…", "Leave the Council..."), GUILayout.Width(220))) confirmLeave = true;
            }
            else
            {
                GUILayout.Label(CouncilGame.Tr("Выход необратим: Академия объявит вам войну, мандаты будут потеряны.", "Leaving is irreversible: the Academy declares war on you and your mandates are lost."), badStyle);
                if (GUILayout.Button(CouncilGame.Tr("Подтвердить выход", "Confirm"), GUILayout.Width(180))) { CouncilGame.SendKind(me, OrderCouncilVote.KindLeave, 0, 0); confirmLeave = false; }
                if (GUILayout.Button(CouncilGame.Tr("Отмена", "Cancel"), GUILayout.Width(100))) confirmLeave = false;
            }
            GUILayout.EndHorizontal();
        }
        else if (c.IsOutcast(me.Index))
            GUILayout.Label(CouncilGame.Tr("Вы покинули Совет: Академия находится с вами в состоянии войны.", "You left the Council: the Academy is at war with you."), badStyle);
    }
}
