// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing.Pdf
{
    /// <summary>
    /// Indicates whether we are within a BT/ET block.
    /// </summary>
    enum StreamMode
    {
        /// <summary>
        /// Graphic mode. This is default.
        /// </summary>
        Graphic,

        /// <summary>
        /// Text mode.
        /// </summary>
        Text,
    }
}
