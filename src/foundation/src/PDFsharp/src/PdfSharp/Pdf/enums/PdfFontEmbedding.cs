// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Specifies the embedding options of an XFont when converted into PDF.
    /// Font embedding is not optional anymore. So Always is the only option.
    /// </summary>
    public enum PdfFontEmbedding
    {
        /// <summary>
        /// OpenType font programs with TrueType outline are embedded as a font subset.
        /// OpenType font programs with PostScript outline are embedded as they are.
        /// </summary>
        Automatic,
        
        /// <summary>
        /// All fonts are embedded as they are.
        /// </summary>
        Always_,

        /// <summary>
        /// Fonts are not embedded. This is not an option anymore.
        /// Treated as Default.
        /// </summary>
        [Obsolete("Fonts must always be embedded. Treated as Automatic.")]
        None,

        /// <summary>
        /// Not yet implemented. Treated as Default.
        /// </summary>
        [Obsolete("Treated as Automatic.")]
        Default_
    }
}
