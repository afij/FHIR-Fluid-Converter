using BenchmarkDotNet.Running;

namespace Fhir.Fluid.Converter.Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var test = new FluidCachedBenchmark();
            //test.InputPayloadFileName = "CDA.ccda";
            //test.InputPayloadFileName = "testModel.txt";
            //var parsedFHIRString = await test.ExecuteBenchmark();
            //Console.WriteLine(parsedFHIRString);
            //var fhirConverterTest = new FhirConverterBenchmark();
            //fhirConverterTest.ParseAndRender();
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
