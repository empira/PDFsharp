// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting
{
    /// <summary>
    /// Defines the format of a line in a shape object.
    /// </summary>
    public class LineFormat : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the LineFormat class.
        /// </summary>
        public LineFormat()
        { }

        /// <summary>
        /// Initializes a new instance of the LineFormat class with the specified parent.
        /// </summary>
        internal LineFormat(DocumentObject parent) : base(parent) { }

        #region Methods
        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new LineFormat Clone() => (LineFormat)DeepCopy();

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether the line should be visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets or sets the width of the line in XUnit.
        /// </summary>
        public XUnit Width { get; set; }

        /// <summary>
        /// Gets or sets the color of the line.
        /// </summary>
        public XColor Color { get; set; } = XColor.Empty;

        /// <summary>
        /// Gets or sets the dash style of the line.
        /// </summary>
        public XDashStyle DashStyle { get; set; }

        /// <summary>
        /// Gets or sets the style of the line.
        /// </summary>
        public LineStyle Style { get; set; }
        #endregion
    }
}