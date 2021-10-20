namespace Cake.MarkdownToPdf
{
    /// <summary>
    /// Margins in mm.
    /// </summary>
    public class PdfPageMargins
    {
        /// <summary>
        /// Gets or sets the left margin.
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// Gets or sets the right margin.
        /// </summary>
        public int Right { get; set; }

        /// <summary>
        /// Gets or sets the  top margin.
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// Gets or sets the bottom margin.
        /// </summary>
        public int Bottom { get; set; }
    }
}
