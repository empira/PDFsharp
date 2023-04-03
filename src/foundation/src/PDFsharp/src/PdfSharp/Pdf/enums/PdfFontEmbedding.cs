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
        /// All fonts are embedded.
        /// </summary>
        Always,

        /// <summary>
        /// Fonts are not embedded. This is not an option anymore.
        /// </summary>
        [Obsolete("Fonts must always be embedded.")]
        None,

        /// <summary>
        /// Unicode fonts are embedded, WinAnsi fonts are not embedded.
        /// </summary>
        [Obsolete("Fonts must always be embedded.")]
        Default,

        /// <summary>
        /// Not yet implemented.
        /// </summary>
        [Obsolete("Fonts must always be embedded.")]
        Automatic,
    }
}
