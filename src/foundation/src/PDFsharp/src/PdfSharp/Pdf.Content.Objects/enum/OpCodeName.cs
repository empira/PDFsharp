// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// ReSharper disable InconsistentNaming

namespace PdfSharp.Pdf.Content.Objects
{
    /// <summary>
    /// The names of the op-codes. 
    /// </summary>
    public enum OpCodeName
    {
        /// <summary>
        /// Pseudo op-code for the name of a dictionary.
        /// </summary>
        Dictionary,  // Name followed by dictionary.

        // I know that this is not useable in VB or other languages with no case sensitivity.

        // Reference: TABLE A.1  PDF content stream operators / Page 985
        // Reference 2.0: Table A.1 — PDF content stream operators / Page 844

        /// <summary>
        /// Close, fill, and stroke path using non-zero winding number rule.
        /// </summary>
        b,

        /// <summary>
        /// Fill and stroke path using non-zero winding number rule.
        /// </summary>
        B,

        /// <summary>
        /// Close, fill, and stroke path using even-odd rule.
        /// </summary>
        bx,  // b*

        /// <summary>
        /// Fill and stroke path using even-odd rule.
        /// </summary>
        Bx,  // B*

        /// <summary>
        /// (PDF 1.2) Begin marked-content sequence with property list
        /// </summary>
        BDC,

        /// <summary>
        /// Begin inline image object.
        /// </summary>
        BI,

        /// <summary>
        /// (PDF 1.2) Begin marked-content sequence.
        /// </summary>
        BMC,

        /// <summary>
        /// Begin text object.
        /// </summary>
        BT,

        /// <summary>
        /// (PDF 1.1) Begin compatibility section.
        /// </summary>
        BX,

        /// <summary>
        /// Append curved segment to path (three control points).
        /// </summary>
        c,

        /// <summary>
        /// Concatenate matrix to current transformation matrix.
        /// </summary>
        cm,

        /// <summary>
        /// (PDF 1.1) Set color space for stroking operations.
        /// </summary>
        CS,

        /// <summary>
        /// (PDF 1.1) Set color space for nonstroking operations.
        /// </summary>
        cs,

        /// <summary>
        /// Set line dash pattern.
        /// </summary>
        d,

        /// <summary>
        /// Set glyph width in Type 3 font.
        /// </summary>
        d0,

        /// <summary>
        /// Set glyph width and bounding box in Type 3 font.
        /// </summary>
        d1,

        /// <summary>
        /// Invoke named XObject.
        /// </summary>
        Do,

        /// <summary>
        /// (PDF 1.2) Define marked-content point with property list.
        /// </summary>
        DP,

        /// <summary>
        /// End inline image object.
        /// </summary>
        EI,

        /// <summary>
        /// (PDF 1.2) End marked-content sequence.
        /// </summary>
        EMC,

        /// <summary>
        /// End text object.
        /// </summary>
        ET,

        /// <summary>
        /// (PDF 1.1) End compatibility section.
        /// </summary>
        EX,


        /// <summary>
        /// Fill path using non-zero winding number rule.
        /// </summary>
        f,

        /// <summary>
        /// Fill path using non-zero winding number rule (deprecated in PDF 2.0).
        /// </summary>
        F,

        /// <summary>
        /// Fill path using even-odd rule.
        /// </summary>
        fx,  // f*

        /// <summary>
        /// Set gray level for stroking operations.
        /// </summary>
        G,

        /// <summary>
        /// Set gray level for nonstroking operations.
        /// </summary>
        g,

        /// <summary>
        /// (PDF 1.2) Set parameters from graphics state parameter dictionary.
        /// </summary>
        gs,

        /// <summary>
        /// Close subpath.
        /// </summary>
        h,

        /// <summary>
        /// Set flatness tolerance.
        /// </summary>
        i,

        /// <summary>
        /// Begin inline image data
        /// </summary>
        ID,

        /// <summary>
        /// Set line join style.
        /// </summary>
        j,

        /// <summary>
        /// Set line cap style
        /// </summary>
        J,

