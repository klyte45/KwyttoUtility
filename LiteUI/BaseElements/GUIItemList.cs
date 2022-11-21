using System;
using UnityEngine;

namespace Kwytto.LiteUI
{
    public static class GUIItemList
    {
        private static Func<IDisposable> CreateHorizontalScope { get; } = () => new GUILayout.HorizontalScope();
        private static Func<IDisposable> CreateVerticalScope { get; } = () => new GUILayout.VerticalScope();
        private static GUIStyle m_horizontalBtn;
        private static GUIStyle m_verticalBtn;
        private static GUIStyle m_horizontalBtnSel;
        private static GUIStyle m_verticalBtnSel;
        private static void InitStyles()
        {
            if (m_horizontalBtn is null)
            {
                m_horizontalBtn = new GUIStyle(GUI.skin.button)
                {
                    wordWrap = false,
                    stretchWidth = false,
                    fixedHeight = 0,
                    fixedWidth = 0,
                    margin = new RectOffset(),
                    border = new RectOffset(2, 2, 2, 2),
                };
            }
            if (m_verticalBtn is null)
            {
                m_verticalBtn = new GUIStyle(GUI.skin.button)
                {
                    wordWrap = true,
                    stretchWidth = true,
                    stretchHeight = false,
                    fixedHeight = 0,
                    fixedWidth = 0,
                    border = new RectOffset(2, 2, 2, 2),
                    margin = new RectOffset()
                };
            }
            if (m_horizontalBtnSel is null)
            {
                m_horizontalBtnSel = new GUIStyle(m_horizontalBtn)
                {
                    normal =
                    {
                        background = GUIKwyttoCommons.almostWhiteTexture,
                        textColor = Color.black
                    },
                    hover =
                    {
                        background = GUIKwyttoCommons.whiteTexture,
                        textColor = Color.black
                    },
                };
            }
            if (m_verticalBtnSel is null)
            {
                m_verticalBtnSel = new GUIStyle(m_verticalBtn)
                {
                    normal =
                    {
                        background = GUIKwyttoCommons.almostWhiteTexture,
                        textColor = Color.black
                    },
                    hover =
                    {
                        background = GUIKwyttoCommons.whiteTexture,
                        textColor = Color.black
                    },
                };
            }
        }

        public static bool Draw(Rect sideListArea, ref Vector2 scrollPosition, int currentSelection, string[] listItems, string addButtonText, GUIStyle addButtonStyle, out int newSelection, bool horizontal, Action extraButtonsGeneration)
        {
            InitStyles();
            Func<IDisposable> layout = horizontal ? CreateHorizontalScope : CreateVerticalScope;
            using (new GUILayout.AreaScope(sideListArea))
            {
                using (layout())
                {
                    newSelection = currentSelection;
                    using (var scroll = new GUILayout.ScrollViewScope(scrollPosition))
                    {
                        using (layout())
                        {
                            for (int i = 0; i < listItems.Length; i++)
                            {
                                string item = listItems[i];
                                var isSel = i == currentSelection;
                                var targetStyle =
                                    horizontal
                                        ? isSel
                                            ? m_horizontalBtnSel
                                            : m_horizontalBtn
                                        : isSel
                                            ? m_verticalBtnSel
                                            : m_verticalBtn;
                                if (GUILayout.Button(item, targetStyle))
                                {
                                    newSelection = i;
                                }
                            }
                            GUILayout.FlexibleSpace();
                        }
                        scrollPosition = scroll.scrollPosition;
                    }
                    GUILayout.FlexibleSpace();
                    if (addButtonText != null && GUILayout.Button(addButtonText, addButtonStyle, GUILayout.ExpandWidth(!horizontal)))
                    {
                        newSelection = listItems.Length;
                        return true;
                    }
                    if (extraButtonsGeneration != null)
                    {
                        GUILayout.Space(10);
                        extraButtonsGeneration();
                    }
                }
            }
            return false;
        }
    }
}