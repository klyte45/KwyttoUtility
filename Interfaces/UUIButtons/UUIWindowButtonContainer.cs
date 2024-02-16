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


        private UUICustomButton m_modButton;
        private bool isBinded;
        private Func<bool> shallShowButton;
        private readonly string buttonName;
        private readonly string iconPath;
        private readonly string tooltip;

        public UUIWindowButtonContainer(string buttonName, string iconPath, string tooltip, Func<GUIWindow> windowGetter, Func<bool> shallShowButton = null)
        {
            window = windowGetter;
            this.shallShowButton = shallShowButton;
            this.buttonName = buttonName;
            this.iconPath = iconPath;
            this.tooltip = tooltip;
            if (shallShowButton?.Invoke() ?? true)
            {
                CreateBtn();
            }
        }

        private void CreateBtn()
        {
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
                btn.enabled = shallShowButton?.Invoke() ?? true;
            }
        }

        public void UpdateVisibility()
        {
            if (m_modButton != null != (shallShowButton?.Invoke() ?? true))
            {
                if (m_modButton is null)
                {
                    CreateBtn();
                }
                else
                {
                    GameObject.Destroy(m_modButton.Button);
                    m_modButton.Release();
                    m_modButton = null;
                }
            }
        }

        public void Close()
        {
            if (window() is GUIWindow w)
            {
                CheckBind(w);
                if (m_modButton != null) m_modButton.IsPressed = false;
                w.Visible = false;
                if (m_modButton != null) m_modButton.Button?.Unfocus();
            }
        }
        public void CheckBind(GUIWindow w)
        {
            if (!isBinded)
            {
                w.EventVisibilityChanged += (x) =>
                {
                    if (m_modButton != null)
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
                if (m_modButton != null) m_modButton.IsPressed = true;
                w.Visible = true;
                w.transform.position = new Vector3(25, 50);
                if (m_modButton != null) m_modButton.Button.Focus();
                GUI.BringWindowToFront(w.Id);
            }
        }
    }

}
