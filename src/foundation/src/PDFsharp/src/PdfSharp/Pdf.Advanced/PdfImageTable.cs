// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Drawing.Internal;
using System.Runtime.InteropServices;
using System.Text;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Contains all used images of a document.
    /// </summary>
    sealed class PdfImageTable : PdfResourceTable
    {
        /// <summary>
        /// Initializes a new instance of this class, which is a singleton for each document.
        /// </summary>
        public PdfImageTable(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Gets a PdfImage from an XImage. If no PdfImage already exists, a new one is created.
        /// </summary>
        public PdfImage GetImage(XImage image)
        {
            var selector = image._selector;
            if (selector == null!)
            {
                selector = new ImageSelector(image, Owner.Options);
                image._selector = selector;
            }

            if (!_images.TryGetValue(selector, out var pdfImage))
            {
                pdfImage = new PdfImage(Owner, image);
                //pdfImage.Document = _document;
                Debug.Assert(pdfImage.Owner == Owner);
                _images[selector] = pdfImage;
            }
            return pdfImage;
        }

        /// <summary>
        /// Map from ImageSelector to PdfImage.
        /// </summary>
        readonly Dictionary<ImageSelector, PdfImage> _images = new();

        /// <summary>
        /// A collection of information that uniquely identifies a particular PdfImage.
        /// </summary>
        public class ImageSelector
        {
            /// <summary>
            /// Initializes a new instance of ImageSelector from an XImage.
            /// </summary>
            public ImageSelector(XImage image, PdfDocumentOptions options)
            {
                // Priority list for current implementation:
                // 1. If we have an _importedImage, we calculate a hash for that image.
                // 2. If we have a filename, we use that filename.
                // 3. Otherwise, create a GUID.
                // TODO: Create hashes also for other image sources.
#if true
                if (image._path == null! || image._importedImage != null)
                {
                    if (image._importedImage != null)
                    {
                        var iid = image._importedImage.ImageData(options);
                        if (iid is ImageDataDct jpeg)
                        {
                            var md5 = System.Security.Cryptography.MD5.Create();
                            var hash = md5.ComputeHash(jpeg.Data, 0, jpeg.Length);
                            image._path = "*md5:" + HashToString(hash);
                        }
                        else if (iid is ImageDataBitmap bmp)
                        {
                            var md5 = System.Security.Cryptography.MD5.Create();
                            var hash = md5.ComputeHash(bmp.Data, 0, bmp.Length);
                            image._path = "*md5:" + HashToString(hash);
                        }
                        else
                            image._path = "*" + Guid.NewGuid().ToString("B");
                    }
                    else if (image._stream != null!)
                    {
                        if (image._stream is MemoryStream ms)
                        {
                            var md5 = System.Security.Cryptography.MD5.Create();
                            ms.Position = 0; // Set stream position before calculating the hash.
                            var hash = md5.ComputeHash(ms);
                            image._path = "*md5:" + HashToString(hash);
                        }
                        else
                        {
                            // TODO Use hash code.
                            image._path = "*" + Guid.NewGuid().ToString("B");
                        }
                    }
#if GDI
                    else if (image._gdiImage != null!)
                    {
                        // TODO Use hash code.
                        image._path = "*" + Guid.NewGuid().ToString("B");
                    }
#endif
#if WPF
                    else if (image.Memory != null)
                    {
                        // TODO Use hash code.
                        image._path = "*" + Guid.NewGuid().ToString("B");
                    }
                    else if (image._wpfImage != null!)
                    {
                        // TODO Use hash code.
                        image._path = "*" + Guid.NewGuid().ToString("B");
                    }
#endif
                    else
                    {
                        // TODO Use hash code.
                        image._path = "*" + Guid.NewGuid().ToString("B");
                    }
                }

                // Use full path plus value of interpolate to identify an image.
                // Path must be at start because '*' identifies pseudo path names.
                // We assume a case-insensitive filesystem under Windows.
#if NET462
                Path = image._path.ToLowerInvariant() + '|' + image.Interpolate;
#else
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    Path = image._path.ToLowerInvariant() + '|' + image.Interpolate;
                else
                    Path = image._path + '|' + image.Interpolate;
#endif
#else
                // HACK: implement a way to identify images when they are reused
                // TODO 4STLA Implementation that calculates MD5 hashes for images generated for the images can be found here: http://forum.pdfsharp.net/viewtopic.php?p=6959#p6959
                if (image._path == null!)
                        image._path = "*" + Guid.NewGuid().ToString("B");
#endif
            }

            private String HashToString(Byte[] hash)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }
                return sb.ToString();
            }

            public string Path { get; }

            public override bool Equals(object? obj)
            {
                if (obj is not ImageSelector selector)
                    return false;
                return Path == selector.Path;
            }

            public override int GetHashCode()
            {
                return Path.GetHashCode();
            }
        }
    }
}
