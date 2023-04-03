// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;

namespace MigraDoc.DocumentObjectModel.Tables
{
    /// <summary>
    /// Contains methods to simplify table formatting.
    /// </summary>
    public static class TableFormatter
    {
        /// <summary>
        /// Adds one variable count of rows and columns to the top, right, bottom and left of the table.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="newSurroundingCells"></param>
        public static void ExpandTable(Table table, int newSurroundingCells) 
            => ExpandTable(table, newSurroundingCells, newSurroundingCells, newSurroundingCells, newSurroundingCells);

        /// <summary>
        /// Adds one variable count of rows and one of columns to the right and the bottom of the table.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="newColsRight"></param>
        /// <param name="newRowsBottom"></param>
        public static void ExpandTable(Table table, int newColsRight, int newRowsBottom) 
            => ExpandTable(table, 0, newColsRight, newRowsBottom, 0);

        /// <summary>
        /// Adds variable counts of rows and columns to the top, right, bottom and of left of the table.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="newRowsTop"></param>
        /// <param name="newColsRight"></param>
        /// <param name="newRowsBottom"></param>
        /// <param name="newColsLeft"></param>
        public static void ExpandTable(Table table, int newRowsTop, int newColsRight, int newRowsBottom, int newColsLeft)
        {
            for (int i = 0; i < newRowsTop; i++)
                table.Rows.InsertObject(0, new Row());

            for (int i = 0; i < newColsLeft; i++)
            {
                table.Columns.InsertObject(0, new Column());
                foreach (var row in table.Rows)
                    (row as Row)?.Cells.InsertObject(0, new Cell());
            }

            for (int i = 0; i < newRowsBottom; i++)
                table.Rows.Add(new Row());
            
            for (int i = 0; i < newColsRight; i++)
            {
                table.Columns.Add(new Column());
                foreach (var row in table.Rows)
                    (row as Row)?.Cells.Add(new Cell());
            }
        }

        /// <summary>
        /// Sets the corner cells of the table to rounded corners.
        /// </summary>
        /// <param name="table"></param>
        public static void SetRoundedCorners(Table table)
        {
            int rowCount = table.Rows.Count;
            int colCount = table.Columns.Count;

            if (rowCount < 2 || colCount < 2)
                return;

            table.Rows[0].Cells[0].RoundedCorner = RoundedCorner.TopLeft;
            table.Rows[0].Cells[colCount - 1].RoundedCorner = RoundedCorner.TopRight;
            table.Rows[rowCount - 1].Cells[colCount - 1].RoundedCorner = RoundedCorner.BottomRight;
            table.Rows[rowCount - 1].Cells[0].RoundedCorner = RoundedCorner.BottomLeft;
        }

        /// <summary>
        /// Sets the width/height of the outer columns/rows to one value.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="width"></param>
        public static void SetOuterCellsWidth(Table table, Unit width) 
            => SetOuterCellsWidth(table, width, width);

        /// <summary>
        /// Sets each the width of the outer columns and the height of the outer rows to a value.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void SetOuterCellsWidth(Table table, Unit width, Unit height)
        {
            int rowCount = table.Rows.Count;
            int colCount = table.Columns.Count;

            if (rowCount < 2 || colCount < 2)
                return;

            table.Columns[0].Width = width;
            table.Columns[colCount - 1].Width = width;

            table.Rows[0].Height = height;
            table.Rows[0].HeightRule = RowHeightRule.Exactly;
            table.Rows[rowCount - 1].Height = height;
            table.Rows[rowCount - 1].HeightRule = RowHeightRule.Exactly;
        }

        /// <summary>
        /// Get the Shading of the first "targetRowCount" Rows from the Row below.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="targetRowCount"></param>
        public static void CopyNeighboringRowShadingToTop(Table table, int targetRowCount)
        {
            if (table.Rows.Count <= targetRowCount)
                return;

            Row source = table.Rows[targetRowCount];
            for (int i = 0; i < targetRowCount; i++)
                table.Rows[i].Shading = source.Shading.Clone();
        }

