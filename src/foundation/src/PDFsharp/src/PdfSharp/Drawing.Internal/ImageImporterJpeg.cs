// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Text;
using PdfSharp.Pdf;

namespace PdfSharp.Drawing.Internal
{
    // ReSharper disable once InconsistentNaming
    class ImageImporterJpeg : ImageImporterRoot, IImageImporter
    {
        // TODO Find information about JPEG2000.

        // Notes: JFIF is big-endian.

        public ImportedImage? ImportImage(StreamReaderHelper stream)
        {
            try
            {

                stream.CurrentOffset = 0;
                // Test 2 magic bytes.
                if (TestFileHeader(stream))
                {
                    // Skip over 2 magic bytes.
                    stream.CurrentOffset += 2;

                    var ipd = new ImagePrivateDataDct(stream.Data, stream.Length);
                    var ii = new ImportedImageJpeg(this, ipd);
                    ii.Information.DefaultDPI = 72; // Assume 72 DPI if information not provided in the file.
                    if (TestJfifHeader(stream, ii))
                    {
                        bool colorHeader = false, infoHeader = false;

                        while (MoveToNextHeader(stream))
                        {
                            if (TestColorFormatHeader(stream, ii))
                            {
                                colorHeader = true;
                            }
                            else if (TestInfoHeader(stream, ii))
                            {
                                infoHeader = true;
                            }
                        }
                        if (colorHeader && infoHeader)
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

        bool TestFileHeader(StreamReaderHelper stream)
        {
            // File must start with 0xffd8.
            return stream.GetWord(0, true) == 0xffd8;
        }

        bool TestJfifHeader(StreamReaderHelper stream, ImportedImage ii)
        {
            // Just in case the header is not the first block in the file, search through all blocks now.
            //var currentOffset = stream.CurrentOffset;

            bool header = TestJfifHeaderWorker(stream, ii) ||
                          TestExifHeaderWorker(stream/*, ii*/) ||
                          TestApp2HeaderWorker(stream) ||
                          TestApp13HeaderWorker(stream);

            return header;
        }

        bool TestJfifHeaderWorker(StreamReaderHelper stream, ImportedImage ii)
        {
            // The App0 header should be the first header in every JFIF file.
            if (stream.GetWord(0, true) == 0xffe0)
            {
                // TODO Skip EXIF header if it precedes the JFIF header.

                // Now check for text "JFIF".
                if (stream.GetDWord(4, true) == 0x4a464946)
                {
                    int blockLength = stream.GetWord(2, true);
                    if (blockLength >= 16)
                    {
                        // ReSharper disable once UnusedVariable
                        int version = stream.GetWord(9, true);
                        int units = stream.GetByte(11);
                        int densityX = stream.GetWord(12, true);
                        int densityY = stream.GetWord(14, true);

                        switch (units)
                        {
                            case 0: // Aspect ratio only.
                                ii.Information.HorizontalAspectRatio = densityX;
                                ii.Information.VerticalAspectRatio = densityY;
                                break;
                            case 1: // DPI.
                                ii.Information.HorizontalDPI = densityX;
                                ii.Information.VerticalDPI = densityY;
                                break;
                            case 2: // DPCM.
                                ii.Information.HorizontalDPM = densityX * 100;
                                ii.Information.VerticalDPM = densityY * 100;
                                break;
                        }

                        // More information here? More tests?
                        return true;
                    }
                }
            }
            return false;
        }

        bool TestExifHeaderWorker(StreamReaderHelper stream/*, ImportedImage ii*/)
        {
            // The App0 header should be the first header in every JFIF file.
            if (stream.GetWord(0, true) == 0xffe1)
            {
                // Now check for text "Exif".
                if (stream.GetDWord(4, true) == 0x45786966)
                {
                    // We expect 00 00 after Exif.
                    int eos = stream.GetWord(8, true);
                    // We expect MM or II.
                    int ft = stream.GetWord(10, true);
                    if (eos == 0 &&
                        (ft == 0x4d4d || ft == 0x4949))
                    {
                        // More information here? More tests?
                        // TODO Find JFIF header in the EXIF header.
                        // EXIF headers are similar to TIFF.
                        return true;
                    }
                }
            }
            return false;
        }

        bool TestApp2HeaderWorker(StreamReaderHelper stream)
        {
            // Check for APP2 header.
            if (stream.GetWord(0, true) == 0xffe2)
            {
                int length = stream.GetWord(2, true);

                StringBuilder identifier = new();
                int idx = 4;
                do
                {
                    byte c = stream.GetByte(idx);
                    if (c == 0x00)
                    {
                        break;
                    }
                    identifier.Append((char)c);
                    ++idx;
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

        bool TestApp13HeaderWorker(StreamReaderHelper stream)
        {
            // Check for APP13 header.
            if (stream.GetWord(0, true) == 0xffed)
            {
                int length = stream.GetWord(2, true);

                StringBuilder identifier = new();
                int idx = 4;
                do
                {
                    byte c = stream.GetByte(idx);
                    if (c == 0x00)
                    {
                        break;
                    }
                    identifier.Append((char)c);
                    ++idx;
                } while (idx < length);

                var id = identifier.ToString();
                if (!id.StartsWith("Photoshop ") && !id.StartsWith("Adobe_Photoshop")) // "Photoshop 3.0", "Adobe_Photoshop2.5:", etc.
                {
                    return false;
                }

                ++idx;
                if (idx + 3 < length && stream.GetDWord(idx, true) == 0x3842494d) // 8BIM
                {
                    return true;
                }
            }
            return false;
        }

        bool TestColorFormatHeader(StreamReaderHelper stream, ImportedImage ii)
        {
            // The SOS header (start of scan).
            if (stream.GetWord(0, true) == 0xffda)
            {
                int components = stream.GetByte(4);
                if (components < 1 || components > 4 || components == 2)
                    return false;
                // 1 for grayscale, 3 for RGB, 4 for CMYK.

                int blockLength = stream.GetWord(2, true);
                // Integrity check: correct size?
                if (blockLength != 6 + 2 * components)
                    return false;

                // Eventually do more tests here.
                // Magic: we assume that all JPEG files with 4 components are RGBW (inverted CMYK) and not CMYK.
                // We add a test to tell CMYK from RGBW when we encounter a test file in CMYK format.
                ii.Information.ImageFormat = components == 3 ? ImageInformation.ImageFormats.JPEG :
                    (components == 1 ? ImageInformation.ImageFormats.JPEGGRAY : ImageInformation.ImageFormats.JPEGRGBW);

                return true;
            }
            return false;
        }

        bool TestInfoHeader(StreamReaderHelper stream, ImportedImage ii)
        {
            // The SOF header (start of frame).
            int header = stream.GetWord(0, true);
            if (header >= 0xffc0 && header <= 0xffc3 ||
                header >= 0xffc9 && header <= 0xffcb)
            {
                // Lines in image.
                int sizeY = stream.GetWord(5, true);
                // Samples per line.
                int sizeX = stream.GetWord(7, true);

                // $THHO TODO: Check if we always get useful information here.
                ii.Information.Width = (uint)sizeX;
                ii.Information.Height = (uint)sizeY;

                return true;
            }
            return false;
        }

        bool MoveToNextHeader(StreamReaderHelper stream)
        {
            int blockLength = stream.GetWord(2, true);

            int headerMagic = stream.GetByte(0);
            int headerType = stream.GetByte(1);

            if (headerMagic == 0xff)
            {
                // EOI: last header.
                if (headerType == 0xd9)
                    return false;

                // Check for standalone markers.
                if (headerType == 0x01 || headerType >= 0xd0 && headerType <= 0xd7)
                {
                    stream.CurrentOffset += 2;
                    return true;
                }

                // Now assume header with block size.
                stream.CurrentOffset += 2 + blockLength;
                return true;
            }
            return false;
        }

        public ImageData PrepareImage(ImagePrivateData data)
        {
            throw new NotImplementedException();
        }

        //int GetJpgSizeTestCode(byte[] pData, uint FileSizeLow, out int pWidth, out int pHeight)
        //{
        //    pWidth = -1;
        //    pHeight = -1;

        //    int i = 0;

        //    if ((pData[i] == 0xFF) && (pData[i + 1] == 0xD8) && (pData[i + 2] == 0xFF) && (pData[i + 3] == 0xE0))
        //    {
        //        i += 4;

        //        // Check for valid JPEG header (null terminated JFIF)
        //        if ((pData[i + 2] == 'J') && (pData[i + 3] == 'F') && (pData[i + 4] == 'I') && (pData[i + 5] == 'F')
        //            && (pData[i + 6] == 0x00))
        //        {

        //            //Retrieve the block length of the first block since the first block will not contain the size of file
        //            int block_length = pData[i] * 256 + pData[i + 1];

        //            while (i < FileSizeLow)
        //            {
        //                //Increase the file index to get to the next block
        //                i += block_length;

        //                if (i >= FileSizeLow)
        //                {
        //                    //Check to protect against segmentation faults
        //                    return -1;
        //                }

        //                if (pData[i] != 0xFF)
        //                {
        //                    return -2;
        //                }

        //                if (pData[i + 1] == 0xC0)
        //                {
        //                    //0xFFC0 is the "Start of frame" marker which contains the file size
        //                    //The structure of the 0xFFC0 block is quite simple [0xFFC0][ushort length][uchar precision][ushort x][ushort y]
        //                    pHeight = pData[i + 5] * 256 + pData[i + 6];
        //                    pWidth = pData[i + 7] * 256 + pData[i + 8];

        //                    return 0;
        //                }
        //                else
        //                {
        //                    i += 2; //Skip the block marker

        //                    //Go to the next block
        //                    block_length = pData[i] * 256 + pData[i + 1];
        //                }
        //            }

        //            //If this point is reached then no size was found
        //            return -3;
        //        }
        //        else
        //        {
        //            return -4;
        //        } //Not a valid JFIF string
        //    }
        //    else
        //    {
        //        return -5;
        //    } //Not a valid SOI header

        //    //return -6;
        //}  // GetJpgSize
    }

    /// <summary>
    /// Imported JPEG image.
    /// </summary>
    class ImportedImageJpeg : ImportedImage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportedImageJpeg"/> class.
        /// </summary>
        public ImportedImageJpeg(IImageImporter importer, ImagePrivateDataDct data)
            : base(importer, data)
        { }

        internal override ImageData PrepareImageData(PdfDocumentOptions options)
        {
            var data = (ImagePrivateDataDct?)Data ?? NRT.ThrowOnNull<ImagePrivateDataDct>();
            var imageData = new ImageDataDct
            {
                Data = data.Data,
                Length = data.Length
            };

            return imageData;
        }
    }

    /// <summary>
    /// Contains data needed for PDF. Will be prepared when needed.
    /// </summary>
    class ImageDataDct : ImageData
    {
        /// <summary>
        /// Gets the data.
        /// </summary>
        public byte[] Data { get; internal set; } = null!;

        /// <summary>
        /// Gets the length.
        /// </summary>
        public int Length { get; internal set; }
    }

    /*internal*/
    /// <summary>
    /// Private data for JPEG images.
    /// </summary>
    class ImagePrivateDataDct : ImagePrivateData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePrivateDataDct"/> class.
        /// </summary>
        public ImagePrivateDataDct(byte[] data, int length)
        {
            Data = data;
            Length = length;
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Gets the length.
        /// </summary>
        public int Length { get; }
    }
}
