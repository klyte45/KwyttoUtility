using ColossalFramework;
using System.IO;
using System.Linq;

namespace Kwytto.Utils
{
    public static class StringExtensions
    {

        static readonly char[] invalidFileNameChars = Path.GetInvalidFileNameChars();

        public static string Right(this string original, int numberCharacters) => original.Substring(original.Length - numberCharacters);
        public static string TrimToNull(this string original) => original.IsNullOrWhiteSpace() ? null : original.Trim();
        public static string AsPathSafe(this string fileName) => new string(fileName.Select(ch => invalidFileNameChars.Contains(ch) ? '_' : ch).ToArray());
    }
}
