// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing.BarCodes
{
    /// <summary>
    /// Specifies the type of the bar code.
    /// </summary>
    public enum CodeType
    {
        /// <summary>
        /// The standard 2 of 5 interleaved bar code.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        Code2of5Interleaved,

        /// <summary>
        /// The standard 3 of 9 bar code.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        Code3of9Standard,

        /// <summary>
        /// The OMR code.
        /// </summary>
        Omr,

        /// <summary>
        /// The data matrix code.
        /// </summary>
        DataMatrix,
    }
}
