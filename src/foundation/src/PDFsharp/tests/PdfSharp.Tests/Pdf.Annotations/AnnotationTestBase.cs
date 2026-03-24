// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.IO;
using FluentAssertions;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests.Pdf.Annotations
{
    [Collection("PDFsharp")]
    public class AnnotationTestBase : IDisposable
    {
        public AnnotationTestBase()
        {
            // These tests only run under Windows or WSL2.
            GlobalFontSettings.UseWindowsFontsUnderWindows = true;
            GlobalFontSettings.UseWindowsFontsUnderWsl2 = true;
        }

        public void Dispose()
        {
            GlobalFontSettings.ResetAll();
        }

        protected PdfDocument CreateDocument(string name)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 18);
            gfx.DrawString(name, font, XBrushes.DarkBlue, 50, 100);

            return document;
        }
    }
}
