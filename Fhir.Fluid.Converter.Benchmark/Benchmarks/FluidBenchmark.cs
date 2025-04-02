using BenchmarkDotNet.Attributes;
using Fhir.Fluid.Converter.Parsers.Options;
using System.Threading.Tasks;

namespace Fhir.Fluid.Converter.Benchmark.Benchmarks
{
    /// <summary>
    /// Recreates parser and template options each execution
    /// </summary>
    [MemoryDiagnoser]
    public class FluidBenchmark : BaseBenchmark
    {
        private FhirConverter _converter;

        public override void SetupBenchmark()
        {
            // No setup required
        }

        public override async Task<string> ParseAndRenderAsync()
        {
            _converter = new FhirConverter(
                new CCDParserOptions()
                {
                    TemplateDirectoryPath = BenchmarkConstants.TemplatesPath,
                    RootTemplate = "CCD.liquid",
                    UseCachedFileProvider = true
                }
            );
            return await _converter.ConvertCcdaToFhirAsync(TestContent);
        }
    }
}
