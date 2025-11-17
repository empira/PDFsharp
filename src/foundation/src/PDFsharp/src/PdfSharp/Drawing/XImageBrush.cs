// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.Windows.Media;
#endif

namespace PdfSharp.Drawing
{
    public sealed class XImageBrush : XBrush, IDisposable
    {

        /// <summary>
        /// Creates an image from the specified file.
        /// </summary>
        /// <param name="path">The path to a BMP, PNG, GIF, JPEG, TIFF, or PDF file.</param>
        public static XImageBrush FromFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            var image = XImage.FromFile(path);
            if (image == null)
                throw new NullReferenceException(nameof(image));

            return new XImageBrush(image);
        }

        /// <summary>
        /// Creates an image from the specified stream.<br/>
        /// </summary>
        /// <param name="stream">The stream containing a BMP, PNG, GIF, JPEG, TIFF, or PDF file.</param>
        public static XImageBrush FromStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var image = XImage.FromStream(stream);
            if (image == null)
                throw new NullReferenceException(nameof(image));

            return new XImageBrush(image);
        }


        internal XImage _xImage;
        XImageBrush(XImage xImage)
        {
            _xImage = xImage;
        }

        public void Dispose()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            _xImage?.Dispose();
            _xImage = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#if WPF
            _imageBrush = null;
#elif GDI
            _textureBrush?.Dispose();
            _textureBrush = null;
#endif
        }

#if GDI
        TextureBrush? _textureBrush;
        internal override Brush RealizeGdiBrush()
        {
            if (_textureBrush == null)
            {
                // Create a GDI+ image.
                try
                {
                    Lock.EnterGdiPlus();
                    _textureBrush = new TextureBrush(_xImage._gdiImage);
                }
                finally
                {
                    Lock.ExitGdiPlus();
                }

                if (_textureBrush == null)
                    throw new NullReferenceException(nameof(_textureBrush));
            }

            return _textureBrush;
        }
#endif
#if WPF
        ImageBrush? _imageBrush;
        internal override Brush RealizeWpfBrush()
        {
            if (_imageBrush == null)
            {
                // Create a WPF BitmapImage.
                _imageBrush = new ImageBrush(_xImage._wpfImage);

                if (_imageBrush == null)
                    throw new NullReferenceException(nameof(_imageBrush));
            }

            return _imageBrush;
        }

#endif

#if WUI
        internal override ICanvasBrush RealizeCanvasBrush()
        {
            throw new NotImplementedException();
        }
#endif
    }
}
