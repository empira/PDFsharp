// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// Interface for classes that generate digital signatures.
    /// </summary>
    public interface IDigitalSigner
    {
        /// <summary>
        /// Gets a human-readable name of the used certificate.
        /// </summary>
        string CertificateName { get; }

        /// <summary>
        /// Gets the size of the signature in bytes.
        /// The size is used to reserve space in the PDF file that is later filled with the signature.
        /// </summary>
        Task<int> GetSignatureSizeAsync();

        /// <summary>
        /// Gets the signatures of the specified stream.
        /// </summary>
        /// <param name="stream"></param>
        Task<byte[]> GetSignatureAsync(Stream stream);
    }
}
