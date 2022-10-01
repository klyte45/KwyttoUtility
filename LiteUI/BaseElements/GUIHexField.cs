using System;
using UnityEngine;

namespace Kwytto.LiteUI
{
    public static class GUIHexField
    {
        private static string lastFocusedFieldId;
        private static string lastValue;

        private static bool EnterPressed()
        {
            var keycode = Event.current.keyCode;
            return keycode == KeyCode.KeypadEnter || keycode == KeyCode.Return;
        }

        public static string HexField(string id, string value, int minChars = int.MaxValue, int maxChars = int.MaxValue, float? fieldWidth = 50)
        {
            var focusedFieldId = GUI.GetNameOfFocusedControl();
            GUILayoutOption widthOption = null;
            if (fieldWidth is float w)
            {
                widthOption = GUILayout.Width(w);
            }

            if (lastValue != null)
            {
                if (id == lastFocusedFieldId && lastFocusedFieldId != focusedFieldId)
                {
                    int targetVal = -1;
                    try
                    {
                        targetVal = (int)Convert.ToUInt64(lastValue.Substring(0, maxChars), 16);
                    }
                    catch { }
                    if (targetVal >= 0)
                    {
                        value = targetVal.ToString($"X{minChars}");
                    }
                    lastValue = null;
                }
                else if (EnterPressed() && id == lastFocusedFieldId)
                {
                    int targetVal = -1;
                    try
                    {
                        targetVal = (int)Convert.ToUInt64(lastValue.Substring(0, maxChars), 16);
                    }
                    catch { }
                    if (targetVal >= 0)
                    {
                        value = targetVal.ToString($"X{minChars}");
                    }
                    lastValue = null;
                    GUI.FocusControl(null);
                }
                else if (lastFocusedFieldId != focusedFieldId || string.IsNullOrEmpty(focusedFieldId))
                {
                    // discard last value if user did not use enter to submit results
                    lastValue = null;
                }
            }

            if (id == focusedFieldId)
            {
                lastValue = lastValue ?? value;
                GUI.SetNextControlName(id);
                lastValue = GUILayout.TextField(lastValue ?? "", widthOption);
                lastFocusedFieldId = focusedFieldId;
            }
            else
            {
                GUI.SetNextControlName(id);
                GUILayout.TextField(value ?? "", widthOption);
            }


            return value;
        }
    }
}