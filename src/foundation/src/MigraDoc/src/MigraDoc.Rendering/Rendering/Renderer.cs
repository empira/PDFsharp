// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Shapes.Charts;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Abstract base class for all renderers.
    /// </summary>
    abstract class Renderer
    {
        internal Renderer(XGraphics gfx, DocumentObject documentObject, FieldInfos? fieldInfos)
        {
            _documentObject = documentObject;
            _gfx = gfx;
            _fieldInfos = fieldInfos;
            _renderInfo = null!;
            _documentRenderer = null!;
        }

        internal Renderer(XGraphics gfx, RenderInfo renderInfo, FieldInfos? fieldInfos)
        {
            _documentObject = renderInfo.DocumentObject;
            _gfx = gfx;
            _renderInfo = renderInfo;
            _fieldInfos = fieldInfos;
            _documentRenderer = null!;
        }

        /// <summary>
        /// Determines the maximum height a single element may have.
        /// </summary>
        internal XUnitPt MaxElementHeight
        {
            get => _maxElementHeight;
            set => _maxElementHeight = value;
        }

        /// <summary>
        /// In inherited classes, gets a layout info with only margin and break information set.
        /// It can be taken before the document object is formatted.
        /// </summary>
        /// <remarks>
        /// In inherited classes, the following parts are set properly:
        /// MarginTop, MarginLeft, MarginRight, MarginBottom, 
        /// KeepTogether, KeepWithNext, PagebreakBefore, Floating,
        /// VerticalReference, HorizontalReference.
        /// </remarks>
        internal abstract LayoutInfo InitialLayoutInfo { get; }

        /// <summary>
        /// Renders the contents shifted to the given Coordinates.
        /// </summary>
        /// <param name="xShift">The x shift.</param>
        /// <param name="yShift">The y shift.</param>
        /// <param name="renderInfos">The render infos.</param>
        protected void RenderByInfos(XUnitPt xShift, XUnitPt yShift, RenderInfo[]? renderInfos)
        {
            if (renderInfos == null)
                return;

            foreach (RenderInfo renderInfo in renderInfos)
            {
                XUnitPt savedX = renderInfo.LayoutInfo.ContentArea.X;
                XUnitPt savedY = renderInfo.LayoutInfo.ContentArea.Y;
                renderInfo.LayoutInfo.ContentArea.X += xShift;
                renderInfo.LayoutInfo.ContentArea.Y += yShift;
                Renderer renderer = Create(_gfx, _documentRenderer, renderInfo, _fieldInfos);
                renderer.Render();
                renderInfo.LayoutInfo.ContentArea.X = savedX;
                renderInfo.LayoutInfo.ContentArea.Y = savedY;
            }
        }

        protected void RenderByInfos(RenderInfo[] renderInfos)
        {
            RenderByInfos(0, 0, renderInfos);
        }

        /// <summary>
        /// Gets the render information necessary to render and position the object.
        /// </summary>
        internal RenderInfo RenderInfo => _renderInfo;

        protected RenderInfo _renderInfo;

        /// <summary>
        /// Sets the field infos object.
        /// </summary>
        /// <remarks>This property is set by the AreaProvider.</remarks>
        internal FieldInfos FieldInfos
        {
            set => _fieldInfos = value;
        }
        protected FieldInfos? _fieldInfos;

        /// <summary>
        /// Renders (draws) the object to the Graphics object.
        /// </summary>
        internal abstract void Render();

        /// <summary>
        /// Formats the object by calculating distances and line breaks and stopping when the area is filled.
        /// </summary>
        /// <param name="area">The area to render into.</param>
        /// <param name="previousFormatInfo">An information object received from a previous call of Format().
        /// Null for the first call.</param>
        internal abstract void Format(Area area, FormatInfo? previousFormatInfo);

        /// <summary>
        /// Creates a fitting renderer for the given document object for formatting.
        /// </summary>
        /// <param name="gfx">The XGraphics object to do measurements on.</param>
        /// <param name="documentRenderer">The document renderer.</param>
        /// <param name="documentObject">the document object to format.</param>
        /// <param name="fieldInfos">The field infos.</param>
        /// <returns>The fitting Renderer.</returns>
        internal static Renderer? Create(XGraphics gfx, DocumentRenderer documentRenderer, DocumentObject documentObject, FieldInfos? fieldInfos)
        {
            Renderer? renderer = null;
            if (documentObject is Paragraph paragraph)
                renderer = new ParagraphRenderer(gfx, paragraph, fieldInfos);
            else if (documentObject is Table table)
                renderer = new TableRenderer(gfx, table, fieldInfos);
            else if (documentObject is PageBreak pageBreak)
                renderer = new PageBreakRenderer(gfx, pageBreak, fieldInfos);
            else if (documentObject is TextFrame textFrame)
                renderer = new TextFrameRenderer(gfx, textFrame, fieldInfos);
            else if (documentObject is Chart chart)
                renderer = new ChartRenderer(gfx, chart, fieldInfos);
            else if (documentObject is Image image)
                renderer = new ImageRenderer(gfx, image, fieldInfos);
            else if (documentObject is Barcode)
                renderer = new BarcodeRenderer(gfx, (Barcode)documentObject, fieldInfos);

            if (renderer != null)
                renderer._documentRenderer = documentRenderer;
#if DEBUG
            // TODO Investigate why we can come here with "null".
            if (renderer == null)
                _ = typeof(int);  // A place for a break point;
#endif
            return renderer!; // BUG??? => return type "Renderer?" // ?? NRT.ThrowOnNull<Renderer>();
        }

        /// <summary>
        /// Creates a fitting renderer for the render info to render and layout with.
        /// </summary>
        /// <param name="gfx">The XGraphics object to render on.</param>
        /// <param name="documentRenderer">The document renderer.</param>
        /// <param name="renderInfo">The RenderInfo object stored after a previous call of Format().</param>
        /// <param name="fieldInfos">The field infos.</param>
        /// <returns>The fitting Renderer.</returns>
        internal static Renderer Create(XGraphics gfx, DocumentRenderer documentRenderer, RenderInfo renderInfo, FieldInfos? fieldInfos)
        {
            Renderer? renderer = null;

            if (renderInfo.DocumentObject is Paragraph)
                renderer = new ParagraphRenderer(gfx, renderInfo, fieldInfos);
            else if (renderInfo.DocumentObject is Table)
                renderer = new TableRenderer(gfx, renderInfo, fieldInfos);
            else if (renderInfo.DocumentObject is PageBreak)
                renderer = new PageBreakRenderer(gfx, renderInfo, fieldInfos);
            else if (renderInfo.DocumentObject is TextFrame)
                renderer = new TextFrameRenderer(gfx, renderInfo, fieldInfos);
            else if (renderInfo.DocumentObject is Chart)
                renderer = new ChartRenderer(gfx, renderInfo, fieldInfos);
            //else if (renderInfo.DocumentObject is Chart)
            //  renderer = new ChartRenderer(gfx, renderInfo, fieldInfos);
            else if (renderInfo.DocumentObject is Image)
                renderer = new ImageRenderer(gfx, renderInfo, fieldInfos);
            else if (renderInfo.DocumentObject is Barcode)
                renderer = new BarcodeRenderer(gfx, renderInfo, fieldInfos);

            if (renderer != null)
                renderer._documentRenderer = documentRenderer;

            return renderer ?? NRT.ThrowOnNull<Renderer>();
        }

        internal static readonly XUnitPt Tolerance = 0.001;
        XUnitPt _maxElementHeight = -1;

        protected DocumentObject _documentObject;
        protected DocumentRenderer _documentRenderer;
        protected XGraphics _gfx;
    }
}
