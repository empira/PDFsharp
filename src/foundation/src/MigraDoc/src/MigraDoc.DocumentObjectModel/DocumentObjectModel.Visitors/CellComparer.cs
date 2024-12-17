// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Tables;

namespace MigraDoc.DocumentObjectModel.Visitors
{
    /// <summary>
    /// Comparer for the cell positions within a table.
    /// It compares the cell positions from top to bottom and left to right.
    /// </summary>
    public class CellComparer : IComparer<Cell>
    {
        /// <summary>
        /// Compares the specified cells.
        /// </summary>
        public int Compare(Cell? cellLhs, Cell? cellRhs)
        {
            if (cellLhs != null && cellRhs != null)
            {
                int rowCmpr = cellLhs.Row.Index - cellRhs.Row.Index;
                if (rowCmpr != 0)
                    return rowCmpr;

                return cellLhs.Column.Index - cellRhs.Column.Index;
            }
            throw new NullReferenceException("Cells must not be null for comparison.");
        }
    }
}
