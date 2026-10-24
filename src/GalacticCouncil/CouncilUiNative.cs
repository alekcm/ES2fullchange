// Galactic Council - native Endless Space 2 window (AGE UI).
//
// The window is a runtime clone of the game's own laws screen (LawsManagementModalWindow):
// the whole visual prefab hierarchy (modal frame, tabs, card table, details pane, buttons, sounds,
// animations) is cloned with UnityEngine.Object.Instantiate, the window component is swapped for
// CouncilModalWindow and the widget references are copied from the clone before the original
// component is destroyed. The window is registered in the GuiManager window list, so ESC/RightClick
// closing, the modal dimming and the input blocking behave exactly like for a vanilla window.
// (The few non-public GuiWindow members needed for that - Initialize, Loaded, GameReady and the
// manager's window list - are touched through reflection: they live in Assembly-CSharp-firstpass
// which the patcher does not publicize.)
//
// Voting interaction (native style): the card list holds one card per question (and option cards
// under the selected targeted/mandate question); the details pane shows the description and two
// context buttons. Left click on a button adds 10 votes, right click removes 10 (see tooltips).
// Toasts go through the game's MessageBoxNonBlockingWindow, confirmations through ShowMessage.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Gui;

/// <summary>The Council window: a GuiModalWindow driving the cloned laws-screen widgets.</summary>
public class CouncilModalWindow : GuiModalWindow
{
    // ---- widget references, copied from the cloned LawsManagementModalWindow in Setup ----
    public AgeTransform TabsTable;             // LawFiltersTable   - horizontal tab strip
    public AgeTransform CardsTable;            // LawCardsTable     - card list
    public Transform TabPrefab;                // LawFilterPrefab   - one tab toggle
    public Transform CardPrefab;               // LawCardPrefab     - one card
    public AgePrimitiveLabel TopLeftLabel;     // VotedLawSlotsLabel    - budget / status line
    public AgePrimitiveLabel TopRightLabel;    // CurrentPrestigeLabel  - next vote line
    public AgeTransform DetailsGroup;          // LawDetails        - right pane, hidden without selection
    public AgePrimitiveLabel DetailsTitle;     // LawTitle
    public AgePrimitiveLabel DetailsSub;       // LawShortTitle     - subtitle line
    public AgePrimitiveLabel DetailsText;      // LawDescription    - body text (word wrap)
    public AgePrimitiveLabel CounterLabel;     // InfluenceCostLabel - vote counter line
    public AgePrimitiveImage DetailsIcon;      // LawIcon           - hidden
    public AgePrimitiveImage PoliticsIcon;     // PoliticsIcon      - hidden
    public AgeControlButton ActionButton;      // VoteButton
    public AgeControlButton SecondButton;      // AbrogateButton
    public AgeTooltip ActionTooltip;           // VoteButtonTooltip
    public Component EffectsFeature;           // PanelFeatureEffects       - hidden
    public Component UpkeepFeature;            // PanelFeatureLawUpkeep     - hidden
    public Component ExperienceFeature;        // PanelFeaturePoliticsExperiencePrerequisite - hidden

    // ---- state ----
    public bool Ready;
    public int Tab;                            // 0 vote, 1 laws, 2 mandates, 3 history
    public int SelCard;
    public int ChairQ = -1;                    // chairman flow: question picked for replacement
    public readonly Dictionary<int, int> SelOption = new Dictionary<int, int>();   // question -> option

    CouncilNativeCallbacks callbacks;
    readonly List<NativeCard> cards = new List<NativeCard>();
    readonly Dictionary<GameObject, int> cardIndex = new Dictionary<GameObject, int>();   // toggle GO -> card index
    readonly List<AgeControlToggle> tabToggles = new List<AgeControlToggle>();
    readonly Dictionary<AgeControlButton, AgePrimitiveLabel> buttonLabels = new Dictionary<AgeControlButton, AgePrimitiveLabel>();

    // ------------------------------------------------------------------ GuiWindow plumbing

    public override IEnumerator Load() { yield break; }

    // OnGameCreated/OnGameReleasing are 'protected internal' in firstpass (not publicized):
    // from a separate assembly they must be overridden as plain 'protected'.
    protected override IEnumerator OnGameCreated() { yield break; }

    protected override void OnGameReleasing()
    {
        base.OnGameReleasing();
        try
        {
            // the content references game data; it is rebuilt on the next show
            cards.Clear();
            cardIndex.Clear();
            ChairQ = -1;
            SelCard = 0;
            CouncilUi.ClearPending();
        }
        catch (Exception) { }
    }

    public override void OnBeginShow(bool instant)
    {
        base.OnBeginShow(instant);
        try { Rebuild(); } catch (Exception e) { CouncilNative.NoteFailure("OnBeginShow", e); }
    }

    // ------------------------------------------------------------------ setup

    /// <summary>Called by CouncilNative.EnsureWindow once, right after the clone was created
    /// and its AGE graph was initialized.</summary>
    public void Setup(LawsManagementModalWindow src)
    {
        TabsTable = src.LawFiltersTable;
        CardsTable = src.LawCardsTable;
        TabPrefab = src.LawFilterPrefab;
        CardPrefab = src.LawCardPrefab;
        TopLeftLabel = src.VotedLawSlotsLabel;
        TopRightLabel = src.CurrentPrestigeLabel;
        DetailsGroup = src.LawDetails;
        DetailsTitle = src.LawTitle;
        DetailsSub = src.LawShortTitle;
        DetailsText = src.LawDescription;
        CounterLabel = src.InfluenceCostLabel;
        DetailsIcon = src.LawIcon;
        PoliticsIcon = src.PoliticsIcon;
        ActionButton = src.VoteButton;
        SecondButton = src.AbrogateButton;
        ActionTooltip = src.VoteButtonTooltip;
        EffectsFeature = src.PanelFeatureEffects as Component;
        UpkeepFeature = src.PanelFeatureLawUpkeep as Component;
        ExperienceFeature = src.PanelFeatureExperience as Component;

        if (TabsTable == null || CardsTable == null || CardPrefab == null || TabPrefab == null ||
            DetailsGroup == null || DetailsTitle == null || DetailsText == null ||
            ActionButton == null || SecondButton == null)
            throw new Exception("laws-screen widgets missing on the clone");

        callbacks = gameObject.AddComponent<CouncilNativeCallbacks>();
        callbacks.Init(this);

        // our own tabs: drop the laws-screen filters and the radio group that drove them
        var radio = GetComponentInChildren<GuiRadioGroup>();
        if (radio != null) UnityEngine.Object.Destroy(radio);
        TabsTable.DestroyAllChildren();
        CardsTable.DestroyAllChildren();

        HideComponent(DetailsIcon);
        HideComponent(PoliticsIcon);
        HideComponent(EffectsFeature);
        HideComponent(UpkeepFeature);
        HideComponent(ExperienceFeature);

        // labels we drive must not run through the localization system
        PrepareLabel(TopLeftLabel); PrepareLabel(TopRightLabel); PrepareLabel(DetailsTitle);
        PrepareLabel(DetailsSub); PrepareLabel(DetailsText); PrepareLabel(CounterLabel);

        WireButton(ActionButton, "OnCouncilAction", "OnCouncilActionRight");
        WireButton(SecondButton, "OnCouncilSecond", "OnCouncilSecondRight");

        BuildTabs();
        Rebuild();
        Ready = true;
    }

