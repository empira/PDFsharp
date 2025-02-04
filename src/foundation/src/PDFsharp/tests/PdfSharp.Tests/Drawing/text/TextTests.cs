// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Fonts.Internal;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests.Drawing
{
    public static class FontHelper
    {
        /// <summary>
        /// Checks if a font has glyphs for all characters in a string.
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        public static bool HasGlyph(this XFont font, string text)
        {
            var codePoints = font.IsSymbolFont
                ? UnicodeHelper.SymbolCodePointsFromString(text, font.OpenTypeDescriptor)
                : UnicodeHelper.Utf32FromString(text /*, font.AnsiEncoding*/);
            var otDescriptor = font.OpenTypeDescriptor;
            var codePointsWithGlyphIndices = otDescriptor.GlyphIndicesFromCodePoints(codePoints);
            bool hasGlyph = codePointsWithGlyphIndices.Length > 0;
            foreach (var item in codePointsWithGlyphIndices)
            {
                if (item.GlyphIndex == 0)
                    return false;
            }

            return hasGlyph;
        }
    }

    [Collection("PDFsharp")]
    public class TextTests : IDisposable
    {
        public TextTests()
        {
            GlobalFontSettings.ResetFontManagement();
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
        }

        public void Dispose()
        {
            GlobalFontSettings.ResetFontManagement();
        }

        [Fact]
        public void PDF_with_Emojis()
        {
            // Create a new PDF document.
            var document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";
            document.Info.Author = "111😢😞💪";
            document.Info.Subject = "111😢😞💪";

            // Create an empty page in this document.
            var page = document.AddPage();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

            var width = page.Width.Point;
            var height = page.Height.Point;

            //var options = new XPdfFontOptions(PdfFontEncoding.Unicode);
            var options = new XPdfFontOptions(PdfFontColoredGlyphs.Version0);
            var font = new XFont(UnitTestFontResolver.EmojiFont, 12, XFontStyleEx.Regular, options);
            gfx.DrawString("Colored 😍🎈🍕🚲🤑💪💕", font, XBrushes.Black, new XRect(0, 0, width, height), XStringFormats.Center);
            gfx.DrawString("glyphs  \ud83d\udca9\ud83d\udc1b\ud83e\udd84\u2615\ud83d\ude82\ud83d\udef8\u2714", font, XBrushes.Black, new XRect(0, 20, width, height), XStringFormats.Center);
            gfx.DrawString("\ud83d\udca9\ud83d\udca9\ud83d\udca9\u2713\u2714\u2705\ud83d\udc1b\ud83d\udc4c\ud83c\udd97\ud83d\udd95 \ud83e\udd84 \ud83e\udd82 \ud83c\udf47 \ud83c\udf46 \u2615 \ud83d\ude82 \ud83d\udef8 \u2601 \u2622 \u264c \u264f \u2705 \u2611 \u2714 \u2122 \ud83c\udd92 \u25fb", font, XBrushes.Black, new XRect(0, 100, width, height), XStringFormats.Center);

            // Save the document...
            string filename = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTests/Drawing/text/HelloEmoji");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Fact]
        public void PDF_with_No_Break_Hyphen()
        {
            // Create a new PDF document.
            var document = new PdfDocument();
            document.Options.CompressContentStreams = false;

            document.RenderEvents.RenderTextEvent += (sender, args) =>
            {
                var length = args.CodePointGlyphIndexPairs.Length;
                for (var idx = 0; idx < length; idx++)
                {
                    ref var item = ref args.CodePointGlyphIndexPairs[idx];
                    if (item.CodePoint is '\u2011')
                    {
                        item.CodePoint = '-';
                        args.ReevaluateGlyphIndices = true;
                    }
                }
            };

            // Create an empty page in this document.
            var page = document.AddPage();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

            var options = new XPdfFontOptions(PdfFontEncoding.Unicode);
            var font = new XFont("Arial", 12, XFontStyleEx.Bold, options);
            gfx.DrawString("No\u2011break\u2011hyphen-Test", font, XBrushes.Black, new XRect(0, 50, page.Width.Point, page.Height.Point), XStringFormats.Center);

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("PdfWithNoBreakHyphen");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);

            // Analyze the drawn text in the PDF’s content stream.
            var streamEnumerator = PdfFileHelper.GetPageContentStreamEnumerator(document, 0);

            streamEnumerator.Text.MoveAndGetNext(true, out var textInfo).Should().BeTrue();
            textInfo!.IsHex.Should().BeTrue();
            var hexString = textInfo.Text;
            hexString.Should().NotBeNull();

            var glyphIds = PdfFileHelper.GetHexStringAsGlyphIndices(hexString);
            glyphIds.Should().NotContain("0", "no char (and no no-break hyphen) should be converted to an invalid glyph (\"0\")");
        }

        [Fact/*(Skip = "Not yet working")*/]
        public void PDF_with_Wingdings()
        {
            // Create a new PDF document.
            var document = new PdfDocument();
            document.Options.CompressContentStreams = false;

            var containsNotFoundGlyphs = false;

            document.RenderEvents.RenderTextEvent += (sender, args) =>
            {
                var length = args.CodePointGlyphIndexPairs.Length;
                for (var idx = 0; idx < length; idx++)
                {
                    ref var item = ref args.CodePointGlyphIndexPairs[idx];

                    // Check for not found glyphs.
                    if (item.GlyphIndex == 0)
                        containsNotFoundGlyphs = true;
                }
            };

            // Create an empty page in this document.
            var page = document.AddPage();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

            var options = new XPdfFontOptions(PdfFontEncoding.Unicode);
            var font = new XFont("Arial", 12, XFontStyleEx.Bold, options);
            gfx.DrawString("1 þ", font, XBrushes.Black, new XRect(50, 50, 20, 20), XStringFormats.TopLeft);

            if (!PdfSharp.Capabilities.Build.IsCoreBuild)
            {
                font = new XFont("Wingdings", 12, XFontStyleEx.Regular, options);
                gfx.DrawString("1 þ", font, XBrushes.Black, new XRect(50, 100, 20, 20), XStringFormats.Center);
            }

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("PdfWithWingdings");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);

            containsNotFoundGlyphs.Should().BeFalse();
        }

        [Fact]
        public void PDF_with_ligatures_text_event()
        {
            bool enableLigatures = false;

            Dictionary<string, char> ligatures = new()
            {
                { "ffi", 'ﬃ' },
                { "ffl", 'ﬄ' },
                { "ff", 'ﬀ' },
                { "fi", 'ﬁ' },
                { "fl", 'ﬂ' },
                { "IJ", 'Ĳ' },
                { "ij", 'ĳ' }
            };

            // Create a new PDF document.
            var document = new PdfDocument();
            document.Options.CompressContentStreams = false;

            document.RenderEvents.PrepareTextEvent += (sender, args) =>
            {
                // ReSharper disable once AccessToModifiedClosure
                if (enableLigatures)
                {
#if true
                    //bool hits = false;
                    var preparedText = args.Text;
                    var font = args.Font;
                    foreach (var ligature in ligatures)
                    {
                        if (font.HasGlyph(ligature.Value.ToString()))
                        {
                            if (preparedText.IndexOf(ligature.Key, StringComparison.Ordinal) >= 0)
                            {
                                preparedText = preparedText.Replace(ligature.Key, ligature.Value.ToString());
                                //hits = true;
                            }
                        }
#if DEBUG
                        else
                        {
                            _ = typeof(int);
                        }
#endif
                    }

                    //if (hits)
                    args.Text = preparedText;
#else
                    bool hits = false;
                    var preparedText = args.Text;
                    foreach (var ligature in ligatures)
                    {
                        int idx;
                        do
                        {
                            idx = preparedText.IndexOf(ligature.Key, StringComparison.Ordinal);
                            if (idx >= 0)
                            {
                                preparedText = preparedText.Substring(0, idx) +
                                       ligature.Value +
                                       preparedText.Substring(idx + ligature.Key.Length);
                                hits = true;
                            }
                        } while (idx >= 0);
                    }

                    if (hits)
                    {
                        args.Text = preparedText;
                        args.UseReturnedText = true;
                    }
#endif
                }
            };

            // Create an empty page in this document.
            var page = document.AddPage();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

#if CORE
            var fontName = "Times New Roman";
#else
            var fontName = "Georgia";
#endif

            int pos = 10;
            const string text = "Infinite off inflammable official offline off; IJsselmeer ijsvrij";
            var options = new XPdfFontOptions(PdfFontEncoding.Unicode);
            //var font = new XFont("Times New Roman", 12, XFontStyleEx.Bold, options);
            var font = new XFont(fontName, 12, XFontStyleEx.Regular, options);
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, pos, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = true;
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, pos + 12, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = false;
            pos += 30;

            font = new XFont("Arial", 12, XFontStyleEx.Regular, options);
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, pos, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = true;
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, pos + 12, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = false;
            pos += 30;

#if !CORE
            font = new XFont("Times New Roman", 12, XFontStyleEx.Regular, options);
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, pos, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = true;
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, pos + 12, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = false;
            pos += 30;

            // Comic Sans MS is an interesting test because there are no ligatures for "ff", "ffi", and "ffl".
            font = new XFont("Comic Sans MS", 12, XFontStyleEx.Regular, options);
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, pos, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = true;
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, pos + 12, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = false;
            pos += 30;
#endif

            font = new XFont(fontName, 12, XFontStyleEx.Bold, options);
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, pos, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = true;
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, pos + 12, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = false;
            pos += 30;

            font = new XFont("Arial", 12, XFontStyleEx.Bold, options);
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, pos, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = true;
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, pos + 12, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = false;
            pos += 30;

#if !CORE
            font = new XFont("Times New Roman", 12, XFontStyleEx.Bold, options);
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, pos, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = true;
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, pos + 12, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = false;
            pos += 30;

            // Comic Sans MS is an interesting test because there are no ligatures for "ff", "ffi", and "ffl".
            font = new XFont("Comic Sans MS", 12, XFontStyleEx.Bold, options);
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, pos, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = true;
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, pos + 12, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = false;
            pos += 30;
#endif

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("PdfLigatureTest-TextEvent");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);

            // Analyze the drawn text in the PDF’s content stream.
            var streamEnumerator = PdfFileHelper.GetPageContentStreamEnumerator(document, 0);

            streamEnumerator.Text.MoveAndGetNext(true, out var textInfo).Should().BeTrue();
            textInfo!.IsHex.Should().BeTrue();
            var hexString = textInfo.Text;
            hexString.Should().NotBeNull();

            var glyphIds = PdfFileHelper.GetHexStringAsGlyphIndices(hexString);
            glyphIds.Should().NotContain("0",
                "no char (and no no-break hyphen) should be converted to an invalid glyph (\"0\")");
        }

        [Fact]
        public void PDF_with_ligatures_render_event()
        {
            bool enableLigatures = false;

            Dictionary<string, char> ligatures = new()
            {
                { "IJ", 'Ĳ' },
                { "ij", 'ĳ' },
                { "ffi", 'ﬃ' },
                { "ffl", 'ﬄ' },
                { "ff", 'ﬀ' },
                { "fi", 'ﬁ' },
                { "fl", 'ﬂ' }
            };

            // Create a new PDF document.
            var document = new PdfDocument();
            document.Options.CompressContentStreams = false;

            document.RenderEvents.RenderTextEvent += (sender, args) =>
            {
                // ReSharper disable once AccessToModifiedClosure
                if (enableLigatures)
                {
                    var keys = new String[ligatures.Keys.Count];
                    ligatures.Keys.CopyTo(keys, 0);
                    var length = args.CodePointGlyphIndexPairs.Length;
                    for (var idx = 0; idx < length; idx++)
                    {
                        ref var item = ref args.CodePointGlyphIndexPairs[idx];

                        for (int ligIdx = 0; ligIdx < ligatures.Keys.Count; ++ligIdx)
                        {
                            var lig = keys[ligIdx];
                            if (item.CodePoint == lig[0] && idx + lig.Length <= length)
                            {
                                bool match = true;
                                // First character matches, check rest.
                                for (int ligIdx2 = 1; ligIdx2 < lig.Length; ++ligIdx2)
                                {
                                    ref var item2 = ref args.CodePointGlyphIndexPairs[idx + ligIdx2];
                                    if (item2.CodePoint != lig[ligIdx2])
                                    {
                                        match = false;
                                        break;
                                    }
                                }

                                if (match)
                                {
                                    item.CodePoint = ligatures[lig];
                                    args.ReevaluateGlyphIndices = true;
                                    // Have to remove the remaining chars.
                                    for (int ligIdx2 = 1; ligIdx2 < lig.Length; ++ligIdx2)
                                    {
                                        ref var item2 = ref args.CodePointGlyphIndexPairs[idx + ligIdx2];
                                        item2.CodePoint = -17; // Internal use.
                                                               //item2.GlyphIndex = 0;
                                    }
                                }
                            }
                        }
                    }

                    if (args.ReevaluateGlyphIndices)
                    {
                        var source = args.CodePointGlyphIndexPairs;
                        var dest = new List<CodePointGlyphIndexPair>();
                        foreach (var item in source)
                        {
                            // Remove obsolete entries.
                            if (item.CodePoint != -17)
                                dest.Add(item);
                        }
                        args.CodePointGlyphIndexPairs = new CodePointGlyphIndexPair[dest.Count];
                        dest.CopyTo(args.CodePointGlyphIndexPairs, 0);
                    }
                }
            };

            // Create an empty page in this document.
            var page = document.AddPage();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

#if CORE
            var fontName = "Times New Roman";
#else
            var fontName = "Georgia";
#endif
            const string text = "Infinite inflammable official offline";
            var options = new XPdfFontOptions(PdfFontEncoding.Unicode);
            //var font = new XFont("Times New Roman", 12, XFontStyleEx.Bold, options);
            var font = new XFont(fontName, 12, XFontStyleEx.Regular, options);
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, 50, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);
            enableLigatures = true;
            gfx.DrawString(text, font, XBrushes.Black, new XRect(50, 62, page.Width.Point, page.Height.Point),
                XStringFormats.TopLeft);

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("PdfLigatureTest-RenderEvent");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);

            // Analyze the drawn text in the PDF’s content stream.
            var streamEnumerator = PdfFileHelper.GetPageContentStreamEnumerator(document, 0);

            streamEnumerator.Text.MoveAndGetNext(true, out var textInfo).Should().BeTrue();
            textInfo!.IsHex.Should().BeTrue();
            var hexString = textInfo.Text;
            hexString.Should().NotBeNull();

            var glyphIds = PdfFileHelper.GetHexStringAsGlyphIndices(hexString);
            glyphIds.Should().NotContain("0",
                "no char (and no no-break hyphen) should be converted to an invalid glyph (\"0\")");
        }
    }
}
