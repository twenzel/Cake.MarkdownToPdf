using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Tooling;
using Markdig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Cake.MarkdownToPdf
{
    /// <summary>
    /// The Tool-Runner for Wkhtmltopdf.
    /// </summary>
    /// <seealso cref="Tool{Settings}" />
    public sealed class MarkdownToPdfRunner : Tool<Settings>
    {
        private readonly ICakeLog _log;
        private readonly IFileSystem _fileSystem;
        private readonly ICakeEnvironment _environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownToPdfRunner"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="processRunner">The process runner.</param>
        /// <param name="tools">The tool locator.</param>
        /// <param name="log">The log.</param>        
        public MarkdownToPdfRunner(
            IFileSystem fileSystem,
            ICakeEnvironment environment,
            IProcessRunner processRunner,
            IToolLocator tools,
            ICakeLog log)
            : base(fileSystem, environment, processRunner, tools)
        {
            _log = log;
            _fileSystem = fileSystem;
            _environment = environment;
        }

        /// <summary>
        /// Runs the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <exception cref="ArgumentNullException">settings.</exception>
        public void Run(Settings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (settings.OutputFile == null)
            {
                _log.Error("No output file given. Please set the output file in the settings!");
                return;
            }

            if (!CanWriteToOutputFile(settings.OutputFile.FullPath))
            {
                _log.Error("Please close the output file first: {0}", settings.OutputFile);
                return;
            }

            var htmlFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"convert{Guid.NewGuid():n}.html");
            var html = ConvertMarkdownToHtml(settings);
            var addinBaseDirectory = GetBaseDirectory();
            var documentBaseDirectory = GetDocumentBaseDirectory(settings);

            html = ApplyTheme(html, settings, addinBaseDirectory, documentBaseDirectory);

            try
            {
                File.WriteAllText(htmlFile, html);

                var outDir = _fileSystem.GetDirectory(settings.OutputFile.GetDirectory());
                if (!outDir.Exists)
                    outDir.Create();

                var procSettings = new ProcessSettings
                {
                    RedirectStandardOutput = true
                };

                Run(settings, GetArguments(settings, htmlFile), procSettings, null);
            }
            finally
            {
                if (File.Exists(htmlFile) && !settings.Debug)
                    File.Delete(htmlFile);
                else if (settings.Debug)
                    _log.Information("Html file written to '{0}'", htmlFile);
            }
        }

        private static string ConvertMarkdownToHtml(Settings settings)
        {
            var markdownText = settings.MarkdownText ?? File.ReadAllText(settings.MarkdownFile.FullPath);

            settings.MarkdownPipeline.DebugLog = Console.Out;

            var html = Markdown.ToHtml(markdownText, settings.MarkdownPipeline.Build());
            return html;
        }

        private string GetDocumentBaseDirectory(Settings settings)
        {
            var documentBaseDirectory = settings.MarkdownBaseDirectory;

            if (string.IsNullOrEmpty(documentBaseDirectory) && settings.MarkdownFile != null)
            {
                documentBaseDirectory = settings.MarkdownFile.GetDirectory().FullPath;
            }

            if (string.IsNullOrEmpty(documentBaseDirectory))
            {
                documentBaseDirectory = settings.WorkingDirectory?.FullPath;
            }

            if (string.IsNullOrEmpty(documentBaseDirectory))
            {
                documentBaseDirectory = _environment.WorkingDirectory?.FullPath;
            }

            if (string.IsNullOrEmpty(documentBaseDirectory))
            {
                documentBaseDirectory = string.Empty;
            }

            return documentBaseDirectory;
        }

        /// <inheritdoc/>
        protected override IEnumerable<string> GetToolExecutableNames()
        {
            yield return "wkhtmltopdf.exe";
            yield return "wkhtmltopdf";
        }

        /// <inheritdoc/>
        protected override string GetToolName() => "wkhtmltopdf";

        private static ProcessArgumentBuilder GetArguments(Settings settings, string htmlFile)
        {
            var builder = new ProcessArgumentBuilder();

            builder.Append("--enable-local-file-access");
            builder.Append($"--image-dpi {settings.Pdf.ImageDpi}");
            builder.Append($"--image-quality {settings.Pdf.ImageQuality}");
            builder.Append($"--page-size {settings.Pdf.PageSize}");
            builder.Append($"--orientation {settings.Pdf.Orientation}");
            builder.Append("--print-media-type");

            if (settings.Pdf.Margins.Left > 0)
                builder.Append($"--margin-left {settings.Pdf.Margins.Left}mm");
            if (settings.Pdf.Margins.Right > 0)
                builder.Append($"--margin-right {settings.Pdf.Margins.Right}mm");
            if (settings.Pdf.Margins.Top > 0)
                builder.Append($"--margin-top {settings.Pdf.Margins.Top}mm");
            if (settings.Pdf.Margins.Bottom > 0)
                builder.Append($"--margin-bottom {settings.Pdf.Margins.Bottom}mm");

            // don' use --disable-smart-shrinking
            // this zooms/fits the content "correct" to the page but the font kerning is a mess
            // use --zom 1.3 instead
            builder.Append("--zoom 1.3");
            builder.Append("--dpi 300");

            if (settings.Pdf.AdditionalGlobalOptions != null)
                builder.Append(settings.Pdf.AdditionalGlobalOptions);

            // in file
            builder.Append("page").AppendQuoted(htmlFile);

            if (settings.Pdf.AdditionalPageOptions != null)
                builder.Append(settings.Pdf.AdditionalPageOptions);

            // out files
            builder.AppendQuoted(settings.OutputFile.FullPath);

            return builder;
        }

        private static string GetBaseDirectory()
        {
            if (Array.Find(AppDomain.CurrentDomain.GetAssemblies(), a => a.FullName.Contains("xunit")) != null)
                return Directory.GetCurrentDirectory();
            else
                return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private string ApplyTheme(string html, Settings settings, string addinBaseDirectory, string documentBaseDirectory)
        {
            if (string.IsNullOrEmpty(settings.CssFile))
                settings.CssFile = System.IO.Path.Combine(addinBaseDirectory, "Themes", settings.Theme.ToString(), "Theme.css");

            if (string.IsNullOrEmpty(settings.HtmlTemplateFile))
                settings.HtmlTemplateFile = System.IO.Path.Combine(addinBaseDirectory, "Themes", settings.Theme.ToString(), "Theme.html");

            if (!System.IO.Path.IsPathRooted(settings.CssFile))
                settings.CssFile = System.IO.Path.GetFullPath(settings.CssFile);

            if (!System.IO.Path.IsPathRooted(settings.HtmlTemplateFile))
                settings.HtmlTemplateFile = System.IO.Path.GetFullPath(settings.HtmlTemplateFile);

            if (!File.Exists(settings.CssFile))
                _log.Error($"CSS file '{0}' not found!", settings.CssFile);
            else
                _log.Debug("Use CSS file: {0}", settings.CssFile);

            if (!File.Exists(settings.HtmlTemplateFile))
                _log.Error($"Html template file '{0}' not found!", settings.HtmlTemplateFile);
            else
                _log.Debug("Use Html template file: {0}", settings.HtmlTemplateFile);

            var template = File.ReadAllText(settings.HtmlTemplateFile);

            if (!documentBaseDirectory.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                documentBaseDirectory += System.IO.Path.DirectorySeparatorChar;

            _log.Debug("Use document base directory: {0}", documentBaseDirectory);

            return template
                .Replace("{$html}", html)
                .Replace("{$cssFile}", settings.CssFile)
                .Replace("{$docPath}", documentBaseDirectory);
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
