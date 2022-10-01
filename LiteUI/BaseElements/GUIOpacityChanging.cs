using Kwytto.Interfaces;
using System;

namespace Kwytto.LiteUI
{
    public abstract class GUIOpacityChanging : GUIRootWindowBase
    {
        public virtual void Awake()
        {
            BgOpacity = BasicIUserMod.Instance.UIOpacity;
        }
        public event Action<float> EventOpacityChanged;
        protected override void OnOpacityChanged(float newVal)
        {
            base.OnOpacityChanged(newVal);
            EventOpacityChanged?.Invoke(newVal);
        }
    }
}
