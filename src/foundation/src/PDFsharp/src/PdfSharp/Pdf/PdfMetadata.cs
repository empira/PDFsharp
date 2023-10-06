// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Text;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents an XML Metadata stream.
    /// </summary>
    public sealed class PdfMetadata : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfMetadata"/> class.
        /// </summary>
        public PdfMetadata()
        {
            Elements.SetName(Keys.Type, "/Metadata");
            Elements.SetName(Keys.Subtype, "/XML");
            SetupStream();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfMetadata"/> class.
        /// </summary>
        /// <param name="document">The document that owns this object.</param>
        public PdfMetadata(PdfDocument document)
            : base(document)
        {
            document.Internals.AddObject(this);
            Elements.SetName(Keys.Type, "/Metadata");
            Elements.SetName(Keys.Subtype, "/XML");
            SetupStream();
        }

        void SetupStream()
        {
            const string begin = @"begin=""";

            var stream = GenerateXmp();

            // Preserve "ï»¿" if text is UTF8 encoded.
            var i = stream.IndexOf(begin, StringComparison.Ordinal);
            var pos = i + begin.Length;
            stream = stream.Substring(0, pos) + "xxx" + string.Join(string.Empty, stream.Skip(pos + 3));
            byte[] bytes = Encoding.UTF8.GetBytes(stream);
            bytes[pos++] = (byte)'ï';
            bytes[pos++] = (byte)'»';
            bytes[pos] = (byte)'¿';

            CreateStream(bytes);
        }

        string GenerateXmp()
        {
            var instanceId = Guid.NewGuid().ToString();
            var documentId = Guid.NewGuid().ToString();

            var creationDate = _document.Info.CreationDate.ToString("o");
            var modificationDate = _document.Info.CreationDate.ToString("o");

            var author = _document.Info.Author;
            var creator = _document.Info.Creator;
            var producer = _document.Info.Producer;
            var title = _document.Info.Title;
            var subject = _document.Info.Subject;
            var keywords = _document.Info.Keywords;

#if true
            // Created based on a PDF created with Microsoft Word.
            var str = $"""
                <?xpacket begin="ï»¿" id="W5M0MpCehiHzreSzNTczkc9d"?>
                  <x:xmpmeta xmlns:x="adobe:ns:meta/" x:xmptk="3.1-701">
                    <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
                      <rdf:Description rdf:about="" xmlns:pdf="http://ns.adobe.com/pdf/1.3/">
                        <pdf:Producer>{producer}</pdf:Producer><pdf:Keywords>{keywords}</pdf:Keywords>
                      </rdf:Description>
                      <rdf:Description rdf:about="" xmlns:dc="http://purl.org/dc/elements/1.1/">
                        <dc:title><rdf:Alt><rdf:li xml:lang="x-default">{title}</rdf:li></rdf:Alt></dc:title>
                        <dc:creator><rdf:Seq><rdf:li>{author}</rdf:li></rdf:Seq></dc:creator>
                        <dc:description><rdf:Alt><rdf:li xml:lang="x-default">{subject}</rdf:li></rdf:Alt></dc:description>
                      </rdf:Description>
                      <rdf:Description rdf:about="" xmlns:xmp="http://ns.adobe.com/xap/1.0/">
                        <xmp:CreatorTool>{creator}</xmp:CreatorTool>
                        <xmp:CreateDate>{creationDate}</xmp:CreateDate>
                        <xmp:ModifyDate>{modificationDate}</xmp:ModifyDate>
                      </rdf:Description>
                      <rdf:Description rdf:about="" xmlns:xmpMM="http://ns.adobe.com/xap/1.0/mm/">
                        <xmpMM:DocumentID>uuid:{documentId}</xmpMM:DocumentID>
                        <xmpMM:InstanceID>uuid:{instanceId}</xmpMM:InstanceID>
                      </rdf:Description>
                    </rdf:RDF>
                  </x:xmpmeta>
                <?xpacket end="w"?>                
                """;
#else
            // Does not exist anymore.
            // XMP Documentation: http://wwwimages.adobe.com/content/dam/Adobe/en/devnet/xmp/pdfs/XMP%20SDK%20Release%20cc-2016-08/XMPSpecificationPart1.pdf

        var str =
            // UTF-8 Byte order mark "ï»¿" and GUID (like in Reference) to avoid accidental usage in data stream.
            "<?xpacket begin=\"ï»¿\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?>\n" +

            "    <x:xmpmeta xmlns:x=\"adobe:ns:meta/\"> \n" +
            "      <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">\n" +
            "        <rdf:Description rdf:about=\"\" xmlns:xmpMM=\"http://ns.adobe.com/xap/1.0/mm/\">\n" +

            "          <xmpMM:InstanceID>uuid:" + instanceId + "</xmpMM:InstanceID>\n" +
            "          <xmpMM:DocumentID>uuid:" + documentId + "</xmpMM:DocumentID>\n" +

            "        </rdf:Description>\n" +
            "        <rdf:Description rdf:about=\"\" xmlns:pdfuaid=\"http://www.aiim.org/pdfua/ns/id/\">\n" +
            "          <pdfuaid:part>1</pdfuaid:part>\n" +
            "        </rdf:Description>\n" +
            "        <rdf:Description rdf:about=\"\" xmlns:xmp=\"http://ns.adobe.com/xap/1.0/\">\n" +

            "          <xmp:CreateDate>" + creationDate + "</xmp:CreateDate>\n" +
            "          <xmp:ModifyDate>" + modificationDate + "</xmp:ModifyDate>\n" +
            "          <xmp:CreatorTool>" + creator + "</xmp:CreatorTool>\n" +
            "          <xmp:MetadataDate>" + modificationDate + "</xmp:MetadataDate>\n" +

            "        </rdf:Description>\n" +
            "        <rdf:Description rdf:about=\"\" xmlns:pdf=\"http://ns.adobe.com/pdf/1.3/\">\n" +

            "          <pdf:Producer>" + producer + "</pdf:Producer>\n" +

            "        </rdf:Description>\n" +
            "        <rdf:Description rdf:about=\"\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\">\n" +
            "          <dc:title>\n" +
            "            <rdf:Alt>\n" +

            "              <rdf:li xml:lang=\"x-default\">" + title + "</rdf:li>\n" +

            "            </rdf:Alt>\n" +
            "          </dc:title>\n" +
            "        </rdf:Description>\n" +
            "      </rdf:RDF>\n" +
            "    </x:xmpmeta>\n" +
            "<?xpacket end=\"r\"?>\n";
#endif

            return str;
        }

        void Foo()
        {
            var documentId = Guid.NewGuid().ToString();
            var instanceId = Guid.NewGuid().ToString();

            var creationDate = _document.Info.CreationDate.ToString("o");
            var modificationDate = _document.Info.CreationDate.ToString("o");

            var author = _document.Info.Author;
            var creator = _document.Info.Creator;
            var producer = _document.Info.Producer;
            var title = _document.Info.Title;
            var subject = _document.Info.Subject;

            string s2 = $"""
                <?xpacket begin="ï»¿" id="W5M0MpCehiHzreSzNTczkc9d"?>
                  <x:xmpmeta xmlns:x="adobe:ns:meta/" x:xmptk="3.1-701">
                    <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
                    
                      <rdf:Description rdf:about=""  xmlns:pdf="http://ns.adobe.com/pdf/1.3/">
                        <pdf:Producer>{producer}</pdf:Producer>
                        <pdf:Keywords>Tag1 Tag 2 Tag3</pdf:Keywords>
                      </rdf:Description>
                      
                      <rdf:Description rdf:about=""  xmlns:dc="http://purl.org/dc/elements/1.1/">
                        <dc:title>
                          <rdf:Alt>
                            <rdf:li xml:lang="x-default">{title}</rdf:li>
                          </rdf:Alt>
                        </dc:title>
                        <dc:creator>
                        <rdf:Seq>
                          <rdf:li>{author}</rdf:li>
                        </rdf:Seq>
                        </dc:creator>
                        <dc:description>
                          <rdf:Alt>
                            <rdf:li xml:lang="x-default">{subject}</rdf:li>
                          </rdf:Alt>
                        </dc:description>
                      </rdf:Description>
                      
                      <rdf:Description rdf:about=""  xmlns:xmp="http://ns.adobe.com/xap/1.0/">
                        <xmp:CreatorTool>{creator}</xmp:CreatorTool>
                        <xmp:CreateDate>{creationDate}</xmp:CreateDate>
                        <xmp:ModifyDate>{modificationDate}</xmp:ModifyDate>
                      </rdf:Description>
                      
                      <rdf:Description rdf:about=""  xmlns:xmpMM="http://ns.adobe.com/xap/1.0/mm/">
                        <xmpMM:DocumentID>uuid:{documentId}</xmpMM:DocumentID>
                        <xmpMM:InstanceID>uuid:{instanceId}</xmpMM:InstanceID>
                      </rdf:Description>
                   
                    </rdf:RDF>
                  </x:xmpmeta>
                <?xpacket end="w"?>                
                """;
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal class Keys : KeysBase
        {
            /// <summary>
            /// (Required) The type of PDF object that this dictionary describes; must be Metadata for a metadata stream.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional, FixedValue = "Metadata")]
            public const string Type = "/Type";

            /// <summary>
            /// (Required) The type of metadata stream that this dictionary describes; must be XML.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional, FixedValue = "XML")]
            public const string Subtype = "/Subtype";
        }
    }
}
