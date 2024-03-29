﻿using Kwytto.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace Kwytto.LiteUI
{
    public static class GUIComboBox
    {
        private const string ExpandDownButtonText = " ▼ ";
        private static PopupWindow popupWindow;

        public static int Box(int itemIndex, string[] items, string callerId, GUIRootWindowBase root, float? maxWidth = null, string nullStr = GUIKwyttoCommons.v_null, string forceBoxText = null, Func<int, string> onNullNameValue = null)
        {
            if (Initialize(ref itemIndex, items, callerId, maxWidth, out var maxWidthObj, new GUIContent(nullStr), forceBoxText is null ? null : new GUIContent(forceBoxText), onNullNameValue) is int retNum)
            {
                return retNum;
            }

            var popupSize = GetPopupDimensions(items);

            GUILayout.Box(forceBoxText ?? (itemIndex < 0 ? nullStr : itemIndex >= items.Length ? GUIKwyttoCommons.v_invalid : items[itemIndex].TrimToNull() is string str ? str : onNullNameValue?.Invoke(itemIndex) ?? GUIKwyttoCommons.v_empty), GUIWindow.DropDownBg, maxWidthObj is null ? new GUILayoutOption[0] : new[] { maxWidthObj });
            var lastRect = GUILayoutUtility.GetLastRect();
            if (GUILayout.Button(ExpandDownButtonText, GUILayout.Width(24f)) && EnsurePopupWindow(root))
            {
                var popupPosition = (GUIUtility.GUIToScreenPoint(default) + lastRect.position) * UIScaler.UIScale;
                if (lastRect.width * UIScaler.UIScale > popupSize.x)
                {
                    popupSize.x = lastRect.width * UIScaler.UIScale;
                }
                popupPosition.y += lastRect.height * UIScaler.UIScale;
                popupWindow.Show(callerId, items, itemIndex, popupPosition, popupSize, onNullNameValue);
            }

            return itemIndex;
        }
        public static int Button(int itemIndex, string[] items, string callerId, GUIRootWindowBase root, float? maxWidth = null, string nullStr = GUIKwyttoCommons.v_null)
        {
            if (Initialize(ref itemIndex, items, callerId, maxWidth, out var maxWidthObj, new GUIContent(nullStr)) is int retNum)
            {
                return retNum;
            }
            GUILayout.Space(0);
            var lastRect = GUILayoutUtility.GetLastRect();
            var content = new GUIContent(itemIndex < 0 ? nullStr : itemIndex >= items.Length ? GUIKwyttoCommons.v_invalid : items[itemIndex]);
            if (GUILayout.Button(content, maxWidthObj is null ? new GUILayoutOption[0] : new[] { maxWidthObj }) && EnsurePopupWindow(root))
            {
                var popupSize = GetPopupDimensions(items);
                var popupPosition = (GUIUtility.GUIToScreenPoint(default) + lastRect.position) * UIScaler.UIScale;
                if ((maxWidth ?? 0) > popupSize.x)
                {
                    popupSize.x = maxWidth ?? popupSize.x;
                }
                popupPosition.y += GUI.skin.button.CalcHeight(content, maxWidth ?? 9999);
                popupWindow.Show(callerId, items, itemIndex, popupPosition, popupSize);
            }

            return itemIndex;
        }
        public static int ContextMenuRect(Rect rect, string[] items, string callerId, GUIRootWindowBase root, string contentShow = ExpandDownButtonText, GUIStyle style = null)
            => ContextMenuRect(rect, items, callerId, root, new GUIContent(contentShow), style);
        public static int ContextMenuRect(Rect rect, string[] items, string callerId, GUIRootWindowBase root, GUIContent contentShow, GUIStyle style = null)
        {
            var itemIndex = -1;
            if (Initialize(ref itemIndex, items, callerId, null, out _, contentShow) is int retNum)
            {
                return retNum;
            };
            if (GUI.Button(rect, contentShow, style ?? GUI.skin.button) && EnsurePopupWindow(root))
            {
                itemIndex = -2;
                var popupSize = GetPopupDimensions(items);
                var popupPosition = (GUIUtility.GUIToScreenPoint(default) + rect.position) * UIScaler.UIScale;
                popupPosition.y += rect.height;
                popupWindow.Show(callerId, items, -3, popupPosition, popupSize);
            }

            return itemIndex;
        }

        private static int? Initialize(ref int itemIndex, string[] items, string callerId, float? maxWidth, out GUILayoutOption maxWidthObj, GUIContent nullStr, GUIContent overrideContent = null, Func<int, string> onNullNameValue = null)
        {
            maxWidthObj = null;
            if (maxWidth != null)
            {
                maxWidthObj = GUILayout.MaxWidth(((maxWidth ?? 4) - 4));
            }
            switch (items.Length)
            {
                case 0:
                    GUILayout.Box(overrideContent ?? nullStr, maxWidthObj is null ? new GUILayoutOption[0] : new[] { maxWidthObj });
                    return -1;

                case 1:
                    if (itemIndex == 0)
                    {
                        var contentTxt = items[0] ?? onNullNameValue?.Invoke(0);
                        GUILayout.Box(overrideContent ?? (contentTxt is null ? nullStr : new GUIContent(contentTxt)), maxWidthObj is null ? new GUILayoutOption[0] : new[] { maxWidthObj });
                        return 0;
                    }
                    break;

            }

            if (popupWindow != null
                && callerId == popupWindow.OwnerId
                && popupWindow.CloseAndGetSelection(out var newSelectedIndex))
            {
                itemIndex = newSelectedIndex;
                GameObject.Destroy(popupWindow);
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
                if (itemSize.x + 8 > width)
                {
                    width = itemSize.x + 8;
                }

                height += itemSize.y + 1;
            }

            return new Vector2(width, height) * UIScaler.UIScale + new Vector2(56, 36);
        }

        private sealed class PopupWindow : MonoBehaviour
        {
            private const float MaxPopupHeight = 400f;

            private static GUIStyle WindowStyle => GUIWindow.DropDownBg; //CreateWindowStyle();

            private readonly int popupWindowId = GUIUtility.GetControlID(FocusType.Passive);
            private readonly GUIStyle hoverStyle;

            private Vector2 popupScrollPosition = Vector2.zero;

            private Rect popupRect;
            private Vector2? mouseClickPoint;
            private bool readyToClose;
            private int selectedIndex;

            private string[] popupItems;
            private Func<int, string> onNullNameValue;

            public PopupWindow() => hoverStyle = CreateHoverStyle();

            public string OwnerId { get; private set; }

            public void Show(string ownerId, string[] items, int currentIndex, Vector2 position, Vector2 popupSize, Func<int, string> onNullNameValue = null)
            {
                OwnerId = ownerId;
                popupItems = items;
                selectedIndex = currentIndex;
                this.onNullNameValue = onNullNameValue;
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
                selectedIndex = GUILayout.SelectionGrid(selectedIndex, onNullNameValue is null ? popupItems : popupItems.Select((x, i) => x.TrimToNull() ?? onNullNameValue(i)).ToArray(), xCount: 1, new GUIStyle(hoverStyle)
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
                onNullNameValue = null;
                selectedIndex = -1;
                mouseClickPoint = null;
            }
        }
    }
}