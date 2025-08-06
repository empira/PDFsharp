// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Security
{
    /// <summary>
    /// Typical settings to initialize encryption with.
    /// With PdfDefaultEncryption, the encryption can be set automized using PdfStandardSecurityHandler.SetPermission() with one single parameter.
    /// </summary>
    public enum PdfDefaultEncryption
    {
        /// <summary>
        /// Do not encrypt the PDF file.
        /// </summary>
        None,

        /// <summary>
        /// Use V4UsingAES, the most recent encryption method not requiring PDF 2.0.
        /// </summary>
        Default,

        /// <summary>
        /// Encrypt with Version 1 (RC4 and a file encryption key length of 40 bits).
        /// </summary>
        V1,

        /// <summary>
        /// Encrypt with Version 2 (RC4 and a file encryption key length of more than 40 bits, PDF 1.4) with a file encryption key length of 40 bits.
        /// </summary>
        V2With40Bits,

        /// <summary>
        /// Encrypt with Version 2 (RC4 and a file encryption key length of more than 40 bits, PDF 1.4) with a file encryption key length of 128 bits.
        /// This was the default encryption in PDFsharp 1.5.
        /// </summary>
        V2With128Bits,

        /// <summary>
        /// Encrypt with Version 4 (RC4 or AES and a file encryption key length of 128 bits using a crypt filter, PDF 1.5) using RC4.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        V4UsingRC4,

        /// <summary>
        /// Encrypt with Version 4 (RC4 or AES and a file encryption key length of 128 bits using a crypt filter, PDF 1.5) using AES (PDF 1.6).
        /// </summary>
        V4UsingAES,

        /// <summary>
        /// Encrypt with Version 5 (AES and a file encryption key length of 256 bits using a crypt filter, PDF 2.0).
        /// </summary>
        V5
    }
}
