using BepInEx.Configuration;
using UnityEngine;

namespace ES2.HackingModes
{
    internal class ModConfig
    {
        internal readonly ConfigEntry<bool> Enabled;
        internal readonly ConfigEntry<float> SciencePercent;
        internal readonly ConfigEntry<int> SleeperPeriod;
        internal readonly ConfigEntry<float> MinorStealPercent;
        internal readonly ConfigEntry<bool> StealHurtsVictim;
        internal readonly ConfigEntry<float> RelationPerTurn;
        internal readonly ConfigEntry<float> RelationCap;
        internal readonly ConfigEntry<bool> MinorAllowNormalOperation;
        internal readonly ConfigEntry<bool> EffectsRequireArrival;
        internal readonly ConfigEntry<KeyCode> ToggleKey;
        internal readonly ConfigEntry<bool> AutoOpenOnNewOperation;
        internal readonly ConfigEntry<bool> ShowCornerButton;
        internal readonly ConfigEntry<bool> EnglishLabels;

        internal ModConfig(ConfigFile file)
        {
            Enabled = file.Bind("1. Общее", "Enabled", true,
                "Включить мод. false — игра работает как в ванили.");

            SciencePercent = file.Bind("2. Подглядывание за идеями", "SciencePercent", 20f,
                "Сколько процентов науки целевой системы копируется вам каждый ход. Цель ничего не теряет.");

            SleeperPeriod = file.Bind("3. Внедрение шпионов", "SleeperPeriod", 5,
                "Раз во сколько ходов в целевую систему внедряется спящий.");

            MinorStealPercent = file.Bind("4. Малые фракции: воровство", "StealPercent", 20f,
                "Сколько процентов производства системы малой фракции воруется каждый ход.");

            StealHurtsVictim = file.Bind("4. Малые фракции: воровство", "StealHurtsVictim", true,
                "true — украденное вычитается у малой фракции (настоящее воровство). false — просто копия.");

            RelationPerTurn = file.Bind("5. Малые фракции: отношения", "RelationPerTurn", 10f,
                "Сколько очков отношений с малой фракцией добавляется каждый ход.");

            RelationCap = file.Bind("5. Малые фракции: отношения", "RelationCap", 100f,
                "Потолок очков отношений, выше которого мод не поднимает.");

            MinorAllowNormalOperation = file.Bind("5. Малые фракции: отношения", "AllowNormalOperation", false,
                "true — для малых фракций в окне появится и кнопка 'Пустить операцию' (обычный взлом).");

            EffectsRequireArrival = file.Bind("1. Общее", "EffectsRequireArrival", false,
                "true — пассивные режимы начинают работать только когда операция дошла до целевой системы.");

            ToggleKey = file.Bind("6. Интерфейс", "ToggleKey", KeyCode.F9,
                "Клавиша, открывающая/закрывающая окно выбора режимов.");

            AutoOpenOnNewOperation = file.Bind("6. Интерфейс", "AutoOpenOnNewOperation", true,
                "Открывать окно автоматически, как только проложена новая операция взлома.");

            ShowCornerButton = file.Bind("6. Интерфейс", "ShowCornerButton", true,
                "Показывать маленькую кнопку в углу экрана, когда окно закрыто.");

            EnglishLabels = file.Bind("6. Интерфейс", "EnglishLabels", false,
                "true — подписи мода на английском (на случай, если шрифт не тянет кириллицу).");
        }
    }
}
