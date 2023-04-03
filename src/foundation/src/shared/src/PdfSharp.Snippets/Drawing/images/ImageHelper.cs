// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public static class ImageHelper
    {
        public enum BmpImages
        {
            TruecolorA,
            GrayscaleA,
            Color8A,
            IndexedmonoA,
            Color4A,
            BlackwhiteA,
// ReSharper disable InconsistentNaming
            Test_OS2
// ReSharper restore InconsistentNaming
        }

        public enum GifImages
        {
            TruecolorTransparency,
            GrayscaleTransparency,
            TruecolorA,
            GrayscaleA,
            Color8A,
            IndexedmonoA,
            Color4A,
            BlackwhiteA,
            Image009
        }

        public enum JpegImages
        {
            TruecolorNoAlpha,
            GrayscaleNoAlpha,
            TruecolorA,
            GrayscaleA,
            Color8A,
            IndexedmonoA,
            Color4A,
            BlackwhiteA,
            Windows7Problem,
            // ReSharper disable InconsistentNaming
            Balloons_CMYK,
            PowerBooks_CMYK
            // ReSharper restore InconsistentNaming
        }

        public enum PngImages
        {
            TruecolorAlpha,
            GrayscaleAlpha,
            TruecolorA,
            GrayscaleA,
            Color8A,
            IndexedmonoA,
            Color4A,
            BlackwhiteA,
            Windows7Problem
        }

        public enum TiffImages
        {
// ReSharper disable InconsistentNaming
            Rose_RGB8,
            Rose_CMYK,
            Balloons_CMYK
// ReSharper restore InconsistentNaming
        }

        public enum PdfFiles
        {
            // ReSharper disable InconsistentNaming
            SomeLayout
            // ReSharper restore InconsistentNaming
        }

        public static XImage GetBmpImage(BmpImages image)
        {
            // Use image from file system.
            return GetXImage("assets\\Images\\bmp\\" + image + ".bmp");
        }

        public static XImage GetGifImage(GifImages image)
        {
            // Use image from file system.
            return GetXImage("assets\\Images\\gif\\" + image + ".gif");
        }

        public static XImage GetJpegImage(JpegImages image)
        {
            // Use image from file system.
            return GetXImage("assets\\Images\\jpeg\\" + image + ".jpg");
        }

        public static XImage GetPngImage(PngImages image)
        {
            // Use image from file system.
            return GetXImage("assets\\Images\\png\\" + image + ".png");
        }

        public static XImage GetTiffImage(TiffImages image)
        {
            // Use image from file system.
            return GetXImage("assets\\Images\\tiff\\" + image + ".tif");
        }

        static XImage GetXImage(string subPath)
        {
            var path = PathHelper.FindPath("internal\\PDFsharp", subPath);
            var img = XImage.FromFile(path);
            return img;
        }

        public static PdfDocument GetPdfFile(PdfFiles file)
        {
            // Use image from file system.
            return GetPdfDocument("assets\\PDFs\\misc\\" + file + ".pdf");
        }

        static PdfDocument GetPdfDocument(string subPath, PdfDocumentOpenMode openMode = PdfDocumentOpenMode.Import)
        {
            var path = PathHelper.FindPath("internal\\PDFsharp", subPath);
            var pdfDoc = PdfReader.Open(path, openMode);
            return pdfDoc;
        }

        public static XPdfForm GetPdfForm(PdfFiles file)
        {
            // Use image from file system.
            return GetPdfForm("assets\\PDFs\\misc\\" + file + ".pdf");
        }

        static XPdfForm GetPdfForm(string subPath, PdfDocumentOpenMode openMode = PdfDocumentOpenMode.Import)
        {
            var path = PathHelper.FindPath("internal\\PDFsharp", subPath);
            var pdfDoc = XPdfForm.FromFile(path);
            return pdfDoc;
        }
    }
}
