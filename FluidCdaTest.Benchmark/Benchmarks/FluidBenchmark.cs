using BenchmarkDotNet.Attributes;
using Fluid;
using FluidCdaTest.Parsers;
using FluidCdaTest.Parsers.Options;
using System.Threading.Tasks;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    /// <summary>
    /// Recreates parser and template options each execution
    /// </summary>
    [MemoryDiagnoser]
    public class FluidBenchmark : BaseBenchmark
    {
        private readonly CCDParser _parser = new CCDParser(
            new CCDParserOptions()
            {
                TemplateDirectoryPath = BenchmarkConstants.TemplatesPath,
                UseCachedFileProvider = true
            }
        );
        private IFluidTemplate _template;

        public override void SetupBenchmark()
        {
            // No setup required
        }

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
