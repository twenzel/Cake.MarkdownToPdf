using Cake.Core;
using NSubstitute;
using Xunit;
using Cake.Core.Diagnostics;

namespace Cake.MarkdownToPdf.Tests
{
    public class MarkdownFileToPdfTests
    {
        [Fact]
        public void ConvertFile()
        {
            ICakeContext context = Substitute.For<ICakeContext>();
           
            //context.When(c => c.Log.Write(Arg.Any<Core.Diagnostics.Verbosity>(), Core.Diagnostics.LogLevel.Error, Arg.Any<string>(), Arg.Any<object[]>())).Do(call =>
            //Assert.False(true, call.Arg<string>())
            //);

            MarkdownToPdfAliases.MarkdownFileToPdf(context, "Assets/Test.md", "Testoutput.pdf", settings =>
            {
                settings.UseAdvancedMarkdownTables();
                settings.Theme = Themes.Github;
            });
        }

        [Fact]
        public void OutputImages()
        {
            ICakeContext context = Substitute.For<ICakeContext>();
            //context.Log.Information("test");

            //context.When(c => c.Log.Write(Arg.Any<Core.Diagnostics.Verbosity>(), Core.Diagnostics.LogLevel.Error, Arg.Any<string>(), Arg.Any<object[]>()))
            //    .Do(call => Assert.False(true, call.Arg<string>()));

            MarkdownToPdfAliases.MarkdownFileToPdf(context, "Assets/TestImage.md", "TestImage.pdf");
        }
    }
}
