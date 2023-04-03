// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.IO
{
    /// <summary>
    /// Determines how the PDF output stream is formatted. Even all formats create valid PDF files,
    /// only Compact or Standard should be used for production purposes.
    /// </summary>
    public enum PdfWriterLayout
    {
        /// <summary>
        /// The PDF stream contains no unnecessary characters. This is default in release build.
        /// </summary>
        Compact,

        /// <summary>
        /// The PDF stream contains some superfluous line feeds but is more readable.
        /// </summary>
        Standard,

        /// <summary>
        /// The PDF stream is indented to reflect the nesting levels of the objects. This is useful
        /// for analyzing PDF files, but increases the size of the file significantly.
        /// </summary>
        Indented,

        /// <summary>
        /// The PDF stream is indented to reflect the nesting levels of the objects and contains additional
        /// information about the PDFsharp objects. Furthermore, content streams are not deflated. This 
        /// is useful for debugging purposes only and increases the size of the file significantly.
        /// </summary>
        Verbose,
    }
}
