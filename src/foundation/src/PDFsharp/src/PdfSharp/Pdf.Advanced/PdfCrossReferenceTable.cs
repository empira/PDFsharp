// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections;
using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents the cross-reference table of a PDF document. 
    /// It contains all indirect objects of a document.
    /// </summary>
    sealed class PdfCrossReferenceTable  // Must not be derived from PdfObject.
    {
        public PdfCrossReferenceTable(PdfDocument document)
        {
            _document = document;
        }
        readonly PdfDocument _document;

        /// <summary>
        /// Represents the relation between PdfObjectID and PdfReference for a PdfDocument.
        /// </summary>
        public Dictionary<PdfObjectID, PdfReference> ObjectTable = [];

        /// <summary>
        /// Gets or sets a value indicating whether this table is under construction.
        /// It is true while reading a PDF file.
        /// </summary>
        internal bool IsUnderConstruction { get; set; }

        /// <summary>
        /// Adds a cross-reference entry to the table. Used when parsing the trailer.
        /// </summary>
        public void Add(PdfReference iref)
        {
            if (iref.ObjectID.IsEmpty)
                iref.ObjectID = new(GetNewObjectNumber());

            // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd because it would not build with .NET Framework.
            if (ObjectTable.ContainsKey(iref.ObjectID))
            {
#if true_
                // Really happens with existing (bad) PDF files.
                // See file 'Detaljer.ARGO.KOD.rev.B.pdf' from https://github.com/ststeiger/PdfSharpCore/issues/362
                throw new InvalidOperationException("Object already in table.");
#else
                // We remove the existing one and use the latter reference.
                // HACK: This is just a quick fix that may not be the best solution in all cases.
                // On GitHub user packdat provides a PR that orders objects. This code is not yet integrated,
                // because releasing 6.1.0 had a higher priority. We will fix this in 6.2.0.
                // However, this quick fix is better than throwing an exception in all cases.
                PdfSharpLogHost.PdfReadingLogger.LogError("Object '{ObjectID}' already exists in xref table. The latter one is used.", iref.ObjectID);
                ObjectTable.Remove(iref.ObjectID);
#endif
            }
            ObjectTable.Add(iref.ObjectID, iref);
        }

        /// <summary>
        /// Adds a PdfObject to the table.
        /// </summary>
        public void Add(PdfObject value)
        {
            if (value.Owner == null!)
                value.Document = _document;
            else
                Debug.Assert(value.Owner == _document);

            if (value.ObjectID.IsEmpty)
                value.SetObjectID(GetNewObjectNumber(), 0);

            if (ObjectTable.ContainsKey(value.ObjectID))
                throw new InvalidOperationException("Object already in table.");

            ObjectTable.Add(value.ObjectID, value.ReferenceNotNull);
        }

        /// <summary>
        /// Adds a PdfObject to the table if it was not already in.
        /// Returns true if it was added, false otherwise.
        /// </summary>
        public bool TryAdd(PdfObject value)
        {
            if (value.ObjectID.IsEmpty || !ObjectTable.ContainsKey(value.ObjectID))
            {
                Add(value);
                return true;
            }
            return false;
        }

        public void Remove(PdfReference iref)
        {
            ObjectTable.Remove(iref.ObjectID);
        }

        /// <summary>
        /// Gets a cross-reference entry from an object identifier.
        /// Returns null if no object with the specified ID exists in the object table.
        /// </summary>
        public PdfReference? this[PdfObjectID objectID]
        {
            get
            {
                ObjectTable.TryGetValue(objectID, out var iref);
                return iref;
            }
        }

        /// <summary>
        /// Indicates whether the specified object identifier is in the table.
        /// </summary>
        public bool Contains(PdfObjectID objectID)
        {
            return ObjectTable.ContainsKey(objectID);
        }

        //public PdfObject GetObject(PdfObjectID objectID)
        //{
        //  return this[objectID].Value;
        //}

        //    /// <summary>
        //    /// Gets the entry for the specified object, or null if the object is not in
        //    /// this XRef table.
        //    /// </summary>
        //    internal PdfReference GetEntry(PdfObjectID objectID)
        //    {
        //      return this[objectID];
        //    }

        /// <summary>
        /// Returns the next free object number.
        /// </summary>
        public int GetNewObjectNumber()
        {
            // New objects are numbered consecutively. If a document is imported, maxObjectNumber is
            // set to the highest object number used in the document.
            return ++MaxObjectNumber;
        }

        /// <summary>
        /// Gets or sets the highest object number used in this document.
        /// </summary>
        internal int MaxObjectNumber { get; set; }

        /// <summary>
        /// Writes the xref section in PDF stream.
        /// </summary>
        internal void WriteObject(PdfWriter writer)
        {
            writer.WriteRaw("xref\n");

            var iRefs = AllReferences;

            int count = iRefs.Length;
            writer.WriteRaw(Invariant($"0 {count + 1}\n"));
            writer.WriteRaw(Invariant($"{0:0000000000} {65535:00000} f \n"));

            for (int idx = 0; idx < count; idx++)
            {
                var iref = iRefs[idx];

                // Acrobat is very pedantic; it must be exactly 20 bytes per line.
                writer.WriteRaw(Invariant($"{iref.Position:0000000000} {iref.GenerationNumber:00000} n \n"));
            }
        }

        /// <summary>
        /// Gets an array of all object identifiers. For debugging purposes only.
        /// </summary>
        internal PdfObjectID[] AllObjectIDs
        {
            get
            {
                ICollection collection = ObjectTable.Keys;
                var objectIDs = new PdfObjectID[collection.Count];
                collection.CopyTo(objectIDs, 0);
                return objectIDs;
            }
        }

        /// <summary>
        /// Gets an array of all cross-references in ascending order by their object identifier.
        /// </summary>
        internal PdfReference[] AllReferences
        {
            get
            {
                var collection = ObjectTable.Values;
                var list = new List<PdfReference>(collection);
                list.Sort(PdfReference.Comparer);
                var iRefs = new PdfReference[collection.Count];
                list.CopyTo(iRefs, 0);
                return iRefs;
            }
        }

        internal void HandleOrphanedReferences()
        { }

        /// <summary>
        /// Removes all objects that cannot be reached from the trailer.
        /// Returns the number of removed objects.
        /// </summary>
        internal int Compact()
        {
            // TODO: remove PdfBooleanObject, PdfIntegerObject etc.
            int removed = ObjectTable.Count;
            //CheckConsistence();
            // TODO: Is this really so easy?
            PdfReference[] irefs = TransitiveClosure(_document.Trailer);

#if DEBUG
            // Have any two objects the same ID?
            Dictionary<int, int> ids = [];
            foreach (PdfObjectID objID in ObjectTable.Keys)
            {
                ids.Add(objID.ObjectNumber, 0);
            }

            // Have any two irefs the same value?
            //Dictionary<int, int> ids = new Dictionary<int, int>();
            ids.Clear();
            foreach (PdfReference iref in ObjectTable.Values)
            {
                ids.Add(iref.ObjectNumber, 0);
            }

            //
            Dictionary<PdfReference, int> refs = new Dictionary<PdfReference, int>();
            foreach (PdfReference iref in irefs)
            {
                refs.Add(iref, 0);
            }

            foreach (PdfReference value in ObjectTable.Values)
            {
                if (!refs.ContainsKey(value))
                    _ = typeof(int);
            }

            foreach (PdfReference iref in ObjectTable.Values)
            {
                if (iref.Value == null!)
                    _ = typeof(int);
                Debug.Assert(iref.Value != null);
            }

            foreach (PdfReference iref in irefs)
            {
                if (!ObjectTable.ContainsKey(iref.ObjectID))
                    _ = typeof(int);
                Debug.Assert(ObjectTable.ContainsKey(iref.ObjectID));

                if (iref.Value == null!)
                    _ = typeof(int);
                Debug.Assert(iref.Value != null);
            }
#endif

            MaxObjectNumber = 0;
            ObjectTable.Clear();
            foreach (PdfReference iref in irefs)
            {
                // This if is needed for corrupt PDF files from the wild.
                // Without the if, an exception will be thrown if the file contains duplicate IDs ("An item with the same key has already been added to the dictionary.").
                // With the if, the first object with the ID will be used and later objects with the same ID will be ignored.
                if (!ObjectTable.ContainsKey(iref.ObjectID))
                {
                    ObjectTable.Add(iref.ObjectID, iref);
                    MaxObjectNumber = Math.Max(MaxObjectNumber, iref.ObjectNumber);
                }
            }
            //CheckConsistence();
            removed -= ObjectTable.Count;
            return removed;
        }

        /// <summary>
        /// Renumbers the objects starting at 1.
        /// </summary>
        internal void Renumber()
        {
            //CheckConsistence();
            PdfReference[] irefs = AllReferences;
            ObjectTable.Clear();
            // Give all objects a new number.
            int count = irefs.Length;
            for (int idx = 0; idx < count; idx++)
            {
                PdfReference iref = irefs[idx];
#if DEBUG_
                if (iref.ObjectNumber == 1108)
                    _ = typeof(int);
#endif
                iref.ObjectID = new PdfObjectID(idx + 1);
                // Rehash with new number.
                ObjectTable.Add(iref.ObjectID, iref);
            }
            MaxObjectNumber = count;
            //CheckConsistence();
        }

        /// <summary>
        /// Gets the position of the object immediately behind the specified object, or -1,
        /// if no such object exists. I.e. -1 means the object is the last one in the PDF file.
        /// </summary>
        internal SizeType GetPositionOfObjectBehind(PdfObject obj, SizeType position)
        {
#if DEBUG
            if (obj.Reference == null)
                _ = typeof(int);
#endif
            var closestPosition = SizeType.MaxValue;
            PdfReference? closest = null;
            foreach (var iref in ObjectTable.Values)
            {
                var pos = iref.Position;
                if (pos < position)
                    continue;
                if (pos < closestPosition && iref != obj.Reference)
                {
                    closestPosition = pos;
                    closest = iref;
                }
            }
            // Variable closest can be null if our object is the last one in PDF stream.
            return closest?.Position ?? -1;
        }

        /// <summary>
        /// Checks the logical consistence for debugging purposes (useful after reconstruction work).
        /// </summary>
        [Conditional("DEBUG_")]
        public void CheckConsistence()
        {
            Dictionary<PdfReference, object?> ht1 = new();
            foreach (var iref in ObjectTable.Values)
            {
                Debug.Assert(!ht1.ContainsKey(iref), "Duplicate iref.");
                Debug.Assert(iref.Value != null);
                ht1.Add(iref, null);
            }

            Dictionary<PdfObjectID, object?> ht2 = new();
            foreach (var iref in ObjectTable.Values)
            {
                Debug.Assert(!ht2.ContainsKey(iref.ObjectID), "Duplicate iref.");
                ht2.Add(iref.ObjectID, null);
            }

            ICollection collection = ObjectTable.Values;
            int count = collection.Count;
            PdfReference[] irefs = new PdfReference[count];
            collection.CopyTo(irefs, 0);
#if true
#if DEBUG
            for (int i = 0; i < count; i++)
                for (int j = 0; j < count; j++)
                    if (i != j)
                    {
                        Debug.Assert(ReferenceEquals(irefs[i].Document, _document));
                        Debug.Assert(irefs[i] != irefs[j]);
                        Debug.Assert(!ReferenceEquals(irefs[i], irefs[j]));
                        Debug.Assert(!ReferenceEquals(irefs[i].Value, irefs[j].Value));
                        Debug.Assert(!Equals(irefs[i].ObjectID, irefs[j].Value.ObjectID));
                        Debug.Assert(irefs[i].ObjectNumber != irefs[j].Value.ObjectNumber);
                        Debug.Assert(ReferenceEquals(irefs[i].Document, irefs[j].Document));
                    }
#endif
#endif
        }

        ///// <summary>
        ///// The garbage collector for PDF objects.
        ///// </summary>
        //public sealed class GC
        //{
        //  PdfXRefTable xrefTable;
        //
        //  internal GC(PdfXRefTable xrefTable)
        //  {
        //    _xrefTable = xrefTable;
        //  }
        //
        //  public void Collect()
        //  { }
        //
        //  public PdfReference[] ReachableObjects()
        //  {
        //    Hash_table objects = new Hash_table();
        //    TransitiveClosure(objects, _xrefTable.document.trailer);
        //  }

        /// <summary>
        /// Calculates the transitive closure of the specified PdfObject with the specified depth, i.e. all indirect objects
        /// recursively reachable from the specified object in up to maximally depth steps.
        /// </summary>
        public PdfReference[] TransitiveClosure(PdfObject pdfObject, int depth = Int16.MaxValue)
        {
            CheckConsistence();
            Dictionary<PdfItem, object?> objects = new();
            _overflow = new();
            TransitiveClosureImplementation(objects, pdfObject);
        TryAgain:
            if (_overflow.Count > 0)
            {
                var array = new PdfObject[_overflow.Count];
                _overflow.Keys.CopyTo(array, 0);
                _overflow = new();
                for (int idx = 0; idx < array.Length; idx++)
                {
                    PdfObject obj = array[idx];
                    TransitiveClosureImplementation(objects, obj);
                }
                goto TryAgain;
            }

            CheckConsistence();

            ICollection collection = objects.Keys;
            int count = collection.Count;
            PdfReference[] irefs = new PdfReference[count];
            collection.CopyTo(irefs, 0);

#if true_
            for (int i = 0; i < count; i++)
                for (int j = 0; j < count; j++)
                    if (i != j)
                    {
                        Debug.Assert(ReferenceEquals(irefs[i].Document, _document));
                        Debug.Assert(irefs[i] != irefs[j]);
                        Debug.Assert(!ReferenceEquals(irefs[i], irefs[j]));
                        Debug.Assert(!ReferenceEquals(irefs[i].Value, irefs[j].Value));
                        Debug.Assert(!Equals(irefs[i].ObjectID, irefs[j].Value.ObjectID));
                        Debug.Assert(irefs[i].ObjectNumber != irefs[j].Value.ObjectNumber);
                        Debug.Assert(ReferenceEquals(irefs[i].Document, irefs[j].Document));
                        _ = typeof(int);
                    }
#endif
            return irefs;
        }

        static int _nestingLevel;
        Dictionary<PdfItem, object?> _overflow = new();

        void TransitiveClosureImplementation(Dictionary<PdfItem, object?> objects, PdfObject pdfObject/*, ref int depth*/)
        {
            try
            {
                _nestingLevel++;
                if (_nestingLevel >= 1000)
                {
                    if (!_overflow.ContainsKey(pdfObject))
                        _overflow.Add(pdfObject, null);
                    return;
                }
#if DEBUG_
                //enterCount++;
                if (enterCount == 5400)
                    _ = typeof(int);
                //if (!Object.ReferenceEquals(pdfObject.Owner, _document))
                //  _ = typeof(int);
                //////Debug.Assert(Object.ReferenceEquals(pdfObject27.Document, _document));
                //      if (item is PdfObject && ((PdfObject)item).ObjectID.ObjectNumber == 5)
                //        Deb/ug.WriteLine("items: " + ((PdfObject)item).ObjectID.ToString());
                //if (pdfObject.ObjectNumber == 5)
                //  _ = typeof(int);
#endif
                IEnumerable? enumerable = null; //(IEnumerator)pdfObject;
                PdfDictionary? dict;
                PdfArray? array;
                if ((dict = pdfObject as PdfDictionary) != null)
                    enumerable = dict.Elements.Values;
                else if ((array = pdfObject as PdfArray) != null)
                    enumerable = array.Elements;
                else
                    Debug.Assert(false, "Should not come here.");

                if (enumerable != null)
                {
                    foreach (PdfItem item in enumerable)
                    {
                        if (item is PdfReference iref)
                        {
                            // Is this an indirect reference to an object that does not exist?
                            //if (iref.Document == null)
                            //{
                            //    Deb/ug.WriteLine("Dead object detected: " + iref.ObjectID.ToString());
                            //    PdfReference dead = DeadObject;
                            //    iref.ObjectID = dead.ObjectID;
                            //    iref.Document = _document;
                            //    iref.SetObject(dead.Value);
                            //    PdfDictionary dict = (PdfDictionary)dead.Value;

                            //    dict.Elements["/DeadObjectCount"] =
                            //      new PdfInteger(dict.Elements.GetInteger("/DeadObjectCount") + 1);

                            //    iref = dead;
                            //}

                            if (!ReferenceEquals(iref.Document, _document))
                            {
                                //Debug.WriteLine($"Bad iref: {iref.ObjectID.ToString()}");
                                PdfSharpLogHost.PdfReadingLogger.LogError($"Bad iref: {iref.ObjectID.ToString()}");
                            }
                            Debug.Assert(ReferenceEquals(iref.Document, _document) || iref.Document == null, "External object detected!");
#if DEBUG_
                            if (iref.ObjectID.ObjectNumber == 23)
                                _ = typeof(int);
#endif
                            if (!objects.ContainsKey(iref))
                            {
                                PdfObject value = iref.Value;

                                // Ignore unreachable objects.
                                if (iref.Document != null)
                                {
                                    // ... from trailer hack
                                    if (value == null)
                                    {
                                        iref = ObjectTable[iref.ObjectID];
                                        Debug.Assert(iref.Value != null);
                                        value = iref.Value;
                                    }
                                    Debug.Assert(ReferenceEquals(iref.Document, _document));
                                    objects.Add(iref, null);
                                    //Debug.WriteLine(String.Format("objects.Add('{0}', null);", iref.ObjectID.ToString()));
                                    if (value is PdfArray || value is PdfDictionary)
                                        TransitiveClosureImplementation(objects, value /*, ref depth*/);
                                }
                                //else
                                //{
                                //  objects2.Add(this[iref.ObjectID], null);
                                //}
                            }
                        }
                        else
                        {
                            //var pdfObject28 = item as PdfObject;
                            ////if (pdfObject28 != null)
                            ////  Debug.Assert(Object.ReferenceEquals(pdfObject28.Document, _document));
                            //if (pdfObject28 != null && (pdfObject28 is PdfDictionary || pdfObject28 is PdfArray))
                            //if (pdfObject28 != null)
                            //  Debug.Assert(Object.ReferenceEquals(pdfObject28.Document, _document));
                            if (item is PdfObject pdfObj and (PdfDictionary or PdfArray))
                                TransitiveClosureImplementation(objects, pdfObj /*, ref depth*/);
                        }
                    }
                }
            }
            finally
            {
                _nestingLevel--;
            }
        }

        /// <summary>
        /// Gets the cross-reference to an object used for undefined indirect references.
        /// </summary>
        public PdfReference DeadObject
        {
            get
            {
                if (_deadObject == null)
                {
                    _deadObject = new PdfDictionary(_document);
                    Add(_deadObject);
                    _deadObject.Elements.Add("/DeadObjectCount", new PdfInteger());
                }
                return _deadObject.ReferenceNotNull;
            }
        }
        PdfDictionary? _deadObject;
    }

    ///// <summary>
    ///// Represents the cross-reference table of a PDF document. 
    ///// It contains all indirect objects of a document.
    ///// </summary>
    //internal sealed class PdfCrossReferenceStreamTable  // Must not be derived from PdfObject.
    //{
    //    public PdfCrossReferenceStreamTable(PdfDocument document)
    //    {
    //        _document = document;
    //    }
    //    readonly PdfDocument _document;

    //    public class Item
    //    {
    //        public PdfReference Reference;

    //        public readonly List<CrossReferenceStreamEntry> Entries = new List<CrossReferenceStreamEntry>();
    //    }
    //}

    //struct CrossReferenceStreamEntry
    //{
    //    public int Type;

    //    public int Field2;

    //    public int Field3;
    //}
}
