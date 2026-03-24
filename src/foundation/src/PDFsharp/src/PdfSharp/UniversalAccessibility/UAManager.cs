// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Events;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.Structure;

namespace PdfSharp.UniversalAccessibility
{
    /// <summary>
    /// The UAManager adds PDF/UA (Accessibility) support to a PdfDocument.
    /// By using its StructureBuilder, you can easily build up the structure tree to give hints to screen readers about how to read the document.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class UAManager : ManagerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UAManager"/> class.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        UAManager(PdfDocument document) : base(document)
        {
            //document._uaManager = this; done in ForDocument
            //Document = document;

            // Set default language to English.
            SetDocumentLanguage("en");

            // DisplayDocTitle must be true.
            Document.ViewerPreferences.DisplayDocTitle = true;

            var internals = Document.Internals;

            Document.Events.PageAdded += OnPageAdded;
            Document.Events.PageRemoved += OnPageRemoved;
            Document.Events.PageGraphicsCreated += OnPageGraphicsCreated;
            Document.Events.PageGraphicsAction += OnPageGraphicsAction;

            // Marked must be true in MarkInfo.
            var markInfo = new PdfMarkInformation();
            internals.AddObject(markInfo);

            markInfo.Elements.SetBoolean(PdfMarkInformation.Keys.Marked, true);
            internals.Catalog.Elements.SetObject(PdfCatalog.Keys.MarkInfo, markInfo);

            // Build Structure Tree.
            StructureTreeRoot = new PdfStructureTreeRoot();
            internals.AddObject(StructureTreeRoot);
            internals.Catalog.Elements.SetObject(PdfCatalog.Keys.StructTreeRoot, StructureTreeRoot);

            // Set parent tree root.
            var parentTreeRoot = new PdfNumberTreeNode(true);
            Document.Internals.AddObject(parentTreeRoot);
            StructureTreeRoot.Elements.SetObject(PdfStructureTreeRoot.Keys.ParentTree, parentTreeRoot);

            // Child node for Document is recommended.
            StructureTreeElementDocument = new PdfStructureElement(Document);
            Document.Internals.AddObject(StructureTreeElementDocument);

            // Parent is root.
            StructureTreeElementDocument.Elements.SetObject(PdfStructureElement.Keys.P, StructureTreeRoot);

            // Type is document.
            StructureTreeElementDocument.Elements.SetName(PdfStructureElement.Keys.S, "/Document");

            StructureTreeRoot.Elements.SetObject(PdfStructureElement.Keys.K, StructureTreeElementDocument);
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
        /// Gets or creates the Universal Accessibility Manager for the specified document.
        /// </summary>
        public static UAManager ForDocument(PdfDocument document)
            => document.UAManager ??= new(document);

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
            => throw new InvalidOperationException("Cannot handle page removing.");

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
                    throw new ArgumentOutOfRangeException(nameof(e), "Invalid ActionType.");
            }
        }

        /// <summary>
        /// Gets the owning document for this UAManager.
        /// </summary>
        public PdfDocument Owner => Document;

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
            => Document.Internals.Catalog.Language = lang;

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
