// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing.Imaging;
#endif
#if WPF
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
#endif
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Internal.Imaging;
using PdfSharp.Logging;
using PdfSharp.Pdf.Filters;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents an image.
    /// </summary>
    public sealed partial class PdfImage : PdfXObject
    {
        /// <summary>
        /// Initializes a new instance of PdfImage from an XImage.
        /// </summary>
        public PdfImage(PdfDocument document, XImage image)
            : base(document)
        {
            Elements.SetName(Keys.Type, "/XObject");
            Elements.SetName(Keys.Subtype, "/Image");

            _image = image;
            ImportedImage = _image._importedImage;

            ////// TODO_OLD: identify images used multiple times. If the image already exists use the same XRef.
            ////_defaultName = PdfImageTable.NextImageName;

            switch (_image.Format.Guid.ToString("B").ToUpper())
            {
                // Pdf supports Jpeg, therefore we can write what we’ve read:
                case "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}":  //XImageFormat.Jpeg
                    InitializeJpeg();
                    break;

                // All other image formats are converted to PDF bitmaps:
                case "{B96B3CAF-0728-11D3-9D7B-0000F81EF32E}":  //XImageFormat.Png
                case "{B96B3CB0-0728-11D3-9D7B-0000F81EF32E}":  //XImageFormat.Gif
                case "{B96B3CB1-0728-11D3-9D7B-0000F81EF32E}":  //XImageFormat.Tiff
                case "{B96B3CB5-0728-11D3-9D7B-0000F81EF32E}":  //XImageFormat.Icon
                    // Future improvement: try Jpeg for size optimization???
                    InitializeNonJpeg();
                    break;

                case "{84570158-DBF0-4C6B-8368-62D6A3CA76E0}":  //XImageFormat.Pdf:
                    Debug.Assert(false, "XPdfForm not expected here.");
                    break;

                default:
                    Debug.Assert(false, "Unexpected image type.");
                    break;
            }
        }

#if CORE
        internal PdfImage(PdfDocument document, ImportedImage importedImage)
            : base(document)
        {
            Elements.SetName(Keys.Type, "/XObject");
            Elements.SetName(Keys.Subtype, "/Image");
            _image = null!; // Image is not used in this case.
            ImportedImage = importedImage;
            if (ImportedImage is ImportedJpegImage)
                InitializeJpeg();
            else
                InitializeNonJpeg();
        }
#endif

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfImage(PdfDictionary dict)
            : base(dict)
        {
            _image = null!;
        }

        /// <summary>
        /// Gets the underlying XImage object.
        /// </summary>
        public XImage? Image => _image;

        readonly XImage? _image;

        ImportedImage? ImportedImage { get; set; }

        /// <summary>
        /// Returns 'Image'.
        /// </summary>
        public override string ToString()
        {
            return "Image";
        }

        /// <summary>
        /// Creates the keys for a JPEG image.
        /// </summary>
        void InitializeJpeg()
        {
            // PDF supports JPEG, so there’s not much to be done.
            MemoryStream? memory = null;
            // Close the MemoryStream if we create it.
            bool ownMemory = false;

            byte[]? imageBits = null;
            int streamLength = 0;

#if CORE || GDI || WPF
            if (ImportedImage != null)
            {
                var idd = ImportedImage.ImageData;
                imageBits = idd;
                streamLength = idd.Length;
            }
#endif

#if CORE || GDI
            if (_image != null && ImportedImage == null)
            {
                if (!_image._path.StartsWith("*", StringComparison.Ordinal))
                {
                    // Image does not come from a stream, so we have the path to the file - just use the path.
                    // If the image was modified in memory, those changes will be lost and the original image, as it was read from the file, will be added to the PDF.
                    using (FileStream sourceFile = File.OpenRead(_image._path))
                    {
                        int count;
                        byte[] buffer = new byte[8192];
                        memory = new MemoryStream((int)sourceFile.Length);
                        ownMemory = true;
                        do
                        {
                            count = sourceFile.Read(buffer, 0, buffer.Length);
                            // memory.Write(buffer, 0, buffer.Length);
                            memory.Write(buffer, 0, count);
                        } while (count > 0);
                    }
                }
                else
                {
                    // If we have a stream, copy data from the stream.
                    if (_image._stream != null! && _image._stream.CanSeek)
                    {
                        memory = new MemoryStream();
                        ownMemory = true;
                        Stream stream = _image._stream;
                        stream.Seek(0, SeekOrigin.Begin);
                        byte[] buffer = new byte[32 * 1024]; // 32K buffer.
                        int bytesRead;
                        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            memory.Write(buffer, 0, bytesRead);
                        }
                    }
                    else
                    {
#if GDI
                        // No stream, no filename, get image data from GDI image.
                        // Save the image to a memory stream.
                        memory = new MemoryStream();
                        ownMemory = true;
                        _image._gdiImage.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
#endif
                    }
                }

                if (memory is null || (int)memory.Length == 0)
                {
                    Debug.Assert(false, "Internal error? JPEG image, but file not found!");
                    throw new InvalidOperationException("JPEG image used, but cannot access image data!");
                }
#if GDI
                {
                    // Use ImageImporter to get meta information about JPEG image. GDI does not return correct information for CMYK images.
                    var ii = ToBeNamed.TryImportImage(memory, out var image);
                    if (ii && image is ImportedJpegImage)
                    {
                        _image._importedImage = image;
                        ImportedImage = image;
                    }
                }
#endif
            }
#endif
#if WPF
            if (_image != null && !_image._path.StartsWith("*", StringComparison.Ordinal))
            {
                // Image does not come from a stream, so we have the path to the file - just use the path.
                // If the image was modified in memory, those changes will be lost and the original image, as it was read from the file, will be added to the PDF.
                if (imageBits == null)
                {
                    // Use ImageImporter to read the image.
                    string filename = XImage.GetImageFilename(_image._wpfImage);
                    var worker = new StreamReaderWorker(filename);
                    var success = ToBeNamed.TryImportImage(worker.Data, out var image);

                    imageBits = image!.GetPdfImageData();
                    streamLength = imageBits.Length;

                    if (_image._importedImage == null)
                    {
                        _image._importedImage = image;
                        ImportedImage = image;
                    }
                }

                memory = _image.Memory;
            }
            else
            {
                // If we have a stream, copy data from the stream.
                if (_image != null && _image._stream != null! && _image._stream.CanSeek)
                {
                    memory = new MemoryStream();
                    ownMemory = true;
                    Stream stream = _image._stream;
                    stream.Seek(0, SeekOrigin.Begin);
                    byte[] buffer = new byte[32 * 1024]; // 32K buffer.
                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        memory.Write(buffer, 0, bytesRead);
                    }
                }
            }

            if (imageBits == null && memory == null && _image != null && _image._wpfImage != null)
            {
                Debug.Assert(false, "Internal error? JPEG image, but file not read!");
            }
#endif
#if WUI
            memory = new MemoryStream();
            ownMemory = true;
