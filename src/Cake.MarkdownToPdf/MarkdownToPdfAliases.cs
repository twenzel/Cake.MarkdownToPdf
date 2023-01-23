using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using Cake.MarkdownToPdf.Internal;
using Markdig;
using System;
using System.IO;
using System.Reflection;

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
        /// <param name="ctx">The context</param>
        /// <param name="markdownFile">The markdown file to convert.</param>
        /// <param name="outputFile">The name of the pdf file to create.</param>
        /// <param name="settingsAction">Settings used for convertion</param>
        [CakeMethodAlias]
        public static void MarkdownFileToPdf(this ICakeContext ctx, string markdownFile, string outputFile, Action<Settings> settingsAction = null)
        {
            if (!Path.IsPathRooted(markdownFile))
                markdownFile = Path.GetFullPath(markdownFile);

            if (!Path.IsPathRooted(outputFile))
                outputFile = Path.GetFullPath(outputFile);

            if (!File.Exists(markdownFile))
            {
                ctx.Log.Error($"Markdown file '{markdownFile}' does not exist!");
                return;
            }

            ctx.Log.Information($"Transforming '{markdownFile}' to '{outputFile}'...");
            ConvertTextToPdf(File.ReadAllText(markdownFile), outputFile, settingsAction, ctx.Log, Path.GetDirectoryName(markdownFile));
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
        /// <param name="ctx">The context</param>
        /// <param name="markdownText">The markdown string to convert.</param>
        /// <param name="outputFile">The name of the pdf file to create.</param>
        /// <param name="settingsAction">Settings used for convertion</param>
        [CakeMethodAlias]
        public static void MarkdownToPdf(this ICakeContext ctx, string markdownText, string outputFile, Action<Settings> settingsAction = null)
        {
            if (!Path.IsPathRooted(outputFile))
                outputFile = Path.GetFullPath(outputFile);

            ctx.Log.Information($"Transforming markdown text {markdownText.Substring(0, Math.Min(markdownText.Length, 20))}... to '{outputFile}'...");
            ConvertTextToPdf(markdownText, outputFile, settingsAction, ctx.Log, Path.GetDirectoryName(outputFile));
        }

        private static void ConvertTextToPdf(string markdownText, string outputFile, Action<Settings> settingsAction, ICakeLog log, string sourceWorkingDirectory)
        {
            if (!CanWriteToOutputFile(outputFile))
            {
                log.Error("Please close the output file first: " + outputFile);
                return;
            }

            var settings = new Settings();

            settingsAction?.Invoke(settings);

            if (string.IsNullOrEmpty(settings.WorkingDirectory))
                settings.WorkingDirectory = sourceWorkingDirectory;

            string htmlFile = Path.Combine(Path.GetTempPath(), $"convert{Guid.NewGuid().ToString("n")}.html");

            settings.MarkdownPipeline.DebugLog = Console.Out;

            var html = Markdown.ToHtml(markdownText, settings.MarkdownPipeline.Build());

            var baseDirectory = GetBaseDirectory();
            html = ApplyTheme(html, settings, log, baseDirectory, settings.WorkingDirectory);

            try
            {
                File.WriteAllText(htmlFile, html);
                var generator = new PdfGenerator();
                var exitCode = generator.ConvertToPdf(htmlFile, outputFile, settings.Pdf, baseDirectory, log);

                if (exitCode != 0)
                {
                    log.Error("Error creating pdf document. Exit code: " + exitCode);
                    log.Error(generator.ExecutionOutputText);
                }
            }
            finally
            {
                if (File.Exists(htmlFile))
                    File.Delete(htmlFile);
            }
        }

        private static string GetBaseDirectory()
        {
            if (Array.Find(AppDomain.CurrentDomain.GetAssemblies(), a => a.FullName.Contains("xunit")) != null)
                return Directory.GetCurrentDirectory();
            else
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private static string ApplyTheme(string html, Settings settings, ICakeLog log, string baseDirectory, string workingDirectory)
        {
            if (string.IsNullOrEmpty(settings.CssFile))
                settings.CssFile = Path.Combine(baseDirectory, "Themes", settings.Theme.ToString(), "Theme.css");

            if (string.IsNullOrEmpty(settings.HtmlTemplateFile))
                settings.HtmlTemplateFile = Path.Combine(baseDirectory, "Themes", settings.Theme.ToString(), "Theme.html");

            if (!Path.IsPathRooted(settings.CssFile))
                settings.CssFile = Path.GetFullPath(settings.CssFile);

            if (!Path.IsPathRooted(settings.HtmlTemplateFile))
                settings.HtmlTemplateFile = Path.GetFullPath(settings.HtmlTemplateFile);

            if (!File.Exists(settings.CssFile))
                log.Error($"CSS file '{settings.CssFile}' not found!");

            if (!File.Exists(settings.HtmlTemplateFile))
                log.Error($"Html template file '{settings.HtmlTemplateFile}' not found!");

            var template = File.ReadAllText(settings.HtmlTemplateFile);

            if (!workingDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
                workingDirectory += Path.DirectorySeparatorChar;

            return template
                .Replace("{$html}", html)
                .Replace("{$cssFile}", settings.CssFile)
                .Replace("{$docPath}", workingDirectory);
        }

        private static bool CanWriteToOutputFile(string outputFile)
        {
            if (File.Exists(outputFile))
            {
                try
                {
                    File.Delete(outputFile);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
    }
}
