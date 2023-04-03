// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using PdfSharp.Internal;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PdfSharp.Internal;

#if !GDI
//using PdfSharp.Internal;
#endif
#endif
#if UWP
using Windows.UI.Xaml.Media.Imaging;
using PdfSharp.Internal;

#endif

// WPFHACK
#pragma warning disable 0169
#pragma warning disable 0649

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Defines a pixel-based bitmap image.
    /// </summary>
    public sealed class XBitmapImage : XBitmapSource
    {
        // TODO: Move code from XImage to this class.

        /// <summary>
        /// Initializes a new instance of the <see cref="XBitmapImage"/> class.
        /// </summary>
        internal XBitmapImage(int width, int height)
        {
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                // Create a default 24-bit ARGB bitmap.
                _gdiImage = new Bitmap(width, height);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF
            DiagnosticsHelper.ThrowNotImplementedException("CreateBitmap");
#endif
#if UWP
            DiagnosticsHelper.ThrowNotImplementedException("CreateBitmap");
#endif
#if CORE || GDI && !WPF // Prevent unreachable code error
            Initialize();
#endif
        }

        /// <summary>
        /// Creates a default 24-bit ARGB bitmap with the specified pixel size.
        /// </summary>
        public static XBitmapSource CreateBitmap(int width, int height)
        {
            // Create a default 24-bit ARGB bitmap.
            return new XBitmapImage(width, height);
        }
    }
}