#endif
            if (imageBits == null)
            {
                streamLength = (int)memory!.Length; // NRT
                imageBits = new byte[streamLength];
                memory.Seek(0, SeekOrigin.Begin);
                _ = memory.Read(imageBits, 0, streamLength);
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (ownMemory)
                    memory.Dispose();
            }
            Debug.Assert(ReferenceEquals(_document2, Document));
            bool tryFlateDecode = Document.Options.UseFlateDecoderForJpegImages == PdfUseFlateDecoderForJpegImages.Automatic;
            bool useFlateDecode = Document.Options.UseFlateDecoderForJpegImages == PdfUseFlateDecoderForJpegImages.Always;

            FlateDecode fd = new FlateDecode();
            byte[]? imageDataCompressed = useFlateDecode || tryFlateDecode ? fd.Encode(imageBits, Document.Options.FlateEncodeMode) : null;
            if (imageDataCompressed != null && (useFlateDecode || (tryFlateDecode && imageDataCompressed.Length < imageBits.Length)))
            {
                Stream = new PdfStream(imageDataCompressed, this);
                Elements.SetInteger(PdfStream.Keys.Length, imageDataCompressed.Length);
                PdfArray arrayFilters = new(Document);
                arrayFilters.Elements.Add(new PdfName("/FlateDecode"));
                arrayFilters.Elements.Add(new PdfName("/DCTDecode"));
                Elements[PdfStream.Keys.Filter] = arrayFilters;
            }
            else
            {
                Stream = new PdfStream(imageBits, this);
                Elements.SetInteger(PdfStream.Keys.Length, streamLength);
                Elements.SetName(PdfStream.Keys.Filter, "/DCTDecode");
            }

            // #PDF-A
            if (_image is { Interpolate: true } && !Document.IsPdfA)
                Elements[Keys.Interpolate] = PdfBoolean.True;
            Elements[Keys.Width] = new PdfInteger(_image?.PixelWidth ?? ImportedImage!.PixelWidth);
            Elements[Keys.Height] = new PdfInteger(_image?.PixelHeight ?? ImportedImage!.PixelHeight);
            Elements[Keys.BitsPerComponent] = new PdfInteger(8);

#if CORE || GDI || WPF
            if (ImportedImage != null)
            {
                if (ImportedImage.ImageFormat == PdfSharp.Internal.Imaging.ImageFormats.JPEGCMYK ||
                    ImportedImage.ImageFormat == PdfSharp.Internal.Imaging.ImageFormats.JPEGRGBW)
                {
                    // TODO_OLD: Test with CMYK JPEG files (so far I only found ImageFlags.ColorSpaceYcck JPEG files ...)
                    Elements[Keys.ColorSpace] = new PdfName("/DeviceCMYK");
                    if (ImportedImage.ImageFormat == PdfSharp.Internal.Imaging.ImageFormats.JPEGRGBW)
                        Elements["/Decode"] = new PdfLiteral("[1 0 1 0 1 0 1 0]"); // Invert colors because YCCK is inverted CMYK.
                }
                else if (ImportedImage.ImageFormat == PdfSharp.Internal.Imaging.ImageFormats.JPEGGRAY)
                {
                    Elements[Keys.ColorSpace] = new PdfName("/DeviceGray");
                }
                else
                {
                    Elements[Keys.ColorSpace] = new PdfName("/DeviceRGB");
                }
            }
#endif
#if GDI
            if (_image != null && ImportedImage == null)
            {
                if ((_image._gdiImage.Flags & ((int)ImageFlags.ColorSpaceCmyk | (int)ImageFlags.ColorSpaceYcck)) != 0)
                {
                    // TODO_OLD: Test with CMYK JPEG files (so far I only found ImageFlags.ColorSpaceYcck JPEG files ...)
                    Elements[Keys.ColorSpace] = new PdfName("/DeviceCMYK");
                    if ((_image._gdiImage.Flags & (int)ImageFlags.ColorSpaceYcck) != 0)
                        //Elements["/Decode"] = new PdfLiteral("[1 0 1 0 1 0 1 0]"); // Invert colors because YCCK is inverted CMYK. #US309
                        Elements["/Decode"] = new PdfArray(1, 0, 1, 0, 1, 0, 1, 0); // Invert colors because YCCK is inverted CMYK.
                }
                else if ((_image._gdiImage.Flags & (int)ImageFlags.ColorSpaceGray) != 0)
                {
                    Elements[Keys.ColorSpace] = new PdfName("/DeviceGray");
                }
                else
                {
                    Elements[Keys.ColorSpace] = new PdfName("/DeviceRGB");
                }
            }
#endif
#if WPF
            if (_image != null && ImportedImage == null)
            {
                string pixelFormat = _image._wpfImage?.Format.ToString() ?? "";
                bool isCmyk = _image.IsCmyk;
                bool isGrey = pixelFormat == "Gray8";
                if (isCmyk)
                {
                    // TODO_OLD: Test with CMYK JPEG files (so far I only found ImageFlags.ColorSpaceYcck JPEG files ...)
                    Elements[Keys.ColorSpace] = new PdfName("/DeviceCMYK");
                    Elements["/Decode"] = new PdfLiteral("[1 0 1 0 1 0 1 0]"); // Invert colors because YCCK is inverted CMYK.
                }
                else if (isGrey)
                {
                    Elements[Keys.ColorSpace] = new PdfName("/DeviceGray");
                }
                else
                {
                    Elements[Keys.ColorSpace] = new PdfName("/DeviceRGB");
                }
            }
