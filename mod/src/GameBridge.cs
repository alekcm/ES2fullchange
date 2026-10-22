using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Amplitude.Unity.Framework;

namespace ES2.HackingModes
{
    /// <summary>
    /// Тонкая прослойка над игровыми сервисами Endless Space 2.
    /// Все обращения к игре идут отсюда, чтобы остальной код не падал, когда игра не загружена.
    /// </summary>
    internal static class GameBridge
    {
        internal static T Service<T>() where T : class, IService
        {
            try
            {
                return Services.GetService<T>();
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static IHackingService Hacking
        {
            get { return Service<IHackingService>(); }
        }

        internal static PlayerController ActivePlayerController
        {
            get
            {
                IPlayerControllerRepositoryService repository = Service<IPlayerControllerRepositoryService>();
                return repository == null ? null : repository.ActivePlayerController;
            }
        }

        /// <summary>Империя игрока, сидящего за клавиатурой.</summary>
        internal static Empire LocalEmpire
        {
            get
            {
                PlayerController controller = ActivePlayerController;
                if (controller == null)
                {
                    return null;
                }
                return controller.Empire as Empire;
            }
        }

        internal static int CurrentTurn
        {
            get
            {
                IEndTurnService endTurn = Service<IEndTurnService>();
                return endTurn == null ? -1 : endTurn.Turn;
            }
        }

        /// <summary>Операции взлома локального игрока (пустой список, если игра не идёт).</summary>
        internal static List<HackingOperation> LocalOperations()
        {
            List<HackingOperation> result = new List<HackingOperation>();
            try
            {
                IHackingService hacking = Hacking;
                Empire empire = LocalEmpire;
                if (hacking == null || empire == null)
                {
                    return result;
                }
                ReadOnlyCollection<HackingOperation> operations = hacking.GetEmpireHackingOperations(empire);
                if (operations == null)
                {
                    return result;
                }
                for (int i = 0; i < operations.Count; i++)
                {
                    HackingOperation operation = operations[i];
                    if (operation == null || operation.IsFinalized)
                    {
                        continue;
                    }
                    if (operation.IsTraceOperation)
                    {
                        // Операции слежки (trace) не получают режимов — это ответный взлом.
                        continue;
                    }
                    result.Add(operation);
                }
            }
            catch (Exception e)
            {
                Log.Error("Не удалось получить список операций: " + e.Message);
            }
            return result;
        }

        /// <summary>Колония, на которую нацелена операция (null, если цель — не колония).</summary>
        internal static ColonizedStarSystem TargetColony(HackingOperation operation)
        {
            if (operation == null)
            {
                return null;
            }
            try
            {
                ColonizedStarSystem colony = operation.GetColonyOnTargetNode();
                if (colony == null)
                {
                    colony = operation.TargetSystem;
                }
                return colony;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Цель принадлежит малой фракции?</summary>
        internal static bool IsMinorTarget(HackingOperation operation)
        {
            ColonizedStarSystem colony = TargetColony(operation);
            return colony != null && colony.Empire is MinorEmpire;
        }

        /// <summary>Цель принадлежит большой империи (обычный объект взлома)?</summary>
        internal static bool IsMajorTarget(HackingOperation operation)
        {
            ColonizedStarSystem colony = TargetColony(operation);
            return colony != null && colony.Empire is MajorEmpire;
        }

        internal static string TargetName(HackingOperation operation)
        {
            ColonizedStarSystem colony = TargetColony(operation);
            if (colony != null)
            {
                string owner = colony.Empire == null ? string.Empty : SafeName(colony.Empire);
                string system = Safe(colony.LocalizedName);
                return string.IsNullOrEmpty(owner) ? system : system + " (" + owner + ")";
            }
            if (operation != null && operation.Target != null)
            {
                return Safe(operation.Target.LocalizedName);
            }
            return "?";
        }

        internal static string SafeName(Amplitude.Unity.Game.Empire empire)
        {
            try
            {
                MajorEmpire major = empire as MajorEmpire;
                if (major != null)
                {
                    return Safe(major.LocalizedName);
                }
                MinorEmpire minor = empire as MinorEmpire;
                if (minor != null)
                {
                    return Safe(minor.LocalizedName);
                }
                return empire == null ? string.Empty : empire.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static string Safe(string value)
        {
            return string.IsNullOrEmpty(value) ? "?" : value;
        }

        /// <summary>Операция уже дотянулась до целевого узла?</summary>
        internal static bool HasReachedTarget(HackingOperation operation)
        {
            try
            {
                HackingOperationStep[] steps = operation.Steps;
                if (steps == null || steps.Length == 0)
                {
                    return false;
                }
                return operation.CurrentStepIndex >= steps.Length - 1 && steps[steps.Length - 1].IsFinished;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static ulong Guid(HackingOperation operation)
        {
            return operation == null ? 0UL : (ulong)operation.GUID;
        }
    }
}
