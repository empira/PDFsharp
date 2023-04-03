// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Specifies how different clipping regions can be combined.
    /// </summary>
    public enum XCombineMode  // Same values as System.Drawing.Drawing2D.CombineMode.
    {
        /// <summary>
        /// One clipping region is replaced by another.
        /// </summary>
        Replace = 0,

        /// <summary>
        /// Two clipping regions are combined by taking their intersection.
        /// </summary>
        Intersect = 1,

        /// <summary>
        /// Not yet implemented in PDFsharp.
        /// </summary>
        Union = 2,

        /// <summary>
        /// Not yet implemented in PDFsharp.
        /// </summary>
        Xor = 3,

        /// <summary>
        /// Not yet implemented in PDFsharp.
        /// </summary>
        Exclude = 4,

        /// <summary>
        /// Not yet implemented in PDFsharp.
        /// </summary>
        Complement = 5,
    }
}
