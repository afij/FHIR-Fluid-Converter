using BenchmarkDotNet.Attributes;
using System.IO;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    public abstract class BaseBenchmark
    {
        protected static string TestContent = null;
        protected static object TestObject = null;
        protected static string TestRootTemplateContent = null;

        [Params(@"CDA.ccda", @"testModel.txt")]
        public string InputPayloadFileName { get; set; }

        public string InputPayloadFilePath => BenchmarkConstants.SampleDataPath + InputPayloadFileName;

        public string ParseAndRender(string inputFilePath)
        {
            TestContent = File.ReadAllText(inputFilePath);
            TestObject = Processors.PreProcessor.ParseToObject(TestContent);
            TestRootTemplateContent = File.ReadAllText(BenchmarkConstants.RootTemplatePath);

            Parse();
            return Render();
        }

        public abstract void Parse();

        public abstract string Render();

        [Benchmark]
        public virtual string ExecuteBenchmark()
        {
            return ParseAndRender(InputPayloadFilePath);
        }

    }
}
