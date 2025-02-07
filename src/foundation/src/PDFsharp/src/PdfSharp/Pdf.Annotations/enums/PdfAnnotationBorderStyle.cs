﻿namespace PdfSharp.Pdf.Annotations.enums
{
    /// <summary>
    /// Specifies the border-style for a <see cref="PdfAnnotation"/>
    /// </summary>
    public enum PdfAnnotationBorderStyle
    {
        /// <summary>
        /// No border
        /// </summary>
        None,
        /// <summary>
        /// A solid rectangle surrounding the annotation.
        /// </summary>
        Solid,
        /// <summary>
        /// A dashed rectangle surrounding the annotation.
        /// The dash pattern may be specified by the D entry of the border-style dictionary.
        /// </summary>
        Dashed,
        /// <summary>
        /// A simulated embossed rectangle that appears to be raised above the surface of the page.
        /// </summary>
        Beveled,
        /// <summary>
        /// A simulated engraved rectangle that appears to be recessed below the surface of the page.
        /// </summary>
        Inset,
        /// <summary>
        /// A single line along the bottom of the annotation rectangle.
        /// </summary>
        Underline
    }
}