#endif
        }

        /// <summary>
        /// Creates the keys for a FLATE image.
        /// </summary>
        void InitializeNonJpeg()
        {
#if CORE || GDI || WPF
            if (ImportedImage != null)
            {
                switch (ImportedImage.ImageFormat)
                {
                    case PdfSharp.Internal.Imaging.ImageFormats.Bgra32:
                        CreateTrueColorMemoryBitmap(3, 8, true);
                        break;

                    case PdfSharp.Internal.Imaging.ImageFormats.Bgr32:
                        CreateTrueColorMemoryBitmap(3, 8, false);
                        break;

                    case PdfSharp.Internal.Imaging.ImageFormats.Gray8:
                        CreateTrueColorMemoryBitmap(1, 8, false);
                        break;

                    case PdfSharp.Internal.Imaging.ImageFormats.Indexed8:
                        CreateIndexedMemoryBitmap(8);
                        break;

                    case PdfSharp.Internal.Imaging.ImageFormats.Indexed4:
                        CreateIndexedMemoryBitmap(4);
                        break;

                    case PdfSharp.Internal.Imaging.ImageFormats.Indexed1:
                        CreateIndexedMemoryBitmap(1);
                        break;

                    default:
                        throw new NotSupportedException("Image format not supported.");
                }
                return;
            }
#endif

#if GDI && !WPF
            switch (_image!._gdiImage.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    ReadTrueColorMemoryBitmap(3, 8, false);
                    break;

                case PixelFormat.Format32bppRgb:
                    ReadTrueColorMemoryBitmap(4, 8, false);
                    break;

                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    ReadTrueColorMemoryBitmap(3, 8, true);
                    break;

                case PixelFormat.Format8bppIndexed:
                    ReadIndexedMemoryBitmap(8);
                    break;

                case PixelFormat.Format4bppIndexed:
                    ReadIndexedMemoryBitmap(4);
                    break;

                case PixelFormat.Format1bppIndexed:
                    ReadIndexedMemoryBitmap(1);
                    break;

                default:
                    //#if DEBUGxxx
                    //          image.image.Save("$$$.bmp", ImageFormat.Bmp);
                    //#endif
                    throw new NotSupportedException("Image format not supported.");
            }
#endif
#if WPF
            // Only called for GDI and WPF.
            // _image is not null here.

            string format = _image!._wpfImage.Format.ToString();
            switch (format)
            {
                case "Bgr24": //Format24bppRgb:
                    ReadTrueColorMemoryBitmap(3, 8, false);
                    break;

                //case .PixelFormat.Format32bppRgb:
                //  ReadTrueColorMemoryBitmap(4, 8, false);
                //  break;

                case "Bgra32":  //PixelFormat.Format32bppArgb:
                    //case PixelFormat.Format32bppPArgb:
                    ReadTrueColorMemoryBitmap(3, 8, true);
                    break;

                case "Bgr32":
                    ReadTrueColorMemoryBitmap(4, 8, false);
                    break;

                case "Pbgra32":
                    ReadTrueColorMemoryBitmap(3, 8, true);
                    break;

                case "Indexed8":  //Format8bppIndexed:
                case "Gray8":
                    ReadIndexedMemoryBitmap(8);
                    break;

                case "Indexed4":  //Format4bppIndexed:
                case "Gray4":
                    ReadIndexedMemoryBitmap(4);
                    break;

                case "Indexed2":
                    ReadIndexedMemoryBitmap(2);
                    break;

                case "Indexed1":  //Format1bppIndexed:
                case "BlackWhite":  //Format1bppIndexed:
                    ReadIndexedMemoryBitmap(1);
                    break;

                default:
#if DEBUG_
                    image.image.Save("$$$.bmp", ImageFormat.Bmp);
#endif
                    throw new NotSupportedException("Image format '" + format + "' not supported.");
            }
#endif
        }

