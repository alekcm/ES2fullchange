using System;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace ES2.HackingModes
{
    /// <summary>
    /// Точка входа BepInEx. Вешает патчи Harmony и создаёт объект с окном выбора режимов.
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "arena.es2.hackingmodes";
        public const string PluginName = "ES2 Hacking Modes";
        public const string PluginVersion = "1.0.0";

        internal static ModConfig Cfg;

        private void Awake()
        {
            Log.Source = Logger;
            Cfg = new ModConfig(Config);
            ModeState.Initialize(Path.Combine(Paths.ConfigPath, "ES2HackingModes.state.txt"));

            Harmony harmony = new Harmony(PluginGuid);
            ApplyPatch(harmony, typeof(Patch_NodeStepSpeed));
            ApplyPatch(harmony, typeof(Patch_MoveStepSpeed));
            ApplyPatch(harmony, typeof(Patch_TurnTick));
            ApplyPatch(harmony, typeof(Patch_StartOperation));
            ApplyPatch(harmony, typeof(Patch_KeepPassiveOperationAlive));

            GameObject host = new GameObject("ES2HackingModesUI");
            host.AddComponent<ModeWindow>();
            UnityEngine.Object.DontDestroyOnLoad(host);

            Log.Info(PluginName + " " + PluginVersion + " загружен. Окно режимов: " + Cfg.ToggleKey.Value);
        }

        /// <summary>Каждый патч ставим отдельно, чтобы одна ошибка не убила остальные.</summary>
        private static void ApplyPatch(Harmony harmony, Type patchClass)
        {
            try
            {
                harmony.CreateClassProcessor(patchClass).Patch();
                Log.Debug("Патч применён: " + patchClass.Name);
            }
            catch (Exception e)
            {
                Log.Error("Патч " + patchClass.Name + " не применился: " + e);
            }
        }
    }
}
