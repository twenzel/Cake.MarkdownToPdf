using Markdig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cake.MarkdownToPdf
{
    /// <summary>
    /// Settings used for the conversion
    /// </summary>
    public class Settings
    {
        public Settings()
        {
            MarkdownPipeline = new MarkdownPipelineBuilder();
        }

        /// <summary>
        /// Settings used for parsing markdown
        /// </summary>
        public MarkdownPipelineBuilder MarkdownPipeline { get; }

        /// <summary>
        /// Gets or sets the Css file used for rendering the HTML/Pdf
        /// </summary>
        public string CssFile { get; set; }

        /// <summary>
        /// Gets or sets a html template file
        /// </summary>
        public string HtmlTemplateFile { get; set; }

        /// <summary>
        /// Gets or sets the used theme
        /// </summary>
        public Themes Theme { get; set; }

        /// <summary>
        /// Settings for the pdf convert
        /// </summary>
        public PdfSettings Pdf { get; } = new PdfSettings();

        /// <summary>
        /// Calls the UseAdvancedExtensions() on the MarkdownPipelineBuilder.
        /// See https://github.com/lunet-io/markdig/blob/master/src/Markdig/MarkdownExtensions.cs
        /// </summary>
        /// <returns></returns>
        public void UseAdvancedMarkdownExtensions()
        {
            MarkdownPipeline.UseAdvancedExtensions();
        }

        /// <summary>
        /// Calls the UsePipeTables() on the MarkdownPipelineBuilder.
        /// See https://github.com/lunet-io/markdig/blob/master/src/Markdig/MarkdownExtensions.cs
        /// </summary>
        /// <returns></returns>
        public void UseAdvancedMarkdownTables()
        {
            MarkdownPipeline.UsePipeTables();
        }
    }

    /// <summary>
    /// Settings for the pdf convert
    /// </summary>
    public class PdfSettings
    {
        public PdfPageSize PageSize { get; set; } = PdfPageSize.A4;

        public PdfPageOrientation Orientation { get; set; } = PdfPageOrientation.Portrait;

        public int ImageDpi { get; set; } = 300;

        /// <summary>
        /// Percentage if the output image quality (compression)
        /// </summary>
        public int ImageQuality { get; set; } = 100;

        /// <summary>
        /// Margins in mm
        /// </summary>
        public PdfPageMargins Margins { get; } = new PdfPageMargins();

        /// <summary>
        /// Customizable path to wkhtmltopdf tool
        /// </summary>
        public string PathToWkhtmltopdf { get; set; }
    }

    public enum PdfPageSize
    {
        Letter,
        A4,
    }

    public enum PdfPageOrientation
    {
        Portrait,
        Landscape
    }

    /// <summary>
    /// Margins in mm
    /// </summary>
    public class PdfPageMargins
    {
        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }
    }

    /// <summary>
    /// The html output themes
    /// </summary>
    public enum Themes
    {
        /// <summary>
        /// Default theme
        /// </summary>
        Default = 0,

        /// <summary>
        /// Github style
        /// </summary>
        Github
    }
}
