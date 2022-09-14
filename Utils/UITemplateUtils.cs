using ColossalFramework.UI;
using System.Collections.Generic;

namespace Kwytto.Utils
{
    internal class UITemplateUtils
    {
        public static Dictionary<string, UIComponent> GetTemplateDict() => (Dictionary<string, UIComponent>) typeof(UITemplateManager).GetField("m_Templates", ReflectionUtils.allFlags).GetValue(UITemplateManager.instance);
    }

}

