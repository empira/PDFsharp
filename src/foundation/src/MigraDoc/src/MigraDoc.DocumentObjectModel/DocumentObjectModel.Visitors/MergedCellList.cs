// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

#define CACHE

using MigraDoc.DocumentObjectModel.Tables;

namespace MigraDoc.DocumentObjectModel.Visitors
{
    /// <summary>
    /// Represents a merged list of cells of a table.
    /// </summary>
    public class MergedCellList : List<Cell>
    {
        /// <summary>
        /// Enumeration of neighbor positions of cells in a table.
        /// </summary>
        enum NeighborPosition
        {
            Top,
            Left,
            Right,
            Bottom
        }

        /// <summary>
        /// Initializes a new instance of the MergedCellList class.
        /// </summary>
        public MergedCellList(Table table)
        {
            Init(table);
        }

        /// <summary>
        /// Initializes this instance from a table.
        /// </summary>
        void Init(Table table)
        {
#if true
            int rows = table.Rows.Count;
            int columns = table.Columns.Count;
            var flags = new Boolean[rows, columns];
            for (int rwIdx = 0; rwIdx < rows; ++rwIdx)
            {
                for (int clmIdx = 0; clmIdx < columns; ++clmIdx)
                {
                    var cell = table[rwIdx, clmIdx];
                    if (!flags[rwIdx, clmIdx])
                    {
                        Add(cell);
                        // Remember cells that were already covered by this cell.
                        if (cell.MergeDown > 0 || cell.MergeRight > 0)
                        {
                            int mergeRight = cell.MergeRight;
                            int mergeDown = cell.MergeDown;

                            // Show user-friendly messages if MergeRight or MergeDown are too large.
                            if (rwIdx + mergeDown >= rows)
                                throw TH.InvalidOperationException_MergeDownTooLarge(rwIdx, clmIdx, mergeDown);
                            if (clmIdx + mergeRight >= columns)
                                throw TH.InvalidOperationException_MergeRightTooLarge(rwIdx, clmIdx, mergeRight);

                            for (int idxMergedRows = 0; idxMergedRows <= mergeDown; idxMergedRows++)
                            {
                                for (int idxMergedColumns = 0; idxMergedColumns <= mergeRight; idxMergedColumns++)
                                {
                                    flags[rwIdx + idxMergedRows, clmIdx + idxMergedColumns] = true;
                                }
                            }
                        }
                    }
                }
            }
#else
            int columns = table.Columns.Count;
            for (int rwIdx = 0; rwIdx < table.Rows.Count; ++rwIdx)
            {
                for (int clmIdx = 0; clmIdx < columns; ++clmIdx)
                {
                    var cell = table[rwIdx, clmIdx];
                    // TOxDO Make this smarter?
                    if (!IsAlreadyCovered(cell))
                        Add(cell);
                }
            }
#endif
        }

