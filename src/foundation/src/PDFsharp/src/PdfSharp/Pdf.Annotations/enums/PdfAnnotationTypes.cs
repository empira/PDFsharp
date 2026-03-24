// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// v7.0.0 Ready

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Specifies the annotation types.
    /// </summary>
    public enum PdfAnnotationTypes
    {
        // Reference 2.0: 12.5.6  Annotation types / Page 476
        // Reference 2.0: Table 171 — Annotation types / Page 476

        /// <summary>
        /// Text annotation.
        /// Markup: Yes
        /// </summary>
        Text = 1,

        /// <summary>
        /// Link annotation.
        /// Markup: No
        /// </summary>
        Link,

        /// <summary>
        /// (PDF 1.3) Free text annotation.
        /// Markup: Yes
        /// </summary>
        FreeText,

        /// <summary>
        /// (PDF 1.3) Line annotation.
        /// Markup: Yes
        /// </summary>
        Line,

        /// <summary>
        /// (PDF 1.3) Square annotation.
        /// Markup: Yes
        /// </summary>
        Square,

        /// <summary>
        /// (PDF 1.3) Circle annotation.
        /// Markup: Yes
        /// </summary>
        Circle,

        /// <summary>
        /// (PDF 1.5) Polygon annotation.
        /// Markup: Yes
        /// </summary>
        Polygon,

        /// <summary>
        /// (PDF 1.5) Polyline annotation.
        /// Markup: Yes
        /// </summary>
        PolyLine,

        /// <summary>
        /// (PDF 1.3) Highlight annotation.
        /// Markup: Yes
        /// </summary>
        Highlight,

        /// <summary>
        /// (PDF 1.3) Underline annotation.
        /// Markup: Yes
        /// </summary>
        Underline,

        /// <summary>
        /// (PDF 1.4) Squiggly-underline annotation.
        /// Markup: Yes
        /// </summary>
        Squiggly,

        /// <summary>
        /// (PDF 1.3) Strikeout annotation..
        /// Markup: Yes
        /// </summary>
        StrikeOut,

        /// <summary>
        /// (PDF 1.5) Caret annotation.
        /// Markup: Yes
        /// </summary>
        Caret,

        /// <summary>
        /// (PDF 1.3) Rubber stamp annotation.
        /// Markup: Yes
        /// </summary>
        [Obsolete("Use Stamp instead.")]
        RubberStamp,

        /// <summary>
        /// (PDF 1.3) Rubber stamp annotation.
        /// Markup: Yes
        /// </summary>
        Stamp,

        /// <summary>
        /// (PDF 1.3) Ink annotation.
        /// Markup: Yes
        /// </summary>
        Ink,

        /// <summary>
        /// (PDF 1.3) Popup annotation.
        /// Markup: No
        /// </summary>
        Popup,

        /// <summary>
        /// (PDF 1.3) File attachment annotation.
        /// Markup: Yes
        /// </summary>
        FileAttachment,

        /// <summary>
        /// (PDF 1.2; deprecated in PDF 2.0) Sound annotation.
        /// Markup: Yes
        /// </summary>
        Sound,

        /// <summary>
        /// (PDF 1.2; deprecated in PDF 2.0) Movie annotation.
        /// Markup: No
        /// </summary>
        Movie,

        /// <summary>
        /// (PDF 1.5) Screen annotation.
        /// Markup: No
        /// </summary>
        Screen,

        /// <summary>
        /// (PDF 1.2) Widget annotation.
        /// Markup: No
        /// </summary>
        Widget,

        /// <summary>
        /// (PDF 1.4) Printer’s mark annotation.
        /// Markup: No
        /// </summary>
        PrinterMark,

        /// <summary>
        /// (PDF 1.3; deprecated in PDF 2.0) Trap network annotation.
        /// Markup: No
        /// </summary>
        TrapNet,

        /// <summary>
        /// (PDF 1.6) Watermark annotation.
        /// Markup: No
        /// </summary>
        Watermark,

        /// <summary>
        /// (PDF 1.6) 3D annotation.
        /// Markup: No
        /// </summary>
        ThreeD, // Use '3D', not 'nameof(ThreeD)'.

        /// <summary>
        /// (PDF 1.7) Redact annotation.
        /// Markup: Yes
        /// </summary>
        Redact,

        /// <summary>
        /// (PDF 2.0) Projection annotation.
        /// Markup: Yes
        /// </summary>
        Projection,

        /// <summary>
        /// (PDF 2.0) RichMedia annotation.
        /// Markup: No
        /// </summary>
        RichMedia
    }
}

namespace PdfSharp.Pdf.Annotations
{
#pragma warning disable CS0414 // Field is assigned but its value is never used  // DELETE

    /// <summary>
    /// Specifies the annotation types.
    /// </summary>
    public static class PdfAnnotationTypeNames
    {
        // Reference 2.0: 12.5.6  Annotation types / Page 476
        // Reference 2.0: Table 171 — Annotation types / Page 476

        /// <summary>
        /// Text annotation.
        /// Markup: Yes
        /// </summary>
        public const string Text = "/" + nameof(PdfAnnotationTypes.Text);

        /// <summary>
        /// Link annotation.
        /// Markup: No
        /// </summary>
        public const string Link = "/" + nameof(PdfAnnotationTypes.Link);

