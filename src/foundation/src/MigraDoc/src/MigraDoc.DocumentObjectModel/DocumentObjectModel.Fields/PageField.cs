// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Fields
{
    /// <summary>
    /// PageField is used to reference the number of the current page.
    /// </summary>
    public class PageField : NumericFieldBase
    {
        /// <summary>
        /// Initializes a new instance of the PageField class.
        /// </summary>
        public PageField(TextRenderOption textRenderOption = TextRenderOption.Default) : base(textRenderOption)
        {
            BaseValues = new PageFieldValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the PageField class with the specified parent.
        /// </summary>
        internal PageField(DocumentObject parent, TextRenderOption textRenderOption = TextRenderOption.Default) : base(parent, textRenderOption)
        {
            BaseValues = new PageFieldValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new PageField Clone() 
            => (PageField)DeepCopy();

        /// <summary>
        /// Converts PageField into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            var str = "\\field(Page)";

            if (!String.IsNullOrEmpty(Values.Format))
                str += "[Format = \"" + Format + "\"]";
            else
                str += "[]"; //Has to be appended to avoid confusion with '[' in immediately following text.

            serializer.Write(str);
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(PageField));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new PageFieldValues Values => (PageFieldValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class PageFieldValues : NumericFieldBaseValues
        {
            internal PageFieldValues(DocumentObject owner) : base(owner)
            { }
        }
    }
}
