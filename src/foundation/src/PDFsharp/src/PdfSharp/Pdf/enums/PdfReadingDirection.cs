// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Specifies how the document should be displayed by a viewer when opened.
    /// </summary>
    public enum PdfReadingDirection
    {
        /// <summary>
        /// Left to right.
        /// </summary>
        LeftToRight,

        /// <summary>
        /// Right to left (including vertical writing systems, such as Chinese, Japanese, and Korean)
        /// </summary>
        RightToLeft,
    }
}