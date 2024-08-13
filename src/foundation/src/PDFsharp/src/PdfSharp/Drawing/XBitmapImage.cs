// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using PdfSharp.Internal;
#endif
#if WPF
using PdfSharp.Internal;
#endif
#if WUI
using Windows.UI.Xaml.Media.Imaging;
using PdfSharp.Internal;
#endif

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
#if WUI
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
