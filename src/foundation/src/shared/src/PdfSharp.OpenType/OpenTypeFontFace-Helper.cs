// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable 0649
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Internal.OpenType
{
    sealed partial class OpenTypeFontFace
    {
        public OpenTypeGlyphMetrics CreateGlyphMetrics(ushort glyphIndex)
        {
            var otgm = new OpenTypeGlyphMetrics();

            var (lsb, advw) = hmtx.GetLsbAndAdvanceWidth(glyphIndex);
            var (tsb, advh) = (0, 0);
            if (vmtx != null!)
                (tsb, advh) = vmtx.GetTsbAndAdvanceHeight(glyphIndex);

            var header = glyf.GetGlyphHeader(glyphIndex);

            int leftSideBearing = lsb;
            int advanceWidth = advw;
            // It seems WPF takes lsb for tsb if no vmtx table exists.
            int topSideBearing = lsb;
            int advanceHeight = advh;

            // Calculate rsb like this:
            // rsb = aw - (lsb + xMax - xMin)
            // see https://learn.microsoft.com/de-de/typography/opentype/spec/hmtx
            int rightSideBearing = (advw - (lsb + header.xMax - header.xMin));
            int bottomSideBearing = (advh - (tsb + header.yMax - header.yMin));

            otgm.LeftSideBearing = leftSideBearing;
            otgm.AdvanceWidth = advanceWidth;
            otgm.RightSideBearing = rightSideBearing;
            otgm.TopSideBearing = topSideBearing;
            otgm.AdvanceHeight = advanceWidth;
            otgm.BottomSideBearing = bottomSideBearing = 0; //os2.sTypoDescender;
            otgm.DistancesFromHorizontalBaselineToBlackBoxBottom = 0;  //os2.sTypoDescender;

            // TODO NYI
            otgm.VerticalOrigin = 0;

            otgm.DrawBounds = new(header.xMin, header.yMin,
                header.xMax - header.xMin, header.yMax - header.yMin);
            return otgm;
        }

        public RenderingGlyphMetrics CreateGlyphMetrics(OpenTypeGlyphMetrics metrics)
        {
            var unitsPerEm = (float)head!.unitsPerEm;

            var rgm = new RenderingGlyphMetrics()
            {
                LeftSideBearing = metrics.LeftSideBearing / unitsPerEm,
                AdvanceWidth = metrics.AdvanceWidth / unitsPerEm,
                RightSideBearing = metrics.RightSideBearing / unitsPerEm,
                TopSideBearing = metrics.TopSideBearing / unitsPerEm,
                AdvanceHeight = metrics.AdvanceHeight / unitsPerEm,
                BottomSideBearing = metrics.BottomSideBearing / unitsPerEm,
                DrawBounds = new(
                    metrics.DrawBounds.X / unitsPerEm, metrics.DrawBounds.Y / unitsPerEm,
                    metrics.DrawBounds.Width / unitsPerEm, metrics.DrawBounds.Height / unitsPerEm)
            };
            return rgm;
        }
    }
}
