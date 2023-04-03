// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.IO;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.Rendering;
using PdfSharp.Pdf.IO;
#if CORE
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;
#endif
using PdfSharp.TestHelper;
using Xunit;
using System.Runtime.InteropServices;

namespace MigraDoc.Tests
{
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
#if CORE
            GlobalFontSettings.FontResolver ??= NewFontResolver.Get();
#endif

            // Create a MigraDoc document.
            var document = CreateDocument();

            // ----- Unicode encoding in MigraDoc is demonstrated here. -----

            // DELETE
            //// A flag indicating whether to create a Unicode PDF or a WinAnsi PDF file.
            //// This setting applies to all fonts used in the PDF document.
            //// This setting has no effect on the RTF renderer.
            //const bool unicode = false;

            // Create a renderer for the MigraDoc document.
            var pdfRenderer = new PdfDocumentRenderer()
            {
                // Associate the MigraDoc document with a renderer.
                Document = document
            };

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = PdfFileHelper.CreateTempFileName("HelloWorld");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileHelper.StartPdfViewerIfDebugging(filename);
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
#if CORE
            GlobalFontSettings.FontResolver ??= NewFontResolver.Get();
#endif

            var options = SecurityTestHelper.TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);

            // Write encrypted file.
            var filename = SecurityTestHelper.AddSuffixToFilename("Image_Formats_write.pdf", options);

            var document = CreateDocument();

            var pdfRenderer = SecurityTestHelper.RenderSecuredDocument(document, options);
            pdfRenderer.Save(filename);

            PdfFileHelper.StartPdfViewerIfDebugging(filename);


            // Read encrypted file and write it without encryption.
            var pdfDocRead = PdfReader.Open(filename, SecurityTestHelper.PasswordOwnerDefault);

            var pdfRendererRead = new PdfDocumentRenderer { PdfDocument = pdfDocRead };

            var filenameRead = SecurityTestHelper.AddSuffixToFilename("Image_Formats_read.pdf", options);
            pdfRendererRead.Save(filenameRead);

            PdfFileHelper.StartPdfViewerIfDebugging(filenameRead);
        }

        /// <summary>
        /// Creates an absolutely minimalistic document.
        /// </summary>
        Document CreateDocument()
        {
            // Create a new MigraDoc document.
            var document = new Document();

            // Add a section to the document.
            var section = document.AddSection();

            // Add test images.
            AddImages(document);

            // Create the primary footer.
            var footer = section.Footers.Primary;

            // Add content to footer.
            var paragraph = footer.AddParagraph();
            paragraph.Add(new DateField { Format = "yyyy/MM/dd HH:mm:ss" });
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            return document;
        }

        void AddImages(Document document)
        {
            var root = @"..\..\..\..\..\..\..\..\..\assets\pdfsharp\images\samples\";
            var images = _testImages;
            var x = GetType();
            var section = document.LastSection;
            section.AddParagraph(x.Assembly.Location);
            var workingDir = Environment.CurrentDirectory;
            section.AddParagraph(workingDir);

            foreach (var image in images)
            {
                var path = root + image.Path;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    path = path.Replace('\\', '/');

                var file = Path.GetFileName(image.Path);
                var header = $"{ image.Comment} ({file})";
                var paragraph = section.AddParagraph(header);
                paragraph.AddLineBreak();
                var img = paragraph.AddImage(path);
                if (image.Width.HasValue)
                    img.Width = image.Width.Value;
                section.AddParagraph(header);
                img = section.AddImage(path);
                if (image.Width.HasValue)
                    img.Width = image.Width.Value;
                section.AddPageBreak();
            }
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
            new() { Path = @"png\windows7problem.png", Comment = "PNG, not supported by Core build", Width = "12cm" },
            new() { Path = @"png\truecolorAlpha.png", Comment = "PNG" },
            new() { Path = @"png\truecolorA.png", Comment = "PNG" },
            new() { Path = @"png\indexedmonoA.png", Comment = "PNG, not supported by Core build" },
            new() { Path = @"png\grayscaleAlpha.png", Comment = "PNG, not supported by Core build" },
            new() { Path = @"png\grayscaleA.png", Comment = "PNG, not supported by Core build" },
            new() { Path = @"png\color8A.png", Comment = "PNG" },
            new() { Path = @"png\color4A.png", Comment = "PNG, not supported by Core build" },
            new() { Path = @"png\blackwhiteA.png", Comment = "PNG, not supported by Core build" },

            // BMP & PNG
            new() { Path = @"MigraDoc.bmp", Comment = "BMP image", Width = "8cm" },
            new() { Path = @"Logo landscape.bmp", Comment = "BMP, not supported by Core build", Width = "12cm" },
            new() { Path = @"Logo landscape MS Paint.bmp", Comment = "BMP, not supported by Core build", Width = "12cm" },
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
