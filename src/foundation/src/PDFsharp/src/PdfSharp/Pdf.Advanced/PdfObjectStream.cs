// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
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
        internal PdfObjectStream(PdfDictionary dict, Parser documentParser)
            : base(dict)
        {
            int n = Elements.GetInteger(Keys.N);
            int first = Elements.GetInteger(Keys.First);
            Stream.TryUncompress();

            var parser = new Parser(_document, new MemoryStream(Stream.UnfilteredValue), documentParser);
            _header = parser.ReadObjectStreamHeader(n, first);

#if DEBUG_ && CORE
            if (Internal.PdfDiagnostics.TraceObjectStreams)
            {
                Debug.WriteLine(String.Format("PdfObjectStream(document) created. Header item count: {0}", _header.GetLength(0)));
            }
#endif
        }

        /// <summary>
        /// Returns all ObjectIDs and read positions for the objects inside this ObjectStream.
        /// </summary>
        internal ICollection<KeyValuePair<PdfObjectID, SizeType>> ReadObjectIDsWithOffsets()
        {
            var length = _header.Length;

            Dictionary<PdfObjectID, SizeType> objectOffsets = [];

            // For duplicate IDs the newest object should be read first, to ignore older objects with the same ID read later.
            // Therefore, we read the offsets from high to low.
            for (var idx = length - 1; idx >= 0; idx--)
            {
                int objectNumber = _header[idx][0];
                int offset = _header[idx][1];

                var objectID = new PdfObjectID(objectNumber);

                // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd
                if (!objectOffsets.ContainsKey(objectID))
                {
                    objectOffsets.Add(objectID, offset);
                }
                else
                {
                    // Ignore object with objectID already on the list.
                    PdfSharpLogHost.PdfReadingLogger.LogWarning("Ignoring object with ID {objectID} while reading offsets in object stream {objectStreamId} because an object with that ID was already read in that object stream.", objectID, ObjectID);
                }
            }

            return objectOffsets;
        }

        /// <summary>
        /// N pairs of integers.
        /// The first integer represents the object number of the compressed object.
        /// The second integer represents the absolute offset of that object in the decoded stream,
        /// i.e. the byte offset plus First entry.
        /// </summary>
        readonly int[][] _header = default!; // Reference: Page 102

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
