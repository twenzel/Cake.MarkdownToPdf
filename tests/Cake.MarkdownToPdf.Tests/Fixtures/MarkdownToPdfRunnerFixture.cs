using Cake.Testing;
using Cake.Testing.Fixtures;

namespace Cake.MarkdownToPdf.Tests.Fixtures
{
    public class MarkdownToPdfRunnerFixture : ToolFixture<Settings>
    {
        public FakeLog Log { get; }

        public MarkdownToPdfRunnerFixture()
            : base("wkhtmltopdf.exe")
        {
            Log = new FakeLog();
        }

        public void GivenProcessReturnsStdOutputOf(string[] stdOutput)
        {
            ProcessRunner.Process.SetStandardOutput(stdOutput);
        }

        protected override void RunTool()
        {
            var tool = new MarkdownToPdfRunner(
                FileSystem,
                Environment,
                ProcessRunner,
                Tools,
                Log);

            tool.Run(Settings);
        }
    }
}
