// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Security
{
    /// <summary>
    /// Abstract class declaring the common methods of crypt filters. These may be the IdentityCryptFilter or PdfCryptFilters defined in the CF dictionary of the security handler.
    /// </summary>
    public abstract class CryptFilterBase : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CryptFilterBase"/> class.
        /// </summary>
        protected CryptFilterBase()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptFilterBase"/> class.
        /// </summary>
        /// <param name="dict"></param>
        protected CryptFilterBase(PdfDictionary dict) : base(dict)
        { }

        /// <summary>
        /// Encrypts the given bytes. Returns true if the crypt filter encrypted the bytes, or false, if the security handler shall do it.
        /// </summary>
        internal abstract bool EncryptForEnteredObject(ref byte[] bytes);

        /// <summary>
        /// Decrypts the given bytes. Returns true if the crypt filter decrypted the bytes, or false, if the security handler shall do it.
        /// </summary>
        internal abstract bool DecryptForEnteredObject(ref byte[] bytes);
    }
}
