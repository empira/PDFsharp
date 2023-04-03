// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting
{
    /// <summary>
    /// Base class for all chart classes.
    /// </summary>
    public class ChartObject : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the ChartObject class.
        /// </summary>
        public ChartObject()
        { }

        /// <summary>
        /// Initializes a new instance of the ChartObject class with the specified parent.
        /// </summary>
        internal ChartObject(DocumentObject parent) : base(parent) { }
    }
}
