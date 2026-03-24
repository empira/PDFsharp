// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Quality
{
    /// <summary>
    /// Static helper functions for file IO.
    /// These functions are intended for unit tests and samples in solution code only.
    /// </summary>
    public static class PdfPngComparer
    {
        static readonly XPoint LeftTopLeft = new(XUnit.FromMillimeter(10.75).Point, XUnit.FromMillimeter(25).Point);
        static readonly XPoint RightTopLeft = new(XUnit.FromMillimeter(159.25).Point, XUnit.FromMillimeter(25).Point);

        static readonly XRect TitleRect = new(LeftTopLeft.X, XUnit.FromMillimeter(10).Point,
           XUnit.FromMillimeter(297 - 21.5).Point, XUnit.FromMillimeter(12).Point);

        static readonly XRect LeftSubTitleRect = new(LeftTopLeft.X, XUnit.FromMillimeter(200).Point,
           XUnit.FromMillimeter(127).Point, XUnit.FromMillimeter(8).Point);

        static readonly XRect RightSubTitleRect = new(RightTopLeft.X, LeftSubTitleRect.Y,
            LeftSubTitleRect.Width, LeftSubTitleRect.Height);

        public static void AppendPdfPngComparisonPage(PdfPngComparerParameters parms)
        {
            PdfDocument document;
            // Note that PdfFileUtility creates an empty temp file.
            if (File.Exists(parms.TargetPdfFilePath) && new FileInfo(parms.TargetPdfFilePath).Length > 0)
            {
                document = PdfReader.Open(parms.TargetPdfFilePath, PdfDocumentOpenMode.Modify);
            }
            else
            {
                document = new PdfDocument();
                document.ViewerPreferences.FitWindow = true;
                document.PageLayout = PdfPageLayout.SinglePage;
                //document.PageMode= 
            }

            var page = document.AddPage();
            page.Size = PageSize.A4;
            page.Orientation = PageOrientation.Landscape;

            var gfx = XGraphics.FromPdfPage(page);
            var titleFont = new XFont("Arial", 22, XFontStyleEx.Bold);
            var subTitleFont = new XFont("Arial", 9, XFontStyleEx.Bold);

            var docLeft = parms.LeftFileStream != null
                ? XPdfForm.FromStream(parms.LeftFileStream)
                : XPdfForm.FromFile(parms.LeftFilePath);

            var docRight = parms.RightFileStream != null 
                ? XImage.FromStream(parms.RightFileStream) 
                : XImage.FromFile(parms.RightFilePath);

            gfx.DrawImage(docLeft, XUnit.FromMillimeter(10.75).Point, XUnit.FromMillimeter(25).Point);
            gfx.DrawImage(docRight, XUnit.FromMillimeter(159.25).Point, XUnit.FromMillimeter(25).Point);

            //360, 480);
            //gfx.DrawImage(docRight, XUnit.FromMillimeter(21.5).Point, XUnit.FromMillimeter(25).Point);

            //gfx.DrawRectangle(XPens.Yellow, TitleRect);
            //gfx.DrawRectangle(XPens.Yellow, LeftSubTitleRect);
            //gfx.DrawRectangle(XPens.Yellow, RightSubTitleRect);
            gfx.DrawString(parms.Title, titleFont, XBrushes.Black, TitleRect, XStringFormats.Center);
            gfx.DrawString(parms.LeftSubTitle, subTitleFont, XBrushes.Black, LeftSubTitleRect, XStringFormats.Center);
            gfx.DrawString(parms.RightSubTitle, subTitleFont, XBrushes.Black, RightSubTitleRect, XStringFormats.Center);

            document.Save(parms.TargetPdfFilePath);
        }
    }

    public class PdfPngComparerParameters
    {
        public string Title { get; set; } = "";

        public string TargetPdfFilePath { get; set; } = "";

        public Stream? LeftFileStream { get; set; } = null!;

        public string LeftFilePath { get; set; } = "";

        public string LeftSubTitle { get; set; } = "";

        public Stream? RightFileStream { get; set; } = null!;

        public string RightFilePath { get; set; } = "";

        public string RightSubTitle { get; set; } = "";
    }
}
