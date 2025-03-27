using BenchmarkDotNet.Attributes;
using System.IO;
using System.Threading.Tasks;

namespace FluidCdaTest.Benchmark.Benchmarks
{
    public abstract class BaseBenchmark
    {
        protected static string TestContent = null;

        [Params("CDA.ccda", "testModel.txt")]
        public string InputPayloadFileName { get; set; }

        public string InputPayloadFilePath => BenchmarkConstants.SampleDataPath + InputPayloadFileName;

        [GlobalSetup]
        public void GlobalSetup()
        {
            TestContent = File.ReadAllText(InputPayloadFilePath);
            SetupBenchmark();
        }

        public abstract void SetupBenchmark();

        public async Task<string> ParseAndRender(string inputFilePath)
        {
            InputPayloadFileName = inputFilePath;
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
