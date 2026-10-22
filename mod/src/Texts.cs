namespace ES2.HackingModes
{
    /// <summary>Все подписи мода. Русский по умолчанию, английский — через конфиг.</summary>
    internal static class Texts
    {
        private static bool En
        {
            get { return Plugin.Cfg != null && Plugin.Cfg.EnglishLabels.Value; }
        }

        internal static string WindowTitle
        {
            get { return En ? "Hacking: operation modes" : "Взлом: режимы операций"; }
        }

        internal static string CornerButton
        {
            get { return En ? "Hacking modes" : "Режимы взлома"; }
        }

        internal static string NoOperations
        {
            get { return En ? "No active hacking operations." : "Активных операций взлома нет."; }
        }

        internal static string Close
        {
            get { return En ? "Close" : "Закрыть"; }
        }

        internal static string ModeNormal
        {
            get { return En ? "Run the operation" : "Пустить операцию"; }
        }

        internal static string ModeScience
        {
            get { return En ? "Peek at ideas" : "Подглядывание за идеями"; }
        }

        internal static string ModeSleepers
        {
            get { return En ? "Insert agents" : "Внедрение шпионов"; }
        }

        internal static string ModeSteal
        {
            get { return En ? "Steal resources" : "Воровство ресурсов"; }
        }

        internal static string ModeRelations
        {
            get { return En ? "Improve relations" : "Улучшение отношений"; }
        }

        internal static string CurrentMode
        {
            get { return En ? "Mode: " : "Режим: "; }
        }

        internal static string Frozen
        {
            get { return En ? "hacking speed 0" : "скорость взлома 0"; }
        }

        internal static string Running
        {
            get { return En ? "hacking in progress" : "взлом идёт"; }
        }

        internal static string Arrived
        {
            get { return En ? "connected to the system" : "подключён к системе"; }
        }

        internal static string LastTurn
        {
            get { return En ? "Last turn: " : "Прошлый ход: "; }
        }

        internal static string TipScience
        {
            get
            {
                return En
                    ? "You copy {0}% of the system science. The victim loses nothing."
                    : "Вы копируете {0}% науки системы. Жертва ничего не теряет.";
            }
        }

        internal static string TipSleepers
        {
            get
            {
                return En
                    ? "A sleeper is inserted into the system every {0} turns."
                    : "Раз в {0} ходов в систему внедряется спящий.";
            }
        }

        internal static string TipSteal
        {
            get
            {
                return En
                    ? "You steal {0}% of the minor faction production (dust / science / influence). They lose it."
                    : "Вы воруете {0}% производства малой фракции (пыль / наука / влияние). Она это теряет.";
            }
        }

        internal static string TipRelations
        {
            get
            {
                return En
                    ? "+{0} relations with the minor faction every turn."
                    : "+{0} к отношениям с малой фракцией каждый ход.";
            }
        }

        internal static string TipNormal
        {
            get
            {
                return En
                    ? "Vanilla behaviour: the operation advances and finishes with a normal outcome."
                    : "Как в ванили: операция идёт и завершается обычным исходом.";
            }
        }

        internal static string ScienceGained
        {
            get { return En ? "+{0:0.#} science" : "+{0:0.#} науки"; }
        }

        internal static string NothingToCopy
        {
            get { return En ? "the system produces no science" : "система не производит науку"; }
        }

        internal static string NothingToSteal
        {
            get { return En ? "nothing to steal" : "воровать нечего"; }
        }

        internal static string Stolen
        {
            get { return En ? "stolen: {0:0.#} dust / {1:0.#} science / {2:0.#} influence" : "украдено: {0:0.#} пыли / {1:0.#} науки / {2:0.#} влияния"; }
        }

        internal static string SleeperSent
        {
            get { return En ? "a sleeper has been inserted" : "внедрён спящий"; }
        }

        internal static string SleeperCountdown
        {
            get { return En ? "next sleeper in {0} turn(s)" : "следующий спящий через {0} ход(ов)"; }
        }

        internal static string RelationsGained
        {
            get { return En ? "+{0:0.#} relations (now {1:0.#})" : "+{0:0.#} к отношениям (стало {1:0.#})"; }
        }

        internal static string RelationsMaxed
        {
            get { return En ? "relations are already at the cap" : "отношения уже на максимуме"; }
        }

        internal static string WaitingForArrival
        {
            get { return En ? "waiting until the route reaches the system" : "ждём, пока взлом дойдёт до системы"; }
        }

        internal static string HintHotkey
        {
            get { return En ? "Hotkey: " : "Горячая клавиша: "; }
        }

        internal static string ModeOf(HackMode mode)
        {
            switch (mode)
            {
                case HackMode.ScienceCopy:
                    return ModeScience;
                case HackMode.Sleepers:
                    return ModeSleepers;
                case HackMode.MinorSteal:
                    return ModeSteal;
                case HackMode.MinorRelations:
                    return ModeRelations;
                default:
                    return ModeNormal;
            }
        }
    }
}
