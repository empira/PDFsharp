// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// ReSharper disable InconsistentNaming

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Specifies how the document should be displayed by a viewer when opened.
    /// </summary>
    public enum PdfPageMode
    {
        /// <summary>
        /// Neither document outline nor thumbnail images visible.
        /// </summary>
        UseNone,

        /// <summary>
        /// Document outline visible.
        /// </summary>
        UseOutlines,

        /// <summary>
        /// Thumbnail images visible.
        /// </summary>
        UseThumbs,

        /// <summary>
        /// Full-screen mode, with no menu bar, window controls, or any other window visible.
        /// </summary>
        FullScreen,

        /// <summary>
        /// (PDF 1.5) Optional content group panel visible.
        /// </summary>
        UseOC,

        /// <summary>
        /// (PDF 1.6) Attachments panel visible.
        /// </summary>
        UseAttachments,
    }
}