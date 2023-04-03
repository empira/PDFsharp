// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;
using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a data label renderer.
    /// </summary>
    abstract class DataLabelRenderer : Renderer
    {
        /// <summary>
        /// Initializes a new instance of the DataLabelRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal DataLabelRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Creates a data label rendererInfo.
        /// Does not return any renderer info.
        /// </summary>
        internal override RendererInfo Init()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            foreach (var sri in cri.SeriesRendererInfos)
            {
                DataLabelRendererInfo dlri = new DataLabelRendererInfo();
                if (cri.Chart.HasDataLabel || cri.Chart._dataLabel != null ||
                    sri.Series.HasDataLabel || sri.Series._dataLabel != null)
                {
                    var dl = sri.Series._dataLabel;
                    if (dl == null)
                        dl = cri.Chart._dataLabel;
                    if (dl == null)
                    {
                        dlri.Format = "0";
                        dlri.Font = cri.DefaultDataLabelFont;
                        dlri.FontColor = new XSolidBrush(XColors.Black);
                        dlri.Position = DataLabelPosition.InsideEnd;
                        if (cri.Chart._type == ChartType.Pie2D || cri.Chart._type == ChartType.PieExploded2D)
                            dlri.Type = DataLabelType.Percent;
                        else
                            dlri.Type = DataLabelType.Value;
                    }
                    else
                    {
                        dlri.Format = dl.Format.Length > 0 ? dl.Format : "0";
                        dlri.Font = Converter.ToXFont(dl._font, cri.DefaultDataLabelFont);
                        dlri.FontColor = Converter.ToXBrush(dl._font, XColors.Black);
                        if (dl._positionInitialized)
                            dlri.Position = dl._position;
                        else
                            dlri.Position = DataLabelPosition.OutsideEnd;
                        if (dl._typeInitialized)
                            dlri.Type = dl._type;
                        else
                        {
                            if (cri.Chart._type == ChartType.Pie2D || cri.Chart._type == ChartType.PieExploded2D)
                                dlri.Type = DataLabelType.Percent;
                            else
                                dlri.Type = DataLabelType.Value;
                        }
                    }

                    sri.DataLabelRendererInfo = dlri;
                }
            }
            return null!; // ?? NRT.ThrowOnNull<RendererInfo>();
        }

        /// <summary>
        /// Calculates the specific positions for each data label.
        /// </summary>
        internal abstract void CalcPositions();
    }
}
