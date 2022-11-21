using ColossalFramework.UI;
using Kwytto.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace Kwytto.LiteUI
{
    public static class GUIKwyttoCommons
    {
        public const string v_null = "<color=#FF00FF>--NULL--</color>";
        public const string v_empty = "<color=#888888>--EMPTY--</color>";
        public const string v_invalid = "<color=#ff0000>--INVALID--</color>";
        public const string v_all = "<color=#FFFF00>--ALL--</color>";
        public static readonly Texture2D darkGreenTexture;
        public static readonly Texture2D greenTexture;
        public static readonly Texture2D yellowTexture;
        public static readonly Texture2D darkRedTexture;
        public static readonly Texture2D redTexture;
        public static readonly Texture2D almostWhiteTexture;
        public static readonly Texture2D whiteTexture;
        public static readonly Texture2D blackTexture;
        public static readonly Texture2D darkTransparentTexture;

        static GUIKwyttoCommons()
        {
            darkGreenTexture = CreateTextureOfColor(Color.Lerp(Color.green, Color.black, 0.8f));
            greenTexture = CreateTextureOfColor(Color.Lerp(Color.green, Color.black, 0.12f));
            darkRedTexture = CreateTextureOfColor(Color.Lerp(Color.red, Color.black, 0.8f));
            redTexture = CreateTextureOfColor(Color.Lerp(Color.red, Color.black, 0.12f));
            almostWhiteTexture = CreateTextureOfColor(Color.Lerp(Color.white, Color.black, 0.28f));
            whiteTexture = CreateTextureOfColor(Color.Lerp(Color.white, Color.gray, 0.12f));
            blackTexture = CreateTextureOfColor(Color.Lerp(Color.black, Color.gray, 0.12f));
            yellowTexture = CreateTextureOfColor(Color.Lerp(Color.yellow, Color.black, 0.12f));
            darkTransparentTexture = CreateTextureOfColor(Color.Lerp(Color.black, Color.clear, 0.5f));
        }

        private static Texture2D CreateTextureOfColor(Color src)
        {
            var texture = TextureUtils.New(1, 1);
            texture.SetPixel(0, 0, src);
            texture.Apply();
            return texture;
        }
        public static Texture GetByNameFromDefaultAtlas(string name) => UIView.GetAView().defaultAtlas?.sprites?.Where(x => x.name == name).FirstOrDefault()?.texture;

        #region Vector inputs
        public static bool AddVector2Field(float totalWidth, Vector2Xml input, string title, string baseFieldName, bool isEditable = true, float minValue = float.MinValue, float maxValue = float.MaxValue)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title, GUILayout.Width(totalWidth / 2));
                GUILayout.FlexibleSpace();
                if (isEditable)
                {
                    var x = GUIFloatField.FloatField(baseFieldName + "_X", input.X, minValue, maxValue);
                    var y = GUIFloatField.FloatField(baseFieldName + "_Y", input.Y, minValue, maxValue);
                    var changed = x != input.X || y != input.Y;
                    input.X = x;
                    input.Y = y;
                    return changed;
                }
                else
                {
                    GUILayout.Label(input.X.ToString("F3"));
                    GUILayout.Label(input.Y.ToString("F3"));
                    return false;
                }
            };
        }
        public static bool AddVector3Field(float totalWidth, Vector3Xml input, string title, string baseFieldName, bool isEditable = true, float minValue = float.MinValue, float maxValue = float.MaxValue)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title, GUILayout.Width(totalWidth / 2));
                GUILayout.FlexibleSpace();
                if (isEditable)
                {
                    var x = GUIFloatField.FloatField(baseFieldName + "_X", input.X, minValue, maxValue);
                    var y = GUIFloatField.FloatField(baseFieldName + "_Y", input.Y, minValue, maxValue);
                    var z = GUIFloatField.FloatField(baseFieldName + "_Z", input.Z, minValue, maxValue);
                    var changed = x != input.X || y != input.Y || z != input.Z;
                    input.X = x;
                    input.Y = y;
                    input.Z = z;
                    return changed;
                }
                else
                {
                    GUILayout.Label(input.X.ToString("F3"));
                    GUILayout.Label(input.Y.ToString("F3"));
                    GUILayout.Label(input.Z.ToString("F3"));
                    return false;
                }
            };
        }
        public static bool AddVector4Field(float totalWidth, Vector4Xml input, string title, string baseFieldName, bool isEditable = true, float minValue = float.MinValue, float maxValue = float.MaxValue)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title, GUILayout.Width(totalWidth / 2));
                GUILayout.FlexibleSpace();
                if (isEditable)
                {
                    var x = GUIFloatField.FloatField(baseFieldName + "_X", input.X, minValue, maxValue);
                    var y = GUIFloatField.FloatField(baseFieldName + "_Y", input.Y, minValue, maxValue);
                    var z = GUIFloatField.FloatField(baseFieldName + "_Z", input.Z, minValue, maxValue);
                    var w = GUIFloatField.FloatField(baseFieldName + "_W", input.W, minValue, maxValue);
                    var changed = x != input.X || y != input.Y || z != input.Z || w != input.W;
                    input.X = x;
                    input.Y = y;
                    input.Z = z;
                    input.W = w;
                    return changed;
                }
                else
                {
                    GUILayout.Label(input.X.ToString("F3"));
                    GUILayout.Label(input.Y.ToString("F3"));
                    GUILayout.Label(input.Z.ToString("F3"));
                    GUILayout.Label(input.W.ToString("F3"));
                    return false;
                }
            };
        }
        public static bool AddVector4Field(float totalWidth, Vector4 input, string title, string baseFieldName, out Vector4 newVal, bool isEditable = true, float minValue = float.MinValue, float maxValue = float.MaxValue)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title, GUILayout.Width(totalWidth / 2));
                GUILayout.FlexibleSpace();
                if (isEditable)
                {
                    var x = GUIFloatField.FloatField(baseFieldName + "_X", input.x, minValue, maxValue);
                    var y = GUIFloatField.FloatField(baseFieldName + "_Y", input.y, minValue, maxValue);
                    var z = GUIFloatField.FloatField(baseFieldName + "_Z", input.z, minValue, maxValue);
                    var w = GUIFloatField.FloatField(baseFieldName + "_W", input.w, minValue, maxValue);
                    var changed = x != input.x || y != input.y || z != input.z || w != input.w;
                    input.x = x;
                    input.y = y;
                    input.z = z;
                    input.w = w;
                    newVal = input;
                    return changed;
                }
                else
                {
                    GUILayout.Label(input.x.ToString("F3"));
                    GUILayout.Label(input.y.ToString("F3"));
                    GUILayout.Label(input.z.ToString("F3"));
                    GUILayout.Label(input.w.ToString("F3"));
                    newVal = input;
                    return false;
                }
            };
        }

        public static bool AddVector3Field(float totalWidth, ref Vector3 input, string title, string baseFieldName, bool isEditable = true, float minValue = float.MinValue, float maxValue = float.MaxValue)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title, GUILayout.Width(totalWidth / 2));
                GUILayout.FlexibleSpace();
                if (isEditable)
                {
                    var x = GUIFloatField.FloatField(baseFieldName + "_X", input.x, minValue, maxValue);
                    var y = GUIFloatField.FloatField(baseFieldName + "_Y", input.y, minValue, maxValue);
                    var z = GUIFloatField.FloatField(baseFieldName + "_Z", input.z, minValue, maxValue);
                    var changed = x != input.x || y != input.y || z != input.z;
                    input.x = x;
                    input.y = y;
                    input.z = z;
                    return changed;
                }
                else
                {
                    GUILayout.Label(input.x.ToString("F3"));
                    GUILayout.Label(input.y.ToString("F3"));
                    GUILayout.Label(input.z.ToString("F3"));
                    return false;
                }
            };
        }
        #endregion
        #region Utility UI structures

        public static void AddButtonSelector(float totalWidth, string label, string buttonText, Action action, bool enabled = true, GUIStyle style = null)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(totalWidth / 3));
                AddButtonSelector(buttonText, action, enabled);
            };
        }

        public static void AddButtonSelector(string buttonText, Action action, bool enabled, GUIStyle style = null)
        {
            if (!enabled)
            {
                AddButtonSelectorDisabled(buttonText, style);
            }
            else
            {
                AddButtonSelector(buttonText, action, style);
            }
        }

        private static void AddButtonSelector(string buttonText, Action action, GUIStyle style)
        {
            if (buttonText == "")
            {
                buttonText = v_empty;
            }
            if (GUILayout.Button(buttonText ?? v_null, style ?? GUI.skin.button))
            {
                action();
            }
        }

        private static string AddButtonSelectorDisabled(string buttonText, GUIStyle style)
        {
            if (buttonText == "")
            {
                buttonText = v_empty;
            }
            GUILayout.Label(buttonText ?? v_null, style ?? GUI.skin.button);
            return buttonText;
        }

        public static bool TextWithLabel(float totalWidth, string label, string value, Action<string> action, bool enabled = true, float textFieldProportion = .6f)
        {
            if (!enabled)
            {
                TextWithLabelDisabled(totalWidth, label, value);
                return false;
            }
            textFieldProportion = Mathf.Clamp(textFieldProportion, .1f, .6f);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(totalWidth * (1 - textFieldProportion - .2f)));
                GUILayout.FlexibleSpace();
                var newText = GUILayout.TextField(value ?? "", GUILayout.Height(20), GUILayout.MaxWidth(totalWidth * textFieldProportion));
                if (value != newText)
                {
                    action(newText);
                }
                return value != newText;
            };
        }
        public static void TextWithLabelDisabled(float totalWidth, string label, string value)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(totalWidth / 3));
                if (value == "")
                {
                    value = v_empty;
                }
                GUILayout.FlexibleSpace();
                GUILayout.Label(value ?? v_null);
            };
        }
        public static bool AddColorPicker(string title, GUIColorPicker picker, Color? value, Action<Color?> onChange, bool enabled = true)
        {
            if (AddColorPicker(title, picker, ref value, enabled))
            {
                onChange(value);
                return true;
            }
            return false;
        }
        public static bool AddColorPicker(string title, GUIColorPicker picker, ref Color? value, bool enabled = true)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title);
                var newVal = picker.PresentColor(title, value, enabled);
                var changed = enabled && newVal != value;
                if (changed)
                {
                    value = newVal;
                }
                return changed;
            };
        }

        [Obsolete("Use version 2: size here is not ensured to be multiplied by resolution factor", true)]
        public static void SquareTextureButton(Texture2D icon, string tooltip, Action onClick, bool condition = true, int size = 30, GUIStyle style = null)
        {
            SquareTextureButton2(icon, tooltip, onClick, condition, size, style);
        }
        public static void SquareTextureButton2(Texture2D icon, string tooltip, Action onClick, bool condition = true, float size = 30, GUIStyle style = null)
        {
            if (condition && GUILayout.Button(new GUIContent(icon, tooltip), new GUIStyle(style ?? GUI.skin.button)
            {
                contentOffset = default,
                padding = new RectOffset(),
                fixedHeight = size,
                fixedWidth = size
            }))
            {
                onClick();
            }
        }
        public static bool AddToggle(string title, ref bool currentVal, bool editable = true, bool condition = true) => AddToggle(title, currentVal, out currentVal, editable, condition);
        public static bool AddToggle(string title, bool currentVal, out bool newVal, bool editable = true, bool condition = true)
        {
            if (condition && currentVal != (newVal = GUICustomToggle.CustomToggle(currentVal, title)) && editable)
            {
                return true;
            }
            newVal = currentVal;
            return false;
        }
        public static bool AddToggle(string title, bool currentVal, Action<bool> newValCall, bool editable = true, bool condition = true)
        {
            if (AddToggle(title, currentVal, out bool newVal, editable, condition))
            {
                newValCall(newVal);
                return true;
            }
            return false;
        }

        public static void Space(float size)
        {
            GUILayout.Space(size);
        }

        public static bool AddSlider(float totalWidth, string i18nLocale, float value, out float newVal, float min, float max, bool isEnabled = true)
        {
            var result = AddSlider(totalWidth, i18nLocale, ref value, min, max, isEnabled);
            newVal = value;
            return result;
        }
        public static bool AddSlider(float totalWidth, string i18nLocale, float value, Action<float> newValCall, float min, float max, bool isEnabled = true)
        {
            var result = AddSlider(totalWidth, i18nLocale, ref value, min, max, isEnabled);
            if (result)
            {
                newValCall(value);
            };
            return result;
        }
        public static bool AddSlider(float totalWidth, string title, ref float value, float min, float max, bool isEnabled = true)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title);
                GUILayout.FlexibleSpace();
                GUILayout.Space(totalWidth / 3);
                if (isEnabled)
                {
                    var rect = GUILayoutUtility.GetLastRect();
                    var newValue = GUI.HorizontalSlider(new Rect(rect.x, rect.yMin + 7, rect.width, 15), value, min, max);
                    newValue = GUIFloatField.FloatField(title, newValue, min, max);
                    if (newValue != value)
                    {
                        value = newValue;
                        return true;
                    }
                }
                else
                {
                    GUILayout.Label(value.ToString("F3"));
                }
                return false;
            };
        }


        public static bool AddSliderInt(float totalWidth, string i18nLocale, int value, out int newVal, int min, int max, bool isEnabled = true)
        {
            var result = AddSliderInt(totalWidth, i18nLocale, ref value, min, max, isEnabled);
            newVal = value;
            return result;
        }
        public static bool AddSliderInt(float totalWidth, string i18nLocale, int value, Action<int> newValCall, int min, int max, bool isEnabled = true)
        {
            var result = AddSliderInt(totalWidth, i18nLocale, ref value, min, max, isEnabled);
            if (result)
            {
                newValCall(value);
            };
            return result;
        }
        public static bool AddSliderInt(float totalWidth, string title, ref int value, int min, int max, bool isEnabled = true)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title);
                GUILayout.FlexibleSpace();
                GUILayout.Space(totalWidth / 3);
                if (isEnabled)
                {
                    var rect = GUILayoutUtility.GetLastRect();
                    var newValue = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(rect.x, rect.yMin + 7, rect.width, 15), value, min, max));
                    newValue = GUIIntField.IntField(title, newValue, min, max, 40) ?? newValue;
                    if (newValue != value)
                    {
                        value = newValue;
                        return true;
                    }
                }
                else
                {
                    GUILayout.Label(value.ToString("F3"));
                }
                return false;
            };
        }
        public static bool AddFloatField(float totalWidth, string i18nLocale, float value, out float newVal, bool isEnabled = true, float min = float.MinValue, float max = float.MaxValue)
        {
            var res = AddFloatField(totalWidth, i18nLocale, ref value, isEnabled, min, max);
            newVal = value;
            return res;
        }

        public static bool AddFloatField(float totalWidth, string title, ref float value, bool isEnabled = true, float min = float.MinValue, float max = float.MaxValue)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title, GUILayout.Width(totalWidth - 75));
                GUILayout.FlexibleSpace();
                if (isEnabled)
                {
                    var newValue = GUIFloatField.FloatField(title, value, min, max);
                    if (newValue != value)
                    {
                        value = newValue;
                        return true;
                    }
                }
                else
                {
                    GUILayout.Label(value.ToString("F3"));
                }
                return false;
            };
        }
        public static bool AddIntField(float totalWidth, string title, ref int? value, bool isEnabled = true, int min = int.MinValue, int max = int.MaxValue)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title, GUILayout.Width(totalWidth - 75));
                GUILayout.FlexibleSpace();
                if (isEnabled)
                {
                    var newValue = GUIIntField.IntField(title, value, min, max);
                    if (newValue != value)
                    {
                        value = newValue;
                        return true;
                    }
                }
                else
                {
                    GUILayout.Label(value?.ToString("0") ?? v_null);
                }
                return false;
            };
        }
        public static bool AddIntField(float totalWidth, string title, int? value, Action<int?> onChanged, bool isEnabled = true, int min = int.MinValue, int max = int.MaxValue, string format = "0")
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title, GUILayout.Width(totalWidth - 75));
                GUILayout.FlexibleSpace();
                if (isEnabled)
                {
                    var newValue = GUIIntField.IntField(title, value, min, max, format: format);
                    if (newValue != value)
                    {
                        onChanged(newValue);
                        return true;
                    }
                }
                else
                {
                    GUILayout.Label(value?.ToString(format) ?? v_null);
                }
                return false;
            };
        }

        public static bool AddComboBox<T>(float totalWidth, string i18nLocale, T selectedVal, string[] options, T[] values, GUIRootWindowBase root, Action<T> onChange, bool isEditable = true, string name = null)
        {
            var selIdx = Array.IndexOf(values, selectedVal);
            var changed = AddComboBox(totalWidth, i18nLocale, ref selIdx, options, root, isEditable, name ?? i18nLocale);
            if (changed)
            {
                if (selIdx >= 0)
                {
                    onChange(values[selIdx]);
                }
                else
                {
                    onChange(default);
                }
            }
            return changed;
        }
        public static bool AddComboBox<T>(float totalWidth, string i18nLocale, ref T selectedVal, string[] options, T[] values, GUIRootWindowBase root, bool isEditable = true, string name = null)
        {
            var selIdx = Array.IndexOf(values, selectedVal);
            var changed = AddComboBox(totalWidth, i18nLocale, ref selIdx, options, root, isEditable, name ?? i18nLocale);
            if (changed)
            {
                if (selIdx >= 0)
                {
                    selectedVal = values[selIdx];
                }
                else
                {
                    selectedVal = default;
                }
            }
            return changed;
        }
        public static bool AddComboBox(float totalWidth, string i18nLocale, int selectedIndex, string[] options, out int result, GUIRootWindowBase root, bool isEditable = true, string name = null)
        {
            var changed = AddComboBox(totalWidth, i18nLocale, ref selectedIndex, options, root, isEditable, name ?? i18nLocale);
            result = selectedIndex;
            return changed;
        }



        public static bool AddComboBox(float totalWidth, string title, ref int selectedIndex, string[] options, GUIRootWindowBase root, bool isEditable = true, string name = null, string nullStr = v_null)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title, GUILayout.MaxWidth(totalWidth / 2));
                if (isEditable)
                {
                    var newVal = GUIComboBox.Box(selectedIndex, options, name ?? title, root, totalWidth / 2);
                    if (newVal != selectedIndex)
                    {
                        selectedIndex = newVal;
                        return true;
                    }
                }
                else
                {
                    GUILayout.Label(selectedIndex >= 0 ? selectedIndex >= options.Length ? v_invalid : options[selectedIndex] : nullStr);
                }
                return false;
            };
        }
        #endregion


    }
}