    static void HideComponent(Component c)
    {
        if (c == null) return;
        var at = c.GetComponent<AgeTransform>();
        if (at != null) at.Visible = false;
    }

    static void PrepareLabel(AgePrimitiveLabel label)
    {
        if (label == null) return;
        label.DisableAutoLocalize = true;
        label.Text = string.Empty;
    }

    void WireButton(AgeControlButton b, string method, string rightMethod)
    {
        if (b == null) return;
        b.UseLeftClick = true;
        b.UseRightClick = true;
        b.UseDoubleClick = false;
        b.OnDoubleClickMethod = null;
        b.OnActivateObject = callbacks.gameObject;
        b.OnActivateMethod = method;
        b.OnRightClickObject = callbacks.gameObject;
        b.OnRightClickMethod = rightMethod;
        var l = ((Component)b).GetComponentInChildren<AgePrimitiveLabel>();
        if (l != null) { PrepareLabel(l); buttonLabels[b] = l; }
    }

    void SetButton(AgeControlButton b, NativeAction action, AgeTooltip tooltip)
    {
        if (b == null) return;
        bool on = action != null;
        b.Visible = on;
        b.Enable = on && action.Enabled;
        if (!on) return;
        AgePrimitiveLabel l;
        if (buttonLabels.TryGetValue(b, out l) && l != null) l.Text = action.Label;
        if (tooltip != null) tooltip.Content = action.Tip ?? string.Empty;
    }

    void BuildTabs()
    {
        string[] names = CouncilNative.TabNames();
        for (int i = 0; i < names.Length; i++)
        {
            var filter = TabsTable.InstantiateChild<LawFilter>(TabPrefab, "CouncilTab" + i.ToString("D3"));
            if (filter == null) throw new Exception("LawFilterPrefab has no LawFilter component");
            PrepareLabel(filter.TitleLabel);
            SetLabel(filter.TitleLabel, names[i]);
            if (filter.Tooltip != null) filter.Tooltip.Content = string.Empty;
            var toggle = filter.Toggle;
            if (toggle == null) throw new Exception("LawFilterPrefab has no toggle");
            toggle.UseDoubleClick = false;
            toggle.OnSwitchObject = callbacks.gameObject;
            toggle.OnSwitchMethod = "OnCouncilTab";
            toggle.OnRightClickMethod = null;
            tabToggles.Add(toggle);
        }
        TabsTable.ArrangeChildren(false);
    }

    // ------------------------------------------------------------------ rebuild (all content)

    public void Rebuild()
    {
        if (TabsTable == null) return;
        var c = CouncilGame.Core;
        var me = CouncilGame.LocalEmpire();

        cards.Clear();
        if (me != null)
        {
            if (Tab == 0) BuildVoteCards(me, c);
            else if (Tab == 1) BuildLawsCards(me, c);
            else if (Tab == 2) BuildMandateCards(me, c);
            else BuildHistoryCards();
        }
        if (cards.Count == 0)
            cards.Add(new NativeCard(CouncilGame.Tr("Пусто", "Nothing here"), null, null, null));
        if (SelCard >= cards.Count) SelCard = cards.Count - 1;
        if (SelCard < 0) SelCard = 0;

        SyncCards();
        RefreshTabs();
        RefreshTopLabels(me, c);
        RefreshDetails();
    }

    void RefreshTabs()
    {
        for (int i = 0; i < tabToggles.Count; i++)
            if (tabToggles[i] != null) tabToggles[i].State = i == Tab;
    }

    void RefreshTopLabels(Empire me, CouncilCore c)
    {
        if (TopLeftLabel == null || TopRightLabel == null) return;
        int turn = CouncilGame.CurTurn();
        if (me == null || !c.Enabled)
        {
            TopLeftLabel.Text = CouncilGame.Tr("Галактический совет не созван", "The Galactic Council is not convened");
            TopRightLabel.Text = string.Empty;
            return;
        }
        bool member = c.IsMember(me.Index);
        if (c.Phase == CouncilCore.PhaseVoting)
        {
            int budget = c.Budget(me.Index);
            int left = budget - CouncilUi.PendingTotal();
            TopLeftLabel.Text = member
                ? CouncilGame.Tr("Голосование открыто. Ваши голоса: ", "Voting is open. Your votes: ") + budget + CouncilGame.Tr(" (осталось ", " (left: ") + left + ")"
                : CouncilGame.Tr("Голосование открыто", "Voting is open");
            TopRightLabel.Text = CouncilGame.Tr("Итоги — в начале следующего хода", "Results at the start of next turn");
        }
        else
        {
            TopLeftLabel.Text = member
                ? CouncilGame.Tr("Вы в Совете. Голосов: ", "You are a member. Votes: ") + c.Budget(me.Index)
                : CouncilGame.Tr("Ваша империя не состоит в Совете", "Your empire is not a council member");
            TopRightLabel.Text = c.Enabled
                ? CouncilGame.Tr("Следующее голосование: ход ", "Next vote: turn ") + c.NextVoteTurn + CouncilGame.Tr(" (через ", " (in ") + c.TurnsToNextVote(turn) + ")"
                : string.Empty;
        }
    }

    void SyncCards()
    {
        CardsTable.ReserveChildren(cards.Count, CardPrefab, "CouncilCard");
        var kids = CardsTable.Children;
        cardIndex.Clear();
        for (int i = 0; i < kids.Count; i++)
        {
            var child = kids[i];
            bool vis = i < cards.Count;
            child.Visible = vis;
            if (!vis) continue;
            var card = cards[i];
            var lc = child.GetComponent<LawCard>();
            if (lc == null) continue;
            SetLabel(lc.LawShortTitle, card.Title);
            if (lc.Tooltip != null) lc.Tooltip.Content = card.Title;
            var toggle = lc.Toggle;
            if (toggle != null)
            {
                toggle.UseDoubleClick = false;
                toggle.OnSwitchObject = callbacks.gameObject;
                toggle.OnSwitchMethod = "OnCouncilCard";
                toggle.OnRightClickMethod = null;
                toggle.State = i == SelCard;
                cardIndex[((Component)toggle).gameObject] = i;
            }
        }
        CardsTable.ArrangeChildren(false);
    }

    static void SetLabel(AgePrimitiveLabel label, string text)
    {
        if (label == null) return;
        if (!label.DisableAutoLocalize) label.DisableAutoLocalize = true;
        label.Text = text ?? string.Empty;
    }

    // ------------------------------------------------------------------ details pane

    void RefreshDetails()
    {
        NativeCard card = SelCard >= 0 && SelCard < cards.Count ? cards[SelCard] : null;
        DetailsGroup.Visible = card != null;
        if (card == null) return;
        SetLabel(DetailsTitle, card.Title);
        SetLabel(DetailsSub, card.Subtitle ?? string.Empty);
        SetLabel(DetailsText, card.Detail ?? string.Empty);
        SetLabel(CounterLabel, card.Counter ?? string.Empty);
        SetButton(ActionButton, card.Action, ActionTooltip);
        SetButton(SecondButton, card.Second, null);
    }

