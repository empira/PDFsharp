// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting
{
    /// <summary>
    /// Represents the collection of the values in a data series.
    /// </summary>
    public class SeriesElements : DocumentObjectCollection
    {
        /// <summary>
        /// Initializes a new instance of the SeriesElements class.
        /// </summary>
        internal SeriesElements()
        { }

        /// <summary>
        /// Initializes a new instance of the SeriesElements class with the specified parent.
        /// </summary>
        internal SeriesElements(DocumentObject parent) : base(parent) 
        { }

        /// <summary>
        /// Gets a point by its index.
        /// </summary>
        public new Point this[int index] => base[index] as Point ?? throw new InvalidOperationException("Element is not a Point.");

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new SeriesElements Clone() => (SeriesElements)DeepCopy();

        /// <summary>
        /// Adds a blank to the series.
        /// </summary>
        public void AddBlank() => base.Add(null);

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
    }
}
