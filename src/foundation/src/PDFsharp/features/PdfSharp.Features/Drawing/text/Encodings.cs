// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Internal;
using PdfSharp.Quality;
using PdfSharp.Snippets.Drawing;
using PdfSharp.Snippets.Font;

#pragma warning disable 1591
namespace PdfSharp.Features.Drawing
{
    public class Encodings : Feature
    {
        public void Ansi()
        {
            GlobalFontSettings.ResetFontResolvers();
            GlobalFontSettings.FontResolver ??= SnippetsFontResolver.Get();

            var doc = new PdfDocument();
            //doc.PageMode = PdfPageMode.;
            doc.PageLayout = PdfPageLayout.OneColumn;
            //doc.ViewerPreferences.FitWindow = true;
            //doc.ViewerPreferences.FitWindow = true;
            var page = doc.AddPage();
            page.Orientation = PageOrientation.Portrait;
            var gfx = XGraphics.FromPdfPage(page, XGraphicsUnit.Presentation);

            new Snippets.Font.FontAnsiEncoding().RenderSnippet(gfx);

            var filename = PdfFileUtility.GetTempPdfFileName(nameof(SurrogateChars));

            SaveAndShowDocument(doc, filename);
        }

        //public override void Execute(Stream stream = null)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
