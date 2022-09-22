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
        public static readonly Texture2D darkGreenTexture;
        public static readonly Texture2D greenTexture;
        public static readonly Texture2D darkRedTexture;
        public static readonly Texture2D redTexture;
        public static readonly Texture2D almostWhiteTexture;
        public static readonly Texture2D whiteTexture;

        static GUIKwyttoCommons()
        {
            darkGreenTexture = CreateTextureOfColor(Color.Lerp(Color.green, Color.gray, 0.5f));
            greenTexture = CreateTextureOfColor(Color.green);
            darkRedTexture = CreateTextureOfColor(Color.Lerp(Color.red, Color.gray, 0.5f));
            redTexture = CreateTextureOfColor(Color.red);
            almostWhiteTexture = CreateTextureOfColor(Color.Lerp(Color.white, Color.gray, 0.28f));
            whiteTexture = CreateTextureOfColor(Color.Lerp(Color.white, Color.gray, 0.12f));
        }

        private static Texture2D CreateTextureOfColor(Color src)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, src);
            texture.Apply();
            return texture;
        }
        public static Texture GetByNameFromDefaultAtlas(string name) => UIView.GetAView().defaultAtlas.sprites.Where(x => x.name == name).FirstOrDefault().texture;

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

        public static bool TextWithLabel(float totalWidth, string label, string value, Action<string> action, bool enabled = true)
        {
            if (!enabled)
            {
                TextWithLabelDisabled(totalWidth, label, value);
                return false;
            }
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(totalWidth / 3));
                var newText = GUILayout.TextField(value ?? "", GUILayout.Height(20));
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
                GUILayout.Label(value ?? v_null);
            };
        }
        public static bool AddColorPicker(string title, GUIColorPicker picker, ref Color value, bool enabled = true)
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

        public static bool CreateItemVerticalList(Rect sideListArea, ref Vector2 scrollPosition, int currentSelection, string[] sideList, string addButtonText, GUIStyle addButtonStyle, out int newSelection)
        {
            var result = false;
            using (new GUILayout.AreaScope(sideListArea))
            {
                using (var scroll = new GUILayout.ScrollViewScope(scrollPosition))
                {
                    newSelection = currentSelection;
                    var newListSel = GUILayout.SelectionGrid(currentSelection, sideList, 1, new GUIStyle(GUI.skin.button) { wordWrap = true });
                    if (newListSel >= 0 && newListSel < sideList.Length)
                    {
                        newSelection = newListSel;
                    }

                    if (addButtonText != null && GUILayout.Button(addButtonText, addButtonStyle, GUILayout.ExpandWidth(true)))
                    {
                        result = true;
                        newSelection = sideList.Length;
                    }
                    scrollPosition = scroll.scrollPosition;
                }
            }
            return result;
        }

        public static void SquareTextureButton(Texture2D icon, string tooltip, Action onClick, bool condition = true)
        {
            if (condition && GUILayout.Button(new GUIContent(icon, tooltip), GUILayout.Width(30), GUILayout.Height(30)))
            {
                onClick();
            }
        }
        public static bool AddToggle(string title, ref bool currentVal, bool editable = true, bool condition = true)
        {
            bool newVal;
            if (condition && currentVal != (newVal = GUILayout.Toggle(currentVal, title)) && editable)
            {
                currentVal = newVal;
                return true;
            }
            return false;
        }
        public static bool AddToggle(string title, bool currentVal, out bool newVal, bool editable = true, bool condition = true)
        {
            if (condition && currentVal != (newVal = GUILayout.Toggle(currentVal, title)) && editable)
            {
                return true;
            }
            newVal = currentVal;
            return false;
        }

        public static bool AddSlider(float totalWidth, string i18nLocale, float value, out float newVal, float min, float max, bool isEnabled = true)
        {
            var result = AddSlider(totalWidth, i18nLocale, ref value, min, max, isEnabled);
            newVal = value;
            return result;
        }
        public static bool AddSlider(float totalWidth, string title, ref float value, float min, float max, bool isEnabled = true)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title, GUILayout.Width(totalWidth / 2));
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
                GUILayout.Label(title, GUILayout.Width(totalWidth / 2));
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
        public static bool AddIntField(float totalWidth, string title, ref int value, bool isEnabled = true, int min = int.MinValue, int max = int.MaxValue)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title, GUILayout.Width(totalWidth / 2));
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
                    GUILayout.Label(value.ToString("0"));
                }
                return false;
            };
        }

        public static bool AddComboBox<T>(float totalWidth, string i18nLocale, T selectedVal, string[] options, T[] values, GUIRootWindowBase root, Action<T> onChange, bool isEditable = true)
        {
            var selIdx = Array.IndexOf(values, selectedVal);
            var changed = AddComboBox(totalWidth, i18nLocale, ref selIdx, options, root, isEditable);
            if (selIdx >= 0)
            {
                onChange(values[selIdx]);
            }
            return changed;
        }
        public static bool AddComboBox<T>(float totalWidth, string i18nLocale, ref T selectedVal, string[] options, T[] values, GUIRootWindowBase root, bool isEditable = true)
        {
            var selIdx = Array.IndexOf(values, selectedVal);
            var changed = AddComboBox(totalWidth, i18nLocale, ref selIdx, options, root, isEditable);
            if (selIdx >= 0)
            {
                selectedVal = values[selIdx];
            }
            return changed;
        }
        public static bool AddComboBox(float totalWidth, string i18nLocale, int selectedIndex, string[] options, out int result, GUIRootWindowBase root, bool isEditable = true)
        {
            var changed = AddComboBox(totalWidth, i18nLocale, ref selectedIndex, options, root, isEditable);
            result = selectedIndex;
            return changed;
        }

        public static bool AddComboBox(float totalWidth, string title, ref int selectedIndex, string[] options, GUIRootWindowBase root, bool isEditable = true)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title, GUILayout.Width(totalWidth / 2));
                if (isEditable)
                {
                    var newVal = GUIComboBox.Box(selectedIndex, options, title, root);
                    if (newVal != selectedIndex)
                    {
                        selectedIndex = newVal;
                        return true;
                    }
                }
                else
                {
                    GUILayout.Label(selectedIndex >= 0 ? selectedIndex >= options.Length ? v_invalid : options[selectedIndex] : v_null);
                }
                return false;
            };
        }
        #endregion

        [Obsolete("Use Scope", true)]
        public static void DoInArea(Rect size, Action<Rect> action)
        {
            GUILayout.BeginArea(size);
            try
            {
                action(size);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                GUILayout.EndArea();
            }
        }
        [Obsolete("Use Scope", true)]
        public static void DoInScroll(ref Vector2 scrollPos, Action action)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            try
            {
                action();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                GUILayout.EndScrollView();
            }
        }
        [Obsolete("Use Scope", true)]
        public static void DoInScroll(ref Vector2 scrollPos, bool alwaysShowHorizontal, bool alwaysShowVertical, Action action, params GUILayoutOption[] options)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, alwaysShowHorizontal, alwaysShowVertical, options);
            try
            {
                action();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                GUILayout.EndScrollView();
            }
        }
        [Obsolete("Use Scope", true)]
        public static void DoInHorizontal(Action action, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(options);
            try
            {
                action();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }
        [Obsolete("Use Scope", true)]
        public static void DoInVertical(Action action, params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(options);
            try
            {
                action();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                GUILayout.EndVertical();
            }
        }
    }
}