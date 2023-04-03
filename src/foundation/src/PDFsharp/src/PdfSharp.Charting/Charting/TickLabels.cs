// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting
{
    /// <summary>
    /// Represents the format of the label of each value on the axis.
    /// </summary>
    public class TickLabels : ChartObject
    {
        /// <summary>
        /// Initializes a new instance of the TickLabels class.
        /// </summary>
        public TickLabels()
        { }

        /// <summary>
        /// Initializes a new instance of the TickLabels class with the specified parent.
        /// </summary>
        internal TickLabels(DocumentObject parent) : base(parent)
        { }

        #region Methods
        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new TickLabels Clone() 
            => (TickLabels)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var tickLabels = (TickLabels)base.DeepCopy();
            if (tickLabels._font != null)
            {
                tickLabels._font = tickLabels._font.Clone();
                tickLabels._font.Parent = tickLabels;
            }
            return tickLabels;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the label's number format.
        /// </summary>
        public string Format { get; set; } = "";

        /// <summary>
        /// Gets the font of the label.
        /// </summary>
        public Font Font => _font ??= new Font(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal Font? _font;
        #endregion
    }
}
