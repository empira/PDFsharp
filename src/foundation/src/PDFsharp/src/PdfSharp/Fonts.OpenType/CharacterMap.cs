// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections;

namespace PdfSharp.Fonts.OpenType
{
    /// <summary>
    /// Filled by cmap type 4 and 12.
    /// </summary>
    internal sealed class CharacterMap : IDictionary<int, ushort> // Called IntMap in WPF.
    {
        /// <summary> 
        /// Glyph count is used for validating cmap contents.
        /// If we discover that glyph index we are about to set or return is outside of glyph range, 
        /// we throw an exception.
        /// </summary>
        internal void SetGlyphCount(ushort glyphCount)
        {
            _glyphCount = glyphCount;
        }

        internal void SetCharacterEntry(int i, ushort value)
        {
            // Some fonts have cmap entries that point to glyphs outside of the font.
            // Just skip such entries. 
            if (value >= _glyphCount)
                return;

            _map[i] = value;
        }

        int GetGlyphPointer(int key) => _map[key];

        // ----------

        public IEnumerator<KeyValuePair<int, ushort>> GetEnumerator() => _map.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(KeyValuePair<int, ushort> item) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public Boolean Contains(KeyValuePair<int, ushort> item) => throw new NotSupportedException();

        public void CopyTo(KeyValuePair<int, ushort>[] array, int arrayIndex) => throw new NotImplementedException();

        public Boolean Remove(KeyValuePair<int, ushort> item) => throw new NotSupportedException();

        public int Count => _map.Count;

        public bool IsReadOnly => true;

        public void Add(int key, ushort value) => throw new NotSupportedException();

        public Boolean ContainsKey(int key) => _map.ContainsKey(key);

        public Boolean Remove(int key) => throw new NotSupportedException();

        public bool TryGetValue(int key, out ushort value) => _map.TryGetValue(key, out value);

        public ushort this[int key]
        {
            get => _map[key];
            set => _map[key] = value;
        }

        public ICollection<int> Keys => _map.Keys;

        public ICollection<ushort> Values => _map.Values;

        ushort _glyphCount;
        readonly Dictionary<int, ushort> _map = new();
    }
}
