// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using System.Diagnostics;
using System.Xml.Xsl;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;

#pragma warning disable 1591
namespace PdfSharp.Features.Drawing
{
    public class NotoSans : Feature
    {
        static NotoSans()
        {
            GlobalFontSettings.FontResolver ??= SnippetsFontResolver.Get();
        }

        public void Load_all_Noto_Sans()
        {
            var doc = new PdfDocument();
            doc.PageLayout = PdfPageLayout.OneColumn;

            var page = doc.AddPage();
            page.Orientation = PageOrientation.Portrait;
            var gfx = XGraphics.FromPdfPage(page, XGraphicsUnit.Presentation);

            //var filename = IOHelper.CreateTemporaryPdfFileName(nameof(Ensure_one_PdfFontDescriptor_per_FontFace));


            var root = IOUtility.GetSolutionRoot();
            Debug.Assert(root != null);
            var assets = Path.Combine(root, @"assets\fonts\Noto\Noto_Sans\static");

            var fontFiles = Directory.GetFiles(assets);

            //fontFiles = Directory.GetFiles(@"c:\windows\fonts", "*.ttf");

            Process proc = Process.GetCurrentProcess();
            var memory1 = proc.PrivateMemorySize64;
            XFont font = default!;
            XFont fontInfo = new XFont("Arial", 10, XFontStyleEx.Regular);
            int y = 100;

            // German version of "The quick brown fox..." with umlauts, sharp s and digits.
            var pangram = "Zwölf Boxkämpfer jagen Viktor quer über den großen Sylter Deich, 1234567890 +-*/";
            for (int idx = 0; idx < fontFiles.Length; idx++)
            {
                var file = fontFiles[idx];

                var source = XFontSource.CreateFromFile(file);

                var glyphTypeface = new XGlyphTypeface(source);

                font = new XFont(glyphTypeface, 10);
                if (font.IsSymbolFont)
                    gfx.DrawString($"{idx + 1:##}: (this is a symbol font)", fontInfo, XBrushes.Black, 20, y);
                else
                    gfx.DrawString($"{idx + 1:##}: {pangram}", font, XBrushes.Black, 20, y);
                gfx.DrawString($"    {font.Name}      {Style(font, glyphTypeface)}", fontInfo, XBrushes.DarkBlue, 20, y + 12);
                y += 30;

                if ((idx + 1) % 30 == 0)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page, XGraphicsUnit.Presentation);
                    y = 100;
                }
            }

            var memory2 = proc.PrivateMemorySize64;
            var diff = memory2 - memory1;

            var info = FontsDevHelper.GetFontCachesState();

            var filename = PdfFileUtility.GetTempPdfFileName(nameof(Load_all_Noto_Sans));
            SaveAndShowDocument(doc, filename);

            //_.GetSolutionRoot();
            string Style(XFont f, XGlyphTypeface glyphTypeface)
            {
                var types = FontsDevHelper.TryGetStretchAndWeight(glyphTypeface);
                return (f.Bold ? "bold " : "") + (f.Italic ? "italic" : "")
                    + $" / {types.Stretch} {types.Weight}";
            }
        }
    }
}
