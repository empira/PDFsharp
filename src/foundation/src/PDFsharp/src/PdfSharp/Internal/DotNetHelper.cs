// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if !NET6_0_OR_GREATER

using System.Numerics;

namespace PdfSharp.Internal
{
    /// <summary>
    /// Class containing replacements for net 6 methods missing in net framework 4.7.2.
    /// </summary>
    static class DotNetHelper
    {
        /// <summary>
        /// Implements the net 6 BigInteger constructor missing in net framework 4.7.2.
        /// Initializes a new instance of the BigInteger structure using the values in a read-only span of bytes, and optionally indicating the signing encoding and the endianness byte order.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isUnsigned"></param>
        /// <param name="isBigEndian"></param>
        /// <returns></returns>
        public static BigInteger CreateBigInteger(ReadOnlySpan<byte> value, bool isUnsigned = false, bool isBigEndian = false)
        {
            var bytes = value.ToArray();

            // Convert to little endian, which is expected by BigInteger constructor.
            if (isBigEndian)
                bytes = bytes.Reverse().ToArray();

            // A leading bit of 1 defines a negative number. If the input should be interpreted as unsigned, prepend a new zero byte, if there's a leading 1.
            // As bytes is in little endian order, check the most significant bit of the last byte. If it is 1, append the zero byte.
            if (isUnsigned && bytes.Length > 0 && (bytes.Last() & 0x80) > 0)
                bytes = bytes.Append((byte)0).ToArray();

            return new BigInteger(bytes);
        }
    }
}

#endif
