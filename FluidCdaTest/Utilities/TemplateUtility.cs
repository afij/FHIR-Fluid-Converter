using FluidCdaTest.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;

namespace FluidCdaTest.Utilities
{
    public class TemplateUtility
    {
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
            catch (JsonException ex)
            {
                //throw new TemplateLoadException(FhirConverterErrorCode.InvalidCodeMapping, Resources.InvalidCodeMapping, ex);
                throw ex;
            }
        }
    }
}
