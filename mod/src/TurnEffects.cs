using System;
using System.Collections.Generic;
using System.Globalization;
using Amplitude;
using Amplitude.Unity.Simulation;

namespace ES2.HackingModes
{
    /// <summary>
    /// Раз в ход прогоняет все операции взлома локального игрока и применяет эффект выбранного режима.
    /// Вызывается из патча на HackingManager.ClientEndTurn.
    /// </summary>
    internal static class TurnEffects
    {
        private static int lastProcessedTurn = int.MinValue;

        internal static void Tick()
        {
            if (!Plugin.Cfg.Enabled.Value)
            {
                return;
            }

            int turn = GameBridge.CurrentTurn;
            if (turn < 0 || turn == lastProcessedTurn)
            {
                return;
            }
            lastProcessedTurn = turn;

            List<HackingOperation> operations = GameBridge.LocalOperations();
            for (int i = 0; i < operations.Count; i++)
            {
                HackingOperation operation = operations[i];
                try
                {
                    Apply(operation, turn);
                }
                catch (Exception e)
                {
                    Log.Error("Ошибка при применении режима взлома: " + e);
                }
            }
        }

        private static void Apply(HackingOperation operation, int turn)
        {
            ulong guid = GameBridge.Guid(operation);
            OperationState state = ModeState.Get(guid);
            if (state.Mode == HackMode.Normal)
            {
                return;
            }

            ColonizedStarSystem colony = GameBridge.TargetColony(operation);
            if (colony == null || colony.Empire == null)
            {
                return;
            }

            Empire hacker = operation.Start == null ? null : operation.Start.Empire;
            if (hacker == null)
            {
                return;
            }

            if (Plugin.Cfg.EffectsRequireArrival.Value && !GameBridge.HasReachedTarget(operation))
            {
                state.LastResult = Texts.WaitingForArrival;
                return;
            }

            state.LastAppliedTurn = turn;

            switch (state.Mode)
            {
                case HackMode.ScienceCopy:
                    ApplyScienceCopy(state, hacker, colony);
                    break;
                case HackMode.Sleepers:
                    ApplySleepers(state, hacker, colony, turn);
                    break;
                case HackMode.MinorSteal:
                    ApplyMinorSteal(state, hacker, colony);
                    break;
                case HackMode.MinorRelations:
                    ApplyMinorRelations(state, hacker, colony);
                    break;
            }
        }

        // ------------------------------------------------------------------
        // "Подглядывание за идеями": копия части науки цели, жертва ничего не теряет.
        // ------------------------------------------------------------------
        private static void ApplyScienceCopy(OperationState state, Empire hacker, ColonizedStarSystem colony)
        {
            float production = GetProperty(colony, SimulationProperties.StarSystem.NetSystemResearch);
            float gain = production * (Plugin.Cfg.SciencePercent.Value / 100f);
            if (gain <= 0f)
            {
                state.LastResult = Texts.NothingToCopy;
                return;
            }

            if (AddResource(hacker, DepartmentOfTheTreasury.Resources.EmpireResearch, gain))
            {
                state.LastResult = string.Format(CultureInfo.InvariantCulture, Texts.ScienceGained, gain);
            }
        }

        // ------------------------------------------------------------------
        // "Внедрение шпионов": раз в N ходов в целевую систему внедряется спящий.
        // ------------------------------------------------------------------
        private static void ApplySleepers(OperationState state, Empire hacker, ColonizedStarSystem colony, int turn)
        {
            int period = Math.Max(1, Plugin.Cfg.SleeperPeriod.Value);
            if (state.LastSleeperTurn != int.MinValue && turn - state.LastSleeperTurn < period)
            {
                int left = period - (turn - state.LastSleeperTurn);
                state.LastResult = string.Format(CultureInfo.InvariantCulture, Texts.SleeperCountdown, left);
                return;
            }

            PlayerController controller = GameBridge.ActivePlayerController;
            if (controller == null)
            {
                return;
            }

            // Ровно тот же приказ, который отдаёт ванильный исход взлома "внедрить агентов".
            controller.PostOrder(new OrderSpawnTraitorPopulation(hacker.Index, colony.GUID));
            state.LastSleeperTurn = turn;
            state.LastResult = Texts.SleeperSent;
            ModeState.Save();
        }

