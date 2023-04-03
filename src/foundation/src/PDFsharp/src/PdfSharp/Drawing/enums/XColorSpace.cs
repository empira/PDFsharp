// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing
{
    ///<summary>
    /// Currently not used. Only DeviceRGB is rendered in PDF.
    /// </summary>
    public enum XColorSpace
    {
        /// <summary>
        /// Identifies the RGB color space.
        /// </summary>
        Rgb,

        /// <summary>
        /// Identifies the CMYK color space.
        /// </summary>
        Cmyk,

        /// <summary>
        /// Identifies the gray scale color space.
        /// </summary>
        GrayScale,
    }
}
