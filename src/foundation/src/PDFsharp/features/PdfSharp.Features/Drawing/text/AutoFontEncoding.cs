// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Fonts.Internal;
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Internal;
using PdfSharp.Quality;
using PdfSharp.Snippets.Drawing;
using PdfSharp.Snippets.Font;

#pragma warning disable 1591
namespace PdfSharp.Features.Drawing
{
    public class AutoFontEncoding : Feature
    {
        public void Ensure_one_PdfFontDescriptor_per_FontFace()
        {
            var doc = new PdfDocument();
            doc.Info.Subject = "s xxxöäüß \U00029C9Cあ";
            //doc.Info.Owner = "s xxxöäüß \U00029C9Cあ";
            doc.PageLayout = PdfPageLayout.OneColumn;

            doc.RenderEvents.RenderTextEvent += (sender, args) =>
            {
                var length = args.CodePointGlyphIndexPairs.Length;
                for (var idx = 0; idx < length; idx++)
                {
                    ref var item = ref args.CodePointGlyphIndexPairs[idx];
                    
                    // Replace X with U.
                    if (item.CodePoint == 'X')
                    {
#if true_
                        var id = GlyphHelper.CodePointToGlyphIndex(args.Font, 'U');
                        args.CodeRun.Items[idx] = new('U', 'U', id);
#else
                        args.CodePointGlyphIndexPairs[idx].CodePoint = 'U';
                        item.CodePoint = 'U';
                        args.ReevaluateGlyphIndices = true;
#endif
                    }

                    if (item.CodePoint == '\u2011')
                    {
                        item.CodePoint = '-';
                        item.GlyphIndex = GlyphHelper.GlyphIndexFromCodePoint('-', args.Font);
                    }
                }
            };

            var page = doc.AddPage();
            page.Orientation = PageOrientation.Portrait;
            var gfx = XGraphics.FromPdfPage(page, XGraphicsUnit.Presentation);

            var fontAnsi = new XFont("Arial", 15, XFontStyleEx.Regular | XFontStyleEx.Underline, XPdfFontOptions.WinAnsiDefault);
            var fontAnsiBold = new XFont("Arial", 15, XFontStyleEx.Bold | XFontStyleEx.Underline, XPdfFontOptions.WinAnsiDefault);
            var fontUnicode = new XFont("Arial", 15, XFontStyleEx.Regular | XFontStyleEx.Underline, XPdfFontOptions.UnicodeDefault);
            var fontUnicodeBold = new XFont("Arial", 15, XFontStyleEx.Bold | XFontStyleEx.Underline, XPdfFontOptions.UnicodeDefault);

            string someText = "A XX \u2011 α";
            //someText = "\u2011";
            //var l1 = gfx.MeasureString(someText, fontAnsi);
            //var l2 = gfx.MeasureString(someText, fontUnicode);

            gfx.DrawString(someText, fontAnsi, XBrushes.DarkGreen, 100, 200);
            gfx.DrawString(someText, fontAnsiBold, XBrushes.DarkGreen, 100, 230);
            gfx.DrawString(someText, fontUnicode, XBrushes.DarkBlue, 100, 300);
            gfx.DrawString(someText, fontUnicodeBold, XBrushes.DarkBlue, 100, 330);

            //gfx.DrawString("A XX \u00a9 α \ud83c\udf39", fontAnsi, XBrushes.DarkGreen, 100, 200);
            //gfx.DrawString("U XX \u00a9 α \ud83c\udf39", fontUnicode, XBrushes.DarkBlue, 100, 300);

            //_ = "Ä ä Ö ö ß Ü ü | „“ ’ ‚‘ »« ›‹ – | · × ² ³ ½ € † … | ✔ ✘ ↯ ± − × ÷ ⋅ √ ≠ ≤ ≥ ≡ | ® © ← ↑ → ↓ ↔ ↕ ∅ |";

            //gfx.DrawString("Ä ä Ö ö ß Ü ü | „“ ’ ‚‘ »« ›‹ – | · × ² ³ ½ € † … | ✔ ✘ ↯ ± − × ÷ ⋅ √ ≠ ≤ ≥ ≡ | ® © ← ↑ → ↓ ↔ ↕ ∅ |", fontAnsi, XBrushes.DarkGreen, 100, 500);
            //gfx.DrawString("Ä ä Ö ö ß Ü ü | „“ ’ ‚‘ »« ›‹ – | · × ² ³ ½ € † … | ✔ ✘ ↯ ± − × ÷ ⋅ √ ≠ ≤ ≥ ≡ | ® © ← ↑ → ↓ ↔ ↕ ∅ |", fontUnicode, XBrushes.DarkBlue, 100, 550);

            var content = FontsDevHelper.GetFontCachesState();
            
            var filename = PdfFileUtility.GetTempPdfFileName(nameof(Ensure_one_PdfFontDescriptor_per_FontFace));

            SaveAndShowDocument(doc, filename);
        }

