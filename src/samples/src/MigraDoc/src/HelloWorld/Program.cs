// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Snippets.Font;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            if (PdfSharp.Capabilities.Build.IsCoreBuild)
                GlobalFontSettings.FontResolver = new SegoeWpFontResolver();

            // Create a MigraDoc document.
            var document = CreateDocument();

            var style = document.Styles[StyleNames.Normal]!;
            style.Font.Name = "Segoe WP";

            // ----- Unicode encoding and font program embedding in MigraDoc is demonstrated here. -----

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

#if true
            pdfRenderer.PdfDocument = new PdfDocument();
            pdfRenderer.PdfDocument.Options.UseFlateDecoderForJpegImages = PdfUseFlateDecoderForJpegImages.Automatic;
#if true
            // Used to get FlateEncoded JPEG images.
            pdfRenderer.PdfDocument.Options.FlateEncodeMode = PdfFlateEncodeMode.BestCompression;
#else
            // Used to get CCITT-encoded images.
            pdfRenderer.PdfDocument.Options.FlateEncodeMode = PdfFlateEncodeMode.BestSpeed;
#endif
            pdfRenderer.PdfDocument.Options.EnableCcittCompressionForBilevelImages = true;
            pdfRenderer.PdfDocument.Options.CompressContentStreams = true;
            pdfRenderer.PdfDocument.Options.NoCompression = false;
