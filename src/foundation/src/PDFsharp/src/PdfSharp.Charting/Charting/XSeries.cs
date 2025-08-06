// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections;

namespace PdfSharp.Charting
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
        { }

        /// <summary>
        /// Gets the xvalue at the specified index.
        /// </summary>
        public XValue? this[int index]
            => _xSeriesElements[index] as XValue;

        /// <summary>
        /// The actual value container of the XSeries.
        /// </summary>
        XSeriesElements _xSeriesElements = new XSeriesElements();

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
            var xSeries = (XSeries)base.DeepCopy();
            xSeries._xSeriesElements = xSeries._xSeriesElements.Clone();
            xSeries._xSeriesElements.Parent = xSeries;
            return xSeries;
        }

        /// <summary>
        /// Adds a blank to the XSeries.
        /// </summary>
        public void AddBlank() 
            => _xSeriesElements.AddBlank();

        /// <summary>
        /// Adds a value to the XSeries.
        /// </summary>
        public XValue Add(string value)
            => _xSeriesElements.Add(value);

        /// <summary>
        /// Adds an array of values to the XSeries.
        /// </summary>
        public void Add(params string[] values) 
            => _xSeriesElements.Add(values);

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        public IEnumerator GetEnumerator()
            => _xSeriesElements.GetEnumerator();

        /// <summary>
        /// Gets the number of xvalues contained in the xseries.
        /// </summary>
        public int Count => _xSeriesElements.Count;
    }
}
