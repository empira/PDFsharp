// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

#if PSGFX
#else
using PdfSharp.Drawing;
#endif
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.Rendering
{
    static class ColorHelper
    {
        /// <summary>
        /// Converts Color to XColor.
        /// </summary>
#if PSGFX
        public static XColor ToXColor(Color color, bool cmyk)
        {
            return XColor.FromArgb((byte)color.A, (byte)color.R, (byte)color.G, (byte)color.B);
        }
#else
        public static XColor ToXColor(Color color, bool cmyk)
        {
            if (color.IsEmpty)
                return XColor.Empty;

            if (cmyk)
                return XColor.FromCmyk(color.Alpha / 100.0, color.C / 100.0, color.M / 100.0, color.Y / 100.0, color.K / 100.0);
            return XColor.FromArgb((uint)color.Argb);
        }
#endif
    }
}