    // ------------------------------------------------------------------ card builders

    void BuildVoteCards(Empire me, CouncilCore c)
    {
        int turn = CouncilGame.CurTurn();
        bool member = c.IsMember(me.Index);
        bool voting = c.Phase == CouncilCore.PhaseVoting && member;
        int budget = c.Budget(me.Index);
        int left = budget - CouncilUi.PendingTotal();

        // the ballot: cast / reset
        var ballot = new StringBuilder();
        string ballotTitle;
        if (!c.Enabled) ballotTitle = CouncilGame.Tr("Совет не созван", "The Council is not convened");
        else if (!member) ballotTitle = CouncilGame.Tr("Вы не в Совете", "You are not a member");
        else if (voting) ballotTitle = CouncilGame.Tr("БЮЛЛЕТЕНЬ: отдать голоса", "BALLOT: cast your votes");
        else ballotTitle = CouncilGame.Tr("БЮЛЛЕТЕНЬ", "BALLOT");
        if (c.Enabled && !voting && member)
            ballot.AppendLine(CouncilGame.Tr("Сейчас голосование не проводится. Следующее откроется в начале хода ", "No vote is in progress. The next one opens at the start of turn ") + c.NextVoteTurn +
                              CouncilGame.Tr(" (через ", " (in ") + c.TurnsToNextVote(turn) + CouncilGame.Tr(" ходов).", " turns)."));
        if (voting)
        {
            ballot.AppendLine(CouncilGame.Tr("Итоги будут подведены в начале следующего хода; законы действуют до следующего голосования.",
                                             "Results are counted at the start of next turn; laws last until the next vote."));
            ballot.AppendLine(CouncilGame.Tr("Ваши голоса: ", "Your votes: ") + budget + CouncilGame.Tr(" (осталось ", " (left: ") + left + ")" +
                              "  -  " + c.BudgetText(me.Index, turn, CouncilGame.IsRussian()));
            ballot.AppendLine();
            for (int q = 0; q < c.Proposals.Count; q++)
            {
                var d = CouncilLaws.Get(c.Proposals[q]);
                if (d == null) continue;
                ballot.Append((q + 1) + ". " + CouncilGame.LawName(d) + ": ");
                ballot.Append(OptionsSummary(c, q, d)).AppendLine();
            }
            if (c.HasVoted(me.Index))
                ballot.AppendLine().AppendLine(CouncilGame.Tr("Ваши голоса приняты (потрачено " + c.Spent(me.Index) + "). Можно изменить до конца хода.",
                                                               "Your votes are in (" + c.Spent(me.Index) + " spent). You can change them until the turn ends."));
        }
        var ballotCard = new NativeCard(ballotTitle, null, ballot.ToString(), null);
        if (voting)
        {
            ballotCard.Counter = CouncilGame.Tr("Потрачено: ", "Spent: ") + CouncilUi.PendingTotal() + " / " + budget;
            ballotCard.Action = new NativeAction(
                c.HasVoted(me.Index) ? CouncilGame.Tr("Изменить голоса", "Change votes") : CouncilGame.Tr("Отдать голоса", "Cast votes"),
                CouncilGame.Tr("Отправить бюллетень (все вопросы сразу).", "Submit the whole ballot at once."),
                delegate (bool right) { if (!right) CastBallot(me); });
            if (CouncilUi.PendingTotal() > 0)
                ballotCard.Second = new NativeAction(CouncilGame.Tr("Сбросить", "Reset"),
                    CouncilGame.Tr("Сбросить все нераспределённые голоса.", "Clear all unspent votes."),
                    delegate (bool right) { if (!right) { CouncilUi.ClearPending(); Rebuild(); } });
        }
        cards.Add(ballotCard);

        if (!c.Enabled || c.Phase != CouncilCore.PhaseVoting) return;

        // one card per question
        for (int q = 0; q < c.Proposals.Count; q++)
        {
            var d = CouncilLaws.Get(c.Proposals[q]);
            if (d == null) continue;
            cards.Add(new NativeCard((q + 1) + ". " + CouncilGame.LawName(d) + PendingBadge(c, q, d), null, null, null));
        }

        // the chairman's replacement right (a card below the questions)
        int chairCard = -1;
        if (c.Holds(me.Index, CouncilLaws.Chairman))
        {
            chairCard = cards.Count;
            cards.Add(new NativeCard(CouncilGame.Tr("Право Председателя", "The Chairman's right"), null, null, null));
        }

        // details of the selected proposal / option cards / chairman flow
        if (SelCard > 0 && SelCard <= c.Proposals.Count)
            FillProposalDetails(me, c, SelCard - 1);
        else if (chairCard >= 0 && SelCard == chairCard)
            FillChairmanDetails(c);
    }

    static string OptionsSummary(CouncilCore c, int q, CouncilLawDef d)
    {
        var sb = new StringBuilder();
        if (d.Kind == CouncilLawDef.KindMandate)
        {
            for (int i = 0; i < c.MandateCandidates.Count; i++)
            {
                int v = CouncilUi.Pend(q, c.MandateCandidates[i]);
                if (v > 0) sb.Append(CouncilGame.EmpireName(c.MandateCandidates[i]) + "=" + v + "  ");
            }
        }
        else if (d.Targeted)
        {
            for (int i = 0; i < c.Members.Count; i++)
            {
                int v = CouncilUi.Pend(q, c.Members[i]);
                if (v > 0) sb.Append(CouncilGame.EmpireName(c.Members[i]) + "=" + v + "  ");
            }
        }
        else
        {
            sb.Append(CouncilGame.Tr("за ", "for ") + CouncilUi.Pend(q, 0) + CouncilGame.Tr(" / против ", " / against ") + CouncilUi.Pend(q, 1));
            return sb.ToString();
        }
        if (sb.Length == 0) sb.Append(CouncilGame.Tr("—", "-"));
        return sb.ToString();
    }

    static string PendingBadge(CouncilCore c, int q, CouncilLawDef d)
    {
        int s = 0;
        if (d.Kind == CouncilLawDef.KindMandate)
        {
            for (int i = 0; i < c.MandateCandidates.Count; i++) s += CouncilUi.Pend(q, c.MandateCandidates[i]);
        }
        else if (d.Targeted)
        {
            for (int i = 0; i < c.Members.Count; i++) s += CouncilUi.Pend(q, c.Members[i]);
        }
        else s = CouncilUi.Pend(q, 0) + CouncilUi.Pend(q, 1);
        return s > 0 ? "  •" + s : "";
    }

    /// <summary>The option list of a question: mandate candidates for a mandate question, members for a targeted law.</summary>
    static List<int> OptionsOf(CouncilCore c, CouncilLawDef d)
    {
        var list = new List<int>();
        if (d.Kind == CouncilLawDef.KindMandate) list.AddRange(c.MandateCandidates);
        else if (d.Targeted) list.AddRange(c.Members);
        return list;
    }

