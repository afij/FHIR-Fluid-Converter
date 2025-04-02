using Fhir.Fluid.Converter.Parsers;
using Fhir.Fluid.Converter.Parsers.Options;
using System.Threading.Tasks;

namespace Fhir.Fluid.Converter
{
    public class FhirConverter
    {
        private readonly CCDParser _parser;
        private readonly CCDParserOptions _options;

        public FhirConverter(CCDParserOptions options)
        {
            _options = options;
            _parser = new CCDParser(options);
        }

        public async Task<string> ConvertCcdaToFhirAsync(string inputCCDA)
        {
            string convertedFhirData = await _parser.ConvertCcdaToFhirAsync(inputCCDA);
            return convertedFhirData;
        }
    }
}
