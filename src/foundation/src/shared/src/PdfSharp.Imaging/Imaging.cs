// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using System.IO;

namespace PdfSharp.Internal.Imaging
{
    abstract class BitmapImageImporter
    {
        public abstract bool TryImport(byte[] bitmap,
            [MaybeNullWhen(false)] out ImportedImage importedImage);

        protected static void LogTryOpenFailure(string classname, Exception ex)
        {
            LogHost.Logger.LogWarning(
                $"Unhandled exception in {classname}, maybe caused by an unsupported image format. {ex.ToString()}");
        }

        /// <summary>
        /// Updates the height and width based on PixelHeight, PixelWidth, and the DPI settings.
        /// </summary>
        internal void UpdateWidthAndHeight(ImportedImage importedImage)
        {
            importedImage.Width = importedImage.PixelWidth / importedImage.DpiX * 72;
            importedImage.Height = importedImage.PixelHeight / importedImage.DpiY * 72;
        }

        /// <summary>
        /// When DPI values are calculated from metric values, try to round a integer when appropriate.
        /// </summary>
        internal void RoundDpiAfterConversionFromMetricValue(ImportedImage importedImage)
        {
            importedImage.DpiX = ProcessDpi(importedImage.DpiX);
            importedImage.DpiY = ProcessDpi(importedImage.DpiY);
            return;

            float_ ProcessDpi(float_ dpi)
            {
                int i = (int)(dpi + .5);
                double diff = Math.Abs(dpi - (double)i);
                // If DPI value is less than 0.1 % of the nearest integer, return the integer.
#if true
                // Rounding only for 72 and 96 DPI.
                if ((i == 72 || i == 96) && diff * 1000 < i)
                    return i;
#else
                    // Rounding for all values.
                    if (diff * 1000 < i)
                        return i;
#endif
                return dpi;
            }
        }
    }

    /// <summary>
    /// Image importer class.
    /// </summary>
    public static class ToBeNamed // Also rename file.  @@@
    {
        /// <summary>
        /// Tries to import an image. Returns true on success.
        /// </summary>
        /// <param name="bitmapImageStream">The bitmap image stream.</param>
        /// <param name="importedImage">The imported image.</param>
        public static bool TryImportImage(Stream bitmapImageStream,
            [MaybeNullWhen(false)] out ImportedImage importedImage)
        {
            try
            {
                byte[] bitmapBytes;
                using (var worker = new StreamReaderWorker(bitmapImageStream))
                {
                    bitmapBytes = worker.Data;
                    Debug.Assert(bitmapBytes.Length == worker.Length);
                }
                return TryImportImage(bitmapBytes, out importedImage);
            }
            catch (Exception ex)
            {
                LogHost.Logger.LogWarning(
                    $"Unhandled exception in TryImportImage, maybe caused by an unsupported image format. {ex.ToString()}");
            }

            importedImage = null!;
            return false;
        }

        /// <summary>
        /// Tries to import an image. Returns true on success.
        /// </summary>
        /// <param name="bitmapBytes">The bitmap bytes.</param>
        /// <param name="importedImage">The imported image.</param>
        public static bool TryImportImage(byte[] bitmapBytes,
            [MaybeNullWhen(false)] out ImportedImage importedImage)
        {
#if true
            foreach (var importer in Importers)
            {
                if (importer.TryImport(bitmapBytes, out importedImage))
                    return true;
            }
#else
            if (PngImporter.TryImport(bitmapBytes, out importedImage))
                return true;

            if (JpegImporter.TryImport(bitmapBytes, out importedImage))
                return true;

            if (BmpImporter.TryImport(bitmapBytes, out importedImage))
                return true;
#endif
            importedImage = null!;
            return false;
        }

