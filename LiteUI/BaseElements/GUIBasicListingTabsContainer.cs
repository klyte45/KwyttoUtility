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
        private readonly Action<bool> m_extraButtonsGen;
        public event Action<int> EventListItemChanged;
        public int ListSel
        {
            get => m_listSel;
            set
            {
                if (m_listSel != value)
                {
                    m_listSel = value;
                    EventListItemChanged?.Invoke(m_listSel);
                    if (m_listSel >= 0)
                    {
                        foreach (var x in m_tabsUI)
                        {
                            x.Reset();
                        }
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

        private readonly IGUITab<T>[] m_tabsUI;

        public GUIBasicListingTabsContainer(IGUITab<T>[] tabsImages, Action onAdd, Func<string[]> listGetter, Func<int, T> currentItemGetter, Action<int, T> onSetItemAt, Action<bool> extraButtonsGeneration = null)
        {
            m_onAdd = onAdd;
            m_listGetter = listGetter;
            m_tabsUI = tabsImages;
            m_currentItemGetter = currentItemGetter;
            m_onSetItemAt = onSetItemAt;
            m_extraButtonsGen = extraButtonsGeneration;
        }

        public void DrawListTabs(Rect area, bool allowAdd = true, bool horizontal = false)
        {
            var usedHeight = 0f;
            using (new GUILayout.AreaScope(area))
            {
                var sideListArea = horizontal ? new Rect(0, 0, area.width, usedHeight += 48 ) : new Rect(0, 0, 200 , area.height);
                var sideList = m_listGetter() ?? new string[0];
                var addItemText = KStr.comm_addItemList;
                if (GUIKwyttoCommons.CreateItemList(sideListArea, ref m_scrollPosition, ListSel, sideList, allowAdd ? addItemText : null, GreenButton, out int newSel, horizontal, () => m_extraButtonsGen?.Invoke(allowAdd)))
                {
                    m_onAdd();
                }
                ListSel = newSel;
                if (ListSel >= 0 && ListSel < sideList.Length)
                {

                    using (new GUILayout.AreaScope(horizontal ? new Rect(0, usedHeight, area.width, usedHeight += 48 ) : new Rect(205 , 0, area.width - 215 , usedHeight += 48 )))
                    {
                        CurrentTabIdx = GUILayout.SelectionGrid(CurrentTabIdx, m_tabsUI.Select(x => x.TabIcon).ToArray(), m_tabsUI.Length, new GUIStyle(GUI.skin.button)
                        {
                            fixedWidth = 48,
                            fixedHeight = 48,
                        });
                    }

                    var tabAreaRect = new Rect(horizontal ? 0 : 205 , usedHeight, horizontal ? area.width : area.width - 210 , area.height - usedHeight);
                    using (new GUILayout.AreaScope(tabAreaRect))
                    {
                        if (CurrentTabIdx >= 0 && CurrentTabIdx < m_tabsUI.Length)
                        {
                            var item = m_currentItemGetter(ListSel);
                            if (m_tabsUI[CurrentTabIdx].DrawArea(tabAreaRect.size, ref item, ListSel, allowAdd))
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
        bool DrawArea(Vector2 tabAreaSize, ref T currentItem, int currentItemIdx, bool isEditable);
        void Reset();
        Texture TabIcon { get; }
    }
}
