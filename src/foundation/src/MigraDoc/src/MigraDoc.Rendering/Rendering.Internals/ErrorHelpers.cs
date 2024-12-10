// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering.Internals
{
    /// <summary>
    /// </summary>
    // ReSharper disable once InconsistentNaming
    static class TH // RENAME TODO_OLD 
    {
        #region Renderer Messages

        public static InvalidOperationException InvalidOperationException_DocumentOfRendererHasToBeSet() =>
            new("The document of this renderer has to be set first.");

        #endregion
    }
}