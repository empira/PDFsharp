// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// The signature dictionary added to a PDF file when it is to be signed.
    /// </summary>
    public sealed class PdfSignature : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfSignature"/> class.
        /// </summary>
        public PdfSignature()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfSignature"/> class.
        /// </summary>
        public PdfSignature(PdfDocument dict) : base(dict)
        { }

        ///// <summary>
        ///// Encrypts the given bytes. Returns true if the crypt filter encrypted the bytes, or false, if the security handler shall do it.
        ///// </summary>
        //internal abstract bool EncryptForEnteredObject(ref byte[] bytes);

        ///// <summary>
        ///// Decrypts the given bytes. Returns true if the crypt filter decrypted the bytes, or false, if the security handler shall do it.
        ///// </summary>
        //internal abstract bool DecryptForEnteredObject(ref byte[] bytes);
    }
}
