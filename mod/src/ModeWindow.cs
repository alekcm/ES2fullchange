using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ES2.HackingModes
{
    /// <summary>
    /// Простое окно поверх игры: список операций взлома игрока и выбор режима для каждой.
    /// Рисуется через Unity IMGUI, потому что GUI самой игры описан в XML и из плагина не расширяется.
    /// </summary>
    internal class ModeWindow : MonoBehaviour
    {
        private const int WindowId = 0x4845434B; // "HECK"

        private static ModeWindow instance;
        private static readonly HashSet<ulong> KnownOperations = new HashSet<ulong>();

        private bool visible;
        private Rect windowRect = new Rect(40f, 120f, 520f, 420f);
        private Vector2 scroll = Vector2.zero;
        private float nextRefresh;
        private List<HackingOperation> operations = new List<HackingOperation>();
        private GUIStyle headerStyle;
        private GUIStyle tipStyle;
        private GUIStyle activeButton;

        internal static void NotifyNewOperation(HackingOperation operation)
        {
            if (operation == null)
            {
                return;
            }
            ulong guid = GameBridge.Guid(operation);
            if (guid == 0UL || KnownOperations.Contains(guid))
            {
                return;
            }
            KnownOperations.Add(guid);
            if (instance != null && Plugin.Cfg.AutoOpenOnNewOperation.Value)
            {
                instance.visible = true;
                instance.nextRefresh = 0f;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            try
            {
                if (Plugin.Cfg == null || !Plugin.Cfg.Enabled.Value)
                {
                    return;
                }
                if (Input.GetKeyDown(Plugin.Cfg.ToggleKey.Value))
                {
                    visible = !visible;
                    nextRefresh = 0f;
                }
                if (Time.unscaledTime >= nextRefresh)
                {
                    nextRefresh = Time.unscaledTime + 0.5f;
                    operations = GameBridge.LocalOperations();
                    DetectNewOperations();
                }
            }
            catch (Exception e)
            {
                Log.Error("Ошибка в Update окна: " + e.Message);
            }
        }

        private void DetectNewOperations()
        {
            for (int i = 0; i < operations.Count; i++)
            {
                ulong guid = GameBridge.Guid(operations[i]);
                if (guid == 0UL || KnownOperations.Contains(guid))
                {
                    continue;
                }
                KnownOperations.Add(guid);
                if (Plugin.Cfg.AutoOpenOnNewOperation.Value)
                {
                    visible = true;
                }
            }
        }

        private void OnGUI()
        {
            try
            {
                if (Plugin.Cfg == null || !Plugin.Cfg.Enabled.Value)
                {
                    return;
                }
                EnsureStyles();

                if (!visible)
                {
                    if (Plugin.Cfg.ShowCornerButton.Value && operations.Count > 0)
                    {
                        if (GUI.Button(new Rect(8f, 8f, 170f, 26f), Texts.CornerButton))
                        {
                            visible = true;
                        }
                    }
                    return;
                }

                windowRect = GUILayout.Window(WindowId, windowRect, DrawWindow, Texts.WindowTitle);
            }
            catch (Exception e)
            {
                Log.Error("Ошибка отрисовки окна: " + e.Message);
            }
        }

        private void EnsureStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(GUI.skin.label);
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.wordWrap = true;
            }
            if (tipStyle == null)
            {
                tipStyle = new GUIStyle(GUI.skin.label);
                tipStyle.wordWrap = true;
                tipStyle.fontSize = Mathf.Max(10, GUI.skin.label.fontSize - 1);
            }
            if (activeButton == null)
            {
                activeButton = new GUIStyle(GUI.skin.button);
                activeButton.fontStyle = FontStyle.Bold;
            }
        }

        private void DrawWindow(int id)
        {
            GUILayout.Space(4f);
            if (operations.Count == 0)
            {
                GUILayout.Label(Texts.NoOperations, tipStyle);
            }
            else
            {
                scroll = GUILayout.BeginScrollView(scroll);
                for (int i = 0; i < operations.Count; i++)
                {
                    DrawOperation(operations[i]);
                    GUILayout.Space(8f);
                }
                GUILayout.EndScrollView();
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.Label(Texts.HintHotkey + Plugin.Cfg.ToggleKey.Value, tipStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Texts.Close, GUILayout.Width(110f)))
            {
                visible = false;
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 22f));
        }

        private void DrawOperation(HackingOperation operation)
        {
            ulong guid = GameBridge.Guid(operation);
            OperationState state = ModeState.Get(guid);
            bool minor = GameBridge.IsMinorTarget(operation);

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(GameBridge.TargetName(operation), headerStyle);

            string status;
            if (state.Mode == HackMode.Normal)
            {
                status = Texts.Running;
            }
            else
            {
                status = Texts.Frozen;
                if (GameBridge.HasReachedTarget(operation))
                {
                    status += ", " + Texts.Arrived;
                }
            }
            GUILayout.Label(Texts.CurrentMode + Texts.ModeOf(state.Mode) + "  [" + status + "]", tipStyle);

            GUILayout.BeginHorizontal();
            if (minor)
            {
                DrawModeButton(operation, state, HackMode.MinorSteal, Texts.ModeSteal);
                DrawModeButton(operation, state, HackMode.MinorRelations, Texts.ModeRelations);
                if (Plugin.Cfg.MinorAllowNormalOperation.Value)
                {
                    DrawModeButton(operation, state, HackMode.Normal, Texts.ModeNormal);
                }
            }
            else
            {
                DrawModeButton(operation, state, HackMode.ScienceCopy, Texts.ModeScience);
                DrawModeButton(operation, state, HackMode.Sleepers, Texts.ModeSleepers);
                DrawModeButton(operation, state, HackMode.Normal, Texts.ModeNormal);
            }
            GUILayout.EndHorizontal();

            GUILayout.Label(Tip(operation, state.Mode, minor), tipStyle);

            if (!string.IsNullOrEmpty(state.LastResult))
            {
                GUILayout.Label(Texts.LastTurn + state.LastResult, tipStyle);
            }
            GUILayout.EndVertical();
        }

        private void DrawModeButton(HackingOperation operation, OperationState state, HackMode mode, string label)
        {
            bool selected = state.Mode == mode;
            string caption = selected ? "• " + label : label;
            if (GUILayout.Button(caption, selected ? activeButton : GUI.skin.button))
            {
                ModeState.SetMode(GameBridge.Guid(operation), mode);
                Log.Info("Режим операции " + GameBridge.TargetName(operation) + " → " + Texts.ModeOf(mode));
            }
        }

        private string Tip(HackingOperation operation, HackMode mode, bool minor)
        {
            switch (mode)
            {
                case HackMode.ScienceCopy:
                    return string.Format(CultureInfo.InvariantCulture, Texts.TipScience, Plugin.Cfg.SciencePercent.Value)
                        + "  " + TurnEffects.Preview(operation, HackMode.ScienceCopy);
                case HackMode.Sleepers:
                    return string.Format(CultureInfo.InvariantCulture, Texts.TipSleepers, Plugin.Cfg.SleeperPeriod.Value);
                case HackMode.MinorSteal:
                    return string.Format(CultureInfo.InvariantCulture, Texts.TipSteal, Plugin.Cfg.MinorStealPercent.Value)
                        + "  " + TurnEffects.Preview(operation, HackMode.MinorSteal);
                case HackMode.MinorRelations:
                    return string.Format(CultureInfo.InvariantCulture, Texts.TipRelations, Plugin.Cfg.RelationPerTurn.Value);
                default:
                    return Texts.TipNormal;
            }
        }
    }
}
