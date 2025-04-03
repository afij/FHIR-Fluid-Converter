using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Fhir.Fluid.Converter.Benchmark.Benchmarks
{
    [MemoryDiagnoser, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class ComparisonBenchmarks
    {
        [Params("CDA.ccda", "LargeCDA.ccda")]
        public string InputPayloadFileName { get; set; }

        public string InputPayloadFilePath => BenchmarkConstants.SampleDataPath + InputPayloadFileName;

        private static FluidBenchmark _fluidBenchmark;
        private static FluidStaticParserBenchmark _staticParserBenchmark;
        private static FluidStaticParserCachedProviderBenchmark _staticCachedParserBenchmark;
        private readonly MicrosoftFhirConverterBenchmark _fhirConverterBenchmark = new();

        [GlobalSetup]
        public void GlobalSetup()
        {
            _fluidBenchmark = new FluidBenchmark
            {
                InputPayloadFileName = InputPayloadFileName
            };
            _fluidBenchmark.GlobalSetup();

            _staticParserBenchmark = new FluidStaticParserBenchmark
            {
                InputPayloadFileName = InputPayloadFileName
            };
            _staticParserBenchmark.GlobalSetup();

            _staticCachedParserBenchmark = new FluidStaticParserCachedProviderBenchmark
            {
                InputPayloadFileName = InputPayloadFileName
            };
            _staticCachedParserBenchmark.GlobalSetup();
        }

        [Benchmark, BenchmarkCategory("ParseAndRender")]
        public object Fluid_Parse()
        {
            return _fluidBenchmark.ParseAndRender(InputPayloadFilePath);
        }

        [Benchmark, BenchmarkCategory("ParseAndRender")]
        public object Fluid_Parse_Static()
        {
            return _staticParserBenchmark.ParseAndRender(InputPayloadFilePath);
        }

        [Benchmark, BenchmarkCategory("ParseAndRender")]
        public object Fluid_Parse_Static_Cached()
        {
            return _staticCachedParserBenchmark.ParseAndRender(InputPayloadFilePath);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("ParseAndRender")]
        public object Microsoft_FhirConverter_Parse()
        {
            return _fhirConverterBenchmark.ParseAndRender(InputPayloadFilePath);
        }
    }
}
