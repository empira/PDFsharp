// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
using System.Net.Http;
#endif
using System.Reflection;
using FluentAssertions;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Filters;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
#if CORE
#endif
using Xunit;

namespace PdfSharp.Tests.Drawing
{
    [Collection("PDFsharp")]
    public class ImageTests : IDisposable
    {
        public ImageTests()
        {
            PdfSharpCore.ResetAll();
#if CORE
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
#endif
        }

        public void Dispose()
        {
            PdfSharpCore.ResetAll();
        }

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
            var width = page.Width.Point;
            var height = page.Height.Point;
            gfx.DrawLine(XPens.Red, 0, 0, width, height);
            gfx.DrawLine(XPens.Red, width, 0, 0, height);

            // Draw a circle with a red pen which is 1.5 point thick.
            var r = width / 5;
            gfx.DrawEllipse(new XPen(XColors.Red, 1.5), XBrushes.White, new XRect(width / 2 - r, height / 2 - r, 2 * r, 2 * r));

            // Create a font.
            var font = new XFont("Arial", 20, XFontStyleEx.BoldItalic);

            // Draw the text.
            gfx.DrawString("Hello, World!", font, XBrushes.Black,
                new XRect(0, 0, width, height), XStringFormats.Center);

            //var imagePath = "PDFsharp/images/samples/jpeg/windows7problem.jpg"; // OK
            //var imagePath = "PDFsharp/images/samples/jpeg/TruecolorNoAlpha.jpg"; // OK
            //var imagePath = "PDFsharp/images/samples/jpeg/truecolorA.jpg"; // OK
            //var imagePath = "PDFsharp/images/samples/jpeg/PowerBooks_CMYK.jpg"; // OK. Starts with EXIF header, not JFIF header.
            //var imagePath = "PDFsharp/images/samples/jpeg/indexedmonoA.jpg"; // OK
            //var imagePath = "PDFsharp/images/samples/jpeg/grayscaleNoAlpha.jpg"; // OK
            //var imagePath = "PDFsharp/images/samples/jpeg/grayscaleA.jpg"; // OK
            //var imagePath = "PDFsharp/images/samples/jpeg/color8A.jpg"; // OK
            //var imagePath = "PDFsharp/images/samples/jpeg/color4A.jpg"; // OK
            //var imagePath = "PDFsharp/images/samples/jpeg/blackwhiteA.jpg"; // OK
            //var imagePath = "PDFsharp/images/samples/jpeg/Balloons_CMYK.jpg"; // OK
            //var imagePath = "PDFsharp/images/samples/jpeg/Balloons_CMYK - Copy.jpg"; // OK

            //var imagePath = "PDFsharp/images/samples/bmp/BlackwhiteA.bmp"; // OK
            //var imagePath = "PDFsharp/images/samples/bmp/BlackwhiteA2.bmp"; // OK
            //var imagePath = "PDFsharp/images/samples/bmp/BlackwhiteTXT.bmp"; // OK
            //var imagePath = "PDFsharp/images/samples/bmp/Color4A.bmp"; // OK
            //var imagePath = "PDFsharp/images/samples/bmp/Color8A.bmp"; // OK
            //var imagePath = "PDFsharp/images/samples/bmp/GrayscaleA.bmp"; // OK
            //var imagePath = "PDFsharp/images/samples/bmp/IndexedmonoA.bmp"; // OK
            //var imagePath = "PDFsharp/images/samples/bmp/Test_OS2.bmp"; // NYI!
            //var imagePath = "PDFsharp/images/samples/bmp/TruecolorA.bmp"; // OK
            //var imagePath = "PDFsharp/images/samples/bmp/TruecolorMSPaint.bmp"; // OK

