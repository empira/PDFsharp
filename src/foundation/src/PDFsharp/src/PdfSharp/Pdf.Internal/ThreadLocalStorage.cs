// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Internal
{
    /// <summary>
    /// Provides a thread-local cache for large objects.
    /// </summary>
    class ThreadLocalStorage // #???
    {
        public ThreadLocalStorage()
        {
            _importedDocuments = new Dictionary<string, PdfDocument.DocumentHandle>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddDocument(string path, PdfDocument document)
        {
            _importedDocuments.Add(path, document.Handle);
        }

        public void RemoveDocument(string path)
        {
            _importedDocuments.Remove(path);
        }

        public PdfDocument GetDocument(string path)
        {
            Debug.Assert(path.StartsWith("*", StringComparison.Ordinal) || Path.IsPathRooted(path), "Path must be full qualified.");

            PdfDocument? document = null;
            if (_importedDocuments.TryGetValue(path, out var handle))
            {
                document = handle.Target;
                if (document == null)
                    RemoveDocument(path);
            }
            if (document == null)
            {
                document = PdfReader.Open(path, PdfDocumentOpenMode.Import);
                _importedDocuments.Add(path, document.Handle);
            }
            return document;
        }

        public PdfDocument[] Documents
        {
            get
            {
                List<PdfDocument> list = new List<PdfDocument>();
                foreach (PdfDocument.DocumentHandle handle in _importedDocuments.Values)
                {
                    if (handle.IsAlive)
                        list.Add(handle.Target ?? NRT.ThrowOnNull<PdfDocument>());
                }
                return list.ToArray();
            }
        }

        public void DetachDocument(PdfDocument.DocumentHandle handle)
        {
            if (handle.IsAlive)
            {
                foreach (string path in _importedDocuments.Keys)
                {
                    if (_importedDocuments[path] == handle)
                    {
                        _importedDocuments.Remove(path);
                        break;
                    }
                }
            }

            // Clean table
            bool itemRemoved = true;
            while (itemRemoved)
            {
                itemRemoved = false;
                foreach (string path in _importedDocuments.Keys)
                {
                    if (!_importedDocuments[path].IsAlive)
                    {
                        _importedDocuments.Remove(path);
                        itemRemoved = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Maps path to document handle.
        /// </summary>
        readonly Dictionary<string, PdfDocument.DocumentHandle> _importedDocuments;
    }
}