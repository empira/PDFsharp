// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Snippets.Drawing
{
    /// <summary>
    /// 
    /// </summary>
    public class GraphicsUnitUpwards : GraphicsUnitBase
    {
        public GraphicsUnitUpwards()
        {
            Title = "Graphics Unit";
            PathName = "snippets/drawing/graphics/GraphicsUnit-Upwards";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var direction = XPageDirection.Upwards;
#pragma warning restore CS0618 // Type or member is obsolete

            // Page 1 - PU
            gfx.DrawString("This is not implemented", font10ptinPP, XBrushes.Black, 100, 150);
            gfx.DrawLine(XPens.Red, 0, 0, 100, 100);
            gfx.DrawRectangle(XPens.DarkBlue, 200, 200, 50, 50);
            gfx.DrawString("Hello, 10pt", font10ptinPP, XBrushes.Black, 50, 100);
            EndPdfPage();

            // Page 2 - PT
            gfx = BeginPdfPage(XUnit.FromPoint(WidthInPoint), XUnit.FromPoint(HeightInPoint), XGraphicsUnit.Point, direction);
            gfx.DrawLine(XPens.Red, 0, 0, 100, 100);
            gfx.DrawString("Hello, 10pt", font10pt, XBrushes.Black, 10, 80);
            EndPdfPage();

            // Page 2 - mm
            gfx = BeginPdfPage(XUnit.FromPoint(WidthInPoint), XUnit.FromPoint(HeightInPoint), XGraphicsUnit.Millimeter, direction);
            gfx.DrawLine(XPens.Red, 0, 0, 100, 100);
            gfx.DrawRectangle(XPens.DarkBlue, 70, 120, 30, 30);
            gfx.DrawString("Hello, 10pt", font10ptInMM, XBrushes.Black, 10, 80);
            EndPdfPage();

            // Page 4 - cm
            gfx = BeginPdfPage(XUnit.FromPoint(WidthInPoint), XUnit.FromPoint(HeightInPoint), XGraphicsUnit.Centimeter, direction);
            var pen = new XPen(XColors.Red, 0.1);
            gfx.DrawLine(XPens.Red, 0, 0, 10, 10);
            gfx.DrawRectangle(pen, 7, 12, 3, 3);
            gfx.DrawString("Hello, 10pt", font10ptInCM, XBrushes.Black, 1, 8);
            EndPdfPage();
        }
    }
}
