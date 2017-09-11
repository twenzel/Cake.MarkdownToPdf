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
        /// <summary>
        /// Initialize a new settings instance
        /// </summary>
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
        /// <summary>
        /// Gets or sets the pdf page size
        /// </summary>
        public PdfPageSize PageSize { get; set; } = PdfPageSize.A4;

        /// <summary>
        /// Gets or sets the page orientation
        /// </summary>
        public PdfPageOrientation Orientation { get; set; } = PdfPageOrientation.Portrait;

        /// <summary>
        /// Gets or sets the dpi of the images within the pdf
        /// </summary>
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

    /// <summary>
    /// The Pdf page sizes
    /// </summary>
    public enum PdfPageSize
    {
        /// <summary>
        /// Letter format
        /// </summary>
        Letter,

        /// <summary>
        /// DIN A4 format
        /// </summary>
        A4,
    }
    /// <summary>
    /// The page orientations
    /// </summary>

    public enum PdfPageOrientation
    {
        /// <summary>
        /// Potrait mode
        /// </summary>
        Portrait,

        /// <summary>
        /// Landscape mode
        /// </summary>
        Landscape
    }

    /// <summary>
    /// Margins in mm
    /// </summary>
    public class PdfPageMargins
    {
        /// <summary>
        /// Left margin
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// Right margin
        /// </summary>
        public int Right { get; set; }

        /// <summary>
        /// Top margin
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// Bottom margin
        /// </summary>
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
