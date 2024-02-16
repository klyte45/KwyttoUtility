extern alias UUI;
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
        public readonly Func<bool> shallShowButton;

        public UUIWindowButtonContainerPlaceholder(string buttonName, string iconPath, string tooltip, Func<GUIWindow> windowGetter, Func<bool> shallShowButton = null)
        {
            this.buttonName = buttonName;
            this.iconPath = iconPath;
            this.tooltip = tooltip;
            this.windowGetter = windowGetter;
            this.shallShowButton = shallShowButton;
        }

        public void Create()
        {
            m_button = new UUIWindowButtonContainer(buttonName, iconPath, tooltip, windowGetter, shallShowButton);
        }
        public void Destroy()
        {
            m_button = null;
        }
        public void UpdateVisibility() => m_button?.UpdateVisibility();
        public void Open() => m_button?.Open();
        public void Close() => m_button?.Close();
    }

}
