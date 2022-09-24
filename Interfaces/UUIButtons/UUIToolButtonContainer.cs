extern alias UUI;

using ColossalFramework.UI;
using Kwytto.Tools;
using Kwytto.Utils;
using UnityEngine;
using UUI::UnifiedUI.Helpers;

namespace Kwytto.Interfaces
{
    public class UUIToolButtonContainer
    {
        private readonly UIComponent m_modButton;
        public UUIToolButtonContainer(string buttonName, string iconPath, string tooltip, KwyttoToolBase tool)
        {
            m_modButton = UUIHelpers.RegisterToolButton(
              name: buttonName,
              groupName: "Klyte45",
              tooltip: tooltip,
              tool: tool,
              icon: KResourceLoader.LoadTextureMod(iconPath)
          );
            tool.EventEnableChanged += ApplyButtonColor;
            ApplyButtonColor(false);

            if (m_modButton is UIButton btn)
            {
                btn.state = UIButton.ButtonState.Normal;
            }
        }
        private void ApplyButtonColor(bool active)
        {
            m_modButton.color = Color.Lerp(Color.gray, active ? Color.white : Color.black, 0.5f);
            m_modButton.Unfocus();
        }
    }

}
