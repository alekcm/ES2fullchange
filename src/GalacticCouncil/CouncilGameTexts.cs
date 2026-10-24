// Galactic Council: vote sources read from the game (technology stage), the "gives votes" notes in the game texts
// (Republic government, Empire Development stages) and the explanation of the sources shown in the Council window.
using System;
using System.Collections.Generic;
using System.Text;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Gui;
using Amplitude.Unity.Localization;

public static partial class CouncilGame
{
    // ---------------------------------------------------------------- Empire Development stage

    static List<StaticString> devQuadrants;

    static void EnsureDevQuadrants()
    {
        if (devQuadrants != null) return;
        var list = new List<StaticString>();
        try
        {
            var db = Databases.GetDatabase<ScienceConstructibleElement>(false);
            if (db != null)
            {
                var all = db.GetValues();
                for (int i = 0; i < all.Length; i++)
                {
                    var stage = all[i] as TechnologyStageDefinition;
                    if (stage == null || stage.TechnologyQuadrant == null) continue;
                    string n = stage.TechnologyQuadrant.Name.ToString();
                    if (n.IndexOf("EmpireDevelopment", StringComparison.OrdinalIgnoreCase) < 0) continue;
                    bool known = false;
                    for (int k = 0; k < list.Count; k++) if (list[k] == stage.TechnologyQuadrant.Name) known = true;
                    if (!known) list.Add(stage.TechnologyQuadrant.Name);
                }
            }
        }
        catch (Exception e) { Fail("EnsureDevQuadrants", e); }
        if (list.Count == 0) list.Add("TechnologyQuadrantEmpireDevelopment");
        devQuadrants = list;
    }

    /// <summary>Highest unlocked stage (1..5) of the Empire Development technology branch; 0 when nothing is known.</summary>
    public static int EmpireDevelopmentStage(Empire e)
    {
        try
        {
            var sci = e.GetAgency<DepartmentOfScience>();
            if (sci == null) return 0;
            EnsureDevQuadrants();
            int best = 0;
            for (int i = 0; i < devQuadrants.Count; i++) best = Math.Max(best, sci.GetCurrentStageNumber(devQuadrants[i]));
            return best;
        }
        catch (Exception ex) { Fail("EmpireDevelopmentStage", ex); return 0; }
    }

    // ---------------------------------------------------------------- explanation of the vote sources (Council window)

    /// <summary>Lines "source: rule (now: value)" for one empire. The numbers are live values; the session counts them when the vote opens.</summary>
    public static List<string> VoteSourceLines(Empire e)
    {
        var lines = new List<string>();
        bool ru = IsRussian();
        int turn = CurTurn();
        lines.Add(Tr("База: ", "Base: ") + CouncilCore.BaseVotes + Tr(" голосов у каждого участника.", " votes for every member."));
        try
        {
            int n2 = 0, n3 = 0, n4 = 0;
            var interior = e.GetAgency<DepartmentOfTheInterior>();
            if (interior != null)
                foreach (ColonizedStarSystem cs in interior.ColonizedStarSystems)
                {
                    if (cs == null || cs.SimulationObject == null) continue;
                    int lv = cs.Level;
                    if (lv >= 4) n4++; else if (lv == 3) n3++; else if (lv == 2) n2++;
                }
            lines.Add(Tr("Уровни систем: 2-й уровень +5, 3-й +10, 4-й +20 за каждую систему (не складываются). Сейчас: ",
                         "System levels: level 2 +5, level 3 +10, level 4 +20 per system (not cumulative). Now: ")
                      + n2 + "/" + n3 + "/" + n4 + " = +" + (n2 * 5 + n3 * 10 + n4 * 20));
        }
        catch (Exception) { }
        try
        {
            lines.Add(Tr("Республика: +", "Republic: +") + CouncilCore.RepublicVotes + Tr(" голосов. Сейчас: ", " votes. Now: ") + (IsRepublic(e) ? Tr("да", "yes") : Tr("нет", "no")));
        }
        catch (Exception) { }
        try
        {
            int st = EmpireDevelopmentStage(e);
            lines.Add(Tr("Технологии «Развитие империи»: переход на Т3 даёт +10, на Т4 +20, на Т5 +30 (не складываются). Сейчас: Т" + st + " = +" + CouncilCore.TechStageVotes(st),
                         "Empire Development technologies: reaching T3 gives +10, T4 +20, T5 +30 (not cumulative). Now: T" + st + " = +" + CouncilCore.TechStageVotes(st)));
        }
        catch (Exception) { }
        try
        {
            var major = e as MajorEmpire;
            int hv = major == null ? 0 : HeroRework.CouncilVotesOf(major);
            lines.Add(Tr("Сенаторы: навыки героев-сенаторов с пометкой «голоса». Сейчас: +", "Senators: hero skills marked \"votes\". Now: +") + hv);
        }
        catch (Exception) { }
        lines.Add(Tr("Мандаты: +" + CouncilCore.MandateVotes + " за каждый обычный, Верховный легат +" + CouncilCore.SupremeMandateVotes + ".",
                     "Mandates: +" + CouncilCore.MandateVotes + " for each ordinary one, Supreme Legate +" + CouncilCore.SupremeMandateVotes + "."));
        lines.Add(Tr("Законы: «Заслуги» +30 выбранной фракции навсегда; «Стратегическое присутствие» даёт голоса за удерживаемые отмеченные системы.",
                     "Laws: Merit gives the chosen faction +30 permanently; Strategic Presence gives votes for held marked systems."));
        int wars = Core.RecentWars(e.Index, turn);
        lines.Add(Tr("Войны: −" + CouncilCore.WarPenaltyVotes + " голосов за каждую объявленную войну на " + Core.IntervalTurns + " ходов. Сейчас: −" + (wars * CouncilCore.WarPenaltyVotes),
                     "Wars: -" + CouncilCore.WarPenaltyVotes + " votes for every declared war for " + Core.IntervalTurns + " turns. Now: -" + (wars * CouncilCore.WarPenaltyVotes)));
        return lines;
    }

