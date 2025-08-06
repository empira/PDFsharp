// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Contains information about document content, author etc.
    /// </summary>
    public class DocumentInfo : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the DocumentInfo class.
        /// </summary>
        public DocumentInfo()
        {
            BaseValues = new DocumentInfoValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the DocumentInfo class with the specified parent.
        /// </summary>
        internal DocumentInfo(DocumentObject parent) : base(parent)
        {
            BaseValues = new DocumentInfoValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new DocumentInfo Clone()
            => (DocumentInfo)DeepCopy();

        /// <summary>
        /// Gets or sets the document title.
        /// </summary>
        public string Title
        {
            get => Values.Title ?? "";
            set => Values.Title = value;
        }

        /// <summary>
        /// Gets or sets the document author.
        /// </summary>
        public string Author
        {
            get => Values.Author ?? "";
            set => Values.Author = value;
        }

        /// <summary>
        /// Gets or sets keywords related to the document.
        /// </summary>
        public string Keywords
        {
            get => Values.Keywords ?? "";
            set => Values.Keywords = value;
        }

        /// <summary>
        /// Gets or sets the subject of the document.
        /// </summary>
        public string Subject
        {
            get => Values.Subject ?? "";
            set => Values.Subject = value;
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
        /// Converts DocumentInfo into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteComment(Values.Comment);
            int pos = serializer.BeginContent("Info");
            {
                if (Title != "")
                    serializer.WriteSimpleAttribute("Title", Title);

                if (Subject != "")
                    serializer.WriteSimpleAttribute("Subject", Subject);

                if (Author != "")
                    serializer.WriteSimpleAttribute("Author", Author);

                if (Keywords != "")
                    serializer.WriteSimpleAttribute("Keywords", Keywords);
            }
            serializer.EndContent(pos);
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(DocumentInfo));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public DocumentInfoValues Values => (DocumentInfoValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class DocumentInfoValues : Values
        {
            internal DocumentInfoValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Title { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Author { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Keywords { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Subject { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Comment { get; set; }
        }
    }
}
