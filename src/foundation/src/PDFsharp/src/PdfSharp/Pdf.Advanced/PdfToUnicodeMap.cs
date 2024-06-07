// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using Microsoft.Extensions.Logging;
using PdfSharp.Fonts;
using PdfSharp.Logging;
using PdfSharp.Pdf.Filters;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents a ToUnicode map for composite font.
    /// </summary>
    sealed class PdfToUnicodeMap : PdfDictionary
    {
        public PdfToUnicodeMap(PdfDocument document)
            : base(document)
        { }

        public PdfToUnicodeMap(PdfDocument document, CMapInfo cmapInfo)
            : base(document)
        {
            CMapInfo = cmapInfo;
        }

        /// <summary>
        /// Gets or sets the CMap info.
        /// </summary>
        public CMapInfo CMapInfo { get; set; } = default!;

        /// <summary>
        /// Creates the ToUnicode map from the CMapInfo.
        /// </summary>
        internal override void PrepareForSave()
        {
            base.PrepareForSave();

            // This code comes literally from PDF Reference.
            string prefix =
              "/CIDInit /ProcSet findresource begin\n" +
              "12 dict begin\n" +
              "begincmap\n" +
              "/CIDSystemInfo << /Registry (Adobe)/Ordering (UCS)/Supplement 0>> def\n" +
              "/CMapName /Adobe-Identity-UCS def /CMapType 2 def\n";
            string suffix = "endcmap CMapName currentdict /CMap defineresource pop end end";

            //var glyphIndexToCharacter = new Dictionary<int, char>();
            var glyphIndexToCharacter = new Dictionary<int, int>();
            int lowIndex = 65536, hiIndex = -1;
            foreach (KeyValuePair<int, ushort> entry in CMapInfo.CodePointsToGlyphIndices)
            {
                int index = (int)entry.Value;
                lowIndex = Math.Min(lowIndex, index);
                hiIndex = Math.Max(hiIndex, index);
                //glyphIndexToCharacter.Add(index, entry.Key);
                glyphIndexToCharacter[index] = entry.Key;  // BUG - Key may be a surrogate pair. Should it be the code point?
            }

            using var ms = new MemoryStream();
            using var wrt = new StreamWriter(ms, Encoding.ASCII);

            wrt.Write(prefix);

            wrt.WriteLine("1 begincodespacerange");
            //wrt.WriteLine(String.Format(CultureInfo.InvariantCulture, "<{0:X4}><{1:X4}>", lowIndex, hiIndex));
            wrt.WriteLine(Invariant($"<{lowIndex:X4}><{hiIndex:X4}>"));
            wrt.WriteLine("endcodespacerange");

            // Sorting seems not necessary. The limit is 100 entries, we will see.
            //wrt.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0} beginbfrange", glyphIndexToCharacter.Count));
            wrt.WriteLine(Invariant($"{glyphIndexToCharacter.Count} beginbfrange"));
            var array = glyphIndexToCharacter.Keys.ToArray();
            Array.Sort(array);

            //foreach (var item in glyphIndexToCharacter)
            for (int idx = 0; idx < array.Length; idx++)
            {
                var key = array[idx];
                var value = glyphIndexToCharacter[key];
#if DEBUG
                PdfSharpLogHost.Logger.LogDebug($"Glyph index: {key,-7} {(value /*& 0xFFFF_0000*/) >>> 16,4:X4}-{value & 0xFFFF,4:X4}");
#endif
                //wrt.WriteLine(String.Format(CultureInfo.InvariantCulture, "<{0:X4}><{0:X4}><{1:X4}>", item.Key, (int)item.Value));
                if ((value & 0xFFFF_0000) == 0)
                {
                    // TODO: handle surrogate pairs here.
                }
                wrt.WriteLine(Invariant($"<{key:X4}><{key:X4}><{(uint)value:X4}>"));
            }
            wrt.WriteLine("endbfrange");
            wrt.Write(suffix);
            wrt.Close();

            // Compress like content streams.
            byte[] bytes = ms.ToArray();
            ms.Close();

            if (Owner.Options.CompressContentStreams)
            {
                Elements.SetName("/Filter", "/FlateDecode");
                bytes = Filtering.FlateDecode.Encode(bytes, _document.Options.FlateEncodeMode);
            }
            else
            {
                Elements.Remove("/Filter");
            }

            if (Stream == null!)
                CreateStream(bytes);
            else
            {
                Stream.Value = bytes;
                Elements.SetInteger("/Length", Stream.Length);
            }
        }

        public sealed class Keys : PdfStream.Keys
        {
            // No new keys.
        }
    }
}
