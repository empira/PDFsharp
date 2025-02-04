// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Quality;
using PdfSharp.TestHelper;
using Xunit;
using FluentAssertions;
using PdfSharp.Diagnostics;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace MigraDoc.Tests.Fonts
{
    [Collection("PDFsharp")]
    public class PredefinedFontsTests
    {

        [Fact]
        public void Use_default_error_font()
        {
            PredefinedFontsAndChars.Reset();
            try
            {
#if CORE
                // The error font is fixed and is created initially when rendering starts. This lets rendering fail if the font is not available.
                // Otherwise, rendering would only fail if an error message is tried to be rendered to the document.
                // Due to this, rendering any document in Core build without an appropriate FontResolver for the error font
                // and without UseWindowsFontsUnderWindows or UseWindowsFontsUnderWsl2 set to true must fail.

                PdfSharpCore.ResetAll();

                var document = CreateDocument();

                // Even without error message RenderDocument() must fail as the font is missing.
                RenderDocumentShouldFail(document);
#elif GDI || WPF
                // In GDI and WPF builds rendering a document must succeed if the error font is available - even is an error message is rendered.
                var document = CreateDocumentWithErrorMessage();

                var filename = PdfFileUtility.GetTempPdfFileName("Use_default_error_font");

                // Even with error message the document must be successfully created.
                var pdfDocument = RenderDocumentShouldSucceed(document, filename);

                CheckErrorFont(pdfDocument, "Courier New");
#else
                throw new NotImplementedException("Unknown flavor.");
#endif
            }
            finally
            {
                PredefinedFontsAndChars.Reset();
            }
        }


        [Fact]
        public void Use_alternate_error_font()
        {
            PredefinedFontsAndChars.Reset();
            try
            {
                PredefinedFontsAndChars.ErrorFontName = "Segoe UI";

#if CORE
                // The error font is fixed and is created initially when rendering starts. This lets rendering fail if the font is not available.
                // Otherwise, rendering would only fail if an error message is tried to be rendered to the document.
                // Due to this, rendering any document in Core build without an appropriate FontResolver for the error font
                // and without UseWindowsFontsUnderWindows or UseWindowsFontsUnderWsl2 set to true must fail.
                var document = CreateDocument();
                
                // Even without error message RenderDocument() must fail as the font is missing.
                RenderDocumentShouldFail(document);
#elif GDI || WPF
                // In GDI and WPF builds rendering a document must succeed if the error font is available - even is an error message is rendered.
                var document = CreateDocumentWithErrorMessage();

                var filename = PdfFileUtility.GetTempPdfFileName("Use_alternate_error_font");

                // Even with error message the document must be successfully created.
                var pdfDocument = RenderDocumentShouldSucceed(document, filename);

                CheckErrorFont(pdfDocument, "Segoe UI");
#else
                throw new NotImplementedException("Unknown flavor.");
#endif
            }
            finally
            {
                PredefinedFontsAndChars.Reset();
            }
        }

        [Fact]
        public void Fail_with_missing_error_font()
        {
            PredefinedFontsAndChars.Reset();
            try
            {
                PredefinedFontsAndChars.ErrorFontName = "Unknown font name";

                // The error font is fixed and is created initially when rendering starts. This lets rendering fail if the font is not available.
                // Otherwise, rendering would only fail if an error message is tried to be rendered to the document.
                // Due to this, rendering any document with an unavailable error font must fail.
                var document = CreateDocument();

                // Even without error message RenderDocument() must fail as the font is missing.
                RenderDocumentShouldFail(document);
            }
            finally
            {
                PredefinedFontsAndChars.Reset();
            }
        }

        [Fact]
        public void Use_default_bullets_font()
        {
            PredefinedFontsAndChars.Reset();
            try
            {
#if CORE
                // Unlike the error font, bullet fonts are not fixed as their style depends on the context.
                // For that reason there is no bullet font created when rendering starts and only documents with bullet lists may fail if a font is not available.
                // Rendering a document with a bullet list in Core build without an appropriate FontResolver for the bullet fonts
                // and without UseWindowsFontsUnderWindows or UseWindowsFontsUnderWsl2 set to true must fail.

                PdfSharpCore.ResetAll();

                var document = CreateDocumentWithBulletList();

                // RenderDocument() must fail as the font is missing.
                RenderDocumentShouldFail(document);
#elif GDI || WPF
                // In GDI and WPF builds rendering a document with bullet lists must succeed if the bullet fonts are available.
                var document = CreateDocumentWithBulletList();

                var filename = PdfFileUtility.GetTempPdfFileName("Use_default_bullets_font");

                // The document must be successfully created.
                var pdfDocument = RenderDocumentShouldSucceed(document, filename);

                CheckBulletFonts(pdfDocument, "Courier New", "Courier New", "Courier New");
#else
                throw new NotImplementedException("Unknown flavor.");
#endif
            }
            finally
            {
                PredefinedFontsAndChars.Reset();
            }
        }

        [Fact]
        public void Use_alternate_bullets_font()
        {
            PredefinedFontsAndChars.Reset();
            try
            {
                PredefinedFontsAndChars.Bullets.Level1FontName = "Arial";
                PredefinedFontsAndChars.Bullets.Level2FontName = "Times new roman";
                PredefinedFontsAndChars.Bullets.Level3FontName = "Verdana";
#if CORE
                // Unlike the error font, bullet fonts are not fixed as their style depends on the context.
                // For that reason there is no bullet font created when rendering starts and only documents with bullet lists may fail if a font is not available.
                // Rendering a document with a bullet list in Core build without an appropriate FontResolver for the bullet fonts
                // and without UseWindowsFontsUnderWindows or UseWindowsFontsUnderWsl2 set to true must fail.
                var document = CreateDocumentWithBulletList();

                // RenderDocument() must fail as the fonts are missing.
                RenderDocumentShouldFail(document);
#elif GDI || WPF
                // In GDI and WPF builds rendering a document with bullet lists must succeed if the bullet fonts are available.
                var document = CreateDocumentWithBulletList();

                var filename = PdfFileUtility.GetTempPdfFileName("Use_alternate_bullets_font");

                // The document must be successfully created.
                var pdfDocument = RenderDocumentShouldSucceed(document, filename);

                CheckBulletFonts(pdfDocument, "Arial", "Times new roman", "Verdana");
#else
                throw new NotImplementedException("Unknown flavor.");
#endif
            }
            finally
            {
                PredefinedFontsAndChars.Reset();
            }
        }

        [Fact]
        public void Fail_with_missing_bullets_font()
        {
            PredefinedFontsAndChars.Reset();
            try
            {
                PredefinedFontsAndChars.Bullets.Level1FontName = "Unknown font name";
                PredefinedFontsAndChars.Bullets.Level2FontName = "Unknown font name";
                PredefinedFontsAndChars.Bullets.Level3FontName = "Unknown font name";
                
                // Unlike the error font, bullet fonts are not fixed as their style depends on the context.
                // For that reason there is no bullet font created when rendering starts and only documents with bullet lists may fail if a font is not available.
                // Rendering a document with a bullet list with an unavailable bullet font must fail.
                var document = CreateDocumentWithBulletList();

                // RenderDocument() must fail as the font is missing.
                RenderDocumentShouldFail(document);
            }
            finally
            {
                PredefinedFontsAndChars.Reset();
            }
        }

        void RenderDocumentShouldFail(Document document)
        {
            InvalidOperationException? exception = null;
            try
            {
                // Create a renderer for the MigraDoc document.
                var pdfRenderer = new PdfDocumentRenderer()
                {
                    // Associate the MigraDoc document with a renderer.
                    Document = document
                };

                // Layout and render document to PDF.
                pdfRenderer.RenderDocument();

            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }
            exception.Should().NotBeNull();
        }

#if GDI || WPF
        PdfDocument RenderDocumentShouldSucceed(Document document, string filename)
        {
            // Create a renderer for the MigraDoc document.
            var pdfRenderer = new PdfDocumentRenderer()
            {
                // Associate the MigraDoc document with a renderer.
                Document = document,
                PdfDocument = { Options = { CompressContentStreams = false } }
            };

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            var pdfDocument = pdfRenderer.PdfDocument;

            // Save the document...
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);

            return pdfDocument;
        }

        void CheckErrorFont(PdfDocument pdfDocument, string expectedFontName)
        {
            // Check font of the error message.
            var fontName = PdfFileHelper.GetCurrentFontName(0, pdfDocument, streamEnumerator =>
            {
                // Move to "Image '???' not found.".
                streamEnumerator.Text.MoveAndGetNext(x => x.Text == "Image '???' not found.", true, out _).Should().BeTrue();
            });
            fontName.ToLower().Should().Be(expectedFontName.ToLower());
        }

        void CheckBulletFonts(PdfDocument pdfDocument, string expectedBulletLevel1FontName, string expectedBulletLevel2FontName, string expectedBulletLevel3FontName)
        {
            // Check font of the bullet level 1.
            var bulletLevel1FontName = PdfFileHelper.GetCurrentFontName(0, pdfDocument, streamEnumerator =>
            {
                // Move to text "Level1".
                streamEnumerator.Text.MoveAndGetNext(x => x.Text == "Level1", true, out _).Should().BeTrue();
                // The bullet is the previous hex text.
                streamEnumerator.Text.MoveAndGetPrevious(x => x.IsHex, true, out _).Should().BeTrue();
            });
            bulletLevel1FontName.ToLower().Should().Be(expectedBulletLevel1FontName.ToLower());

            // Check font of the bullet level 2.
            var bulletLevel2FontName = PdfFileHelper.GetCurrentFontName(0, pdfDocument, streamEnumerator =>
            {
                // Move to text "Level2".
                streamEnumerator.Text.MoveAndGetNext(x => x.Text == "Level2", true, out _).Should().BeTrue();
                // The bullet is the previous hex text.
                streamEnumerator.Text.MoveAndGetPrevious(x => x.IsHex, true, out _).Should().BeTrue();
            });
            bulletLevel2FontName.ToLower().Should().Be(expectedBulletLevel2FontName.ToLower());

            // Check font of the bullet level 3.
            var bulletLevel3FontName = PdfFileHelper.GetCurrentFontName(0, pdfDocument, streamEnumerator =>
            {
                // Move to text "Level3".
                streamEnumerator.Text.MoveAndGetNext(x => x.Text == "Level3", true, out _).Should().BeTrue();
                // The bullet is the previous hex text.
                streamEnumerator.Text.MoveAndGetPrevious(x => x.IsHex, true, out _).Should().BeTrue();
            });
            bulletLevel3FontName.ToLower().Should().Be(expectedBulletLevel3FontName.ToLower());
        }
#endif

        /// <summary>
        /// Creates an absolutely minimalistic document.
        /// Rendering any document with a missing error font must fail, as the font is created initially when rendering starts.
        /// </summary>
        static Document CreateDocument()
        {
            // Create a new MigraDoc document.
            var document = new Document();

            // Add a section to the document.
            var section = document.AddSection();

            // Add a paragraph to the section.
            section.AddParagraph("Simple document");

            return document;
        }

#if GDI || WPF
        /// <summary>
        /// Creates a document with an error message.
        /// Not necessary in Core build as any document without an appropriate FontResolver for the error font
        /// and without UseWindowsFontsUnderWindows or UseWindowsFontsUnderWsl2 set must fail.
        /// </summary>
        static Document CreateDocumentWithErrorMessage()
        {
            // Create a new MigraDoc document.
            var document = new Document();

            // Add a section to the document.
            var section = document.AddSection();

            // Add a not existing image to force error message.
            section.AddImage("not-existing-image");

            return document;
        }
#endif

        /// <summary>
        /// Creates a document with bullet list.
        /// Unlike the error font, bullet fonts are not created when rendering starts.
        /// Due to this a document must contain a bullet list, to check the availability of the bullet fonts.
        /// </summary>
        static Document CreateDocumentWithBulletList()
        {
            // Create a new MigraDoc document.
            var document = new Document();

            // Style for list level 1.
            const string listLevel1StyleName = "ListLevel1";
            var style = document.Styles.AddStyle(listLevel1StyleName, StyleNames.List);
            style.ParagraphFormat.ListInfo.ListType = ListType.BulletList1;
            style.ParagraphFormat.ListInfo.NumberPosition = Unit.Zero;
            style.ParagraphFormat.LeftIndent = Unit.FromCentimeter(0.5);

            // Style for list level 2.
            const string listLevel2StyleName = "ListLevel2";
            style = document.Styles.AddStyle(listLevel2StyleName, StyleNames.List);
            style.ParagraphFormat.ListInfo.ListType = ListType.BulletList2;
            style.ParagraphFormat.ListInfo.NumberPosition = Unit.FromCentimeter(0.5);
            style.ParagraphFormat.LeftIndent = Unit.FromCentimeter(1);

            // Style for list level 3.
            const string listLevel3StyleName = "ListLevel3";
            style = document.Styles.AddStyle(listLevel3StyleName, StyleNames.List);
            style.ParagraphFormat.ListInfo.ListType = ListType.BulletList3;
            style.ParagraphFormat.ListInfo.NumberPosition = Unit.FromCentimeter(1);
            style.ParagraphFormat.LeftIndent = Unit.FromCentimeter(1.5);

            // Add a section to the document.
            var section = document.AddSection();

            // Add a list item of level 1.
            section.AddParagraph("Level1", listLevel1StyleName);

            // Add a list item of level 2.
            section.AddParagraph("Level2", listLevel2StyleName);

            // Add a list item of level 3.
            section.AddParagraph("Level3", listLevel3StyleName);


            return document;
        }
    }
}
