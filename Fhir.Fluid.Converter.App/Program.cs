// See https://aka.ms/new-console-template for more information
using Fhir.Fluid.Converter;
using Fhir.Fluid.Converter.Parsers.Options;

var TemplateDirectoryPath = $"{AppDomain.CurrentDomain.BaseDirectory}/Templates/Ccda";

var converter = new FhirConverter(
    new CCDParserOptions()
    {
        TemplateDirectoryPath = TemplateDirectoryPath,
        RootTemplate = "CCD.liquid",
        UseCachedFileProvider = true
    }
);

var inputCCDA = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/SampleData/CDA.ccda");

string renderedString = await converter.ConvertCcdaToFhirAsync(inputCCDA);

Console.WriteLine(renderedString);