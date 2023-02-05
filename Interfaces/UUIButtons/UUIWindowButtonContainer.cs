extern alias UUI;

using ColossalFramework.UI;
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
        private bool isBinded;

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
            m_modButton.IsPressed = false;
            if (m_modButton.Button is UIButton btn)
            {
                btn.state = UIButton.ButtonState.Normal;
            }
        }

        public void Close()
        {
            if (window() is GUIWindow w)
            {
                CheckBind(w);
                m_modButton.IsPressed = false;
                w.Visible = false;
                m_modButton.Button?.Unfocus();
            }
        }
        public void CheckBind(GUIWindow w)
        {
            if (!isBinded)
            {
                w.EventVisibilityChanged += (x) =>
                {
                    m_modButton.IsPressed = x;
                    if (x)
                    {
                        m_modButton.Button.Focus();
                    }
                    else
                    {
                        m_modButton.Button.Unfocus();
                    }
                };
                isBinded = true;
            }
        }
        public void Open()
        {
            if (window() is GUIWindow w)
            {
                CheckBind(w);
                m_modButton.IsPressed = true;
                w.Visible = true;
                w.transform.position = new Vector3(25, 50);
                m_modButton.Button.Focus();
                GUI.BringWindowToFront(w.Id);
            }
        }
    }

}
