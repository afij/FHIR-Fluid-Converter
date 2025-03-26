using Fluid;
using FluidCdaTest.Parsers;
using FluidCdaTest.Parsers.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FluidCdaTest
{
    public class Program
    {
        private const string TemplateDirectoryPath = @"C:\work\HAG-FHIR\HAG.FHIR.API\data\Templates\Ccda";

        static async Task Main()
        {
            var parser = new CCDParser(new CCDParserOptions() { TemplateDirectoryPath = TemplateDirectoryPath, UseCachedFileProvider = true });
            IFluidTemplate template = await parser.Parse();

            //var inputCCDA = await File.ReadAllTextAsync(@"C:\work\FluidCdaTest\data\SampleData\CDA.ccda");
            var inputCCDA = await File.ReadAllTextAsync(@"C:\work\FluidCdaTest\data\SampleData\testModel.txt");
            string renderedString = await parser.RenderAsync(template, inputCCDA);
            Console.WriteLine(renderedString);
        }
    }
}
