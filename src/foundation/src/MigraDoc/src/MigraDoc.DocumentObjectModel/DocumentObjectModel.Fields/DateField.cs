// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Fields
{
    /// <summary>
    /// DateField is used to reference the date and time the printing starts.
    /// </summary>
    public class DateField : TextBasedDocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the DateField class.
        /// </summary>
        public DateField(TextRenderOption textRenderOption = TextRenderOption.Default) : base(textRenderOption)
        {
            BaseValues = new DateFieldValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the DateField class with the specified parent.
        /// </summary>
        internal DateField(DocumentObject parent, TextRenderOption textRenderOption = TextRenderOption.Default) : base(parent, textRenderOption)
        {
            BaseValues = new DateFieldValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new DateField Clone() 
            => (DateField)DeepCopy();

        /// <summary>
        /// Gets or sets the format of the date.
        /// </summary>
        public string Format
        {
            get => Values.Format ?? "";
            set => Values.Format = value;
        }

        /// <summary>
        /// Converts DateField into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            var str = "\\field(Date)";
            if (!String.IsNullOrEmpty(Values.Format))
                str += "[Format = \"" + Format + "\"]";
            else
                str += "[]"; //Has to be appended to avoid confusion with '[' in immediately following text.

            serializer.Write(str);
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

        static readonly Meta TheMeta = new(typeof(DateField));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public DateFieldValues Values => (DateFieldValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class DateFieldValues : Values
        {
            internal DateFieldValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Format { get; set; }
        }
    }
}
