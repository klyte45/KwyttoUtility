using Kwytto.Interfaces;

namespace Kwytto.LiteUI
{
    public abstract class GUIOpacityChanging : GUIRootWindowBase
    {
        public virtual void Awake()
        {
            BgOpacity = BasicIUserMod.Instance.UIOpacity;
        }
    }
}
