// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.IO;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents an object stream that contains compressed objects.
    /// PDF 1.5.
    /// </summary>
    public class PdfObjectStream : PdfDictionary
    {
        // Reference: 3.4.6  Object Streams / Page 100

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfObjectStream"/> class.
        /// </summary>
        public PdfObjectStream(PdfDocument document)
            : base(document)
        {
#if DEBUG && CORE
            if (Internal.PdfDiagnostics.TraceObjectStreams)
            {
                Debug.WriteLine("PdfObjectStream(document) created.");
            }
#endif
        }

        /// <summary>
        /// Initializes a new instance from an existing dictionary. Used for object type transformation.
        /// </summary>
        internal PdfObjectStream(PdfDictionary dict)
            : base(dict)
        {
            // while objects inside an object-stream are not encrypted, the object-streams themself ARE !
            // 7.5.7, Page 47:
            // In an encrypted file (i.e., entire object stream is encrypted),
            // strings occurring anywhere in an object stream shall not be separately encrypted.
            var xrefEncrypt = _document._trailer.Elements[PdfTrailer.Keys.Encrypt] as PdfReference;
            if (xrefEncrypt != null && _document.SecurityHandler != null)
                _document.SecurityHandler.DecryptObject(dict);

            int n = Elements.GetInteger(Keys.N);
            int first = Elements.GetInteger(Keys.First);
            Stream.TryUnfilter();

            Parser parser = new Parser(null, new MemoryStream(Stream.Value));
            _header = parser.ReadObjectStreamHeader(n, first);

#if DEBUG && CORE
            if (Internal.PdfDiagnostics.TraceObjectStreams)
            {
                Debug.WriteLine(String.Format("PdfObjectStream(document) created. Header item count: {0}", _header.GetLength(0)));
            }
#endif
        }

        /// <summary>
        /// Reads the compressed object with the specified index.
        /// </summary>
        internal void ReadReferences(PdfCrossReferenceTable xrefTable)
        {
            ////// Create parser for stream.
            ////Parser parser = new Parser(_document, new MemoryStream(Stream.Value));
            for (int idx = 0; idx < _header.Length; idx++)
            {
                int objectNumber = _header[idx][0];
                int offset = _header[idx][1];

                PdfObjectID objectID = new PdfObjectID(objectNumber);

                // HACK: -1 indicates compressed object.
                PdfReference iref = new PdfReference(objectID, -1);
                ////iref.ObjectID = objectID;
                ////iref.Value = xrefStream;
                if (!xrefTable.Contains(iref.ObjectID))
                {
                    xrefTable.Add(iref);
                }
                else
                {
#if DEBUG
                    GetType();
#endif
                }
            }
        }

        /// <summary>
        /// Reads the compressed object with the specified index.
        /// </summary>
        internal PdfReference ReadCompressedObject(int index)
        {
            Parser parser = new Parser(_document, new MemoryStream(Stream.Value));
            int objectNumber = _header[index][0];
            int offset = _header[index][1];
            return parser.ReadCompressedObject(objectNumber, offset);
        }

        /// <summary>
        /// N pairs of integers.
        /// The first integer represents the object number of the compressed object.
        /// The second integer represents the absolute offset of that object in the decoded stream,
        /// i.e. the byte offset plus First entry.
        /// </summary>
        readonly int[][] _header = null!; // NRT  // Reference: Page 102

        /// <summary>
        /// Predefined keys common to all font dictionaries.
        /// </summary>
        public class Keys : PdfStream.Keys
        {
            // Reference: TABLE 3.14  Additional entries specific to an object stream dictionary / Page 101

            /// <summary>
            /// (Required) The type of PDF object that this dictionary describes;
            /// must be ObjStmfor an object stream.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required, FixedValue = "ObjStm")]
            public const string Type = "/Type";

            /// <summary>
            /// (Required) The number of compressed objects in the stream.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string N = "/N";

            /// <summary>
            /// (Required) The byte offset (in the decoded stream) of the first
            /// compressed object.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string First = "/First";

            /// <summary>
            /// (Optional) A reference to an object stream, of which the current object
            /// stream is considered an extension. Both streams are considered part of
            /// a collection of object streams (see below). A given collection consists
            /// of a set of streams whose Extendslinks form a directed acyclic graph.
            /// </summary>
            [KeyInfo(KeyType.Stream | KeyType.Optional)]
            public const string Extends = "/Extends";
        }
    }

#if DEBUG && CORE
    static class ObjectStreamDiagnostics
    {
        public static void AddObjectStreamXRef()
        { }
    }
#endif
}
