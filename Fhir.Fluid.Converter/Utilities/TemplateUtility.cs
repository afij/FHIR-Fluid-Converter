using Fhir.Fluid.Converter.Models;
using Newtonsoft.Json;
using System;

namespace Fhir.Fluid.Converter.Utilities
{
    public class TemplateUtility
    {
        /// <summary>
        /// Deserialize string content into CodeMapping object
        /// </summary>
        /// <param name="content">String containing serialized CodeMappingObject</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static CodeMapping ParseCodeMapping(string content)
        {
            if (content == null)
            {
                return null;
            }

            try
            {
                var mapping = JsonConvert.DeserializeObject<CodeMapping>(content);
                if (mapping?.Mapping == null)
                {
                    throw new Exception();
                }

                return mapping;
            }
            catch (JsonException)
            {
                //throw new TemplateLoadException(FhirConverterErrorCode.InvalidCodeMapping, Resources.InvalidCodeMapping, ex);
                throw;
            }
        }
    }
}
