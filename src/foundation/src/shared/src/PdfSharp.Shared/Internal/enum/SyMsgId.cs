// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable 1591 // Because this is preview code.

namespace PdfSharp.Internal
{
    enum FooBarEnum3 { xxx = 843 }

    /// <summary>
    /// System message ID.
    /// </summary>
    /// <remarks>
    /// The enum value should be fixed over time, but the enum name
    /// must be stable in the future. This is because the name is
    /// the link to any kind of outer resources, e.g. an entry in
    /// the PDFsharp technical reference.
    /// </remarks>
    enum SyMsgId
    {
        None = 0,
        ToDo = 0,

        // ----- General messages ------------------------------------------------------------------

        IndexOutOfRange = MessageIdOffset.Sy,
        IndexOutOfRange2,


        UnexpectedNullValueRetrieved = MessageIdOffset.Sy + 10,

        // ----- PDF object model ------------------------------------------------------------------

        #region new message ids, TODO review before release

        IndirectReferenceMustNotBeNull = MessageIdOffset.Om + 1,

        ObjectWithoutOwner,

        ArrayEntryIsOfWrongType,

        DictionaryEntryDoesNotExist,

        DictionaryEntryDoesNotExistAndNoDefaultSpecified,

        DictionaryEntryIsOfWrongType,

        IndirectObjectExpected,
        IndirectObjectExpectedButDirectObjectFound,

        DirectObjectExpected,
        DirectObjectExpectedButIndirectObjectFound,


        CannotConvertArrayIntoDictionary = MessageIdOffset.Om + 100,

        CannotConvertDictionaryIntoArray,

        #endregion
    }

    /// <summary>
    /// Offsets to ensure that all message IDs are pairwise distinct
    /// within PDFsharp foundation.
    /// </summary>
    public enum MessageIdOffset
    {
        /// <summary>
        /// General system messages.
        /// </summary>
        Sy = 1000,

        /// <summary>
        /// PDFsharp object model messages
        /// </summary>
        Om = 2000,

        /// <summary>
        /// General PDFsharp messages.
        /// </summary>
        Ps = 3000,

        /// <summary>
        /// PDFsharp cryptography messages.
        /// </summary>
        PsCrypto = 4000,

        /// <summary>
        /// MigraDoc document object model messages.
        /// </summary>
        MdDom = 5000,

        /// <summary>
        /// MigraDoc PDF renderer messages.
        /// </summary>
        MdPdf = 6000,

        /// <summary>
        /// MigraDoc RTF renderer messages.
        /// </summary>
        MdRtf = 7000,

        /// <summary>
        /// PDFsharp Graphics messages.
        /// </summary>
        Gfx = 10_000,
    }
}
