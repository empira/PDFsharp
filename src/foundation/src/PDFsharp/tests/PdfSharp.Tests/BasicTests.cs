using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests
{
    public class BasicTests
    {
        [Fact]
        public void Create_Hello_World_BasicTests()
        {
            GlobalFontSettings.FontResolver ??= NewFontResolver.Get();

            // Create a new PDF document.
            var document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";

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

            bool newFontResolver = GlobalFontSettings.FontResolver!.GetType()!.FullName!.EndsWith("NewFontResolver");

            // Create a font.
            var font = new XFont(newFontResolver ? "Times New Roman" : "segoe wp", 20, XFontStyleEx.BoldItalic);

            // Draw the text.
            gfx.DrawString("Hello, dotnet 6.0!", font, XBrushes.Black,
                new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);

            // Save the document...
            string filename = PdfFileHelper.CreateTempFileName("HelloWorld");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileHelper.StartPdfViewerIfDebugging(filename);
        }

        [Theory]
        [InlineData(@"C:\Windows\Fonts\seguiemj.ttf")]
        public void RenderEmojis(string fontPath)
        {
            File.Exists(fontPath).Should().BeTrue();

            var fontName = Path.GetFileNameWithoutExtension(fontPath);

            var fontResolver = new ApplicationFontResolver();
            var fontLocations = new List<string>
            {
                @"C:\Windows\Fonts",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Windows\\Fonts")
            };
            foreach (var fontLocation in fontLocations)
            {
                fontResolver.RegisterSearchPath(fontLocation);
            }
            GlobalFontSettings.FontResolver = fontResolver;

            using var document = new PdfDocument();

            var renderFont = new XFont(fontName, 20);
            var brush = new XSolidBrush(XColors.Black);
            var left = 60.0;
            var top = 60.0;
            var gapY = 40.0;
            var x = left;
            var y = top;
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            gfx.DrawString("XxX😢XxX😢XxX😢XxX", renderFont, brush, x, y);
            y += gapY;
            gfx.DrawString("XxX 😞 XxX", renderFont, brush, x, y);
            y += gapY;
            gfx.DrawString("XxX 🤣 XxX", renderFont, brush, x, y);
            y += gapY;
            gfx.DrawString("XxX🤣😞😢XxX", renderFont, brush, x, y);

            gfx.Dispose();

            var outFileName = Path.Combine(Path.GetTempPath(), "emojis.pdf");
            document.Save(outFileName);
        }

        [Theory]
        [InlineData(@"C:\Windows\Fonts\seguiemj.ttf")]  // Segoe UI Emoji font
        public void RenderFontGlyphs(string fontPath)
        {
            File.Exists(fontPath).Should().BeTrue();

            var fontName = Path.GetFileNameWithoutExtension(fontPath);

            var fontResolver = new ApplicationFontResolver();
            var fontLocations = new List<string>
            {
                @"C:\Windows\Fonts",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Windows\\Fonts")
            };
            foreach (var fontLocation in fontLocations)
            {
                fontResolver.RegisterSearchPath(fontLocation);
            }
            GlobalFontSettings.FontResolver = fontResolver;

            using var document = new PdfDocument();

            var renderFont = new XFont(fontName, 16);
            var arialFont = new XFont("Arial", 12);
            var headerFont = new XFont("Arial", 36);
            var brush = new XSolidBrush(XColors.Black);
            var left = 60.0;
            var top = 60.0;
            var bottom = 60.0;
            var gapX = 100.0;
            var gapY = 20.0;
            var x = left;
            var y = top;
            var page = document.AddPage();
            var fullFontName = renderFont.FullFaceName;
            var gfx = XGraphics.FromPdfPage(page);

            gfx.DrawString(fullFontName, headerFont, brush, x, y);
            y += 50;

            var characterList = renderFont.GetSupportedCharacters();
            if (characterList.Any())
            {
                for (var i = 0; i < characterList.Count; i++)
                {
                    var c = characterList[i];
                    gfx.DrawString(c.ToString("X4"), arialFont, brush, x, y);
                    var s = char.ConvertFromUtf32(c);
                    gfx.DrawString(s, renderFont, brush, x + 40, y);
                    x += gapX;
                    if (x + gapX >= page.Width.Point)
                    {
                        x = left;
                        y += gapY;
                        if (y >= page.Height.Point - bottom)
                        {
                            gfx.Dispose();
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            x = left;
                            y = top;
                        }
                    }
                }
            }
            else
                Console.WriteLine($"Font {fontName} has no glyphs");

            gfx.Dispose();

            var outFileName = Path.Combine(Path.GetTempPath(), $"FontGlyphs_{fontName}.pdf");
            document.Save(outFileName);
        }
    }
}
