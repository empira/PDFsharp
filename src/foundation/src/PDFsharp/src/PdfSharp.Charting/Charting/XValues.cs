// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting
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
        { }

        /// <summary>
        /// Initializes a new instance of the XValues class with the specified parent.
        /// </summary>
        internal XValues(DocumentObject parent) : base(parent) 
        { }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new XValues Clone() => (XValues)DeepCopy();

        /// <summary>
        /// Gets an XSeries by its index.
        /// </summary>
        public new XSeries this[int index] => base[index] as XSeries ?? throw new InvalidOperationException("Element is not an XSeries.");

        /// <summary>
        /// Adds a new XSeries to the collection.
        /// </summary>
        public XSeries AddXSeries()
        {
            var xSeries = new XSeries();
            Add(xSeries);
            return xSeries;
        }
    }
}
