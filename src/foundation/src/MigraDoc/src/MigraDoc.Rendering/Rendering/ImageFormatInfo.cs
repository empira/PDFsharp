// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Formatting information for an image.
    /// </summary>
    sealed class ImageFormatInfo : ShapeFormatInfo
    {
        internal int CropX;
        internal int CropY;
        internal int CropWidth;
        internal int CropHeight;
        internal XUnit Width;
        internal XUnit Height;

        internal ImageFailure Failure;
        internal string ImagePath = null!;
    }
}
