using BenchmarkDotNet.Attributes;
using Fluid;
using FluidCdaTest.Parsers;
using FluidCdaTest.Parsers.Options;
using System.Threading.Tasks;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    /// <summary>
    /// Uses a static parser instance during repeated execution in addition to using the CachedCDAFileProvider
    /// </summary>
    [MemoryDiagnoser]
    public class FluidStaticParserCachedProviderBenchmark : BaseBenchmark
    {
        private static CCDParser _parser;
        private IFluidTemplate _template;

        public override void SetupBenchmark()
        {
            _parser = new CCDParser(
                new CCDParserOptions()
                {
                    TemplateDirectoryPath = BenchmarkConstants.TemplatesPath,
                    RootTemplate = "CCD.liquid",
                    UseCachedFileProvider = true
                }
            );
        }

        public override void Parse()
        {
            _template = _parser.Parse();
        }

        public override async Task<string> RenderAsync()
        {
            return await _parser.RenderAsync(_template, TestContent);
        }
    }
}
