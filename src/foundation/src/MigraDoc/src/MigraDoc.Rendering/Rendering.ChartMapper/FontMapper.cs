// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;

using Font = PdfSharp.Charting.Font;
using Underline = PdfSharp.Charting.Underline;

namespace MigraDoc.Rendering.ChartMapper
{
    class FontMapper
    {
        void MapObject(Font font, DocumentObjectModel.Font domFont)
        {
            font.Bold = domFont.Bold;
            if (domFont.Color.IsEmpty)
                font.Color = XColor.Empty;
            else
            {
#if noCMYK
                font.Color = XColor.FromArgb((int)domFont.Color.Argb);
#else
                Debug.Assert(domFont.Document != null, "domFont.Document != null");
                font.Color = ColorHelper.ToXColor(domFont.Color, domFont.Document.UseCmykColor);
#endif
            }
            font.Italic = domFont.Italic;
            if (!domFont.Values.Name.IsValueNullOrEmpty())
                font.Name = domFont.Name;
            //if (!domFont.IsNull("Size"))
            if (!domFont.Values.Size.IsValueNullOrEmpty())
                font.Size = domFont.Size.Point;
            font.Subscript = domFont.Subscript;
            font.Superscript = domFont.Superscript;
            font.Underline = (Underline)domFont.Underline;
        }

        internal static void Map(Font font, DocumentObjectModel.Document domDocument, string domStyleName)
        {
            var domStyle = domDocument.Styles[domStyleName];
            if (domStyle != null)
            {
                var mapper = new FontMapper();
                mapper.MapObject(font, domStyle.Font);
            }
        }

        internal static void Map(Font font, DocumentObjectModel.Font domFont)
        {
            var mapper = new FontMapper();
            mapper.MapObject(font, domFont);
        }
    }
}
