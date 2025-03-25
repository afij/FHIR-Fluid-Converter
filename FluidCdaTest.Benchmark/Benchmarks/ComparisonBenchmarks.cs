using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    [MemoryDiagnoser, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class ComparisonBenchmarks
    {
        [Params(@"C:\work\FluidCdaTest\data\SampleData\CDA.ccda", @"C:\work\FluidCdaTest\data\SampleData\testModel.txt")]
        public string FilePath { get; set; }

        private readonly FluidCachedBenchmark _cachedFluidBenchmarks = new FluidCachedBenchmark();
        private readonly FluidCachedParserOnlyBenchmark _partialCachedFluidBenchmarks = new FluidCachedParserOnlyBenchmark();
        private readonly FluidBenchmark _fluidBenchmarks = new FluidBenchmark();
        private readonly FhirConverterBenchmark _fhirConverterBenchmarks = new FhirConverterBenchmark();

        [Benchmark, BenchmarkCategory("ParseAndRender")]
        public object Fluid_Parse_Cache()
        {
            return _cachedFluidBenchmarks.ParseAndRender(FilePath);
        }

        [Benchmark, BenchmarkCategory("ParseAndRender")]
        public object Fluid_Parse_PartialCache()
        {
            return _partialCachedFluidBenchmarks.ParseAndRender(FilePath);
        }

        [Benchmark, BenchmarkCategory("ParseAndRender")]
        public object Fluid_Parse_NoCache()
        {
            return _fluidBenchmarks.ParseAndRender(FilePath);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("ParseAndRender")]
        public object FhirConverter_Parse()
        {
            return _fhirConverterBenchmarks.ParseAndRender(FilePath);
        }
    }
}
