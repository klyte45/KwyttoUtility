using ColossalFramework.UI;
using System;
using UnityEngine;

namespace Kwytto.LiteUI
{
    internal class KwyttoDialog : GUIRootWindowBase
    {
        private BindProperties properties;
        protected override bool ShowCloseButton => properties.showClose;
        protected override float FontSizeMultiplier => 2f;
        public static KwyttoDialog ShowModal(BindProperties properties)
        {
            var parent = UIView.GetAView();
            var container = new GameObject();
            container.transform.SetParent(parent.transform);
            var uiItem = container.AddComponent<KwyttoDialog>();
            uiItem.Init(properties);
            return uiItem;
        }
        public void Init(BindProperties properties)
        {
            requireModal = true;
            this.properties = properties;
            base.Init(properties.title, new Rect(200, (Screen.height / 2) - 250, Screen.width - 400, 500), false, true, new Vector2(500, 500));
            properties.showClose |= properties.buttons is null || properties.buttons.Length == 0;
            this.Visible = true;
        }

        protected override void DrawWindow()
        {
            var area = new Rect(5, TitleBarHeight, WindowRect.width - 10, WindowRect.height - TitleBarHeight);
            using (new GUILayout.AreaScope(area))
            {
                var textArea = new Rect(0, 0, area.size.x, area.size.y - 60 * ResolutionMultiplier); ;
                var buttonsArea = new Rect(0, area.size.y - 60 * ResolutionMultiplier, area.size.x, 60 * ResolutionMultiplier);
                using (new GUILayout.AreaScope(textArea))
                {
                    GUILayout.Label(properties.message, new GUIStyle(GUI.skin.label)
                    {
                        alignment = properties.messageAlign
                    });

                }
                using (new GUILayout.AreaScope(buttonsArea))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        foreach (var button in properties.buttons)
                        {
                            if (button.IsSpace)
                            {
                                GUILayout.FlexibleSpace();
                            }
                            else if (button.onClick is null)
                            {
                                GUILayout.Label(button.title, GUILayout.ExpandHeight(true));
                            }
                            else
                            {
                                if (GUILayout.Button(button.title, GUILayout.ExpandHeight(true)))
                                {
                                    if (button.onClick())
                                    {
                                        Destroy(gameObject);
                                        return;
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }
        protected override void OnWindowClosed()
        {
            base.OnWindowClosed();
            Destroy(gameObject);
        }

        #region Extra Classes
        internal struct ButtonDefinition
        {
            public string title;
            public Func<bool> onClick;
            public bool IsSpace;

        }
        internal struct BindProperties
        {
            public string title;
            // public string icon;
            public bool showClose;
            public string message;
            public TextAnchor messageAlign;
            public ButtonDefinition[] buttons;
            // public bool showTextField;
            // public bool showDropDown;
            // public string[] dropDownOptions;
            //  public int dropDownCurrentSelection;
            //   public string defaultTextFieldContent;

        }
        #endregion
    }
}
