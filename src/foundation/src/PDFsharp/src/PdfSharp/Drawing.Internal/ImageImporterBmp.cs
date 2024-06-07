// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using static PdfSharp.Drawing.ImageInformation;

namespace PdfSharp.Drawing.Internal
{
    class ImageImporterBmp : ImageImporterRoot, IImageImporter
    {
        public ImportedImage? ImportImage(StreamReaderHelper stream)
        {
            try
            {
                stream.CurrentOffset = 0;
                if (TestBitmapFileHeader(stream, out var offsetImageData))
                {
                    // Note: TestBitmapFileHeader updates stream.CurrentOffset on success.

                    ImagePrivateDataBitmap ipd = new ImagePrivateDataBitmap(stream.Data, stream.Length);
                    ImportedImage ii = new ImportedImageBitmap(this, ipd);
                    ii.Information.DefaultDPI = 96; // Assume 96 DPI if information not provided in the file.

                    if (TestBitmapInfoHeader(stream, ii, offsetImageData))
                    {
                        return ii;
                    }
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
            }
            return null;
        }

        bool TestBitmapFileHeader(StreamReaderHelper stream, out int offset)
        {
            offset = 0;
            // File must start with "BM".
            if (stream.GetWord(0, true) == 0x424d)
            {
                int filesize = (int)stream.GetDWord(2, false);
                // Integrity check: filesize set in BM header should match size of the stream.
                // We test "<" instead of "!=" to allow extra bytes at the end of the stream.
                if (filesize < stream.Length)
                    return false;

                offset = (int)stream.GetDWord(10, false);
                stream.CurrentOffset += 14;
                return true;
            }
            return false;
        }

        bool TestBitmapInfoHeader(StreamReaderHelper stream, ImportedImage ii, int offset)
        {
            int size = (int)stream.GetDWord(0, false);
            if (size == 40 || size == 108 || size == 124) // sizeof BITMAPINFOHEADER == 40, sizeof BITMAPV4HEADER == 108, sizeof BITMAPV5HEADER == 124
            {
                uint width = stream.GetDWord(4, false);
                int height = (int)stream.GetDWord(8, false);
                int planes = stream.GetWord(12, false);
                uint bitCount = stream.GetWord(14, false);
                int compression = (int)stream.GetDWord(16, false);
                int sizeImage = (int)stream.GetDWord(20, false);
                int xPelsPerMeter = (int)stream.GetDWord(24, false);
                int yPelsPerMeter = (int)stream.GetDWord(28, false);
                uint colorsUsed = stream.GetDWord(32, false);
                uint colorsImportant = stream.GetDWord(36, false);
                // TODO Integrity and plausibility checks.
                if (sizeImage != 0 && sizeImage + offset > stream.Length)
                    return false;

                var privateData = (ImagePrivateDataBitmap?)ii.Data;

                // Return true only for supported formats.
                if (compression is 0 or 3) // BI_RGB == 0, BI_BITFIELDS == 3
                {
                    var data = (ImagePrivateDataBitmap?)ii.Data ?? NRT.ThrowOnNull<ImagePrivateDataBitmap>();
                    //((ImagePrivateDataBitmap)ii.Data).Offset = offset;
                    //((ImagePrivateDataBitmap)ii.Data).ColorPaletteOffset = stream.CurrentOffset + size;
                    data.Offset = offset;
                    data.ColorPaletteOffset = stream.CurrentOffset + size;
                    ii.Information.BitCount = bitCount;
                    ii.Information.Width = width;
                    ii.Information.Height = (uint)Math.Abs(height);
                    ii.Information.HorizontalDPM = xPelsPerMeter;
                    ii.Information.VerticalDPM = yPelsPerMeter;
                    if (privateData == null)
                        NRT.ThrowOnNull();
                    privateData.FlippedImage = height < 0;
                    if (planes == 1 && bitCount == 24)
                    {
                        // RGB24
                        ii.Information.ImageFormat = ImageInformation.ImageFormats.RGB24;

                        // TODO: Verify Mask if size >= 108 && compression == 3.
                        return true;
                    }
                    if (planes == 1 && bitCount == 32)
                    {
                        // ARGB32
                        //ii.Information.ImageFormat = ImageInformation.ImageFormats.ARGB32;
                        ii.Information.ImageFormat = compression == 0 ?
                            ImageInformation.ImageFormats.RGB24 :
                            ImageInformation.ImageFormats.ARGB32;

                        // TODO: tell RGB from ARGB. Idea: assume RGB if alpha is always 0.

                        // Verify Mask if size >= 108 && compression == 3.
                        if (size >= 108 && compression == 3)
                        {
                            uint maskRed = stream.GetDWord(40, false);
                            uint maskGreen = stream.GetDWord(44, false);
                            uint maskBlue = stream.GetDWord(48, false);
                            uint maskAlpha = stream.GetDWord(52, false);
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
                        ii.Information.ImageFormat = ImageInformation.ImageFormats.Palette8;
                        ii.Information.ColorsUsed = colorsUsed;

                        return true;
                    }
                    if (planes == 1 && bitCount == 4)
                    {
                        // Palette4
                        ii.Information.ImageFormat = ImageInformation.ImageFormats.Palette4;
                        ii.Information.ColorsUsed = colorsUsed;

                        return true;
                    }
                    if (planes == 1 && bitCount == 1)
                    {
                        // Palette1
                        ii.Information.ImageFormat = ImageInformation.ImageFormats.Palette1;
                        ii.Information.ColorsUsed = colorsUsed;

                        return true;
                    }
                    // TODO Implement more formats!
                }
            }
            return false;
        }

        public ImageData PrepareImage(ImagePrivateData data)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Bitmap refers to the format used in PDF. Will be used for BMP, PNG, TIFF, GIF, and others.
    /// </summary>
    class ImportedImageBitmap : ImportedImage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportedImageBitmap"/> class.
        /// </summary>
        public ImportedImageBitmap(IImageImporter importer, ImagePrivateDataBitmap data)
            : base(importer, data)
        { }

        internal override ImageData PrepareImageData(PdfDocumentOptions options)
        {
            ImagePrivateDataBitmap data = (ImagePrivateDataBitmap?)Data ?? NRT.ThrowOnNull<ImagePrivateDataBitmap>();
            ImageDataBitmap imageData = new ImageDataBitmap(options);
            //imageData.Data = data.Data;
            //imageData.Length = data.Length;

            data.CopyBitmap(imageData, options);

            return imageData;
        }
    }

    /// <summary>
    /// Contains data needed for PDF. Will be prepared when needed.
    /// Bitmap refers to the format used in PDF. Will be used for BMP, PNG, TIFF, GIF, and others.
    /// </summary>
    class ImageDataBitmap : ImageData
    {
        internal ImageDataBitmap(PdfDocumentOptions options)
        {
            Options = options;
        }

        internal ImageDataBitmap(byte[] data, byte[] mask)
        {
            Data = data;
            Length = Data.Length;
            AlphaMask = mask;
            AlphaMaskLength = AlphaMask?.Length ?? 0;
            // TODO Bitmap mask?
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        public byte[] Data { get; internal set; } = null!;

        /// <summary>
        /// Gets the length.
        /// </summary>
        public int Length { get; internal set; }

        /// <summary>
        /// Gets the data for the CCITT format.
        /// </summary>
        public byte[]? DataFax { get; internal set; } = null;

        /// <summary>
        /// Gets the length.
        /// </summary>
        public int LengthFax { get; internal set; }

        public byte[] AlphaMask { get; internal set; } = null!;

        public int AlphaMaskLength { get; internal set; }

        public byte[] BitmapMask { get; internal set; } = null!;

        public int BitmapMaskLength { get; internal set; }

        public byte[] PaletteData { get; set; } = null!;

        public int PaletteDataLength { get; set; }

        public bool SegmentedColorMask;

        public int IsBitonal;

        public int K;

        public bool IsGray;

        internal readonly PdfDocumentOptions? Options;
    }

    /// <summary>
    /// Image data needed for Windows bitmap images.
    /// </summary>
    class ImagePrivateDataBitmap : ImagePrivateData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePrivateDataBitmap"/> class.
        /// </summary>
        public ImagePrivateDataBitmap(byte[] data, int length)
        {
            _data = data;
            _length = length;
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        public byte[] Data => _data;

        readonly byte[] _data;

        /// <summary>
        /// Gets the length.
        /// </summary>
        public int Length => _length;

        readonly int _length;

        /// <summary>
        /// True if first line is the top line, false if first line is the bottom line of the image. When needed, lines will be reversed while converting data into PDF format.
        /// </summary>
        internal bool FlippedImage;

        /// <summary>
        /// The offset of the image data in Data.
        /// </summary>
        internal int Offset;

        /// <summary>
        /// The offset of the color palette in Data.
        /// </summary>
        internal int ColorPaletteOffset;

        internal void CopyBitmap(ImageDataBitmap dest, PdfDocumentOptions options)
        {
            switch (Image?.Information.ImageFormat ?? NRT.ThrowOnNull<ImageFormats>())
            {
                case ImageInformation.ImageFormats.ARGB32:
                    CopyTrueColorMemoryBitmap(4, 8, true, dest);
                    break;

                case ImageInformation.ImageFormats.RGB24:
                    if (this.Image.Information.BitCount == 32)
                        CopyTrueColorMemoryBitmap(4, 8, false, dest);
                    else
                        CopyTrueColorMemoryBitmap(3, 8, false, dest);
                    break;

                case ImageInformation.ImageFormats.Palette8:
                    CopyIndexedMemoryBitmap(8, false, dest, options);
                    break;

                case ImageInformation.ImageFormats.Palette4:
                    CopyIndexedMemoryBitmap(4, false, dest, options);
                    break;

                case ImageInformation.ImageFormats.Palette1:
                    CopyIndexedMemoryBitmap(1, false, dest, options);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Copies images without color palette.
        /// </summary>
        /// <param name="components">4 (32bpp RGB), 3 (24bpp RGB, 32bpp ARGB)</param>
        /// <param name="bits">8</param>
        /// <param name="hasAlpha">true (ARGB), false (RGB)</param>
        /// <param name="dest">Destination </param>
        void CopyTrueColorMemoryBitmap(int components, int bits, bool hasAlpha, ImageDataBitmap dest)
        {
            int width = (int)Image.Information.Width;
            int height = (int)Image.Information.Height;

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
            if (logicalComponents == 3)
            {
                if (hasAlpha)
                {
                    // $THHO TODO: Get byte order for V5 bitmaps from bitmap header.
                    // https://docs.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapv5header
                    // "If the bV5Compression member of the BITMAPV5HEADER is BI_BITFIELDS, the bmiColors member contains three DWORD color masks that specify the red, green, and blue components of each pixel. Each DWORD in the bitmap array represents a single pixel."

                    // Experimental: Expected BGRA, but found ARGB.
                    ++nFileOffset;
                }

                for (int y = 0; y < height; ++y)
                {
                    // TODO Handle Flipped.
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
                            byte maskValue = Data[nFileOffset + nOffsetRead - 1]; // Experimental. See above.
                            mask!.AddPel(maskValue);
                            alphaMask![nOffsetWriteAlpha] = maskValue;
                            if (!hasMask || !hasAlphaMask)
                            {
                                if (maskValue != 255)
                                {
                                    hasMask = true;
                                    if (maskValue != 0)
                                        hasAlphaMask = true;
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
                throw new NotImplementedException("Image format not supported (grayscales).");
            }

            dest.Data = imageData;
            dest.Length = imageData.Length;

            if (alphaMask != null && hasAlphaMask)
            {
                dest.AlphaMask = alphaMask;
                dest.AlphaMaskLength = alphaMask.Length;
            }

            if (mask != null && hasMask)
            {
                dest.BitmapMask = mask.MaskData;
                dest.BitmapMaskLength = mask.MaskData.Length;
            }
        }

        void CopyIndexedMemoryBitmap(int bits, bool checkTransparency, ImageDataBitmap dest, PdfDocumentOptions options)
        {
            int firstMaskColor = -1, lastMaskColor = -1;
            bool segmentedColorMask = false;

            int bytesColorPaletteOffset = ((ImagePrivateDataBitmap)Image.Data!).ColorPaletteOffset; // GDI+ always returns Windows bitmaps: sizeof BITMAPFILEHEADER + sizeof BITMAPINFOHEADER

            int bytesFileOffset = ((ImagePrivateDataBitmap)Image.Data).Offset;
            uint paletteColors = Image.Information.ColorsUsed;
            int width = (int)Image.Information.Width;
            int height = (int)Image.Information.Height;

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

            bool isFaxEncoding = false;
            byte[] imageData = new byte[((width * bits + 7) / 8) * height];
            byte[] imageDataFax = null!;
            int k = 0;

            if (bits == 1 && options.EnableCcittCompressionForBilevelImages)
            {
                // TODO: flag/option?
                // We try Group 3 1D and Group 4 (2D) encoding here and keep the smaller byte array.
                //byte[] temp = new byte[imageData.Length];
                //int ccittSize = DoFaxEncoding(ref temp, imageBits, (uint)bytesFileOffset, (uint)width, (uint)height);

                // It seems that Group 3 2D encoding never beats both other encodings, therefore we don’t call it here.
                //byte[] temp2D = new byte[imageData.Length];
                //uint dpiY = (uint)image.VerticalResolution;
                //uint kTmp = 0;
                //int ccittSize2D = DoFaxEncoding2D((uint)bytesFileOffset, ref temp2D, imageBits, (uint)width, (uint)height, dpiY, out kTmp);
                //k = (int) kTmp;

                byte[] tempG4 = new byte[imageData.Length];
                int ccittSizeG4 = PdfImage.DoFaxEncodingGroup4(ref tempG4, Data, (uint)bytesFileOffset, (uint)width, (uint)height);

                isFaxEncoding = /*ccittSize > 0 ||*/ ccittSizeG4 > 0;
                if (isFaxEncoding)
                {
                    //if (ccittSize == 0)
                    //  ccittSize = 0x7fffffff;
                    if (ccittSizeG4 == 0)
                        ccittSizeG4 = 0x7fffffff;
                    //if (ccittSize <= ccittSizeG4)
                    //{
                    //  Array.Resize(ref temp, ccittSize);
                    //  imageDataFax = temp;
                    //  k = 0;
                    //}
                    //else
                    {
                        Array.Resize(ref tempG4, ccittSizeG4);
                        imageDataFax = tempG4;
                        k = -1;
                    }
                }
            }

            //if (!isFaxEncoding)
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
                                    // TODO???: segmentedColorMask == true => bad mask NYI
                                    mask?.AddPel((n >= firstMaskColor) && (n <= lastMaskColor));
                                }
                                else if (bits == 4)
                                {
                                    // TODO???: segmentedColorMask == true => bad mask NYI
                                    int n1 = (n & 0xf0) / 16;
                                    int n2 = (n & 0x0f);
                                    mask?.AddPel((n1 >= firstMaskColor) && (n1 <= lastMaskColor));
                                    mask?.AddPel((n2 >= firstMaskColor) && (n2 <= lastMaskColor));
                                }
                                else if (bits == 1)
                                {
                                    // TODO???: segmentedColorMask == true => bad mask NYI
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
                    throw new NotImplementedException("ReadIndexedMemoryBitmap: unsupported format #3");
                }
            }

            dest.Data = imageData;
            dest.Length = imageData.Length;

            if (imageDataFax != null)
            {
                dest.DataFax = imageDataFax;
                dest.LengthFax = imageDataFax.Length;
            }

            dest.IsGray = isGray;
            dest.K = k;
            dest.IsBitonal = isBitonal;

            dest.PaletteData = paletteData;
            dest.PaletteDataLength = paletteData.Length;
            dest.SegmentedColorMask = segmentedColorMask;

            //if (alphaMask != null)
            //{
            //    dest.AlphaMask = alphaMask;
            //    dest.AlphaMaskLength = alphaMask.Length;
            //}

            if (mask != null && mask.MaskUsed && firstMaskColor != -1)
            {
                dest.BitmapMask = mask.MaskData;
                dest.BitmapMaskLength = mask.MaskData.Length;
            }

        }
    }
}
