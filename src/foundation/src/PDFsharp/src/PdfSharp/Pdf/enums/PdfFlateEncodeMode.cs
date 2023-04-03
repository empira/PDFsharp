// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Sets the mode for the Deflater (FlateEncoder).
    /// </summary>
    public enum PdfFlateEncodeMode
    {
        /// <summary>
        /// The default mode.
        /// </summary>
        Default,

        /// <summary>
        /// Fast encoding, but larger PDF files.
        /// </summary>
        BestSpeed,

        /// <summary>
        /// Best compression, but takes more time.
        /// </summary>
        BestCompression,
    }
}