        /// <summary>
        /// Tries to import a BMP image. Returns true on success.
        /// </summary>
        /// <param name="bitmapBytes">The bitmap bytes.</param>
        /// <param name="importedImage">The imported image.</param>
        public static bool TryImportImageBmp(byte[] bitmapBytes,
            [MaybeNullWhen(false)] out ImportedImage importedImage)
        {
            if (BmpImporter.TryImport(bitmapBytes, out importedImage))
                return true;
            importedImage = null!;
            return false;
        }

        static readonly BmpImageImporter BmpImporter = new();
        static readonly JpegImageImporter JpegImporter = new();
        static readonly PngImageImporter PngImporter = new();
        static readonly BitmapImageImporter[] Importers = [JpegImporter, PngImporter, BmpImporter];
    }
}

namespace PdfSharp.Internal.Imaging // #FILE
{
    /// <summary>
    /// Represents an imported image.
    /// </summary>
    public class ImportedImage
    {
        /// <summary>
        /// The number of inches per meter
        /// </summary>
        public const float_ InchPerMeter = 100 / 2.54f; // 39.37007874015748031496062992126

        /// <summary>
        /// Gets the image format.
        /// </summary>
        public ImageFormats ImageFormat { get; internal set; } = ImageFormats.Unknown;

        /// <summary>
        /// Gets the width of the image in Point.
        /// </summary>
        public float_ Width { get; internal set; }

        /// <summary>
        /// Gets the height of the image in Point.
        /// </summary>
        public float_ Height { get; internal set; }

        /// <summary>
        /// Gets the horizontal DPI value.
        /// </summary>
        public float_ DpiX { get; internal set; } = 96;  // Never 0

        /// <summary>
        /// Gets the vertical DPI value.
        /// </summary>
        public float_ DpiY { get; internal set; } = 96;

        /// <summary>
        /// Gets the width of the image in pixel.
        /// </summary>
        public int PixelWidth { get; internal set; }

        /// <summary>
        /// Gets the height of the image in pixel.
        /// </summary>
        public int PixelHeight { get; internal set; }

        /// <summary>
        /// Gets the raw image data.
        /// </summary>
        public byte[] ImageData { get; internal set; } = null!;

        /// <summary>
        /// Gets the image data needed for PDF.
        /// </summary>
        public virtual byte[] GetPdfImageData( /* some options if we have them*/)
        {
            // With JPEG, we embed the unaltered JPEG file. No conversion required. Override for images that require conversion.
            return ImageData;
        }
    }
}

namespace PdfSharp.Internal.Imaging
{
    /// <summary>
    /// Represents an imported raster image.
    /// </summary>
    /// <seealso cref="PdfSharp.Internal.Imaging.ImportedImage" />
    public class ImportedRasterImage : ImportedImage
    {
        /// <summary>
        /// Gets the alpha mask data.
        /// </summary>
        public byte[]? AlphaMaskData { get; internal set; } = null;

        /// <summary>
        /// Gets the bitmap mask data.
        /// </summary>
        public byte[]? BitmapMaskData { get; internal set; } = null;

        /// <summary>
        /// Gets the bitmap mask.
        /// </summary>
        public MonochromeMask? BitmapMask { get; internal set; } = null;

        /// <summary>
        /// Gets the extracted image data.
        /// </summary>
        public byte[]? ExtractedImageData { get; internal set; } = null;

        /// <summary>
        /// Gets the color palette data.
        /// </summary>
        public byte[]? PaletteData { get; internal set; } = null;

        /// <summary>
        /// Gets the number of palette colors.
        /// </summary>
        public int PaletteColors { get; internal set; } = 0;

        /// <summary>
        /// Gets the image data needed for PDF.
        /// </summary>
        /// <returns></returns>
        public new byte[] GetPdfImageData( /* some options if we have them*/)
        {
            return ExtractedImageData!;
        }

        /// <summary>
        /// Gets the alpha mask data needed for PDF.
        /// </summary>
        /// <returns></returns>
        public virtual byte[]? GetPdfAlphaMaskData( /* some options if we have them*/)
        {
            return AlphaMaskData;
        }

