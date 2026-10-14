// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal.Imaging.Png.BigGustave;

namespace PdfSharp.Internal.Imaging
{
    sealed class PngImageImporter : BitmapImageImporter
    {
        public override bool TryImport(byte[] bitmap,
            [MaybeNullWhen(false)] out ImportedImage importedImage)
        {
            importedImage = null!;

            try
            {
                var dataParser = new DataParser(bitmap);
                dataParser.CurrentOffset = 0;

                if (TestPngFileHeader(dataParser))
                {
                    var result = new ImportedPngImage();
                    if (TestPngInfoHeader(dataParser, result))
                    {
                        result.ImageData = bitmap;

                        // Determine width and height.
                        UpdateWidthAndHeight(result);

                        importedImage = result;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Eat exceptions to have this image importer skipped.
                // We try to find an image importer that can handle the image.
                LogTryOpenFailure(nameof(PngImageImporter), ex);
            }

            return false;
        }

        /// <summary>
        /// A quick check for PNG files, checking the first 16 bytes.
        /// </summary>
        bool TestPngFileHeader(DataParser dataParser)
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
                if (dataParser.GetByte(offset) != headerByte)
                    return false;
                ++offset;
            }

            return true;
        }

        /// <summary>
        ///  Read information from PNG image header.
        /// </summary>
        private bool TestPngInfoHeader(DataParser dataParser, ImportedPngImage ii)
        {
            // Width: 4 bytes
            // Height: 4 bytes
            // Bit depth: 1 byte
            // Color type: 1 byte
            // Compression method: 1 byte
            // Filter method: 1 byte
            // Interlace method: 1 byte

            dataParser.CurrentOffset = 16;
            var width = dataParser.GetDWord(0, true);
            var height = dataParser.GetDWord(4, true);
            var bitDepth = dataParser.GetByte(8);
            var colorType = dataParser.GetByte(9);
            var compressionMethod = dataParser.GetByte(10);
            var filterMethod = dataParser.GetByte(11);
            var interlaceMethod = dataParser.GetByte(12);

            ii.PixelWidth = (int)width;
            ii.PixelHeight = (int)height;
            ii.DpiX = 96;
            ii.DpiY = 96;
            //ii.Information.HorizontalDPM = 0;
            //ii.Information.VerticalDPM = 0;
            //ii.Information.HorizontalAspectRatio = 0;
            //ii.Information.VerticalAspectRatio = 0;
            //ii.Information.DefaultDPI = 96; // Assume 96 DPI if information not provided in the file.

            ii.ColorsUsed = 0;
            // colorType can be 0, 2, 3, 4, or 6.
            switch (colorType)
            {
                case 0:
                    // Each pixel is a grayscale sample. 1,2,4,8,16.
                    switch (bitDepth)
                    {
                        case 8:
                            ii.ImageFormat = ImageFormats.Gray8;
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
                            ii.ImageFormat = ImageFormats.Bgr32;
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
                            ii.ImageFormat = ImageFormats.Indexed1;
                            break;
                        case 4:
                            ii.ImageFormat = ImageFormats.Indexed4;
                            break;
                        case 8:
                            ii.ImageFormat = ImageFormats.Indexed8;
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
                            ii.ImageFormat = ImageFormats.Gray8;
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported bit depth {bitDepth} for PNG color type {colorType}.");
                    }
                    break;

                case 6:
                    // Each pixel is an R,G,B triple, followed by an alpha sample. 8, 16.
                    switch (bitDepth)
                    {
                        case 8:
                            ii.ImageFormat = ImageFormats.Bgra32;
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
                var myVisitor = new MyVisitor();
                var png = PdfSharp.Internal.Imaging.Png.BigGustave.Png.Open(dataParser.Data!, myVisitor);

                if (png.Width != ii.PixelWidth ||
                    png.Height != ii.PixelHeight)
                {
                    throw new InvalidOperationException($"Unsupported PNG image - internal error.");
                }

                if (myVisitor.IsValid)
                {
                    var x = myVisitor.Horizontal;
                    var y = myVisitor.Vertical;
                    if (myVisitor.IsMeter)
                    {
                        // Unit is meter.
                        ii.DpiX = x / ImportedImage.InchPerMeter;
                        ii.DpiY = y / ImportedImage.InchPerMeter;
                        if (ii.DpiX == 0 || ii.DpiY == 0)
                        {
                            ii.DpiX = 96;
                            ii.DpiY = 96;
                        }
                        else
                        {
                            RoundDpiAfterConversionFromMetricValue(ii);
                        }
                    }
                    else
                    {
                        // Unit is unknown.
                        if (x > 0 && y > 0)
                        {
                            ii.DpiX = 96;
                            ii.DpiY = (float)(96d * x / y);
                        }
                    }
                }

                switch (ii.ImageFormat)
                {
                    // Copy image data later, only when it is needed? Or do it here because the stream is already open?
                    case ImageFormats.Bgr32:
                    case ImageFormats.Bgra32:
                        {
                            if (png.HasAlphaChannel != true &&
                                ii.ImageFormat == ImageFormats.Bgra32)
                            {
                                throw new InvalidOperationException($"Unsupported PNG ARGB32 image - internal error.");
                            }
                            if (png.HasAlphaChannel != false &&
                                ii.ImageFormat == ImageFormats.Bgr32)
                            {
                                throw new InvalidOperationException($"Unsupported PNG RGB24 image - internal error.");
                            }

                            bool hasMask = ii.ImageFormat == ImageFormats.Bgra32;
                            var length = png.Width * 3 * png.Height;
                            var lengthMask = png.Width * png.Height;
                            var data = new Byte[length];
                            var mask = hasMask ? new Byte[lengthMask] : null;
                            ii.ExtractedImageData = data;
                            ii.AlphaMaskData = mask;
                            int offset = 0;
                            int maskOffset = 0;
                            bool maskUsed = false;
                            for (int y = 0; y < png.Height; ++y)
                            {
                                for (int x = 0; x < png.Width; ++x)
                                {
                                    // Performance idea: Add GetRow to PNG library?
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
                                ii.AlphaMaskData = null;
                            }
                        }
                        break;

                    // Image with palette and 1 BPP.
                    case ImageFormats.Indexed1:
                        {
                            var hasAlpha = png.HasAlphaChannel;
                            var palette = png.GetPalette();
                            if (palette!.HasAlphaValues != hasAlpha)
                                throw new InvalidOperationException($"Unsupported PNG Palette1 image - internal error.");

                            var bytesPerLine = (png.Width + 7) / 8;
                            var length = bytesPerLine * png.Height;
                            var data = new Byte[length];
                            var alphaMask = hasAlpha ? new Byte[png.Width * png.Height] : null;
                            ii.ExtractedImageData = data;
                            ii.AlphaMaskData = alphaMask;

                            int colors = palette.Data.Length / 4;
                            ii.ColorsUsed = colors;
                            ii.PaletteData = new Byte[colors * 3];
                            var alpha = hasAlpha ? new Byte[colors] : null;
                            int offset = 0;
                            for (int c = 0; c < colors; ++c)
                            {
                                var pel = palette.GetPixel(c);
                                ii.PaletteData[offset++] = pel.R;
                                ii.PaletteData[offset++] = pel.G;
                                ii.PaletteData[offset++] = pel.B;
                                //if (hasAlpha)
                                if (alpha != null)
                                    alpha[c] = pel.A;
                            }

                            var alphaUsed = false;
                            offset = 0;
                            var offsetAlpha = 0;
                            for (int y = 0; y < png.Height; ++y)
                            {
                                for (int x = 0; x < bytesPerLine; ++x)
                                {
                                    // Performance idea: Add GetRow to PNG library?
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

                                            alphaMask![offsetAlpha] = alpha![pel];
                                            alphaUsed |= alphaMask[offsetAlpha] != 255;
                                            ++offsetAlpha;
                                        }
                                    }
                                    data[offset] = (byte)pels;
                                    ++offset;
                                }
                            }

                            if (!alphaUsed)
                                ii.AlphaMaskData = null;
                        }
                        break;

                    // Image with palette and 4 BPP.
                    case ImageFormats.Indexed4:
                        {
                            var hasAlpha = png.HasAlphaChannel;
                            var palette = png.GetPalette();
                            if (palette!.HasAlphaValues != hasAlpha)
                                throw new InvalidOperationException($"Unsupported PNG Palette4 image - internal error.");

                            var lineBytes = (png.Width + 1) / 2;
                            var length = lineBytes * png.Height;
                            var data = new Byte[length];
                            var alphaMask = hasAlpha ? new Byte[png.Width * png.Height] : null;
                            ii.ExtractedImageData = data;
                            ii.AlphaMaskData = alphaMask;

                            int colors = palette.Data.Length / 4;
                            ii.ColorsUsed = colors;
                            var paletteData = new Byte[colors * 3];
                            ii.PaletteData = paletteData;
                            var alpha = hasAlpha ? new Byte[colors] : null;
                            int offset = 0;
                            for (int c = 0; c < colors; ++c)
                            {
                                var pel = palette.GetPixel(c);
                                paletteData[offset++] = pel.R;
                                paletteData[offset++] = pel.G;
                                paletteData[offset++] = pel.B;
                                //if (hasAlpha)
                                alpha?[c] = pel.A;
                            }

                            var alphaUsed = false;
                            offset = 0;
                            var offsetAlpha = 0;
                            for (int y = 0; y < png.Height; ++y)
                            {
                                for (int x = 0; x < png.Width; x += 2)
                                {
                                    // Performance idea: Add GetRow to PNG library?
                                    var pel = png.GetPixelIndex(x, y);
                                    var pel2 = x + 1 < png.Width ? png.GetPixelIndex(x + 1, y) : 0;
                                    data[offset] = (byte)(pel2 + (pel << 4));
                                    if (hasAlpha)
                                    {
                                        // alphaMask and alpha cannot be null here if hasAlpha is true. Suppress warnings in editor.
                                        alphaMask![offsetAlpha] = alpha![pel];
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
                                ii.AlphaMaskData = null;
                        }
                        break;

                    // Image with palette and 8 BPP.
                    case ImageFormats.Indexed8:
                        {
                            var hasAlpha = png.HasAlphaChannel;
                            var palette = png.GetPalette();
                            if (palette!.HasAlphaValues != hasAlpha)
                                throw new InvalidOperationException($"Unsupported PNG Palette8 image - internal error.");

                            var length = png.Width * png.Height;
                            var data = new Byte[length];
                            var alphaMask = hasAlpha ? new Byte[length] : null;
                            ii.ExtractedImageData = data;
                            ii.AlphaMaskData = alphaMask;

                            int colors = palette.Data.Length / 4;
                            ii.ColorsUsed = colors;
                            var paletteData = new Byte[colors * 3];
                            ii.PaletteData = paletteData;
                            var alpha = hasAlpha ? new Byte[colors] : null;
                            int offset = 0;
                            for (int c = 0; c < colors; ++c)
                            {
                                var pel = palette.GetPixel(c);
                                paletteData[offset++] = pel.R;
                                paletteData[offset++] = pel.G;
                                paletteData[offset++] = pel.B;
                                alpha?[c] = pel.A;
                            }

                            var alphaUsed = false;
                            offset = 0;
                            for (int y = 0; y < png.Height; ++y)
                            {
                                for (int x = 0; x < png.Width; ++x)
                                {
                                    // Performance idea: Add GetRow to PNG library?
                                    var pel = png.GetPixelIndex(x, y);
                                    data[offset] = (byte)pel;
                                    if (hasAlpha)
                                    {
                                        // alphaMask and alpha cannot be null here if hasAlpha is true. Suppress warnings in editor.
                                        Debug.Assert(alphaMask != null, nameof(alphaMask) + " != null");
                                        Debug.Assert(alpha != null, nameof(alpha) + " != null");

                                        alphaMask![offset] = alpha![pel];
                                        alphaUsed |= alphaMask[offset] != 255;
                                    }
                                    ++offset;
                                }
                            }

                            if (!alphaUsed)
                                ii.AlphaMaskData = null;
                        }
                        break;

                    // Image with grayscale and 8 BPP.
                    case ImageFormats.Gray8:
                        {
                            var hasAlpha = png.HasAlphaChannel;

                            var length = png.Width * png.Height;
                            var data = new Byte[length];
                            var alphaMask = hasAlpha ? new Byte[length] : null;
                            ii.ExtractedImageData = data;
                            ii.AlphaMaskData = alphaMask;

                            var alphaUsed = false;
                            var offset = 0;
                            for (int y = 0; y < png.Height; ++y)
                            {
                                for (int x = 0; x < png.Width; ++x)
                                {
                                    // Performance idea: Add GetRow to PNG library?
                                    var pel = png.GetPixel(x, y);
                                    data[offset] = pel.R;
                                    if (hasAlpha)
                                    {
                                        // alphaMask and alpha cannot be null here if hasAlpha is true. Suppress warnings in editor.
                                        Debug.Assert(alphaMask != null, nameof(alphaMask) + " != null");

                                        alphaMask![offset] = pel.A;
                                        alphaUsed |= alphaMask[offset] != 255;
                                    }

                                    ++offset;
                                }
                            }

                            if (!alphaUsed)
                                ii.AlphaMaskData = null;
                        }
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported PNG format {ii.ImageFormat}.");
                }
            }

            return true; // PNG image found.
        }

        class MyVisitor : IChunkVisitor
        {
            internal bool IsValid;
            // Unit is Meter if true, otherwise unknown.
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
    }
}
