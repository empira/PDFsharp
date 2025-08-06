﻿// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// BigGustave is distributed with PDFsharp, but was published under a different license.
// See file LICENSE in the folder containing this file.

namespace PdfSharp.Internal.Png.BigGustave
{
    using System.Collections.Generic;

    /// <summary>
    /// Used to calculate the Adler-32 checksum used for ZLIB data in accordance with 
    /// RFC 1950: ZLIB Compressed Data Format Specification.
    /// </summary>
    public static class Adler32Checksum
    {
        // Both sums (s1 and s2) are done modulo 65521.
        const int AdlerModulus = 65521;

        /// <summary>
        /// Calculate the Adler-32 checksum for some data.
        /// </summary>
        public static int Calculate(IEnumerable<byte> data, int length = -1)
        {
            // s1 is the sum of all bytes.
            var s1 = 1;

            // s2 is the sum of all s1 values.
            var s2 = 0;

            var count = 0;
            foreach (var b in data)
            {
                if (length > 0 && count == length)
                {
                    break;
                }

                s1 = (s1 + b) % AdlerModulus;
                s2 = (s1 + s2) % AdlerModulus;
                count++;
            }

            // The Adler-32 checksum is stored as s2*65536 + s1.
            return (s2 << 16) + s1;
        }
    }
}