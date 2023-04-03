// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Specifies the page layout to be used by a viewer when the document is opened.
    /// </summary>
    public enum PdfPageLayout
    {
        /// <summary>
        /// Display one page at a time.
        /// </summary>
        SinglePage,

        /// <summary>
        /// Display the pages in one column.
        /// </summary>
        OneColumn,

        /// <summary>
        /// Display the pages in two columns, with odd-numbered pages on the left.
        /// </summary>
        TwoColumnLeft,

        /// <summary>
        /// Display the pages in two columns, with odd-numbered pages on the right.
        /// </summary>
        TwoColumnRight,

        /// <summary>
        /// (PDF 1.5) Display the pages two at a time, with odd-numbered pages on the left.
        /// </summary>
        TwoPageLeft,

        /// <summary>
        /// (PDF 1.5) Display the pages two at a time, with odd-numbered pages on the right.
        /// </summary>
        TwoPageRight,
    }
}
