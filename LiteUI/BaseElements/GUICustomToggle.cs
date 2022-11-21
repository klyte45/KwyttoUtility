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
                GUI.DrawTexture(new Rect(btnRect.position, new Vector2(20, 20)), GUIKwyttoCommons.whiteTexture);
                shouldInvertValue |= GUI.Button(new Rect(btnRect.position + new Vector2(2, 2), new Vector2(16, 16)), "", isSelected ? m_selectionBtnSel : m_selectionBtnUns);
                GUIKwyttoCommons.Space(5);
                shouldInvertValue |= GUILayout.Button(title, m_labelYellowOnHover);
            }
            return shouldInvertValue != isSelected;
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
                        background = GUIKwyttoCommons.greenTexture
                    },
                    hover =
                    {
                        background = GUIKwyttoCommons.yellowTexture
                    },
                    fixedHeight = 16,
                    fixedWidth = 16
                };
            }
            if (m_selectionBtnUns is null)
            {
                m_selectionBtnUns = new GUIStyle(GUI.skin.label)
                {
                    normal =
                    {
                        background = GUIKwyttoCommons.blackTexture
                    },
                    hover =
                    {
                        background = GUIKwyttoCommons.whiteTexture
                    },
                    fixedHeight = 16,
                    fixedWidth = 16
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