using System.Collections.Generic;

namespace Fhir.Fluid.Converter.Models
{
    internal class CodeMapping
    {
        public CodeMapping(Dictionary<string, Dictionary<string, Dictionary<string, string>>> mapping)
        {
            Mapping = mapping;
        }

        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> Mapping { get; set; }
    }
}
