// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if CORE
#endif
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows.Media;
#endif

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Converts XGraphics enums to GDI+ enums.
    /// </summary>
    static class XConvert
    {
#if GDI
        /// <summary>
        /// Converts XLineJoin to LineJoin.
        /// </summary>
        public static LineJoin ToLineJoin(XLineJoin lineJoin)
        {
            return GdiLineJoin[(int)lineJoin];
        }
        static readonly LineJoin[] GdiLineJoin = [LineJoin.Miter, LineJoin.Round, LineJoin.Bevel];
#endif

#if GDI
        /// <summary>
        /// Converts XLineCap to LineCap.
        /// </summary>
        public static LineCap ToLineCap(XLineCap lineCap)
        {
            return GdiLineCap[(int)lineCap];
        }
        static readonly LineCap[] GdiLineCap = [LineCap.Flat, LineCap.Round, LineCap.Square];
#endif

#if WPF
        /// <summary>
        /// Converts XLineJoin to PenLineJoin.
        /// </summary>
        public static PenLineJoin ToPenLineJoin(XLineJoin lineJoin)
        {
            return WpfLineJoin[(int)lineJoin];
        }
        static readonly PenLineJoin[] WpfLineJoin = [PenLineJoin.Miter, PenLineJoin.Round, PenLineJoin.Bevel];
#endif

#if WPF
        /// <summary>
        /// Converts XLineCap to PenLineCap.
        /// </summary>
        public static PenLineCap ToPenLineCap(XLineCap lineCap)
        {
            return WpfLineCap[(int)lineCap];
        }
        static readonly PenLineCap[] WpfLineCap = [PenLineCap.Flat, PenLineCap.Round, PenLineCap.Square];
#endif
    }
}