    /// <summary>Fills the details/buttons of the selected question card and inserts its option cards after it.</summary>
    void FillProposalDetails(Empire me, CouncilCore c, int q)
    {
        var d = CouncilLaws.Get(c.Proposals[q]);
        if (d == null) return;
        var card = cards[SelCard];
        int cardIdx = SelCard;

        var body = new StringBuilder();
        body.AppendLine(CouncilGame.LawDesc(d));
        body.AppendLine();
        int budget = c.Budget(me.Index);
        int left = budget - CouncilUi.PendingTotal();
        var options = OptionsOf(c, d);
        int sel = SelectedOption(q, options);

        if (d.Kind == CouncilLawDef.KindMandate)
        {
            var mand = CouncilLaws.GetMandate(d.Id);
            body.AppendLine(CouncilGame.Tr("Мандат: выберите империю-владельца из карточек ниже, затем кнопками распределите голоса.",
                                           "Mandate: pick the holder empire from the cards below, then distribute votes with the buttons."));
            if (mand != null) body.AppendLine(CouncilGame.IsRussian() ? mand.RuDesc : mand.EnDesc);
        }
        else if (d.Targeted)
        {
            body.AppendLine(CouncilGame.Tr("Закон против/для одной империи: выберите цель из карточек ниже, затем кнопками распределите голоса.",
                                           "Aimed at one empire: pick the target from the cards below, then distribute votes with the buttons."));
        }
        else
        {
            bool supreme = d.Id == CouncilLaws.NoConfidence;
            if (supreme)
            {
                body.AppendLine(CouncilGame.Tr("Верховный легат: ", "Supreme Legate: ") + CouncilGame.EmpireName(c.SupremeHolder) +
                    CouncilGame.Tr(". Чтобы он остался, нужно голосов «за»: ", ". To keep him the votes in favour must be at least: ") + c.KeepPercentNeeded + "%" +
                    (c.SupremeInAlliance ? CouncilGame.Tr(" (владелец в альянсе)", " (the holder is in an alliance)") : ""));
                body.AppendLine(CouncilGame.Tr("«За» — оставить легата, «против» — снять.", "'For' keeps the Legate, 'against' removes him."));
            }
            else
            {
                body.AppendLine(CouncilGame.Tr("Закон для всех: «за» или «против».", "Applies to everyone: vote for or against."));
            }
        }

        // option cards under the selected card
        for (int i = 0; i < options.Count; i++)
        {
            int emp = options[i];
            int v = CouncilUi.Pend(q, emp);
            var opt = new NativeCard("→ " + CouncilGame.EmpireName(emp) + (emp == me.Index ? CouncilGame.Tr(" (вы)", " (you)") : "") + (v > 0 ? "  •" + v : ""), null, null, null);
            opt.Kind = NativeCard.KOption;
            opt.A = q; opt.B = emp;
            cards.Insert(++cardIdx, opt);
        }

        string tip = CouncilGame.Tr("ЛКМ — +10 голосов, ПКМ — −10.", "Left click: +10 votes, right click: -10.");
        if (options.Count > 0)
        {
            string targetName = CouncilGame.EmpireName(sel);
            card.Counter = CouncilGame.Tr("→ ", "-> ") + targetName + ": " + CouncilUi.Pend(q, sel) + CouncilGame.Tr("  ·  осталось ", "  ·  left: ") + left;
            card.Action = new NativeAction(CouncilGame.Tr("→ ", "-> ") + targetName + "  +10", tip,
                delegate (bool right) { AddVote(me, q, sel, right ? -1 : 1); },
                CouncilUi.Pend(q, sel) > 0 || left >= CouncilCore.VoteStep);
            card.Second = new NativeAction(CouncilGame.Tr("Сбросить вопрос", "Clear question"),
                CouncilGame.Tr("Убрать все голоса этого вопроса.", "Remove all votes of this question."),
                delegate (bool right) { if (!right) { ClearQuestion(c, q); } });
        }
        else
        {
            string optA = d.Id == CouncilLaws.NoConfidence ? CouncilGame.Tr("Оставить", "Keep") : CouncilGame.Tr("Принять", "Pass");
            string optB = d.Id == CouncilLaws.NoConfidence ? CouncilGame.Tr("Снять", "Remove") : CouncilGame.Tr("Отклонить", "Reject");
            card.Counter = optA + ": " + CouncilUi.Pend(q, 0) + CouncilGame.Tr("  ·  ", "  ·  ") + optB + ": " + CouncilUi.Pend(q, 1) +
                           CouncilGame.Tr("  ·  осталось ", "  ·  left: ") + left;
            card.Action = new NativeAction(optA + "  +10", tip,
                delegate (bool right) { AddVote(me, q, 0, right ? -1 : 1); },
                CouncilUi.Pend(q, 0) > 0 || left >= CouncilCore.VoteStep);
            card.Second = new NativeAction(optB + "  +10", tip,
                delegate (bool right) { AddVote(me, q, 1, right ? -1 : 1); },
                CouncilUi.Pend(q, 1) > 0 || left >= CouncilCore.VoteStep);
        }
    }

    /// <summary>Details of the chairman card: pick the question, then the replacement law.</summary>
    void FillChairmanDetails(CouncilCore c)
    {
        var card = cards[SelCard];
        int cardIdx = SelCard;
        var body = new StringBuilder();
        var me = CouncilGame.LocalEmpire();
        if (me == null || !c.CanChairmanReplace(me.Index))
        {
            body.AppendLine(CouncilGame.Tr("Председатель Совета: право замены вопроса уже использовано или голосование началось.",
                                           "Council Chairman: the replacement right is used up or voting has already started."));
            card.Detail = body.ToString();
            return;
        }
        if (ChairQ < 0)
        {
            body.AppendLine(CouncilGame.Tr("Председатель Совета: можно один раз заменить обычный вопрос другим (пока никто не проголосовал). Выберите вопрос из карточек ниже.",
                                           "Council Chairman: you may replace one ordinary question once (until anybody has voted). Pick the question from the cards below."));
            card.Detail = body.ToString();
            for (int q = 0; q < c.Proposals.Count; q++)
            {
                var d = CouncilLaws.Get(c.Proposals[q]);
                if (d == null || d.Kind == CouncilLawDef.KindMandate || d.Id == CouncilLaws.NoConfidence) continue;
                var qc = new NativeCard(CouncilGame.Tr("Заменить вопрос ", "Replace question ") + (q + 1), null, null, null);
                qc.Kind = NativeCard.KChairQuestion;
                qc.A = q;
                cards.Insert(++cardIdx, qc);
            }
        }
        else
        {
            body.AppendLine(CouncilGame.Tr("Выберите новый закон для вопроса ", "Pick the new law for question ") + (ChairQ + 1) + ":");
            card.Detail = body.ToString();
            for (int i = 0; i < CouncilLaws.All.Count; i++)
            {
                var nd = CouncilLaws.All[i];
                if (!nd.Implemented || nd.Kind == CouncilLawDef.KindMandate || nd.Id == CouncilLaws.NoConfidence) continue;
                if (c.Proposals.Contains(nd.Id)) continue;
                var lc = new NativeCard(CouncilGame.LawName(nd) + (nd.Targeted ? CouncilGame.Tr("  [империя]", "  [empire]") : ""), null, null, null);
                lc.Kind = NativeCard.KChairLaw;
                lc.A = i;
                cards.Insert(++cardIdx, lc);
            }
        }
    }