        /// <summary>
        /// Gets the bitmap mask data needed for PDF.
        /// </summary>
        public virtual byte[]? GetPdfBitmapMaskData( /* some options if we have them*/)
        {
            if (BitmapMaskData is null && AlphaMaskData is not null)
            {
                // Can be generated from AlphaMask.
                throw new NotImplementedException("GetPdfBitmapMaskData not yet implemented for this image file.");
            }

            return BitmapMaskData;
        }

        /// <summary>
        /// Gets the PDF bitmap mask.
        /// </summary>
        /// <returns></returns>
        public virtual MonochromeMask? GetPdfBitmapMask( /* some options if we have them*/)
        {
            return BitmapMask;
        }

        /// <summary>
        /// Gets the palette data needed for PDF.
        /// </summary>
        /// <returns></returns>
        public virtual byte[]? GetPdfPaletteData( /* some options if we have them*/)
        {
            return PaletteData;
        }

        /// <summary>
        /// The bit count per pixel.
        /// </summary>
        public int BitCount { get; internal set; }

        /// <summary>
        /// The colors used. Only valid for images with palettes, will be 0 otherwise.
        /// </summary>
        public int ColorsUsed { get; internal set; }
    }
}

namespace PdfSharp.Internal.Imaging
{
    /// <summary>
    /// Represents an imported BMP image.
    /// </summary>
    /// <seealso cref="PdfSharp.Internal.Imaging.ImportedRasterImage" />
    public class ImportedBmpImage : ImportedRasterImage
    {
        /// <summary>
        /// Gets a value indicating if the image is bitonal.
        /// </summary>
        public int Bitonal { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this image is grayscale.
        /// </summary>
        public bool IsGray { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the image has a segmented color mask.
        /// </summary>
        public bool SegmentedColorMask { get; internal set; }

        /// <summary>
        /// Gets the first color of the mask.
        /// </summary>
        public int FirstMaskColor { get; internal set; } = -1;

        /// <summary>
        /// Gets the last color of the mask.
        /// </summary>
        public int LastMaskColor { get; internal set; } = -1;

        /// <summary>
        /// Gets the file offset of the data bytes.
        /// </summary>
        public int BytesFileOffset { get; internal set; }
    }
}

namespace PdfSharp.Internal.Imaging
{
    /// <summary>
    /// Represents an imported PNG image.
    /// </summary>
    /// <seealso cref="PdfSharp.Internal.Imaging.ImportedRasterImage" />
    public class ImportedPngImage : ImportedRasterImage
    {
        // Additional Png specific stuff.
    }
}

namespace PdfSharp.Internal.Imaging
{
    /// <summary>
    /// Represents an imported JPEG image.
    /// </summary>
    /// <seealso cref="PdfSharp.Internal.Imaging.ImportedImage" />
    public class ImportedJpegImage : ImportedImage
    {
        // Additional Jpeg specific stuff.

        /// <summary>
        /// The horizontal DPM (dots per meter). Can be 0 if not supported by the image format.
        /// </summary>
        public float_ DpmX { get; internal set; }

        /// <summary>
        /// The vertical DPM (dots per meter). Can be 0 if not supported by the image format.
        /// </summary>
        public float_ DpmY { get; internal set; }

        /// <summary>
        /// The horizontal component of the aspect ratio. Can be 0 if not supported by the image format.
        /// Note: Aspect ratio will be set if either DPI or DPM was set but may also be available in the absence of both DPI and DPM.
        /// </summary>
        public float_ HorizontalAspectRatio { get; internal set; }

        /// <summary>
        /// The vertical component of the aspect ratio. Can be 0 if not supported by the image format.
        /// </summary>
        public float_ VerticalAspectRatio { get; internal set; }
    }
}

namespace PdfSharp.Internal.Imaging
{
    /// <summary>
    /// The image formats.
    /// </summary>
    public enum ImageFormats
    {
        /// <summary>
        /// The unknown format.
        /// </summary>
        Unknown,

