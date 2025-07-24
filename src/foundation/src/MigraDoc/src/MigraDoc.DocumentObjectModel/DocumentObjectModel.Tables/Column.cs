// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics.CodeAnalysis;
using static MigraDoc.DocumentObjectModel.Tables.Cell;
using static MigraDoc.DocumentObjectModel.Tables.Row;

namespace MigraDoc.DocumentObjectModel.Tables
{
    /// <summary>
    /// Represents a column of a table.
    /// </summary>
    public class Column : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the Column class.
        /// </summary>
        public Column()
        {
            BaseValues = new ColumnValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Column class with the specified parent.
        /// </summary>
        internal Column(DocumentObject parent) : base(parent)
        {
            BaseValues = new ColumnValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Column Clone()
            => (Column)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var column = (Column)base.DeepCopy();
            column.ResetCachedValues();
            if (column.Values.Format != null)
            {
                column.Values.Format = column.Values.Format.Clone();
                column.Values.Format.Parent = column;
            }
            if (column.Values.Borders != null)
            {
                column.Values.Borders = column.Values.Borders.Clone();
                column.Values.Borders.Parent = column;
            }
            if (column.Values.Shading != null)
            {
                column.Values.Shading = column.Values.Shading.Clone();
                column.Values.Shading.Parent = column;
            }
            return column;
        }

        /// <summary>
        /// Resets the cached values.
        /// </summary>
        internal override void ResetCachedValues()
        {
            base.ResetCachedValues();
            _table = null;
            Values.Index = null;
            _hasIdx = false;
        }

        /// <summary>
        /// Gets the table the Column belongs to. Can be null if Column was not added to a table.
        /// </summary>
        public Table Table
        {
            [return: MaybeNull]
            get
            {
                if (_table == null)
                {
                    if (Parent is Columns clms)
                        _table = clms.Parent as Table;
                }
                return _table!;
            }
        }
        Table? _table;

        /// <summary>
        /// Gets the index of the column. First column has index 0.
        /// </summary>
        public int Index
        {
            get
            {
                if (_hasIdx)
                    return _idx;
                if (Values.Index is null && Parent is Columns clms)
                {
                    for (int idx = 0; idx < clms.Count; idx++)
                    {
                        clms[idx].Values.Index = idx;
                        clms[idx]._idx = idx;
                        clms[idx]._hasIdx = true;
                    }
                    if (_hasIdx)
                        return _idx;
                }
                _hasIdx = true;
                return _idx = Values.Index ?? throw new InvalidOperationException("Column index was not computed."); // TODO_OLD throw correct? Return -1 instead?
            }
        }
        int _idx;
        internal bool _hasIdx = false;

        /// <summary>
        /// Gets a cell by its row index. The first cell has index 0.
        /// </summary>
        public Cell? this[int index] => Values.Index is not null ? Table.Rows[index][Values.Index.Value] : null; // BUG_OLD Doesn’t use Index property to guarantee getter loop ran.

        /// <summary>
        /// Sets or gets the default style name for all cells of the column.
        /// </summary>
        public string Style
        {
            get => Values.Style ?? "";
            set => Values.Style = value;
        }

        /// <summary>
        /// Gets the default ParagraphFormat for all cells of the column.
        /// </summary>
        /// 
        public ParagraphFormat Format
        {
            get => Values.Format ??= new(this);
            set
            {
                SetParentOf(value);
                Values.Format = value;
            }
        }

        /// <summary>
        /// Gets or sets the width of a column.
        /// </summary>
        public Unit Width
        {
            get => Values.Width ?? Unit.Empty;
            set => Values.Width = value;
        }

        /// <summary>
        /// Gets or sets the default left padding for all cells of the column.
        /// </summary>
        public Unit LeftPadding
        {
            get => Values.LeftPadding ?? Unit.Empty;
            set => Values.LeftPadding = value;
        }

        /// <summary>
        /// Gets or sets the default right padding for all cells of the column.
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
            set
            {
                SetParentOf(value);
                Values.Borders = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of columns that should be kept together with
        /// current column in case of a page break.
        /// </summary>
        public int KeepWith
        {
            get => Values.KeepWith ?? 0;
            set => Values.KeepWith = value;
        }

        /// <summary>
        /// Gets or sets a value which define whether the column is a header.
        /// </summary>
        public bool HeadingFormat
        {
            get => Values.HeadingFormat ?? false;
            set => Values.HeadingFormat = value;
        }

        /// <summary>
        /// Gets the default Shading object for all cells of the column.
        /// </summary>
        public Shading Shading
        {
            get => Values.Shading ??= new(this);
            set
            {
                SetParentOf(value);
                Values.Shading = value;
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
        /// Converts Column into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteComment(Values.Comment);
            serializer.WriteLine("\\column");

            int pos = serializer.BeginAttributes();

            if (!String.IsNullOrEmpty(Values.Style))
                serializer.WriteSimpleAttribute("Style", Style);

            Values.Format?.Serialize(serializer, "Format", null);

            if (Values.HeadingFormat is not null)
                serializer.WriteSimpleAttribute("HeadingFormat", HeadingFormat);

            //if (Values.LeftPadding is not null)
            if (!Values.LeftPadding.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("LeftPadding", LeftPadding);

            //if (Values.RightPadding is not null)
            if (!Values.RightPadding.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("RightPadding", RightPadding);

            //if (Values.Width is not null)
            if (!Values.Width.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Width", Width);

            if (Values.KeepWith is not null)
                serializer.WriteSimpleAttribute("KeepWith", KeepWith);

            Values.Borders?.Serialize(serializer, null);

            Values.Shading?.Serialize(serializer);

            serializer.EndAttributes(pos);

            // columns has no content
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Column));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public ColumnValues Values => (ColumnValues)BaseValues!;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class ColumnValues : Values
        {
            internal ColumnValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public int? Index { get; set; } // This is reset by ResetCachedValues.

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
            public Unit? Width { get; set; }

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
            public int? KeepWith { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? HeadingFormat { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Shading? Shading { get; set; }

            /// <summary>
            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Comment { get; set; }
        }
    }
}
