// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections;
using PdfSharp.Internal.OpenType;

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Helper class that collects the characters and glyphs used in a particular font.
    /// The glyphs are used in /FontDescriptor for calculating the font subset.
    /// The characters are used in /TrueType fonts with /WinAnsiEncoding for the /Widths array.
    /// The characters are used in /Type0 fonts with /Identity-H encoding for /ToUnicode map and
    /// in the descendant /CIDFontType2 for the /W array.
    /// </summary>
    class CMapInfo
    {
        public CMapInfo(int glyphCount)
        {
            GlyphIndicesNew = new(glyphCount);
        }

        public void AddChars(CodePointGlyphIndexPair[] codePoints)
        {
            int length = codePoints.Length;

            for (int idx = 0; idx < length; idx++)
            {
                var item = codePoints[idx];
                if (item.GlyphIndex == 0)
                    continue;
#if true  // #PSGFX
                if (!CodePointsToGlyphIndices.ContainsKey(item.CodePoint))
                {
                    CodePointsToGlyphIndices.Add(item.CodePoint, item.GlyphIndex);
                    GlyphIndices[item.GlyphIndex] = null;
                    GlyphIndicesNew[item.GlyphIndex] = true;
                    MinCodePoint = Math.Min(MinCodePoint, item.CodePoint);
                    MaxCodePoint = Math.Max(MaxCodePoint, item.CodePoint);
                }
                else
                {
                    // In PDFsharp Graphics we also can draw glyphs without a code point.
                    GlyphIndices[item.GlyphIndex] = null;
                    GlyphIndicesNew[item.GlyphIndex] = true;
                }
#else
                if (CodePointsToGlyphIndices.ContainsKey(item.CodePoint))
                    continue;

                CodePointsToGlyphIndices.Add(item.CodePoint, item.GlyphIndex);
                GlyphIndices[item.GlyphIndex] = default;
                MinCodePoint = Math.Min(MinCodePoint, item.CodePoint);
                MaxCodePoint = Math.Max(MaxCodePoint, item.CodePoint);
#endif
            }
        }

        internal bool Contains(char ch)
            => CodePointsToGlyphIndices.ContainsKey(ch);

        public int[] Chars
        {
            get
            {
                var chars = new int[CodePointsToGlyphIndices.Count];
                CodePointsToGlyphIndices.Keys.CopyTo(chars, 0);
                Array.Sort(chars);
                return chars;
            }
        }

        /// <summary>
        /// Gets an ordered array glyph indices.
        /// </summary>
        public ushort[] GetGlyphIndices()
        {
            var indices = new ushort[GlyphIndices.Count];
            GlyphIndices.Keys.CopyTo(indices, 0);
            Array.Sort(indices);
            return indices;
        }

        public int MinCodePoint = Int32.MaxValue;
        public int MaxCodePoint = Int32.MinValue;

        /// <summary>
        /// Maps a Unicode code point to a glyphs.
        /// Contains all used codepoints and their glyphs.
        /// Used for ToUnicode table and /W entry.
        /// </summary>
        public readonly Dictionary<int, ushort> CodePointsToGlyphIndices = [];

        /// <summary>
        /// Collects all used glyph IDs. Value is now used by PDFsharp Graphics where it is possible
        /// to draw only glyphs without associated code point. Therefore, this dictionary is now
        /// relevant for computing the font subset.
        /// We also use this dictionary to fast lookup if a glyph is already added.
        /// </summary>
        public readonly Dictionary<ushort, object?> GlyphIndices = [];
        
        // TODO #PSGFX Use a BitArray for collection the glyphs.
        public readonly BitArray GlyphIndicesNew;
    }
}
