using Cake.Core;
using Cake.MarkdownToPdf.Tests.Fixtures;
using Cake.Testing;
using Shouldly;
using System;
using Xunit;

namespace Cake.MarkdownToPdf.Tests
{
    public class MarkdownToPdfRunnerTests
    {
        [Fact]
        public void Should_Throw_If_Settings_Are_Null()
        {
            var fixture = new MarkdownToPdfRunnerFixture { Settings = null! };

            Action result = () =>
            {
                fixture.Run();
            };

            result.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Should_Log_Error_If_OutputFile_Is_Null()
        {
            var fixture = new MarkdownToPdfRunnerFixture { Settings = new Settings() };

            fixture.Run();

            fixture.Log.Entries.ShouldHaveSingleItem();
            fixture.Log.Entries.ShouldContain(e => e.Message == "No output file given. Please set the output file in the settings!");
        }

        [Fact]
        public void Should_Throw_If_Executable_Was_Not_Found()
        {
            var fixture = new MarkdownToPdfRunnerFixture
            {
                Settings = new Settings
                {
                    OutputFile = "test.pdf",
                    MarkdownText = "#test"
                }
            };
            fixture.GivenDefaultToolDoNotExist();

            Action result = () =>
            {
                fixture.Run();
            };

            result.ShouldThrow<CakeException>().Message.ShouldBe("wkhtmltopdf: Could not locate executable.");
        }
    }
}
