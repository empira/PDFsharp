// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using PdfSharp.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace MigraDoc.Tests
{
    [Collection("MGD")]
    public class TextTests
    {
        [Fact]
        public void Surrogate_Pairs_Test()
        {
#if CORE
            GlobalFontSettings.FontResolver = NewFontResolver.Get();
#endif

            // Create a MigraDoc document.
            var document = CreateDocument();

            // ----- Unicode encoding in MigraDoc is demonstrated here. -----

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

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = PdfFileHelper.CreateTempFileName("HelloEmoji");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileHelper.StartPdfViewerIfDebugging(filename);

#if DEBUG___
            MigraDoc.DocumentObjectModel.IO.DdlWriter dw = new MigraDoc.DocumentObjectModel.IO.DdlWriter(filename + "_2.mdddl");
            dw.WriteDocument(document);
            dw.Close();
#endif
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

            paragraph = section.AddParagraph("111😢😞💪");
            paragraph.Format.Font.Name = "Segoe UI Emoji";
            paragraph.AddLineBreak();
            paragraph.AddText("💩💩💩✓✔✅🐛👌🆗🖕 🦄 🦂 🍇 🍆 ☕ 🚂 \U0001f6f8 ☁ ☢ ♌ ♏ ✅ ☑ ✔ ™ 🆒 ◻");

            paragraph = section.AddParagraph("111😢😞💪");
            paragraph.Format.Font.Name = "Segoe UI Emoji";
            paragraph.AddLineBreak();
            paragraph.AddText("💩💩💩✓✔✅ 🐛👌🆗🖕 🦄 🦂 🍇 🍆 ☕ 🚂 \U0001f6f8 ☁ ☢ ♌ ♏ ✅ ☑ ✔ ™ 🆒 ◻" +
                "💩💩💩✓✔✅ 🐛👌🆗🖕 🦄 🦂 🍇 🍆 ☕ 🚂 \U0001f6f8 ☁ ☢ ♌ ♏ ✅ ☑ ✔ ™ 🆒 ◻" +
                "💩💩💩✓✔✅ 🐛👌🆗🖕🖕🖕🖕 🦄 🦂 🍇 🍆 ☕ 🚂 \U0001f6f8 ☁ ☢ ♌ ♏ ✅ ☑ ✔ ™ 🆒 ◻" +
                "💩💩💩✓✔✅ 🐛👌🆗🖕 🦄 🦂 🍇 🍆🍆🍆🍆🍆🍆🍆🍆🍆 ☕ 🚂 \U0001f6f8 ☁ ☢ ♌ ♏ ✅ ☑ ✔ ™ 🆒 ◻" +
                "💩💩💩✓✔✅ 🐛👌🆗🖕 🦄 🦂🦂🦂🦂🦂🦂 🍇 🍆 ☕ 🚂 \U0001f6f8 ☁ ☢ ♌ ♏ ✅ ☑ ✔ ™ 🆒 ◻" +
                "💩💩💩✓✔✅ 🐛👌🆗🖕 🦄 🦂 🍇🍇🍇🍇🍇🍇🍇🍇🍇🍇 🍆 ☕ 🚂 \U0001f6f8 ☁ ☢ ♌ ♏ ✅ ☑ ✔ ™ 🆒 ◻");

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
