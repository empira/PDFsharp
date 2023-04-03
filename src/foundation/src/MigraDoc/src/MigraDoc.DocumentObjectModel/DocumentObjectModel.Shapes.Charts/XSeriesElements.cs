// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// Represents the collection of the value in an XSeries.
    /// </summary>
    public class XSeriesElements : DocumentObjectCollection
    {
        /// <summary>
        /// Initializes a new instance of the XSeriesElements class.
        /// </summary>
        public XSeriesElements()
        {
            BaseValues = new XSeriesElementsValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new XSeriesElements Clone() 
            => (XSeriesElements)base.DeepCopy();

        /// <summary>
        /// Adds a blank to the XSeries.
        /// </summary>
        public void AddBlank() 
            => base.Add(null);

        /// <summary>
        /// Adds a value to the XSeries.
        /// </summary>
        public XValue Add(string value)
        {
            var xValue = new XValue(value);
            Add(xValue);
            return xValue;
        }

        /// <summary>
        /// Adds an array of values to the XSeries.
        /// </summary>
        public void Add(params string[] values)
        {
            foreach (string val in values)
                Add(val);
        }

        /// <summary>
        /// Converts XSeriesElements into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            int count = Count;
            for (int index = 0; index < count; index++)
            {
                if (this[index] is not XValue xValue)
                    serializer.Write("null, ");
                else
                    xValue.Serialize(serializer);
            }
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(XSeriesElements));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public XSeriesElementsValues Values => (XSeriesElementsValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class XSeriesElementsValues : Values
        {
            internal XSeriesElementsValues(DocumentObject owner) : base(owner)
            { }
        }
    }
}
