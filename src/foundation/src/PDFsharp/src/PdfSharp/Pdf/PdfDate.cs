// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a direct date value.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public sealed class PdfDate : PdfItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDate"/> class.
        /// </summary>
        public PdfDate()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDate"/> class.
        /// </summary>
        public PdfDate(string value)
        {
            Value = Parser.ParseDateTime(value, DateTime.MinValue);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDate"/> class.
        /// </summary>
        public PdfDate(DateTime value)
        {
            // #PDF-A
            // We cannot check here whether the document must be PDF/A conform or not.
            // So we always chop milliseconds.
            // Remove milliseconds to ensure that date values in Metadata and Info are equal.
            value = new(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);
            Value = value;
        }

        /// <summary>
        /// Gets the value as DateTime.
        /// </summary>
        public DateTime Value { get; }

        /// <summary>
        /// Returns the value in the PDF date format.
        /// </summary>
        public override string ToString()
        {
            string delta = Value.ToString("zzz").Replace(':', '\'');
            return $"D:{Value:yyyyMMddHHmmss}{delta}'";
        }

        /// <summary>
        /// Writes the value in the PDF date format.
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
        {
            writer.WriteDocString(ToString());
        }
    }
}