    int SelectedOption(int q, List<int> options)
    {
        int sel;
        if (!SelOption.TryGetValue(q, out sel) || !options.Contains(sel))
            sel = options.Count > 0 ? options[0] : 0;
        return sel;
    }

    void BuildLawsCards(Empire me, CouncilCore c)
    {
        var sources = new StringBuilder();
        sources.AppendLine(CouncilGame.Tr("ОТКУДА БЕРУТСЯ ГОЛОСА", "WHERE THE VOTES COME FROM"));
        var lines = CouncilGame.VoteSourceLines(me);
        for (int i = 0; i < lines.Count; i++) sources.AppendLine("• " + lines[i]);
        sources.AppendLine();
        sources.AppendLine(CouncilGame.Tr("Системы, Республика, технологии, сенаторы и войны считаются в момент открытия голосования.",
                                          "Systems, Republic, technologies, senators and wars are counted when the vote opens."));
        sources.AppendLine();
        sources.AppendLine(CouncilGame.Tr("ПОСТОЯННЫЕ ЭФФЕКТЫ", "PERMANENT EFFECTS"));
        bool any = false;
        for (int i = 0; i < c.Members.Count; i++)
        {
            int e = c.Members[i];
            int vb = c.Budget(e) - CouncilCore.BaseVotes, cd = c.GetColonyDelta(e);
            if (vb == 0 && cd == 0) continue;
            any = true;
            sources.AppendLine("• " + CouncilGame.EmpireName(e) + ": " + CouncilGame.Tr("голосов ", "votes ") + c.Budget(e) +
                (cd != 0 ? CouncilGame.Tr(", колонизационный лимит ", ", colonization limit ") + (cd > 0 ? "+" : "") + cd : ""));
        }
        if (!any) sources.AppendLine(CouncilGame.Tr("Нет.", "None."));
        var marks = CouncilGame.PresenceLines();
        if (marks.Count > 0)
        {
            sources.AppendLine();
            sources.AppendLine(CouncilGame.Tr("ОТМЕЧЕННЫЕ СИСТЕМЫ (стратегическое присутствие): на следующем заседании каждая удерживаемая отмеченная система даёт +10 голосов (максимум +50).",
                                              "MARKED SYSTEMS (Strategic Presence): at the next session every held marked system gives +10 votes (max +50)."));
            for (int i = 0; i < marks.Count; i++) sources.AppendLine("• " + marks[i]);
        }
        cards.Add(new NativeCard(CouncilGame.Tr("Откуда берутся голоса", "Where the votes come from"), null, sources.ToString(), null));

        if (c.Active.Count == 0)
        {
            cards.Add(new NativeCard(CouncilGame.Tr("Нет действующих законов", "No laws in force"),
                CouncilGame.Tr("Законы принимаются голосованием Совета и действуют до следующего голосования.",
                               "Laws are enacted by Council votes and last until the next vote."), null, null));
            return;
        }
        for (int i = 0; i < c.Active.Count; i++)
        {
            var a = c.Active[i];
            var d = CouncilLaws.Get(a.Id);
            if (d == null) continue;
            cards.Add(new NativeCard(CouncilGame.LawName(d) + (a.Target >= 0 ? " → " + CouncilGame.EmpireName(a.Target) : ""),
                a.Target >= 0 ? CouncilGame.Tr("Цель: ", "Target: ") + CouncilGame.EmpireName(a.Target)
                              : CouncilGame.Tr("Действует до следующего голосования", "In force until the next vote"),
                CouncilGame.LawDesc(d), null));
        }
    }

    void BuildMandateCards(Empire me, CouncilCore c)
    {
        int turn = CouncilGame.CurTurn();
        bool member = c.IsMember(me.Index);
        bool voting = c.Phase == CouncilCore.PhaseVoting && member;

        if (!string.IsNullOrEmpty(c.PendingMandate))
        {
            var pm = CouncilLaws.GetMandate(c.PendingMandate);
            cards.Add(new NativeCard(CouncilGame.Tr("На следующем голосовании: ", "On the next vote: ") +
                                     (pm != null ? (CouncilGame.IsRussian() ? pm.Ru : pm.En) : c.PendingMandate), null, null, null));
        }
        int vl = c.VictoryTurnsLeft(turn);
        if (vl >= 0)
            cards.Add(new NativeCard(CouncilGame.Tr("Верховный легат: ", "Supreme Legate: ") + CouncilGame.EmpireName(c.SupremeHolder) +
                                     CouncilGame.Tr(" — до победы ходов: ", " - turns to victory: ") + vl, null, null, null));

        for (int m = 0; m < CouncilLaws.Mandates.Count; m++)
        {
            var d = CouncilLaws.Mandates[m];
            int holder = c.HolderOf(d.Id);
            var body = new StringBuilder();
            body.AppendLine(CouncilGame.IsRussian() ? d.RuDesc : d.EnDesc);
            body.AppendLine();
            body.AppendLine(d.Victory ? CouncilGame.Tr("Голоса: +" + CouncilCore.SupremeMandateVotes + " владельцу", "Votes: +" + CouncilCore.SupremeMandateVotes + " to the holder")
                                      : CouncilGame.Tr("Голоса: +" + CouncilCore.MandateVotes + " владельцу", "Votes: +" + CouncilCore.MandateVotes + " to the holder"));
            body.AppendLine(CouncilGame.Tr("Мандат с наибольшим числом голосов попадёт в следующее голосование вместо одного закона; действует до переизбрания, выхода владельца из Совета или уничтожения Академии.",
                                           "The mandate with the most votes goes onto the next vote instead of one law; it lasts until re-election, the holder leaving the Council or the Academy's destruction."));
            bool qual = member && c.Qualifies(me.Index, d.Id);
            string prog = d.Victory ? "" : "  (" + CouncilGame.ProgressText(me.Index, d) + ")";
            body.AppendLine(CouncilGame.Tr("Требование: ", "Requirement: ") + (CouncilGame.IsRussian() ? d.RuNeed : d.EnNeed) + prog + " - " +
                            (qual ? CouncilGame.Tr("выполнено", "met") : CouncilGame.Tr("не выполнено", "not met")));
            if (voting && qual)
                body.AppendLine(CouncilGame.Tr("Голосуйте за мандат кнопкой ниже (из того же запаса голосов).",
                                               "Vote for the mandate with the button below (from the same pool of votes)."));

            var card = new NativeCard((CouncilGame.IsRussian() ? d.Ru : d.En) + "  [" + (holder >= 0 ? CouncilGame.EmpireName(holder) : CouncilGame.Tr("вакантно", "vacant")) + "]",
                CouncilGame.Tr("Мандат Совета", "Council mandate"), body.ToString(), null);
            if (voting && qual)
            {
                int qq = CouncilCore.MandateQBase + m;
                int budget = c.Budget(me.Index);
                int left = budget - CouncilUi.PendingTotal();
                card.Counter = CouncilGame.Tr("За мандат: ", "For mandate: ") + CouncilUi.Pend(qq, 0) + CouncilGame.Tr("  ·  осталось ", "  ·  left: ") + left;
                card.Action = new NativeAction(CouncilGame.Tr("За мандат  +10", "For mandate  +10"),
                    CouncilGame.Tr("ЛКМ — +10 голосов за мандат, ПКМ — −10.", "Left click: +10 votes for the mandate, right click: -10."),
                    delegate (bool right) { AddVote(me, qq, 0, right ? -1 : 1); },
                    CouncilUi.Pend(qq, 0) > 0 || left >= CouncilCore.VoteStep);
            }
            if (c.Holds(me.Index, d.Id))
            {
                if (d.Id == CouncilLaws.Marshal && c.CanHireMarshal(me.Index))
                    card.Second = new NativeAction(CouncilGame.Tr("Нанять контингент Академии", "Hire the Academy contingent"),
                        CouncilGame.Tr("Содержание контингента — за ваш счёт.", "You pay the upkeep of the contingent."),
                        delegate (bool right) { if (!right) { CouncilGame.SendKind(me, OrderCouncilVote.KindMarshalHire, 0, 0); Rebuild(); } });
                else if (d.Id == CouncilLaws.Quartermaster && c.CanUseQuartermaster(me.Index, turn))
                    card.Second = new NativeAction(CouncilGame.Tr("Запросить поставку у Академии", "Request a delivery from the Academy"), null,
                        delegate (bool right) { if (!right) { CouncilGame.SendKind(me, OrderCouncilVote.KindQuartermaster, 0, 0); Rebuild(); } });
                else if (d.Id == CouncilLaws.Eye)
                    card.Second = new NativeAction(CouncilGame.Tr("Действует: +1 взлом, +20 мощности", "Active: +1 hack, +20 processing power"), null, null);
                else if (d.Id == CouncilLaws.Chairman)
                    card.Second = new NativeAction(CouncilGame.Tr("Право замены вопроса — вкладка «Голосование»", "Question replacement - see the Voting tab"), null, null);
            }
            cards.Add(card);
        }

        if (member)
        {
            var leave = new NativeCard(CouncilGame.Tr("Выйти из Совета…", "Leave the Council..."),
                CouncilGame.Tr("Выход", "Leaving"),
                CouncilGame.Tr("Выход необратим: Академия объявит вам войну, мандаты будут потеряны.",
                               "Leaving is irreversible: the Academy declares war on you and your mandates are lost."), null);
            leave.Action = new NativeAction(CouncilGame.Tr("Выйти из Совета", "Leave the Council"), null,
                delegate (bool right)
                {
                    if (right) return;
                    CouncilNative.Confirm(CouncilGame.Tr("Выход необратим: Академия объявит вам войну, мандаты будут потеряны. Выйти?",
                                                         "Leaving is irreversible: the Academy declares war on you and your mandates are lost. Leave?"),
                                          CouncilGame.Tr("Выйти", "Leave"),
                                          delegate { CouncilGame.SendKind(me, OrderCouncilVote.KindLeave, 0, 0); Rebuild(); });
                });
            cards.Add(leave);
        }
        else if (c.IsOutcast(me.Index))
        {
            cards.Add(new NativeCard(CouncilGame.Tr("Вы покинули Совет", "You left the Council"), null,
                CouncilGame.Tr("Вы покинули Совет: Академия находится с вами в состоянии войны.",
                               "You left the Council: the Academy is at war with you."), null));
        }
    }

