// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Security
{
    /// <summary>
    /// Specifies which operations are permitted when the document is opened with user access.
    /// </summary>
    [Flags]
    enum PdfUserAccessPermission : uint
    {
        /// <summary>
        /// Permits everything. This is the default value.
        /// </summary>
        //PermitAll = -3, // = 0xFFFFFFFC,
        PermitAll = 0xFFFF_FFFC,

        // Bit 1–2 Reserved. Must be zero (0).

        // Bit 3 (Security handlers of revision 2) Print the document.
        // (Security handlers of revision 3 or greater) Print the document (possibly not at the highest
        // quality level, depending on whether bit 12 is also set).
        PermitPrint = 0x0000_0004,  // 1 << (3 - 1),

        // Bit 4 Modify the contents of the document by operations other than
        // those controlled by bits 6, 9, and 11.
        PermitModifyDocument = 0x0000_0008,  // 1 << (4 - 1),

        // Bit 5 Copy or otherwise extract text and graphics from the
        // document. However, for the limited purpose of providing this content to assistive technology,
        // a PDF reader should behave as if this bit was set to 1.
        // NOTE For accessibility, ISO 32000-1 had this option restricted by bit 10,
        // but that exception has been deprecated in PDF 2.0.
        PermitExtractContent = 0x0000_0010,  // 1 << (5 - 1),

        // Bit 6 Add or modify text annotations, fill in interactive form fields, and,
        // if bit 4 is also set, create or modify interactive form fields (including
        // signature fields).
        PermitAnnotations = 0x0000_0020,  // 1 << (6 - 1),

        // Bit 7–8 Reserved. Must be 1.

        // 9 (Security handlers of revision 3 or greater) Fill in existing interactive form fields (including
        // signature fields), even if bit 6 is clear.
        PermitFormsFill = 0x0000_0100,  // 1 << (9 - 1),

        // Bit 10 Not used.
        // This bit was previously used to determine whether content could be extracted for the
        // purposes of accessibility, however, that restriction has been deprecated in PDF 2.0.
        // PDF readers shall ignore this bit and PDF writers shall always set this bit to 1 to
        // ensure compatibility with PDF readers following earlier specifications.

        // Bit 11 (Security handlers of revision 3 or greater) Assemble the document (insert, rotate, or delete
        // pages and create document outline items or thumbnail images), even if bit 4
        // is clear.
        PermitAssembleDocument = 0x0000_0400,  // 1 << (11 - 1),

        // Bit 12 (Security handlers of revision 3 or greater) Print the document to a representation from
        // which a faithful digital copy of the PDF content could be generated, based on an implementation-dependent algorithm.
        // When this bit is clear (and bit 3 is set), printing shall be limited to a low-level
        // representation of the appearance, possibly of degraded quality.
        PermitFullQualityPrint = 0x0000_0800,  // 1 << (12 - 1),

        // Bit 13–32 (Security handlers of revision 3 or greater) Reserved. Must be 1.
    }
}
