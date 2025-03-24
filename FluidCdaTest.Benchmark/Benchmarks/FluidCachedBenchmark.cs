using BenchmarkDotNet.Attributes;
using Fluid;
using FluidCdaTest.Filters;
using FluidCdaTest.Providers;
using FluidCdaTest.Utilities;
using System.Collections.Generic;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    /// <summary>
    /// Caches parser and template options so parse and render does only that
    /// Also uses the CachedCDAFileProvider
    /// </summary>
    [MemoryDiagnoser]
    public class FluidCachedBenchmark : BaseBenchmark
    {
        private readonly TemplateOptions _options = new TemplateOptions();
        private readonly CCDParser _parser = new CCDParser();

        public FluidCachedBenchmark()
        {
            _parser.RegisterCustomTags();
            _options.Filters.RegisterCustomFilters();

            CachedCDAFileProvider provider = new CachedCDAFileProvider(TestRootTemplateDir);
            _options.FileProvider = provider;
        }

        [Benchmark]
        public override string ParseAndRender()
        {
            _parser.TryParse(TestRootTemplateContent, out var template);
            var context = new TemplateContext(new Dictionary<string, object> { { "msg", TestObject } }, _options);

            // Preload ValueSet data as CodeMapping obj
            var valueSetString = ((CachedCDAFileProvider)_options.FileProvider).ReadTemplateFile(@"ValueSet/ValueSet");
            context.AmbientValues.Add(GeneralFilters.CODE_MAPPING_VALUE_NAME, TemplateUtility.ParseCodeMapping(valueSetString));

            return template.Render(context);
        }
    }
}
