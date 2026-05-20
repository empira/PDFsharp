// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Fields
{
    /// <summary>
    /// NumPagesField is used to reference the number of all pages in the document.
    /// </summary>
    public class NumPagesField : NumericFieldBase
    {
        /// <summary>
        /// Initializes a new instance of the NumPagesField class.
        /// </summary>
        public NumPagesField(TextRenderOption textRenderOption = TextRenderOption.Default) : base(textRenderOption)
        {
            BaseValues = new NumPagesFieldValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the NumPagesField class with the specified parent.
        /// </summary>
        internal NumPagesField(DocumentObject parent, TextRenderOption textRenderOption = TextRenderOption.Default) : base(parent, textRenderOption)
        {
            BaseValues = new NumPagesFieldValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new NumPagesField Clone() 
            => (NumPagesField)DeepCopy();

        /// <summary>
        /// Converts NumPagesField into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            var str = "\\field(NumPages)";

            if (!String.IsNullOrEmpty(Values.Format))
                str += "[Format = \"" + Format + "\"]";
            else
                str += "[]"; // Has to be appended to avoid confusion with '[' in directly following text.

            serializer.Write(str);
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(NumPagesField));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new NumPagesFieldValues Values => (NumPagesFieldValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class NumPagesFieldValues : NumericFieldBaseValues
        {
            internal NumPagesFieldValues(DocumentObject owner) : base(owner)
            { }
        }
    }
}
