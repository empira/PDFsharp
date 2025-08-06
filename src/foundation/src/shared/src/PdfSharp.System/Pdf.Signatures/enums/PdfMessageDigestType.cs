// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// ReSharper disable InconsistentNaming

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// Specifies the algorithm used to generate the message digest.
    /// </summary>
    public enum PdfMessageDigestType
    {
        /// <summary>
        /// (PDF 1.3)
        /// </summary>
        SHA1 = 0,

        /// <summary>
        /// (PDF 1.6)
        /// </summary>
        SHA256 = 1,

        /// <summary>
        /// (PDF 1.7)
        /// </summary>
        SHA384 = 2,

        /// <summary>
        /// (PDF 1.7)
        /// </summary>
        SHA512 = 3,

        /// <summary>
        /// (PDF 1.7)
        /// </summary>
        RIPEMD160 = 4
    }
}
