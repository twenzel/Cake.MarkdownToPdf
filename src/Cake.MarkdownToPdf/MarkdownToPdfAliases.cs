using System;
using System.IO;

using Cake.Core;
using Cake.Core.Annotations;

namespace Cake.MarkdownToPdf
{
    /// <summary>
    /// Contains functionality to convert markdown to pdf files.
    /// </summary>
    [CakeAliasCategory("Documentation")]
    public static class MarkdownToPdfAliases
    {
        /// <summary>
        /// Converts a markdown file to pdf file.
        /// </summary>
        /// <example>
        /// <code>
        /// Task("Convert")
        ///   .Does(() => {
        ///     MarkdownFileToPdf("readme.md", "output.pdf");
        ///
        ///     // or with settings
        ///      MarkdownFileToPdf("readme.md", "output.pdf", settings => {
        ///         settings.Theme = Themes.Github
        ///         settings.UseAdvancedMarkdownTables();
        ///      });
        /// });
        /// </code>
        /// </example>
        /// <param name="context">The context.</param>
        /// <param name="markdownFile">The markdown file to convert.</param>
        /// <param name="outputFile">The name of the pdf file to create.</param>
        /// <param name="settingsAction">Settings used for convertion.</param>
        [CakeMethodAlias]
        public static void MarkdownFileToPdf(this ICakeContext context, string markdownFile, string outputFile, Action<WkHtmlToPdfSettings> settingsAction = null)
        {
            if (!File.Exists(markdownFile))
            {
                throw new ArgumentException($"Markdown file '{markdownFile}' does not exist.", nameof(markdownFile));
            }

            if (string.IsNullOrWhiteSpace(outputFile))
            {
                throw new ArgumentException("Output file is missing or empty.", nameof(outputFile));
            }

            var settings = new WkHtmlToPdfSettings();
            settings.InputFile = markdownFile;
            settings.OutputFile = outputFile;
            settingsAction?.Invoke(settings);

            MarkdownToPdf(context, settings);
        }

        /// <summary>
        /// Converts a markdown string to pdf file.
        /// </summary>
        /// <example>
        /// <code>
        /// Task("Convert")
        ///   .Does(() => {
        ///     MarkdownFileToPdf("Some text in Markdown format...", "output.pdf");
        /// });
        /// </code>
        /// </example>
        /// <param name="context">The context.</param>
        /// <param name="markdownText">The markdown string to convert.</param>
        /// <param name="outputFile">The name of the pdf file to create.</param>
        /// <param name="settingsAction">Settings used for convertion.</param>
        [CakeMethodAlias]
        public static void MarkdownToPdf(this ICakeContext context, string markdownText, string outputFile, Action<WkHtmlToPdfSettings> settingsAction = null)
        {
            if (string.IsNullOrWhiteSpace(markdownText))
            {
                throw new ArgumentException("Markdown text is missing or empty.", nameof(markdownText));
            }

            if (string.IsNullOrWhiteSpace(outputFile))
            {
                throw new ArgumentException("Output file is missing or empty.", nameof(outputFile));
            }

            var settings = new WkHtmlToPdfSettings();
            settings.InputText = markdownText;
            settings.OutputFile = outputFile;
            settingsAction?.Invoke(settings);

            MarkdownToPdf(context, settings);
        }

        /// <summary>
        /// Converts a markdown string to pdf file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="settings">The tool settings to convert the markdown.</param>
        [CakeMethodAlias]
        public static void MarkdownToPdf(this ICakeContext context, WkHtmlToPdfSettings settings)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            var runner = new WkHtmlToPdfRunner(context.FileSystem, context.Environment, context.ProcessRunner, context.Tools, context.Log);

            runner.Run(settings);
        }
    }
}
