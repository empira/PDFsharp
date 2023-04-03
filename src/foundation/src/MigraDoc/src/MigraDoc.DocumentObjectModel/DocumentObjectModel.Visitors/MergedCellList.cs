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

                            for (int idxMergedRows = 0; idxMergedRows <= mergeDown; ++idxMergedRows)
                            {
                                for (int idxMergedColumns = 0; idxMergedColumns <= mergeRight; ++idxMergedColumns)
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
                    // TODO Make this smarter?
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
        /// Gets a borders object that should be used for rendering.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        ///   Thrown when the cell is not in this list.
        ///   This situation occurs if the given cell is merged "away" by a previous one.
        /// </exception>
        public Borders GetEffectiveBorders(Cell cell)
        {
#if CACHE
            if (_effectiveBordersLookup.TryGetValue(cell, out var result))
                return result;
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

            var topNeighbor = GetNeighborTop(cellIdx);
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
            _effectiveBordersLookup.Add(cell, borders);
#endif
            return borders;
        }
#if CACHE
        Dictionary<Cell, Borders> _effectiveBordersLookup = new Dictionary<Cell, Borders>();
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
        /// If that border doesn't exist, it returns a new border object that inherits all properties from the given borders object.
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

            // TODO Avoid 'GetValue("'.
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
        /// Gets the specified cell's uppermost neighbor at the specified position.
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
                    for (int index = cellIdx - 1; index >= 0; --index) // THHO4THHO Check this!
                    {
                        var currCell = this[index];
                        if (IsNeighbor(cell, currCell, position))
                            return currCell;
                    }
                    break;

                case NeighborPosition.Bottom:
                    for (int index = cellIdx + 1; index < Count; ++index)
                    {
                        var currCell = this[index];
                        if (IsNeighbor(cell, currCell, position))
                            return currCell;
                    }
                    break;
            }
            return null;
        }

        Cell? GetNeighborTop(int cellIdx)
        {
            Cell cell = this[cellIdx];
            if (cell.Row.Index == 0)
                return null;

            for (int index = cellIdx - 1; index >= 0; --index)
            {
                var currCell = this[index];
                if (IsNeighborTop(cell, currCell))
                    return currCell;
            }
            return null;
        }

        Cell? GetNeighborLeft(int cellIdx)
        {
            Cell cell = this[cellIdx];
            if (cell.Column.Index == 0)
                return null;

            if (cellIdx >= 1)
            {
                var cell2 = this[cellIdx - 1];
                if (cell2.Row.Index == cell.Row.Index)
                    return cell2;
            }
            for (int index = cellIdx - 2; index >= 0; --index)
            {
                var currCell = this[index];
                if (IsNeighborLeft(cell, currCell))
                    return currCell;
            }
            return null;
        }

        Cell? GetNeighborRight(int cellIdx)
        {
            Cell cell = this[cellIdx];
            if (cell.Column.Index + cell.MergeRight == cell.Table.Columns.Count - 1)
                return null;

            if (cellIdx + 1 < Count)
            {
                var cell2 = this[cellIdx + 1];
                if (cell2.Row.Index == cell.Row.Index)
                    return cell2;
            }
            for (int index = cellIdx + 2; index < Count; ++index)
            {
                var currCell = this[index];
                if (IsNeighborRight(cell, currCell))
                    return currCell;
            }
            return null;
        }

        Cell? GetNeighborBottom(int cellIdx)
        {
            Cell cell = this[cellIdx];
            if (cell.Row.Index + cell.MergeDown == cell.Table.Rows.Count - 1)
                return null;

            for (int index = cellIdx + 1; index < Count; ++index)
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
            bool isNeighbor = false;
            int bottomRowIdx = cell1.Row.Index + cell1.MergeDown + 1;
            var c1CI = cell1.Column.Index;
            var c2CI = cell2.Column.Index;
            isNeighbor = cell2.Row.Index == bottomRowIdx &&
                         c2CI <= c1CI &&
                         c2CI + cell2.MergeRight >= c1CI;
            return isNeighbor;
        }

        bool IsNeighborLeft(Cell cell1, Cell cell2)
        {
            bool isNeighbor = false;
            int leftClmIdx = cell1.Column.Index - 1;
            isNeighbor = cell2.Row.Index <= cell1.Row.Index &&
                         cell2.Row.Index + cell2.MergeDown >= cell1.Row.Index &&
                         cell2.Column.Index + cell2.MergeRight == leftClmIdx;
            return isNeighbor;
        }

        bool IsNeighborRight(Cell cell1, Cell cell2)
        {
            bool isNeighbor = false;
            int rightClmIdx = cell1.Column.Index + cell1.MergeRight + 1;
            isNeighbor = cell2.Row.Index <= cell1.Row.Index &&
                         cell2.Row.Index + cell2.MergeDown >= cell1.Row.Index &&
                         cell2.Column.Index == rightClmIdx;
            return isNeighbor;
        }

        bool IsNeighborTop(Cell cell1, Cell cell2)
        {
            int topRowIdx = cell1.Row.Index - 1;
            var c1CI = cell1.Column.Index;
            var c2CI = cell2.Column.Index;
            return cell2.Row.Index + cell2.MergeDown == topRowIdx &&
                             c2CI + cell2.MergeRight >= c1CI &&
                             c2CI <= c1CI;
        }
    }
}
