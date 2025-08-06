// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.ComponentModel;

namespace PdfSharp.Charting
{
    /// <summary>
    /// This class represents an axis in a chart.
    /// </summary>
    public class Axis : ChartObject
    {
        /// <summary>
        /// Initializes a new instance of the Axis class with the specified parent.
        /// </summary>
        internal Axis(DocumentObject parent) : base(parent) { }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Axis Clone() => (Axis)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            Axis axis = (Axis)base.DeepCopy();
            if (axis._title != null)
            {
                axis._title = axis._title.Clone();
                axis._title.Parent = axis;
            }
            if (axis._tickLabels != null)
            {
                axis._tickLabels = axis._tickLabels.Clone();
                axis._tickLabels.Parent = axis;
            }
            if (axis._lineFormat != null)
            {
                axis._lineFormat = axis._lineFormat.Clone();
                axis._lineFormat.Parent = axis;
            }
            if (axis._majorGridlines != null)
            {
                axis._majorGridlines = axis._majorGridlines.Clone();
                axis._majorGridlines.Parent = axis;
            }
            if (axis._minorGridlines != null)
            {
                axis._minorGridlines = axis._minorGridlines.Clone();
                axis._minorGridlines.Parent = axis;
            }
            return axis;
        }

        /// <summary>
        /// Gets the title of the axis.
        /// </summary>
        public AxisTitle Title => _title ??= new AxisTitle(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal AxisTitle? _title;

        /// <summary>
        /// Gets or sets the minimum value of the axis.
        /// </summary>
        public double MinimumScale { get; set; } = Double.NaN;

        /// <summary>
        /// Gets or sets the maximum value of the axis.
        /// </summary>
        public double MaximumScale { get; set; } = Double.NaN;

        /// <summary>
        /// Gets or sets the interval of the primary tick.
        /// </summary>
        public double MajorTick { get; set; } = Double.NaN;

        /// <summary>
        /// Gets or sets the interval of the secondary tick.
        /// </summary>
        public double MinorTick { get; set; } = Double.NaN;

        /// <summary>
        /// Gets or sets the type of the primary tick mark.
        /// </summary>
        public TickMarkType MajorTickMark
        {
            get => _majorTickMark;
            set
            {
                if (!Enum.IsDefined(typeof(TickMarkType), value))
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(TickMarkType));
                _majorTickMark = value;
                _majorTickMarkInitialized = true;
            }
        }
        // ReSharper disable once InconsistentNaming because this is old code
        internal TickMarkType _majorTickMark;
        // ReSharper disable once InconsistentNaming because this is old code
        internal bool _majorTickMarkInitialized;

        /// <summary>
        /// Gets or sets the type of the secondary tick mark.
        /// </summary>
        public TickMarkType MinorTickMark
        {
            get => _minorTickMark;
            set
            {
                if (!Enum.IsDefined(typeof(TickMarkType), value))
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(TickMarkType));
                _minorTickMark = value;
                _minorTickMarkInitialized = true;
            }
        }
        // ReSharper disable once InconsistentNaming because this is old code
        internal TickMarkType _minorTickMark;
        // ReSharper disable once InconsistentNaming because this is old code
        internal bool _minorTickMarkInitialized;

        /// <summary>
        /// Gets the label of the primary tick.
        /// </summary>
        public TickLabels TickLabels => _tickLabels ??= new TickLabels(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal TickLabels? _tickLabels;

        /// <summary>
        /// Gets the format of the axis line.
        /// </summary>
        public LineFormat LineFormat => _lineFormat ??= new LineFormat(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal LineFormat? _lineFormat;

        /// <summary>
        /// Gets the primary gridline object.
        /// </summary>
        public Gridlines MajorGridlines => _majorGridlines ??= new Gridlines(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal Gridlines? _majorGridlines;

        /// <summary>
        /// Gets the secondary gridline object.
        /// </summary>
        public Gridlines MinorGridlines => _minorGridlines ??= new Gridlines(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal Gridlines? _minorGridlines;

        /// <summary>
        /// Gets or sets, whether the axis has a primary gridline object.
        /// </summary>
        public bool HasMajorGridlines { get; set; }

        /// <summary>
        /// Gets or sets, whether the axis has a secondary gridline object.
        /// </summary>
        public bool HasMinorGridlines { get; set; }
    }
}
