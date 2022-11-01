using Kwytto.Interfaces;
using Kwytto.Utils;
using System;
using System.Linq;
using static CardinalPoint;

namespace Kwytto.Localization
{
    public static class EnumI18nExtensionsKwytto
    {
        public static string ValueToI18nKwytto(this Enum variable)
        {
            switch (variable)
            {
                case Cardinal16 e:
                    switch (e)
                    {
                        case Cardinal16.N: return KStr.comm_Enum__Cardinal16_N;
                        case Cardinal16.NNE: return KStr.comm_Enum__Cardinal16_NNE;
                        case Cardinal16.NE: return KStr.comm_Enum__Cardinal16_NE;
                        case Cardinal16.ENE: return KStr.comm_Enum__Cardinal16_ENE;
                        case Cardinal16.E: return KStr.comm_Enum__Cardinal16_E;
                        case Cardinal16.ESE: return KStr.comm_Enum__Cardinal16_ESE;
                        case Cardinal16.SE: return KStr.comm_Enum__Cardinal16_SE;
                        case Cardinal16.SSE: return KStr.comm_Enum__Cardinal16_SSE;
                        case Cardinal16.S: return KStr.comm_Enum__Cardinal16_S;
                        case Cardinal16.SSW: return KStr.comm_Enum__Cardinal16_SSW;
                        case Cardinal16.SW: return KStr.comm_Enum__Cardinal16_SW;
                        case Cardinal16.WSW: return KStr.comm_Enum__Cardinal16_WSW;
                        case Cardinal16.W: return KStr.comm_Enum__Cardinal16_W;
                        case Cardinal16.WNW: return KStr.comm_Enum__Cardinal16_WNW;
                        case Cardinal16.NW: return KStr.comm_Enum__Cardinal16_NW;
                        case Cardinal16.NNW: return KStr.comm_Enum__Cardinal16_NNW;
                    }
                    break;
            }
            LogUtils.DoLog($"<data name=\"{BasicIUserMod.Instance.Acronym.ToLower()}_Enum__{variable?.GetType().Name}_{variable}\" xml:space=\"preserve\">    <value>{variable?.GetType()}|{variable}</value>  </data>");
            return $"{variable?.GetType()}|{variable}";
        }

        public static string[] GetAllValuesI18n<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<Enum>().Select(x => x.ValueToI18nKwytto()).ToArray();
    }
}
