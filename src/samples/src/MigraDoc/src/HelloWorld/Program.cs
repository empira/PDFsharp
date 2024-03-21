// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;


// For clarity, we do not use var keyword.
// ReSharper disable SuggestVarOrType_SimpleTypes
#pragma warning disable IDE0090

namespace HelloWorld
{
    class Program
    {
        static void Main()
        {
            if (PdfSharp.Capabilities.Build.IsCoreBuild)
                GlobalFontSettings.FontResolver = new FailsafeFontResolver();

            // Create a MigraDoc document.
            Document document = CreateDocument();

            Style style = document.Styles[StyleNames.Normal]!;
            style.Font.Name = "Arial";

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

            PdfDocument pdfDocument = pdfRenderer.PdfDocument;

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            var fullName = PdfFileUtility.GetTempPdfFullFileName("MigraDocSamples/HelloWorld/HelloWorld" + Capabilities.Build.BuildTag);
            pdfRenderer.PdfDocument.Save(fullName);
            // ...and start a viewer.
            PdfFileUtility.ShowDocument(fullName);
        }

        /// <summary>
        /// Creates an absolutely minimalistic document.
        /// </summary>
        static Document CreateDocument()
        {
            // Create a new MigraDoc document.
            Document document = new Document();

            // Add a section to the document.
            Section section = document.AddSection();

            // Add a paragraph to the section.
            Paragraph paragraph = section.AddParagraph();

            // Set font color.
            paragraph.Format.Font.Color = Colors.DarkBlue;

            // Add some text to the paragraph.
            paragraph.AddFormattedText("Hello, World!", TextFormat.Bold);

            // Create the primary footer.
            HeaderFooter footer = section.Footers.Primary;

            // Add content to footer.
            paragraph = footer.AddParagraph();
            paragraph.Add(new DateField { Format = "yyyy/MM/dd HH:mm:ss" });
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            AddImage(document);

            return document;
        }

        static void AddImage(Document document)
        {
#if GDI
            MigraDoc.DocumentObjectModel.Shapes.
#endif
            Image image = document.LastSection.AddImage(IOUtility.GetAssetsPath("migradoc/images/MigraDoc-landscape.png")!);

            // Width of A4 page is 21cm. Left and right margin is 2.5cm each.
            image.Width = "16cm";
        }
    }
}
