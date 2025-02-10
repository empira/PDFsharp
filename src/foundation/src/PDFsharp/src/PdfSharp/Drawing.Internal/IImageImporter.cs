// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using PdfSharp.Pdf;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// This interface will be implemented by specialized classes, one for JPEG, one for BMP, one for PNG, one for GIF. Maybe more.
    /// </summary>
    interface IImageImporter
    {
        /// <summary>
        /// Imports the image. Returns null if the image importer does not support the format.
        /// </summary>
        ImportedImage? ImportImage(StreamReaderHelper stream);

        /// <summary>
        /// Prepares the image data needed for the PDF file.
        /// </summary>
        ImageData PrepareImage(ImagePrivateData data);
    }

    /// <summary>
    /// Helper for dealing with Stream data.
    /// </summary>
    class StreamReaderHelper : IDisposable
    {
        internal StreamReaderHelper(byte[] data)
        {
            OriginalStream = null!;
            Data = data;
            Length = data.Length;
        }

        internal StreamReaderHelper(Stream stream, int streamLength)
        {
            OriginalStream = stream;

            if (stream is MemoryStream ms)
            {
                // If the given stream is a MemoryStream, work with it.
                if (ms.TryGetBuffer(out var buffer))
                {
                    // Buffer is accessible - use it.
                    Data = buffer.Array ?? throw new ArgumentNullException(nameof(stream), "Stream has no content byte array.");
                    Length = (int)ms.Length;
                }
                else
                {
                    // Buffer of given stream is not accessible, so read stream into new buffer.
                    OwnedMemoryStream = new(streamLength);
                    stream.CopyTo(OwnedMemoryStream);
                    Data = OwnedMemoryStream.GetBuffer();
                    Length = (int)OwnedMemoryStream.Length;
                    PdfSharpLogHost.Logger.LogWarning("LoadImage: MemoryStream with buffer that is not publicly visible was used. " +
                                                      "For better performance, set 'publiclyVisible' to true when creating the MemoryStream.");
                }
            }
            else
            {
                // If the given stream is not a MemoryStream, copy the stream to a new MemoryStream.
                if (streamLength > -1)
                {
                    // Simple case: length of stream is known, create a MemoryStream with correct buffer size.
                    OwnedMemoryStream = new(streamLength);
                    stream.CopyTo(OwnedMemoryStream);
                    Data = OwnedMemoryStream.GetBuffer();
                    Length = (int)OwnedMemoryStream.Length;
                }
                else
                {
                    // Complex case: length of stream is not known.
                    // This only occurs with streams that do not support the Length property.
                    OwnedMemoryStream = new();
                    stream.CopyTo(OwnedMemoryStream);
                    Data = OwnedMemoryStream.GetBuffer();
                    Length = (int)OwnedMemoryStream.Length;
                    // If buffer is larger than needed, create a new buffer with required size.
                    if (Data.Length > Length)
                    {
                        var tmp = new Byte[Length];
                        Buffer.BlockCopy(Data, 0, tmp, 0, Length);
                        Data = tmp;
                    }
                }
            }
        }

        internal byte GetByte(int offset)
        {
            if (CurrentOffset + offset >= Length)
                throw new InvalidOperationException("Index out of range.");

            return Data[CurrentOffset + offset];
        }

        internal ushort GetWord(int offset, bool bigEndian)
        {
            if (CurrentOffset + offset + 1 >= Length)
                throw new InvalidOperationException("Index out of range.");

            return (ushort)(bigEndian
                ? (Data[CurrentOffset + offset++] << 8) + Data[CurrentOffset + offset]
                : Data[CurrentOffset + offset++] + (Data[CurrentOffset + offset] << 8));
        }

        internal uint GetDWord(int offset, bool bigEndian)
        {
            if (CurrentOffset + offset + 3 >= Length)
                throw new InvalidOperationException("Index out of range.");

            // Are you a good developer?
            // What’s wrong with this code?
            //return (bigEndian
            //    ? ((uint)Data[CurrentOffset + offset++] << 24) + ((uint)Data[CurrentOffset + offset++] << 16) 
            //       + ((uint)Data[CurrentOffset + offset++] << 8) + Data[CurrentOffset + offset]
            //    : Data[CurrentOffset + offset++] + ((uint)Data[CurrentOffset + offset++] << 8)) 
            //       + ((uint)Data[CurrentOffset + offset++] << 16) + ((uint)Data[CurrentOffset + offset] << 24);
            return (uint)(bigEndian
                ? (Data[CurrentOffset + offset++] << 24) +
                  (Data[CurrentOffset + offset++] << 16) +
                  (Data[CurrentOffset + offset++] << 8) +
                   Data[CurrentOffset + offset]
                 : Data[CurrentOffset + offset++] +
                  (Data[CurrentOffset + offset++] << 8) +
                  (Data[CurrentOffset + offset++] << 16) +
                  (Data[CurrentOffset + offset] << 24));
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset() => CurrentOffset = 0;

        /// <summary>
        /// Gets the original stream.
        /// </summary>
        public Stream OriginalStream { get; }

        internal int CurrentOffset { get; set; }

        /// <summary>
        /// Gets the data as byte[].
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Gets the length of Data.
        /// </summary>
        public int Length { get; }

#if CORE || GDI || WPF
        /// <summary>
        /// Gets the owned memory stream. Can be null if no MemoryStream was created.
        /// </summary>
        public MemoryStream? OwnedMemoryStream { get; private set; }
#endif

        public void Dispose()
        {
#if CORE || GDI || WPF
            OwnedMemoryStream?.Dispose();
            OwnedMemoryStream = null;
#endif
        }
    }

    /// <summary>
    /// The imported image.
    /// </summary>
    abstract class ImportedImage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportedImage"/> class.
        /// </summary>
        protected ImportedImage(ImagePrivateData? data)
        {
            Data = data;
            if (data != null)
                data.Image = this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportedImage"/> class.
        /// </summary>
        protected ImportedImage()
            : this(null)
        { }

        /// <summary>
        /// Gets information about the image.
        /// </summary>
        public ImageInformation Information { get; private set; } = new ImageInformation();

#if true
        /// <summary>
        /// Gets the image data needed for the PDF file.
        /// </summary>
        // Data is created on demand without internal caching.
        public ImageData ImageData(PdfDocumentOptions options)
        {
            return PrepareImageData(options);
        }
#else
        /// <summary>
        /// Gets the image data needed for the PDF file.
        /// </summary>
        public ImageData ImageData
        {
            get { if(!HasImageData) _imageData = PrepareImageData();  return _imageData; }
            private set => _imageData = value;
        }
        ImageData _imageData;

        /// <summary>
        /// Gets a value indicating whether image data for the PDF file was already prepared.
        /// </summary>
        public bool HasImageData => _imageData != null;
#endif

        internal virtual ImageData PrepareImageData(PdfDocumentOptions options)
        {
            throw new NotImplementedException();
        }

        //IImageImporter _importer;
        internal ImagePrivateData? Data;
    }

    /// <summary>
    /// Public information about the image, filled immediately.
    /// Note: The stream will be read and decoded on the first call to PrepareImageData().
    /// ImageInformation can be filled for corrupted images that will throw an exception on PrepareImageData().
    /// </summary>
    class ImageInformation
    {
        internal enum ImageFormats
        {
            // ReSharper disable InconsistentNaming
            /// <summary>
            /// Value not set.
            /// </summary>
            Undefined = -1,

            /// <summary>
            /// Standard JPEG format (RGB).
            /// </summary>
            JPEG,

            /// <summary>
            /// Gray-scale JPEG format.
            /// </summary>
            JPEGGRAY,

            /// <summary>
            /// JPEG file with inverted CMYK, thus RGBW.
            /// </summary>
            JPEGRGBW,

            /// <summary>
            /// JPEG file with CMYK.
            /// </summary>
            JPEGCMYK,

            Palette1,
            Palette4,
            Palette8,
            Grayscale8,
            RGB24,
            ARGB32

            // ReSharper restore InconsistentNaming
        }

        internal ImageFormats ImageFormat = ImageFormats.Undefined;

        /// <summary>
        /// The width of the image in pixel.
        /// </summary>
        internal uint Width;

        /// <summary>
        /// The height of the image in pixel.
        /// </summary>
        internal uint Height;

        /// <summary>
        /// The horizontal DPI (dots per inch). Can be 0 if not supported by the image format.
        /// Note: JFIF (JPEG) files may contain either DPI or DPM or just the aspect ratio. Windows BMP files will contain DPM. Other formats may support any combination, including none at all.
        /// </summary>
        internal double HorizontalDPI;

        /// <summary>
        /// The vertical DPI (dots per inch). Can be 0 if not supported by the image format.
        /// </summary>
        internal double VerticalDPI;

        /// <summary>
        /// The horizontal DPM (dots per meter). Can be 0 if not supported by the image format.
        /// </summary>
        internal double HorizontalDPM;

        /// <summary>
        /// The vertical DPM (dots per meter). Can be 0 if not supported by the image format.
        /// </summary>
        internal double VerticalDPM;

        /// <summary>
        /// The horizontal component of the aspect ratio. Can be 0 if not supported by the image format.
        /// Note: Aspect ratio will be set if either DPI or DPM was set but may also be available in the absence of both DPI and DPM.
        /// </summary>
        internal double HorizontalAspectRatio;

        /// <summary>
        /// The vertical component of the aspect ratio. Can be 0 if not supported by the image format.
        /// </summary>
        internal double VerticalAspectRatio;

        /// <summary>
        /// The bit count per pixel. Only valid for certain images, will be 0 otherwise.
        /// </summary>
        internal uint BitCount;

        /// <summary>
        /// The colors used. Only valid for images with palettes, will be 0 otherwise.
        /// </summary>
        internal uint ColorsUsed;

        /// <summary>
        /// The default DPI (dots per inch) for images that do not have DPI information.
        /// </summary>
        internal double DefaultDPI;
    }

    /// <summary>
    /// Contains internal data. This includes a reference to the Stream if data for PDF was not yet prepared.
    /// </summary>
    abstract class ImagePrivateData
    {
        internal ImagePrivateData()
        { }

        /// <summary>
        /// Gets the image.
        /// </summary>
        public ImportedImage Image
        {
            get => _image ?? NRT.ThrowOnNull<ImportedImage>();
            internal set => _image = value;
        }
        ImportedImage? _image;
    }

    /// <summary>
    /// Contains data needed for PDF. Will be prepared when needed.
    /// </summary>
    abstract class ImageData
    { }
}
