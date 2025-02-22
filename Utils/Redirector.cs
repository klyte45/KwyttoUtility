using HarmonyLib;
using Kwytto.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace Kwytto.Utils
{
    public sealed class RedirectorUtils
    {
        public static readonly BindingFlags allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.GetProperty;
    }

    public interface IRedirectable
    {
    }

    public class Redirector : MonoBehaviour
    {
        #region Class Base
        private static readonly Harmony m_harmony = new Harmony($"com.klyte.redirectors.{BasicIUserMod.Instance.Acronym}");
        private static readonly List<MethodInfo> m_patches = new List<MethodInfo>();
        private static readonly List<Action> m_onUnpatchActions = new List<Action>();

        private readonly List<MethodInfo> m_detourList = new List<MethodInfo>();


        public Harmony GetHarmonyInstance() => m_harmony;
        #endregion

        public static readonly MethodInfo semiPreventDefaultMI = new Func<bool>(() =>
        {
            var stackTrace = new StackTrace();
            StackFrame[] stackFrames = stackTrace.GetFrames();
           if (BasicIUserMod.DebugMode) LogUtils.DoLog($"SemiPreventDefault fullStackTrace: \r\n {Environment.StackTrace}");
            for (int i = 2; i < stackFrames.Length; i++)
            {
                if (stackFrames[i].GetMethod().DeclaringType.ToString().StartsWith("Klyte."))
                {
                    return false;
                }
            }
            return true;
        }).Method;

        public static bool PreventDefault() => false;

        public void AddRedirect(MethodInfo oldMethod, MethodInfo newMethodPre, MethodInfo newMethodPost = null, MethodInfo transpiler = null)
        {

           if (BasicIUserMod.DebugMode) LogUtils.DoLog($"Adding patch! {oldMethod.DeclaringType} {oldMethod}");
            m_detourList.Add(GetHarmonyInstance().Patch(oldMethod, newMethodPre != null ? new HarmonyMethod(newMethodPre) : null, newMethodPost != null ? new HarmonyMethod(newMethodPost) : null, transpiler != null ? new HarmonyMethod(transpiler) : null));
            m_patches.Add(oldMethod);
        }
        public void AddUnpatchAction(Action unpatchAction) => m_onUnpatchActions.Add(unpatchAction);

        public static void UnpatchAll()
        {
            LogUtils.DoWarnLog($"Unpatching all: {m_harmony.Id}");
            foreach (MethodInfo method in m_patches)
            {
                m_harmony.Unpatch(method, HarmonyPatchType.All, m_harmony.Id);
            }
            foreach (Action action in m_onUnpatchActions)
            {
                action?.Invoke();
            }
            m_onUnpatchActions.Clear();
            m_patches.Clear();
        }
        public static void PatchAll()
        {
            LogUtils.DoWarnLog($"Patching all: {m_harmony.Id}");
            GameObject m_topObj = GameObject.Find("k45_Redirectors") ?? new GameObject("k45_Redirectors");
            Type typeTarg = typeof(IRedirectable);
            List<Type> instances = ReflectionUtils.GetInterfaceImplementations(typeTarg);
           if (BasicIUserMod.DebugMode) LogUtils.DoLog($"Found Redirectors: {instances.Count}");
            Application.logMessageReceived += ErrorPatchingHandler;
            try
            {
                foreach (Type t in instances)
                {
                   if (BasicIUserMod.DebugMode) LogUtils.DoLog($"Redirector: {t}");
                    m_topObj.AddComponent(t);

                }
            }
            finally
            {
                Application.logMessageReceived -= ErrorPatchingHandler;
            }

        }

        private static void ErrorPatchingHandler(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                //K45DialogControl.ShowModal(new K45DialogControl.BindProperties
                //{
                //    title = "AN ERROR HAS OCCURRED!",
                //    message = $"An error happened while trying to patch all code needed to make the <color yellow>{BasicIUserMod.Instance.ModName}</color> work properly. Check output_log.txt file to see details." +
                //    (BasicIUserMod.Instance.GitHubRepoPath.IsNullOrWhiteSpace() ? "" : "\nPlease open a issue in GitHub with the output file attached on it to help to fix this problem. Thanks!") +
                //    $"\n\n<color #FF00FF>{logString}</color>\n{stackTrace}",
                //    showButton1 = true,
                //    showButton2 = true,
                //    showButton3 = !BasicIUserMod.Instance.GitHubRepoPath.IsNullOrWhiteSpace(),
                //    textButton1 = "OK",
                //    textButton2 = "Go to output_log.txt file (WIN)",
                //    textButton3 = "GitHub: open an issue to fix this",
                //    useFullWindowWidth = true
                //}, (x) =>
                //{
                //    if (x == 2)
                //    {
                //        ColossalFramework.Utils.OpenInFileBrowser("Cities_Data/output_log.txt");
                //        return false;
                //    }
                //    if (x == 3)
                //    {
                //        FileSystemUtils.OpenURLInOverlayOrBrowser($"https://github.com/{BasicIUserMod.Instance.GitHubRepoPath}/issues/new");
                //        return false;
                //    }
                //    return true;
                //});
                LogUtils.DoErrorLog($"{logString}\n{stackTrace}");
            }
        }

        public void EnableDebug() => Harmony.DEBUG = true;
        public void DisableDebug() => Harmony.DEBUG = false;
    }
}

