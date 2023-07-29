// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;
using PdfSharp.Drawing;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Represents a formatted header or footer.
    /// </summary>
    class FormattedHeaderFooter : IAreaProvider
    {
        internal FormattedHeaderFooter(HeaderFooter headerFooter, DocumentRenderer documentRenderer, FieldInfos fieldInfos)
        {
            _headerFooter = headerFooter;
            _fieldInfos = fieldInfos;
            _documentRenderer = documentRenderer;
        }

        internal void Format(XGraphics gfx)
        {
            _gfx = gfx;
            _isFirstArea = true;
            _formatter = new TopDownFormatter(this, _documentRenderer, _headerFooter.Elements);
            _formatter.FormatOnAreas(gfx, false);
            _contentHeight = RenderInfo.GetTotalHeight(GetRenderInfos());
        }

        Area? IAreaProvider.GetNextArea()
        {
            if (_isFirstArea)
                return new Rectangle(ContentRect.X, ContentRect.Y, ContentRect.Width, double.MaxValue);

            return null;
        }

        Area? IAreaProvider.ProbeNextArea()
            => null;

        FieldInfos IAreaProvider.AreaFieldInfos => _fieldInfos;

        void IAreaProvider.StoreRenderInfos(List<RenderInfo> renderInfos) 
            => _renderInfos = renderInfos;

        bool IAreaProvider.IsAreaBreakBefore(LayoutInfo layoutInfo) 
            => false;

        internal RenderInfo[] GetRenderInfos()
        {
            if (_renderInfos != null)
                return _renderInfos.ToArray();

            //return new RenderInfo[0];
            return Array.Empty<RenderInfo>();
        }

        internal Rectangle ContentRect { get; set; } = null!;

        XUnit ContentHeight => _contentHeight;
        XUnit _contentHeight;

        bool IAreaProvider.PositionVertically(LayoutInfo layoutInfo)
        {
            IAreaProvider formattedDoc = _documentRenderer.FormattedDocument;
            return formattedDoc.PositionVertically(layoutInfo);
        }

        bool IAreaProvider.PositionHorizontally(LayoutInfo layoutInfo)
        {
            IAreaProvider formattedDoc = _documentRenderer.FormattedDocument;
            return formattedDoc.PositionHorizontally(layoutInfo);
        }

        readonly HeaderFooter _headerFooter;
        readonly FieldInfos _fieldInfos;
        TopDownFormatter _formatter = null!;  // Set in Format.
        List<RenderInfo>? _renderInfos;
        XGraphics _gfx = null!;  // Set in Format.
        bool _isFirstArea;
        readonly DocumentRenderer _documentRenderer;
    }
}
