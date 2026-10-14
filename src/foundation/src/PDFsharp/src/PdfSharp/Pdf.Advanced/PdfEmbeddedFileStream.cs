// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.PdfItemExtensions;
using PdfSharp.Pdf.Security;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents a PDF embedded file stream.
    /// </summary>
    public class PdfEmbeddedFileStream : PdfDictionary
    {
        // Reference 2.0: 7.11.4  Embedded file streams / Page 137

        /// <summary>
        /// Initializes a new instance of PdfEmbeddedFileStream from a stream.
        /// </summary>
        public PdfEmbeddedFileStream(PdfDocument document, Stream stream, string? subType = null, DateTimeOffset? modDate = null)
            : base(document)
        {
            // TODO: Use TryGetBuffer if stream is MemoryStream.
            // use the byte array from stream function here.

            var length = stream.Length;
            var bytes = new byte[length];
            using (stream)
            {
                var bytesRead = stream.Read(bytes, 0, (int)length);
                // TODO: bytesRead may be smaller than length.
                //Debug.Assert(bytesRead != length);
            }
            InitializeNew(bytes, subType, modDate);
        }

        /// <summary>
        /// Initializes a new instance of PdfEmbeddedFileStream from a stream.
        /// </summary>
        public PdfEmbeddedFileStream(PdfDocument document, byte[] bytes, string? subType = null, DateTimeOffset? modDate = null)
            : base(document)
        {
            InitializeNew(bytes, subType, modDate);
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfEmbeddedFileStream(PdfDictionary dict)
            : base(dict)
        { }

        void InitializeNew(byte[] bytes, string? subType, DateTimeOffset? modDate)
        {
            Elements.SetName(Keys.Type, TypeValue);
            Elements.SetInteger("/Length", bytes.Length);

            // HACK TODO remove
            Elements.SetName(Keys.Subtype, "text/xml");
            if (subType != null)
                Elements.SetName(Keys.Subtype, subType);

            var now = DateTimeOffset.Now;
            modDate ??= now;

            //var fileParams = Elements.GetValue(Keys.Params, VCF.Create).AsDictionary<PdfEmbeddedFileParameters>();
            var fileParams = Elements.GetRequiredDictionary<PdfEmbeddedFileParameters>(Keys.Params, VCF.Create);

            fileParams.Elements.SetInteger(PdfEmbeddedFileParameters.Keys.Size, bytes.Length);
            fileParams.Elements.SetDateTime(PdfEmbeddedFileParameters.Keys.CreationDate, now);
            fileParams.Elements.SetDateTime(PdfEmbeddedFileParameters.Keys.ModDate, modDate.Value);
            fileParams.Elements.SetString(PdfEmbeddedFileParameters.Keys.CheckSum, MD5Managed.ComputeHashHex(bytes));

            Stream = new(bytes, this);
        }

        /// <summary>
        /// Determines, if dictionary is an embedded file stream.
        /// </summary>
        public static bool IsEmbeddedFile(PdfDictionary dictionary)
        {
            return dictionary.Elements.GetName(Keys.Type) == TypeValue;
        }

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
            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfEmbeddedFileParameters))]
            public const string Params = "/Params";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
