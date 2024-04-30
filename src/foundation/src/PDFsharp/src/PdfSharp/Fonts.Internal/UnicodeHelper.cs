// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Logging;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Fonts.Internal
{
    /// <summary>
    /// A bunch of internal functions to handle Unicode.
    /// </summary>
    static class UnicodeHelper
    {
        internal const char HighSurrogateStart = '\uD800';
        internal const char HighSurrogateEnd = '\uDBFF';
        internal const char LowSurrogateStart = '\uDC00';
        internal const char LowSurrogateEnd = '\uDFFF';
        internal const int HighSurrogateRange = 0x3FF;
        internal const int UnicodePlane01Start = 0x10000;

        /// <summary>
        /// Converts a UTF-16 string into an array of Unicode code points.
        /// </summary>
        /// <param name="s">The string to be converted.</param>
        /// <param name="coerceAnsi">if set to <c>true</c> [coerce ANSI].</param>
        /// <param name="nonAnsi">The non ANSI.</param>
        public static int[] Utf32FromString(string s, bool coerceAnsi = false, byte nonAnsi = (byte)'?')  // #NFM docu, design, ...
        {
            if (String.IsNullOrEmpty(s))
                return [];

            int length = s.Length;
            var result = new int[length];
            int iRes = 0;

            for (int idx = 0; idx < length; idx++)
            {
                ref var current = ref result[iRes++];

                var ch = s[idx];
                current = ch;
                int high10Bits = ch - HighSurrogateStart;
                if ((uint)high10Bits <= HighSurrogateRange)
                {
                    // Case: ch is a high surrogate.
                    idx++;
                    if ((uint)idx < (uint)s.Length)
                    {
                        var ch2 = s[idx];
                        int low10Bits = ch2 - LowSurrogateStart;
                        if ((uint)low10Bits <= HighSurrogateRange)
                        {
                            // Case: ch2 is a low surrogate.

                            // Combine high and low surrogate code points into UTF-32 Unicode code point.
                            current = (high10Bits << 10) + low10Bits + UnicodePlane01Start;
                        }
                        else
                        {
                            // Case: Unpaired high surrogate.
                            PdfSharpLogHost.FontManagementLogger.LogDebug("High surrogate 0x{Char:X2} not followed by a low surrogate.", ch);
                            idx--;
                            continue;
                        }
                    }
                    else
                    {
                        // Case: High surrogate at string end.
                        PdfSharpLogHost.FontManagementLogger.LogDebug("High surrogate 0x{Char:X2} found at end of string.", ch);
                        break;
                    }
                }
                else
                {
                    // Case: ch is in BMP range.

                    //if (ch - LOW_SURROGATE_START <= HIGH_SURROGATE_RANGE)
                    if ((uint)ch - LowSurrogateStart <= HighSurrogateRange)
                    {
                        // Case: unpaired low surrogate.
                        // We only come here when the text contains a low surrogate not preceded by a high surrogate.
                        // This is an error in the UTF-32 text.
                        PdfSharpLogHost.FontManagementLogger.LogDebug("Unexpected low surrogate found: 0x{Char:X2}", ch);
                        continue;
                    }

                    if (coerceAnsi && !AnsiEncoding.IsAnsi(ch))
                        current = nonAnsi;
                }
            }
            if (iRes < length)
                Array.Resize(ref result, iRes);
            return result;
        }

        /// <summary>
        /// Converts a UTF-16 string into an array of code points of a symbol font.
        /// </summary>
        public static int[] SymbolCodePointsFromString(string s, OpenTypeDescriptor openTypeDescriptor)
        {
            if (String.IsNullOrEmpty(s))
                return [];

            int length = s.Length;
            var result = new int[length];

            for (int idx = 0; idx < length; idx++)
                result[idx] = openTypeDescriptor.RemapSymbolChar(s[idx]);

            return result;
        }

        /// <summary>
        /// Convert a surrogate pair to UTF-32 code point.
        /// Similar to Char.ConvertToUtf32 but never throws an error.
        /// Instead, returns 0 if one of the surrogates are invalid.
        /// </summary>
        /// <param name="highSurrogate">The high surrogate.</param>
        /// <param name="lowSurrogate">The low surrogate.</param>
        public static int ConvertToUtf32(char highSurrogate, char lowSurrogate)
        {
            uint highSurrogateOffset = (uint)highSurrogate - UnicodeHelper.HighSurrogateStart;
            uint lowSurrogateOffset = (uint)lowSurrogate - UnicodeHelper.LowSurrogateStart;

            // If surrogates not in range return 0.
            // The cool code using underflow effect to check two ranges with one comparison comes from the .NET source code.
            if ((highSurrogateOffset | lowSurrogateOffset) > UnicodeHelper.HighSurrogateRange)
                return 0;
            // Convert to code point.
            return ((int)highSurrogateOffset << 10) + (lowSurrogate - UnicodeHelper.LowSurrogateStart) + UnicodePlane01Start;
        }

        internal static bool IsInRange(char c, char min, char max) => (uint)(c - min) <= (uint)(max - min);
    }
}
