using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace Kwytto.Utils
{
    public class LogUtils
    {
        #region Log Utils

        private static readonly object fileLock;
        public static string logPath;
        public static char indentChar;
        public static int indentLevel;
        private static List<string> buffer;

        static LogUtils()
        {
            fileLock = new object();
            indentChar = '\t';
            indentLevel = 0;
            buffer = new List<string>();

            string folderPath = Path.Combine(KFileUtils.BASE_FOLDER_PATH, "_LOGS");
            KFileUtils.EnsureFolderCreation(folderPath);
            logPath = Path.Combine(folderPath, CommonProperties.Acronym + ".log.txt");
        }
        private static void LogBuffered(string format, params object[] args)
        {
            Debug.LogFormat(format, args);
            lock (fileLock)
            {
                buffer.Add(string.Format(format, args));
            }
        }
        private static void FlushBuffer()
        {
            lock (fileLock)
            {
                if (buffer.Count <= 0)
                {
                    return;
                }

                using (StreamWriter streamWriter = File.AppendText(logPath))
                {
                    foreach (string item in buffer)
                    {
                        streamWriter.WriteLine(item);
                    }
                }

                buffer.Clear();
            }
        }
        private static string IndentString()
        {
            return new string(indentChar, indentLevel);
        }
        public static void Log(string format, params object[] args)
        {
            Debug.LogFormat(format, args);
            lock (fileLock)
            {
                using (StreamWriter streamWriter = File.AppendText(logPath))
                    streamWriter.WriteLine(IndentString() + string.Format(format, args));
            }
        }

        public static void DoLog(string format, params object[] args)
        {
            try
            {
                if (CommonProperties.DebugMode)
                {
                    LogBuffered(string.Format($"{CommonProperties.Acronym}v" + CommonProperties.Version + " " + format, args));
                    FlushBuffer();
                }

            }
            catch
            {
               Log($"{CommonProperties.Acronym}: Erro ao fazer log: {{0}} (args = {{1}})", format, args == null ? "[]" : string.Join(",", args.Select(x => x != null ? x.ToString() : "--NULL--").ToArray()));
            }
        }
        public static void DoWarnLog(string format, params object[] args)
        {
            try
            {
                LogBuffered($"{CommonProperties.Acronym}v" + CommonProperties.Version + " " + format, args);
                FlushBuffer();
            }
            catch
            {
               Log($"{CommonProperties.Acronym}: Erro ao fazer warn log: {{0}} (args = {{1}})", format, args == null ? "[]" : string.Join(",", args.Select(x => x != null ? x.ToString() : "--NULL--").ToArray()));
            }
        }
        public static void DoErrorLog(string format, params object[] args)
        {
            try
            {
                Log($"{CommonProperties.Acronym}v" + CommonProperties.Version + " " + format, args);
                FlushBuffer();
            }
            catch
            {
               Log($"{CommonProperties.Acronym}: Erro ao fazer err log: {{0}} (args = {{1}})", format, args == null ? "[]" : string.Join(",", args.Select(x => x != null ? x.ToString() : "--NULL--").ToArray()));
            }
        }

        public static void PrintMethodIL(IEnumerable<CodeInstruction> inst, bool force = false)
        {
            if (force || CommonProperties.DebugMode)
            {
                int j = 0;
                LogBuffered($"TRANSPILLED:\n\t{string.Join("\n\t", inst.Select(x => $"{(j++).ToString("D8")} {x.opcode.ToString().PadRight(10)} {ParseOperand(inst, x.operand)}").ToArray())}");
            }
        }

        public static string GetLinesPointingToLabel(IEnumerable<CodeInstruction> inst, Label lbl)
        {
            int j = 0;
            return "\t" + string.Join("\n\t", inst.Select(x => Tuple.New(x, $"{(j++).ToString("D8")} {x.opcode.ToString().PadRight(10)} {ParseOperand(inst, x.operand)}")).Where(x => x.First.operand is Label label && label == lbl).Select(x => x.Second).ToArray());
        }


        internal static string ParseOperand(IEnumerable<CodeInstruction> instr, object operand)
        {
            if (operand is null)
            {
                return null;
            }

            if (operand is Label lbl)
            {
                return "LBL: " + instr.Select((x, y) => Tuple.New(x, y)).Where(x => x.First.labels.Contains(lbl)).Select(x => $"{x.Second.ToString("D8")} {x.First.opcode.ToString().PadRight(10)} {ParseOperand(instr, x.First.operand)}").FirstOrDefault();
            }
            else
            {
                return operand.ToString() + $" (Type={operand.GetType()})";
            }
        }
        #endregion
    }
}
