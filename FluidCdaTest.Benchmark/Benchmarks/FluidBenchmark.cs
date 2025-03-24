using BenchmarkDotNet.Attributes;
using Fluid;
using FluidCdaTest.Filters;
using FluidCdaTest.Providers;
using FluidCdaTest.Utilities;
using System.Collections.Generic;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    /// <summary>
    /// Recreates parser and template options each execution
    /// </summary>
    [MemoryDiagnoser]
    public class FluidBenchmark : BaseBenchmark
    {
        public FluidBenchmark()
        {
            // Do nothing
        }

        [Benchmark]
        public override string ParseAndRender()
        {

            TemplateOptions options = new TemplateOptions();
            CCDParser parser = new CCDParser();

            parser.RegisterCustomTags();
            options.Filters.RegisterCustomFilters();

            CDAFileProvider provider = new CDAFileProvider(TestRootTemplateDir);
            options.FileProvider = provider;

            parser.TryParse(TestRootTemplateContent, out var template);
            var context = new TemplateContext(new Dictionary<string, object> { { "msg", TestObject } }, options);

            // Preload ValueSet data as CodeMapping obj
            var valueSetString = ((CDAFileProvider)options.FileProvider).ReadTemplateFile(@"ValueSet/ValueSet");
            context.AmbientValues.Add(GeneralFilters.CODE_MAPPING_VALUE_NAME, TemplateUtility.ParseCodeMapping(valueSetString));

            return template.Render(context);
        }
    }
}
