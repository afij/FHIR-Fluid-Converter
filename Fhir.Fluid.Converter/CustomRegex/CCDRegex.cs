using System.Text.RegularExpressions;

namespace Fhir.Fluid.Converter.CustomRegex
{
    internal static partial class CCDRegex
    {
        [GeneratedRegex(@"\r\n?|\n")]
        internal static partial Regex InvalidLineBreakRegex();

        [GeneratedRegex("[^A-Za-z0-9]")]
        internal static partial Regex NormalizeSectionNameRegex();

        [GeneratedRegex(@"^((?<year>\d{4})((?<month>\d{2})((?<day>\d{2})(?<time>((?<hour>\d{2})((?<minute>\d{2})((?<second>\d{2})(\.(?<millisecond>\d+))?)?)?))?)?)?(?<timeZone>(?<sign>-|\+)(?<timeZoneHour>\d{2})(?<timeZoneMinute>\d{2}))?)$")]
        internal static partial Regex DateTimeRegex();

        [GeneratedRegex(@"^(?<year>([0-9]([0-9]([0-9][1-9]|[1-9]0)|[1-9]00)|[1-9]000))((-(?<month>0[1-9]|1[0-2]))((-(?<day>0[1-9]|[1-2][0-9]|3[0-1]))(?<time>T(?<hour>[01][0-9]|2[0-3]):(?<minute>[0-5][0-9]):((?<second>[0-5][0-9]|60)(\.(?<millisecond>[0-9]+))?)(?<timeZone>Z|(?<sign>\+|-)((?<timeZoneHour>0[0-9]|1[0-3]):(?<timeZoneMinute>[0-5][0-9])|(?<timeZoneHour>14):(?<timeZoneMinute>00))))?)?)?$")]
        internal static partial Regex FhirDateTimeRegex();
    }
}