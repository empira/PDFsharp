// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics.CodeAnalysis;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Base class of all objects of the MigraDoc Document Object Model.
    /// </summary>
    public abstract class DocumentObject : ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the DocumentObject class.
        /// </summary>
        internal DocumentObject()
        { }

        /// <summary>
        /// Initializes a new instance of the DocumentObject class with the specified parent.
        /// </summary>
        internal DocumentObject(DocumentObject parent)
        {
            Debug.Assert(parent != null, "Parent must not be null.");
            Parent = parent;
        }

        /// <summary>
        /// Creates a deep copy of the DocumentObject. The parent of the new object is null.
        /// </summary>
        public object Clone()
            => DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected virtual object DeepCopy()
        {
            var value = (DocumentObject)MemberwiseClone();
            // value.ResetCachedValues();
            value.Parent = null; // Calls ResetCachedValues().
            value.BaseValues = (Values)BaseValues.Clone();
            value.BaseValues.Owner = value;
            return value;
        }

        /// <summary>
        /// Creates an object using the default constructor.
        /// </summary>
        public object CreateValue(string name)
        {
            var vd = Meta[name];
            return vd.CreateValue() ?? throw new InvalidOperationException($"No Value named '{name}' exists.");
        }

        /// <summary>
        /// Gets the parent object, or null if the object has no parent yet.
        /// </summary>
        internal DocumentObject? Parent
        {
            get => _parent;
            // Calculate some properties when the Parent changes to avoid calculations in property getters.
            set { _parent = value; ResetCachedValues(); }
        }

        DocumentObject? _parent;

        /// <summary>
        /// Gets the document of the object, or null if the object is not associated with a document.
        /// </summary>
        public Document Document
        {
            // Note: Parent cannot change once it was set.
            [return: MaybeNull]
            get
            {
                if (_document != null)
                    return _document;
                var parent = Parent;
                if (parent is Document document)
                    return _document = document;
                // Call document at the parent - recursive call instead of while loop - and all parent objects will also update their _document fields.
                // Next call to Document from a sibling will succeed without climbing all the way up.
                return parent != null ? _document = parent.Document : null!;
            }
        }
        Document? _document;

        /// <summary>
        /// Gets the section of the object, or null if the object is not associated with a section.
        /// </summary>
        public Section? Section
        {
            // Note: Parent cannot change once it was set.
            [return: MaybeNull]
            get
            {
                if (_section != null)
                    return _section;
                var parent = Parent;
                if (parent != null)
                {
                    if (parent is Section section)
                        return _section = section;
                    return _section = parent.Section;
                }
                return null;
            }
        }
        Section? _section;

        /// <summary>
        /// Converts DocumentObject into DDL.
        /// </summary>
        internal abstract void Serialize(Serializer serializer);

        /// <summary>
        /// Returns the value with the specified name and optional value flags.
        /// </summary>
        public virtual object? GetValue(string name, GV flags = GV.ReadWrite)
            => Meta.GetValue(this, name, flags);

        /// <summary>
        /// Sets the given value and sets its parent afterwards.
        /// </summary>
        public virtual void SetValue(string name, object? val)
        {
            Meta.SetValue(this, name, val);
            if (val is DocumentObject documentObject)
                documentObject.Parent = this;
        }

        /// <summary>
        /// Determines whether this instance has a value of the given name.
        /// </summary>
        public virtual bool HasValue(string name)
            => Meta.HasValue(name);

        /// <summary>
        /// Determines whether the value of the given name is null.
        /// </summary>
        public virtual bool IsNull(string name)
            => Meta.IsNull(this, name);

        /// <summary>
        /// Resets the value of the given name, i.e. IsNull(name) will return true afterwards.
        /// </summary>
        public virtual void SetNull(string name)
            => Meta.SetNull(this, name);

        /// <summary>
        /// Determines whether this instance is null (not set).
        /// </summary>
        public virtual bool IsNull()
            => Meta.IsNull(this);

        /// <summary>
        /// Resets this instance, i.e. IsNull() will return true afterwards.
        /// </summary>
        public virtual void SetNull()
            => Meta.SetNull(this);

        /// <summary>
        /// Gets or sets a value that contains arbitrary information about this object.
        /// </summary>
        public object? Tag { get; set; }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal abstract Meta Meta { get; }

        /// <summary>
        /// Sets the parent of the specified value.
        /// If a parent is already set, an ArgumentException will be thrown.
        /// </summary>
        protected void SetParent(DocumentObject? val)
        {
            if (val != null)
            {
                if (val.Parent != null)
                    throw new ArgumentException(MdDomMsgs.ParentAlreadySet(val, this).Message);

                val.Parent = this;
                val._document = null;
                val._section = null;
            }
        }

        /// <summary>
        /// When overridden in a derived class resets cached values
        /// (like column index).
        /// </summary>
        internal virtual void ResetCachedValues()
        {
            _document = null;
            _section = null;
        }

        internal Values BaseValues = default!; // Is set on the constructors of derived classes.
    }
}
