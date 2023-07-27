// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class PathText : Snippet
    {
        // Some chars with ascenders and descenders.
        const string TestChars = " Îygp";

        public PathText()
        {
            Title = "Text as Path";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            // Glyph as path.
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
#if CORE
                DrawNotSupportedFeature(gfx);
#else
                DrawAlignmentGrid(gfx);

                var path = new XGraphicsPath();

                path.AddString("@©", new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 100, BoxCenter, XStringFormats.Center);

                gfx.DrawPath(XPens.Black, XBrushes.Yellow, path);

                DrawCenterPoint(gfx);
#endif
            }
            EndBox(gfx);

            // Glyph as path with alignment at rect.
            BeginBox(gfx, 2, BoxOptions.Tile);
            {
#if CORE
                DrawNotSupportedFeature(gfx);
#else
                DrawAlignmentGrid(gfx, true);

                var path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 30, RectBox, XStringFormats.TopLeft);
                gfx.DrawPath(XPens.Black, XBrushes.White, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 30, RectBox, XStringFormats.TopCenter);
                gfx.DrawPath(XPens.Black, XBrushes.LightBlue, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 30, RectBox, XStringFormats.TopRight);
                gfx.DrawPath(XPens.Black, XBrushes.LightPink, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 30, RectCenterVLine, XStringFormats.BaseLineLeft);
                gfx.DrawPath(XPens.Black, XBrushes.Yellow, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 30, RectCenterVLine, XStringFormats.BaseLineCenter);
                gfx.DrawPath(XPens.Black, XBrushes.YellowGreen, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 30, RectCenterVLine, XStringFormats.BaseLineRight);
                gfx.DrawPath(XPens.Black, XBrushes.Green, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 30, RectBox, XStringFormats.CenterLeft);
                gfx.DrawPath(XPens.Black, XBrushes.LightGray, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 30, RectBox, XStringFormats.Center);
                gfx.DrawPath(XPens.Black, XBrushes.Blue, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 30, RectBox, XStringFormats.CenterRight);
                gfx.DrawPath(XPens.Black, XBrushes.Red, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 30, RectBox, XStringFormats.BottomLeft);
                gfx.DrawPath(XPens.Black, XBrushes.Gray, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 30, RectBox, XStringFormats.BottomCenter);
                gfx.DrawPath(XPens.Black, XBrushes.DarkBlue, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 30, RectBox, XStringFormats.BottomRight);
                gfx.DrawPath(XPens.Black, XBrushes.DarkRed, path);

                DrawCenterPoint(gfx);
#endif
            }
            EndBox(gfx);

            // Glyph as path with alignment at point (top).
            BeginBox(gfx, 3, BoxOptions.Tile);
            {
#if CORE
                DrawNotSupportedFeature(gfx);
#else
                DrawAlignmentGrid(gfx);

                var path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 50, BoxCenter, XStringFormats.TopLeft);
                gfx.DrawPath(XPens.Black, XBrushes.White, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 50, BoxCenter, XStringFormats.TopCenter);
                gfx.DrawPath(XPens.Black, XBrushes.Violet, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 50, BoxCenter, XStringFormats.TopRight);
                gfx.DrawPath(XPens.Black, XBrushes.Red, path);

                DrawCenterPoint(gfx);
#endif
            }
            EndBox(gfx);

            // Glyph as path with alignment at point (center).
            BeginBox(gfx, 4, BoxOptions.Tile);
            {
#if CORE
                DrawNotSupportedFeature(gfx);
#else
                DrawAlignmentGrid(gfx);

                var path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 50, BoxCenter, XStringFormats.CenterLeft);
                gfx.DrawPath(XPens.Black, XBrushes.White, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 50, BoxCenter, XStringFormats.Center);
                gfx.DrawPath(XPens.Black, XBrushes.Violet, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 50, BoxCenter, XStringFormats.CenterRight);
                gfx.DrawPath(XPens.Black, XBrushes.Red, path);

                DrawCenterPoint(gfx);
#endif
            }
            EndBox(gfx);

            // Glyph as path with alignment at point (bottom).
            BeginBox(gfx, 5, BoxOptions.Tile);
            {
#if CORE
                DrawNotSupportedFeature(gfx);
#else
                DrawAlignmentGrid(gfx);

                var path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 50, BoxCenter, XStringFormats.BottomLeft);
                gfx.DrawPath(XPens.Black, XBrushes.White, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 50, BoxCenter, XStringFormats.BottomCenter);
                gfx.DrawPath(XPens.Black, XBrushes.Violet, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 50, BoxCenter, XStringFormats.BottomRight);
                gfx.DrawPath(XPens.Black, XBrushes.Red, path);

                DrawCenterPoint(gfx);
#endif
            }
            EndBox(gfx);

            // Glyph as path with alignment at point (baseLine).
            BeginBox(gfx, 6, BoxOptions.Tile);
            {
#if CORE
                DrawNotSupportedFeature(gfx);
#else
                DrawAlignmentGrid(gfx);

                var path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 50, BoxCenter, XStringFormats.BaseLineLeft);
                gfx.DrawPath(XPens.Black, XBrushes.White, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 50, BoxCenter, XStringFormats.BaseLineCenter);
                gfx.DrawPath(XPens.Black, XBrushes.Violet, path);

                path = new XGraphicsPath();
                path.AddString(TestChars, new XFontFamily("Segoe UI"), XFontStyleEx.Regular, 50, BoxCenter, XStringFormats.BaseLineRight);
                gfx.DrawPath(XPens.Black, XBrushes.Red, path);

                DrawCenterPoint(gfx);
#endif
            }
            EndBox(gfx);
        }

        static readonly XRect RectCenterVLine = new XRect(0, BoxHeight / 2, BoxWidth, 0);
    }
}
