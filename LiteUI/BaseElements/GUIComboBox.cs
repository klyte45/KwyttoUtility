﻿using UnityEngine;

namespace Kwytto.LiteUI
{
    public static class GUIComboBox
    {
        private const string ExpandDownButtonText = " ▼ ";
        private static PopupWindow popupWindow;

        public static int Box(int itemIndex, string[] items, string callerId, GUIRootWindowBase root, float? maxWidth = null)
        {
            if (Initialize(ref itemIndex, items, callerId, maxWidth, out var maxWidthObj) is int retNum)
            {
                return retNum;
            }

            var popupSize = GetPopupDimensions(items);

            GUILayout.Box(itemIndex < 0 ? GUIKwyttoCommons.v_null : itemIndex >= items.Length ? GUIKwyttoCommons.v_invalid : items[itemIndex], maxWidthObj);
            var lastRect = GUILayoutUtility.GetLastRect();
            if (GUILayout.Button(ExpandDownButtonText, GUILayout.Width(24f)) && EnsurePopupWindow(root))
            {
                var popupPosition = (GUIUtility.GUIToScreenPoint(default) + lastRect.position) * UIScaler.UIScale;
                if (lastRect.width * UIScaler.UIScale > popupSize.x)
                {
                    popupSize.x = lastRect.width * UIScaler.UIScale;
                }
                popupPosition.y += lastRect.height * UIScaler.UIScale;
                popupWindow.Show(callerId, items, itemIndex, popupPosition, popupSize);
            }

            return itemIndex;
        }
        public static int Button(int itemIndex, string[] items, string callerId, GUIRootWindowBase root, float? maxWidth = null)
        {
            if (Initialize(ref itemIndex, items, callerId, maxWidth, out var maxWidthObj) is int retNum)
            {
                return retNum;
            }
            GUILayout.Space(0);
            var lastRect = GUILayoutUtility.GetLastRect();
            if (GUILayout.Button(itemIndex < 0 ? GUIKwyttoCommons.v_null : itemIndex >= items.Length ? GUIKwyttoCommons.v_invalid : items[itemIndex], maxWidthObj) && EnsurePopupWindow(root))
            {
                var popupSize = GetPopupDimensions(items);
                var popupPosition = (GUIUtility.GUIToScreenPoint(default) + lastRect.position) * UIScaler.UIScale;
                if ((maxWidth ?? 0) > popupSize.x)
                {
                    popupSize.x = maxWidth ?? popupSize.x;
                }
                popupPosition.y += lastRect.height; ;
                popupWindow.Show(callerId, items, itemIndex, popupPosition, popupSize);
            }

            return itemIndex;
        }

        private static int? Initialize(ref int itemIndex, string[] items, string callerId, float? maxWidth, out GUILayoutOption maxWidthObj)
        {
            maxWidthObj = null;
            if (maxWidth != null)
            {
                maxWidthObj = GUILayout.MaxWidth((maxWidth ?? 4) - 4);
            }
            switch (items.Length)
            {
                case 0:
                    GUILayout.Box(GUIKwyttoCommons.v_null, maxWidthObj);
                    return -1;

                case 1:
                    if (itemIndex == 0)
                    {
                        GUILayout.Box(items[0], maxWidthObj);
                        return 0;
                    }
                    break;

            }

            if (popupWindow != null
                && callerId == popupWindow.OwnerId
                && popupWindow.CloseAndGetSelection(out var newSelectedIndex))
            {
                itemIndex = newSelectedIndex;
                Object.Destroy(popupWindow);
                popupWindow = null;
            }
            return null;
        }

        private static bool EnsurePopupWindow(GUIRootWindowBase root)
        {
            if (popupWindow != null)
            {
                return true;
            }

            if (root == null)
            {
                return false;
            }

            if (root.GetComponent<PopupWindow>() == null)
            {
                popupWindow = root.gameObject.AddComponent<PopupWindow>();
            }

            return popupWindow != null;
        }

        private static Vector2 GetPopupDimensions(string[] items)
        {
            float width = 0;
            float height = 0;

            for (var i = 0; i < items.Length; ++i)
            {
                var itemSize = GUI.skin.button.CalcSize(new GUIContent(items[i]));
                if (itemSize.x + 3 > width)
                {
                    width = itemSize.x + 3;
                }

                height += itemSize.y + 1;
            }

            return new Vector2(width + 36 * UIScaler.UIScale, height + 36 * UIScaler.UIScale);
        }

