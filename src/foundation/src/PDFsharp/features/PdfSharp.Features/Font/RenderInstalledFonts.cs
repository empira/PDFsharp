// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Quality;
#if GDI
using System.Drawing.Text;
#endif

#pragma warning disable 1591
namespace PdfSharp.Features.Font
{
    public class RenderInstalledFonts : Feature
    {
        public void RenderInstalledFontsTest()
        {
            RenderSnippetAsPdf(new RenderInstalledFontsSnippet());
        }

        protected static void RenderSnippetAsPdf(RenderInstalledFontsSnippet snippet)
        {
            snippet.RenderSnippetAsPdfMultiPage();
#if !UWP
            snippet.SaveAndShowFile(snippet.PdfBytes, "Test_tempfile.pdf", true);
#else
            snippet.UwpSaveAndShowFile(snippet.PdfBytes, "Test_tempfile.pdf", true);
#endif
        }
    }

    public class RenderInstalledFontsSnippet : Snippet
    {
#if true
        static readonly XPdfFontOptions PdfOptions = XPdfFontOptions.WinAnsiDefault;
#else
        private static readonly XPdfFontOptions PdfOptions = XPdfFontOptions.UnicodeDefault;
#endif
        // WPF's System.Windows.Media.Fonts.SystemFontFamilies gets less families than InstalledFontCollection().Families, because some faces are embedded in a family here, while they are not in InstalledFontCollection().Families.
        // E.g. Arial Black is included in Arial in WPF, but not in CORE and GDI. To get the same fonts set, you can manually add the fonts found by CORE and GDI here (but don't check-in the changes).
        // For the fonts that would not be found by System.Windows.Media.Fonts.SystemFontFamilies, but by the name in this list, some FontStyles may not be applied correctly.
        readonly List<string> _wpfFonts = new()
        {
        };

        const string TestString = "äöüßéèê";

        const int FontsPerPage = 5;
        const int PosX = 40;
        const int PosXTestString = 350;
        const int FontOffset = 30;
        const int FaceOffset = 20;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        /*readonly*/ List<string> _fonts = new List<string>();
        int _fontIdx = -1;

        // Fonts starting with one of the strings in _excludedFonts* are excluded. Use "/r", "/b", "/i", "/bi" to exclude only regular, bold, italic or bolditalic face (Combine it to exclude several faces: e.g. "/b/i/bi").
        readonly List<string> _excludedFonts = new List<string>();
        readonly List<string> _excludedFontsUnicode = new List<string>();
        readonly List<string> _excludedFontsWinAnsi = new List<string> { "Myriad Pro", "rotis" };

        XFont? _headingFont;
        XFont? _exceptionFont;

        //public RenderInstalledFontsSnippet(List<string> fonts)
        //{
        //    _fonts = fonts;
        //}

        public override void RenderSnippet(XGraphics gfx)
        {
            _headingFont = new XFont("Segoe UI", 10, XFontStyleEx.Underline, PdfOptions);
            _exceptionFont = new XFont("Segoe UI", 10, XFontStyleEx.Regular, PdfOptions);

            if (_fontIdx == -1)
            {
                gfx.DrawString("Call RenderSnippetMultiPage to render this Snippet", _exceptionFont, XBrushes.Black, 40, 40);
                return;
            }

            var posY = 40;

            var pageFontCutoff = _fontIdx + FontsPerPage;

            while (_fontIdx < pageFontCutoff)
            {
                if (_fontIdx == _fonts.Count)
                    break;

                var font = _fonts[_fontIdx];

                Console.WriteLine($"Rendering font {_fontIdx + 1} of {_fonts.Count}: {font}");

                gfx.DrawString($"{font}", _headingFont, XBrushes.Black, PosX, posY);
                posY += FaceOffset;

                TryDrawString(gfx, font, XFontStyleEx.Regular, PosX, posY);
                posY += FaceOffset;

                TryDrawString(gfx, font, XFontStyleEx.Bold, PosX, posY);
                posY += FaceOffset;

                TryDrawString(gfx, font, XFontStyleEx.Italic, PosX, posY);
                posY += FaceOffset;

                TryDrawString(gfx, font, XFontStyleEx.BoldItalic, PosX, posY);
                posY += FontOffset;

                _fontIdx++;
            }
        }

