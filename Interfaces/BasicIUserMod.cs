extern alias UUI;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Packaging;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using Kwytto.Localization;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using static Kwytto.LiteUI.KwyttoDialog;

namespace Kwytto.Interfaces
{
    public abstract class BasicIUserMod<U, C> : BasicIUserMod
        where U : BasicIUserMod<U, C>, new()
        where C : BaseController<U, C>
    {
        private static C controller;

        protected override void OnLevelLoadedInherit(LoadMode mode)
        {
            if (IsValidLoadMode(mode))
            {
                if (!typeof(C).IsGenericType)
                {
                    m_topObj = GameObject.Find(Name) ?? new GameObject(Name);
                    Controller = m_topObj.AddComponent<C>();
                }
                SimulationManager.instance.StartCoroutine(LevelUnloadBinds());
                ShowVersionInfoPopup();
                SearchIncompatibilitiesModal();
            }
            else
            {
                LogUtils.DoWarnLog($"Invalid load mode: {mode}. The mod will not be loaded!");
                Redirector.UnpatchAll();
            }
        }
        public override BaseController GetController() => Controller;
        public static C Controller
        {
            get
            {
                if (controller is null && LoadingManager.instance.m_currentlyLoading)
                {
                    LogUtils.DoLog($"Trying to access controller while loading. NOT ALLOWED!\nAsk at Klyte45's GitHub to fix this. Stacktrace:\n{Environment.StackTrace}");
                }
                return controller;
            }
            private set => controller = value;
        }
    }
    public abstract class BasicIUserMod : IUserMod, ILoadingExtension, IViewStartActions
    {
        #region Saved shared config
        public static SavedBool DebugMode { get; }
        public static SavedString CurrentSaveVersion { get; }
        private static SavedFloat UIScaleSaved { get; }
        #endregion

        #region Old CommonProperties Static
        public static BasicIUserMod Instance
        {
            get
            {
                return m_instance;
            }
        }
        protected static readonly BasicIUserMod m_instance;

        private static ulong m_modId;
        public static ulong ModId
        {
            get
            {
                if (m_modId == 0)
                {
                    m_modId = Singleton<PluginManager>.instance.GetPluginsInfo().Where((PluginManager.PluginInfo pi) =>
                 pi.assemblyCount > 0
                 && pi.isEnabled
                 && pi.GetAssemblies().Where(x => x == Instance.GetType().Assembly).Count() > 0
             ).Select(x => x?.publishedFileID.AsUInt64 ?? ulong.MaxValue).Min();
                }
                return m_modId;
            }
        }

        private static string m_rootFolder;

        public static string ModSettingsRootFolder
        {
            get
            {
                if (m_rootFolder == null)
                {
                    m_rootFolder = KFileUtils.BASE_FOLDER_PATH + Instance.SafeName;
                }
                return m_rootFolder;
            }
        }
        private static string m_modWsRootFolder;

        public static string ModWsRootFolder
        {
            get
            {
                if (m_modWsRootFolder == null)
                {
                    m_modWsRootFolder = Singleton<PluginManager>.instance.GetPluginsInfo().Where((PluginManager.PluginInfo pi) =>
                         pi.assemblyCount > 0
                         && pi.isEnabled
                         && pi.GetAssemblies().Where(x => x == Instance.GetType().Assembly).Count() > 0
                     ).FirstOrDefault()?.modPath;
                }
                return m_modWsRootFolder;
            }
        }
        public static bool IsCityLoaded => Singleton<SimulationManager>.instance.m_metaData != null;
        public static string MinorVersion => Instance.MinorVersion_;
        public static string MajorVersion => Instance.MajorVersion_;
        public static string FullVersion => Instance.FullVersion_;
        public static string Version => Instance.Version_;
        #endregion

