// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Filters
{
    /// <summary>
    /// Implements a dummy filter used for not implemented filters.
    /// </summary>
    public abstract class NoOpFiler : Filter
    {
        /// <summary>
        /// Returns a copy of the input data.
        /// </summary>
        public override byte[] Encode(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return (byte[])data.Clone();
        }

        /// <summary>
        /// Returns a copy of the input data.
        /// </summary>
        public override byte[] Decode(byte[] data, FilterParms? parms)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return (byte[])data.Clone();
        }
    }
}
