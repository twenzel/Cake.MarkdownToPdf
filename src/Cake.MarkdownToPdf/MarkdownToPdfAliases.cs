using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using System;
using System.IO;
using Markdig;
using Cake.MarkdownToPdf.Internal;
using System.Reflection;
using System.Linq;

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
            if (AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault( a => a.FullName.Contains("xunit")) != null)
                return Directory.GetCurrentDirectory();
            else
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private static string ApplyTheme(string html, Settings settings, ICakeLog log, string baseDirectory, string workingDirectory)
        {
            if (string.IsNullOrEmpty(settings.CssFile))
                settings.CssFile = Path.Combine(baseDirectory, $"Themes\\{settings.Theme}\\Theme.css");

            if (string.IsNullOrEmpty(settings.HtmlTemplateFile))
                settings.HtmlTemplateFile = Path.Combine(baseDirectory, $"Themes\\{settings.Theme}\\Theme.html");

            if (!Path.IsPathRooted(settings.CssFile))
                settings.CssFile = Path.GetFullPath(settings.CssFile);

            if (!Path.IsPathRooted(settings.HtmlTemplateFile))
                settings.HtmlTemplateFile = Path.GetFullPath(settings.HtmlTemplateFile);

            if (!File.Exists(settings.CssFile))
                log.Error($"CSS file '{settings.CssFile}' not found!");

            if (!File.Exists(settings.HtmlTemplateFile))
                log.Error($"Html template file '{settings.HtmlTemplateFile}' not found!");

            var template = File.ReadAllText(settings.HtmlTemplateFile);

            if (!workingDirectory.EndsWith("\\") && !workingDirectory.EndsWith("/"))
                workingDirectory += "\\";

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

        //private static string RemoveSpareLineEndings(string markdownText)
        //{
        //    markdownText = markdownText.Replace(Environment.NewLine + Environment.NewLine, "\n\n\n\n");

        //    markdownText = Regex.Replace(markdownText, "(\\r\\n(\\w))", " $2");

        //    markdownText = markdownText.Replace("\n\n\n\n", Environment.NewLine + Environment.NewLine);

        //    return markdownText;
        //}

        //private static void ReplaceHtmlEntities(DocumentObjectCollection elements)
        //{
        //    foreach (var element in elements)
        //    {
        //        switch (element)
        //        {
        //            case Paragraph para:
        //                ReplaceHtmlEntities(para.Elements);
        //                break;
        //            case FormattedText formattedText:
        //                ReplaceHtmlEntities(formattedText.Elements);
        //                break;
        //            case Hyperlink hyperlink:
        //                ReplaceHtmlEntities(hyperlink.Elements);
        //                break;
        //            case Text text:
        //                ReplaceHtmlEntities(text);
        //                break;
        //            case MigraDoc.DocumentObjectModel.Tables.Table table:
        //                ReplaceHtmlEntities(table.Rows);
        //                break;
        //            case MigraDoc.DocumentObjectModel.Tables.Row row:
        //                ReplaceHtmlEntities(row.Cells);
        //                break;
        //            case MigraDoc.DocumentObjectModel.Tables.Cell cell:
        //                ReplaceHtmlEntities(cell.Elements);
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //}

        //private static void ReplaceHtmlEntities(Text textElement)
        //{
        //    textElement.Content = System.Net.WebUtility.HtmlDecode(textElement.Content.Replace("&amp;", "&"));
        //}

        //private static void ApplyDefaultStyle(MigraDoc.DocumentObjectModel.Document document, MigraDoc.DocumentObjectModel.Section section)
        //{
        //    var normalFontSize = 10; // 14 => 28,24,18,16,14,12  //10 => 20,18,14,12,10,8           
        //    var paragraphMargin = 11;

        //    var normal = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Normal];
        //    normal.Font = new MigraDoc.DocumentObjectModel.Font("Helvetica", MigraDoc.DocumentObjectModel.Unit.FromPoint(normalFontSize));

        //    var heading1 = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Heading1];
        //    heading1.Font = new MigraDoc.DocumentObjectModel.Font("Helvetica", MigraDoc.DocumentObjectModel.Unit.FromPoint(normalFontSize * 2));
        //    heading1.Font.Bold = true;
        //    heading1.ParagraphFormat.SpaceAfter = MigraDoc.DocumentObjectModel.Unit.FromPoint(6);
        //    heading1.ParagraphFormat.SpaceBefore = MigraDoc.DocumentObjectModel.Unit.FromPoint(12);

        //    var heading2 = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Heading2];
        //    heading2.BaseStyle = FSharp.Markdown.Pdf.MarkdownStyleNames.Heading1;
        //    heading2.Font.Size = MigraDoc.DocumentObjectModel.Unit.FromPoint(normalFontSize + 8);
        //    heading2.ParagraphFormat.Borders.Bottom.Color = new MigraDoc.DocumentObjectModel.Color(204, 204, 204);
        //    heading2.ParagraphFormat.Borders.Bottom.Width = MigraDoc.DocumentObjectModel.Unit.FromPoint(1);

        //    var heading3 = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Heading3];
        //    heading3.BaseStyle = FSharp.Markdown.Pdf.MarkdownStyleNames.Heading1;
        //    heading3.Font.Size = MigraDoc.DocumentObjectModel.Unit.FromPoint(normalFontSize + 4);

        //    var heading4 = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Heading4];
        //    heading4.BaseStyle = FSharp.Markdown.Pdf.MarkdownStyleNames.Heading1;
        //    heading4.Font.Size = MigraDoc.DocumentObjectModel.Unit.FromPoint(normalFontSize + 2);

        //    var heading5 = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Heading5];
        //    heading5.BaseStyle = FSharp.Markdown.Pdf.MarkdownStyleNames.Heading1;
        //    heading5.Font.Size = MigraDoc.DocumentObjectModel.Unit.FromPoint(normalFontSize);

        //    var heading6 = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Heading6];
        //    heading6.BaseStyle = FSharp.Markdown.Pdf.MarkdownStyleNames.Heading1;
        //    heading6.Font.Size = MigraDoc.DocumentObjectModel.Unit.FromPoint(normalFontSize - 2);

        //    var code = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Code];
        //    code.Font = new MigraDoc.DocumentObjectModel.Font("Consolas", MigraDoc.DocumentObjectModel.Unit.FromPoint(normalFontSize - 2));
        //    code.ParagraphFormat.Borders.Color = new MigraDoc.DocumentObjectModel.Color(234, 234, 234);
        //    code.ParagraphFormat.Borders.Visible = true;
        //    code.ParagraphFormat.Borders.Style = MigraDoc.DocumentObjectModel.BorderStyle.Single;
        //    code.ParagraphFormat.Borders.Width = MigraDoc.DocumentObjectModel.Unit.FromPoint(1);

        //    code.ParagraphFormat.Shading.Color = new MigraDoc.DocumentObjectModel.Color(248, 248, 248);
        //    code.ParagraphFormat.Shading.Visible = true;
        //    code.ParagraphFormat.SpaceAfter = MigraDoc.DocumentObjectModel.Unit.FromPoint(paragraphMargin);
        //    code.ParagraphFormat.SpaceBefore = MigraDoc.DocumentObjectModel.Unit.FromPoint(paragraphMargin);

        //    var hyperlink = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Hyperlink];
        //    hyperlink.Font.Color = new MigraDoc.DocumentObjectModel.Color(65, 131, 196);
        //    hyperlink.Font.Underline = MigraDoc.DocumentObjectModel.Underline.None;

        //    var p = document.AddStyle("p", FSharp.Markdown.Pdf.MarkdownStyleNames.Normal);
        //    p.ParagraphFormat.SpaceAfter = MigraDoc.DocumentObjectModel.Unit.FromPoint(paragraphMargin);
        //    p.ParagraphFormat.SpaceBefore = MigraDoc.DocumentObjectModel.Unit.FromPoint(paragraphMargin);

        //    foreach (var element in section.Elements)
        //    {
        //        var para = element as MigraDoc.DocumentObjectModel.Paragraph;
        //        if (para != null)
        //        {
        //            if (para.Style == string.Empty)
        //            {
        //                para.Style = "p";
        //            }
        //        }
        //    }

        //}
    }
}
