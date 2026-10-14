// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Internal
{
    public static class ChecksumHelper
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
                    break;

                s1 = (s1 + b) % AdlerModulus;
                s2 = (s1 + s2) % AdlerModulus;
                count++;
            }

            // The Adler-32 checksum is stored as s2*65536 + s1.
            return (s2 << 16) + s1;
        }


        /// <summary>
        /// Calculates an Adler32 checksum combined with the buffer length
        /// in a 64-bit unsigned integer.
        /// </summary>
        public static ulong CalcChecksum(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            const uint prime = 65521; // largest prime smaller than 65536
            uint s1 = 0;
            uint s2 = 0;
            int length = buffer.Length;
            int offset = 0;
            while (length > 0)
            {
                int n = 3800;
                if (n > length)
                    n = length;
                length -= n;
                while (--n >= 0)
                {
                    s1 += buffer[offset++];
                    s2 += s1;
                }
                s1 %= prime;
                s2 %= prime;
            }
            //return ((ulong)((ulong)(((ulong)s2 << 16) | (ulong)s1)) << 32) | (ulong)buffer.Length;
            ulong ul1 = ((ulong)s2 << 16) | s1;
            //ul1 |= s1;
            uint ui2 = (uint)buffer.Length;
            return (ul1 << 32) | ui2;
        }
    }
}
