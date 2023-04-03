// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;
using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a column chart renderer.
    /// </summary>
    class ColumnChartRenderer : ColumnLikeChartRenderer
    {
        /// <summary>
        /// Initializes a new instance of the ColumnChartRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal ColumnChartRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Returns an initialized renderer-specific rendererInfo.
        /// </summary>
        internal override RendererInfo Init()
        {
            var cri = new ChartRendererInfo
            {
                Chart = (Chart)_rendererParms.DrawingItem
            };
            _rendererParms.RendererInfo = cri;

            InitSeriesRendererInfo();

            LegendRenderer lr = new ColumnLikeLegendRenderer(_rendererParms);
            cri.LegendRendererInfo = (LegendRendererInfo?)lr.Init();

            AxisRenderer xar = new HorizontalXAxisRenderer(_rendererParms);
            cri.XAxisRendererInfo = (AxisRendererInfo?)xar.Init();

            AxisRenderer yar = GetYAxisRenderer();
            cri.YAxisRendererInfo = (AxisRendererInfo?)yar.Init();

            var plotArea = cri.Chart.PlotArea;
            var renderer = GetPlotAreaRenderer();
            cri.PlotAreaRendererInfo = (PlotAreaRendererInfo)renderer.Init();

            DataLabelRenderer dlr = new ColumnDataLabelRenderer(_rendererParms);
            dlr.Init();

            return cri;
        }

        /// <summary>
        /// Layouts and calculates the space used by the column chart.
        /// </summary>
        internal override void Format()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            LegendRenderer lr = new ColumnLikeLegendRenderer(_rendererParms);
            lr.Format();

            // Axes.
            AxisRenderer xar = new HorizontalXAxisRenderer(_rendererParms);
            xar.Format();

            AxisRenderer yar = GetYAxisRenderer();
            yar.Format();

            // Calculate rects and positions.
            CalcLayout();

            // Calculated remaining plot area, now it's safe to format.
            var renderer = GetPlotAreaRenderer();
            renderer.Format();

            DataLabelRenderer dlr = new ColumnDataLabelRenderer(_rendererParms);
            dlr.Format();
        }

        /// <summary>
        /// Draws the column chart.
        /// </summary>
        internal override void Draw()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            LegendRenderer lr = new ColumnLikeLegendRenderer(_rendererParms);
            lr.Draw();

            var wr = new WallRenderer(_rendererParms);
            wr.Draw();

            GridlinesRenderer glr = new ColumnLikeGridlinesRenderer(_rendererParms);
            glr.Draw();

            var pabr = new PlotAreaBorderRenderer(_rendererParms);
            pabr.Draw();

            var renderer = GetPlotAreaRenderer();
            renderer.Draw();

            DataLabelRenderer dlr = new ColumnDataLabelRenderer(_rendererParms);
            dlr.Draw();

            if (cri.XAxisRendererInfo?.Axis != null)
            {
                AxisRenderer xar = new HorizontalXAxisRenderer(_rendererParms);
                xar.Draw();
            }

            if (cri.YAxisRendererInfo?.Axis != null)
            {
                AxisRenderer yar = GetYAxisRenderer();
                yar.Draw();
            }
        }

        /// <summary>
        /// Returns the specific plot area renderer.
        /// </summary>
        PlotAreaRenderer GetPlotAreaRenderer()
        {
            var chart = (Chart)_rendererParms.DrawingItem;
            return chart._type switch
            {
                ChartType.Column2D => new ColumnClusteredPlotAreaRenderer(_rendererParms),
                ChartType.ColumnStacked2D => new ColumnStackedPlotAreaRenderer(_rendererParms),
                _ => throw new InvalidOperationException("Invalid chart type.")
            };
        }

        /// <summary>
        /// Returns the specific y axis renderer.
        /// </summary>
        YAxisRenderer GetYAxisRenderer()
        {
            var chart = (Chart)_rendererParms.DrawingItem;
            return chart._type switch
            {
                ChartType.Column2D => new VerticalYAxisRenderer(_rendererParms),
                ChartType.ColumnStacked2D => new VerticalStackedYAxisRenderer(_rendererParms),
                _ => throw new InvalidOperationException("Invalid chart type.")
            };
        }

        /// <summary>
        /// Initializes all necessary data to draw all series for a column chart.
        /// </summary>
        void InitSeriesRendererInfo()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            var seriesColl = cri.Chart.SeriesCollection;
            cri.SeriesRendererInfos = new SeriesRendererInfo[seriesColl.Count];
            for (int idx = 0; idx < seriesColl.Count; ++idx)
            {
                SeriesRendererInfo sri = new SeriesRendererInfo();
                sri.Series = seriesColl[idx];
                cri.SeriesRendererInfos[idx] = sri;
            }
            InitSeries();
        }

        /// <summary>
        /// Initializes all necessary data to draw all series for a column chart.
        /// </summary>
        internal void InitSeries()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            int seriesIndex = 0;
            foreach (SeriesRendererInfo sri in cri.SeriesRendererInfos)
            {
                sri.LineFormat = Converter.ToXPen(sri.Series._lineFormat, XColors.Black, ChartRenderer.DefaultSeriesLineWidth);
                sri.FillFormat = Converter.ToXBrush(sri.Series._fillFormat, ColumnColors.Item(seriesIndex++));

                sri.PointRendererInfos = new ColumnRendererInfo[sri.Series._seriesElements?.Count ?? 0];
                for (int pointIdx = 0; pointIdx < sri.PointRendererInfos.Length; ++pointIdx)
                {
                    PointRendererInfo pri = new ColumnRendererInfo();
                    Point point = sri.Series._seriesElements?[pointIdx] ?? NRT.ThrowOnNull<Point>();
                    pri.Point = point;
                    if (point != null)
                    {
                        pri.LineFormat = sri.LineFormat;
                        pri.FillFormat = sri.FillFormat;
                        if (point._lineFormat != null)
                            pri.LineFormat = Converter.ToXPen(point._lineFormat, sri.LineFormat);
                        if (point._fillFormat != null && !point._fillFormat.Color.IsEmpty)
                            pri.FillFormat = new XSolidBrush(point._fillFormat.Color);
                    }
                    sri.PointRendererInfos[pointIdx] = pri;
                }
            }
        }
    }
}
