// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Specifies the direction of a linear gradient.
    /// </summary>
    public enum XLinearGradientMode  // same values as System.Drawing.LinearGradientMode
    {
        /// <summary>
        /// Specifies a gradient from left to right.
        /// </summary>
        Horizontal = 0,

        /// <summary>
        /// Specifies a gradient from top to bottom.
        /// </summary>
        Vertical = 1,

        /// <summary>
        /// Specifies a gradient from upper left to lower right.
        /// </summary>
        ForwardDiagonal = 2,

        /// <summary>
        /// Specifies a gradient from upper right to lower left.
        /// </summary>
        BackwardDiagonal = 3,
    }
}
