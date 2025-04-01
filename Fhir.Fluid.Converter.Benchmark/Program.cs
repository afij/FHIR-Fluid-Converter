using BenchmarkDotNet.Running;

namespace Fhir.Fluid.Converter.Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var test = new FluidStaticParserCachedProviderBenchmark();
            //test.InputPayloadFileName = "CDA.ccda";
            //test.GlobalSetup();
            //var parsedFHIRString = await test.ExecuteBenchmark();
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
