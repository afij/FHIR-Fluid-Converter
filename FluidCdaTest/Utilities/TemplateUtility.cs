using FluidCdaTest.Models;
using System;
using System.Text.Json;

namespace FluidCdaTest.Utilities
{
    public class TemplateUtility
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };

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
                var mapping = JsonSerializer.Deserialize<CodeMapping>(content, _jsonOptions);
                if (mapping?.Mapping == null)
                {
                    throw new Exception();
                }

                return mapping;
            }
            catch (JsonException ex)
            {
                throw ex;
            }
        }
    }
}
