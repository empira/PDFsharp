// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.ComponentModel;

namespace PdfSharp.Charting
{
    /// <summary>
    /// Represents a legend of a chart.
    /// </summary>
    public class Legend : ChartObject
    {
        /// <summary>
        /// Initializes a new instance of the Legend class.
        /// </summary>
        public Legend()
        { }

        /// <summary>
        /// Initializes a new instance of the Legend class with the specified parent.
        /// </summary>
        internal Legend(DocumentObject parent) : base(parent) { }

        #region Methods
        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Legend Clone() 
            => (Legend)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var legend = (Legend)base.DeepCopy();
            if (legend._lineFormat != null)
            {
                legend._lineFormat = legend._lineFormat.Clone();
                legend._lineFormat.Parent = legend;
            }
            if (legend._font != null)
            {
                legend._font = legend._font.Clone();
                legend._font.Parent = legend;
            }
            return legend;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the line format of the legend’s border.
        /// </summary>
        public LineFormat LineFormat => _lineFormat ??= new LineFormat(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal LineFormat? _lineFormat;

        /// <summary>
        /// Gets the font of the legend.
        /// </summary>
        public Font Font => _font ??= new Font(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal Font? _font;

        /// <summary>
        /// Gets or sets the docking type.
        /// </summary>
        public DockingType Docking
        {
            get => _docking;
            set
            {
                if (!Enum.IsDefined(typeof(DockingType), value))
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(DockingType));
                _docking = value;
            }
        }
        // ReSharper disable once InconsistentNaming because this is old code
        internal DockingType _docking;
        #endregion
    }
}
