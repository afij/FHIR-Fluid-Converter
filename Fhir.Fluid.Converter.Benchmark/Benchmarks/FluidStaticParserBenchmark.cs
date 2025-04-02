using BenchmarkDotNet.Attributes;
using Fhir.Fluid.Converter.Parsers.Options;
using System.Threading.Tasks;

namespace Fhir.Fluid.Converter.Benchmark.Benchmarks
{
    /// <summary>
    /// Uses a static parser instance during repeated execution
    /// </summary>
    [MemoryDiagnoser]
    public class FluidStaticParserBenchmark : BaseBenchmark
    {
        private static FhirConverter _converter;

        public override void SetupBenchmark()
        {
            _converter = new FhirConverter(
                new CCDParserOptions()
                {
                    TemplateDirectoryPath = BenchmarkConstants.TemplatesPath,
                    RootTemplate = "CCD.liquid",
                    UseCachedFileProvider = false
                }
            );
        }

        public override async Task<string> ParseAndRenderAsync()
        {
            return await _converter.ConvertCcdaToFhirAsync(TestContent);
        }
    }
}
