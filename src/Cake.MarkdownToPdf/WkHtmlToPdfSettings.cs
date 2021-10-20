using Cake.Core.Tooling;

using Markdig;

namespace Cake.MarkdownToPdf
{
    /// <summary>
    /// Settings of the WkHtmlToPdf tool.
    /// </summary>
    public sealed class WkHtmlToPdfSettings : ToolSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WkHtmlToPdfSettings"/> class.
        /// </summary>
        public WkHtmlToPdfSettings()
        {
            MarkdownPipeline = new MarkdownPipelineBuilder();
        }

        /// <summary>
        /// Gets or sets the output file name.
        /// </summary>
        public string OutputFile { get; set; }

        /// <summary>
        /// Gets or sets the pdf page size.
        /// </summary>
        public PdfPageSize PageSize { get; set; } = PdfPageSize.A4;

        /// <summary>
        /// Gets or sets the page orientation.
        /// </summary>
        public PdfPageOrientation Orientation { get; set; } = PdfPageOrientation.Portrait;

        /// <summary>
        /// Gets or sets the dpi of the images within the pdf.
        /// </summary>
        public int ImageDpi { get; set; } = 300;

        /// <summary>
        /// Gets or sets the percentage if the output image quality (compression).
        /// </summary>
        public int ImageQuality { get; set; } = 100;

        /// <summary>
        /// Gets margins in mm.
        /// </summary>
        public PdfPageMargins Margins { get; } = new PdfPageMargins();

        /// <summary>
        /// Gets or sets the customizable path to wkhtmltopdf tool.
        /// </summary>
        public string PathToWkhtmltopdf { get; set; }

        /// <summary>
        /// Gets or sets additional wkhtmltopdf Global Options.
        /// </summary>
        public string AdditionalGlobalOptions { get; set; }

        /// <summary>
        /// Gets or sets additional wkhtmltopdf Page Options.
        /// </summary>
        public string AdditionalPageOptions { get; set; }

        /// <summary>
        /// Gets or sets the used Html template file.
        /// </summary>
        public string HtmlFile { get; set; }

        /// <summary>
        /// Gets the settings used for parsing markdown.
        /// </summary>
        public MarkdownPipelineBuilder MarkdownPipeline { get; }

        /// <summary>
        /// Gets or sets the Css file used for rendering the HTML/Pdf.
        /// </summary>
        public string CssFile { get; set; }

        /// <summary>
        /// Gets or sets a html template file.
        /// </summary>
        public string HtmlTemplateFile { get; set; }

        /// <summary>
        /// Gets or sets the used theme.
        /// </summary>
        public Themes Theme { get; set; }

        /// <summary>
        /// Gets or sets the asset directory. Used for image retrieving.
        /// </summary>
        public string AssetDirectory { get; set; }

        /// <summary>
        /// Gets or sets Markdown text which should be converted.
        /// </summary>
        public string InputText { get; set; }

        /// <summary>
        /// Gets or sets the markdown file which should be converted.
        /// </summary>
        public string InputFile { get; set; }

        /// <summary>
        /// Calls the UseAdvancedExtensions() on the MarkdownPipelineBuilder.
        /// See https://github.com/lunet-io/markdig/blob/master/src/Markdig/MarkdownExtensions.cs .
        /// </summary>
        public void UseAdvancedMarkdownExtensions()
        {
            MarkdownPipeline.UseAdvancedExtensions();
        }

        /// <summary>
        /// Calls the UsePipeTables() on the MarkdownPipelineBuilder.
        /// See https://github.com/lunet-io/markdig/blob/master/src/Markdig/MarkdownExtensions.cs .
        /// </summary>
        public void UseAdvancedMarkdownTables()
        {
            MarkdownPipeline.UsePipeTables();
        }
    }
}
