// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// v7.0.0 TODO

namespace PdfSharp.Pdf.Content.Objects
{
    /// <summary>
    /// Specifies the group of operations the op-code belongs to.
    /// </summary>
    [Flags]
    public enum OpCodeFlags
    {
        /// <summary>
        /// 
        /// </summary>
        None,

        /// <summary>
        /// 
        /// </summary>
        TextOut = 0x0001,

        /// <summary>
        /// BI, ID, or EI.
        /// </summary>
        InlineImage = 0x0002,

        //Color, Pattern, Images,...
    }
}
