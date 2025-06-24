// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// With this define each iref object gets a unique number (uid) to make them distinguishable in the debugger.
#define UNIQUE_IREF_

using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents an indirect reference to a PdfObject.
    /// </summary>
    [DebuggerDisplay("iref({ObjectNumber}, {GenerationNumber})")]
    public sealed class PdfReference : PdfItem
    {
        // About PdfReference
        //
        // * A PdfReference holds either the ObjectID or the PdfObject or both.
        //
        // * Each PdfObject has a PdfReference if and only if it is an indirect object. Direct objects have
        //   no PdfReference, because they are embedded in a parent object.
        //
        // * PdfReference objects are used to reference PdfObject instances. A value in a PDF dictionary
        //   or array that is a PdfReference represents an indirect reference. A value in a PDF dictionary
        //   or array that is a PdfObject represents a direct (or embedded) object.
        //
        // * When a PDF file is imported, the PdfXRefTable is filled with PdfReference objects keeping the
        //   ObjectsIDs and file positions (offsets) of all indirect objects.
        //
        // * Indirect objects can easily be renumbered because they do not rely on their ObjectsIDs.
        //
        // * During modification of a document the ObjectID of an indirect object has no meaning,
        //   except that they must be different in pairs.

        /// <summary>
        /// Initializes a new PdfReference instance for the specified indirect object.
        /// An indirect PDF object has one and only one reference.
        /// You cannot create an instance of PdfReference.
        /// </summary>
        PdfReference(PdfObject pdfObject, PdfObjectID objectID, SizeType position)
        {
            if (pdfObject.Reference != null)
                throw new InvalidOperationException("Must not create iref for an object that already has one.");
            _value = pdfObject;
            pdfObject.Reference = this;
            _objectID = objectID;

            if (position != 0)
                _position = position;
            //else
            //{
            //    // Can be 0 or -1.
            //    //_ = typeof(int);
            //    //PdfSharpLogHost.DocumentProcessingLogger.LogError("Position is 0.");
            //}
#if UNIQUE_IREF && DEBUG
            _uid = ++s_counter;
#endif
        }

        /// <summary>
        /// Initializes a new PdfReference instance from the specified object identifier and file position.
        /// </summary>
        PdfReference(PdfObjectID objectID, SizeType position)
        {
            _objectID = objectID;
            _position = position;
#if UNIQUE_IREF && DEBUG
            _uid = ++s_counter;
#endif
        }

        /// <summary>
        /// Creates a PdfReference from a PdfObject.
        /// </summary>
        /// <param name="pdfObject"></param>
        /// <param name="objectID"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        internal static PdfReference CreateFromObject(PdfObject pdfObject, PdfObjectID objectID, SizeType position)
            => new(pdfObject, objectID, position);

        /// <summary>
        /// Creates a PdfReference from a PdfObjectID.
        /// </summary>
        /// <param name="objectID"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        internal static PdfReference CreateForObjectID(PdfObjectID objectID, SizeType position)
            => new(objectID, position);

        /// <summary>
        /// Writes the object in PDF iref table format.
        /// </summary>
        internal void WriteXRefEntry(PdfWriter writer)
        {
            // PDFsharp does not yet support PDF 1.5 object streams for writing.

            // Each line must be exactly 20 bytes long, otherwise Acrobat repairs the file.
            switch (Document.Options.LineEnding.Length)
            {
                case 1: writer.WriteRaw(Invariant($"{_position:0000000000} {_objectID.GenerationNumber:00000} n ")); break;
                case 2: writer.WriteRaw(Invariant($"{_position:0000000000} {_objectID.GenerationNumber:00000} n")); break;
                default: throw new ArgumentOutOfRangeException(nameof(Document.Options.LineEnding), Document.Options.LineEnding.Length, "Line ending length is invalid");
            }
            writer.WriteRaw(Document.Options.LineEnding);
        }

        /// <summary>
        /// Writes an indirect reference.
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
        {
            writer.Write(this);
        }

        /// <summary>
        /// Gets or sets the object identifier.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public PdfObjectID ObjectID
        {
            get => _objectID;
            set
            {
                _objectID = value;
#if true_
                if (Document != null)
                {
                    //PdfXRefTable table = Document.xrefTable;
                    //table.Remove(this);
                    //objectID = value;
                    //table.Add(this);
                }
#endif
            }
        }

        // ReSharper disable once InconsistentNaming
        PdfObjectID _objectID;

        /// <summary>
        /// Gets the object number of the object identifier.
        /// </summary>
        public int ObjectNumber => _objectID.ObjectNumber;

        /// <summary>
        /// Gets the generation number of the object identifier.
        /// </summary>
        public int GenerationNumber => _objectID.GenerationNumber;

        /// <summary>
        /// Gets or sets the file position of the related PdfObject.
        /// </summary>
        public SizeType Position
        {
            get => _position;
            internal set
            {
                Debug.Assert(value >= 0);
                _position = value;
            }
        }
        SizeType _position;

        /// <summary>
        /// Gets or sets the referenced PdfObject.
        /// </summary>
        public PdfObject Value
        {
            get => _value;
            internal set
            {
                Debug.Assert(value != null, "The value of a PdfReference must never be null.");
                Debug.Assert(value.Reference == null || ReferenceEquals(value.Reference, this), "The reference of the value must be null or this.");
                _value = value;
                // value must never be null.
                value.Reference = this;
            }
        }
        PdfObject _value = null!;

        /// <summary>
        /// Resets value to null. Used when reading object stream.
        /// </summary>
        internal void ResetObject() => _value = null!;

        /// <summary>
        /// Gets or sets the document this object belongs to.
        /// </summary>
        public PdfDocument Document
        {
            get
            {
#if DEBUG
                if (_document == null)
                {
                    PdfSharpLogHost.Logger.LogDebug("Document of object {_objectID} is null.", _objectID);
                }
#endif
                return _document!;
            }
            set => _document = value;
        }
        PdfDocument? _document;

        /// <summary>
        /// Gets a string representing the object identifier.
        /// </summary>
        public override string ToString() => _objectID + " R";

        /// <summary>
        /// Dereferences the specified item. If the item is a PdfReference, the item is set
        /// to the referenced value. Otherwise, no action is taken.
        /// </summary>
        public static void Dereference(ref object item)
        {
            if (item is PdfReference reference)
                item = reference.Value;
        }

        /// <summary>
        /// Dereferences the specified item. If the item is a PdfReference, the item is set
        /// to the referenced value. Otherwise, no action is taken.
        /// </summary>
        public static void Dereference(ref PdfItem item)
        {
            if (item is PdfReference reference)
                item = reference.Value;
        }

        internal static PdfReferenceComparer Comparer => new();

        /// <summary>
        /// Implements a comparer that compares PdfReference objects by their PdfObjectID.
        /// </summary>
        internal class PdfReferenceComparer : IComparer<PdfReference>
        {
            public int Compare(PdfReference? l, PdfReference? r)
            {
                if (l != null)
                {
                    if (r != null)
                        return l._objectID.CompareTo(r._objectID);
                    return -1;
                }
                return r != null ? 1 : 0;
            }
        }

#if UNIQUE_IREF && DEBUG
        static int s_counter = 0;
        int _uid;
#endif
    }
}
