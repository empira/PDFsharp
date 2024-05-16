// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Runtime.InteropServices;
using PdfSharp.Pdf.IO;
using PdfSharp.TestHelper;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.Rendering;
using Xunit;
#if CORE
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;
#endif
#if WPF
using System.IO;
#endif

namespace MigraDoc.Tests
{
    [Collection("PDFsharp")]
    public class ImageFormats
    {
        [Fact]
#if WPF
        public void Test_Image_Formats_Wpf()
#elif GDI
        public void Test_Image_Formats_Gdi()
#else
        public void Test_Image_Formats()
#endif
        {
            // Create a MigraDoc document.
            var document = CreateDocument(true);

            // ----- Unicode encoding in MigraDoc is demonstrated here. -----

            // Create a renderer for the MigraDoc document.
            var pdfRenderer = new PdfDocumentRenderer()
            {
                // Associate the MigraDoc document with a renderer.
                Document = document
            };

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("Image_Formats");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Fact]
#if WPF
        public void Test_Image_BASE64_Wpf()
#elif GDI
        public void Test_Image_BASE64_Gdi()
#else
        public void Test_Image_BASE64()
#endif
        {
            // Create a MigraDoc document.
            var document = CreateDocument(false);

            // Associate the MigraDoc document with a renderer.
            var pdfRenderer = new PdfDocumentRenderer
            {
                Document = document,
                PdfDocument =
                {
                    PageLayout = PdfPageLayout.SinglePage
                }
            };

            AddImageFromBase64(document, "Image from base64 memory stream:");

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("Image_Base64");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Fact]
#if WPF
        public void Test_Image_Interpolation_Wpf()
#elif GDI
        public void Test_Image_Interpolation_Gdi()
#else
        public void Test_Image_Interpolation()
#endif
        {
            // Attempt to avoid "image file locked" under .NET 4.7.2.
            GC.Collect();
            GC.WaitForFullGCComplete();

            {
                // Create a MigraDoc document.
                var document = CreateDocument(false);

                // Associate the MigraDoc document with a renderer.
                var pdfRenderer = new PdfDocumentRenderer
                {
                    Document = document,
                    PdfDocument =
                {
                    PageLayout = PdfPageLayout.SinglePage
                }
                };

                AddImagesFromBase64(document);

                // Add everything from Format tests
                document.LastSection.AddPageBreak();
                AddImages(document, false);

                // Layout and render document to PDF.
                pdfRenderer.RenderDocument();

                // Save the document...
                var filename = PdfFileUtility.GetTempPdfFileName("Image_Interpolation");
                pdfRenderer.PdfDocument.Save(filename);
                // ...and start a viewer.
                PdfFileUtility.ShowDocumentIfDebugging(filename);
            }

            // Attempt to avoid "image file locked" under .NET 4.7.2.
            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        [Theory]
        [InlineData(SecurityTestHelper.TestOptionsEnum.None)]
        [InlineData(SecurityTestHelper.TestOptionsEnum.V5)] // We only want to ensure image filters are not affected by crypt filters, so testing encryption version 5 should do it.
#if WPF
        public void Test_Image_Formats_Encrypted_Wpf(SecurityTestHelper.TestOptionsEnum optionsEnum)
#elif GDI
        public void Test_Image_Formats_Encrypted_Gdi(SecurityTestHelper.TestOptionsEnum optionsEnum)
#else
        public void Test_Image_Formats_Encrypted(SecurityTestHelper.TestOptionsEnum optionsEnum)
#endif
        {
            // Attempt to avoid "image file locked" under .NET 4.7.2.
            GC.Collect();
            GC.WaitForFullGCComplete();

            {
                var options = SecurityTestHelper.TestOptions.ByEnum(optionsEnum);
                options.SetDefaultPasswords(true, true);

                // Write encrypted file.
                var filename = SecurityTestHelper.AddSuffixToFilename("Image_Formats_write.pdf", options);

                var document = CreateDocument(true);
                AddImagesFromBase64(document);

                var pdfRenderer = SecurityTestHelper.RenderSecuredDocument(document, options);
                pdfRenderer.Save(filename);
                // ReSharper disable once RedundantAssignment
                pdfRenderer = null;

                PdfFileUtility.ShowDocumentIfDebugging(filename);

                // Read encrypted file and write it without encryption.
                var pdfDocRead = PdfReader.Open(filename, SecurityTestHelper.PasswordOwnerDefault);

                var pdfRendererRead = new PdfDocumentRenderer { PdfDocument = pdfDocRead };

                var filenameRead = SecurityTestHelper.AddSuffixToFilename("Image_Formats_read.pdf", options);
                pdfRendererRead.Save(filenameRead);
                // ReSharper disable once RedundantAssignment
                pdfRendererRead = null;

                PdfFileUtility.ShowDocumentIfDebugging(filenameRead);
            }

            // Attempt to avoid "image file locked" under .NET 4.7.2.
            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        /// <summary>
        /// Creates an absolutely minimalistic document.
        /// </summary>
        Document CreateDocument(bool addImages, bool interpolation = true)
        {
            // Create a new MigraDoc document.
            var document = new Document();

            // Add a section to the document.
            var section = document.AddSection();

            // Add test images.
            if (addImages)
                AddImages(document, interpolation);

            // Create the primary footer.
            var footer = section.Footers.Primary;

            // Add content to footer.
            var paragraph = footer.AddParagraph();
            paragraph.Add(new DateField { Format = "yyyy/MM/dd HH:mm:ss" });
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            return document;
        }

        void AddImages(Document document, bool interpolation = true)
        {
            var root = IOUtility.GetAssetsPath(@"pdfsharp\images\samples\");
            var images = _testImages;
            var x = GetType();
            var section = document.LastSection;
            section.AddParagraph(x.Assembly.GetOriginalLocation());
            var workingDir = Environment.CurrentDirectory;
            section.AddParagraph(workingDir);

            foreach (var image in images)
            {
                var path = root + image.Path;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    path = path.Replace('\\', '/');

                var file = Path.GetFileName(image.Path);
                var header = $"{image.Comment} ({file})" + (interpolation ? "" : " - No interpolation");
                var paragraph = section.AddParagraph("Paragraph.AddImage: " + header);
                paragraph.AddLineBreak();
                var img = paragraph.AddImage(path);
                if (!interpolation) // only touch img.Interpolate if we are not using the default (true)
                    img.Interpolate = false;
                if (image.Width.HasValue)
                    img.Width = image.Width.Value;
                section.AddParagraph("Section.AddImage: " + header);
                img = section.AddImage(path);
                if (image.Width.HasValue)
                    img.Width = image.Width.Value;
                section.AddPageBreak();
            }
        }

        Image AddImageFromBase64(Document document, string header)
        {
            // The string is generated from the Nothing-Up-My-Sleeve-QR.NET Fiddle found here https://dotnetfiddle.net/Lz1VC3
            string StrFromMS = @"base64:iVBORw0KGgoAAAANSUhEUgAAAB0AAAAdCAYAAABWk2cPAAAABHNCSVQICAgIfAhkiAAAAPNJREFUSIntlksOwzAIRE3V+1/ZXSGRyYwBW8qiraUqboR5fE1szjnHw+v1NPC3oG98YWZS2NNvZrd95ZyEMiFmjMOibOWchKJwVKbg2bm4tnJ62mXbhZTlcbVkeDNvVAFVokChux5Uz92gnXxViqYEXfWdtwj2KRqQGVIqpAjz/yyXVW8plIWNeYI9inCV47R62c3DYAzSCi+G0hX6T0FcPm237hDP8lrxtj1lXHGschaZla72lGHhZUNA6Rlj0TIsh2hUfGJoVxV8dOErY1W4j6EKgDcWWxLqCljz456FclWQ7SnDvGPfTLi/6Ph/bH8d9AOCj+ow+d/+1wAAAABJRU5ErkJggg==";
  
            var s = document.LastSection;

            var p = s.AddParagraph(header);
            p.AddLineBreak();
            return p.AddImage(StrFromMS);
        }

        void AddImagesFromBase64(Document document)
        {
            AddImageFromBase64(document, "Image from base64 memory stream:");

            var img1 = AddImageFromBase64(document, "Image from base64 memory stream, no interpolation:");
            img1.Interpolate = false;

            var img2 = AddImageFromBase64(document, "Image from base64 memory stream, upscale by 10:");
            img2.ScaleHeight = 10.0;

            var img3 = AddImageFromBase64(document, "Image from base64 memory stream, upscale by 10, no interpolation:");
            img3.ScaleHeight = 10.0;
            img3.Interpolate = false;

            document.LastSection.AddPageBreak();
        }

        struct TestImage
        {
            public string Comment { get; set; }

            public string Path { get; set; }

            public Unit? Width { get; set; }
        }

        readonly TestImage[] _testImages = {
            // JPEG
            new() { Path = @"jpeg\windows7problem.jpg", Comment = "JPEG image", Width = "12cm" },
            new() { Path = @"jpeg\TruecolorNoAlpha.jpg", Comment = "JPEG image" },
            new() { Path = @"jpeg\truecolorA.jpg", Comment = "JPEG image" },
            new() { Path = @"jpeg\PowerBooks_CMYK.jpg", Comment = "JPEG with EXIF header" },
            new() { Path = @"jpeg\indexedmonoA.jpg", Comment = "JPEG image" },
            new() { Path = @"jpeg\grayscaleNoAlpha.jpg", Comment = "JPEG image" },
            new() { Path = @"jpeg\grayscaleA.jpg", Comment = "JPEG image" },
            new() { Path = @"jpeg\color8A.jpg", Comment = "JPEG image" },
            new() { Path = @"jpeg\color4A.jpg", Comment = "JPEG image" },
            new() { Path = @"jpeg\blackwhiteA.jpg", Comment = "JPEG image" },
            new() { Path = @"jpeg\Balloons_CMYK.jpg", Comment = "CMYK" },
            new() { Path = @"jpeg\Balloons_CMYK - Copy.jpg", Comment = "CMYK?" },

            // BMP
            new() { Path = @"bmp\BlackwhiteA.bmp", Comment = "BMP image" },
            new() { Path = @"bmp\BlackwhiteA2.bmp", Comment = "BMP image" },
            new() { Path = @"bmp\BlackwhiteTXT.bmp", Comment = "BMP image", Width = "8cm" },
            new() { Path = @"bmp\Color4A.bmp", Comment = "BMP image" },
            new() { Path = @"bmp\Color8A.bmp", Comment = "BMP image" },
            new() { Path = @"bmp\GrayscaleA.bmp", Comment = "BMP image" },
            new() { Path = @"bmp\IndexedmonoA.bmp", Comment = "BMP image" },
            new() { Path = @"bmp\Test_OS2.bmp", Comment = "OS/2 bitmap, not supported by Core build" },
            new() { Path = @"bmp\TruecolorA.bmp", Comment = "BMP image" },
            new() { Path = @"bmp\TruecolorMSPaint.bmp", Comment = "BMP image" },

            // PNG
            new() { Path = @"png\windows7problem.png", Comment = "PNG", Width = "12cm" },
            new() { Path = @"png\truecolorAlpha.png", Comment = "PNG" },
            new() { Path = @"png\truecolorA.png", Comment = "PNG" },
            new() { Path = @"png\indexedmonoA.png", Comment = "PNG" },
            new() { Path = @"png\grayscaleAlpha.png", Comment = "PNG" },
            new() { Path = @"png\grayscaleA.png", Comment = "PNG" },
            new() { Path = @"png\color8A.png", Comment = "PNG" },
            new() { Path = @"png\color4A.png", Comment = "PNG" },
            new() { Path = @"png\blackwhiteA.png", Comment = "PNG" },

            // BMP & PNG
            new() { Path = @"MigraDoc.bmp", Comment = "BMP image", Width = "8cm" },
            new() { Path = @"Logo landscape.bmp", Comment = "BMP", Width = "12cm" },
            new() { Path = @"Logo landscape MS Paint.bmp", Comment = "BMP", Width = "12cm" },
            new() { Path = @"Logo landscape 256.bmp", Comment = "BMP image", Width = "12cm" },
            new() { Path = @"MigraDoc.png", Comment = "PNG image", Width = "8cm" },
            new() { Path = @"Logo landscape.png", Comment = "PNG image", Width = "12cm" },
            new() { Path = @"Logo landscape 256.png", Comment = "PNG image", Width = "12cm" },

            // GIF, TIFF, PNG
            new() { Path = @"misc\image009.gif", Comment = "GIF, not supported by Core build" },
            new() { Path = @"misc\Rose (RGB 8).tif", Comment = "TIFF, not supported by Core build" },
            new() { Path = @"misc\Test.gif", Comment = "GIF, not supported by Core build" },
            new() { Path = @"misc\Test.png", Comment = "PNG image" }
        };
    }
}