    void BuildHistoryCards()
    {
        var c = CouncilGame.Core;
        if (c.History.Count == 0)
        {
            cards.Add(new NativeCard(CouncilGame.Tr("Итогов пока нет", "Nothing yet"), null,
                CouncilGame.Tr("Здесь появятся результаты прошлых голосований.", "Results of previous votes will appear here."), null));
            return;
        }
        for (int i = c.History.Count - 1; i >= 0; i--)
        {
            var r = c.History[i];
            cards.Add(new NativeCard(DescribeShort(r), null, CouncilUiImgui.DescribeResult(r), null));
        }
    }

    static string DescribeShort(CouncilResult r)
    {
        string name = r.LawId;
        if (r.LawId.StartsWith("MV:"))
        {
            var m = CouncilLaws.GetMandate(r.LawId.Substring(3));
            name = m != null ? (CouncilGame.IsRussian() ? m.Ru : m.En) : r.LawId;
        }
        else if (r.LawId.StartsWith("CH:")) name = CouncilGame.Tr("Замена вопроса", "Question replaced");
        else { var d = CouncilLaws.Get(r.LawId); if (d != null) name = CouncilGame.LawName(d); }
        return "T" + r.Turn + ": " + name + (r.Passed ? " ✓" : " ✗");
    }

    // ------------------------------------------------------------------ actions

    void CastBallot(Empire me)
    {
        var entries = new List<int[]>();
        foreach (var kv in CouncilUi.Pending)
            if (kv.Value > 0) entries.Add(new[] { kv.Key / 1000, kv.Key % 1000, kv.Value });
        if (CouncilGame.CastBallot(me, entries))
            CouncilNative.Toast(CouncilGame.Tr("Ваши голоса учтены", "Your votes are recorded"));
        Rebuild();
    }

    void AddVote(Empire me, int q, int opt, int dir)
    {
        var c = CouncilGame.Core;
        int cur = CouncilUi.Pend(q, opt);
        int left = c.Budget(me.Index) - CouncilUi.PendingTotal();
        if (dir > 0 && left < CouncilCore.VoteStep) return;
        if (dir < 0 && cur < CouncilCore.VoteStep) return;
        CouncilUi.SetPend(q, opt, cur + dir * CouncilCore.VoteStep);
        Rebuild();
    }

    void ClearQuestion(CouncilCore c, int q)
    {
        for (int i = 0; i < c.Members.Count; i++) CouncilUi.SetPend(q, c.Members[i], 0);
        for (int i = 0; i < c.MandateCandidates.Count; i++) CouncilUi.SetPend(q, c.MandateCandidates[i], 0);
        CouncilUi.SetPend(q, 0, 0);
        CouncilUi.SetPend(q, 1, 0);
        Rebuild();
    }

    // ------------------------------------------------------------------ input callbacks (routed from CouncilNativeCallbacks)

    public void OnTab(GameObject sender)
    {
        for (int i = 0; i < tabToggles.Count; i++)
            if (tabToggles[i] != null && ((Component)tabToggles[i]).gameObject == sender) { Tab = i; break; }
        ChairQ = -1;
        SelCard = 0;
        Rebuild();
    }

    public void OnCard(GameObject sender)
    {
        int idx;
        if (!cardIndex.TryGetValue(sender, out idx)) return;
        if (idx < 0 || idx >= cards.Count) return;
        var card = cards[idx];
        if (card.Kind == NativeCard.KOption)
        {
            SelOption[card.A] = card.B;
            // keep the parent proposal selected: it is the card right before the option cards
            if (SelCard > idx) SelCard = idx - 1;
            Rebuild();
            return;
        }
        if (card.Kind == NativeCard.KChairQuestion)
        {
            ChairQ = card.A;
            Rebuild();
            return;
        }
        if (card.Kind == NativeCard.KChairLaw)
        {
            var me = CouncilGame.LocalEmpire();
            if (me != null && ChairQ >= 0)
            {
                CouncilGame.SendKind(me, OrderCouncilVote.KindChairmanReplace, ChairQ, card.A);
                CouncilNative.Toast(CouncilGame.Tr("Председатель заменил вопрос", "The Chairman replaced the question"));
            }
            ChairQ = -1;
            Rebuild();
            return;
        }
        SelCard = idx;
        Rebuild();
    }

