// See https://aka.ms/new-console-template for more information
using Fhir.Fluid.Converter.Parsers;
using Fhir.Fluid.Converter.Parsers.Options;

var TemplateDirectoryPath = $"{AppDomain.CurrentDomain.BaseDirectory}/Templates/Ccda";

var parser = new CCDParser(
    new CCDParserOptions()
    {
        TemplateDirectoryPath = TemplateDirectoryPath,
        RootTemplate = "CCD.liquid",
        UseCachedFileProvider = true
    }
);

var inputCCDA = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/SampleData/CDA.ccda");

string renderedString = await parser.ParseAndRenderAsync(inputCCDA);

Console.WriteLine(renderedString);