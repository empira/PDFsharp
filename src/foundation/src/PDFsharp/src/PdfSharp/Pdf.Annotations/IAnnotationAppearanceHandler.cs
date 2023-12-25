// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Pdf.Annotations
{
    public interface IAnnotationAppearanceHandler
    {
        void DrawAppearance(XGraphics gfx, XRect rect);
    }
}
