﻿using ColossalFramework;
using ColossalFramework.UI;
using Kwytto.Localization;
using Kwytto.Utils;
using System;
using UnityEngine;

namespace Kwytto.LiteUI
{
    public class KwyttoDialog : GUIRootWindowBase
    {
        private BindProperties properties;
        protected override bool ShowCloseButton => properties.showClose;
        protected override float FontSizeMultiplier => 1f;

        private Vector2 scrollPosition;

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
            this.properties = properties;
            base.Init(properties.title, new Rect(new Vector2((Screen.width / 2) - 400 , (Screen.height / 2) - 250 ) / UIScaler.UIScale, new Vector2(800, 500)  / UIScaler.UIScale), false, true, new Vector2(500, 500));
            properties.showClose = properties.buttons is null || properties.buttons.Length == 0;
            this.Visible = true;
        }

        protected override void DrawWindow(Vector2 size)
        {
            var area = new Rect(5 , 0, size.x - 10 , size.y);
            using (new GUILayout.AreaScope(area))
            {
                var textArea = new Rect(0, 0, area.size.x, area.size.y - 30 ); ;
                var buttonsArea = new Rect(0, area.size.y - 30 , area.size.x, 30 );
                using (new GUILayout.AreaScope(textArea))
                {
                    using (new GUILayout.VerticalScope())
                    {
                        if (properties.message.TrimToNull() != null)
                        {
                            GUILayout.Label((properties.messageTextSizeMultiplier > 0 ? $"<size={Mathf.RoundToInt(properties.messageTextSizeMultiplier * DefaultSize)}>" : "") + properties.message + (properties.messageTextSizeMultiplier > 0 ? "</size>" : "") + "\n", new GUIStyle(GUI.skin.label)
                            {
                                alignment = properties.messageAlign
                            }, GUILayout.ExpandHeight(properties.scrollText.IsNullOrWhiteSpace()), GUILayout.ExpandWidth(true));
                        }
                        if (!properties.scrollText.IsNullOrWhiteSpace())
                        {
                            using (var view = new GUILayout.ScrollViewScope(scrollPosition))
                            {
                                GUILayout.Label((properties.scrollTextSizeMultiplier > 0 ? $"<size={Mathf.RoundToInt(properties.scrollTextSizeMultiplier * DefaultSize)}>" : "") + properties.scrollText + (properties.scrollTextSizeMultiplier > 0 ? "</size>" : "") + "\n",
                                    new GUIStyle(GUI.skin.label)
                                    {
                                        alignment = properties.scrollTextAlign
                                    },
                                    GUILayout.ExpandWidth(true));
                                scrollPosition = view.scrollPosition;
                            }
                        }
                    }
                }
                using (new GUILayout.AreaScope(buttonsArea))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        foreach (var button in properties.buttons)
                        {
                            if (button.isSpace)
                            {
                                GUILayout.FlexibleSpace();
                            }
                            else if (button.onClick is null)
                            {
                                GUILayout.Label(button.title, new GUIStyle(GUI.skin.label)
                                {
                                    alignment = TextAnchor.MiddleCenter,
                                },
                                GUILayout.ExpandHeight(true));
                            }
                            else
                            {
                                if (GUILayout.Button(button.title, new GUIStyle(GetButtonStyleGui(button.style))
                                {
                                    alignment = TextAnchor.MiddleCenter,
                                }))
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
        protected override void OnCloseButtonPress()
        {
            Destroy(gameObject);
        }

        #region Extra Classes
        public struct ButtonDefinition
        {
            public string title;
            public Func<bool> onClick;
            public bool isSpace;
            public ButtonStyle style;
        }

        public static readonly ButtonDefinition SpaceBtn = new ButtonDefinition { isSpace = true };

        public enum ButtonStyle
        {
            Default,
            White,
            Green,
            Red
        }
        public void Update()
        {
            Visible = true;
        }

        private GUIStyle GetButtonStyleGui(ButtonStyle style)
        {
            switch (style)
            {
                case ButtonStyle.White:
                    return WhiteButton;
                case ButtonStyle.Green:
                    return GreenButton;
                case ButtonStyle.Red:
                    return RedButton;
                default:
                case ButtonStyle.Default:
                    return GUI.skin.button;
            }
        }

        public static readonly ButtonDefinition[] basicOkButtonBar = new ButtonDefinition[]
        {
            SpaceBtn,
            new ButtonDefinition
            {
                title = KStr.comm_releaseNotes_Ok,
                onClick = ()=>true
            }
        };

        public struct BindProperties
        {
            public string title;
            public string icon;
            public bool showClose;
            public float messageTextSizeMultiplier;
            public string message;
            public TextAnchor messageAlign;
            public ButtonDefinition[] buttons;
            public float scrollTextSizeMultiplier;
            public string scrollText;
            public TextAnchor scrollTextAlign;
            // public bool showTextField;
            // public bool showDropDown;
            // public string[] dropDownOptions;
            //  public int dropDownCurrentSelection;
            //   public string defaultTextFieldContent;

        }
        #endregion

        #region Styles


        private GUIStyle m_greenButton;
        private GUIStyle m_redButton;
        private GUIStyle m_whiteButton;
        public GUIStyle GreenButton
        {
            get
            {
                if (m_greenButton is null)
                {
                    m_greenButton = new GUIStyle(Skin.button)
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



        public GUIStyle RedButton
        {
            get
            {
                if (m_redButton is null)
                {
                    m_redButton = new GUIStyle(Skin.button)
                    {
                        normal = new GUIStyleState()
                        {
                            background = GUIKwyttoCommons.darkRedTexture,
                            textColor = Color.white
                        },
                        hover = new GUIStyleState()
                        {
                            background = GUIKwyttoCommons.redTexture,
                            textColor = Color.white
                        },
                    };
                }
                return m_redButton;
            }
        }
        public GUIStyle WhiteButton
        {
            get
            {
                if (m_whiteButton is null)
                {
                    m_whiteButton = new GUIStyle(Skin.button)
                    {
                        normal = new GUIStyleState()
                        {
                            background = GUIKwyttoCommons.almostWhiteTexture,
                            textColor = Color.black
                        },
                        hover = new GUIStyleState()
                        {
                            background = GUIKwyttoCommons.whiteTexture,
                            textColor = Color.black
                        },
                    };
                }
                return m_whiteButton;
            }
        }

        protected override bool showOverModals => true;

        protected override bool requireModal => true;

        #endregion
    }
}
