// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting
{
    /// <summary>
    /// Defines the background filling of the shape.
    /// </summary>
    public class FillFormat : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the FillFormat class.
        /// </summary>
        public FillFormat()
        { }

        /// <summary>
        /// Initializes a new instance of the FillFormat class with the specified parent.
        /// </summary>
        internal FillFormat(DocumentObject parent) : base(parent)
        { }

        #region Methods
        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new FillFormat Clone() 
            => (FillFormat)DeepCopy();

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the color of the filling.
        /// </summary>
        public XColor Color { get; set; } = XColor.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the background color should be visible.
        /// </summary>
        public bool Visible { get; set; }

        #endregion
    }
}
