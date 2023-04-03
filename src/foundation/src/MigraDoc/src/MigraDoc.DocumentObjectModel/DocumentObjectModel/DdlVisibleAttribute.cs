// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
#if true_ // Do not delete.
    /// <summary>
    /// Under Construction. NOT YET USED.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    class DdlVisibleAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the DdlVisibleAttribute class.
        /// </summary>
        public DdlVisibleAttribute() => Visible = true;

        /// <summary>
        /// Initializes a new instance of the DdlVisibleAttribute class with the specified visibility.
        /// </summary>
        public DdlVisibleAttribute(bool visible) => Visible = visible;

        /// <summary>
        /// Gets or sets the visibility.
        /// </summary>
        public bool Visible { get; set; }

        public bool CanAddValue { get; set; }

        public bool CanRemoveValue { get; set; }
    }
#endif
}
