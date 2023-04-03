// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing.BarCodes
{
    /// <summary>
    /// Specifies whether and how the text is displayed at the code.
    /// </summary>
    public enum TextLocation
    {
        /// <summary>
        /// No text is drawn.
        /// </summary>
        None,

        /// <summary>
        /// The text is located above the code.
        /// </summary>
        Above,

        /// <summary>
        /// The text is located below the code.
        /// </summary>
        Below,


        /// <summary>
        /// The text is located above within the code.
        /// </summary>
        AboveEmbedded,


        /// <summary>
        /// The text is located below within the code.
        /// </summary>
        BelowEmbedded,
    }
}
