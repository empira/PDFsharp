// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Specifies how the interior of a closed path is filled.
    /// </summary>
    public enum XFillMode  // Same values as System.Drawing.FillMode.
    {
        /// <summary>
        /// Specifies the alternate fill mode. Called the 'odd-even rule' in PDF terminology.
        /// </summary>
        Alternate = 0,

        /// <summary>
        /// Specifies the winding fill mode. Called the 'nonzero winding number rule' in PDF terminology.
        /// </summary>
        Winding = 1,
    }
}
