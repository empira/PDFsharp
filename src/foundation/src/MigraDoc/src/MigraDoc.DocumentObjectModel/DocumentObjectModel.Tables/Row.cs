// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics.CodeAnalysis;
using MigraDoc.DocumentObjectModel.Visitors;

namespace MigraDoc.DocumentObjectModel.Tables
{
    /// <summary>
    /// Represents a row of a table.
    /// </summary>
    public class Row : DocumentObject, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the Row class.
        /// </summary>
        public Row()
        {
            BaseValues = new RowValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Row class with the specified parent.
        /// </summary>
        internal Row(DocumentObject parent) : base(parent)
        {
            BaseValues = new RowValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Row Clone()
            => (Row)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var row = (Row)base.DeepCopy();
            if (row.Values.Format != null)
            {
                row.Values.Format = row.Values.Format.Clone();
                row.Values.Format.Parent = row;
            }
            if (row.Values.Borders != null)
            {
                row.Values.Borders = row.Values.Borders.Clone();
                row.Values.Borders.Parent = row;
            }
            if (row.Values.Shading != null)
            {
                row.Values.Shading = row.Values.Shading.Clone();
                row.Values.Shading.Parent = row;
            }
            if (row.Values.Cells != null)
            {
                row.Values.Cells = row.Values.Cells.Clone();
                row.Values.Cells.Parent = row;
            }

            // Now reset cached values.

            row.ResetCachedValues();
            row.Cells.ResetCachedValues();
            for (var columnIndex = 0; columnIndex < row.Cells.Count; columnIndex++)
            {
                var cell = row.Cells[columnIndex];
                cell.ResetCachedValues();
            }

            return row;
        }

        /// <summary>
        /// Resets the index of the row.
        /// The value is stored in a backing field to make rendering faster.
        /// Set by DocumentObjectCollection when rows are inserted or deleted.
        /// </summary>
        public void ResetIndex(int index)
        {
            Values.Index = _idx = index;
        }

        /// <summary>
        /// Resets the cached values.
        /// Some values are cached in backing fields to make rendering faster.
        /// </summary>
        internal override void ResetCachedValues()
        {
            base.ResetCachedValues();
            _table = null;
            Values.Index = null;

            if (Parent is Rows rws)
            {
                // All children must update their ancestors.
                // Now reset cached values.
                Cells.ResetCachedValues();
                // Assign _table before resetting children that may depend on Table of their parent.
                _table = rws.Table;
                for (var columnIndex = 0; columnIndex < Cells.Count; columnIndex++)
                {
                    var cell = Cells[columnIndex];
                    cell.ResetCachedValues();
                }
            }
        }

        /// <summary>
        /// Gets the table the row belongs to. Can be null if Row was not added to a table.
        /// </summary>
        public Table Table
        {
            [return: MaybeNull]
            get
            {
                // Set in ResetCachedValues.
                return _table!;
            }
        }
        Table? _table;

        /// <summary>
        /// Gets the index of the row. First row has index 0.
        /// </summary>
        public int Index
        {
            get
            {
                // Set in ResetCachedValues/ResetIndex.
                return _idx;
            }
        }
        int _idx;

        /// <summary>
        /// Gets a cell by its column index. The first cell has index 0.
        /// </summary>
        public Cell this[int index] => Cells[index];

        /// <summary>
        /// Gets or sets the default style name for all cells of the row.
        /// </summary>
        public string Style
        {
            get => Values.Style ?? "";
            set => Values.Style = value;
        }

        /// <summary>
        /// Gets the default ParagraphFormat for all cells of the row.
        /// </summary>
        public ParagraphFormat Format
        {
            get => Values.Format ??= new(this);
            set
            {
                SetParent(value);
                Values.Format = value;
            }
        }

        /// <summary>
        /// Gets or sets the default vertical alignment for all cells of the row.
        /// </summary>
        public VerticalAlignment VerticalAlignment
        {
            get => Values.VerticalAlignment ?? VerticalAlignment.Top;
            set => Values.VerticalAlignment = value;
        }

        /// <summary>
        /// Gets or sets the height of the row.
        /// </summary>
        public Unit Height
        {
            get => Values.Height ?? Unit.Empty;
            set => Values.Height = value;
        }

        /// <summary>
        /// Gets or sets the rule which is used to determine the height of the row.
        /// </summary>
        public RowHeightRule HeightRule
        {
            get => Values.HeightRule ?? RowHeightRule.AtLeast;
            set => Values.HeightRule = value;
        }

        /// <summary>
        /// Gets or sets the default value for all cells of the row.
        /// </summary>
        public Unit TopPadding
        {
            get => Values.TopPadding ?? Unit.Empty;
            set => Values.TopPadding = value;
        }

        /// <summary>
        /// Gets or sets the default value for all cells of the row.
        /// </summary>
        public Unit BottomPadding
        {
            get => Values.BottomPadding ?? Unit.Empty;
            set => Values.BottomPadding = value;
        }

        /// <summary>
        /// Gets or sets a value which define whether the row is a header.
        /// </summary>
        public bool HeadingFormat
        {
            get => Values.HeadingFormat ?? false;
            set => Values.HeadingFormat = value;
        }

        /// <summary>
        /// Gets the default Borders object for all cells of the row.
        /// </summary>
        public Borders Borders
        {
            get => Values.Borders ??= new(this);
            set
            {
                SetParent(value);
                Values.Borders = value;
            }
        }

        /// <summary>
        /// Gets the default Shading object for all cells of the row.
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
        /// Gets or sets the number of rows that should be
        /// kept together with the current row in case of a page break.
        /// </summary>
        public int KeepWith
        {
            get => Values.KeepWith ?? 0;
            set => Values.KeepWith = value;
        }

        /// <summary>
        /// Gets the Cells collection of the table.
        /// </summary>
        public Cells Cells
        {
            get => Values.Cells ??= new(this);
            set
            {
                SetParent(value);
                Values.Cells = value;
            }
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
        /// Converts Row into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteComment(Values.Comment);
            serializer.WriteLine("\\row");

            int pos = serializer.BeginAttributes();

            if (!String.IsNullOrEmpty(Values.Style))
                serializer.WriteSimpleAttribute("Style", Style);

            Values.Format?.Serialize(serializer, "Format", null);

            //if (Values.Height is not null)
            if (!Values.Height.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Height", Height);

            if (Values.HeightRule is not null)
                serializer.WriteSimpleAttribute("HeightRule", HeightRule);

            //if (Values.TopPadding is not null)
            if (!Values.TopPadding.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("TopPadding", TopPadding);

            //if (Values.BottomPadding is not null)
            if (!Values.BottomPadding.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("BottomPadding", BottomPadding);

            if (Values.HeadingFormat is not null)
                serializer.WriteSimpleAttribute("HeadingFormat", HeadingFormat);

            if (Values.VerticalAlignment is not null)
                serializer.WriteSimpleAttribute("VerticalAlignment", VerticalAlignment);

            if (Values.KeepWith is not null)
                serializer.WriteSimpleAttribute("KeepWith", KeepWith);

            //Borders & Shading
            Values.Borders?.Serialize(serializer, null);

            Values.Shading?.Serialize(serializer);

            serializer.EndAttributes(pos);

            serializer.BeginContent();
            Values.Cells?.Serialize(serializer);
            serializer.EndContent();
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitRow(this);

            if (Values.Cells is not null)
            {
                foreach (var cell in Values.Cells)
                {
                    if (cell is not null)
                        ((IVisitable)cell).AcceptVisitor(visitor, visitChildren);
                }
            }
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Row));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public RowValues Values => (RowValues)BaseValues!;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class RowValues : Values
        {
            internal RowValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public int? Index { get; set; }

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
            public VerticalAlignment? VerticalAlignment { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? Height { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public RowHeightRule? HeightRule { get; set; }

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
            public bool? HeadingFormat { get; set; }

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
            public int? KeepWith { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Cells? Cells { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Comment { get; set; }
        }
    }
}
