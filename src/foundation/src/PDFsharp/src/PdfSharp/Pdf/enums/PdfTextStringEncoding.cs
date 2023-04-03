// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// ReSharper disable InconsistentNaming

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Specifies how text strings are encoded. A text string is any text used outside of a page content 
    /// stream, e.g. document information, outline text, annotation text etc.
    /// </summary>
    public enum PdfTextStringEncoding
    {
        /// <summary>
        /// Specifies that hypertext uses PDF DocEncoding.
        /// </summary>
        PDFDocEncoding = 0,

        /// <summary>
        /// Specifies that hypertext uses Unicode encoding.
        /// </summary>
        Unicode = 1,
    }
}
