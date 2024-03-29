﻿using ColossalFramework.UI;
using Kwytto.LiteUI;
using System;
using System.Linq;
using UnityEngine;

namespace Kwytto.UI
{
    public class GUIVerticalTabsContainer
    {
        public int CurrentTabIdx { get; set; } = 0;

        private readonly IGUIVerticalITab[] m_tabs;
        private string[] m_tabsNamesList;
        public Texture2D m_listBgTexture;

        public GUIVerticalTabsContainer(IGUIVerticalITab[] tabs)
        {
            m_tabs = tabs;
            m_tabsNamesList = tabs.Select(x => x.TabDisplayName).ToArray();
        }
        [Obsolete("Use version 2: listwidth wasn't ensured to multiply by the resolution multiplier", true)]
        public void DrawListTabs(Rect area, float listWidth = 120)
        {
            DrawListTabs2(area, listWidth );
        }
        public void DrawListTabs2(Rect area, float listWidth = 120)
        {
            using (new GUILayout.AreaScope(area))
            {
                var sideListArea = new Rect(0, 0, listWidth, area.height);
                if (m_listBgTexture != null)
                {
                    GUI.DrawTexture(sideListArea, m_listBgTexture);
                }
                using (new GUILayout.AreaScope(sideListArea))
                {
                    CurrentTabIdx = GUILayout.SelectionGrid(CurrentTabIdx, m_tabsNamesList, 1, new GUIStyle(GUI.skin.button) { wordWrap = true }); ;
                }
                var usedHeight = 0f;

                var tabAreaRect = new Rect(listWidth + 5 , usedHeight, area.width - listWidth - 10 , area.height);
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
