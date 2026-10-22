using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ES2.HackingModes
{
    /// <summary>Режим, в котором работает конкретная операция взлома.</summary>
    internal enum HackMode
    {
        /// <summary>"Пустить операцию" — ванильное поведение.</summary>
        Normal = 0,

        /// <summary>"Подглядывание за идеями" — копия 20% науки системы, цель ничего не теряет.</summary>
        ScienceCopy = 1,

        /// <summary>"Внедрение шпионов" — раз в N ходов в систему внедряется спящий.</summary>
        Sleepers = 2,

        /// <summary>Малая фракция: воровство 20% ресурсов (жертва теряет).</summary>
        MinorSteal = 3,

        /// <summary>Малая фракция: +10 к отношениям в ход.</summary>
        MinorRelations = 4
    }

    internal class OperationState
    {
        public HackMode Mode = HackMode.Normal;
        public int LastSleeperTurn = int.MinValue;
        public int LastAppliedTurn = int.MinValue;

        /// <summary>Описание того, что режим дал в прошлый ход (для окна интерфейса).</summary>
        public string LastResult = string.Empty;
    }

    /// <summary>
    /// Хранилище режимов по GUID операции. Пишется в текстовый файл рядом с конфигом BepInEx,
    /// чтобы режимы переживали перезагрузку игры/сейва.
    /// </summary>
    internal static class ModeState
    {
        private static readonly Dictionary<ulong, OperationState> States = new Dictionary<ulong, OperationState>();
        private static string statePath;
        private static bool loaded;

        internal static void Initialize(string path)
        {
            statePath = path;
            Load();
        }

        internal static OperationState Get(ulong guid)
        {
            OperationState state;
            if (!States.TryGetValue(guid, out state))
            {
                state = new OperationState();
                States[guid] = state;
            }
            return state;
        }

        internal static HackMode GetMode(ulong guid)
        {
            OperationState state;
            return States.TryGetValue(guid, out state) ? state.Mode : HackMode.Normal;
        }

        internal static bool IsPassive(ulong guid)
        {
            return GetMode(guid) != HackMode.Normal;
        }

        internal static void SetMode(ulong guid, HackMode mode)
        {
            OperationState state = Get(guid);
            if (state.Mode == mode)
            {
                return;
            }
            state.Mode = mode;
            state.LastResult = string.Empty;
            // Таймер спящих сбрасываем, чтобы первый агент пошёл через полный период.
            state.LastSleeperTurn = int.MinValue;
            Save();
        }

        internal static void Forget(ulong guid)
        {
            if (States.Remove(guid))
            {
                Save();
            }
        }

        internal static void Load()
        {
            if (loaded || string.IsNullOrEmpty(statePath))
            {
                return;
            }
            loaded = true;
            try
            {
                if (!File.Exists(statePath))
                {
                    return;
                }
                string[] lines = File.ReadAllLines(statePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (line.Length == 0 || line[0] == '#')
                    {
                        continue;
                    }
                    string[] parts = line.Split(';');
                    if (parts.Length < 2)
                    {
                        continue;
                    }
                    ulong guid;
                    if (!ulong.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out guid))
                    {
                        continue;
                    }
                    OperationState state = new OperationState();
                    try
                    {
                        state.Mode = (HackMode)Enum.Parse(typeof(HackMode), parts[1], true);
                    }
                    catch
                    {
                        state.Mode = HackMode.Normal;
                    }
                    int sleeperTurn;
                    if (parts.Length > 2 && int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out sleeperTurn))
                    {
                        state.LastSleeperTurn = sleeperTurn;
                    }
                    States[guid] = state;
                }
            }
            catch (Exception e)
            {
                Log.Error("Не удалось прочитать файл режимов: " + e.Message);
            }
        }

        internal static void Save()
        {
            if (string.IsNullOrEmpty(statePath))
            {
                return;
            }
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("# ES2 Hacking Modes — режимы операций взлома");
                sb.AppendLine("# формат: guid;режим;ход последнего спящего");
                foreach (KeyValuePair<ulong, OperationState> pair in States)
                {
                    if (pair.Value.Mode == HackMode.Normal)
                    {
                        continue;
                    }
                    sb.Append(pair.Key.ToString(CultureInfo.InvariantCulture));
                    sb.Append(';');
                    sb.Append(pair.Value.Mode.ToString());
                    sb.Append(';');
                    sb.Append(pair.Value.LastSleeperTurn.ToString(CultureInfo.InvariantCulture));
                    sb.AppendLine();
                }
                File.WriteAllText(statePath, sb.ToString());
            }
            catch (Exception e)
            {
                Log.Error("Не удалось сохранить файл режимов: " + e.Message);
            }
        }
    }
}
