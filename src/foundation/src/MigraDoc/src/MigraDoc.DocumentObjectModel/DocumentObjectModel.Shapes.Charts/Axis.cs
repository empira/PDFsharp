// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// This class represents an axis in a chart.
    /// </summary>
    public class Axis : ChartObject
    {
        /// <summary>
        /// Initializes a new instance of the Axis class.
        /// </summary>
        public Axis()
        {
            BaseValues = new AxisValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Axis class with the specified parent.
        /// </summary>
        internal Axis(DocumentObject parent) : base(parent)
        {
            BaseValues = new AxisValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Axis Clone()
            => (Axis)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var axis = (Axis)base.DeepCopy();
            if (axis.Values.Title is not null)
            {
                axis.Values.Title = axis.Values.Title.Clone();
                axis.Values.Title.Parent = axis;
            }
            if (axis.Values.TickLabels is not null)
            {
                axis.Values.TickLabels = axis.Values.TickLabels.Clone();
                axis.Values.TickLabels.Parent = axis;
            }
            if (axis.Values.LineFormat is not null)
            {
                axis.Values.LineFormat = axis.Values.LineFormat.Clone();
                axis.Values.LineFormat.Parent = axis;
            }
            if (axis.Values.MajorGridlines is not null)
            {
                axis.Values.MajorGridlines = axis.Values.MajorGridlines.Clone();
                axis.Values.MajorGridlines.Parent = axis;
            }
            if (axis.Values.MinorGridlines != null)
            {
                axis.Values.MinorGridlines = axis.Values.MinorGridlines.Clone();
                axis.Values.MinorGridlines.Parent = axis;
            }
            return axis;
        }

        /// <summary>
        /// Gets the title of the axis.
        /// </summary>
        public AxisTitle Title
        {
            get => Values.Title ??= new AxisTitle(this);
            set
            {
                SetParent(value);
                Values.Title = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum value of the axis.
        /// </summary>
        public double MinimumScale
        {
            get => Values.MinimumScale ?? double.NaN;
            set => Values.MinimumScale = value;
        }

        /// <summary>
        /// Gets or sets the maximum value of the axis.
        /// </summary>
        public double MaximumScale
        {
            get => Values.MaximumScale ?? double.NaN;
            set => Values.MaximumScale = value;
        }

        /// <summary>
        /// Gets or sets the interval of the primary tick.
        /// </summary>
        public double MajorTick
        {
            get => Values.MajorTick ?? double.NaN;
            set => Values.MajorTick = value;
        }

        /// <summary>
        /// Gets or sets the interval of the secondary tick.
        /// </summary>
        public double MinorTick
        {
            get => Values.MinorTick ?? double.NaN;
            set => Values.MinorTick = value;
        }

        /// <summary>
        /// Gets or sets the type of the primary tick mark.
        /// </summary>
        public TickMarkType MajorTickMark
        {
            get => Values.MajorTickMark ?? TickMarkType.None;
            set => Values.MajorTickMark = value;
        }

        /// <summary>
        /// Gets or sets the type of the secondary tick mark.
        /// </summary>
        public TickMarkType MinorTickMark
        {
            get => Values.MinorTickMark ?? TickMarkType.None;
            set => Values.MinorTickMark = value;
        }

        /// <summary>
        /// Gets the label of the primary tick.
        /// </summary>
        public TickLabels TickLabels
        {
            get => Values.TickLabels ??= new TickLabels(this);
            set
            {
                SetParent(value);
                Values.TickLabels = value;
            }
        }

        /// <summary>
        /// Gets the format of the axis line.
        /// </summary>
        public LineFormat LineFormat
        {
            get => Values.LineFormat ??= new LineFormat(this);
            set
            {
                SetParent(value);
                Values.LineFormat = value;
            }
        }

        /// <summary>
        /// Gets the primary gridline object.
        /// </summary>
        public Gridlines MajorGridlines
        {
            get => Values.MajorGridlines ??= new Gridlines(this);
            set
            {
                SetParent(value);
                Values.MajorGridlines = value;
            }
        }

        /// <summary>
        /// Gets the secondary gridline object.
        /// </summary>
        public Gridlines MinorGridlines
        {
            get => Values.MinorGridlines ??= new Gridlines(this);
            set
            {
                SetParent(value);
                Values.MinorGridlines = value;
            }
        }

        /// <summary>
        /// Gets or sets, whether the axis has a primary gridline object.
        /// </summary>
        public bool HasMajorGridlines
        {
            get => Values.HasMajorGridlines ?? false;
            set => Values.HasMajorGridlines = value;
        }

        /// <summary>
        /// Gets or sets, whether the axis has a secondary gridline object.
        /// </summary>
        public bool HasMinorGridlines
        {
            get => Values.HasMinorGridlines ?? false;
            set => Values.HasMinorGridlines = value;
        }

        /// <summary>
        /// Determines whether the specified gridlines object is MajorGridlines or MinorGridlines.
        /// </summary>
        internal string CheckGridlines(Gridlines gridlines)
        {
            if (Values.MajorGridlines is not null && gridlines == Values.MajorGridlines)
                return "MajorGridlines";
            if (Values.MinorGridlines is not null && gridlines == Values.MinorGridlines)
                return "MinorGridlines";
            return "";
        }

        /// <summary>
        /// Converts Axis into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            var chartObject = Parent as Chart;
            Debug.Assert(chartObject != null);

            serializer.WriteLine("\\" + chartObject.CheckAxis(this)); // HACK // BUG: What if Parent is not Chart?
            var pos = serializer.BeginAttributes();

            if (Values.MinimumScale is not null)
                serializer.WriteSimpleAttribute("MinimumScale", MinimumScale);
            if (Values.MaximumScale is not null)
                serializer.WriteSimpleAttribute("MaximumScale", MaximumScale);
            if (Values.MajorTick is not null)
                serializer.WriteSimpleAttribute("MajorTick", MajorTick);
            if (Values.MinorTick is not null)
                serializer.WriteSimpleAttribute("MinorTick", MinorTick);
            if (Values.HasMajorGridlines is not null)
                serializer.WriteSimpleAttribute("HasMajorGridLines", HasMajorGridlines);
            if (Values.HasMinorGridlines is not null)
                serializer.WriteSimpleAttribute("HasMinorGridLines", HasMinorGridlines);
            if (Values.MajorTickMark is not null)
                serializer.WriteSimpleAttribute("MajorTickMark", MajorTickMark);
            if (Values.MinorTickMark is not null)
                serializer.WriteSimpleAttribute("MinorTickMark", MinorTickMark);

            Values.Title?.Serialize(serializer);

            Values.LineFormat?.Serialize(serializer);

            Values.MajorGridlines?.Serialize(serializer);

            Values.MinorGridlines?.Serialize(serializer);

            Values.TickLabels?.Serialize(serializer);

            serializer.EndAttributes(pos);
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Axis));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new AxisValues Values => (AxisValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class AxisValues : ChartObjectValues
        {
            internal AxisValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public AxisTitle? Title { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public double? MinimumScale { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public double? MaximumScale { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public double? MajorTick { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public double? MinorTick { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public TickMarkType? MajorTickMark { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public TickMarkType? MinorTickMark { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public TickLabels? TickLabels { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public LineFormat? LineFormat { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Gridlines? MajorGridlines { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Gridlines? MinorGridlines { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? HasMajorGridlines { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? HasMinorGridlines { get; set; }
        }
    }
}
