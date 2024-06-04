// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
namespace PdfSharp.Forms
{
    /// <summary>
    /// Specifies how to reander the preview.
    /// </summary>
    public enum RenderMode
    {
        /// <summary>
        /// Draw immediately.
        /// </summary>
        Direct = 0,

        /// <summary>
        /// Draw using a metafile.
        /// </summary>
        Metafile = 1,

        /// <summary>
        /// Draw using a bitmap image.
        /// </summary>
        Bitmap = 2
    }
}
#endif
