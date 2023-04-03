// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Security
{
    /// <summary>
    /// Represents the identity crypt filter, which shall be provided by a PDF processor and pass the data unchanged.
    /// </summary>
    class IdentityCryptFilter : CryptFilterBase
    {
        internal static IdentityCryptFilter Instance { get; } = new();
        
        /// <summary>
        /// Encrypts the given bytes. Returns true if the crypt filter encrypted the bytes, or false, if the security handler shall do it.
        /// </summary>
        internal override bool EncryptForEnteredObject(ref byte[] bytes)
        {
            // Nothing to do as Identity crypt filter shall pass all data unchanged.
            // Return true, so that also the security handler will not encrypt the bytes.
            return true;
        }

        /// <summary>
        /// Decrypts the given bytes. Returns true if the crypt filter decrypted the bytes, or false, if the security handler shall do it.
        /// </summary>
        internal override bool DecryptForEnteredObject(ref byte[] bytes)
        {
            // Nothing to do as Identity crypt filter shall pass all data unchanged.
            // Return true, so that also the security handler will not decrypt the bytes.
            return true;
        }
    }
}
