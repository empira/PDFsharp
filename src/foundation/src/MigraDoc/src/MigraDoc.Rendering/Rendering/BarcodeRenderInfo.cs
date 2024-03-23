// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Represents rendering information for barcodes.
    /// </summary>
    internal class BarcodeRenderInfo : ShapeRenderInfo
    {
        public BarcodeRenderInfo()
        {
        }

        public override FormatInfo FormatInfo
        {
            get => _formatInfo;
            internal set => _formatInfo = (BarcodeFormatInfo)value;
        }
        BarcodeFormatInfo _formatInfo = new();
    }
}
