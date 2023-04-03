// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Specifies whether to compress JPEG images with the FlateDecode filter.
    /// </summary>
    public enum PdfUseFlateDecoderForJpegImages
    {
        /// <summary>
        /// PDFsharp will try FlateDecode and use it if it leads to a reduction in PDF file size.
        /// When FlateEncodeMode is set to BestCompression, this is more likely to reduce the file size,
        /// but it takes considerably more time to create the PDF file.
        /// </summary>
        Automatic,

        /// <summary>
        /// PDFsharp will never use FlateDecode - files may be a few bytes larger, but file creation is faster.
        /// </summary>
        Never,

        /// <summary>
        /// PDFsharp will always use FlateDecode, even if this leads to larger files;
        /// this option is meant for testing purposes only and should not be used for production code.
        /// </summary>
        Always,
    }
}