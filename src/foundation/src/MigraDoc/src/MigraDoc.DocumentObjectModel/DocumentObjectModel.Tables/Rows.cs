// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;
using static MigraDoc.DocumentObjectModel.Tables.Cell;
using static MigraDoc.DocumentObjectModel.Tables.Row;

namespace MigraDoc.DocumentObjectModel.Tables
{
    /// <summary>
    /// Represents the collection of all rows of a table.
    /// </summary>
    public sealed class Rows : DocumentObjectCollection, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the Rows class.
        /// </summary>
        public Rows()
        {
            BaseValues = new RowsValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Rows class with the specified parent.
        /// </summary>
        internal Rows(DocumentObject parent) : base(parent)
        {
            BaseValues = new RowsValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Rows Clone()
            => (Rows)DeepCopy();

        /// <summary>
        /// Adds a new row to the rows collection. Allowed only if at least one column exists.
        /// </summary>
        public Row AddRow()
        {
            if (Table?.Columns.Count == 0)
                throw new InvalidOperationException("Cannot add row, because no columns exists.");

            var row = new Row();
            Add(row);
            return row;
        }

        /// <summary>
        /// Gets the table the rows collection belongs to.
        /// </summary>
        public Table? Table => Parent as Table;

        /// <summary>
        /// Gets a row by its index.
        /// </summary>
        public new Row this[int index]
        {
            get
            {
                if (base[index] is Row row)
                    return row;

                throw new Exception("Rows must only contain Row elements.");
            }
        }

        /// <summary>
        /// Gets or sets the row alignment of the table.
        /// </summary>
        public RowAlignment Alignment
        {
            get => Values.Alignment ?? RowAlignment.Left;
            set => Values.Alignment = value;
        }

        /// <summary>
        /// Gets or sets the left indent of the table. If row alignment is not Left, 
        /// the value is ignored.
        /// </summary>
        public Unit LeftIndent
        {
            get => Values.LeftIndent ?? Unit.Empty;
            set => Values.LeftIndent = value;
        }

        /// <summary>
        /// Gets or sets the default vertical alignment for all rows.
        /// </summary>
        public VerticalAlignment VerticalAlignment
        {
            get => Values.VerticalAlignment ?? VerticalAlignment.Top;
            set => Values.VerticalAlignment = value;
        }

        /// <summary>
        /// Gets or sets the height of the rows.
        /// </summary>
        public Unit Height
        {
            get => Values.Height ?? Unit.Empty;
            set => Values.Height = value;
        }

        /// <summary>
        /// Gets or sets the rule which is used to determine the height of the rows.
        /// </summary>
        public RowHeightRule HeightRule
        {
            get => Values.HeightRule ?? RowHeightRule.AtLeast;
            set => Values.HeightRule = value;
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
        /// Converts Rows into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteComment(Values.Comment);
            serializer.WriteLine("\\rows");

            int pos = serializer.BeginAttributes();

            if (Values.Alignment is not null)
                serializer.WriteSimpleAttribute("Alignment", Alignment);

            //if (Values.Height is not null)
            if (!Values.Height.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Height", Height);

            if (Values.HeightRule is not null)
                serializer.WriteSimpleAttribute("HeightRule", HeightRule);

            //if (Values.LeftIndent is not null)
            if (!Values.LeftIndent.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("LeftIndent", LeftIndent);

            if (Values.VerticalAlignment is not null)
                serializer.WriteSimpleAttribute("VerticalAlignment", VerticalAlignment);

            serializer.EndAttributes(pos);

            serializer.BeginContent();
            int rows = Count;
            if (rows > 0)
            {
                for (int row = 0; row < rows; row++)
                    this[row].Serialize(serializer);
            }
            else
                serializer.WriteComment("Invalid - no rows defined. Table will not render.");
            serializer.EndContent();
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitRows(this);

            foreach (var row in this)
                if (row is not null)
                    ((IVisitable)row).AcceptVisitor(visitor, visitChildren);
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Rows));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public RowsValues Values => (RowsValues)BaseValues!;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class RowsValues : Values
        {
            internal RowsValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public RowAlignment? Alignment { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? LeftIndent { get; set; }

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
            public string? Comment { get; set; }
        }
    }
}
