// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// Represents the collection of values on the X-Axis.
    /// </summary>
    public class XValues : DocumentObjectCollection
    {
        /// <summary>
        /// Initializes a new instance of the XValues class.
        /// </summary>
        public XValues()
        {
            BaseValues = new XValuesValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the XValues class with the specified parent.
        /// </summary>
        internal XValues(DocumentObject parent) : base(parent)
        {
            BaseValues = new XValuesValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new XValues Clone() 
            => (XValues)DeepCopy();

        /// <summary>
        /// Gets an XSeries by its index.
        /// </summary>
        public new XSeries? this[int index] => base[index] as XSeries;

        /// <summary>
        /// Adds a new XSeries to the collection.
        /// </summary>
        public XSeries AddXSeries()
        {
            var xSeries = new XSeries();
            Add(xSeries);
            return xSeries;
        }

        /// <summary>
        /// Converts XValues into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            int count = Count;
            for (int index = 0; index < count; index++)
            {
                var xSeries = this[index] as XSeries;
                xSeries?.Serialize(serializer);
            }
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(XValues));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public XValuesValues Values => (XValuesValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class XValuesValues : Values
        {
            internal XValuesValues(DocumentObject owner) : base(owner)
            { }
        }
    }
}
