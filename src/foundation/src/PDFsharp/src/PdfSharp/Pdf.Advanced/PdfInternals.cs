// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Provides access to the internal document data structures.
    /// This class prevents the public interfaces from pollution with too many internal functions.
    /// </summary>
    public class PdfInternals  // TODO: PdfDocumentInternals... PdfPageInternals etc.
    {
        internal PdfInternals(PdfDocument document)
        {
            _document = document;
        }

        readonly PdfDocument _document;

        /// <summary>
        /// Gets or sets the first document identifier.
        /// </summary>
        public string FirstDocumentID
        {
            get => _document.Trailer.GetDocumentID(0);
            set => _document.Trailer.SetDocumentID(0, value);
        }

        /// <summary>
        /// Gets the first document identifier as GUID.
        /// </summary>
        public Guid FirstDocumentGuid => GuidFromString(_document.Trailer.GetDocumentID(0));

        /// <summary>
        /// Gets or sets the second document identifier.
        /// </summary>
        public string SecondDocumentID
        {
            get => _document.Trailer.GetDocumentID(1);
            set => _document.Trailer.SetDocumentID(1, value);
        }

        /// <summary>
        /// Gets the first document identifier as GUID.
        /// </summary>
        public Guid SecondDocumentGuid => GuidFromString(_document.Trailer.GetDocumentID(0));

        Guid GuidFromString(string id)
        {
            if (id is not { Length: 16 })
                return Guid.Empty;

            var guid = new StringBuilder();
            for (int idx = 0; idx < 16; idx++)
                guid.AppendFormat("{0:X2}", (byte)id[idx]);

            return new Guid(guid.ToString());
        }

        /// <summary>
        /// Gets the catalog dictionary.
        /// </summary>
        public PdfCatalog Catalog => _document.Catalog;

        /// <summary>
        /// Gets the ExtGStateTable object.
        /// </summary>
        public PdfExtGStateTable ExtGStateTable => _document.ExtGStateTable;

        /// <summary>
        /// This property is not documented by intention.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public object? UAManager  // @PDF/UA
            => _document._uaManager;

        /// <summary>
        /// Returns the object with the specified Identifier, or null if no such object exists.
        /// </summary>
        public PdfObject? GetObject(PdfObjectID objectID)
        {
            return _document.IrefTable[objectID]?.Value;
        }

        /// <summary>
        /// Maps the specified external object to the substitute object in this document.
        /// Returns null if no such object exists.
        /// </summary>
        public PdfObject? MapExternalObject(PdfObject externalObject)
        {
            PdfFormXObjectTable table = _document.FormTable;
            PdfImportedObjectTable iot = table.GetImportedObjectTable(externalObject.Owner);
            var reference = iot[externalObject.ObjectID];
            return reference == null ? null : reference.Value;
        }

        /// <summary>
        /// Returns the PdfReference of the specified object, or null if the object is not in the
        /// document’s object table.
        /// </summary>
        public static PdfReference? GetReference(PdfObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            return obj.Reference;
        }

        /// <summary>
        /// Gets the object identifier of the specified object.
        /// </summary>
        public static PdfObjectID GetObjectID(PdfObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            return obj.ObjectID;
        }

        /// <summary>
        /// Gets the object number of the specified object.
        /// </summary>
        public static int GetObjectNumber(PdfObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            return obj.ObjectNumber;
        }

        /// <summary>
        /// Gets the generation number of the specified object.
        /// </summary>
        public static int GenerationNumber(PdfObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            return obj.GenerationNumber;
        }

        /// <summary>
        /// Gets all indirect objects ordered by their object identifier.
        /// </summary>
        public PdfObject[] GetAllObjects()
        {
            PdfReference[] irefs = _document.IrefTable.AllReferences;
            int count = irefs.Length;
            PdfObject[] objects = new PdfObject[count];
            for (int idx = 0; idx < count; idx++)
                objects[idx] = irefs[idx].Value;
            return objects;
        }

        /// <summary>
        /// Gets all indirect objects ordered by their object identifier.
        /// </summary>
        [Obsolete("Use GetAllObjects.")]  // Properties should not return arrays
        public PdfObject[] AllObjects => GetAllObjects();

        /// <summary>
        /// Creates the indirect object of the specified type, adds it to the document,
        /// and returns the object.
        /// </summary>
        public T CreateIndirectObject<T>() where T : PdfObject
        {
#if true
            T obj = Activator.CreateInstance<T>();
            _document.IrefTable.Add(obj);
#else
            T result = null;
            ConstructorInfo ctorInfo = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.ExactBinding,
                null, new Type[] { typeof(PdfDocument) }, null);
            if (ctorInfo != null)
            {
                result = (T)ctorInfo.Invoke(new object[] { _document });
                Debug.Assert(result != null);
                AddObject(result);
            }
            Debug.Assert(result != null, "CreateIndirectObject failed with type " + typeof(T).FullName);
#endif
            return obj;
        }

        /// <summary>
        /// Adds an object to the PDF document. This operation and only this operation makes the object 
        /// an indirect object owned by this document.
        /// </summary>
        public void AddObject(PdfObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (obj.Owner == null!)
                obj.Document = _document;
            else if (obj.Owner != _document)
                throw new InvalidOperationException("Object does not belong to this document.");
            _document.IrefTable.Add(obj);
        }

        /// <summary>
        /// Removes an object from the PDF document.
        /// </summary>
        public void RemoveObject(PdfObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (obj.Reference == null)
                throw new InvalidOperationException("Only indirect objects can be removed.");
            if (obj.Owner != _document)
                throw new InvalidOperationException("Object does not belong to this document.");

            _document.IrefTable.Remove(obj.Reference);
        }

        /// <summary>
        /// Returns an array containing the specified object as first element follows by its transitive
        /// closure. The closure of an object are all objects that can be reached by indirect references.
        /// The transitive closure is the result of applying the calculation of the closure to a closure
        /// as long as no new objects came along. This is e.g. useful for getting all objects belonging
        /// to the resources of a page.
        /// </summary>
        public PdfObject[] GetClosure(PdfObject obj)
        {
            return GetClosure(obj, Int32.MaxValue);
        }

        /// <summary>
        /// Returns an array containing the specified object as first element follows by its transitive
        /// closure limited by the specified number of iterations.
        /// </summary>
        public PdfObject[] GetClosure(PdfObject obj, int depth)
        {
            PdfReference[] references = _document.IrefTable.TransitiveClosure(obj, depth);
            int count = references.Length + 1;
            PdfObject[] objects = new PdfObject[count];
            objects[0] = obj;
            for (int idx = 1; idx < count; idx++)
                objects[idx] = references[idx - 1].Value;
            return objects;
        }

        /// <summary>
        /// Writes a PdfItem into the specified stream.
        /// </summary>
        // This function exists to keep PdfWriter and PdfItem.WriteObject internal.
        public void WriteObject(Stream stream, PdfItem item)
        {
            // Never write an encrypted object
            PdfWriter writer = new(stream, _document, null)
            {
                Options = PdfWriterOptions.OmitStream
            };
            item.WriteObject(writer);
        }

        /// <summary>
        /// The name of the custom value key.
        /// </summary>
        public string CustomValueKey = "/PdfSharp.CustomValue";
    }
}
