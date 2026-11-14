// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Xml;
using FluentAssertions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Metadata;
using PdfSharp.Quality;
using Xunit;

namespace PdfSharp.Tests.Pdf.Metadata
{
    [Collection("PDFsharp")]
    public class MetadataTests : IDisposable
    {
        readonly string _tempRoot = PdfFileUtility.GetUnitTestPath(typeof(MetadataTests));

        public MetadataTests()
        { }

        public void Dispose()
        { }

        [Fact]
        public void Test_Catalog_entries()
        {
            var src = @"C:\Users\StLa\Desktop\PDFsharp\PDF-References/pdfreference1.7.pdf";
            //var src = "d:/pdfreference1.7.pdf";
            var dest = PdfFileUtility.GetTempPdfFullFileName(_tempRoot + "PdfReference1.7.pdf");
            if (!File.Exists(src))
                return;

            //var doc = PdfReader.Open(file, "User", PdfDocumentOpenMode.Import);
            var doc = PdfReader.Open(src, PdfDocumentOpenMode.Modify);
            var mdManager = MetadataManager.ForDocument(doc);
            //var metadata = mdManager.Metadata;

            var metadata = doc.Catalog.GetOrCreateMetadata();

            var xml = metadata.ToString();
            xml = xml[..17] + "UTF8" + xml[21..];
            // XMP metadata will be used only if ModDate of document info is not newer than XMP. Manipulate XMP date to make it newer.
            xml = xml.Replace("<xap:MetadataDate>2006-11-09T14:01:16-03:30</xap:MetadataDate>", $"<xap:MetadataDate>{DateTime.Now.Year:4}-12-31T14:01:16-03:30</xap:MetadataDate>");
            // Modify title in XMP to show that XMP is used for display.
            xml = xml.Replace("<rdf:li xml:lang=\"x-default\">PDF Reference, version 1.7</rdf:li>", "<rdf:li xml:lang=\"x-default\">PDF Reference, version 1.7 (modified)</rdf:li>");
            metadata.SetMetadata(xml);

            doc.Options.Layout = PdfWriterLayout.Verbose;
            doc.Save(dest);
        }

        [Theory]
        [InlineData(MetadataEncodingType.UTF8)]
        //[InlineData(MetadataEncodingType.UTF16LE)]  // Creates an invalid PDF file that is accepted by Acrobat.
        //[InlineData(MetadataEncodingType.UTF16BE)]  // Creates an invalid PDF file that is accepted by Acrobat.
        //[InlineData(MetadataEncodingType.UTF32LE)]  // Creates an invalid PDF file that is not accepted by Acrobat.
        //[InlineData(MetadataEncodingType.UTF32BE)]  // Creates an invalid PDF file that is not accepted by Acrobat.
        void ReadWrite_AutoCreate_metadata(MetadataEncodingType type)
        {
            var doc = new PdfDocument();
            doc.AddPage();
            SetSomeMetadata(doc);

            var mdManager = MetadataManager.ForDocument(doc);
            var metadata = doc.Catalog.GetOrCreateMetadata()!;

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

        [Fact]
        public void CreateDefaultMetadata_escapes_xml_and_strips_disallowed_control_chars()
        {
            var doc = new PdfDocument();
            doc.Info.Title = "Profit & Loss <2026> \"Q1\" 'Draft'\u0000\u000B\u001F";
            doc.Info.Author = "Smith & Sons <legal@example.com>";
            doc.Info.Subject = "A & B > C";
            doc.Info.Keywords = "tag1 & tag2, <xml>";
            doc.Info.Creator = "Tool <1.0> & Co";
            doc.Info.Elements.SetString(PdfDocumentInformation.Keys.Producer, "Producer & Engine");

            var metadata = doc.Catalog.GetOrCreateMetadata();
            var xml = metadata.CreateDefaultMetadata();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            var ns = new XmlNamespaceManager(xmlDoc.NameTable);
            ns.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            ns.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            ns.AddNamespace("pdf", "http://ns.adobe.com/pdf/1.3/");
            ns.AddNamespace("xap", "http://ns.adobe.com/xap/1.0/");

            xmlDoc.SelectSingleNode("//dc:title/rdf:Alt/rdf:li", ns)!.InnerText
                .Should().Be("Profit & Loss <2026> \"Q1\" 'Draft'");
            xmlDoc.SelectSingleNode("//dc:creator/rdf:Seq/rdf:li", ns)!.InnerText
                .Should().Be("Smith & Sons <legal@example.com>");
            xmlDoc.SelectSingleNode("//dc:description/rdf:Alt/rdf:li", ns)!.InnerText
                .Should().Be("A & B > C");
            xmlDoc.SelectSingleNode("//pdf:Keywords", ns)!.InnerText
                .Should().Be("tag1 & tag2, <xml>");
            xmlDoc.SelectSingleNode("//xap:CreatorTool", ns)!.InnerText
                .Should().Be("Tool <1.0> & Co");
            xmlDoc.SelectSingleNode("//pdf:Producer", ns)!.InnerText
                .Should().Be("Producer & Engine");
        }
    }
}
