using PdfSharp.Drawing;

namespace PdfSharp.Pdf.Signatures
{
    public interface ISignatureRenderer
    {
        void Render(XGraphics gfx, XRect rect, PdfSignatureOptions options);
    }
}
