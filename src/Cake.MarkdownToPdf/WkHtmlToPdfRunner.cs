using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.Tooling;

using Markdig;

namespace Cake.MarkdownToPdf
{
    /// <summary>
    /// Tool runner for WkHtmlToPdf.
    /// </summary>
    public sealed class WkHtmlToPdfRunner : Tool<WkHtmlToPdfSettings>
    {
        private readonly ICakeLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="WkHtmlToPdfRunner"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="processRunner">The process runner.</param>
        /// <param name="tools">The tool locator.</param>
        /// <param name="log">The logger.</param>
        public WkHtmlToPdfRunner(Core.IO.IFileSystem fileSystem, ICakeEnvironment environment, Core.IO.IProcessRunner processRunner, IToolLocator tools, ICakeLog log)
            : base(fileSystem, environment, processRunner, tools)
        {
            _log = log;
        }

        /// <summary>
        /// Runs WkHtmlToPdf with the provided settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void Run(WkHtmlToPdfSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (!System.IO.Path.IsPathRooted(settings.OutputFile))
            {
                settings.OutputFile = System.IO.Path.GetFullPath(settings.OutputFile);
            }

            if (!string.IsNullOrEmpty(settings.InputFile) && !System.IO.Path.IsPathRooted(settings.InputFile))
            {
                settings.InputFile = System.IO.Path.GetFullPath(settings.InputFile);
            }

            if (!CanWriteToOutputFile(settings.OutputFile))
            {
                throw new InvalidOperationException($"Please close the output file first: {settings.OutputFile}");
            }

            CheckAssetDirectory(settings);

            var tempPath = System.IO.Path.Combine(Path.GetTempPath(), $"CakeMarkdownToPdf{Guid.NewGuid():n}");
            Directory.CreateDirectory(tempPath);

            try
            {
                if (!string.IsNullOrEmpty(settings.HtmlFile))
                {
                    GenerateHtmlTemplate(settings, tempPath);
                }

                var outDir = System.IO.Path.GetDirectoryName(settings.OutputFile);
                if (!Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }

                Run(settings, GetArguments(settings));
            }
            finally
            {
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
            }
        }

        /// <summary>
        /// Gets the possible names of the WkHtmlToPdf's tool executable.
        /// </summary>
        /// <returns>All possible executable names.</returns>
        protected override IEnumerable<string> GetToolExecutableNames() => new[] { "WkHtmlToPdf.exe", "WkHtmlToPdf", "wkhtmltopdf" };

        /// <summary>
        /// Get WkHtmlToPdf tool name.
        /// </summary>
        /// <returns>The tools name.</returns>
        protected override string GetToolName() => "WkHtmlToPdf";

        private void GenerateHtmlTemplate(WkHtmlToPdfSettings settings, string tempPath)
        {
            settings.MarkdownPipeline.DebugLog = Console.Out;

            string html;
            if (!string.IsNullOrWhiteSpace(settings.InputFile))
            {
                _log.Debug($"Converting file '{settings.InputFile}' to Html...");
                html = Markdown.ToHtml(File.ReadAllText(settings.InputFile), settings.MarkdownPipeline.Build());
            }
            else
            {
                _log.Information($"Transforming markdown text {settings.InputText.Substring(0, Math.Min(settings.InputText.Length, 20))}... to Html...");
                html = Markdown.ToHtml(settings.InputText, settings.MarkdownPipeline.Build());
            }

            html = ApplyTheme(html, settings, _log, tempPath, settings.AssetDirectory);

            settings.HtmlFile = Path.Combine(tempPath, "convert.html");
            File.WriteAllText(settings.HtmlFile, html);
        }

        private void CheckAssetDirectory(WkHtmlToPdfSettings settings)
        {
            if (string.IsNullOrEmpty(settings.AssetDirectory))
            {
                if (!string.IsNullOrEmpty(settings.InputFile))
                {
                    settings.AssetDirectory = System.IO.Path.GetDirectoryName(settings.InputFile);
                }
                else
                {
                    settings.AssetDirectory = System.IO.Path.GetDirectoryName(settings.OutputFile);
                }
            }
        }

