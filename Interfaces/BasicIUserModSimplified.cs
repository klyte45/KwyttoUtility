using ColossalFramework;
using ColossalFramework.Packaging;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using Klyte._commons.Localization;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ColossalFramework.UI.UITextureAtlas;
using static Kwytto.LiteUI.KwyttoDialog;

namespace Kwytto.Interfaces
{

    public abstract class BasicIUserMod<U, C> : IUserMod, ILoadingExtension, IViewStartActions
        where U : BasicIUserMod<U, C>, new()
        where C : BaseController<U, C>
    {
        public abstract string SimpleName { get; }
        public virtual string IconName { get; } = $"K45_{CommonProperties.Acronym}_Icon";
        public virtual bool UseGroup9 => true;
        public virtual void DoLog(string fmt, params object[] args) => LogUtils.DoLog(fmt, args);
        public virtual void DoErrorLog(string fmt, params object[] args) => LogUtils.DoErrorLog(fmt, args);
        public virtual void TopSettingsUI(UIHelper ext) { }

        private GameObject m_topObj;
        public Transform RefTransform => m_topObj?.transform;

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
                 && pi.GetAssemblies().Where(x => x == typeof(U).Assembly).Count() > 0
             ).Select(x => x?.publishedFileID.AsUInt64 ?? ulong.MaxValue).Min();
                }
                return m_modId;
            }
        }

        private static string m_rootFolder;

        public static string RootFolder
        {
            get
            {
                if (m_rootFolder == null)
                {
                    m_rootFolder = Singleton<PluginManager>.instance.GetPluginsInfo().Where((PluginManager.PluginInfo pi) =>
                 pi.assemblyCount > 0
                 && pi.isEnabled
                 && pi.GetAssemblies().Where(x => x == typeof(U).Assembly).Count() > 0
             ).FirstOrDefault()?.modPath;
                }
                return m_rootFolder;
            }
        }
        public string Name => $"{SimpleName} {Version}";
        public abstract string Description { get; }
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
        }

        protected virtual void OnLevelLoadedInherit(LoadMode mode)
        {
            if (IsValidLoadMode(mode))
            {
                if (!typeof(C).IsGenericType)
                {
                    m_topObj = GameObject.Find(typeof(U).Name) ?? new GameObject(typeof(U).Name);
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

        private IEnumerator LevelUnloadBinds()
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
        public string GeneralName => $"{SimpleName} (v{Version})";

        public void OnLevelUnloading()
        {
            Controller = null;
            Redirector.UnpatchAll();
            PatchesApply();
        }
        public virtual void OnReleased() => PluginManager.instance.eventPluginsStateChanged -= SearchIncompatibilitiesModal;

        protected void PatchesApply()
        {
            UnsubAuto();
            Redirector.PatchAll();
            OnPatchesApply();
        }

        protected virtual void OnPatchesApply() { }

        public void OnEnabled()
        {
            if (CurrentSaveVersion.value != FullVersion)
            {
                needShowPopup = true;
            }
            KFileUtils.EnsureFolderCreation(CommonProperties.ModRootFolder);
            PatchesApply();
        }

        public void OnDisabled() => Redirector.UnpatchAll();

        public static string MinorVersion => MajorVersion + "." + typeof(U).Assembly.GetName().Version.Build;
        public static string MajorVersion => typeof(U).Assembly.GetName().Version.Major + "." + typeof(U).Assembly.GetName().Version.Minor;
        public static string FullVersion => MinorVersion + " r" + typeof(U).Assembly.GetName().Version.Revision;
        public static string Version
        {
            get
            {
                if (typeof(U).Assembly.GetName().Version.Minor == 0 && typeof(U).Assembly.GetName().Version.Build == 0)
                {
                    return typeof(U).Assembly.GetName().Version.Major.ToString();
                }
                if (typeof(U).Assembly.GetName().Version.Build > 0)
                {
                    return MinorVersion;
                }
                else
                {
                    return MajorVersion;
                }
            }
        }

        public bool needShowPopup;

        public static SavedBool DebugMode { get; } = new SavedBool(CommonProperties.Acronym + "_DebugMode", Settings.gameSettingsFile, false, true);
        private SavedString CurrentSaveVersion { get; } = new SavedString(CommonProperties.Acronym + "SaveVersion", Settings.gameSettingsFile, "null", true);
        public static bool IsCityLoaded => Singleton<SimulationManager>.instance.m_metaData != null;

        public static U m_instance = new U();
        public static U Instance => m_instance;

        private UIComponent m_onSettingsUiComponent;
        private static C controller;

        public void OnSettingsUI(UIHelperBase helperDefault)
        {

            m_onSettingsUiComponent = ((UIHelper)helperDefault).self as UIComponent;

            DoWithSettingsUI((UIHelper)helperDefault);
        }

        private void DoWithSettingsUI(UIHelper helper)
        {
            foreach (Transform child in (helper.self as UIComponent).transform)
            {
                GameObject.Destroy(child?.gameObject);
            }

            var newSprites = new List<SpriteInfo>();
            TextureAtlasUtils.LoadImagesFromResources("_commons.UI.Images", ref newSprites);
            TextureAtlasUtils.LoadImagesFromResources("UI.Images", ref newSprites);
            LogUtils.DoLog($"ADDING {newSprites.Count} sprites!");
            TextureAtlasUtils.RegenerateDefaultTextureAtlas(newSprites);

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
            group9.AddButton("Report-a-bug helper", () => ShowModal(new BindProperties()
            {
                // icon = IconName,
                title = "Report-a-bug helper",
                message = "If you find any problem with this mod, please send me the output_log.txt (or player.log on Mac/Linux) in the mod Workshop page. If applies, a printscreen can help too to make a better guess about what is happening wrong here...\n\n" +
                         "There's a link for a Workshop guide by <color #008800>aubergine18</color> explaining how to find your log file, depending of OS you're using.\nFeel free to create a topic at Workshop or just leave a comment linking your files.",
                buttons = new ButtonDefinition[]
                {
                    new ButtonDefinition
                    {
                        title= "Okay...",
                        onClick= () => true
                    },
                    new ButtonDefinition
                    {
                        title=  "Go to the guide",
                        onClick= ()=>{
                            ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=463645931");
                            return false;
                        }
                    },
                    new ButtonDefinition
                    {
                        title= "Go to mod page",
                        onClick= ()=>{
                            ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=" + ModId);
                            return false;
                        }
                    },
                },
            }));

            //UIDropDown dd = null;
            //dd = group9.AddDropdown("K45_MOD_LANG", (new string[] { "K45_GAME_DEFAULT_LANGUAGE" }.Concat(KlyteLocaleManager.locales.Select(x => $"K45_LANG_{x}")).Select(x => Locale.Get(x))).ToArray(), KlyteLocaleManager.GetLoadedLanguage(), delegate (int idx)
            //{
            //    KlyteLocaleManager.SaveLoadedLanguage(idx);
            //    KlyteLocaleManager.ReloadLanguage();
            //    KlyteLocaleManager.RedrawUIComponents();
            //});


        }

        public virtual void Group9SettingsUI(UIHelper group9) { }

        protected virtual Tuple<string, string> GetButtonLink() => null;

        public bool ShowVersionInfoPopup(bool force = false)
        {
            if ((needShowPopup &&
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
                    string notes = KlyteResourceLoader.LoadResourceString("UI.VersionNotes.txt");
                    var fullWidth = notes.StartsWith("<extended>");
                    if (fullWidth)
                    {
                        notes = notes.Substring("<extended>".Length);
                    }
                    string text = $"<size=28>{SimpleName} was updated! Release notes:</size>\n\n<size=14>{notes}</size>";
                    var targetUrl = GetButtonLink();
                    ShowModal(new BindProperties()
                    {
                        //icon = IconName,
                        showClose = true,
                        buttons = new ButtonDefinition[]
                        {
                            new ButtonDefinition
                            {
                                title = $"Current Version:\n<color=#FFFF00ff>{FullVersion}</color>"
                            },
                            new ButtonDefinition
                            {
                                IsSpace=true
                            },
                            new ButtonDefinition
                            {
                                title=  "See the news on the mod page at Workshop!",
                                onClick= ()=>{
                                    ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=" + ModId);
                                    return false;
                                }
                            },
                            new ButtonDefinition
                            {
                                title= "Follow Kwytto on Twitter to get mods news!",
                                onClick= ()=>{
                                    ColossalFramework.Utils.OpenUrlThreaded("https://twitter.com/kwytto");
                                    return false;
                                }
                            },
                            new ButtonDefinition
                            {
                                title= "Okay!",
                                onClick= () => true
                            },
                        },
                        messageAlign = TextAnchor.MiddleLeft,
                        title = title,
                        message = text,
                    });

                    return true;
                }
                catch (Exception e)
                {
                    DoErrorLog("showVersionInfoPopup ERROR {0} {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
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
                DoErrorLog("SearchIncompatibilitiesModal ERROR {0} {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
            }
        }

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
        public void OnViewStart() => ExtraOnViewStartActions();
        protected virtual void ExtraOnViewStartActions() { }
        protected virtual Dictionary<ulong, string> IncompatibleModList { get; } = new Dictionary<ulong, string>();
        protected virtual List<string> IncompatibleDllModList { get; } = new List<string>();
        private Dictionary<ulong, string> IncompatibleModListCommons { get; } = new Dictionary<ulong, string>();
        private List<string> IncompatibleDllModListCommons { get; } = new List<string>();
        protected virtual List<ulong> AutomaticUnsubMods { get; } = new List<ulong>();
        public IEnumerable<KeyValuePair<ulong, string>> IncompatibleModListAll => IncompatibleModListCommons.Union(IncompatibleModList);
        public IEnumerable<string> IncompatibleDllModListAll => IncompatibleDllModListCommons.Union(IncompatibleDllModList);

    }

}
