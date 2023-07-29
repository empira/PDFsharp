// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Visitors;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Base class of all collections of the MigraDoc Document Object Model.
    /// </summary>
    public abstract class DocumentObjectCollection : DocumentObject, IList<DocumentObject?>, IVisitable
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
        /// Gets the first value in the Collection, if there is any, otherwise null.
        /// </summary>
        public DocumentObject? First => Count > 0 ? this[0] : null;

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new DocumentObjectCollection Clone()
            => (DocumentObjectCollection)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var coll = (DocumentObjectCollection)base.DeepCopy();

            int count = Count;
            coll._elements = new List<DocumentObject?>(count);
            for (int index = 0; index < count; ++index)
            {
                var doc = this[index];
                if (doc != null)
                {
                    //doc = doc.Clone() as DocumentObject;
                    doc = (DocumentObject)doc.Clone();
                    doc.Parent = coll;
                }
                coll._elements.Add(doc);
            }
            return coll;
        }

        /// <summary>
        /// Copies the entire collection to a compatible one-dimensional System.Array,
        /// starting at the specified index of the target array.
        /// </summary>
        public void CopyTo(DocumentObject?[] array, int index)
        {
            Array.Copy(_elements.ToArray(), 0, array, index, array.Length - index);
        }

        /// <summary>
        /// Gets a value indicating whether the Collection is read-only.
        /// </summary>
        bool ICollection<DocumentObject?>.IsReadOnly => false;

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count => _elements.Count;

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public void Clear()
        {
            // Call ResetCachedValues for all affected objects.
            int count = ((IList<DocumentObject?>)this).Count;
            for (int idx = 0; idx < count; ++idx)
            {
                var obj = (DocumentObject)((IList<DocumentObject?>)this)[idx]!;
                obj.ResetCachedValues();
                if (obj is Row row)
                    UpdateRowCachedValues(row);
            }

            // Now clear the list.
            ((IList<DocumentObject?>)this).Clear();
        }

        /// <summary>
        /// Inserts an object at the specified index.
        /// </summary>
        public virtual void InsertObject(int index, DocumentObject val)
        {
            SetParent(val);
            ((IList<DocumentObject?>)this).Insert(index, val);
            // Call ResetCachedValues for all objects moved by the Insert operation.
            int count = ((IList<DocumentObject?>)this).Count;
            for (int idx = index + 1; idx < count; ++idx)
            {
                var obj = ((IList<DocumentObject?>)this)[idx]!;
                if (obj is Row row)
                    UpdateRowCachedValues(row);
                obj.ResetCachedValues();
            }
        }

        /// <summary>
        /// Determines the index of a specific item in the collection.
        /// </summary>
        public int IndexOf(DocumentObject? val)
        {
            //return ((IList)this).IndexOf(val);
            return _elements.IndexOf(val);
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        public virtual DocumentObject? this[int index]
        {
            get => _elements[index];
            set
            {
                SetParent(value);
                _elements[index] = value;
            }
        }

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

        /// <summary>
        /// Removes the element at the specified index.
        /// </summary>
        public void RemoveObjectAt(int index)
        {
            ((IList<DocumentObject?>)this).RemoveAt(index);
            // Call ResetCachedValues for all objects moved by the RemoveAt operation.
            int count = ((IList<DocumentObject?>)this).Count;
            for (int idx = index; idx < count; ++idx)
            {
                var obj = (DocumentObject)((IList<DocumentObject?>)this)[idx]!;
                if (obj is Row row)
                    UpdateRowCachedValues(row);
                obj.ResetCachedValues();
            }
        }

        /// <summary>
        /// Inserts the object into the collection and sets its parent.
        /// </summary>
        public virtual void Add(DocumentObject? value)
        {
            SetParent(value);
            _elements.Add(value);

            if (value is Row row)
                UpdateRowCachedValues(row, true);
        }

        /// <summary>
        /// Calculate some properties of the Row object when the Rows collection has inserts or removes.
        /// </summary>
        void UpdateRowCachedValues(Row row, bool reset = false)
        {
            if (row.Parent is Rows rws)
            {
                if (reset)
                {
                    // Called from AddRow: Calculate index without loop.
                    var idx = rws.Count - 1;
                    row.Values.Index = idx;
                    row.ResetIndex(idx);
                }
                else
                {
                    // Called from Clear, InsertObject, or RemoveObjectAt: use a loop.
                    for (int i = 0; i < rws.Count; ++i)
                    {
                        if (row == rws[i])
                        {
                            row.Values.Index = i;
                            row.ResetIndex(i);
                        }
                    }
                }
            }
            else
            {
                row.Values.Index = null;
            }

            if (reset)
                row.ResetCachedValues();
        }

        ///// <summary>
        ///// Determines whether this instance is null.
        ///// </summary>
        //public override bool IsNull()
        //{
        //    if (!Meta.IsNull(this))
        //        return false;
        //    //if (_elements == null)
        //    //    return true;
        //    foreach (DocumentObject? doc in _elements)
        //    {
        //        if (doc != null && !doc.IsNull())
        //            return false;
        //    }
        //    return true;
        //}

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitDocumentObjectCollection(this);

            foreach (var doc in this)
            {
                var visitable = doc as IVisitable;
                visitable?.AcceptVisitor(visitor, visitChildren);
            }
        }

        /// <summary>
        /// Returns an enumerator that can iterate through this collection.
        /// </summary>
        public IEnumerator<DocumentObject?> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        /// <summary>
        /// The elements can contain null depending on the derived class.
        /// </summary>
        List<DocumentObject?> _elements = new();

        internal IEnumerable<DocumentObject?> Elements => _elements;

        #region IList Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        /// <summary>
        /// Gets or sets the element at the specified index. 
        /// </summary>
        DocumentObject? IList<DocumentObject?>.this[int index]
        {
            get => _elements[index];
            set => _elements[index] = value; // ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Removes the item at the specified index from the Collection.
        /// </summary>
        void IList<DocumentObject?>.RemoveAt(int index)
            => _elements.RemoveAt(index);

        /// <summary>
        /// Inserts an object at the specified index.
        /// </summary>
        void IList<DocumentObject?>.Insert(int index, DocumentObject? value)
            => _elements.Insert(index, value/* ?? throw new ArgumentNullException(nameof(value))*/);

        /// <summary>
        /// Removes the first occurrence of the specific object.
        /// </summary>
        bool ICollection<DocumentObject?>.Remove(DocumentObject? value)
            => _elements.Remove(value /*?? throw new ArgumentNullException(nameof(value))*/);

        /// <summary>
        /// Determines whether an element exists.
        /// </summary>
        bool ICollection<DocumentObject?>.Contains(DocumentObject? value)
            => _elements.Contains(value /*?? throw new ArgumentNullException(nameof(value))*/);

        /// <summary>
        /// Determines the index of a specific item in the Collection.
        /// </summary>
        int IList<DocumentObject?>.IndexOf(DocumentObject? value)
            => _elements.IndexOf(value /*?? throw new ArgumentNullException(nameof(value))*/);

        /// <summary>
        /// Adds an item to the Collection.
        /// </summary>
        void ICollection<DocumentObject?>.Add(DocumentObject? value)
            => _elements.Add(value /*?? throw new ArgumentNullException(nameof(value))*/);

        /// <summary>
        /// Removes all items from the Collection.
        /// </summary>
        void ICollection<DocumentObject?>.Clear()
            => _elements.Clear();

        #endregion

        ///// <summary>
        ///// Returns the value with the specified name and optional value flags.
        ///// </summary>
        //public override object? GetValue(string name, GV flags = GV.ReadWrite)
        //    => Meta.GetValue(this, name, flags);

        ///// <summary>
        ///// Sets the given value and sets its parent afterwards.
        ///// </summary>
        //public override void SetValue(string name, object? val)
        //{
        //    Meta.SetValue(this, name, val);
        //    if (val is DocumentObject documentObject)
        //        documentObject.Parent = this;
        //}

        ///// <summary>
        ///// Determines whether this instance has a value of the given name.
        ///// </summary>
        //public override bool HasValue(string name)
        //    => Meta.HasValue(name);

        /// <summary>
        /// Determines whether the value of the given name is null.
        /// </summary>
        public override bool IsNull(string name)
            => Meta.IsNull(this, name);

        ///// <summary>
        ///// Resets the value of the given name, i.e. IsNull(name) will return true afterwards.
        ///// </summary>
        //public override void SetNull(string name)
        //{
        //    //Meta.SetNull(this, name);

        //    //_elements.Clear();
        //}

        /// <summary>
        /// Determines whether this instance is null (not set).
        /// </summary>
        public override bool IsNull()
        {
            //Meta.IsNull(this);

            foreach (var dom in _elements)
            {
                if (dom?.IsNull() is false)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Resets this instance, i.e. IsNull() will return true afterwards.
        /// </summary>
        public override void SetNull()
        {
            //Meta.SetNull(this);
            _elements.Clear();
        }
    }
}
