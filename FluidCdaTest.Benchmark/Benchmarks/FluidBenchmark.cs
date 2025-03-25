using BenchmarkDotNet.Attributes;
using DotLiquid;
using Fluid;
using FluidCdaTest.Filters;
using FluidCdaTest.Parsers;
using FluidCdaTest.Providers;
using FluidCdaTest.Utilities;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    /// <summary>
    /// Recreates parser and template options each execution
    /// </summary>
    [MemoryDiagnoser]
    public class FluidBenchmark : BaseBenchmark
    {
        private TemplateOptions _templateOptions;
        private TemplateContext _templateContext;
        private IFluidTemplate _template;

        public override void Parse()
        {
            _templateOptions = new TemplateOptions();
            CCDParser parser = new CCDParser();

            parser.RegisterCustomTags();
            _templateOptions.Filters.RegisterCustomFilters();

            CDAFileProvider provider = new CDAFileProvider(TestRootTemplateDir);
            _templateOptions.FileProvider = provider;

            parser.TryParse(TestRootTemplateContent, out _template);
            _templateContext = new TemplateContext(new Dictionary<string, object> { { "msg", TestObject } }, _templateOptions);
        }

        public override string Render()
        {
            // Preload ValueSet data as CodeMapping obj
            var valueSetString = ((CDAFileProvider)_templateOptions.FileProvider).ReadTemplateFile(@"ValueSet/ValueSet");
            _templateContext.AmbientValues.Add(GeneralFilters.CODE_MAPPING_VALUE_NAME, TemplateUtility.ParseCodeMapping(valueSetString));

            return _template.Render(_templateContext);
        }

        //[Benchmark]
        //protected override string ExecuteBenchmarkParseAndRender()
        //{

        //    TemplateOptions options = new TemplateOptions();
        //    CCDParser parser = new CCDParser();

        //    parser.RegisterCustomTags();
        //    options.Filters.RegisterCustomFilters();

        //    CDAFileProvider provider = new CDAFileProvider(TestRootTemplateDir);
        //    options.FileProvider = provider;

        //    parser.TryParse(TestRootTemplateContent, out var template);
        //    var context = new TemplateContext(new Dictionary<string, object> { { "msg", TestObject } }, options);

        //    // Preload ValueSet data as CodeMapping obj
        //    var valueSetString = ((CDAFileProvider)options.FileProvider).ReadTemplateFile(@"ValueSet/ValueSet");
        //    context.AmbientValues.Add(GeneralFilters.CODE_MAPPING_VALUE_NAME, TemplateUtility.ParseCodeMapping(valueSetString));

        //    return template.Render(context);
        //}

        public override string ExecuteBenchmark()
        {
            return base.ExecuteBenchmark();
        }
    }
}
