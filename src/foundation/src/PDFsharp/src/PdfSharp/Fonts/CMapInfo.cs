// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using System.Collections.Generic;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Helper class that determines the characters used in a particular font.
    /// </summary>
    class CMapInfo
    {
        public CMapInfo(OpenTypeDescriptor descriptor)
        {
            Debug.Assert(descriptor != null);
            _descriptor = descriptor;
        }
        internal OpenTypeDescriptor _descriptor;

        /// <summary>
        /// Adds the characters of the specified string to the hashtable.
        /// </summary>
        public void AddChars(string text)
        {
            if (text != null)
            {
                bool symbol = _descriptor.FontFace.cmap.symbol;
                int length = text.Length;
                for (int idx = 0; idx < length; idx++)
                {
                    int glyphIndex;
                    if (char.IsSurrogate(text, idx)
                        || char.IsHighSurrogate(text, idx)
                        || char.IsSurrogatePair(text, idx))
                    {
                        glyphIndex = _descriptor.CharCodeToGlyphIndex(text, ref idx);
                        GlyphIndices[glyphIndex] = default!;
                        continue;
                    }
                    char ch = text[idx];
                    if (!CharacterToGlyphIndex.ContainsKey(ch))
                    {
                        var ch2 = ch;
                        if (symbol)
                        {
                            // Remap ch for symbol fonts.
                            ch2 = (char)(ch | (_descriptor.FontFace.os2.usFirstCharIndex & 0xFF00));  // @@@ refactor
                        }
                        glyphIndex = _descriptor.CharCodeToGlyphIndex(ch2);
                        CharacterToGlyphIndex.Add(ch, glyphIndex);
                        GlyphIndices[glyphIndex] = default!;
                        MinChar = (char)Math.Min(MinChar, ch);
                        MaxChar = (char)Math.Max(MaxChar, ch);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the glyphIndices to the hashtable.
        /// </summary>
        public void AddGlyphIndices(string glyphIndices)
        {
            if (glyphIndices != null!)
            {
                int length = glyphIndices.Length;
                for (int idx = 0; idx < length; idx++)
                {
                    int glyphIndex = glyphIndices[idx];
                    GlyphIndices[glyphIndex] = null!;
                }
            }
        }

        /// <summary>
        /// Adds a ANSI characters.
        /// </summary>
        internal void AddAnsiChars()
        {
            byte[] ansi = new byte[256 - 32];
            for (int idx = 0; idx < 256 - 32; idx++)
                ansi[idx] = (byte)(idx + 32);
#if EDF_CORE
            string text = null; // PdfEncoders.WinAnsiEncoding.GetString(ansi, 0, ansi.Length);
#else
            string text = PdfEncoders.WinAnsiEncoding.GetString(ansi, 0, ansi.Length);
#endif
            AddChars(text);
        }

        internal bool Contains(char ch)
        {
            return CharacterToGlyphIndex.ContainsKey(ch);
        }

        public char[] Chars
        {
            get
            {
                char[] chars = new char[CharacterToGlyphIndex.Count];
                CharacterToGlyphIndex.Keys.CopyTo(chars, 0);
                Array.Sort(chars);
                return chars;
            }
        }

        public int[] GetGlyphIndices()
        {
            int[] indices = new int[GlyphIndices.Count];
            GlyphIndices.Keys.CopyTo(indices, 0);
            Array.Sort(indices);
            return indices;
        }

        public char MinChar = Char.MaxValue;
        public char MaxChar = Char.MinValue;
        public Dictionary<char, int> CharacterToGlyphIndex = new Dictionary<char, int>();
        public Dictionary<int, object> GlyphIndices = new Dictionary<int, object>();
    }
}
