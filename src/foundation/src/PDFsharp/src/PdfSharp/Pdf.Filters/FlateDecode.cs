﻿// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.IO.Compression;

namespace PdfSharp.Pdf.Filters
{
    /// <summary>
    /// Implements the FlateDecode filter by wrapping SharpZipLib.
    /// </summary>
    public class FlateDecode : Filter
    {
        // Reference: 3.3.3  LZWDecode and FlateDecode Filters / Page 71

        /// <summary>
        /// Encodes the specified data.
        /// </summary>
        public override byte[] Encode(byte[] data)
        {
            return Encode(data, PdfFlateEncodeMode.Default);
        }

        /// <summary>
        /// Encodes the specified data.
        /// </summary>
        public byte[] Encode(byte[] data, PdfFlateEncodeMode mode)
        {
            using var ms = new MemoryStream();

            CompressionLevel level;
            switch (mode)
            {
                case PdfFlateEncodeMode.BestCompression:
#if NET462 || NETSTANDARD2_0
                    level = CompressionLevel.Optimal;
#else
                    level = CompressionLevel.SmallestSize;
#endif
                    break;
                case PdfFlateEncodeMode.BestSpeed:
                    level = CompressionLevel.Fastest;
                    break;
                default:
                    level = CompressionLevel.Optimal;
                    break;
            }

            // This is the header SharpZipLib produced previously.
            // See http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=97064
            // 
            // Excerpt from the RFC 1950 specs for first byte:
            //
            // CMF (Compression Method and flags)
            //    This byte is divided into a 4-bit compression method and a 4-
            //    bit information field depending on the compression method.
            //
            //      bits 0 to 3  CM     Compression method
            //      bits 4 to 7  CINFO  Compression info
            //
            // CM (Compression method)
            //    This identifies the compression method used in the file. CM = 8
            //    denotes the "deflate" compression method with a window size up
            //    to 32K.  This is the method used by gzip and PNG (see
            //    references [1] and [2] in Chapter 3, below, for the reference
            //    documents).  CM = 15 is reserved.  It might be used in a future
            //    version of this specification to indicate the presence of an
            //    extra field before the compressed data.
            //
            // CINFO (Compression info)
            //    For CM = 8, CINFO is the base-2 logarithm of the LZ77 window
            //    size, minus eight (CINFO=7 indicates a 32K window size). Values
            //    of CINFO above 7 are not allowed in this version of the
            //    specification.  CINFO is not defined in this specification for
            //    CM not equal to 8.
            ms.WriteByte(0x78);
            // Excerpt from the RFC 1950 specs for second byte:
            //
            // FLG (FLaGs)
            //    This flag byte is divided as follows:
            //
            //       bits 0 to 4  FCHECK  (check bits for CMF and FLG)
            //       bit  5       FDICT   (preset dictionary)
            //       bits 6 to 7  FLEVEL  (compression level)
            //
            //    The FCHECK value must be such that CMF and FLG, when viewed as
            //    a 16-bit unsigned integer stored in MSB order (CMF*256 + FLG),
            //    is a multiple of 31.
            //
            // FDICT (Preset dictionary)
            //    If FDICT is set, a DICT dictionary identifier is present
            //    immediately after the FLG byte. The dictionary is a sequence of
            //    bytes which are initially fed to the compressor without
            //    producing any compressed output. DICT is the Adler-32 checksum
            //    of this sequence of bytes (see the definition of ADLER32
            //    below).  The decompressor can use this identifier to determine
            //    which dictionary has been used by the compressor.
            //
            // FLEVEL (Compression level)
            //    These flags are available for use by specific compression
            //    methods.  The "deflate" method (CM = 8) sets these flags as
            //    follows:
            //
            //       0 - compressor used fastest algorithm
            //       1 - compressor used fast algorithm
            //       2 - compressor used default algorithm
            //       3 - compressor used maximum compression, slowest algorithm
            //
            //    The information in FLEVEL is not needed for decompression; it
            //    is there to indicate if recompression might be worthwhile.
            ms.WriteByte(0xDA); // FLEVEL may not always be correct here, but that’s okay.

            using var zip = new DeflateStream(ms, level, true);
            zip.Write(data, 0, data.Length);
            // Flush has no effect on DeflateStream.
            // Must call Close to ensure all bytes are written with .NET 4.6.2.
            // Works with .NET 6 even without Close. But when adding support for .NET 4.6.2, we searched a while until we found that only the Close was needed.
            zip.Close();

            ms.Capacity = (int)ms.Length;
            return ms.GetBuffer();
        }

        /// <summary>
        /// Decodes the specified data.
        /// </summary>
        public override byte[] Decode(byte[] data, FilterParms? parms)
        {
            using var msInput = new MemoryStream(data);
            using var msOutput = new MemoryStream();

            // ReSharper disable once RedundantAssignment
            var header = new byte[]
            {
                (byte)msInput.ReadByte(), // CMF (Compression Method and flags)
                (byte)msInput.ReadByte()  // Flags
            };
#if true
            Debug.Assert((header[0] & 0xF) == 0x8); // Compression method must be deflate.
            Debug.Assert((header[1] & 0x20) == 0);  // DeflateStream does not support Adler32.
#endif

            using var zip = new DeflateStream(msInput, CompressionMode.Decompress, true);
            zip.CopyTo(msOutput);
            msOutput.Flush();

            if (msOutput.Length >= 0)
            {
                msOutput.Capacity = (int)msOutput.Length;
                if (parms?.DecodeParms != null)
                    return StreamDecoder.Decode(msOutput.GetBuffer(), parms.DecodeParms);
            }

            return msOutput.GetBuffer();
        }
    }
}
