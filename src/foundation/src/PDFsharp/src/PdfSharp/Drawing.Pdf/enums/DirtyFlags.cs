// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace PdfSharp.Drawing.Pdf
{
    [Flags]
    enum DirtyFlags
    {
        Ctm = 0x00000001,
        ClipPath = 0x00000002,
        LineWidth = 0x00000010,
        LineJoin = 0x00000020,
        MiterLimit = 0x00000040,
        StrokeFill = 0x00000070,
    }
}
