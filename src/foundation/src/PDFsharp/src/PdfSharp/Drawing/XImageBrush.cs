// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

#if GDI
using PdfSharp.Internal;
#endif

namespace PdfSharp.Drawing
{
    public sealed class XImageBrush : XBrush
    {

#if GDI || WPF || CORE
        /// <summary>
        /// Creates an image from the specified file.
        /// </summary>
        /// <param name="path">The path to a BMP, PNG, GIF, JPEG, TIFF file.</param>
        public static XImageBrush FromFile(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            path = Path.GetFullPath(path);
            if (!File.Exists(path))
                throw new FileNotFoundException(PSSR.FileNotFound(path));

            return new XImageBrush(path);
        }

        /// <summary>
        /// Creates an image from the specified stream.<br/>
        /// </summary>
        /// <param name="stream">The stream containing a BMP, PNG, GIF, JPEG, TIFF file.</param>
        public static XImageBrush FromStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return new XImageBrush(stream);
        }
#else
        /// <summary>
        /// Creates an image from the specified file.
        /// </summary>
        /// <param name="path">The path to a BMP, PNG, GIF, JPEG, TIFF file.</param>
        public static XImageBrush FromFile(string path)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Creates an image from the specified stream.<br/>
        /// </summary>
        /// <param name="stream">The stream containing a BMP, PNG, GIF, JPEG, TIFF file.</param>
        public static XImageBrush FromStream(Stream stream)
        {
            //throw new NotImplementedException();
        }

        XImageBrush()
        {

        }
#endif

        internal XImage _xImage;
        XImageBrush(string path)
        {

#if GDI
            try
            {
                Lock.EnterGdiPlus();
                Image _gdiImage = Image.FromFile(path);
                _textureBrush = new TextureBrush(_gdiImage);
            }
            finally { Lock.ExitGdiPlus(); }

#elif WPF
            var wpfImage = BitmapFromUri(new Uri(path));
            _imageBrush = new ImageBrush(wpfImage);

#endif
            _xImage = XImage.FromFile(path);
        }

        XImageBrush(Stream stream) {

#if GDI
            // Create a GDI+ image.
            try
            {
                Lock.EnterGdiPlus();
                Image _gdiImage = Image.FromStream(stream);
                _textureBrush = new TextureBrush(_gdiImage);
            }
            finally { Lock.ExitGdiPlus(); }
#elif WPF
            // Create a WPF BitmapImage.
            BitmapImage bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.StreamSource = stream;
            bmi.EndInit();
            _imageBrush = new ImageBrush(bmi);
#endif
            _xImage = XImage.FromStream(stream);
        }


#if GDI
        TextureBrush _textureBrush;
        internal override Brush RealizeGdiBrush() => _textureBrush;
#endif

#if WPF
        ImageBrush _imageBrush;
        internal override Brush RealizeWpfBrush() => _imageBrush;

        /// <summary>
        /// Creates an BitmapImage from URI. Sets BitmapCacheOption.OnLoad for WPF to prevent image file from being locked.
        /// </summary>
        public static BitmapImage BitmapFromUri(Uri uri)
        {
#if true
            // Using new BitmapImage(uri) will leave a lock on the file, leading to problems with temporary image files in server environments.
            // We use BitmapCacheOption.OnLoad to prevent this lock.
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
#else
            return new BitmapImage(uri);
#endif
        }
#endif


#if UWP
        internal override ICanvasBrush RealizeCanvasBrush()
        {
            throw new NotImplementedException();
        }
#endif
    }
}
