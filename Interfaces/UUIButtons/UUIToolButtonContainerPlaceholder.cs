extern alias UUI;
using Kwytto.Tools;
using System;

namespace Kwytto.Interfaces
{
    public class UUIToolButtonContainerPlaceholder : IUUIButtonContainerPlaceholder
    {
        public readonly string buttonName;
        public readonly string iconPath;
        public readonly string tooltip;
        public readonly Func<KwyttoToolBase> toolGetter;

        public UUIToolButtonContainerPlaceholder(string buttonName, string iconPath, string tooltip, Func<KwyttoToolBase> toolGetter)
        {
            this.buttonName = buttonName;
            this.iconPath = iconPath;
            this.tooltip = tooltip;
            this.toolGetter = toolGetter;
        }

        public void Create()
        {
            new UUIToolButtonContainer(buttonName, iconPath, tooltip, toolGetter());
        }
        public void Destroy()
        {
        }
    }

}
