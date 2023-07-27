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
        // AG_HACK
        //public int Compare(object lhs, object rhs)
        //{
        //  if (!(lhs is Cell))
        //    throw new ArgumentException(DomSR.CompareJustCells, "lhs");

        //  if (!(rhs is Cell))
        //    throw new ArgumentException(DomSR.CompareJustCells, "rhs");

        //  Cell cellLhs = lhs as Cell;
        //  Cell cellRhs = rhs as Cell;
        //  int rowCmpr = cellLhs.Row.Index - cellRhs.Row.Index;
        //  if (rowCmpr != 0)
        //    return rowCmpr;

        //  return cellLhs.Column.Index - cellRhs.Column.Index;
        //}

        //int IComparer<object>.Compare(object lhs, object rhs)
        //{
        //  if (!(lhs is Cell))
        //    throw new ArgumentException(DomSR.CompareJustCells, "lhs");

        //  if (!(rhs is Cell))
        //    throw new ArgumentException(DomSR.CompareJustCells, "rhs");

        //  Cell cellLhs = lhs as Cell;
        //  Cell cellRhs = rhs as Cell;
        //  int rowCmpr = cellLhs.Row.Index - cellRhs.Row.Index;
        //  if (rowCmpr != 0)
        //    return rowCmpr;

        //  return cellLhs.Column.Index - cellRhs.Column.Index;
        //}

        /// <summary>
        /// Compares the specified cells.
        /// </summary>
        /// <returns></returns>
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
