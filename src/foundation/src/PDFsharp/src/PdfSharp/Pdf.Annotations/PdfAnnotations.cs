// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections;
using PdfSharp.Pdf.Forms;
using PdfSharp.Pdf.Advanced;

// v7.0.0 TODO review 

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents the annotations array of a page.
    /// </summary>
    public sealed class PdfAnnotations : PdfArray
    {
        // Reference 2.0: /Annots key in table 31 — Entries in a page object / Page 105

        internal PdfAnnotations(PdfDocument document)
            : base(document)
        { }

        internal PdfAnnotations(PdfArray array)
            : base(array)
        { }

        /// <summary>
        /// Adds the specified annotation.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        public void Add(PdfAnnotation annotation)
        {
            annotation.Document = Owner;
            annotation.Page = Page;
            Elements.Add(annotation.RequiredReference);
        }

        /// <summary>
        /// Removes an annotation from the document.
        /// </summary>
        public void Remove(PdfAnnotation annotation)
        {
            if (annotation.Owner != Owner)
                throw new InvalidOperationException("The annotation does not belong to this document.");

            Elements.Remove(annotation.RequiredReference);
        }

        /// <summary>
        /// Removes all the annotations from the current page.
        /// </summary>
        public void Clear()
        {
            for (int idx = Count - 1; idx >= 0; idx--)
                //Page.Annotations.Remove(Page.Annotations[idx]); // ???
                Elements.RemoveAt(idx);
        }

        /// <summary>
        /// Gets the number of annotations in this collection.
        /// </summary>
        public int Count => Elements.Count;

        /// <summary>
        /// Gets the <see cref="PdfSharp.Pdf.Annotations.PdfAnnotation"/> at the specified index.
        /// </summary>
        public PdfAnnotation this[int index]
        {
            get
            {
                PdfReference? iref;
                PdfDictionary dict;
                var item = Elements[index];
                if ((iref = item as PdfReference) != null)
                {
                    Debug.Assert(iref.Value is PdfDictionary, "Reference to dictionary expected.");
                    dict = (PdfDictionary)iref.Value;
                }
                else
                {
                    Debug.Assert(item is PdfDictionary, "Dictionary expected.");
                    dict = (PdfDictionary)item;
                }
                var annotation = dict as PdfAnnotation;
                if (annotation == null)
                {
                    //annotation = new PdfGenericAnnotation(dict);
                    annotation = PdfAnnotation.CreateAnnotation(dict, Page);
                    if (iref == null)
                        Elements[index] = annotation;
                }
                return annotation;
            }
        }

        /// <summary>
        /// Gets the page the annotations belongs to.
        /// </summary>
        internal PdfPage Page
        {
            get => _page ?? NRT.ThrowOnNull<PdfPage>();
            set => _page = value;
        }
        PdfPage? _page;

        internal void DeriveInstances()
        {
            int count = Elements.Count;
            for (int idx = 0; idx < count; idx++)
            {
                var dict = Elements.GetRequiredDictionary(idx);
                if (dict is PdfAnnotation)
                    continue;
                dict = PdfAnnotation.CreateAnnotation(dict, _page);
                Elements[idx] = dict;
            }
        }

        /// <summary>
        /// Fixes the /P element in imported annotation.
        /// </summary>
        internal static void FixImportedAnnotation(PdfPage page)
        {
            var annotations = page.Elements.GetArray(PdfPage.Keys.Annots);
            if (annotations != null)
            {
                int count = annotations.Elements.Count;
                for (int idx = 0; idx < count; idx++)
                {
                    var annot = annotations.Elements.GetDictionary(idx);
                    if (annot != null && annot.Elements.ContainsKey(PdfAnnotation.Keys.P))
                        annot.Elements[PdfAnnotation.Keys.P] = page;
                }
            }
            // DELETE
            //var annots = page.Elements.GetArray(PdfPage.Keys.Annots);
            //if (annots != null)
            //{
            //    int count = annots.Elements.Count;
            //    for (int idx = 0; idx < count; idx++)
            //    {
            //        var annot = annots.Elements.GetDictionary(idx);
            //        if (annot != null && annot.Elements.ContainsKey("/P"))
            //            annot.Elements["/P"] = page.RequiredReference;
            //    }
            //}
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        public override IEnumerator<PdfItem> GetEnumerator()
            => new AnnotationsIterator(this);

        class AnnotationsIterator(PdfAnnotations annotations) : IEnumerator<PdfItem /*PdfAnnotation*/>
        {
            public PdfItem/*PdfAnnotation*/ Current => annotations[_index];

            object IEnumerator.Current => Current;

            public bool MoveNext() => ++_index < annotations.Count;

            public void Reset() => _index = -1;

            public void Dispose()
            { }

            int _index = -1;
        }

        internal static class AnnotationPreparer
        {
            public static void PrepareDocument(PdfDocument doc)
            {
                CreateAnnotationObjects(doc);
            }

            static void CreateAnnotationObjects(PdfDocument doc)
            {
                var pages = doc.Catalog.Pages;
                foreach (var page in pages)
                {
                    var annots = page.Elements.GetArray(PdfPage.Keys.Annots);
                    if (annots != null)
                    {
                        var count = annots.Elements.Count;
                        for (int idx = 0; idx < count; idx++)
                        {
                            var annot = annots.Elements.GetDictionary(idx);

                            // Already handled in Acro fields.
                            if (annot is PdfWidgetAnnotation widget)
                                continue;

                            Debug.Assert(annot is not PdfFormField);

                            if (annot != null)
                            {
                                Debug.Assert(annot.IsIndirect);

                                var type = annot.GetType();
                                if (type != typeof(PdfDictionary))
                                    throw new InvalidOperationException("Not a dictionary???");

                                PdfAnnotation.CreateAnnotation(annot);
                            }
                            else
                            {
                                throw new InvalidOperationException("Not an annotation???");
                            }
                        }
                    }
                }
            }
        }
    }
}
