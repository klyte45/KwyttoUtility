extern alias UUI;
using Kwytto.LiteUI;
using Kwytto.Utils;
using UnityEngine;
using UUI::UnifiedUI.Helpers;

namespace Kwytto.Interfaces
{
    public class UUIWindowButtonContainer
    {
        private readonly GUIWindow window;


        private readonly UUICustomButton m_modButton;

        public UUIWindowButtonContainer(string buttonName, string iconPath, string tooltip, GUIWindow window)
        {
            this.window = window;
            m_modButton = UUIHelpers.RegisterCustomButton(
             name: buttonName,
             groupName: "Klyte45",
             tooltip: tooltip,
             onToggle: (value) => { if (value) { Open(); } else { Close(); } },
             onToolChanged: null,
             icon: KResourceLoader.LoadTextureMod(iconPath),
             hotkeys: new UUIHotKeys { }

             );
            Close();
        }

        public void Close()
        {
            m_modButton.IsPressed = false;
            window.Visible = false;
            m_modButton.Button?.Unfocus();
            ApplyButtonColor();
        }

        private void ApplyButtonColor() => m_modButton.Button.color = Color.Lerp(Color.gray, m_modButton.IsPressed ? Color.white : Color.black, 0.5f);
        public void Open()
        {
            m_modButton.IsPressed = true;
            window.Visible = true;
            window.transform.position = new Vector3(25, 50);
            ApplyButtonColor();
        }
    }

}
