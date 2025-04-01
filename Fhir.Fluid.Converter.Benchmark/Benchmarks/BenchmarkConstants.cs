using System;

namespace Fhir.Fluid.Converter.Benchmark.Benchmarks
{
    public static class BenchmarkConstants
    {
        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public static readonly string SampleDataPath = $"{BaseDirectory}/SampleData/";
        public static readonly string TemplatesPath = $"{BaseDirectory}/Templates/Ccda";
        public static readonly string RootTemplatePath = $"{BaseDirectory}/Templates/Ccda/CCD.liquid";
    }
}