    public void OnAction(bool right)
    {
        var card = SelCard >= 0 && SelCard < cards.Count ? cards[SelCard] : null;
        if (card == null || card.Action == null) return;
        card.Action.Run(right);
    }

    public void OnSecond(bool right)
    {
        var card = SelCard >= 0 && SelCard < cards.Count ? cards[SelCard] : null;
        if (card == null || card.Second == null) return;
        card.Second.Run(right);
    }
}

// ---------------------------------------------------------------------- card model

/// <summary>One card of the list (with the details shown when it is selected).</summary>
public sealed class NativeCard
{
    public const int KOption = 1;        // option row under a proposal (A = question, B = option value)
    public const int KChairQuestion = 2; // chairman flow: question to replace (A = question index)
    public const int KChairLaw = 3;      // chairman flow: replacement law (A = index in CouncilLaws.All)

    public string Title;
    public string Subtitle;
    public string Detail;
    public string Counter;
    public NativeAction Action, Second;
    public int Kind;
    public int A, B;

    public NativeCard(string title, string subtitle, string detail, string counter)
    {
        Title = title; Subtitle = subtitle; Detail = detail; Counter = counter;
    }
}

/// <summary>An action of one of the two details buttons; Run(true) is the right click.</summary>
public sealed class NativeAction
{
    public string Label;
    public string Tip;
    public Action<bool> Run;
    public bool Enabled;

    public NativeAction(string label, string tip, Action<bool> run) : this(label, tip, run, true) { }

    public NativeAction(string label, string tip, Action<bool> run, bool enabled)
    {
        Label = label; Tip = tip; Run = run; Enabled = enabled;
    }
}

/// <summary>Receives the AGE SendMessage callbacks and routes them to the window.</summary>
public class CouncilNativeCallbacks : MonoBehaviour
{
    CouncilModalWindow win;

    public void Init(CouncilModalWindow window) { win = window; }

    void OnCouncilTab(GameObject sender) { if (win != null) win.OnTab(sender); }
    void OnCouncilCard(GameObject sender) { if (win != null) win.OnCard(sender); }
    void OnCouncilAction(GameObject sender) { if (win != null) win.OnAction(false); }
    void OnCouncilActionRight(GameObject sender) { if (win != null) win.OnAction(true); }
    void OnCouncilSecond(GameObject sender) { if (win != null) win.OnSecond(false); }
    void OnCouncilSecondRight(GameObject sender) { if (win != null) win.OnSecond(true); }
    void OnCouncilLauncher(GameObject sender) { CouncilNative.Toggle(); }
}

// ---------------------------------------------------------------------- non-public GuiWindow plumbing

/// <summary>Reflection access to the few non-public members of the game's GuiWindow/GuiManager that a
/// runtime-created window needs (they live in Assembly-CSharp-firstpass, which is not publicized).</summary>
static class WinPriv
{
    static readonly MethodInfo initializeMethod = typeof(GuiWindow).GetMethod("Initialize",
        BindingFlags.Instance | BindingFlags.NonPublic);
    static readonly PropertyInfo loadedProperty = typeof(GuiWindow).GetProperty("Loaded");
    static readonly PropertyInfo gameReadyProperty = typeof(GuiWindow).GetProperty("GameReady");
    static readonly FieldInfo windowListField = typeof(Amplitude.Unity.Gui.GuiManager).GetField("guiWindowsFromBackToFront",
        BindingFlags.Instance | BindingFlags.NonPublic);

    public static void Initialize(GuiWindow window, GuiWindowsStack stack)
    {
        if (initializeMethod == null) throw new Exception("GuiWindow.Initialize not found");
        initializeMethod.Invoke(window, new object[] { stack, false });
    }

    public static void SetLoaded(GuiWindow window, bool value)
    {
        if (loadedProperty == null) throw new Exception("GuiWindow.Loaded not found");
        loadedProperty.SetValue(window, value, null);
    }

    public static void SetGameReady(GuiWindow window, bool value)
    {
        if (gameReadyProperty == null) throw new Exception("GuiWindow.GameReady not found");
        gameReadyProperty.SetValue(window, value, null);
    }

    /// <summary>Adds the window to the manager's window list, like GuiManager.InstantiateWindows does;
    /// the list drives input dispatch (ESC/right-click), HideAllWindows and the game-ready pulses.</summary>
    public static void Register(GuiManager guiManager, GuiWindow window)
    {
        var list = guiManager != null && windowListField != null
            ? windowListField.GetValue(guiManager) as List<GuiWindow>
            : null;
        if (list == null) throw new Exception("GuiManager.guiWindowsFromBackToFront not found");
        if (!list.Contains(window)) list.Add(window);
    }
}

// ---------------------------------------------------------------------- controller

/// <summary>Builds and drives the native window, the HUD launcher button and the native toasts.</summary>
public static class CouncilNative
{
    public static CouncilModalWindow Window;
    public static bool BuildFailed;
    public static string BuildError = "";

    static AgeControlButton launcher;
    static AgePrimitiveLabel launcherLabel;

    public static string[] TabNames()
    {
        return new[]
        {
            CouncilGame.Tr("Голосование", "Voting"),
            CouncilGame.Tr("Законы", "Laws"),
            CouncilGame.Tr("Мандаты", "Mandates"),
            CouncilGame.Tr("Итоги", "Results")
        };
    }

    // ------------------------------------------------------------------ build

    /// <summary>Creates the window once the game GUI is loaded. False = not ready yet or failed.</summary>
    public static bool EnsureWindow()
    {
        if (Window != null) return true;
        if (BuildFailed) return false;
        try
        {
            var gui = Services.GetService<IGuiService>();
            if (gui == null || !gui.GuiWindowsLoaded) return false;   // retry on a later call
            var donor = gui.GetWindow<LawsManagementModalWindow>(false);
            if (donor == null) throw new Exception("LawsManagementModalWindow not found");
            var stack = ((Component)donor).GetComponentInParent<GuiWindowsStack>();
            if (stack == null) throw new Exception("GuiWindowsStack not found");

            // 1. clone the whole laws-screen prefab hierarchy (frame, tables, buttons, sounds, animations)
            var go = (GameObject)UnityEngine.Object.Instantiate(donor.gameObject);
            go.name = "CouncilModalWindow";
            go.transform.parent = donor.transform.parent;
            var win = go.AddComponent<CouncilModalWindow>();

            // 2. link the AGE graph of the clone (Instantiate does not copy the runtime child lists)
            win.AgeTransform.Init();

            // 3. copy the widget references from the clone's own component, then remove it
            var src = go.GetComponent<LawsManagementModalWindow>();
            if (src == null) throw new Exception("clone has no LawsManagementModalWindow");
            win.HideGuiBehind = src.HideGuiBehind;
            win.Setup(src);
            UnityEngine.Object.Destroy(src);

            // 4. bind the modal chrome (BackgroundGroup/MainContainer) by child names, like GuiModalWindow.Load
            GuiBoundAttribute.BindAllProperties(win);
            if (win.BackgroundGroup == null || win.MainContainer == null)
                throw new Exception("modal chrome was not bound on the clone");

            // 5. register the window the way GuiManager.InstantiateWindows would
            //    (the non-public GuiWindow plumbing lives in firstpass -> reflection)
            WinPriv.Initialize(win, stack);
            WinPriv.SetLoaded(win, true);
            WinPriv.SetGameReady(win, true);
            if (win.AgeTransform != null && win.AgeTransform.Visible) gui.HideWindow(win, true);
            WinPriv.Register(gui as GuiManager, win);

            Window = win;
            EnsureLauncher(gui);
            RefreshLauncher();
            return true;
        }
        catch (Exception e)
        {
            BuildFailed = true;
            BuildError = e.GetType().Name + ": " + e.Message;
            CouncilGame.LastError = "native UI: " + BuildError;
            try { Diagnostics.LogError("Galactic Council: native UI failed, falling back to IMGUI. " + e); } catch (Exception) { }
            return false;
        }
    }

