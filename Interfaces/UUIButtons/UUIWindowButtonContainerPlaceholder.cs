extern alias UUI;

using ColossalFramework.UI;
using Kwytto.LiteUI;
using System;

namespace Kwytto.Interfaces
{
    public class UUIWindowButtonContainerPlaceholder : IUUIButtonContainerPlaceholder
    {
        public readonly string buttonName;
        public readonly string iconPath;
        public readonly string tooltip;
        public readonly Func<GUIWindow> windowGetter;
        private UUIWindowButtonContainer m_button;

        public UUIWindowButtonContainerPlaceholder(string buttonName, string iconPath, string tooltip, Func<GUIWindow> windowGetter)
        {
            this.buttonName = buttonName;
            this.iconPath = iconPath;
            this.tooltip = tooltip;
            this.windowGetter = windowGetter;
        }

        public void Create()
        {
            m_button = new UUIWindowButtonContainer(buttonName, iconPath, tooltip, windowGetter);

        }
        public void Destroy()
        {
            m_button = null;
        }
        public void Open() => m_button?.Open();
        public void Close() => m_button?.Close();
    }

}
