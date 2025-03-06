// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Fonts.StandardFonts;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Specifies the embedding options of an XFont when converted into PDF.
    /// Font embedding is not optional anymore.
    /// So TryComputeSubset and EmbedCompleteFontFile are the only options.
    /// </summary>
    public enum PdfFontEmbedding
    {
        /// <summary>
        /// OpenType font files with TrueType outline are embedded as a font subset.
        /// OpenType font files with PostScript outline are embedded as they are,
        /// because PDFsharp cannot compute subsets from this type of font files.
        /// </summary>
        TryComputeSubset = 0,

        /// <summary>
        /// Use TryComputeSubset.
        /// </summary>
        [Obsolete("Renamed to TryComputeSubset.")]
        Automatic = 0,

        /// <summary>
        /// The font file is completely embedded. No subset is computed.
        /// </summary>
        EmbedCompleteFontFile = 1,

        /// <summary>
        /// Use EmbedCompleteFontFile.
        /// </summary>
        [Obsolete("Renamed to EmbedCompleteFontFile.")]
        Always = 1,

        /// <summary>
        /// No embedding if the font is one of the 14 standard-fonts defined in the PDF-specification. (PDF 1.0 to PDF 1.7)<br></br>
        /// If the font is not one of the standard-fonts, an exception is raised when creating an <see cref="XFont"/> and specifying this value.<br></br>
        /// See also <see cref="StandardFontNames"/>
        /// </summary>
        /// <remarks>
        /// This value should not be used, unless you prefer small files over PDF/A compliance.
        /// </remarks>
        OmitStandardFont = 2,

        /// <summary>
        /// Fonts are not embedded. This is not an option anymore.
        /// Treated as Automatic.
        /// </summary>
        [Obsolete("Fonts must always be embedded. Treated as Automatic.")]
        None = 0,

        /// <summary>
        /// Not yet implemented. Treated as Default.
        /// </summary>
        [Obsolete("Treated as Automatic.")]
        Default = 0
    }
}