        #region Old CommonProperties Overridable
        public abstract string SimpleName { get; }
        public virtual string IconName { get; } = "ModIcon";
        public abstract string SafeName { get; }
        public virtual string GitHubRepoPath { get; } = "";
        public abstract string Acronym { get; }
        public abstract Color ModColor { get; }
        public float UIScale => Mathf.Clamp(UIScaleSaved.value, 0.5f, 4);
        public virtual string[] AssetExtraDirectoryNames { get; } = new string[0];
        public virtual string[] AssetExtraFileNames { get; } = new string[] { };
        public virtual string ModRootFolder => KFileUtils.BASE_FOLDER_PATH + SimpleName;
        public abstract string Description { get; }
        public virtual IUUIButtonContainerPlaceholder[] UUIButtons { get; } = new IUUIButtonContainerPlaceholder[0];

        #endregion

        #region Old CommonProperties Fixed
        public string Name => $"{SimpleName} {Version}";
        public string GeneralName => $"{SimpleName} (v{Version})";
        public abstract BaseController GetController();

        public void RequireRunCoroutine(string name, IEnumerator routine)
        {
            if (TasksRunningOnController.TryGetValue(name, out var old))
            {
                GetController().StopCoroutine(old);
            }
            if (GetController() is BaseController bc)
            {
                TasksRunningOnController[name] = bc.StartCoroutine(routine);
            }
        }

        private readonly Dictionary<string, Coroutine> TasksRunningOnController = new Dictionary<string, Coroutine>();
        private string MinorVersion_ => MajorVersion + "." + GetType().Assembly.GetName().Version.Build;
        private string MajorVersion_ => GetType().Assembly.GetName().Version.Major + "." + GetType().Assembly.GetName().Version.Minor;
        private string FullVersion_ => MinorVersion + " r" + GetType().Assembly.GetName().Version.Revision;
        private string Version_ =>
           GetType().Assembly.GetName().Version.Minor == 0 && GetType().Assembly.GetName().Version.Build == 0
                    ? GetType().Assembly.GetName().Version.Major.ToString()
                    : GetType().Assembly.GetName().Version.Build > 0
                        ? MinorVersion
                        : MajorVersion;

        #endregion

        #region Events

        public virtual void OnCreated(ILoading loading)
        {
            if (loading == null || (!loading.loadingComplete && !IsValidLoadMode(loading)))
            {
                Redirector.UnpatchAll();
            }
        }

        public void OnLevelLoaded(LoadMode mode)
        {
            OnLevelLoadedInherit(mode);
            OnLevelLoadingInternal();

            InitializeMod();
        }

        protected abstract void OnLevelLoadedInherit(LoadMode mode);

        protected IEnumerator LevelUnloadBinds()
        {
            yield return 0;
            UIButton toMainMenuButton = GameObject.Find("ToMainMenu")?.GetComponent<UIButton>();
            if (toMainMenuButton != null)
            {
                toMainMenuButton.eventClick += (x, y) =>
                {
                    GameObject.FindObjectOfType<ToolsModifierControl>().CloseEverything();
                    ExtraUnloadBinds();
                };
            }
        }

        protected virtual void ExtraUnloadBinds() { }

        protected virtual void OnLevelLoadingInternal()
        {

        }

        protected virtual bool IsValidLoadMode(ILoading loading) => loading?.currentMode == AppMode.Game;
        protected virtual bool IsValidLoadMode(LoadMode mode) => mode == LoadMode.LoadGame || mode == LoadMode.LoadScenario || mode == LoadMode.NewGame || mode == LoadMode.NewGameFromScenario;


        public void OnLevelUnloading()
        {
            Redirector.UnpatchAll();
            PatchesApply();
            DestroyMod();
            LogUtils.FlushBuffer();
        }
        public virtual void OnReleased() => PluginManager.instance.eventPluginsStateChanged -= SearchIncompatibilitiesModal;
        protected virtual void OnPatchesApply() { }

        public void OnEnabled()
        {
            if (CurrentSaveVersion.value != FullVersion)
            {
                needShowVersionPopup = true;
            }
            KFileUtils.EnsureFolderCreation(ModRootFolder);
            PatchesApply();
            DoOnEnable();
            LogUtils.FlushBuffer();
        }

