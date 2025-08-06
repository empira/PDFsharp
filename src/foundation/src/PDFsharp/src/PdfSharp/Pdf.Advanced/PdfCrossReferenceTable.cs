// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// Turn on all test code at one place and ensure, that it does not come accidentally to the release build.
#if true && TEST_CODE
#define TEST_CODE  // The #define is redundant, but clarifies what it means.
#else
#undef TEST_CODE
#endif

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
    /// <remarks>
    /// New implementation:<br/>
    /// * No deep nesting recursion anymore.<br/>
    /// * Uses a Stack&lt;PdfObject&gt;.<br/>
    /// <br/>
    /// We use Dictionary&lt;PdfReference, object?&gt; instead of Set&lt;PdfReference&gt; because a dictionary
    /// with an unused value is faster than a set.
    /// </remarks>
    sealed class PdfCrossReferenceTable(PdfDocument document) // Must not be derived from PdfObject.
    {
#if TEST_CODE
        readonly Stopwatch _stopwatch = new();
#endif
        /// <summary>
        /// Gets or sets a value indicating whether this table is under construction.
        /// It is true while reading a PDF file.
        /// </summary>
        internal bool IsUnderConstruction { get; set; }

        /// <summary>
        /// Gets the current number of references in the table.
        /// </summary>
        public int Count => _objectTable.Count;

        /// <summary>
        /// Adds a cross-reference entry to the table. Used when parsing a trailer.
        /// </summary>
        public void Add(PdfReference iref)
        {
            if (iref.ObjectID.IsEmpty)
            {
                // When happens this?
                iref.ObjectID = new(GetNewObjectNumber());
                PdfSharpLogHost.DocumentProcessingLogger.LogWarning("iRef with empty object ID found.");
            }

            // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd because it would not build with .NET Framework.
            if (_objectTable.ContainsKey(iref.ObjectID))
            {
                var oldIref = _objectTable.First(x => x.Key == iref.ObjectID).Value;

                // We remove the existing one and use the latter reference.
                // Choosing the latter reference may not be the best solution in all cases.
                // On GitHub user packdat provides a PR that orders objects. This code is not yet integrated,
                // because releasing 6.1.0 had a higher priority. We will fix this in a later release.
                // However, using the last added object and logging an error is better than throwing an exception in all cases.
                PdfSharpLogHost.PdfReadingLogger.LogError("Object '{ObjectID}' already exists in xref table’s objects, referring to position {Position}. The latter one referring to position {Position} is used. " +
                                                          $"This should not occur. If you think this is a bug in PDFsharp, please visit {UrlLiterals.LinkToCannotOpenPdfFile} for further information.", oldIref.ObjectID, oldIref.Position, iref.Position);

                _objectTable.Remove(iref.ObjectID);
            }
            _objectTable.Add(iref.ObjectID, iref);

            // Always adjust MaxObjectNumber when a new object is added.
            MaxObjectNumber = Math.Max(MaxObjectNumber, iref.ObjectNumber);
        }

        /// <summary>
        /// Adds a PdfObject to the table.
        /// </summary>
        public void Add(PdfObject value)
        {
            // ReSharper disable once NullableWarningSuppressionIsUsed
            if (value.Owner == null!)
            {
                PdfSharpLogHost.PdfReadingLogger.LogWarning("Object without owner gets owned by the document it was added to.");
                value.Document = document;
            }
            else
            {
                Debug.Assert(value.Owner == document);
                if (value.Owner != document)
                {
                    PdfSharpLogHost.PdfReadingLogger.LogError("Object not owned by the document it was added to.");
                }
            }

            if (value.ObjectID.IsEmpty)
            {
                // Create new object number.
                value.SetObjectID(GetNewObjectNumber(), 0);
            }

            if (_objectTable.ContainsKey(value.ObjectID))
            {
                // This must not happen.
                throw new InvalidOperationException("Object already in table.");
            }

            _objectTable.Add(value.ObjectID, value.ReferenceNotNull);

            // Always adjust MaxObjectNumber when a new object is added.
            MaxObjectNumber = Math.Max(MaxObjectNumber, value.ObjectNumber);
        }

        /// <summary>
        /// Adds a PdfObject to the table if it was not already in.
        /// Returns true if it was added, false otherwise.
        /// </summary>
        public bool TryAdd(PdfObject value)
        {
            if (value.ObjectID.IsEmpty || !_objectTable.ContainsKey(value.ObjectID))
            {
                Add(value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a PdfObject from the table.
        /// </summary>
        /// <param name="iref"></param>
        public bool Remove(PdfReference iref)
        {
            // Remove the reference by its ID.
            return _objectTable.Remove(iref.ObjectID);
        }

        /// <summary>
        /// Gets a cross-reference entry from an object identifier.
        /// Returns null if no object with the specified ID exists in the object table.
        /// </summary>
        public PdfReference? this[PdfObjectID objectID]
        {
            get
            {
                _objectTable.TryGetValue(objectID, out var iref);
                return iref;
            }
        }

        /// <summary>
        /// Indicates whether the specified object identifier is in the table.
        /// </summary>
        public bool Contains(PdfObjectID objectID) => _objectTable.ContainsKey(objectID);

        /// <summary>
        /// Gets a collection of all values in the table.
        /// </summary>
        public Dictionary<PdfObjectID, PdfReference>.ValueCollection Values => _objectTable.Values;

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
                ICollection collection = _objectTable.Keys;
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
                var collection = _objectTable.Values;
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
            // IMPROVE: remove PdfBooleanObject, PdfIntegerObject etc.
            int removed = _objectTable.Count;
            PdfReference[] irefs = TransitiveClosure(document.Trailer);

#if DEBUG_ // Turn on again
            // Have any two objects the same ID?
            Dictionary<int, int> ids = [];
            foreach (PdfObjectID objID in _objectTable.Keys)
            {
                ids.Add(objID.ObjectNumber, 0);
            }

            // Have any two irefs the same value?
            //Dictionary<int, int> ids = new Dictionary<int, int>();
            ids.Clear();
            foreach (PdfReference iref in _objectTable.Values)
            {
                ids.Add(iref.ObjectNumber, 0);
            }

            // Are all references different?
            Dictionary<PdfReference, int> refs = new Dictionary<PdfReference, int>();
            foreach (PdfReference iref in irefs)
            {
                refs.Add(iref, 0);
            }

            foreach (PdfReference value in _objectTable.Values)
            {
                if (!refs.ContainsKey(value))
                    _ = typeof(int);
            }

            foreach (PdfReference iref in _objectTable.Values)
            {
                if (iref.Value == null!)
                    _ = typeof(int);
                Debug.Assert(iref.Value != null);
            }

            foreach (PdfReference iref in irefs)
            {
                if (!_objectTable.ContainsKey(iref.ObjectID))
                    _ = typeof(int);
                Debug.Assert(_objectTable.ContainsKey(iref.ObjectID));

                if (iref.Value == null!)
                    _ = typeof(int);
                Debug.Assert(iref.Value != null);
            }
#endif

            MaxObjectNumber = 0;
            _objectTable.Clear();
            foreach (var iref in irefs)
            {
                // This if-statement is needed for corrupt PDF files from the wild.
                // Without the if-statement, an exception will be thrown if the file contains duplicate IDs ("An item with the same key has already been added to the dictionary.").
                // With the if-statement, the first object with the ID will be used and later objects with the same ID will be ignored.
                // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd because it would not build with .NET Framework.
                if (!_objectTable.ContainsKey(iref.ObjectID))
                {
                    _objectTable.Add(iref.ObjectID, iref);
                    MaxObjectNumber = Math.Max(MaxObjectNumber, iref.ObjectNumber);
                }
            }
            //CheckConsistence();
            removed -= _objectTable.Count;
            return removed;
        }

        /// <summary>
        /// Renumbers the objects starting at 1.
        /// </summary>
        internal void Renumber()
        {
            //CheckConsistence();
            PdfReference[] irefs = AllReferences;
            _objectTable.Clear();
            // Give all objects a new number.
            int count = irefs.Length;
            for (int idx = 0; idx < count; idx++)
            {
                var iref = irefs[idx];
                iref.ObjectID = new PdfObjectID(idx + 1);
                // Rehash with new number.
                _objectTable.Add(iref.ObjectID, iref);
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
#if DEBUG_
            if (obj.Reference == null)
                _ = typeof(int);
#endif
            var closestPosition = SizeType.MaxValue;
            PdfReference? closest = null;
            foreach (var iref in _objectTable.Values)
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
            foreach (var iref in _objectTable.Values)
            {
                Debug.Assert(!ht1.ContainsKey(iref), "Duplicate iref.");
                Debug.Assert(iref.Value != null);
                ht1.Add(iref, null);
            }

            Dictionary<PdfObjectID, object?> ht2 = new();
            foreach (var iref in _objectTable.Values)
            {
                Debug.Assert(!ht2.ContainsKey(iref.ObjectID), "Duplicate iref.");
                ht2.Add(iref.ObjectID, null);
            }

            ICollection collection = _objectTable.Values;
            int count = collection.Count;
            PdfReference[] irefs = new PdfReference[count];
            collection.CopyTo(irefs, 0);
#if DEBUG
            for (int i = 0; i < count; i++)
                for (int j = 0; j < count; j++)
                    if (i != j)
                    {
                        Debug.Assert(ReferenceEquals(irefs[i].Document, document));
                        Debug.Assert(irefs[i] != irefs[j]);
                        Debug.Assert(!ReferenceEquals(irefs[i], irefs[j]));
                        Debug.Assert(!ReferenceEquals(irefs[i].Value, irefs[j].Value));
                        Debug.Assert(!Equals(irefs[i].ObjectID, irefs[j].Value.ObjectID));
                        Debug.Assert(irefs[i].ObjectNumber != irefs[j].Value.ObjectNumber);
                        Debug.Assert(ReferenceEquals(irefs[i].Document, irefs[j].Document));
                    }
#endif
        }

        /// <summary>
        /// Calculates the transitive closure of the specified PdfObject with the specified depth, i.e. all indirect objects
        /// recursively reachable from the specified object.
        /// </summary>
        public PdfReference[] TransitiveClosure(PdfObject pdfObject)
        {
            var logger = PdfSharpLogHost.DocumentProcessingLogger;

            CheckConsistence();
#if TEST_CODE
            // ReSharper disable once InconsistentNaming because it is test-code.
            Dictionary<PdfReference, object?> references_old = new();
            _stopwatch.Reset();
            _stopwatch.Start();
            TransitiveClosureImplementation_old(references_old, pdfObject);
            _stopwatch.Stop();
            logger.LogInformation(nameof(TransitiveClosureImplementation_old) + " runs {MS}ms.", _stopwatch.ElapsedMilliseconds);
            logger.LogInformation($"TC: {references_old.Count}");
            logger.LogInformation("--------------------");
#endif

#if TEST_CODE
            _stopwatch.Reset();
            _stopwatch.Start();
#endif
            Dictionary<PdfReference, object?> references = new();
            TransitiveClosureImplementation(references, pdfObject);
#if TEST_CODE
            _stopwatch.Stop();
            logger.LogInformation(nameof(TransitiveClosureImplementation) + " runs {MS}ms.", _stopwatch.ElapsedMilliseconds);
            logger.LogInformation($"TC: {references.Count}");
            logger.LogInformation("--------------------");

            Debug.Assert(references.Count == references_old.Count);
#endif

#if TEST_CODE
            _stopwatch.Reset();
            _stopwatch.Start();
            foreach (var val in references_old)
            {
                var valNew = references.ContainsKey((PdfReference)val.Key);
                if (valNew == false)
                    _ = typeof(int);
                Debug.Assert(valNew);
            }
            _stopwatch.Stop();
            logger.LogInformation("references_old check runs {MS}ms.", _stopwatch.ElapsedMilliseconds);

#endif
            CheckConsistence();

            ICollection collection = references.Keys;
            int count = collection.Count;
            PdfReference[] irefs = new PdfReference[count];
            collection.CopyTo(irefs, 0);

            return irefs;
        }

#if TEST_CODE
        /// <summary>
        /// This is the old implementation of the transitive closure. It is a recursive implementation.
        /// We keep it some time to use it for counter-checking the new non-recursive implementation.
        /// </summary>
        /// <param name="references"></param>
        /// <param name="pdfObject"></param>
        void TransitiveClosureImplementation_old(Dictionary<PdfReference, object?> references, PdfObject pdfObject)
        {
            IEnumerable? enumerable = null;
            PdfDictionary? dict;
            PdfArray? array;
            if ((dict = pdfObject as PdfDictionary) != null)
                enumerable = dict.Elements.Values;
            else if ((array = pdfObject as PdfArray) != null)
                enumerable = array.Elements;
            else
                Debug.Assert(false, "Should not come here.");

            if (enumerable != null!)
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

                        if (!ReferenceEquals(iref.Document, document))
                        {
                            //Debug.WriteLine($"Bad iref: {iref.ObjectID.ToString()}");
                            PdfSharpLogHost.PdfReadingLogger.LogError($"Bad iref: {iref.ObjectID.ToString()}");
                        }
                        Debug.Assert(ReferenceEquals(iref.Document, document) || iref.Document == null, "External object detected!");

                        if (references.ContainsKey(iref) is false)
                        {
                            PdfObject value = iref.Value;

                            // Ignore unreachable objects.
                            if (iref.Document != null)
                            {
                                // ... from trailer hack
                                if (value == null!) // Can it be null?
                                {
                                    iref = _objectTable[iref.ObjectID];
                                    Debug.Assert(iref.Value != null);
                                    value = iref.Value;
                                }
                                Debug.Assert(ReferenceEquals(iref.Document, document));
                                references.Add(iref, null);
                                //Debug.WriteLine(String.Format("objects.Add('{0}', null);", iref.ObjectID.ToString()));
                                if (value is PdfArray or PdfDictionary)
                                    TransitiveClosureImplementation_old(references, value);
                            }
#if DEBUG
                            else
                            {
                                _ = typeof(int);
                            }
#endif
                        }
                    }
                    else
                    {
                        if (item is PdfObject pdfObj and (PdfDictionary or PdfArray))
                            TransitiveClosureImplementation_old(references, pdfObj);
                    }
                }
            }
        }
#endif
        /// <summary>
        /// The new non-recursive implementation.
        /// </summary>
        /// <param name="references"></param>
        /// <param name="pdfObject"></param>
        void TransitiveClosureImplementation(Dictionary<PdfReference, object?> references, PdfObject pdfObject)
        {
            var logger = PdfSharpLogHost.DocumentProcessingLogger;
#if TEST_CODE
            //Dictionary<PdfObject, object?> doubleCheckReferences = [];
            //Dictionary<PdfObject, object?> pivots = [];
            int loopCounter = 0;
            int maxStackLength = 0;
            int alreadyInTable = 0;
            int elementsAdded = 0;
#endif
            // Initialize the stack.
            Stack<PdfObject> stack = [];
            FindReferencedItems(pdfObject);

            // Loop until no more new references are found.
            while (stack.Count > 0)
            {
                var pivot = stack.Pop();
#if TEST_CODE
                maxStackLength = Math.Max(maxStackLength, stack.Count);
                loopCounter++;

                //// ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd because TryAdd does not exist in .NET Framework.
                //if (!pivots.ContainsKey(pivot))
                //{
                //    pivots.Add(pivot, null);
                //}
                //else
                //{
                //    _ = pivot.Equals(null);
                //    _ = typeof(int);
                //}
#endif
                FindReferencedItems(pivot);
            }
#if TEST_CODE
            logger.LogInformation(
                "LoopCounter: {LoopCounter}, MaxStackLength: {MaxStackLength}, AlreadyInTable: {AlreadyInTable}, ElementsAdded: {ElementsAdded}",
                loopCounter, maxStackLength, alreadyInTable, elementsAdded);

            //Dictionary<int, object?> test = [];
            //foreach (var pivotsValue in pivots.Keys)
            //{
            //    if (pivotsValue.ObjectID.ObjectNumber == 0)
            //        continue;

            //    test.Add(pivotsValue.ObjectID.ObjectNumber, null);
            //}
            //int ids = test.Count;
#endif
            return;

            // Add all dictionaries and arrays referenced by the specified object
            // that are not already in references to the stack.
            void FindReferencedItems(PdfObject pdfObj)
            {
                Debug.Assert(pdfObj is PdfDictionary or PdfArray, "Call with dictionary or array only.");

                IEnumerable? items = null;
                PdfDictionary? dict;
                PdfArray? array;
                if ((dict = pdfObj as PdfDictionary) is not null)
                    items = dict.Elements.Values;
                else if ((array = pdfObj as PdfArray) is not null)
                    items = array.Elements;
                else
                    Debug.Assert(false, "Should not come here.");

                foreach (PdfItem item in items)
                {
                    if (item is PdfReference iref)
                    {
                        // Case: The item is an indirect object.

                        // Check if the reference belongs to the current document.
                        if (!ReferenceEquals(iref.Document, document))
                        {
                            logger.LogError($"Bad iref: {iref.ObjectID.ToString()}");
                        }

                        Debug.Assert(ReferenceEquals(iref.Document, document) || iref.Document == null,
                            "External object detected!");

                        // Is the reference not yet in the collection of referenced objects?
                        if (references.ContainsKey(iref))
                        {
#if TEST_CODE
                            alreadyInTable++;
#endif
                            continue;
                        }

                        var newObject = iref.Value;

                        // Ignore unreachable objects.
                        if (iref.Document != null)
                        {
                            // ... from trailer hack
                            if (newObject == null!) // Can it be null?
                            {
                                logger.LogInformation("Value of a PdfReference is null.");
                                iref = _objectTable[iref.ObjectID];
                                Debug.Assert(iref.Value != null);
                                newObject = iref.Value;
                            }

                            Debug.Assert(ReferenceEquals(iref.Document, document));
                            if (newObject.ObjectID.ObjectNumber != 0)
                                references.Add(iref, null);
#if TEST_CODE__
                            // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd because auf .NET Framework / Standard
                            if (!doubleCheckReferences.ContainsKey(value))
                                doubleCheckReferences.Add(value, null);
                            else
                                _ = typeof(int);
#endif

                            if (newObject is PdfDictionary or PdfArray)
                            {
                                stack.Push(newObject);
#if TEST_CODE
                                elementsAdded++;
#endif
                            }
                        }
                        else
                        {
                            // Can we come here?
                            logger.LogWarning("Document has no owner.");
                        }
                    }
                    else
                    {
                        // Case: The item is a direct object.

                        if (item is PdfObject pdfDictionaryOrArray and (PdfDictionary or PdfArray))
                        {
#if TEST_CODE_
                            // Not useful, is too slow.
                            if (stack.Contains(pdfDictionaryOrArray))
                                _ = typeof(int);
#endif
                            stack.Push(pdfDictionaryOrArray);
#if TEST_CODE
                            elementsAdded++;
#endif
                        }
                    }
                }
            }
        }

#if true_  // Not used.
        /// <summary>
        /// Gets the cross-reference to an object used for undefined indirect references.
        /// </summary>
        public PdfReference DeadObject
        {
            get
            {
                if (_deadObject == null)
                {
                    _deadObject = new PdfDictionary(document);
                    Add(_deadObject);
                    _deadObject.Elements.Add("/DeadObjectCount", new PdfInteger());
                }
                return _deadObject.ReferenceNotNull;
            }
        }
        PdfDictionary? _deadObject;
#endif
        /// <summary>
        /// Represents the relation between PdfObjectID and PdfReference for a PdfDocument.
        /// </summary>
        readonly Dictionary<PdfObjectID, PdfReference> _objectTable = [];
    }
}
