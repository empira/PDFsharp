// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents the annotations array of a page.
    /// </summary>
    public sealed class PdfAnnotations : PdfArray
    {
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
            Owner.IrefTable.Add(annotation);
            Elements.Add(annotation.ReferenceNotNull);
        }

        /// <summary>
        /// Removes an annotation from the document.
        /// </summary>
        public void Remove(PdfAnnotation annotation)
        {
            if (annotation.Owner != Owner)
                throw new InvalidOperationException("The annotation does not belong to this document.");

            Owner.Internals.RemoveObject(annotation);
            Elements.Remove(annotation.ReferenceNotNull);
        }

        /// <summary>
        /// Removes all the annotations from the current page.
        /// </summary>
        public void Clear()
        {
            for (int idx = Count - 1; idx >= 0; idx--)
                Page.Annotations.Remove(Page.Annotations[idx]);
        }

        //public void Insert(int index, PdfAnnotation annotation)
        //{
        //  annotation.Document = Document;
        //  annotations.Insert(index, annotation);
        //}

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
                    annotation = new PdfGenericAnnotation(dict);
                    if (iref == null)
                        Elements[index] = annotation;
                }
                return annotation;
            }
        }

        //public PdfAnnotation this[int index]
        //{
        //  get 
        //  {
        //      //DMH 6/7/06
        //      //Broke this out to simplify debugging
        //      //Use a generic annotation to access the metadata
        //      //Assign this as the parent of the annotation
        //      PdfReference r = Elements[index] as PdfReference;
        //      PdfDictionary d = r.Value as PdfDictionary;
        //      PdfGenericAnnotation a = new PdfGenericAnnotation(d);
        //      a.Collection = this;
        //      return a;
        //  }
        //}

        /// <summary>
        /// Gets the page the annotations belongs to.
        /// </summary>
        internal PdfPage Page
        {
            get => _page ?? NRT.ThrowOnNull<PdfPage>();
            set => _page = value;
        }
        PdfPage? _page;

        /// <summary>
        /// Fixes the /P element in imported annotation.
        /// </summary>
        internal static void FixImportedAnnotation(PdfPage page)
        {
            var annots = page.Elements.GetArray(PdfPage.Keys.Annots);
            if (annots != null)
            {
                int count = annots.Elements.Count;
                for (int idx = 0; idx < count; idx++)
                {
                    var annot = annots.Elements.GetDictionary(idx);
                    if (annot != null && annot.Elements.ContainsKey("/P"))
                        annot.Elements["/P"] = page.Reference;
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        public override IEnumerator<PdfItem> GetEnumerator()
        {
            return (IEnumerator<PdfItem>)new AnnotationsIterator(this);
        }

        class AnnotationsIterator : IEnumerator<PdfItem/*PdfAnnotation*/>
        {
            public AnnotationsIterator(PdfAnnotations annotations)
            {
                _annotations = annotations;
                _index = -1;
            }

            public PdfItem/*PdfAnnotation*/ Current => _annotations[_index];

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                return ++_index < _annotations.Count;
            }

            public void Reset()
            {
                _index = -1;
            }

            public void Dispose()
            {
                //throw new NotImplementedException();
            }

            readonly PdfAnnotations _annotations;
            int _index;
        }
    }
}
