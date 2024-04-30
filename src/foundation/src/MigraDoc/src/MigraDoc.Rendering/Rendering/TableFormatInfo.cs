// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Visitors;
using PdfSharp.Drawing;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Formatting information for tables.
    /// </summary>
    public class TableFormatInfo : FormatInfo
    {
        internal TableFormatInfo()
        { }

        internal override bool EndingIsComplete => _isEnding;

        internal override bool StartingIsComplete => !IsEmpty && StartRow > LastHeaderRow;

        internal override bool IsComplete => false;

        internal override bool IsEmpty => StartRow < 0;

        internal override bool IsEnding => _isEnding;
        internal bool _isEnding;

        internal override bool IsStarting => StartRow == LastHeaderRow + 1;

        internal int StartColumn = -1;
        internal int EndColumn = -1;

        /// <summary>
        /// The first row of the table that is showing on a page.
        /// </summary>
        public int StartRow = -1;
        /// <summary>
        /// The last row of the table that is showing on a page.
        /// </summary>
        public int EndRow = -1;

        internal int LastHeaderRow = -1;
        /// <summary>
        /// The formatted cells.
        /// </summary>
        public Dictionary<Cell, FormattedCell>? FormattedCells; //Sorted_List formattedCells;
        internal MergedCellList? MergedCells;
        internal Dictionary<int, XUnitPt>? BottomBorderMap; //Sorted_List bottomBorderMap;
        //internal Dictionary<int, int>? ConnectedRowsMap; //Sorted_List connectedRowsMap;
        internal int[]? ConnectedRowsMap; //Sorted_List connectedRowsMap;
        internal int[]? MinMergedCellRowMap;
    }
}
