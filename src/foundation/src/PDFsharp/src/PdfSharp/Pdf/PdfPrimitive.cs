// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Base class for all types that cannot be indirect PDF objects.
    /// All primitive types are immutable and can be used multiple times.
    /// Therefore, e.g. for strings exists PdfString and PdfStringObject.
    /// </summary>
    public abstract class PdfPrimitive : PdfItem
    {
        // All derived primitive types are immutable.

        /// <summary>
        /// Initialized a new instance of this class.
        /// </summary>
        protected PdfPrimitive()
        { }

        /// <summary>
        /// Initialized a new instance of this class.
        /// </summary>
        protected PdfPrimitive(PdfItem item) : base(item)
        { }


#if PRESERVE_PARSED_VALUES
        /// <summary>
        /// Gets or sets the byte string that was originally read from the lexer.
        /// </summary>
        internal string? ParsedValue { get; set; }
#endif
    }
}
