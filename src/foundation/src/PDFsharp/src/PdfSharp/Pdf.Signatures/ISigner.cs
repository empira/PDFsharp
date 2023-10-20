// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Signatures
{
    public interface ISigner
    {
        byte[] GetSignedCms(Stream stream, string recommendedDigestAlgorithm);

        string GetName();
    }
}