    // ---------------------------------------------------------------- "gives votes" notes in the game texts

    const string NoteMarker = "\u200B\u200B";           // invisible marker: the note is already there
    static List<string[]> noteTargets;                  // { localization key (no %), note ru, note en }
    static bool noteKeysResolved;
    public static string TextsStatus = "";

    static bool IsStageName(string name, out int stage)
    {
        stage = 0;
        const string a = "TechnologyStageDefinitionEmpireDevelopment";
        const string b = "TechnologyStageEmpireDevelopment";
        string tail = null;
        if (name.StartsWith(a, StringComparison.Ordinal)) tail = name.Substring(a.Length);
        else if (name.StartsWith(b, StringComparison.Ordinal)) tail = name.Substring(b.Length);
        if (tail == null || tail.Length != 1) return false;
        stage = tail[0] - '0';
        return stage >= 3 && stage <= 5;
    }

    static void ResolveNoteKeys()
    {
        noteKeysResolved = true;
        noteTargets = new List<string[]>();
        int republic = 0, stages = 0;
        try
        {
            var gui = Databases.GetDatabase<GuiElement>(false);
            if (gui == null) { TextsStatus = "GuiElement database not found"; return; }
            var all = gui.GetValues();
            var seen = new HashSet<string>();
            for (int i = 0; i < all.Length; i++)
            {
                var ge = all[i];
                if (ge == null) continue;
                string name = ge.Name.ToString();
                string desc = ge.Description;
                int stage;
                if (name.StartsWith("Government", StringComparison.Ordinal) && name.IndexOf("Republic", StringComparison.Ordinal) > 0 && name.IndexOf("Law", StringComparison.Ordinal) < 0)
                {
                    if (string.IsNullOrEmpty(desc) || desc[0] != '%' || !seen.Add(desc)) continue;
                    noteTargets.Add(new[] { desc.Substring(1),
                        "Галактический совет: республика даёт +" + CouncilCore.RepublicVotes + " голосов.",
                        "Galactic Council: the Republic gives +" + CouncilCore.RepublicVotes + " votes." });
                    republic++;
                }
                else if (IsStageName(name, out stage))
                {
                    string key = desc;
                    if (string.IsNullOrEmpty(key) || key[0] != '%')
                    {
                        key = "%CouncilTechStage" + stage + "Note";
                        ge.Description = key;
                        CustomHackingHelper.RegisterLoc(key.Substring(1), "");
                    }
                    if (!seen.Add(key)) continue;
                    noteTargets.Add(new[] { key.Substring(1),
                        "Галактический совет: переход на Т" + stage + " в ветке «Развитие империи» даёт +" + CouncilCore.TechStageVotes(stage) + " голосов.",
                        "Galactic Council: reaching T" + stage + " of the Empire Development branch gives +" + CouncilCore.TechStageVotes(stage) + " votes." });
                    stages++;
                }
            }
        }
        catch (Exception e) { Fail("ResolveNoteKeys", e); }
        TextsStatus = "Republic texts: " + republic + ", technology stage texts: " + stages;
    }

    /// <summary>Appends the "gives votes" notes to the game texts (once; again after a language change).</summary>
    static void EnsureVoteTexts()
    {
        try
        {
            if (!noteKeysResolved) ResolveNoteKeys();
            var db = Databases.GetDatabase<LocalizationDatatableElement>(false);
            if (db == null) return;
            for (int i = 0; i < noteTargets.Count; i++)
            {
                var t = noteTargets[i];
                LocalizationDatatableElement el;
                string cur = "";
                if (db.TryGetValue(t[0], out el) && el != null) cur = el.Value.ToString();
                if (cur.IndexOf(NoteMarker, StringComparison.Ordinal) >= 0) continue;
                string note = IsRussian() ? t[1] : t[2];
                CustomHackingHelper.RegisterLoc(t[0], (cur.Length > 0 ? cur + "\n\n" : "") + NoteMarker + note);
            }
        }
        catch (Exception e) { Fail("EnsureVoteTexts", e); }
    }
}
