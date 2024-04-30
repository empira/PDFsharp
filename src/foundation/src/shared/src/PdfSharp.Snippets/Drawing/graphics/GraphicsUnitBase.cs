// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Quality;
using System.Xml;

namespace PdfSharp.Snippets.Drawing
{
    public abstract class GraphicsUnitBase : Snippet
    {
        protected XFont font10ptinPP = new("Arial", XUnit.FromPoint(10).Presentation);
        protected XFont font10pt = new("Arial", 10);
        protected XFont font10ptInMM = new("Arial", XUnit.FromPoint(10).Millimeter);
        protected XFont font10ptInCM = new("Arial", XUnit.FromPoint(10).Centimeter);

        protected double CalcSizeForGraphics(double size, XGraphicsUnit graphicsUnit)
        {
            var x = new XUnit(size, graphicsUnit);

            x.ConvertType(XGraphics.PageUnit);
            return x.Value;
        }

        //public GraphicsUnit()
        //{
        //    Title = "Graphics Unit";
        //    PathName = "snippets/drawing/graphics/GraphicsUnit";
        //}

        //public override void RenderSnippet(XGraphics gfx)
        //{
        //    var font10ptinPP = new XFont("Arial", new XUnit(10).Presentation);
        //    var font10pt = new XFont("Arial", 10);
        //    var font10ptInMM = new XFont("Arial", new XUnit(10).Millimeter);
        //    var font10ptInCM = new XFont("Arial", new XUnit(10).Centimeter);

        //    // Page 1 - PU
        //    gfx.DrawLine(XPens.Red, 0, 0, 100, 100);
        //    gfx.DrawRectangle(XPens.DarkBlue, 200, 200, 50, 50);

        //    gfx.DrawString("Hello, 10pt", font10ptinPP, XBrushes.Black, 50, 100);

        //    EndPdfPage();

        //    // Page 2 - PT
        //    gfx = BeginPdfPage(WidthInPoint, HeightInPoint, XGraphicsUnit.Point);

        //    gfx.DrawLine(XPens.Red, 0, 0, 100, 100);
        //    gfx.DrawString("Hello, 10pt", font10pt, XBrushes.Black, 10, 80);

        //    EndPdfPage();


        //    // Page 2 - mm
        //    gfx = BeginPdfPage(WidthInPoint, HeightInPoint, XGraphicsUnit.Millimeter);

        //    gfx.DrawLine(XPens.Red, 0, 0, 100, 100);
        //    gfx.DrawRectangle(XPens.DarkBlue, 70, 120, 30, 30);
        //    gfx.DrawString("Hello, 10pt", font10ptInMM, XBrushes.Black, 10, 80);

        //    EndPdfPage();

        //    // Page 4 - cm
        //    gfx = BeginPdfPage(WidthInPoint, HeightInPoint, XGraphicsUnit.Centimeter);
        //    var pen = new XPen(XColors.Red, 0.1);
        //    gfx.DrawLine(XPens.Red, 0, 0, 10, 10);
        //    gfx.DrawRectangle(pen, 7, 12, 3, 3);
        //    gfx.DrawString("Hello, 10pt", font10ptInCM, XBrushes.Black, 1, 8);

        //    EndPdfPage();
        //}
    }
}
