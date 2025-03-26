using BenchmarkDotNet.Attributes;
using System.IO;
using System.Threading.Tasks;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    public abstract class BaseBenchmark
    {
        protected static string TestContent = null;
        protected static object TestObject = null;
        protected static string TestRootTemplateContent = null;

        [Params("CDA.ccda", "testModel.txt")]
        public string InputPayloadFileName { get; set; }

        public string InputPayloadFilePath => BenchmarkConstants.SampleDataPath + InputPayloadFileName;

        public async Task<string> ParseAndRender(string inputFilePath)
        {
            TestContent = File.ReadAllText(inputFilePath);
            TestObject = Processors.PreProcessor.ParseToObject(TestContent);
            TestRootTemplateContent = File.ReadAllText(BenchmarkConstants.RootTemplatePath);

            await ParseAsync();
            return await RenderAsync();
        }

        public abstract Task ParseAsync();

        public abstract Task<string> RenderAsync();

        [Benchmark]
        public virtual async Task<string> ExecuteBenchmark()
        {
            return await ParseAndRender(InputPayloadFilePath);
        }

    }
}
