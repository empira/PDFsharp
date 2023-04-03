// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
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
            _value = Parser.ParseDateTime(value, DateTime.MinValue);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDate"/> class.
        /// </summary>
        public PdfDate(DateTime value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the value as DateTime.
        /// </summary>
        public DateTime Value => _value;

        DateTime _value;

        /// <summary>
        /// Returns the value in the PDF date format.
        /// </summary>
        public override string ToString()
        {
            string delta = _value.ToString("zzz").Replace(':', '\'');
            return String.Format("D:{0:yyyyMMddHHmmss}{1}'", _value, delta);
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
