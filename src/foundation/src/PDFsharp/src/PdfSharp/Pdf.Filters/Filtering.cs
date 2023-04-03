// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Pdf.Filters
{
    /// <summary>
    /// Applies standard filters to streams.
    /// </summary>
    public static class Filtering
    {
        /// <summary>
        /// Gets the filter specified by the case sensitive name.
        /// </summary>
        public static Filter GetFilter(string filterName)
        {
            if (filterName.StartsWith("/", StringComparison.Ordinal))
                filterName = filterName.Substring(1);

            // Some tools use abbreviations.
            switch (filterName)
            {
                case "ASCIIHexDecode":
                case "AHx":
                    return _asciiHexDecode ??= new AsciiHexDecode();

                case "ASCII85Decode":
                case "A85":
                    return _ascii85Decode ??= new Ascii85Decode();

                case "LZWDecode":
                case "LZW":
                    return _lzwDecode ??= new LzwDecode();

                case "FlateDecode":
                case "Fl":
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

                case "DCTDecode":
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

                case "RunLengthDecode":
                case "CCITTFaxDecode":
                case "JBIG2Decode":
                case "JPXDecode":
                case "Crypt":
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
                // Array length of filter and decode parms should match. If they don't, return data unmodified.
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
