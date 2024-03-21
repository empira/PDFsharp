// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Font
{
    public class ExoticFontResolverSnippet : Snippet
    {
        public override void RenderSnippet(XGraphics gfx)
        {
            GlobalFontSettings.FontResolver = new ExoticFontsFontResolver();

            XPdfFontOptions options = XPdfFontOptions.WinAnsiDefault;

            // Create a font.
            const double fontSize = 16;
            var times = new XFont("Times New Roman", fontSize, XFontStyleEx.Regular, options);
            _ = times;

            var obliviousFont = new XFont("Oblivious", fontSize, XFontStyleEx.Regular, options);
            var xFilesFont = new XFont("XFiles", fontSize, XFontStyleEx.Regular, options);

            var obliviousItalicFont = new XFont("Oblivious", fontSize, XFontStyleEx.Italic, options);
            var xFilesItalicFont = new XFont("XFiles", fontSize, XFontStyleEx.Italic, options);

            // Draw the text.
            string text = "Sphinx ";
            double x = 40;
            double y = 50;
            double dy = 35;

            gfx.DrawString(text + "(Oblivious - regular)", obliviousFont, XBrushes.Black, x, y);
            y += dy;

            gfx.DrawString(text + "(Oblivious - italic simulated)", obliviousItalicFont, XBrushes.Black, x, y);
            y += dy;

            gfx.DrawString(text + "(X-Files - regular)", xFilesFont, XBrushes.Black, x, y);
            y += dy;

            gfx.DrawString(text + "(X-Files - italic simulated)", xFilesItalicFont, XBrushes.Black, x, y);
            //y += dy;
        }
    }
}
