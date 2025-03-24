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

        static BaseBenchmark()
        {
            //var assembly = typeof(BaseBenchmark).Assembly;
            TestContent = File.ReadAllText(@"C:\work\FluidCdaTest\FluidCdaTest\testModel.txt");
            //TestContent = File.ReadAllText(@"C:\work\FluidCdaTest\CurrentCDA-Test1g.txt");

            TestObject = Processors.PreProcessor.ParseToObject(TestContent);

            TestRootTemplateContent = File.ReadAllText(TestRootTemplateFile);
        }

        public BaseBenchmark()
        {
        }

        public abstract string ParseAndRender();

    }
}
