// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
#if CORE
#endif
using Xunit;

namespace PdfSharp.Tests
{
    [Collection("PDFsharp")]
    public class BasicTests : IDisposable
    {
        public BasicTests()
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
        public void Create_Hello_World_BasicTests()
        {
            // Create a new PDF document.
            var document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";

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
            var font = new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);

            // Draw the text.
#if NET6_0_OR_GREATER
            gfx.DrawString($"Hello, dotnet {Environment.Version.Major}.{Environment.Version.Minor}!", font, XBrushes.Black,
                new XRect(0, 0, width, height), XStringFormats.Center);
#else
            gfx.DrawString("Hello, World!", font, XBrushes.Black,
                new XRect(0, 0, width, height), XStringFormats.Center);
#endif

            // Save the document...
            string filename = PdfFileUtility.GetTempPdfFileName("HelloWorld");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Fact]
        public void Create_CropBox_BasicTests()
        {
            // Create a new PDF document.
            var document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";

            // Create an empty page in this document.
            var page = document.AddPage();

            var mediaBox = page.MediaBoxReadOnly;
            mediaBox.Should().NotBeNull();
            mediaBox.Should().NotBe(XRect.Empty);
            mediaBox.IsZero.Should().BeFalse();

            // CropBox does not exist by default.
            var cropBox = page.CropBoxReadOnly;
            cropBox.IsZero.Should().BeTrue();

            cropBox = page.EffectiveCropBoxReadOnly;
            cropBox.Should().NotBeNull();
            cropBox.Should().NotBe(XRect.Empty);
            cropBox.IsZero.Should().BeFalse();

            page.Width = XUnit.FromCentimeter(5);
            page.Height = XUnit.FromCentimeter(2);

            mediaBox = page.MediaBoxReadOnly;
            mediaBox.Should().NotBeNull();
            mediaBox.Should().NotBe(XRect.Empty);
            mediaBox.IsZero.Should().BeFalse();

            cropBox = page.CropBoxReadOnly;
            cropBox.IsZero.Should().BeTrue();

            cropBox = page.EffectiveCropBoxReadOnly;
            cropBox.Should().NotBe(XRect.Empty);
            cropBox.IsZero.Should().BeFalse();

            // Calling page.CropBox sets "new PdfRectangle()" as CropBox.
            // For "new PdfRectangle()", IsZero is true.
            cropBox = page.CropBox;
            cropBox.Should().NotBeNull();
            cropBox.Should().NotBe(XRect.Empty);
            cropBox.IsZero.Should().BeTrue();

            cropBox = page.EffectiveCropBoxReadOnly;
            cropBox.Should().NotBeNull();
            cropBox.Should().NotBe(XRect.Empty);
            cropBox.IsZero.Should().BeTrue();

            cropBox = page.CropBoxReadOnly;
            cropBox.Should().NotBeNull();
            cropBox.Should().NotBe(XRect.Empty);
            cropBox.IsZero.Should().BeTrue();

            cropBox = page.CropBox;
            cropBox.Should().NotBeNull();
            cropBox.Should().NotBe(XRect.Empty);
            cropBox.IsZero.Should().BeTrue();

            page.CropBox = new PdfRectangle(XUnitPt.FromMillimeter(1), XUnitPt.FromMillimeter(1),
                XUnitPt.FromMillimeter(49), XUnitPt.FromMillimeter(19));

            cropBox = page.CropBox;
            cropBox.Should().NotBeNull();
            cropBox.Should().NotBe(XRect.Empty);
            cropBox.IsZero.Should().BeFalse();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

            // Draw two lines with a red default pen.
            var width = page.Width.Point;
            var height = page.Height.Point;
            gfx.DrawLine(XPens.Red, 0, 0, width, height);
            gfx.DrawLine(XPens.Red, width, 0, 0, height);

            // Draw a circle with a red pen which is 1.5 point thick.
            var r = width / 6;
            gfx.DrawEllipse(new XPen(XColors.Red, 1.5), XBrushes.White, new XRect(width / 2 - r, height / 2 - r, 2 * r, 2 * r));

            // Create a font.
            var font = new XFont("Times New Roman", 10, XFontStyleEx.BoldItalic);

#if NET6_0_OR_GREATER
            gfx.DrawString($"Hello, dotnet {Environment.Version.Major}.{Environment.Version.Minor}!", font, XBrushes.Black,
                new XRect(0, 0, width, height), XStringFormats.Center);
#else
            gfx.DrawString("Hello, World!", font, XBrushes.Black,
                new XRect(0, 0, width, height), XStringFormats.Center);
#endif

            // Save the document...
            string filename = PdfFileUtility.GetTempPdfFileName("BasicMediaBoxTest");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Fact]
        public void Create_all_boxes_BasicTests()
        {
            // Create a new PDF document.
            var document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";

            // Create an empty page in this document.
            var page = document.AddPage();
            var pageWidth = page.Width.Point;
            var pageHeight = page.Height.Point;
            var pageRectangle = new PdfRectangle(0, 0, pageWidth, pageHeight);
            var defaultRectangle = new PdfRectangle();

            var mediaBox = page.MediaBoxReadOnly;
            mediaBox.Should().NotBeNull();
            mediaBox.Should().Be(pageRectangle);

            // CropBox does not exist by default.
            var cropBox = page.CropBoxReadOnly;
            cropBox.Should().Be(defaultRectangle);

            cropBox = page.EffectiveCropBoxReadOnly;
            cropBox.Should().NotBeNull();
            cropBox.Should().Be(pageRectangle);

            page.Width = XUnit.FromCentimeter(5);
            page.Height = XUnit.FromCentimeter(2);

            mediaBox = page.MediaBoxReadOnly;
            mediaBox.Should().NotBeNull();
            mediaBox.Should().NotBe(XRect.Empty);
            mediaBox.IsZero.Should().BeFalse();

            // ----- CropBox -----
            cropBox = page.CropBoxReadOnly;
            cropBox.IsZero.Should().BeTrue();

            cropBox = page.EffectiveCropBoxReadOnly;
            cropBox.Should().NotBe(XRect.Empty);
            cropBox.IsZero.Should().BeFalse();

            // Calling page.CropBox sets "new PdfRectangle()" as CropBox.
            // For "new PdfRectangle()", IsZero is true.
            cropBox = page.CropBox;
            cropBox.Should().NotBeNull();
            cropBox.Should().NotBe(XRect.Empty);
            cropBox.IsZero.Should().BeTrue();

            cropBox = page.EffectiveCropBoxReadOnly;
            cropBox.Should().NotBeNull();
            cropBox.Should().NotBe(XRect.Empty);
            cropBox.IsZero.Should().BeTrue();

            cropBox = page.CropBoxReadOnly;
            cropBox.Should().NotBeNull();
            cropBox.Should().NotBe(XRect.Empty);
            cropBox.IsZero.Should().BeTrue();

            cropBox = page.CropBox;
            cropBox.Should().NotBeNull();
            cropBox.Should().NotBe(XRect.Empty);
            cropBox.IsZero.Should().BeTrue();

            page.CropBox = page.MediaBox;

            cropBox = page.CropBox;
            cropBox.Should().NotBeNull();
            cropBox.Should().NotBe(XRect.Empty);
            cropBox.IsZero.Should().BeFalse();

            // ----- ArtBox -----
            var artBox = page.ArtBoxReadOnly;
            artBox.IsZero.Should().BeTrue();

            artBox = page.EffectiveArtBoxReadOnly;
            artBox.Should().NotBe(XRect.Empty);
            artBox.IsZero.Should().BeFalse();

            // Calling page.ArtBox sets "new PdfRectangle()" as ArtBox.
            // For "new PdfRectangle()", IsZero is true.
            artBox = page.ArtBox;
            artBox.Should().NotBeNull();
            artBox.Should().NotBe(XRect.Empty);
            artBox.IsZero.Should().BeTrue();

            artBox = page.EffectiveArtBoxReadOnly;
            artBox.Should().NotBeNull();
            artBox.Should().NotBe(XRect.Empty);
            artBox.IsZero.Should().BeTrue();

            artBox = page.ArtBoxReadOnly;
            artBox.Should().NotBeNull();
            artBox.Should().NotBe(XRect.Empty);
            artBox.IsZero.Should().BeTrue();

            artBox = page.ArtBox;
            artBox.Should().NotBeNull();
            artBox.Should().NotBe(XRect.Empty);
            artBox.IsZero.Should().BeTrue();

            page.ArtBox = page.MediaBox;

            artBox = page.ArtBox;
            artBox.Should().NotBeNull();
            artBox.Should().NotBe(XRect.Empty);
            artBox.IsZero.Should().BeFalse();

            // ----- BleedBox -----
            var bleedBox = page.BleedBoxReadOnly;
            bleedBox.IsZero.Should().BeTrue();

            bleedBox = page.EffectiveBleedBoxReadOnly;
            bleedBox.Should().NotBe(XRect.Empty);
            bleedBox.IsZero.Should().BeFalse();

            // Calling page.BleedBox sets "new PdfRectangle()" as BleedBox.
            // For "new PdfRectangle()", IsZero is true.
            bleedBox = page.BleedBox;
            bleedBox.Should().NotBeNull();
            bleedBox.Should().NotBe(XRect.Empty);
            bleedBox.IsZero.Should().BeTrue();

            bleedBox = page.EffectiveBleedBoxReadOnly;
            bleedBox.Should().NotBeNull();
            bleedBox.Should().NotBe(XRect.Empty);
            bleedBox.IsZero.Should().BeTrue();

            bleedBox = page.BleedBoxReadOnly;
            bleedBox.Should().NotBeNull();
            bleedBox.Should().NotBe(XRect.Empty);
            bleedBox.IsZero.Should().BeTrue();

            bleedBox = page.BleedBox;
            bleedBox.Should().NotBeNull();
            bleedBox.Should().NotBe(XRect.Empty);
            bleedBox.IsZero.Should().BeTrue();

            page.BleedBox = page.MediaBox;

            bleedBox = page.BleedBox;
            bleedBox.Should().NotBeNull();
            bleedBox.Should().NotBe(XRect.Empty);
            bleedBox.IsZero.Should().BeFalse();

            // ----- TrimBox -----
            var trimBox = page.TrimBoxReadOnly;
            trimBox.IsZero.Should().BeTrue();

            trimBox = page.EffectiveTrimBoxReadOnly;
            trimBox.Should().NotBe(XRect.Empty);
            trimBox.IsZero.Should().BeFalse();

            // Calling page.TrimBox sets "new PdfRectangle()" as TrimBox.
            // For "new PdfRectangle()", IsZero is true.
            trimBox = page.TrimBox;
            trimBox.Should().NotBeNull();
            trimBox.Should().NotBe(XRect.Empty);
            trimBox.IsZero.Should().BeTrue();

            trimBox = page.EffectiveTrimBoxReadOnly;
            trimBox.Should().NotBeNull();
            trimBox.Should().NotBe(XRect.Empty);
            trimBox.IsZero.Should().BeTrue();

            trimBox = page.TrimBoxReadOnly;
            trimBox.Should().NotBeNull();
            trimBox.Should().NotBe(XRect.Empty);
            trimBox.IsZero.Should().BeTrue();

            trimBox = page.TrimBox;
            trimBox.Should().NotBeNull();
            trimBox.Should().NotBe(XRect.Empty);
            trimBox.IsZero.Should().BeTrue();

            page.TrimBox = page.MediaBox;

            trimBox = page.TrimBox;
            trimBox.Should().NotBeNull();
            trimBox.Should().NotBe(XRect.Empty);
            trimBox.IsZero.Should().BeFalse();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

            // Draw two lines with a red default pen.
            var width = page.Width.Point;
            var height = page.Height.Point;
            gfx.DrawLine(XPens.Red, 0, 0, width, height);
            gfx.DrawLine(XPens.Red, width, 0, 0, height);

            // Draw a circle with a red pen which is 1.5 point thick.
            var r = width / 6;
            gfx.DrawEllipse(new XPen(XColors.Red, 1.5), XBrushes.White, new XRect(width / 2 - r, height / 2 - r, 2 * r, 2 * r));

            // Create a font.
            var font = new XFont("Times New Roman", 10, XFontStyleEx.BoldItalic);

#if NET6_0_OR_GREATER
            gfx.DrawString($"Hello, dotnet {Environment.Version.Major}.{Environment.Version.Minor}!", font, XBrushes.Black,
                new XRect(0, 0, width, height), XStringFormats.Center);
#else
            gfx.DrawString("Hello, World!", font, XBrushes.Black,
                new XRect(0, 0, width, height), XStringFormats.Center);
#endif

            // Save the document...
            string filename = PdfFileUtility.GetTempPdfFileName("BasicAllBoxesTest");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Fact]
        public void Create_gradient_brushes()
        {
            // Create a new PDF document.
            var document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";

            // Create an empty page in this document.
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            gfx.Save();
            gfx.TranslateTransform(50, 50);
            gfx.DrawRectangle(null, new XLinearGradientBrush(new XPoint(50, 50), new XPoint(150, 150), XColors.Red, XColors.Green)
            {
                ExtendLeft = false,
                ExtendRight = false
            }, new XRect(0, 0, 200, 200));
            
            gfx.Restore();
            gfx.Save();
            gfx.TranslateTransform(300, 50);
            
            gfx.DrawRectangle(null, new XLinearGradientBrush(new XPoint(50, 50), new XPoint(150, 150), XColors.Red, XColors.Green)
            {
                ExtendLeft = true,
                ExtendRight = true
            }, new XRect(0, 0, 200, 200));
            
            gfx.Restore();
            gfx.Save();
            gfx.TranslateTransform(50, 300);
            
            gfx.DrawRectangle(null, new XRadialGradientBrush(new XPoint(100, 100), new XPoint(100, 100), 50, 100, XColors.Red, XColors.Green)
            {
                ExtendLeft = false,
                ExtendRight = false
            }, new XRect(0, 0, 200, 200));
            
            gfx.Restore();
            gfx.Save();
            gfx.TranslateTransform(300, 300);
            
            gfx.DrawRectangle(null, new XRadialGradientBrush(new XPoint(100, 100), new XPoint(100, 100), 50, 100, XColors.Red, XColors.Green)
            {
                ExtendLeft = true,
                ExtendRight = true
            }, new XRect(0, 0, 200, 200));
            
            gfx.Restore();
            gfx.Save();
            gfx.TranslateTransform(50, 550);
            
            gfx.DrawRectangle(null, new XRadialGradientBrush(new XPoint(50, 50), new XPoint(150, 150), 50, 100, XColors.Red, XColors.Green)
            {
                ExtendLeft = false,
                ExtendRight = false
            }, new XRect(0, 0, 200, 200));
            
            gfx.Restore();
            gfx.Save();
            gfx.TranslateTransform(300, 550);
            
            gfx.DrawRectangle(null, new XRadialGradientBrush(new XPoint(50, 50), new XPoint(150, 150), 50, 100, XColors.Red, XColors.Green)
            {
                ExtendLeft = true,
                ExtendRight = true
            }, new XRect(0, 0, 200, 200));

            // Save the document...
            string filename = PdfFileUtility.GetTempPdfFileName("GradientTest");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }
    }
}
