// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Pdf;
using PdfSharp.Quality.Testing;
using Xunit;

namespace PdfSharp.Tests.Pdf.Objects
{
    [Collection("PDFsharp")]
    public class PdfCatalogTests : PdfSharpTestBase
    {
        public PdfCatalogTests()
        { }

        protected override void Dispose(bool _)
        { }

        [Fact]
        public void Test_Catalog_entries()
        {
            var doc = new PdfDocument();

            var catalog = doc.Catalog;
            catalog.HasNames.Should().BeFalse();

            catalog.HasMetadata.Should().BeFalse();
            var metadata = catalog.GetOrCreateMetadata();
            metadata.Should().NotBeNull();
            metadata.IsIndirect.Should().BeTrue();

            catalog.HasNames.Should().BeFalse();
            var names = catalog.Names;
            names.Should().NotBeNull();
            names.IsIndirect.Should().BeTrue();
        }
    }
}
