// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;

namespace PdfSharp.Internal.Imaging
{
    sealed class JpegImageImporter : BitmapImageImporter
    {
        public override bool TryImport(byte[] bitmap,
            [MaybeNullWhen(false)] out ImportedImage importedImage)
        {
            importedImage = null;
            try
            {
                var dataParser = new DataParser(bitmap);
                dataParser.CurrentOffset = 0;
                // Test 2 magic bytes.
                if (TestFileHeader(dataParser))
                {
                    var result = new ImportedJpegImage();
                    importedImage = result;

                    // Skip over 2 magic bytes.
                    dataParser.CurrentOffset += 2;

                    result.DpiX = 0;
                    result.DpiY = 0;

                    // Check for known headers here, but treat header as optional.
                    var header = TestJfifHeader(dataParser, result);

                    bool colorHeader = false, infoHeader = false;

                    while (MoveToNextHeader(dataParser))
                    {
                        if (TestColorFormatHeader(dataParser, result))
                        {
                            colorHeader = true;
                        }
                        else if (TestInfoHeader(dataParser, result))
                        {
                            infoHeader = true;
                        }
                    }

                    if (colorHeader && infoHeader)
                    {
                        importedImage.ImageData = bitmap;
                        if (result.DpiX == 0 || result.DpiY == 0)
                        {
                            result.DpiX = 72; // Assume 72 DPI if information not provided in the file.
                            if (result.HorizontalAspectRatio != 0 && result.VerticalAspectRatio != 0)
                            {
                                // Calculate DpiY to reflect the aspect ratio of the image.
                                result.DpiY = 72 * result.VerticalAspectRatio / result.HorizontalAspectRatio;
                            }
                            else
                            {
                                result.DpiY = 72; // Assume 72 DPI if information not provided in the file.
                            }
                        }

                        UpdateWidthAndHeight(result);


                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Eat exceptions to have this image importer skipped.
                // We try to find an image importer that can handle the image.
                LogTryOpenFailure(nameof(JpegImageImporter), ex);
            }
            return false;
        }

        bool TestFileHeader(DataParser dataParser)
        {
            // File must start with 0xffd8.
            return dataParser.GetWord(0, true) == 0xffd8;
        }

        bool TestJfifHeader(DataParser dataParser, ImportedJpegImage ii)
        {
            // Just in case the header is not the first block in the file, search through all blocks now.
            //var currentOffset = dataParser.CurrentOffset;

            bool header = TestJfifHeaderWorker(dataParser, ii) ||
                          TestExifHeaderWorker(dataParser/*, ii*/) ||
                          TestApp2HeaderWorker(dataParser) ||
                          TestApp13HeaderWorker(dataParser);

            return header;
        }

        bool TestJfifHeaderWorker(DataParser dataParser, ImportedJpegImage ii)
        {
            // The App0 header should be the first header in every JFIF file.
            if (dataParser.GetWord(0, true) == 0xffe0)
            {
                // TODO_OLD Skip EXIF header if it precedes the JFIF header.

                // Now check for text "JFIF".
                if (dataParser.GetDWord(4, true) == 0x4a464946)
                {
                    int blockLength = dataParser.GetWord(2, true);
                    if (blockLength >= 16)
                    {
                        // ReSharper disable once UnusedVariable
                        int version = dataParser.GetWord(9, true);
                        int units = dataParser.GetByte(11);
                        int densityX = dataParser.GetWord(12, true);
                        int densityY = dataParser.GetWord(14, true);

                        switch (units)
                        {
                            case 0: // Aspect ratio only. Leave DpiX at 72 and adjust DpiY.
                                ii.HorizontalAspectRatio = densityX;
                                ii.VerticalAspectRatio = densityY;
                                if (densityX > 0 && densityY > 0)
                                {
                                    ii.DpiX = 72;
                                    ii.DpiY = (int)(72d * densityX / densityY);
                                }
                                break;
                            case 1: // DPI.
                                ii.DpiX = densityX;
                                ii.DpiY = densityY;
                                break;
                            case 2: // DPCM: Calculate DotsPerMeter and DotsPerInch from DotsPerCentimeter.
                                ii.DpmX = densityX * 100;
                                ii.DpmY = densityY * 100;
                                ii.DpiX = ii.DpmX / ImportedImage.InchPerMeter;
                                ii.DpiY = ii.DpmY / ImportedImage.InchPerMeter;
                                RoundDpiAfterConversionFromMetricValue(ii);
                                break;
                        }

                        // More information here? More tests?
                        return true;
                    }
                }
            }
            return false;
        }

        bool TestExifHeaderWorker(DataParser dataParser)
        {
            // The App0 header should be the first header in every JFIF file.
            if (dataParser.GetWord(0, true) == 0xffe1)
            {
                // Now check for text "Exif".
                if (dataParser.GetDWord(4, true) == 0x45786966)
                {
                    // We expect 00 00 after Exif.
                    int eos = dataParser.GetWord(8, true);
                    // We expect MM or II.
                    int ft = dataParser.GetWord(10, true);
                    if (eos == 0 &&
                        (ft == 0x4d4d || ft == 0x4949))
                    {
                        // More information here? More tests?
                        // TODO_OLD Find JFIF header in the EXIF header.
                        // EXIF headers are similar to TIFF.
                        return true;
                    }
                }
            }
            return false;
        }

        bool TestApp2HeaderWorker(DataParser dataParser)
        {
            // Check for APP2 header.
            if (dataParser.GetWord(0, true) == 0xffe2)
            {
                int length = dataParser.GetWord(2, true);

                StringBuilder identifier = new();
                int idx = 4;
                do
                {
                    byte c = dataParser.GetByte(idx);
                    if (c == 0x00)
                    {
                        break;
                    }
                    identifier.Append((char)c);
                    idx++;
                } while (idx < length);

                var id = identifier.ToString();
                if (!id.Equals("ICC_PROFILE"))
                {
                    return false;
                }

                return true;
            }
            return false;
        }

        bool TestApp13HeaderWorker(DataParser dataParser)
        {
            // Check for APP13 header.
            if (dataParser.GetWord(0, true) == 0xffed)
            {
                int length = dataParser.GetWord(2, true);

                StringBuilder identifier = new();
                int idx = 4;
                do
                {
                    byte c = dataParser.GetByte(idx);
                    if (c == 0x00)
                    {
                        break;
                    }
                    identifier.Append((char)c);
                    idx++;
                } while (idx < length);

                var id = identifier.ToString();
                if (!id.StartsWith("Photoshop ") && !id.StartsWith("Adobe_Photoshop")) // "Photoshop 3.0", "Adobe_Photoshop2.5:", etc.
                {
                    return false;
                }

                idx++;
                if (idx + 3 < length && dataParser.GetDWord(idx, true) == 0x3842494d) // 8BIM
                {
                    return true;
                }
            }
            return false;
        }

        bool TestColorFormatHeader(DataParser dataParser, ImportedJpegImage ii)
        {
            // The SOS header (start of scan).
            if (dataParser.GetWord(0, true) == 0xffda)
            {
                int components = dataParser.GetByte(4);
                if (components < 1 || components > 4 || components == 2)
                    return false;
                // 1 for grayscale, 3 for RGB, 4 for CMYK.

                int blockLength = dataParser.GetWord(2, true);
                // Integrity check: correct size?
                if (blockLength != 6 + 2 * components)
                    return false;

                // Eventually do more tests here.
                // Info: we assume that all JPEG files with 4 components are RGBW (inverted CMYK) and not CMYK.
                // We add a test to tell CMYK from RGBW when we encounter a test file in CMYK format.
                var format = components == 3
                    ? ImageFormats.JPEG
                    : (components == 1
                        ? ImageFormats.JPEGGRAY
                        : ImageFormats.JPEGRGBW);
                if (ii.ImageFormat == ImageFormats.Unknown)
                    ii.ImageFormat = format;
#if DEBUG
                else
                {
                    Debug.Assert(format == ii.ImageFormat);
                }
#endif

                return true;
            }
            return false;
        }

        bool TestInfoHeader(DataParser dataParser, ImportedJpegImage ii)
        {
            // The SOF header (start of frame).
            int header = dataParser.GetWord(0, true);
            if (header >= 0xffc0 && header <= 0xffc3 ||
                header >= 0xffc9 && header <= 0xffcb)
            {
                // Lines in image.
                int sizeY = dataParser.GetWord(5, true);
                // Samples per line.
                int sizeX = dataParser.GetWord(7, true);

                int components = dataParser.GetByte(9);
                if (components is 1 or 3 or 4)
                {
                    // Info: we assume that all JPEG files with 4 components are RGBW (inverted CMYK) and not CMYK.
                    // We add a test to tell CMYK from RGBW when we encounter a test file in CMYK format.
                    var format = components == 3
                        ? ImageFormats.JPEG
                        : (components == 1
                            ? ImageFormats.JPEGGRAY
                            : ImageFormats.JPEGRGBW);

                    // We have more faith in the information from the SOF header than the SOS header.
#if DEBUG
                    // Assert information in Debug mode.
                    if (ii.ImageFormat != ImageFormats.Unknown)
                    {
                        Debug.Assert(format == ii.ImageFormat);
                    }
#endif
                    // Always use information from SOF header. Just overwrite previously set format, maybe from SOS header.
                    ii.ImageFormat = format;
                }

                ii.PixelWidth = sizeX;
                ii.PixelHeight = sizeY;

                return true;
            }
            return false;
        }

        bool MoveToNextHeader(DataParser dataParser)
        {
            int blockLength = dataParser.GetWord(2, true);

            int headerMagic = dataParser.GetByte(0);
            int headerType = dataParser.GetByte(1);

            if (headerMagic == 0xff)
            {
                // EOI: last header.
                if (headerType == 0xd9)
                    return false;

                // 0xff followed by 0x00 is not a valid JPEG tag. Skip this entry.
                // If the value 0xFF is ever needed in a JPEG file, it must be escaped by immediately
                // following it with 0x00. This is called "byte stuffing".
                // Source: https://www.ccoderun.ca/programming/2017-01-31_jpeg/
                // Check for standalone markers 0x01 and 0xd0 through 0xd7.
                if (headerType is 0x00 or 0x01 or >= 0xd0 and <= 0xd7)
                {
                    dataParser.CurrentOffset += 2;
                    return true;
                }

                // Now assume header with block size.
                dataParser.CurrentOffset += 2 + blockLength;
                return true;
            }
            return false;
        }
    }
}