#if CORE || GDI || WPF
        void CreateIndexedMemoryBitmap(int bits)
        {
            var ii = (ImportedRasterImage)ImportedImage!;
            var ibi = ii as ImportedBmpImage;

            int pdfVersion = Owner.Version;
            int firstMaskColor = ibi?.FirstMaskColor ?? -1, lastMaskColor = ibi?.LastMaskColor ?? -1;
            // TODO_OLD MaskColor transparency.
            bool segmentedColorMask = ibi?.SegmentedColorMask ?? false;
            bool isGray = ibi?.IsGray ?? false;
            int bitonal = ibi?.Bitonal ?? 0;
            bool hasAlphaMask = ii.AlphaMaskData != null;
            bool hasBitmapMask = ii.BitmapMaskData != null;

            bool isFaxEncoding = false;
            byte[]? imageDataFax = null;
            int k = 0;

            {
                FlateDecode fd = new FlateDecode();

                // Color mask: currently disabled.
                // Color mask.
                if (firstMaskColor != -1 && lastMaskColor != -1)
                {
                    // Color mask requires Reader 4.0 or higher.
                    if (!segmentedColorMask && pdfVersion >= 13 && !isGray)
                    {
                        PdfArray array = new PdfArray(Document);
                        array.Elements.Add(new PdfInteger(firstMaskColor));
                        array.Elements.Add(new PdfInteger(lastMaskColor));
                        Elements[Keys.Mask] = array;
                    }
                    else
                    {
                        // Monochrome mask.
                        byte[] maskDataCompressed = fd.Encode(ii.GetPdfBitmapMaskData()!, Document.Options.FlateEncodeMode);
                        var pdfMask = new PdfDictionary(Document);
                        pdfMask.Elements.SetName(Keys.Type, "/XObject");
                        pdfMask.Elements.SetName(Keys.Subtype, "/Image");

                        Owner.IrefTable.Add(pdfMask);
                        pdfMask.Stream = new PdfStream(maskDataCompressed, pdfMask);
                        pdfMask.Elements.SetInteger(PdfStream.Keys.Length, maskDataCompressed.Length);
                        pdfMask.Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
                        pdfMask.Elements.SetInteger(Keys.Width, ii.PixelWidth);
                        pdfMask.Elements.SetInteger(Keys.Height, ii.PixelHeight);
                        pdfMask.Elements.SetInteger(Keys.BitsPerComponent, 1);
                        pdfMask.Elements.SetBoolean(Keys.ImageMask, true);
                        Elements[Keys.Mask] = pdfMask.RequiredReference;
                    }
                }

                if (hasAlphaMask && pdfVersion >= 14)
                {
                    // #PDF-A
                    if (Document.IsPdfA)
                    {
                        PdfSharpLogHost.Logger.LogWarning("PDF/A: Alpha mask of PdfImage suppressed.");
                    }
                    else
                    {
                        // The image provides an alpha mask (requires Arcrobat 5.0 or higher).
                        byte[] alphaMaskCompressed = fd.Encode(ii.GetPdfAlphaMaskData()!, Document.Options.FlateEncodeMode);
                        var smask = new PdfDictionary(Document);
                        smask.Elements.SetName(Keys.Type, "/XObject");
                        smask.Elements.SetName(Keys.Subtype, "/Image");

                        Owner.IrefTable.Add(smask);
                        smask.Stream = new PdfStream(alphaMaskCompressed, smask);
                        smask.Elements.SetInteger(PdfStream.Keys.Length, alphaMaskCompressed.Length);
                        smask.Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
                        smask.Elements.SetInteger(Keys.Width, ii.PixelWidth);
                        smask.Elements.SetInteger(Keys.Height, ii.PixelHeight);
                        smask.Elements.SetInteger(Keys.BitsPerComponent, 8);
                        smask.Elements.SetName(Keys.ColorSpace, "/DeviceGray");
                        Elements[Keys.SMask] = smask.RequiredReference;
                    }
                }

                var imageBits = ii.ImageData;
                var imageData = ii.GetPdfImageData()!;

                // If fax encoding is allowed, try if fax encoding reduces the size.
                if (bits == 1 && Document.Options.EnableCcittCompressionForBilevelImages &&
                    ibi != null)
                {
                    // Only try Group 4 encoding.
                    // It seems that Group 3 2D encoding never beats both other encodings, therefore we don’t call it here.

                    byte[] tempG4 = new byte[imageData.Length];
                    int ccittSizeG4 = DoFaxEncodingGroup4(ref tempG4, imageBits, (uint)ibi.BytesFileOffset, (uint)ibi.PixelWidth, (uint)ibi.PixelHeight);

                    isFaxEncoding = ccittSizeG4 > 0;
                    if (isFaxEncoding)
                    {
                        Array.Resize(ref tempG4, ccittSizeG4);
                        imageDataFax = tempG4;
                        k = -1;
                    }
                }

                byte[] imageDataCompressed = fd.Encode(imageData, Document.Options.FlateEncodeMode);
                byte[]? imageDataFaxCompressed = imageDataFax != null ? fd.Encode(imageDataFax, Document.Options.FlateEncodeMode) : null;
                bool usesCcittEncoding = false;
                if (imageDataFax != null && imageDataFaxCompressed != null &&
                  (imageDataFax.Length < imageDataCompressed.Length ||
                  imageDataFaxCompressed.Length < imageDataCompressed.Length))
                {
                    // /CCITTFaxDecode creates the smaller file (with or without /FlateDecode).
                    usesCcittEncoding = true;

                    if (imageDataFax.Length < imageDataCompressed.Length)
                    {
                        Stream = new PdfStream(imageDataFax, this);
                        Elements["/Length"] = new PdfInteger(imageDataFax.Length);
                        Elements[PdfStream.Keys.Filter] = new PdfName("/CCITTFaxDecode");
                        var dictionary = new PdfDictionary();
                        if (k != 0)
                            dictionary.Elements.Add("/K", new PdfInteger(k));
                        if (bitonal < 0)
                            dictionary.Elements.Add("/BlackIs1", new PdfBoolean(true));
                        dictionary.Elements.Add("/EndOfBlock", new PdfBoolean(false));
                        dictionary.Elements.Add("/Columns", new PdfInteger(ii.PixelWidth));
                        dictionary.Elements.Add("/Rows", new PdfInteger(ii.PixelHeight));
                        Elements[PdfStream.Keys.DecodeParms] = dictionary;
                    }
                    else
                    {
                        Stream = new PdfStream(imageDataFaxCompressed, this);
                        Elements["/Length"] = new PdfInteger(imageDataFaxCompressed.Length);
                        var arrayFilters = new PdfArray(Document);
                        arrayFilters.Elements.Add(new PdfName("/FlateDecode"));
                        arrayFilters.Elements.Add(new PdfName("/CCITTFaxDecode"));
                        Elements[PdfStream.Keys.Filter] = arrayFilters;
                        var arrayDecodeParms = new PdfArray(Document);

                        var dictFlateDecodeParms = new PdfDictionary();

                        var dictCcittFaxDecodeParms = new PdfDictionary();
                        if (k != 0)
                            dictCcittFaxDecodeParms.Elements.Add("/K", new PdfInteger(k));
                        if (bitonal < 0)
                            dictCcittFaxDecodeParms.Elements.Add("/BlackIs1", new PdfBoolean(true));
                        dictCcittFaxDecodeParms.Elements.Add("/EndOfBlock", new PdfBoolean(false));
                        dictCcittFaxDecodeParms.Elements.Add("/Columns", new PdfInteger(ii.PixelWidth));
                        dictCcittFaxDecodeParms.Elements.Add("/Rows", new PdfInteger(ii.PixelHeight));

                        arrayDecodeParms.Elements.Add(dictFlateDecodeParms); // How to add the "null object"?
                        arrayDecodeParms.Elements.Add(dictCcittFaxDecodeParms);
                        Elements[PdfStream.Keys.DecodeParms] = arrayDecodeParms;
                    }
                }
                else
                {
                    // /FlateDecode creates the smaller file (or no monochrome bitmap).
                    Stream = new PdfStream(imageDataCompressed, this);
                    Elements.SetInteger(PdfStream.Keys.Length, imageDataCompressed.Length);
                    Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
                }

                Elements[Keys.Width] = new PdfInteger(ii.PixelWidth);
                Elements[Keys.Height] = new PdfInteger(ii.PixelHeight);
                Elements[Keys.BitsPerComponent] = new PdfInteger(bits);
                // Anything needed for CMYK? Do we have sample images?

                // CCITT encoding: we need color palette for isBitonal == 0.
                // FlateDecode: we need color palette for isBitonal <= 0 unless we have grayscales.
                if ((usesCcittEncoding && bitonal == 0) ||
                  (!usesCcittEncoding && bitonal <= 0 && !isGray))
                {
                    var colorPalette = new PdfDictionary(Document);
                    var paletteData = ii.GetPdfPaletteData();
                    byte[]? packedPaletteData = paletteData != null && paletteData!.Length >= 48 ? fd.Encode(paletteData, Document.Options.FlateEncodeMode) : null; // Don’t compress small palettes.
                    if (packedPaletteData != null && packedPaletteData.Length + 20 < paletteData!.Length) // +20: compensate for the overhead (estimated value).
                    {
                        // Create compressed color palette.
                        colorPalette.CreateStream(packedPaletteData);
                        colorPalette.Elements.SetInteger(PdfStream.Keys.Length, packedPaletteData.Length);
                        colorPalette.Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
                    }
                    else
                    {
                        // Create uncompressed color palette.
                        colorPalette.CreateStream(paletteData!);
                        colorPalette.Elements.SetInteger(PdfStream.Keys.Length, paletteData!.Length);
                    }
                    Owner.IrefTable.Add(colorPalette);

                    var arrayColorSpace = new PdfArray(Document);
                    arrayColorSpace.Elements.Add(new PdfName("/Indexed"));
                    arrayColorSpace.Elements.Add(new PdfName("/DeviceRGB"));
                    arrayColorSpace.Elements.Add(new PdfInteger((int)ii.ColorsUsed - 1));
                    arrayColorSpace.Elements.Add(colorPalette.Reference!);
                    Elements[Keys.ColorSpace] = arrayColorSpace;
                }
                else
                {
                    Elements[Keys.ColorSpace] = new PdfName("/DeviceGray");
                }

                if (_image is { Interpolate: true })
                {
                    // #PDF-A
                    if (Document.IsPdfA)
                    {
                        PdfSharpLogHost.Logger.LogWarning("PDF/A: Image interpolation suppressed.");
                    }
                    else
                    {
                        Elements[Keys.Interpolate] = PdfBoolean.True;
                    }
                }
            }
        }

        void CreateTrueColorMemoryBitmap(int components, int bits, bool hasAlpha)
        {
            // TODO_OLD Use hasAlpha? Or is it superfluous?
            int pdfVersion = Owner.Version;
            var fd = new FlateDecode();
            var ii = (ImportedRasterImage)ImportedImage!;
            var ibi = ii as ImportedBmpImage;
            bool hasAlphaMask = ii.AlphaMaskData != null;
            bool hasBitmapMask = ii.BitmapMaskData != null;
            bool hasMask = hasAlphaMask || hasBitmapMask;

            if (hasMask && hasBitmapMask)
            {
                // Monochrome mask is either sufficient or
                // provided for compatibility with older reader versions.
                byte[] maskDataCompressed = fd.Encode(ii.GetPdfBitmapMaskData()!, Document.Options.FlateEncodeMode);
                var pdfMask = new PdfDictionary(Document);
                pdfMask.Elements.SetName(Keys.Type, "/XObject");
                pdfMask.Elements.SetName(Keys.Subtype, "/Image");

                Owner.IrefTable.Add(pdfMask);
                pdfMask.Stream = new PdfStream(maskDataCompressed, pdfMask);
                pdfMask.Elements.SetInteger(PdfStream.Keys.Length, maskDataCompressed.Length);
                pdfMask.Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
                pdfMask.Elements[Keys.Width] = new PdfInteger(ii.PixelWidth);
                pdfMask.Elements[Keys.Height] = new PdfInteger(ii.PixelHeight);
                pdfMask.Elements[Keys.BitsPerComponent] = new PdfInteger(1);
                pdfMask.Elements[Keys.ImageMask] = new PdfBoolean(true);
                Elements[Keys.Mask] = pdfMask.RequiredReference;
            }
            if (hasMask && hasAlphaMask && pdfVersion >= 14)
            {
                // #PDF-A
                if (Document.IsPdfA)
                {
                    PdfSharpLogHost.Logger.LogWarning("PDF/A: Alpha mask of PdfImage suppressed.");
                }
                else
                {
                    // The image provides an alpha mask (requires Acrobat 5.0 or higher).
                    byte[] alphaMaskCompressed = fd.Encode(ii.GetPdfAlphaMaskData()!, Document.Options.FlateEncodeMode);
                    var smask = new PdfDictionary(Document);
                    smask.Elements.SetName(Keys.Type, "/XObject");
                    smask.Elements.SetName(Keys.Subtype, "/Image");

                    Owner.IrefTable.Add(smask);
                    smask.Stream = new PdfStream(alphaMaskCompressed, smask);
                    smask.Elements.SetInteger(PdfStream.Keys.Length, alphaMaskCompressed.Length);
                    smask.Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
                    smask.Elements[Keys.Width] = new PdfInteger(ii.PixelWidth);
                    smask.Elements[Keys.Height] = new PdfInteger(ii.PixelHeight);
                    smask.Elements[Keys.BitsPerComponent] = new PdfInteger(8);
                    smask.Elements[Keys.ColorSpace] = new PdfName("/DeviceGray");
                    Elements[Keys.SMask] = smask.RequiredReference;
                }
            }

            byte[] imageDataCompressed = fd.Encode(ii.GetPdfImageData(), Document.Options.FlateEncodeMode);

            Stream = new PdfStream(imageDataCompressed, this);
            Elements.SetInteger(PdfStream.Keys.Length, imageDataCompressed.Length);
            Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
            Elements[Keys.Width] = new PdfInteger(ii.PixelWidth);
            Elements[Keys.Height] = new PdfInteger(ii.PixelHeight);
            Elements[Keys.BitsPerComponent] = new PdfInteger(8);
            // Anything needed for CMYK? Do we have sample images?
            Elements[Keys.ColorSpace] = new PdfName(components == 1 ? "/DeviceGray" : "/DeviceRGB");

            if (_image is { Interpolate: true })
            {
                // #PDF-A
                if (Document.IsPdfA)
                {
                    PdfSharpLogHost.Logger.LogWarning("PDF/A: Image interpolation suppressed.");
                }
                else
                {
                    Elements[Keys.Interpolate] = PdfBoolean.True;
                }
            }
        }
