// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Filters
{
    /// <summary>
    /// Implements a dummy DCTDecode filter.
    /// Filter does nothing, but allows to read and write PDF files with
    /// DCT encoded streams.
    /// </summary>
    public class DctDecode : NoOpFiler
    {
        // Reference: 3.3.7  DCTDecode Filter / Page 84
        // Reference 2.0: 7.4.8  DCTDecode filter / Page 48

        // Implemented as a hack to read the ISO_32000-2_2020(en).pdf file.

        /// <summary>
        /// Returns a copy of the input data.
        /// </summary>
        // ReSharper disable once RedundantOverriddenMember
        public override byte[] Encode(byte[] data)
        {
            // TODO. Check where it is used.
            return base.Encode(data);
        }

        /// <summary>
        /// Returns a copy of the input data.
        /// </summary>
        // ReSharper disable once RedundantOverriddenMember
        public override byte[] Decode(byte[] data, FilterParms? parms)
        {
            // TODO. Check where it is used.
            return base.Decode(data, parms);
        }
    }
}
