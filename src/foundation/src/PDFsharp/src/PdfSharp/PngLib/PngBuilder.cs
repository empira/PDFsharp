// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#nullable enable annotations

// ReSharper disable once CheckNamespace
namespace PdfSharp.BigGustave
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Used to construct PNG images. Call <see cref="Create"/> to make a new builder.
    /// </summary>
    public class PngBuilder
    {
        private const byte Deflate32KbWindow = 120;
        private const byte ChecksumBits = 1;

        private readonly byte[] rawData;
        private readonly bool hasAlphaChannel;
        private readonly int width;
        private readonly int height;
        private readonly int bytesPerPixel;

        private bool hasTooManyColorsForPalette;

        private readonly int backgroundColorInt;
        private readonly Dictionary<int, int> colorCounts;

        private readonly List<(string keyword, byte[] data)> storedStrings = new List<(string keyword, byte[] data)>();

        /// <summary>
        /// Create a builder for a PNG with the given width and size.
        /// </summary>
        public static PngBuilder Create(int width, int height, bool hasAlphaChannel)
        {
            var bpp = hasAlphaChannel ? 4 : 3;

            var length = (height * width * bpp) + height;

            return new PngBuilder(new byte[length], hasAlphaChannel, width, height, bpp);
        }

        /// <summary>
        /// Create a builder from a <see cref="Png"/>.
        /// </summary>
        public static PngBuilder FromPng(Png png)
        {
            var result = Create(png.Width, png.Height, png.HasAlphaChannel);

            for (int y = 0; y < png.Height; y++)
            {
                for (int x = 0; x < png.Width; x++)
                {
                    result.SetPixel(png.GetPixel(x, y), x, y);
                }
            }

            return result;
        }

        /// <summary>
        /// Create a builder from the bytes of the specified PNG image.
        /// </summary>
        public static PngBuilder FromPngBytes(byte[] png)
        {
            var pngActual = Png.Open(png);
            return FromPng(pngActual);
        }

        /// <summary>
        /// Create a builder from the bytes in the BGRA32 pixel format.
        /// https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.pixelformats.bgra32
        /// </summary>
        /// <param name="data">The pixels in BGRA32 format.</param>
        /// <param name="width">The width in pixels.</param>
        /// <param name="height">The height in pixels.</param>
        /// <param name="useAlphaChannel">Whether to include an alpha channel in the output.</param>
        public static PngBuilder FromBgra32Pixels(byte[] data, int width, int height, bool useAlphaChannel = true)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                var builder = FromBgra32Pixels(memoryStream, width, height, useAlphaChannel);

                return builder;
            }
        }

        /// <summary>
        /// Create a builder from the bytes in the BGRA32 pixel format.
        /// https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.pixelformats.bgra32
        /// </summary>
        /// <param name="data">The pixels in BGRA32 format.</param>
        /// <param name="width">The width in pixels.</param>
        /// <param name="height">The height in pixels.</param>
        /// <param name="useAlphaChannel">Whether to include an alpha channel in the output.</param>
        public static PngBuilder FromBgra32Pixels(Stream data, int width, int height, bool useAlphaChannel = true)
        {
            var bpp = useAlphaChannel ? 4 : 3;

            var length = (height * width * bpp) + height;

            var builder = new PngBuilder(new byte[length], useAlphaChannel, width, height, bpp);

            var buffer = new byte[4];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var read = data.Read(buffer, 0, buffer.Length);

                    if (read != 4)
                    {
                        throw new InvalidOperationException($"Unexpected end of stream, expected to read 4 bytes at offset {data.Position - read} for (x: {x}, y: {y}), instead got {read}.");
                    }

                    if (useAlphaChannel)
                    {
                        builder.SetPixel(new Pixel(buffer[0], buffer[1], buffer[2], buffer[3], false), x, y);
                    }
                    else
                    {
                        builder.SetPixel(buffer[0], buffer[1], buffer[2], x, y);
                    }
                }
            }

            return builder;
        }

        private PngBuilder(byte[] rawData, bool hasAlphaChannel, int width, int height, int bytesPerPixel)
        {
            this.rawData = rawData;
            this.hasAlphaChannel = hasAlphaChannel;
            this.width = width;
            this.height = height;
            this.bytesPerPixel = bytesPerPixel;

            backgroundColorInt = PixelToColorInt(0, 0, 0, hasAlphaChannel ? (byte)0 : byte.MaxValue);

            colorCounts = new Dictionary<int, int>()
            {
                { backgroundColorInt, (width * height)}
            };
        }

        /// <summary>
        /// Sets the RGB pixel value for the given column (x) and row (y).
        /// </summary>
        public PngBuilder SetPixel(byte r, byte g, byte b, int x, int y) => SetPixel(new Pixel(r, g, b), x, y);

        /// <summary>
        /// Set the pixel value for the given column (x) and row (y).
        /// </summary>
        public PngBuilder SetPixel(Pixel pixel, int x, int y)
        {
            if (!hasTooManyColorsForPalette)
            {
                var val = PixelToColorInt(pixel);
                if (val != backgroundColorInt)
                {
                    if (!colorCounts.ContainsKey(val))
                    {
                        colorCounts[val] = 1;
                    }
                    else
                    {
                        colorCounts[val]++;
                    }

                    colorCounts[backgroundColorInt]--;
                    if (colorCounts[backgroundColorInt] == 0)
                    {
                        colorCounts.Remove(backgroundColorInt);
                    }
                }

                if (colorCounts.Count > 256)
                {
                    hasTooManyColorsForPalette = true;
                }
            }

            var start = (y * ((width * bytesPerPixel) + 1)) + 1 + (x * bytesPerPixel);

            rawData[start++] = pixel.R;
            rawData[start++] = pixel.G;
            rawData[start++] = pixel.B;

            if (hasAlphaChannel)
            {
                rawData[start] = pixel.A;
            }

            return this;
        }

        /// <summary>
        /// Allows you to store arbitrary text data in the "iTXt" international textual data
        /// chunks of the generated PNG image.
        /// </summary>
        /// <param name="keyword">
        /// A keyword identifying the text data between 1-79 characters in length.
        /// Must not start with, end with, or contain consecutive whitespace characters.
        /// Only characters in the range 32 - 126 and 161 - 255 are permitted.
        /// </param>
        /// <param name="text">
        /// The text data to store. Encoded as UTF-8 that may not contain zero (0) bytes but can be zero-length.
        /// </param>
        public PngBuilder StoreText(string keyword, string text)
        {
            if (keyword == null)
            {
                throw new ArgumentNullException(nameof(keyword), "Keyword may not be null.");
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text), "Text may not be null.");
            }

            if (keyword == string.Empty)
            {
                throw new ArgumentException("Keyword may not be empty.", nameof(keyword));
            }

            if (keyword.Length > 79)
            {
                throw new ArgumentException($"Keyword must be between 1 - 79 characters, provided keyword '{keyword}' has length of {keyword.Length} characters.",
                    nameof(keyword));
            }

            for (var i = 0; i < keyword.Length; i++)
            {
                var c = keyword[i];
                var isValid = (c >= 32 && c <= 126) || (c >= 161 && c <= 255);
                if (!isValid)
                {
                    throw new ArgumentException("The keyword can only contain printable Latin 1 characters and spaces in the ranges 32 - 126 or 161 -255. " +
                                                $"The provided keyword '{keyword}' contained an invalid character ({c}) at index {i}.", nameof(keyword));
                }

                // pngTODO: trailing, leading and consecutive whitespaces are also prohibited.
            }

            var bytes = Encoding.UTF8.GetBytes(text);

            for (var i = 0; i < bytes.Length; i++)
            {
                var b = bytes[i];

                if (b == 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(text), "The provided text contained a null (0) byte when converted to UTF-8. Null bytes are not permitted. " +
                                                                        $"Text was: '{text}'");
                }
            }

            storedStrings.Add((keyword, bytes));

            return this;
        }

        /// <summary>
        /// Get the bytes of the PNG file for this builder.
        /// </summary>
        public byte[] Save(SaveOptions? options = null)
        {
            using var memoryStream = new MemoryStream();
            Save(memoryStream, options);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Write the PNG file bytes to the provided stream.
        /// </summary>
        public void Save(Stream outputStream, SaveOptions? options = null)
        {
            options = options ?? new SaveOptions();

            byte[]? palette = null;
            var dataLength = rawData.Length;
            var bitDepth = 8;

            if (!hasTooManyColorsForPalette && !hasAlphaChannel)
            {
                var paletteColors = colorCounts.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
                bitDepth = paletteColors.Count > 16 ? 8 : 4;
                var samplesPerByte = bitDepth == 8 ? 1 : 2;
                var applyShift = samplesPerByte == 2;

                palette = new byte[3 * paletteColors.Count];

                for (var i = 0; i < paletteColors.Count; i++)
                {
                    var color = ColorIntToPixel(paletteColors[i]);
                    var startIndex = i * 3;
                    palette[startIndex++] = color.r;
                    palette[startIndex++] = color.g;
                    palette[startIndex] = color.b;
                }

                var rawDataIndex = 0;

                for (var y = 0; y < height; y++)
                {
                    // None filter - we don’t use filtering for palette images.
                    rawData[rawDataIndex++] = 0;

                    for (var x = 0; x < width; x++)
                    {
                        var index = ((y * width * bytesPerPixel) + y + 1) + (x * bytesPerPixel);

                        var r = rawData[index++];
                        var g = rawData[index++];
                        var b = rawData[index];

                        var colorInt = PixelToColorInt(r, g, b);

                        var value = (byte)paletteColors.IndexOf(colorInt);

                        if (applyShift)
                        {
                            // apply mask and shift
                            var withinByteIndex = x % 2;

                            if (withinByteIndex == 1)
                            {
                                rawData[rawDataIndex] = (byte)(rawData[rawDataIndex] + value);
                                rawDataIndex++;
                            }
                            else
                            {
                                rawData[rawDataIndex] = (byte)(value << 4);
                            }
                        }
                        else
                        {
                            rawData[rawDataIndex++] = value;
                        }

                    }
                }

                dataLength = rawDataIndex;
            }
            else
            {
                AttemptCompressionOfRawData(rawData, options);
            }

            outputStream.Write(HeaderValidationResult.ExpectedHeader, 0, HeaderValidationResult.ExpectedHeader.Length);

            var stream = new PngStreamWriteHelper(outputStream);

            stream.WriteChunkLength(13);
            stream.WriteChunkHeader(ImageHeader.HeaderBytes);

            StreamHelper.WriteBigEndianInt32(stream, width);
            StreamHelper.WriteBigEndianInt32(stream, height);
            stream.WriteByte((byte)bitDepth);

            var colorType = ColorType.ColorUsed;
            if (hasAlphaChannel)
            {
                colorType |= ColorType.AlphaChannelUsed;
            }

            if (palette != null)
            {
                colorType |= ColorType.PaletteUsed;
            }

            stream.WriteByte((byte)colorType);
            stream.WriteByte((byte)CompressionMethod.DeflateWithSlidingWindow);
            stream.WriteByte((byte)FilterMethod.AdaptiveFiltering);
            stream.WriteByte((byte)InterlaceMethod.None);

            stream.WriteCrc();

            if (palette != null)
            {
                stream.WriteChunkLength(palette.Length);
                stream.WriteChunkHeader(Encoding.ASCII.GetBytes("PLTE"));
                stream.Write(palette, 0, palette.Length);
                stream.WriteCrc();
            }

            var imageData = Compress(rawData, dataLength, options);
            stream.WriteChunkLength(imageData.Length);
            stream.WriteChunkHeader(Encoding.ASCII.GetBytes("IDAT"));
            stream.Write(imageData, 0, imageData.Length);
            stream.WriteCrc();

            foreach (var storedString in storedStrings)
            {
                var keyword = Encoding.GetEncoding("iso-8859-1").GetBytes(storedString.keyword);
                var length = keyword.Length
                             + 1 // Null separator
                             + 1 // Compression flag
                             + 1 // Compression method
                             + 1 // Null separator
                             + 1 // Null separator
                             + storedString.data.Length;

                stream.WriteChunkLength(length);
                stream.WriteChunkHeader(Encoding.ASCII.GetBytes("iTXt"));
                stream.Write(keyword, 0, keyword.Length);

                stream.WriteByte(0); // Null separator
                stream.WriteByte(0); // Compression flag (0 for uncompressed)
                stream.WriteByte(0); // Compression method (0, ignored since flag is zero)
                stream.WriteByte(0); // Null separator
                stream.WriteByte(0); // Null separator

                stream.Write(storedString.data, 0, storedString.data.Length);
                stream.WriteCrc();
            }

            stream.WriteChunkLength(0);
            stream.WriteChunkHeader(Encoding.ASCII.GetBytes("IEND"));
            stream.WriteCrc();
        }

        static byte[] Compress(byte[] data, int dataLength, SaveOptions options)
        {
            const int headerLength = 2;
            const int checksumLength = 4;

            var compressionLevel = options?.AttemptCompression == true ? CompressionLevel.Optimal : CompressionLevel.Fastest;

            using (var compressStream = new MemoryStream())
            using (var compressor = new DeflateStream(compressStream, compressionLevel, true))
            {
                compressor.Write(data, 0, dataLength);
                compressor.Close();

                compressStream.Seek(0, SeekOrigin.Begin);

                var result = new byte[headerLength + compressStream.Length + checksumLength];

                // Write the ZLib header.
                result[0] = Deflate32KbWindow;
                result[1] = ChecksumBits;

                // Write the compressed data.
                int streamValue;
                var i = 0;
                while ((streamValue = compressStream.ReadByte()) != -1)
                {
                    result[headerLength + i] = (byte)streamValue;
                    i++;
                }

                // Write Checksum of raw data.
                var checksum = Adler32Checksum.Calculate(data, dataLength);

                var offset = headerLength + compressStream.Length;

                result[offset++] = (byte)(checksum >> 24);
                result[offset++] = (byte)(checksum >> 16);
                result[offset++] = (byte)(checksum >> 8);
                result[offset] = (byte)(checksum >> 0);

                return result;
            }
        }

        /// <summary>
        /// Attempt to improve compressibility of the raw data by using adaptive filtering.
        /// </summary>
        private void AttemptCompressionOfRawData(byte[] rawData, SaveOptions options)
        {
            if (!options.AttemptCompression)
            {
                return;
            }

            var bytesPerScanline = 1 + (bytesPerPixel * width);
            var scanlineCount = rawData.Length / bytesPerScanline;

            var scanData = new byte[bytesPerScanline - 1];

            for (var scanlineRowIndex = 0; scanlineRowIndex < scanlineCount; scanlineRowIndex++)
            {
                var sourceIndex = (scanlineRowIndex * bytesPerScanline) + 1;

                Array.Copy(rawData, sourceIndex, scanData, 0, bytesPerScanline - 1);

                var noneFilterSum = 0;
                for (int i = 0; i < scanData.Length; i++)
                {
                    noneFilterSum += scanData[i];
                }

                //var leftFilterSum = 0;
                for (int i = 0; i < scanData.Length; i++)
                {

                }
                /* 
                 * A heuristic approach is to use adaptive filtering as follows: 
                 *    independently for each row, apply all five filters and select the filter that produces the smallest sum of absolute values per row. 
                 */
            }
        }

        static int PixelToColorInt(Pixel p) => PixelToColorInt(p.R, p.G, p.B, p.A);
        static int PixelToColorInt(byte r, byte g, byte b, byte a = 255)
        {
            return (a << 24) + (r << 16) + (g << 8) + b;
        }

        static (byte r, byte g, byte b, byte a) ColorIntToPixel(int i) => ((byte)(i >> 16), (byte)(i >> 8), (byte)i, (byte)(i >> 24));

        /// <summary>
        /// Options for configuring generation of PNGs from a <see cref="PngBuilder"/>.
        /// </summary>
        public class SaveOptions
        {
            /// <summary>
            /// Whether the library should try to reduce the resulting image size.
            /// This process does not affect the original image data (it is lossless) but may 
            /// result in longer save times.
            /// </summary>
            public bool AttemptCompression { get; set; }

            /// <summary>
            /// The number of parallel tasks allowed during compression.
            /// </summary>
            public int MaxDegreeOfParallelism { get; set; } = 1;
        }
    }
}
