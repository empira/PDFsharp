// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.Rendering;
using PdfSharp.Diagnostics;
#if CORE
using PdfSharp.Fonts;
#endif
using PdfSharp.Pdf;
using PdfSharp.Quality;
using Xunit;
using FluentAssertions;

namespace MigraDoc.Tests
{
    [Collection("PDFsharp")]
    public class MemoryTests : IDisposable
    {
        public MemoryTests()
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
        public void MigraDoc_MemoryLeak_Test()
        {
            // Background information:
            // We had a bug that a variable used for caching caused
            // a MigraDoc Document object to remain in memory.
            // This test creates a MigraDoc Document, renders a PDF,
            // and finally assures that Document, PdfRenderer, and PdfDocument
            // are disposed after a garbage collection.

            // Create weak references for testing.
            WeakReference<Document> documentReference;
            WeakReference<PdfDocument> pdfDocumentReference;
            WeakReference<PdfDocumentRenderer> pdfDocumentRendererReference;

            // Create a PDF file and assign weak references.
            MyCreateDocument();

            // All weak references refer to objects that are out of scope here.

            // Wait for Garbage Collection.
            //GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForFullGCComplete();

            // Check that all weak references are invalid now.
            documentReference.TryGetTarget(out _).Should().BeFalse();
            pdfDocumentReference.TryGetTarget(out _).Should().BeFalse();
            pdfDocumentRendererReference.TryGetTarget(out _).Should().BeFalse();
            return;

            // Create a PDF file and assign weak references.
            void MyCreateDocument()
            {
                // Create a MigraDoc document.
                var document = new Document();
                documentReference = new(document);

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

                // Create a renderer for the MigraDoc document.
                var pdfRenderer = new PdfDocumentRenderer()
                {
                    // Associate the MigraDoc document with a renderer.
                    Document = document
                };
                pdfDocumentRendererReference = new(pdfRenderer);

                // Layout and render document to PDF.
                pdfRenderer.RenderDocument();
                pdfDocumentReference = new WeakReference<PdfDocument>(pdfRenderer.PdfDocument);

                // Save the document…
                var filename = PdfFileUtility.GetTempPdfFullFileName("unittests/migradoc/text/MigraDocMemoryLeakTest");
                pdfRenderer.PdfDocument.Save(filename);
                // … and start a viewer.
                PdfFileUtility.ShowDocumentIfDebugging(filename);

                // Check that all weak references are valid now.
                documentReference.TryGetTarget(out _).Should().BeTrue();
                pdfDocumentReference.TryGetTarget(out _).Should().BeTrue();
                pdfDocumentRendererReference.TryGetTarget(out _).Should().BeTrue();
            }
        }
    }
}
