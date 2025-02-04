// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// This class is intended for empira internal use only and may change or drop in future releases.
    /// </summary>
    public class PdfCustomValue : PdfDictionary
    {
        /// <summary>
        /// This function is intended for empira internal use only.
        /// </summary>
        public PdfCustomValue()
        {
            CreateStream([]);
        }

        /// <summary>
        /// This function is intended for empira internal use only.
        /// </summary>
        public PdfCustomValue(byte[] bytes)
        {
            CreateStream(bytes);
        }

        internal PdfCustomValue(PdfDocument document)
            : base(document)
        {
            CreateStream([]);
        }

        internal PdfCustomValue(PdfDictionary dict)
            : base(dict)
        {
            // TODO_OLD: uncompress stream
        }

        /// <summary>
        /// This property is intended for empira internal use only.
        /// </summary>
        public PdfCustomValueCompressionMode CompressionMode;

        /// <summary>
        /// This property is intended for empira internal use only.
        /// </summary>
        public byte[]? Value
        {
            get => Stream?.Value;
            set
            {
                if (Stream != null)
                    Stream.Value = value!;
            }
        }
    }
}
