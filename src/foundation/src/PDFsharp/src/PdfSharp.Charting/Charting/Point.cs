// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting
{
    /// <summary>
    /// Represents a formatted value on the data series.
    /// </summary>
    public class Point : ChartObject
    {
        /// <summary>
        /// Initializes a new instance of the Point class.
        /// </summary>
        internal Point()
        { }

        /// <summary>
        /// Initializes a new instance of the Point class with a real value.
        /// </summary>
        public Point(double value) : this()
            => Value = value;

        /// <summary>
        /// Initializes a new instance of the Point class with a string value.
        /// </summary>
        public Point(string value) : this()
        {
            // = "34.5 23.9"
            Value = 0; // BUG: Not implemented
            throw new NotImplementedException("Point from string.");
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Point Clone()
            => (Point)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var point = (Point)base.DeepCopy();
            if (point._lineFormat != null)
            {
                point._lineFormat = point._lineFormat.Clone();
                point._lineFormat.Parent = point;
            }
            if (point._fillFormat != null)
            {
                point._fillFormat = point._fillFormat.Clone();
                point._fillFormat.Parent = point;
            }
            return point;
        }

        /// <summary>
        /// Gets the line format of the data point's border.
        /// </summary>
        public LineFormat LineFormat => _lineFormat ??= new LineFormat(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal LineFormat? _lineFormat;

        /// <summary>
        /// Gets the filling format of the data point.
        /// </summary>
        public FillFormat FillFormat => _fillFormat ??= new FillFormat(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal FillFormat? _fillFormat;

        /// <summary>
        /// The actual value of the data point.
        /// </summary>
        public double Value { get; set; }
    }
}
