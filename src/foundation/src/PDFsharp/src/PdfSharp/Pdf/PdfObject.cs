// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Base class of all PDF objects that can be used indirectly.
    /// </summary>
    public abstract class PdfObject : PdfItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfObject"/> class.
        /// </summary>
        protected PdfObject()
        {
            ItemFlags = ItemFlags.IsPrimitiveItem;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfObject"/> class.
        /// </summary>
        protected PdfObject(PdfDocument document, bool createIndirect = false)
        {
            // Calling a virtual member in a constructor is dangerous.
            // In PDFsharp Document is overridden in PdfPage and the code is checked to be safe
            // when called for a not completely initialized object.
            // ReSharper disable once VirtualMemberCallInConstructor
            Document = document;
            ItemFlags = ItemFlags.IsCompoundObject;
            if (createIndirect)
                document.IrefTable.Add(this);
        }

        /// <summary>
        /// Initializes a new instance from an existing object. Used for object type transformation.
        /// </summary>
        protected PdfObject(PdfObject obj)
            : base(obj)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            Document = obj.Owner;

            // If the object that was transformed to an instance of a derived class was an indirect object
            // set the value of the reference to this.
            if (obj._iref != null)
            {
                Debug.Assert(obj.ParentInfo == null);
                // Case: Indirect object is transformed.
                obj._iref.Value = this;
                //TODO: obj._iref = null; ???? Write tests for this case.
            }
#if DEBUG  // TODO: Really debug? A transformed direct object must conserve the structure parent - test this.
            else
            {
                // Case: A direct object like a PdfDictionary is transformed to a derived type.
                _ = typeof(int);

                // This old text is wrong:
                //// If this occurs it is an internal error
                //Debug.Assert(false, "Object type transformation must not be done with direct objects");
            }
#endif
            // ParentInfo keeps null and is set in SetValueInternal.

            ItemFlags = ItemFlags.IsCompoundObject;
        }

        /// <summary>
        /// Creates a copy of this object. The clone does not belong to a document, i.e. its owner and its iref are null.
        /// </summary>
        public new PdfObject Clone() => (PdfObject)Copy();

        /// <summary>
        /// Implements the copy mechanism. Must be overridden in derived classes.
        /// </summary>
        protected override object Copy()
        {
            // Create a shallow copy.
            var obj = (PdfObject)base.Copy();

            // Here we do not know the new owner document.
            obj._document = null!;

            // If we are an indirect object, the caller must set the correct PdfReference
            // for this object.
            obj._iref = null;

            // If we are a direct object, the caller must set the correct new ParentInfo.
            obj._parentInfo = null;

            return obj;
        }

#if true_  // Works, but may lead to other problems that I cannot assess
        /// <summary>
        /// Determines whether the specified object is equal to the current PdfObject.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is PdfObject)
            {
                PdfObject other = (PdfObject)obj;
                // Take object type transformation into account
                if (_iref != null && other._iref != null)
                {
                    Debug.Assert(_iref.Value != null, "iref without value.");
                    Debug.Assert(other.iref.Value != null, "iref without value.");
                    return Object.ReferenceEquals(_iref.Value, other.iref.Value);
                }
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (_iref != null)
            {
                Debug.Assert(_iref.Value != null, "iref without value.");
                return _iref.GetHashCode();
            }
            return base.GetHashCode();
        }
