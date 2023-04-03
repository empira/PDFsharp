// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting
{
    /// <summary>
    /// Represents the title of an axis.
    /// </summary>
    public class AxisTitle : ChartObject
    {
        /// <summary>
        /// Initializes a new instance of the AxisTitle class.
        /// </summary>
        public AxisTitle()
        { }

        /// <summary>
        /// Initializes a new instance of the AxisTitle class with the specified parent.
        /// </summary>
        internal AxisTitle(DocumentObject parent) : base(parent) { }

        #region Methods
        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new AxisTitle Clone()
            => (AxisTitle)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var axisTitle = (AxisTitle)base.DeepCopy();
            if (axisTitle._font != null)
            {
                axisTitle._font = axisTitle._font.Clone();
                axisTitle._font.Parent = axisTitle;
            }
            return axisTitle;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the caption of the title.
        /// </summary>
        public string Caption { get; set; } = "";

        // ReSharper disable once InconsistentNaming

        /// <summary>
        /// Gets the font of the title.
        /// </summary>
        public Font Font => _font ??= new Font(this);
        // ReSharper disable once InconsistentNaming
        internal Font? _font;

        /// <summary>
        /// Gets or sets the orientation of the caption.
        /// </summary>
        public double Orientation { get; set; }

        /// <summary>
        /// Gets or sets the horizontal alignment of the caption.
        /// </summary>
        public HorizontalAlignment Alignment { get; set; }

        /// <summary>
        /// Gets or sets the vertical alignment of the caption.
        /// </summary>
        public VerticalAlignment VerticalAlignment { get; set; }

        #endregion
    }
}
