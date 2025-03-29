using Fluid;
using Fhir.Fluid.Converter.Parsers;
using Fhir.Fluid.Converter.Parsers.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Fhir.Fluid.Converter
{
    public class Program
    {
        private const string TemplateDirectoryPath = @"C:\work\HAG-FHIR\HAG.FHIR.API\data\Templates\Ccda";

        static async Task Main()
        {
            var parser = new CCDParser(
                new CCDParserOptions()
                { 
                    TemplateDirectoryPath = TemplateDirectoryPath,
                    RootTemplate = "CCD.liquid",
                    UseCachedFileProvider = true
                }
            );
            IFluidTemplate template = parser.Parse();

            //var inputCCDA = await File.ReadAllTextAsync(@"C:\work\FluidCdaTest\data\SampleData\CDA.ccda");
            var inputCCDA = File.ReadAllText(@"C:\work\FluidCdaTest\data\SampleData\LargeCDA.ccda");

            string renderedString = await parser.RenderAsync(template, inputCCDA);

            for (int i = 0; i < 100; i++)
            {
                await parser.RenderAsync(template, inputCCDA);
            }
            //string renderedString = await parser.RenderAsync(template, inputCCDA);
            Console.WriteLine(renderedString);
        }
    }
}
