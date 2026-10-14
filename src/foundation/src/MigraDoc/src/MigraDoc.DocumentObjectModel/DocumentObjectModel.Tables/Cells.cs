// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics.CodeAnalysis;

namespace MigraDoc.DocumentObjectModel.Tables
{
    /// <summary>
    /// Represents the collection of all cells of a row.
    /// </summary>
    public class Cells : DocumentObjectCollection
    {
        /// <summary>
        /// Initializes a new instance of the Cells class.
        /// </summary>
        public Cells()
        {
            BaseValues = new CellsValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Cells class with the specified parent.
        /// </summary>
        internal Cells(DocumentObject parent) : base(parent)
        {
            BaseValues = new CellsValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Cells Clone()
        {
            var cells = (Cells)base.DeepCopy();
            cells.ResetCachedValues();
            return cells;
        }

        /// <summary>
        /// Resets the cached values.
        /// </summary>
        internal override void ResetCachedValues()
        {
            base.ResetCachedValues();
            _row = null;
            _table = null;
        }

        /// <summary>
        /// Gets the table the cells collection belongs to. Can be null if Cells was not added to a table.
        /// </summary>
        public Table Table
        {
            [return: MaybeNull]
            get
            {
                if (_table == null && Parent is Row rw)
                    _table = rw.Table;

                return _table!;
            }
        }
        Table? _table;

        /// <summary>
        /// Gets the row the cells collection belongs to. Can be null if Cells was not added to a row.
        /// </summary>
        public Row Row
        {
            [return: MaybeNull]
            get { return _row ??= (Parent as Row)!; }
        }
        Row? _row;

        /// <summary>
        /// Gets a cell by its index. The first cell has the index 0.
        /// </summary>
        public new Cell this[int index]
        {
            get
            {
                if (index < 0 || (Table != null && index >= Table.Columns.Count))
                    throw new ArgumentOutOfRangeException(nameof(index));

                Resize(index);

                if (base[index] is Cell cell)
                    return cell;

                throw new Exception("Cells must only contain Cell elements.");
            }
        }

        /// <summary>
        /// Resizes these cells¹ list if necessary.
        /// </summary>
        void Resize(int index)
        {
            for (int currentIndex = Count; currentIndex <= index; currentIndex++)
                Add(new Cell());
        }

        /// <summary>
        /// Converts Cells into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            int cells = Count;
            for (int cell = 0; cell < cells; cell++)
                this[cell].Serialize(serializer);
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Cells));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public CellsValues Values => (CellsValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class CellsValues : Values
        {
            internal CellsValues(DocumentObject owner) : base(owner)
            { }
        }
    }
}
