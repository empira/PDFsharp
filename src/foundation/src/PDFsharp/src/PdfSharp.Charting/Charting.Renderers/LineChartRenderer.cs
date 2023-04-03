// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a line chart renderer.
    /// </summary>
    class LineChartRenderer : ColumnLikeChartRenderer
    {
        /// <summary>
        /// Initializes a new instance of the LineChartRenderer class with the specified renderer parameters.
        /// </summary>
        internal LineChartRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Returns an initialized renderer-specific rendererInfo.
        /// </summary>
        internal override RendererInfo Init()
        {
            var cri = new ChartRendererInfo();
            cri.Chart = (Chart)_rendererParms.DrawingItem;
            _rendererParms.RendererInfo = cri;

            InitSeriesRendererInfo();

            LegendRenderer lr = new ColumnLikeLegendRenderer(_rendererParms);
            cri.LegendRendererInfo = (LegendRendererInfo?)lr.Init();

            AxisRenderer xar = new HorizontalXAxisRenderer(_rendererParms);
            cri.XAxisRendererInfo = (AxisRendererInfo?)xar.Init();

            AxisRenderer yar = new VerticalYAxisRenderer(_rendererParms);
            cri.YAxisRendererInfo = (AxisRendererInfo?)yar.Init();

            PlotArea plotArea = cri.Chart.PlotArea;
            LinePlotAreaRenderer lpar = new LinePlotAreaRenderer(_rendererParms);
            cri.PlotAreaRendererInfo = (PlotAreaRendererInfo)lpar.Init();

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

            // axes
            AxisRenderer xar = new HorizontalXAxisRenderer(_rendererParms);
            xar.Format();

            AxisRenderer yar = new VerticalYAxisRenderer(_rendererParms);
            yar.Format();

            // Calculate rects and positions.
            CalcLayout();

            // Calculated remaining plot area, now it's safe to format.
            LinePlotAreaRenderer lpar = new LinePlotAreaRenderer(_rendererParms);
            lpar.Format();
        }

        /// <summary>
        /// Draws the line chart.
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

            PlotAreaBorderRenderer pabr = new PlotAreaBorderRenderer(_rendererParms);
            pabr.Draw();

            // Draw line chart's plot area.
            LinePlotAreaRenderer lpar = new LinePlotAreaRenderer(_rendererParms);
            lpar.Draw();

            // Draw x- and y-axis.
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
        /// Initializes all necessary data to draw a series for a line chart.
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
        /// Initializes all necessary data to draw a series for a line chart.
        /// </summary>
        internal void InitSeries()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            int seriesIndex = 0;
            foreach (var sri in cri.SeriesRendererInfos)
            {
                if (sri.Series.MarkerBackgroundColor.IsEmpty)
                    sri.LineFormat = Converter.ToXPen(sri.Series._lineFormat, LineColors.Item(seriesIndex), ChartRenderer.DefaultSeriesLineWidth);
                else
                    sri.LineFormat = Converter.ToXPen(sri.Series._lineFormat, sri.Series.MarkerBackgroundColor, ChartRenderer.DefaultSeriesLineWidth);
                sri.LineFormat.LineJoin = XLineJoin.Bevel;

                MarkerRendererInfo mri = new MarkerRendererInfo();
                sri.MarkerRendererInfo = mri;

                mri.MarkerForegroundColor = sri.Series.MarkerForegroundColor;
                if (mri.MarkerForegroundColor.IsEmpty)
                    mri.MarkerForegroundColor = XColors.Black;

                mri.MarkerBackgroundColor = sri.Series.MarkerBackgroundColor;
                if (mri.MarkerBackgroundColor.IsEmpty)
                    mri.MarkerBackgroundColor = sri.LineFormat.Color;

                mri.MarkerSize = sri.Series.MarkerSize;
                if (mri.MarkerSize == 0)
                    mri.MarkerSize = 7;

                if (!sri.Series._markerStyleInitialized)
                    //mri.MarkerStyle = (MarkerStyle)(seriesIndex % (Enum.GetNames(typeof(MarkerStyle)).Length - 1) + 1);
                    mri.MarkerStyle = (MarkerStyle)(seriesIndex % (10 - 1) + 1);
                else
                    mri.MarkerStyle = sri.Series._markerStyle;

                ++seriesIndex;
            }
        }
    }
}
