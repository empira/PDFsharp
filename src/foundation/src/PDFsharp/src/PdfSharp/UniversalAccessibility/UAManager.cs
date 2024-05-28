// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Events;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Structure;

namespace PdfSharp.UniversalAccessibility
{
    /// <summary>
    /// This is just a scratch.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class UAManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UAManager"/> class.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        UAManager(PdfDocument document)
        {
            document._uaManager = this;
            _document = document;

            // Set default language to English.
            SetDocumentLanguage("en");

            // DisplayDocTitle must be true.
            _document.ViewerPreferences.DisplayDocTitle = true;

            var internals = _document.Internals;

            _document.Events.PageAdded += OnPageAdded;
            _document.Events.PageRemoved += OnPageRemoved;
            _document.Events.PageGraphicsCreated += OnPageGraphicsCreated;
            _document.Events.PageGraphicsAction += OnPageGraphicsAction;

            // Marked must be true in MarkInfo.
            var markInfo = new PdfMarkInformation();
            internals.AddObject(markInfo);

            markInfo.Elements.SetBoolean(PdfMarkInformation.Keys.Marked, true);
            internals.Catalog.Elements.SetReference(PdfCatalog.Keys.MarkInfo, markInfo);

            // Build Structure Tree.
            StructureTreeRoot = new PdfStructureTreeRoot();
            internals.AddObject(StructureTreeRoot);
            internals.Catalog.Elements.SetReference(PdfCatalog.Keys.StructTreeRoot, StructureTreeRoot);

            // Set parent tree root.
            var parentTreeRoot = new PdfNumberTreeNode(true);
            _document.Internals.AddObject(parentTreeRoot);
            StructureTreeRoot.Elements.SetReference(PdfStructureTreeRoot.Keys.ParentTree, parentTreeRoot);

            // Child node for Document is recommended.
            StructureTreeElementDocument = new PdfStructureElement(_document);
            _document.Internals.AddObject(StructureTreeElementDocument);

            // Parent is root.
            StructureTreeElementDocument.Elements.SetReference(PdfStructureElement.Keys.P, StructureTreeRoot);

            // Type is document.
            StructureTreeElementDocument.Elements.SetName(PdfStructureElement.Keys.S, "/Document");

            StructureTreeRoot.Elements.SetReference(PdfStructureElement.Keys.K, StructureTreeElementDocument);
        }

        /// <summary>
        /// Root of the structure tree.
        /// </summary>
        public PdfStructureTreeRoot StructureTreeRoot { get; set; }

        /// <summary>
        /// Structure element of the document.
        /// </summary>
        public PdfStructureElement StructureTreeElementDocument { get; set; }

        /// <summary>
        /// Gets the Universal Accessibility Manager for the document.
        /// </summary>
        public static UAManager ForDocument(PdfDocument document)
        {
            return document._uaManager ?? new UAManager(document);
        }

        /// <summary>
        /// Gets the structure builder.
        /// </summary>
        public StructureBuilder StructureBuilder => _sb ??= new StructureBuilder(this);
        StructureBuilder? _sb;

        void OnPageAdded(object sender, PageEventArgs e)
        {
            // Only fresh pages can be processed.
            if (e.EventType == PageEventType.Moved)
                throw new InvalidOperationException("Cannot handle page moving.");
            if (e.EventType == PageEventType.Imported)
                throw new InvalidOperationException("Cannot handle page import.");

            CurrentPage = e.Page;
            StructureBuilder.OnAddPage();
        }

        void OnPageRemoved(object sender, PageEventArgs e)
        {
            throw new InvalidOperationException("Cannot handle page removing.");
        }

        void OnPageGraphicsCreated(object sender, PageGraphicsEventArgs e)
        {
            if (CurrentPage != e.Page)
                throw new InvalidOperationException("Cannot handle XGraphics objects for pages other than the current page.");

            CurrentGraphics = e.Graphics;
        }

        void OnPageGraphicsAction(object sender, PageGraphicsEventArgs e)
        {
            if (CurrentPage != e.Page)
                throw new InvalidOperationException("Cannot handle XGraphics objects for pages other than the current page.");

            if (CurrentGraphics != e.Graphics)
                throw new InvalidOperationException("Cannot handle XGraphics objects other than the current one.");

            switch (e.ActionType)
            {
                case PageGraphicsActionType.DrawString:
                    StructureBuilder.OnDrawString();
                    break;

                case PageGraphicsActionType.Draw:
                    StructureBuilder.OnDraw();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Gets the owning document for this UAManager.
        /// </summary>
        public PdfDocument Owner => _document;
        readonly PdfDocument _document;

        /// <summary>
        /// Gets the current page.
        /// </summary>
        public PdfPage CurrentPage { get; private set; } = default!;

        /// <summary>
        /// Gets the current XGraphics object.
        /// </summary>
        public XGraphics CurrentGraphics { get; private set; } = default!;

        /// <summary>
        /// Sets the language of the document.
        /// </summary>
        public void SetDocumentLanguage(string lang) 
            => _document.Internals.Catalog.Language = lang;

        /// <summary>
        /// Sets the text mode.
        /// </summary>
        public void BeginTextMode() 
            => PdfRendererExtensions.BeginTextMode(CurrentGraphics);

        /// <summary>
        /// Sets the graphic mode.
        /// </summary>
        public void BeginGraphicMode() 
            => PdfRendererExtensions.BeginGraphicMode(CurrentGraphics);

        /// <summary>
        /// Determine if renderer is in Text mode or Graphic mode.
        /// </summary>
        public bool IsInTextMode() 
            => PdfRendererExtensions.IsInTextMode(CurrentGraphics);
    }
}
