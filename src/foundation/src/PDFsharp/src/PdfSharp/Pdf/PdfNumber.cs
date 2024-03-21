// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Base class for direct number values (not yet used, maybe superfluous).
    /// </summary>
    public abstract class PdfNumber : PdfItem
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is a 32-bit signed integer.
        /// </summary>
        public bool IsInteger { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a 64-bit signed integer.
        /// </summary>
        public bool IsLongInteger { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a floating point number.
        /// </summary>
        public bool IsReal { get; protected set; }
    }
}
