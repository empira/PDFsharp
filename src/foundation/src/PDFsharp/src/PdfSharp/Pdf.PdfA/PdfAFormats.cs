// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// ReSharper disable InconsistentNaming

namespace PdfSharp.Pdf.PdfA
{
    /// <summary>
    /// Defines all well-defined PDF/A formats.
    /// See https://en.wikipedia.org/wiki/PDF/A
    /// </summary>
    public static class PdfAFormats
    {
        /// <summary>
        /// PDF/A-1b – Level B (Basic) conformance.
        /// </summary>
        public static PdfAFormat PdfA_1a { get; } = new(1, 'a');

        /// <summary>
        /// PDF/A-1a – Level A (Accessible) conformance.
        /// </summary>
        public static PdfAFormat PdfA_1b { get; } = new(1, 'b');

        /// <summary>
        /// PDF/A-2a – Level A (Accessible) conformance.
        /// </summary>
        public static PdfAFormat PdfA_2a { get; } = new(2, 'a');

        /// <summary>
        /// PDF/A-2b – Level B (Basic) conformance.
        /// </summary>
        public static PdfAFormat PdfA_2b { get; } = new(2, 'b');

        /// <summary>
        /// PDF/A-2u – Level U (Unicode) conformance.
        /// </summary>
        public static PdfAFormat PdfA_2u { get; } = new(2, 'b');

        /// <summary>
        /// PDF/A-3a – Level A (Accessible) conformance.
        /// </summary>
        public static PdfAFormat PdfA_3a { get; } = new(3, 'a');

        /// <summary>
        /// PDF/A-3b – Level B (Basic) conformance.
        /// </summary>
        public static PdfAFormat PdfA_3b { get; } = new(3, 'b');

        /// <summary>
        /// PDF/A-3u – Level U (Unicode) conformance.
        /// </summary>
        public static PdfAFormat PdfA_3u { get; } = new(3, 'u');

        /// <summary>
        /// PDF/A-4a – Level A (Accessible) conformance.
        /// </summary>
        public static PdfAFormat PdfA_4a { get; } = new(4, 'a');

        /// <summary>
        /// PDF/A-4b – Level B (Basic) conformance.
        /// </summary>
        public static PdfAFormat PdfA_4b { get; } = new(4, 'b');

        /// <summary>
        /// PDF/A-4f – Level F (Engineering) conformance.
        /// </summary>
        public static PdfAFormat PdfA_4e { get; } = new(4, 'e');

        /// <summary>
        /// PDF/A-4e – Level E (arbitrary Files) conformance.
        /// </summary>
        public static PdfAFormat PdfA_4f { get; } = new(4, 'f');
    }
}