        private Core.IO.ProcessArgumentBuilder GetArguments(WkHtmlToPdfSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.HtmlFile))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0}: Input file not specified", GetToolName()));
            }

            var builder = new Core.IO.ProcessArgumentBuilder();

            builder.Append($"--image-dpi {settings.ImageDpi} ");
            builder.Append($"--image-quality {settings.ImageQuality} ");
            builder.Append($"--page-size {settings.PageSize} ");
            builder.Append($"--orientation {settings.Orientation} ");
            builder.Append("--print-media-type ");

            if (settings.Margins.Left > 0)
            {
                builder.Append($"--margin-left {settings.Margins.Left}mm ");
            }

            if (settings.Margins.Right > 0)
            {
                builder.Append($"--margin-right {settings.Margins.Right}mm ");
            }

            if (settings.Margins.Top > 0)
            {
                builder.Append($"--margin-top {settings.Margins.Top}mm ");
            }

            if (settings.Margins.Bottom > 0)
            {
                builder.Append($"--margin-bottom {settings.Margins.Bottom}mm ");
            }

            // don' use --disable-smart-shrinking
            // this zooms/fits the content "correct" to the page but the font kerning is a mess
            // use --zom 1.3 instead
            builder.Append("--zoom 1.3 ");
            builder.Append("--dpi 300 ");

            if (settings.AdditionalGlobalOptions != null)
            {
                builder.Append($"{settings.AdditionalGlobalOptions} ");
            }

            // in file
            builder.Append($"page \"{settings.HtmlFile}\" ");

            if (settings.AdditionalPageOptions != null)
            {
                builder.Append($"{settings.AdditionalPageOptions} ");
            }

            // out files
            builder.AppendQuoted(settings.OutputFile);

            return builder;
        }

        private static string ApplyTheme(string html, WkHtmlToPdfSettings settings, ICakeLog log, string tempDirectory, string assetDirectory)
        {
            if (string.IsNullOrEmpty(settings.CssFile))
            {
                WriteResourceFile($"Themes.{settings.Theme}.Theme.css", tempDirectory);
                settings.CssFile = Path.Combine(tempDirectory, $"Themes.{settings.Theme}.Theme.css");
            }

            if (string.IsNullOrEmpty(settings.HtmlTemplateFile))
            {
                WriteResourceFile($"Themes.{settings.Theme}.Theme.html", tempDirectory);
                settings.HtmlTemplateFile = Path.Combine(tempDirectory, $"Themes.{settings.Theme}.Theme.html");
            }

            if (!Path.IsPathRooted(settings.CssFile))
            {
                settings.CssFile = Path.GetFullPath(settings.CssFile);
            }

            if (!Path.IsPathRooted(settings.HtmlTemplateFile))
            {
                settings.HtmlTemplateFile = Path.GetFullPath(settings.HtmlTemplateFile);
            }

            if (!File.Exists(settings.CssFile))
            {
                log.Error($"CSS file '{settings.CssFile}' not found!");
            }

            if (!File.Exists(settings.HtmlTemplateFile))
            {
                log.Error($"Html template file '{settings.HtmlTemplateFile}' not found!");
            }

            var template = File.ReadAllText(settings.HtmlTemplateFile);

            if (!assetDirectory.EndsWith(Path.PathSeparator.ToString()))
            {
                assetDirectory += Path.PathSeparator;
            }

            return template
                .Replace("{$html}", html)
                .Replace("{$cssFile}", settings.CssFile)
                .Replace("{$docPath}", assetDirectory);
        }

        private static void WriteResourceFile(string resourceName, string tempDirectory)
        {
            var stream = typeof(MarkdownToPdfAliases).Assembly.GetManifestResourceStream(resourceName);
            string path = Path.Combine(tempDirectory, resourceName);
            using (FileStream outputFileStream = new FileStream(path, FileMode.Create))
            {
                stream.CopyTo(outputFileStream);
            }
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
