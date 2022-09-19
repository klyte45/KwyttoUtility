using ColossalFramework.UI;
using System.Linq;
using UnityEngine;

namespace Kwytto.UI
{
    public class GUIVerticalTabsContainer
    {
        public int CurrentTabIdx { get; private set; } = 0;

        private readonly IGUIVerticalITab[] m_tabs;
        private string[] m_tabsNamesList;

        public GUIVerticalTabsContainer(IGUIVerticalITab[] tabs)
        {
            m_tabs = tabs;
            m_tabsNamesList = tabs.Select(x => x.TabDisplayName).ToArray();
        }

        public void DrawListTabs(Rect area, float listWidth = 120)
        {
            using (new GUILayout.AreaScope(area))
            {
                var sideListArea = new Rect(0, 0, listWidth, area.height);
                using (new GUILayout.AreaScope(sideListArea))
                {
                    CurrentTabIdx = GUILayout.SelectionGrid(CurrentTabIdx, m_tabsNamesList, 1, new GUIStyle(GUI.skin.button) { wordWrap = true }); ;
                }
                var usedHeight = 0f;

                var tabAreaRect = new Rect(listWidth + 5, usedHeight, area.width - listWidth - 10, area.height);
                using (new GUILayout.AreaScope(tabAreaRect))
                {
                    m_tabs[CurrentTabIdx].DrawArea(tabAreaRect.size);
                };

            }
        }

        public void Reset()
        {
            m_tabs.ForEach(x => x.Reset());
            m_tabsNamesList = m_tabs.Select(x => x.TabDisplayName).ToArray();
            CurrentTabIdx = 0;
        }
    }
    public interface IGUIVerticalITab
    {
        void DrawArea(Vector2 tabAreaSize);
        void Reset();
        string TabDisplayName { get; }
    }
}
