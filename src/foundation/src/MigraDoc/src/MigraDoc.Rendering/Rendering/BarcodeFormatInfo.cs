// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using PdfSharp.Drawing;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Formatting information for an barcode.
    /// </summary>
    internal class BarcodeFormatInfo : ShapeFormatInfo
    {
        internal BarcodeFormatInfo()
        {
        }

        internal XUnit Width;
        internal XUnit Height;
    }
}
