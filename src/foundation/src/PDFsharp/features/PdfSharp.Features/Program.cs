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


            //var doc = PdfReader.Open(@"C:\Users\StLa\Desktop\slide0.pdf");
            //var doc = PdfReader.Open(@"C:\Users\StLa\Desktop\04_Cloud-UseCase.pdf");
            //doc.Save(@"C:\Users\StLa\Desktop\04_Cloud-UseCase_.pdf");
            //doc.GetType();


            // Paths
            //Features.Drawing.Paths.PathCurves();
            //Features.Drawing.Paths.PathText();
            //Features.Drawing.Paths.PathText();
            //Features.Drawing.Paths.PathText();

            // Font
            //FontSelection.HelloWord1Test();
            //FontResolvers.TestSegoeWpFontResolver();
            //FontSelection.DefaultFontConstructionTest();
            //FontSelection.FontFamilyConstructionTest();
            //FontSelection.FrutigerFontsTest();
            FontSelection.PlatformFontConstructionTest();
            //FontResolvers.TestExoticFontResolver();


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