        /// <summary>
        /// Returns whether the given cell is already covered by a preceding cell in this instance.
        /// </summary>
        /// <remarks>
        /// Help function for Init().
        /// </remarks>
        [Obsolete("Slow: scans all cells. Do not call this in a loop.")]
        bool IsAlreadyCovered(Cell cell)
        {
            int cellColIndex = cell.Column.Index;
            int cellRowIndex = cell.Row.Index;
            for (int index = Count - 1; index >= 0; --index)
            {
                Cell currentCell = this[index];
                int currentColIndex = currentCell.Column.Index;
                if (currentColIndex <= cellColIndex && currentColIndex + currentCell.MergeRight >= cellColIndex)
                {
                    int currentRowIndex = currentCell.Row.Index;
                    int currentMergeDown = currentCell.MergeDown;
                    if (currentRowIndex <= cellRowIndex && currentRowIndex + currentMergeDown >= cellRowIndex)
                        return true;

                    if (currentRowIndex + currentMergeDown == cellRowIndex - 1)
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the cell at the specified position.
        /// </summary>
        public new Cell this[int index] => base[index];

        /// <summary>
        /// Gets a borders object that should be used for RTF rendering.
        /// In RTF heading rows must not be considered, as repeated heading row rendering is done in the RTF application.
        /// Further, inserting rows later in the RTF application should not move or duplicate any heading row border formatting,
        /// that could otherwise be assigned here to the following row, down to the table content.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        ///   Thrown when the cell is not in this list.
        ///   This situation occurs if the given cell is merged "away" by a previous one.
        /// </exception>
        public Borders GetEffectiveBordersRtf(Cell cell)
        {
            return GetEffectiveBordersInternal(cell, false, null);
        }

        /// <summary>
        /// Gets a borders object that should be used for PDF rendering.
        /// In PDF heading rows must be considered, as repeated heading row rendering is done in PDF rendering
        /// Therefore the index of the last heading row (original, not repetition) must be committed.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        ///   Thrown when the cell is not in this list.
        ///   This situation occurs if the given cell is merged "away" by a previous one.
        /// </exception>
        public Borders GetEffectiveBordersPdf(Cell cell, int lastHeadingRow)
        {
            return GetEffectiveBordersInternal(cell, true, lastHeadingRow);
        }

        Borders GetEffectiveBordersInternal(Cell cell, bool considerHeadingRows, int? consideredLastHeadingRow)
        {
#if CACHE
            var lastKnownHeaderInsertionRowIndex = GetLastKnownHeaderInsertionRowIndex();

            // If a cached borders value exists for this cell, determine if it has to be determined again.
            var existingLookupKey = _effectiveBordersLookup.TryGetValue(cell, out var result);
            if (existingLookupKey)
            {
                // The cached borders object can be returned as no changes are expected, when...
                if (!considerHeadingRows // ...not considering heading rows (considerHeadingRows should be always true for PDF and false for RTF rendering)...
                    || lastKnownHeaderInsertionRowIndex <= result.LastKnownHeaderInsertionRowIndex // ...or no heading has been inserted since the last borders determination...
                    || !_headerInsertionRowIndices.Contains(cell.Row.Index)) // ...or the changed _headerInsertionRowIndices does not contain this row
                                                                             // (so this row is still not following a repeated heading row).
                    return result.Borders;
            }
#endif

            var borders = cell.Values.Borders;
            if (borders != null)
            {
                borders = borders.Clone();
                borders.Parent = cell;
            }
            else
                borders = new Borders(cell.Parent!); // Parent cannot be null here in real-world scenarios.

            int cellIdx = BinarySearch(cell, new CellComparer());
            if (!(cellIdx >= 0 && cellIdx < Count))
                throw new ArgumentException("cell is not a relevant cell", nameof(cell));

            int cellRowIndex = cell.Row.Index;
            int cellColIndex = cell.Column.Index;
            int cellMergeRight = cell.MergeRight;
            if (cellMergeRight > 0)
            {
                var rightBorderCell = cell.Table[cellRowIndex, cellColIndex + cellMergeRight];
                if (rightBorderCell.Values.Borders != null && rightBorderCell.Values.Borders.Values.Right != null)
                    borders.Right = rightBorderCell.Values.Borders.Values.Right.Clone();
                else
                    borders.Values.Right = null;
            }

            int cellMergeDown;
            if (cell.Values.MergeDown is not null && (cellMergeDown = cell.Values.MergeDown.Value) > 0)
            {
                var bottomBorderCell = cell.Table[cellRowIndex + cellMergeDown, cellColIndex];
                if (bottomBorderCell.Values.Borders != null && bottomBorderCell.Values.Borders.Values.Bottom != null)
                    borders.Bottom = bottomBorderCell.Values.Borders.Values.Bottom.Clone();
                else
                    borders.Values.Bottom = null;
            }

            // For BorderTypes Top, Right, Bottom and Left update the width with the neighbors touching border where required.
            // In case of rounded corners this should not be done.

            var leftNeighbor = GetNeighborLeft(cellIdx);
            if (leftNeighbor != null && leftNeighbor.RoundedCorner != RoundedCorner.TopRight && leftNeighbor.RoundedCorner != RoundedCorner.BottomRight)
            {
                var nbrBrdrs = leftNeighbor.Values.Borders;
                if (nbrBrdrs != null && GetEffectiveBorderWidth(nbrBrdrs, BorderType.Right) >= GetEffectiveBorderWidth(borders, BorderType.Left))
                    borders.SetValue("Left", GetBorderFromBorders(nbrBrdrs, BorderType.Right));
            }

            var rightNeighbor = GetNeighborRight(cellIdx);
            if (rightNeighbor != null && rightNeighbor.RoundedCorner != RoundedCorner.TopLeft && rightNeighbor.RoundedCorner != RoundedCorner.BottomLeft)
            {
                var nbrBrdrs = rightNeighbor.Values.Borders;
                if (nbrBrdrs != null && GetEffectiveBorderWidth(nbrBrdrs, BorderType.Left) > GetEffectiveBorderWidth(borders, BorderType.Right))
                    borders.SetValue("Right", GetBorderFromBorders(nbrBrdrs, BorderType.Left));
            }
            
            // If considering heading rows and if cellRowIndex is an index a header is been inserted at, override topRowIndex with the last heading row.
            // This way for rows after a repeated heading row the original last heading row is considered as the top neighbor for top border determination
            // instead of the last content row on the page before.
            // Repetitions of the heading are not managed in MergedCellList (what would be wrong for RTF rendering), therefore jumping to the original header is necessary.
            // This behaviour is wanted for PDF only, where all heading rows repetitions are rendered with their border formatting into the document and
            // where their neighbors have to consider this.
            int? topRowIndexOverride;
            if (considerHeadingRows && _headerInsertionRowIndices.Contains(cellRowIndex))
                topRowIndexOverride = consideredLastHeadingRow;
            else
                topRowIndexOverride = null;

            var topNeighbor = GetNeighborTop(cellIdx, topRowIndexOverride);

            // If not considering heading rows and if the topNeighbor is a heading row, set it to null to ignore it for top border determination.
            // This way the first content row’s top border doesn’t possibly get the bottom border of the original last heading row.
            // This behaviour is wanted for RTF only, where all heading rows repetitions are rendered with their border formatting in the RTF application when displaying the
            // document and where content row movement or insertion in the RTF application must not copy formatting values that actually belong to the original last heading row.
            if (!considerHeadingRows && topNeighbor?.Row.HeadingFormat == true)
                topNeighbor = null;

            if (topNeighbor != null && topNeighbor.RoundedCorner != RoundedCorner.BottomLeft && topNeighbor.RoundedCorner != RoundedCorner.BottomRight)
            {
                var nbrBrdrs = topNeighbor.Values.Borders;
                if (nbrBrdrs != null && GetEffectiveBorderWidth(nbrBrdrs, BorderType.Bottom) >= GetEffectiveBorderWidth(borders, BorderType.Top))
                    borders.SetValue("Top", GetBorderFromBorders(nbrBrdrs, BorderType.Bottom));
            }

            var bottomNeighbor = GetNeighborBottom(cellIdx);
            if (bottomNeighbor != null && bottomNeighbor.RoundedCorner != RoundedCorner.TopLeft && bottomNeighbor.RoundedCorner != RoundedCorner.TopRight)
            {
                var nbrBrdrs = bottomNeighbor.Values.Borders;
                if (nbrBrdrs != null && GetEffectiveBorderWidth(nbrBrdrs, BorderType.Top) > GetEffectiveBorderWidth(borders, BorderType.Bottom))
                    borders.SetValue("Bottom", GetBorderFromBorders(nbrBrdrs, BorderType.Top));
            }
#if CACHE
            // Add or update cached borders and lastKnownHeaderInsertionRowIndex for cell.
            _effectiveBordersLookup[cell] = (borders, lastKnownHeaderInsertionRowIndex);
#endif
            return borders;
        }

#if CACHE
        /// <summary>
        /// A dictionary assigning the determined effective borders and the last known header insertion row index at that time to a cell for caching purposes.
        /// If a new last heading insertion row is known, the cached borders may have to be determined again.
        /// </summary>
        readonly Dictionary<Cell, (Borders Borders, int LastKnownHeaderInsertionRowIndex)> _effectiveBordersLookup = new();
#endif

        /// <summary>
        /// Gets the cell that covers the given cell by merging. Usually the cell itself if not merged.
        /// </summary>
        public Cell? GetCoveringCell(Cell cell)
        {
            int cellIdx = BinarySearch(cell, new CellComparer());
            if (cellIdx >= 0 && cellIdx < Count)
                return this[cellIdx];

            // Binary Search returns the complement of the next value, therefore, "~cellIdx - 1" is the previous cell.
            cellIdx = ~cellIdx - 1;

            int cellColIndex = cell.Column.Index;
            int cellRowIndex = cell.Row.Index;
            for (int index = cellIdx; index >= 0; --index)
            {
                Cell currCell = this[index];
                if (currCell.Column.Index <= cellColIndex &&
                  currCell.Column.Index + currCell.MergeRight >= cellColIndex &&
                  currCell.Row.Index <= cellRowIndex &&
                  currCell.Row.Index + currCell.MergeDown >= cellRowIndex)
                    return currCell;
            }
            return null;
        }

        /// <summary>
        /// Returns the border of the given borders-object of the specified type (top, bottom, ...).
        /// If that border doesn’t exist, it returns a new border object that inherits all properties from the given borders object.
        /// </summary>
        static Border GetBorderFromBorders(Borders borders, BorderType type)
        {
            Border? returnBorder = borders.GetBorder(type);
            if (returnBorder == null)
            {
                returnBorder = new Border();
                returnBorder.Values.Style = borders.Values.Style;
                returnBorder.Values.Width = borders.Values.Width;
                returnBorder.Values.Color = borders.Values.Color;
                returnBorder.Values.Visible = borders.Values.Visible;
            }
            return returnBorder;
        }

        /// <summary>
        /// Returns the width of the border at the specified position.
        /// </summary>
        static Unit GetEffectiveBorderWidth(Borders? borders, BorderType type)
        {
            if (borders == null)
                return 0;

            var border = borders.GetBorder(type);

#if true
            // Use "borders" if "border" is null or empty.
            if (border != null && !border.Values.Width.IsValueNullOrEmpty())
            {
                // Use border.
                var visible = border.Values.Visible;
                if (visible != null && !(bool)visible)
                    return 0;

                var width = border.Values.Width;
                if (width != null)
                    return (Unit)width;

                var color = border.Values.Color;
                if (color != null)
                    return 0.5;

                var style = border.Values.Style;
                if (style != null)
                    return 0.5;
                return 0;
            }
            else
            {
                // Use borders.
                var visible = borders.Values.Visible;
                if (visible != null && !(bool)visible)
                    return 0;

                var width = borders.Values.Width;
                if (width != null)
                    return (Unit)width;

                var color = borders.Values.Color;
                if (color != null)
                    return 0.5;

                var style = borders.Values.Style;
                if (style != null)
                    return 0.5;
                return 0;
            }
#else
            // Use "borders" if "border" is null or empty.
            DocumentObject? relevantDocObj = border;
            if (border == null || border.Values.Width.IsValueNullOrEmpty())
                relevantDocObj = borders;

            // TOxDO Avoid 'GetValue("'.
            // Avoid unnecessary GetValue calls. => Not trivial because it can be Border or Borders.
            object? visible = relevantDocObj!.GetValue("visible", GV.GetNull); // relevantDocObj cannot be null here.
            if (visible != null && !(bool)visible)
                return 0;

            object? width = relevantDocObj.GetValue("width", GV.GetNull);
            if (width != null)
                return (Unit)width;

            object? color = relevantDocObj.GetValue("color", GV.GetNull);
            if (color != null)
                return 0.5;

            object? style = relevantDocObj.GetValue("style", GV.GetNull);
            if (style != null)
                return 0.5;
            return 0;
#endif
        }

        /// <summary>
        /// Gets the specified cell’s uppermost neighbor at the specified position.
        /// </summary>
        [Obsolete("Use GetNeighborTop, GetNeighborBottom, GetNeighborLeft, GetNeighborRight instead.")]
        Cell? GetNeighbor(int cellIdx, NeighborPosition position)
        {
            Cell cell = this[cellIdx];
            if (cell.Column.Index == 0 && position == NeighborPosition.Left ||
              cell.Row.Index == 0 && position == NeighborPosition.Top ||
              cell.Row.Index + cell.MergeDown == cell.Table.Rows.Count - 1 && position == NeighborPosition.Bottom ||
              cell.Column.Index + cell.MergeRight == cell.Table.Columns.Count - 1 && position == NeighborPosition.Right)
                return null;

            switch (position)
            {
                case NeighborPosition.Top:
                case NeighborPosition.Left:
                    for (int index = cellIdx - 1; index >= 0; --index)
                    {
                        var currCell = this[index];
                        if (IsNeighbor(cell, currCell, position))
                            return currCell;
                    }
                    break;

                case NeighborPosition.Right:
                    if (cellIdx + 1 < Count)
                    {
                        var cell2 = this[cellIdx + 1];
                        if (cell2.Row.Index == cell.Row.Index)
                            return cell2;
                    }
                    for (int index = cellIdx + 1; index < Count; index++)
                    {
                        var currCell = this[index];
                        if (IsNeighbor(cell, currCell, position))
                            return currCell;
                    }
                    break;

                case NeighborPosition.Bottom:
                    for (int index = cellIdx + 1; index < Count; index++)
                    {
                        var currCell = this[index];
                        if (IsNeighbor(cell, currCell, position))
                            return currCell;
                    }
                    break;
            }
            return null;
        }

        /// <summary>
        /// Gets the top neighbor of a cell.
        /// TopRowIndexOverride may be used to manually override the row index of the top neighbor row.
        /// For PDF effective border determination in case of heading repetitions the lastHeadingRow index can be used to get a cell
        /// in the original last heading row as neighbor instead of the last content row on the page before.
        /// Repetitions of the heading are not managed in MergedCellList (what would be wrong for RTF rendering), therefore jumping to the original header is necessary.
        /// </summary>
        Cell? GetNeighborTop(int cellIdx, int? topRowIndexOverride = null)
        {
            var cell = this[cellIdx];
            if (cell.Row.Index == 0)
                return null;

            for (var index = cellIdx - 1; index >= 0; --index)
            {
                var currCell = this[index];
                if (IsNeighborTop(cell, currCell, topRowIndexOverride))
                    return currCell;
            }
            return null;
        }

        Cell? GetNeighborLeft(int cellIdx)
        {
            var cell = this[cellIdx];
            if (cell.Column.Index == 0)
                return null;

            if (cellIdx >= 1)
            {
                var cell2 = this[cellIdx - 1];
                if (cell2.Row.Index == cell.Row.Index)
                    return cell2;
            }
            for (var index = cellIdx - 2; index >= 0; --index)
            {
                var currCell = this[index];
                if (IsNeighborLeft(cell, currCell))
                    return currCell;
            }
            return null;
        }

        Cell? GetNeighborRight(int cellIdx)
        {
            var cell = this[cellIdx];
            if (cell.Column.Index + cell.MergeRight == cell.Table.Columns.Count - 1)
                return null;

            if (cellIdx + 1 < Count)
            {
                var cell2 = this[cellIdx + 1];
                if (cell2.Row.Index == cell.Row.Index)
                    return cell2;
            }
            for (var index = cellIdx + 2; index < Count; index++)
            {
                var currCell = this[index];
                if (IsNeighborRight(cell, currCell))
                    return currCell;
            }
            return null;
        }

        Cell? GetNeighborBottom(int cellIdx)
        {
            var cell = this[cellIdx];
            if (cell.Row.Index + cell.MergeDown == cell.Table.Rows.Count - 1)
                return null;

            for (var index = cellIdx + 1; index < Count; index++)
            {
                var currCell = this[index];
                if (IsNeighborBottom(cell, currCell))
                    return currCell;
            }
            return null;
        }

        /// <summary>
        /// Returns whether cell2 is a neighbor of cell1 at the specified position.
        /// </summary>
        [Obsolete("Use IsNeighborTop, IsNeighborBottom, IsNeighborLeft, IsNeighborRight instead.")]
        bool IsNeighbor(Cell cell1, Cell cell2, NeighborPosition position)
        {
            bool isNeighbor = false;
            switch (position)
            {
                case NeighborPosition.Bottom:
                    int bottomRowIdx = cell1.Row.Index + cell1.MergeDown + 1;
                    isNeighbor = cell2.Row.Index == bottomRowIdx &&
                      cell2.Column.Index <= cell1.Column.Index &&
                      cell2.Column.Index + cell2.MergeRight >= cell1.Column.Index;
                    break;

                case NeighborPosition.Left:
                    int leftClmIdx = cell1.Column.Index - 1;
                    isNeighbor = cell2.Row.Index <= cell1.Row.Index &&
                      cell2.Row.Index + cell2.MergeDown >= cell1.Row.Index &&
                      cell2.Column.Index + cell2.MergeRight == leftClmIdx;
                    break;

                case NeighborPosition.Right:
                    int rightClmIdx = cell1.Column.Index + cell1.MergeRight + 1;
                    isNeighbor = cell2.Row.Index <= cell1.Row.Index &&
                      cell2.Row.Index + cell2.MergeDown >= cell1.Row.Index &&
                      cell2.Column.Index == rightClmIdx;
                    break;

                case NeighborPosition.Top:
                    int topRowIdx = cell1.Row.Index - 1;
                    isNeighbor = cell2.Row.Index + cell2.MergeDown == topRowIdx &&
                      cell2.Column.Index + cell2.MergeRight >= cell1.Column.Index &&
                      cell2.Column.Index <= cell1.Column.Index;
                    break;
            }
            return isNeighbor;
        }

        bool IsNeighborBottom(Cell cell1, Cell cell2)
        {
            var bottomRowIdx = cell1.Row.Index + cell1.MergeDown + 1;
            var c1CI = cell1.Column.Index;
            var c2CI = cell2.Column.Index;
            var isNeighbor = cell2.Row.Index == bottomRowIdx &&
                             c2CI <= c1CI &&
                             c2CI + cell2.MergeRight >= c1CI;
            return isNeighbor;
        }

        bool IsNeighborLeft(Cell cell1, Cell cell2)
        {
            var leftClmIdx = cell1.Column.Index - 1;
            var isNeighbor = cell2.Row.Index <= cell1.Row.Index &&
                             cell2.Row.Index + cell2.MergeDown >= cell1.Row.Index &&
                             cell2.Column.Index + cell2.MergeRight == leftClmIdx;
            return isNeighbor;
        }

        bool IsNeighborRight(Cell cell1, Cell cell2)
        {
            var rightClmIdx = cell1.Column.Index + cell1.MergeRight + 1;
            var isNeighbor = cell2.Row.Index <= cell1.Row.Index &&
                             cell2.Row.Index + cell2.MergeDown >= cell1.Row.Index &&
                             cell2.Column.Index == rightClmIdx;
            return isNeighbor;
        }

        /// <summary>
        /// Returns true, if cell2 is the top neighbor of cell1.
        /// TopRowIndexOverride may be used to manually override the top neighbor row index determined by cell1.
        /// For PDF effective border determination in case of heading repetitions the lastHeadingRow index can be used to get a cell
        /// in the original last heading row as neighbor instead of the last content row on the page before.
        /// Repetitions of the heading are not managed in MergedCellList (what would be wrong for RTF rendering), therefore jumping to the original header is necessary.
        /// </summary>
        bool IsNeighborTop(Cell cell1, Cell cell2, int? topRowIndexOverride = null)
        {
            var topRowIdx = topRowIndexOverride ?? cell1.Row.Index - 1;
            var c1CI = cell1.Column.Index;
            var c2CI = cell2.Column.Index;
            return cell2.Row.Index + cell2.MergeDown == topRowIdx &&
                             c2CI + cell2.MergeRight >= c1CI &&
                             c2CI <= c1CI;
        }

        readonly SortedSet<int> _headerInsertionRowIndices = new();

        /// <summary>
        /// After a heading has been inserted in PDF rendering, the index of the first following content row should be added here
        /// to consider this heading when determining the effective borders.
        /// </summary>
        public void AddHeaderInsertionRowIndex(int rowIndex)
        {
            _headerInsertionRowIndices.Add(rowIndex);
        }

        int GetLastKnownHeaderInsertionRowIndex()
        {
            return _headerInsertionRowIndices.Any() ? _headerInsertionRowIndices.Last() : -1;
        }

    }
}
