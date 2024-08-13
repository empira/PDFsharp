// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if CORE
using PdfSharp.BigGustave;
#endif
using PdfSharp.Pdf;

namespace PdfSharp.Drawing.Internal
{
    class ImageImporterPng : ImageImporterRoot, IImageImporter
    {
        public ImportedImage? ImportImage(StreamReaderHelper stream)
        {
            // Only used for Core build.
            // TODO Enable for GDI and WPF for testing?
#if WPF || GDI
            // We don’t handle any files for WPF or GDI+ build.
            return null;
#endif

#if CORE
            try
            {
                stream.CurrentOffset = 0;
                if (TestPngFileHeader(stream))
                {
                    ImportedImage ii = new ImportedImagePng(this);
                    if (TestPngInfoHeader(stream, ii))
                    {
                        return ii;
                    }
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            { }
            return null;
#endif
        }

#if CORE
        /// <summary>
        /// A quick check for PNG files, checking the first 16 bytes.
        /// </summary>
        bool TestPngFileHeader(StreamReaderHelper stream)
        {
            var header = new Byte[]
            {
                0x89, 0x50, 0x4e, 0x47, // ?PNG
                0x0d, 0x0a, 0x1a, 0x0a, // CR, LF, CTRL-Z, LF
                0x00, 0x00, 0x00, 0x0d, // Chunk length: 13 bytes
                0x49, 0x48, 0x44, 0x52  // IHDR
            };

            int offset = 0;

            // File must start with specific header bytes.
            foreach (var headerByte in header)
            {
                if (stream.GetByte(offset) != headerByte)
                    return false;
                ++offset;
            }

            return true;
        }

        /// <summary>
        ///  Read information from PNG image header.
        /// </summary>
        private Boolean TestPngInfoHeader(StreamReaderHelper stream, ImportedImage ii)
        {
            // Width: 4 bytes
            // Height: 4 bytes
            // Bit depth: 1 byte
            // Color type: 1 byte
            // Compression method: 1 byte
            // Filter method: 1 byte
            // Interlace method: 1 byte

            stream.CurrentOffset = 16;
            var width = stream.GetDWord(0, true);
            var height = stream.GetDWord(4, true);
            var bitDepth = stream.GetByte(8);
            var colorType = stream.GetByte(9);
            var compressionMethod = stream.GetByte(10);
            var filterMethod = stream.GetByte(11);
            var interlaceMethod = stream.GetByte(12);

            ii.Information.Width = width;
            ii.Information.Height = height;
            ii.Information.HorizontalDPI = 0;
            ii.Information.VerticalDPI = 0;
            ii.Information.HorizontalDPM = 0;
            ii.Information.VerticalDPM = 0;
            ii.Information.HorizontalAspectRatio = 0;
            ii.Information.VerticalAspectRatio = 0;
            ii.Information.DefaultDPI = 96; // Assume 96 DPI if information not provided in the file.

            ii.Information.ColorsUsed = 0;
            // colorType can be 0, 2, 3, 4, or 6.
            switch (colorType)
            {
                case 0:
                    // Each pixel is a grayscale sample. 1,2,4,8,16.
                    switch (bitDepth)
                    {
                        case 8:
                            ii.Information.ImageFormat = ImageInformation.ImageFormats.Grayscale8;
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported bit depth {bitDepth} for PNG color type {colorType}.");
                    }
                    // ReSharper disable once HeuristicUnreachableCode
#pragma warning disable CS0162
                    break;
#pragma warning restore CS0162

                case 2:
                    // Each pixel is an R,G,B triple. 8, 16.
                    switch (bitDepth)
                    {
                        case 8:
                            ii.Information.ImageFormat = ImageInformation.ImageFormats.RGB24;
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported bit depth {bitDepth} for PNG color type {colorType}.");
                    }
                    break;

                case 3:
                    // Each pixel is a palette index; a PLTE chunk must appear. 1, 2, 4, 8.
                    switch (bitDepth)
                    {
                        case 1:
                            ii.Information.ImageFormat = ImageInformation.ImageFormats.Palette1;
                            break;
                        case 4:
                            ii.Information.ImageFormat = ImageInformation.ImageFormats.Palette4;
                            break;
                        case 8:
                            ii.Information.ImageFormat = ImageInformation.ImageFormats.Palette8;
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported bit depth {bitDepth} for PNG color type {colorType}.");
                    }
                    break;

                case 4:
                    // Each pixel is a grayscale sample, followed by an alpha sample. 8, 16.
                    switch (bitDepth)
                    {
                        case 8:
                            ii.Information.ImageFormat = ImageInformation.ImageFormats.Grayscale8;
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported bit depth {bitDepth} for PNG color type {colorType}.");
                    }
                    break;
                // TODO case 4:

                case 6:
                    // Each pixel is an R,G,B triple, followed by an alpha sample. 8, 16.
                    switch (bitDepth)
                    {
                        case 8:
                            ii.Information.ImageFormat = ImageInformation.ImageFormats.ARGB32;
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported bit depth {bitDepth} for PNG color type {colorType}.");
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported PNG color type {colorType}.");
            }

            // Now access the PNG pixels.
            // Png does not implement IDisposable.
            {
                // Do not use OriginalStream if we have Data.
                if (stream.Data == null! && stream.OriginalStream != null!)
                    stream.OriginalStream.Position = 0;
                var myVisitor = new MyVisitor();
                var png = stream.Data == null! && stream.OriginalStream != null ?
                    Png.Open(stream.OriginalStream, myVisitor) :
                    Png.Open(stream.Data!, myVisitor);

                if (png.Width != ii.Information.Width ||
                    png.Height != ii.Information.Height)
                {
                    throw new InvalidOperationException($"Unsupported PNG image - internal error.");
                }

                if (myVisitor.IsValid)
                {
                    if (myVisitor.IsMeter)
                    {
                        ii.Information.HorizontalDPM = myVisitor.Horizontal;
                        ii.Information.VerticalDPM = myVisitor.Vertical;
                    }
                    ii.Information.HorizontalAspectRatio = myVisitor.Horizontal;
                    ii.Information.VerticalAspectRatio = myVisitor.Vertical;
                }

                switch (ii.Information.ImageFormat)
                {
                    // Copy image data later, only when it is needed? Or do it here because the stream is already open?
                    case ImageInformation.ImageFormats.RGB24:
                    case ImageInformation.ImageFormats.ARGB32:
                        {
                            if (png.HasAlphaChannel != true &&
                                ii.Information.ImageFormat == ImageInformation.ImageFormats.ARGB32)
                            {
                                throw new InvalidOperationException($"Unsupported PNG ARGB32 image - internal error.");
                            }
                            if (png.HasAlphaChannel != false &&
                                ii.Information.ImageFormat == ImageInformation.ImageFormats.RGB24)
                            {
                                throw new InvalidOperationException($"Unsupported PNG RGB24 image - internal error.");
                            }

                            bool hasMask = ii.Information.ImageFormat == ImageInformation.ImageFormats.ARGB32;
                            var length = png.Width * 3 * png.Height;
                            var lengthMask = png.Width * png.Height;
                            var data = new Byte[length];
                            var mask = hasMask ? new Byte[lengthMask] : null;
                            ImagePrivateDataPng pngData;
                            ii.Data = pngData = new ImagePrivateDataPng(data, mask);
                            ii.Data.Image = ii;
                            int offset = 0;
                            int maskOffset = 0;
                            bool maskUsed = false;
                            for (int y = 0; y < png.Height; ++y)
                            {
                                for (int x = 0; x < png.Width; ++x)
                                {
                                    // TODO Add GetRow to PNG library?
                                    var pel = png.GetPixel(x, y);
                                    data[offset] = pel.R;
                                    data[offset + 1] = pel.G;
                                    data[offset + 2] = pel.B;
                                    offset += 3;
                                    if (hasMask)
                                    {
                                        mask![maskOffset++] = pel.A;
                                        maskUsed |= pel.A != 255;
                                    }
                                }
                            }

                            if (!maskUsed)
                            {
                                // No pixels with transparency found, delete the mask.
                                pngData.AlphaMask = null;
                            }
                        }
                        break;

                    case ImageInformation.ImageFormats.Palette1:
                        {
                            var hasAlpha = png.HasAlphaChannel;
                            var palette = png.GetPalette();
                            if (palette!.HasAlphaValues != hasAlpha)
                                throw new InvalidOperationException($"Unsupported PNG Palette4 image - internal error.");

                            var lineBytes = (png.Width + 1) / 2;
                            var length = lineBytes * png.Height;
                            var data = new Byte[length];
                            var alphaMask = hasAlpha ? new Byte[png.Width * png.Height] : null;
                            ImagePrivateDataPng pngData;
                            ii.Data = pngData = new ImagePrivateDataPng(data, alphaMask);
                            ii.Data.Image = ii;

                            uint colors = (uint)palette.Data.Length / 4;
                            ii.Information.ColorsUsed = colors;
                            pngData.PaletteData = new Byte[colors * 3];
                            var alpha = hasAlpha ? new Byte[colors] : null;
                            int offset = 0;
                            for (int c = 0; c < colors; ++c)
                            {
                                var pel = palette.GetPixel(c);
                                pngData.PaletteData[offset++] = pel.R;
                                pngData.PaletteData[offset++] = pel.G;
                                pngData.PaletteData[offset++] = pel.B;
                                //if (hasAlpha)
                                if (alpha != null)
                                    alpha[c] = pel.A;
                            }

                            var alphaUsed = false;
                            offset = 0;
                            var offsetAlpha = 0;
                            var bytesPerLine = (png.Width + 7) / 8;
                            for (int y = 0; y < png.Height; ++y)
                            {
                                for (int x = 0; x < bytesPerLine; ++x)
                                {
                                    // TODO Add GetRow to PNG library? Performance optimization.
                                    int pels = 0;
                                    for (var index = 0; index < 8; index++)
                                    {
                                        var pel = png.GetPixelIndex(x * 8 + index, y);
                                        pels |= pel << (7 - index);
                                        if (hasAlpha)
                                        {
                                            // alphaMask and alpha cannot be null here if hasAlpha is true. Suppress warnings in editor.
                                            Debug.Assert(alphaMask != null, nameof(alphaMask) + " != null");
                                            Debug.Assert(alpha != null, nameof(alpha) + " != null");

                                            alphaMask[offsetAlpha] = alpha[pel];
                                            alphaUsed |= alphaMask[offsetAlpha] != 255;
                                            ++offsetAlpha;
                                        }
                                    }
                                    data[offset] = (byte)pels;
                                    ++offset;
                                }
                            }

                            if (!alphaUsed)
                                pngData.AlphaMask = null;
                        }
                        break;

                    case ImageInformation.ImageFormats.Palette4:
                        {
                            var hasAlpha = png.HasAlphaChannel;
                            var palette = png.GetPalette();
                            if (palette!.HasAlphaValues != hasAlpha)
                                throw new InvalidOperationException($"Unsupported PNG Palette4 image - internal error.");

                            var lineBytes = (png.Width + 1) / 2;
                            var length = lineBytes * png.Height;
                            var data = new Byte[length];
                            var alphaMask = hasAlpha ? new Byte[png.Width * png.Height] : null;
                            ImagePrivateDataPng pngData;
                            ii.Data = pngData = new ImagePrivateDataPng(data, alphaMask);
                            ii.Data.Image = ii;

                            uint colors = (uint)palette.Data.Length / 4;
                            ii.Information.ColorsUsed = colors;
                            pngData.PaletteData = new Byte[colors * 3];
                            var alpha = hasAlpha ? new Byte[colors] : null;
                            int offset = 0;
                            for (int c = 0; c < colors; ++c)
                            {
                                var pel = palette.GetPixel(c);
                                pngData.PaletteData[offset++] = pel.R;
                                pngData.PaletteData[offset++] = pel.G;
                                pngData.PaletteData[offset++] = pel.B;
                                //if (hasAlpha)
                                if (alpha != null)
                                    alpha[c] = pel.A;
                            }

                            var alphaUsed = false;
                            offset = 0;
                            var offsetAlpha = 0;
                            for (int y = 0; y < png.Height; ++y)
                            {
                                for (int x = 0; x < png.Width; x += 2)
                                {
                                    // TODO Add GetRow to PNG library? Performance optimization.
                                    var pel = png.GetPixelIndex(x, y);
                                    var pel2 = x + 1 < png.Width ? png.GetPixelIndex(x + 1, y) : 0;
                                    data[offset] = (byte)(pel2 + (pel << 4));
                                    if (hasAlpha)
                                    {
                                        // alphaMask and alpha cannot be null here if hasAlpha is true. Suppress warnings in editor.
                                        Debug.Assert(alphaMask != null, nameof(alphaMask) + " != null");
                                        Debug.Assert(alpha != null, nameof(alpha) + " != null");

                                        alphaMask[offsetAlpha] = alpha[pel];
                                        alphaUsed |= alphaMask[offsetAlpha] != 255;
                                        if (x + 1 < png.Width)
                                        {
                                            alphaMask[offsetAlpha] = alpha[pel2];
                                            alphaUsed |= alphaMask[offsetAlpha] != 255;
                                        }
                                    }

                                    ++offset;
                                }
                            }

                            if (!alphaUsed)
                                pngData.AlphaMask = null;
                        }
                        break;

                    case ImageInformation.ImageFormats.Palette8:
                        {
                            var hasAlpha = png.HasAlphaChannel;
                            var palette = png.GetPalette();
                            if (palette!.HasAlphaValues != hasAlpha)
                                throw new InvalidOperationException($"Unsupported PNG Palette8 image - internal error.");

                            var length = png.Width * png.Height;
                            var data = new Byte[length];
                            var alphaMask = hasAlpha ? new Byte[length] : null;
                            ImagePrivateDataPng pngData;
                            ii.Data = pngData = new ImagePrivateDataPng(data, alphaMask);
                            ii.Data.Image = ii;

                            uint colors = (uint)palette.Data.Length / 4;
                            ii.Information.ColorsUsed = colors;
                            pngData.PaletteData = new Byte[colors * 3];
                            var alpha = hasAlpha ? new Byte[colors] : null;
                            int offset = 0;
                            for (int c = 0; c < colors; ++c)
                            {
                                var pel = palette.GetPixel(c);
                                pngData.PaletteData[offset++] = pel.R;
                                pngData.PaletteData[offset++] = pel.G;
                                pngData.PaletteData[offset++] = pel.B;
                                //if (hasAlpha)
                                if (alpha != null)
                                    alpha[c] = pel.A;
                            }

                            var alphaUsed = false;
                            offset = 0;
                            for (int y = 0; y < png.Height; ++y)
                            {
                                for (int x = 0; x < png.Width; ++x)
                                {
                                    // TODO Add GetRow to PNG library? Performance optimization.
                                    var pel = png.GetPixelIndex(x, y);
                                    data[offset] = (byte)pel;
                                    if (hasAlpha)
                                    {
                                        // alphaMask and alpha cannot be null here if hasAlpha is true. Suppress warnings in editor.
                                        Debug.Assert(alphaMask != null, nameof(alphaMask) + " != null");
                                        Debug.Assert(alpha != null, nameof(alpha) + " != null");

                                        alphaMask[offset] = alpha[pel];
                                        alphaUsed |= alphaMask[offset] != 255;
                                    }
                                    ++offset;
                                }
                            }

                            if (!alphaUsed)
                                pngData.AlphaMask = null;
                        }
                        break;

                    case ImageInformation.ImageFormats.Grayscale8:
                        {
                            var hasAlpha = png.HasAlphaChannel;

                            var length = png.Width * png.Height;
                            var data = new Byte[length];
                            var alphaMask = hasAlpha ? new Byte[length] : null;
                            ImagePrivateDataPng pngData;
                            ii.Data = pngData = new ImagePrivateDataPng(data, alphaMask);
                            ii.Data.Image = ii;

                            var alphaUsed = false;
                            var offset = 0;
                            for (int y = 0; y < png.Height; ++y)
                            {
                                for (int x = 0; x < png.Width; ++x)
                                {
                                    // TODO Add GetRow to PNG library? Performance optimization.
                                    var pel = png.GetPixel(x, y);
                                    data[offset] = pel.R;
                                    if (hasAlpha)
                                    {
                                        // alphaMask and alpha cannot be null here if hasAlpha is true. Suppress warnings in editor.
                                        Debug.Assert(alphaMask != null, nameof(alphaMask) + " != null");

                                        alphaMask[offset] = pel.A;
                                        alphaUsed |= alphaMask[offset] != 255;
                                    }

                                    ++offset;
                                }
                            }

                            if (!alphaUsed)
                                pngData.AlphaMask = null;
                        }
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported PNG format {ii.Information.ImageFormat}.");
                }
            }

            return true; // PNG image found.
        }

        class MyVisitor : IChunkVisitor
        {
            internal bool IsValid;
            internal bool IsMeter;
            internal int Vertical;
            internal int Horizontal;

            /// <summary>
            /// Invoked for every chunk of the PNG file. Used to extract additional information.
            /// </summary>
            public void Visit(Stream stream, ImageHeader header, ChunkHeader chunkHeader, byte[] data, byte[] crc)
            {
                if (chunkHeader.Name == "pHYs" && data.Length >= 9)
                {
                    // Get physical pixel dimensions.
                    IsMeter = data[8] == 1;
                    Horizontal = data[3] +
                        data[2] * 0x100 +
                        data[1] * 0x10000 +
                        data[0] * 0x1000000;
                    Vertical = data[7] +
                        data[6] * 0x100 +
                        data[5] * 0x10000 +
                        data[4] * 0x1000000;
                    IsValid = true;
                }
            }
        }

        public ImageData PrepareImage(ImagePrivateData data)
        {
            throw new NotImplementedException();
        }
#endif
#if GDI || WPF
        // Not used, not implemented.
        public ImageData PrepareImage(ImagePrivateData data)
        {
            throw new NotImplementedException();
        }
#endif
    }

#if CORE
    /// <summary>
    /// Data imported from PNG files. Used to prepare the data needed for PDF.
    /// </summary>
    class ImportedImagePng : ImportedImage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportedImagePng"/> class.
        /// </summary>
        public ImportedImagePng(IImageImporter importer)
            : base(importer)
        { }

        internal override ImageData PrepareImageData(PdfDocumentOptions options)
        {
            var data = (ImagePrivateDataPng?)Data ?? NRT.ThrowOnNull<ImagePrivateDataPng>();
            ImageDataBitmap imageData = new ImageDataBitmap(data.Bitmap, data.AlphaMask!); // NRT Check in constructor.

            if (data.PaletteData != null)
            {
                imageData.PaletteData = data.PaletteData;
                imageData.PaletteDataLength = data.PaletteData.Length;
            }
            return imageData;
        }
    }

    /// <summary>
    /// Image data needed for PDF bitmap images.
    /// </summary>
    class ImagePrivateDataPng : ImagePrivateData
    {
        public ImagePrivateDataPng(byte[] bitmap, byte[]? alphaMask)
        {
            Bitmap = bitmap;
            AlphaMask = alphaMask;
        }

        internal readonly byte[] Bitmap;
        internal byte[]? AlphaMask;

        internal byte[]? PaletteData { get; set; }
    }
#endif
}