        // WPF names for raster images.

        // Not yet used: BlackWhite,
        /// <summary>
        /// Color palette with 1 bit per pixel.
        /// </summary>
        Indexed1,
        /// <summary>
        /// Color palette with 4 bits per pixel.
        /// </summary>
        Indexed4,
        /// <summary>
        /// Color palette with 8 bits per pixel.
        /// </summary>
        Indexed8,
        /// <summary>
        /// Grayscale image.
        /// </summary>
        Gray8,

        /// <summary>
        /// RGB image with 24 bits per pixel.
        /// </summary>
        Bgr32,
        /// <summary>
        /// RGB image with 24 bits per pixel and alpha channel.
        /// </summary>
        Bgra32,
        // Not yet used: Pbgra32

        // Own names for JPEG formats that have no individual names in WPF.

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
    }
}

namespace PdfSharp.Internal.Imaging
{
    /// <summary>
    /// Helper for dealing with byte[] data.
    /// </summary>
    class DataParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataParser"/> class.
        /// </summary>
        /// <param name="data">The raw data.</param>
        public DataParser(byte[] data)
        {
            Data = data;
            Length = data.Length;
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

        internal int CurrentOffset { get; set; }

        /// <summary>
        /// Gets the data as byte[].
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Gets the length of Data.
        /// </summary>
        public int Length { get; }
    }
}

namespace PdfSharp.Internal.Imaging
{
    /// <summary>
    /// Helper for dealing with Streams and files.
    /// </summary>
    public class ReaderHelper
    {
        /// <summary>
        /// Reads from a file.
        /// </summary>
        /// <exception cref="InvalidOperationException">Image files larger than 2 GiB are not supported.</exception>
        public static byte[] ReadFromFile(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length > Int32.MaxValue)
                    throw new InvalidOperationException("Image files larger than 2 GiB are not supported.");
                return ReadFromStream(stream);
            }
        }

        /// <summary>
        /// Reads from a stream.
        /// </summary>
        /// <exception cref="ArgumentNullException">stream - Stream has no content byte array.</exception>
        public static byte[] ReadFromStream(Stream stream)
        {
            int streamLength = (int)stream.Length;
            byte[] data = null!;
            int length = 0;

            if (stream is MemoryStream ms)
            {
                // If the given stream is a MemoryStream, work with it.
                if (ms.TryGetBuffer(out var buffer))
                {
                    // Buffer is accessible - use it.
                    data = buffer.Array ??
                           throw new ArgumentNullException(nameof(stream), "Stream has no content byte array.");
                    length = (int)ms.Length;
                    // If buffer is larger than needed, create a new buffer with required size.
                    if (data.Length > length)
                    {
                        var tmp = new Byte[length];
                        Buffer.BlockCopy(data, 0, tmp, 0, length);
                        data = tmp;
                    }
                }
                else
                {
                    // Buffer of given stream is not accessible, so read stream into new buffer.
                    var memoryStream = new MemoryStream(streamLength);
                    stream.CopyTo(memoryStream);
                    data = memoryStream.GetBuffer();
                    length = (int)memoryStream.Length;
                    LogHost.Logger.LogWarning(
                        "LoadImage: MemoryStream with buffer that is not publicly visible was used. " +
                        "For better performance, set 'publiclyVisible' to true when creating the MemoryStream.");
                }
            }
            else
            {
                // If the given stream is not a MemoryStream, copy the stream to a new MemoryStream.
                if (streamLength > -1)
                {
                    // Simple case: length of stream is known, create a MemoryStream with correct buffer size.
                    var memoryStream = new MemoryStream(streamLength);
                    stream.CopyTo(memoryStream);
                    data = memoryStream.GetBuffer();
                    length = (int)memoryStream.Length;
                }
                else
                {
                    // Complex case: length of stream is not known.
                    // This only occurs with streams that do not support the Length property.
                    var memoryStream = new MemoryStream(streamLength);
                    stream.CopyTo(memoryStream);
                    data = memoryStream.GetBuffer();
                    length = (int)memoryStream.Length;
                    // If buffer is larger than needed, create a new buffer with required size. Should never happen.
                    if (data.Length > length)
                    {
                        var tmp = new Byte[length];
                        Buffer.BlockCopy(data, 0, tmp, 0, length);
                        data = tmp;
                    }
                }
            }

            Debug.Assert(data.Length == length);
            return data;
        }
    }
}

