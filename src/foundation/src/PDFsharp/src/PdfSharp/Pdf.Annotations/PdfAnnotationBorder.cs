﻿using PdfSharp.Pdf.Annotations.enums;

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Specifies the characteristics of an <see cref="PdfAnnotation"/>'s border
    /// </summary>
    public class PdfAnnotationBorder
    {
        /// <summary>
        /// The width of the border in points
        /// </summary>
        public double Width { get; set; } = 1;

        /// <summary>
        /// Horizontal radius of the border
        /// </summary>
        public double HorizontalRadius { get; set; } = 0;

        /// <summary>
        /// Vertical radius of the border
        /// </summary>
        public double VerticalRadius { get; set; } = 0;

        /// <summary>
        /// The border-style
        /// </summary>
        public PdfAnnotationBorderStyle BorderStyle { get; set; } = PdfAnnotationBorderStyle.Solid;

        /// <summary>
        /// A dash array defining a pattern of dashes and gaps that shall be used in drawing a dashed border
        /// </summary>
        public int[]? DashPattern { get; set; }
    }
}
