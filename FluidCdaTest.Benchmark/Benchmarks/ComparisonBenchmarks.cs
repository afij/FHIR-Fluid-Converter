using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    [MemoryDiagnoser, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class ComparisonBenchmarks
    {
        //[Params("CDA.ccda", "testModel.txt")]
        [Params("CDA.ccda")]
        public string InputPayloadFileName { get; set; }

        public string InputPayloadFilePath => BenchmarkConstants.SampleDataPath + InputPayloadFileName;

        private static FluidStaticParserBenchmark _staticParserBenchmark;
        private static FluidStaticParserCachedProviderBenchmark _staticCachedParserBenchmark;
        private readonly FhirConverterBenchmark _fhirConverterBenchmark = new FhirConverterBenchmark();

        [GlobalSetup]
        public void GlobalSetup()
        {
            _staticParserBenchmark = new FluidStaticParserBenchmark();
            _staticParserBenchmark.InputPayloadFileName = InputPayloadFileName;
            _staticParserBenchmark.GlobalSetup();

            _staticCachedParserBenchmark = new FluidStaticParserCachedProviderBenchmark();
            _staticCachedParserBenchmark.InputPayloadFileName = InputPayloadFileName;
            _staticCachedParserBenchmark.GlobalSetup();
        }

        [Benchmark, BenchmarkCategory("ParseAndRender")]
        public object Fluid_Parse_Static_Cached()
        {
            return _staticCachedParserBenchmark.ParseAndRender(InputPayloadFilePath);
        }

        [Benchmark, BenchmarkCategory("ParseAndRender")]
        public object Fluid_Parse_Static()
        {
            return _staticParserBenchmark.ParseAndRender(InputPayloadFilePath);
        }

        [Benchmark, BenchmarkCategory("ParseAndRender")]
        public object Fluid_Parse()
        {
            FluidBenchmark fluidBenchmark = new FluidBenchmark();
            fluidBenchmark.InputPayloadFileName = InputPayloadFileName;
            fluidBenchmark.GlobalSetup();
            return fluidBenchmark.ParseAndRender(InputPayloadFilePath);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("ParseAndRender")]
        public object FhirConverter_Parse()
        {
            return _fhirConverterBenchmark.ParseAndRender(InputPayloadFilePath);
        }
    }
}
