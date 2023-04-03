// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting
{
    /// <summary>
    /// Represents the gridlines on the axes.
    /// </summary>
    public class Gridlines : ChartObject
    {
        /// <summary>
        /// Initializes a new instance of the Gridlines class.
        /// </summary>
        public Gridlines()
        { }

        /// <summary>
        /// Initializes a new instance of the Gridlines class with the specified parent.
        /// </summary>
        internal Gridlines(DocumentObject parent) : base(parent)
        { }

        #region Methods
        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Gridlines Clone() 
            => (Gridlines)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var gridlines = (Gridlines)base.DeepCopy();
            if (gridlines._lineFormat != null)
            {
                gridlines._lineFormat = gridlines._lineFormat.Clone();
                gridlines._lineFormat.Parent = gridlines;
            }
            return gridlines;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the line format of the grid.
        /// </summary>
        public LineFormat LineFormat => _lineFormat ??= new LineFormat(this);

        // ReSharper disable once InconsistentNaming because this is old code
        internal LineFormat? _lineFormat;
        #endregion
    }
}
