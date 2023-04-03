// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing.BarCodes
{
    /// <summary>
    /// Specifies the drawing direction of the code.
    /// </summary>
    public enum CodeDirection
    {
        /// <summary>
        /// Does not rotate the code.
        /// </summary>
        LeftToRight,

        /// <summary>
        /// Rotates the code 180° at the anchor position.
        /// </summary>
        BottomToTop,

        /// <summary>
        /// Rotates the code 180° at the anchor position.
        /// </summary>
        RightToLeft,

        /// <summary>
        /// Rotates the code 180° at the anchor position.
        /// </summary>
        TopToBottom,
    }
}
