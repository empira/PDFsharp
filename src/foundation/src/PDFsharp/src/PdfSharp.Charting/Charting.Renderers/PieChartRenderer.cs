// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;
using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a pie chart renderer.
    /// </summary>
    class PieChartRenderer : ChartRenderer
    {
        /// <summary>
        /// Initializes a new instance of the PieChartRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal PieChartRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Returns an initialized renderer-specific rendererInfo.
        /// </summary>
        internal override RendererInfo Init()
        {
            var cri = new ChartRendererInfo();
            cri.Chart = (Chart)_rendererParms.DrawingItem;
            _rendererParms.RendererInfo = cri;

            InitSeries(cri);

            LegendRenderer lr = new PieLegendRenderer(_rendererParms);
            cri.LegendRendererInfo = (LegendRendererInfo?)lr.Init();

            var plotArea = cri.Chart.PlotArea;
            var renderer = GetPlotAreaRenderer();
            cri.PlotAreaRendererInfo = (PlotAreaRendererInfo)renderer.Init();

            DataLabelRenderer dlr = new PieDataLabelRenderer(_rendererParms);
            dlr.Init();

            return cri;
        }

        /// <summary>
        /// Layouts and calculates the space used by the pie chart.
        /// </summary>
        internal override void Format()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            LegendRenderer lr = new PieLegendRenderer(_rendererParms);
            lr.Format();

            // Calculate rects and positions.
            XRect chartRect = LayoutLegend();
            cri.PlotAreaRendererInfo.Rect = chartRect;
            double edge = Math.Min(chartRect.Width, chartRect.Height);
            cri.PlotAreaRendererInfo.X += (chartRect.Width - edge) / 2;
            cri.PlotAreaRendererInfo.Y += (chartRect.Height - edge) / 2;
            cri.PlotAreaRendererInfo.Width = edge;
            cri.PlotAreaRendererInfo.Height = edge;

            DataLabelRenderer dlr = new PieDataLabelRenderer(_rendererParms);
            dlr.Format();

            // Calculated remaining plot area, now it’s safe to format.
            PlotAreaRenderer renderer = GetPlotAreaRenderer();
            renderer.Format();

            dlr.CalcPositions();
        }

        /// <summary>
        /// Draws the pie chart.
        /// </summary>
        internal override void Draw()
        {
            LegendRenderer lr = new PieLegendRenderer(_rendererParms);
            lr.Draw();

            WallRenderer wr = new WallRenderer(_rendererParms);
            wr.Draw();

            PlotAreaBorderRenderer pabr = new PlotAreaBorderRenderer(_rendererParms);
            pabr.Draw();

            PlotAreaRenderer renderer = GetPlotAreaRenderer();
            renderer.Draw();

            DataLabelRenderer dlr = new PieDataLabelRenderer(_rendererParms);
            dlr.Draw();
        }

        /// <summary>
        /// Returns the specific plot area renderer.
        /// </summary>
        PlotAreaRenderer GetPlotAreaRenderer()
        {
            var chart = (Chart)_rendererParms.DrawingItem;
            return chart._type switch
            {
                ChartType.Pie2D => new PieClosedPlotAreaRenderer(_rendererParms),
                ChartType.PieExploded2D => new PieExplodedPlotAreaRenderer(_rendererParms),
                _ => null! ?? NRT.ThrowOnNull<PlotAreaRenderer>()
            };
        }

        /// <summary>
        /// Initializes all necessary data to draw a series for a pie chart.
        /// </summary>
        protected void InitSeries(ChartRendererInfo rendererInfo)
        {
            var seriesColl = rendererInfo.Chart.SeriesCollection;
            rendererInfo.SeriesRendererInfos = new SeriesRendererInfo[seriesColl.Count];
            for (int idx = 0; idx < seriesColl.Count; idx++)
            {
                SeriesRendererInfo sri = new SeriesRendererInfo();
                rendererInfo.SeriesRendererInfos[idx] = sri;
                sri.Series = seriesColl[idx];

                sri.LineFormat = Converter.ToXPen(sri.Series._lineFormat, XColors.Black, ChartRenderer.DefaultSeriesLineWidth);
                sri.FillFormat = Converter.ToXBrush(sri.Series._fillFormat, ColumnColors.Item(idx));

                sri.PointRendererInfos = new SectorRendererInfo[sri.Series._seriesElements?.Count ?? NRT.ThrowOnNull<int>()] ?? NRT.ThrowOnNull<PointRendererInfo[]>();
                for (int pointIdx = 0; pointIdx < sri.PointRendererInfos.Length; ++pointIdx)
                {
                    PointRendererInfo pri = new SectorRendererInfo();
                    var point = sri.Series._seriesElements[pointIdx];
                    pri.Point = point;
                    if (point != null!)
                    {
                        pri.LineFormat = sri.LineFormat;
                        if (point._lineFormat != null && !point._lineFormat.Color.IsEmpty)
                            pri.LineFormat = new XPen(point._lineFormat.Color);
                        if (point._fillFormat != null && !point._fillFormat.Color.IsEmpty)
                            pri.FillFormat = new XSolidBrush(point._fillFormat.Color);
                        else
                            pri.FillFormat = new XSolidBrush(PieColors.Item(pointIdx));
                        pri.LineFormat.LineJoin = XLineJoin.Round;
                    }
                    sri.PointRendererInfos[pointIdx] = pri;
                }
            }
        }
    }
}
