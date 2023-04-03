// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// Represents the collection of the values in a data series.
    /// </summary>
    public class SeriesElements : DocumentObjectCollection
    {
        /// <summary>
        /// Initializes a new instance of the SeriesElements class.
        /// </summary>
        public SeriesElements()
        {
            BaseValues = new SeriesElementsValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the SeriesElements class with the specified parent.
        /// </summary>
        internal SeriesElements(DocumentObject parent) : base(parent)
        {
            BaseValues = new SeriesElementsValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new SeriesElements Clone()
            => (SeriesElements)DeepCopy();

        /// <summary>
        /// Adds a blank to the series.
        /// </summary>
        public void AddBlank()
            => base.Add(null);

        /// <summary>
        /// Adds a new point with a real value to the series.
        /// </summary>
        public Point Add(double value)
        {
            var point = new Point(value);
            Add(point);
            return point;
        }

        /// <summary>
        /// Adds an array of new points with real values to the series.
        /// </summary>
        public void Add(params double[] values)
        {
            foreach (var val in values)
                Add(val);
        }

        /// <summary>
        /// Converts SeriesElements into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            int count = Count;
            for (int index = 0; index < count; ++index)
            {
                if (this[index] is not Point point)
                    serializer.Write("null, ");
                else
                    point.Serialize(serializer);
            }
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(SeriesElements));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public SeriesElementsValues Values => (SeriesElementsValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class SeriesElementsValues : Values
        {
            internal SeriesElementsValues(DocumentObject owner) : base(owner)
            { }
        }
    }
}