namespace PdfSharp.Internal.Imaging
{
    /// <summary>
    /// Helper for dealing with Stream data, including streams that do not implement the Length property.
    /// </summary>
    internal class StreamReaderWorker : IDisposable
    {
        public StreamReaderWorker(string path)
        {
            OriginalStream = null!;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var worker = new StreamReaderWorker(stream);
                Data = worker.Data;
                Length = worker.Length;
            }
        }

        public StreamReaderWorker(byte[] data)
        {
            OriginalStream = null!;
            Data = data;
            Length = data.Length;
        }

        /// <summary>
        /// Gets the length of the stream. Return -1 if length cannot be determined.
        /// </summary>
        static int StreamLengthFromStream(Stream stream)
        {
            long length = -1;
            try
            {
                length = stream.Length;
            }
            catch (NotSupportedException)
            {
                // We eat this exception.
                // We can handle streams that do not return their length.
            }
            catch (Exception ex)
            {
                // Unexpected exception.
                throw new InvalidOperationException("Cannot determine the length of the stream. Use a stream that supports the Length property. Consider copying the image to a MemoryStream.", ex);
            }

            if (length is < -1 or > Int32.MaxValue)
                throw new InvalidOperationException($"Image files with a size of {length} bytes are not supported. Use image files smaller than 2 GiB.");

            return (int)length;
        }

        public StreamReaderWorker(Stream stream)
        {
            OriginalStream = stream;

            int streamLength = StreamLengthFromStream(stream);

            if (stream is MemoryStream ms && streamLength > -1)
            {
                // If the given stream is a MemoryStream, work with it.
                if (ms.TryGetBuffer(out var buffer))
                {
                    // Buffer is accessible - use it.
                    Data = buffer.Array ?? throw new ArgumentNullException(nameof(stream), "Stream has no content byte array.");
                    Length = (int)ms.Length;
                    // If buffer is larger than needed, create a new buffer with required size.
                    if (Data.Length > Length)
                    {
                        var tmp = new Byte[Length];
                        Buffer.BlockCopy(Data, 0, tmp, 0, Length);
                        Data = tmp;
                    }
                }
                else
                {
                    // Buffer of given stream is not accessible, so read stream into new buffer.
                    OwnedMemoryStream = new(streamLength);
                    stream.CopyTo(OwnedMemoryStream);
                    Data = OwnedMemoryStream.GetBuffer();
                    Length = (int)OwnedMemoryStream.Length;
                    LogHost.Logger.LogWarning("LoadImage: MemoryStream with buffer that is not publicly visible was used. " +
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
                    OwnedMemoryStream = new(); // Length is not yet known.
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

        public byte GetByte(int offset)
        {
            if (CurrentOffset + offset >= Length)
                throw new InvalidOperationException("Index out of range.");

            return Data[CurrentOffset + offset];
        }

        public ushort GetWord(int offset, bool bigEndian)
        {
            if (CurrentOffset + offset + 1 >= Length)
                throw new InvalidOperationException("Index out of range.");

            return (ushort)(bigEndian
                ? (Data[CurrentOffset + offset++] << 8) + Data[CurrentOffset + offset]
                : Data[CurrentOffset + offset++] + (Data[CurrentOffset + offset] << 8));
        }

        public uint GetDWord(int offset, bool bigEndian)
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

        public int CurrentOffset { get; set; }

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
}
