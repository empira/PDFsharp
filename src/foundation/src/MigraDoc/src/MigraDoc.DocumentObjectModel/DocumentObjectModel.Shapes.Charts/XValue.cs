// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// Represents the actual value on the XSeries.
    /// </summary>
    public class XValue : ChartObject
    {
        /// <summary>
        /// Initializes a new instance of the XValue class.
        /// </summary>
        public XValue()
        {
            BaseValues = new XValueValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the XValue class with the specified value.
        /// </summary>
        public XValue(string? value)
            : this()
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The actual value of the XValue.
        /// </summary>
        protected internal string? Value // TODO Previous implementation did not have a property?
        {
            get => Values.Value;
            set => Values.Value = value;
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new XValue Clone() 
            => (XValue)DeepCopy();

        /// <summary>
        /// Converts XValue into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.Write("\"" + Value + "\", ");
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(XValue));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new XValueValues Values => (XValueValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class XValueValues : ChartObjectValues
        {
            internal XValueValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Value { get; set; }
        }
    }
}
