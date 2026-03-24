// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// With this define each iref object gets a unique number (uid) to make them distinguishable in the debugger.
#define UNIQUE_IREF_xxx  // TODO Move to Directory.Build.targets

using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using PdfSharp.Pdf.IO;

// v7.0.0 TODO review MakeDocument and proxy objects


namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents an indirect reference to a PdfObject.
    /// </summary>
    [DebuggerDisplay("iref({ObjectNumber}, {GenerationNumber})")]
    public sealed class PdfReference : PdfItem
    {
        /// <summary>
        /// Initializes a new PdfReference instance for the specified indirect object.
        /// An indirect PDF object has one and only one reference.
        /// You cannot create an instance of PdfReference.
        /// </summary>
        /// <param name="pdfObject">A direct PDF object.</param>
        /// <param name="objectID">The object ID. Can be undefined.</param>
        /// <param name="position">The position in the PDF file stream, or -1, if unknown.</param>
        /// <exception cref="InvalidOperationException"></exception>
        PdfReference(PdfObject pdfObject, PdfObjectID objectID, SizeType position)
        {
            if (pdfObject.Reference != null)
                throw new InvalidOperationException("Must not create iref for an object that already has one.");
            _value = pdfObject;
            pdfObject.Reference = this;
            _objectID = objectID;

            if (position != 0)
                _position = position;
#if UNIQUE_IREF && DEBUG
            _uid = ++s_counter;
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfReference"/> class from the specified
        /// object identifier and file position.
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
        /// Initializes a new instance of the <see cref="PdfReference"/> class for a proxy container.
        /// Used for objects that represents two different types, e.g. a PDF dictionary that is both
        /// an interactive field and a widget annotation.
        /// </summary>
        /// <param name="cont">The dominant container which is in the IRefTable.</param>
        /// <param name="proxy">The proxy container which will use the Elements of the master.</param>
        internal PdfReference(PdfContainer cont, PdfContainer proxy)
            : base(cont)
        {
            Debug.Assert(cont.IsIndirect, "The dominant container must be an indirect object.");
            Debug.Assert(!proxy.IsIndirect, "The proxy container must be a direct object.");

            // DELETE ItemFlags = ItemFlags.IsProxyReference;

            var xref = cont.RequiredReference;
            _document = xref.Document;
            _objectID = xref.ObjectID;
            _position = xref.Position;
            _value = proxy;

            // Make proxy an indirect object that only exists in memory.
            proxy.Reference = this;
        }

        /// <summary>
        /// Creates a PdfReference from a PdfObject.
        /// </summary>
        /// <param name="pdfObject">A direct PDF object.</param>
        /// <param name="objectID">The object ID. Can be undefined.</param>
        /// <param name="position">The position in the PDF file stream, or -1, if unknown.</param>
        /// <returns></returns>
        internal static PdfReference CreateFromObject(PdfObject pdfObject, PdfObjectID objectID, SizeType position)
        {
#if TEST_CODE
            Debug.Assert(pdfObject.ObjectID.IsEmpty || pdfObject.ObjectID == objectID); // TODO Remove objectID in parameters?
#endif
            return new(pdfObject, objectID, position);
        }

        /// <summary>
        /// Creates a PdfReference from a PdfObjectID.
        /// </summary>
        /// <param name="objectID"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        internal static PdfReference CreateForObjectID(PdfObjectID objectID, SizeType position)
        {
            return new(objectID, position);
        }

        /// <summary>
        /// Writes the object in PDF iref table format.
        /// </summary>
        internal void WriteXRefEntry(PdfWriter writer)
        {
            // PDFsharp does not yet support PDF 1.5 object streams for writing.

            // Each line must be exactly 20 bytes long, otherwise Acrobat wants to repair the file.
            writer.WriteRaw(Invariant($"{_position:0000000000} {_objectID.GenerationNumber:00000} n \n"));
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
            internal set => _objectID = value;
        }
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
        /// <remarks>
        /// The returned value can be null during the reading of a PDF file.
        /// </remarks>
        public PdfObject Value
        {
            get => _value;
            internal set
            {
                Debug.Assert(value != null, "The value of a PdfReference must never be null.");
                Debug.Assert(value.Reference == null || ReferenceEquals(value.Reference, this), "The reference of the value must be null or this.");

                // value must never be null.
                if (value == null!)
                    throw new ArgumentNullException(nameof(value), "value must not be null.");

                _value = value;
#if DEBUG
                if (value.Reference != null)
                {
                    _ = typeof(int);
                    Debug.Assert(ReferenceEquals(value.Reference, this));
                }
#endif
                value.Reference = this;
            }
        }
        PdfObject _value = null!;

        /// <summary>
        /// Resets value to null. Used when reading object stream.
        /// </summary>
        internal void ResetObject() => _value = null!;

        /// <summary>
        /// Gets the document this object belongs to.
        /// </summary>
        public PdfDocument Document
        {
            get
            {
#if DEBUG
                if (_document == null)
                    PdfSharpLogHost.Logger.LogDebug("Document of object {_objectID} is null.", _objectID);
#endif
                return _document!;
            }
            internal set => _document = value;
        }
        PdfDocument? _document;

        /// <summary>
        /// Gets a string representing the object identifier.
        /// </summary>
        public override string ToString() => _objectID + " R";

        /// <summary>
        /// Returns true if the specified item is an indirect object; false otherwise.
        /// </summary>
        public static bool IsIndirect(PdfItem item)  // PDFsharp/NT
            => item is PdfReference or PdfObject { Reference: not null };

        /// <summary>
        /// Dereferences the specified item. If the item is a PdfReference, the item is set
        /// to the referenced value. Otherwise, no action is taken.
        /// </summary>
        public static void Dereference(ref PdfItem item)  // Do not change to 'ref PdfItem?'.
        {
            if (item is PdfReference reference)
                item = reference.Value;
        }

        /// <summary>
        /// If the specified item is an indirect object it is replaced by its PdfReference.
        /// Otherwise, no action is taken.
        /// </summary>
        internal static void ToReference(ref PdfItem item)
        {
            if (item is PdfObject { Reference: not null } obj)
                item = obj.RequiredReference;
        }

        /// <summary>
        /// Makes a PDF object an indirect object of the specified document
        /// and returns a reference to it.
        /// </summary>
        /// <param name="obj">A newly created object.</param>
        /// <param name="doc">The PDF document the object is added as indirect object.</param>
        public static PdfReference MakeIndirect(PdfObject obj, PdfDocument doc)  // PDFsharp/NT
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            //ArgumentNullException.ThrowIfNull(obj);
            //ArgumentNullExceptionEx.ThrowIfNull(obj);

            if (doc == null)
                throw new ArgumentNullException(nameof(doc));

            if (obj.IsIndirect)
                throw new InvalidOperationException("Object is already an indirect object.");

            if (obj.Owner != null! && obj.Owner != doc)
                throw new InvalidOperationException("Object already has an owner that is not the specified document.");

            doc.Internals.AddObject(obj);

            Debug.Assert(obj.Reference != null);
            return obj.Reference;
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
                    {
                        if (l.ObjectNumber == r.ObjectNumber)
                            return l.GenerationNumber - r.GenerationNumber;
                        return l.ObjectNumber - r.ObjectNumber;
                    }
                    return -1;
                }
                return r != null ? 1 : 0;
            }
        }

        internal void SetTemp() => ItemFlags |= ItemFlags.IsTempRef;

        internal void ClearTemp() => ItemFlags &= ~ItemFlags.IsTempRef;

        internal bool IsTemp() => (ItemFlags & ItemFlags.IsTempRef) != 0;

        /// <summary>
        /// Not yet used.
        /// </summary>
        internal int AddRef()  // TODO: Use it for all indirect objects.
        {
            return Interlocked.Increment(ref _refCount);
        }

        /// <summary>
        /// Not yet used.
        /// </summary>
        internal int Release()
        {
            return Interlocked.Decrement(ref _refCount);
        }

        /// <summary>
        /// Gets the reference count of this PdfReference.
        /// </summary>
        internal int RefCounter => _refCount;

        int _refCount;

#if UNIQUE_IREF && DEBUG
        static int s_counter = 0;
        int _uid;
#endif
    }
}
