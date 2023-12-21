// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering.Resources;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Provides methods to render the document or single parts of it to a XGraphics object.
    /// </summary>
    /// <remarks>
    /// One prepared instance of this class can serve to render several output formats.
    /// </remarks>
    public class DocumentRenderer
    {
        /// <summary>
        /// Initializes a new instance of the DocumentRenderer class.
        /// </summary>
        /// <param name="document">The MigraDoc document to render.</param>
        public DocumentRenderer(Document document)
        {
            _document = document;
            WorkingDirectory = null!;
        }

        /// <summary>
        /// Prepares this instance for rendering.
        /// </summary>
        public void PrepareDocument()
        {
            var visitor = new PdfFlattenVisitor();
            visitor.Visit(_document);
            _previousListNumbers[ListType.NumberList1] = 0;
            _previousListNumbers[ListType.NumberList2] = 0;
            _previousListNumbers[ListType.NumberList3] = 0;
            _formattedDocument = new FormattedDocument(_document, this);

            XGraphics gfx = XGraphics.CreateMeasureContext(new XSize(2000, 2000), XGraphicsUnit.Point, XPageDirection.Downwards);

            _previousListInfo = null;
            _formattedDocument.Format(gfx);
        }

        /// <summary>
        /// Occurs while the document is being prepared (can be used to show a progress bar).
        /// </summary>
        public event PrepareDocumentProgressEventHandler? PrepareDocumentProgress;

        /// <summary>
        /// Allows applications to display a progress indicator while PrepareDocument() is being executed.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maximum"></param>
        internal virtual void OnPrepareDocumentProgress(int value, int maximum)
        {
            if (PrepareDocumentProgress != null)
            {
                // Invokes the delegates.
                var e = new PrepareDocumentProgressEventArgs(value, maximum);
                PrepareDocumentProgress(this, e);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance supports PrepareDocumentProgress.
        /// </summary>
        public bool HasPrepareDocumentProgress => PrepareDocumentProgress != null;

        /// <summary>
        /// Gets the formatted document of this instance. Will be null if document was not yet prepared.
        /// </summary>
        public FormattedDocument FormattedDocument
        {
#if NET6_0_OR_GREATER
            [return: MaybeNull]
#endif
            get { return _formattedDocument!; }
        }
        FormattedDocument? _formattedDocument;

        /// <summary>
        /// Renders a MigraDoc document to the specified graphics object.
        /// </summary>
        public void RenderPage(XGraphics gfx, int page)
            => RenderPage(gfx, page, PageRenderOptions.All);

        /// <summary>
        /// Renders a MigraDoc document to the specified graphics object.
        /// </summary>
        public void RenderPage(XGraphics gfx, int page, PageRenderOptions options)
        {
            if (_formattedDocument?.IsEmptyPage(page) ?? NRT.ThrowOnNull<bool>("FormattedDocument is null because document was not prepared yet"))
                return;

            var fieldInfos = _formattedDocument.GetFieldInfos(page);

            fieldInfos.Date = PrintDate != DateTime.MinValue ? PrintDate : DateTime.Now;

            if ((options & PageRenderOptions.RenderHeader) == PageRenderOptions.RenderHeader)
                RenderHeader(gfx, page);
            if ((options & PageRenderOptions.RenderFooter) == PageRenderOptions.RenderFooter)
                RenderFooter(gfx, page);

            if ((options & PageRenderOptions.RenderContent) == PageRenderOptions.RenderContent)
            {
                var renderInfos = _formattedDocument.GetRenderInfos(page);
                if (renderInfos != null)
                {
                    //foreach (RenderInfo renderInfo in renderInfos)
                    int count = renderInfos.Length;
                    for (int idx = 0; idx < count; idx++)
                    {
                        var renderInfo = renderInfos[idx];
                        var renderer = Renderer.Create(gfx, this, renderInfo, fieldInfos);
                        renderer.Render();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the document objects that get rendered on the specified page.
        /// </summary>
        public DocumentObject[] GetDocumentObjectsFromPage(int page)
        {
            var renderInfos = FormattedDocument.GetRenderInfos(page);
            if (renderInfos == null)
                return Array.Empty<DocumentObject>();

            int count = renderInfos.Length;
            var documentObjects = new DocumentObject[count];
            for (int idx = 0; idx < count; idx++)
                documentObjects[idx] = renderInfos[idx].DocumentObject;
            return documentObjects;
        }

        /// <summary>
        /// Gets the render information for document objects that get rendered on the specified page.
        /// </summary>
        public RenderInfo[]? GetRenderInfoFromPage(int page)
            => FormattedDocument.GetRenderInfos(page);

        /// <summary>
        /// Renders a single object to the specified graphics object at the given point.
        /// </summary>
        /// <param name="graphics">The graphics object to render on.</param>
        /// <param name="xPosition">The left position of the rendered object.</param>
        /// <param name="yPosition">The top position of the rendered object.</param>
        /// <param name="width">The width.</param>
        /// <param name="documentObject">The document object to render. Can be paragraph, table, or shape.</param>
        /// <remarks>This function is still in an experimental state.</remarks>
        public void RenderObject(XGraphics graphics, XUnit xPosition, XUnit yPosition, XUnit width, DocumentObject documentObject)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));

            if (documentObject == null)
                throw new ArgumentNullException(nameof(documentObject));

            if (!(documentObject is Shape) && !(documentObject is Table) &&
                !(documentObject is Paragraph))
                throw new ArgumentException(Messages2.ObjectNotRenderable, nameof(documentObject));

            var renderer = Renderer.Create(graphics, this, documentObject, null);
            renderer!.Format(new Rectangle(xPosition, yPosition, width, double.MaxValue), null);

            RenderInfo renderInfo = renderer.RenderInfo;
            renderInfo.LayoutInfo.ContentArea.X = xPosition;
            renderInfo.LayoutInfo.ContentArea.Y = yPosition;

            renderer = Renderer.Create(graphics, this, renderer.RenderInfo, null);
            renderer.Render();
        }

        /// <summary>
        /// Gets or sets the working directory for rendering.
        /// </summary>
        public string WorkingDirectory { get; set; }

        void RenderHeader(XGraphics graphics, int page)
        {
            var formattedHeader = FormattedDocument.GetFormattedHeader(page);
            if (formattedHeader == null)
                return;

            /* var headerArea = */
            FormattedDocument.GetHeaderArea(page);  // BUG Does it return something?
            RenderInfo[] renderInfos = formattedHeader.GetRenderInfos();
            var fieldInfos = FormattedDocument.GetFieldInfos(page);
            foreach (RenderInfo renderInfo in renderInfos)
            {
                var renderer = Renderer.Create(graphics, this, renderInfo, fieldInfos);
                renderer.Render();
            }
        }

        void RenderFooter(XGraphics graphics, int page)
        {
            var formattedFooter = FormattedDocument.GetFormattedFooter(page);
            if (formattedFooter == null)
                return;

            var footerArea = FormattedDocument.GetFooterArea(page);
            var renderInfos = formattedFooter.GetRenderInfos();

            // The footer is bottom-aligned and grows with its contents. topY specifies the Y position where the footer begins.
            XUnit topY = footerArea.Y + footerArea.Height - RenderInfo.GetTotalHeight(renderInfos);
            // offsetY specifies the offset (amount of movement) for all footer items. It's the difference between topY and the position calculated for the first item.
            XUnit offsetY = 0;
            bool notFirst = false;

            var fieldInfos = FormattedDocument.GetFieldInfos(page);
            foreach (RenderInfo renderInfo in renderInfos)
            {
                var renderer = Renderer.Create(graphics, this, renderInfo, fieldInfos);
                if (!notFirst)
                {
                    offsetY = renderer.RenderInfo.LayoutInfo.ContentArea.Y - topY;
                    notFirst = true;
                }
                var savedY = renderer.RenderInfo.LayoutInfo.ContentArea.Y;
                // Apply offsetY only to items that do not have an absolute position.
                if (renderer.RenderInfo.LayoutInfo.Floating != Floating.None)
                    renderer.RenderInfo.LayoutInfo.ContentArea.Y -= offsetY;
                renderer.Render();
                renderer.RenderInfo.LayoutInfo.ContentArea.Y = savedY;
            }
        }

        internal void AddOutline(int level, string title, PdfPage? destinationPage, XPoint position)
        {
            if (level < 1 || destinationPage == null)
                return;

            var document = destinationPage.Owner;
            Debug.Assert(document != null);
            //if (document == null)
            //    return;

            var outlines = document.Outlines;
            while (--level > 0)
            {
                int count = outlines.Count;
                if (count == 0)
                {
                    // You cannot add empty bookmarks to PDF. So we use blank here.
                    var outline = AddOutline(outlines, " ", destinationPage, position);
                    outlines = outline.Outlines;
                }
                else
                    outlines = outlines[count - 1].Outlines;
            }
            AddOutline(outlines, title, destinationPage, position);
        }

        PdfOutline AddOutline(PdfOutlineCollection outlines, string title, PdfPage destinationPage, XPoint position)
        {
            var outline = outlines.Add(title, destinationPage, true);
            outline.Left = position.X;
            outline.Top = position.Y;
            return outline;
        }

        internal int NextListNumber(ListInfo listInfo)
        {
            var listType = listInfo.ListType;
            bool isNumberList = listType == ListType.NumberList1 ||
              listType == ListType.NumberList2 ||
              listType == ListType.NumberList3;

            int listNumber = Int32.MinValue;
            if (listInfo == _previousListInfo)
            {
                if (isNumberList)
                    return _previousListNumbers[listType];
                return listNumber;
            }

            //bool listTypeChanged = _previousListInfo == null || _previousListInfo.ListType != listType;

            if (isNumberList)
            {
                listNumber = 1;
                if (/*!listTypeChanged &&*/ (listInfo.Values.ContinuePreviousList is null || listInfo.ContinuePreviousList))
                    listNumber = _previousListNumbers[listType] + 1;

                _previousListNumbers[listType] = listNumber;
            }

            _previousListInfo = listInfo;
            return listNumber;
        }

        ListInfo? _previousListInfo;
        readonly Dictionary<ListType, int> _previousListNumbers = new(3);
        readonly Document _document;

        /// <summary>
        /// Gets or sets the print date, i.e. the rendering date.
        /// </summary>
        public DateTime PrintDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Arguments for the PrepareDocumentProgressEvent which is called while a document is being prepared (you can use this to display a progress bar).
        /// </summary>
        public class PrepareDocumentProgressEventArgs : EventArgs
        {
            /// <summary>
            /// Indicates the current step reached in document preparation.
            /// </summary>
            public int Value;
            /// <summary>
            /// Indicates the final step in document preparation. The quotient of Value and Maximum can be used to calculate a percentage (e. g. for use in a progress bar).
            /// </summary>
            public int Maximum;

            /// <summary>
            /// Initializes a new instance of the <see cref="PrepareDocumentProgressEventArgs"/> class.
            /// </summary>
            /// <param name="value">The current step in document preparation.</param>
            /// <param name="maximum">The latest step in document preparation.</param>
            public PrepareDocumentProgressEventArgs(int value, int maximum)
            {
                Value = value;
                Maximum = maximum;
            }
        }

        /// <summary>
        /// The event handler that is being called for the PrepareDocumentProgressEvent event.
        /// </summary>
        public delegate void PrepareDocumentProgressEventHandler(object sender, PrepareDocumentProgressEventArgs e);

        internal int ProgressMaximum;
        internal int ProgressCompleted;
    }
}
