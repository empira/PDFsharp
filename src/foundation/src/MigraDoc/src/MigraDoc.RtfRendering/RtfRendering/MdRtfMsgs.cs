// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal;

namespace MigraDoc.RtfRendering
{
    enum MdRtfMsgId
    {
        None = 0,

        UpdateField = MessageIdOffset.MdRtf,
        TextFrameContentsNotTurned,
        InvalidNumericFieldFormat,
        ImageFreelyPlacedInWrongContext,
        ChartFreelyPlacedInWrongContext,
        ImageNotFound,
        ImageNotReadable,
        ImageTypeNotSupported,
        CharacterNotAllowedInDateFormat
    }

    /// <summary>
    /// Provides diagnostic messages taken from the resources.
    /// </summary>
    // ReSharper disable once IdentifierTypo
    class MdRtfMsgs
    {
        internal static MdRtfMsg TextFrameContentsNotTurned
            => new(MdRtfMsgId.TextFrameContentsNotTurned, "Text-frame contents could not be turned. Only paragraphs can be turned within text-frames.");

        internal static MdRtfMsg ImageFreelyPlacedInWrongContext(string imageName)
            => new(MdRtfMsgId.ImageFreelyPlacedInWrongContext, $"Images can be placed freely only within headers, footers and sections. Image {imageName} will be ignored.");

        internal static MdRtfMsg ChartFreelyPlacedInWrongContext
            => new(MdRtfMsgId.ChartFreelyPlacedInWrongContext, "Chart is being ignored. A chart can be placed freely only within headers, footers and sections.");

        internal static MdRtfMsg ImageNotFound(string imageName)
            => new(MdRtfMsgId.ImageNotFound, $"Image '{imageName}' could not be found.");

        internal static MdRtfMsg ImageNotReadable(string imageName, string innerException)
            => new(MdRtfMsgId.ImageNotReadable, $"Image '{imageName}' could not be read. Inner Exception:\r\n{innerException}.");

        internal static MdRtfMsg ImageTypeNotSupported(string imageName)
            => new(MdRtfMsgId.ImageTypeNotSupported, $"Type of image '{imageName}' is not supported.");

        internal static MdRtfMsg InvalidNumericFieldFormat(string format)
            => new(MdRtfMsgId.InvalidNumericFieldFormat, $"'{format}' is not a valid format for a numeric field and will be ignored.");

        internal static MdRtfMsg CharacterNotAllowedInDateFormat(char character)
            => new(MdRtfMsgId.CharacterNotAllowedInDateFormat, $"The character '{character}' is not allowed in a date field’s format string and will be ignored.");

        internal static MdRtfMsg UpdateField
            => new(MdRtfMsgId.UpdateField, "< Please update this field. >");

        // ReSharper disable InconsistentNaming
    }
}
