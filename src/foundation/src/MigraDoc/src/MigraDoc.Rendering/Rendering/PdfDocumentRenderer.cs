// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
#if WPF
using System.IO;
#endif
using System.Reflection;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering.Resources;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Provides the functionality to convert a MigraDoc document into PDF.
    /// </summary>
    public class PdfDocumentRenderer
    {
        /// <summary>
        /// Initializes a new instance of the PdfDocumentRenderer class.
        /// </summary>
        public PdfDocumentRenderer()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDocumentRenderer"/> class.
        /// </summary>
        /// <param name="unicode">If true Unicode encoding is used for all text. If false, WinAnsi encoding is used.</param>
        [Obsolete("Code is always unicode.")]
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        public PdfDocumentRenderer(bool unicode)
        {
            if (unicode is false)
                throw new ArgumentException("Text is always rendered as unicode.");
        }

        /// <summary>
        /// Gets a value indicating whether the text is rendered as Unicode.
        /// Returns true because text is rendered always in unicode.
        /// </summary>
        [Obsolete("Code is always unicode.")]
        public bool Unicode => true;

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        /// <value>The language.</value>
        public string Language
        {
            get => _language;
            set => _language = value;
        }
        string _language = "";

        /// <summary>
        /// Set the MigraDoc document to be rendered by this printer.
        /// </summary>
        public Document Document
        {
            set
            {
                _document = null;
                value.BindToRenderer(this);
                _document = value;
            }
        }
        Document? _document;

        /// <summary>
        /// Gets or sets a document renderer.
        /// </summary>
        /// <remarks>
        /// A document renderer is automatically created and prepared
        /// when printing before this property was set.
        /// </remarks>
        public DocumentRenderer DocumentRenderer
        {
            get
            {
                if (_documentRenderer == null)
                    PrepareDocumentRenderer();

                Debug.Assert(_documentRenderer is not null);

                return _documentRenderer;
            }
            set => _documentRenderer = value;
        }
        DocumentRenderer? _documentRenderer;

        void PrepareDocumentRenderer()
            => PrepareDocumentRenderer(false);

        void PrepareDocumentRenderer(bool prepareCompletely)
        {
            if (_document == null)
                throw new InvalidOperationException(Messages2.PropertyNotSetBefore("DocumentRenderer", MethodBase.GetCurrentMethod()!.Name));

            _documentRenderer ??= new(_document)
            {
                WorkingDirectory = _workingDirectory! // BUG  ?? NRT.ThrowOnNull<string>()
            };

            if (prepareCompletely && _documentRenderer.FormattedDocument == null!)
                _documentRenderer.PrepareDocument();
        }

        /// <summary>
        /// Renders the document into a PdfDocument containing all pages of the document.
        /// </summary>
        public void RenderDocument()
        {
#if true
            PrepareRenderPages();
#else
            if (this.documentRenderer == null)
                PrepareDocumentRenderer();

            if (this.pdfDocument == null)
            {
                this.pdfDocument = new PdfDocument();
                this.pdfDocument.Info.Creator = VersionInfo.Creator;
            }

            WriteDocumentInformation();
#endif
            RenderPages(1, DocumentRenderer.FormattedDocument.PageCount);
        }

        /// <summary>
        /// Renders the document into a PdfDocument containing all pages of the document.
        /// </summary>
        public void PrepareRenderPages()
        {
            //if (this.documentRenderer == null)
            PrepareDocumentRenderer(true);

            if (_pdfDocument == null)
            {
                _pdfDocument = CreatePdfDocument();
                if (_document?.UseCmykColor ?? NRT.ThrowOnNull<bool>())
                    _pdfDocument.Options.ColorMode = PdfColorMode.Cmyk;
            }

            // Add embedded files, that are defined in MigraDoc _document to PDFsharp _pdfDocument.
            foreach (var item in _document?.EmbeddedFiles ?? NRT.ThrowOnNull<EmbeddedFiles>())
            {
                if (item as EmbeddedFile is { } embeddedFile)
                    _pdfDocument.AddEmbeddedFile(embeddedFile.Name, embeddedFile.Path);
                else
                    NRT.ThrowOnNull<EmbeddedFile>();
            }

            WriteDocumentInformation();
            //RenderPages(1, this.documentRenderer.FormattedDocument.PageCount);
        }

        /// <summary>
        /// Gets the count of pages.
        /// </summary>
        public int PageCount => _documentRenderer?.FormattedDocument.PageCount ?? NRT.ThrowOnNull<int>();

        /// <summary>
        /// Saves the PdfDocument to the specified path. If a file already exists, it will be overwritten.
        /// </summary>
        public void Save(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path == "")
                throw new ArgumentException("PDF file path must not be empty.");

            if (_workingDirectory != null)
                path = Path.Combine(_workingDirectory, path);

            PdfDocument.Save(path);
        }

        /// <summary>
        /// Saves the PDF document to the specified stream.
        /// </summary>
        public void Save(Stream stream, bool closeStream)
        {
            PdfDocument.Save(stream, closeStream);
        }

        /// <summary>
        /// Renders the specified page range.
        /// </summary>
        /// <param name="startPage">The first page to print.</param>
        /// <param name="endPage">The last page to print.</param>
        public void RenderPages(int startPage, int endPage)
        {
            if (startPage < 1)
                throw new ArgumentOutOfRangeException(nameof(startPage));

            if (endPage > (_documentRenderer?.FormattedDocument.PageCount ?? NRT.ThrowOnNull<int>()))
                throw new ArgumentOutOfRangeException(nameof(endPage));

            if (_documentRenderer == null)
                PrepareDocumentRenderer();

            _pdfDocument ??= CreatePdfDocument();

            DocumentRenderer.PrintDate = DateTime.Now;
            for (int pageNr = startPage; pageNr <= endPage; ++pageNr)
            {
                var pdfPage = _pdfDocument.AddPage();
                var pageInfo = DocumentRenderer.FormattedDocument.GetPageInfo(pageNr);
                pdfPage.Width = pageInfo.Width;
                pdfPage.Height = pageInfo.Height;
                pdfPage.Orientation = pageInfo.Orientation;

                using var gfx = XGraphics.FromPdfPage(pdfPage);
                DocumentRenderer.RenderPage(gfx, pageNr);
            }
        }

        /// <summary>
        /// Gets or sets a working directory for the printing process.
        /// </summary>
        public string WorkingDirectory
        {
            get => _workingDirectory ?? NRT.ThrowOnNull<string>();
            set => _workingDirectory = value;
        }
        string? _workingDirectory;

        /// <summary>
        /// Gets or sets the PDF document to render on.
        /// </summary>
        /// <remarks>A PDF document in memory is automatically created when printing before this property was set.</remarks>
        public PdfDocument PdfDocument
        {
            get => _pdfDocument ?? NRT.ThrowOnNull<PdfDocument>();
            set => _pdfDocument = value;
        }
        PdfDocument? _pdfDocument;

        /// <summary>
        /// Writes document information like author and subject to the PDF document.
        /// </summary>
        public void WriteDocumentInformation()
        {
            if (!_document!.Values.Info.IsValueNullOrEmpty())
            {
                var docInfo = _document.Info;
                var pdfInfo = _pdfDocument!.Info;

                if (!docInfo.Values.Author.IsValueNullOrEmpty())
                    pdfInfo.Author = docInfo.Author;

                if (!docInfo.Values.Keywords.IsValueNullOrEmpty())
                    pdfInfo.Keywords = docInfo.Keywords;

                if (!docInfo.Values.Subject.IsValueNullOrEmpty())
                    pdfInfo.Subject = docInfo.Subject;

                if (!docInfo.Values.Title.IsValueNullOrEmpty())
                    pdfInfo.Title = docInfo.Title;
            }
        }

        /// <summary>
        /// Creates a new PDF document.
        /// </summary>
        PdfDocument CreatePdfDocument()
        {
            var document = new PdfDocument();
            document.Info.Creator = MigraDocProductVersionInformation.Creator;
            if (!String.IsNullOrEmpty(_language))
                document.Language = _language;
            return document;
        }
    }
}
