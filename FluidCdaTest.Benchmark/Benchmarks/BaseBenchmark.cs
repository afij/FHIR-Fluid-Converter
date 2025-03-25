using BenchmarkDotNet.Attributes;
using System.IO;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    public abstract class BaseBenchmark
    {
        protected static string TestContent = null;
        protected static object TestObject = null;
        protected static string TestRootTemplateDir = @"C:\work\HAG-FHIR\HAG.FHIR.API\data\Templates\Ccda";
        protected static string TestRootTemplateFile = @"C:\work\HAG-FHIR\HAG.FHIR.API\data\Templates\Ccda\CCD.liquid";
        protected static string TestRootTemplateContent = null;

        [Params(@"C:\work\FluidCdaTest\data\SampleData\CDA.ccda", @"C:\work\FluidCdaTest\data\SampleData\testModel.txt")]
        public string FilePath { get; set; }

        static BaseBenchmark()
        {
            //var assembly = typeof(BaseBenchmark).Assembly;
            //TestContent = File.ReadAllText(@"C:\work\FluidCdaTest\data\SampleData\testModel.txt");
            //TestContent = File.ReadAllText(@"C:\work\FluidCdaTest\data\SampleData\CDA.ccda");
            //TestContent = File.ReadAllText(@"C:\work\FluidCdaTest\CurrentCDA-Test1g.txt");
        }

        public string ParseAndRender(string inputFilePath)
        {
            TestContent = File.ReadAllText(inputFilePath);
            TestObject = Processors.PreProcessor.ParseToObject(TestContent);
            TestRootTemplateContent = File.ReadAllText(TestRootTemplateFile);

            Parse();
            return Render();
        }

        public abstract void Parse();

        public abstract string Render();

        [Benchmark]
        public virtual string ExecuteBenchmark()
        {
            return ParseAndRender(FilePath);
        }

    }
}
