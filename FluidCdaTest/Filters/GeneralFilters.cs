using Fluid.Values;
using Fluid;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using FluidCdaTest.Models;
using System.Data;
using System.Security.Cryptography;

namespace FluidCdaTest.Filters
{
    public static class GeneralFilters
    {
        public const string CODE_MAPPING_VALUE_NAME = "CodeMapping"; 
        public static void RegisterGeneralFilters(this FilterCollection filters)
        {
            filters.AddFilter("get_property", GetProperty);
            filters.AddFilter("generate_uuid", GenerateUUID);
        }

        public static ValueTask<FluidValue> GetProperty(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            string valueSetType = null;
            string property = "code";
            if (arguments != null && arguments.Count > 0)
            {
                valueSetType = arguments.At(0).ToStringValue();
                var tempProperty = arguments.At(1).ToStringValue();
                if (!string.IsNullOrEmpty(tempProperty))
                {
                    property = tempProperty;
                }
            }
            else
            {
                throw new Exception("Invalid get_property usage, incorrect amount of arguments");
            }
            context.AmbientValues.TryGetValue(CODE_MAPPING_VALUE_NAME, out var codeMappingObj);
            CodeMapping codeMapping = (CodeMapping)codeMappingObj;
            var map = codeMapping?.Mapping?.GetValueOrDefault(valueSetType, null);
            var codeMappingValue = map?.GetValueOrDefault(input.ToStringValue(), null) ?? map?.GetValueOrDefault("__default__", null);
            var returnValue = codeMappingValue?.GetValueOrDefault(property, null)
                ?? ((property.Equals("code") || property.Equals("display")) ? input.ToStringValue() : null);
            return new StringValue(returnValue);
        }

        public static ValueTask<FluidValue> GenerateUUID(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.ToStringValue() == "null" || string.IsNullOrWhiteSpace(input.ToStringValue()))
            {
                return NilValue.Empty;
            }

            var bytes = Encoding.UTF8.GetBytes(input.ToStringValue());
            var algorithm = SHA256.Create();
            var hash = algorithm.ComputeHash(bytes);
            var guid = new byte[16];
            Array.Copy(hash, 0, guid, 0, 16);
            var computedUUID = new Guid(guid).ToString();
            return new StringValue(computedUUID);
        }
    }

}
