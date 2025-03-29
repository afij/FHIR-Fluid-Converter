using BenchmarkDotNet.Attributes;
using Fhir.Fluid.Converter.Parsers;
using Fhir.Fluid.Converter.Parsers.Options;
using Fluid;
using System.Threading.Tasks;

namespace Fhir.Fluid.Converter.Benchmark.Benchmarks
{
    /// <summary>
    /// Uses a static parser instance during repeated execution
    /// </summary>
    [MemoryDiagnoser]
    public class FluidStaticParserBenchmark : BaseBenchmark
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
                    UseCachedFileProvider = false
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