        /// <summary>
        /// Set CMYK color for stroking operations.
        /// </summary>
        K,

        /// <summary>
        /// Set CMYK color for nonstroking operations.
        /// </summary>
        k,

        /// <summary>
        /// Append straight line segment to path.
        /// </summary>
        l,

        /// <summary>
        /// Begin new subpath.
        /// </summary>
        m,

        /// <summary>
        /// Set miter limit.
        /// </summary>
        M,

        /// <summary>
        /// (PDF 1.2) Define marked-content point.
        /// </summary>
        MP,

        /// <summary>
        /// End path without filling or stroking.
        /// </summary>
        n,

        /// <summary>
        /// Save graphics state.
        /// </summary>
        q,

        /// <summary>
        /// Restore graphics state.
        /// </summary>
        Q,

        /// <summary>
        /// Append rectangle to path.
        /// </summary>
        re,

        /// <summary>
        /// Set RGB color for stroking operations.
        /// </summary>
        RG,

        /// <summary>
        /// Set RGB color for nonstroking operations.
        /// </summary>
        rg,

        /// <summary>
        /// Set color rendering intent.
        /// </summary>
        ri,

        /// <summary>
        /// Close and stroke path.
        /// </summary>
        s,

        /// <summary>
        /// Stroke path.
        /// </summary>
        S,

        /// <summary>
        /// (PDF 1.1) Set color for stroking operations.
        /// </summary>
        SC,

        /// <summary>
        /// (PDF 1.1) Set color for nonstroking operations.
        /// </summary>
        sc,

        /// <summary>
        /// (PDF 1.2) Set color for stroking operations (ICCBased and special color spaces).
        /// </summary>
        SCN,

        /// <summary>
        /// (PDF 1.2) Set color for nonstroking operations (ICCBased and special color spaces).
        /// </summary>
        scn,

        /// <summary>
        /// (PDF 1.3) Paint area defined by shading pattern.
        /// </summary>
        sh,

        /// <summary>
        /// Move to start of next text line.
        /// </summary>
        Tx,  // T*

        /// <summary>
        /// Set character spacing.
        /// </summary>
        Tc,

        /// <summary>
        /// Move text position.
        /// </summary>
        Td,

        /// <summary>
        /// Move text position and set leading.
        /// </summary>
        TD,

        /// <summary>
        /// Set text font and size.
        /// </summary>
        Tf,

        /// <summary>
        /// Show text.
        /// </summary>
        Tj,

        /// <summary>
        /// Show text, allowing individual glyph positioning.
        /// </summary>
        TJ,

        /// <summary>
        /// Set text leading.
        /// </summary>
        TL,

        /// <summary>
        /// Set text matrix and text line matrix.
        /// </summary>
        Tm,

        /// <summary>
        /// Set text rendering mode.
        /// </summary>
        Tr,

        /// <summary>
        /// Set text rise.
        /// </summary>
        Ts,

        /// <summary>
        /// Set word spacing.
        /// </summary>
        Tw,

        /// <summary>
        /// Set horizontal text scaling.
        /// </summary>
        Tz,

        /// <summary>
        /// Append curved segment to path (initial point replicated).
        /// </summary>
        v,

        /// <summary>
        /// Set line width.
        /// </summary>
        w,

        /// <summary>
        /// Set clipping path using non-zero winding number rule.
        /// </summary>
        W,

        /// <summary>
        /// Set clipping path using even-odd rule.
        /// </summary>
        Wx,  // W*

        /// <summary>
        /// Append curved segment to path (final point replicated).
        /// </summary>
        y,

        /// <summary>
        /// Move to next line and show text.
        /// </summary>
        QuoteSingle, // '

        /// <summary>
        /// Set word and character spacing, move to next line, and show text.
        /// </summary>
        QuoteDouble,  // "

        /// <summary>
        /// Set word and character spacing, move to next line, and show text.
        /// </summary>
        [Obsolete($"Use '{nameof(QuoteDouble)}'.")]
        QuoteDbl = QuoteDouble,
    }
}
