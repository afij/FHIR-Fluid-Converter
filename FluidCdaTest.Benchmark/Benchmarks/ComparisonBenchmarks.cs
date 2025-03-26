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

        private readonly FluidStaticParserCachedProviderBenchmark _staticCachedParserBenchmark = new FluidStaticParserCachedProviderBenchmark();
        private readonly FluidStaticParserBenchmark _staticParserBenchmark = new FluidStaticParserBenchmark();
        private readonly FluidBenchmark _fluidBenchmark = new FluidBenchmark();
        private readonly FhirConverterBenchmark _fhirConverterBenchmark = new FhirConverterBenchmark();

        [Benchmark, BenchmarkCategory("ParseAndRender")]
        public object Fluid_Parse_Cache()
        {
            return _staticCachedParserBenchmark.ParseAndRender(InputPayloadFilePath);
        }

        [Benchmark, BenchmarkCategory("ParseAndRender")]
        public object Fluid_Parse_PartialCache()
        {
            return _staticParserBenchmark.ParseAndRender(InputPayloadFilePath);
        }

        [Benchmark, BenchmarkCategory("ParseAndRender")]
        public object Fluid_Parse_NoCache()
        {
            return _fluidBenchmark.ParseAndRender(InputPayloadFilePath);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("ParseAndRender")]
        public object FhirConverter_Parse()
        {
            return _fhirConverterBenchmark.ParseAndRender(InputPayloadFilePath);
        }
    }
}
