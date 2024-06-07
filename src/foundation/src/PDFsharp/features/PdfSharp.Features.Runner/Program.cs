// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//using Microsoft.Extensions.Logging;
//using PdfSharp.Fonts;
//using PdfSharp.Logging;
//using PdfSharp.Quality;
//using PdfSharp.Snippets.Font;

using Features = PdfSharp.Features.PdfSharpFeatures;

namespace PdfSharp.Features
{
    class Program
    {
        static void Main(string[] args)
        {
            Memory<byte> m = null;
            ReadOnlyMemory<byte> rom = null;
            Span<byte> s = null;
            ReadOnlySpan<byte> ros = null;

            var features = new PdfSharpFeatures();
            // Set a logger factory.
            //Feature.SetDefaultLoggerFactory();

            // ========== Drawing ==========

            // Drawing/paths
            //features[PdfSharpFeatures.Names.Drawing_paths_Paths__PathCurves].Run();
            //features[PdfSharpFeatures.Names.Drawing_paths_Paths__PathMisc].Run();
            //features[PdfSharpFeatures.Names.Drawing_paths_Paths__PathShapes].Run();
            //features[PdfSharpFeatures.Names.Drawing_paths_Paths__PathText].Run();
            features[PdfSharpFeatures.Names.Drawing_paths_Paths__PathWpf].Run();

            // Drawing/graphics
            //features[PdfSharpFeatures.Names.Drawing_graphics_GraphicsUnit__Upwards].Run();

            //features[PdfSharpFeatures.Names.Drawing_graphics_GraphicsUnit__Downwards].Run();

            // Drawing/text
            //features[PdfSharpFeatures.Names.Drawing_text_SurrogateChars__Surrogates].Run();
            //features[PdfSharpFeatures.Names.Drawing_text_SymbolFonts__Symbols].Run();

            //features[PdfSharpFeatures.Names.Font_encoding_Encodings_AnsiEncoding].Run();

            // === reviewed up to here ===

            //Action act = () => new Drawing.Paths().PathCurves();

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

            //new Features.Drawing.NotoSans().Load_all_Noto_Sans();

            // Paths
            //new Drawing.Paths().PathCurves();
            //new Drawing.Paths().PathMisc();
            //new Drawing.Paths().PathShapes();
            //new Drawing.Paths().PathText();

            // Font
            //new FontSelection().HelloWord1Test();
            //new FontResolvers().TestSegoeWpFontResolver();
            //new FontResolvers().TestExoticFontResolver();
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
