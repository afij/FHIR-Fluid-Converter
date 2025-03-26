using BenchmarkDotNet.Attributes;
using Fluid;
using FluidCdaTest.Parsers;
using FluidCdaTest.Parsers.Options;
using System.Threading.Tasks;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    /// <summary>
    /// Uses a static parser instance during repeated execution
    /// </summary>
    [MemoryDiagnoser]
    public class FluidStaticParserBenchmark : BaseBenchmark
    {
        private static readonly CCDParser _parser = new CCDParser(
            new CCDParserOptions()
            {
                TemplateDirectoryPath = BenchmarkConstants.TemplatesPath,
                UseCachedFileProvider = false
            }
        );
        private IFluidTemplate _template;

        public override async Task ParseAsync()
        {
            _template = await _parser.Parse();
        }

        public override async Task<string> RenderAsync()
        {
            return await _parser.RenderAsync(_template, TestContent);
        }
    }
}