        /// <summary>
        /// Get the Shading of the last "targetRowCount" Rows from the Row above.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="targetRowCount"></param>
        public static void CopyNeighboringRowShadingToBottom(Table table, int targetRowCount)
        {
            int lastIndex = table.Rows.Count - 1;
            if (lastIndex - targetRowCount < 0)
                return;

            Row source = table.Rows[lastIndex - targetRowCount];
            for (int i = 0; i < targetRowCount; i++)
                table.Rows[lastIndex - i].Shading = source.Shading.Clone();
        }

        /// <summary>
        /// Removes all inner horizontal Borders between "start" index and the next "count" Rows.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        public static void RemoveInnerHorizontalBorders(Table table, int start, int count)
        {
            int end = start + count;
            for (int i = start; i < end - 1; i++)
            {
                if (i + 1 >= table.Rows.Count)
                    break;
                table.Rows[i].Borders.Bottom.Visible = false;
                table.Rows[i + 1].Borders.Top.Visible = false;
            }
        }

        /// <summary>
        /// Removes all inner vertical Borders between "start" index and the next "count" Columns.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        public static void RemoveInnerVerticalBorders(Table table, int start, int count)
        {
            int end = start + count;
            for (int i = start; i < end - 1; i++)
            {
                if (i + 1 >= table.Columns.Count)
                    break;
                table.Columns[i].Borders.Right.Visible = false;
                table.Columns[i + 1].Borders.Left.Visible = false;
            }
        }

        /// <summary>
        /// Inserts a magic glue column to avoid PageBreaks at the first "rowCountTop" and the last "rowCountBottom" of a Table.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="insertAtIndex"></param>
        /// <param name="rowCountTop">For example 3 for rounded corners Row, Header Row and first content Row.</param>
        /// <param name="rowCountBottom">For example 2 for last content Row and rounded corners Row.</param>
        public static void InsertGlueColumn(Table table, int insertAtIndex, int rowCountTop, int rowCountBottom)
        {
            if (table.Columns.Count == 0)
                return;

            int glueColumnIndex = insertAtIndex + 1;
            Unit glueColumnWidth = Unit.FromPoint(0.1);

            var glueColumn = new Column();
            glueColumn.Width = glueColumnWidth;

            if (table.Columns[insertAtIndex].Width > glueColumnWidth)
                table.Columns[insertAtIndex].Width -= glueColumnWidth;
            table.Columns.InsertObject(glueColumnIndex, glueColumn);

            foreach (var obj in table.Rows)
            {
                if (obj is Row row && row.Cells.Count > glueColumnIndex)
                    row.Cells.InsertObject(glueColumnIndex, new Cell());
            }

            int mergeTop = rowCountTop - 1;
            if (mergeTop < table.Rows.Count)
                table.Rows[0].Cells[glueColumnIndex].MergeDown = mergeTop;

            int mergeBottom = rowCountBottom - 1;
            if (table.Rows.Count - 1 - mergeBottom >= 0)
                table.Rows[table.Rows.Count - 1 - mergeBottom].Cells[glueColumnIndex].MergeDown = mergeBottom;
        }

        /// <summary>
        /// Surrounds "count" Elements beginning with index "start" with a one-Column Table of the specified width, where each row contains one of the paragraphs.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="columnWidth"></param>
        public static Table SurroundContentWithTable(Section section, int start, int count, Unit columnWidth)
        {
            var table = new Table();
            table.Columns.AddColumn();

            table.Borders.Width = 0;
            table.Columns[0].Width = columnWidth;

            while (count-- > 0)
            {
                var row = table.AddRow();
                row.Cells.Add(new Cell());

                var paragraph = section.Elements[start].Clone() as Paragraph;

                if (paragraph != null)
                {
                    row.Cells[0].Add(paragraph);
                    section.Elements.RemoveObjectAt(start);
                }
                else
                    Debug.Assert(false);
            }

            section.Elements.InsertObject(start, table);
            return table;
        }
    }
}
