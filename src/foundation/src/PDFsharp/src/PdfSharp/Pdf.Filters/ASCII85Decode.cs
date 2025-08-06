// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Filters
{
    /// <summary>
    /// Implements the ASCII85Decode filter.
    /// </summary>
    public class Ascii85Decode : Filter
    {
        // Reference 1.7: 3.3.2  ASCII85Decode Filter / Page 69
        // Reference 2.0: 7.4.3  ASCII85Decode filter / Page 37
        // Wikipedia https://en.wikipedia.org/wiki/Ascii85
        // Padding the input data to a multiple of 4 bytes for encoding or
        // 5 bytes for decoding respectively is well explained in the Wikipedia article.
        // It may not be intuitively utterly clear why it works.

        // Rewritten March 2025.

        /// <summary>
        /// Encodes the specified data.
        /// </summary>
        public override byte[] Encode(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Number of bytes in source data.
            int sourceLength = data.Length;  // length == 0 is not treated as a special case.

            // Number of 4 byte groups.
            int words = sourceLength / 4;

            // Number of bytes modulo 4 (final padding group, if any).
            int rest = sourceLength - 4 * words;

            // Number of four-byte groups including an optional padding group.
            int wordsPadded = words + (rest == 0 ? 0 : 1);

            // If rest != 0, the last block is padded and must not
            // be encoded as 'z' in case it is 0.
            int lastBlock = rest == 0 ? wordsPadded + 1 : wordsPadded - 1;

            // Allocate source bytes including padding.
            byte[] source = new byte[4 * wordsPadded];
            Array.Copy(data, source, sourceLength);  // 0..3 trailing zeros.

            // Max result length including suffix.
            int resultLength = wordsPadded * 5 + 2;

            // Allocate result bytes.
            byte[] result = new byte[resultLength];

            int idxSrc = 0, idxRes = 0;
            int wordCount = 0;
            while (wordCount < wordsPadded)
            {
                uint value = ((uint)source[idxSrc++] << 24)
                             + ((uint)source[idxSrc++] << 16)
                             + ((uint)source[idxSrc++] << 8)
                             + source[idxSrc++];
                if (value == 0 && wordCount != lastBlock)
                {
                    // Encode 0 as 'z' instead of '!!!!!'
                    result[idxRes++] = (byte)'z';
                }
                else
                {
                    // Convert from radix-256 to radix-85.
                    byte ch5 = (byte)(value % 85 + '!');
                    value /= 85;
                    byte ch4 = (byte)(value % 85 + '!');
                    value /= 85;
                    byte ch3 = (byte)(value % 85 + '!');
                    value /= 85;
                    byte ch2 = (byte)(value % 85 + '!');
                    value /= 85;
                    byte ch1 = (byte)(value + '!');

                    result[idxRes++] = ch1;
                    result[idxRes++] = ch2;
                    result[idxRes++] = ch3;
                    result[idxRes++] = ch4;
                    result[idxRes++] = ch5;
                }
                wordCount++;
            }

            // Chop result if rest is not 0.
            int effectiveResultLength = idxRes - (rest != 0 ? 4 - rest : 0) + 2;
            if (effectiveResultLength < resultLength)
                Array.Resize(ref result, effectiveResultLength);

            // Set suffix.
            result[^2] = (byte)'~';
            result[^1] = (byte)'>';

            return result;
        }

        /// <summary>
        /// Decodes the specified data.
        /// </summary>
        public override byte[] Decode(byte[] data, FilterParms? _)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Number of bytes in source data.
            int sourceLength = data.Length;

            // Allocate max. source bytes including up to 3 'u' digits for padding.
            byte[] source = new byte[sourceLength + 3];

            // Number of z characters.
            int zCount = 0;

            // Digit index (0..4) in the 5 digit of the radix-85 number.
            int digitIndex = 0;

            // Flag indicating '~' as first character of suffix was parsed.
            // A PDFsharp user found a PDF file where '~>' was separated by
            // a LF. So take that into account.
            bool tilde = false;

            // Analyse, validate, and clean up input data.
            int idx;
            int idxSrc = 0;
            for (idx = 0; idx < sourceLength; idx++)
            {
                char ch = (char)data[idx];

                // According to Wikipedia the string can start with '<~'.
                // PDF specs does not mention this case.
                if (idx == 0 && sourceLength >= 2)
                {
                    // Here we do not expect any white-space before or between
                    // '<' and '~'.
                    if (ch == '<' && (char)data[1] == '~')
                    {
                        idx++;
                        continue;
                    }
                }

                // According to the specs skip white-space.
                if (Lexer.IsWhiteSpace(ch))
                    continue;

                // Check for start of postfix.
                if (tilde)
                {
                    if (ch != '>')
                        throw new ArgumentException($"'~' followed by illegal character '{ch}'.", nameof(data));

                    // Ensure that idx never gets the value of length.
                    // Also ignore any characters beyond suffix.
                    break;
                }

                if (ch is >= '!' and <= 'u')
                {
                    source[idxSrc++] = (byte)ch;
                    if (++digitIndex == 5)
                        digitIndex = 0;
                }
                else if (ch == 'z')
                {
                    if (digitIndex != 0)
                        throw new ArgumentException($"A 'z' appears illegally within a 5 digit radix-85 number.", nameof(data));
                    source[idxSrc++] = (byte)ch;
                    zCount++;
                }
                else if (ch == '~')
                {
                    // We cannot expect that the next character is a '>'.
                    // It can be some white-space.
                    tilde = true;
                }
                else
                {
                    // Ignore unknown character, but log an error.
                    PdfSharpLogHost.PdfReadingLogger.LogError("Illegal char in ASCII85 string: '{ch}'.", ch);
                }
            }

            // Loop not ended with break?
            if (idx == sourceLength)
                throw new ArgumentException("Illegal character.", nameof(data));

            // Effective source length cleaned up by prefix, suffix, and white-space.
            sourceLength = idxSrc;

            // Number of radix-85 digits.
            int nonZero = sourceLength - zCount;

            // Number of decoded bytes in case no padding was needed.
            // Full 4 byte blocks.
            int resultLength = 4 * (zCount + nonZero / 5);

            // Can be 2, 3, or 4 single radix-85 digits.
            int rest = nonZero % 5;

            // The rest cannot be 1 as a result of the padding method.
            if (rest == 1)
                throw new ArgumentException("The ASCII-85 string has an illegal number of padding characters.", nameof(data));

            // Number of 'u' digits to be padded for decoding (1, 2, or 3).
            int padding = 0;
            // It is not intuitively completely clear to me why the padding method works this way, but it does.
            if (rest != 0)
            {
                padding = 5 - rest;  // 1, 2, or 3
                resultLength += rest - 1;

                // Append 'u' digits to make length a multiple of 5.
                for (idx = 0; idx < padding; idx++)
                    source[sourceLength++] = (byte)'u';
            }

            Debug.Assert((resultLength + padding) % 4 == 0);

            // Allocate result bytes with padding.
            byte[] result = new byte[resultLength + padding];

            int idxRes = 0;
            idx = 0;
            while (idx + 5 <= sourceLength)
            {
                char ch = (char)source[idx];
                if (ch == 'z')
                {
                    // Add 4 zero bytes.
                    idx++;
                    idxRes += 4;
                }
                else
                {
                    // We already ensured above that there is no 'z' and no white-space in the next 5 radix-85 digits.
                    // Using '!!!!!' instead of 'z' is not treated as an error.
                    var value =
                      (long)(source[idx++] - '!') * (85 * 85 * 85 * 85) +  // Interesting: Without parentheses the compiler 
                      (uint)(source[idx++] - '!') * (85 * 85 * 85) +       // does not treat the powers of 85 as a constant
                      (uint)(source[idx++] - '!') * (85 * 85) +            // expression, but multiplies all factors 
                      (uint)(source[idx++] - '!') * 85 +                   // from left to right.
                      (uint)(source[idx++] - '!');

                    // Because 85^5 > 256^4 there can be an overflow in a five digit radix-85 number.
                    if (value > UInt32.MaxValue)
                    {
                        // Note that padding cannot lead to an overflow.
                        // The largest possible value created during padding is 4278608874 / 0xff0663ea
                        throw new InvalidOperationException("The value of a 5 digit radix-85 number is greater than 2^32 - 1.");
                    }

                    result[idxRes++] = (byte)(value >> 24);
                    result[idxRes++] = (byte)(value >> 16);
                    result[idxRes++] = (byte)(value >> 8);
                    result[idxRes++] = (byte)value;
                }
            }

            // Chop the padding bytes if rest is not 0.
            if (rest != 0)
                Array.Resize(ref result, resultLength);

            return result;
        }
    }
}