        public void Test2()
        {
            var doc = new PdfDocument();
            doc.Info.Subject = "s xxxöäüß \U00029C9Cあ";
            //doc.Info.Owner = "s xxxöäüß \U00029C9Cあ";
            doc.PageLayout = PdfPageLayout.OneColumn;

            doc.RenderEvents.RenderTextEvent += (sender, args) =>
            {
                // Test if a copy also works
                //args.CodeRun.Items = (CodePointGlyphIndex[])args.CodeRun.Items.Clone();

                var length = args.CodePointGlyphIndexPairs.Length;
                for (var idx = 0; idx < length; idx++)
                {
                    ref var item = ref args.CodePointGlyphIndexPairs[idx];
                    if (item.CodePoint == 'X')
                    {
#if true_
                        var id = GlyphHelper.CodePointToGlyphIndex(args.Font, 'U');
                        args.CodeRun.Items[idx] = new('U', 'U', id);
#else
                        item.CodePoint = 'U';
                        //args.CodeRun.Items[idx].CodePoint = 'U';
                        //args.CodeRun.Items[idx].GlyphIndex = GlyphHelper.CodePointToGlyphIndex(args.Font, 'U');
                        args.ReevaluateGlyphIndices = true;
#endif
                    }

                    //if (item.Character == '\u2011')
                    //{
                    //    args.CodeRun.Items[idx].Character = '-';
                    //    args.CodeRun.Items[idx].CodePoint = '-';
                    //    args.CodeRun.Items[idx].GlyphIndex = GlyphHelper.CodePointToGlyphID(args.Font, '-');
                    //}
                }
            };

            var page = doc.AddPage();
            page.Orientation = PageOrientation.Portrait;
            var gfx = XGraphics.FromPdfPage(page, XGraphicsUnit.Presentation);

            //var fontAnsi = new XFont("Arial", 25, XFontStyleEx.Regular | XFontStyleEx.Underline, XPdfFontOptions.WinAnsiDefault);
            //var fontUnicode = new XFont("Arial", 25, XFontStyleEx.Regular | XFontStyleEx.Underline, XPdfFontOptions.UnicodeDefault);
            var font = new XFont("Arial", 25, XFontStyleEx.Regular | XFontStyleEx.Underline, XPdfFontOptions.AutomaticEncoding);

            const string someText1 = "A XX \u2011 a";
            const string someText2 = "A XX \u2011 α";
            const string someText3 = "A XX \u2011 α";
            _ = someText1;
            _ = someText2;
            _ = someText3;
            //var l1 = gfx.MeasureString(someText, fontAnsi);
            //var l2 = gfx.MeasureString(someText, fontUnicode);

            gfx.DrawString(someText1, font, XBrushes.DarkGreen, 100, 200);
            gfx.DrawString(someText2, font, XBrushes.DarkGreen, 100, 300);

            //gfx.DrawString("A XX \u00a9 α \ud83c\udf39", fontAnsi, XBrushes.DarkGreen, 100, 200);
            //gfx.DrawString("U XX \u00a9 α \ud83c\udf39", fontUnicode, XBrushes.DarkBlue, 100, 300);

            _ = "Ä ä Ö ö ß Ü ü | „“ ’ ‚‘ »« ›‹ – | · × ² ³ ½ € † … | ✔ ✘ ↯ ± − × ÷ ⋅ √ ≠ ≤ ≥ ≡ | ® © ← ↑ → ↓ ↔ ↕ ∅ |";

            //gfx.DrawString("Ä ä Ö ö ß Ü ü | „“ ’ ‚‘ »« ›‹ – | · × ² ³ ½ € † … | ✔ ✘ ↯ ± − × ÷ ⋅ √ ≠ ≤ ≥ ≡ | ® © ← ↑ → ↓ ↔ ↕ ∅ |", fontAnsi, XBrushes.DarkGreen, 100, 500);
            //gfx.DrawString("Ä ä Ö ö ß Ü ü | „“ ’ ‚‘ »« ›‹ – | · × ² ³ ½ € † … | ✔ ✘ ↯ ± − × ÷ ⋅ √ ≠ ≤ ≥ ≡ | ® © ← ↑ → ↓ ↔ ↕ ∅ |", fontUnicode, XBrushes.DarkBlue, 100, 600);

            var filename = PdfFileUtility.GetTempPdfFileName(nameof(SurrogateChars));

            SaveAndShowDocument(doc, filename);
        }

