// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Fields
{
    /// <summary>
    /// SectionPagesField is used to reference the number of all pages of the current section.
    /// </summary>
    public class SectionPagesField : NumericFieldBase
    {
        /// <summary>
        /// Initializes a new instance of the SectionPagesField class.
        /// </summary>
        public SectionPagesField()
        {
            BaseValues = new SectionPagesFieldValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the SectionPagesField class with the specified parent.
        /// </summary>
        internal SectionPagesField(DocumentObject parent) : base(parent)
        {
            BaseValues = new SectionPagesFieldValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new SectionPagesField Clone() 
            => (SectionPagesField)DeepCopy();

        /// <summary>
        /// Converts SectionPagesField into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            var str = "\\field(SectionPages)";

            if (!String.IsNullOrEmpty(Values.Format))
                str += "[Format = \"" + Format + "\"]";
            else
                str += "[]"; // Has to be appended to avoid confusion with '[' in directly following text.

            serializer.Write(str);
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(SectionPagesField));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new SectionPagesFieldValues Values => (SectionPagesFieldValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class SectionPagesFieldValues : NumericFieldBaseValues
        {
            internal SectionPagesFieldValues(DocumentObject owner) : base(owner)
            { }
        }
    }
}