        protected virtual void DoOnEnable() { }

        public void OnDisabled() => Redirector.UnpatchAll();

        protected void PatchesApply()
        {
            UnsubAuto();
            Redirector.PatchAll();
            OnPatchesApply();
        }
        public void OnViewStart() => ExtraOnViewStartActions();

        protected virtual void ExtraOnViewStartActions() { }

        private void InitializeMod()
        {
            LocaleManager.eventLocaleChanged += LocaleChanged;
            UUIButtons.ForEach(x => x.Create());
        }

        private void DestroyMod()
        {
            LocaleManager.eventLocaleChanged -= LocaleChanged;
            UUIButtons.ForEach(x => x.Destroy());
        }
        #endregion

        #region Settings UI
        public virtual bool UseGroup9 => true;
        protected GameObject m_topObj;
        public virtual void TopSettingsUI(UIHelper ext) { }
        public Transform RefTransform => m_topObj?.transform;


        public void OnSettingsUI(UIHelperBase helperDefault)
        {
            LocaleChanged();

            DoWithSettingsUI((UIHelper)helperDefault);
        }

        private void DoWithSettingsUI(UIHelper helper)
        {
            foreach (Transform child in (helper.self as UIComponent).transform)
            {
                GameObject.Destroy(child?.gameObject);
            }

            TopSettingsUI(helper);

            if (UseGroup9)
            {
                CreateGroup9(helper);
            }

            LogUtils.DoLog("End Loading Options");
        }



        protected virtual void CreateGroup9(UIHelper helper)
        {
            var group9 = helper.AddGroup(KStr.comm_betaExtraInfo) as UIHelper;
            Group9SettingsUI(group9);
            UILabel label = null;
            var obj = group9.AddSlider($"{KStr.comm_uiScale} {UIScaleSaved.value:0%}", .3f, 2, .05f, UIScaleSaved.value, (x) =>
            {
                UIScaleSaved.value = x;
                label.text = $"{KStr.comm_uiScale} {UIScaleSaved.value:0%}";
            }) as UISlider;
            label = obj.parent.GetComponentInChildren<UILabel>();
            group9.AddCheckbox(KStr.comm_debugMode, DebugMode.value, delegate (bool val)
            { DebugMode.value = val; });
            var uselessGroup = (group9.AddGroup(string.Format(KStr.comm_currentVersionFormat, FullVersion)) as UIHelper).self as UIComponent;
            var versionLabel = (uselessGroup).parent.GetComponentInChildren<UILabel>();
            versionLabel.processMarkup = true;
            GameObject.Destroy(uselessGroup);
            group9.AddButton(KStr.comm_releaseNotes, delegate ()
            {
                ShowVersionInfoPopup(true);
            });
            group9.AddButton(KStr.comm_reportABugBtn, ReportABugPopupShow);

            //UIDropDown dd = null;
            //dd = group9.AddDropdown("K45_MOD_LANG", (new string[] { "K45_GAME_DEFAULT_LANGUAGE" }.Concat(KlyteLocaleManager.locales.Select(x => $"K45_LANG_{x}")).Select(x => Locale.Get(x))).ToArray(), KlyteLocaleManager.GetLoadedLanguage(), delegate (int idx)
            //{
            //    KlyteLocaleManager.SaveLoadedLanguage(idx);
            //    KlyteLocaleManager.ReloadLanguage();
            //    KlyteLocaleManager.RedrawUIComponents();
            //});

        }
        public virtual void Group9SettingsUI(UIHelper group9) { }
        #endregion

