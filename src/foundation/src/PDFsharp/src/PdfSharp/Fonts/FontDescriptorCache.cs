// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections.Concurrent;
using PdfSharp.Drawing;
using PdfSharp.Fonts.OpenType;

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Global table of OpenType font descriptor objects.
    /// </summary>
    static class FontDescriptorCache
    {
        /// <summary>
        /// Gets the FontDescriptor identified by the specified XFont. If no such object 
        /// exists, a new FontDescriptor is created and added to the cache.
        /// </summary>
        public static FontDescriptor GetOrCreateDescriptorFor(XFont font)
        {
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            font.GlyphTypeface.CheckVersion();

            //FontSelector1 selector = new FontSelector1(font);
            string fontDescriptorKey = font.GlyphTypeface.Key;
            var cache = Globals.Global.Fonts.FontDescriptorCache;
            if (cache.TryGetValue(fontDescriptorKey, out var descriptor))
                return descriptor;

            descriptor = new OpenTypeDescriptor(fontDescriptorKey, font);
            cache.TryAdd(fontDescriptorKey, descriptor);
            return descriptor;
        }

        public static FontDescriptor GetOrCreateDescriptorFor(XGlyphTypeface glyphTypeface)
        {
            glyphTypeface.CheckVersion();

            string fontDescriptorKey = glyphTypeface.Key;
            var cache = Globals.Global.Fonts.FontDescriptorCache;
            if (cache.TryGetValue(fontDescriptorKey, out var descriptor))
                return descriptor;

            descriptor = new OpenTypeDescriptor(fontDescriptorKey, glyphTypeface);
            cache.TryAdd(fontDescriptorKey, descriptor);
            return descriptor;
        }

        /// <summary>
        /// Gets the FontDescriptor identified by the specified FontSelector. If no such object 
        /// exists, a new FontDescriptor is created and added to the stock.
        /// </summary>
        public static FontDescriptor GetOrCreateDescriptor(string fontFamilyName, XFontStyleEx style)
        {
            if (String.IsNullOrEmpty(fontFamilyName))
                throw new ArgumentNullException(nameof(fontFamilyName));

            //FontSelector1 selector = new FontSelector1(fontFamilyName, style);
            string fontDescriptorKey = FontDescriptor.ComputeFdKey(fontFamilyName, style);
            var cache = Globals.Global.Fonts.FontDescriptorCache;
            if (!cache.TryGetValue(fontDescriptorKey, out var descriptor))
            {
                var font = new XFont(fontFamilyName, 10, style);
                descriptor = GetOrCreateDescriptorFor(font);
                // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd because there is not TryAdd in .NET Framework
                if (cache.ContainsKey(fontDescriptorKey))
                    _ = typeof(int); // Just a NOP for a break point.
                else
                    cache.TryAdd(fontDescriptorKey, descriptor);
            }

            return descriptor;
        }

        internal static void Reset()
        {
            Globals.Global.Fonts.FontDescriptorCache.Clear();
        }
    }
}

namespace PdfSharp.Internal
{
    partial class Globals
    {
        partial class FontStorage
        {
            /// <summary>
            /// Maps font descriptor key to font descriptor which is currently only an OpenTypeFontDescriptor.
            /// </summary>
            public readonly ConcurrentDictionary<string, FontDescriptor> FontDescriptorCache = [];
        }
    }
}
