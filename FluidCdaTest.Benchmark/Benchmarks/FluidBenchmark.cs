using BenchmarkDotNet.Attributes;
using DotLiquid;
using Fluid;
using FluidCdaTest.Filters;
using FluidCdaTest.Parsers;
using FluidCdaTest.Providers;
using FluidCdaTest.Utilities;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    /// <summary>
    /// Recreates parser and template options each execution
    /// </summary>
    [MemoryDiagnoser]
    public class FluidBenchmark : BaseBenchmark
    {
        private CCDParser _parser = new CCDParser(BenchmarkConstants.TemplatesPath);
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
