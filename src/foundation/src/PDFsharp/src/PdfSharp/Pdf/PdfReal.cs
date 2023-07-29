// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a direct real value.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public sealed class PdfReal : PdfNumber
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfReal"/> class.
        /// </summary>
        public PdfReal()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfReal"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public PdfReal(double value)
        {
            if (value is < Single.MinValue or > Single.MaxValue)
                Debug.Assert(false);
            _value = value;
        }

        /// <summary>
        /// Gets the value as double.
        /// </summary>
        public double Value => _value;

        readonly double _value;

        /// <summary>
        /// Returns the real number as string.
        /// </summary>
        public override string ToString() 
            => _value.ToString(Config.SignificantFigures3, CultureInfo.InvariantCulture);

        /// <summary>
        /// Writes the real value with up to three digits.
        /// </summary>
        internal override void WriteObject(PdfWriter writer) 
            => writer.Write(this);
    }
}
