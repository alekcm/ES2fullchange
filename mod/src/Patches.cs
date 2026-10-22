using System;
using HarmonyLib;

namespace ES2.HackingModes
{
    // ----------------------------------------------------------------------
    // Скорость взлома = 0, пока операция в пассивном режиме.
    // В ванили HackingOperation.ClientEndTurn прекращает набор прогресса,
    // если множитель скорости шага <= 0, а GetRemainingTurns возвращает "никогда".
    // ----------------------------------------------------------------------
    [HarmonyPatch(typeof(NodeHackingOperationStep), "GetSpeedMultiplier")]
    internal static class Patch_NodeStepSpeed
    {
        // __1 — второй аргумент метода (HackingOperation operation)
        private static void Postfix(HackingOperation __1, ref float __result)
        {
            SpeedPatchHelper.Apply(__1, ref __result);
        }
    }

    [HarmonyPatch(typeof(MoveHackingOperationStep), "GetSpeedMultiplier")]
    internal static class Patch_MoveStepSpeed
    {
        private static void Postfix(HackingOperation __1, ref float __result)
        {
            SpeedPatchHelper.Apply(__1, ref __result);
        }
    }

    internal static class SpeedPatchHelper
    {
        internal static void Apply(HackingOperation operation, ref float result)
        {
            try
            {
                if (operation == null || Plugin.Cfg == null || !Plugin.Cfg.Enabled.Value)
                {
                    return;
                }
                if (ModeState.IsPassive((ulong)operation.GUID))
                {
                    result = 0f;
                }
            }
            catch (Exception)
            {
                // В интерфейсе этот метод дёргается часто — молчим, чтобы не засорять лог.
            }
        }
    }

    // ----------------------------------------------------------------------
    // Раз в ход применяем эффекты режимов.
    // ----------------------------------------------------------------------
    [HarmonyPatch(typeof(HackingManager), "ClientEndTurn")]
    internal static class Patch_TurnTick
    {
        private static void Postfix()
        {
            try
            {
                TurnEffects.Tick();
            }
            catch (Exception e)
            {
                Log.Error("Ошибка в обработке конца хода: " + e);
            }
        }
    }

    // ----------------------------------------------------------------------
    // Новая операция — предложить выбор режима сразу же.
    // ----------------------------------------------------------------------
    [HarmonyPatch(typeof(HackingManager), "StartHackingOperation",
        new Type[] { typeof(GameEntityGUID), typeof(IHackingOperationStart), typeof(IHackingOperationTarget), typeof(NodePosition[]), typeof(GameEntityGUID) })]
    internal static class Patch_StartOperation
    {
        private static void Postfix(HackingOperation __result)
        {
            try
            {
                if (__result == null || Plugin.Cfg == null || !Plugin.Cfg.Enabled.Value)
                {
                    return;
                }
                ModeWindow.NotifyNewOperation(__result);
            }
            catch (Exception e)
            {
                Log.Error("Ошибка при создании операции: " + e);
            }
        }
    }

    // ----------------------------------------------------------------------
    // Операция в пассивном режиме не "срабатывает", даже если дошла до цели:
    // она просто висит подключённой к системе и кормит нас эффектом.
    // ----------------------------------------------------------------------
    [HarmonyPatch(typeof(HackingOperation), "ClientBeginTurn")]
    internal static class Patch_KeepPassiveOperationAlive
    {
        private static bool Prefix(HackingOperation __instance)
        {
            try
            {
                if (__instance == null || Plugin.Cfg == null || !Plugin.Cfg.Enabled.Value)
                {
                    return true;
                }
                if (!ModeState.IsPassive((ulong)__instance.GUID))
                {
                    return true;
                }
                HackingOperationStep[] steps = __instance.Steps;
                if (steps == null)
                {
                    return true;
                }
                // Ванильный код завершает операцию только когда пройдены все шаги —
                // в этом случае (и только в нём) мы его пропускаем.
                if (__instance.CurrentStepIndex >= steps.Length)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Error("Ошибка в ClientBeginTurn: " + e);
            }
            return true;
        }
    }
}
