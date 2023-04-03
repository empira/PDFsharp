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
    /// This class is used as a handle for restoring the context.
    /// </summary>
    public sealed class XGraphicsState
    {
        // This class is simply a wrapper of InternalGraphicsState.
#if CORE
        internal XGraphicsState()
        { }
#endif
#if GDI
        internal XGraphicsState(GraphicsState? state)
        {
            GdiState = state;
        }
        internal GraphicsState? GdiState;
#endif
#if WPF
        internal XGraphicsState()
        { }
#endif
        internal InternalGraphicsState InternalState = null!; // NRT
    }
}
