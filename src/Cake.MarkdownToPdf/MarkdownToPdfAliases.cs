using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using System;
using System.IO;

namespace Cake.MarkdownToPdf
{
    /// <summary>
    /// Contains functionality to convert markdown to pdf files
    /// </summary>
    [CakeAliasCategory("Documentation")]
    public static class MarkdownToPdfAliases
    {
        /// <summary>
        /// Converts a markdown file to pdf file.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
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
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="context">The <see cref="ICakeContext"/>.</param>
        /// <param name="markdownFile">The markdown file to convert.</param>
        /// <param name="outputFile">The name of the pdf file to create.</param>
        /// <param name="settingsAction">Settings used for convertion</param>
        [CakeMethodAlias]
        public static void MarkdownFileToPdf(this ICakeContext context, FilePath markdownFile, FilePath outputFile, Action<Settings> settingsAction = null)
        {
            if (!System.IO.Path.IsPathRooted(markdownFile.FullPath))
                markdownFile = System.IO.Path.GetFullPath(markdownFile.FullPath);

            if (!System.IO.Path.IsPathRooted(outputFile.FullPath))
                outputFile = System.IO.Path.GetFullPath(outputFile.FullPath);

            if (!File.Exists(markdownFile.FullPath))
            {
                context.Log.Error($"Markdown file '{markdownFile.FullPath}' does not exist!");
                return;
            }

            var settings = new Settings();
            settings.MarkdownFile = markdownFile;
            settings.OutputFile = outputFile;
            settingsAction?.Invoke(settings);

            context.Log.Information($"Transforming '{settings.MarkdownFile}' to '{settings.OutputFile}'...");

            MarkdownToPdf(context, settings);
        }

        /// <summary>
        /// Converts a markdown string to pdf file.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Task("Convert")
        ///   .Does(() => {        
        ///     MarkdownToPdf("Some text in Markdown format...", "output.pdf");
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="context">The <see cref="ICakeContext"/>.</param>
        /// <param name="markdownText">The markdown string to convert.</param>
        /// <param name="outputFile">The name of the pdf file to create.</param>
        /// <param name="settingsAction">Settings used for convertion</param>
        [CakeMethodAlias]
        public static void MarkdownToPdf(this ICakeContext context, string markdownText, FilePath outputFile, Action<Settings> settingsAction = null)
        {
            if (!System.IO.Path.IsPathRooted(outputFile.FullPath))
                outputFile = System.IO.Path.GetFullPath(outputFile.FullPath);

            var settings = new Settings();
            settings.MarkdownText = markdownText;
            settings.OutputFile = outputFile;
            settingsAction?.Invoke(settings);

            context.Log.Information($"Transforming markdown text {markdownText.Substring(0, Math.Min(markdownText.Length, 20))}... to '{outputFile}'...");
            MarkdownToPdf(context, settings);
        }

        /// <summary>
        /// Runs Wkhtmltopdf, using <see cref="Settings"/> for configuration.
        /// </summary>
        /// <param name="context">The <see cref="ICakeContext"/>.</param>
        /// <param name="settings">The <see cref="Settings"/>.</param>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Task("Convert")
        ///     .Does(() =>
        /// {
        ///     MarkdownToPdf(new Settings
        ///     {
        ///         MarkdownFile = "documtation.md",
        ///         OutputFile = "documentation.pdf"     
        ///     });
        /// });
        /// ]]>
        /// </code>
        /// </example>
        [CakeMethodAlias]
        public static void MarkdownToPdf(this ICakeContext context, Settings settings)
        {
            var runner = new MarkdownToPdfRunner(
                context.FileSystem,
                context.Environment,
                context.ProcessRunner,
                context.Tools,
                context.Log);
            runner.Run(settings);
        }
    }
}
