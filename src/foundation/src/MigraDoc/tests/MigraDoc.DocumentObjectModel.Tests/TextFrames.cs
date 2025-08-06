// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;
using PdfSharp.Quality;
using PdfSharp.TestHelper;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.Rendering;
#if CORE
#endif
using Xunit;

namespace MigraDoc.DocumentObjectModel.Tests
{
    [Collection("PDFsharp")]
    public class TextFrames : IDisposable
    {
        public TextFrames()
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
        public void Create_TextFrames()
        {
            // Create a MigraDoc document.
            var document = CreateDocument();

            // ----- Unicode encoding in MigraDoc is demonstrated here. -----

            //// A flag indicating whether to create a Unicode PDF or a WinAnsi PDF file.
            //// This setting applies to all fonts used in the PDF document.
            //// This setting has no effect on the RTF renderer.
            //const bool unicode = false;

#if DEBUG_
            var ddl = MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToString(document);
#endif

            // Create a renderer for the MigraDoc document.
            var pdfRenderer = new PdfDocumentRenderer()
            {
                // Associate the MigraDoc document with a renderer.
                Document = document
            };

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("Create_TextFrames");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
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

            paragraph = section.AddParagraph("Hello, World! ");

            // Add some text to the paragraph.
            paragraph.AddFormattedText("Hello, World! ", TextFormat.Italic);
            paragraph.AddFormattedText("Hello, World! ", TextFormat.Underline);
            paragraph.AddFormattedText("Hello, World! ", TextFormat.NotBold);
            paragraph.AddFormattedText("Hello, World! ", TextFormat.Bold | TextFormat.Italic);
            paragraph.AddFormattedText("Hello, World!", TextFormat.NotBold);

            var tf = section.AddTextFrame();
            tf.Left = "4cm";
            tf.FillFormat.Color = Colors.Red;
            tf.Width = "2cm";
            tf.Height = "1cm";

            tf = section.AddTextFrame();
            tf.FillFormat.Color = Colors.LemonChiffon;
            tf.Width = "2cm";
            tf.Height = "1cm";
            tf.AddParagraph("Hello, color!");

            tf = section.AddTextFrame();
            tf.FillFormat.Color = Colors.LightGreen;
            tf.AddParagraph("LightGreen");

            // Create the primary footer.
            var footer = section.Footers.Primary;

            // Add content to footer.
            paragraph = footer.AddParagraph();
            paragraph.Add(new DateField { Format = "yyyy/MM/dd HH:mm:ss" });
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            return document;
        }
    }
}
