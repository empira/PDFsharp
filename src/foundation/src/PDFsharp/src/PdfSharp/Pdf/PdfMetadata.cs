// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Internal;
using System.Text;
using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using PdfSharp.Pdf.Metadata;

// v7.0.0 TODO review and sync with document information, DateTimeOffset review
// TODO DocumentID, InstanceID

/// <summary>
/// Superfluous implementation. XMP in PDF files must use only UTF-8 encoding.
/// We keep it for internal tests.
/// </summary>
enum MetadataEncodingType
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Encodes metadata stream using UTF-8 encoding.
    /// This is the default and the only recommended option.
    /// </summary>
    UTF8,

    /// <summary>
    /// Encodes metadata stream using UTF-16 little-endian encoding.
    /// </summary>
    UTF16LE,

    /// <summary>
    /// Encodes metadata stream using UTF-16 big-endian encoding.
    /// </summary>
    UTF16BE,

    /// <summary>
    /// Encodes metadata stream using UTF-32 little-endian encoding.
    /// </summary>
    UTF32LE,

    /// <summary>
    /// Encodes metadata stream using UTF-32 big-endian encoding.
    /// </summary>
    UTF32BE,

    // ReSharper restore InconsistentNaming
}

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents an XML Metadata stream.
    /// </summary>
    public sealed class PdfMetadata : PdfDictionary
    {
        // Reference 2.0: 14.3.2 Metadata streams / Page 714

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfMetadata"/> class.
        /// </summary>
        public PdfMetadata()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfMetadata"/> class.
        /// </summary>
        /// <param name="document">The document that owns this object.</param>
        public PdfMetadata(PdfDocument document)
            : base(document, true)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfMetadata(PdfDictionary dictionary)
            : base(dictionary)
        { }


        void Initialize()
        {
            Elements.SetName(Keys.Type, "/Metadata");
            Elements.SetName(Keys.Subtype, "/XML");
        }

        /// <summary>
        /// Obsolete, use ToString.
        /// </summary>
        [Obsolete("Use ToString.")]
        public string Xml
            => Stream == null ? "" : Stream.ToString();

        /// <summary>
        /// Creates the XMP metadata for the PDF document.
        /// Creating XMP metadata is not the business of PDFsharp.
        /// This is a suggestion how you can do it.
        /// </summary>
        // <remarks>
        // .NET contains classes to create XML content. We use simple text substitution here.
        // </remarks>
        public string CreateDefaultMetadata() => CreateDefaultMetadata(MetadataEncodingType.UTF8);

        // I wrote the code, and we will keep it for reference.
        internal string CreateDefaultMetadata(MetadataEncodingType encodingType)
        {
            // See XMP SPECIFICATION PART 1 - 3 for details.
            // The files names are “XMPSpecificationPart1.pdf”, “XMPSpecificationPart2.pdf”, and “XMPSpecificationPart3.pdf”.
            // They are created by Adobe in 2008 and can still be found by Google.
            //
            // I just scanned the specs und overlooked the fact that XMP data can be written in 5 encodings,
            // BUT for PDF files only UTF-8 is allowed. Acrobat also accepts UFT-16 little and big endian,
            // but not UTF-32. I wasted hours to make it work for UTF-32
            // 
            // Now we have the code created and PDFsharp would accept all encodings, but writes only UTF-8.

            var metadataInfo = MetadataManager.GetMetadataInfo();
            // ReSharper disable StringLiteralTypo because we deal with XML elements here.
            bool isPdfA = metadataInfo.PdfAFormat.HasValue;

            var xml =
                // XMP header.
                // Note that in a .NET string the BOM is represented as a human-readable string.
                // PDFsharp converts this string to correct BOM when encoded.
                $"""
                 <?xpacket begin="{encodingType}" id="W5M0MpCehiHzreSzNTczkc9d"?>
                 <x:xmpmeta xmlns:x="adobe:ns:meta/">
                   <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
                 """ + "\r\n" +
                // PDF/A section.
                (isPdfA ?
                $"""
                     <rdf:Description rdf:about="" xmlns:pdfaid="http://www.aiim.org/pdfa/ns/id/">
                       <pdfaid:part>{metadataInfo.PdfAFormat?.Part}</pdfaid:part>
                       <pdfaid:conformance>{metadataInfo.PdfAFormat?.ConformanceLevel}</pdfaid:conformance>
                     </rdf:Description>
                 """ + "\r\n"
                : ""
                ) +
                // xmlns:pdf: Producer, Keywords
                $"""
                     <rdf:Description rdf:about="" xmlns:pdf="http://ns.adobe.com/pdf/1.3/">
                       <pdf:Producer>{metadataInfo.Producer}</pdf:Producer>
                       <pdf:Keywords>{metadataInfo.Keywords}</pdf:Keywords>
                     </rdf:Description>
                 """ + "\r\n" +
                // xmlns:xap: CreatorTool, CreateDate, ModifyDate
                $"""
                     <rdf:Description xmlns:xap="http://ns.adobe.com/xap/1.0/" rdf:about="">
                       <xap:CreatorTool>{metadataInfo.Creator}</xap:CreatorTool>
                       <xap:CreateDate>{metadataInfo.CreationDate}</xap:CreateDate>
                       <xap:ModifyDate>{metadataInfo.ModificationDate}</xap:ModifyDate>
                       <xap:MetadataDate>{(!String.IsNullOrWhiteSpace(metadataInfo.ModificationDate) ? metadataInfo.ModificationDate : metadataInfo.CreationDate)}</xap:MetadataDate>
                     </rdf:Description>
                 """ + "\r\n" +
                // xmlns:dc: Title, Creator (author), Description
                $"""
                     <rdf:Description rdf:about="" xmlns:dc="http://purl.org/dc/elements/1.1/">
                       <dc:title>
                         <rdf:Alt>
                           <rdf:li xml:lang="x-default">{metadataInfo.Title}</rdf:li>
                         </rdf:Alt>
                       </dc:title>
                       <dc:creator>
                         <rdf:Seq>
                           <rdf:li>{metadataInfo.Author}</rdf:li>
                         </rdf:Seq>
                       </dc:creator>
                       <dc:description>
                         <rdf:Alt>
                           <rdf:li xml:lang="x-default">{metadataInfo.Subject}</rdf:li>
                         </rdf:Alt>
                       </dc:description>
                     </rdf:Description>
                 """ + "\r\n" +
#if true_
                // TODO: what if string from trailer is not a UUID???
                // xmlns:xapMM: DocumentID, InstanceID
                $"""
                     <rdf:Description rdf:about="" xmlns:xapMM="http://ns.adobe.com/xap/1.0/mm/">
                       <xapMM:DocumentID>uuid:{metadataInfo.DocumentID}</xapMM:DocumentID>
                       <xapMM:InstanceID>uuid:{metadataInfo.InstanceID}</xapMM:InstanceID>
                     </rdf:Description>
                 """ + "\r\n" +
#endif
                // XMP trailer.
                """
                   </rdf:RDF>
                 </x:xmpmeta>
                 <?xpacket end="w"?>
                 """;
            // ReSharper restore StringLiteralTypo
            return xml;
        }

        /// <summary>
        /// Converts the stream content to a string.<br/>
        /// The BOM in “&lt;?xpacket begin="{BOM}"” is set to “UTF-8” because a UTF-8 BOM cannot be a
        /// part of a Unicode string.
        /// </summary>
        public override String ToString() => StreamToString();

        /// <summary>
        /// Replaces the stream content with the specified byte array.
        /// </summary>
        /// <param name="xml"></param>
        public void SetMetadata(byte[] xml)
        {
            //var bytes = PdfEncoders.RawEncoding.GetBytes(xml);
            Stream = null;
            CreateStream(xml);
        }

        /// <summary>
        /// Sets the metadata stream.<br/>
        /// The BOM tag of the “&lt;?xpacket begin="{BOM}"” attribute must either be empty or 'UTF-8'.
        /// </summary>
        public void SetMetadata(string xml)
        {
            var bytes = MetadataEncoder.GetBytes(xml);
            SetMetadata(bytes);
        }

        /// <summary>
        /// Converts the byte array of the stream’s value to a regular Unicode .NET string and
        /// replaces the BOM by a string that shows the original in hexadecimal notation.
        /// </summary>
        string StreamToString() => MetadataEncoder.GetString(Stream?.UnfilteredValue ?? []);

        MetadataManager MetadataManager => _metadataManager ??= MetadataManager.ForDocument(Document
            ?? throw new InvalidOperationException("PdfMetadata must belong to a document to create metadata."));
        MetadataManager? _metadataManager;

        // TODO: Table 348 — Additional entry for components having metadata

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public class Keys : KeysBase
        {
            // Reference 2.0: Table 347 — Additional entries in a metadata stream dictionary / Page 713

            /// <summary>
            /// (Required) The type of PDF object that this dictionary describes; must be Metadata
            /// for a metadata stream.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional, FixedValue = "Metadata")]
            public const string Type = "/Type";

            /// <summary>
            /// (Required) The type of metadata stream that this dictionary describes; must be XML.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional, FixedValue = "XML")]
            public const string Subtype = "/Subtype";
        }

        internal static class MetadataPreparer  // TODO StL Eliminate this class.
        {
            public static void PrepareDocument(PdfDocument doc)
            {
                //doc.Catalog.GetMetadata();
            }
        }

        static class MetadataEncoder
        {
            /* For reference see XMP SPECIFICATION PART 3 page 14
             *
             * Search for “<?xpacket begin=” in five possible encodings.
             *
             * · 8-bit encoding (UTF-8):
             *    0x3C 0x3F 0x78 0x70 0x61 0x63 0x6B 0x65 0x74 0x20 0x62 0x65 0x67 0x69 0x6E 0x3D
             *
             * · 16-bit encoding (UCS-2, UTF-16): (either big- or little-endian order):
             *    0x3C 0x00 0x3F 0x00 0x78 0x00 0x70 0x00 0x61 0x00 0x63 0x00 0x6B 0x00 0x65 0x00 0x74 0x00 0x20 0x00 0x62 0x00 0x65 0x00 0x67 0x00 0x69 0x00 0x6E 0x00 0x3D [0x00]
             *
             * · 32-bit encoding (UCS-4):
             *    The pattern is similar to the UCS-2 pattern above, except with three 0x00 bytes for every one in the UCS-2 version.
             *
             * We expect one of the following BOMs.
             *    UTF-8                    0xEF 0xBB 0xBF
             *    UTF-16, little-endian    0xFF 0xFE
             *    UTF-16, big-endian       0xFE 0xFF
             *    UTF-32, little-endian    0xFF 0xFE 0x00 0x00
             *    UTF-32, big-endian       0x00 0x00 0xFE 0xFF
             *
             * The code below can definitely be written shorter, but less readable. We use raw strings
             * because this allows to use string operations on byte arrays.
             *
             * This is a quote from the XMP SPECIFICATION:
             *
             * “For 16-bit and 32-bit encodings, a scanner cannot be sure whether the 0x00 values are
             * in the high- or low-order portion of the character until it reads the byte-order mark
             * (the value of the begin attribute). As you can see from the pattern, it starts with the
             * first non-zero value, regardless of byte order, which means that there might or might
             * not be a terminal 0x00 value.
             *
             * A scanner can choose to simply skip 0x00 values and search for the 8-bit pattern. Once
             * the byte order is established, the scanner should switch to consuming characters rather
             * than bytes.
             *
             * After finding a matching byte pattern, the scanner must consume a quote character, which
             * can be either the single quote (apostrophe) (U+0027) or double quote (U+0022) character.
             *
             * Note that individual attribute values in the processing instruction can have either single
             * or double quotes.”
             */

            // ReSharper disable StringLiteralTypo
            // ReSharper disable InconsistentNaming
            // ReSharper disable once CommentTypo

            const string header = "<?xpacket begin=";
            const int hl = 17;  // Header length is header.Length;

            // The header string “<?xpacket begin=” in different encodings represented as raw strings.
            const string utf8Header =
                "\u003C\u003F\u0078\u0070\u0061\u0063\u006B\u0065\u0074\u0020\u0062\u0065\u0067\u0069\u006E\u003D";

            const string utf16HeaderLE =
                "\u003C\0\u003F\0\u0078\0\u0070\0\u0061\0\u0063\0\u006B\0\u0065\0\u0074\0\u0020\0\u0062\0\u0065\0\u0067\0\u0069\0\u006E\0\u003D\0";

            const string utf16HeaderBE =
                "\0\u003C\0\u003F\0\u0078\0\u0070\0\u0061\0\u0063\0\u006B\0\u0065\0\u0074\0\u0020\0\u0062\0\u0065\0\u0067\0\u0069\0\u006E\0\u003D";

            const string utf32HeaderLE =
                "\u003C\0\0\0\u003F\0\0\0\u0078\0\0\0\u0070\0\0\0\u0061\0\0\0\u0063\0\0\0\u006B\0\0\0\u0065\0\0\0\u0074\0\0\0\u0020\0\0\0\u0062\0\0\0\u0065\0\0\0\u0067\0\0\0\u0069\0\0\0\u006E\0\0\0\u003D\0\0\0";

            const string utf32HeaderBE =
                "\0\0\0\u003C\0\0\0\u003F\0\0\0\u0078\0\0\0\u0070\0\0\0\u0061\0\0\0\u0063\0\0\0\u006B\0\0\0\u0065\0\0\0\u0074\0\0\0\u0020\0\0\0\u0062\0\0\0\u0065\0\0\0\u0067\0\0\0\u0069\0\0\0\u006E\0\0\0\u003D";

            // The BOMs as raw strings.
            const string utf8Bom = "\u00EF\u00BB\u00BF";   // <=> “ï»¿”     // Only valid BOM for XMP metadata in a PDF file.
            const string utf16BomLE = "\u00FF\u00FE";      // <=> “ÿþ”      // \
            const string utf16BomBE = "\u00FE\u00FF";      // <=> “þÿ”      //  \ Valid BOMs for XMP metadata, but not in a PDF file.
            const string utf32BomLE = "\u00FF\u00FE\0\0";  // <=> “ÿþ\0\0”  //  /
            const string utf32BomBE = "\0\0\u00FE\u00FF";  // <=> “\0\0þÿ”  // /

            // ReSharper restore InconsistentNaming
            // ReSharper restore StringLiteralTypo

            public static string GetString(byte[] bytes)
            {
                // Convert stream bytes into raw string for easier coding.
                var rawString = PdfEncoders.RawEncoding.GetString(bytes);
                if (String.IsNullOrEmpty(rawString))
                    return "";

                string result;
                int pos;
                if ((pos = rawString.IndexOf(utf8Header, StringComparison.Ordinal)) != -1)
                {
                    if (pos > 0)
                        rawString = rawString[pos..];

                    // ReSharper disable once CommentTypo for better readability
                    // “<?xpacket begin="ï»¿"…”
                    //  ^0               ^17^20
                    const int leftPartLength = 17;
                    const int rightPartStart = leftPartLength + 3;
                    char leftQuot = rawString[leftPartLength - 1];
                    char rightQuot = rawString[rightPartStart];
                    if (leftQuot is '\"' or '\'' && rawString[leftPartLength - 1 + 1] is '\"' or '\'')
                    {
                        // No given BOM is equivalent to UTF-8.
                    }
                    else if (leftQuot is '\"' or '\'' && rawString[leftPartLength..rightPartStart] == utf8Bom && rightQuot is '\"' or '\'')
                    {
                        // Remove UTF-8 BOM.
                        rawString = rawString[..leftPartLength] + rawString[rightPartStart..];
                    }
                    else
                        throw new InvalidOperationException("An UTF-8 or ASCII encoded metadata stream has an invalid BOM entry.");

                    // Get bytes from stream without BOM.
                    bytes = PdfEncoders.RawEncoding.GetBytes(rawString);
                    result = Encoding.UTF8.GetString(bytes);
                    // Insert a readable encoding name.
                    result = result[..leftPartLength] + MetadataEncodingType.UTF8 + result[leftPartLength..];
                }
                else if ((pos = rawString.IndexOf(utf16HeaderLE, StringComparison.Ordinal)) is not (-1 or 1))  // 1 because utf16HeaderBE also matches here.
                {
                    PdfSharpLogHost.Logger.LogWarning("XMP metadata is encoded using UTF-16 little endian, which is not allowed in PDF files, but accepted by PDFsharp.");

                    if (pos > 0)
                        rawString = rawString[pos..];

                    const int leftPartLength = 2 * 17;
                    const int rightPartStart = leftPartLength + 2;
                    char leftQuot = rawString[leftPartLength - 2];
                    char rightQuot = rawString[rightPartStart];
                    if (leftQuot is '\"' or '\'' && rawString[leftPartLength..rightPartStart] == utf16BomLE && rightQuot is '\"' or '\'')
                    {
                        // Remove UTF-16LE BOM.
                        rawString = rawString[..leftPartLength] + rawString[rightPartStart..];
                    }
                    else
                        throw new InvalidOperationException("An UTF-16 encoded metadata stream has an invalid BOM entry.");

                    // Get bytes from stream without BOM.
                    bytes = PdfEncoders.RawEncoding.GetBytes(rawString);
                    result = Encoding.Unicode.GetString(bytes);
                    // Insert a readable encoding name.
                    result = result[..17] + MetadataEncodingType.UTF16LE + result[17..];
                }
                else if ((pos = rawString.IndexOf(utf16HeaderBE, StringComparison.Ordinal)) != -1)
                {
                    PdfSharpLogHost.Logger.LogWarning("XMP metadata is encoded using UTF-16 big endian, which is not allowed in PDF files, but accepted by PDFsharp.");

                    if (pos > 0)
                        rawString = rawString[pos..];

                    const int leftPartLength = 2 * 17 + 1;
                    const int rightPartStart = leftPartLength + 2;
                    char leftQuot = rawString[leftPartLength - 2];
                    char rightQuot = rawString[rightPartStart];
                    if (leftQuot is '\"' or '\'' && rawString[(leftPartLength - 1)..(rightPartStart - 1)] == utf16BomBE && rightQuot is '\"' or '\'')
                    {
                        // Remove UTF-16BE BOM.
                        rawString = rawString[..(leftPartLength - 1)] + rawString[(rightPartStart - 1)..];
                    }
                    else
                        throw new InvalidOperationException("An UTF-16 big-endian encoded metadata stream has an invalid BOM entry.");

                    // Get bytes from stream without BOM.
                    bytes = PdfEncoders.RawEncoding.GetBytes(rawString);
                    result = Encoding.BigEndianUnicode.GetString(bytes);
                    // Insert a readable encoding name.
                    result = result[..17] + MetadataEncodingType.UTF16BE + result[17..];
                }
                else if ((pos = rawString.IndexOf(utf32HeaderLE, StringComparison.Ordinal)) is not (-1 or 3))  // 3 because utf32HeaderBE also matches here.
                {
                    PdfSharpLogHost.Logger.LogWarning("XMP metadata is encoded using UTF-32 little endian, which is not allowed in PDF files, but accepted by PDFsharp.");

                    if (pos > 0)
                        rawString = rawString[pos..];

                    const int leftPartLength = 4 * 17;
                    const int rightPartStart = leftPartLength + 4;
                    char leftQuot = rawString[leftPartLength - 4];
                    char rightQuot = rawString[rightPartStart];
                    if (leftQuot is '\"' or '\'' && rawString[leftPartLength..rightPartStart] == utf32BomLE && rightQuot is '\"' or '\'')
                    {
                        // Remove UTF-32LE BOM.
                        rawString = rawString[..leftPartLength] + rawString[rightPartStart..];
                    }
                    else
                        throw new InvalidOperationException("An UTF-32 encoded metadata stream has an invalid BOM entry.");

                    // Get bytes from stream without BOM.
                    bytes = PdfEncoders.RawEncoding.GetBytes(rawString);
                    result = Encoding.UTF32.GetString(bytes);
                    // Insert a readable encoding name.
                    result = result[..17] + MetadataEncodingType.UTF32LE + result[17..];
                }
                else if ((pos = rawString.IndexOf(utf32HeaderBE, StringComparison.Ordinal)) != -1)
                {
                    PdfSharpLogHost.Logger.LogWarning(
                        "XMP metadata is encoded using UTF-32 big endian, which is not allowed in PDF files, but accepted by PDFsharp.");

                    if (pos > 0)
                        rawString = rawString[pos..];

                    const int leftPartLength = 4 * 17 + 3;
                    const int rightPartStart = leftPartLength + 4;
                    char leftQuot = rawString[leftPartLength - 4];
                    char rightQuot = rawString[rightPartStart];
                    if (leftQuot is '\"' or '\'' &&
                        rawString[(leftPartLength - 3)..(rightPartStart - 3)] == utf32BomBE &&
                        rightQuot is '\"' or '\'')
                    {
                        // Remove UTF-32BE BOM.
                        rawString = rawString[..(leftPartLength - 3)] + rawString[(rightPartStart - 3)..];
                    }
                    else
                        throw new InvalidOperationException(
                            "An UTF-32 big-endian encoded metadata stream has an invalid BOM entry.");

                    // Get bytes from stream without BOM.
                    bytes = PdfEncoders.RawEncoding.GetBytes(rawString);
                    result = Utf32BigEndianEncoding.GetString(bytes);
                    // Insert a readable encoding name.
                    result = result[..17] + MetadataEncodingType.UTF32BE + result[17..];
                }
                else
                {
                    // We cannot determine the encoding and leave it as is.
                    result = rawString;
                }

                return result;
            }

            public static byte[] GetBytes(string xml)
            {
                if (!xml.StartsWith(header))
                    throw new InvalidOperationException("An XMP metadata string must start with '" + header + "'.");

                const int leftPartLength = 17;
                var rightPartStart = xml.IndexOf("\"", leftPartLength, StringComparison.Ordinal);
                if (rightPartStart == -1)
                    rightPartStart = xml.IndexOf("'", leftPartLength + 1, StringComparison.Ordinal);
                if (xml[leftPartLength - 1] is not ('\"' or '\'') || xml[rightPartStart] is not ('\"' or '\'') || rightPartStart < leftPartLength)
                    throw new InvalidOperationException("An XMP metadata string has an invalid header.");

                // Get BOM tag and remove it.
                var bomTag = xml[leftPartLength..rightPartStart];
                xml = xml[..leftPartLength] + xml[rightPartStart..];
                MetadataEncodingType encoding = MetadataEncodingType.UTF8;
                if (bomTag.Length > 0)
                {
                    if (!Enum.TryParse<MetadataEncodingType>(bomTag, false, out encoding))
                        throw new InvalidOperationException($"Unknown metadata encoding tag '{bomTag}'.");
                }

                if (encoding != MetadataEncodingType.UTF8)
                {
                    PdfSharpLogHost.Logger.LogError("XMP metadata must not be encoded using {Encoding} in a PDF file.", encoding);
                }
                byte[] bytes;
                switch (encoding)
                {
                    case MetadataEncodingType.UTF8:
                        bytes = Encoding.UTF8.GetBytes(xml);
                        xml = PdfEncoders.RawEncoding.GetString(bytes);
                        xml = xml[..leftPartLength] + utf8Bom + xml[leftPartLength..];
                        break;

                    case MetadataEncodingType.UTF16LE:
                        bytes = Encoding.Unicode.GetBytes(xml);
                        xml = PdfEncoders.RawEncoding.GetString(bytes);
                        var pos = 2 * 17;
                        xml = xml[..pos] + utf16BomLE + xml[pos..];
                        break;

                    case MetadataEncodingType.UTF16BE:
                        bytes = Encoding.BigEndianUnicode.GetBytes(xml);
                        xml = PdfEncoders.RawEncoding.GetString(bytes);
                        xml = xml[..(2 * 17)] + utf16BomBE + xml[(2 * 17)..];
                        break;

                    case MetadataEncodingType.UTF32LE:
                        bytes = Encoding.UTF32.GetBytes(xml);
                        xml = PdfEncoders.RawEncoding.GetString(bytes);
                        xml = xml[..(4 * 17)] + utf32BomLE + xml[(4 * 17)..];
                        break;

                    case MetadataEncodingType.UTF32BE:
                        bytes = Utf32BigEndianEncoding.GetBytes(xml);
                        xml = PdfEncoders.RawEncoding.GetString(bytes);
                        xml = xml[..(4 * 17)] + utf32BomBE + xml[(4 * 17)..];
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown metadata encoding tag '{bomTag}'.");
                }
                bytes = PdfEncoders.RawEncoding.GetBytes(xml);
                return bytes;
            }

            // Is only instantiated in some unit tests.
            static UTF32Encoding Utf32BigEndianEncoding => _utf32BigEndianEncoding ??= new(true, false);
            static UTF32Encoding? _utf32BigEndianEncoding;
        }
    }
}