            //var imagePath = "PDFsharp/images/samples/png/windows7problem.png"; // NYI
            //var imagePath = "PDFsharp/images/samples/png/truecolorAlpha.png"; // OK
            //var imagePath = "PDFsharp/images/samples/png/truecolorA.png"; // OK
            //var imagePath = "PDFsharp/images/samples/png/indexedmonoA.png"; // NYI
            //var imagePath = "PDFsharp/images/samples/png/grayscaleAlpha.png"; // NYI
            //var imagePath = "PDFsharp/images/samples/png/grayscaleA.png"; // NYI
            //var imagePath = "PDFsharp/images/samples/png/color8A.png"; // OK
            //var imagePath = "PDFsharp/images/samples/png/color4A.png"; // NYI
            //var imagePath = "PDFsharp/images/samples/png/blackwhiteA.png"; // NYI

            //var imagePath = "PDFsharp/images/samples/MigraDoc.bmp"; // ARGB32
            //var imagePath = "PDFsharp/images/samples/Logo landscape.bmp"; // RGB24
            //var imagePath = "PDFsharp/images/samples/Logo landscape MS Paint.bmp"; // RGB24
            //var imagePath = "PDFsharp/images/samples/Logo landscape 256.bmp"; // Palette8
            //var imagePath = "PDFsharp/images/samples/MigraDoc.png"; // ARGB32
            //var imagePath = "PDFsharp/images/samples/Logo landscape.png"; // RGB24
            //var imagePath = "PDFsharp/images/samples/Logo landscape 256.png"; // Palette8

            var imagePath = "pdfsharp-6.x/images/jpeg/extern/Zoo_JPEG_8BIM.jpg"; // OK

            var fullName = IOUtility.GetAssetsPath(imagePath)!;
            var image = XImage.FromFile(fullName);

            gfx.DrawImage(image, 100, 100, 100, 100);

            // Save the document...
            string filename = PdfFileUtility.GetTempPdfFileName("ImageTests");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
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
            var width = page.Width.Point;
            var height = page.Height.Point;
            gfx.DrawLine(XPens.Red, 0, 0, width, height);
            gfx.DrawLine(XPens.Red, width, 0, 0, height);

            // Draw a circle with a red pen which is 1.5 point thick.
            var r = width / 5;
            gfx.DrawEllipse(new XPen(XColors.Red, 1.5), XBrushes.White, new XRect(width / 2 - r, height / 2 - r, 2 * r, 2 * r));

            // Create a font.
            var font = new XFont("Arial", 20, XFontStyleEx.BoldItalic);

            // Draw the text.
            gfx.DrawString("Hello, World!", font, XBrushes.Black,
                new XRect(0, 0, width, height), XStringFormats.Center);

            var imagePaths = new[]
            {
                "PDFsharp/images/samples/jpeg/Balloons_CMYK - Copy.jpg",
                "PDFsharp/images/samples/jpeg/Balloons_CMYK.jpg",
                "PDFsharp/images/samples/jpeg/blackwhiteA.jpg",
                "PDFsharp/images/samples/jpeg/color4A.jpg",
                "PDFsharp/images/samples/jpeg/color8A.jpg",
                "PDFsharp/images/samples/jpeg/grayscaleA.jpg",
                "PDFsharp/images/samples/jpeg/grayscaleNoAlpha.jpg",
                "PDFsharp/images/samples/jpeg/indexedmonoA.jpg",
                "PDFsharp/images/samples/jpeg/PowerBooks_CMYK.jpg",
                "PDFsharp/images/samples/jpeg/truecolorA.jpg",
                "PDFsharp/images/samples/jpeg/TruecolorNoAlpha.jpg",
                "PDFsharp/images/samples/jpeg/windows7problem.jpg"
            };

            // Attempt to avoid "Out of memory" under .NET 4.6.2.
            GC.Collect();
            GC.WaitForFullGCComplete();

            int offset = 0;
            int imageHeight = 800 / imagePaths.Length;
            foreach (var imagePath in imagePaths)
            {
                var fullName = IOUtility.GetAssetsPath(imagePath)!;
                var image = XImage.FromFile(fullName);

                gfx.DrawImage(image, 10, 10 + offset, imageHeight, imageHeight);

                offset += imageHeight;
            }

            // Save the document.
            string filename = PdfFileUtility.GetTempPdfFileName("FlateDecodeImageTest");
            document.Save(filename);
            PdfFileUtility.ShowDocumentIfDebugging(filename);

