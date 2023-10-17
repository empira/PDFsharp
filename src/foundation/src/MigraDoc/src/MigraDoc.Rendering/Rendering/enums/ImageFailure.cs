// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering
{
    enum ImageFailure
    {
        /// <summary>
        /// No failure has occurred.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Image file was not found.
        /// </summary>
        FileNotFound,

        /// <summary>
        /// Image type is not supported.
        /// </summary>
        InvalidType,

        /// <summary>
        /// Image could not be read.
        /// </summary>
        NotRead,

        /// <summary>
        /// Image has empty or invalid size.
        /// </summary>
        EmptySize
    }
}
