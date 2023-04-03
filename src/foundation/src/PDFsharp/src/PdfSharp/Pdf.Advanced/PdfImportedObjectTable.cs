// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents the imported objects of an external document. Used to cache objects that are
    /// already imported when a PdfFormXObject is added to a page.
    /// </summary>
    sealed class PdfImportedObjectTable
    {
        /// <summary>
        /// Initializes a new instance of this class with the document the objects are imported from.
        /// </summary>
        public PdfImportedObjectTable(PdfDocument owner, PdfDocument externalDocument)
        {
            if (externalDocument == null)
                throw new ArgumentNullException(nameof(externalDocument));
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _externalDocumentHandle = externalDocument.Handle;
            _xObjects = new PdfFormXObject[externalDocument.PageCount];
        }

        readonly PdfFormXObject[] _xObjects;

        /// <summary>
        /// Gets the document this table belongs to.
        /// </summary>
        public PdfDocument Owner => _owner;

        readonly PdfDocument _owner;

        /// <summary>
        /// Gets the external document, or null if the external document is garbage collected.
        /// </summary>
        public PdfDocument? ExternalDocument => _externalDocumentHandle.IsAlive ? _externalDocumentHandle.Target : null;

        readonly PdfDocument.DocumentHandle _externalDocumentHandle;

        public PdfFormXObject GetXObject(int pageNumber)
        {
            return _xObjects[pageNumber - 1];
        }

        public void SetXObject(int pageNumber, PdfFormXObject xObject)
        {
            _xObjects[pageNumber - 1] = xObject;
        }

        /// <summary>
        /// Indicates whether the specified object is already imported.
        /// </summary>
        public bool Contains(PdfObjectID externalID)
        {
            return _externalIDs.ContainsKey(externalID.ToString());
        }

        /// <summary>
        /// Adds a cloned object to this table.
        /// </summary>
        /// <param name="externalID">The object identifier in the foreign object.</param>
        /// <param name="iref">The cross reference to the clone of the foreign object, which belongs to
        /// this document. In general, the clone has a different object identifier.</param>
        public void Add(PdfObjectID externalID, PdfReference iref)
        {
            _externalIDs[externalID.ToString()] = iref;
        }

        /// <summary>
        /// Gets the cloned object that corresponds to the specified external identifier.
        /// </summary>
        public PdfReference this[PdfObjectID externalID] => _externalIDs[externalID.ToString()];

        /// <summary>
        /// Maps external object identifiers to cross reference entries of the importing document
        /// {PdfObjectID -> PdfReference}.
        /// </summary>
        readonly Dictionary<string, PdfReference> _externalIDs = new Dictionary<string, PdfReference>();
    }
}
