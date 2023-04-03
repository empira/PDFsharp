// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// Represents a series of data on the X-Axis.
    /// </summary>
    public class XSeries : ChartObject
    {
        /// <summary>
        /// Initializes a new instance of the XSeries class.
        /// </summary>
        public XSeries()
        {
            BaseValues = new XSeriesValues(this);
            XSeriesElements = new XSeriesElements();
        }

        /// <summary>
        /// The actual value container of the XSeries.
        /// </summary>
        protected internal XSeriesElements XSeriesElements // TODO Previous implementation did not have a property?
        {
            get => Values.XSeriesElements ??= new();
            set => Values.XSeriesElements = value;
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new XSeries Clone() 
            => (XSeries)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            XSeries xSeries = (XSeries)base.DeepCopy();
            //if (xSeries.XSeriesElements != null)
            //{
            xSeries.XSeriesElements = xSeries.XSeriesElements.Clone();
            xSeries.XSeriesElements.Parent = xSeries;
            //}
            return xSeries;
        }

        /// <summary>
        /// Adds a blank to the XSeries.
        /// </summary>
        public void AddBlank()
        {
            XSeriesElements.AddBlank();
        }

        /// <summary>
        /// Adds a value to the XSeries.
        /// </summary>
        public XValue Add(string value)
        {
            return XSeriesElements.Add(value);
        }

        /// <summary>
        /// Adds an array of values to the XSeries.
        /// </summary>
        public void Add(params string[] values)
        {
            XSeriesElements.Add(values);
        }

        /// <summary>
        /// Converts XSeries into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteLine("\\xvalues");

            serializer.BeginContent();
            XSeriesElements.Serialize(serializer);
            serializer.WriteLine("");
            serializer.EndContent();
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(XSeries));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new XSeriesValues Values => (XSeriesValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class XSeriesValues : ChartObjectValues
        {
            internal XSeriesValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public XSeriesElements? XSeriesElements { get; set; }
        }
    }
}
