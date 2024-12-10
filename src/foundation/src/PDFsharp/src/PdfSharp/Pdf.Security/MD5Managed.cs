// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// Based on code from here (not available anymore):
// http://archive.msdn.microsoft.com/SilverlightMD5/Release/ProjectReleases.aspx?ReleaseId=2206
// Today (2024) we can still find the original source code, e.g. here:
// https://github.com/microsoft/surveillance-camera/blob/master/ServerApi/SurveillanceCamera/SurveillanceCamera/MD5.cs
//
// **************************************************************
// * Raw implementation of the MD5 hash algorithm
// * from RFC 1321.
// *
// * Written By: Reid Borsuk and Jenny Zheng
// * Copyright (c) Microsoft Corporation.  All rights reserved.
// **************************************************************
// 
// We originally used this code for PDFsharp for Silverlight. The original code has a bug for
// buffers with a length that was a multiple of 64-8. One of the original authors fixed this
// quickly and this code works fine for all buffer sizes.
// However, the code of PDFsharp 1.5 still contains the bug. WTF.
//
// We reintroduced this code in PDFsharp 6.2 because today the .NET MD5 implementation issues
// a security warning and PDFsharp does not work for users that must use code that is FIPS
// compliant. We also need the code for PDFsharp running on Blazor and other platforms
// where the .NET implementation lacks of the retired MD5 class.

