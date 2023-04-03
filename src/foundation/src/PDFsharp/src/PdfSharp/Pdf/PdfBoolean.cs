// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a direct boolean value.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public sealed class PdfBoolean : PdfItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfBoolean"/> class.
        /// </summary>
        public PdfBoolean()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfBoolean"/> class.
        /// </summary>
        public PdfBoolean(bool value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the value of this instance as boolean value.
        /// </summary>
        public bool Value { get; }

        /// <summary>
        /// A pre-defined value that represents <c>true</c>.
        /// </summary>
        public static readonly PdfBoolean True = new(true);

        /// <summary>
        /// A pre-defined value that represents <c>false</c>.
        /// </summary>
        public static readonly PdfBoolean False = new(false);

        /// <summary>
        /// Returns 'false' or 'true'.
        /// </summary>
        public override string ToString() 
            => Value ? bool.TrueString : bool.FalseString;

        /// <summary>
        /// Writes 'true' or 'false'.
        /// </summary>
        internal override void WriteObject(PdfWriter writer) 
            => writer.Write(this);
    }
}
