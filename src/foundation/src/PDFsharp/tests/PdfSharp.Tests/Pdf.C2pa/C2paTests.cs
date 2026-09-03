// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using PdfSharp.Diagnostics;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Attachments;
using PdfSharp.Pdf.C2pa;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using Xunit;
using FluentAssertions;

namespace PdfSharp.Tests.Pdf.C2pa
{
    [Collection("PDFsharp")]
    public class C2paTests : IDisposable
    {
        public C2paTests()
        {
            PdfSharpCore.ResetAll();
#if CORE
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
#endif
        }

        public void Dispose()
        {
            PdfSharpCore.ResetAll();
        }

        static string DuckPdfPath => Path.Combine(AppContext.BaseDirectory, "Pdf.C2pa", "testdata", "duck.pdf");

        /// <summary>
        /// Builds a minimal document with an /EmbeddedFiles name tree entry whose file specification
        /// points to a stream that carries the embedded file parameters under the (wrong) /F key
        /// instead of /Params, reproducing the structure reported in issue #389.
        /// </summary>
        static PdfDocument BuildDocumentWithC2paManifest(byte[] manifestBytes)
        {
            var document = new PdfDocument();
            document.AddPage();

            var manifestStream = new PdfDictionary(document);
            document.Internals.AddObject(manifestStream);
            manifestStream.CreateStream(manifestBytes);
            // Misplaced key: a real embedded file stream would use /Params here, not /F.
            var badParams = new PdfDictionary(document);
            badParams.Elements.SetString("/Subtype", "application/c2pa");
            manifestStream.Elements.SetObject("/F", badParams);

            var ef = new PdfDictionary(document);
            ef.Elements.SetReference("/F", manifestStream.Reference!);

            var fileSpec = new PdfDictionary(document);
            document.Internals.AddObject(fileSpec);
            fileSpec.Elements.SetName("/Type", "/Filespec");
            fileSpec.Elements.SetString("/F", "Content Credentials");
            fileSpec.Elements.SetString("/UF", "Content Credentials");
            fileSpec.Elements.SetString("/Desc", "Content Credentials");
            fileSpec.Elements.SetString("/Subtype", "application/c2pa");
            fileSpec.Elements.SetName("/AFRelationship", "/" + PdfAFRelationship.C2paManifest);
            fileSpec.Elements.SetObject("/EF", ef);

            var embeddedFiles = document.Catalog.Names.GetEmbeddedFiles(true)!;
            embeddedFiles.AddName("Content Credentials", fileSpec.Reference!);

            var af = document.Catalog.Elements.GetRequiredArray<PdfArrayOfDictionaries>(PdfCatalog.Keys.AF, VCF.Create);
            af.AddDictionary(fileSpec);

            return document;
        }

        static PdfDocument SaveAndReopen(PdfDocument document, PdfDocumentOpenMode openMode)
        {
            using var stream = new MemoryStream();
            document.Save(stream);
            stream.Position = 0;
            return PdfReader.Open(stream, openMode);
        }

        [Fact]
        public void Open_document_with_C2PA_manifest()
        {
            var manifestBytes = Encoding.ASCII.GetBytes("dummy JUMBF content");
            using var document = BuildDocumentWithC2paManifest(manifestBytes);

            var action = () => SaveAndReopen(document, PdfDocumentOpenMode.Import);

            action.Should().NotThrow();
        }

        [Fact]
        public void Get_C2PA_manifest_info()
        {
            var manifestBytes = Encoding.ASCII.GetBytes("dummy JUMBF content");
            using var document = BuildDocumentWithC2paManifest(manifestBytes);
            using var reopened = SaveAndReopen(document, PdfDocumentOpenMode.Import);

            var c2pa = C2paManager.ForDocument(reopened);

            c2pa.HasManifests.Should().BeTrue();
            c2pa.ManifestCount.Should().Be(1);

            var info = c2pa.Manifests[0];
            info.NamesKey.Should().Be("Content Credentials");
            info.FileName.Should().Be("Content Credentials");
            info.Description.Should().Be("Content Credentials");
            info.MimeType.Should().Be("application/c2pa");
            info.AFRelationship.Should().Be(PdfAFRelationship.C2paManifest);
            info.Data.Should().Equal(manifestBytes);
        }

        [Fact]
        public void Get_C2PA_manifest_info_from_document_without_manifest()
        {
            using var document = new PdfDocument();
            document.AddPage();
            using var reopened = SaveAndReopen(document, PdfDocumentOpenMode.Import);

            var c2pa = C2paManager.ForDocument(reopened);

            c2pa.HasManifests.Should().BeFalse();
            c2pa.ManifestCount.Should().Be(0);
            c2pa.Manifests.Should().BeEmpty();
        }

        [Fact]
        public void Open_document_with_external_file_stream()
        {
            using var document = new PdfDocument();
            document.AddPage();

            var externalStream = new PdfDictionary(document);
            document.Internals.AddObject(externalStream);
            externalStream.CreateStream([1, 2, 3, 4]);

            var fileSpec = new PdfDictionary(document);
            fileSpec.Elements.SetName("/Type", "/Filespec");
            fileSpec.Elements.SetString("/F", "some/external/file.bin");
            externalStream.Elements.SetObject("/F", fileSpec);

            // Attach the stream to the catalog so it survives object compaction on save.
            document.Catalog.Elements.SetReference("/Test389ExternalFile", externalStream.Reference!);

            using var stream = new MemoryStream();
            document.Save(stream);
            stream.Position = 0;

            var action = () => PdfReader.Open(stream, PdfDocumentOpenMode.Import);

            action.Should().Throw<NotImplementedException>().WithMessage("File streams are not yet implemented.");
        }

        [Theory]
        [InlineData(PdfDocumentOpenMode.Import)]
        [InlineData(PdfDocumentOpenMode.Modify)]
        public void Open_duck_pdf(PdfDocumentOpenMode openMode)
        {
            var action = () => PdfReader.Open(DuckPdfPath, openMode);

            action.Should().NotThrow();
        }

        [Fact]
        public void Get_C2PA_manifest_info_from_duck_pdf()
        {
            using var document = PdfReader.Open(DuckPdfPath, PdfDocumentOpenMode.Import);

            var c2pa = C2paManager.ForDocument(document);

            c2pa.HasManifests.Should().BeTrue();
            c2pa.ManifestCount.Should().Be(1);

            var info = c2pa.Manifests[0];
            info.NamesKey.Should().Be("Content Credentials");
            info.FileName.Should().Be("Content Credentials");
            info.Description.Should().Be("Content Credentials");
            info.MimeType.Should().Be("application/c2pa");
            info.AFRelationship.Should().Be(PdfAFRelationship.C2paManifest);
            info.Data.Length.Should().Be(20166);

            // The manifest must be a valid JUMBF superbox: 4-byte big-endian length, followed by the "jumb" type.
            var length = (info.Data[0] << 24) | (info.Data[1] << 16) | (info.Data[2] << 8) | info.Data[3];
            length.Should().Be(info.Data.Length);
            Encoding.ASCII.GetString(info.Data, 4, 4).Should().Be("jumb");
        }
    }
}