        // ------------------------------------------------------------------
        // Малая фракция: воровство процента производства (жертва теряет).
        // ------------------------------------------------------------------
        private static void ApplyMinorSteal(OperationState state, Empire hacker, ColonizedStarSystem colony)
        {
            float percent = Plugin.Cfg.MinorStealPercent.Value / 100f;
            float dust = StealResource(hacker, colony, SimulationProperties.StarSystem.NetSystemMoney,
                DepartmentOfTheTreasury.Resources.EmpireMoney, percent);
            float science = StealResource(hacker, colony, SimulationProperties.StarSystem.NetSystemResearch,
                DepartmentOfTheTreasury.Resources.EmpireResearch, percent);
            float influence = StealResource(hacker, colony, SimulationProperties.StarSystem.NetSystemEmpirePoint,
                DepartmentOfTheTreasury.Resources.EmpireEmpirePoint, percent);

            if (dust <= 0f && science <= 0f && influence <= 0f)
            {
                state.LastResult = Texts.NothingToSteal;
                return;
            }
            state.LastResult = string.Format(CultureInfo.InvariantCulture, Texts.Stolen, dust, science, influence);
        }

        private static float StealResource(Empire hacker, ColonizedStarSystem colony, StaticString systemProperty,
            StaticString resourceName, float percent)
        {
            float amount = GetProperty(colony, systemProperty) * percent;
            if (amount <= 0f)
            {
                return 0f;
            }

            if (!AddResource(hacker, resourceName, amount))
            {
                return 0f;
            }

            if (Plugin.Cfg.StealHurtsVictim.Value)
            {
                // Отнимаем у жертвы столько же — это именно воровство, а не копия.
                AddResource(colony.Empire as Empire, resourceName, -amount);
            }
            return amount;
        }

        // ------------------------------------------------------------------
        // Малая фракция: +N к отношениям каждый ход.
        // ------------------------------------------------------------------
        private static void ApplyMinorRelations(OperationState state, Empire hacker, ColonizedStarSystem colony)
        {
            MinorEmpire minor = colony.Empire as MinorEmpire;
            MajorEmpire major = hacker as MajorEmpire;
            if (minor == null || major == null)
            {
                return;
            }

            DepartmentOfForeignAffairs foreignAffairs = minor.GetAgency<DepartmentOfForeignAffairs>();
            if (foreignAffairs == null)
            {
                return;
            }

            float current = foreignAffairs.GetMinorRelationPoints(major);
            float cap = Plugin.Cfg.RelationCap.Value;
            float next = current + Plugin.Cfg.RelationPerTurn.Value;
            if (next > cap)
            {
                next = cap;
            }
            if (next <= current)
            {
                state.LastResult = Texts.RelationsMaxed;
                return;
            }

            PlayerController controller = GameBridge.ActivePlayerController;
            if (controller == null)
            {
                return;
            }

            controller.PostOrder(new OrderSetMinorRelationPoints(hacker.Index, minor.Index, next));
            state.LastResult = string.Format(CultureInfo.InvariantCulture, Texts.RelationsGained, next - current, next);
        }

        // ------------------------------------------------------------------
        // Утилиты
        // ------------------------------------------------------------------
        internal static float GetProperty(SimulationObjectWrapper wrapper, StaticString property)
        {
            try
            {
                return wrapper == null ? 0f : wrapper.GetPropertyValue(property);
            }
            catch (Exception)
            {
                return 0f;
            }
        }

        private static bool AddResource(Empire empire, StaticString resourceName, float amount)
        {
            if (empire == null || amount == 0f)
            {
                return false;
            }
            try
            {
                DepartmentOfTheTreasury treasury = empire.GetAgency<DepartmentOfTheTreasury>();
                if (treasury == null)
                {
                    return false;
                }
                SimulationObject simulationObject = empire.SimulationObject;
                if (simulationObject == null)
                {
                    return false;
                }
                return treasury.TryTransferResources(simulationObject, resourceName, amount, true);
            }
            catch (Exception e)
            {
                Log.Error("Не удалось изменить ресурс " + resourceName + ": " + e.Message);
                return false;
            }
        }

        /// <summary>Оценка выгоды режима за ход — для показа в окне выбора.</summary>
        internal static string Preview(HackingOperation operation, HackMode mode)
        {
            ColonizedStarSystem colony = GameBridge.TargetColony(operation);
            if (colony == null)
            {
                return string.Empty;
            }
            switch (mode)
            {
                case HackMode.ScienceCopy:
                    return string.Format(CultureInfo.InvariantCulture, "+{0:0.#}",
                        GetProperty(colony, SimulationProperties.StarSystem.NetSystemResearch) * (Plugin.Cfg.SciencePercent.Value / 100f));
                case HackMode.MinorSteal:
                    float percent = Plugin.Cfg.MinorStealPercent.Value / 100f;
                    return string.Format(CultureInfo.InvariantCulture, "+{0:0.#} / +{1:0.#} / +{2:0.#}",
                        GetProperty(colony, SimulationProperties.StarSystem.NetSystemMoney) * percent,
                        GetProperty(colony, SimulationProperties.StarSystem.NetSystemResearch) * percent,
                        GetProperty(colony, SimulationProperties.StarSystem.NetSystemEmpirePoint) * percent);
                default:
                    return string.Empty;
            }
        }
    }
}
