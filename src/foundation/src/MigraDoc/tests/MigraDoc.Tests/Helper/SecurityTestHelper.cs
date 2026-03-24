// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Quality;
using System.IO;
using static PdfSharp.TestHelper.SecurityTestHelper;

namespace MigraDoc.Tests.Helper
{
    static class SecurityTestHelper
    {
        public static Document CreateEmptyTestDocument()
        {
            var doc = new Document();
            return doc;
        }

        public static Document CreateStandardTestDocument()
        {
            var doc = CreateEmptyTestDocument();
            doc.AddSection().AddParagraph().AddText("Text");

            return doc;
        }

        public static PdfDocumentRenderer RenderDocument(Document document)
        {
            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();

            return pdfRenderer;
        }

        public static PdfDocumentRenderer RenderSecuredDocument(Document document, TestOptions options)
        {
            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();

            SecureDocument(pdfRenderer.PdfDocument, options);

            return pdfRenderer;
        }

        public static void WriteSecuredTestDocument(Document document, string filename, TestOptions options)
        {
            var pdfRenderer = RenderSecuredDocument(document, options);
            pdfRenderer.Save(filename);
        }

        public static PdfDocumentRenderer RenderSecuredStandardTestDocument(TestOptions options)
        {
            return RenderSecuredDocument(CreateStandardTestDocument(), options);
        }

        public static void WriteStandardTestDocument(Stream stream)
        {
            var pdfRenderer = RenderDocument(CreateStandardTestDocument());
            pdfRenderer.Save(stream, false);
        }

        public static Stream GetStandardTestDocument()
        {
            var stream = CreateTempStream("unittests/SecurityTests/StandardTestDocument");
            WriteStandardTestDocument(stream);
            return stream;
        }

        public static void WriteSecuredStandardTestDocument(Stream stream, TestOptions options)
        {
            var pdfRenderer = RenderSecuredStandardTestDocument(options);
            pdfRenderer.Save(stream, false);
        }

        public static Stream GetSecuredStandardTestDocument(TestOptions options)
        {
            Stream stream;
            if (options.Encryption == TestEncryptions.V5R5ReadOnly)
            {
                // Open file from Assets.
                GetAssetsTestFile(options, out var filename);
                stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            else
            {
                // Render secured test document.
                stream = CreateTempStream("unittests/SecurityTests/SecuredStandardTestDocument");
                WriteSecuredStandardTestDocument(stream, options);
            }
            return stream;
        }
    }
}