        public void MeasureString_Test()
        {
            var doc = new PdfDocument();
            doc.Info.Subject = "s xxxöäüß \U00029C9Cあ";  // BUG in encoding UTF-16
            doc.PageLayout = PdfPageLayout.SinglePage;

            var page = doc.AddPage();
            page.Orientation = PageOrientation.Portrait;
            var gfx = XGraphics.FromPdfPage(page, XGraphicsUnit.Presentation);

            //var fontAnsi = new XFont("Arial", 25, XFontStyleEx.Regular | XFontStyleEx.Underline, XPdfFontOptions.WinAnsiDefault);
            //var fontUnicode = new XFont("Arial", 25, XFontStyleEx.Regular | XFontStyleEx.Underline, XPdfFontOptions.UnicodeDefault);
            var fontCaption = new XFont("Arial", 27, XFontStyleEx.Bold);
            var fontAuto = new XFont("Arial", 25, XFontStyleEx.Regular | XFontStyleEx.Underline, XPdfFontOptions.AutomaticEncoding);
            var fontAnsi = new XFont("Arial", 25, XFontStyleEx.Regular | XFontStyleEx.Underline, XPdfFontOptions.WinAnsiDefault);
            var fontUnicode = new XFont("Arial", 25, XFontStyleEx.Regular | XFontStyleEx.Underline, XPdfFontOptions.UnicodeDefault);

            //_ = "Ä ä Ö ö ß Ü ü | „“ ’ ‚‘ »« ›‹ – | · × ² ³ ½ € † … | ✔ ✘ ↯ ± − × ÷ ⋅ √ ≠ ≤ ≥ ≡ | ® © ← ↑ → ↓ ↔ ↕ ∅ |";

            var copyright = "\u00a9|©"; // © Unicode code point is identical with ANSI code.
            var euro = "\u20ac|€";      // € Unicode code point is different from ANSI code.
            var phi = "\u03c8|ψ";       // ψ Unicode code point has no ANSI counterpart.
            // Surrogate char
            string[] texts =
            [
                "Copyright (ANSI): " + copyright + "   XXX",
                "Euro (ANSI): " + euro + "   XXX",
                "Phi (not ANSI): " + phi + "   XXX",
            ];

            int x = 100;
            int y = 200;
            int dy = 30;

            gfx.DrawString("Automatic font selection", fontCaption, XBrushes.Black, x, y);
            foreach (var text in texts)
            {
                var width1 = gfx.MeasureString(text, fontAuto).Width;
                y += dy;
                gfx.DrawString(text, fontAuto, XBrushes.Black, x, y);
                gfx.DrawString("XXX", fontAuto, XBrushes.DarkBlue, x + width1, y);
            }
            y += 200;

            gfx.DrawString("ANSI font", fontCaption, XBrushes.Black, x, y);
            foreach (var text in texts)
            {
                var width = gfx.MeasureString(text, fontAnsi).Width;
                y += dy;
                gfx.DrawString(text, fontAnsi, XBrushes.Black, x, y);
                gfx.DrawString("XXX", fontAnsi, XBrushes.DarkBlue, x + width, y);
            }
            y += 200;

            gfx.DrawString("CID font", fontCaption, XBrushes.Black, x, y);
            foreach (var text in texts)
            {
                var width = gfx.MeasureString(text, fontUnicode).Width;
                y += dy;
                gfx.DrawString(text, fontUnicode, XBrushes.Black, x, y);
                gfx.DrawString("XXX", fontUnicode, XBrushes.DarkBlue, x + width, y);
            }

            var filename = PdfFileUtility.GetTempPdfFileName(nameof(MeasureString_Test));
            SaveAndShowDocument(doc, filename);
        }
    }
}
