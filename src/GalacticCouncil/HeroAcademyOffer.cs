// Hero rework - "variant 2b" of the Academy offer. The vanilla game answers a filled Academy gauge with a
// random trio of heroes (Academy.PickHeroesForRecruitment -> OrderOfferHeroChoice -> the recruitment
// notification). This file adds one question for the local player: WHICH KIND of hero he wants. The answer
// travels as OrderAcademyHeroKind; the processor then reshapes that pending choice into TWO candidates of
// the picked kind on every machine (deterministic pick from the Academy pool with the game PRNG), returns
// the dropped trio heroes to the pool and raises EventHeroUnlockGaugeFilled again, so the recruitment
// notification redraws with the two new cards. A senator pick under a Dictatorship government is warned
// about first (only the dictator hero can hold the single council seat there). AI empires, a closed dialog
// and an empty pool keep the vanilla trio - the vanilla flow is never broken.
using System;
using System.Collections;
using System.Collections.Generic;
using Amplitude;
using Amplitude.Unity.Event;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Gui;

public static partial class HeroRework
{
    /// <summary>God-Mod switch (session only): ask the kind of the new hero and offer a pair of candidates instead of the vanilla trio.</summary>
    public static bool AcademyOfferEnabled = true;

    // one question per pending choice object: a new trio is a new HeroChoice instance, so the same trio is
    // never asked twice while the next trio will be asked again
    static readonly HashSet<DepartmentOfEducation.HeroChoice> askedChoices = new HashSet<DepartmentOfEducation.HeroChoice>();

    // ------------------------------------------------------------------ client hook (start of HeroRecruitmentNotificationWindow.Refresh)

    /// <summary>Called at the start of the recruitment notification Refresh: offers the kind question about a fresh vanilla trio.
    /// The window fills its own Choice field only later in Refresh, so the choice is read from the notification itself.</summary>
    public static void OnRecruitNoteRefresh(HeroRecruitmentNotificationWindow window)
    {
        try
        {
            if (!AcademyOfferEnabled || !Enabled || window == null) return;
            var note = window.GuiNotification as NotificationHeroRecruitment;
            if (note == null) return;
            var choice = note.Choice;
            if (choice == null) return;
            var refs = choice.HeroDefinitionReferences;
            if (refs == null || refs.Length != 3) return;              // only an untouched vanilla trio is asked about
            if (askedChoices.Contains(choice)) return;                 // already asked, answered or reshaped
            var empire = CouncilGame.GetMajor(note.EmpireIndex);
            if (empire == null || empire != CouncilGame.LocalEmpire()) return;   // ask only the human owner
            var gui = Services.GetService<IGuiService>() as GuiManager;
            if (gui == null) return;
            if (gui.IsAnyModalVisible) return;                         // a modal is already up (the Academy window, a second trio...): keep the vanilla trio
            askedChoices.Add(choice);
            AskKind(gui, empire, choice, window);
        }
        catch (Exception e) { Fail("OnRecruitNoteRefresh", e); }
    }

    // ------------------------------------------------------------------ dialogs (native MessageBoxWindow: 2 or 3 buttons, ESC = silent decline)

    static void AskKind(GuiManager gui, MajorEmpire empire, DepartmentOfEducation.HeroChoice choice, HeroRecruitmentNotificationWindow window)
    {
        bool ru = CouncilGame.IsRussian();
        MessageBoxResultEventArgs.EventHandler handler = delegate (object sender, MessageBoxResultEventArgs e)
        {
            HeroKind kind;
            if (e == null || e.Result == MessageBoxResult.Ok) kind = HeroKind.Admiral;
            else if (e.Result == MessageBoxResult.Cancel) kind = HeroKind.Governor;
            else if (e.Result == MessageBoxResult.Alternative) kind = HeroKind.Senator;
            else return;                                               // silent close (ESC / right click): the vanilla trio stays
            if (kind == HeroKind.Senator && IsDictatorship(SenateOf(empire)))
                gui.StartCoroutine(NextFrame(delegate { WarnDictator(gui, empire, choice, window); }));
            else
                SendKind(empire, choice, window, kind);
        };
        gui.ShowMessage(CouncilGame.Tr("Шкала Академии заполнена. Какого героя пригласит Академия?",
                                       "The Academy gauge is full. What kind of hero shall the Academy invite?"),
            MessageBoxType.INFORMATIVE, handler,
            CouncilGame.Tr("Академия", "The Academy"),
            HeroKinds.Name(HeroKind.Admiral, ru), HeroKinds.Name(HeroKind.Governor, ru), HeroKinds.Name(HeroKind.Senator, ru));
    }

