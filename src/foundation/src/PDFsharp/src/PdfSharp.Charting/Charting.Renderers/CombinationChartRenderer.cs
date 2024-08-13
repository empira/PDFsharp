// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a renderer for combinations of charts.
    /// </summary>
    class CombinationChartRenderer : ChartRenderer
    {
        /// <summary>
        /// Initializes a new instance of the CombinationChartRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal CombinationChartRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Returns an initialized renderer-specific rendererInfo.
        /// </summary>
        internal override RendererInfo Init()
        {
            var cri = new CombinationRendererInfo();
            cri.Chart = (Chart)_rendererParms.DrawingItem;
            _rendererParms.RendererInfo = cri;

            InitSeriesRendererInfo();
            DistributeSeries();

            if (cri.AreaSeriesRendererInfos != null)
            {
                cri.SeriesRendererInfos = cri.AreaSeriesRendererInfos;
                AreaChartRenderer renderer = new AreaChartRenderer(_rendererParms);
                renderer.InitSeries();
            }
            if (cri.ColumnSeriesRendererInfos != null)
            {
                cri.SeriesRendererInfos = cri.ColumnSeriesRendererInfos;
                ColumnChartRenderer renderer = new ColumnChartRenderer(_rendererParms);
                renderer.InitSeries();
            }
            if (cri.LineSeriesRendererInfos != null)
            {
                cri.SeriesRendererInfos = cri.LineSeriesRendererInfos;
                LineChartRenderer renderer = new LineChartRenderer(_rendererParms);
                renderer.InitSeries();
            }
            cri.SeriesRendererInfos = cri.CommonSeriesRendererInfos;

            LegendRenderer lr = new ColumnLikeLegendRenderer(_rendererParms);
            cri.LegendRendererInfo = (LegendRendererInfo?)lr.Init();

            AxisRenderer xar = new HorizontalXAxisRenderer(_rendererParms);
            cri.XAxisRendererInfo = (AxisRendererInfo?)xar.Init();

            AxisRenderer yar = new VerticalYAxisRenderer(_rendererParms);
            cri.YAxisRendererInfo = (AxisRendererInfo?)yar.Init();

            PlotArea plotArea = cri.Chart.PlotArea;
            PlotAreaRenderer apar = new AreaPlotAreaRenderer(_rendererParms);
            cri.PlotAreaRendererInfo = (PlotAreaRendererInfo)apar.Init();

            // Draw data labels.
            if (cri.ColumnSeriesRendererInfos != null)
            {
                cri.SeriesRendererInfos = cri.ColumnSeriesRendererInfos;
                DataLabelRenderer dlr = new ColumnDataLabelRenderer(_rendererParms);
                dlr.Init();
            }

            return cri;
        }

        /// <summary>
        /// Layouts and calculates the space used by the combination chart.
        /// </summary>
        internal override void Format()
        {
            var cri = (CombinationRendererInfo)_rendererParms.RendererInfo;
            cri.SeriesRendererInfos = cri.CommonSeriesRendererInfos;

            LegendRenderer lr = new ColumnLikeLegendRenderer(_rendererParms);
            lr.Format();

            // axes
            AxisRenderer xar = new HorizontalXAxisRenderer(_rendererParms);
            xar.Format();

            AxisRenderer yar = new VerticalYAxisRenderer(_rendererParms);
            yar.Format();

            if (cri.XAxisRendererInfo == null || cri.YAxisRendererInfo == null)
                return;

            // Calculate rects and positions.
            XRect chartRect = LayoutLegend();
            cri.XAxisRendererInfo.X = chartRect.Left + cri.YAxisRendererInfo.Width;
            cri.XAxisRendererInfo.Y = chartRect.Bottom - cri.XAxisRendererInfo.Height;
            cri.XAxisRendererInfo.Width = chartRect.Width - cri.YAxisRendererInfo.Width;
            cri.YAxisRendererInfo.X = chartRect.Left;
            cri.YAxisRendererInfo.Y = chartRect.Top;
            cri.YAxisRendererInfo.Height = chartRect.Height - cri.XAxisRendererInfo.Height;
            cri.PlotAreaRendererInfo.X = cri.XAxisRendererInfo.X;
            cri.PlotAreaRendererInfo.Y = cri.YAxisRendererInfo.InnerRect.Y;
            cri.PlotAreaRendererInfo.Width = cri.XAxisRendererInfo.Width;
            cri.PlotAreaRendererInfo.Height = cri.YAxisRendererInfo.InnerRect.Height;

            // Calculated remaining plot area, now it’s safe to format.
            PlotAreaRenderer renderer;
            if (cri.AreaSeriesRendererInfos != null)
            {
                cri.SeriesRendererInfos = cri.AreaSeriesRendererInfos;
                renderer = new AreaPlotAreaRenderer(_rendererParms);
                renderer.Format();
            }
            if (cri.ColumnSeriesRendererInfos != null)
            {
                cri.SeriesRendererInfos = cri.ColumnSeriesRendererInfos;
                //TODO Check for Clustered- or StackedPlotAreaRenderer
                renderer = new ColumnClusteredPlotAreaRenderer(_rendererParms);
                renderer.Format();
            }
            if (cri.LineSeriesRendererInfos != null)
            {
                cri.SeriesRendererInfos = cri.LineSeriesRendererInfos;
                renderer = new LinePlotAreaRenderer(_rendererParms);
                renderer.Format();
            }

            // Draw data labels.
            if (cri.ColumnSeriesRendererInfos != null)
            {
                cri.SeriesRendererInfos = cri.ColumnSeriesRendererInfos;
                DataLabelRenderer dlr = new ColumnDataLabelRenderer(_rendererParms);
                dlr.Format();
            }
        }

        /// <summary>
        /// Draws the column chart.
        /// </summary>
        internal override void Draw()
        {
            var cri = (CombinationRendererInfo)_rendererParms.RendererInfo;
            cri.SeriesRendererInfos = cri.CommonSeriesRendererInfos;

            LegendRenderer lr = new ColumnLikeLegendRenderer(_rendererParms);
            lr.Draw();

            var wr = new WallRenderer(_rendererParms);
            wr.Draw();

            GridlinesRenderer glr = new ColumnLikeGridlinesRenderer(_rendererParms);
            glr.Draw();

            var pabr = new PlotAreaBorderRenderer(_rendererParms);
            pabr.Draw();

            PlotAreaRenderer renderer;
            if (cri.AreaSeriesRendererInfos != null)
            {
                cri.SeriesRendererInfos = cri.AreaSeriesRendererInfos;
                renderer = new AreaPlotAreaRenderer(_rendererParms);
                renderer.Draw();
            }
            if (cri.ColumnSeriesRendererInfos != null)
            {
                cri.SeriesRendererInfos = cri.ColumnSeriesRendererInfos;
                //TODO Check for Clustered- or StackedPlotAreaRenderer
                renderer = new ColumnClusteredPlotAreaRenderer(_rendererParms);
                renderer.Draw();
            }
            if (cri.LineSeriesRendererInfos != null)
            {
                cri.SeriesRendererInfos = cri.LineSeriesRendererInfos;
                renderer = new LinePlotAreaRenderer(_rendererParms);
                renderer.Draw();
            }

            // Draw data labels.
            if (cri.ColumnSeriesRendererInfos != null)
            {
                cri.SeriesRendererInfos = cri.ColumnSeriesRendererInfos;
                DataLabelRenderer dlr = new ColumnDataLabelRenderer(_rendererParms);
                dlr.Draw();
            }

            // Draw axes.
            cri.SeriesRendererInfos = cri.CommonSeriesRendererInfos;
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
        /// Initializes all necessary data to draw series for a combination chart.
        /// </summary>
        void InitSeriesRendererInfo()
        {
            var cri = (CombinationRendererInfo)_rendererParms.RendererInfo;
            var seriesColl = cri.Chart.SeriesCollection;
            cri.SeriesRendererInfos = new SeriesRendererInfo[seriesColl.Count];
            for (int idx = 0; idx < seriesColl.Count; idx++)
            {
                SeriesRendererInfo sri = new SeriesRendererInfo();
                sri.Series = seriesColl[idx];
                cri.SeriesRendererInfos[idx] = sri;
            }
        }

        /// <summary>
        /// Sort all series renderer info dependent on their chart type.
        /// </summary>
        void DistributeSeries()
        {
            var cri = (CombinationRendererInfo)_rendererParms.RendererInfo;

            var areaSeries = new List<SeriesRendererInfo>();
            var columnSeries = new List<SeriesRendererInfo>();
            var lineSeries = new List<SeriesRendererInfo>();
            foreach (var sri in cri.SeriesRendererInfos)
            {
                switch (sri.Series._chartType)
                {
                    case ChartType.Area2D:
                        areaSeries.Add(sri);
                        break;

                    case ChartType.Column2D:
                        columnSeries.Add(sri);
                        break;

                    case ChartType.Line:
                        lineSeries.Add(sri);
                        break;

                    default:
                        throw new InvalidOperationException(PSCSR.InvalidChartTypeForCombination(sri.Series._chartType));
                }
            }

            cri.CommonSeriesRendererInfos = cri.SeriesRendererInfos;
            if (areaSeries.Count > 0)
            {
                cri.AreaSeriesRendererInfos = new SeriesRendererInfo[areaSeries.Count];
                areaSeries.CopyTo(cri.AreaSeriesRendererInfos);
            }
            if (columnSeries.Count > 0)
            {
                cri.ColumnSeriesRendererInfos = new SeriesRendererInfo[columnSeries.Count];
                columnSeries.CopyTo(cri.ColumnSeriesRendererInfos);
            }
            if (lineSeries.Count > 0)
            {
                cri.LineSeriesRendererInfos = new SeriesRendererInfo[lineSeries.Count];
                lineSeries.CopyTo(cri.LineSeriesRendererInfos);
            }
        }
    }
}
