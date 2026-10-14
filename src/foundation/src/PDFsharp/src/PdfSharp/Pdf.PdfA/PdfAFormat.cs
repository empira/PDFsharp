// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.PdfA
{
    /// <summary>
    /// Represents the part and conformance level of a PDF/A document.
    /// </summary>
    [DebuggerDisplay("({this.Name})")]
    public readonly struct PdfAFormat
    {
        internal PdfAFormat(int part, char conformanceLevel)
        {
            Part = part;
            ConformanceLevel = Char.ToUpper(conformanceLevel);
        }

        /// <summary>
        /// Gets the part number of PDF/A format.
        /// E.g. 1, 2, 3, or 4.
        /// </summary>
        public int Part { get; }

        /// <summary>
        /// Gets the level of conformance of PDF/A format.
        /// E.g. B, A, or U.
        /// </summary>
        public char ConformanceLevel { get; }

        /// <summary>
        /// Gets readable name of PDF/A format without the prefix PDF/A.
        /// E.g. '3B'.
        /// </summary>
        public string Name => Invariant($"{Part}{ConformanceLevel}");

        /// <summary>
        /// Gets readable name of PDF/A format including the prefix PDF/A.
        /// E.g. 'PDF/A-3B'.
        /// </summary>
        public string FullName => Invariant($"PDF/A-{Part}{ConformanceLevel}");

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        public override String ToString() => Part != 0 ? FullName : "(n/a)";

        /// <summary>
        /// Forces to create a format not defined in the class PdfAFormats.
        /// May be useful for testing only.
        /// </summary>
        /// <param name="part">Part number of PDF/A format</param>
        /// <param name="conformanceLevel">Level of conformance of PDF/A format</param>
        public static PdfAFormat ForcePdfAFormat(int part, char conformanceLevel) => new(part, conformanceLevel);
    }
}
