// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using Microsoft.Extensions.Logging;
using PdfSharp.Features.Font;
using PdfSharp.Fonts;
using PdfSharp.Fonts.Internal;
using PdfSharp.Logging;
using PdfSharp.Snippets.Font;

namespace PdfSharp.Features
{
    class Program
    {
        static void Main(string[] args)
        {
            // Set a logger factory.
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    //.AddFilter("Some.Category", LogLevel.Debug)
                    //.AddFilter("PDFsharp", LogLevel.Information)
                    .AddConsole();
            });
            LogHost.Factory = loggerFactory;
#if CORE
            //GlobalFontSettings.FontResolver = new SegoeWpFontResolver();
            GlobalFontSettings.FontResolver = new FailsafeFontResolver();
#endif
            // Drawing.graphics
            //new Features.Drawing.GraphicsFromImage().t ();

            // Drawing.lines
            // new Features.Drawing.Lines1()..t();

            // Drawing.text
            //new Features.Drawing.AutoFontEncoding().Ensure_one_PdfFontDescriptor_per_FontFace();
            //new Features.Drawing.AutoFontEncoding().Test2();
            //new Features.Drawing.AutoFontEncoding().MeasureString_Test();
            //new Features.Drawing.Encodings().Ansi();
            //new Features.Drawing.SurrogateChars().Test1();
            
            
            new Features.Drawing.NotoSans().Load_all_Noto_Sans();
            
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
            //new FontSelection().FontFamilyConstructionTest();
            //new FontSelection().FrutigerFontsTest();
            //new FontSelection().PlatformFontConstructionTest();
            //new FontResolvers().TestExoticFontResolver();

            //  IO.Info.ReadPdfInfo();
            // Annotations.LinkAnnotations.MergeDocumentsWithLinkAnnotations();

            //new IO.LargePdfFiles().LargePdfFileTest();
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
