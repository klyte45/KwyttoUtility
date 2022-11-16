using ColossalFramework.UI;
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

        public static float MaxWidth => Screen.width / UIScale;

        public static float MaxHeight => Screen.height / UIScale;

        public static float UIScale => BasicIUserMod.Instance.UIScale * Screen.height / 1080f;

        public static Matrix4x4 ScaleMatrix => Matrix4x4.Scale(Vector3.one * UIScale);

        public static Vector2 MousePosition
        {
            get
            {
                var mouse = Input.mousePosition;
                mouse.y = Screen.height - mouse.y;
                return mouse / UIScale;
            }
        }
    }
}