#endif

        /// <summary>
        /// Sets the object and generation number.
        /// Setting the object identifier makes this object an indirect object, i.e. the object gets
        /// a PdfReference entry in the PdfReferenceTable.
        /// </summary>
        internal void SetObjectID(int objectNumber, int generationNumber)
        {
            var objectID = new PdfObjectID(objectNumber, generationNumber);

            _iref ??= _document.IrefTable[objectID];
            if (_iref == null)
            {
                // Re/Sharper disable once ObjectCreationAsStatement because the new object is set to this object.
                // in the constructor of PdfReference.
                PdfReference.CreateFromObject(this, objectID, 0);
                Debug.Assert(_iref != null);
            }
            _iref.Value = this;
            _iref.Document = _document;
        }

        /// <summary>
        /// Gets the PdfDocument this object belongs to.
        /// </summary>
        public virtual PdfDocument Owner => _document;

        internal PdfDocument OwningDocument
        {
            get
            {
                PdfDocument owner;
                if (_document != null!)
                    owner = _document;
                else if (ParentInfo != null)
                    owner = ParentInfo.OwningElements.OwningContainer.Owner;
                else
                    throw new InvalidOperationException(SyMsgs.ObjectWithoutOwner.Message);
                return owner;
            }
        }

        /// <summary>
        /// Sets the PdfDocument this object belongs to.
        /// </summary>
        public virtual PdfDocument Document
        {
            get => _document;
            internal set
            {
                if (!ReferenceEquals(_document, value))
                {
                    if (_document != null)
                        throw new InvalidOperationException("Cannot change document if it was once set.");
                    _document = value;
                    _document2 = value;
                    if (_iref != null)
                        _iref.Document = value;
                }
            }
        }
        PdfDocument _document = null!;
        internal PdfDocument _document2 = null!;  // TODO: Remove after testing

        /// <summary>
        /// Gets the ParentInfo if this object is a direct PDF object.
        /// </summary>
        internal ParentInfo? ParentInfo
        {
            get => _parentInfo;
        }
        ParentInfo? _parentInfo;

        /// <summary>
        /// Sets the ParentInfo if the owning container is a PDF array.
        /// </summary>
        /// <param name="elements">The array elements that owns this PDF object.</param>
        /// <param name="index">The index within the owning PDF array.</param>
        /// <exception cref="InvalidOperationException"></exception>
        internal void SetStructureParent(PdfArray.ArrayElements elements, int index)
        {
            if (_parentInfo != null)
                throw new InvalidOperationException("StructureParent already set.");
            _parentInfo = new(elements, index);
        }

        /// <summary>
        /// Sets the ParentInfo if the owning container is a PDF dictionary.
        /// </summary>
        /// <param name="elements">The dictionary elements that owns this PDF object.</param>
        /// <param name="key">The key within the owning PDF dictionary.</param>
        /// <exception cref="InvalidOperationException"></exception>
        internal void SetStructureParent(PdfDictionary.DictionaryElements elements, string key)
        {
            if (_parentInfo != null)
                throw new InvalidOperationException("StructureParent already set.");
            _parentInfo = new(elements, key);
        }

        internal void SetStructureParentNull()
        {
            if (_parentInfo == null)
                throw new InvalidOperationException("StructureParent already null.");
            _parentInfo = null;
        }

        /// <summary>
        /// Gets or sets the comment for debugging purposes.
        /// </summary>
        public string Comment { get; set; } = "";

        /// <summary>
        /// Indicates whether the object is an indirect object.
        /// </summary>
        public bool IsIndirect => _iref is not null;

        /// <summary>
        /// Gets the PdfInternals object of this document, that grants access to some internal structures
        /// which are not part of the public interface of PdfDocument.
        /// </summary>
        public PdfObjectInternals Internals => _internals ??= new PdfObjectInternals(this);

        PdfObjectInternals? _internals;

        /// <summary>
        /// When overridden in a derived class, prepares the object to get saved.
        /// </summary>
        internal virtual void PrepareForSave()
        { }

        /// <summary>
        /// Saves the stream position. 2nd Edition.
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
            => Debug.Assert(false, "Must not come here, WriteObject must be overridden in all derived classes.");

        /// <summary>
        /// Gets the object identifier. Returns PdfObjectID.Empty for direct objects,
        /// i.e. never returns null.
        /// </summary>
        internal PdfObjectID ObjectID => _iref?.ObjectID ?? PdfObjectID.Empty;

        /// <summary>
        /// Gets the object number.
        /// </summary>
        internal int ObjectNumber => ObjectID.ObjectNumber;

        /// <summary>
        /// Gets the generation number.
        /// </summary>
        internal int GenerationNumber => ObjectID.GenerationNumber;

        ///// <summary>
        ///// Creates a deep copy of the specified value and its transitive closure and adds the
        ///// new objects to the specified owner document.
        ///// </summary>
        /// <param name="owner">The document that owns the cloned objects.</param>
        /// <param name="externalObject">The root object to be cloned.</param>
        /// <returns>The clone of the root object</returns>
        internal static PdfObject DeepCopyClosure(PdfDocument owner, PdfObject externalObject)
        {
            // Get transitive closure.
            var objects = externalObject.Owner.Internals.GetClosure(externalObject);
            int count = objects.Length;
#if DEBUG
            for (int idx = 0; idx < count; idx++)
            {
                var obj = objects[idx];
                if (obj.Reference == null)
                {
                    _ = typeof(int);
                    continue;  // HACK
                }

                // TODO: Assertion fails with 
                //const string Pdf = @"D:/repos/empira/PDFsharp.Tests/assets/user/23-11-01-marionojp-{6FBD7268-26BC-4D71-AE5A-6B3144505CAF}/i-130.pdf";
                Debug.Assert(obj.Reference != null);
                Debug.Assert(obj.Reference!.Document != null);
                Debug.Assert(obj.Owner != null);
                Debug.Assert(obj.ParentInfo != null);
                //if (objects[idx].ObjectID.ObjectNumber == 12)
                //    _ = typeof(int);
            }
#endif
            // 1st loop. Replace all objects by their clones.
            var importedObjectTable = new PdfImportedObjectTable(owner, externalObject.Owner);
            for (int idx = 0; idx < count; idx++)
            {
                var obj = objects[idx];
                var clone = obj.Clone();
                Debug.Assert(clone.Reference == null);
                clone.Document = owner;
                if (obj.Reference != null)
                {
                    // Case: The cloned object was an indirect object.

                    // Add clone to new owner document.
                    owner.IrefTable.Add(clone);

                    // The clone gets an iref by adding it to its new owner.
                    Debug.Assert(clone.Reference != null);

                    // Save an association from old object identifier to new iref.
                    importedObjectTable.Add(obj.ObjectID, clone.Reference);
                }
                else
                {
                    // Case: The cloned object was a direct object.
                    // Only the root object can be a direct object.
                    Debug.Assert(idx == 0);
                }
                // Replace external object by its clone.
                objects[idx] = clone;
            }
#if DEBUG
            for (int idx = 0; idx < count; idx++)
            {
                var obj = objects[idx];
                if (obj.Reference == null)
                {
                    _ = typeof(int);
                    continue;  // HACK
                }

                Debug.Assert(obj.Reference != null);
                Debug.Assert(obj.Reference!.Document != null);
                Debug.Assert(obj.Owner != null);
                if (obj.ObjectID.ObjectNumber == 12)
                    _ = typeof(int);
            }
#endif

            // 2nd loop. Fix up all indirect references that still refer to the import document.
            for (int idx = 0; idx < count; idx++)
            {
                var obj = objects[idx];
                Debug.Assert(obj.Owner == owner);
                FixUpObject(importedObjectTable, owner, obj);
            }

            // Return the clone of the former root object.
            return objects[0];
        }

        ///// <summary>
        ///// Imports an object and its transitive closure to the specified document.
        ///// </summary>
        /// <param name="importedObjectTable">The imported object table of the owner for the external document.</param>
        /// <param name="owner">The document that owns the cloned objects.</param>
        /// <param name="externalObject">The root object to be cloned.</param>
        /// <returns>The clone of the root object</returns>
        internal static PdfObject ImportClosure(PdfImportedObjectTable importedObjectTable, PdfDocument owner, PdfObject externalObject)
        {
            Debug.Assert(ReferenceEquals(importedObjectTable.Owner, owner), "importedObjectTable does not belong to the owner.");
            Debug.Assert(ReferenceEquals(importedObjectTable.ExternalDocument, externalObject.Owner),
                "The ExternalDocument of the importedObjectTable does not belong to the owner of object to be imported.");

            // Get transitive closure of external object.
            PdfObject[] objects = externalObject.Owner.Internals.GetClosure(externalObject);
            int count = objects.Length;
#if DEBUG
            for (int idx = 0; idx < count; idx++)
            {
                if (idx != 0)
                {
                    //Debug.Assert(objects[idx].Reference != null);
                    //Debug.Assert(objects[idx].Reference!.Document != null);
                    var iref = objects[idx].Reference;
                    Debug.Assert(iref != null);
                    Debug.Assert(iref!.Document != null);
                }

                Debug.Assert(objects[idx].Owner != null);
                if (objects[idx].ObjectID.ObjectNumber == 12)
                    _ = typeof(int);
            }
#endif
            // 1st loop. Already imported objects are reused and new ones are cloned.
            for (int idx = 0; idx < count; idx++)
            {
                var obj = objects[idx];
                Debug.Assert(!ReferenceEquals(obj.Owner, owner));

                if (importedObjectTable.Contains(obj.ObjectID))
                {
#if DEBUG_
                    if (obj.ObjectID.ObjectNumber == 5894)
                        _ = typeof(int);
#endif
                    // Case: External object was already imported.

                    PdfReference reference = importedObjectTable[obj.ObjectID];
                    Debug.Assert(reference != null);
                    Debug.Assert(reference.Value != null);
                    Debug.Assert(reference.Document == owner);
                    // Replace external object by the already cloned counterpart.
                    objects[idx] = reference.Value;
                }
                else
                {
                    // Case: External object was not yet imported earlier and must be cloned.

#if DEBUG_
                    if (idx == 6)
                        _ = typeof(int);
#endif
                    var clone = obj.Clone();
                    Debug.Assert(clone.Reference == null);
                    clone.Document = owner;
                    if (obj.Reference != null)
                    {
                        // Case: The cloned object was an indirect object.

                        // Add clone to new owner document.
                        owner.IrefTable.Add(clone);
                        Debug.Assert(clone.Reference != null);
                        // Save an association from old object identifier to new iref.
                        importedObjectTable.Add(obj.ObjectID, clone.Reference);
                    }
                    else
                    {
                        // Case: The cloned object was a direct object.

                        // Only the root object can be a direct object.
                        Debug.Assert(idx == 0);
                    }
                    // Replace external object by its clone.
                    objects[idx] = clone;
                }
            }
#if DEBUG
            for (int idx = 0; idx < count; idx++)
            {
                if (idx != 0)
                {
                    Debug.Assert(objects[idx].IsIndirect == true);
                }
                Debug.Assert(objects[idx].Owner != null);
                //if (elements[idx].ObjectID.ObjectNumber == 12)
                //    _ = typeof(int);
            }
#endif
            // 2nd loop. Fix up indirect references that still refers to the external document.
            for (int idx = 0; idx < count; idx++)
            {
                var obj = objects[idx];
                Debug.Assert(owner != null);
                FixUpObject(importedObjectTable, importedObjectTable.Owner, obj);
            }

            // Return the imported root object.
            return objects[0];
        }

        /// <summary>
        /// Replace all indirect references to external objects by their cloned counterparts
        /// owned by the importer document.
        /// </summary>
        static void FixUpObject(PdfImportedObjectTable iot, PdfDocument owner, PdfObject value)
        {
            Debug.Assert(ReferenceEquals(iot.Owner, owner));

            //PdfDictionary? dict;
            //PdfArray? array;
            //if ((dict = value as PdfDictionary) is not null)
            if (value is PdfDictionary dict)
            {
                // Case: The object is a dictionary.

                // Set document for cloned direct objects.
                if (dict.Owner == null!)
                {
                    // If the dictionary has not yet an owner set the owner to the importing document.
                    dict.Document = owner;
                }
                else
                {
                    // If the dictionary already has an owner it must be the importing document.
                    Debug.Assert(dict.Owner == owner);
                }

                // Search for indirect references in all dictionary elements.
                var names = dict.Elements.KeyNames;
                foreach (var name in names)
                {
                    var item = dict.Elements[name]; // Special treatment for References below. // TODO #US373
                    Debug.Assert(item != null, "A dictionary element cannot be null.");

                    // Is item an iref?
                    if (item is PdfReference iref)
                    {
                        // Case: The item is a reference.

                        // Does the iref already belong to the new owner?
                        if (iref.Document == owner)
                        {
                            // Yes: fine. Happens when an already cloned object is reused.
                            continue;
                        }

                        //Debug.Assert(iref.Document == iot.Document);
                        // No: Replace with iref of cloned object.
                        // The iot maps the external ID to its internal PdfReference.
                        var newXRef = iot[iref.ObjectID];
                        Debug.Assert(newXRef != null);
                        Debug.Assert(newXRef.Document == owner);
                        dict.Elements[name] = newXRef;
                    }
                    else
                    {
                        // Case: The item is not a reference.

                        // If item is an object recursively fix its inner items.
                        if (item is PdfObject pdfObject)
                        {
                            // Fix up inner objects, i.e. recursively walk down the object tree.
                            FixUpObject(iot, owner, pdfObject);
                        }
                        else
                        {
                            // The item is something else, e.g. a name.
                            // Nothing to do.

                            // ...but let’s double-check this case in DEBUG build.
                            DebugCheckNonObjects(item);
                        }
                    }
                }
            }
            //else if ((array = value as PdfArray) is not null)
            else if (value is PdfArray array)
            {
                // Case: The object is an array.
                // Set document for cloned direct objects.
                if (array.Owner == null!)
                {
                    // If the array has not yet an owner set the owner to the importing document.
                    array.Document = owner;
                }
                else
                {
                    // If the array already has an owner it must be the importing document.
                    Debug.Assert(array.Owner == owner);
                }

                // Search for indirect references in all array elements.
                int count = array.Elements.Count;
                for (int idx = 0; idx < count; idx++)
                {
                    var item = array.Elements[idx];
                    Debug.Assert(item != null, "An array element cannot be null.");

                    // Is item an iref?
                    if (item is PdfReference iref)
                    {
                        // Case: The item is a reference.
                        // Does the iref already belongs to the owner?
                        if (iref.Document == owner)
                        {
                            // Yes: fine. Happens when an already cloned object is reused.
                            continue;
                        }

                        //Debug.Assert(iref.Document == iot.ExternalDocument);
                        // No: replace with iref of cloned object.
                        // The iot maps the external ID to its internal PdfReference.
                        var newXRef = iot[iref.ObjectID];
                        Debug.Assert(newXRef != null);
                        Debug.Assert(newXRef.Document == owner);
                        array.Elements[idx] = newXRef;
                    }
                    else
                    {
                        // Case: The item is not a reference.
                        // If item is an object recursively fix its inner items.
                        if (item is PdfObject pdfObject)
                        {
                            // Fix up inner objects, i.e. recursively walk down the object tree.
                            FixUpObject(iot, owner, pdfObject);
                        }
                        else
                        {
                            // The item is something else, e.g. a name.
                            // Nothing to do.

                            // ...but let’s double-check this case in DEBUG build.
                            DebugCheckNonObjects(item);
                        }
                    }
                }
            }
            else
            {
                // Case: The item is some other indirect object.
                // Indirect integers, booleans, etc. are allowed, but PDFsharp do not create them.
                // If such objects occur in imported PDF files from other producers, nothing more is to do.
                // The owner was already set, which is double-checked by the assertions below.
                if (value is PdfNumberObject or PdfNameObject or PdfStringObject or PdfBooleanObject or PdfNumberObject or PdfNullObject)
                {
                    Debug.Assert(value.IsIndirect);
                    Debug.Assert(value.Owner == owner);
                }
                else
                    Debug.Assert(false, "Should not come here. Object is neither a dictionary nor an array.");
            }
        }

        /// <summary>
        /// Ensure for future versions of PDFsharp not to forget code for a new kind of PdfItem.
        /// </summary>
        /// <param name="item">The item.</param>
        [Conditional("DEBUG")]
        static void DebugCheckNonObjects(PdfItem item)
        {
            switch (item)
            {
                case PdfNumber:  // Includes PdfInteger and PdfLongInteger.
                case PdfName:
                case PdfBoolean:
                case PdfString:
                case PdfRectangle:
                case PdfNull:
                    return;

                default:
                    {
#if DEBUG
                        var type = item.GetType();
                        Debug.Assert(type != null, $"CheckNonObjects: Add {type.Name} to the list.");
#endif
                        break;
                    }
            }
        }

        /// <summary>
        /// Gets the indirect reference of this object. If the value is null, this object is a direct object.
        /// </summary>
        public PdfReference? Reference
        {
            get => _iref;

            // Setting the reference outside PDFsharp is not considered as a valid operation.
            internal set => _iref = value;
        }
        PdfReference? _iref;

        /// <summary>
        /// Gets the indirect reference of this object. Throws if it is null.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The indirect reference must be not null here.</exception>
        public PdfReference RequiredReference
            => _iref ?? throw new InvalidOperationException("The indirect reference must be not null here.");

        /// <summary>
        /// Gets the indirect reference of this object. Throws if it is null.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The indirect reference must be not null here.</exception>
        [Obsolete("Use RequiredReference.")]
        public PdfReference ReferenceNotNull
            => RequiredReference;

        /// <summary>
        /// Gets the actual reference.
        /// This function exists only for PdfWidgetAnnotations that are a part of an AcroField.
        /// </summary>
        internal virtual PdfReference? ActualReference => _iref;
    }
}
