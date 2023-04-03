// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Formatting information for a page break.
    /// </summary>
    sealed class PageBreakFormatInfo : FormatInfo
    {
        internal override bool EndingIsComplete => true;

        internal override bool IsComplete => true;

        internal override bool IsEmpty => false;

        internal override bool IsEnding => true;

        internal override bool IsStarting => true;

        internal override bool StartingIsComplete => true;
    }
}
