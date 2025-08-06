// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if CORE
#endif
#if GDI
using PdfSharp.Internal;
#endif
#if WPF
#endif
#if WUI
using Windows.UI.Xaml.Media.Imaging;
#endif

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Defines an abstract base class for pixel-based images.
    /// </summary>
    public abstract class XBitmapSource : XImage
    {
        // TODO_OLD: Move code from XImage to this class.

        /// <summary>
        /// Gets the width of the image in pixels.
        /// </summary>
        public override int PixelWidth
        {
            get
            {
#if CORE
                if (_importedImage != null)
                    return (int)_importedImage.Information.Width;
                return 100;
#endif
#if GDI && !WPF
                try
                {
                    Lock.EnterGdiPlus();
                    return _gdiImage.Width;
                }
                finally { Lock.ExitGdiPlus(); }
#endif
#if GDI && WPF
                int gdiWidth = _gdiImage.Width;
                int wpfWidth = _wpfImage.PixelWidth;
                Debug.Assert(gdiWidth == wpfWidth);
                return wpfWidth;
#endif
#if WPF && !GDI
                return _wpfImage.PixelWidth;
#endif
#if WUI
                return _wrtImage.PixelWidth;
#endif
            }
        }

        /// <summary>
        /// Gets the height of the image in pixels.
        /// </summary>
        public override int PixelHeight
        {
            get
            {
#if CORE
                if (_importedImage != null)
                    return (int)_importedImage.Information.Height;
                return 100;
#endif
#if GDI && !WPF
                try
                {
                    Lock.EnterGdiPlus();
                    return _gdiImage.Height;
                }
                finally { Lock.ExitGdiPlus(); }
#endif
#if GDI && WPF
                int gdiHeight = _gdiImage.Height;
                int wpfHeight = _wpfImage.PixelHeight;
                Debug.Assert(gdiHeight == wpfHeight);
                return wpfHeight;
#endif
#if WPF && !GDI
                return _wpfImage.PixelHeight;
#endif
#if WUI
                return _wrtImage.PixelHeight;
#endif
            }
        }
    }
}
