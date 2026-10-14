// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Metadata;
using PdfSharp.Quality;
using Xunit;
using FluentAssertions;
using PdfSharp.Pdf.PdfA;

namespace PdfSharp.Tests.Pdf.Metadata
{
    [Collection("PDFsharp")]
    public class MetadataManagerTests : IDisposable
    {
        readonly string _tempRoot = PdfFileUtility.GetUnitTestPath(typeof(MetadataManagerTests));

        public MetadataManagerTests()
        {
            GlobalFontSettings.ResetFontManagement();
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
        }

        public void Dispose()
        { }

        [Fact]
        public void Create_document_with_no_metadata()
        {
            var doc = new PdfDocument();
            var page = doc.AddPage();

            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 10);
            gfx.DrawString($"{nameof(Create_document_with_no_metadata)}", font, XBrushes.Black, 10, 20);

            var dest = PdfFileUtility.GetTempPdfFullFileName(_tempRoot + nameof(Create_document_with_no_metadata));
            doc.Options.Layout = PdfWriterLayout.Verbose;
            doc.Save(dest);

            var doc2 = PdfReader.Open(dest);
            var mmd = doc2.Catalog.Elements.GetDictionary(PdfCatalog.Keys.Metadata);
            var b = doc2.Catalog.HasMetadata;
            doc2.Catalog.HasMetadata.Should().BeFalse();

            var dest2 = PdfFileUtility.GetTempPdfFullFileName(_tempRoot + nameof(Create_document_with_no_metadata) + "#2");
            doc2.Save(dest2);

            var doc3 = PdfReader.Open(dest2);
            doc3.Catalog.HasMetadata.Should().BeFalse();

            var xml = doc3.Catalog.GetOrCreateMetadata().ToString();
            xml.Length.Should().Be(0);
        }

        [Fact]
        public void Create_another_document_with_no_metadata()
        {
            var doc = new PdfDocument();
            var page = doc.AddPage();

            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 10);
            gfx.DrawString($"{nameof(Create_document_with_no_metadata)}", font, XBrushes.Black, 10, 20);

            var dest = PdfFileUtility.GetTempPdfFullFileName(_tempRoot + nameof(Create_document_with_no_metadata));
            doc.Options.Layout = PdfWriterLayout.Verbose;
            doc.Save(dest);
            doc.Catalog.HasMetadata.Should().BeFalse();

            var doc2 = PdfReader.Open(dest);
            doc2.Catalog.HasMetadata.Should().BeFalse();
            var mmd = doc2.Catalog.Elements.GetDictionary(PdfCatalog.Keys.Metadata);
            var b = doc2.Catalog.HasMetadata;
            doc2.Catalog.HasMetadata.Should().BeFalse();

            // HasMeta should be true after accessing Metadata. Empty Metadata must not be saved to PDF.
            var metadata2 = doc2.Catalog.GetOrCreateMetadata();
            doc2.Catalog.HasMetadata.Should().BeTrue();

            var dest2 = PdfFileUtility.GetTempPdfFullFileName(_tempRoot + nameof(Create_document_with_no_metadata) + "#2");
            doc2.Save(dest2);

            var doc3 = PdfReader.Open(dest2);
            // HasMeta should be false here because empty Metadata was not saved.
            doc3.Catalog.HasMetadata.Should().BeFalse();

