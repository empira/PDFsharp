// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Specifies how to join consecutive line or curve segments in a figure or subpath.
    /// </summary>
    public enum XLineJoin
    {
        /// <summary>
        /// Specifies a mitered join. This produces a sharp corner or a clipped corner,
        /// depending on whether the length of the miter exceeds the miter limit.
        /// </summary>
        Miter = 0,

        /// <summary>
        /// Specifies a circular join. This produces a smooth, circular arc between the lines.
        /// </summary>
        Round = 1,

        /// <summary>
        /// Specifies a beveled join. This produces a diagonal corner.
        /// </summary>
        Bevel = 2,
    }
}
