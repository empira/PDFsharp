// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Security.Cryptography;
using PdfSharp.Pdf.PdfItemExtensions;
using PdfSharp.Pdf.Security;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents an embedded file parameter dictionary.
    /// </summary>
    public sealed class PdfEmbeddedFileParameters : PdfDictionary
    {
        // Reference 2.0: 7.11.4  Embedded file streams / Page 138

        /// <summary>
        /// Initializes a new instance of PdfEmbeddedFileParameters.
        /// </summary>
        public PdfEmbeddedFileParameters(PdfDocument document) : base(document)
        {
            var now = new PdfDate(DateTimeOffset.Now);
            Elements[Keys.CreationDate] = now;
            Elements[Keys.ModDate] = now;
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfEmbeddedFileParameters(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public class Keys : PdfStream.Keys
        {
            /// <summary>
            /// (Optional) The size of the uncompressed embedded file, in bytes.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string Size = "/Size";

            /// <summary>
            /// (Optional) The date and time when the embedded file was created.
            /// </summary>
            [KeyInfo(KeyType.Date | KeyType.Optional)]
            public const string CreationDate = "/CreationDate";

            /// <summary>
            /// (Optional, required in the case of an embedded file stream used as an associated file)
            /// The date and time when the embedded file was last modified.
            /// </summary>
            [KeyInfo(KeyType.Date | KeyType.Optional)]
            public const string ModDate = "/ModDate";

            /// <summary>
            /// (Optional; deprecated in PDF 2.0)
            /// A subdictionary containing additional information specific to Mac OS files.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string Mac = "/Mac";

            /// <summary>
            ///(Optional) A 16-byte string that is the checksum of the bytes of the uncompressed embedded file.
            /// The checksum shall be calculated by applying the standard MD5 message-digest algorithm
            /// (defined in Internet RFC 1321) to the bytes of the embedded file stream.
            /// </summary>
            [KeyInfo(KeyType.String | KeyType.Optional)]
            public const string CheckSum = "/CheckSum";
        }
    }
}
