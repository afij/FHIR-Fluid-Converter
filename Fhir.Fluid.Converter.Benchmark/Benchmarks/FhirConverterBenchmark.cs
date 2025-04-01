using BenchmarkDotNet.Attributes;
using Microsoft.Health.Fhir.Liquid.Converter.Tool;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;
using System.Threading.Tasks;

namespace Fhir.Fluid.Converter.Benchmark.Benchmarks
{
    [MemoryDiagnoser]
    public class FhirConverterBenchmark : BaseBenchmark
    {
        public override void SetupBenchmark()
        {
            // No setup required
        }

        public override void Parse()
        {
            // Do nothing
        }

        public override Task<string> RenderAsync()
        {
            ConverterOptions _options = new()
            {
                InputDataContent = TestContent,
                TemplateDirectory = BenchmarkConstants.TemplatesPath,
                RootTemplate = "CCD",
            };

            var convertedFhirString = ConverterLogicHandler.ConvertToString(_options);
            return Task.FromResult(convertedFhirString);
        }
    }
}
