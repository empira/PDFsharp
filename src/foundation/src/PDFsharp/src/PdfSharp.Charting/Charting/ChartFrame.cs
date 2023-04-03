// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Charting.Renderers;

namespace PdfSharp.Charting
{
    /// <summary>
    /// Represents the frame which holds one or more charts.
    /// </summary>
    public class ChartFrame
    {
        /// <summary>
        /// Initializes a new instance of the ChartFrame class.
        /// </summary>
        public ChartFrame()
        { }

        /// <summary>
        /// Initializes a new instance of the ChartFrame class with the specified rectangle.
        /// </summary>
        public ChartFrame(XRect rect)
        {
            Location = rect.Location;
            Size = rect.Size;
        }

        /// <summary>
        /// Gets or sets the location of the ChartFrame.
        /// </summary>
        public XPoint Location { get; set; }

        /// <summary>
        /// Gets or sets the size of the ChartFrame.
        /// </summary>
        public XSize Size { get; set; }

        /// <summary>
        /// Adds a chart to the ChartFrame.
        /// </summary>
        public void Add(Chart chart)
        {
            _chartList ??= new List<Chart>();
            _chartList.Add(chart);
        }

        /// <summary>
        /// Draws all charts inside the ChartFrame.
        /// </summary>
        public void Draw(XGraphics gfx)
        {
            // Draw frame of ChartFrame. First shadow frame.
            const int dx = 5;
            const int dy = 5;
            gfx.DrawRoundedRectangle(XBrushes.Gainsboro,
                                     Location.X + dx, Location.Y + dy,
                                     Size.Width, Size.Height, 20, 20);

            XRect chartRect = new XRect(Location.X, Location.Y, Size.Width, Size.Height);
            var brush = new XLinearGradientBrush(chartRect, XColor.FromArgb(0xFFD0DEEF), XColors.White,
                                                                  XLinearGradientMode.Vertical);
            var penBorder = new XPen(XColors.SteelBlue, 2.5);
            gfx.DrawRoundedRectangle(penBorder, brush,
                                     Location.X, Location.Y, Size.Width, Size.Height,
                                     15, 15);

            if (_chartList is null)
                return;

            var state = gfx.Save();
            gfx.TranslateTransform(Location.X, Location.Y);

            // Calculate rectangle for all charts. Y-Position will be moved for each chart.
            int charts = _chartList.Count;
            const uint dxChart = 20;
            const uint dyChart = 20;
            const uint dyBetweenCharts = 30;
            XRect rect = new XRect(dxChart, dyChart,
              Size.Width - 2 * dxChart,
              (Size.Height - (charts - 1) * dyBetweenCharts - 2 * dyChart) / charts);

            // Draw each chart in list.
            foreach (var chart in _chartList)
            {
                var parms = new RendererParameters(gfx, rect)
                {
                    DrawingItem = chart
                };

                var renderer = GetChartRenderer(chart, parms) ?? throw new InvalidOperationException("No chart renderer found.");
                renderer.Init();
                renderer.Format();
                renderer.Draw();

                rect.Y += rect.Height + dyBetweenCharts;
            }
            gfx.Restore(state);

            //      // Calculate rectangle for all charts. Y-Position will be moved for each chart.
            //      int charts = chartList.Count;
            //      uint dxChart = 0;
            //      uint dyChart = 0;
            //      uint dyBetweenCharts = 0;
            //      XRect rect = new XRect(dxChart, dyChart,
            //        size.Width - 2 * dxChart,
            //        (size.Height - (charts - 1) * dyBetweenCharts - 2 * dyChart) / charts);
            //
            //      // draw each chart in list
            //      foreach (Chart chart in chartList)
            //      {
            //        RendererParameters parms = new RendererParameters(gfx, rect);
            //        parms.DrawingItem = chart;
            //
            //        ChartRenderer renderer = GetChartRenderer(chart, parms);
            //        renderer.Init();
            //        renderer.Format();
            //        renderer.Draw();
            //
            //        rect.Y += rect.Height + dyBetweenCharts;
            //      }
        }

        /// <summary>
        /// Draws first chart only.
        /// </summary>
        public void DrawChart(XGraphics gfx)
        {
            if (_chartList is null || _chartList.Count == 0)
                return;

            var state = gfx.Save();
            {
                gfx.TranslateTransform(Location.X, Location.Y);

                if (_chartList.Count > 0)
                {
                    XRect chartRect = new XRect(0, 0, Size.Width, Size.Height);
                    var chart = _chartList[0];
                    var parms = new RendererParameters(gfx, chartRect)
                    {
                        DrawingItem = chart
                    };

                    var renderer = GetChartRenderer(chart, parms) ??
                                   throw new InvalidOperationException("No chart renderer found.");
                    renderer.Init();
                    renderer.Format();
                    renderer.Draw();
                }
            }
            gfx.Restore(state);
        }

        /// <summary>
        /// Returns the chart renderer appropriate for the chart.
        /// </summary>
        ChartRenderer? GetChartRenderer(Chart chart, RendererParameters parms)
        {
            var chartType = chart.Type;
            bool useCombinationRenderer = false;
            if (chart._seriesCollection is not null)
            {
                foreach (Series series in chart._seriesCollection)
                {
                    if (series._chartType != chartType)
                    {
                        useCombinationRenderer = true;
                        break;
                    }
                }
            }

            if (useCombinationRenderer)
                return new CombinationChartRenderer(parms);

            return chartType switch
            {
                ChartType.Line => new LineChartRenderer(parms),
                ChartType.Column2D => new ColumnChartRenderer(parms),
                ChartType.ColumnStacked2D => new ColumnChartRenderer(parms),
                ChartType.Bar2D => new BarChartRenderer(parms),
                ChartType.BarStacked2D => new BarChartRenderer(parms),
                ChartType.Area2D => new AreaChartRenderer(parms),
                ChartType.Pie2D => new PieChartRenderer(parms),
                ChartType.PieExploded2D => new PieChartRenderer(parms),
                _ => null
            };
        }

        /// <summary>
        /// Holds the charts which will be drawn inside the ChartFrame.
        /// </summary>
        List<Chart>? _chartList;
    }
}
