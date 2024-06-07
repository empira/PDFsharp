// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.Rendering.Extensions;
using PdfSharp.Charting;
using PdfSharp.Drawing;

namespace MigraDoc.Rendering.ChartMapper
{
    /// <summary>
    /// The LineFormatMapper class.
    /// </summary>
    public class LineFormatMapper
    {
        void MapObject(LineFormat lineFormat, DocumentObjectModel.Shapes.LineFormat domLineFormat)
        {
            if (domLineFormat.Color.IsEmpty)
                lineFormat.Color = XColor.Empty;
            else
            {
#if noCMYK
                lineFormat.Color = XColor.FromArgb(domLineFormat.Color.Argb);
#else
                Debug.Assert(domLineFormat.Document != null, "domLineFormat.Document != null");
                lineFormat.Color = ColorHelper.ToXColor(domLineFormat.Color, domLineFormat.Document.UseCmykColor);
#endif
            }

            lineFormat.DashStyle = domLineFormat.DashStyle switch
            {
                DocumentObjectModel.Shapes.DashStyle.Dash => XDashStyle.Dash,
                DocumentObjectModel.Shapes.DashStyle.DashDot => XDashStyle.DashDot,
                DocumentObjectModel.Shapes.DashStyle.DashDotDot => XDashStyle.DashDotDot,
                DocumentObjectModel.Shapes.DashStyle.Solid => XDashStyle.Solid,
                DocumentObjectModel.Shapes.DashStyle.SquareDot => XDashStyle.Dot,
                _ => XDashStyle.Solid
            };
            lineFormat.Style = domLineFormat.Style switch
            {
                DocumentObjectModel.Shapes.LineStyle.Single => LineStyle.Single,
                _ => lineFormat.Style
            };
            lineFormat.Visible = domLineFormat.Visible;
            //if (domLineFormat.IsNull("Visible"))
            if (domLineFormat.Values.Visible is null)
                lineFormat.Visible = true;
            lineFormat.Width = domLineFormat.Width.ToXUnit();
        }

        internal static void Map(LineFormat lineFormat, DocumentObjectModel.Shapes.LineFormat domLineFormat)
        {
            var mapper = new LineFormatMapper();
            mapper.MapObject(lineFormat, domLineFormat);
        }
    }
}
