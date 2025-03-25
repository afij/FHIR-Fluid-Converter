using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    [MemoryDiagnoser, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class ComparisonBenchmarks
    {
        [Params(@"CDA.ccda", @"testModel.txt")]
        public string InputPayloadFileName { get; set; }

        public string InputPayloadFilePath => BenchmarkConstants.SampleDataPath + InputPayloadFileName;

        private readonly FluidCachedBenchmark _cachedFluidBenchmarks = new FluidCachedBenchmark();
        private readonly FluidCachedParserOnlyBenchmark _partialCachedFluidBenchmarks = new FluidCachedParserOnlyBenchmark();
        private readonly FluidBenchmark _fluidBenchmarks = new FluidBenchmark();
        private readonly FhirConverterBenchmark _fhirConverterBenchmarks = new FhirConverterBenchmark();

        [Benchmark, BenchmarkCategory("ParseAndRender")]
        public object Fluid_Parse_Cache()
        {
            return _cachedFluidBenchmarks.ParseAndRender(InputPayloadFilePath);
        }

        [Benchmark, BenchmarkCategory("ParseAndRender")]
        public object Fluid_Parse_PartialCache()
        {
            return _partialCachedFluidBenchmarks.ParseAndRender(InputPayloadFilePath);
        }

        [Benchmark, BenchmarkCategory("ParseAndRender")]
        public object Fluid_Parse_NoCache()
        {
            return _fluidBenchmarks.ParseAndRender(InputPayloadFilePath);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("ParseAndRender")]
        public object FhirConverter_Parse()
        {
            return _fhirConverterBenchmarks.ParseAndRender(InputPayloadFilePath);
        }
    }
}
