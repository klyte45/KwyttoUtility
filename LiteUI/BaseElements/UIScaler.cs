﻿using ColossalFramework.UI;
using Kwytto.Interfaces;
using UnityEngine;

namespace Kwytto.LiteUI
{
    public static class UIScaler
    {
        private static bool TryGetScreenResolution(out Vector2 resolution)
        {
            UIView uIView = UIView.GetAView();
            if (uIView)
            {
                resolution = uIView.GetScreenResolution();
                return true;
            }
            else
            {
                resolution = default;
                return false;
            }
        }

        public static float BaseResolutionX
        {
            get
            {
                if (TryGetScreenResolution(out Vector2 resolution))
                {
                    return resolution.x;  // 1920f if aspect ratio is 16:9;
                }
                else
                {
                    return 1080f * AspectRatio;
                }
            }
        }

        public static float BaseResolutionY
        {
            get
            {
                if (TryGetScreenResolution(out Vector2 resolution))
                {
                    return resolution.y; // always 1080f. But we keep this code for the sake of future proofing
                }
                else
                {
                    return 1080f;
                }
            }
        }

        public static float AspectRatio => Screen.width / (float)Screen.height;

        public static float MaxWidth
        {
            get
            {
                float ret =
                    Screen.width;
                return ret / BasicIUserMod.Instance.UIScale;
            }
        }

        public static float MaxHeight
        {
            get
            {
                float ret =
                    Screen.height;
                return ret / BasicIUserMod.Instance.UIScale;
            }
        }

        public static float UIScale
        {
            get
            {
                var w = Screen.width / MaxWidth;
                var h = Screen.height / MaxHeight;
                return Mathf.Min(w, h);
            }
        }

        public static Matrix4x4 ScaleMatrix => Matrix4x4.Scale(Vector3.one * UIScaler.UIScale);

        public static Vector2 MousePosition
        {
            get
            {
                var mouse = Input.mousePosition;
                mouse.y = Screen.height - mouse.y;
                return mouse / UIScaler.UIScale;
            }
        }
    }
}
