// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting
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
        { }

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
    }
}
