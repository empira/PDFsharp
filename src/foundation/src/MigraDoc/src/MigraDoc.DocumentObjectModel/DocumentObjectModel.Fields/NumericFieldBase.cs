// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using static MigraDoc.DocumentObjectModel.Shapes.Charts.ChartObject;
using static MigraDoc.DocumentObjectModel.Shapes.Shape;

namespace MigraDoc.DocumentObjectModel.Fields
{
    /// <summary>
    /// NumericFieldBase serves as a base for Numeric fields, which are: 
    /// NumPagesField, PageField, PageRefField, SectionField, SectionPagesField
    /// </summary>
    public abstract class NumericFieldBase : TextBasedDocumentObject
    {
        /// <summary>
        /// The valid format strings for the supported numeric types.
        /// </summary>
        protected static readonly string[] ValidFormatStrings =
        {
            "",
            "ROMAN",
            "roman",
            "ALPHABETIC",
            "alphabetic"
        };

        /// <summary>
        /// Initializes a new instance of the NumericFieldBase class.
        /// </summary>
        internal NumericFieldBase(TextRenderOption textRenderOption = TextRenderOption.Default) : base(textRenderOption)
        { }

        /// <summary>
        /// Initializes a new instance of the NumericFieldBase class with the specified parent.
        /// </summary>
        internal NumericFieldBase(DocumentObject parent, TextRenderOption textRenderOption = TextRenderOption.Default) : base(parent, textRenderOption)
        { }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new NumericFieldBase Clone()
            => (NumericFieldBase)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            NumericFieldBase numericFieldBase = (NumericFieldBase)base.DeepCopy();
            return numericFieldBase;
        }

        /// <summary>
        /// Gets or sets the format of the number.
        /// </summary>
        public string Format
        {
            get => Values.Format ?? "";
            set
            {
                if (IsValidFormat(value))
                    Values.Format = value;
                else
                    throw new ArgumentException(MdDomMsgs.InvalidFieldFormat(value).Message);
            }
        }

        /// <summary>
        /// Determines whether the format is valid for numeric fields.
        /// </summary>
        protected bool IsValidFormat(string format)
        {
            foreach (string name in ValidFormatStrings)
            {
                if (name == Format)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether this instance is null (not set).
        /// </summary>
        public override bool IsNull() => false;

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public NumericFieldBaseValues Values => (NumericFieldBaseValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class NumericFieldBaseValues : Values
        {
            internal NumericFieldBaseValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Format { get; set; }
        }
    }
}
