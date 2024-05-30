// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Fields
{
    /// <summary>
    /// InfoField is used to reference one of the DocumentInfo fields in the document.
    /// </summary>
    public class InfoField : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the InfoField class.
        /// </summary>
        public InfoField()
        {
            BaseValues = new InfoFieldValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the InfoField class with the specified parent.
        /// </summary>
        internal InfoField(DocumentObject parent) : base(parent)
        {
            BaseValues = new InfoFieldValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new InfoField Clone()
            => (InfoField)DeepCopy();

        /// <summary>
        /// Gets or sets the name of the information to be shown in the field.
        /// </summary>
        public string Name
        {
            get => Values.Name ?? "";
            set
            {
                if (IsValidName(value))
                    Values.Name = value;
                else
                    throw new ArgumentException(DomSR.InvalidInfoFieldName(value));
            }
        }

        /// <summary>
        /// Determines whether the name is a valid InfoFieldType.
        /// </summary>
        bool IsValidName(string name)
        {
            // Check using a way that never throws an exception
            foreach (string validName in validNames)
            {
                if (String.Compare(validName, name, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }
            return false;
        }

        static string[] validNames = Enum.GetNames(typeof(InfoFieldType));

        /// <summary>
        /// Determines whether this instance is null (not set).
        /// </summary>
        public override bool IsNull()
        {
            return false;
        }
        /// <summary>
        /// Converts InfoField into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            if (Name == "")
                throw new InvalidOperationException(DomSR.MissingObligatoryProperty(nameof(Name), nameof(InfoField)));

            string str = "\\field(Info)";

            str += "[Name = \"" + Name + "\"]";

            serializer.Write(str);
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(InfoField));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public InfoFieldValues Values => (InfoFieldValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class InfoFieldValues : Values
        {
            internal InfoFieldValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Name { get; set; }
        }
    }
}
