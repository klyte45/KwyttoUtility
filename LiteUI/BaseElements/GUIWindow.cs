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
        private Vector2 maxSize = new Vector2(999999, 999999);
        protected Rect windowRect = new Rect(0, 0, 64, 64);

        private bool visible;
        protected abstract bool showOverModals { get; }
        protected abstract bool requireModal { get; }
        protected virtual bool moveEverywhere { get; } = false;
        private Texture2D cachedModIcon;

        private bool oldModalState;
        private int oldModalZorder;
        public event Action<bool> EventVisibilityChanged;

        protected virtual float FontSizeMultiplier { get; } = 1;
        public float BgOpacity
        {
            get => bgOpacity; set
            {
                bgOpacity = value;
                if (BgTexture != null)
                {
                    BgTexture.SetPixel(0, 0, new Color(bgColor.r, bgColor.g, bgColor.b, bgOpacity));
                    BgTexture.Apply();
                }
                OnOpacityChanged(bgOpacity);
            }
        }
        protected float EffectiveFontSizeMultiplier => FontSizeMultiplier * ResolutionMultiplier;
        public static float ResolutionMultiplier => Screen.height / 1080f;

        protected virtual bool ShowCloseButton { get; } = true;
        protected virtual bool ShowMinimizeButton { get; } = false;
        public bool Minimized { get; protected set; } = false;

        public static GUIStyle HighlightStyle => GUIStyle.none;
        public int DefaultSize => Mathf.RoundToInt(16 * EffectiveFontSizeMultiplier);

        protected void Init(string title, Rect rect, bool resizable = true, bool hasTitlebar = true, Vector2 minSize = default, Vector2 maxSize = default)
        {
            Id = UnityEngine.Random.Range(1024, int.MaxValue);
            Title = title ?? BasicIUserMod.Instance.GeneralName;
            windowRect = rect;
            Resizable = resizable;
            HasTitlebar = hasTitlebar;
            this.minSize = minSize == default ? new Vector2(64.0f, 64.0f) : minSize;
            this.maxSize = maxSize == default ? this.maxSize : maxSize;
            windowRect.size = Vector2.Min(this.maxSize, Vector2.Max(windowRect.size, this.minSize));
            Panel = gameObject.AddComponent<UIPanel>();
            Panel.zOrder = int.MaxValue;
            Windows.Add(this);
            visible = false;
            Visible = true;
        }
        protected virtual void OnOpacityChanged(float newVal)
        {
        }
        protected Rect TitleBarArea => new Rect(windowRect.x, windowRect.y, TitleBarWidthMinimized, TitleBarHeight);
        public Rect WindowRect => Minimized && HasTitlebar && ShowMinimizeButton ? TitleBarArea : windowRect;
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
                        if (requireModal)
                        {
                            UIView.PushModal(Panel);
                        }
                        GUI.BringWindowToFront(Id);
                        OnWindowOpened();

                    }
                    else
                    {
                        if (requireModal)
                        {
                            if (UIView.GetModalComponent() == Panel)
                            {
                                UIView.PopModal();
                            }
                            UIView.GetAView().panelsLibraryModalEffect.isVisible = oldModalState;
                            UIView.GetAView().panelsLibraryModalEffect.zOrder = oldModalZorder;
                        }
                        OnWindowClosed();
                    }
                }
                EventVisibilityChanged?.Invoke(value);
            }
        }

        protected Texture2D BgTexture { get; set; }

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
                if (UIView.GetModalComponent() == Panel)
                {
                    UIView.PopModal();
                }
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
                bgColor = BasicIUserMod.Instance.ModColor.SetBrightness(.15f);
                titleBar = BasicIUserMod.Instance.ModColor.SetBrightness(.33f);
                titleBarHover = BasicIUserMod.Instance.ModColor.SetBrightness(1);

                BgTexture = TextureUtils.New(1, 1);
                BgTexture.SetPixel(0, 0, new Color(bgColor.r, bgColor.g, bgColor.b, bgOpacity));
                BgTexture.Apply();

                ResizeNormalTexture = TextureUtils.New(1, 1);
                ResizeNormalTexture.SetPixel(0, 0, Color.white);
                ResizeNormalTexture.Apply();

                ResizeHoverTexture = TextureUtils.New(1, 1);
                ResizeHoverTexture.SetPixel(0, 0, Color.blue);
                ResizeHoverTexture.Apply();

                CloseNormalTexture = TextureUtils.New(1, 1);
                CloseNormalTexture.SetPixel(0, 0, ColorExtensions.FromRGB("AA0000"));
                CloseNormalTexture.Apply();

                CloseHoverTexture = TextureUtils.New(1, 1);
                CloseHoverTexture.SetPixel(0, 0, ColorExtensions.FromRGB("FF6666"));
                CloseHoverTexture.Apply();

                MoveNormalTexture = TextureUtils.New(1, 1);
                MoveNormalTexture.SetPixel(0, 0, titleBar);
                MoveNormalTexture.Apply();

                MoveHoverTexture = TextureUtils.New(1, 1);
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
                Skin.toggle = new GUIStyle(GUI.skin.toggle)
                {
                    wordWrap = true
                };
                Skin.verticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);
                Skin.verticalScrollbarDownButton = new GUIStyle(GUI.skin.verticalScrollbarDownButton);
                Skin.verticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);
                Skin.verticalScrollbarUpButton = new GUIStyle(GUI.skin.verticalScrollbarUpButton);
                Skin.verticalSlider = new GUIStyle(GUI.skin.verticalSlider);
                Skin.verticalSliderThumb = new GUIStyle(GUI.skin.verticalSliderThumb);
                Skin.window = new GUIStyle(GUI.skin.window);
                Skin.window.normal.background = BgTexture;
                Skin.window.onNormal.background = BgTexture;
                Skin.window.clipping = TextClipping.Overflow;


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
                if (UIView.GetModalComponent() != Panel)
                {
                    Panel.isVisible = false;
                    return;
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
                BeforeDrawWindow();
                var windowRect = GUI.Window(Id, WindowRect, WindowFunction, string.Empty);
                Panel.absolutePosition = windowRect.position / ResolutionMultiplier;
                Panel.enabled = true;
                if (!Minimized)
                {
                    Panel.size = windowRect.size / ResolutionMultiplier;
                    this.windowRect = windowRect;
                }
                else
                {
                    Panel.size = new Vector2(TitleBarWidthMinimized, TitleBarHeight) / ResolutionMultiplier;
                }

                OnWindowDrawn();
            }
            finally
            {
                GUI.matrix = oldMatrix;

                GUI.skin = oldSkin;
            }
        }

        public static Texture2D LoadHighlightTexture() => highlightTexture = KResourceLoader.LoadTextureKwytto(UI.CommonsSpriteNames.highlight);

        public void MoveResize(Rect newWindowRect) => windowRect = newWindowRect;

        private bool isOnTop;
        private float bgOpacity = 1;

        private void Update()
        {
            if (requireModal)
            {
                Visible = UIView.GetModalComponent() == Panel;
            }
            else
            {
                var mouseOverWindow = Visible && WindowRect.Contains(UIScaler.MousePosition);
                if (mouseOverWindow && !(UIView.GetModalComponent()?.isVisible ?? false))
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
            return Windows.FindIndex(window => window.Visible && window.WindowRect.Contains(mouse)) >= 0;
        }

        protected abstract void DrawWindow(Vector2 size);
        protected Action DrawOverWindow;
        protected virtual void HandleException(Exception ex)
        {
        }
        protected virtual void BeforeDrawWindow()
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
            var effectiveIsMinimized = HasTitlebar && ShowMinimizeButton && Minimized;
            try
            {
                if (!effectiveIsMinimized)
                {
                    var effectiveArea = HasTitlebar ? new Rect(0, TitleBarHeight, windowRect.width, windowRect.height - TitleBarHeight) : new Rect(default, windowRect.size);
                    using (new GUILayout.AreaScope(effectiveArea))
                    {
                        DrawWindow(effectiveArea.size);
                    }
                }
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
                if (ShowMinimizeButton)
                {
                    DrawMinimizeButton(mouse);
                }
                if (ShowCloseButton)
                {
                    DrawCloseButton(mouse);
                }
            }
            else if (moveEverywhere)
            {
                CheckMoveAnywhere(mouse);
            }

            if (Resizable && !effectiveIsMinimized)
            {
                DrawResizeHandle(mouse);
            }
            if (DrawOverWindow != null || GUI.tooltip?.TrimToNull() != null)
            {
                DrawOverWindow?.Invoke();
                if (GUI.tooltip?.TrimToNull() != null)
                {
                    var tooltipRect = GUILayoutUtility.GetRect(new GUIContent(GUI.tooltip), GUI.skin.label);
                    tooltipRect.width += 4;
                    tooltipRect.height += 4;
                    var position = GUIUtility.ScreenToGUIPoint(default) + UIScaler.MousePosition;
                    tooltipRect.position = new Vector2(position.x, position.y - tooltipRect.height);
                    GUI.Label(tooltipRect, GUIKwyttoCommons.blackTexture);
                    GUI.Label(new Rect(tooltipRect.position + (Vector2.one * 2), tooltipRect.size - Vector2.one * 4), GUI.tooltip);
                }
            }
        }

        private void DrawBorder()
        {
            var leftRect = new Rect(0.0f, 0.0f, 1.0f, WindowRect.height);
            var rightRect = new Rect(WindowRect.width - 1.0f, 0.0f, 1.0f, WindowRect.height);
            var bottomRect = new Rect(0.0f, WindowRect.height - 1.0f, WindowRect.width, 1.0f);
            GUI.DrawTexture(leftRect, MoveNormalTexture);
            GUI.DrawTexture(rightRect, MoveNormalTexture);
            GUI.DrawTexture(bottomRect, MoveNormalTexture);
            if (!HasTitlebar)
            {
                var topRect = new Rect(0.0f, 0.0f, WindowRect.width, 1.0f);
                GUI.DrawTexture(topRect, MoveNormalTexture);
            }
        }

        private void FitScreen()
        {
            var windowRectCalc = WindowRect;
            var minSize = Minimized && HasTitlebar && ShowMinimizeButton ? new Vector2(TitleBarWidthMinimized, TitleBarHeight) : this.minSize;
            windowRectCalc.width = Mathf.Clamp(windowRectCalc.width, minSize.x * ResolutionMultiplier, Mathf.Min(UIScaler.MaxWidth, maxSize.x * ResolutionMultiplier));
            windowRectCalc.height = Mathf.Clamp(windowRectCalc.height, minSize.y * ResolutionMultiplier, Mathf.Min(UIScaler.MaxHeight, maxSize.y * ResolutionMultiplier));
            windowRect.x = Mathf.Clamp(windowRectCalc.x, 0, UIScaler.MaxWidth);
            windowRect.y = Mathf.Clamp(windowRectCalc.y, 0, UIScaler.MaxHeight);
            if (windowRectCalc.xMax > UIScaler.MaxWidth)
            {
                windowRect.x = UIScaler.MaxWidth - windowRectCalc.width;
            }

            if (windowRectCalc.yMax > UIScaler.MaxHeight)
            {
                windowRect.y = UIScaler.MaxHeight - windowRectCalc.height;
            }
        }

        protected float TitleBarHeight => (12 * ResolutionMultiplier) + (16 * EffectiveFontSizeMultiplier);
        protected float TitleBarWidthMinimized => (160 * ResolutionMultiplier);

        private void CheckMoveAnywhere(Vector3 mouse)
        {
            if (!GUIUtility.hasModalWindow)
            {
                if (movingWindow == this)
                {
                    DoMove(mouse);
                }
                else if (WindowRect.Contains(mouse))
                {
                    StartMove(mouse);
                }
            }
        }
        private void DrawTitlebar(Vector3 mouse)
        {
            var moveRect = new Rect(windowRect.x, windowRect.y, (Minimized ? TitleBarWidthMinimized : windowRect.width) - (TitleBarHeight * ((ShowMinimizeButton ? 1 : 0) + (ShowCloseButton ? 1 : 0))), TitleBarHeight);
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
                        DoMove(mouse);
                    }
                }
                else if (moveRect.Contains(mouse))
                {
                    moveTex = MoveHoverTexture;
                    GUI.contentColor = titleBarHover.ContrastColor();
                    StartMove(mouse);
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
                cachedModIcon = KResourceLoader.LoadTextureMod(BasicIUserMod.Instance.IconName);
                if (cachedModIcon is null)
                {
                    cachedModIcon = TextureUtils.New(1, 1);
                    cachedModIcon.SetPixel(0, 0, Color.clear);
                    cachedModIcon.Apply();
                }
                cachedModIcon.filterMode = FilterMode.Trilinear;
            }
            GUI.DrawTexture(new Rect(3.0f, 0.0f, TitleBarHeight, TitleBarHeight), cachedModIcon, ScaleMode.StretchToFill, true);
            GUI.Label(new Rect(18 * ResolutionMultiplier + 16 * EffectiveFontSizeMultiplier, 0.0f, windowRect.width - 30 * ResolutionMultiplier + 32 * EffectiveFontSizeMultiplier, TitleBarHeight - 2 * EffectiveFontSizeMultiplier), Title, new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft
            });
            GUI.contentColor = Color.white;
        }

        private void StartMove(Vector3 mouse)
        {
            if (Event.current.type == EventType.mouseDown && Input.GetMouseButtonDown(0) && resizingWindow == null)
            {
                movingWindow = this;
                moveDragHandle = new Vector2(windowRect.x, windowRect.y) - new Vector2(mouse.x, mouse.y);
            }
        }

        private void DoMove(Vector3 mouse)
        {
            if (movingWindow == this)
            {
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

        private void DrawCloseButton(Vector3 mouse)
        {
            var closeRect = new Rect(WindowRect.x + WindowRect.width - TitleBarHeight, WindowRect.y, TitleBarHeight, TitleBarHeight);
            var closeTex = CloseNormalTexture;

            if (!GUIUtility.hasModalWindow && closeRect.Contains(mouse))
            {
                closeTex = CloseHoverTexture;

                if (Event.current.type == EventType.mouseDown && Input.GetMouseButtonDown(0))
                {
                    resizingWindow = null;
                    movingWindow = null;
                    Visible = false;
                    if (UIView.GetModalComponent() == Panel)
                    {
                        UIView.PopModal();
                    }
                    OnCloseButtonPress();
                }
            }
            var oldColor = GUI.contentColor;
            GUI.contentColor = Color.white;
            GUI.DrawTexture(new Rect(WindowRect.width - TitleBarHeight, 0.0f, TitleBarHeight, TitleBarHeight), closeTex, ScaleMode.StretchToFill);
            GUI.Label(new Rect(WindowRect.width - TitleBarHeight, 0.0f, TitleBarHeight, TitleBarHeight), "X", new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            });
            GUI.contentColor = oldColor;
        }
        private void DrawMinimizeButton(Vector3 mouse)
        {
            var xOffset = (TitleBarHeight * (ShowCloseButton ? 2 : 1));
            var btnRect = new Rect(WindowRect.x + WindowRect.width - xOffset, windowRect.y, TitleBarHeight, TitleBarHeight);
            var btnTex = ResizeNormalTexture;

            if (!GUIUtility.hasModalWindow && btnRect.Contains(mouse))
            {
                btnTex = ResizeHoverTexture;

                if (Event.current.type == EventType.MouseUp && Input.GetMouseButtonUp(0))
                {
                    Minimized = !Minimized;
                }
            }
            var oldColor = GUI.contentColor;
            GUI.contentColor = Color.black;
            GUI.DrawTexture(new Rect(WindowRect.width - xOffset, 0.0f, TitleBarHeight, TitleBarHeight), btnTex, ScaleMode.StretchToFill);
            GUI.Label(new Rect(WindowRect.width - xOffset, 0.0f, TitleBarHeight, TitleBarHeight), Minimized ? "□" : "_", new GUIStyle(GUI.skin.label)
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
                            windowRect.size = Vector2.Min(Vector2.Max(size, minSize), maxSize);

                            // calling FitScreen() here causes gradual expansion of window when mouse is past the screen
                            // so we do like this:
                            windowRect.xMax = Mathf.Min(windowRect.xMax, UIScaler.MaxWidth);
                            windowRect.yMax = Mathf.Min(windowRect.yMax, UIScaler.MaxHeight);
                            windowRect.xMin = Mathf.Max(windowRect.xMin, 0);
                            windowRect.yMin = Mathf.Max(windowRect.yMin, 0);
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
                    if (Event.current.type == EventType.mouseDown && Input.GetMouseButtonDown(0))
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