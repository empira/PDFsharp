// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;
using static MigraDoc.DocumentObjectModel.Tables.Row;

namespace MigraDoc.DocumentObjectModel.Tables
{
    /// <summary>
    /// Represents a table in a document.
    /// </summary>
    public class Table : DocumentObject, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the Table class.
        /// </summary>
        public Table()
        {
            BaseValues = new TableValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Table class with the specified parent.
        /// </summary>
        internal Table(DocumentObject parent) : base(parent)
        {
            BaseValues = new TableValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Table Clone()
            => (Table)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            Table table = (Table)base.DeepCopy();
            if (table.Values.Columns != null)
            {
                table.Values.Columns = table.Values.Columns.Clone();
                table.Values.Columns.Parent = table;
            }
            if (table.Values.Rows != null)
            {
                table.Values.Rows = table.Values.Rows.Clone();
                table.Values.Rows.Parent = table;
            }
            if (table.Values.Format != null)
            {
                table.Values.Format = table.Values.Format.Clone();
                table.Values.Format.Parent = table;
            }
            if (table.Values.Borders != null)
            {
                table.Values.Borders = table.Values.Borders.Clone();
                table.Values.Borders.Parent = table;
            }
            if (table.Values.Shading != null)
            {
                table.Values.Shading = table.Values.Shading.Clone();
                table.Values.Shading.Parent = table;
            }
            return table;
        }

        /// <summary>
        /// Adds a new column to the table. Allowed only before any row was added.
        /// </summary>
        public Column AddColumn() 
            => Columns.AddColumn();

        /// <summary>
        /// Adds a new column of the specified width to the table. Allowed only before any row was added.
        /// </summary>
        public Column AddColumn(Unit width)
        {
            Column clm = Columns.AddColumn();
            clm.Width = width;
            return clm;
        }

        /// <summary>
        /// Adds a new row to the table. Allowed only if at least one column was added.
        /// </summary>
        public Row AddRow() 
            => Rows.AddRow();

        /// <summary>
        /// Returns true if no cell exists in the table.
        /// </summary>
        public bool IsEmpty => Rows.Count == 0 || Columns.Count == 0;

        /// <summary>
        /// Sets a shading of the specified Color in the specified Tablerange.
        /// </summary>
        public void SetShading(int clm, int row, int clms, int rows, Color clr)
        {
            int rowsCount = Values.Rows?.Count ?? 0;
            int clmsCount = Values.Columns?.Count ?? 0;

            if (row < 0 || row >= rowsCount)
                throw new ArgumentOutOfRangeException(nameof(row), "Invalid row index.");

            if (clm < 0 || clm >= clmsCount)
                throw new ArgumentOutOfRangeException(nameof(clm), "Invalid column index.");

            if (rows <= 0 || row + rows > rowsCount)
                throw new ArgumentOutOfRangeException(nameof(rows), "Invalid row count.");

            if (clms <= 0 || clm + clms > clmsCount)
                throw new ArgumentOutOfRangeException(nameof(clms), "Invalid column count.");

            int maxRow = row + rows - 1;
            int maxClm = clm + clms - 1;
            for (int r = row; r <= maxRow; r++)
            {
                var currentRow = Rows[r];
                for (int c = clm; c <= maxClm; c++) 
                    currentRow[c].Shading.Color = clr;
            }
        }

        /// <summary>
        /// Sets the borders surrounding the specified range of the table.
        /// </summary>
        public void SetEdge(int clm, int row, int clms, int rows,
          Edge edge, BorderStyle style, Unit width, Color clr)  // TODO: make Color?
        {
            int maxRow = row + rows - 1;
            int maxClm = clm + clms - 1;
            for (int r = row; r <= maxRow; r++)
            {
                var currentRow = Rows[r];
                for (int c = clm; c <= maxClm; c++)
                {
                    var currentCell = currentRow[c];

                    Border border;
                    if ((edge & Edge.Top) == Edge.Top && r == row)
                    {
                        border = currentCell.Borders.Top;
                        border.Style = style;
                        border.Width = width;
                        if (clr != Color.Empty)
                            border.Color = clr;
                    }
                    if ((edge & Edge.Left) == Edge.Left && c == clm)
                    {
                        border = currentCell.Borders.Left;
                        border.Style = style;
                        border.Width = width;
                        if (clr != Color.Empty)
                            border.Color = clr;
                    }
                    if ((edge & Edge.Bottom) == Edge.Bottom && r == maxRow)
                    {
                        border = currentCell.Borders.Bottom;
                        border.Style = style;
                        border.Width = width;
                        if (clr != Color.Empty)
                            border.Color = clr;
                    }
                    if ((edge & Edge.Right) == Edge.Right && c == maxClm)
                    {
                        border = currentCell.Borders.Right;
                        border.Style = style;
                        border.Width = width;
                        if (clr != Color.Empty)
                            border.Color = clr;
                    }
                    if ((edge & Edge.Horizontal) == Edge.Horizontal && r < maxRow)
                    {
                        border = currentCell.Borders.Bottom;
                        border.Style = style;
                        border.Width = width;
                        if (clr != Color.Empty)
                            border.Color = clr;
                    }
                    if ((edge & Edge.Vertical) == Edge.Vertical && c < maxClm)
                    {
                        border = currentCell.Borders.Right;
                        border.Style = style;
                        border.Width = width;
                        if (clr != Color.Empty)
                            border.Color = clr;
                    }
                    if ((edge & Edge.DiagonalDown) == Edge.DiagonalDown)
                    {
                        border = currentCell.Borders.DiagonalDown;
                        border.Style = style;
                        border.Width = width;
                        if (clr != Color.Empty)
                            border.Color = clr;
                    }
                    if ((edge & Edge.DiagonalUp) == Edge.DiagonalUp)
                    {
                        border = currentCell.Borders.DiagonalUp;
                        border.Style = style;
                        border.Width = width;
                        if (clr != Color.Empty)
                            border.Color = clr;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the borders surrounding the specified range of the table.
        /// </summary>
        public void SetEdge(int clm, int row, int clms, int rows, Edge edge, BorderStyle style, Unit width)
        {
            SetEdge(clm, row, clms, rows, edge, style, width, Color.Empty);
        }

        /// <summary>
        /// Gets or sets the Columns collection of the table.
        /// </summary>
        public Columns Columns
        {
            get => Values.Columns ??= new(this);
            set
            {
                SetParent(value);
                Values.Columns = value;
            }
        }

        /// <summary>
        /// Gets the Rows collection of the table.
        /// </summary>
        public Rows Rows
        {
            get => Values.Rows ??= new(this);
            set
            {
                SetParent(value);
                Values.Rows = value;
            }
        }

        /// <summary>
        /// Sets or gets the default style name for all rows and columns of the table.
        /// </summary>
        public string Style
        {
            get => Values.Style ?? "";
            set => Values.Style = value;
        }

        /// <summary>
        /// Gets the default ParagraphFormat for all rows and columns of the table.
        /// </summary>
        public ParagraphFormat Format
        {
            get => Values.Format ??= new(this);
            set => Values.Format = value;
        }

        /// <summary>
        /// Gets or sets the default top padding for all cells of the table.
        /// </summary>
        public Unit TopPadding
        {
            get => Values.TopPadding ?? Unit.Empty;
            set => Values.TopPadding = value;
        }

        /// <summary>
        /// Gets or sets the default bottom padding for all cells of the table.
        /// </summary>
        public Unit BottomPadding
        {
            get => Values.BottomPadding ?? Unit.Empty;
            set => Values.BottomPadding = value;
        }

        /// <summary>
        /// Gets or sets the default left padding for all cells of the table.
        /// </summary>
        public Unit LeftPadding
        {
            get => Values.LeftPadding ?? Unit.Empty;
            set => Values.LeftPadding = value;
        }

        /// <summary>
        /// Gets or sets the default right padding for all cells of the table.
        /// </summary>
        public Unit RightPadding
        {
            get => Values.RightPadding ?? Unit.Empty;
            set => Values.RightPadding = value;
        }

        /// <summary>
        /// Gets the default Borders object for all cells of the column.
        /// </summary>
        public Borders Borders
        {
            get => Values.Borders ??= new(this);
            set => Values.Borders = value;
        }

        /// <summary>
        /// Gets the default Shading object for all cells of the column.
        /// </summary>
        public Shading Shading
        {
            get => Values.Shading ??= new(this);
            set
            {
                SetParent(value);
                Values.Shading = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether
        /// to keep all the table rows on the same page.
        /// </summary>
        public bool KeepTogether
        {
            get => Values.KeepTogether ?? false;
            set => Values.KeepTogether = value;
        }

        /// <summary>
        /// Gets or sets a comment associated with this object.
        /// </summary>
        public string Comment
        {
            get => Values.Comment ?? "";
            set => Values.Comment = value;
        }

        /// <summary>
        /// Converts Table into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteComment(Values.Comment);

            serializer.WriteLine("\\table");

            int pos = serializer.BeginAttributes();

            if (!String.IsNullOrEmpty(Values.Style))
                serializer.WriteSimpleAttribute("Style", Style);

            Values.Format?.Serialize(serializer, "Format", null);

            //if (Values.TopPadding is not null)
            if (!Values.TopPadding.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("TopPadding", TopPadding);

            //if (Values.LeftPadding is not null)
            if (!Values.LeftPadding.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("LeftPadding", LeftPadding);

            //if (Values.RightPadding is not null)
            if (!Values.RightPadding.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("RightPadding", RightPadding);

            //if (Values.BottomPadding is not null)
            if (!Values.BottomPadding.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("BottomPadding", BottomPadding);

            Values.Borders?.Serialize(serializer, null);

            Values.Shading?.Serialize(serializer);

            serializer.EndAttributes(pos);

            serializer.BeginContent();
            Columns.Serialize(serializer);
            Rows.Serialize(serializer);
            serializer.EndContent();
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitTable(this);

            ((IVisitable?)Values.Columns)?.AcceptVisitor(visitor, visitChildren);
            ((IVisitable?)Values.Rows)?.AcceptVisitor(visitor, visitChildren);
        }

        /// <summary>
        /// Gets the cell with the given row and column indices.
        /// </summary>
        public Cell this[int rwIdx, int clmIdx] => Rows[rwIdx].Cells[clmIdx];

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Table));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public TableValues Values => (TableValues)BaseValues!;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class TableValues : Values
        {
            internal TableValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Columns? Columns { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Rows? Rows { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Style { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public ParagraphFormat? Format { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? TopPadding { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? BottomPadding { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? LeftPadding { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? RightPadding { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Borders? Borders { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Shading? Shading { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? KeepTogether { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Comment { get; set; }
        }
    }
}
