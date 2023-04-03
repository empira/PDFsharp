// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Specifies the format of a text.
    /// Bold, Italic, or Underline will be ignored if NotBold, NotItalic, or NoUnderline respectively are specified at the same time.
    /// </summary>
    [Flags]
    public enum TextFormat
    {
        /// <summary>
        /// Specifies bold text (heavy font weight).
        /// </summary>
        Bold = 0x000001,

        /// <summary>
        /// Specifies normal font weight.
        /// </summary>
        NotBold = 0x000003,

        /// <summary>
        /// Specifies italic text.
        /// </summary>
        Italic = 0x000004,

        /// <summary>
        /// Specifies upright text.
        /// </summary>
        NotItalic = 0x00000C,

        /// <summary>
        /// Specifies underlined text.
        /// </summary>
        Underline = 0x000010,

        /// <summary>
        /// Specifies text without underline.
        /// </summary>
        NoUnderline = 0x000030
    }
}
