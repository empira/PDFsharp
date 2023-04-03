// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.Rendering
{
    static class ColorHelper
    {
        /// <summary>
        /// Converts Color to XColor.
        /// </summary>
        public static XColor ToXColor(Color color, bool cmyk)
        {
            if (color.IsEmpty)
                return XColor.Empty;

            if (cmyk)
                return XColor.FromCmyk(color.Alpha / 100.0, color.C / 100.0, color.M / 100.0, color.Y / 100.0, color.K / 100.0);
            return XColor.FromArgb((int)color.Argb);
        }
    }
}
