
namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Specifies the option for rendering text.
    /// </summary>
    public enum TextRenderOption
    {
        /// <summary>
        /// Text will be rendered as text.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Text will be rendered as path.
        /// </summary>
        Path,

        /// <summary>
        /// Text will be rendered as path and the path will be flattened.
        /// The size will be reduced by flattening the path.
        /// </summary>
        FlattenPath
    }
}