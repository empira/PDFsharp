// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Filters
{
    /// <summary>
    /// Implements the LzwDecode filter.
    /// </summary>
    public class LzwDecode : Filter
    {
        // Reference: 3.3.3  LZWDecode and FlateDecode Filters / Page 71

        /// <summary>
        /// Throws a NotImplementedException because the obsolete LZW encoding is not supported by PDFsharp.
        /// </summary>
        public override byte[] Encode(byte[] data)
        {
            throw new NotImplementedException("PDFsharp does not support LZW encoding.");
        }

        /// <summary>
        /// Decodes the specified data.
        /// </summary>
        public override byte[] Decode(byte[] data, FilterParms? parms)
        {
            if (data[0] == 0x00 && data[1] == 0x01)
                throw new Exception("LZW flavor not supported.");

            MemoryStream outputStream = new MemoryStream();

            InitializeDictionary();

            _data = data;
            _bytePointer = 0;
            _nextData = 0;
            _nextBits = 0;
            int code, oldCode = 0;
            byte[] str;

            while ((code = NextCode) != 257)
            {
                if (code == 256)
                {
                    InitializeDictionary();
                    code = NextCode;
                    if (code == 257)
                    {
                        break;
                    }
                    outputStream.Write(_stringTable[code], 0, _stringTable[code].Length);
                    oldCode = code;

                }
                else
                {
                    if (code < _tableIndex)
                    {
                        str = _stringTable[code];
                        outputStream.Write(str, 0, str.Length);
                        AddEntry(_stringTable[oldCode], str[0]);
                        oldCode = code;
                    }
                    else
                    {
                        str = _stringTable[oldCode];
                        outputStream.Write(str, 0, str.Length);
                        AddEntry(str, str[0]);
                        oldCode = code;
                    }
                }
            }

            if (outputStream.Length >= 0)
            {
                outputStream.Capacity = (int)outputStream.Length;

                if (parms?.DecodeParms != null)
                    return StreamDecoder.Decode(outputStream.GetBuffer(), parms.DecodeParms);

                return outputStream.GetBuffer();
            }
            return Array.Empty<byte>();
        }

        /// <summary>
        /// Initialize the dictionary.
        /// </summary>
        void InitializeDictionary()
        {
            _stringTable = new byte[8192][];

            for (int i = 0; i < 256; i++)
            {
                _stringTable[i] = new byte[1];
                _stringTable[i][0] = (byte)i;
            }

            _tableIndex = 258;
            _bitsToGet = 9;
        }

        /// <summary>
        /// Add a new entry to the Dictionary.
        /// </summary>
        void AddEntry(byte[] oldstring, byte newstring)
        {
            int length = oldstring.Length;
            byte[] str = new byte[length + 1];
            Array.Copy(oldstring, 0, str, 0, length);
            str[length] = newstring;

            _stringTable[_tableIndex++] = str;

            if (_tableIndex == 511)
                _bitsToGet = 10;
            else if (_tableIndex == 1023)
                _bitsToGet = 11;
            else if (_tableIndex == 2047)
                _bitsToGet = 12;
        }

        /// <summary>
        /// Returns the next set of bits.
        /// </summary>
        int NextCode
        {
            get
            {
                try
                {
                    _nextData = (_nextData << 8) | (_data[_bytePointer++] & 0xff);
                    _nextBits += 8;

                    if (_nextBits < _bitsToGet)
                    {
                        _nextData = (_nextData << 8) | (_data[_bytePointer++] & 0xff);
                        _nextBits += 8;
                    }

                    int code = (_nextData >> (_nextBits - _bitsToGet)) & _andTable[_bitsToGet - 9];
                    _nextBits -= _bitsToGet;

                    return code;
                }
                catch
                {
                    return 257;
                }
            }
        }

        readonly int[] _andTable = [511, 1023, 2047, 4095];
        byte[][] _stringTable = default!;
        byte[] _data = default!;
        int _tableIndex, _bitsToGet = 9;
        int _bytePointer;
        int _nextData = 0;
        int _nextBits = 0;
    }
}
