// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// The collection of data series.
    /// </summary>
    public class SeriesCollection : DocumentObjectCollection
    {
        /// <summary>
        /// Initializes a new instance of the SeriesCollection class.
        /// </summary>
        public SeriesCollection()
        {
            BaseValues = new SeriesCollectionValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the SeriesCollection class with the specified parent.
        /// </summary>
        internal SeriesCollection(DocumentObject parent) : base(parent)
        {
            BaseValues = new SeriesCollectionValues(this);
        }

        /// <summary>
        /// Gets a series by its index.
        /// </summary>
        public new Series? this[int index] => base[index] as Series;

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new SeriesCollection Clone() 
            => (SeriesCollection)DeepCopy();

        /// <summary>
        /// Adds a new series to the collection.
        /// </summary>
        public Series AddSeries()
        {
            Series series = new Series();
            Add(series);
            return series;
        }

        /// <summary>
        /// Converts SeriesCollection into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            int count = Count;
            for (int index = 0; index < count; ++index)
            {
                var series = this[index];
                series?.Serialize(serializer);
            }
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(SeriesCollection));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public SeriesCollectionValues Values => (SeriesCollectionValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class SeriesCollectionValues : Values
        {
            internal SeriesCollectionValues(DocumentObject owner) : base(owner)
            { }
        }
    }
}
