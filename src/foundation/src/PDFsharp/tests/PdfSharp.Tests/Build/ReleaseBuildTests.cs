// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests.Build
{
    [Collection("PDFsharp")]
    public class ReleaseBuildTests
    {
        [Fact]
        public void Check_renamed_identifiers()
        {
            // Check to undo some temporary renames.
            const string automatic = nameof(PdfFontEmbedding.TryComputeSubset);
#if !DEBUG
            (!automatic.EndsWith("_")).Should().BeTrue("some identifiers must be re-renamed before release.");
#endif
            _ = automatic;
        }
    }
}
