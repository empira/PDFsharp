// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Specifies whether smoothing (or anti-aliasing) is applied to lines and curves
    /// and the edges of filled areas.
    /// </summary>
    [Flags]
    public enum XSmoothingMode  // same values as System.Drawing.Drawing2D.SmoothingMode
    {
        // Not used in PDF rendering process.

        /// <summary>
        /// Specifies an invalid mode.
        /// </summary>
        Invalid = -1,

        /// <summary>
        /// Specifies the default mode.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Specifies high-speed, low-quality rendering.
        /// </summary>
        HighSpeed = 1,

        /// <summary>
        /// Specifies high-quality, low-speed rendering.
        /// </summary>
        HighQuality = 2,

        /// <summary>
        /// Specifies no anti-aliasing.
        /// </summary>
        None = 3,

        /// <summary>
        /// Specifies anti-aliased rendering.
        /// </summary>
        AntiAlias = 4,
    }
}