            // Load the document.
            var docRead = PdfReader.Open(filename);

            // Inflate and extract images.
            var assembly = Assembly.GetExecutingAssembly();
            var dir = Path.GetDirectoryName(assembly.GetOriginalLocation()) ?? throw new Exception();
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
                // TODO_OLD Check filter types. This works for "/Filter [/FlateDecode /DCTDecode]" only.
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

        [Fact]
        public void PDF_with_Image_from_stream()
        {
            // Attempt to avoid "image file locked" under .NET 4.6.2.
            GC.Collect();
            GC.WaitForFullGCComplete();

            {
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                var imagePath = IOUtility.GetAssetsPath("PDFsharp/images/samples/jpeg/truecolorA.jpg")!;

                var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                using var xImage = XImage.FromStream(stream);

                gfx.DrawImage(xImage, 100, 100, 100, 100);

                // Save the document...
                var filename = PdfFileUtility.GetTempPdfFileName("ImageFromStream");
                document.Save(filename);
                // ...and start a viewer.
                PdfFileUtility.ShowDocumentIfDebugging(filename);
            }

            // Attempt to avoid "image file locked" under .NET 4.6.2.
            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        [Fact]
        public void PDF_with_Image_from_private_memorystream()
        {
            // Attempt to avoid "image file locked" under .NET 4.6.2.
            GC.Collect();
            GC.WaitForFullGCComplete();

            {
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                var imagePath = IOUtility.GetAssetsPath("PDFsharp/images/samples/jpeg/truecolorA.jpg")!;
                var pngBytes = File.ReadAllBytes(imagePath);

                // Create a MemoryStream that does not allow GetBuffer.
                var stream = new MemoryStream(pngBytes);
                using var xImage = XImage.FromStream(stream);

                gfx.DrawImage(xImage, 100, 100, 100, 100);

                // Save the document...
                var filename = PdfFileUtility.GetTempPdfFileName("ImageFromStream");
                document.Save(filename);
                // ...and start a viewer.
                PdfFileUtility.ShowDocumentIfDebugging(filename);
            }

            // Attempt to avoid "image file locked" under .NET 4.6.2.
            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        [Fact]
        public void PDF_with_Image_from_public_memorystream()
        {
            // Attempt to avoid "image file locked" under .NET 4.6.2.
            GC.Collect();
            GC.WaitForFullGCComplete();

            {
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                var imagePath = IOUtility.GetAssetsPath("PDFsharp/images/samples/jpeg/truecolorA.jpg")!;
                var pngBytes = File.ReadAllBytes(imagePath);

                // Create a MemoryStream that allows GetBuffer.
                var stream = new MemoryStream(pngBytes, 0, pngBytes.Length, false, true);
                using var xImage = XImage.FromStream(stream);

                gfx.DrawImage(xImage, 100, 100, 100, 100);

                // Save the document...
                var filename = PdfFileUtility.GetTempPdfFileName("ImageFromStream");
                document.Save(filename);
                // ...and start a viewer.
                PdfFileUtility.ShowDocumentIfDebugging(filename);
            }

            // Attempt to avoid "image file locked" under .NET 4.6.2.
            GC.Collect();
            GC.WaitForFullGCComplete();
        }

#if GDI
        [Fact]
        public void PDF_with_image_from_GDI()
        {
            // Create a new PDF document.
            var document = new PdfDocument();

#if DEBUG
            // Create PDF files that are somewhat human-readable.
            document.Options.Layout = PdfWriterLayout.Verbose;
#endif

            // Create an empty page in this document.
            var page = document.AddPage();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

            var imageFolder = IOUtility.GetAssetsPath("pdfsharp/images/samples/jpeg");
            var imageFile = Path.Combine(imageFolder ?? throw new InvalidOperationException("Call Download-Assets.ps1 before running the tests."), "truecolorA.jpg");

            var gdiImage = Image.FromFile(imageFile);
            var xImage = XImage.FromGdiPlusImage(gdiImage);
            gfx.DrawImage(xImage, new RectangleF(0f, 0f, 128f, 128f));

            imageFolder = IOUtility.GetAssetsPath("pdfsharp/images/samples/png");
            imageFile = Path.Combine(imageFolder ?? throw new InvalidOperationException("Call Download-Assets.ps1 before running the tests."), "truecolorA.png");

            gdiImage = Image.FromFile(imageFile);
            xImage = XImage.FromGdiPlusImage(gdiImage);
            gfx.DrawImage(xImage, new RectangleF(0f, 144f, 128f, 128f));

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("ImageFromStream");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

#endif

#if NET6_0_OR_GREATER
        [Fact]
        public async Task PDF_with_Image_from_http_stream()
        {
            {
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                using var client = new HttpClient();
                await using var imageStream =
                    await client.GetStreamAsync("https://docs.pdfsharp.net/images/PDFsharp-80x80.png").ConfigureAwait(false);
                //await using var imageStream =
                //    await client.GetStreamAsync("https://upload.wikimedia.org/wikipedia/commons/7/70/Example.png").ConfigureAwait(false);

#if WPF
                XImage xImage = null!;
                // ReSharper disable once AccessToDisposedClosure
                Action createImage = () => xImage = XImage.FromStream(imageStream);
                createImage.Should().Throw<InvalidOperationException>();
#else
                var xImage = XImage.FromBitmapImageStreamThatCannotSeek(imageStream);
                gfx.DrawImage(xImage, 100, 100, 100, 100);

                // Save the document...
                var filename = PdfFileUtility.GetTempPdfFileName("ImageFromStream");
                document.Save(filename);
                // ...and start a viewer.
                PdfFileUtility.ShowDocumentIfDebugging(filename);
#endif

            }
        }
#endif

        [Fact]
        public void Create_brush_by_image()
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

            var imagePath = IOUtility.GetAssetsPath("pdfsharp/images/PDFsharp-80x80.png")!;
            XBrush xBrush = XImageBrush.FromFile(imagePath);
            XPen xPen = XPens.Black;

            gfx.DrawEllipse(xPen, xBrush, new XRect(10, 10, 200, 190));
            gfx.DrawRectangle(xPen, xBrush, new XRect(250, 10, 200, 190));
            gfx.DrawRoundedRectangle(xPen, xBrush, new XRect(10, 250, 200, 190), new XSize(30, 30));

            XGraphicsPath path = new XGraphicsPath();
            path.AddLines(new XPoint[] { new XPoint(350, 250), new XPoint(250, 450), new XPoint(450, 330), new XPoint(250, 330), new XPoint(450, 450), new XPoint(350, 250) });
            gfx.DrawPath(xPen, xBrush, path);

            gfx.DrawPie(xPen, xBrush, 10, 500, 190, 190, 50, 50);
            gfx.DrawPolygon(xPen, xBrush, new XPoint[] { new XPoint(300, 500), new XPoint(400, 500), new XPoint(450, 550), new XPoint(450, 650), new XPoint(400, 700), new XPoint(300, 700), new XPoint(250, 650), new XPoint(250, 550), new XPoint(300, 500) }, XFillMode.Alternate);

            // Create an second empty page in this document.
            page = document.AddPage();

            // Get an XGraphics object for drawing on this page.
            gfx = XGraphics.FromPdfPage(page);

            gfx.DrawClosedCurve(xPen, xBrush, new XPoint[] { new XPoint(110, 10), new XPoint(190, 190), new XPoint(10, 190) });
            gfx.DrawRectangle(xPen, xBrush, new XRect(250, 10, 200, 190));

            // Create a font.
            var font = new XFont("Arial", 20, XFontStyleEx.BoldItalic);
            gfx.DrawString("Hello, dotnet 6.0!", font, xBrush, new XRect(250, 10, 200, 200), XStringFormats.Center); // Idk what we're gonna do

            // Save the document...
            string filename = PdfFileUtility.GetTempPdfFileName("HelloImageWorld");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }
    }
}
