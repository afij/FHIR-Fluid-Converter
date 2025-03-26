using BenchmarkDotNet.Attributes;
using Fluid;
using FluidCdaTest.Parsers;
using System.Threading.Tasks;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    /// <summary>
    /// Reuses parser and options during repeated execution
    /// </summary>
    [MemoryDiagnoser]
    public class FluidCachedParserOnlyBenchmark : BaseBenchmark
    {
        private static readonly CCDParser _parser = new CCDParser(BenchmarkConstants.TemplatesPath);
        private IFluidTemplate _template;

        public override async Task ParseAsync()
        {
            _template = await _parser.Parse();
        }

        public override async Task<string> RenderAsync()
        {
            return await _parser.RenderAsync(_template);
        }
    }
}
