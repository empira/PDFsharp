// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// This class is undocumented and may change or drop in future releases.
    /// </summary>
    public enum PdfCustomValueCompressionMode
    {
        /// <summary>
        /// Use document default to determine compression.
        /// </summary>
        Default,

        /// <summary>
        /// Leave custom values uncompressed.
        /// </summary>
        Uncompressed,

        /// <summary>
        /// Compress custom values using FlateDecode.
        /// </summary>
        Compressed,
    }
}