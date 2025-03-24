using Fluid.Values;
using Fluid;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using FluidCdaTest.Models;

namespace FluidCdaTest.Filters
{
    public static class DateFilters
    {
        public static void RegisterDateFilters(this FilterCollection filters)
        {
            filters.AddFilter("format_as_date_time", FormatAsDateTime);
            filters.AddFilter("add_hyphens_date", AddHyphensDate);
            filters.AddFilter("now", Now);
        }

        public static ValueTask<FluidValue> FormatAsDateTime(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var timeZoneHandling = "local";
            var argument1 = arguments.At(0);
            if (argument1 != null && !(argument1 is NilValue))
            {
                timeZoneHandling = arguments.At(0).ToStringValue();
            }
            if (string.IsNullOrEmpty(input.ToStringValue()))
            {
                return input;
            }

            if (!Enum.TryParse(timeZoneHandling, true, out TimeZoneHandlingMethod outputTimeZoneHandling))
            {
                throw new Exception("Invalid timezone");
            }

            PartialDateTime dateTimeObject;
            try
            {
                dateTimeObject = new PartialDateTime(input.ToStringValue(), DateTimeType.Hl7v2);
            }
            catch (Exception)
            {
                throw new Exception("Invalid datetime format: " + input.ToStringValue());
            }

            return FluidValue.Create(dateTimeObject.ToFhirString(outputTimeZoneHandling), context.Options);
        }

        public static ValueTask<FluidValue> AddHyphensDate(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var timeZoneHandling = "preserve";
            var argument1 = arguments.At(0);
            if (argument1 != null && !(argument1 is NilValue))
            {
                timeZoneHandling = arguments.At(0).ToStringValue();
            }
            if (string.IsNullOrEmpty(input.ToStringValue()))
            {
                return input;
            }

            if (!Enum.TryParse(timeZoneHandling, true, out TimeZoneHandlingMethod outputTimeZoneHandling))
            {
                throw new Exception("Invalid timezone handling");
            }

            PartialDateTime dateTimeObject;
            try
            {
                dateTimeObject = new PartialDateTime(input.ToStringValue(), DateTimeType.Hl7v2);
            }
            catch (Exception)
            {
                throw new Exception("Invalid timezone format: " + input.ToStringValue());
            }

            dateTimeObject.ConvertToDate();
            return FluidValue.Create(dateTimeObject.ToFhirString(outputTimeZoneHandling), context.Options);
        }

        public static ValueTask<FluidValue> Now(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            string format = "yyyy-MM-ddTHH:mm:ss.FFFZ";
            var argument1 = arguments.At(0);
            if (argument1 != null && !(argument1 is NilValue))
            {
                format = arguments.At(0).ToStringValue();
            }
            return FluidValue.Create(DateTime.UtcNow.ToString(format), context.Options);
        }
    }
}