#endif

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            //const string filename = "HelloWorld.pdf";
            var filename = $"HelloWorld-{Guid.NewGuid().ToString("N").ToUpperInvariant()}_tempfile.pdf";
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
        }

        /// <summary>
        /// Creates an absolutely minimalistic document.
        /// </summary>
        static Document CreateDocument()
        {
            // Create a new MigraDoc document.
            var document = new Document();

            // Add a section to the document.
            var section = document.AddSection();

            // Add a paragraph to the section.
            var paragraph = section.AddParagraph();

            // Set font color.
            //paragraph.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);
            paragraph.Format.Font.Color = Colors.DarkBlue;

            // Add some text to the paragraph.
            paragraph.AddFormattedText("Hello, World!", TextFormat.Bold);

            // Create the primary footer.
            var footer = section.Footers.Primary;

            // Add content to footer.
            paragraph = footer.AddParagraph();
            paragraph.Add(new DateField { Format = "yyyy/MM/dd HH:mm:ss" });
            paragraph.Format.Alignment = ParagraphAlignment.Center;

#if true_
#warning THHO Test code - to be removed.
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\windows7problem.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\TruecolorNoAlpha.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\truecolorA.jpg"; // OK
            // Now also works with GDI build.
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\PowerBooks_CMYK.jpg"; // OK. Starts with EXIF header, not JFIF header.
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\indexedmonoA.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\grayscaleNoAlpha.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\grayscaleA.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\color8A.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\color4A.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\blackwhiteA.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\Balloons_CMYK.jpg"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\jpeg\Balloons_CMYK - Copy.jpg"; // OK

            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\BlackwhiteA.bmp"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\BlackwhiteA2.bmp"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\BlackwhiteTXT.bmp"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\Color4A.bmp"; // OK
            var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\Color8A.bmp"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\GrayscaleA.bmp"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\IndexedmonoA.bmp"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\Test_OS2.bmp"; // NYI! => WPF and GDI builds only.
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\TruecolorA.bmp"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\bmp\TruecolorMSPaint.bmp"; // OK

            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\windows7problem.png"; // NYI
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\truecolorAlpha.png"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\truecolorA.png"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\indexedmonoA.png"; // NYI
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\grayscaleAlpha.png"; // NYI
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\grayscaleA.png"; // NYI
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\color8A.png"; // OK
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\png\color4A.png"; // NYI
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\blackwhiteA.png"; // NYI

            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\MigraDoc.bmp"; // ARGB32
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\Logo landscape.bmp"; // RGB24
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\Logo landscape MS Paint.bmp"; // RGB24
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\Logo landscape 256.bmp"; // Palette8
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\MigraDoc.png"; // ARGB32
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\Logo landscape.png"; // RGB24
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\Logo landscape 256.png"; // Palette8

            // ===== Test images for WPF and GDI builds =====
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\misc\image009.gif"; // ???
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\misc\Rose (RGB 8).tif"; // ???
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\misc\Test.gif"; // ???
            //var imagePath = @"..\..\..\..\..\..\..\..\..\assets\PDFsharp\images\samples\misc\Test.png"; // ???

            var logo = document.LastSection.AddImage(imagePath);
#else
            AddImage(document);
#endif

            return document;
        }

        private static void AddImage(Document document)
        {
            var base64 = @"base64:iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAMAAACdt4HsAAACUlBMVEUAAAABAAACAQADAgAHBAAH
BQANCAALCQARCwARDgAcFQAeFQAeGAAtHAApHgAoHwAuIQAvJQAtJwA4KAA/KQA3LABBLwBGMQBC
NABANwD/AABGOQBSNwD/BABTOAD/BQD/CAD/CQD/DABbPQBPQQD/DQD/EAD/EQBTRgD/FAD/FQBl
RQD/GQD/GgBaTAD/HQD/HgD/IQD/IgD/JQD/JgBlVAD/KQD/KgB3VAD/LQD/LgB1WAB0WQD/MgCI
VgD/MwD/NgD/NwB4YgD/OgD/OwCDYQD/PgD/PwCCZQCKYwD/QgD/QwD/RwCbZQD/SACSaQD/SwD/
TACLbwD/TwD/UAD/UwCRdAD/VACJeQD/VwD/WACtdAD/XACrdQD/YAD/YQCzeAD/ZAD/ZQD/aAD/
aQCchwD/bAD/bQD/cAD/cQDAhACojAD/dQD/dgD/eQD/egCulAD/fQD/fgD/gQCzmAD/ggD/hQD/
hgDTlADikAD/iQD/igDPnAD/jgDBoQD/jwDOngD/kgD/kwD/lgD/lwDvnADdogD/mgDjowD/mwD/
ngD/nwDUrQDpqADcrQD/owD/pAD4pwD2qAD/pwD/qAD/qwD/rAD+rwDktwD/rwD/sAD/swDougD/
tAD/twD/uAD/vAD/vQD/wADjyQD/wQD/xAD/xQD/yAD/yQD1zQDw0AD/zAD/zQD/0QD50wD/0gD+
1AD/1QD/1gD/2QD/2gD/3QD/3gD/4QD/4gD/5QD/5gD/6gD/6wD/7gD/7wD/8gD/8wD/9gD/9wD/
+gD/+wD//wBS8BVmAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAB3RJTUUH3gsbDiwNWTYMaQAAAB1p
VFh0Q29tbWVudAAAAAAAQ3JlYXRlZCB3aXRoIEdJTVBkLmUHAAAFpUlEQVRYw3VXiV/URRydsNPu
u+gALJYgTRGDsgMIwsIyaSWii1wIqAQSYiuKkKO2sBuhFgJjk9UCMmGXq2WRNv+vZub7nZnvzG+Z
P+B9vsd77/uGZWZm5eT6CnbtLtxXsv+pssoDB184fKT2tTfeOtrY8u6x9uPdH33a2z8YOvn9qdHx
ydO/RWbOzc1fWIwvr/2zvvHvf5cusczM+7J3+Ap2AkB5ZdXBQ4f9dfUcoIkDdBzvkgBfC4AwBzhj
AJIEIK9g557ComIBwCuo8fMKGgCgUwAMDHGAEQQ4ywFi8ZW1xPrGZkoCyBZkBY9jC37ZQlOzaAEB
hjkAtjDLAZZWeAsXVQU5soW9cgYVooIjdTiDtnZZQb+sYGxcV7DoAOSKFvYWwRCrOQAMUVbQHVQz
GAuTIXKApAbIFgC75QxwC6KCQFOLaqF/KDSsKoie0xVsmBbyrBY4gN4CtqDWOI0VLIkhXrS2wIeo
tgA8CMghdgoeDAzqLUTIDDY2dQs+BQAV+Gvr9Qy6ggpgzGxhcUnyAAGycAZAJAOgZ9DXbwBwBnG5
BRtAzqAcKpBrlADYQuik4kGUt7DAAVYTSdICABTvLxVUVgCNugLJRLpGSSS+xpR3C5SJsoKuoBZT
2J4BWSNUUOSoEcQEauRURjVGoYJlSiRVAdkCEKn1GFB5YBC0MIEzkENMOFRWRKqqpkRqN3IWW5i6
g7G75vQabSLhFqo0kRpbbCKNhSe+yti+PeMXJFLSZWJRyRZM7FNUnniePfEke/m83EJC80DNQKsR
xERbgDWOT97EPjvBbp5fSOsHhUZMZo0d1ho/ZjdEojeyb9whOi0cokwkch4NP8aejcw8x56RlkaG
mJNrAMCRah0qD0kq/3wt+zIS/Y5d93csvrwqtKA8kbewizuSNhTpyspQUI0/nBp9j90qiHQ7+zwG
TNRiMn5QBmusI2tUMxgZfZi9NM0BXmGPLloAuAUzxBoxgwYjJgT48ept34oKft12zV8wg810TORD
rCFqFBX0AcDr7E6phdl72AeSB2qIWdlGC4KJsIUGcKTOLiWmkXuZfg/FLR5k77AdSdt6K+cBMJET
6YvLDcAVf4IWUpatoyNV4xbUDILIxBfZ/dLSzs7OP8jeWXGOaz6/jcKRYAt+che6lRpvY2+CnGfn
32d3r6x67kJhmtOm/IBX8MllV/6kbP2PqzJ+J0wUppoPl6m0rFKtUTBRziDYI9f4NHsAbJ1XcOER
9qrVQq6PzMBooZlepuvZ28bSPmS3rBkqE08sVadNE4kykVymuCPnPB1xKmgL2lDIaQNTjcMQUx45
g6H4UQutMuL04HHllqYAFjGhOGIqUYeFbMGsEQ9LVN4FAZBMV4Gkcg3hAW1BnzYMGB4xFVszEC20
oaVBxAlPTtnH1Y55cBsrdMQJGDkPDKKpTpHbSFxZEYmcd5uJeFic47rutGA8Ud6FhoA5rgNWyCIx
L+UJmngXai1D6SUBI2JClt5CFsa8fZapNgQ8rmwnlERyy8tUAy00wnkPmtNGzju9jXbEMURqMnIe
DKl8oHPiqjvEPUBlctqEJ3YYJuoZzPLbCC0QOeejGstJUlU86On1iMmJuhi2TcQxnthBiMTP+xTN
yjQj+WTYVo6kxNSs7gL+F+yoyyuwDcWdQaApLRNFCwsxO6UpWy82570e1tjmZCTliU4LnIn55tOl
QxbxA0Mk2cICfjg2U3bIKoZPl3YkctpCNpVjngq0I1XYQZPOgFcwNa0AltMAEDViBa3qyyNbwCGK
sB1zeJBLWjhAwnYrzoCk9TMzOupqOWd5wjb9uXZBThzWM5AVIA+2uI1KTM0mYNAfy5z7b9QfT7sF
JWfyZ5owLazYIUtQGXlQ5VwmklTDYgv042nLmX77as23r5v8XCdOT/OAMXfew0TjB0TOR52AIXng
VeP/8gP+s//MzMQAAAAASUVORK5CYII=
";

            var logo = document.LastSection.AddImage(base64);
        }
    }
}
