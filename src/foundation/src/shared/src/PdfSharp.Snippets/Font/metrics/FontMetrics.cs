// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Resources;
using PdfSharp.Drawing;
using PdfSharp.Quality;
#if GDI
using System.Drawing;
using System.Drawing.Text;
using GdiFont = System.Drawing.Font;
#endif
#if WPF
using System.Windows.Markup;
using System.Windows.Media;
#endif

namespace PdfSharp.Snippets.Font
{
    public class FontMetrics : Feature
    {
        public static void TestFontMetrics()
        {
            var fonts = new List<string> { "Segoe WP", "Calibri", "Arial", "Times New Roman", "Segoe UI", "Consolas", "Verdana" };
            var systemFonts = new List<string>();
            var skippedFonts = new List<string>();

            // Load all system fonts.
#if GDI
            var installedFontCollection = new InstalledFontCollection();
            foreach (var fontFamily in installedFontCollection.Families)
                systemFonts.Add(fontFamily.Name);
#endif
#if WPF
            var systemFontFamilies = System.Windows.Media.Fonts.SystemFontFamilies;
            foreach (var fontFamily in systemFontFamilies)
            {
                fontFamily.FamilyNames.TryGetValue(XmlLanguage.GetLanguage("de-de"), out var german);
                fontFamily.FamilyNames.TryGetValue(XmlLanguage.GetLanguage("en-us"), out var english);
                var en = fontFamily.FamilyNames.Values.GetEnumerator();
                en.MoveNext();
                var first = en.Current;
                en.Dispose();
                var name = !String.IsNullOrEmpty(german)
                    ? german
                    : !String.IsNullOrEmpty(english)
                        ? english
                        : first;
                if (String.IsNullOrEmpty(name))
                    continue;
                systemFonts.Add(name);
            }
#endif

            // Check fonts.
#if true // Check all system fonts.
            bool allFonts = true;
            foreach (var fontName in systemFonts)
#else // Check only list "fonts".
            bool allFonts = false;
            foreach (var fontName in fonts)
#endif
            {
                var emHeightPix = 16;
                XFont xFont;
                try
                {
                    xFont = new XFont(fontName, emHeightPix);
                }
                catch (Exception)
                {
                    if (!allFonts)
                        throw;
                    else // Suppress exceptions, if checking all system fonts, because of font containers, that are not supported. Add font to skippedFonts instead.
                    {
                        skippedFonts.Add(fontName);
                        continue;
                    }
                }

                // Basic PdfSharp metrics in xFont.Metrics.
                var xEmHeight = xFont.Metrics.UnitsPerEm;
                var xAscent = xFont.Metrics.Ascent;
                var xDescent = xFont.Metrics.Descent;
                var xLineSpacing = xFont.Metrics.LineSpacing;

                // Basic PdfSharp metrics from xFonts.FontFamily methods.
                var xfEmHeight = xFont.FontFamily.GetEmHeight(xFont.Style);
                var xfAscent = xFont.FontFamily.GetCellAscent(xFont.Style);
                var xfDescent = xFont.FontFamily.GetCellDescent(xFont.Style);
                var xflineSpacing = xFont.FontFamily.GetLineSpacing(xFont.Style);

                Debug.Assert(xEmHeight == xfEmHeight);
                Debug.Assert(xAscent == xfAscent);
                Debug.Assert(xDescent == xfDescent);
                Debug.Assert(xLineSpacing == xflineSpacing);

                // Calculate and check metrics relationships.
                var xCellHeight = xAscent + xDescent;
                var xInternalLeading = xCellHeight - xEmHeight;
                var xExternalLeading = xLineSpacing - xCellHeight;

                Debug.Assert(xLineSpacing == xCellHeight + xExternalLeading);
                Debug.Assert(xCellHeight == xInternalLeading + xEmHeight);
                Debug.Assert(xCellHeight == xAscent + xDescent);

                // Get design unit to pixel factor.
                var xEmHeightPix = xFont.Size;
                Debug.Assert(Math.Abs(xEmHeightPix - (xFont.Size)) < 0.00001);
                var xDesignToPixels = xEmHeightPix / xEmHeight;

                // Calculate Metrics in pixels and check relationships.
                var xAscentPix = xAscent * xDesignToPixels;
                var xDescentPix = xDescent * xDesignToPixels;
                var xCellHeightPix = xCellHeight * xDesignToPixels;
                var xInternalLeadingPix = xInternalLeading * xDesignToPixels;
                var xExternalLeadingPix = xExternalLeading * xDesignToPixels;
                var xLineSpacingPix = xLineSpacing * xDesignToPixels;

                Debug.Assert(Math.Abs(xLineSpacingPix - (xCellHeightPix + xExternalLeadingPix)) < 0.00001);
                Debug.Assert(Math.Abs(xCellHeightPix - (xInternalLeadingPix + xEmHeightPix)) < 0.00001);
                Debug.Assert(Math.Abs(xCellHeightPix - (xAscentPix + xDescentPix)) < 0.00001);

                // Returns the line spacing, in pixels, of this font. The line spacing is the vertical distance
                // between the base lines of two consecutive lines of text. Thus, the line spacing includes the
                // blank space between lines along with the height of the character itself.
                //   The Font::GetHeight method gets the line spacing, in pixels, of this font. 
                //   The line spacing is the vertical distance between the base lines of two consecutive lines of text.
                //   Thus, the line spacing includes the blank space between lines along with the height of the character itself.
                //   https://msdn.microsoft.com/en-us/library/windows/desktop/ms534437%28v=vs.85%29.aspxvar lineSpacing = xfont.GetHeight();
                var xGetLineSpacingPix = xFont.GetHeight();

                // xFont.GetHeight should match the calculated line spacing in pixels.
                Debug.Assert(Math.Abs(xLineSpacingPix - xGetLineSpacingPix) < 0.00001);

#if !WPF && GDI
                // GDI+ comparision.
                var font = new GdiFont(fontName, 16);

                // basic GDI+ metrics from font.FontFamily methods.
                var fEmHeight = font.FontFamily.GetEmHeight(FontStyle.Regular);
                var fAscent = font.FontFamily.GetCellAscent(FontStyle.Regular);
                var fDescent = font.FontFamily.GetCellDescent(FontStyle.Regular);
                var fLineSpacing = font.FontFamily.GetLineSpacing(FontStyle.Regular);

                // GDI+ font.Size is the fonts EmHeight in pixels.
                var fEmHeightPix = font.Size;

                // PdfSharp values should match GDI+ values
                // Known divergences:
                // Aldhabi, DokChampa, Gabriola: Fonts whose line spacings are calculated by Typo Metrics. The TypoGap is not added here (in difference to WPF).
                // Helvetica Neue: Ascender is 967 instead of 952 for unknown reason.
                Debug.Assert(fEmHeight == xEmHeight);
                Debug.Assert(fAscent == xAscent);
                Debug.Assert(fDescent == xDescent);
                Debug.Assert(fLineSpacing == xLineSpacing);
                Debug.Assert(Math.Abs(fEmHeightPix - xEmHeightPix) < 0.00001);
#endif
#if WPF
                // WPF comparison. No divergences should occur here.
                var fontFamily = new FontFamily(fontName);
                var wLineSpacing = (int)Math.Round(fontFamily.LineSpacing * xEmHeight);

                Debug.Assert(Math.Abs(wLineSpacing - xLineSpacing) < 0.00001);
#endif
            }

            Debug.Assert(skippedFonts.Count == 0);
        }
    }
}
