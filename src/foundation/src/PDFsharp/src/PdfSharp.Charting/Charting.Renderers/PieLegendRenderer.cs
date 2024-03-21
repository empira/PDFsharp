// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Globalization;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents the legend renderer specific to pie charts.
    /// </summary>
    class PieLegendRenderer : LegendRenderer
    {
        /// <summary>
        /// Initializes a new instance of the PieLegendRenderer class with the specified renderer
        /// parameters.
        /// </summary>
        internal PieLegendRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Initializes the legend's renderer info. Each data point will be represented through
        /// a legend entry renderer info.
        /// </summary>
        internal override RendererInfo? Init()
        {
            LegendRendererInfo? lri = null;
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            if (cri.Chart._legend != null)
            {
                lri = new LegendRendererInfo
                {
                    Legend = cri.Chart._legend
                };

                lri.Font = Converter.ToXFont(lri.Legend._font, cri.DefaultFont);
                lri.FontColor = new XSolidBrush(XColors.Black);

                if (lri.Legend._lineFormat != null)
                    lri.BorderPen = Converter.ToXPen(lri.Legend._lineFormat, XColors.Black, DefaultLineWidth, XDashStyle.Solid);

                XSeries? xseries = null;
                if (cri.Chart._xValues != null)
                    xseries = cri.Chart._xValues[0];

                int index = 0;
                var sri = cri.SeriesRendererInfos[0];
                lri.Entries = new LegendEntryRendererInfo[sri.PointRendererInfos.Length];
                foreach (var pri in sri.PointRendererInfos)
                {
                    var leri = new LegendEntryRendererInfo
                    {
                        SeriesRendererInfo = sri,
                        LegendRendererInfo = lri,
                        EntryText = ""
                    };
                    if (xseries != null)
                    {
                        if (xseries.Count > index)
                            leri.EntryText = xseries[index]?.ValueField ?? NRT.ThrowOnNull<string>();
                    }
                    else
                        leri.EntryText = (index + 1).ToString(CultureInfo.InvariantCulture); // create default/dummy entry
                    leri.MarkerPen = pri.LineFormat;
                    leri.MarkerBrush = pri.FillFormat;

                    lri.Entries[index++] = leri;
                }
            }
            return lri;
        }
    }
}