    static void EnsureLauncher(IGuiService gui)
    {
        if (launcher != null || Window == null || Window.ActionButton == null) return;
        try
        {
            var overlay = gui.GetWindow<GameOverlayWindow>(false);
            if (overlay == null || overlay.AgeTransform == null) return;
            var at = overlay.AgeTransform.InstantiateChild<Transform>(Window.ActionButton.AgeTransform.transform, "CouncilLauncherButton");
            var lat = at.GetComponent<AgeTransform>();
            if (lat == null) return;
            // pin to the top-right corner of the screen-sized overlay, 236x32
            lat.Anchored = true;
            lat.AttachLeft = false; lat.AttachRight = true;
            lat.AttachBottom = false; lat.AttachTop = true;
            lat.PercentLeft = 0; lat.PercentRight = 0; lat.PercentTop = 0; lat.PercentBottom = 0;
            lat.PixelMarginLeft = 0; lat.PixelMarginRight = 8;
            lat.PixelMarginTop = 6; lat.PixelMarginBottom = 0;
            lat.PixelOffsetLeft = 0; lat.PixelOffsetRight = 0;
            lat.PixelOffsetTop = 0; lat.PixelOffsetBottom = 0;
            lat.Width = 236;
            lat.Height = 32;
            lat.ComputeExtents(overlay.AgeTransform.Width, overlay.AgeTransform.Height);

            launcher = at.GetComponent<AgeControlButton>();
            if (launcher == null) return;
            launcherLabel = at.GetComponentInChildren<AgePrimitiveLabel>();
            var callbacks = ((Component)Window).GetComponent<CouncilNativeCallbacks>();
            launcher.UseLeftClick = true;
            launcher.UseRightClick = false;
            launcher.UseDoubleClick = false;
            launcher.OnActivateObject = callbacks != null ? callbacks.gameObject : ((Component)Window).gameObject;
            launcher.OnActivateMethod = "OnCouncilLauncher";
            launcher.OnRightClickMethod = null;
            var tip = at.GetComponentInChildren<AgeTooltip>();
            if (tip != null) tip.Content = CouncilGame.Tr("Галактический совет Академии", "The Galactic Council of the Academy");
        }
        catch (Exception e) { NoteFailure("launcher", e); }
    }

    /// <summary>Highlights the launcher button while a vote is open and not yet cast.</summary>
    public static void RefreshLauncher()
    {
        if (launcher == null || launcherLabel == null) return;
        try
        {
            var c = CouncilGame.Core;
            var me = CouncilGame.LocalEmpire();
            bool voting = c.Enabled && c.Phase == CouncilCore.PhaseVoting && me != null && c.IsMember(me.Index) && !c.HasVoted(me.Index);
            launcherLabel.Text = CouncilGame.Tr("Галактический совет", "Galactic Council") + (voting ? "  (!)" : "");
            launcherLabel.TintColor = voting ? new Color(1f, 0.75f, 0.25f) : Color.white;
        }
        catch (Exception) { }
    }

    // ------------------------------------------------------------------ show / hide

    public static bool IsVisible
    {
        get { return Window != null && Window.AgeTransform != null && Window.AgeTransform.Visible; }
    }

    public static void Show()
    {
        if (!EnsureWindow() || Window == null) return;
        try
        {
            var gui = Services.GetService<IGuiService>();
            if (gui != null) gui.ShowWindow(Window, false);
        }
        catch (Exception e) { NoteFailure("show", e); }
        try { Window.Rebuild(); } catch (Exception e) { NoteFailure("show-rebuild", e); }
        RefreshLauncher();
    }

    public static void Hide()
    {
        if (Window == null) return;
        try
        {
            var gui = Services.GetService<IGuiService>();
            if (gui != null) gui.HideWindow(Window, false);
        }
        catch (Exception e) { NoteFailure("hide", e); }
        RefreshLauncher();
    }

    public static void Toggle()
    {
        if (IsVisible) Hide();
        else Show();
    }

    public static void RebuildIfVisible()
    {
        if (Window != null && IsVisible)
        {
            try { Window.Rebuild(); } catch (Exception e) { NoteFailure("rebuild", e); }
        }
        RefreshLauncher();
    }

    // ------------------------------------------------------------------ native messages

    /// <summary>A native toast: the game's non-blocking message box, auto-closed after a few seconds.</summary>
    public static void Toast(string text)
    {
        try
        {
            var gui = Services.GetService<IGuiService>();
            if (gui == null) return;
            var box = gui.GetWindow<MessageBoxNonBlockingWindow>(false);
            if (box == null) return;
            box.Title = CouncilGame.Tr("Галактический совет", "Galactic Council");
            box.Message = text;
            box.Type = MessageBoxType.INFORMATIVE;
            box.ActiveEventHandler = null;
            box.ValidateTitle = null;
            box.CancelTitle = null;
            box.TimeoutDuration = 14;
            gui.ShowWindow(box, false);
        }
        catch (Exception e) { NoteFailure("toast", e); }
    }

    /// <summary>A native blocking confirmation with Validate/Cancel buttons.</summary>
    public static void Confirm(string message, string validateTitle, Action onOk)
    {
        try
        {
            var gui = Services.GetService<IGuiService>();
            var gm = gui as GuiManager;
            if (gm == null) { if (onOk != null) onOk(); return; }
            MessageBoxResultEventArgs.EventHandler handler = delegate (object sender, MessageBoxResultEventArgs e)
            {
                if (e != null && e.Result == MessageBoxResult.Ok && onOk != null) onOk();
            };
            gm.ShowMessage(message, MessageBoxType.INFORMATIVE, handler,
                CouncilGame.Tr("Галактический совет", "Galactic Council"), validateTitle,
                CouncilGame.Tr("Отмена", "Cancel"), null);
        }
        catch (Exception e) { NoteFailure("confirm", e); }
    }

    public static void NoteFailure(string where, Exception e)
    {
        CouncilGame.LastError = "native UI " + where + ": " + e.GetType().Name + ": " + e.Message;
        try { Diagnostics.LogError("Galactic Council native UI (" + where + "): " + e); } catch (Exception) { }
    }
}
