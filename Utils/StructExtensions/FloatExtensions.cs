using ColossalFramework.Globalization;

namespace Kwytto.Utils
{
    public static class FloatExtensions
    {
        public static string ToGameCurrencyFormat(this float f) => f.ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
        public static string ToGameCurrencyFormatNoCents(this float f) => f.ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo);
    }
}
