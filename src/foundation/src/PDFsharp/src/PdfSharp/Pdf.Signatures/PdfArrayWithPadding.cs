// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Signatures
{
    internal class PdfArrayWithPadding : PdfArray
    {
        public int PaddingRight { get; private set; }

        public PdfArrayWithPadding(PdfDocument document, int paddingRight, params PdfItem[] items) 
            : base(document, items)
        {
            PaddingRight = paddingRight;
        }

        internal override void WriteObject(PdfWriter writer)
        {
            PositionStart = writer.Position;

            base.WriteObject(writer);

            if (PaddingRight > 0)
            {
                var bytes = new byte[PaddingRight];
                for (int i = 0; i < PaddingRight; i++)
                    bytes[i] = 32;// space

                writer.Write(bytes);
            }

            PositionEnd = writer.Position;
        }

        /// <summary>
        /// Position of the first byte of this string in PdfWriter's Stream
        /// </summary>
        public int PositionStart { get; internal set; }

        /// <summary>
        /// Position of the last byte of this string in PdfWriter's Stream
        /// </summary>
        public int PositionEnd { get; internal set; }
    }
}
