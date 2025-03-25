using BenchmarkDotNet.Attributes;
using Microsoft.Health.Fhir.Liquid.Converter.Tool;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    [MemoryDiagnoser]
    public class FhirConverterBenchmark : BaseBenchmark
    {
        public FhirConverterBenchmark()
        {
        }

        public override void Parse()
        {
            // Do nothing
        }

        public override string Render()
        {
            ConverterOptions _options = new ConverterOptions()
            {
                InputDataContent = TestContent,
                TemplateDirectory = @"C:\work\FluidCdaTest\FluidCdaTest.Benchmark\DevBranchTemplates\Ccda",
                RootTemplate = "CCD",
            };

            var convertedFhirString = ConverterLogicHandler.ConvertToString(_options);
            return convertedFhirString;
        }
    }
}
