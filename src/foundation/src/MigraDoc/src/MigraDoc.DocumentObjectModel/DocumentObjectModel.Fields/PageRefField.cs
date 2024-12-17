// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Fields
{
    /// <summary>
    /// PageRefField is used to reference the page number of a bookmark in the document.
    /// </summary>
    public class PageRefField : NumericFieldBase
    {
        /// <summary>
        /// Initializes a new instance of the PageRefField class.
        /// </summary>    
        public PageRefField(TextRenderOption textRenderOption = TextRenderOption.Default) : base(textRenderOption)
        {
            BaseValues = new PageRefFieldValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the PageRefField class with the necessary bookmark name.
        /// </summary>
        public PageRefField(string name, TextRenderOption textRenderOption = TextRenderOption.Default) : this(textRenderOption)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the PageRefField class with the specified parent.
        /// </summary>
        internal PageRefField(DocumentObject parent, TextRenderOption textRenderOption = TextRenderOption.Default) : base(parent, textRenderOption)
        { }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new PageRefField Clone() 
            => (PageRefField)DeepCopy();

        /// <summary>
        /// Gets or sets the bookmark name whose page is to be shown.
        /// </summary>
        public string Name
        {
            get => Values.Name ?? "";
            set => Values.Name = value;
        }

        /// <summary>
        /// Converts PageRefField into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            var str = "\\field(PageRef)";
            str += "[Name = \"" + Name + "\"";

            if (!String.IsNullOrEmpty(Values.Format))
                str += " Format = \"" + Format + "\"";
            str += "]";

            serializer.Write(str);
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(PageRefField));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new PageRefFieldValues Values => (PageRefFieldValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class PageRefFieldValues : NumericFieldBaseValues
        {
            internal PageRefFieldValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Name { get; set; }
        }
    }
}
