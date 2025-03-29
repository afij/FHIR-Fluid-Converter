using Fluid;
using Fluid.Values;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fhir.Fluid.Converter.Filters
{
    public static class StringFilters
    {
        public static void RegisterStringFilters(this FilterCollection filters)
        {
            filters.AddFilter("to_json_string", ToJsonString);
            filters.AddFilter("matches", Matches);
            filters.AddFilter("gzip", Gzip);
            //filters.AddFilter("replace", Replace);
        }

        /// <summary>
        /// Convert input into a JSON string value
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ValueTask<FluidValue> ToJsonString(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input is ArrayValue)
            {
                string outputString = "";
                foreach (var item in input.Enumerate(context))
                {
                    outputString += input is NilValue ? null : ToJsonString(input);
                }
            }
            string json = input is NilValue ? null : ToJsonString(input);
            return new StringValue(json);
        }

        public static string ToJsonString(object data)
        {
            return data == null ? null : JsonConvert.SerializeObject(data, Formatting.None);
        }

        /// <summary>
        /// Returns an array containing matches with a regular expression
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ValueTask<FluidValue> Matches(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (string.IsNullOrEmpty(input.ToStringValue()))
            {
                return FluidValue.Create(new List<string>(), context.Options);
            }

            var regex = new Regex(arguments.At(0).ToStringValue());
            var matches = regex.Matches(input.ToStringValue()).SelectMany(match => match.Captures).Select(capture => capture.Value).ToList();
            return FluidValue.Create(matches, context.Options);
        }

        /// <summary>
        /// Override's fluids replace filter - upgraded to perform regex replace
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ValueTask<FluidValue> Replace(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var output = Regex.Replace(input.ToStringValue(), arguments.At(0).ToStringValue(), arguments.At(1).ToStringValue());
            return new StringValue(output);
        }

        /// <summary>
        /// GZIP compresses input string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ValueTask<FluidValue> Gzip(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(input.ToStringValue()));
            using var outputStream = new MemoryStream();
            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                inputStream.CopyTo(gzipStream);
            }

            return FluidValue.Create(Convert.ToBase64String(outputStream.ToArray()), context.Options);
        }
    }
}
