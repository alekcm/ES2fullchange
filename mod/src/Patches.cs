using System;
using HarmonyLib;

namespace ES2.HackingModes
{
    // ----------------------------------------------------------------------
    // Скорость взлома = 0 для пассивных режимов.
    //
    // По умолчанию обнуление включается, когда операция дотянулась до целевой системы:
    // линия взлома проложена и висит подключённой, но внутрь система так и не вскрывается,
    // то есть ванильный исход никогда не применяется и операция не исчезает.
    // Если в конфиге FreezeImmediately = true, скорость обнуляется сразу после выбора режима.
    //
    // Ваниль это переживает штатно: HackingOperation.ClientEndTurn не набирает прогресс
    // при множителе <= 0, а GetRemainingTurns в этом случае возвращает int.MaxValue.
    // ----------------------------------------------------------------------
    [HarmonyPatch(typeof(NodeHackingOperationStep), "GetSpeedMultiplier")]
    internal static class Patch_NodeStepSpeed
    {
        // __1 — второй аргумент метода: HackingOperation operation
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
                if (!ModeState.IsPassive((ulong)operation.GUID))
                {
                    return;
                }
                if (Plugin.Cfg.FreezeImmediately.Value || GameBridge.IsConnected(operation))
                {
                    result = 0f;
                }
            }
            catch (Exception)
            {
                // Метод дёргается интерфейсом каждый кадр — молча игнорируем, чтобы не залить лог.
            }
        }
    }

    // ----------------------------------------------------------------------
    // Раз в ход применяем эффекты режимов. Патчим и начало, и конец хода:
    // внутри TurnEffects стоит защёлка по номеру хода, так что эффект применится один раз.
    // ----------------------------------------------------------------------
    [HarmonyPatch(typeof(HackingManager), "ClientBeginTurn")]
    internal static class Patch_TurnTickBegin
    {
        private static void Postfix()
        {
            SafeTick();
        }

        internal static void SafeTick()
        {
            try
            {
                TurnEffects.Tick();
            }
            catch (Exception e)
            {
                Log.Error("Ошибка в обработке хода: " + e);
            }
        }
    }

    [HarmonyPatch(typeof(HackingManager), "ClientEndTurn")]
    internal static class Patch_TurnTickEnd
    {
        private static void Postfix()
        {
            Patch_TurnTickBegin.SafeTick();
        }
    }

    // ----------------------------------------------------------------------
    // Новая операция — сразу предложить выбор режима.
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
    // Страховка: если операция в пассивном режиме всё-таки прошла все шаги
    // (например, режим переключили, когда взлом уже добрался до цели),
    // не даём ванили применить исход и убить операцию — она остаётся подключённой.
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
                if (GameBridge.IsCompleted(__instance))
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Error("Ошибка в ClientBeginTurn операции: " + e);
            }
            return true;
        }
    }
}
