using Cake.Core;
using NSubstitute;
using Xunit;

namespace Cake.MarkdownToPdf.Tests
{
    public class MarkdownFileToPdfTests
    {
        [Fact]
        public void ConvertFile()
        {
            ICakeContext context = Substitute.For<ICakeContext>();

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

            MarkdownToPdfAliases.MarkdownFileToPdf(context, "Assets/TestImage.md", "TestImage.pdf");
        }
    }
}
