// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;
using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a bar chart renderer.
    /// </summary>
    class BarChartRenderer : ChartRenderer
    {
        /// <summary>
        /// Initializes a new instance of the BarChartRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal BarChartRenderer(RendererParameters parms) : base(parms)
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

            var lr = GetLegendRenderer();
            cri.LegendRendererInfo = (LegendRendererInfo?)lr.Init();

            AxisRenderer xar = new VerticalXAxisRenderer(_rendererParms);
            cri.XAxisRendererInfo = (AxisRendererInfo?)xar.Init();

            AxisRenderer yar = GetYAxisRenderer();
            cri.YAxisRendererInfo = (AxisRendererInfo?)yar.Init();

            var plotArea = cri.Chart.PlotArea;
            var renderer = GetPlotAreaRenderer();
            cri.PlotAreaRendererInfo = (PlotAreaRendererInfo)renderer.Init();

            DataLabelRenderer dlr = new BarDataLabelRenderer(_rendererParms);
            dlr.Init();

            return cri;
        }

        /// <summary>
        /// Layouts and calculates the space used by the column chart.
        /// </summary>
        internal override void Format()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            var lr = GetLegendRenderer();
            lr.Format();

            // axes
            AxisRenderer xar = new VerticalXAxisRenderer(_rendererParms);
            xar.Format();

            AxisRenderer yar = GetYAxisRenderer();
            yar.Format();

            if (cri.XAxisRendererInfo == null || cri.YAxisRendererInfo == null)
                return;

            // Calculate rects and positions.
            XRect chartRect = LayoutLegend();
            cri.XAxisRendererInfo.X = chartRect.Left;
            cri.XAxisRendererInfo.Y = chartRect.Top;
            cri.XAxisRendererInfo.Height = chartRect.Height - cri.YAxisRendererInfo.Height;
            cri.YAxisRendererInfo.X = chartRect.Left + cri.XAxisRendererInfo.Width;
            cri.YAxisRendererInfo.Y = chartRect.Bottom - cri.YAxisRendererInfo.Height;
            cri.YAxisRendererInfo.Width = chartRect.Width - cri.XAxisRendererInfo.Width;
            cri.PlotAreaRendererInfo.X = cri.YAxisRendererInfo.X;
            cri.PlotAreaRendererInfo.Y = cri.XAxisRendererInfo.Y;
            cri.PlotAreaRendererInfo.Width = cri.YAxisRendererInfo.InnerRect.Width;
            cri.PlotAreaRendererInfo.Height = cri.XAxisRendererInfo.Height;

            // Calculated remaining plot area, now it's safe to format.
            var renderer = GetPlotAreaRenderer();
            renderer.Format();

            DataLabelRenderer dlr = new BarDataLabelRenderer(_rendererParms);
            dlr.Format();
        }

        /// <summary>
        /// Draws the column chart.
        /// </summary>
        internal override void Draw()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            var lr = GetLegendRenderer();
            lr.Draw();

            var wr = new WallRenderer(_rendererParms);
            wr.Draw();

            GridlinesRenderer glr = new BarGridlinesRenderer(_rendererParms);
            glr.Draw();

            var pabr = new PlotAreaBorderRenderer(_rendererParms);
            pabr.Draw();

            var renderer = GetPlotAreaRenderer();
            renderer.Draw();

            DataLabelRenderer dlr = new BarDataLabelRenderer(_rendererParms);
            dlr.Draw();

            if (cri.XAxisRendererInfo?.Axis != null)
            {
                AxisRenderer xar = new VerticalXAxisRenderer(_rendererParms);
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
                ChartType.Bar2D => new BarClusteredPlotAreaRenderer(_rendererParms),
                ChartType.BarStacked2D => new BarStackedPlotAreaRenderer(_rendererParms),
                _ => throw new InvalidOperationException("Chart type is not valid.")
            };
        }

        /// <summary>
        /// Returns the specific legend renderer.
        /// </summary>
        LegendRenderer GetLegendRenderer()
        {
            Chart chart = (Chart)_rendererParms.DrawingItem;
            return chart._type switch
            {
                ChartType.Bar2D => new BarClusteredLegendRenderer(_rendererParms),
                ChartType.BarStacked2D => new ColumnLikeLegendRenderer(_rendererParms),
                _ => throw new InvalidOperationException("Chart type is not valid.")
            };
        }

        /// <summary>
        /// Returns the specific plot area renderer.
        /// </summary>
        YAxisRenderer GetYAxisRenderer()
        {
            var chart = (Chart)_rendererParms.DrawingItem;
            return chart._type switch
            {
                ChartType.Bar2D => new HorizontalYAxisRenderer(_rendererParms),
                ChartType.BarStacked2D => new HorizontalStackedYAxisRenderer(_rendererParms),
                _ => throw new InvalidOperationException("Chart type is not valid.")
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
            // Lowest series is the first, like in Excel 
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
            foreach (var sri in cri.SeriesRendererInfos)
            {
                sri.LineFormat = Converter.ToXPen(sri.Series?._lineFormat, XColors.Black, DefaultSeriesLineWidth);
                sri.FillFormat = Converter.ToXBrush(sri.Series?._fillFormat, ColumnColors.Item(seriesIndex++));

                // ReSharper disable once CoVariantArrayConversion because we read only.
                sri.PointRendererInfos = new ColumnRendererInfo[sri.Series?._seriesElements?.Count ?? 0];
                for (int pointIdx = 0; pointIdx < sri.PointRendererInfos.Length; ++pointIdx)
                {
                    PointRendererInfo pri = new ColumnRendererInfo();
                    var point = sri.Series?._seriesElements?[pointIdx] ?? null;
                    pri.Point = point ?? NRT.ThrowOnNull<Point>();
                    if (point != null!)
                    {
                        pri.LineFormat = sri.LineFormat;
                        pri.FillFormat = sri.FillFormat;
                        if (point._lineFormat is { Color.IsEmpty: false })
                            pri.LineFormat = Converter.ToXPen(point._lineFormat, sri.LineFormat);
                        if (point._fillFormat is { Color.IsEmpty: false })
                            pri.FillFormat = new XSolidBrush(point._fillFormat.Color);
                    }
                    sri.PointRendererInfos[pointIdx] = pri;
                }
            }
        }
    }
}
