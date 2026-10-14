// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Runtime.InteropServices;
using PdfSharp.Drawing;
#if CORE
using PdfSharp.Internal.Imaging;
#endif
using System.Security.Cryptography;
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

#if CORE
        public PdfImage GetImage(ImportedImage importedImage)
        {
            byte[] bytes = importedImage.ImageData;

            var selector = new ImageSelector(bytes);

            if (!_images.TryGetValue(selector, out var pdfImage))
            {
                pdfImage = new(Owner, importedImage);
                Debug.Assert(pdfImage.Owner == Owner);
                _images[selector] = pdfImage;
            }
            return pdfImage;
        }
#endif

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
                // TODO_OLD: Create hashes also for other image sources.
                var selector = image._path;
                if (image._path == null! || image._importedImage != null)
                {
                    if (image._importedImage != null)
                    {
                        var iid2 = image._importedImage.ImageData;
                        var hashCreator = GetHashCreator();
                        var hash = hashCreator.ComputeHash(iid2, 0, iid2.Length);
                        selector = GetHashSelectorPrefix(image) + HashToString(hash);
                    }
                    else if (image._stream != null!)
                    {
                        if (image._stream is MemoryStream ms)
                        {
                            var hashCreator = GetHashCreator();
                            ms.Position = 0; // Set stream position before calculating the hash.
                            var hash = hashCreator.ComputeHash(ms);
                            selector = GetHashSelectorPrefix(image) + HashToString(hash);
                        }
                        else
                        {
                            // TODO_OLD Use hash code.
                            selector = "*" + Guid.NewGuid().ToString("B");
                        }
                    }
#if GDI
                    else if (image._gdiImage != null!)
                    {
                        // TODO_OLD Use hash code.
                        selector = "*" + Guid.NewGuid().ToString("B");
                    }
#endif
#if WPF
                    else if (image.Memory != null)
                    {
                        // We have a MemoryStream with the image data.
                        var hashCreator = GetHashCreator();
                        image.Memory.Position = 0; // Set stream position before calculating the hash.
                        var hash = hashCreator.ComputeHash(image.Memory);
                        selector = GetHashSelectorPrefix(image) + HashToString(hash);
                    }
                    else if (image._wpfImage != null!)
                    {
                        // TODO_OLD Use hash code.
                        selector = "*" + Guid.NewGuid().ToString("B");
                    }
#endif
                    else
                    {
                        // TODO_OLD Use hash code.
                        selector = "*" + Guid.NewGuid().ToString("B");
                    }
                }

                // Use full path plus value of interpolate to identify an image.
                // Path must be at start because '*' identifies pseudo path names.
                // We assume a case-insensitive filesystem under Windows.
#if NET462
                Path = selector.ToLowerInvariant() + '|' + image.Interpolate;
#else
                if (Capabilities.OperatingSystem.IsWindows)
                    Path = selector.ToLowerInvariant() + '|' + image.Interpolate;
                else
                    Path = selector + '|' + image.Interpolate;
#endif
                // Path must be set. Leading '*' indicates pseudo-paths.
                if (image._path == null!)
                    image._path = Path;

            }

            ///// <summary>
            ///// Initializes a new instance of ImageSelector from an XImage.
            ///// </summary>
            public ImageSelector(byte[] bytes)
            {
                var hashCreator = GetHashCreator();
                var hash = hashCreator.ComputeHash(bytes, 0, bytes.Length);
                var selector = HashToString(hash);

                Path = GetHashSelectorPrefix() + selector;
            }

            /// <summary>
            /// Creates an instance of HashAlgorithm for use in ImageSelector.
            /// </summary>
            HashAlgorithm GetHashCreator()
            {
                // Note hat beginning with PDFsharp 6.2.0 Preview 2, we use SHA1 here.
                // Earlier versions used MD5.
                // MD5 is deprecated and not FIPS compliant, so we decided to avoid MD5 here.
                // SHA1 is not deprecated and similarly efficient as MD5.
                // Note 2: We do not use the hash for cryptography, we only use it to identify
                // images that are used multiple times in a PDF file.
                var x = SHA1.Create();
                return x;
            }

            /// <summary>
            /// Image selectors that are no path names start with an asterisk.
            /// We combine image dimensions with the hashcode of the image bits to reduce chance af ambiguity.
            /// </summary>
            string GetHashSelectorPrefix(XImage image)
            {
                return "*hash:" + image.PixelWidth + ':' + image.PixelHeight + ':';
            }

            /// <summary>
            /// Image selectors that are no path names start with an asterisk.
            /// We combine image dimensions with the hashcode of the image bits to reduce chance af ambiguity.
            /// </summary>
            string GetHashSelectorPrefix()
            {
                return "*hash:";
            }

            string HashToString(byte[] hash)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }
                return sb.ToString();
            }

            public string Path { get; }  // string Selection Key

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
