// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Pdf.Advanced
{
    interface IContentStream
    {
        PdfResources Resources { get; }

        string GetFontName(XFont font, out PdfFont pdfFont);

        string GetFontName(string idName, byte[] fontData, out PdfFont pdfFont);

        string GetImageName(XImage image);

        string GetFormName(XForm form);
    }
}