    /// <summary>Dictatorship warning before a senator pair (verbatim wording of the design document).</summary>
    static void WarnDictator(GuiManager gui, MajorEmpire empire, DepartmentOfEducation.HeroChoice choice, HeroRecruitmentNotificationWindow window)
    {
        MessageBoxResultEventArgs.EventHandler handler = delegate (object sender, MessageBoxResultEventArgs e)
        {
            if (e == null || e.Result == MessageBoxResult.None) return;            // silent close: the vanilla trio stays
            if (e.Result == MessageBoxResult.Ok)
                SendKind(empire, choice, window, HeroKind.Senator);                // "I know what I am doing"
            else
                gui.StartCoroutine(NextFrame(delegate { AskAdmiralOrGovernor(gui, empire, choice, window); }));
        };
        gui.ShowMessage(CouncilGame.Tr(
                "Внимание, ваш политический строй - Диктатура! В этом моде лишь диктатор может занимать единственное доступное место в совете. Если вы выберите нового героя-сенатора и ничего не поменяете в вашем государстве, то не сможете посадить этого героя в сенат!",
                "Warning: your government is a Dictatorship! In this mod only the dictator can hold the single council seat. If you recruit a new senator hero and change nothing about your empire, you will not be able to seat this hero in the senate!"),
            MessageBoxType.WARNING, handler,
            CouncilGame.Tr("Академия", "The Academy"),
            CouncilGame.Tr("Я понимаю, что делаю, дайте мне выбрать нового героя роли Сенатора!", "I know what I am doing: let me pick the new Senator hero!"),
            CouncilGame.Tr("Ой... Я ошибся. Пожалуйста, дайте мне выбрать между Адмиралом и Наместником", "Oops... I was wrong. Please let me choose between an Admiral and a Governor"),
            null);
    }

    static void AskAdmiralOrGovernor(GuiManager gui, MajorEmpire empire, DepartmentOfEducation.HeroChoice choice, HeroRecruitmentNotificationWindow window)
    {
        bool ru = CouncilGame.IsRussian();
        MessageBoxResultEventArgs.EventHandler handler = delegate (object sender, MessageBoxResultEventArgs e)
        {
            if (e == null || e.Result == MessageBoxResult.None) return;            // silent close: the vanilla trio stays
            SendKind(empire, choice, window, e.Result == MessageBoxResult.Ok ? HeroKind.Admiral : HeroKind.Governor);
        };
        gui.ShowMessage(CouncilGame.Tr("Какого героя пригласит Академия?", "What kind of hero shall the Academy invite?"),
            MessageBoxType.INFORMATIVE, handler,
            CouncilGame.Tr("Академия", "The Academy"),
            HeroKinds.Name(HeroKind.Admiral, ru), HeroKinds.Name(HeroKind.Governor, ru), null);
    }

    /// <summary>Waits one frame: the message box that is calling back closes itself right after the handler,
    /// and it is the same window object the next dialog will be shown on.</summary>
    static IEnumerator NextFrame(Action action)
    {
        yield return null;
        if (action != null) action();
    }

    static void SendKind(MajorEmpire empire, DepartmentOfEducation.HeroChoice choice, HeroRecruitmentNotificationWindow window, HeroKind kind)
    {
        try
        {
            // hide the trio notification: the processor raises the unlock event again and a fresh pair is offered
            var note = window == null ? null : window.GuiNotification;
            if (note != null) Gui.GuiNotificationService.DismissGuiNotification(note, false);
            var pc = empire.PlayerControllers;
            if (pc != null && pc.Client != null)
                pc.Client.PostOrder(new OrderAcademyHeroKind(empire.Index, choice.GaugeIndex, (int)kind));
        }
        catch (Exception e) { Fail("SendKind", e); }
    }

    // ------------------------------------------------------------------ order (both entry points run on every machine)

    public static bool AcademyHeroKindPreprocessor(OrderAcademyHeroKind order)
    {
        try
        {
            if (order == null || !Enabled) return false;
            if (order.Kind < 0 || order.Kind > 2 || order.GaugeIndex < 0) return false;
            var edu = EducationOf(order.EmpireIndex);
            return edu != null && FindTrio(edu, order.GaugeIndex) != null;
        }
        catch (Exception e) { Fail("AcademyHeroKindPreprocessor", e); return false; }
    }

