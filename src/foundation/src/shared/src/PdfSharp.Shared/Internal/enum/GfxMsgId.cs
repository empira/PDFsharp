// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable 1591 // Because this is preview code.

namespace PdfSharp.Internal
{
    /// <summary>
    /// System message ID.
    /// </summary>
    enum GfxMsgId
    {
        None = 0,

        // ----- General messages ------------------------------------------------------------------

        IndexOutOfRange = MessageIdOffset.Gfx,
        IndexOutOfRange2,


        UnexpectedNullValueRetrieved = MessageIdOffset.Sy + 10,

        // ----- PDF object model ------------------------------------------------------------------

        #region new message ids, TODO review before release

        ToBeDone = MessageIdOffset.Om + 1,


        #endregion
    }
}
