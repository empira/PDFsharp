// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;

namespace PdfSharp.Pdf.Internal
{
    /// <summary>
    /// An encoder for raw strings. The raw encoding is simply the identity relation between
    /// characters and bytes. PDFsharp internally works with raw encoded strings instead of
    /// byte arrays because strings are much more handy than byte arrays.
    /// </summary>
    /// <remarks>
    /// Raw encoded strings represent an array of bytes. Therefore a character greater than
    /// 255 is not valid in a raw encoded string.
    /// </remarks>
    public sealed class RawEncoding : Encoding
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RawEncoding"/> class.
        /// </summary>
        // ReSharper disable EmptyConstructor
        public RawEncoding()
        { }
        // ReSharper restore EmptyConstructor

        /// <summary>
        /// When overridden in a derived class, calculates the number of bytes produced by encoding a set of characters from the specified character array.
        /// </summary>
        /// <param name="chars">The character array containing the set of characters to encode.</param>
        /// <param name="index">The index of the first character to encode.</param>
        /// <param name="count">The number of characters to encode.</param>
        /// <returns>
        /// The number of bytes produced by encoding the specified characters.
        /// </returns>
        public override int GetByteCount(char[] chars, int index, int count)
        {
            return count;
        }

        /// <summary>
        /// When overridden in a derived class, encodes a set of characters from the specified character array into the specified byte array.
        /// </summary>
        /// <param name="chars">The character array containing the set of characters to encode.</param>
        /// <param name="charIndex">The index of the first character to encode.</param>
        /// <param name="charCount">The number of characters to encode.</param>
        /// <param name="bytes">The byte array to contain the resulting sequence of bytes.</param>
        /// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes.</param>
        /// <returns>
        /// The actual number of bytes written into <paramref name="bytes"/>.
        /// </returns>
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            for (int count = charCount; count > 0; charIndex++, byteIndex++, count--)
            {
#if DEBUG_
                if ((uint) chars[charIndex] > 255)
                    Debug-Break.Break(true);
#endif
                //Debug.Assert((uint)chars[charIndex] < 256, "Raw string contains invalid character with a value > 255.");
                bytes[byteIndex] = (byte)chars[charIndex];
                //#warning Here is a HA/CK that must not be ignored!
                // HA/CK: 
                // bytes[byteIndex] = (byte)chars[charIndex];
            }
            return charCount;
        }

        /// <summary>
        /// When overridden in a derived class, calculates the number of characters produced by decoding a sequence of bytes from the specified byte array.
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
        /// <param name="index">The index of the first byte to decode.</param>
        /// <param name="count">The number of bytes to decode.</param>
        /// <returns>
        /// The number of characters produced by decoding the specified sequence of bytes.
        /// </returns>
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return count;
        }

        /// <summary>
        /// When overridden in a derived class, decodes a sequence of bytes from the specified byte array into the specified character array.
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
        /// <param name="byteIndex">The index of the first byte to decode.</param>
        /// <param name="byteCount">The number of bytes to decode.</param>
        /// <param name="chars">The character array to contain the resulting set of characters.</param>
        /// <param name="charIndex">The index at which to start writing the resulting set of characters.</param>
        /// <returns>
        /// The actual number of characters written into <paramref name="chars"/>.
        /// </returns>
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            for (int count = byteCount; count > 0; byteIndex++, charIndex++, count--)
                chars[charIndex] = (char)bytes[byteIndex];
            return byteCount;
        }

        /// <summary>
        /// When overridden in a derived class, calculates the maximum number of bytes produced by encoding the specified number of characters.
        /// </summary>
        /// <param name="charCount">The number of characters to encode.</param>
        /// <returns>
        /// The maximum number of bytes produced by encoding the specified number of characters.
        /// </returns>
        public override int GetMaxByteCount(int charCount)
        {
            return charCount;
        }

        /// <summary>
        /// When overridden in a derived class, calculates the maximum number of characters produced by decoding the specified number of bytes.
        /// </summary>
        /// <param name="byteCount">The number of bytes to decode.</param>
        /// <returns>
        /// The maximum number of characters produced by decoding the specified number of bytes.
        /// </returns>
        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }
    }
}
