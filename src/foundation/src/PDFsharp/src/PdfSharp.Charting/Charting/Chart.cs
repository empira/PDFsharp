// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting
{
    /// <summary>
    /// Represents charts with different types.
    /// </summary>
    public class Chart : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the Chart class.
        /// </summary>
        public Chart()
        { }

        /// <summary>
        /// Initializes a new instance of the Chart class with the specified parent.
        /// </summary>
        internal Chart(DocumentObject parent) 
            : base(parent) 
        { }

        /// <summary>
        /// Initializes a new instance of the Chart class with the specified chart type.
        /// </summary>
        public Chart(ChartType type) : this()
        {
            Type = type;
        }

        #region Methods
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
            var chart = (Chart)base.DeepCopy();
            if (chart._xAxis != null)
            {
                chart._xAxis = chart._xAxis.Clone();
                chart._xAxis.Parent = chart;
            }
            if (chart._yAxis != null)
            {
                chart._yAxis = chart._yAxis.Clone();
                chart._yAxis.Parent = chart;
            }
            if (chart._zAxis != null)
            {
                chart._zAxis = chart._zAxis.Clone();
                chart._zAxis.Parent = chart;
            }
            if (chart._seriesCollection != null)
            {
                chart._seriesCollection = chart._seriesCollection.Clone();
                chart._seriesCollection.Parent = chart;
            }
            if (chart._xValues != null)
            {
                chart._xValues = chart._xValues.Clone();
                chart._xValues.Parent = chart;
            }
            if (chart._plotArea != null)
            {
                chart._plotArea = chart._plotArea.Clone();
                chart._plotArea.Parent = chart;
            }
            if (chart._dataLabel != null)
            {
                chart._dataLabel = chart._dataLabel.Clone();
                chart._dataLabel.Parent = chart;
            }
            return chart;
        }

        /// <summary>
        /// Determines the type of the given axis.
        /// </summary>
        internal string CheckAxis(Axis axis)
        {
            if (_xAxis != null && axis == _xAxis)
                return "xaxis";
            if (_yAxis != null && axis == _yAxis)
                return "yaxis";
            if (_zAxis != null && axis == _zAxis)
                return "zaxis";

            return "";
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the base type of the chart.
        /// ChartType of the series can be overwritten.
        /// </summary>
        public ChartType Type
        {
            get => _type;
            set => _type = value;
        }
        // ReSharper disable once InconsistentNaming because this is old code
        internal ChartType _type;

        /// <summary>
        /// Gets or sets the font for the chart. This will be the default font for all objects which are
        /// part of the chart.
        /// </summary>
        public Font Font => _font ??= new Font(this);

        // ReSharper disable once InconsistentNaming because this is old code
        internal Font? _font;

        /// <summary>
        /// Gets the legend of the chart.
        /// </summary>
        public Legend Legend => _legend ??= new Legend(this);

        // ReSharper disable once InconsistentNaming because this is old code
        internal Legend? _legend;

        /// <summary>
        /// Gets the X-Axis of the Chart.
        /// </summary>
        public Axis XAxis => _xAxis ??= new Axis(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal Axis? _xAxis;

        /// <summary>
        /// Gets the Y-Axis of the Chart.
        /// </summary>
        public Axis YAxis => _yAxis ??= new Axis(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal Axis? _yAxis;

        /// <summary>
        /// Gets the Z-Axis of the Chart.
        /// </summary>
        public Axis ZAxis => _zAxis ??= new Axis(this);
        // ReSharper disable once InconsistentNaming because this is old code
        Axis? _zAxis;

        /// <summary>
        /// Gets the collection of the data series.
        /// </summary>
        public SeriesCollection SeriesCollection
            => _seriesCollection ??= new SeriesCollection(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal SeriesCollection? _seriesCollection;

        /// <summary>
        /// Gets the collection of the values written on the X-Axis.
        /// </summary>
        public XValues XValues => _xValues ??= new XValues(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal XValues? _xValues;

        /// <summary>
        /// Gets the plot (drawing) area of the chart.
        /// </summary>
        public PlotArea PlotArea => _plotArea ??= new PlotArea(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal PlotArea? _plotArea;

        /// <summary>
        /// Gets or sets a value defining how blanks in the data series should be shown.
        /// </summary>
        public BlankType DisplayBlanksAs { get; set; }

        /// <summary>
        /// Gets the DataLabel of the chart.
        /// </summary>
        public DataLabel DataLabel => _dataLabel ??= new DataLabel(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal DataLabel? _dataLabel;

        /// <summary>
        /// Gets or sets whether the chart has a DataLabel.
        /// </summary>
        public bool HasDataLabel { get; set; }
        #endregion
    }
}
