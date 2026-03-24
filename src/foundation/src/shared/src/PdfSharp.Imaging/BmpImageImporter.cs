// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Internal.Imaging
{
    sealed class BmpImageImporter : BitmapImageImporter
    {
        public override bool TryImport(byte[] bitmap,
            [MaybeNullWhen(false)] out ImportedImage importedImage)
        {
            importedImage = null;

            try
            {
                Reset();

                var dataParser = new DataParser(bitmap);
                dataParser.CurrentOffset = 0;

                if (TestBitmapFileHeader(dataParser, out var offsetImageData))
                {
                    var result = new ImportedBmpImage();
                    if (TestBitmapInfoHeader(dataParser, result, offsetImageData, out var colorPaletteOffset))
                    {
                        result.ImageData = bitmap;

                        // Determine width and height.
                        UpdateWidthAndHeight(result);

                        importedImage = result;

                        var importer = new BmpImporter(bitmap, result, offsetImageData, colorPaletteOffset, HaveRgbMask);
                        importer.CopyBitmap();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Eat exceptions to have this image importer skipped.
                // We try to find an image importer that can handle the image.
                LogTryOpenFailure(nameof(BmpImageImporter), ex);
            }

            return false;
        }

        void Reset()
        {
            HaveRgbMask = false;
        }

        bool TestBitmapFileHeader(DataParser dataParser, out int offset)
        {
            offset = 0;
            // File must start with "BM".
            if (dataParser.GetWord(0, true) == 0x424d)
            {
                int fileSize = (int)dataParser.GetDWord(2, false);
                // Integrity check: fileSize set in BM header should match size of the stream.
                // We test "<" instead of "!=" to allow extra bytes at the end of the stream.
                if (fileSize < dataParser.Length)
                    return false;

                offset = (int)dataParser.GetDWord(10, false);
                dataParser.CurrentOffset += 14;
                return true;
            }
            return false;
        }

        bool TestBitmapInfoHeader(DataParser dataParser, ImportedBmpImage ii, int offset, out int colorPaletteOffset)
        {
            colorPaletteOffset = 0;

            int size = (int)dataParser.GetDWord(0, false);
            if (size is 40 or 108 or 124) // sizeof BITMAPINFOHEADER == 40, sizeof BITMAPV4HEADER == 108, sizeof BITMAPV5HEADER == 124
            {
                int width = (int)dataParser.GetDWord(4, false);
                int height = (int)dataParser.GetDWord(8, false);
                int planes = dataParser.GetWord(12, false);
                int bitCount = dataParser.GetWord(14, false);
                int compression = (int)dataParser.GetDWord(16, false);
                int sizeImage = (int)dataParser.GetDWord(20, false);
                int xPelsPerMeter = (int)dataParser.GetDWord(24, false);
                int yPelsPerMeter = (int)dataParser.GetDWord(28, false);
                int colorsUsed = (int)dataParser.GetDWord(32, false);
                int colorsImportant = (int)dataParser.GetDWord(36, false);
                // TODO_OLD Integrity and plausibility checks.
                if (sizeImage != 0 && sizeImage + offset > dataParser.Length)
                    return false;

                if (xPelsPerMeter > 0 && yPelsPerMeter > 0)
                {
                    // Calculate from dots per meter.
                    ii.DpiX = xPelsPerMeter / ImportedImage.InchPerMeter;
                    ii.DpiY = yPelsPerMeter / ImportedImage.InchPerMeter;
                    RoundDpiAfterConversionFromMetricValue(ii);
                }
                else
                {
                    // Use default values.
                    ii.DpiX = 96;
                    ii.DpiY = 96;
                }

                // Return true only for supported formats.
                if (compression is 0 or 3) // BI_RGB == 0, BI_BITFIELDS == 3
                {
                    //data.Offset = offset;
                    colorPaletteOffset = dataParser.CurrentOffset + size;
                    ii.BitCount = bitCount;
                    ii.PixelWidth = width;
                    ii.PixelHeight = Math.Abs(height);
                    // Implement flipped images when we encounter files that require this.
                    var flippedImage = height < 0;
                    if (planes == 1 && bitCount == 24)
                    {
                        // RGB24
                        ii.ImageFormat = ImageFormats.Bgr32;

                        // Do we have to verify Mask if size >= 108 && compression == 3 also for bitCount == 24?
                        return true;
                    }
                    if (planes == 1 && bitCount == 32)
                    {
                        // ARGB32
                        ii.ImageFormat = ImageFormats.Bgra32;

                        // Mask for bytes if size >= 108 && compression == 3.
                        HaveRgbMask = size >= 108 && compression == 3;

                        // Verify Mask if size >= 108 && compression == 3.
                        if (HaveRgbMask)
                        {
                            uint maskRed = dataParser.GetDWord(40, false);
                            uint maskGreen = dataParser.GetDWord(44, false);
                            uint maskBlue = dataParser.GetDWord(48, false);
                            uint maskAlpha = dataParser.GetDWord(52, false);
                            // Reject images that do not have the expected byte order.
                            if (maskRed != 0xff000000 ||
                                maskGreen != 0x00ff0000 ||
                                maskBlue != 0x0000ff00 ||
                                maskAlpha != 0x000000ff)
                            {
                                // Not yet supported.
                                return false;
                            }
                        }
                        return true;
                    }
                    if (planes == 1 && bitCount == 8)
                    {
                        // Palette8
                        ii.ImageFormat = ImageFormats.Indexed8;
                        ii.ColorsUsed = colorsUsed;

                        return true;
                    }
                    if (planes == 1 && bitCount == 4)
                    {
                        // Palette4
                        ii.ImageFormat = ImageFormats.Indexed4;
                        ii.ColorsUsed = colorsUsed;

                        return true;
                    }
                    if (planes == 1 && bitCount == 1)
                    {
                        // Palette1
                        ii.ImageFormat = ImageFormats.Indexed1;
                        ii.ColorsUsed = colorsUsed;

                        return true;
                    }
                }
            }
            return false;
        }

        bool HaveRgbMask { get; set; } = false;
    }

    /// <summary>
    /// Imports data from a BMP "file" into an ImportedBmpImage.
    /// </summary>
    class BmpImporter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BmpImporter"/> class.
        /// </summary>
        public BmpImporter(byte[] data, ImportedBmpImage image,
            int offset, int colorPaletteOffset, bool haveRgbMask)
        {
            Data = data;
            Image = image;
            Offset = offset;
            ColorPaletteOffset = colorPaletteOffset;
            HaveRgbMask = haveRgbMask;
        }

        public void CopyBitmap()
        {
            switch (Image.ImageFormat)
            {
                case ImageFormats.Bgra32:
                    CopyTrueColorMemoryBitmap(4, 8, true);
                    break;

                case ImageFormats.Bgr32:
                    if (Image.BitCount == 32)
                        CopyTrueColorMemoryBitmap(4, 8, false);
                    else
                        CopyTrueColorMemoryBitmap(3, 8, false);
                    break;

                case ImageFormats.Indexed8:
                    CopyIndexedMemoryBitmap(8, true/*, options*/);
                    break;

                case ImageFormats.Indexed4:
                    CopyIndexedMemoryBitmap(4, true/*, options*/);
                    break;

                case ImageFormats.Indexed1:
                    CopyIndexedMemoryBitmap(1, false/*, options*/);
                    break;

                default:
                    throw new NotSupportedException($"Image format {Image.ImageFormat} is not implemented here.");
            }
        }

        /// <summary>
        /// Copies images without color palette.
        /// </summary>
        /// <param name="components">4 (32bpp RGB), 3 (24bpp RGB, 32bpp ARGB)</param>
        /// <param name="bits">8</param>
        /// <param name="hasAlpha">true (ARGB), false (RGB)</param>
        void CopyTrueColorMemoryBitmap(int components, int bits, bool hasAlpha)
        {
            int width = Image.PixelWidth;
            int height = Image.PixelHeight;

            int logicalComponents = components;
            if (components == 4)
                logicalComponents = 3;

            byte[] imageData = new byte[logicalComponents * width * height];

            bool hasMask = false;
            bool hasAlphaMask = false;
            byte[]? alphaMask = hasAlpha ? new byte[width * height] : null;
            MonochromeMask? mask = hasAlpha ?
                new MonochromeMask(width, height) : null;

            int nFileOffset = Offset;
            int nOffsetRead = 0;
            bool maskAllTransparent = true;
            if (logicalComponents == 3)
            {
                if (hasAlpha && HaveRgbMask)
                {
                    // Improvement: Get byte order for V5 bitmaps from bitmap header.
                    // https://docs.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapv5header
                    // "If the bV5Compression member of the BITMAPV5HEADER is BI_BITFIELDS, the bmiColors member contains three DWORD color masks that specify the red, green, and blue components of each pixel. Each DWORD in the bitmap array represents a single pixel."

                    // Experimental: Expected BGRA, but found ARGB.
                    ++nFileOffset;
                }

                for (int y = 0; y < height; ++y)
                {
                    // Handle flipped images if we find files that require it.
                    int nOffsetWrite = 3 * (height - 1 - y) * width;
                    int nOffsetWriteAlpha = 0;
                    if (hasAlpha)
                    {
                        mask!.StartLine(y); // NRT
                        nOffsetWriteAlpha = (height - 1 - y) * width;
                    }

                    for (int x = 0; x < width; ++x)
                    {
                        imageData[nOffsetWrite] = Data[nFileOffset + nOffsetRead + 2];
                        imageData[nOffsetWrite + 1] = Data[nFileOffset + nOffsetRead + 1];
                        imageData[nOffsetWrite + 2] = Data[nFileOffset + nOffsetRead];
                        if (hasAlpha)
                        {
                            //byte maskValue = Data[nFileOffset + nOffsetRead + 3]; // Expected, but does not always work.
                            byte maskValue = HaveRgbMask ?
                                Data[nFileOffset + nOffsetRead - 1] : // Experimental. See above.
                                Data[nFileOffset + nOffsetRead + 3];
                            mask!.AddPel(maskValue);
                            alphaMask![nOffsetWriteAlpha] = maskValue;
                            if (!hasMask || !hasAlphaMask || maskAllTransparent)
                            {
                                if (maskValue != 255)
                                {
                                    hasMask = true; // Found a value different from 100 %.
                                    if (maskValue != 0)
                                        hasAlphaMask = true; // Found a value different from 0 % and 100 %.
                                }
                                else
                                {
                                    maskAllTransparent = false; // Found a value different from 0 %.
                                }
                            }
                            ++nOffsetWriteAlpha;
                        }
                        nOffsetRead += hasAlpha ? 4 : components;
                        nOffsetWrite += 3;
                    }
                    nOffsetRead = 4 * ((nOffsetRead + 3) / 4); // Align to 32 bit boundary
                }
            }
            else if (components == 1)
            {
                // Grayscale
                throw new NotSupportedException("Image format not supported (grayscales).");
            }

            Image.ExtractedImageData = imageData;

            if (alphaMask != null && hasAlphaMask)
            {
                Image.AlphaMaskData = alphaMask;
            }

            if (mask != null && hasMask && !maskAllTransparent)
            {
                Image.BitmapMaskData = mask.MaskData;
                Image.BitmapMask = mask;
            }
        }

        void CopyIndexedMemoryBitmap(int bits, bool checkTransparency)
        {
            int firstMaskColor = -1, lastMaskColor = -1;
            bool segmentedColorMask = false;

            int bytesColorPaletteOffset = ColorPaletteOffset; // GDI+ always returns Windows bitmaps: sizeof BITMAPFILEHEADER + sizeof BITMAPINFOHEADER

            int bytesFileOffset = Offset;
            int paletteColors = Image.ColorsUsed;
            int width = Image.PixelWidth;
            int height = Image.PixelHeight;

            MonochromeMask? mask = checkTransparency ? new MonochromeMask(width, height) : null;

            bool isGray = bits == 8 && (paletteColors == 256 || paletteColors == 0);
            int isBitonal = 0; // 0: false; >0: true; <0: true (inverted)
            byte[] paletteData = new byte[3 * paletteColors];
            for (int color = 0; color < paletteColors; ++color)
            {
                paletteData[3 * color] = Data[bytesColorPaletteOffset + 4 * color + 2];
                paletteData[3 * color + 1] = Data[bytesColorPaletteOffset + 4 * color + 1];
                paletteData[3 * color + 2] = Data[bytesColorPaletteOffset + 4 * color + 0];
                if (isGray)
                    isGray = paletteData[3 * color] == paletteData[3 * color + 1] &&
                      paletteData[3 * color] == paletteData[3 * color + 2];

                if (checkTransparency && Data[bytesColorPaletteOffset + 4 * color + 3] < 128)
                {
                    // We treat this as transparency:
                    if (firstMaskColor == -1)
                        firstMaskColor = color;
                    if (lastMaskColor == -1 || lastMaskColor == color - 1)
                        lastMaskColor = color;
                    if (lastMaskColor != color)
                        segmentedColorMask = true;
                }
                //else
                //{
                //  // We treat this as opacity:
                //}
            }

            if (firstMaskColor == 0 && lastMaskColor == paletteColors - 1)
            {
                // If all colors are masked out, ignore mask colors.
                firstMaskColor = -1;
                lastMaskColor = -1;
            }

            if (bits == 1)
            {
                if (paletteColors == 0)
                    isBitonal = 1;
                if (paletteColors == 2)
                {
                    if (paletteData[0] == 0 &&
                      paletteData[1] == 0 &&
                      paletteData[2] == 0 &&
                      paletteData[3] == 255 &&
                      paletteData[4] == 255 &&
                      paletteData[5] == 255)
                        isBitonal = 1; // Black on white
                    if (paletteData[5] == 0 &&
                      paletteData[4] == 0 &&
                      paletteData[3] == 0 &&
                      paletteData[2] == 255 &&
                      paletteData[1] == 255 &&
                      paletteData[0] == 255)
                        isBitonal = -1; // White on black
                }
            }

            // NYI: (no sample found where this was required) 
            // if (segmentedColorMask = true)
            // { ... }

            byte[] imageData = new byte[((width * bits + 7) / 8) * height];
            //bool isFGaxEncoded = false;
            //byte[]? imageDataFax = null;
            //int k = 0;

            // No fax encoding here
#if false
            if (bits == 1 && options.EnableCcittCompressionForBilevelImages)
            {
                // We try  Group 4 (2D) encoding here and keep the smaller byte array.
                byte[] tempG4 = new byte[imageData.Length];
                // Size will be 0 if fax encoding increases the size.
                int ccittSizeG4 = PdfImage.DoFaxEncodingGroup4(ref tempG4, Data, (uint)bytesFileOffset, (uint)width, (uint)height);

                isFGaxEncoded = ccittSizeG4 > 0;
                if (isFGaxEncoded)
                {
                    Array.Resize(ref tempG4, ccittSizeG4);
                    imageDataFax = tempG4;
                    k = -1;
                }
            }
#endif

            {
                int bytesOffsetRead = 0;
                if (bits == 8 || bits == 4 || bits == 1)
                {
                    int bytesPerLine = (width * bits + 7) / 8;
                    for (int y = 0; y < height; ++y)
                    {
                        mask?.StartLine(y);
                        int bytesOffsetWrite = (height - 1 - y) * ((width * bits + 7) / 8);
                        for (int x = 0; x < bytesPerLine; ++x)
                        {
                            if (isGray)
                            {
                                // Lookup the gray value from the palette:
                                imageData[bytesOffsetWrite] = paletteData[3 * Data[bytesFileOffset + bytesOffsetRead]];
                            }
                            else
                            {
                                // Store the palette index.
                                imageData[bytesOffsetWrite] = Data[bytesFileOffset + bytesOffsetRead];
                            }
                            if (firstMaskColor != -1)
                            {
                                int n = Data[bytesFileOffset + bytesOffsetRead];
                                if (bits == 8)
                                {
                                    // Support segmentedColorMask when we encounter files that require it.
                                    mask?.AddPel((n >= firstMaskColor) && (n <= lastMaskColor));
                                }
                                else if (bits == 4)
                                {
                                    // Support segmentedColorMask when we encounter files that require it.
                                    int n1 = (n & 0xf0) / 16;
                                    int n2 = (n & 0x0f);
                                    mask?.AddPel((n1 >= firstMaskColor) && (n1 <= lastMaskColor));
                                    mask?.AddPel((n2 >= firstMaskColor) && (n2 <= lastMaskColor));
                                }
                                else // (bits == 1)
                                {
                                    // Support segmentedColorMask when we encounter files that require it.
                                    for (int bit = 1; bit <= 8; ++bit)
                                    {
                                        int n1 = (n & 0x80) / 128;
                                        mask?.AddPel((n1 >= firstMaskColor) && (n1 <= lastMaskColor));
                                        n *= 2;
                                    }
                                }
                            }
                            bytesOffsetRead += 1;
                            bytesOffsetWrite += 1;
                        }
                        bytesOffsetRead = 4 * ((bytesOffsetRead + 3) / 4); // Align to 32 bit boundary
                    }
                }
                else
                {
                    throw new NotSupportedException("ReadIndexedMemoryBitmap: unsupported format #3");
                }
            }

            Image.ExtractedImageData = imageData;

            // CCITT FAX encoding not yet supported, but has no advantages over FlateDecode.
#if false
            if (imageDataFax != null)
            {
                Image.CcittFaxData = imageDataFax;
            }
            Image.CcittFaxK = k;
#endif

            Image.IsGray = isGray;
            Image.Bitonal = isBitonal;
            Image.BytesFileOffset = bytesFileOffset;

            Image.PaletteData = paletteData;
            Image.PaletteColors = paletteColors;
            Image.SegmentedColorMask = segmentedColorMask;
            Image.FirstMaskColor = firstMaskColor;
            Image.LastMaskColor = lastMaskColor;

            if (mask != null && mask.MaskUsed && firstMaskColor != -1)
            {
                Image.BitmapMaskData = mask.MaskData;
                Image.BitmapMask = mask;
            }
        }

        public byte[] Data { get; }
        public ImportedBmpImage Image { get; }
        public int Offset { get; }
        public int ColorPaletteOffset { get; }
        public bool HaveRgbMask { get; }
    }

    /// <summary>
    /// Helper class for creating bitmap masks (8 pels per byte).
    /// </summary>
    public class MonochromeMask
    {
        /// <summary>
        /// Returns the bitmap mask that will be written to PDF.
        /// </summary>
        public byte[] MaskData => _maskData;

        readonly byte[] _maskData;

        /// <summary>
        /// Indicates whether the mask has transparent pels.
        /// </summary>
        public bool MaskUsed => _used;

        /// <summary>
        /// Creates a bitmap mask.
        /// </summary>
        public MonochromeMask(int sizeX, int sizeY)
        {
            _sizeX = sizeX;
            _sizeY = sizeY;
            int byteSize = ((sizeX + 7) / 8) * sizeY;
            _maskData = new byte[byteSize];
            StartLine(0);
        }

        /// <summary>
        /// Starts a new line.
        /// </summary>
        public void StartLine(int newCurrentLine)
        {
            _bitsWritten = 0;
            _byteBuffer = 0;
            _writeOffset = ((_sizeX + 7) / 8) * (_sizeY - 1 - newCurrentLine);
        }

        /// <summary>
        /// Adds a pel to the current line.
        /// </summary>
        /// <param name="isTransparent"></param>
        public void AddPel(bool isTransparent)
        {
            if (_bitsWritten < _sizeX)
            {
                // Mask: 0: opaque, 1: transparent (default mapping)
                if (isTransparent)
                {
                    _byteBuffer = (_byteBuffer << 1) + 1;
                    _used = true;
                }
                else
                    _byteBuffer = _byteBuffer << 1;
                ++_bitsWritten;
                if ((_bitsWritten & 7) == 0)
                {
                    _maskData[_writeOffset] = (byte)_byteBuffer;
                    ++_writeOffset;
                    _byteBuffer = 0;
                }
                else if (_bitsWritten == _sizeX)
                {
                    int n = 8 - (_bitsWritten & 7);
                    _byteBuffer = _byteBuffer << n;
                    _maskData[_writeOffset] = (byte)_byteBuffer;
                }
            }
        }

        /// <summary>
        /// Adds a pel from an alpha mask value.
        /// </summary>
        public void AddPel(int shade)
        {
            // NYI: dithering.
            AddPel(shade < 128);
        }

        readonly int _sizeX;
        readonly int _sizeY;
        int _writeOffset;
        int _byteBuffer;
        int _bitsWritten;
        bool _used;
    }
}