        private sealed class PopupWindow : MonoBehaviour
        {
            private const float MaxPopupHeight = 400f;

            private static readonly GUIStyle WindowStyle = CreateWindowStyle();

            private readonly int popupWindowId = GUIUtility.GetControlID(FocusType.Passive);
            private readonly GUIStyle hoverStyle;

            private Vector2 popupScrollPosition = Vector2.zero;

            private Rect popupRect;
            private Vector2? mouseClickPoint;
            private bool readyToClose;
            private int selectedIndex;

            private string[] popupItems;

            public PopupWindow() => hoverStyle = CreateHoverStyle();

            public string OwnerId { get; private set; }

            public void Show(string ownerId, string[] items, int currentIndex, Vector2 position, Vector2 popupSize)
            {
                OwnerId = ownerId;
                popupItems = items;
                selectedIndex = currentIndex;
                popupRect = new Rect(position, new Vector2(popupSize.x, Mathf.Min(MaxPopupHeight, popupSize.y, Screen.height - position.y - 16)));
                popupScrollPosition = default;
                mouseClickPoint = null;
                readyToClose = false;
            }

            public bool CloseAndGetSelection(out int currentIndex)
            {
                if (readyToClose)
                {
                    currentIndex = selectedIndex;
                    Close();
                    return true;
                }

                currentIndex = -1;
                return false;
            }

            public void OnGUI()
            {
                if (OwnerId != null)
                {
                    GUI.ModalWindow(popupWindowId, popupRect, WindowFunction, string.Empty, WindowStyle);
                }
            }

            public void Update()
            {
                if (OwnerId == null)
                {
                    return;
                }

                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                {
                    var mousePos = Input.mousePosition;
                    mousePos.y = Screen.height - mousePos.y;
                    mouseClickPoint = mousePos;
                }
                else
                {
                    mouseClickPoint = null;
                }
            }

            private static GUIStyle CreateHoverStyle()
            {
                var result = new GUIStyle(GUI.skin.label);
                result.hover.textColor = Color.black;
                result.hover.background = GUIKwyttoCommons.greenTexture;
                result.font = GUI.skin.font;

                result.focused.textColor = Color.yellow;
                result.focused.background = GUIKwyttoCommons.darkGreenTexture;


                return result;
            }

            private static GUIStyle CreateWindowStyle()
            {
                var background = new Texture2D(16, 16, TextureFormat.ARGB32, false, true)
                {
                    wrapMode = TextureWrapMode.Clamp,
                };

                for (var x = 0; x < background.width; x++)
                {
                    for (var y = 0; y < background.height; y++)
                    {
                        if (x == 0 || x == background.width - 1 || y == 0 || y == background.height - 1)
                        {
                            background.SetPixel(x, y, new Color(0, 0, 0, 1));
                        }
                        else
                        {
                            background.SetPixel(x, y, new Color(0.05f, 0.05f, 0.05f, 0.95f));
                        }
                    }
                }

                background.Apply();

                var result = new GUIStyle(GUI.skin.window);
                result.normal.background = background;
                result.onNormal.background = background;
                result.border.top = result.border.bottom;
                result.padding.top = result.padding.bottom;

                return result;
            }

            private void WindowFunction(int windowId)
            {
                if (OwnerId == null)
                {
                    return;
                }

                popupScrollPosition = GUILayout.BeginScrollView(popupScrollPosition, false, false);

                var oldSelectedIndex = selectedIndex;
                selectedIndex = GUILayout.SelectionGrid(selectedIndex, popupItems, xCount: 1, new GUIStyle(hoverStyle)
                {
                    fontSize = Mathf.RoundToInt(16 * UIScaler.UIScale)
                });

                GUILayout.EndScrollView();

                if (oldSelectedIndex != selectedIndex || (mouseClickPoint.HasValue && !popupRect.Contains(mouseClickPoint.Value)))
                {
                    readyToClose = true;
                }
            }

            private void Close()
            {
                OwnerId = null;
                popupItems = null;
                selectedIndex = -1;
                mouseClickPoint = null;
            }
        }
    }
}