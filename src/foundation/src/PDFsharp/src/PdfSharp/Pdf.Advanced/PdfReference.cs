// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// With this define each iref object gets a unique number (uid) to make them distinguishable in the debugger
#define UNIQUE_IREF_

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents an indirect reference to a PdfObject.
    /// </summary>
    [DebuggerDisplay("iref({ObjectNumber}, {GenerationNumber})")]
    public class PdfReference : PdfItem
    {
        // About PdfReference 
        // 
        // * A PdfReference holds either the ObjectID or the PdfObject or both.
        // 
        // * Each PdfObject has a PdfReference if and only if it is an indirect object. Direct objects have
        //   no PdfReference, because they are embedded in a parent objects.
        //
        // * PdfReference objects are used to reference PdfObject instances. A value in a PDF dictionary
        //   or array that is a PdfReference represents an indirect reference. A value in a PDF dictionary or
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
        /// </summary>
        public PdfReference(PdfObject pdfObject)
        {
            if (pdfObject.Reference != null)
                throw new InvalidOperationException("Must not create iref for an object that already has one.");
            _value = pdfObject;
            pdfObject.Reference = this;
#if UNIQUE_IREF && DEBUG
            _uid = ++s_counter;
#endif
        }

        /// <summary>
        /// Initializes a new PdfReference instance from the specified object identifier and file position.
        /// </summary>
        public PdfReference(PdfObjectID objectID, int position)
        {
            _objectID = objectID;
            _position = position;
#if UNIQUE_IREF && DEBUG
            _uid = ++s_counter;
#endif
        }

        /// <summary>
        /// Writes the object in PDF iref table format.
        /// </summary>
        internal void WriteXRefEntry(PdfWriter writer)
        {
            // PDFsharp does not yet support PDF 1.5 object streams for writing.

            // Each line must be exactly 20 bytes long, otherwise Acrobat repairs the file.
            writer.WriteRaw(Invariant($"{_position:0000000000} {_objectID.GenerationNumber:00000} n \n"));

            // DELETE
            //string text = String.Format(CultureInfo.InvariantCulture, "{0:0000000000} {1:00000} n\n",
            //  _position, _objectID.GenerationNumber); // InUse ? 'n' : 'f');
            //writer.WriteRaw(text);
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
                // Makes no sense anymore.
                //// Ignore redundant invocations.
                //if (_objectID == value)
                //    return;

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
        public int Position
        {
            get => _position;
#if true
            set
            {
#if DEBUG_
                if (value < 0)
                    GetType();
#endif
                Debug.Assert(value >= 0);
                _position = value;
            }
#else
                set => _position = value;
#endif
        }

        int _position;  // I know it should be long, but I have never seen a 2GB PDF file.

        /// <summary>
        /// Gets or sets the referenced PdfObject.
        /// </summary>
        public virtual PdfObject Value
        {
            get => _value;
            set
            {
                Debug.Assert(value != null, "The value of a PdfReference must never be null.");
                Debug.Assert(value.Reference == null || ReferenceEquals(value.Reference, this), "The reference of the value must be null or this.");
                _value = value;
                // value must never be null.
                value.Reference = this;
            }
        }
        PdfObject _value = default!;

        /// <summary>
        /// Hack for dead objects.
        /// </summary>
        internal void SetObject(PdfObject value)
        {
            _value = value;
        }

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
                    LogHost.Logger.LogDebug("Document of object {_objectID} is null.", _objectID);
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
        /// to the referenced value. Otherwise no action is taken.
        /// </summary>
        public static void Dereference(ref object item)
        {
            if (item is PdfReference reference)
                item = reference.Value;
        }

        /// <summary>
        /// Dereferences the specified item. If the item is a PdfReference, the item is set
        /// to the referenced value. Otherwise no action is taken.
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
                //var l = x;
                //var r = y;
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

    /// <summary>
    /// Represents an indirect reference to an object stored in an <see cref="PdfObjectStream"/><br></br>
    /// The value of this object is "lazily" loaded when first accessed.
    /// </summary>
    public sealed class PdfReferenceToCompressedObject : PdfReference
    {
        private readonly int _objectStreamNumber;
        private readonly int _indexInObjectStream;

        internal PdfReferenceToCompressedObject(PdfDocument doc, PdfObjectID objectID,
            int objectStreamNumber, int indexInObjectStream)
            : base(objectID, -1)
        {
            Document = doc ?? throw new ArgumentNullException(nameof(doc));
            _objectStreamNumber = objectStreamNumber;
            _indexInObjectStream = indexInObjectStream;
        }

        public override PdfObject Value
        {
            get
            {
                if (base.Value is null)
                {
                    ReadValue();
                }
                return base.Value!;
            }
            set => base.Value = value;
        }

        /// <summary>
        /// Reads the value of this object
        /// </summary>
        void ReadValue()
        {
            PdfObjectStream? ostm = null;
            var stmObjID = new PdfObjectID(_objectStreamNumber);
            // reference to object stream
            var streamRef = Document.IrefTable[stmObjID];
            if (streamRef is not null)
            {
                if (streamRef.Value is null)
                {
                    // object stream not yet loaded. do it now
                    var parser = Document.GetParser()!;
                    var state = parser.SaveState();
                    var obj = parser.ReadObject(null, stmObjID, false, false);
                    if (obj is PdfDictionary ostmDict)
                    {
                        // decrypt if necessary
                        // must be done before type-transformation because PdfObjectStream
                        // tries to parse the stream-header in the constructor
                        Document.EffectiveSecurityHandler?.DecryptObject(ostmDict);
                        ostm = new PdfObjectStream(ostmDict);
                    }
                    parser.RestoreState(state);
                    Debug.Assert(ostm != null, "Object stream should not be null here");
                }
                // already transformed ?
                else if (streamRef.Value is not PdfObjectStream existingOstm)
                {
                    if (streamRef.Value is PdfDictionary ostmDict)
                    {
                        // decrypt if necessary
                        Document.EffectiveSecurityHandler?.DecryptObject(ostmDict);
                        ostm = new PdfObjectStream(ostmDict);
                    }
                    Debug.Assert(ostm != null, "Object stream should not be null here");
                }
                else
                    ostm = existingOstm;

                if (ostm is not null)
                {
                    // store the loaded and decrypted object-stream
                    streamRef.Value = ostm;
                    // read the actual object we're looking for
                    var iref = ostm.ReadCompressedObject(_indexInObjectStream);
                    if (iref is not null)
                    {
                        Debug.Assert(iref.ObjectID == ObjectID, "ObjectID mismatch");
                        base.Value = iref.Value;
                    }
                }
            }
        }
    }
}
