// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using Microsoft.Extensions.Logging;
using PdfSharp.Internal;
using PdfSharp.Logging;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf
{
    // TODO_OLD: Make code more readable with PDF 1.7 strings: text string, ASCII string, byte string etc.
    // See [xx]()

    /// <summary>
    /// Determines the encoding of a PdfString or PdfStringObject.
    /// </summary>
    [Flags]
    public enum PdfStringEncoding
    {
        /// <summary>
        /// The characters of the string are actually bytes with an unknown or context specific meaning or encoding.
        /// With this encoding the 8 high bits of each character is zero.
        /// </summary>
        RawEncoding = PdfStringFlags.RawEncoding,

        /// <summary>
        /// Not yet used by PDFsharp.
        /// </summary>
        StandardEncoding = PdfStringFlags.StandardEncoding,

        /// <summary>
        /// The characters of the string are actually bytes with PDF document encoding.
        /// With this encoding the 8 high bits of each character is zero.
        /// </summary>
        // ReSharper disable InconsistentNaming because the name is spelled as in the Adobe reference.
        PDFDocEncoding = PdfStringFlags.PDFDocEncoding,
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// The characters of the string are actually bytes with Windows ANSI encoding.
        /// With this encoding the 8 high bits of each character is zero.
        /// </summary>
        WinAnsiEncoding = PdfStringFlags.WinAnsiEncoding,

        /// <summary>
        /// Not yet used by PDFsharp.
        /// </summary>
        MacRomanEncoding = PdfStringFlags.MacRomanEncoding,

        /// <summary>
        /// Not yet used by PDFsharp.
        /// </summary>
        MacExpertEncoding = PdfStringFlags.MacExpertEncoding,

        /// <summary>
        /// The characters of the string are Unicode code units.
        /// Each char of the string is either a BMP code point or a high or low surrogate.
        /// </summary>
        Unicode = PdfStringFlags.Unicode,
    }

    /// <summary>
    /// Internal wrapper for PdfStringEncoding.
    /// </summary>
    [Flags]
    enum PdfStringFlags
    {
        // ReSharper disable InconsistentNaming
        RawEncoding = 0x00,        // Each char maps to a byte.
        StandardEncoding = 0x01,   // Not used by PDFsharp.
        PDFDocEncoding = 0x02,
        WinAnsiEncoding = 0x03,
        MacRomanEncoding = 0x04,   // Not used by PDFsharp.
        MacExpertEncoding = 0x05,  // Not used by PDFsharp.
        Unicode = 0x06,
        EncodingMask = 0x0F,

        HexLiteral = 0x80,
        // ReSharper restore InconsistentNaming
    }

    /// <summary>
    /// Represents a direct text string value.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public sealed class PdfString : PdfItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfString"/> class.
        /// </summary>
        public PdfString()
        {
            // Redundant assignment.
            //_flags = PdfStringFlags.RawEncoding;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfString"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public PdfString(string value)
        {
#if true
            if (!IsRawEncoding(value))
                _flags = PdfStringFlags.Unicode;
            _value = value;
#else
            CheckRawEncoding(value);
            _value = value;
            //_flags = PdfStringFlags.RawEncoding;
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfString"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="encoding">The encoding.</param>
        public PdfString(string value, PdfStringEncoding encoding)
        {
            switch (encoding)
            {
                case PdfStringEncoding.RawEncoding:
                    AssertRawEncoding(value);
                    break;

                case PdfStringEncoding.StandardEncoding:
                    break;

                case PdfStringEncoding.PDFDocEncoding:
                    break;

                case PdfStringEncoding.WinAnsiEncoding:
                    AssertRawEncoding(value);
                    break;

                case PdfStringEncoding.MacRomanEncoding:
                    break;

                case PdfStringEncoding.Unicode:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(encoding));
            }
            _value = value;
            //if ((flags & PdfStringFlags.EncodingMask) == 0)
            //  flags |= PdfStringFlags.PDFDocEncoding;
            _flags = (PdfStringFlags)encoding;
        }

        internal PdfString(string? value, PdfStringFlags flags)
        {
            _value = value ?? "";
            _flags = flags;
        }

        /// <summary>
        /// Gets the number of characters in this string.
        /// </summary>
        public int Length => _value?.Length ?? 0;

        /// <summary>
        /// Gets the encoding.
        /// </summary>
        public PdfStringEncoding Encoding => (PdfStringEncoding)(_flags & PdfStringFlags.EncodingMask);

        /// <summary>
        /// Gets a value indicating whether the string is a hexadecimal literal.
        /// </summary>
        public bool HexLiteral => (_flags & PdfStringFlags.HexLiteral) != 0;

        internal PdfStringFlags Flags => _flags;

        PdfStringFlags _flags;

        /// <summary>
        /// Gets the string value.
        /// </summary>
        public string Value => _value ?? "";

        string? _value;

        internal static byte[] ToRawBytes(string? value)
        {
            return value == null ? [] : PdfEncoders.RawEncoding.GetBytes(value);
        }

        internal static string FromRawBytes(byte[] value)
        {
            return PdfEncoders.RawEncoding.GetString(value, 0, value.Length);
        }

        internal static string FromRawBytes(byte[] value, ref PdfStringFlags flagsToUpdate, PdfStringEncoding? encoding = null)
        {
            if (encoding != null)
                ChangeEncoding(ref flagsToUpdate, encoding.Value);
            return FromRawBytes(value);
        }

        static void ChangeEncoding(ref PdfStringFlags flagsToUpdate, PdfStringEncoding encoding)
        {
            if (flagsToUpdate == (PdfStringFlags)encoding)
                return;

            flagsToUpdate = flagsToUpdate - (flagsToUpdate & PdfStringFlags.EncodingMask) + (PdfStringFlags)encoding;
        }

        internal byte[] GetRawBytes() => ToRawBytes(_value);

        internal void SetRawBytes(byte[] value, PdfStringEncoding? encoding = null)
        {
            _value = FromRawBytes(value, ref _flags, encoding);
        }

        /// <summary>
        /// Checks this PdfString for valid BOMs and rereads it with the specified Unicode encoding.
        /// </summary>
        internal bool TryRereadAsUnicode()
        {
            return TryRereadAsUnicode(ref _value, ref _flags);
        }

        /// <summary>
        /// Checks string for valid BOMs and rereads it with the specified Unicode encoding.
        /// The referenced PdfStringFlags are updated according to the encoding.
        /// </summary>
        internal static bool TryRereadAsUnicode(ref string? value, ref PdfStringFlags flagsToUpdate)
        {
            var result = TryRereadAsUnicode(ref value);

            if (result)
                ChangeEncoding(ref flagsToUpdate, PdfStringEncoding.Unicode);

            return result;
        }

        /// <summary>
        /// Checks string for valid BOMs and rereads it with the specified Unicode encoding.
        /// </summary>
        static bool TryRereadAsUnicode(ref string? value)
        {
            // UTF-16BE Unicode strings start with U+FEFF ("þÿ"). There can be empty strings with UTF-16BE prefix.
            // Old code:
            //   if (value.Length >= 2 && value[0] == '\xFE' && value[1] == '\xFF')
            //
            // Check for UTF-16BE encoding.
            // ---
            // Fun fact (Jan 2024): The following line of code was originally suggested by ReSharper, but the new
            // JetBrains AI Assistant considered it as illegal C# because of '..' in the array pattern.
            // ChatGPT 4 however explains the next line correctly.
            if (value is ['\xFE', '\xFF', ..])
            {
                // Combine two ANSI characters to get one Unicode character.
                var temp = new StringBuilder(value);
                var length = temp.Length;
                if ((length & 1) == 1)
                {
                    // What does the PDF Reference say about this case? Assume (char)0 or treat the file as corrupted?
                    // We add a (char)0.
                    PdfSharpLogHost.Logger.LogWarning("A PDF UTF-16 string contains an odd number of bytes. We add a dummy byte of 0.");
                    temp.Append(0);  // BUG_OLD: definitely wrong. Last character is multiplied with 256.
                    ++length;
                }

                var unicodeValue = new StringBuilder();
                for (var i = 2; i < length; i += 2)
                {
                    unicodeValue.Append((char)(256 * temp[i] + temp[i + 1]));
                }
                value = unicodeValue.ToString();
                return true;
            }

            // UTF-16LE Unicode strings start with U+FFE ("ÿþ").
            // Adobe Reader also supports UTF-16LE with BOM, so we do.
            if (value is ['\xFF', '\xFE', ..])
            {
                // Combine two ANSI characters to get one Unicode character.
                var temp = new StringBuilder(value);
                var length = temp.Length;
                if ((length & 1) == 1)
                {
                    // What does the PDF Reference say about this case? Assume (char)0 or treat the file as corrupted?
                    // We add a (char)0.
                    PdfSharpLogHost.Logger.LogWarning("A PDF UTF-16 string contains an odd number of bytes. We add a dummy byte of 0.");
                    temp.Append(0);  // See BE case. But this is correct here.
                    ++length;
                }

                var unicodeValue = new StringBuilder();
                for (var i = 2; i < length; i += 2)
                {
                    unicodeValue.Append((char)(256 * temp[i + 1] + temp[i]));
                }
                value = unicodeValue.ToString();
                return true;
            }

            // UTF-8 Unicode strings start with U+EFBBBF ("ï»¿").
            if (value is ['\xEF', '\xBB', '\xBF', ..])
            {
                // UTF8 is not implemented as Encoding for PdfStrings. After conversion, value holds the UTF-16 representation.
                // We return true, so it will be handled as UTF-16 from now.
                var rawBytes = ToRawBytes(value[3..]);
                value = System.Text.Encoding.UTF8.GetString(rawBytes);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the string.
        /// </summary>
        public override string ToString()
        {
#if true
            var encoding = (PdfStringEncoding)(_flags & PdfStringFlags.EncodingMask);
            string pdf = (_flags & PdfStringFlags.HexLiteral) == 0 ?
                PdfEncoders.ToStringLiteral(Value, encoding, null) :
                PdfEncoders.ToHexStringLiteral(Value, encoding, null);
            return pdf;
#else
            return _value;
#endif
        }

#if true_ 
        /// <summary>
        /// H/ack for document encoded bookmarks.
        /// </summary>
        public string ToStringFromPdfDocEncoded()
        {
            if (_value == null)
                return "";
            int length = _value.Length;
            char[] bytes = new char[length];
            for (int idx = 0; idx < length; idx++)
            {
                char ch = _value[idx];
                if (ch <= 255)
                {
                    bytes[idx] = Encode[ch];
                }
                else
                {
                    //Debug-Break.Break();
                    throw new InvalidOperationException("DocEncoded string contains char greater 255.");
                }
            }
            var sb = new StringBuilder(length);
            for (int idx = 0; idx < length; idx++)
                sb.Append(bytes[idx]);
            return sb.ToString();
        }
#endif

        static readonly char[] Encode =
        [
            '\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07', '\x08', '\x09', '\x0A', '\x0B', '\x0C', '\x0D', '\x0E', '\x0F',
            '\x10', '\x11', '\x12', '\x13', '\x14', '\x15', '\x16', '\x17', '\x18', '\x19', '\x1A', '\x1B', '\x1C', '\x1D', '\x1E', '\x1F',
            '\x20', '\x21', '\x22', '\x23', '\x24', '\x25', '\x26', '\x27', '\x28', '\x29', '\x2A', '\x2B', '\x2C', '\x2D', '\x2E', '\x2F',
            '\x30', '\x31', '\x32', '\x33', '\x34', '\x35', '\x36', '\x37', '\x38', '\x39', '\x3A', '\x3B', '\x3C', '\x3D', '\x3E', '\x3F',
            '\x40', '\x41', '\x42', '\x43', '\x44', '\x45', '\x46', '\x47', '\x48', '\x49', '\x4A', '\x4B', '\x4C', '\x4D', '\x4E', '\x4F',
            '\x50', '\x51', '\x52', '\x53', '\x54', '\x55', '\x56', '\x57', '\x58', '\x59', '\x5A', '\x5B', '\x5C', '\x5D', '\x5E', '\x5F',
            '\x60', '\x61', '\x62', '\x63', '\x64', '\x65', '\x66', '\x67', '\x68', '\x69', '\x6A', '\x6B', '\x6C', '\x6D', '\x6E', '\x6F',
            '\x70', '\x71', '\x72', '\x73', '\x74', '\x75', '\x76', '\x77', '\x78', '\x79', '\x7A', '\x7B', '\x7C', '\x7D', '\x7E', '\x7F',
            '\x2022', '\x2020', '\x2021', '\x2026', '\x2014', '\x2013', '\x0192', '\x2044', '\x2039', '\x203A', '\x2212', '\x2030', '\x201E', '\x201C', '\x201D', '\x2018',
            '\x2019', '\x201A', '\x2122', '\xFB01', '\xFB02', '\x0141', '\x0152', '\x0160', '\x0178', '\x017D', '\x0131', '\x0142', '\x0153', '\x0161', '\x017E', '\xFFFD',
            '\x20AC', '\xA1', '\xA2', '\xA3', '\xA4', '\xA5', '\xA6', '\xA7', '\xA8', '\xA9', '\xAA', '\xAB', '\xAC', '\xAD', '\xAE', '\xAF',
            '\xB0', '\xB1', '\xB2', '\xB3', '\xB4', '\xB5', '\xB6', '\xB7', '\xB8', '\xB9', '\xBA', '\xBB', '\xBC', '\xBD', '\xBE', '\xBF',
            '\xC0', '\xC1', '\xC2', '\xC3', '\xC4', '\xC5', '\xC6', '\xC7', '\xC8', '\xC9', '\xCA', '\xCB', '\xCC', '\xCD', '\xCE', '\xCF',
            '\xD0', '\xD1', '\xD2', '\xD3', '\xD4', '\xD5', '\xD6', '\xD7', '\xD8', '\xD9', '\xDA', '\xDB', '\xDC', '\xDD', '\xDE', '\xDF',
            '\xE0', '\xE1', '\xE2', '\xE3', '\xE4', '\xE5', '\xE6', '\xE7', '\xE8', '\xE9', '\xEA', '\xEB', '\xEC', '\xED', '\xEE', '\xEF',
            '\xF0', '\xF1', '\xF2', '\xF3', '\xF4', '\xF5', '\xF6', '\xF7', '\xF8', '\xF9', '\xFA', '\xFB', '\xFC', '\xFD', '\xFE', '\xFF'
        ];

        [Conditional("DEBUG")]
        static void AssertRawEncoding(string s)
        {
            if (String.IsNullOrEmpty(s))
                return;

            int length = s.Length;
            for (int idx = 0; idx < length; idx++)
            {
                Debug.Assert(s[idx] < 256, "RawString contains invalid character.");
            }
        }

        static bool IsRawEncoding(string s)
        {
            if (String.IsNullOrEmpty(s))
                return true;

            int length = s.Length;
            for (int idx = 0; idx < length; idx++)
            {
                if (!(s[idx] < 256))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Writes the string DocEncoded.
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
        {
            writer.Write(this);
        }
    }
}
