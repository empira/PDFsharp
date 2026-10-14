// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

using System.Globalization;

// v7.0 TODO review

namespace PdfSharp.Internal.OpenType
{
    /// <summary>
    /// The raw bytes of a font file in memory.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class OpenTypeFontSource
    {
        // Signature of a true type collection font.
        const uint ttcf = 0x66637474;

        OpenTypeFontSource(byte[] bytes, ulong checksumKey)
        {
            Bytes = bytes;
            ChecksumKey = checksumKey; // Is calculated later if 0.
        }

        /// <summary>
        /// Gets an existing font source or creates a new one.
        /// A new font source is cached in OpenTypeFontSourceCache.
        /// </summary>
        public static OpenTypeFontSource GetOrCreateFrom(byte[] bytes, ulong checksumKey = 0)
        {
            checksumKey = checksumKey == 0 ? ChecksumHelper.CalcChecksum(bytes) : checksumKey;
            Debug.Assert(checksumKey == ChecksumHelper.CalcChecksum(bytes));

            var openTypeFontSourceCache = OtGlobals.Global.OTFonts.OpenTypeFontSourceCache;
            if (openTypeFontSourceCache.TryGetFontSourceByKey(checksumKey, out var fontSource))
                return fontSource;

            fontSource = new(bytes, checksumKey);
            fontSource = openTypeFontSourceCache.CacheFontSource(fontSource);
            return fontSource;
        }

        /// <summary>
        /// Creates an OpenTypeFontSource from a URI.
        /// Returns null if the URI does not exist or is not a file URI.
        /// </summary>
        public static OpenTypeFontSource? GetOrCreateFrom(Uri fontUri)
        {
            if (fontUri.IsFile)
            {
                var path = fontUri.LocalPath;
                if (String.IsNullOrEmpty(path))
                    throw new ArgumentNullException(nameof(path), "URI does not contain a local path.");

                var bytes = File.ReadAllBytes(path);
                return GetOrCreateFrom(bytes);
            }
            return null;
        }

#if not_used  // DELETE
        static OpenTypeFontSource GetOrCreateFromBytes(string typefaceKey, byte[] fontBytes)
        {
            ulong checksumKey = ChecksumHelper.CalcChecksum(fontBytes);
            if (OpenTypeFontFactory.TryGetFontSourceByKey(checksumKey, out var fontSource))
            {
                // The font source already exists, but is not yet cached under the specified typeface key.
                OpenTypeFontFactory.CacheExistingFontSourceWithNewTypefaceKey(typefaceKey, fontSource);
            }
            else
            {
                // No font source exists. Create new one and cache it.
                fontSource = new(fontBytes, checksumKey);
                OpenTypeFontFactory.CacheNewFontSource(typefaceKey, fontSource);
            }
            return fontSource;
        }
#endif

        /// <summary>
        /// Creates a font source from a byte array.
        /// The result is not cached.
        /// </summary>
        public static OpenTypeFontSource CreateCompiledFont(byte[] bytes)
        {
            var fontSource = new OpenTypeFontSource(bytes, 0);
            return fontSource;
        }

        /// <summary>
        /// Gets or sets the (first) font face.
        /// </summary>
        public OpenTypeFontFace OTFontFace { get; set; } = null!;

        public bool IsTrueTypeCollection => NumberOfFontFaces > 1;

        public int NumberOfFontFaces => 1;  // OpenType collections is not yet implemented

        public OpenTypeFontFace GetOTFontFace(int index)
        {
#if NET8_0_OR_GREATER // TODO EXTENSIONS    
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, NumberOfFontFaces, nameof(index));
#endif

            if (index == 1)
                return OTFontFace;

            throw new NotSupportedException("TODO other font faces");
        }

        /// <summary>
        /// Gets the key that uniquely identifies this font source.
        /// </summary>
        public ulong ChecksumKey
        {
            get
            {
                if (field == 0)
                    field = ChecksumHelper.CalcChecksum(Bytes);
                return field;
            }
        }

        /// <summary>
        /// //Gets the name of the font’s name table.
        /// </summary>
        public string FontFaceKey => OTFontFace?.OTDescriptor.Key ?? null!; //_fontFaceKey;

        //string _fontFaceKey = null!;

        /// <summary>
        /// Gets the bytes of the font.
        /// </summary>
        public byte[] Bytes { get; }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        public override int GetHashCode()
            => (int)((ChecksumKey >> 32) ^ ChecksumKey);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is not OpenTypeFontSource fontSource)
                return false;
            return ChecksumKey == fontSource.ChecksumKey;
        }

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        internal string DebuggerDisplay =>
            String.Format(CultureInfo.InvariantCulture, "OTFontSource: '{0}', keyhash={1}", FontFaceKey, ChecksumKey % 99991 /* largest prime number less than 100000 */);
    }
}
