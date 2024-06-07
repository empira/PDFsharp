// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// ReSharper disable once CheckNamespace
namespace PdfSharp.BigGustave
{
    /// <summary>
    /// Indicates the transmission order of the image data.
    /// </summary>
    public enum InterlaceMethod : byte
    {
        /// <summary>
        /// No interlace.
        /// </summary>
        None = 0,
        /// <summary>
        /// Adam7 interlace.
        /// </summary>
        Adam7 = 1
    }
}