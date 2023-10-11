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
                    if (char.IsLowSurrogate(text, idx))
                        continue; // Ignore the second char of a surrogate pair.

                    char ch = text[idx];
                    if (!CharacterToGlyphIndex.ContainsKey(ch) || char.IsHighSurrogate(ch))
                    {
                        char ch2 = ch;
                        if (symbol)
                        {
                            // Remap ch for symbol fonts.
                            ch2 = (char)(ch | (_descriptor.FontFace.os2.usFirstCharIndex & 0xFF00));  // @@@ refactor
                        }
                        uint glyphIndex;

                        if (char.IsHighSurrogate(ch))
                        {
                            // If high surrogate char hasn't been added yet, add high and low surrogate chars:
                            if (!SurrogatePairs.ContainsKey(ch))
                                SurrogatePairs.Add(ch, new List<char>(text[idx + 1]));
                            // If high surrogate char has been added and low surrogate char hasn't been added yet, add low surrogate char:
                            else if (SurrogatePairs.ContainsKey(ch) && !SurrogatePairs[ch].Contains(text[idx + 1]))
                                SurrogatePairs[ch].Add(text[idx + 1]);
                            // If high and low surrogate chars have been added, continue with next loop:
                            else
                                continue;
                            glyphIndex = _descriptor.CharCodeToGlyphIndex(ch, text[idx + 1]);
                        }
                        else
                            glyphIndex = _descriptor.CharCodeToGlyphIndex(ch2);

                        if (!CharacterToGlyphIndex.ContainsKey(ch)) // To do (for support of reading PDF?): Surrogate pair chars with same high surrogate chars and different low surrogate chars are missing in "CharacterToGlyphIndex"!
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
                    var glyphIndex = glyphIndices[idx];
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

        public uint[] GetGlyphIndices()
        {
            uint[] indices = new uint[GlyphIndices.Count];
            GlyphIndices.Keys.CopyTo(indices, 0);
            Array.Sort(indices);
            return indices;
        }

        public char MinChar = Char.MaxValue;
        public char MaxChar = Char.MinValue;
        public Dictionary<char, uint> CharacterToGlyphIndex = new Dictionary<char, uint>();
        public Dictionary<uint, object> GlyphIndices = new Dictionary<uint, object>();
        private Dictionary<char, List<char>> SurrogatePairs = new Dictionary<char, List<char>>();
    }
}
