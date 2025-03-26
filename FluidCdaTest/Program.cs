using Fluid;
using FluidCdaTest.Parsers;
using System;
using System.Threading.Tasks;

namespace FluidCdaTest
{
    public class Program
    {
        private const string TemplateDirectoryPath = @"C:\work\HAG-FHIR\HAG.FHIR.API\data\Templates\Ccda";

        static async Task Main()
        {
            var parser = new CCDParser(TemplateDirectoryPath);

            IFluidTemplate template = await parser.Parse();
            string renderedString = await parser.RenderAsync(template);
            Console.WriteLine(renderedString);
        }
    }
}
