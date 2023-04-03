// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Filters;
using PdfSharp.Pdf.IO;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests
{
    public class ImageTests
    {
        [Fact]
        public void PDF_with_Images()
        {
            // Create a new PDF document.
            var document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";

            document.Options.EnableCcittCompressionForBilevelImages = true;
            document.Options.FlateEncodeMode = PdfFlateEncodeMode.BestCompression; // Makes CCITT compression obsolete.
            //document.Options.FlateEncodeMode = PdfFlateEncodeMode.BestSpeed; // Some images will use CCITT compression, e.g. BlackwhiteTXT.bmp.
            document.Options.UseFlateDecoderForJpegImages = PdfUseFlateDecoderForJpegImages.Automatic;

            // Create an empty page in this document.
            var page = document.AddPage();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

            // Draw two lines with a red default pen.
            var width = page.Width;
            var height = page.Height;
            gfx.DrawLine(XPens.Red, 0, 0, width, height);
            gfx.DrawLine(XPens.Red, width, 0, 0, height);

            // Draw a circle with a red pen which is 1.5 point thick.
            var r = width / 5;
            gfx.DrawEllipse(new XPen(XColors.Red, 1.5), XBrushes.White, new XRect(width / 2 - r, height / 2 - r, 2 * r, 2 * r));

            GlobalFontSettings.FontResolver ??= NewFontResolver.Get();

            // Create a font.
            var font = new XFont("Arial", 20, XFontStyleEx.BoldItalic);

            // Draw the text.
            gfx.DrawString("Hello, dotnet 6.0!", font, XBrushes.Black,
                new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);

            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\windows7problem.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\TruecolorNoAlpha.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\truecolorA.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\PowerBooks_CMYK.jpg"; // OK. Starts with EXIF header, not JFIF header.
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\indexedmonoA.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\grayscaleNoAlpha.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\grayscaleA.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\color8A.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\color4A.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\blackwhiteA.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\Balloons_CMYK.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\Balloons_CMYK - Copy.jpg"; // OK

            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\BlackwhiteA.bmp"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\BlackwhiteA2.bmp"; // OK
            var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\BlackwhiteTXT.bmp"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\Color4A.bmp"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\Color8A.bmp"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\GrayscaleA.bmp"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\IndexedmonoA.bmp"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\Test_OS2.bmp"; // NYI!
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\TruecolorA.bmp"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\TruecolorMSPaint.bmp"; // OK

            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\windows7problem.png"; // NYI
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\truecolorAlpha.png"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\truecolorA.png"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\indexedmonoA.png"; // NYI
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\grayscaleAlpha.png"; // NYI
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\grayscaleA.png"; // NYI
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\color8A.png"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\color4A.png"; // NYI
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\blackwhiteA.png"; // NYI

            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\MigraDoc.bmp"; // ARGB32
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\Logo landscape.bmp"; // RGB24
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\Logo landscape MS Paint.bmp"; // RGB24
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\Logo landscape 256.bmp"; // Palette8
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\MigraDoc.png"; // ARGB32
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\Logo landscape.png"; // RGB24
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\Logo landscape 256.png"; // Palette8

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                imagePath = imagePath.Replace('\\', '/');

            var assembly = Assembly.GetExecutingAssembly();
            var dir = Path.GetDirectoryName(assembly.Location)!;

            var fullName = Path.Combine(dir, imagePath);
            var image = XImage.FromFile(fullName);

            gfx.DrawImage(image, 100, 100, 100, 100);

            // Save the document...
            string filename = PdfFileHelper.CreateTempFileName("HelloImageWorld");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileHelper.StartPdfViewerIfDebugging(filename);
        }

        [Fact]
        public void WriteAndRead_PDF_with_FlateDecode()
        {
            // Create a new PDF document.
            var document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";

            // Set compression options.
            document.Options.EnableCcittCompressionForBilevelImages = true;
            document.Options.FlateEncodeMode = PdfFlateEncodeMode.BestCompression;
            document.Options.UseFlateDecoderForJpegImages = PdfUseFlateDecoderForJpegImages.Always; // Deflate images.
            document.Options.NoCompression = false;
            document.Options.CompressContentStreams = true; // Deflate contents.

            // Create an empty page in this document.
            var page = document.AddPage();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

            // Draw two lines with a red default pen.
            var width = page.Width;
            var height = page.Height;
            gfx.DrawLine(XPens.Red, 0, 0, width, height);
            gfx.DrawLine(XPens.Red, width, 0, 0, height);

            // Draw a circle with a red pen which is 1.5 point thick.
            var r = width / 5;
            gfx.DrawEllipse(new XPen(XColors.Red, 1.5), XBrushes.White, new XRect(width / 2 - r, height / 2 - r, 2 * r, 2 * r));

            GlobalFontSettings.FontResolver ??= NewFontResolver.Get();

            // Create a font.
            var font = new XFont("Arial", 20, XFontStyleEx.BoldItalic);

            // Draw the text.
            gfx.DrawString("Hello, dotnet 6.0!", font, XBrushes.Black,
                new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);

            var imagePaths = new[]
            {
                @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\Balloons_CMYK - Copy.jpg",
                @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\Balloons_CMYK.jpg",
                @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\blackwhiteA.jpg",
                @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\color4A.jpg",
                @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\color8A.jpg",
                @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\grayscaleA.jpg",
                @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\grayscaleNoAlpha.jpg",
                @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\indexedmonoA.jpg",
                @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\PowerBooks_CMYK.jpg",
                @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\truecolorA.jpg",
                @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\TruecolorNoAlpha.jpg",
                @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\windows7problem.jpg"
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                for (int i = 0; i < imagePaths.Length; ++i)
                    imagePaths[i] = imagePaths[i].Replace('\\', '/');

            var assembly = Assembly.GetExecutingAssembly();
            var dir = Path.GetDirectoryName(assembly.Location) ?? throw new Exception();

            int offset = 0;
            int imageHeight = 800 / imagePaths.Length;
            foreach (var imagePath in imagePaths)
            {
                var fullName = Path.Combine(dir, imagePath);
                var image = XImage.FromFile(fullName);

                gfx.DrawImage(image, 10, 10 + offset, imageHeight, imageHeight);

                offset += imageHeight;
            }

            // Save the document.
            string filename = PdfFileHelper.CreateTempFileName("HelloImageWorld");
            document.Save(filename);
            PdfFileHelper.StartPdfViewerIfDebugging(filename);

            // Load the document.
            var docRead = PdfReader.Open(filename);

            // Inflate and extract images.
            foreach (var readPage in docRead.Pages)
            {
                var resources = readPage.Elements.GetDictionary("/Resources");
                if (resources is null)
                    continue;
                var xObjects = resources.Elements.GetDictionary("/XObject");
                if (xObjects is null)
                    continue;
                var items = xObjects.Elements.Values;
                foreach (var item in items)
                {
                    var reference = item as PdfReference;
                    if (reference is null)
                        continue;
                    var xObject = reference.Value as PdfDictionary;
                    if (xObject is null || xObject.Elements.GetString("/Subtype") != "/Image")
                        continue;

                    ExportJpeg(xObject);
                }
            }

            void ExportJpeg(PdfDictionary image)
            {
                // TODO Check filter types. This works for "/Filter [/FlateDecode /DCTDecode]" only.
                var imageFilename = Path.Combine(dir, $"image-{Guid.NewGuid():N}.jpg");

                var stream = image.Stream.Value;
                var fd = new FlateDecode();
                var decoded = fd.Decode(stream);
                using var fs = new FileStream(imageFilename, FileMode.Create, FileAccess.Write);
                using var bw = new BinaryWriter(fs);
                bw.Write(decoded);
                bw.Flush();
            }
        }
    }
}
