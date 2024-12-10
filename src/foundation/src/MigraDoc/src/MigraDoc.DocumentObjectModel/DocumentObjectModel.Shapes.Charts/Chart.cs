// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// Represents charts with different types.
    /// </summary>
    public class Chart : Shape, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the Chart class.
        /// </summary>
        public Chart()
        {
            BaseValues = new ChartValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Chart class with the specified parent.
        /// </summary>
        internal Chart(DocumentObject parent) : base(parent)
        {
            BaseValues = new ChartValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Chart class with the specified chart type.
        /// </summary>
        public Chart(ChartType type) : this()
        {
            Type = type;
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Chart Clone()
            => (Chart)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            Chart chart = (Chart)base.DeepCopy();
            if (chart.Values.Format != null)
            {
                chart.Values.Format = chart.Values.Format.Clone();
                chart.Values.Format.Parent = chart;
            }
            if (chart.Values.XAxis != null)
            {
                chart.Values.XAxis = chart.Values.XAxis.Clone();
                chart.Values.XAxis.Parent = chart;
            }
            if (chart.Values.YAxis != null)
            {
                chart.Values.YAxis = chart.Values.YAxis.Clone();
                chart.Values.YAxis.Parent = chart;
            }
            if (chart.Values.ZAxis != null)
            {
                chart.Values.ZAxis = chart.Values.ZAxis.Clone();
                chart.Values.ZAxis.Parent = chart;
            }
            if (chart.Values.SeriesCollection != null)
            {
                chart.Values.SeriesCollection = chart.Values.SeriesCollection.Clone();
                chart.Values.SeriesCollection.Parent = chart;
            }
            if (chart.Values.XValues != null)
            {
                chart.Values.XValues = chart.Values.XValues.Clone();
                chart.Values.XValues.Parent = chart;
            }
            if (chart.Values.HeaderArea != null)
            {
                chart.Values.HeaderArea = chart.Values.HeaderArea.Clone();
                chart.Values.HeaderArea.Parent = chart;
            }
            if (chart.Values.BottomArea != null)
            {
                chart.Values.BottomArea = chart.Values.BottomArea.Clone();
                chart.Values.BottomArea.Parent = chart;
            }
            if (chart.Values.TopArea != null)
            {
                chart.Values.TopArea = chart.Values.TopArea.Clone();
                chart.Values.TopArea.Parent = chart;
            }
            if (chart.Values.FooterArea != null)
            {
                chart.Values.FooterArea = chart.Values.FooterArea.Clone();
                chart.Values.FooterArea.Parent = chart;
            }
            if (chart.Values.LeftArea != null)
            {
                chart.Values.LeftArea = chart.Values.LeftArea.Clone();
                chart.Values.LeftArea.Parent = chart;
            }
            if (chart.Values.RightArea != null)
            {
                chart.Values.RightArea = chart.Values.RightArea.Clone();
                chart.Values.RightArea.Parent = chart;
            }
            if (chart.Values.PlotArea != null)
            {
                chart.Values.PlotArea = chart.Values.PlotArea.Clone();
                chart.Values.PlotArea.Parent = chart;
            }
            if (chart.Values.DataLabel != null)
            {
                chart.Values.DataLabel = chart.Values.DataLabel.Clone();
                chart.Values.DataLabel.Parent = chart;
            }
            return chart;
        }

        /// <summary>
        /// Gets or sets the base type of the chart.
        /// ChartType of the series can be overwritten.
        /// </summary>
        public ChartType Type
        {
            get => Values.Type ?? ChartType.Line;
            set => Values.Type = value;
        }

        /// <summary>
        /// Gets or sets the default style name of the whole chart.
        /// </summary>
        public string Style
        {
            get => Values.Style ?? "";
            set => Values.Style = value;
        }

        /// <summary>
        /// Gets the default paragraph format of the whole chart.
        /// </summary>
        public ParagraphFormat Format
        {
            get => Values.Format ??= new(this);
            set
            {
                SetParentOf(value);
                Values.Format = value;
            }
        }

        /// <summary>
        /// Gets the X-Axis of the Chart.
        /// </summary>
        public Axis XAxis
        {
            get => Values.XAxis ??= new(this);
            set
            {
                SetParentOf(value);
                Values.XAxis = value;
            }
        }

        /// <summary>
        /// Gets the Y-Axis of the Chart.
        /// </summary>
        public Axis YAxis
        {
            get => Values.YAxis ??= new(this);
            set
            {
                SetParentOf(value);
                Values.YAxis = value;
            }
        }

        /// <summary>
        /// Gets the Z-Axis of the Chart.
        /// </summary>
        public Axis ZAxis
        {
            get => Values.ZAxis ??= new(this);
            set
            {
                SetParentOf(value);
                Values.ZAxis = value;
            }
        }

        /// <summary>
        /// Gets the collection of the data series.
        /// </summary>
        public SeriesCollection SeriesCollection
        {
            get => Values.SeriesCollection ??= new(this);
            set
            {
                SetParentOf(value);
                Values.SeriesCollection = value;
            }
        }

        /// <summary>
        /// Gets the collection of the values written on the X-Axis.
        /// </summary>
        public XValues XValues
        {
            get => Values.XValues ??= new(this);
            set
            {
                SetParentOf(value);
                Values.XValues = value;
            }
        }

        /// <summary>
        /// Gets the header area of the chart.
        /// </summary>
        public TextArea HeaderArea
        {
            get => Values.HeaderArea ??= new(this);
            set
            {
                SetParentOf(value);
                Values.HeaderArea = value;
            }
        }

        /// <summary>
        /// Gets the bottom area of the chart.
        /// </summary>
        public TextArea BottomArea
        {
            get => Values.BottomArea ??= new(this);
            set
            {
                SetParentOf(value);
                Values.BottomArea = value;
            }
        }

        /// <summary>
        /// Gets the top area of the chart.
        /// </summary>
        public TextArea TopArea
        {
            get => Values.TopArea ??= new(this);
            set
            {
                SetParentOf(value);
                Values.TopArea = value;
            }
        }

        /// <summary>
        /// Gets the footer area of the chart.
        /// </summary>
        public TextArea FooterArea
        {
            get => Values.FooterArea ??= new(this);
            set
            {
                SetParentOf(value);
                Values.FooterArea = value;
            }
        }

        /// <summary>
        /// Gets the left area of the chart.
        /// </summary>
        public TextArea LeftArea
        {
            get => Values.LeftArea ??= new(this);
            set
            {
                SetParentOf(value);
                Values.LeftArea = value;
            }
        }

        /// <summary>
        /// Gets the right area of the chart.
        /// </summary>
        public TextArea RightArea
        {
            get => Values.RightArea ??= new(this);
            set
            {
                SetParentOf(value);
                Values.RightArea = value;
            }
        }

        /// <summary>
        /// Gets the plot (drawing) area of the chart.
        /// </summary>
        public PlotArea PlotArea
        {
            get => Values.PlotArea ??= new(this);
            set
            {
                SetParentOf(value);
                Values.PlotArea = value;
            }
        }

        /// <summary>
        /// Gets or sets a value defining how blanks in the data series should be shown.
        /// </summary>
        public BlankType DisplayBlanksAs
        {
            get => Values.DisplayBlanksAs ?? BlankType.NotPlotted;
            set => Values.DisplayBlanksAs = value;
        }

        /// <summary>
        /// Gets or sets whether XAxis Labels should be merged.
        /// </summary>
        public bool PivotChart
        {
            get => Values.PivotChart ?? false;
            set => Values.PivotChart = value;
        }

        /// <summary>
        /// Gets the DataLabel of the chart.
        /// </summary>
        public DataLabel DataLabel
        {
            get => Values.DataLabel ??= new(this);
            set
            {
                SetParentOf(value);
                Values.DataLabel = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the chart has a DataLabel.
        /// </summary>
        public bool HasDataLabel
        {
            get => Values.HasDataLabel ?? false;
            set => Values.HasDataLabel = value;
        }

        /// <summary>
        /// Determines the type of the given axis.
        /// </summary>
        internal string CheckAxis(Axis axis)
        {
            if ((Values.XAxis != null) && (axis == Values.XAxis))
                return "xaxis";
            if ((Values.YAxis != null) && (axis == Values.YAxis))
                return "yaxis";
            if ((Values.ZAxis != null) && (axis == Values.ZAxis))
                return "zaxis";

            return "";
        }

        /// <summary>
        /// Determines the type of the given textarea.
        /// </summary>
        internal string CheckTextArea(TextArea textArea)
        {
            if ((Values.HeaderArea != null) && (textArea == Values.HeaderArea))
                return "headerarea";
            if ((Values.FooterArea != null) && (textArea == Values.FooterArea))
                return "footerarea";
            if ((Values.LeftArea != null) && (textArea == Values.LeftArea))
                return "leftarea";
            if ((Values.RightArea != null) && (textArea == Values.RightArea))
                return "rightarea";
            if ((Values.TopArea != null) && (textArea == Values.TopArea))
                return "toparea";
            if ((Values.BottomArea != null) && (textArea == Values.BottomArea))
                return "bottomarea";

            return "";
        }

        /// <summary>
        /// Converts Chart into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteLine("\\chart(" + Type + ")");
            int pos = serializer.BeginAttributes();

            base.Serialize(serializer);
            if (Values.DisplayBlanksAs is not null)
                serializer.WriteSimpleAttribute("DisplayBlanksAs", DisplayBlanksAs);
            if (Values.PivotChart is not null)
                serializer.WriteSimpleAttribute("PivotChart", PivotChart);
            if (Values.HasDataLabel is not null)
                serializer.WriteSimpleAttribute("HasDataLabel", HasDataLabel);

            if (Values.Style is not null)
                serializer.WriteSimpleAttribute("Style", Style);
            Values.Format?.Serialize(serializer, "Format", null);
            Values.DataLabel?.Serialize(serializer);
            serializer.EndAttributes(pos);

            serializer.BeginContent();

            if (Values.PlotArea is not null && !Values.PlotArea.IsNull())
                Values.PlotArea.Serialize(serializer);
            if (Values.HeaderArea is not null && !Values.HeaderArea.IsNull())
                Values.HeaderArea.Serialize(serializer);
            if (Values.FooterArea is not null && !Values.FooterArea.IsNull())
                Values.FooterArea.Serialize(serializer);
            if (Values.TopArea is not null && !Values.TopArea.IsNull())
                Values.TopArea.Serialize(serializer);
            if (Values.BottomArea is not null && !Values.BottomArea.IsNull())
                Values.BottomArea.Serialize(serializer);
            if (Values.LeftArea is not null && !Values.LeftArea.IsNull())
                Values.LeftArea.Serialize(serializer);
            if (Values.RightArea is not null && !Values.RightArea.IsNull())
                Values.RightArea.Serialize(serializer);

            Values.XAxis?.Serialize(serializer);
            Values.YAxis?.Serialize(serializer);
            Values.ZAxis?.Serialize(serializer);

            Values.SeriesCollection?.Serialize(serializer);
            Values.XValues?.Serialize(serializer);

            serializer.EndContent();
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitChart(this);
            if (visitChildren)
            {
                if (Values.BottomArea is not null)
                    ((IVisitable)Values.BottomArea).AcceptVisitor(visitor, true);

                if (Values.FooterArea is not null)
                    ((IVisitable)Values.FooterArea).AcceptVisitor(visitor, true);

                if (Values.HeaderArea is not null)
                    ((IVisitable)Values.HeaderArea).AcceptVisitor(visitor, true);

                if (Values.LeftArea is not null)
                    ((IVisitable)Values.LeftArea).AcceptVisitor(visitor, true);

                if (Values.RightArea is not null)
                    ((IVisitable)Values.RightArea).AcceptVisitor(visitor, true);

                if (Values.TopArea != null)
                    ((IVisitable)Values.TopArea).AcceptVisitor(visitor, true);
            }
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Chart));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new ChartValues Values => (ChartValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class ChartValues : ShapeValues
        {
            internal ChartValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public ChartType? Type { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Style { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public ParagraphFormat? Format { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Axis? XAxis { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Axis? YAxis { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Axis? ZAxis { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public SeriesCollection? SeriesCollection { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public XValues? XValues { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public TextArea? HeaderArea { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public TextArea? BottomArea { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public TextArea? TopArea { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public TextArea? FooterArea { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public TextArea? LeftArea { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public TextArea? RightArea { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public PlotArea? PlotArea { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public BlankType? DisplayBlanksAs { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? PivotChart { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public DataLabel? DataLabel { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? HasDataLabel { get; set; }
        }
    }
}
