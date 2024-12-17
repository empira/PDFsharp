// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal;

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// PDFsharp cryptography message IDs.
    /// </summary>
    // GPT 4 recommends to use Crypto instead of Cry as abbreviation,
    // because Cry is too ambiguous.
    enum PsCryptoMsgId
    {
        None = 0,

        // ----- Signature messages ----------------------------------------------------------------

        SampleMessage1 = MessageIdOffset.PsCrypto,
    }
}
