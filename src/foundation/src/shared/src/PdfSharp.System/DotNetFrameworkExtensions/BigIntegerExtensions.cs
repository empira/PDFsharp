// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if !NET8_0_OR_GREATER

using System.Numerics;

namespace PdfSharp.DotNetFrameworkExtensions
{
    /// <summary>
    /// PDFsharp extensions for .NET types.
    /// </summary>
    public static class BigIntegerExtensions
    {
        extension(BigInteger _)
        {
            /// <summary>
            /// Implements the .NET 6 BigInteger constructor missing in .NET framework 4.6.2.
            /// Initializes a new instance of the BigInteger structure using the values in a read-only span of bytes, and optionally indicating the signing encoding and the endianness byte order.
            /// </summary>
            /// <param name="value"></param>
            /// <param name="isUnsigned"></param>
            /// <param name="isBigEndian"></param>
            public static BigInteger CreateBigInteger(ReadOnlySpan<byte> value, bool isUnsigned = false, bool isBigEndian = false)
            {
                var bytes = value.ToArray();

                // Convert to little endian, which is expected by BigInteger constructor.
                if (isBigEndian)
                {
                    //bytes = bytes.Reverse().ToArray();
                    // In .NET 10 is for 'bytes.Reverse()' 'MemoryExtensions.Reverse(this Span<T>)' resolved
                    // instead of 'Enumerable.Reverse(this IEnumerable<T>)'.
                    // See  https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/breaking-changes/compiler%20breaking%20changes%20-%20dotnet%2010#enumerablereverse
                    // ReSharper disable once InvokeAsExtensionMethod because we must call the extension method explicitly in .NET 10.
                    bytes = Enumerable.Reverse(bytes).ToArray();
                }

                // A leading bit of 1 defines a negative number. If the input should be interpreted as unsigned, prepend a new zero byte, if there’s a leading 1.
                // As bytes is in little endian order, check the most significant bit of the last byte. If it is 1, append the zero byte.
                if (isUnsigned && bytes.Length > 0 && (bytes.Last() & 0x80) != 0)
                {
#if NET462
                    var len = bytes.Length;
                    var bytes2 = new byte[len + 1];
                    bytes.CopyTo(bytes2, 0);
                    bytes2[len] = 0;
                    bytes = bytes2;
#else
                    bytes = bytes.Append((byte)0).ToArray();
#endif
                }
                return new BigInteger(bytes);
            }
        }
    }
}
#endif