        #region Modals
        public bool needShowVersionPopup;
        private void ReportABugPopupShow() => ShowModal(new BindProperties()
        {
            // icon = IconName,
            title = KStr.comm_reportABugBtn,
            message = KStr.comm_reportABugContent,
            buttons = new ButtonDefinition[]
                {
                    new ButtonDefinition
                    {
                        title=  KStr.comm_reportABugOpt_GoToModLog,
                        onClick= ()=>{
                            ColossalFramework.Utils.OpenInFileBrowser(LogUtils.LogPath);
                            return false;
                        }
                    },
                    new ButtonDefinition
                    {
                        isSpace = true
                    },
                    new ButtonDefinition
                    {
                        title= KStr.comm_reportABugOpt_GoToGuide,
                        onClick= ()=>{
                            ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=463645931");
                            return false;
                        }
                    },
                    new ButtonDefinition
                    {
                        title= KStr.comm_reportABugOpt_GoToModPage,
                        onClick= ()=>{
                            ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=" + ModId);
                            return false;
                        }
                    },
                    new ButtonDefinition
                    {
                        isSpace = true
                    },
                    new ButtonDefinition
                    {
                        title= KStr.comm_reportABugOpt_Ok,
                        onClick= () => true,
                        style=ButtonStyle.White
                    },
                },
            messageAlign = TextAnchor.MiddleLeft,
        });
        public bool ShowVersionInfoPopup(bool force = false)
        {
            if ((needShowVersionPopup &&
                (SimulationManager.instance.m_metaData?.m_updateMode == SimulationManager.UpdateMode.LoadGame
                || SimulationManager.instance.m_metaData?.m_updateMode == SimulationManager.UpdateMode.NewGameFromMap
                || SimulationManager.instance.m_metaData?.m_updateMode == SimulationManager.UpdateMode.NewGameFromScenario
                || PackageManager.noWorkshop
                ))
                || force)
            {
                try
                {
                    string title = $"{SimpleName} v{Version}";
                    string notes = KResourceLoader.LoadResourceStringMod("UI.VersionNotes.txt");
                    string text = string.Format(KStr.comm_releaseNotes_WasUpdatedTitle, SimpleName);
                    ShowModal(new BindProperties()
                    {
                        //icon = IconName,
                        showClose = true,
                        scrollText = notes,
                        scrollTextAlign = TextAnchor.UpperLeft,
                        buttons = new ButtonDefinition[]
                        {
                            new ButtonDefinition
                            {
                                title = $"{KStr.comm_releaseNotes_CurrentVersion} <color=#FFFF00ff>{FullVersion}</color>"
                            },
                            new ButtonDefinition
                            {
                                isSpace=true
                            },
                            new ButtonDefinition
                            {
                                title=  KStr.comm_releaseNotes_ToWorkshop,
                                onClick= ()=>{
                                    ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=" + ModId);
                                    return false;
                                }
                            },
                            new ButtonDefinition
                            {
                                title=  KStr.comm_releaseNotes_FollowKwytto,
                                onClick= ()=>{
                                    ColossalFramework.Utils.OpenUrlThreaded("https://twitter.com/kwytto");
                                    return false;
                                }
                            },
                            new ButtonDefinition
                            {
                                title= KStr.comm_releaseNotes_Ok,
                                onClick= () => true,
                                style=ButtonStyle.White
                            },
                        },
                        messageAlign = TextAnchor.UpperLeft,
                        title = title,
                        message = text,
                    });

                    CurrentSaveVersion.value = FullVersion;
                    return true;
                }
                catch (Exception e)
                {
                    LogUtils.DoErrorLog("showVersionInfoPopup ERROR {0} {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
                }
            }
            return false;
        }
        public void SearchIncompatibilitiesModal()
        {
            try
            {
                Dictionary<ulong, Tuple<string, string>> notes = SearchIncompatibilities();
                if (notes != null && notes.Count > 0)
                {
                    string title = $"{SimpleName} - Incompatibility report";
                    string text;
                    unchecked
                    {
                        text = $"Some conflicting mods were found active. Disable or unsubscribe them to make the <color=yellow>{SimpleName}</color> work properly.\n\n" +
                           string.Join("\n\n", notes.Select(x => $"\t -{x.Value.First} (id: {(x.Key == (ulong)-1 ? "<LOCAL>" : x.Key.ToString())})\n" +
                            $"\t\t<color=yellow>WHY?</color> {x.Value.Second ?? "This DLL have a name of an incompatible mod, but it's installed locally. Ignore this warning if you know what you are doing."}").ToArray()) +
                            $"\n\nDisable or unsubscribe them at main menu and try again!";
                    }
                    ShowModal(new BindProperties()
                    {
                        buttons = new ButtonDefinition[]
                        {
                            new ButtonDefinition
                            {
                                title= "Err... Okay!",
                                onClick= () => true
                            },
                        },
                        //  icon = IconName,
                        messageAlign = TextAnchor.MiddleLeft,
                        title = title,
                        message = text,
                    });
                }
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog("SearchIncompatibilitiesModal ERROR {0} {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
            }
        }
        #endregion

        #region Compatibility Check
        private void UnsubAuto()
        {
            if (AutomaticUnsubMods.Count > 0)
            {
                var modsToUnsub = PluginUtils.VerifyModsSubscribed(AutomaticUnsubMods);
                foreach (var mod in modsToUnsub)
                {
                    LogUtils.DoWarnLog($"Unsubscribing from mod: {mod.Value} (id: {mod.Key})");
                    PlatformService.workshop.Unsubscribe(new PublishedFileId(mod.Key));
                }
            }
        }

        public Dictionary<ulong, Tuple<string, string>> SearchIncompatibilities() => IncompatibleModList.Count == 0 ? null : PluginUtils.VerifyModsEnabled(IncompatibleModList, IncompatibleDllModList);
        protected virtual Dictionary<ulong, string> IncompatibleModList { get; } = new Dictionary<ulong, string>();
        protected virtual List<string> IncompatibleDllModList { get; } = new List<string>();
        private Dictionary<ulong, string> IncompatibleModListCommons { get; } = new Dictionary<ulong, string>();
        private List<string> IncompatibleDllModListCommons { get; } = new List<string>();
        protected virtual List<ulong> AutomaticUnsubMods { get; } = new List<ulong>();
        public IEnumerable<KeyValuePair<ulong, string>> IncompatibleModListAll => IncompatibleModListCommons.Union(IncompatibleModList);
        public IEnumerable<string> IncompatibleDllModListAll => IncompatibleDllModListCommons.Union(IncompatibleDllModList);
        #endregion

        #region Locale management
        protected abstract void SetLocaleCulture(CultureInfo culture);
        public static CultureInfo Culture => new CultureInfo(SingletonLite<LocaleManager>.instance.language == "zh" ? "zh-cn" : SingletonLite<LocaleManager>.instance.language);
        public static void LocaleChanged()
        {
            var newCulture = Culture;
            LogUtils.DoLog($"{Instance.SimpleName} Locale changed {KStr.Culture?.Name}->{newCulture.Name}");
            KStr.Culture = newCulture;
            Instance.SetLocaleCulture(newCulture);
        }
        #endregion

        #region Static Initializer
        static BasicIUserMod()
        {
            try
            {
                m_instance = Singleton<PluginManager>.instance.GetPluginsInfo().First((PluginManager.PluginInfo pi) =>
                             pi.assemblyCount > 0
                             && pi.GetAssemblies().Where(x => x == typeof(BasicIUserMod).Assembly).Count() > 0
                         ).GetAssemblies().SelectMany(x =>
                         {
                             try
                             {
                                 return x.GetExportedTypes();
                             }
                             catch
                             {
                                 return new Type[0];
                             }
                         })
                         .First(t => t.IsClass && typeof(BasicIUserMod).IsAssignableFrom(t) && !t.IsAbstract)
                        .GetConstructor(new Type[0])
                        .Invoke(new object[0]) as BasicIUserMod;
                DebugMode = new SavedBool(m_instance.Acronym + "_DebugMode", Settings.gameSettingsFile, false, true);
                UIScaleSaved = new SavedFloat(m_instance.Acronym + "_uiScale", Settings.gameSettingsFile, 1, true);
                CurrentSaveVersion = new SavedString(m_instance.Acronym + "_SaveVersion", Settings.gameSettingsFile, "null", true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        #endregion
    }

}
