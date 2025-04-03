# FHIR Fluid Converter
This project reimplements the core functionality of Microsoft's FHIR-Converter library for converting CCDAs to FHIR resources, but using the Fluid liquid template parser. This project aims to improve performance by reducing conversion time and minimizing memory allocations.

## Origin
While working on a FHIR solution, it was decided that we would support the conversion of CCDAs into FHIR bundle objects which could be processed by the server. We noticed immediately that Microsoft's solution didn't scale well when processing extremely large CCDAs.

Microsoft's FHIR-Converter library uses DotLiquid internally which hasn't been updated in a while and seemed to be a likely bottleneck. Fluid's own benchmarks seemed promising so I started this project to emulate Microsoft's original conversion logic but against the Fluid parser engine.

## Usage
Initialize an instance of the `FhirConverter` with your desired settings using `CCDParserOptions`.

```csharp
var converter = new FhirConverter(new CCDParserOptions
{
    TemplateDirectoryPath = "<Path to Liquid Templates>",
    RootTemplate = "CCD.liquid",
    UseCachedFileProvider = true
});
```
- `TemplateDirectoryPath` - Path to a directory containing all of your Liquid templates
- `RootTemplate` - The entry Liquid template where rendering starts and spiders out from
- `UseCachedFileProvider` - Whether or not to use an internal IFileProvider which caches the contents of read Liquid templates for quick retrieval when accessing the same file repeatedly. Aims to improve throughput by reducing IO which is sometimes affected by external factors (e.g., antivirus software).

Pass the CCDA document as a string to the converter, which returns a FHIR resource as a JSON string

```csharp
var inputCCDA = File.ReadAllText("<Path to CCDA File>");

string fhirJson = await converter.ConvertCcdaToFhirAsync(inputCCDA);
```

The resulting FHIR resource can then be saved, transmitted, or processed further as needed!

## Performance

### Benchmark
The benchmarks below compares the Fluid implementation against a precompiled version of Microsoft's FHIR-Converter library with a modified entry point to facilitate the tests.

#### Methods
We compare four different use cases:
- **Fluid_Parse**: Creates a new `FhirConverter` instance for each execution with the **CachedFileProvider enabled**
- **Fluid_Parse_Static**: Reuses a static `FhirConverter` for each execution with the **CachedFileProvider disabled**
- **Fluid_Parse_Static_Cached**: Reuses a static `FhirConverter` for each execution with the **CachedFileProvider enabled**
- **Microsoft_FhirConverter_Parse**: Creates a new Microsoft FHIR Converter instance for each execution

#### Input Payloads
The benchmarked methods execute twice: 
- `CDA.ccda` - a small basic CCDA file
- `LargeCDA.ccda` - a much larger file containing test data from an EHR

#### Results
``` text
BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.5039/23H2/2023Update/SunValley3)
11th Gen Intel Core i7-11800H 2.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.201
  [Host]     : .NET 8.0.14 (8.0.1425.11118), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 8.0.14 (8.0.1425.11118), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


| Method                    | InputPayloadFileName | Mean       | Error     | StdDev     | Median     | Ratio | RatioSD | Gen0       | Gen1      | Gen2      | Allocated | Alloc Ratio |
|-------------------------- |--------------------- |-----------:|----------:|-----------:|-----------:|------:|--------:|-----------:|----------:|----------:|----------:|------------:|
| Fluid_Parse               | CDA.ccda             |  25.086 ms | 0.4517 ms |  0.4226 ms |  25.039 ms |  1.63 |    0.08 |   656.2500 |  343.7500 |   62.5000 |   8.16 MB |        0.53 |
| Fluid_Parse_Static        | CDA.ccda             |  14.950 ms | 1.0713 ms |  3.1419 ms |  14.662 ms |  0.97 |    0.21 |   500.0000 |  250.0000 |         - |   5.97 MB |        0.39 |
| Fluid_Parse_Static_Cached | CDA.ccda             |   8.610 ms | 0.1706 ms |  0.2706 ms |   8.582 ms |  0.56 |    0.03 |   593.7500 |  437.5000 |   62.5000 |   7.45 MB |        0.49 |
| Microsoft_FhirConverter_Parse       | CDA.ccda             |  15.407 ms | 0.3053 ms |  0.7314 ms |  15.092 ms |  1.00 |    0.06 |  1593.7500 |  812.5000 |  718.7500 |   15.3 MB |        1.00 |
|                           |                      |            |           |            |            |       |         |            |           |           |           |             |
| Fluid_Parse               | LargeCDA.ccda        | 133.789 ms | 2.6454 ms |  4.1186 ms | 132.446 ms |  0.55 |    0.03 |  5000.0000 | 2500.0000 |  500.0000 |  66.99 MB |        0.35 |
| Fluid_Parse_Static        | LargeCDA.ccda        | 108.327 ms | 3.9510 ms | 11.6496 ms | 107.782 ms |  0.44 |    0.05 |  6000.0000 | 3000.0000 |  333.3333 |  76.27 MB |        0.40 |
| Fluid_Parse_Static_Cached | LargeCDA.ccda        | 110.226 ms | 2.1849 ms |  3.8836 ms | 110.744 ms |  0.45 |    0.03 |  6000.0000 | 4000.0000 | 1500.0000 |  66.35 MB |        0.35 |
| Microsoft_FhirConverter_Parse       | LargeCDA.ccda        | 245.369 ms | 4.8877 ms | 11.1317 ms | 246.137 ms |  1.00 |    0.06 | 15000.0000 | 8000.0000 | 4000.0000 | 190.72 MB |        1.00 |
```