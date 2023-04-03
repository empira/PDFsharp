// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using System.ComponentModel;

namespace PdfSharp.Charting
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
        { }

        #region Methods
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
            var series = (Series)base.DeepCopy();
            if (series._seriesElements != null)
            {
                series._seriesElements = series._seriesElements.Clone();
                series._seriesElements.Parent = series;
            }
            if (series._lineFormat != null)
            {
                series._lineFormat = series._lineFormat.Clone();
                series._lineFormat.Parent = series;
            }
            if (series._fillFormat != null)
            {
                series._fillFormat = series._fillFormat.Clone();
                series._fillFormat.Parent = series;
            }
            if (series._dataLabel != null)
            {
                series._dataLabel = series._dataLabel.Clone();
                series._dataLabel.Parent = series;
            }
            return series;
        }

        /// <summary>
        /// Adds a blank to the series.
        /// </summary>
        public void AddBlank() 
            => Elements.AddBlank();

        /// <summary>
        /// Adds a real value to the series.
        /// </summary>
        public Point Add(double value) 
            => Elements.Add(value);

        /// <summary>
        /// Adds an array of real values to the series.
        /// </summary>
        public void Add(params double[] values)
            => Elements.Add(values);
        #endregion

        #region Properties
        /// <summary>
        /// The actual value container of the series.
        /// </summary>
        public SeriesElements Elements => _seriesElements ??= new SeriesElements(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal SeriesElements? _seriesElements;

        /// <summary>
        /// Gets or sets the name of the series which will be used in the legend.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets the line format of the border of each data.
        /// </summary>
        public LineFormat LineFormat 
            => _lineFormat ??= new LineFormat(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal LineFormat? _lineFormat;

        /// <summary>
        /// Gets the background filling of the data.
        /// </summary>
        public FillFormat FillFormat 
            => _fillFormat ??= new FillFormat(this);

        // ReSharper disable once InconsistentNaming because this is old code
        internal FillFormat? _fillFormat;

        /// <summary>
        /// Gets or sets the size of the marker in a line chart.
        /// </summary>
        public XUnit MarkerSize { get; set; }

        /// <summary>
        /// Gets or sets the style of the marker in a line chart.
        /// </summary>
        public MarkerStyle MarkerStyle
        {
            get => _markerStyle;
            set
            {
                if (!Enum.IsDefined(typeof(MarkerStyle), value))
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(MarkerStyle));

                _markerStyle = value;
                _markerStyleInitialized = true;
            }
        }
        // ReSharper disable once InconsistentNaming because this is old code
        internal MarkerStyle _markerStyle;
        // ReSharper disable once InconsistentNaming because this is old code
        internal bool _markerStyleInitialized;

        /// <summary>
        /// Gets or sets the foreground color of the marker in a line chart.
        /// </summary>
        public XColor MarkerForegroundColor { get; set; } = XColor.Empty;

        /// <summary>
        /// Gets or sets the background color of the marker in a line chart.
        /// </summary>
        public XColor MarkerBackgroundColor { get; set; } = XColor.Empty;

        /// <summary>
        /// Gets or sets the chart type of the series if it's intended to be different than the
        /// global chart type.
        /// </summary>
        public ChartType ChartType
        {
            get => _chartType;
            set
            {
                if (!Enum.IsDefined(typeof(ChartType), value))
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(ChartType));
                _chartType = value;
            }
        }
        // ReSharper disable once InconsistentNaming because this is old code
        internal ChartType _chartType;

        /// <summary>
        /// Gets the DataLabel of the series.
        /// </summary>
        public DataLabel DataLabel => _dataLabel ??= new DataLabel(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal DataLabel? _dataLabel;

        /// <summary>
        /// Gets or sets whether the series has a DataLabel.
        /// </summary>
        public bool HasDataLabel { get; set; }

        /// <summary>
        /// Gets the element count of the series.
        /// </summary>
        public int Count => _seriesElements?.Count ?? 0;
        #endregion
    }
}
