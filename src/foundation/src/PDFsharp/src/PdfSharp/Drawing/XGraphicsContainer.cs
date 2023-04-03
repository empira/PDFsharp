// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

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
    /// Represents the internal state of an XGraphics object.
    /// </summary>
    public sealed class XGraphicsContainer
    {
#if GDI
        internal XGraphicsContainer(GraphicsState? state)
        {
            GdiState = state;
        }
        internal GraphicsState? GdiState;
#endif
#if WPF
        internal XGraphicsContainer()
        { }
#endif
        internal InternalGraphicsState InternalState = null!; // NRT
    }
}
