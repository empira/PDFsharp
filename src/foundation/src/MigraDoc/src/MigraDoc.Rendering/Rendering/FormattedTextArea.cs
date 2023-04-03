// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using PdfSharp.Drawing;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Represents a formatted text area.
    /// </summary>
    class FormattedTextArea : IAreaProvider
    {
        internal FormattedTextArea(DocumentRenderer documentRenderer, TextArea textArea, FieldInfos? fieldInfos)
        {
            TextArea = textArea;
            _fieldInfos = fieldInfos;
            _documentRenderer = documentRenderer;
        }

        internal void Format(XGraphics gfx)
        {
            _gfx = gfx;
            _isFirstArea = true;
            _formatter = new TopDownFormatter(this, _documentRenderer, TextArea.Elements);
            _formatter.FormatOnAreas(gfx, false);
        }

        internal XUnit InnerWidth
        {
            set => _innerWidth = value;
            get
            {
                if (double.IsNaN(_innerWidth))
                {
                    if (TextArea.Values.Width is not null)
                        _innerWidth = TextArea.Width.Point;
                    else
                        _innerWidth = CalcInherentWidth();
                }
                return _innerWidth;
            }
        }

        XUnit _innerWidth = double.NaN;

        internal XUnit InnerHeight
        {
            get
            {
                //if (TextArea.Values.Height is null)
                if (TextArea.Values.Height.IsValueNullOrEmpty())
                    return ContentHeight + TextArea.TopPadding + TextArea.BottomPadding;
                return TextArea.Height.Point;
            }
        }


        XUnit CalcInherentWidth()
        {
            XUnit inherentWidth = 0;
            foreach (var doc in TextArea.Elements)
            {
                if (doc == null)
                    NRT.ThrowOnNull();

                var renderer = Renderer.Create(_gfx, _documentRenderer, doc, _fieldInfos);
                if (renderer != null!)
                {
                    renderer.Format(new Rectangle(0, 0, double.MaxValue, double.MaxValue), null);
                    inherentWidth = Math.Max(renderer.RenderInfo.LayoutInfo.MinWidth, inherentWidth);
                }
            }
            inherentWidth += TextArea.LeftPadding;
            inherentWidth += TextArea.RightPadding;
            return inherentWidth;
        }

        Area? IAreaProvider.GetNextArea()
        {
            if (_isFirstArea)
                return CalcContentRect();

            return null;
        }

        Area? IAreaProvider.ProbeNextArea()
            => null;

        FieldInfos IAreaProvider.AreaFieldInfos => _fieldInfos ?? NRT.ThrowOnNull<FieldInfos>();

        void IAreaProvider.StoreRenderInfos(List<RenderInfo> renderInfos)
            => _renderInfos = renderInfos;

        bool IAreaProvider.IsAreaBreakBefore(LayoutInfo layoutInfo) => false;

        internal RenderInfo[] GetRenderInfos()
        {
            if (_renderInfos != null)
                return _renderInfos.ToArray();

            return Array.Empty<RenderInfo>();
        }

        internal XUnit ContentHeight => RenderInfo.GetTotalHeight(GetRenderInfos());

        Rectangle CalcContentRect()
        {
            XUnit width = InnerWidth - TextArea.LeftPadding - TextArea.RightPadding;
            XUnit height = double.MaxValue;
            return new Rectangle(0, 0, width, height);
        }

        bool IAreaProvider.PositionVertically(LayoutInfo layoutInfo) => false;

        bool IAreaProvider.PositionHorizontally(LayoutInfo layoutInfo) => false;

        internal readonly TextArea TextArea;

        readonly FieldInfos? _fieldInfos;
        TopDownFormatter _formatter = null!;
        List<RenderInfo>? _renderInfos;
        XGraphics _gfx = null!;
        bool _isFirstArea;
        readonly DocumentRenderer _documentRenderer;
    }
}
