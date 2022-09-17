using ColossalFramework.UI;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kwytto.LiteUI
{
    public abstract class GUIWindow : MonoBehaviour
    {
        private static readonly List<GUIWindow> Windows = new List<GUIWindow>();

        private static GUIWindow resizingWindow;
        private static Vector2 resizeDragHandle = Vector2.zero;

        private static GUIWindow movingWindow;
        private static Vector2 moveDragHandle = Vector2.zero;

        private static Texture2D highlightTexture;

        private static GUIStyle highlightstyle;
        private bool Resizable;
        private bool HasTitlebar;

        private Vector2 minSize = Vector2.zero;
        private Rect windowRect = new Rect(0, 0, 64, 64);

        private bool visible;
        protected bool showOverModals;
        protected bool requireModal;
        private Texture2D cachedModIcon;

        private bool oldModalState;
        private int oldModalZorder;

        protected virtual float FontSizeMultiplier { get; } = 1;
        private float EffectiveFontSizeMultiplier => FontSizeMultiplier * ResolutionMultiplier;
        protected virtual float ResolutionMultiplier => Screen.height / 1080f;

        protected virtual bool ShowCloseButton { get; } = true;

        public static GUIStyle HighlightStyle => GUIStyle.none;
        public int DefaultSize => Mathf.RoundToInt(12 * EffectiveFontSizeMultiplier);

        internal void Init(string title, Rect rect, bool resizable = true, bool hasTitlebar = true, Vector2 minSize = default)
        {
            Id = UnityEngine.Random.Range(1024, int.MaxValue);
            Title = title;
            windowRect = rect;
            Resizable = resizable;
            HasTitlebar = hasTitlebar;
            this.minSize = minSize == default ? new Vector2(64.0f, 64.0f) : minSize;

            Panel = gameObject.AddComponent<UIPanel>();
            Panel.zOrder = int.MaxValue;
            if (requireModal)
            {
                UIView.PushModal(Panel);
            }
            Windows.Add(this);
        }


        public Rect WindowRect => windowRect;
        protected GUISkin Skin { get; private set; }

        public bool Visible
        {
            get => visible;

            set
            {
                var wasVisible = visible;
                visible = value;
                if (visible != wasVisible)
                {
                    if (visible)
                    {
                        GUI.BringWindowToFront(Id);
                        OnWindowOpened();
                    }
                    else
                    {
                        OnWindowClosed();
                    }
                }
            }
        }

        protected static Texture2D BgTexture { get; set; }

        protected static Texture2D ResizeNormalTexture { get; set; }

        protected static Texture2D ResizeHoverTexture { get; set; }

        protected static Texture2D CloseNormalTexture { get; set; }

        protected static Texture2D CloseHoverTexture { get; set; }

        protected static Texture2D MoveNormalTexture { get; set; }

        protected static Texture2D MoveHoverTexture { get; set; }

        protected string Title { get; set; }

        public UIPanel Panel { get; private set; }

        public int Id { get; private set; }

        public void UpdateFont()
        {
            if (Skin.font is null)
            {
                Skin.font = Font.CreateDynamicFontFromOSFont(new string[0], DefaultSize);
            }
        }

        public void OnDestroy()
        {
            OnWindowDestroyed();
            if (requireModal)
            {
                UIView.GetAView().panelsLibraryModalEffect.isVisible = oldModalState;
                UIView.GetAView().panelsLibraryModalEffect.zOrder = oldModalZorder;
                UIView.PopModal();
            }
            Windows.Remove(this);
        }

        private Color bgColor;
        private Color titleBar;
        private Color titleBarHover;
        public void OnGUI()
        {
            if (Skin == null)
            {
                bgColor = BasicIUserMod.Instance.ModColor.SetBrightness(.30f);
                titleBar = BasicIUserMod.Instance.ModColor.SetBrightness(.60f);
                titleBarHover = BasicIUserMod.Instance.ModColor.SetBrightness(1);

                BgTexture = new Texture2D(1, 1);
                BgTexture.SetPixel(0, 0, new Color(bgColor.r, bgColor.g, bgColor.b, 1));
                BgTexture.Apply();

                ResizeNormalTexture = new Texture2D(1, 1);
                ResizeNormalTexture.SetPixel(0, 0, Color.white);
                ResizeNormalTexture.Apply();

                ResizeHoverTexture = new Texture2D(1, 1);
                ResizeHoverTexture.SetPixel(0, 0, Color.blue);
                ResizeHoverTexture.Apply();

                CloseNormalTexture = new Texture2D(1, 1);
                CloseNormalTexture.SetPixel(0, 0, ColorExtensions.FromRGB("AA0000"));
                CloseNormalTexture.Apply();

                CloseHoverTexture = new Texture2D(1, 1);
                CloseHoverTexture.SetPixel(0, 0, ColorExtensions.FromRGB("FF6666"));
                CloseHoverTexture.Apply();

                MoveNormalTexture = new Texture2D(1, 1);
                MoveNormalTexture.SetPixel(0, 0, titleBar);
                MoveNormalTexture.Apply();

                MoveHoverTexture = new Texture2D(1, 1);
                MoveHoverTexture.SetPixel(0, 0, titleBarHover);
                MoveHoverTexture.Apply();

                Skin = ScriptableObject.CreateInstance<GUISkin>();
                Skin.box = new GUIStyle(GUI.skin.box);
                Skin.button = new GUIStyle(GUI.skin.button);
                Skin.horizontalScrollbar = new GUIStyle(GUI.skin.horizontalScrollbar);
                Skin.horizontalScrollbarLeftButton = new GUIStyle(GUI.skin.horizontalScrollbarLeftButton);
                Skin.horizontalScrollbarRightButton = new GUIStyle(GUI.skin.horizontalScrollbarRightButton);
                Skin.horizontalScrollbarThumb = new GUIStyle(GUI.skin.horizontalScrollbarThumb);
                Skin.horizontalSlider = new GUIStyle(GUI.skin.horizontalSlider);
                Skin.horizontalSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
                Skin.label = new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                };
                Skin.scrollView = new GUIStyle(GUI.skin.scrollView);
                Skin.textArea = new GUIStyle(GUI.skin.textArea);
                Skin.textField = new GUIStyle(GUI.skin.textField);
                Skin.toggle = new GUIStyle(GUI.skin.toggle);
                Skin.verticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);
                Skin.verticalScrollbarDownButton = new GUIStyle(GUI.skin.verticalScrollbarDownButton);
                Skin.verticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);
                Skin.verticalScrollbarUpButton = new GUIStyle(GUI.skin.verticalScrollbarUpButton);
                Skin.verticalSlider = new GUIStyle(GUI.skin.verticalSlider);
                Skin.verticalSliderThumb = new GUIStyle(GUI.skin.verticalSliderThumb);
                Skin.window = new GUIStyle(GUI.skin.window);
                Skin.window.normal.background = BgTexture;
                Skin.window.onNormal.background = BgTexture;


                Skin.settings.cursorColor = GUI.skin.settings.cursorColor;
                Skin.settings.cursorFlashSpeed = GUI.skin.settings.cursorFlashSpeed;
                Skin.settings.doubleClickSelectsWord = GUI.skin.settings.doubleClickSelectsWord;
                Skin.settings.selectionColor = GUI.skin.settings.selectionColor;
                Skin.settings.tripleClickSelectsLine = GUI.skin.settings.tripleClickSelectsLine;
                Skin.label.richText = true;

                highlightstyle = new GUIStyle(GUI.skin.button)
                {
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0)
                };
                highlightstyle.normal = highlightstyle.onNormal = new GUIStyleState();
                LoadHighlightTexture();
                highlightstyle.onHover = highlightstyle.hover = new GUIStyleState
                {
                    background = highlightTexture,
                };
            }

            if (!Visible)
            {
                Panel.isVisible = false;
                return;
            }
            if (requireModal)
            {
                if (UIView.GetAView().panelsLibraryModalEffect.zOrder != int.MaxValue)
                {
                    oldModalState = UIView.GetAView().panelsLibraryModalEffect.isVisible;
                    oldModalZorder = UIView.GetAView().panelsLibraryModalEffect.zOrder;
                    UIView.GetAView().panelsLibraryModalEffect.zOrder = int.MaxValue;
                    UIView.GetAView().panelsLibraryModalEffect.isVisible = true;
                }
            }
            else if (!showOverModals && UIView.GetAView().panelsLibraryModalEffect.isVisible)
            {
                return;
            }

            Panel.isVisible = true;

            var oldSkin = GUI.skin;
            if (Skin != null)
            {
                UpdateFont();
                GUI.skin = Skin;
            }

            var oldMatrix = GUI.matrix;
            try
            {
                GUI.matrix = UIScaler.ScaleMatrix;

                windowRect = GUI.Window(Id, windowRect, WindowFunction, string.Empty);
                Panel.absolutePosition = windowRect.position;
                Panel.absolutePosition = windowRect.position;
                Panel.size = windowRect.size;
                OnWindowDrawn();
            }
            finally
            {
                GUI.matrix = oldMatrix;

                GUI.skin = oldSkin;
            }
        }

        internal static Texture2D LoadHighlightTexture() => highlightTexture = KResourceLoader.LoadCommonsTexture(UI.CommonsSpriteNames.highlight);

        public void MoveResize(Rect newWindowRect) => windowRect = newWindowRect;

        private bool isOnTop;

        private void Update()
        {
            if (requireModal)
            {
                Visible = UIView.GetModalComponent() == Panel;
            }
            else
            {
                var mouseOverWindow = Visible && windowRect.Contains(UIScaler.MousePosition);
                if (mouseOverWindow)
                {
                    if (!isOnTop)
                    {
                        isOnTop = true;
                        UIView.PushModal(Panel);
                    }
                }
                else
                {
                    if (isOnTop && UIView.GetModalComponent() == Panel)
                    {
                        isOnTop = false;
                        UIView.PopModal();
                    }
                }
            }
        }
        protected static bool IsMouseOverWindow()
        {
            var mouse = UIScaler.MousePosition;
            return Windows.FindIndex(window => window.Visible && window.windowRect.Contains(mouse)) >= 0;
        }

        protected abstract void DrawWindow();

        protected virtual void HandleException(Exception ex)
        {
        }

        protected virtual void OnWindowDrawn()
        {
        }

        protected virtual void OnWindowOpened()
        {
        }

        protected virtual void OnWindowClosed()
        {
        }

        protected virtual void OnWindowResized(Vector2 size)
        {
        }

        protected virtual void OnWindowMoved(Vector2 position)
        {
        }

        protected virtual void OnWindowDestroyed()
        {
        }

        private void WindowFunction(int windowId)
        {
            FitScreen();
            GUILayout.Space(8.0f);

            try
            {
                DrawWindow();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            GUILayout.Space(16.0f);

            var mouse = UIScaler.MousePosition;

            DrawBorder();

            if (HasTitlebar)
            {
                DrawTitlebar(mouse);
                if (ShowCloseButton)
                {
                    DrawCloseButton(mouse);
                }
            }

            if (Resizable)
            {
                DrawResizeHandle(mouse);
            }
        }

        private void DrawBorder()
        {
            var leftRect = new Rect(0.0f, 0.0f, 1.0f, windowRect.height);
            var rightRect = new Rect(windowRect.width - 1.0f, 0.0f, 1.0f, windowRect.height);
            var bottomRect = new Rect(0.0f, windowRect.height - 1.0f, windowRect.width, 1.0f);
            GUI.DrawTexture(leftRect, MoveNormalTexture);
            GUI.DrawTexture(rightRect, MoveNormalTexture);
            GUI.DrawTexture(bottomRect, MoveNormalTexture);
        }

        private void FitScreen()
        {
            windowRect.width = Mathf.Clamp(windowRect.width, minSize.x, UIScaler.MaxWidth);
            windowRect.height = Mathf.Clamp(windowRect.height, minSize.y, UIScaler.MaxHeight);
            windowRect.x = Mathf.Clamp(windowRect.x, 0, UIScaler.MaxWidth);
            windowRect.y = Mathf.Clamp(windowRect.y, 0, UIScaler.MaxHeight);
            if (windowRect.xMax > UIScaler.MaxWidth)
            {
                windowRect.x = UIScaler.MaxWidth - windowRect.width;
            }

            if (windowRect.yMax > UIScaler.MaxHeight)
            {
                windowRect.y = UIScaler.MaxHeight - windowRect.height;
            }
        }

        protected float TitleBarHeight => 12 + (12 * EffectiveFontSizeMultiplier);
        private void DrawTitlebar(Vector3 mouse)
        {
            var moveRect = new Rect(windowRect.x, windowRect.y, windowRect.width - 12 - 12 * EffectiveFontSizeMultiplier, TitleBarHeight);
            var moveTex = MoveNormalTexture;

            // TODO: reduce nesting
            if (!GUIUtility.hasModalWindow)
            {
                if (movingWindow != null)
                {
                    if (movingWindow == this)
                    {
                        moveTex = MoveHoverTexture;
                        GUI.contentColor = titleBarHover.ContrastColor();

                        if (Input.GetMouseButton(0))
                        {
                            var pos = new Vector2(mouse.x, mouse.y) + moveDragHandle;
                            windowRect.x = pos.x;
                            windowRect.y = pos.y;
                            FitScreen();
                        }
                        else
                        {
                            movingWindow = null;

                            OnWindowMoved(windowRect.position);
                        }
                    }
                }
                else if (moveRect.Contains(mouse))
                {
                    moveTex = MoveHoverTexture;
                    GUI.contentColor = titleBarHover.ContrastColor();
                    if (Input.GetMouseButtonDown(0) && resizingWindow == null)
                    {
                        movingWindow = this;
                        moveDragHandle = new Vector2(windowRect.x, windowRect.y) - new Vector2(mouse.x, mouse.y);
                    }
                }
                else
                {
                    GUI.contentColor = titleBar.ContrastColor();
                }
            }
            else
            {
                GUI.contentColor = titleBar.ContrastColor();
            }

            GUI.DrawTexture(new Rect(0.0f, 0.0f, windowRect.width, TitleBarHeight), moveTex, ScaleMode.StretchToFill);
            if (cachedModIcon is null)
            {
                cachedModIcon = KResourceLoader.LoadModTexture(BasicIUserMod.Instance.IconName);
                if (cachedModIcon is null)
                {
                    cachedModIcon = new Texture2D(1, 1);
                    cachedModIcon.SetPixel(0, 0, Color.clear);
                    cachedModIcon.Apply();
                }
                cachedModIcon.filterMode = FilterMode.Trilinear;
            }
            GUI.DrawTexture(new Rect(3.0f, 0.0f, TitleBarHeight, TitleBarHeight), cachedModIcon, ScaleMode.StretchToFill, true);
            GUI.Label(new Rect(18 + 12 * EffectiveFontSizeMultiplier, 0.0f, windowRect.width - 30 + 24 * EffectiveFontSizeMultiplier, TitleBarHeight - 2), Title, new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft
            });
            GUI.contentColor = Color.white;
        }

        private void DrawCloseButton(Vector3 mouse)
        {
            var closeRect = new Rect(windowRect.x + windowRect.width - TitleBarHeight, windowRect.y, TitleBarHeight, TitleBarHeight);
            var closeTex = CloseNormalTexture;

            if (!GUIUtility.hasModalWindow && closeRect.Contains(mouse))
            {
                closeTex = CloseHoverTexture;

                if (Input.GetMouseButtonDown(0))
                {
                    resizingWindow = null;
                    movingWindow = null;
                    Visible = false;
                    OnCloseButtonPress();
                }
            }
            var oldColor = GUI.contentColor;
            GUI.contentColor = Color.white;
            GUI.DrawTexture(new Rect(windowRect.width - TitleBarHeight, 0.0f, TitleBarHeight, TitleBarHeight), closeTex, ScaleMode.StretchToFill);
            GUI.Label(new Rect(windowRect.width - TitleBarHeight, 0.0f, TitleBarHeight, TitleBarHeight), "X", new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            });
            GUI.contentColor = oldColor;

        }

        protected virtual void OnCloseButtonPress()
        {
        }

        private void DrawResizeHandle(Vector3 mouse)
        {
            var resizeRect = new Rect(windowRect.x + windowRect.width - 12, windowRect.y + windowRect.height - 12, 12, 12);
            var resizeTex = ResizeNormalTexture;

            // TODO: reduce nesting
            if (!GUIUtility.hasModalWindow)
            {
                if (resizingWindow != null)
                {
                    if (resizingWindow == this)
                    {
                        resizeTex = ResizeHoverTexture;

                        if (Input.GetMouseButton(0))
                        {
                            var size = new Vector2(mouse.x, mouse.y)
                                + resizeDragHandle
                                - new Vector2(windowRect.x, windowRect.y);
                            windowRect.width = Mathf.Max(size.x, minSize.x);
                            windowRect.height = Mathf.Max(size.y, minSize.y);

                            // calling FitScreen() here causes gradual expansion of window when mouse is past the screen
                            // so we do like this:
                            windowRect.xMax = Mathf.Min(windowRect.xMax, UIScaler.MaxWidth);
                            windowRect.yMax = Mathf.Min(windowRect.yMax, UIScaler.MaxHeight);
                        }
                        else
                        {
                            resizingWindow = null;
                            OnWindowResized(windowRect.size);
                        }
                    }
                }
                else if (resizeRect.Contains(mouse))
                {
                    resizeTex = ResizeHoverTexture;
                    if (Input.GetMouseButtonDown(0))
                    {
                        resizingWindow = this;
                        resizeDragHandle =
                            new Vector2(windowRect.x + windowRect.width, windowRect.y + windowRect.height) -
                            new Vector2(mouse.x, mouse.y);
                    }
                }
            }

            GUI.DrawTexture(new Rect(windowRect.width - 12, windowRect.height - 12, 12, 12), resizeTex, ScaleMode.StretchToFill);
        }


    }
}