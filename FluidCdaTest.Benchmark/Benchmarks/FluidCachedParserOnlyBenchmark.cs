using BenchmarkDotNet.Attributes;
using Fluid;
using FluidCdaTest.Filters;
using FluidCdaTest.Parsers;
using FluidCdaTest.Providers;
using FluidCdaTest.Utilities;
using System.Collections.Generic;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    /// <summary>
    /// Reuses parser and options during repeated execution
    /// </summary>
    [MemoryDiagnoser]
    public class FluidCachedParserOnlyBenchmark : BaseBenchmark
    {
        private static readonly TemplateOptions _templateOptions = new TemplateOptions();
        private static readonly CCDParser _parser = new CCDParser();
        private TemplateContext _templateContext;
        private IFluidTemplate _template;

        static FluidCachedParserOnlyBenchmark()
        {
            _parser.RegisterCustomTags();
            _templateOptions.Filters.RegisterCustomFilters();

            CDAFileProvider provider = new CDAFileProvider(BenchmarkConstants.TemplatesPath);
            _templateOptions.FileProvider = provider;
        }

        public override void Parse()
        {
            _parser.TryParse(TestRootTemplateContent, out _template);
            _templateContext = new TemplateContext(new Dictionary<string, object> { { "msg", TestObject } }, _templateOptions);

            // Preload ValueSet data as CodeMapping obj
            var valueSetString = ((CDAFileProvider)_templateOptions.FileProvider).ReadTemplateFile(@"ValueSet/ValueSet");
            _templateContext.AmbientValues.Add(GeneralFilters.CODE_MAPPING_VALUE_NAME, TemplateUtility.ParseCodeMapping(valueSetString));
        }

        public override string Render()
        {
            return _template.Render(_templateContext);
        }
    }
}
