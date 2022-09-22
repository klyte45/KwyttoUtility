extern alias UUI;
using Kwytto.LiteUI;
using Kwytto.Utils;
using System;
using UnityEngine;
using UUI::UnifiedUI.Helpers;

namespace Kwytto.Interfaces
{
    public class UUIWindowButtonContainer
    {
        private readonly Func<GUIWindow> window;


        private readonly UUICustomButton m_modButton;

        public UUIWindowButtonContainer(string buttonName, string iconPath, string tooltip, Func<GUIWindow> windowGetter)
        {
            window = windowGetter;
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
            if (window() is GUIWindow w)
            {
                m_modButton.IsPressed = false;
                w.Visible = false;
                m_modButton.Button?.Unfocus();
                ApplyButtonColor();
            }
        }

        private void ApplyButtonColor() => m_modButton.Button.color = Color.Lerp(Color.gray, m_modButton.IsPressed ? Color.white : Color.black, 0.5f);
        public void Open()
        {
            if (window() is GUIWindow w)
            {
                m_modButton.IsPressed = true;
                w.Visible = true;
                w.transform.position = new Vector3(25, 50);
                ApplyButtonColor();
            }
        }
    }

}
