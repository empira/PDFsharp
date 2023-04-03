// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting
{
    /// <summary>
    /// Represents the area where the actual chart is drawn.
    /// </summary>
    public class PlotArea : ChartObject
    {
        /// <summary>
        /// Initializes a new instance of the PlotArea class.
        /// </summary>
        internal PlotArea()
        { }

        /// <summary>
        /// Initializes a new instance of the PlotArea class with the specified parent.
        /// </summary>
        internal PlotArea(DocumentObject parent) : base(parent) { }

        #region Methods
        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new PlotArea Clone() => (PlotArea)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var plotArea = (PlotArea)base.DeepCopy();
            if (plotArea._lineFormat != null)
            {
                plotArea._lineFormat = plotArea._lineFormat.Clone();
                plotArea._lineFormat.Parent = plotArea;
            }
            if (plotArea._fillFormat != null)
            {
                plotArea._fillFormat = plotArea._fillFormat.Clone();
                plotArea._fillFormat.Parent = plotArea;
            }
            return plotArea;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the line format of the plot area's border.
        /// </summary>
        public LineFormat LineFormat => _lineFormat ??= new LineFormat(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal LineFormat? _lineFormat;

        /// <summary>
        /// Gets the background filling of the plot area.
        /// </summary>
        public FillFormat FillFormat => _fillFormat ??= new FillFormat(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal FillFormat? _fillFormat;

        /// <summary>
        /// Gets or sets the left padding of the area.
        /// </summary>
        public XUnit LeftPadding { get; set; }

        /// <summary>
        /// Gets or sets the right padding of the area.
        /// </summary>
        public XUnit RightPadding { get; set; }

        /// <summary>
        /// Gets or sets the top padding of the area.
        /// </summary>
        public XUnit TopPadding { get; set; }

        /// <summary>
        /// Gets or sets the bottom padding of the area.
        /// </summary>
        public XUnit BottomPadding { get; set; }
        #endregion
    }
}
