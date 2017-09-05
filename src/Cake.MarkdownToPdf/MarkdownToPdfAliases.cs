using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using FSharp.Markdown.Pdf;
using System;
using System.IO;
using MigraDoc.DocumentObjectModel;

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
        /// });
        /// </code>
        /// </example>
        /// <param name="ctx">The context</param>
        /// <param name="markdownFile">The markdown file to convert.</param>
        /// <param name="outputFile">The name of the pdf file to create.</param>
        [CakeMethodAlias]
        public static void MarkdownFileToPdf(this ICakeContext ctx, string markdownFile, string outputFile)
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

            var oldDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(markdownFile));

            try
            {
                ctx.Log.Information($"Transforming '{markdownFile}' to '{outputFile}'...");
                ConvertTextToPdf(File.ReadAllText(markdownFile), outputFile);                
            }
            finally
            {
                Directory.SetCurrentDirectory(oldDirectory);
            }
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
        [CakeMethodAlias]
        public static void MarkdownToPdf(this ICakeContext ctx, string markdownText, string outputFile)
        {
            if (!Path.IsPathRooted(outputFile))
                outputFile = Path.GetFullPath(outputFile);

            var oldDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(outputFile));

            try
            {
                ctx.Log.Information($"Transforming markdown text {markdownText.Substring(0, Math.Min(markdownText.Length, 20))}... to '{outputFile}'...");

                ConvertTextToPdf(markdownText, outputFile);
            }
            finally
            {
                Directory.SetCurrentDirectory(oldDirectory);
            }
        }

        private static void ConvertTextToPdf(string markdownText, string outputFile)
        {
            var d = new MigraDoc.DocumentObjectModel.Document();
            var section = d.AddSection();

            MarkdownPdf.AddMarkdown(d, section, FSharp.Markdown.Markdown.Parse(markdownText, Environment.NewLine));
            ReplaceHtmlEntities(section.Elements);
            ApplyDefaultStyle(d, section);

            var r = new MigraDoc.Rendering.PdfDocumentRenderer(false, PdfSharp.Pdf.PdfFontEmbedding.Always)
            {
                Document = d
            };
            r.RenderDocument();
            r.PdfDocument.Save(outputFile);
        }

        private static void ReplaceHtmlEntities(DocumentObjectCollection elements)
        {
            foreach(var element in elements)
            {
                switch (element)
                {
                    case Paragraph para:
                        ReplaceHtmlEntities(para.Elements);
                        break;
                    case FormattedText formattedText:
                        ReplaceHtmlEntities(formattedText.Elements);
                        break;
                    case Hyperlink hyperlink:
                        ReplaceHtmlEntities(hyperlink.Elements);
                        break;
                    case Text text:
                        ReplaceHtmlEntities(text);
                        break;
                    case MigraDoc.DocumentObjectModel.Tables.Table table:
                        ReplaceHtmlEntities(table.Rows);
                        break;
                    case MigraDoc.DocumentObjectModel.Tables.Row row:
                        ReplaceHtmlEntities(row.Cells);
                        break;
                    case MigraDoc.DocumentObjectModel.Tables.Cell cell:
                        ReplaceHtmlEntities(cell.Elements);
                        break;
                    case Character c:
                        
                        break;
                    default:
                        break;
                }                           
            }
        }

        private static void ReplaceHtmlEntities(Text textElement)
        {
            textElement.Content = System.Net.WebUtility.HtmlDecode(textElement.Content.Replace("&amp;", "&"));
        }

            private static void ApplyDefaultStyle(MigraDoc.DocumentObjectModel.Document document, MigraDoc.DocumentObjectModel.Section section)
        {
            var normal = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Normal];
            normal.Font = new MigraDoc.DocumentObjectModel.Font("Helvetica", MigraDoc.DocumentObjectModel.Unit.FromPoint(14));            


            var heading1 = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Heading1];
            heading1.Font = new MigraDoc.DocumentObjectModel.Font("Helvetica", MigraDoc.DocumentObjectModel.Unit.FromPoint(28));
            heading1.Font.Bold = true;
            heading1.ParagraphFormat.SpaceAfter = MigraDoc.DocumentObjectModel.Unit.FromPoint(10);
            heading1.ParagraphFormat.SpaceBefore = MigraDoc.DocumentObjectModel.Unit.FromPoint(20);

            var heading2 = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Heading2];
            heading2.BaseStyle = FSharp.Markdown.Pdf.MarkdownStyleNames.Heading1;
            heading2.Font.Size = MigraDoc.DocumentObjectModel.Unit.FromPoint(24);
            heading2.ParagraphFormat.Borders.Bottom.Color = new MigraDoc.DocumentObjectModel.Color(204, 204, 204);
            heading2.ParagraphFormat.Borders.Bottom.Width = MigraDoc.DocumentObjectModel.Unit.FromPoint(1);

            var heading3 = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Heading3];
            heading3.BaseStyle = FSharp.Markdown.Pdf.MarkdownStyleNames.Heading1;
            heading3.Font.Size = MigraDoc.DocumentObjectModel.Unit.FromPoint(18);

            var heading4 = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Heading4];
            heading4.BaseStyle = FSharp.Markdown.Pdf.MarkdownStyleNames.Heading1;
            heading4.Font.Size = MigraDoc.DocumentObjectModel.Unit.FromPoint(16);

            var heading5 = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Heading5];
            heading5.BaseStyle = FSharp.Markdown.Pdf.MarkdownStyleNames.Heading1;
            heading5.Font.Size = MigraDoc.DocumentObjectModel.Unit.FromPoint(14);

            var heading6 = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Heading6];
            heading6.BaseStyle = FSharp.Markdown.Pdf.MarkdownStyleNames.Heading1;
            heading6.Font.Size = MigraDoc.DocumentObjectModel.Unit.FromPoint(12);

            var code = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Code];
            code.Font = new MigraDoc.DocumentObjectModel.Font("Consolas", MigraDoc.DocumentObjectModel.Unit.FromPoint(12));
            code.ParagraphFormat.Borders.Color = new MigraDoc.DocumentObjectModel.Color(234, 234, 234);
            //code.ParagraphFormat.Borders.Bottom.Width = MigraDoc.DocumentObjectModel.Unit.FromPoint(1);
            //code.ParagraphFormat.Borders.Bottom.Style = MigraDoc.DocumentObjectModel.BorderStyle.Single;
            //code.ParagraphFormat.Borders.Bottom.Visible = true;
            //code.ParagraphFormat.Borders.Top.Width = MigraDoc.DocumentObjectModel.Unit.FromPoint(1);
            //code.ParagraphFormat.Borders.Left.Width = MigraDoc.DocumentObjectModel.Unit.FromPoint(1);
            //code.ParagraphFormat.Borders.Right.Width = MigraDoc.DocumentObjectModel.Unit.FromPoint(1);
            code.ParagraphFormat.Borders.Visible = true;
            code.ParagraphFormat.Borders.Style = MigraDoc.DocumentObjectModel.BorderStyle.Single;
            code.ParagraphFormat.Borders.Width = MigraDoc.DocumentObjectModel.Unit.FromPoint(1);

            code.ParagraphFormat.Shading.Color = new MigraDoc.DocumentObjectModel.Color(248, 248, 248);
            code.ParagraphFormat.Shading.Visible = true;
            code.ParagraphFormat.SpaceAfter = MigraDoc.DocumentObjectModel.Unit.FromPoint(15);
            code.ParagraphFormat.SpaceBefore = MigraDoc.DocumentObjectModel.Unit.FromPoint(15);          

            var hyperlink = document.Styles[FSharp.Markdown.Pdf.MarkdownStyleNames.Hyperlink];
            hyperlink.Font.Color = new MigraDoc.DocumentObjectModel.Color(65, 131, 196);
            hyperlink.Font.Underline = MigraDoc.DocumentObjectModel.Underline.None;

            var p = document.AddStyle("p", FSharp.Markdown.Pdf.MarkdownStyleNames.Normal);
            p.ParagraphFormat.SpaceAfter = MigraDoc.DocumentObjectModel.Unit.FromPoint(15);
            p.ParagraphFormat.SpaceBefore = MigraDoc.DocumentObjectModel.Unit.FromPoint(15);

            foreach (var element in section.Elements)
            {
                var para = element as MigraDoc.DocumentObjectModel.Paragraph;
                if (para != null)
                {
                    if (para.Style == string.Empty)
                    {
                        para.Style = "p";
                    }
                }
            }

        }
    }
}