        void TryDrawString(XGraphics gfx, string font, XFontStyleEx style, int posX, int posY)
        {
            var styleName = Enum.GetName(typeof(XFontStyleEx), style);

            try
            {
                if (IsFontExcluded(font, style))
                    throw new Exception("This font is currently excluded in RenderInstalledFontsSnippet");

                var xFont = new XFont(font, 10, style, PdfOptions);

                gfx.DrawString($"{font} - {styleName}", xFont, XBrushes.Black, posX, posY);
                gfx.DrawString(TestString, xFont, XBrushes.Black, PosXTestString, posY);
            }
            catch (Exception e)
            {
                gfx.DrawString($"{font} - {styleName} - Exception: {e.Message}", _exceptionFont!, XBrushes.Red, posX, posY);
            }
        }

        bool IsFontExcluded(string font, XFontStyleEx style)
        {
            if (IsFontExcluded(font, style, _excludedFonts))
                return true;

            if (PdfOptions.FontEncoding == PdfFontEncoding.Unicode && IsFontExcluded(font, style, _excludedFontsUnicode))
                return true;

            if (PdfOptions.FontEncoding == PdfFontEncoding.WinAnsi && IsFontExcluded(font, style, _excludedFontsWinAnsi))
                return true;

            return false;
        }

        bool IsFontExcluded(string font, XFontStyleEx style, List<string> excludeList)
        {
            font = font.ToLower();

            foreach (var excludedFont in excludeList)
            {
                var excludedFontSplit = excludedFont.Split('/');
                var excludedFontName = excludedFontSplit[0].ToLower();

                if (!font.StartsWith(excludedFontName, StringComparison.Ordinal))
                    // This font is not excluded by excludedFont.
                    continue;

                var excludedFontStyles = excludedFontSplit.Skip(1).ToList();

                // Handle /r, /b, /i and /bi declarations.
                if (!excludedFontStyles.Any())
                    // Exclude all faces.
                    return true;
                if (style == XFontStyleEx.Regular && excludedFontStyles.Any(x => x == "r"))
                    return true;
                if (style == XFontStyleEx.Bold && excludedFontStyles.Any(x => x == "b"))
                    return true;
                if (style == XFontStyleEx.Italic && excludedFontStyles.Any(x => x == "i"))
                    return true;
                if (style == XFontStyleEx.BoldItalic && excludedFontStyles.Any(x => x == "bi"))
                    return true;
            }

            return false;
        }

        public void RenderSnippetMultiPage()
        {
#if GDI
            using (var fontsCollection = new InstalledFontCollection())
            {
                _fonts = fontsCollection.Families.Select(x => x.Name).Where(x => !String.IsNullOrEmpty(x)).ToList();
            }
#endif
#if WPF
            _fonts = _wpfFonts.Any()
                // Use the manual font list, if it contains font names.
                ? _wpfFonts
                // Get the font list by WPF otherwise.
                : System.Windows.Media.Fonts.SystemFontFamilies.Select(x => x.Source).ToList();
#endif

            _fontIdx = 0;

            var pageCount = (int)Math.Ceiling(_fonts.Count / (double)FontsPerPage);

            for (var i = 0; i < pageCount; i++)
            {
                if (Page != null)
                    EndPdfPage();
                BeginPdfPage();
                RenderSnippet();
                DrawPdfHeader();
            }
        }

        public void RenderSnippetAsPdfMultiPage()
        {
            BeginPdfDocument();
            RenderSnippetMultiPage();
            //DrawPdfHeader();
            EndPdfDocument();
        }
    }
}
