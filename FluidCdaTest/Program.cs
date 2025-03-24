using Fluid;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluidCdaTest.Providers;
using FluidCdaTest.Filters;
using FluidCdaTest.Utilities;
using FluidCdaTest.Processors;

namespace FluidCdaTest
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var parser = new CCDParser();
            parser.RegisterCustomTags();
            parser.RegisterEvaluateTag();

            // Register filters and custom provider
            var options = new TemplateOptions();
            options.Filters.RegisterCustomFilters();

            //CDAFileProvider provider = new CDAFileProvider(@"C:\work\HAG-FHIR\HAG.FHIR.API\data\Templates\Ccda");
            CachedCDAFileProvider provider = new CachedCDAFileProvider(@"C:\work\HAG-FHIR\HAG.FHIR.API\data\Templates\Ccda");
            options.FileProvider = provider;

            // Load model from disk
            var testModel  = await File.ReadAllTextAsync(@"C:\work\FluidCdaTest\FluidCdaTest\testModel.txt");
            //var currentCdaTest1g = await File.ReadAllTextAsync(@"C:\work\FluidCdaTest\CurrentCDA-Test1g.txt");


            // Process model into an object (fixes data etc) and add to a new context
            //var testObj = ParseToObject(testModel);
            var testObj = PreProcessor.ParseToObject(testModel);
            var context = new TemplateContext(new Dictionary<string, object> { { "msg", testObj } }, options);

            // Preload ValueSet data as CodeMapping obj
            var valueSetString = provider.ReadTemplateFile(@"ValueSet/ValueSet");
            context.AmbientValues.Add(GeneralFilters.CODE_MAPPING_VALUE_NAME, TemplateUtility.ParseCodeMapping(valueSetString));

            var rootTemplate = await File.ReadAllTextAsync(@"C:\work\HAG-FHIR\HAG.FHIR.API\data\Templates\Ccda\CCD.liquid");
            //var rootTemplate = "{% evaluate patientId using 'Utils/GenerateId' obj: msg.ClinicalDocument.recordTarget.patientRole -%}{% include 'Test' test: 'testxd' test2: msg -%}";
            //var rootTemplate = "{% evaluate patientId using 'Utils/GenerateId' obj: msg.ClinicalDocument.recordTarget.patientRole -%}{% include 'Header' test: testval, test2: testval -%}";
            //var rootTemplate = "{% evaluate patientId using 'Utils/GenerateId' obj: msg.ClinicalDocument.recordTarget.patientRole -%}{% include 'Header', test: testval, test2: testval2, t3: t3 -%}";
            //var rootTemplate = "{% evaluate patientId using 'Utils/GenerateId' obj: msg.ClinicalDocument.recordTarget.patientRole -%}{% include 'Header' test: testval, test2: testval2, t3: t3 -%}";
            //var rootTemplate = @"{% evaluate patientId using 'Utils/GenerateId' obj: msg.ClinicalDocument.recordTarget.patientRole -%}
            //value: {{ patientId }}";

            // Parse the template
            if (parser.TryParse(rootTemplate, out var template, out var errors))
            {
                var result = await template.RenderAsync(context);
                Console.WriteLine(result);
                var mergedJsonString = PostProcessor.Process(result);
                Console.WriteLine(mergedJsonString);
            }
            else
            {
                throw new Exception("Failed to parse template: " + string.Join(", ", errors));
            }
        }
    }
}
