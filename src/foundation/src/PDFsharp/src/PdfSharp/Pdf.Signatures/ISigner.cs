
namespace PdfSharp.Pdf.Signatures
{
    public interface ISigner
    {
        byte[] GetSignedCms(Stream documentStream, PdfDocument document);
        byte[] GetSignedCms(byte[] range, PdfDocument document);

        string? GetName();
    }
}
