using BenchmarkDotNet.Attributes;
using Microsoft.Health.Fhir.Liquid.Converter.Tool;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    [MemoryDiagnoser]
    public class FhirConverterBenchmark : BaseBenchmark
    {
        private readonly ConverterOptions _options = new ConverterOptions()
        {
            InputDataContent = TestContent,
            TemplateDirectory = @"C:\work\FluidCdaTest\FluidCdaTest.Benchmark\DevBranchTemplates\Ccda",
            RootTemplate = "CCD",
        };

        public FhirConverterBenchmark()
        {
        }

        [Benchmark]
        public override string ParseAndRender()
        {
            var convertedFhirString = ConverterLogicHandler.ConvertToString(_options);
            return convertedFhirString;
        }
    }
}
