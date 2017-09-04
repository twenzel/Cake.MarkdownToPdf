using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using FSharp.Markdown.Pdf;
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
            if (!File.Exists(markdownFile))
            {
                ctx.Log.Error($"Markdown file '{markdownFile}' does not exist!");
                return;
            }

            ctx.Log.Information($"Transforming {markdownFile} to {outputFile}...");
            MarkdownPdf.Transform(File.ReadAllText(markdownFile), outputFile);
        }
    }
}
