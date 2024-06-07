// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Events;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Snippets.Font;
using Xunit;

namespace PdfSharp.Tests.Drawing
{
    [Collection("PDFsharp")]
    public class MeasurementTests
    {
        [Fact]
        public static void MeasureContextTest()
        {
            // Create a new PDF document.
            var document = new PdfDocument();
            document.Options.CompressContentStreams = false;

            var renderTextEventCallCount = 0;
            var renderTextEventReplacementsCount = 0;

#if true
            var renderEvents = new RenderEvents();
            renderEvents.RenderTextEvent += (sender, args) =>
#else
            document.RenderEvents.RenderTextEvent += (sender, args) =>
#endif
            {
                renderTextEventCallCount++;

                var length = args.CodePointGlyphIndexPairs.Length;
                for (var idx = 0; idx < length; idx++)
                {
                    ref var item = ref args.CodePointGlyphIndexPairs[idx];
                    if (item.CodePoint is '\u2011')
                    {
                        renderTextEventReplacementsCount++;

                        item.CodePoint = '-';
                        args.ReevaluateGlyphIndices = true;
                    }
                }
            };

            var options = new XPdfFontOptions(PdfFontEncoding.Unicode);
            var font = new XFont("Arial", 12, XFontStyleEx.Bold, options);

            renderTextEventCallCount.Should().Be(0, "no text has been measured.");
            renderTextEventReplacementsCount.Should().Be(0, "no text has been measured.");

            var gfx = XGraphics.CreateMeasureContext(new XSize(2000, 2000), XGraphicsUnit.Point, XPageDirection.Downwards, renderEvents);
            gfx.MeasureString("Usual-hyphen", font, XStringFormats.TopLeft);

            renderTextEventCallCount.Should().Be(1, "one text has been measured.");
            renderTextEventReplacementsCount.Should().Be(0, "no hyphen had to be replaced.");

            gfx.MeasureString("Usual-hyphen No\u2011break\u2011hyphen", font, XStringFormats.TopLeft);

            renderTextEventCallCount.Should().Be(2, "two texts have been measured.");
            renderTextEventReplacementsCount.Should().Be(2, "two no-break hyphens had to be replaced.");
        }
    }
}
