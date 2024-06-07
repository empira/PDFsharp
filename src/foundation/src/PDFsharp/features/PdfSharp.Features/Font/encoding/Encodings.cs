// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Internal;
using PdfSharp.Quality;
using PdfSharp.Snippets.Drawing;
using PdfSharp.Snippets.Font;

#pragma warning disable 1591
namespace PdfSharp.Features.Font
{
    public class Encodings : Feature
    {
        public void AnsiEncodingTable()
        {
            PdfSharpCore.ResetAll();

            var doc = new PdfDocument();
            doc.PageLayout = PdfPageLayout.OneColumn;
            var page = doc.AddPage();
            page.Orientation = PageOrientation.Portrait;
            var gfx = XGraphics.FromPdfPage(page, XGraphicsUnit.Presentation);

            var snippet = new FontAnsiEncoding();
            snippet.RenderSnippet(gfx);

            var filename = PdfFileUtility.GetTempPdfFileName(snippet.PathName);

            SaveAndShowDocument(doc, filename);
        }
    }
}
