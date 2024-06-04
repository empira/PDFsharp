// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// Represents the area where the actual chart is drawn.
    /// </summary>
    public class PlotArea : ChartObject
    {
        /// <summary>
        /// Initializes a new instance of the PlotArea class.
        /// </summary>
        public PlotArea()
        {
            BaseValues = new PlotAreaValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the PlotArea class with the specified parent.
        /// </summary>
        internal PlotArea(DocumentObject parent) : base(parent)
        {
            BaseValues = new PlotAreaValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new PlotArea Clone()
            => (PlotArea)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var plotArea = (PlotArea)base.DeepCopy();
            if (plotArea.Values.LineFormat is not null)
            {
                plotArea.Values.LineFormat = plotArea.Values.LineFormat.Clone();
                plotArea.Values.LineFormat.Parent = plotArea;
            }
            if (plotArea.Values.FillFormat is not null)
            {
                plotArea.Values.FillFormat = plotArea.Values.FillFormat.Clone();
                plotArea.Values.FillFormat.Parent = plotArea;
            }
            return plotArea;
        }

        /// <summary>
        /// Gets the line format of the plot area’s border.
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
        /// Gets the background filling of the plot area.
        /// </summary>
        public FillFormat FillFormat
        {
            get => Values.FillFormat ??= new FillFormat(this);
            set
            {
                SetParent(value);
                Values.FillFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the left padding of the area.
        /// </summary>
        public Unit LeftPadding
        {
            get => Values.LeftPadding ?? Unit.Empty;
            set => Values.LeftPadding = value;
        }

        /// <summary>
        /// Gets or sets the right padding of the area.
        /// </summary>
        public Unit RightPadding
        {
            get => Values.RightPadding ?? Unit.Empty;
            set => Values.RightPadding = value;
        }

        /// <summary>
        /// Gets or sets the top padding of the area.
        /// </summary>
        public Unit TopPadding
        {
            get => Values.TopPadding ?? Unit.Empty;
            set => Values.TopPadding = value;
        }

        /// <summary>
        /// Gets or sets the bottom padding of the area.
        /// </summary>
        public Unit BottomPadding
        {
            get => Values.BottomPadding ?? Unit.Empty;
            set => Values.BottomPadding = value;
        }

        /// <summary>
        /// Converts PlotArea into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteLine("\\plotarea");
            var pos = serializer.BeginAttributes();

            //if (Values.TopPadding is not null)
            if (!Values.TopPadding.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("TopPadding", TopPadding);
            //if (Values.LeftPadding is not null)
            if (!Values.LeftPadding.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("LeftPadding", LeftPadding);
            //if (Values.RightPadding is not null)
            if (!Values.RightPadding.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("RightPadding", RightPadding);
            //if (Values.BottomPadding is not null)
            if (!Values.BottomPadding.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("BottomPadding", BottomPadding);

            Values.LineFormat?.Serialize(serializer);
            Values.FillFormat?.Serialize(serializer);

            serializer.EndAttributes(pos);

            serializer.BeginContent();
            serializer.EndContent();
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(PlotArea));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new PlotAreaValues Values => (PlotAreaValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class PlotAreaValues : ChartObjectValues
        {
            internal PlotAreaValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public LineFormat? LineFormat { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public FillFormat? FillFormat { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? LeftPadding { get; set; } //= Unit.NullValue;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? RightPadding { get; set; } //= Unit.NullValue;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? TopPadding { get; set; } //= Unit.NullValue;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? BottomPadding { get; set; } //= Unit.NullValue;
        }
    }
}
