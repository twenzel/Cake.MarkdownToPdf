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

            MarkdownToPdfAliases.MarkdownFileToPdf(context, "Assets/Test.md", "Testoutput.pdf");
        }
    }
}