        /// <summary>
        /// (PDF 1.3) Free text annotation.
        /// Markup: Yes
        /// </summary>
        public const string FreeText = "/" + nameof(PdfAnnotationTypes.FreeText);

        /// <summary>
        /// (PDF 1.3) Line annotation.
        /// Markup: Yes
        /// </summary>
        public const string Line = "/" + nameof(PdfAnnotationTypes.Line);

        /// <summary>
        /// (PDF 1.3) Square annotation.
        /// Markup: Yes
        /// </summary>
        public const string Square = "/" + nameof(PdfAnnotationTypes.Square);

        /// <summary>
        /// (PDF 1.3) Circle annotation.
        /// Markup: Yes
        /// </summary>
        public const string Circle = "/" + nameof(PdfAnnotationTypes.Circle);

        /// <summary>
        /// (PDF 1.5) Polygon annotation.
        /// Markup: Yes
        /// </summary>
        public const string Polygon = "/" + nameof(PdfAnnotationTypes.Polygon);

        /// <summary>
        /// (PDF 1.5) Polyline annotation.
        /// Markup: Yes
        /// </summary>
        public const string PolyLine = "/" + nameof(PdfAnnotationTypes.PolyLine);

        /// <summary>
        /// (PDF 1.3) Highlight annotation.
        /// Markup: Yes
        /// </summary>
        public const string Highlight = "/" + nameof(PdfAnnotationTypes.Highlight);

        /// <summary>
        /// (PDF 1.3) Underline annotation.
        /// Markup: Yes
        /// </summary>
        public const string Underline = "/" + nameof(PdfAnnotationTypes.Underline);

        /// <summary>
        /// (PDF 1.4) Squiggly-underline annotation.
        /// Markup: Yes
        /// </summary>
        public const string Squiggly = "/" + nameof(PdfAnnotationTypes.Squiggly);

        /// <summary>
        /// (PDF 1.3) Strikeout annotation..
        /// Markup: Yes
        /// </summary>
        public const string StrikeOut = "/" + nameof(PdfAnnotationTypes.StrikeOut);

        /// <summary>
        /// (PDF 1.5) Caret annotation.
        /// Markup: Yes
        /// </summary>
        public const string Caret = "/" + nameof(PdfAnnotationTypes.Caret);

        /// <summary>
        /// (PDF 1.3) Rubber stamp annotation.
        /// Markup: Yes
        /// </summary>
        public const string Stamp = "/" + nameof(PdfAnnotationTypes.Stamp);

        /// <summary>
        /// (PDF 1.3) Ink annotation.
        /// Markup: Yes
        /// </summary>
        public const string Ink = "/" + nameof(PdfAnnotationTypes.Ink);

        /// <summary>
        /// (PDF 1.3) Popup annotation.
        /// Markup: No
        /// </summary>
        public const string Popup = "/" + nameof(PdfAnnotationTypes.Popup);

        /// <summary>
        /// (PDF 1.3) File attachment annotation.
        /// Markup: Yes
        /// </summary>
        public const string FileAttachment = "/" + nameof(PdfAnnotationTypes.FileAttachment);

        /// <summary>
        /// (PDF 1.2; deprecated in PDF 2.0) Sound annotation.
        /// Markup: Yes
        /// </summary>
        public const string Sound = "/" + nameof(PdfAnnotationTypes.Sound);

        /// <summary>
        /// (PDF 1.2; deprecated in PDF 2.0) Movie annotation.
        /// Markup: No
        /// </summary>
        public const string Movie = "/" + nameof(PdfAnnotationTypes.Movie);

        /// <summary>
        /// (PDF 1.5) Screen annotation.
        /// Markup: No
        /// </summary>
        public const string Screen = "/" + nameof(PdfAnnotationTypes.Screen);

        /// <summary>
        /// (PDF 1.2) Widget annotation.
        /// Markup: No
        /// </summary>
        public const string Widget = "/" + nameof(PdfAnnotationTypes.Widget);

        /// <summary>
        /// (PDF 1.4) Printer’s mark annotation.
        /// Markup: No
        /// </summary>
        public const string PrinterMark = "/" + nameof(PdfAnnotationTypes.PrinterMark);

        /// <summary>
        /// (PDF 1.3; deprecated in PDF 2.0) Trap network annotation.
        /// Markup: No
        /// </summary>
        public const string TrapNet = "/" + nameof(PdfAnnotationTypes.TrapNet);

        /// <summary>
        /// (PDF 1.6) Watermark annotation.
        /// Markup: No
        /// </summary>
        public const string Watermark = "/" + nameof(PdfAnnotationTypes.Watermark);

        /// <summary>
        /// (PDF 1.6) 3D annotation.
        /// Markup: No
        /// </summary>
        public const string ThreeD = "/3D";

        /// <summary>
        /// (PDF 1.7) Redact annotation.
        /// Markup: Yes
        /// </summary>
        public const string Redact = "/" + nameof(PdfAnnotationTypes.Redact);

        /// <summary>
        /// (PDF 2.0) Projection annotation.
        /// Markup: Yes
        /// </summary>
        public const string Projection = "/" + nameof(PdfAnnotationTypes.Projection);

        /// <summary>
        /// (PDF 2.0) RichMedia annotation.
        /// Markup: No
        /// </summary>
        public const string RichMedia = "/" + nameof(PdfAnnotationTypes.RichMedia);
    }
}
