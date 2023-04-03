// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents an area chart renderer.
    /// </summary>
    class AreaChartRenderer : ColumnLikeChartRenderer
    {
        /// <summary>
        /// Initializes a new instance of the AreaChartRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal AreaChartRenderer(RendererParameters parms) : base(parms)
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

            AxisRenderer yar = new VerticalYAxisRenderer(_rendererParms);
            cri.YAxisRendererInfo = (AxisRendererInfo?)yar.Init();

            var plotArea = cri.Chart.PlotArea;
            PlotAreaRenderer renderer = new AreaPlotAreaRenderer(_rendererParms);
            cri.PlotAreaRendererInfo = (PlotAreaRendererInfo)renderer.Init();

            return cri;
        }

        /// <summary>
        /// Layouts and calculates the space used by the line chart.
        /// </summary>
        internal override void Format()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            LegendRenderer lr = new ColumnLikeLegendRenderer(_rendererParms);
            lr.Format();

            // Axes.
            AxisRenderer xar = new HorizontalXAxisRenderer(_rendererParms);
            xar.Format();

            AxisRenderer yar = new VerticalYAxisRenderer(_rendererParms);
            yar.Format();

            // Calculate rects and positions.
            CalcLayout();

            // Calculated remaining plot area, now it's safe to format.
            PlotAreaRenderer renderer = new AreaPlotAreaRenderer(_rendererParms);
            renderer.Format();
        }

        /// <summary>
        /// Draws the column chart.
        /// </summary>
        internal override void Draw()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            LegendRenderer lr = new ColumnLikeLegendRenderer(_rendererParms);
            lr.Draw();

            // Draw wall.
            var wr = new WallRenderer(_rendererParms);
            wr.Draw();

            // Draw gridlines.
            GridlinesRenderer glr = new ColumnLikeGridlinesRenderer(_rendererParms);
            glr.Draw();

            var pabr = new PlotAreaBorderRenderer(_rendererParms);
            pabr.Draw();

            PlotAreaRenderer renderer = new AreaPlotAreaRenderer(_rendererParms);
            renderer.Draw();

            // Draw axes.
            if (cri.XAxisRendererInfo?.Axis != null)
            {
                AxisRenderer xar = new HorizontalXAxisRenderer(_rendererParms);
                xar.Draw();
            }

            if (cri.YAxisRendererInfo?.Axis != null)
            {
                AxisRenderer yar = new VerticalYAxisRenderer(_rendererParms);
                yar.Draw();
            }
        }

        /// <summary>
        /// Initializes all necessary data to draw a series for an area chart.
        /// </summary>
        void InitSeriesRendererInfo()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            var seriesColl = cri.Chart.SeriesCollection;
            cri.SeriesRendererInfos = new SeriesRendererInfo[seriesColl.Count];
            for (int idx = 0; idx < seriesColl.Count; ++idx)
            {
                var sri = new SeriesRendererInfo();
                sri.Series = seriesColl[idx];
                cri.SeriesRendererInfos[idx] = sri;
            }

            InitSeries();
        }

        /// <summary>
        /// Initializes all necessary data to draw a series for an area chart.
        /// </summary>
        internal void InitSeries()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            int seriesIndex = 0;
            foreach (SeriesRendererInfo sri in cri.SeriesRendererInfos)
            {
                sri.LineFormat = Converter.ToXPen(sri.Series._lineFormat, XColors.Black, ChartRenderer.DefaultSeriesLineWidth);
                sri.FillFormat = Converter.ToXBrush(sri.Series._fillFormat, ColumnColors.Item(seriesIndex++));

                var elements = sri.Series._seriesElements;
                if (elements != null)
                {
                    sri.PointRendererInfos = new PointRendererInfo[elements.Count ];
                    for (int pointIdx = 0; pointIdx < sri.PointRendererInfos.Length; ++pointIdx)
                    {
                        var pri = new PointRendererInfo();
                        var point = elements[pointIdx];
                        pri.Point = point;
                        if (point != null!)
                        {
                            pri.LineFormat = sri.LineFormat;
                            pri.FillFormat = sri.FillFormat;
                            if (point._lineFormat != null && !point._lineFormat.Color.IsEmpty)
                                pri.LineFormat = new XPen(point._lineFormat.Color, point._lineFormat.Width);
                            if (point._fillFormat != null && !point._fillFormat.Color.IsEmpty)
                                pri.FillFormat = new XSolidBrush(point._fillFormat.Color);
                        }
                        sri.PointRendererInfos[pointIdx] = pri;
                    }
                }
            }
        }
    }
}
