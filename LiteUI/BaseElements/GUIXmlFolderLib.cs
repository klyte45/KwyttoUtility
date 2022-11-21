using ColossalFramework;
using ColossalFramework.Globalization;
using Kwytto.Interfaces;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Kwytto.LiteUI
{
    public abstract class GUIXmlFolderLib<T> where T : class, ILibable
    {
        private string libraryFilter = "";
        private Vector2 libraryScroll;
        private readonly Wrapper<string[]> librarySearchResults = new Wrapper<string[]>();
        public abstract string LibFolderPath { get; }

        public GUIXmlFolderLib()
        {
            KFileUtils.EnsureFolderCreation(LibFolderPath);
        }

        private readonly Texture2D ImportAdd = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Plus);
        private readonly Texture2D ImportTex = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Import);
        private readonly Texture2D ExportTex = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Export);
        private readonly Texture2D FolderTex = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Load);

        private GUIStyle m_btnNoBorder;

        public string DeleteQuestionI18n { get; set; } = "";
        public string ImportI18n { get; set; } = "";
        public string ExportI18n { get; set; } = "";
        public string ImportAdditiveI18n { get; set; } = "";
        public string DeleteButtonI18n { get; set; } = "";
        public string NameAskingI18n { get; set; } = "";
        public string NameAskingOverwriteI18n { get; set; } = "";

        public FooterBarStatus Status { get; private set; }
        private void RestartLibraryFilterCoroutine()
        {
            BasicIUserMod.Instance.RequireRunCoroutine($"OnFilterLib {GetType()}", OnFilterLib());
        }
        private IEnumerator OnFilterLib()
        {
            yield return librarySearchResults.Value = Directory.GetFiles(LibFolderPath, "*.xml")
              .Select(x => Path.GetFileNameWithoutExtension(x))
              .Where((x) => libraryFilter.IsNullOrWhiteSpace() || LocaleManager.cultureInfo.CompareInfo.IndexOf(x, libraryFilter, CompareOptions.IgnoreCase) >= 0)
              .OrderBy((x) => x)
              .ToArray();
        }

        public void DrawImportView(Action<T, bool> OnSelect)
        {
            InitStyles();
            using (new GUILayout.HorizontalScope())
            {
                var newFilterVal = GUILayout.TextField(libraryFilter, GUILayout.Height(30));
                if (newFilterVal != libraryFilter)
                {
                    libraryFilter = newFilterVal;
                    RestartLibraryFilterCoroutine();
                }
                GUIKwyttoCommons.SquareTextureButton2(FolderTex, ".", () => ColossalFramework.Utils.OpenInFileBrowser(LibFolderPath));
            };

            using (var scroll = new GUILayout.ScrollViewScope(libraryScroll))
            {
                var selectLayout = GUILayout.SelectionGrid(-1, librarySearchResults.Value, 1);
                if (selectLayout >= 0)
                {
                    var filePath = Path.Combine(LibFolderPath, librarySearchResults.Value[selectLayout] + ".xml");
                    if (File.Exists(filePath))
                    {
                        OnSelect(XmlUtils.DefaultXmlDeserialize<T>(File.ReadAllText(filePath)), Status == FooterBarStatus.AskingToImportAdditive);
                        Status = FooterBarStatus.Normal;
                    }

                }
                libraryScroll = scroll.scrollPosition;
            };

            Draw(null, null, null);
        }

        public void Draw(GUIStyle removeButtonStyle, Action doOnDelete, Func<T> getCurrent, Action<GUIStyle> onNormalDraw = null)
        {
            InitStyles();
            using (new GUILayout.HorizontalScope())
            {
                switch (Status)
                {
                    case FooterBarStatus.AskingToRemove:
                        GUILayout.Label(string.Format(DeleteQuestionI18n, getCurrent().SaveName));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(Locale.Get("YES"), removeButtonStyle))
                        {
                            Status = FooterBarStatus.Normal;
                            doOnDelete();
                        }
                        if (GUILayout.Button(Locale.Get("NO")))
                        {
                            Status = FooterBarStatus.Normal;
                        }
                        break;
                    case FooterBarStatus.AskingToImport:
                    case FooterBarStatus.AskingToImportAdditive:
                        if (GUILayout.Button(Locale.Get("CANCEL")))
                        {
                            Status = FooterBarStatus.Normal;
                        }
                        break;
                    case FooterBarStatus.AskingToExport:
                        GUILayout.Label(NameAskingI18n);
                        footerInputVal = GUILayout.TextField(footerInputVal, GUILayout.Width(150));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(Locale.Get("SAVE")))
                        {
                            var filePath = Path.Combine(LibFolderPath, footerInputVal + ".xml");
                            if (!File.Exists(filePath))
                            {
                                File.WriteAllText(filePath, XmlUtils.DefaultXmlSerialize(getCurrent()));
                                Status = FooterBarStatus.Normal;
                            }
                            else
                            {
                                Status = FooterBarStatus.AskingToExportOverwrite;
                            }
                        }
                        if (GUILayout.Button(Locale.Get("CANCEL")))
                        {
                            Status = FooterBarStatus.Normal;
                        }
                        break;
                    case FooterBarStatus.AskingToExportOverwrite:
                        GUILayout.Label(string.Format(NameAskingOverwriteI18n));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(Locale.Get("YES"), removeButtonStyle))
                        {
                            var filePath = Path.Combine(LibFolderPath, footerInputVal + ".xml");
                            File.WriteAllText(filePath, XmlUtils.DefaultXmlSerialize(getCurrent()));
                            Status = FooterBarStatus.Normal;
                        }
                        if (GUILayout.Button(Locale.Get("NO")))
                        {
                            Status = FooterBarStatus.AskingToExport;
                        }
                        break;

                    case FooterBarStatus.Normal:
                        onNormalDraw?.Invoke(removeButtonStyle);
                        break;
                }
            }
        }

        private void InitStyles()
        {
            if (m_btnNoBorder is null)
            {
                m_btnNoBorder = new GUIStyle(GUI.skin.button)
                {
                    padding = new RectOffset(),
                };
            }
        }

        private FooterBarStatus m_currentHover;
        public void FooterDraw(GUIStyle removeButtonStyle)
        {
            InitStyles();
            var hoverSomething = false;
            if (!ImportAdditiveI18n.IsNullOrWhiteSpace())
            {
                GUI.SetNextControlName(GetHashCode() + "_IMPORT_ADD");
                if (GUILayout.Button(ImportAdd, m_btnNoBorder, GUILayout.MaxHeight(20)))
                {
                    GoToImportAdditive();
                }
                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    m_currentHover = FooterBarStatus.AskingToImportAdditive;
                    hoverSomething = true;
                }
            }
            GUI.SetNextControlName(GetHashCode() + "_IMPORT");
            if (GUILayout.Button(ImportTex, m_btnNoBorder, GUILayout.MaxHeight(20)))
            {
                GoToImport();
            }
            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                m_currentHover = FooterBarStatus.AskingToImport;
                hoverSomething = true;
            }
            GUI.SetNextControlName(GetHashCode() + "_EXPORT");
            if (GUILayout.Button(ExportTex, m_btnNoBorder, GUILayout.MaxHeight(20)))
            {
                GoToExport();
            }
            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                m_currentHover = FooterBarStatus.AskingToExport;
                hoverSomething = true;
            }
            if (!hoverSomething)
            {
                m_currentHover = default;
            }
            DrawLabel(() => DrawRemoveButton(removeButtonStyle));
        }

        public void DrawLabel(Action drawDefault)
        {
            switch (m_currentHover)
            {
                case FooterBarStatus.AskingToImport:
                    GUILayout.Label(ImportI18n, GUILayout.Width(300), GUILayout.ExpandHeight(true));
                    break;
                case FooterBarStatus.AskingToImportAdditive:
                    GUILayout.Label(ImportAdditiveI18n, GUILayout.Width(300), GUILayout.ExpandHeight(true));
                    break;
                case FooterBarStatus.AskingToExport:
                    GUILayout.Label(ExportI18n, GUILayout.Width(300), GUILayout.ExpandHeight(true));
                    break;
                default:
                    GUILayout.Label("", GUILayout.Width(300), GUILayout.ExpandHeight(true));
                    break;
            }
            drawDefault?.Invoke();
            GUILayout.FlexibleSpace();
        }

        private void DrawRemoveButton(GUIStyle removeButtonStyle)
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(DeleteButtonI18n, removeButtonStyle))
            {
                GoToRemove();
            }
        }

        public void GoToRemove() => Status = FooterBarStatus.AskingToRemove;

        public void GoToExport()
        {
            Status = FooterBarStatus.AskingToExport;
            footerInputVal = "";
        }

        public void GoToImport()
        {
            Status = FooterBarStatus.AskingToImport;
            libraryFilter = "";
            librarySearchResults.Value = new string[0];
            RestartLibraryFilterCoroutine();
        }
        public void GoToImportAdditive()
        {
            Status = FooterBarStatus.AskingToImportAdditive;
            libraryFilter = "";
            librarySearchResults.Value = new string[0];
            RestartLibraryFilterCoroutine();
        }

        public void ResetStatus() => Status = FooterBarStatus.Normal;

        private string footerInputVal = "";
    }
}
