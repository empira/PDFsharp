// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;

namespace MigraDoc.DocumentObjectModel.Tables
{
    /// <summary>
    /// Represents the columns of a table.
    /// </summary>
    public sealed class Columns : DocumentObjectCollection, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the Columns class.
        /// </summary>
        public Columns()
        {
            BaseValues = new ColumnsValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Columns class containing columns of the specified widths.
        /// </summary>
        public Columns(params Unit[] widths)
        {
            BaseValues = new ColumnsValues(this);
            foreach (var width in widths)
            {
                var clm = new Column
                {
                    Width = width
                };
                Add(clm);
            }
        }

        /// <summary>
        /// Initializes a new instance of the Columns class with the specified parent.
        /// </summary>
        internal Columns(DocumentObject parent) : base(parent)
        {
            BaseValues = new ColumnsValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Columns Clone() 
            => (Columns)DeepCopy();

        /// <summary>
        /// Adds a new column to the columns collection. Allowed only before any row was added.
        /// </summary>
        public Column AddColumn()
        {
            if (Table?.Rows.Count > 0)
                throw new InvalidOperationException("Cannot add column because rows collection is not empty.");

            var column = new Column();
            Add(column);
            return column;
        }

        /// <summary>
        /// Gets the table the columns collection belongs to.
        /// </summary>
        public Table? Table 
            => Parent as Table;

        /// <summary>
        /// Gets a column by its index.
        /// </summary>
        public new Column this[int index]
        {
            get
            {
                if (base[index] is not Column column)
                    throw new Exception("Columns must only contain Column elements.");

                return column;
            }
        }

        /// <summary>
        /// Gets or sets the default width of all columns.
        /// </summary>
        public Unit Width
        {
            get => Values.Width ?? Unit.Empty;
            set => Values.Width = value;
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
        /// Converts Columns into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteComment(Values.Comment);
            serializer.WriteLine("\\columns");

            int pos = serializer.BeginAttributes();

            //if (Values.Width is not null)
            if (!Values.Width.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Width", Width);

            serializer.EndAttributes(pos);

            serializer.BeginContent();
            int clms = Count;
            if (clms > 0)
            {
                for (int clm = 0; clm < clms; clm++)
                    this[clm].Serialize(serializer);
            }
            else
                serializer.WriteComment("Invalid - no columns defined. Table will not render.");
            serializer.EndContent();
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitColumns(this);
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Columns));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public ColumnsValues Values => (ColumnsValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class ColumnsValues : Values
        {
            internal ColumnsValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? Width { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Comment { get; set; }
        }
    }
}