#endif

        static int ReadWord(byte[] ab, int offset)
        {
            return ab[offset] + 256 * ab[offset + 1];
        }

        static int ReadDWord(byte[] ab, int offset)
        {
            return ReadWord(ab, offset) + 0x10000 * ReadWord(ab, offset + 2);
        }

        static int ReadWord(ReadOnlySpan<byte> bytes, int offset)
        {
            //return ab[offset] + 256 * ab[offset + 1];
            return bytes[offset++] | (bytes[offset] << 8);
        }

        static int ReadDWord(ReadOnlySpan<byte> bytes, int offset)
        {
            //return ReadWord(ab, offset) + 0x10000 * ReadWord(ab, offset + 2);
            return bytes[offset++] | (bytes[offset++] << 8) | (bytes[offset++] << 16) | (bytes[offset] << 24);
        }

        /// <summary>
        /// Reads images that are returned from GDI+ without color palette.
        /// </summary>
        /// <param name="components">4 (32bpp RGB), 3 (24bpp RGB, 32bpp ARGB)</param>
        /// <param name="bits">8</param>
        /// <param name="hasAlpha">true (ARGB), false (RGB)</param>
        void ReadTrueColorMemoryBitmap(int components, int bits, bool hasAlpha)
        {
            // Only called for GDI and WPF.
            // _image is not null here.

            //#if DEBUG_
            //          image.image.Save("$$$.bmp", ImageFormat.Bmp);
            //#endif
            int pdfVersion = Owner.Version;
            var memory = new MemoryStream();
#if GDI
            _image!._gdiImage.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
#endif
#if WPF
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(_image!._wpfImage));
            encoder.Save(memory);
