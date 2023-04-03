// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel.Tables;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Represents a formatted cell.
    /// </summary>
    public class FormattedCell : IAreaProvider
    {
        internal FormattedCell(Cell cell, DocumentRenderer documentRenderer, Borders cellBorders, FieldInfos? fieldInfos, XUnit xOffset, XUnit yOffset)
        {
            Cell = cell;
            _fieldInfos = fieldInfos;
            _yOffset = yOffset;
            _xOffset = xOffset;
            _bordersRenderer = new BordersRenderer(cellBorders, null!);  // BUG null
            _documentRenderer = documentRenderer;
        }

        /// <summary>
        /// Gets the cell the formatting information refers to.
        /// </summary>
        public Cell Cell { get; }

        Area? IAreaProvider.GetNextArea()
        {
            if (_isFirstArea)
            {
                var rect = CalcContentRect();
                _isFirstArea = false;
                return rect;
            }
            return null;
        }
        bool _isFirstArea = true;

        Area? IAreaProvider.ProbeNextArea() => null;

        internal void Format(XGraphics gfx)
        {
            _gfx = gfx;
            _formatter = new TopDownFormatter(this, _documentRenderer, Cell.Elements);
            _formatter.FormatOnAreas(gfx, false);
            _contentHeight = CalcContentHeight(_documentRenderer);
        }

        Rectangle CalcContentRect()
        {
            var column = Cell.Column;
            Debug.Assert(column != null, nameof(column) + " != null");
            XUnit width = InnerWidth;
            width -= column.LeftPadding.Point;
            Debug.Assert(Cell.Table != null, "Cell.Table != null");
            var rightColumn = Cell.Table.Columns[column.Index + Cell.MergeRight];
            width -= rightColumn.RightPadding.Point;

            XUnit height = double.MaxValue;
            return new Rectangle(_xOffset, _yOffset, width, height);
        }

        internal XUnit ContentHeight => _contentHeight;

        internal XUnit InnerHeight
        {
            get
            {
                Debug.Assert(Cell != null, nameof(Cell) + " != null");
                var row = Cell.Row;
                Debug.Assert(row != null, nameof(row) + " != null");
                XUnit verticalPadding = row.TopPadding.Point;
                verticalPadding += row.BottomPadding.Point;

                switch (row.HeightRule)
                {
                    case RowHeightRule.Exactly:
                        return row.Height.Point;

                    case RowHeightRule.Auto:
                        return verticalPadding + _contentHeight;

                    case RowHeightRule.AtLeast:
                    default:
                        return Math.Max(row.Height, verticalPadding + _contentHeight);
                }
            }
        }

        internal XUnit InnerWidth
        {
            get
            {
                XUnit width = 0;
                Debug.Assert(Cell != null, nameof(Cell) + " != null");
                Debug.Assert(Cell.Column != null, "Cell.Column != null");
                int cellColumnIdx = Cell.Column.Index;
                for (int toRight = 0; toRight <= Cell.MergeRight; ++toRight)
                {
                    int columnIdx = cellColumnIdx + toRight;
                    Debug.Assert(Cell.Table != null, "Cell.Table != null");
                    width += Cell.Table.Columns[columnIdx]?.Width ?? NRT.ThrowOnNull<Unit>();
                }
                width -= _bordersRenderer.GetWidth(BorderType.Right);

                return width;
            }
        }

        FieldInfos IAreaProvider.AreaFieldInfos => _fieldInfos ?? NRT.ThrowOnNull<FieldInfos>();

        void IAreaProvider.StoreRenderInfos(List<RenderInfo> renderInfos)
            => _renderInfos = renderInfos;

        bool IAreaProvider.IsAreaBreakBefore(LayoutInfo layoutInfo)
            => false;

        bool IAreaProvider.PositionVertically(LayoutInfo layoutInfo)
            => false;

        bool IAreaProvider.PositionHorizontally(LayoutInfo layoutInfo)
            => false;

        XUnit CalcContentHeight(DocumentRenderer documentRenderer)
        {
            XUnit height = RenderInfo.GetTotalHeight(GetRenderInfos());
            if (height == 0)
            {
                height = ParagraphRenderer.GetLineHeight(Cell.Format, _gfx, documentRenderer);
                height += Cell.Format.SpaceBefore;
                height += Cell.Format.SpaceAfter;
            }
            return height;
        }

        XUnit _contentHeight = 0;

        internal RenderInfo[]? GetRenderInfos()
        {
            if (_renderInfos != null)
                return _renderInfos.ToArray();

            return null;
        }

        readonly FieldInfos? _fieldInfos;
        List<RenderInfo>? _renderInfos;
        readonly XUnit _xOffset;
        readonly XUnit _yOffset;

        TopDownFormatter _formatter = null!;  // Set in Format.
        readonly BordersRenderer _bordersRenderer;
        XGraphics _gfx = null!;  // Set in Format.
        readonly DocumentRenderer _documentRenderer;
    }
}
