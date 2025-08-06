// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// MigraDoc PDF renderer message id.
    /// </summary>
    // ReSharper disable InconsistentNaming
    enum MdPdfMsgId
    {
        PropertyNotSetBefore = MessageIdOffset.MdPdf,
        BookmarkNotDefined,
        ImageNotFound,
        InvalidImageType,
        ImageNotReadable,
        EmptyImageSize,
        ObjectNotRenderable,
        NumberTooLargeForRoman,
        NumberTooLargeForLetters,
        DisplayEmptyImageSize,
        DisplayImageFileNotFound,
        DisplayInvalidImageType,
        DisplayImageNotRead
    }

    // ReSharper restore InconsistentNaming

    /// <summary>
    /// MigraDoc PDF renderer messages.
    /// Provides diagnostic messages taken from the resources.
    /// </summary>
    // ReSharper disable once IdentifierTypo
    static class MdPdfMsgs
    {
        internal static MdPdfMsg NumberTooLargeForRoman(int number)
            => new(MdPdfMsgId.NumberTooLargeForRoman, Invariant($"The number {number} is to large to be displayed as roman number."));

        internal static MdPdfMsg NumberTooLargeForLetters(int number)
            => new(MdPdfMsgId.NumberTooLargeForLetters, $"The number {number} is to large to be displayed as letters.");

        internal static MdPdfMsg DisplayEmptyImageSize
            => new(MdPdfMsgId.DisplayEmptyImageSize, "Image has empty size.");

        internal static MdPdfMsg DisplayImageFileNotFound(string fileName)
            => new(MdPdfMsgId.DisplayImageFileNotFound, $"Image '{fileName}' not found.");

        internal static MdPdfMsg DisplayInvalidImageType
            => new(MdPdfMsgId.DisplayInvalidImageType, "Image has no valid type.");

        internal static MdPdfMsg DisplayImageNotRead
            => new(MdPdfMsgId.DisplayImageNotRead, "Image could not be read.");

        internal static MdPdfMsg PropertyNotSetBefore(string propertyName, string functionName)
            => new(MdPdfMsgId.PropertyNotSetBefore, $"'{propertyName}' must be set before calling '{functionName}'.");

        internal static MdPdfMsg BookmarkNotDefined(string bookmarkName)
            => new(MdPdfMsgId.BookmarkNotDefined, $"Bookmark '{bookmarkName}' is not defined within the document.");

        internal static MdPdfMsg ImageNotFound(string imageName)
            => new(MdPdfMsgId.ImageNotFound, $"Image '{imageName}' not found.");

        internal static MdPdfMsg InvalidImageType(string type)
            => new(MdPdfMsgId.InvalidImageType, $"Invalid image type: '{type}'.");

        internal static MdPdfMsg ImageNotReadable(string imageName, string innerException)
            => new(MdPdfMsgId.ImageNotReadable,
                $"Image '{imageName}' could not be read.\\n Inner exception: {innerException}");

        internal static MdPdfMsg EmptyImageSize
            => new(MdPdfMsgId.EmptyImageSize, "The specified image size is empty.");

        internal static MdPdfMsg ObjectNotRenderable(string typeName)
            => new(MdPdfMsgId.ObjectNotRenderable, "Only images, text-frames, charts and paragraphs can be rendered freely.");
    }
}