    public static IEnumerator AcademyHeroKindProcessor(OrderAcademyHeroKind order)
    {
        try { ReshapeChoice(order); }
        catch (Exception e) { Fail("AcademyHeroKindProcessor", e); }
        yield break;
    }

    static DepartmentOfEducation EducationOf(int empireIndex)
    {
        var empire = CouncilGame.GetMajor(empireIndex);
        if (empire == null) return null;
        try { return empire.GetAgency<DepartmentOfEducation>(); }
        catch (Exception) { return null; }
    }

    /// <summary>The pending choice of the gauge if it is still the untouched vanilla trio.</summary>
    static DepartmentOfEducation.HeroChoice FindTrio(DepartmentOfEducation edu, int gaugeIndex)
    {
        if (edu == null || edu.PendingHeroChoices == null) return null;
        for (int i = 0; i < edu.PendingHeroChoices.Count; i++)
        {
            var c = edu.PendingHeroChoices[i];
            if (c == null || c.GaugeIndex != gaugeIndex) continue;
            var r = c.HeroDefinitionReferences;
            if (r != null && r.Length == 3) return c;
        }
        return null;
    }

    static void ReshapeChoice(OrderAcademyHeroKind order)
    {
        var game = CouncilGame.GetGame();
        if (game == null) return;
        var empire = CouncilGame.GetMajor(order.EmpireIndex);
        if (empire == null) return;
        var edu = empire.GetAgency<DepartmentOfEducation>();
        var choice = FindTrio(edu, order.GaugeIndex);
        if (choice == null) return;                          // already picked or reshaped: nothing to do
        var academy = edu.Academy;
        if (academy == null) return;

        var kind = (HeroKind)order.Kind;
        var trio = new List<string>();
        for (int i = 0; i < choice.HeroDefinitionReferences.Length; i++) trio.Add(choice.HeroDefinitionReferences[i].ToString());

        // candidates of the wanted kind: the trio itself first, then the Academy pool
        var candidates = new List<string>();
        HeroKind trioKind;
        for (int i = 0; i < trio.Count; i++)
            if (!candidates.Contains(trio[i]) && TryKindOfName(trio[i], out trioKind) && trioKind == kind) candidates.Add(trio[i]);
        var pool = academy.AvailableHeroes;
        if (pool != null)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                var d = pool[i];
                if (d == null) continue;
                string name = d.Name.ToString();
                if (trio.Contains(name) || candidates.Contains(name)) continue;
                HeroKind poolKind;
                if (TryKindOfName(name, out poolKind) && poolKind == kind) candidates.Add(name);
            }
        }
        if (candidates.Count == 0) return;                   // no hero of that kind exists: the vanilla trio stays

        // deterministic pick: sorting makes the pool order irrelevant, the seed is the same on every machine
        var pair = HeroAcademy.PickPair(candidates, unchecked((int)game.Galaxy.Seed) + game.Turn * 7 + order.GaugeIndex * 131 + order.EmpireIndex * 1009);
        if (pair.Count == 0) return;

        // pool bookkeeping: dropped trio heroes go back, new candidates leave the pool
        for (int i = 0; i < trio.Count; i++)
            if (!pair.Contains(trio[i])) academy.MakeHeroAvailableAgain(new StaticString(trio[i]));
        for (int i = 0; i < pair.Count; i++)
            if (!trio.Contains(pair[i])) academy.MakeHeroUnavailable(new StaticString(pair[i]));

        // the recruitment window draws any number of cards and picks by index, so two references are enough
        choice.HeroDefinitionReferences = ToStaticStrings(pair);
        Services.GetService<IEventService>().Notify(new EventHeroUnlockGaugeFilled(empire, choice));
    }

    /// <summary>Kind of a hero definition by name; false when the name is not in the database (never offered).</summary>
    static bool TryKindOfName(string name, out HeroKind kind)
    {
        kind = HeroKind.Admiral;
        try
        {
            HeroDefinition d;
            var db = Databases.GetDatabase<HeroDefinition>(false);
            if (db == null || !db.TryGetValue(new StaticString(name), out d) || d == null) return false;
            if (d.Class != null && HeroKinds.TryKindOfClass(d.Class.Name.ToString(), out kind)) return true;
            kind = HeroKinds.KindOfGuid(HeroAcademy.StableHash(name));      // unknown (modded) class: spread by name
            return true;
        }
        catch (Exception) { return false; }
    }

    static StaticString[] ToStaticStrings(List<string> names)
    {
        var arr = new StaticString[names.Count];
        for (int i = 0; i < names.Count; i++) arr[i] = new StaticString(names[i]);
        return arr;
    }
}
