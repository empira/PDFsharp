// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Fields
{
    /// <summary>
    /// SectionField is used to reference the number of the current section.
    /// </summary>
    public class SectionField : NumericFieldBase
    {
        /// <summary>
        /// Initializes a new instance of the SectionField class.
        /// </summary>
        public SectionField()
        {
            BaseValues = new SectionFieldValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the SectionField class with the specified parent.
        /// </summary>
        internal SectionField(DocumentObject parent) : base(parent)
        {
            BaseValues = new SectionFieldValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new SectionField Clone() 
            => (SectionField)DeepCopy();

        /// <summary>
        /// Converts SectionField into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            var str = "\\field(Section)";

            if (!String.IsNullOrEmpty(Values.Format))
                str += "[Format = \"" + Format + "\"]";
            else
                str += "[]"; //Has to be appended to avoid confusion with '[' in directly following text.

            serializer.Write(str);
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(SectionField));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new SectionFieldValues Values => (SectionFieldValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class SectionFieldValues : NumericFieldBaseValues
        {
            internal SectionFieldValues(DocumentObject owner) : base(owner)
            { }
        }
    }
}
