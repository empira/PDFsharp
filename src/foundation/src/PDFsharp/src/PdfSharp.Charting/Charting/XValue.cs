// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace PdfSharp.Charting
{
    /// <summary>
    /// Represents the actual value on the XSeries.
    /// </summary>
    public class XValue : ChartObject
    {
        ///// <summary>
        ///// Initializes a new instance of the XValue class.
        ///// </summary>
        //XValue()
        //{ }

        /// <summary>
        /// Initializes a new instance of the XValue class with the specified value.
        /// </summary>
        public XValue(string value)
            //: this()
        {
            ValueField = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new XValue Clone()
            => (XValue)DeepCopy();

        /// <summary>
        /// The actual value of the XValue.
        /// </summary>
        internal string ValueField;
    }
}
