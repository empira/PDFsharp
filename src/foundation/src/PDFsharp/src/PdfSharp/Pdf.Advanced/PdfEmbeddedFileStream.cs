// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif

using System.Reflection.Metadata;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents an embedded file stream.
    /// PDF 1.3.
    /// </summary>
    public class PdfEmbeddedFileStream : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of PdfEmbeddedFileStream from a stream.
        /// </summary>
        /// <param name="checksum">A 16-byte string which is a MD5 checksum of the bytes of the file</param>
        public PdfEmbeddedFileStream(PdfDocument document, Stream stream, string? checksum = null) : base(document)
        {
            _data = new byte[stream.Length];
            using (stream)
            {
                stream.Read(_data, 0, (int)stream.Length);
            }
            _checksum = checksum;

            Initialize();
        }

        void Initialize()
        {
            Elements.SetName(Keys.Type, TypeValue);
            Elements.SetInteger("/Length", _data.Length);

            var objParams = new PdfDictionary(_document);
            objParams.Elements.SetInteger("/Size", _data.Length);
            var now = DateTime.Now;
            objParams.Elements.SetDateTime("/CreationDate", now);
            objParams.Elements.SetDateTime("/ModDate", now);
            if (!string.IsNullOrEmpty(_checksum))
                objParams.Elements.SetString("/CheckSum", _checksum);
            Elements.SetObject(Keys.Params, objParams);

            Stream = new PdfStream(_data, this);
        }

        /// <summary>
        /// Determines, if dictionary is an embedded file stream.
        /// </summary>
        public static bool IsEmbeddedFile(PdfDictionary dictionary)
        {
            return dictionary.Elements.GetName(Keys.Type) == TypeValue;
        }

        readonly byte[] _data;
        readonly string? _checksum;

        const string TypeValue = "/EmbeddedFile";

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public class Keys : PdfStream.Keys
        {
            /// <summary>
            /// (Optional) The type of PDF object that this dictionary describes; if present,
            /// must be EmbeddedFile for an embedded file stream.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Type = "/Type";

            /// <summary>
            /// (Optional) The subtype of the embedded file. The value of this entry must be a first-class name,
            /// as defined in Appendix E. Names without a registered prefix must conform to the MIME media type names
            /// defined in Internet RFC 2046, Multipurpose Internet Mail Extensions (MIME), Part Two: Media Types
            /// (see the Bibliography), with the provision that characters not allowed in names must use the
            /// 2-character hexadecimal code format described in Section 3.2.4, “Name Objects.”
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Subtype = "/Subtype";

            /// <summary>
            /// (Optional) An embedded file parameter dictionary containing additional,
            /// file-specific information (see Table 3.43).
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string Params = "/Params";
        }
    }
}
