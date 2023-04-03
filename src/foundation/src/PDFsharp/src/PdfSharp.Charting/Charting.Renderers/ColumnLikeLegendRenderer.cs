// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;
using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents the legend renderer specific to charts like column, line, or bar.
    /// </summary>
    class ColumnLikeLegendRenderer : LegendRenderer
    {
        /// <summary>
        /// Initializes a new instance of the ColumnLikeLegendRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal ColumnLikeLegendRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Initializes the legend's renderer info. Each data series will be represented through
        /// a legend entry renderer info.
        /// </summary>
        internal override RendererInfo Init()
        {
            LegendRendererInfo? lri = null;
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            if (cri.Chart._legend != null)
            {
                lri = new LegendRendererInfo();
                lri.Legend = cri.Chart._legend;

                lri.Font = Converter.ToXFont(lri.Legend._font, cri.DefaultFont);
                lri.FontColor = new XSolidBrush(XColors.Black);

                if (lri.Legend._lineFormat != null)
                    lri.BorderPen = Converter.ToXPen(lri.Legend._lineFormat, XColors.Black, DefaultLineWidth, XDashStyle.Solid);

                lri.Entries = new LegendEntryRendererInfo[cri.SeriesRendererInfos.Length];
                int index = 0;
                foreach (SeriesRendererInfo sri in cri.SeriesRendererInfos)
                {
                    var leri = new LegendEntryRendererInfo
                    {
                        SeriesRendererInfo = sri,
                        LegendRendererInfo = lri,
                        EntryText = sri.Series.Name
                    };
                    if (sri.MarkerRendererInfo != null!)
                    {
                        leri.MarkerSize.Width = leri.MarkerSize.Height = sri.MarkerRendererInfo.MarkerSize.Point;
                        leri.MarkerPen = new XPen(sri.MarkerRendererInfo.MarkerForegroundColor);
                        leri.MarkerBrush = new XSolidBrush(sri.MarkerRendererInfo.MarkerBackgroundColor);
                    }
                    else
                    {
                        leri.MarkerPen = sri.LineFormat;
                        leri.MarkerBrush = sri.FillFormat;
                    }

                    if (cri.Chart._type == ChartType.ColumnStacked2D)
                        // Stacked columns are in reverse order.
                        lri.Entries[cri.SeriesRendererInfos.Length - index++ - 1] = leri;
                    else
                        lri.Entries[index++] = leri;
                }
            }
            return lri!; // ?? NRT.ThrowOnNull<LegendRendererInfo>();
        }
    }
}
