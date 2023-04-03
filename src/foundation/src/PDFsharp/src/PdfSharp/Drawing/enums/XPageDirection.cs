// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Specifies the direction of the y-axis.
    /// </summary>
    public enum XPageDirection
    {
        /// <summary>
        /// Increasing Y values go downwards. This is the default.
        /// </summary>
        Downwards = 0,

        /// <summary>
        /// Increasing Y values go upwards. This is only possible when drawing on a PDF page.
        /// It is not implemented when drawing on a System.Drawing.Graphics object.
        /// </summary>
        [Obsolete("Not implemeted - yagni")]
        Upwards = 1, // Possible, but needs a lot of case differentiation - postponed.
    }
}
