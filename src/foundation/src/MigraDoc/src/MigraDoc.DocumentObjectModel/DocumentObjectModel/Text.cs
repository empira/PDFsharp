// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents text in a paragraph.
    /// </summary>
    public class Text : TextBasedDocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the Text class.
        /// </summary>
        public Text(TextRenderOption textRenderOption = TextRenderOption.Default) : base(textRenderOption)
        {
            BaseValues = new TextValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Text class with the specified parent.
        /// </summary>
        internal Text(DocumentObject parent, TextRenderOption textRenderOption = TextRenderOption.Default) : base(parent, textRenderOption)
        {
            BaseValues = new TextValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Text class with a string as paragraph content.
        /// </summary>
        public Text(string content, TextRenderOption textRenderOption = TextRenderOption.Default) : this(textRenderOption)
        {
            Content = content;
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Text Clone() 
            => (Text)DeepCopy();

        /// <summary>
        /// Gets or sets the text content.
        /// </summary>
        public string Content
        {
            get => Values.Content ?? "";
            set => Values.Content = value;
        }

        /// <summary>
        /// Converts Text into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            var text = DdlEncoder.StringToText(Values.Content);
            // To make DDL more readable write soft hyphens as keywords.
            text = text?.Replace(new string((char)173, 1), "\\-") ?? NRT.ThrowOnNull<string>(); // BUG_OLD New throw on null.
            serializer.Write(text);
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Text));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public TextValues Values => (TextValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class TextValues : Values
        {
            internal TextValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Content { get; set; }
        }
    }
}
