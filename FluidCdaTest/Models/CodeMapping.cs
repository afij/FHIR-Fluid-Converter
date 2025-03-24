using System.Collections.Generic;

namespace FluidCdaTest.Models
{
    public class CodeMapping
    {
        public CodeMapping(Dictionary<string, Dictionary<string, Dictionary<string, string>>> mapping)
        {
            Mapping = mapping;
        }

        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> Mapping { get; set; }
    }
}
