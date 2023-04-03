// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections;

namespace PdfSharp.Charting
{
    /// <summary>
    /// Base class of all collections.
    /// </summary>
    public abstract class DocumentObjectCollection : DocumentObject, IList
    {
        /// <summary>
        /// Initializes a new instance of the DocumentObjectCollection class.
        /// </summary>
        internal DocumentObjectCollection()
        { }

        /// <summary>
        /// Initializes a new instance of the DocumentObjectCollection class with the specified parent.
        /// </summary>
        internal DocumentObjectCollection(DocumentObject parent) : base(parent)
        { }

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        public virtual DocumentObject? this[int index]
        {
            get => _elements[index];
            internal set => _elements[index] = value;
        }

        #region Methods
        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new DocumentObjectCollection Clone() => (DocumentObjectCollection)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var coll = (DocumentObjectCollection)base.DeepCopy();

            int count = Count;
            coll._elements = new List<DocumentObject?>(count);
            for (int index = 0; index < count; ++index)
                coll._elements.Add((DocumentObject?)this[index]?.Clone());
            return coll;
        }

        /// <summary>
        /// Copies the Array or a portion of it to a one-dimensional array.
        /// </summary>
        public void CopyTo(Array array, int index)
            => _elements.CopyTo(_elements.ToArray(), index); // 4STLA Check this implementation. "array" is not used.

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public void Clear()
            => _elements.Clear();

        /// <summary>
        /// Inserts an element into the collection at the specified position.
        /// </summary>
        public virtual void InsertObject(int index, DocumentObject val)
            => _elements.Insert(index, val);

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence.
        /// </summary>
        public int IndexOf(DocumentObject val)
            => _elements.IndexOf(val);

        /// <summary>
        /// Removes the element at the specified index.
        /// </summary>
        public void RemoveObjectAt(int index)
            => _elements.RemoveAt(index);

        /// <summary>
        /// Adds the specified document object to the collection.
        /// </summary>
        public virtual void Add(DocumentObject? value)
        {
            if (value != null)
                value.Parent = this;

            _elements.Add(value);
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count => _elements.Count;

        /// <summary>
        /// Gets the first value in the collection, if there is any, otherwise null.
        /// </summary>
        public DocumentObject? First => Count > 0 ? this[0] : null;

        /// <summary>
        /// Gets the last element, or null if no such element exists.
        /// </summary>
        public DocumentObject? LastObject
        {
            get
            {
                int count = _elements.Count;
                return count > 0 ? _elements[count - 1] : null;
            }
        }
        #endregion

        #region IList
        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => false;

        object? IList.this[int index]
        {
            get => _elements[index];
            set => _elements[index] = (DocumentObject?)value ?? throw new ArgumentNullException(nameof(value));
        }

        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException("IList.RemoveAt");
            // TODO:  Add DocumentObjectCollection.RemoveAt implementation
        }

        void IList.Insert(int index, object? value)
        {
            throw new NotImplementedException("IList.Insert");
            // TODO:  Add DocumentObjectCollection.Insert implementation
        }

        void IList.Remove(object? value)
        {
            throw new NotImplementedException("IList.Remove");
            // TODO:  Add DocumentObjectCollection.Remove implementation
        }

        bool IList.Contains(object? value)
        {
            throw new NotImplementedException("IList.Contains");
            // TODO:  Add DocumentObjectCollection.Contains implementation
            //return false;
        }

        int IList.IndexOf(object? value)
        {
            throw new NotImplementedException("IList.IndexOf");
            // TODO:  Add DocumentObjectCollection.System.Collections.IList.IndexOf implementation
            //return 0;
        }

        int IList.Add(object? value)
        {
            throw new NotImplementedException("IList.Add");
            // TODO:  Add DocumentObjectCollection.Add implementation
            //return 0;
        }
        #endregion

        #region ICollection

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot { get; } = new Object();

        #endregion

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator GetEnumerator() => _elements.GetEnumerator();

        List<DocumentObject?> _elements = new();
    }
}
