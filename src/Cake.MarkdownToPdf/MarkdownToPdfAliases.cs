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
                MarkdownPdf.Transform(File.ReadAllText(markdownFile), outputFile);
            }
            finally
            {
                Directory.SetCurrentDirectory(oldDirectory);
            }
        }
    }
}
