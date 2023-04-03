// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting
{
    /// <summary>
    /// The collection of data series.
    /// </summary>
    public class SeriesCollection : DocumentObjectCollection
    {
        /// <summary>
        /// Initializes a new instance of the SeriesCollection class.
        /// </summary>
        internal SeriesCollection()
        { }

        /// <summary>
        /// Initializes a new instance of the SeriesCollection class with the specified parent.
        /// </summary>
        internal SeriesCollection(DocumentObject parent) : base(parent) 
        { }

        /// <summary>
        /// Gets a series by its index.
        /// </summary>
        public new Series this[int index] => base[index] as Series ?? throw new InvalidOperationException("Element is not a Series.");

        #region Methods
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
            var series = new Series
            {
                // Initialize chart type for each new series.
                _chartType = ((Chart?)Parent)?._type ?? throw new InvalidOperationException("SeriesCollection has no parent")
            };
            Add(series);
            return series;
        }
        #endregion
    }
}
