// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using PdfSharp.Features.Font;
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;

namespace PdfSharp.Features
{
    class Program
    {
        static void Main(string[] args)
        {
#if CORE
            //GlobalFontSettings.FontResolver = new SegoeWpFontResolver();
            GlobalFontSettings.FontResolver = new FailsafeFontResolver();
#endif
            // Drawing.graphics
            //new Features.Drawing.GraphicsFromImage().t ();

            // Drawing.lines
            // new Features.Drawing.Lines1()..t();

            // === reviewed up to here ===

            // Paths
            //new Drawing.Paths().PathCurves();
            //new Drawing.Paths().PathMisc();
            //new Drawing.Paths().PathShapes();
            //new Drawing.Paths().PathText();

            // Font
            //new FontSelection().HelloWord1Test();
            //new FontSelection()..TestSegoeWpFontResolver();
            //new FontSelection().DefaultFontConstructionTest();
            new FontSelection().FontFamilyConstructionTest();
            //new FontSelection().FrutigerFontsTest();
            //new FontSelection().PlatformFontConstructionTest();
            //new FontResolvers().TestExoticFontResolver();

            //  IO.Info.ReadPdfInfo();
            // Annotations.LinkAnnotations.MergeDocumentsWithLinkAnnotations();

            //IO.ObjectStreams.ReadPdfWithObjectStreams();
            //IO.ObjectStreams.ReadManyPdfsWithObjectStreams();


            //GraphicsFromImage.Test1();

            //Paths.Test1();

            //FontMetrics.TestFontMetrics();

            //RotisWinAnsiTester.RotisWinAnsiTest();
            //RenderInstalledFonts.RenderInstalledFontsTest();
        }
    }
}
