using System.Text.RegularExpressions;

namespace FluidCdaTest.CustomRegex
{
    internal static partial class CCDRegex
    {
        [GeneratedRegex(@"\r\n?|\n")]
        internal static partial Regex InvalidLineBreakRegex();
    }
}