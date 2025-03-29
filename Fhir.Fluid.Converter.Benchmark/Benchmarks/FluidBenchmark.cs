using BenchmarkDotNet.Attributes;
using Fhir.Fluid.Converter.Parsers;
using Fhir.Fluid.Converter.Parsers.Options;
using Fluid;
using System.Threading.Tasks;

namespace Fhir.Fluid.Converter.Benchmark.Benchmarks
{
    /// <summary>
    /// Recreates parser and template options each execution
    /// </summary>
    [MemoryDiagnoser]
    public class FluidBenchmark : BaseBenchmark
    {
        private CCDParser _parser;
        private IFluidTemplate _template;

        public override void SetupBenchmark()
        {
            // No setup required
        }

        public override void Parse()
        {
            _parser = new CCDParser(
                new CCDParserOptions()
                {
                    TemplateDirectoryPath = BenchmarkConstants.TemplatesPath,
                    RootTemplate = "CCD.liquid",
                    UseCachedFileProvider = true
                }
            );
            _template = _parser.Parse();
        }

        public override async Task<string> RenderAsync()
        {
            return await _parser.RenderAsync(_template, TestContent);
        }
    }
}