#endif

            int streamLength = (int)memory.Length;
            Debug.Assert(streamLength > 0, "Bitmap image encoding failed.");
            if (streamLength > 0)
            {
#if !WUI
                // Available with wrt, but not with wrt81.
                // Note that imageBits.Length can be larger than streamLength. Do not use these extra bytes.
                byte[] imageBits = memory.GetBuffer();
#else
                byte[] imageBits = new byte[streamLength];
                memory.Seek(0, SeekOrigin.Begin);
                memory.Read(imageBits, 0, streamLength);
                memory.Dispose();
#endif

                int height = _image!.PixelHeight;
                int width = _image!.PixelWidth;

                // We use TryImportImageBmp here to avoid redundant code.

                var ii = ToBeNamed.TryImportImageBmp(imageBits, out var image);
                if (!ii)
                    throw new NotSupportedException("ReadTrueColorMemoryBitmap: unsupported format");
                var bmpImage = image as ImportedBmpImage;
                if (bmpImage == null)
                    throw new NotSupportedException("ReadTrueColorMemoryBitmap: unsupported format");

                var imageData = bmpImage!.GetPdfImageData();
                var hasAlphaMask = bmpImage.AlphaMaskData != null;
                var hasMask = bmpImage.BitmapMaskData != null;
                var alphaMask = bmpImage.GetPdfAlphaMaskData();
                var mask = bmpImage.GetPdfBitmapMask();

                var fd = new FlateDecode();
                if (hasMask && mask != null && mask.MaskUsed)
                {
                    // Monochrome mask is either sufficient or
                    // provided for compatibility with older reader versions.
                    byte[] maskDataCompressed = fd.Encode(mask.MaskData, Document.Options.FlateEncodeMode);
                    var pdfMask = new PdfDictionary(Document);
                    pdfMask.Elements.SetName(Keys.Type, "/XObject");
                    pdfMask.Elements.SetName(Keys.Subtype, "/Image");

                    Owner.IrefTable.Add(pdfMask);
                    pdfMask.Stream = new PdfStream(maskDataCompressed, pdfMask);
                    pdfMask.Elements.SetInteger(PdfStream.Keys.Length, maskDataCompressed.Length);
                    pdfMask.Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
                    pdfMask.Elements[Keys.Width] = new PdfInteger(width);
                    pdfMask.Elements[Keys.Height] = new PdfInteger(height);
                    pdfMask.Elements[Keys.BitsPerComponent] = new PdfInteger(1);
                    pdfMask.Elements[Keys.ImageMask] = new PdfBoolean(true);
                    Elements[Keys.Mask] = pdfMask.RequiredReference;
                }
                if (/*hasMask &&*/ hasAlphaMask && pdfVersion >= 14 && alphaMask != null)
                {
                    // #PDF-A
                    if (Document.IsPdfA)
                    {
                        PdfSharpLogHost.Logger.LogWarning("PDF/A: Alpha mask of PdfImage suppressed.");
                    }
                    else
                    {
                        // The image provides an alpha mask (requires Acrobat 5.0 or higher).
                        byte[] alphaMaskCompressed = fd.Encode(alphaMask, Document.Options.FlateEncodeMode);
                        var smask = new PdfDictionary(Document);
                        smask.Elements.SetName(Keys.Type, "/XObject");
                        smask.Elements.SetName(Keys.Subtype, "/Image");

                        Owner.IrefTable.Add(smask);
                        smask.Stream = new PdfStream(alphaMaskCompressed, smask);
                        smask.Elements.SetInteger(PdfStream.Keys.Length, alphaMaskCompressed.Length);
                        smask.Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
                        smask.Elements[Keys.Width] = new PdfInteger(width);
                        smask.Elements[Keys.Height] = new PdfInteger(height);
                        smask.Elements[Keys.BitsPerComponent] = new PdfInteger(8);
                        smask.Elements[Keys.ColorSpace] = new PdfName("/DeviceGray");
                        Elements[Keys.SMask] = smask.RequiredReference;
                    }
                }

                byte[] imageDataCompressed = fd.Encode(imageData, Document.Options.FlateEncodeMode);

                Stream = new PdfStream(imageDataCompressed, this);
                Elements.SetInteger(PdfStream.Keys.Length, imageDataCompressed.Length);
                Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
                Elements[Keys.Width] = new PdfInteger(width);
                Elements[Keys.Height] = new PdfInteger(height);
                Elements[Keys.BitsPerComponent] = new PdfInteger(8);
                // Anything needed for CMYK? Do we have sample images?
                Elements[Keys.ColorSpace] = new PdfName("/DeviceRGB");

                if (_image.Interpolate)
                {
                    // #PDF-A
                    if (Document.IsPdfA)
                    {
                        PdfSharpLogHost.Logger.LogWarning("PDF/A: Image interpolation suppressed.");
                    }
                    else
                    {
                        Elements[Keys.Interpolate] = PdfBoolean.True;
                    }
                }
            }
        }

        void ReadIndexedMemoryBitmap(int bits)
        {
            // Only called for GDI and WPF.
            // _image is not null here.

            int pdfVersion = Owner.Version;
            int firstMaskColor = -1, lastMaskColor = -1;
            bool segmentedColorMask = false;

            MemoryStream memory = new MemoryStream();
#if GDI
            _image!._gdiImage.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
#endif
#if WPF
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            //if (!_image._path.StartsWith("*", StringComparison.Ordinal))
            //    encoder.Frames.Add(BitmapFrame.Create(new Uri(_image._path), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad));
            //else
            encoder.Frames.Add(BitmapFrame.Create(_image!._wpfImage));
            encoder.Save(memory);
#endif
            int streamLength = (int)memory.Length;
            Debug.Assert(streamLength > 0, "Bitmap image encoding failed.");
            if (streamLength > 0)
            {
                int height = _image!.PixelHeight;
                int width = _image!.PixelWidth;

                byte[] imageBits = new byte[streamLength];
                memory.Seek(0, SeekOrigin.Begin);
                _ = memory.Read(imageBits, 0, streamLength);
#if !WUI
                memory.Close();
#else
                memory.Dispose();
#endif

                // We use TryImportImageBmp here to avoid redundant code.

                bool isFaxEncoding = false;
                byte[]? imageDataFax = null;
                int k = 0;

                var ii = ToBeNamed.TryImportImageBmp(imageBits, out var image);
                if (!ii)
                    throw new NotSupportedException("ReadIndexedMemoryBitmap: unsupported format");
                var bmpImage = image as ImportedBmpImage;
                if (bmpImage == null)
                    throw new NotSupportedException("ReadIndexedMemoryBitmap: unsupported format");
                if (bmpImage.PaletteData == null)
                    throw new NotSupportedException("ReadIndexedMemoryBitmap: unsupported format");

                var imageData = bmpImage!.GetPdfImageData();
                var paletteData = bmpImage.GetPdfPaletteData()!;
                var paletteColors = bmpImage.PaletteColors;
                //var hasAlphaMask = bmpImage.AlphaMaskData != null;
                var hasMask = bmpImage.BitmapMaskData != null;
                //var alphaMask = bmpImage.GetPdfAlphaMaskData();
                var mask = bmpImage.GetPdfBitmapMask();
                var isGray = bmpImage.IsGray;
                var bytesFileOffset = bmpImage.BytesFileOffset;
                segmentedColorMask = bmpImage.SegmentedColorMask;
                firstMaskColor = bmpImage.FirstMaskColor;
                lastMaskColor = bmpImage.LastMaskColor;
                hasMask |= firstMaskColor != -1 && lastMaskColor != -1;
                var isBitonal = bmpImage.Bitonal;
                if (bits != bmpImage.BitCount)
                {
                    if (bmpImage.BitCount == 1 || bmpImage.BitCount == 4 || bmpImage.BitCount == 8)
                        bits = bmpImage.BitCount;
                }

                // If fax encoding is allowed, try if fax encoding reduces the size.
                if (bits == 1 && Document.Options.EnableCcittCompressionForBilevelImages)
                {
                    // Only try Group 4 encoding.
                    // It seems that Group 3 2D encoding never beats both other encodings, therefore we don’t call it here.

                    byte[] tempG4 = new byte[imageData.Length];
                    int ccittSizeG4 = DoFaxEncodingGroup4(ref tempG4, imageBits, (uint)bytesFileOffset, (uint)width, (uint)height);

                    isFaxEncoding = ccittSizeG4 > 0;
                    if (isFaxEncoding)
                    {
                        Array.Resize(ref tempG4, ccittSizeG4);
                        imageDataFax = tempG4;
                        k = -1;
                    }
                }

                var flateDecode = new FlateDecode();
                if (hasMask)
                {
                    // Color mask requires Reader 4.0 or higher.
                    if (!segmentedColorMask && pdfVersion >= 13 && !isGray)
                    {
                        PdfArray array = new PdfArray(Document);
                        array.Elements.Add(new PdfInteger(firstMaskColor));
                        array.Elements.Add(new PdfInteger(lastMaskColor));
                        Elements[Keys.Mask] = array;
                    }
                    else
                    {
                        // Monochrome mask.
                        byte[] maskDataCompressed = flateDecode.Encode(mask!.MaskData, Document.Options.FlateEncodeMode);
                        var pdfMask = new PdfDictionary(Document);
                        pdfMask.Elements.SetName(Keys.Type, "/XObject");
                        pdfMask.Elements.SetName(Keys.Subtype, "/Image");

                        Owner.IrefTable.Add(pdfMask);
                        pdfMask.Stream = new PdfStream(maskDataCompressed, pdfMask);
                        //pdfMask.Elements["/Length"] = new PdfInteger(maskDataCompressed.Length);
                        pdfMask.Elements.SetInteger(PdfStream.Keys.Length, maskDataCompressed.Length);
                        pdfMask.Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
                        pdfMask.Elements[Keys.Width] = new PdfInteger(width);
                        pdfMask.Elements[Keys.Height] = new PdfInteger(height);
                        pdfMask.Elements[Keys.BitsPerComponent] = new PdfInteger(1);
                        pdfMask.Elements[Keys.ImageMask] = new PdfBoolean(true);
                        Elements[Keys.Mask] = pdfMask.RequiredReference;
                    }
                }

                Debug.Assert((isFaxEncoding && imageDataFax != null) || (!isFaxEncoding && imageDataFax == null));

                byte[] imageDataCompressed = flateDecode.Encode(imageData, Document.Options.FlateEncodeMode);
                byte[]? imageDataFaxCompressed = isFaxEncoding ? flateDecode.Encode(imageDataFax!, Document.Options.FlateEncodeMode) : null;

                bool usesCcittEncoding = false;
                if (isFaxEncoding &&
                    (imageDataFax!.Length < imageDataCompressed.Length ||
                     (imageDataFaxCompressed != null && imageDataFaxCompressed.Length < imageDataCompressed.Length)))
                {
                    // /CCITTFaxDecode creates the smaller file (with or without /FlateDecode).
                    usesCcittEncoding = true;

                    if (imageDataFax.Length < imageDataCompressed.Length)
                    {
                        Stream = new PdfStream(imageDataFax, this);
                        Elements[PdfStream.Keys.Length] = new PdfInteger(imageDataFax.Length);
                        Elements[PdfStream.Keys.Filter] = new PdfName("/CCITTFaxDecode");
                        var dictionary = new PdfDictionary();
                        if (k != 0)
                            dictionary.Elements.Add("/K", new PdfInteger(k));
                        if (isBitonal < 0)
                            dictionary.Elements.Add("/BlackIs1", new PdfBoolean(true));
                        dictionary.Elements.Add("/EndOfBlock", new PdfBoolean(false));
                        dictionary.Elements.Add("/Columns", new PdfInteger(width));
                        dictionary.Elements.Add("/Rows", new PdfInteger(height));
                        //array2.Elements.Add(dictionary);
                        Elements[PdfStream.Keys.DecodeParms] = dictionary; // array2;
                    }
                    else
                    {
                        if (imageDataFaxCompressed == null)
                            throw new ArgumentNullException(nameof(imageDataFaxCompressed));

                        Stream = new PdfStream(imageDataFaxCompressed, this);
                        Elements[PdfStream.Keys.Length] = new PdfInteger(imageDataFaxCompressed.Length);
                        var arrayFilters = new PdfArray(Document);
                        arrayFilters.Elements.Add(new PdfName("/FlateDecode"));
                        arrayFilters.Elements.Add(new PdfName("/CCITTFaxDecode"));
                        Elements[PdfStream.Keys.Filter] = arrayFilters;
                        var arrayDecodeParms = new PdfArray(Document);

                        var dictFlateDecodeParms = new PdfDictionary();
                        var dictCcittFaxDecodeParms = new PdfDictionary();
                        if (k != 0)
                            dictCcittFaxDecodeParms.Elements.Add("/K", new PdfInteger(k));
                        if (isBitonal < 0)
                            dictCcittFaxDecodeParms.Elements.Add("/BlackIs1", new PdfBoolean(true));
                        dictCcittFaxDecodeParms.Elements.Add("/EndOfBlock", new PdfBoolean(false));
                        dictCcittFaxDecodeParms.Elements.Add("/Columns", new PdfInteger(width));
                        dictCcittFaxDecodeParms.Elements.Add("/Rows", new PdfInteger(height));

                        arrayDecodeParms.Elements.Add(dictFlateDecodeParms); // How to add the "null object"?
                        arrayDecodeParms.Elements.Add(dictCcittFaxDecodeParms);
                        Elements[PdfStream.Keys.DecodeParms] = arrayDecodeParms;
                    }
                }
                else
                {
                    // /FlateDecode creates the smaller file (or no monochrome bitmap).
                    Stream = new PdfStream(imageDataCompressed, this);
                    Elements.SetInteger(PdfStream.Keys.Length, imageDataCompressed.Length);
                    Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
                }

                Elements[Keys.Width] = new PdfInteger(width);
                Elements[Keys.Height] = new PdfInteger(height);
                Elements[Keys.BitsPerComponent] = new PdfInteger(bits);
                // Anything needed for CMYK? Do we have sample images?

                // CCITT encoding: we need color palette for isBitonal == 0.
                // FlateDecode: we need color palette for isBitonal <= 0 unless we have grayscales.
                if ((usesCcittEncoding && isBitonal == 0) ||
                  (!usesCcittEncoding && isBitonal <= 0 && !isGray))
                {
                    var colorPalette = new PdfDictionary(Document);
                    byte[]? packedPaletteData = paletteData.Length >= 48 ? flateDecode.Encode(paletteData, Document.Options.FlateEncodeMode) : null; // Don’t compress small palettes.
                    if (packedPaletteData != null && packedPaletteData.Length + 20 < paletteData.Length) // +20: compensate for the overhead (estimated value).
                    {
                        // Create compressed color palette.
                        colorPalette.CreateStream(packedPaletteData);
                        colorPalette.Elements.SetInteger(PdfStream.Keys.Length, packedPaletteData.Length);
                        colorPalette.Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
                    }
                    else
                    {
                        // Create uncompressed color palette.
                        colorPalette.CreateStream(paletteData);
                        colorPalette.Elements[PdfStream.Keys.Length] = new PdfInteger(paletteData.Length);
                    }
                    Owner.IrefTable.Add(colorPalette);

                    var arrayColorSpace = new PdfArray(Document);
                    arrayColorSpace.Elements.Add(new PdfName("/Indexed"));
                    arrayColorSpace.Elements.Add(new PdfName("/DeviceRGB"));
                    arrayColorSpace.Elements.Add(new PdfInteger(paletteColors - 1));
                    arrayColorSpace.Elements.Add(colorPalette.Reference!); // NRT
                    Elements[Keys.ColorSpace] = arrayColorSpace;
                }
                else
                {
                    Elements[Keys.ColorSpace] = new PdfName("/DeviceGray");
                }

                if (_image.Interpolate)
                {
                    // #PDF-A
                    if (Document.IsPdfA)
                    {
                        PdfSharpLogHost.Logger.LogWarning("PDF/A: Image interpolation suppressed.");
                    }
                    else
                    {
                        Elements[Keys.Interpolate] = PdfBoolean.True;
                    }
                }
            }
        }

        /// <summary>
        /// Common keys for all streams.
        /// </summary>
        public new sealed class Keys : PdfXObject.Keys
        {
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional) The type of PDF object that this dictionary describes;
            /// if present, must be XObject for an image XObject.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Type = "/Type";

            /// <summary>
            /// (Required) The type of XObject that this dictionary describes;
            /// must be Image for an image XObject.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public const string Subtype = "/Subtype";

            /// <summary>
            /// (Required) The width of the image, in samples.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string Width = "/Width";

            /// <summary>
            /// (Required) The height of the image, in samples.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string Height = "/Height";

            /// <summary>
            /// (Required for images, except those that use the JPXDecode filter; not allowed for image masks)
            /// The color space in which image samples are specified; it can be any type of color space except
            /// Pattern. If the image uses the JPXDecode filter, this entry is optional:
            /// • If ColorSpace is present, any color space specifications in the JPEG2000 data are ignored.
            /// • If ColorSpace is absent, the color space specifications in the JPEG2000 data are used.
            ///   The Decode array is also ignored unless ImageMask is true.
            /// </summary>
            [KeyInfo(KeyType.NameOrArray | KeyType.Required)]
            public const string ColorSpace = "/ColorSpace";

            /// <summary>
            /// (Required except for image masks and images that use the JPXDecode filter)
            /// The number of bits used to represent each color component. Only a single value may be specified;
            /// the number of bits is the same for all color components. Valid values are 1, 2, 4, 8, and 
            /// (in PDF 1.5) 16. If ImageMask is true, this entry is optional, and if specified, its value 
            /// must be 1.
            /// If the image stream uses a filter, the value of BitsPerComponent must be consistent with the 
            /// size of the data samples that the filter delivers. In particular, a CCITTFaxDecode or JBIG2Decode 
            /// filter always delivers 1-bit samples, a RunLengthDecode or DCTDecode filter delivers 8-bit samples,
            /// and an LZWDecode or FlateDecode filter delivers samples of a specified size if a predictor function
            /// is used.
            /// If the image stream uses the JPXDecode filter, this entry is optional and ignored if present.
            /// The bit depth is determined in the process of decoding the JPEG2000 image.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string BitsPerComponent = "/BitsPerComponent";

            /// <summary>
            /// (Optional; PDF 1.1) The name of a color rendering intent to be used in rendering the image.
            /// Default value: the current rendering intent in the graphics state.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Intent = "/Intent";

            /// <summary>
            /// (Optional) A flag indicating whether the image is to be treated as an image mask.
            /// If this flag is true, the value of BitsPerComponent must be 1 and Mask and ColorSpace should
            /// not be specified; unmasked areas are painted using the current non-stroking color.
            /// Default value: false.
            /// </summary>
            [KeyInfo(KeyType.Boolean | KeyType.Optional)]
            public const string ImageMask = "/ImageMask";

            /// <summary>
            /// (Optional except for image masks; not allowed for image masks; PDF 1.3)
            /// An image XObject defining an image mask to be applied to this image, or an array specifying 
            /// a range of colors to be applied to it as a color key mask. If ImageMask is true, this entry
            /// must not be present.
            /// </summary>
            [KeyInfo(KeyType.StreamOrArray | KeyType.Optional)]
            public const string Mask = "/Mask";

            /// <summary>
            /// (Optional) An array of numbers describing how to map image samples into the range of values
            /// appropriate for the image’s color space. If ImageMask is true, the array must be either
            /// [0 1] or [1 0]; otherwise, its length must be twice the number of color components required 
            /// by ColorSpace. If the image uses the JPXDecode filter and ImageMask is false, Decode is ignored.
            /// Default value: see “Decode Arrays”.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string Decode = "/Decode";

            /// <summary>
            /// (Optional) A flag indicating whether image interpolation is to be performed. 
            /// Default value: false.
            /// </summary>
            [KeyInfo(KeyType.Boolean | KeyType.Optional)]
            public const string Interpolate = "/Interpolate";

            /// <summary>
            /// (Optional; PDF 1.3) An array of alternate image dictionaries for this image. The order of 
            /// elements within the array has no significance. This entry may not be present in an image 
            /// XObject that is itself an alternate image.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string Alternates = "/Alternates";

            /// <summary>
            /// (Optional; PDF 1.4) A subsidiary image XObject defining a soft-mask image to be used as a 
            /// source of mask shape or mask opacity values in the transparent imaging model. The alpha 
            /// source parameter in the graphics state determines whether the mask values are interpreted as
            /// shape or opacity. If present, this entry overrides the current soft mask in the graphics state,
            /// as well as the image’s Mask entry, if any. (However, the other transparency related graphics 
            /// state parameters — blend mode and alpha constant — remain in effect.) If SMask is absent, the 
            /// image has no associated soft mask (although the current soft mask in the graphics state may
            /// still apply).
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string SMask = "/SMask";

            /// <summary>
            /// (Optional for images that use the JPXDecode filter, meaningless otherwise; PDF 1.5)
            /// A code specifying how soft-mask information encoded with image samples should be used:
            /// 0 If present, encoded soft-mask image information should be ignored.
            /// 1 The image’s data stream includes encoded soft-mask values. An application can create
            ///   a soft-mask image from the information to be used as a source of mask shape or mask 
            ///   opacity in the transparency imaging model.
            /// 2 The image’s data stream includes color channels that have been preblended with a 
            ///   background; the image data also includes an opacity channel. An application can create
            ///   a soft-mask image with a Matte entry from the opacity channel information to be used as
            ///   a source of mask shape or mask opacity in the transparency model. If this entry has a 
            ///   nonzero value, SMask should not be specified.
            /// Default value: 0.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string SMaskInData = "/SMaskInData";

            /// <summary>
            /// (Required in PDF 1.0; optional otherwise) The name by which this image XObject is 
            /// referenced in the XObject subdictionary of the current resource dictionary.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Name = "/Name";

            /// <summary>
            /// (Required if the image is a structural content item; PDF 1.3) The integer key of the 
            /// image’s entry in the structural parent tree.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string StructParent = "/StructParent";

            /// <summary>
            /// (Optional; PDF 1.3; indirect reference preferred) The digital identifier of the image’s
            /// parent Web Capture content set.
            /// </summary>
            [KeyInfo(KeyType.String | KeyType.Optional)]
            public const string ID = "/ID";

            /// <summary>
            /// (Optional; PDF 1.2) An OPI version dictionary for the image. If ImageMask is true, 
            /// this entry is ignored.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string OPI = "/OPI";

            /// <summary>
            /// (Optional; PDF 1.4) A metadata stream containing metadata for the image.
            /// </summary>
            [KeyInfo(KeyType.Stream | KeyType.Optional)]
            public const string Metadata = "/Metadata";

            /// <summary>
            /// (Optional; PDF 1.5) An optional content group or optional content membership dictionary,
            /// specifying the optional content properties for this image XObject. Before the image is
            /// processed, its visibility is determined based on this entry. If it is determined to be 
            /// invisible, the entire image is skipped, as if there were no Do operator to invoke it.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string OC = "/OC";

            // ReSharper restore InconsistentNaming
        }
    }
}
