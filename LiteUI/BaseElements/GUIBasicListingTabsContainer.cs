using Kwytto.LiteUI;
using Kwytto.Localization;
using System;
using System.Linq;
using UnityEngine;

namespace Kwytto.UI
{
    public class GUIBasicListingTabsContainer<T>
    {
        private int m_listSel = -1;
        private readonly Action m_onAdd;
        private readonly Action<int, T> m_onSetItemAt;
        private readonly Func<string[]> m_listGetter;
        private readonly Func<int, T> m_currentItemGetter;
        public event Action<int> EventListItemChanged;
        public int ListSel
        {
            get => m_listSel;
            private set
            {
                if (m_listSel != value)
                {
                    m_listSel = value;
                    EventListItemChanged?.Invoke(m_listSel);
                    if (m_listSel >= 0)
                    {
                        foreach (var x in m_tabsImages)
                        {
                            x.Reset();
                        }
                        CurrentTabIdx = 0;
                    }
                }
            }
        }

        public int CurrentTabIdx { get; private set; } = 0;

        private GUIStyle m_greenButton;
        private GUIStyle GreenButton
        {
            get
            {
                if (m_greenButton is null)
                {
                    m_greenButton = new GUIStyle(GUI.skin.button)
                    {
                        normal = new GUIStyleState()
                        {
                            background = GUIKwyttoCommons.darkGreenTexture,
                            textColor = Color.white
                        },
                        hover = new GUIStyleState()
                        {
                            background = GUIKwyttoCommons.greenTexture,
                            textColor = Color.black
                        },
                    };
                }
                return m_greenButton;
            }
        }

        private Vector2 m_scrollPosition;

        private readonly IGUITab<T>[] m_tabsImages;

        public GUIBasicListingTabsContainer(IGUITab<T>[] tabsImages, Action onAdd, Func<string[]> listGetter, Func<int, T> currentItemGetter, Action<int, T> onSetItemAt)
        {
            m_onAdd = onAdd;
            m_listGetter = listGetter;
            m_tabsImages = tabsImages;
            m_currentItemGetter = currentItemGetter;
            m_onSetItemAt = onSetItemAt;
        }

        public void DrawListTabs(Rect area, bool allowAdd = true)
        {
            using (new GUILayout.AreaScope(area))
            {
                var sideListArea = new Rect(0, 0, 120, area.height);
                var sideList = m_listGetter() ?? new string[0];
                var addItemText = KStr.comm_addItemList;
                if (GUIKwyttoCommons.CreateItemVerticalList(sideListArea, ref m_scrollPosition, ListSel, sideList, allowAdd ? addItemText : null, GreenButton, out int newSel))
                {
                    m_onAdd();
                }
                ListSel = newSel;
                if (ListSel >= 0 && ListSel < sideList.Length)
                {
                    var usedHeight = 0f;

                    using (new GUILayout.AreaScope(new Rect(125, 0, area.width - 135, usedHeight += 40)))
                    {
                        CurrentTabIdx = GUILayout.SelectionGrid(CurrentTabIdx, m_tabsImages.Select(x => x.TabIcon).ToArray(), m_tabsImages.Length, new GUIStyle(GUI.skin.button)
                        {
                            fixedWidth = 32,
                            fixedHeight = 32,
                        });
                    }

                    var tabAreaRect = new Rect(125, usedHeight, area.width - 130, area.height - usedHeight);
                    using (new GUILayout.AreaScope(tabAreaRect))
                    {
                        if (CurrentTabIdx >= 0 && CurrentTabIdx < m_tabsImages.Length)
                        {
                            var item = m_currentItemGetter(ListSel);
                            if (m_tabsImages[CurrentTabIdx].DrawArea(tabAreaRect.size, ref item, ListSel))
                            {
                                m_onSetItemAt(ListSel, item);
                            }
                        }
                    };
                }
            }
        }

        public void Reset() => ListSel = -1;
    }
    public interface IGUITab<T>
    {
        bool DrawArea(Vector2 tabAreaSize, ref T currentItem, int currentItemIdx);
        void Reset();
        Texture TabIcon { get; }
    }
}
