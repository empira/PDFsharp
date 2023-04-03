// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Specifies the style of dashed lines drawn with an XPen object.
    /// </summary>
    public enum XDashStyle  // Same values as System.Drawing.Drawing2D.DashStyle.
    {
        /// <summary>
        /// Specifies a solid line.
        /// </summary>
        Solid = 0,

        /// <summary>
        /// Specifies a line consisting of dashes.
        /// </summary>
        Dash = 1,

        /// <summary>
        /// Specifies a line consisting of dots.
        /// </summary>
        Dot = 2,

        /// <summary>
        /// Specifies a line consisting of a repeating pattern of dash-dot.
        /// </summary>
        DashDot = 3,

        /// <summary>
        /// Specifies a line consisting of a repeating pattern of dash-dot-dot.
        /// </summary>
        DashDotDot = 4,

        /// <summary>
        /// Specifies a user-defined custom dash style.
        /// </summary>
        Custom = 5,
    }
}
