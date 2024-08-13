// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Fields
{
    /// <summary>
    /// BookmarkField is used as target for Hyperlinks or PageRefs.
    /// </summary>
    public class BookmarkField : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the BookmarkField class.
        /// </summary>
        public BookmarkField()
        {
            BaseValues = new BookmarkFieldValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the BookmarkField class with the specified parent.
        /// </summary>
        internal BookmarkField(DocumentObject parent) : base(parent)
        {
            BaseValues = new BookmarkFieldValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the BookmarkField class with the necessary bookmark name.
        /// </summary>
        public BookmarkField(string name)
            : this()
        {
            Name = name;
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new BookmarkField Clone()
            => (BookmarkField)DeepCopy();

        /// <summary>
        /// Gets or sets the name of the bookmark.
        /// Used to reference the bookmark from a Hyperlink or PageRef.
        /// </summary>
        public string Name
        {
            get => Values.Name ?? "";
            set => Values.Name = value;
        }

        /// <summary>
        /// Converts BookmarkField into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            if (String.IsNullOrEmpty(Values.Name))
                throw new InvalidOperationException(MdDomMsgs.MissingObligatoryProperty(nameof(Name), nameof(BookmarkField)).Message);

            serializer.Write("\\field(Bookmark)[Name = \"" + Name.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"]");
        }

        /// <summary>
        /// Determines whether this instance is null (not set).
        /// </summary>
        public override bool IsNull()
        {
            return false;
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(BookmarkField));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public BookmarkFieldValues Values => (BookmarkFieldValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class BookmarkFieldValues : Values
        {
            internal BookmarkFieldValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Name { get; set; }
        }
    }
}
