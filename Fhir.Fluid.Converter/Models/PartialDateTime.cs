using Fhir.Fluid.Converter.CustomRegex;
using System;
using System.Text.RegularExpressions;

namespace Fhir.Fluid.Converter.Models
{
    internal class PartialDateTime
    {
        public PartialDateTime(string input, DateTimeType type = DateTimeType.Fhir)
        {
            Regex regex = type switch
            {
                DateTimeType.Ccda => CCDRegex.DateTimeRegex(),
                DateTimeType.Hl7v2 => CCDRegex.DateTimeRegex(),
                DateTimeType.Fhir => CCDRegex.FhirDateTimeRegex(),
                _ => throw new ArgumentException("Invalid datetime format: " + type.ToString()),
            };

            var matches = regex.Matches(input);
            if (matches.Count != 1 || !matches[0].Groups["year"].Success)
            {
                throw new ArgumentException("Invalid datetime format: " + type.ToString());
            }

            var groups = matches[0].Groups;

            int year = int.Parse(groups["year"].Value);
            int month = groups["month"].Success ? int.Parse(groups["month"].Value) : 1;
            int day = groups["day"].Success ? int.Parse(groups["day"].Value) : 1;
            int hour = groups["hour"].Success ? int.Parse(groups["hour"].Value) : 0;
            int minute = groups["minute"].Success ? int.Parse(groups["minute"].Value) : 0;
            int second = groups["second"].Success ? int.Parse(groups["second"].Value) : 0;
            int millisecond = groups["millisecond"].Success ? int.Parse(groups["millisecond"].Value) : 0;

            var timeSpan = TimeSpan.FromHours(TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Hours);
            if (groups["timeZone"].Success)
            {
                if (groups["timeZone"].Value == "Z")
                {
                    timeSpan = TimeSpan.Zero;
                }
                else
                {
                    var sign = groups["sign"].Success && groups["sign"].Value == "-" ? -1 : 1;
                    var timeZoneHour = int.Parse(groups["timeZoneHour"].Value) * sign;
                    var timeZoneMinute = int.Parse(groups["timeZoneMinute"].Value) * sign;
                    timeSpan = new TimeSpan(timeZoneHour, timeZoneMinute, 0);
                }
            }

            DateTimeValue = new DateTimeOffset(year, month, day, hour, minute, second, millisecond, timeSpan);
            Precision =
                    groups["millisecond"].Success ? DateTimePrecision.Milliseconds :
                    groups["second"].Success ? DateTimePrecision.Second :
                    groups["minute"].Success ? DateTimePrecision.Minute :
                    groups["hour"].Success ? DateTimePrecision.Hour :
                    groups["day"].Success ? DateTimePrecision.Day :
                    groups["month"].Success ? DateTimePrecision.Month :
                    DateTimePrecision.Year;
            HasTimeZone = groups["timeZone"].Success;
        }

        public DateTimeOffset DateTimeValue { get; private set; }

        public bool HasTimeZone { get; private set; }

        public DateTimePrecision Precision { get; private set; }

        public PartialDateTime ConvertToDate()
        {
            Precision = Precision < DateTimePrecision.Day ? Precision : DateTimePrecision.Day;
            return this;
        }

        public PartialDateTime AddSeconds(double seconds)
        {
            DateTimeValue = DateTimeValue.AddSeconds(seconds);
            if (Precision != DateTimePrecision.Milliseconds)
            {
                Precision = DateTimeValue.Millisecond == 0 ? DateTimePrecision.Second : DateTimePrecision.Milliseconds;
            }

            return this;
        }

        public string ToFhirString(TimeZoneHandlingMethod timeZoneHandling = TimeZoneHandlingMethod.Preserve)
        {
            var resultDateTime = timeZoneHandling switch
            {
                TimeZoneHandlingMethod.Preserve => DateTimeValue,
                TimeZoneHandlingMethod.Utc => DateTimeValue.ToUniversalTime(),
                TimeZoneHandlingMethod.Local => DateTimeValue.ToLocalTime(),
                _ => throw new ArgumentException("Invalid TimezoneHandling"),
            };

            if (Precision <= DateTimePrecision.Day)
            {
                return Precision switch
                {
                    DateTimePrecision.Day => resultDateTime.ToString("yyyy-MM-dd"),
                    DateTimePrecision.Month => resultDateTime.ToString("yyyy-MM"),
                    DateTimePrecision.Year => resultDateTime.ToString("yyyy"),
                    _ => throw new ArgumentException("Invalid dateTimeObject with empty Year field.")
                };
            }

            var timeZoneSuffix = string.Empty;
            if (HasTimeZone)
            {
                // Using "Z" to represent zero timezone.
                timeZoneSuffix = resultDateTime.Offset == TimeSpan.Zero ? "Z" : "%K";
            }

            if (timeZoneHandling == TimeZoneHandlingMethod.Utc)
            {
                timeZoneSuffix = "Z";
            }

            var dateTimeFormat = Precision < DateTimePrecision.Milliseconds ? "yyyy-MM-ddTHH:mm:ss" + timeZoneSuffix : "yyyy-MM-ddTHH:mm:ss.fff" + timeZoneSuffix;
            return resultDateTime.ToString(dateTimeFormat);
        }
    }
}
