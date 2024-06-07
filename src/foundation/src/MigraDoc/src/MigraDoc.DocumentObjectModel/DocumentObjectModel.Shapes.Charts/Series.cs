// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// Represents a series of data on the chart.
    /// </summary>
    public class Series : ChartObject
    {
        /// <summary>
        /// Initializes a new instance of the Series class.
        /// </summary>
        public Series()
        {
            BaseValues = new SeriesValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Series Clone()
            => (Series)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            Series series = (Series)base.DeepCopy();
            if (series.Values.Elements != null)
            {
                series.Values.Elements = series.Values.Elements.Clone();
                series.Values.Elements.Parent = series;
            }
            if (series.Values.LineFormat != null)
            {
                series.Values.LineFormat = series.Values.LineFormat.Clone();
                series.Values.LineFormat.Parent = series;
            }
            if (series.Values.FillFormat != null)
            {
                series.Values.FillFormat = series.Values.FillFormat.Clone();
                series.Values.FillFormat.Parent = series;
            }
            if (series.Values.DataLabel != null)
            {
                series.Values.DataLabel = series.Values.DataLabel.Clone();
                series.Values.DataLabel.Parent = series;
            }
            return series;
        }

        /// <summary>
        /// Adds a blank to the series.
        /// </summary>
        public void AddBlank()
        {
            Elements.AddBlank();
        }

        /// <summary>
        /// Adds a real value to the series.
        /// </summary>
        public Point Add(double value)
        {
            return Elements.Add(value);
        }

        /// <summary>
        /// Adds an array of real values to the series.
        /// </summary>
        public void Add(params double[] values)
        {
            Elements.Add(values);
        }

        /// <summary>
        /// The actual value container of the series.
        /// </summary>
        public SeriesElements Elements
        {
            get => Values.Elements ??= new(this);
            set
            {
                SetParent(value);
                Values.Elements = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the series which will be used in the legend.
        /// </summary>
        public string Name
        {
            get => Values.Name ?? "";
            set => Values.Name = value;
        }

        /// <summary>
        /// Gets the line format of the border of each data.
        /// </summary>
        public LineFormat LineFormat
        {
            get => Values.LineFormat ??= new(this);
            set
            {
                SetParent(value);
                Values.LineFormat = value;
            }
        }

        /// <summary>
        /// Gets the background filling of the data.
        /// </summary>
        public FillFormat FillFormat
        {
            get => Values.FillFormat ??= new(this);
            set
            {
                SetParent(value);
                Values.FillFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the marker in a line chart.
        /// </summary>
        public Unit MarkerSize
        {
            get => Values.MarkerSize ?? Unit.Empty;
            set => Values.MarkerSize = value;
        }

        /// <summary>
        /// Gets or sets the style of the marker in a line chart.
        /// </summary>
        public MarkerStyle MarkerStyle
        {
            get => Values.MarkerStyle ?? MarkerStyle.None;
            set => Values.MarkerStyle = value;
        }

        /// <summary>
        /// Gets or sets the foreground color of the marker in a line chart.
        /// </summary>
        public Color MarkerForegroundColor
        {
            get => Values.MarkerForegroundColor ?? Color.Empty;
            set => Values.MarkerForegroundColor = value;
        }

        /// <summary>
        /// Gets or sets the background color of the marker in a line chart.
        /// </summary>
        public Color MarkerBackgroundColor
        {
            get => Values.MarkerBackgroundColor ?? Color.Empty;
            set => Values.MarkerBackgroundColor = value;
        }

        /// <summary>
        /// Gets or sets the chart type of the series if it’s intended to be different than the global chart type.
        /// </summary>
        public ChartType ChartType
        {
            get => Values.ChartType ?? ChartType.Line;
            set => Values.ChartType = value;
        }

        /// <summary>
        /// Gets the DataLabel of the series.
        /// </summary>
        public DataLabel DataLabel
        {
            get => Values.DataLabel ??= new(this);
            set
            {
                SetParent(value);
                Values.DataLabel = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the series has a DataLabel.
        /// </summary>
        public bool HasDataLabel
        {
            get => Values.HasDataLabel ?? false;
            set => Values.HasDataLabel = value;
        }

        /// <summary>
        /// Gets the element count of the series.
        /// </summary>
        public int Count
        {
            get
            {
                if (Values.Elements != null)
                    return Values.Elements.Count;

                return 0;
            }
        }

        /// <summary>
        /// Converts Series into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteLine("\\series");

            int pos = serializer.BeginAttributes();

            if (Values.Name is not null)
                serializer.WriteSimpleAttribute("Name", Name);

            //if (Values.MarkerSize is not null)
            if (!Values.MarkerSize.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("MarkerSize", MarkerSize);
            if (Values.MarkerStyle is not null)
                serializer.WriteSimpleAttribute("MarkerStyle", MarkerStyle);

            //if (Values.MarkerBackgroundColor is not null)
            if (!Values.MarkerBackgroundColor.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("MarkerBackgroundColor", MarkerBackgroundColor);
            //if (Values.MarkerForegroundColor is not null)
            if (!Values.MarkerForegroundColor.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("MarkerForegroundColor", MarkerForegroundColor);

            if (Values.ChartType is not null)
                serializer.WriteSimpleAttribute("ChartType", ChartType);

            if (Values.HasDataLabel is not null)
                serializer.WriteSimpleAttribute("HasDataLabel", HasDataLabel);

            Values.LineFormat?.Serialize(serializer);
            Values.FillFormat?.Serialize(serializer);
            Values.DataLabel?.Serialize(serializer);

            serializer.EndAttributes(pos);

            serializer.BeginContent();
            Values.Elements?.Serialize(serializer);
            serializer.WriteLine("");
            serializer.EndContent();
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Series));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new SeriesValues Values => (SeriesValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class SeriesValues : ChartObjectValues
        {
            internal SeriesValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public SeriesElements? Elements { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Name { get; set; }

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
            public Unit? MarkerSize { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public MarkerStyle? MarkerStyle { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Color? MarkerForegroundColor
            {
                get => _color;
                set => _color = Color.MakeNullIfEmpty(value);
            }
            Color? _color;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Color? MarkerBackgroundColor
            {
                get => _markerBackgroundColor;
                set => _markerBackgroundColor = Color.MakeNullIfEmpty(value);
            } 
            Color? _markerBackgroundColor;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public ChartType? ChartType { get; set; }

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
