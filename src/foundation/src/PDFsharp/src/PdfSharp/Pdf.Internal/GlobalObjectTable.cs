// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections.Generic;

namespace PdfSharp.Pdf.Internal
{
#if true_
    /// <summary>
    /// Provides a thread-local cache for large objects.
    /// </summary>
    internal class GlobalObjectTable_not_in_use
    {
        public GlobalObjectTable_not_in_use()
        { }

        public void AttachDocument(PdfDocument.DocumentHandle handle)
        {
            lo ck (_documentHandles)
            {
                _documentHandles.Add(handle);
            }
        }

        public void DetachDocument(PdfDocument.DocumentHandle handle)
        {
            lo ck (_documentHandles)
            {
                // Notify other documents about detach
                int count = _documentHandles.Count;
                for (int idx = 0; idx < count; idx++)
                {
                    if (((PdfDocument.DocumentHandle)_documentHandles[idx]).IsAlive)
                    {
                        PdfDocument target = ((PdfDocument.DocumentHandle)_documentHandles[idx]).Target;
                        if (target != null)
                            target.OnExternalDocumentFinalized(handle);
                    }
                }

                // Clean up table
                for (int idx = 0; idx < _documentHandles.Count; idx++)
                {
                    PdfDocument target = ((PdfDocument.DocumentHandle)_documentHandles[idx]).Target;
                    if (target == null)
                    {
                        _documentHandles.RemoveAt(idx);
                        idx--;
                    }
                }
            }

            //lo ck (documents)
            //{
            //  int index = IndexOf(document);
            //  if (index != -1)
            //  {
            //    documents.RemoveAt(index);
            //    int count = documents.Count;
            //    for (int idx = 0; idx < count; idx++)
            //    {
            //      PdfDocument target = ((WeakReference)documents[idx]).Target as PdfDocument;
            //      if (target != null)
            //        target.OnExternalDocumentFinalized(document);
            //    }

            //    for (int idx = 0; idx < documents.Count; idx++)
            //    {
            //      PdfDocument target = ((WeakReference)documents[idx]).Target as PdfDocument;
            //      if (target == null)
            //      {
            //        documents.RemoveAt(idx);
            //        idx--;
            //      }
            //    }
            //  }
            //}
        }

        //int IndexOf(PdfDocument.Handle handle)
        //{
        //  int count = documents.Count;
        //  for (int idx = 0; idx < count; idx++)
        //  {
        //    if ((PdfDocument.Handle)documents[idx] == handle)
        //      return idx;
        //    //if (Object.ReferenceEquals(((WeakReference)documents[idx]).Target, document))
        //    //  return idx;
        //  }
        //  return -1;
        //}

        /// <summary>
        /// Array of handles to all documents.
        /// </summary>
        readonly List<object> _documentHandles = new List<object>();
    }
#endif
}
