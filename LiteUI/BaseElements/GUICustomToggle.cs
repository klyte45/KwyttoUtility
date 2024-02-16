using Kwytto.Utils;
using UnityEngine;

namespace Kwytto.LiteUI
{
    public static class GUICustomToggle
    {

        public static bool CustomToggle(bool isSelected, string title)
        {
            InitToggleLayouts();
            var shouldInvertValue = false;
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                var btnRect = GUILayoutUtility.GetLastRect();
                shouldInvertValue |= GUI.Button(new Rect(btnRect.position + new Vector2(2, 5), new Vector2(16, 16)), "", isSelected ? m_selectionBtnSel : m_selectionBtnUns);
                GUIKwyttoCommons.Space(5);
                shouldInvertValue |= GUILayout.Button(title, m_labelYellowOnHover);
            }
            return shouldInvertValue != isSelected;
        }

        public static GUIStyle SelectionBtnSel
        {
            get
            {
                if (m_selectionBtnSel is null)
                {
                    InitToggleLayouts();
                }
                return m_selectionBtnSel;
            }
        }
        public static GUIStyle SelectionBtnUns
        {
            get
            {
                if (m_selectionBtnUns is null)
                {
                    InitToggleLayouts();
                }
                return m_selectionBtnUns;

            }
        }
        private static GUIStyle m_selectionBtnSel;
        private static GUIStyle m_selectionBtnUns;
        private static GUIStyle m_labelYellowOnHover;
        private static void InitToggleLayouts()
        {
            if (m_selectionBtnSel is null)
            {
                m_selectionBtnSel = new GUIStyle(GUI.skin.label)
                {
                    normal =
                    {
                        background = KResourceLoader.LoadTextureKwytto(UI.CommonsSpriteNames.UI_Toggle_Checked)
                    },
                    fixedHeight = 20,
                    fixedWidth = 20,
                    margin = new RectOffset(0, 0, 5, 0),
                    padding = new RectOffset(),
                    border = new RectOffset()
                };
            }
            if (m_selectionBtnUns is null)
            {
                m_selectionBtnUns = new GUIStyle(GUI.skin.label)
                {
                    normal =
                    {
                        background = KResourceLoader.LoadTextureKwytto(UI.CommonsSpriteNames.UI_Toggle)
                    },
                    fixedHeight = 20,
                    fixedWidth = 20,
                    margin = new RectOffset(0, 0, 5, 0),
                    padding = new RectOffset(),
                    border = new RectOffset()
                };
            }
            if (m_labelYellowOnHover is null)
            {
                m_labelYellowOnHover = new GUIStyle(GUI.skin.label)
                {
                    onHover =
                    {
                        textColor = Color.yellow
                    },
                    hover =
                    {
                        textColor = Color.yellow
                    },
                };
            }
        }
    }
}