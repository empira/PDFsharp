namespace PdfSharp.Pdf.Annotations.enums
{
    /// <summary>
    /// The annotation's highlighting mode, the visual effect to be used when
    /// the mouse button is pressed or held down inside its active area
    /// </summary>
    public enum PdfAnnotationHighlightingMode
    {
        /// <summary>
        /// No highlighting
        /// </summary>
        None,
        /// <summary>
        /// Invert the contents of the annotation rectangle
        /// </summary>
        Invert,
        /// <summary>
        /// Invert the annotation's border
        /// </summary>
        Outline,
        /// <summary>
        /// Display the annotation's down appearance, if any
        /// </summary>
        Push,
        /// <summary>
        /// Same as <see cref="Push"/> (which is preferred)
        /// </summary>
        Toggle
    }
}
