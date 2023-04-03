// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel.Internals;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Tables;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Renders a table to an XGraphics object.
    /// </summary>
    class TableRenderer : Renderer
    {
        internal TableRenderer(XGraphics gfx, Table documentObject, FieldInfos? fieldInfos)
            : base(gfx, documentObject, fieldInfos)
        {
            _table = documentObject;
        }

        internal TableRenderer(XGraphics gfx, RenderInfo renderInfo, FieldInfos? fieldInfos)
            : base(gfx, renderInfo, fieldInfos)
        {
            _table = (Table)_renderInfo.DocumentObject;
        }

        internal override LayoutInfo InitialLayoutInfo
        {
            get
            {
                var layoutInfo = new LayoutInfo();
                layoutInfo.KeepTogether = _table.KeepTogether;
                layoutInfo.KeepWithNext = false;
                layoutInfo.MarginBottom = 0;
                layoutInfo.MarginLeft = 0;
                layoutInfo.MarginTop = 0;
                layoutInfo.MarginRight = 0;
                return layoutInfo;
            }
        }

        void InitRendering()
        {
            var formatInfo = (TableFormatInfo)_renderInfo.FormatInfo;
            _bottomBorderMap = formatInfo.BottomBorderMap ?? NRT.ThrowOnNull<Dictionary<int, XUnit>>();
            //_connectedRowsMap = formatInfo.ConnectedRowsMap ?? NRT.ThrowOnNull<Dictionary<int, int>>();
            _connectedRowsMap = formatInfo.ConnectedRowsMap ?? NRT.ThrowOnNull<int[]>();
            _minMergedCellRowMap = formatInfo.MinMergedCellRowMap ?? NRT.ThrowOnNull<int[]>();
            _formattedCells = formatInfo.FormattedCells ?? NRT.ThrowOnNull<Dictionary<Cell, FormattedCell>>();

            _currRow = formatInfo.StartRow;
            _startRow = formatInfo.StartRow;
            _endRow = formatInfo.EndRow;

            _mergedCells = formatInfo.MergedCells ?? NRT.ThrowOnNull<MergedCellList>();
            _lastHeaderRow = formatInfo.LastHeaderRow;
            _startX = _renderInfo.LayoutInfo.ContentArea.X;
            _startY = _renderInfo.LayoutInfo.ContentArea.Y;
        }

        void RenderHeaderRows()
        {
            if (_lastHeaderRow < 0)
                return;

            int count = _mergedCells.Count;
            for (var index = 0; index < count; index++)
            {
                var cell = _mergedCells[index];
                Debug.Assert(cell != null, nameof(cell) + " != null");
                Debug.Assert(cell.Row != null, "cell.Row != null");
                if (cell.Row.Index <= _lastHeaderRow)
                    RenderCell(cell);
                else
                    break; // Exit the loop when we hit the first non-header cell. _mergedCells is sorted.
            }
        }

        void RenderCell(Cell cell)
        {
            var innerRect = GetInnerRect(CalcStartingHeight(), cell);
            RenderShading(cell, innerRect);
            RenderContent(cell, innerRect);
            RenderBorders(cell, innerRect);
        }

        void EqualizeRoundedCornerBorders(Cell cell)
        {
            // If any of a corner relevant border is set, we want to copy its values to the second corner relevant border, 
            // to ensure the innerWidth of the cell is the same, regardless of which border is used.
            // If set, we use the vertical borders as source for the values, otherwise we use the horizontal borders.
            var roundedCorner = cell.RoundedCorner;

            if (roundedCorner == RoundedCorner.None)
                return;

            BorderType primaryBorderType = BorderType.Top, secondaryBorderType = BorderType.Top;

            if (roundedCorner == RoundedCorner.TopLeft || roundedCorner == RoundedCorner.BottomLeft)
                primaryBorderType = BorderType.Left;
            if (roundedCorner == RoundedCorner.TopRight || roundedCorner == RoundedCorner.BottomRight)
                primaryBorderType = BorderType.Right;

            if (roundedCorner == RoundedCorner.TopLeft || roundedCorner == RoundedCorner.TopRight)
                secondaryBorderType = BorderType.Top;
            if (roundedCorner == RoundedCorner.BottomLeft || roundedCorner == RoundedCorner.BottomRight)
                secondaryBorderType = BorderType.Bottom;

            // If both borders don't exist, there's nothing to do and we should not create one by accessing it.
            if (!cell.Borders.HasBorder(primaryBorderType) && !cell.Borders.HasBorder(secondaryBorderType))
                return;

            // Get the borders. By using GV.ReadWrite we create the border, if not existing.
            var primaryBorder = (Border)cell.Borders.GetValue(primaryBorderType.ToString(), GV.ReadWrite)!;  // Border is created.
            var secondaryBorder = (Border)cell.Borders.GetValue(secondaryBorderType.ToString(), GV.ReadWrite)!;  // Border is created.

            var source = primaryBorder.Visible
                ? primaryBorder
                : secondaryBorder.Visible ? secondaryBorder : null;
            var target = primaryBorder.Visible
                ? secondaryBorder
                : secondaryBorder.Visible ? primaryBorder : null;

            if (source == null || target == null)
                return;

            target.Visible = source.Visible;
            target.Width = source.Width;
            target.Style = source.Style;
            target.Color = source.Color;
        }

        void RenderShading(Cell cell, Rectangle innerRect)
        {
            var shadeRenderer = new ShadingRenderer(_gfx, cell.Shading);
            shadeRenderer.Render(innerRect.X, innerRect.Y, innerRect.Width, innerRect.Height, cell.RoundedCorner);
        }

        void RenderBorders(Cell cell, Rectangle innerRect)
        {
            XUnit leftPos = innerRect.X;
            XUnit rightPos = leftPos + innerRect.Width;
            XUnit topPos = innerRect.Y;
            XUnit bottomPos = innerRect.Y + innerRect.Height;
            var mergedBorders = _mergedCells.GetEffectiveBorders(cell);

            var bordersRenderer = new BordersRenderer(mergedBorders, _gfx);
            XUnit bottomWidth = bordersRenderer.GetWidth(BorderType.Bottom);
            XUnit leftWidth = bordersRenderer.GetWidth(BorderType.Left);
            XUnit topWidth = bordersRenderer.GetWidth(BorderType.Top);
            XUnit rightWidth = bordersRenderer.GetWidth(BorderType.Right);

            if (cell.RoundedCorner != RoundedCorner.None)
            {
                // Difficult case: rounded borders.
                if (cell.RoundedCorner == RoundedCorner.TopLeft)
                    bordersRenderer.RenderRounded(cell.RoundedCorner, innerRect.X, innerRect.Y, innerRect.Width + rightWidth, innerRect.Height + bottomWidth);
                else if (cell.RoundedCorner == RoundedCorner.TopRight)
                    bordersRenderer.RenderRounded(cell.RoundedCorner, innerRect.X - leftWidth, innerRect.Y, innerRect.Width + leftWidth, innerRect.Height + bottomWidth);
                else if (cell.RoundedCorner == RoundedCorner.BottomLeft)
                    bordersRenderer.RenderRounded(cell.RoundedCorner, innerRect.X, innerRect.Y - topWidth, innerRect.Width + rightWidth, innerRect.Height + topWidth);
                else if (cell.RoundedCorner == RoundedCorner.BottomRight)
                    bordersRenderer.RenderRounded(cell.RoundedCorner, innerRect.X - leftWidth, innerRect.Y - topWidth, innerRect.Width + leftWidth, innerRect.Height + topWidth);

                // Render horizontal and vertical borders only if touching no rounded corner.
                if (cell.RoundedCorner != RoundedCorner.TopRight && cell.RoundedCorner != RoundedCorner.BottomRight)
                    bordersRenderer.RenderVertically(BorderType.Right, rightPos, topPos, bottomPos + bottomWidth - topPos);

                if (cell.RoundedCorner != RoundedCorner.TopLeft && cell.RoundedCorner != RoundedCorner.BottomLeft)
                    bordersRenderer.RenderVertically(BorderType.Left, leftPos - leftWidth, topPos, bottomPos + bottomWidth - topPos);

                if (cell.RoundedCorner != RoundedCorner.BottomLeft && cell.RoundedCorner != RoundedCorner.BottomRight)
                    bordersRenderer.RenderHorizontally(BorderType.Bottom, leftPos - leftWidth, bottomPos, rightPos + rightWidth + leftWidth - leftPos);

                if (cell.RoundedCorner != RoundedCorner.TopLeft && cell.RoundedCorner != RoundedCorner.TopRight)
                    bordersRenderer.RenderHorizontally(BorderType.Top, leftPos - leftWidth, topPos - topWidth, rightPos + rightWidth + leftWidth - leftPos);
            }
            else
            {
                // Simple case: no rounded borders.
                bordersRenderer.RenderVertically(BorderType.Right, rightPos, topPos, bottomPos + bottomWidth - topPos);
                bordersRenderer.RenderVertically(BorderType.Left, leftPos - leftWidth, topPos, bottomPos + bottomWidth - topPos);
                bordersRenderer.RenderHorizontally(BorderType.Bottom, leftPos - leftWidth, bottomPos, rightPos + rightWidth + leftWidth - leftPos);
                bordersRenderer.RenderHorizontally(BorderType.Top, leftPos - leftWidth, topPos - topWidth, rightPos + rightWidth + leftWidth - leftPos);
            }

            RenderDiagonalBorders(mergedBorders, innerRect);
        }

        void RenderDiagonalBorders(Borders mergedBorders, Rectangle innerRect)
        {
            var bordersRenderer = new BordersRenderer(mergedBorders, _gfx);
            bordersRenderer.RenderDiagonally(BorderType.DiagonalDown, innerRect.X, innerRect.Y, innerRect.Width, innerRect.Height);
            bordersRenderer.RenderDiagonally(BorderType.DiagonalUp, innerRect.X, innerRect.Y, innerRect.Width, innerRect.Height);
        }

        void RenderContent(Cell cell, Rectangle innerRect)
        {
            var formattedCell = _formattedCells[cell];
            var renderInfos = formattedCell.GetRenderInfos();

            if (renderInfos == null)
                return;

            VerticalAlignment verticalAlignment = cell.VerticalAlignment;
            XUnit contentHeight = formattedCell.ContentHeight;
            XUnit innerHeight = innerRect.Height;
            Debug.Assert(cell.Column != null, "cell.Column != null");
            XUnit targetX = innerRect.X + cell.Column.LeftPadding;

            XUnit targetY;
            Debug.Assert(cell.Row != null, "cell.Row != null");
            if (verticalAlignment == VerticalAlignment.Bottom)
            {
                targetY = innerRect.Y + innerRect.Height;
                targetY -= cell.Row.BottomPadding;
                targetY -= contentHeight;
            }
            else if (verticalAlignment == VerticalAlignment.Center)
            {
                targetY = innerRect.Y + cell.Row.TopPadding;
                targetY += innerRect.Y + innerRect.Height - cell.Row.BottomPadding;
                targetY -= contentHeight;
                targetY /= 2;
            }
            else
                targetY = innerRect.Y + cell.Row.TopPadding;

            RenderByInfos(targetX, targetY, renderInfos);
        }

        Rectangle GetInnerRect(XUnit startingHeight, Cell cell)
        {
            var bordersRenderer = new BordersRenderer(_mergedCells.GetEffectiveBorders(cell), _gfx);
            var formattedCell = _formattedCells[cell];
            XUnit width = formattedCell.InnerWidth;

            XUnit y = _startY;
            Debug.Assert(cell.Row != null, "cell.Row != null");
            int cellRowIndex = cell.Row.Index; // Cache property result.
            if (cellRowIndex > _lastHeaderRow)
                y += startingHeight;
            else
                y += CalcMaxTopBorderWidth(0);

#if true
            // !!!new 18-03-09 Attempt to fix an exception. begin
            if (!_bottomBorderMap.TryGetValue(cellRowIndex, out var upperBorderPos))
            {
                //GetType();
            }
            // !!!new 18-03-09 Attempt to fix an exception. end
#else
            XUnit upperBorderPos = _bottomBorderMap[cell.Row.Index];
#endif

            y += upperBorderPos;
            if (cell.Row.Index > _lastHeaderRow)
                y -= _bottomBorderMap[_startRow];

#if true
            // !!!new 18-03-09 Attempt to fix an exception. begin
            if (!_bottomBorderMap.TryGetValue(cellRowIndex + cell.MergeDown + 1, out var lowerBorderPos))
            {
                //GetType();
            }
            // !!!new 18-03-09 Attempt to fix an exception. end
#else
            XUnit lowerBorderPos = _bottomBorderMap[cellRowIndex + cell.MergeDown + 1];
#endif

            XUnit height = lowerBorderPos - upperBorderPos;
            height -= bordersRenderer.GetWidth(BorderType.Bottom);

            XUnit x = _startX;
            Debug.Assert(cell.Column != null, "cell.Column != null");
            int cellColIndex = cell.Column.Index; // Cache property result.
            for (int clmIdx = 0; clmIdx < cellColIndex; ++clmIdx)
            {
                x += _table.Columns[clmIdx]?.Width ?? NRT.ThrowOnNull<int>();
            }
            x += LeftBorderOffset;

            return new(x, y, width, height);
        }

        internal override void Render()
        {
            InitRendering();
            RenderHeaderRows();
            if (_startRow < _table.Rows.Count)
            {
                Cell cell; // = _table[_startRow, 0];

                int cellIdx = _mergedCells.BinarySearch(_table[_startRow, 0], new CellComparer());
                while (cellIdx < _mergedCells.Count)
                {
                    cell = _mergedCells[cellIdx];
                    Debug.Assert(cell.Row != null, "cell.Row != null");
                    if (cell.Row.Index > _endRow)
                        break;

                    RenderCell(cell);
                    ++cellIdx;
                }
            }
        }

        void InitFormat(Area area, FormatInfo? previousFormatInfo)
        {
            var prevTableFormatInfo = (TableFormatInfo?)previousFormatInfo;
            var tblRenderInfo = new TableRenderInfo
            {
                DocumentObject = _table
            };

            _renderInfo = tblRenderInfo;

            if (prevTableFormatInfo != null)
            {
                _mergedCells = prevTableFormatInfo.MergedCells ?? NRT.ThrowOnNull<MergedCellList>();
                _formattedCells = prevTableFormatInfo.FormattedCells ?? NRT.ThrowOnNull<Dictionary<Cell, FormattedCell>>();
                _bottomBorderMap = prevTableFormatInfo.BottomBorderMap ?? NRT.ThrowOnNull<Dictionary<int, XUnit>>();
                _lastHeaderRow = prevTableFormatInfo.LastHeaderRow;
                //_connectedRowsMap = prevTableFormatInfo.ConnectedRowsMap ?? NRT.ThrowOnNull<Dictionary<int, int>>();
                _connectedRowsMap = prevTableFormatInfo.ConnectedRowsMap ?? NRT.ThrowOnNull<int[]>();
                _minMergedCellRowMap = prevTableFormatInfo.MinMergedCellRowMap ?? NRT.ThrowOnNull<int[]>();
                _startRow = prevTableFormatInfo.EndRow + 1;
            }
            else
            {
                // InitFormat is called for every page. Some tasks must be performed only once. Do them here.

                // Equalize the two borders, that are used to determine a rounded corner's border.
                // This way the innerWidth of the cell, which is got by the saved _formattedCells, is the same regardless of which corner relevant border is set.
                foreach (var row in _table.Rows.Cast<Row>()) // BUG Make better enumerator.
                {
                    foreach (var cell in row.Cells.Cast<Cell>())
                        EqualizeRoundedCornerBorders(cell);
                }

                _mergedCells = new MergedCellList(_table);
                FormatCells();
                CalcLastHeaderRow();
                CreateConnectedRows();
                CreateBottomBorderMap();
                if (_doHorizontalBreak)
                {
                    CalcLastHeaderColumn();
                    CreateConnectedColumns();
                }
                _startRow = _lastHeaderRow + 1;
            }
            ((TableFormatInfo)tblRenderInfo.FormatInfo).MergedCells = _mergedCells;
            ((TableFormatInfo)tblRenderInfo.FormatInfo).FormattedCells = _formattedCells;
            ((TableFormatInfo)tblRenderInfo.FormatInfo).BottomBorderMap = _bottomBorderMap;
            ((TableFormatInfo)tblRenderInfo.FormatInfo).ConnectedRowsMap = _connectedRowsMap;
            ((TableFormatInfo)tblRenderInfo.FormatInfo).MinMergedCellRowMap = _minMergedCellRowMap;
            ((TableFormatInfo)tblRenderInfo.FormatInfo).LastHeaderRow = _lastHeaderRow;
        }

        void FormatCells()
        {
            _formattedCells = new(); //new Sorted_List(new CellComparer());
            int count = _mergedCells.Count;
            for (var index = 0; index < count; index++)
            {
                var cell = _mergedCells[index];
                FormattedCell formattedCell = new FormattedCell(cell, _documentRenderer,
                    _mergedCells.GetEffectiveBorders(cell),
                    _fieldInfos, 0, 0);
                formattedCell.Format(_gfx);
                _formattedCells.Add(cell, formattedCell);
            }
        }

        /// <summary>
        /// Formats (measures) the table.
        /// </summary>
        /// <param name="area">The area on which to fit the table.</param>
        /// <param name="previousFormatInfo"> </param>
        internal override void Format(Area area, FormatInfo? previousFormatInfo)
        {
            if (DocumentRelations.GetParent(_table) is DocumentElements elements)
            {
                if (DocumentRelations.GetParent(elements) is Section section)
                    _doHorizontalBreak = section.PageSetup.HorizontalPageBreak;
            }

            _renderInfo = new TableRenderInfo();
            InitFormat(area, previousFormatInfo);

            // Don't take any Rows higher then MaxElementHeight
            XUnit topHeight = CalcStartingHeight();
            XUnit offset;
            if (_startRow > _lastHeaderRow + 1 &&
                _startRow < _table.Rows.Count)
                offset = _bottomBorderMap[_startRow] - topHeight;
            else
                offset = -CalcMaxTopBorderWidth(0);

            int probeRow = _startRow;
            XUnit currentHeight = 0;
            XUnit startingHeight = 0;
            bool isEmpty = false;

            while (probeRow < _table.Rows.Count)
            {
                bool firstProbe = probeRow == _startRow;
                probeRow = _connectedRowsMap[probeRow];
                // Don't take any Rows higher then MaxElementHeight
                XUnit probeHeight = _bottomBorderMap[probeRow + 1] - offset;
                // First test whether MaxElementHeight has been set.
                if (MaxElementHeight > 0 && firstProbe && probeHeight > MaxElementHeight - Tolerance)
                    probeHeight = MaxElementHeight - Tolerance;
                //if (firstProbe && probeHeight > MaxElementHeight - Tolerance)
                //    probeHeight = MaxElementHeight - Tolerance;

                //The height for the first new row(s) + headerrows:
                if (startingHeight == 0)
                {
                    if (probeHeight > area.Height)
                    {
                        isEmpty = true;
                        break;
                    }
                    startingHeight = probeHeight;
                }

                if (probeHeight > area.Height)
                    break;

                else
                {
                    _currRow = probeRow;
                    currentHeight = probeHeight;
                    ++probeRow;
                }
            }
            if (!isEmpty)
            {
                var formatInfo = (TableFormatInfo)_renderInfo.FormatInfo;
                formatInfo.StartRow = _startRow;
                formatInfo._isEnding = _currRow >= _table.Rows.Count - 1;
                formatInfo.EndRow = _currRow;

                UpdateThisPagesBookmarks(_startRow, _currRow);
            }
            FinishLayoutInfo(area, currentHeight, startingHeight);
        }

        /// <summary>
        /// Updates the bookmarks in the given rows.
        /// Otherwise each BookmarkField will refer to the first page of the table, because initially they are set before the table gets split over the pages.
        /// </summary>
        void UpdateThisPagesBookmarks(int startRow, int endRow)
        {
            if (_table.Rows.Count == 0)
                return;

            if (_fieldInfos == null)
                NRT.ThrowOnNull();

            for (var r = startRow; r <= endRow; r++)
            {
                var row = _table.Rows[r];

                foreach (var bookmark in row.GetElementsRecursively<BookmarkField>())
                {
                    _fieldInfos.AddBookmark(bookmark.Name);
                }
            }
        }

        void FinishLayoutInfo(Area area, XUnit currentHeight, XUnit startingHeight)
        {
            var layoutInfo = _renderInfo.LayoutInfo;
            layoutInfo.StartingHeight = startingHeight;
            //REM: Trailing height would have to be calculated in case tables had a keep with next property.
            layoutInfo.TrailingHeight = 0;
            if (_currRow >= 0)
            {
                layoutInfo.ContentArea = new Rectangle(area.X, area.Y, 0, currentHeight);
                XUnit width = LeftBorderOffset;
                //foreach (Column clm in _table.Columns)
                foreach (var clm in _table.Columns.Cast<Column>())
                {
                    width += clm.Width;
                }
                layoutInfo.ContentArea.Width = width;
            }
            layoutInfo.MinWidth = layoutInfo.ContentArea.Width;

            //if (_table.Rows.Values.LeftIndent is not null)
            if (!_table.Rows.Values.LeftIndent.IsValueNullOrEmpty())
                layoutInfo.Left = _table.Rows.LeftIndent.Point;

            else if (_table.Rows.Alignment == RowAlignment.Left)
            {
                XUnit leftOffset = LeftBorderOffset;
                leftOffset += _table.Columns[0]!.LeftPadding; // BUG check
                layoutInfo.Left = -leftOffset;
            }

            switch (_table.Rows.Alignment)
            {
                case RowAlignment.Left:
                    layoutInfo.HorizontalAlignment = ElementAlignment.Near;
                    break;

                case RowAlignment.Right:
                    layoutInfo.HorizontalAlignment = ElementAlignment.Far;
                    break;

                case RowAlignment.Center:
                    layoutInfo.HorizontalAlignment = ElementAlignment.Center;
                    break;
            }
        }

        XUnit LeftBorderOffset
        {
            get
            {
                if (_leftBorderOffset < 0)
                {
                    if (_table.Rows.Count > 0 && _table.Columns.Count > 0)
                    {
                        var borders = _mergedCells.GetEffectiveBorders(_table[0, 0]);
                        var bordersRenderer = new BordersRenderer(borders, _gfx);
                        _leftBorderOffset = bordersRenderer.GetWidth(BorderType.Left);
                    }
                    else
                        _leftBorderOffset = 0;
                }
                return _leftBorderOffset;
            }
        }

        XUnit _leftBorderOffset = -1;

        /// <summary>
        /// Calculates either the height of the header rows or the height of the uppermost top border.
        /// </summary>
        XUnit CalcStartingHeight()
        {
            XUnit height = 0;
            if (_lastHeaderRow >= 0)
            {
                height = _bottomBorderMap[_lastHeaderRow + 1];
                height += CalcMaxTopBorderWidth(0);
            }
            else
            {
                if (_table.Rows.Count > _startRow)
                    height = CalcMaxTopBorderWidth(_startRow);
            }
            return height;
        }

        void CalcLastHeaderColumn()
        {
            _lastHeaderColumn = -1;
            foreach (var clm in _table.Columns.Cast<Column>())
            {
                if (clm.HeadingFormat)
                    _lastHeaderColumn = clm.Index;
                else break;
            }
            if (_lastHeaderColumn >= 0)
                _lastHeaderRow = CalcLastConnectedColumn(_lastHeaderColumn);

            // Ignore heading format if all the table is heading:
            if (_lastHeaderRow == _table.Rows.Count - 1)
                _lastHeaderRow = -1;
        }

        void CalcLastHeaderRow()
        {
            _lastHeaderRow = -1;
            foreach (var row in _table.Rows.Cast<Row>())
            {
                if (row.HeadingFormat)
                    _lastHeaderRow = row.Index;
                else break;
            }
            // Note: Do not use _connectedRowsMap here, it was not yet initialized.
            if (_lastHeaderRow >= 0)
                _lastHeaderRow = CalcLastConnectedRowDirect(_lastHeaderRow);

            // Ignore heading format if all the table is heading:
            if (_lastHeaderRow == _table.Rows.Count - 1)
                _lastHeaderRow = -1;
        }

        /// <summary>
        /// Calculate the last covered row where every row, taking MergeDown into account.
        /// </summary>
        void CreateConnectedRows()
        {
            int rows = _table.Rows.Count;
            int cols = _table.Columns.Count;
            _connectedRowsMap = new int[rows];
            _minMergedCellRowMap = new int[rows];
            int lastRowIndex = -1;
            int count = _mergedCells.Count;
            for (var index = 0; index < count; index++)
            {
                var cell = _mergedCells[index];
                Debug.Assert(cell.Row != null, "cell.Row != null");
                // _mergedCells are sorted from top to bottom and left to right.
                int cellRowIndex = cell.Row.Index;
                // Skip cells from the same row we already handled.
                if (lastRowIndex != cellRowIndex /*&& !_connectedRowsMap.ContainsKey(cellRowIndex)*/)
                {
                    int lastConnectedRow = CalcLastConnectedRowDirect(cellRowIndex);
                    _connectedRowsMap[cellRowIndex] = lastConnectedRow;
                    lastRowIndex = cellRowIndex;
                }

                int cellBottom = cellRowIndex + cell.MergeDown;
                if (cellBottom > 0 && _minMergedCellRowMap[cellBottom] == 0) // 0 can be the correct value for some rows.
                    _minMergedCellRowMap[cellBottom] = cellRowIndex;
            }
        }

        void CreateConnectedColumns()
        {
            _connectedColumnsMap = new(); //new SortedList();
            int count = _mergedCells.Count;
            for (var index = 0; index < count; index++)
            {
                var cell = _mergedCells[index];
                Debug.Assert(cell.Column != null, "cell.Column != null");
                if (!_connectedColumnsMap.ContainsKey(cell.Column.Index))
                {
                    int lastConnectedColumn = CalcLastConnectedColumn(cell.Column.Index);
                    _connectedColumnsMap[cell.Column.Index] = lastConnectedColumn;
                }
            }
        }

        void CreateBottomBorderMap()
        {
#if DEBUG
            CreateBottomBorderMapOld();
#endif
            _bottomBorderMap = new(); //new SortedList();
            _bottomBorderMap.Add(0, XUnit.FromPoint(0));
            int skipIndex = 0;
            int lastBorderRow = 0;
            int count = _table.Rows.Count;
            while (!_bottomBorderMap.ContainsKey(count))
            {
                CreateNextBottomBorderPosition(ref skipIndex, ref lastBorderRow);
            }
        }

#if DEBUG
        void CreateBottomBorderMapOld()
        {
            _bottomBorderMapOld = new Dictionary<int, XUnit>(); //new SortedList();
            _bottomBorderMapOld.Add(0, XUnit.FromPoint(0));
            while (!_bottomBorderMapOld.ContainsKey(_table.Rows.Count))
            {
                CreateNextBottomBorderPositionOld();
            }
        }

        Dictionary<int, XUnit> _bottomBorderMapOld = null!;

        /// <summary>
        ///   Creates the next bottom border position.
        /// </summary>
        void CreateNextBottomBorderPositionOld()
        {
            //int lastIdx = _bottomBorderMap.Count - 1;
            // SortedList version:
            //int lastBorderRow = (int)bottomBorderMap.GetKey(lastIdx);
            //XUnit lastPos = (XUnit)bottomBorderMap.GetByIndex(lastIdx);
            int lastBorderRow = 0;
            foreach (int key in _bottomBorderMapOld.Keys)
            {
                if (key > lastBorderRow)
                    lastBorderRow = key;
            }
            XUnit lastPos = _bottomBorderMapOld[lastBorderRow];

            Cell minMergedCell = GetMinMergedCell(lastBorderRow);
            FormattedCell minMergedFormattedCell = _formattedCells[minMergedCell];
            XUnit maxBottomBorderPosition = lastPos + minMergedFormattedCell.InnerHeight;
            maxBottomBorderPosition += CalcBottomBorderWidth(minMergedCell);

            foreach (Cell cell in _mergedCells)
            {
                if (cell.Row!.Index > minMergedCell.Row!.Index + minMergedCell.MergeDown)
                    break;

                if (cell.Row.Index + cell.MergeDown == minMergedCell.Row.Index + minMergedCell.MergeDown)
                {
                    FormattedCell formattedCell = _formattedCells[cell];
                    // !!!new 18-03-09 Attempt to fix an exception. begin
                    // if (cell.Row.Index < _bottomBorderMap.Count)
                    {
                        // !!!new 18-03-09 Attempt to fix an exception. end
#if true
                        // !!!new 18-03-09 Attempt to fix an exception. begin
                        XUnit topBorderPos = maxBottomBorderPosition;
                        if (!_bottomBorderMapOld.TryGetValue(cell.Row.Index, out topBorderPos))
                        {
                            //GetType();
                        }
                        // !!!new 18-03-09 Attempt to fix an exception. end
#else
                    XUnit topBorderPos = _bottomBorderMapOld[cell.Row.Index];
#endif
                        XUnit bottomBorderPos = topBorderPos + formattedCell.InnerHeight;
                        bottomBorderPos += CalcBottomBorderWidth(cell);
                        if (bottomBorderPos > maxBottomBorderPosition)
                            maxBottomBorderPosition = bottomBorderPos;
                        // !!!new 18-03-09 Attempt to fix an exception. begin
                    }
                    // !!!new 18-03-09 Attempt to fix an exception. end
                }
            }
            _bottomBorderMapOld.Add(minMergedCell.Row!.Index + minMergedCell.MergeDown + 1, maxBottomBorderPosition);
        }
#endif

        /// <summary>
        /// Calculates the top border width for the first row that is rendered or formatted.
        /// </summary>
        /// <param name="row">The row index.</param>
        XUnit CalcMaxTopBorderWidth(int row)
        {
            XUnit maxWidth = 0;
            if (_table.Rows.Count > row)
            {
                int cellIdx = _mergedCells.BinarySearch(_table[row, 0], new CellComparer());
                while (cellIdx < _mergedCells.Count)
                {
                    var rowCell = _mergedCells[cellIdx];
                    Debug.Assert(rowCell.Row != null, "rowCell.Row != null");
                    if (rowCell.Row.Index > row)
                        break;

                    if (rowCell.Values.Borders != null)
                    {
                        BordersRenderer bordersRenderer = new BordersRenderer(rowCell.Borders, _gfx);
                        XUnit width = bordersRenderer.GetWidth(BorderType.Top);
                        if (width > maxWidth)
                            maxWidth = width;
                    }
                    ++cellIdx;
                }
            }
            return maxWidth;
        }

        /// <summary>
        /// Creates the next bottom border position.
        /// </summary>
        void CreateNextBottomBorderPosition(ref int skipIndex, ref int lastBorderRow)
        {
            XUnit lastPos = _bottomBorderMap[lastBorderRow];

            var minMergedCell = GetMinMergedCell(lastBorderRow);
            var minMergedFormattedCell = _formattedCells[minMergedCell];
            XUnit maxBottomBorderPosition = lastPos + minMergedFormattedCell.InnerHeight;
            maxBottomBorderPosition += CalcBottomBorderWidth(minMergedCell);

            Debug.Assert(minMergedCell.Row != null, "minMergedCell.Row != null");
            int minRowIndex = minMergedCell.Row.Index; // Cache property result.
            int minMergeDown = minMergedCell.MergeDown; // Cache property result.
            int minRowIndexPlusMergeDown = minRowIndex + minMergeDown;

            bool first = true;
            int rowToSkip = -1;
            // Do not change skipIndex if we have MergeDown and still need it.
            int count = _bottomBorderMap.Count - 1;
            if (_connectedRowsMap[count] == count)
                rowToSkip = count;
            else
                first = false;

            //foreach (Cell cell in _mergedCells)
            int cells = _mergedCells.Count;
            for (int idx = skipIndex; idx < cells; ++idx)
            {
                var cell = _mergedCells[idx];

                Debug.Assert(cell.Row != null, "cell.Row != null");
                int rowIndex = cell.Row.Index; // Cache property result.
                int cellMergeDown = cell.MergeDown; // Cache property result.

                if (first && rowIndex > rowToSkip)
                {
                    skipIndex = idx;
                    first = false;
                }

                if (rowIndex > minRowIndexPlusMergeDown)
                    break;

                if (rowIndex + cell.MergeDown == minRowIndexPlusMergeDown)
                {
                    FormattedCell formattedCell = _formattedCells[cell];
                    // !!!new 18-03-09 Attempt to fix an exception. begin
                    // if (cell.Row.Index < _bottomBorderMap.Count)
                    {
                        // !!!new 18-03-09 Attempt to fix an exception. end
#if true
                        // !!!new 18-03-09 Attempt to fix an exception. begin
                        if (!_bottomBorderMap.TryGetValue(rowIndex, out var topBorderPos))
                        {
                            //GetType();
                        }
                        // !!!new 18-03-09 Attempt to fix an exception. end
#else
                    XUnit topBorderPos = _bottomBorderMap[rowIndex];
#endif
                        XUnit bottomBorderPos = topBorderPos + formattedCell.InnerHeight;
                        bottomBorderPos += CalcBottomBorderWidth(cell);
                        if (bottomBorderPos > maxBottomBorderPosition)
                            maxBottomBorderPosition = bottomBorderPos;
                        // !!!new 18-03-09 Attempt to fix an exception. begin
                    }
                    // !!!new 18-03-09 Attempt to fix an exception. end
                }
            }

            if (_bottomBorderMap.TryGetValue(minRowIndexPlusMergeDown + 1, out var temp))
            {
                // We can only come here if all cells of a row are "hidden" by MergeDown in rows above.
                //GetType();
                Debug.Assert(temp == maxBottomBorderPosition);
                lastBorderRow = 0;
                foreach (int key in _bottomBorderMap.Keys)
                {
                    if (key > lastBorderRow)
                        lastBorderRow = key;
                }
            }
            else
            {
#if DEBUG
                var f = _bottomBorderMapOld.TryGetValue(minRowIndexPlusMergeDown + 1, out var tempUnit);
                if (f)
                {
                    if (maxBottomBorderPosition != tempUnit)
                        GetType();
                    Debug.Assert(maxBottomBorderPosition == tempUnit);
                }
                else
                {
                    GetType();
                    Debug.Assert(false, "Unexpected mismatch");
                }
#endif
                _bottomBorderMap.Add(minRowIndexPlusMergeDown + 1, maxBottomBorderPosition);
                lastBorderRow = minRowIndexPlusMergeDown + 1;
            }
        }

        /// <summary>
        /// Calculates bottom border width of a cell.
        /// </summary>
        /// <param name="cell">The cell the bottom border of the row that is probed.</param>
        /// <returns>The calculated border width.</returns>
        XUnit CalcBottomBorderWidth(Cell cell)
        {
            var borders = _mergedCells.GetEffectiveBorders(cell);
            if (borders != null)
            {
                BordersRenderer bordersRenderer = new BordersRenderer(borders, _gfx);
                return bordersRenderer.GetWidth(BorderType.Bottom);
            }
            return 0;
        }

        /// <summary>
        /// Gets the first cell that ends in the given row or as close as possible.
        /// </summary>
        /// <param name="row">The row to probe.</param>
        /// <returns>The first cell with minimal vertical merge.</returns>
        Cell GetMinMergedCell(int row)
        {
#if DEBUG
            // Comparing the result of the new implementation with the old version.
            // Note: It does not matter which cell is actually returned. Just the bottom margin of the cell matters.
            var originalResult = GetMinMergedCellOriginal(row);
#endif

            //var resultRowIndex = _minMergedCellRowMap[row];
            var resultRowIndex = row;
            var resultRow = _table.Rows[resultRowIndex];
            int clsCount = _table.Columns.Count;
            for (int idx = 0; idx < clsCount; ++idx)
            {
                var cell = resultRow[idx];
                if (resultRowIndex + cell.MergeDown == row)
                {
                    // We need a result that is in _mergedCells.
                    if (!_mergedCells.Contains(cell))
                        continue;
#if DEBUG
                    // Does it matter which cell we return??? 2023-03-08 Yes, we need a result that is in _mergedCells.
                    Debug.Assert(originalResult == cell);

                    // Note: It does not matter which cell is actually returned. Just the bottom margin of the cell matters.
                    Debug.Assert(originalResult.Row!.Index + originalResult.MergeDown ==
                                 cell.Row!.Index + cell.MergeDown);
#endif
                    return cell;
                }
            }
            throw new InvalidOperationException("GetMinMergedCell: Unexpected problem #1");
        }

#if DEBUG
        Cell GetMinMergedCellOriginal(int row)
        {
            int minMerge = _table.Rows.Count;
            Cell minCell = null!;
            foreach (Cell cell in _mergedCells)
            {
                if (cell.Row!.Index == row)
                {
                    if (cell.MergeDown == 0)
                    {
                        minCell = cell;
                        break;
                    }
                    else if (cell.MergeDown < minMerge)
                    {
                        minMerge = cell.MergeDown;
                        minCell = cell;
                    }
                }
                else if (cell.Row.Index > row)
                    break;
            }
            return minCell ?? NRT.ThrowOnNull<Cell>();
        }
#endif

        /// <summary>
        /// Calculates the last row that is connected with the given row. Uses cached values and can only be used after initialization.
        /// </summary>
        /// <param name="row">The row that is probed for downward connection.</param>
        /// <returns>The last row that is connected with the given row.</returns>
        int CalcLastConnectedRow(int row)
        {
            return _connectedRowsMap[row];
        }

        /// <summary>
        /// Calculates the last row that is connected with the given row. This version does not rely on cached values and can be called anytime.
        /// </summary>
        /// <param name="row">The row that is probed for downward connection.</param>
        /// <returns>The last row that is connected with the given row.</returns>
        int CalcLastConnectedRowDirect(int row)
        {
            int lastConnectedRow = row;
            var cellRow = _table.Rows[row];
            var cells = cellRow.Cells;
            var count = cells.Count;
            bool finished = false;
            do
            {
                finished = false;
                for (var index = 0; index < count; index++)
                {
                    var cell = cells[index];
                    Debug.Assert(cell != null, nameof(cell) + " != null");
                    Debug.Assert(cell.Row != null, "cell.Row != null");
                    int downConnection = Math.Max(cellRow.KeepWith, cell.MergeDown);
                    if (lastConnectedRow < row + downConnection)
                    {
                        lastConnectedRow = row + downConnection;
                    }
                }

                // Do we have to inspect further rows?
                if (row < lastConnectedRow)
                {
                    // Yes, check next row also.
                    ++row;
                    cellRow = _table.Rows[row];
                    cells = cellRow.Cells;
                    count = cells.Count;
                }
                else
                {
                    // No, finished.
                    finished = true;
                }
            } while (!finished);

            return lastConnectedRow;
        }

        /// <summary>
        /// Calculates the last column that is connected with the specified column.
        /// </summary>
        /// <param name="column">The column that is probed for downward connection.</param>
        /// <returns>The last column that is connected with the given column.</returns>
        int CalcLastConnectedColumn(int column)
        {
            int lastConnectedColumn = column;
            int count = _mergedCells.Count;
            for (var index = 0; index < count; index++)
            {
                var cell = _mergedCells[index];
                Debug.Assert(cell.Column != null, "cell.Column != null");
                if (cell.Column.Index <= lastConnectedColumn)
                {
                    int rightConnection = Math.Max(cell.Column.KeepWith, cell.MergeRight);
                    if (lastConnectedColumn < cell.Column.Index + rightConnection)
                        lastConnectedColumn = cell.Column.Index + rightConnection;
                }
            }

            return lastConnectedColumn;
        }

        readonly Table _table;
        MergedCellList _mergedCells = null!;  // Set in InitRendering.
        Dictionary<Cell, FormattedCell> _formattedCells = null!;  // Set in InitRendering. //SortedList formattedCells;
        Dictionary<int, XUnit> _bottomBorderMap = null!;  // Set in InitRendering. //SortedList bottomBorderMap;
        //Dictionary<int, int> _connectedRowsMap = null!;  // Set in InitRendering. //SortedList connectedRowsMap;
        int[] _connectedRowsMap = null!;  // Set in InitRendering. // Array connectedRowsMap;
        int[] _minMergedCellRowMap = null!;  // Set in InitRendering. // Array connectedRowsMap;
        Dictionary<int, int> _connectedColumnsMap = null!;  // Set in InitRendering. //SortedList connectedColumnsMap;

        int _lastHeaderRow;
        int _lastHeaderColumn;
        int _startRow;
        int _currRow;
        int _endRow = -1;

        bool _doHorizontalBreak;
        XUnit _startX;
        XUnit _startY;
    }
}