            var xml = doc3.Catalog.GetOrCreateMetadata().ToString();
            xml.Length.Should().Be(0);
            doc3.Catalog.HasMetadata.Should().BeTrue();
        }

        [Fact]
        public void Create_document_with_metadata()
        {
            var doc = new PdfDocument();
            var page = doc.AddPage();

            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 10);
            gfx.DrawString($"{nameof(Create_document_with_metadata)}", font, XBrushes.Black, 10, 20);

            var metadataManager = MetadataManager.ForDocument(doc);
            metadataManager.Strategy = DocumentMetadataStrategy.AutoGenerate;

            var dest = PdfFileUtility.GetTempPdfFullFileName(_tempRoot + nameof(Create_document_with_metadata));
            doc.Options.Layout = PdfWriterLayout.Verbose;
            doc.Save(dest);

            var doc2 = PdfReader.Open(dest);
            doc2.Catalog.HasMetadata.Should().BeTrue();
            var metadata2 = doc2.Catalog.GetOrCreateMetadata();
            metadata2.ToString().Should().StartWith(@"<?xpacket begin=""");
        }

        [Fact]
        public void Create_PDF_A_document_with_metadata()
        {
            var doc = new PdfDocument();
            doc.SetPdfA(PdfAFormats.PdfA_2b);
            var page = doc.AddPage();

            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 10);
            gfx.DrawString($"{nameof(Create_PDF_A_document_with_metadata)}", font, XBrushes.Black, 10, 20);

            var metadataManager = MetadataManager.ForDocument(doc);
            metadataManager.Strategy = DocumentMetadataStrategy.AutoGenerate;

            var dest = PdfFileUtility.GetTempPdfFullFileName(_tempRoot + nameof(Create_PDF_A_document_with_metadata));
            doc.Options.Layout = PdfWriterLayout.Verbose;
            doc.Save(dest);

            var doc2 = PdfReader.Open(dest);
            doc2.Catalog.HasMetadata.Should().BeTrue();
            var metadata2 = doc2.Catalog.GetOrCreateMetadata();
            metadata2.ToString().Should().StartWith(@"<?xpacket begin=""");
        }

        [Fact]
        public void Create_document_with_user_generated_metadata()
        {
            var doc = new PdfDocument();
            var page = doc.AddPage();

            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 10);
            gfx.DrawString($"{nameof(Create_document_with_metadata)}", font, XBrushes.Black, 10, 20);

            var metadataManager = MetadataManager.ForDocument(doc);
            metadataManager.Strategy = DocumentMetadataStrategy.UserGenerated;
            doc.Events.CreateDocumentMetadata += (sender, args) =>
            {
                args.Metadata.SetMetadata(
                // Note that keeping BOM empty is the best choice.
                $"""
                 <?xpacket begin="" id="W5M0MpCehiHzreSzNTczkc9d"?>
                 <x:xmpmeta xmlns:x="adobe:ns:meta/">
                   <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
                 """ + "\r\n" +
                // Just test, no valid XMP
                """
                     <SomeElement>
                       <OtherElement>This is invalid XMP metadata</OtherElement>
                     </SomeElement>
                """ + "\r\n" +
                """
                   </rdf:RDF>
                 </x:xmpmeta>
                 <?xpacket end="w"?>
                 """);
            };

            var dest = PdfFileUtility.GetTempPdfFullFileName(_tempRoot + nameof(Create_document_with_metadata));
            doc.Options.Layout = PdfWriterLayout.Verbose;
            doc.Save(dest);

            var doc2 = PdfReader.Open(dest);
            doc2.Catalog.HasMetadata.Should().BeTrue();
            var metadata2 = doc2.Catalog.GetOrCreateMetadata();
            metadata2.ToString().Should().StartWith(@"<?xpacket begin=""");
        }
        
        [InlineData(MetadataEncodingType.UTF16BE)]  // Creates an invalid PDF file that is accepted by Acrobat.
        [InlineData(MetadataEncodingType.UTF32LE)]  // Creates an invalid PDF file that is not accepted by Acrobat.
        [InlineData(MetadataEncodingType.UTF32BE)]  // Creates an invalid PDF file that is not accepted by Acrobat.
        void ReadWrite_AutoCreate_metadata(MetadataEncodingType type)
        {
            var doc = new PdfDocument();
            doc.AddPage();
            SetSomeMetadata(doc);

            var mdManager = MetadataManager.ForDocument(doc);
            var metadata = doc.Catalog.GetOrCreateMetadata();

            var xml = metadata.ToString();
            xml.Should().BeEmpty();

            xml = metadata.CreateDefaultMetadata(type);
            metadata.SetMetadata(xml);
            xml = metadata.ToString();
            xml.Should().NotBeEmpty();

            var dest = PdfFileUtility.GetTempPdfFullFileName(_tempRoot + nameof(ReadWrite_AutoCreate_metadata) + $"-with-{type}");
            doc.Options.Layout = PdfWriterLayout.Compact;
            doc.Save(dest);

            var doc2 = PdfReader.Open(dest);
            var xml2 = doc2.Catalog.GetOrCreateMetadata().ToString();
            // ReSharper disable StringLiteralTypo
            xml2.Should().StartWith($"""<?xpacket begin="{type}" id="W5M0MpCehiHzreSzNTczkc9d"?>""");
            // ReSharper restore StringLiteralTypo
        }

        void SetSomeMetadata(PdfDocument doc)
        {
            var info = doc.Info;

            info.Title = "Test document (Title) おはよう സുപ്രഭാതം";
            info.Author = "Stefan Lange (Author)";
            info.Subject = "Test XMP metadata (Subject)";
            info.Keywords = "Some Keywords (Keywords)";
            info.Creator = "PDFsharp (Creator)";
            info.Elements.SetString(PdfDocumentInformation.Keys.Producer, "MigraDoc (Producer)");
#if true_
            info.Title = "TTTT";
            info.Author = "AAAA";
            info.Subject = "SSSS";
            info.Keywords = "KKKK";
            info.Creator = "CCCC";
#endif
        }
    }
}
