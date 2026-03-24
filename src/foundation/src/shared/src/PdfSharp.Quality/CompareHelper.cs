// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Logging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Quality
{
    /// <summary>
    /// </summary>
    public static class CompareHelper
    {
        public static void RenderA4Page(PdfPage page, CompareOptions left, CompareOptions right)
        {
            page.Size = PageSize.A4;
            page.Orientation = PageOrientation.Landscape;

            var leftImage = XImage.FromFile(left.Filename);
            var rightImage = XImage.FromFile(right.Filename);

            var gfx = XGraphics.FromPdfPage(page);
            var rcLeft = new XRect(30, 30, 360, 480);
            gfx.DrawImage(leftImage, rcLeft);
            gfx.DrawRectangle(XPens.LightGray, rcLeft);

            var rcRight = new XRect(451, 30, 360, 480);
            gfx.DrawImage(rightImage, rcRight);
            gfx.DrawRectangle(XPens.LightGray, rcRight);
        }
    }

    public class CompareOptions
    {
        public string Filename { get; set; } = "";
    }
}
