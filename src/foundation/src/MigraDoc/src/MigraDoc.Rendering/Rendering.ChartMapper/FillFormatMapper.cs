// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using PdfSharp.Charting;
using PdfSharp.Drawing;

namespace MigraDoc.Rendering.ChartMapper
{
    class FillFormatMapper
    {
        void MapObject(FillFormat fillFormat, DocumentObjectModel.Shapes.FillFormat domFillFormat)
        {
            if (domFillFormat.Color.IsEmpty)
                fillFormat.Color = XColor.Empty;
            else
            {
#if noCMYK
                fillFormat.Color = XColor.FromArgb((int)domFillFormat.Color.Argb);
#else
                Debug.Assert(domFillFormat.Document != null, "domFillFormat.Document != null");
                fillFormat.Color = ColorHelper.ToXColor(domFillFormat.Color, domFillFormat.Document.UseCmykColor);
#endif
            }
            fillFormat.Visible = domFillFormat.Visible;
        }

        internal static void Map(FillFormat fillFormat, DocumentObjectModel.Shapes.FillFormat domFillFormat)
        {
            var mapper = new FillFormatMapper();
            mapper.MapObject(fillFormat, domFillFormat);
        }
    }
}
