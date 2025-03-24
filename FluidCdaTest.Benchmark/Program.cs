using BenchmarkDotNet.Running;

namespace FluidCdaTest.Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var test = new FluidCachedBenchmark();
            //test.ParseAndRender();
            //var fhirConverterTest = new FhirConverterBenchmark();
            //fhirConverterTest.ParseAndRender();
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
