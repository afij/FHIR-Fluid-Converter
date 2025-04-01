// See https://aka.ms/new-console-template for more information
using Fhir.Fluid.Converter.Parsers;
using Fhir.Fluid.Converter.Parsers.Options;

String TemplateDirectoryPath = @"C:\work\HAG-FHIR\HAG.FHIR.API\data\Templates\Ccda";

var parser = new CCDParser(
    new CCDParserOptions()
    {
        TemplateDirectoryPath = TemplateDirectoryPath,
        RootTemplate = "CCD.liquid",
        UseCachedFileProvider = true
    }
);

var inputCCDA = File.ReadAllText(@"C:\work\FHIR-Fluid-Converter\data\SampleData\LargeCDA.ccda");

string renderedString = await parser.ParseAndRenderAsync(inputCCDA);

Console.WriteLine(renderedString);