﻿// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Helper class for processing image files.
    /// </summary>
    static class ImageHelper
    {
#if WPF && GDI  // KEEP for reference
        /// <summary>
        /// Creates a WPF bitmap source from an GDI image.
        /// </summary>
        public static BitmapSource CreateBitmapSource(Image image)
        {
            MemoryStream stream = new MemoryStream();
            //int width = image.Width;
            //int height = image.Height;
            //double dpiX = image.HorizontalResolution;
            //double dpiY = image.VerticalResolution;
            //System.Windows.Media.PixelFormat pixelformat = PixelFormats.Default;
            BitmapSource bitmapSource = null;

            try
            {
                string guid = image.RawFormat.Guid.ToString("B").ToUpper();
                switch (guid)
                {
                    case "{B96B3CAA-0728-11D3-9D7B-0000F81EF32E}":  // memoryBMP
                    case "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}":  // bmp
                        image.Save(stream, ImageFormat.Bmp);
                        stream.Position = 0;
                        BmpBitmapDecoder bmpDecoder = new BmpBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                        bitmapSource = bmpDecoder.Frames[0];
                        break;

                    case "{B96B3CAF-0728-11D3-9D7B-0000F81EF32E}":  // png
                        image.Save(stream, ImageFormat.Png);
                        stream.Position = 0;
                        PngBitmapDecoder pngDecoder = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                        bitmapSource = pngDecoder.Frames[0];
                        break;

                    case "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}":  // jpeg
                        image.Save(stream, ImageFormat.Jpeg);
                        JpegBitmapDecoder jpegDecoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                        stream.Position = 0;
                        bitmapSource = jpegDecoder.Frames[0];
                        break;

                    case "{B96B3CB0-0728-11D3-9D7B-0000F81EF32E}":  // gif
                        image.Save(stream, ImageFormat.Gif);
                        GifBitmapDecoder gifDecoder = new GifBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                        stream.Position = 0;
                        bitmapSource = gifDecoder.Frames[0];
                        break;

                    case "{B96B3CB1-0728-11D3-9D7B-0000F81EF32E}":  // tiff
                        image.Save(stream, ImageFormat.Tiff);
                        TiffBitmapDecoder tiffDecoder = new TiffBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                        stream.Position = 0;
                        bitmapSource = tiffDecoder.Frames[0];
                        break;

                    case "{B96B3CB5-0728-11D3-9D7B-0000F81EF32E}":  // icon
                        image.Save(stream, ImageFormat.Icon);
                        IconBitmapDecoder iconDecoder = new IconBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                        stream.Position = 0;
                        bitmapSource = iconDecoder.Frames[0];
                        break;

                    case "{B96B3CAC-0728-11D3-9D7B-0000F81EF32E}":  // emf
                    case "{B96B3CAD-0728-11D3-9D7B-0000F81EF32E}":  // wmf
                    case "{B96B3CB2-0728-11D3-9D7B-0000F81EF32E}":  // exif
                    case "{B96B3CB3-0728-11D3-9D7B-0000F81EF32E}":  // photoCD
                    case "{B96B3CB4-0728-11D3-9D7B-0000F81EF32E}":  // flashPIX

                    default:
                        throw new InvalidOperationException("Unsupported image format.");
                }
            }
            catch (Exception ex)
            {
                De/bug.WriteLine("ImageHelper.CreateBitmapSource failed:" + ex.Message);
            }
            finally
            {
                //if (stream != null)
                //  stream.Close();
            }
            return bitmapSource;
        }
#endif
    }
}