using System.Security.Cryptography;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace PdfSharp.Pdf.Security
{
    /// <summary>
    /// A managed implementation of the MD5 algorithm.
    /// Necessary because MD5 is not part of frameworks like Blazor.
    /// We also need it to make PDFsharp FIPS compliant.
    /// </summary>
    class MD5Managed : HashAlgorithm
    {
        // Constants as defined in RFC 1321.
        const uint A = 0x67452301;
        const uint B = 0xefcdab89;
        const uint C = 0x98badcfe;
        const uint D = 0x10325476;

        MD5Managed() { }

        public new static MD5Managed Create() => new();

        public sealed override void Initialize()
        {
            _data = new byte[64];
            _dataSize = 0;
            _totalLength = 0;
            _abcd = new()
            {
                // Initialize values as defined in RFC 1321.
                // Note: The code below may look strange but is correct.
                A = A,
                B = B,
                C = C,
                D = D
            };
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            int startIndex = ibStart;
            int totalArrayLength = _dataSize + cbSize;
            if (totalArrayLength >= 64)
            {
                Array.Copy(array, startIndex, _data, _dataSize, 64 - _dataSize);
                // Process message of 64 bytes (512 bits)
                MD5Core.GetHashBlock(_data, ref _abcd, 0);
                startIndex += 64 - _dataSize;
                totalArrayLength -= 64;
                while (totalArrayLength >= 64)
                {
                    Array.Copy(array, startIndex, _data, 0, 64);
                    MD5Core.GetHashBlock(array, ref _abcd, startIndex);
                    totalArrayLength -= 64;
                    startIndex += 64;
                }

                _dataSize = totalArrayLength;
                Array.Copy(array, startIndex, _data, 0, totalArrayLength);
            }
            else
            {
                Array.Copy(array, startIndex, _data, _dataSize, cbSize);
                _dataSize = totalArrayLength;
            }
            _totalLength += cbSize;
        }

        protected override byte[] HashFinal()
        {
            HashValue = MD5Core.GetHashFinalBlock(_data, 0, _dataSize, _abcd, _totalLength * 8);
            return HashValue;
        }

        byte[] _data = new byte[64];
        MD5Core.ABCD _abcd = new()
        {
            // Initialize values as defined in RFC 1321.
            // Note: The code below may look strange but is correct.
            A = A,
            B = B,
            C = C,
            D = D
        };
        long _totalLength;
        int _dataSize;

        /// <summary>
        /// The actual MD5 algorithm implementation.
        /// </summary>
        static class MD5Core
        {
#if false  // Not used in PDFsharp. KEEP code for reference, but do not include it.
            public static byte[] GetHash(byte[] input)
            {
                if (input == null)
                    throw new ArgumentNullException(nameof(input));

                // Initialize values defined in RFC 1321.
                // Note: The code below may look strange but is correct.
                var abcd = new ABCD
                {
                    A = A,
                    B = B,
                    C = C,
                    D = D
                };

                // We pass in the input array by block, the final block of data must be handled specially for padding & length embedding.
                int startIndex = 0;
                while (startIndex <= input.Length - 64)
                {
                    GetHashBlock(input, ref abcd, startIndex);
                    startIndex += 64;
                }

                // The final data block. 
                return GetHashFinalBlock(input, startIndex, input.Length - startIndex, abcd, (long)input.Length * 8);
            }
#endif

            internal static byte[] GetHashFinalBlock(byte[] input, int ibStart, int cbSize, ABCD abcd, long len)
            {
                byte[] working = new byte[64];
                byte[] length = BitConverter.GetBytes(len);

                // Padding is a single bit 1, followed by the number of 0s required to make size congruent to 448 modulo 512. Step 1 of RFC 1321  
                // The CLR ensures that our buffer is 0-assigned, we don't need to explicitly set it. This is why it ends up being quicker to just
                // use a temporary array rather than doing in-place assignment (5% for small inputs).
                Array.Copy(input, ibStart, working, 0, cbSize);
                working[cbSize] = 0x80;

#if true  // StL: Isn’t this what the code below means?
                // Do we need an additional chunk to store the length?
                if (cbSize >= 56)
                {
                    GetHashBlock(working, ref abcd, 0);
                    // Create an entirely new chunk due to the 0-assigned trick mentioned above, to avoid an extra function call clearing the array.
                    working = new byte[64];
                }
                Array.Copy(length, 0, working, 56, 8);
                GetHashBlock(working, ref abcd, 0);
#else
                // We have enough room to store the length in this chunk.
                if (cbSize < 56)
                {
                    Array.Copy(length, 0, working, 56, 8);
                    GetHashBlock(working, ref abcd, 0);
                }
                else // We need an additional chunk to store the length.
                {
                    GetHashBlock(working, ref abcd, 0);
                    // Create an entirely new chunk due to the 0-assigned trick mentioned above, to avoid an extra function call clearing the array.
                    working = new byte[64];
                    Array.Copy(length, 0, working, 56, 8);
                    GetHashBlock(working, ref abcd, 0);
                }
#endif

                byte[] output = new byte[16];
                Array.Copy(BitConverter.GetBytes(abcd.A), 0, output, 0, 4);
                Array.Copy(BitConverter.GetBytes(abcd.B), 0, output, 4, 4);
                Array.Copy(BitConverter.GetBytes(abcd.C), 0, output, 8, 4);
                Array.Copy(BitConverter.GetBytes(abcd.D), 0, output, 12, 4);
                return output;
            }

            internal static void GetHashBlock(byte[] input, ref ABCD abcd, int ibStart)
            {
                uint[] temp = Converter(input, ibStart);
                uint a = abcd.A;
                uint b = abcd.B;
                uint c = abcd.C;
                uint d = abcd.D;

                a = r1(a, b, c, d, temp[0], 7, 0xd76aa478);
                d = r1(d, a, b, c, temp[1], 12, 0xe8c7b756);
                c = r1(c, d, a, b, temp[2], 17, 0x242070db);
                b = r1(b, c, d, a, temp[3], 22, 0xc1bdceee);
                a = r1(a, b, c, d, temp[4], 7, 0xf57c0faf);
                d = r1(d, a, b, c, temp[5], 12, 0x4787c62a);
                c = r1(c, d, a, b, temp[6], 17, 0xa8304613);
                b = r1(b, c, d, a, temp[7], 22, 0xfd469501);
                a = r1(a, b, c, d, temp[8], 7, 0x698098d8);
                d = r1(d, a, b, c, temp[9], 12, 0x8b44f7af);
                c = r1(c, d, a, b, temp[10], 17, 0xffff5bb1);
                b = r1(b, c, d, a, temp[11], 22, 0x895cd7be);
                a = r1(a, b, c, d, temp[12], 7, 0x6b901122);
                d = r1(d, a, b, c, temp[13], 12, 0xfd987193);
                c = r1(c, d, a, b, temp[14], 17, 0xa679438e);
                b = r1(b, c, d, a, temp[15], 22, 0x49b40821);

                a = r2(a, b, c, d, temp[1], 5, 0xf61e2562);
                d = r2(d, a, b, c, temp[6], 9, 0xc040b340);
                c = r2(c, d, a, b, temp[11], 14, 0x265e5a51);
                b = r2(b, c, d, a, temp[0], 20, 0xe9b6c7aa);
                a = r2(a, b, c, d, temp[5], 5, 0xd62f105d);
                d = r2(d, a, b, c, temp[10], 9, 0x02441453);
                c = r2(c, d, a, b, temp[15], 14, 0xd8a1e681);
                b = r2(b, c, d, a, temp[4], 20, 0xe7d3fbc8);
                a = r2(a, b, c, d, temp[9], 5, 0x21e1cde6);
                d = r2(d, a, b, c, temp[14], 9, 0xc33707d6);
                c = r2(c, d, a, b, temp[3], 14, 0xf4d50d87);
                b = r2(b, c, d, a, temp[8], 20, 0x455a14ed);
                a = r2(a, b, c, d, temp[13], 5, 0xa9e3e905);
                d = r2(d, a, b, c, temp[2], 9, 0xfcefa3f8);
                c = r2(c, d, a, b, temp[7], 14, 0x676f02d9);
                b = r2(b, c, d, a, temp[12], 20, 0x8d2a4c8a);

                a = r3(a, b, c, d, temp[5], 4, 0xfffa3942);
                d = r3(d, a, b, c, temp[8], 11, 0x8771f681);
                c = r3(c, d, a, b, temp[11], 16, 0x6d9d6122);
                b = r3(b, c, d, a, temp[14], 23, 0xfde5380c);
                a = r3(a, b, c, d, temp[1], 4, 0xa4beea44);
                d = r3(d, a, b, c, temp[4], 11, 0x4bdecfa9);
                c = r3(c, d, a, b, temp[7], 16, 0xf6bb4b60);
                b = r3(b, c, d, a, temp[10], 23, 0xbebfbc70);
                a = r3(a, b, c, d, temp[13], 4, 0x289b7ec6);
                d = r3(d, a, b, c, temp[0], 11, 0xeaa127fa);
                c = r3(c, d, a, b, temp[3], 16, 0xd4ef3085);
                b = r3(b, c, d, a, temp[6], 23, 0x04881d05);
                a = r3(a, b, c, d, temp[9], 4, 0xd9d4d039);
                d = r3(d, a, b, c, temp[12], 11, 0xe6db99e5);
                c = r3(c, d, a, b, temp[15], 16, 0x1fa27cf8);
                b = r3(b, c, d, a, temp[2], 23, 0xc4ac5665);

                a = r4(a, b, c, d, temp[0], 6, 0xf4292244);
                d = r4(d, a, b, c, temp[7], 10, 0x432aff97);
                c = r4(c, d, a, b, temp[14], 15, 0xab9423a7);
                b = r4(b, c, d, a, temp[5], 21, 0xfc93a039);
                a = r4(a, b, c, d, temp[12], 6, 0x655b59c3);
                d = r4(d, a, b, c, temp[3], 10, 0x8f0ccc92);
                c = r4(c, d, a, b, temp[10], 15, 0xffeff47d);
                b = r4(b, c, d, a, temp[1], 21, 0x85845dd1);
                a = r4(a, b, c, d, temp[8], 6, 0x6fa87e4f);
                d = r4(d, a, b, c, temp[15], 10, 0xfe2ce6e0);
                c = r4(c, d, a, b, temp[6], 15, 0xa3014314);
                b = r4(b, c, d, a, temp[13], 21, 0x4e0811a1);
                a = r4(a, b, c, d, temp[4], 6, 0xf7537e82);
                d = r4(d, a, b, c, temp[11], 10, 0xbd3af235);
                c = r4(c, d, a, b, temp[2], 15, 0x2ad7d2bb);
                b = r4(b, c, d, a, temp[9], 21, 0xeb86d391);

                abcd.A = unchecked(a + abcd.A);
                abcd.B = unchecked(b + abcd.B);
                abcd.C = unchecked(c + abcd.C);
                abcd.D = unchecked(d + abcd.D);
            }

            // Manually unrolling these equations gets us a 20% performance improvement
            static uint r1(uint a, uint b, uint c, uint d, uint x, int s, uint t)
            {
                //                   (b + LSR((a + F(b, c, d) + x + t), s))
                // F(x, y, z)        ((x & y) | ((x ^ 0xFFFFFFFF) & z))
                return unchecked(b + LSR((a + ((b & c) | ((b ^ 0xFFFFFFFF) & d)) + x + t), s));
            }

            static uint r2(uint a, uint b, uint c, uint d, uint x, int s, uint t)
            {
                //                   (b + LSR((a + G(b, c, d) + x + t), s))
                // G(x, y, z)        ((x & z) | (y & (z ^ 0xFFFFFFFF)))
                return unchecked(b + LSR((a + ((b & d) | (c & (d ^ 0xFFFFFFFF))) + x + t), s));
            }

            static uint r3(uint a, uint b, uint c, uint d, uint x, int s, uint t)
            {
                //                   (b + LSR((a + H(b, c, d) + k + i), s))
                // H(x, y, z)        (x ^ y ^ z)
                return unchecked(b + LSR((a + (b ^ c ^ d) + x + t), s));
            }

            static uint r4(uint a, uint b, uint c, uint d, uint x, int s, uint t)
            {
                //                   (b + LSR((a + I(b, c, d) + k + i), s))
                // I(x, y, z)        (y ^ (x | (z ^ 0xFFFFFFFF)))
                return unchecked(b + LSR((a + (c ^ (b | (d ^ 0xFFFFFFFF))) + x + t), s));
            }

            // Implementation of left rotate.
            // s is an int instead of an uint because the CLR requires the argument passed to ‘>>’ / ‘<<’ is of 
            // type int. Doing the demoting inside this function would add overhead.
            static uint LSR(uint i, int s) => (i << s) | (i >> (32 - s));

            // Convert input array into array of uint values.
            static uint[] Converter(byte[] input, int ibStart)
            {
                if (null == input)
                    throw new ArgumentNullException(nameof(input));

                uint[] result = new uint[16];
                for (int idx = 0; idx < 16; idx++)
                {
                    result[idx] = input[ibStart + idx * 4];
                    result[idx] += (uint)input[ibStart + idx * 4 + 1] << 8;
                    result[idx] += (uint)input[ibStart + idx * 4 + 2] << 16;
                    result[idx] += (uint)input[ibStart + idx * 4 + 3] << 24;

                    // StL: Just repeats the code above. Make this sense for an assertion?
                    Debug.Assert(result[idx] ==
                                 input[ibStart + idx * 4] +
                                 ((uint)input[ibStart + idx * 4 + 1] << 8) +
                                 ((uint)input[ibStart + idx * 4 + 2] << 16) +
                                 ((uint)input[ibStart + idx * 4 + 3] << 24));
                }
                return result;
            }

            // Simple struct for the (a,b,c,d) which is used to compute the message digest.    
            // ReSharper disable once IdentifierTypo
            public struct ABCD
            {
                // ReSharper disable MemberHidesStaticFromOuterClass
                public uint A;
                public uint B;
                public uint C;
                public uint D;
                // ReSharper restore MemberHidesStaticFromOuterClass
            }
        }
    }
}
