// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal.OpenType;
using PdfSharp.Fonts;
using PdfSharp.Pdf.Filters;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents the base class of a PDF font.
    /// </summary>
    public sealed class PdfFontProgram : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFontProgram"/> class.
        /// </summary>
        internal PdfFontProgram(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFontProgram(PdfDictionary dict)
            : base(dict)
        { }

        internal void CreateFontFileAndAddToDescriptor(PdfFontDescriptor pdfFontDescriptor, CMapInfo cmapInfo, bool cidFont)
        {
            //var x = pdfFontDescriptor.Elements[PdfFontDescriptor.Keys.FontFile2];
            var x = pdfFontDescriptor.Elements.GetValue(PdfFontDescriptor.Keys.FontFile2); // #US373

            OpenTypeFontFace subSet = pdfFontDescriptor.Descriptor.FontFace.CreateFontSubset(cmapInfo.GlyphIndices, cidFont);
            byte[] fontData = subSet.OTFontSource.Bytes;

            Owner.Internals.AddObject(this);
            pdfFontDescriptor.Elements[PdfFontDescriptor.Keys.FontFile2] = RequiredReference;

            Elements["/Length1"] = new PdfInteger(fontData.Length);
            if (!Owner.Options.NoCompression)
            {
                fontData = Filtering.FlateDecode.Encode(fontData, Document.Options.FlateEncodeMode);
                Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
            }
            Elements.SetInteger(PdfStream.Keys.Length, fontData.Length);
            CreateStream(fontData);
        }
    }
}
