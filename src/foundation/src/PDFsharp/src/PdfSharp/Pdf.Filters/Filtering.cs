// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Pdf.Filters
{
    /// <summary>
    /// Names of the PDF standard filters and their abbreviations.
    /// </summary>
    public static class PdfFilterNames
    {
        /// <summary>
        /// Decodes data encoded in an ASCII hexadecimal representation, reproducing the original.
        /// binary data.
        /// </summary>
        public const string AsciiHexDecode = "ASCIIHexDecode";

        /// <summary>
        /// Abbreviation of ASCIIHexDecode.
        /// </summary>
        public const string AsciiHexDecodeAbbreviation = "AHx";

        /// <summary>
        /// Decodes data encoded in an ASCII base-85 representation, reproducing the original
        /// binary data.
        /// </summary>
        public const string Ascii85Decode = "ASCII85Decode";

        /// <summary>
        /// Abbreviation of ASCII85Decode.
        /// </summary>
        public const string Ascii85DecodeAbbreviation = "A85";

        /// <summary>
        /// Decompresses data encoded using the LZW(Lempel-Ziv-Welch) adaptive compression method,
        /// reproducing the original text or binary data.
        /// </summary>
        public const string LzwDecode = "LZWDecode";

        /// <summary>
        /// Abbreviation of LZWDecode.
        /// </summary>
        public const string LzwDecodeAbbreviation = "LZW";

        /// <summary>
        /// (PDF 1.2) Decompresses data encoded using the zlib/deflate compression method,
        /// reproducing the original text or binary data.
        /// </summary>
        public const string FlateDecode = "FlateDecode";

        /// <summary>
        /// Abbreviation of FlateDecode.
        /// </summary>
        public const string FlateDecodeAbbreviation = "Fl";

        /// <summary>
        /// Decompresses data encoded using a byte-oriented run-length encoding algorithm,
        /// reproducing the original text or binary data(typically monochrome image data,
        /// or any data that contains frequent long runs of a single byte value).
        /// </summary>
        public const string RunLengthDecode = "RunLengthDecode";

        /// <summary>
        /// Abbreviation of RunLengthDecode.
        /// </summary>
        public const string RunLengthDecodeAbbreviation = "RL";

        /// <summary>
        /// Decompresses data encoded using the CCITT facsimile standard, reproducing the original
        /// data(typically monochrome image data at 1 bit per pixel).
        /// </summary>
        public const string CcittFaxDecode = "CCITTFaxDecode";

        /// <summary>
        /// Abbreviation of CCITTFaxDecode.
        /// </summary>
        public const string CcittFaxDecodeAbbreviation = "CCF";

        /// <summary>
        /// (PDF 1.4) Decompresses data encoded using the JBIG2 standard, reproducing the original
        /// monochrome(1 bit per pixel) image data(or an approximation of that data).
        /// </summary>
        public const string Jbig2Decode = "JBIG2Decode";

        /// <summary>
        /// Decompresses data encoded using a DCT(discrete cosine transform) technique based on the
        /// JPEG standard(ISO/IEC 10918), reproducing image sample data that approximates the original data.
        /// </summary>
        public const string DctDecode = "DCTDecode";

        /// <summary>
        /// Abbreviation of DCTDecode.
        /// </summary>
        public const string DctDecodeAbbreviation = "DCT";

        /// <summary>
        /// (PDF 1.5) Decompresses data encoded using the wavelet-based JPEG 2000 standard,
        /// reproducing the original image data.
        /// </summary>
        public const string JpxDecode = "JPXDecode";

        /// <summary>
        /// (PDF 1.5) Decrypts data encrypted by a security handler, reproducing the data
        /// as it was before encryption.
        /// </summary>
        public const string Crypt = "Crypt";
    }

    /// <summary>
    /// Applies standard filters to streams.
    /// </summary>
    public static class Filtering
    {
        /// <summary>
        /// Gets the filter specified by the case-sensitive name.
        /// </summary>
        public static Filter GetFilter(string filterName)
        {
            if (filterName.StartsWith("/", StringComparison.Ordinal))
                filterName = filterName.Substring(1);

            // Some tools use abbreviations.
            switch (filterName)
            {
                case PdfFilterNames.AsciiHexDecode:
                case PdfFilterNames.AsciiHexDecodeAbbreviation:
                    return _asciiHexDecode ??= new AsciiHexDecode();

                case PdfFilterNames.Ascii85Decode:
                case PdfFilterNames.Ascii85DecodeAbbreviation:
                    return _ascii85Decode ??= new Ascii85Decode();

                case PdfFilterNames.LzwDecode:
                case PdfFilterNames.LzwDecodeAbbreviation:
                    return _lzwDecode ??= new LzwDecode();

                case PdfFilterNames.FlateDecode:
                case PdfFilterNames.FlateDecodeAbbreviation:
                    return _flateDecode ??= new FlateDecode();

                //case "RunLengthDecode":
                //  if (RunLengthDecode == null)
                //    RunLengthDecode = new RunLengthDecode();
                //  return RunLengthDecode;
                //
                //case "CCITTFaxDecode":
                //  if (CCITTFaxDecode == null)
                //    CCITTFaxDecode = new CCITTFaxDecode();
                //  return CCITTFaxDecode;
                //
                //case "JBIG2Decode":
                //  if (JBIG2Decode == null)
                //    JBIG2Decode = new JBIG2Decode();
                //  return JBIG2Decode;

                case PdfFilterNames.DctDecode:
                case PdfFilterNames.DctDecodeAbbreviation:
                    return _dctDecode ??= new DctDecode();

                //case "JPXDecode":
                //  if (JPXDecode == null)
                //    JPXDecode = new JPXDecode();
                //  return JPXDecode;
                //
                //case "Crypt":
                //  if (Crypt == null)
                //    Crypt = new Crypt();
                //  return Crypt;

                
                case PdfFilterNames.RunLengthDecode:
                case PdfFilterNames.CcittFaxDecode:
                case PdfFilterNames.Jbig2Decode:
                case PdfFilterNames.JpxDecode:
                case PdfFilterNames.Crypt:
                    throw new NotImplementedException($"Filter not implemented: {filterName}");

            }
            throw new NotImplementedException("Unknown filter: " + filterName);
        }

        /// <summary>
        /// Gets the filter singleton.
        /// </summary>
        public static AsciiHexDecode AsciiHexDecode
            => _asciiHexDecode ??= new AsciiHexDecode();

        static AsciiHexDecode? _asciiHexDecode;

        /// <summary>
        /// Gets the filter singleton.
        /// </summary>
        public static Ascii85Decode Ascii85Decode => _ascii85Decode ??= new();

        static Ascii85Decode? _ascii85Decode;

        /// <summary>
        /// Gets the filter singleton.
        /// </summary>
        public static LzwDecode LzwDecode => _lzwDecode ??= new();

        static LzwDecode? _lzwDecode;

        /// <summary>
        /// Gets the filter singleton.
        /// </summary>
        public static FlateDecode FlateDecode => _flateDecode ??= new();

        static FlateDecode? _flateDecode;

        //runLengthDecode
        //ccittFaxDecode
        //jbig2Decode

        /// <summary>
        /// Gets the filter singleton.
        /// </summary>
        public static DctDecode DctDecode => _dctDecode ??= new();

        static DctDecode? _dctDecode;

        //jpxDecode
        //crypt

        /// <summary>
        /// Encodes the data with the specified filter.
        /// </summary>
        public static byte[]? Encode(byte[] data, string filterName)
        {
            var filter = GetFilter(filterName);
            return filter?.Encode(data);
        }

        /// <summary>
        /// Encodes a raw string with the specified filter.
        /// </summary>
        public static byte[]? Encode(string rawString, string filterName)
        {
            var filter = GetFilter(filterName);
            return filter?.Encode(rawString);
        }

        /// <summary>
        /// Decodes the data with the specified filter.
        /// </summary>
        public static byte[]? Decode(byte[] data, string filterName, FilterParms parms)
        {
            Filter filter = GetFilter(filterName);
            return filter?.Decode(data, parms);
        }

        /// <summary>
        /// Decodes the data with the specified filter.
        /// </summary>
        public static byte[]? Decode(byte[] data, string filterName)
        {
            var filter = GetFilter(filterName);
            return filter?.Decode(data);
        }

        /// <summary>
        /// Decodes the data with the specified filter.
        /// </summary>
        public static byte[] Decode(byte[] data, PdfItem filterItem, PdfItem? decodeParms)
        {
            byte[]? result = null;

#if true
            PdfReference.Dereference(ref filterItem);
#else
            if (filterItem is PdfReference iref)
            {
                Debug.Assert(iref.Value != null, "Indirect /Filter value is null");
                filterItem = iref.Value;
            }
#endif

            if (filterItem is PdfName && decodeParms is null or PdfDictionary)
            {
                var filter = GetFilter(filterItem.ToString()!);
                if (filter != null!)
                    result = filter.Decode(data, new FilterParms(decodeParms as PdfDictionary));
            }
            else if (filterItem is PdfArray itemArray && decodeParms is null or PdfArray)
            {
                var decodeArray = decodeParms as PdfArray;
                // Array length of filter and decode parms should match. If they don’t, return data unmodified.
                if (decodeArray != null && decodeArray.Elements.Count != itemArray.Elements.Count)
                    return data;
                for (var i = 0; i < itemArray.Elements.Count; i++)
                {
                    var item = itemArray.Elements[i];
                    var parms = decodeArray?.Elements[i];
                    data = Decode(data, item, parms);
                }
                result = data;
            }
            return result ?? NRT.ThrowOnNull<byte[]>();
        }

        /// <summary>
        /// Decodes to a raw string with the specified filter.
        /// </summary>
        public static string DecodeToString(byte[] data, string filterName, FilterParms parms)
        {
            var filter = GetFilter(filterName);
            return filter?.DecodeToString(data, parms) ?? "";
        }

        /// <summary>
        /// Decodes to a raw string with the specified filter.
        /// </summary>
        public static string DecodeToString(byte[] data, string filterName)
        {
            var filter = GetFilter(filterName);
            return filter.DecodeToString(data, null) ?? "";
        }
    }
}
