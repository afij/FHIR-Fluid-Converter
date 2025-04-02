using Fhir.Fluid.Converter.Models;
using Fluid;
using Fluid.Values;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Fluid.Converter.Filters
{
    internal static class GeneralFilters
    {
        public const string CODE_MAPPING_VALUE_NAME = "CodeMapping";
        public static void RegisterGeneralFilters(this FilterCollection filters)
        {
            filters.AddFilter("get_property", GetProperty);
            filters.AddFilter("generate_uuid", GenerateUUID);
        }

        /// <summary>
        /// Returns a specific property of a coding with mapping file CodeSystem.json
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static ValueTask<FluidValue> GetProperty(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (arguments == null || arguments.Count < 1)
            {
                throw new ArgumentException("Invalid get_property usage: At least one argument is required.");
            }

            string valueSetType;
            string property = "code";

            valueSetType = arguments.At(0).ToStringValue();
            var tempProperty = arguments.At(1).ToStringValue();
            if (!string.IsNullOrEmpty(tempProperty))
            {
                property = tempProperty;
            }

            context.AmbientValues.TryGetValue(CODE_MAPPING_VALUE_NAME, out var codeMappingObj);
            CodeMapping codeMapping = (CodeMapping)codeMappingObj;
            var map = codeMapping?.Mapping?.GetValueOrDefault(valueSetType, null);
            var codeMappingValue = map?.GetValueOrDefault(input.ToStringValue(), null) ?? map?.GetValueOrDefault("__default__", null);
            var returnValue = codeMappingValue?.GetValueOrDefault(property, null)
                ?? (property.Equals("code") || property.Equals("display") ? input.ToStringValue() : null);
            return new StringValue(returnValue);
        }

        /// <summary>
        /// Generates an ID based on an input string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ValueTask<FluidValue> GenerateUUID(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.ToStringValue() == "null" || string.IsNullOrWhiteSpace(input.ToStringValue()))
            {
                return NilValue.Empty;
            }

            var bytes = Encoding.UTF8.GetBytes(input.ToStringValue());
            var hash = SHA256.HashData(bytes);
            var guid = new byte[16];
            Array.Copy(hash, 0, guid, 0, 16);
            var computedUUID = new Guid(guid).ToString();
            return new StringValue(computedUUID);
        }
    }

}
