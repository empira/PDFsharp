// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections.Concurrent;
using PdfSharp.Internal.OpenType;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Document specific cache of all PdfFontDescriptor objects of this document.
    /// This allows PdfTrueTypeFont and PdfType0Font 
    /// </summary>
    sealed class PdfFontDescriptorCache(PdfDocument doc)
    {
        //_cache = new Dictionary<OpenTypeDescriptor, PdfFontDescriptor>();

        /// <summary>
        /// Gets the FontDescriptor identified by the specified XFont. If no such object 
        /// exists, a new FontDescriptor is created and added to the cache.
        /// </summary>
        public PdfFontDescriptor GetOrCreatePdfDescriptorFor(OpenTypeFontDescriptor otDescriptor, string baseName)
        {
            if (!_cache.TryGetValue(otDescriptor.Key, out var pdfDescriptor))
            {
                pdfDescriptor = new PdfFontDescriptor(Owner, otDescriptor);
                pdfDescriptor.FontName = pdfDescriptor.CreateEmbeddedFontSubsetName(baseName);
                _cache.TryAdd(otDescriptor.Key, pdfDescriptor);
            }
            return pdfDescriptor;
        }

        PdfDocument Owner { get; } = doc;

        /// <summary>
        /// Maps OpenType descriptor to document specific PDF font descriptor.
        /// </summary>
        readonly ConcurrentDictionary<string, PdfFontDescriptor> _cache = [];
    }
}
