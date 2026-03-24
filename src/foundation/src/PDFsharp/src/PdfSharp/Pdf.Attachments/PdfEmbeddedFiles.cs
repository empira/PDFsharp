// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Pdf.Attachments
{
    /// <summary>
    /// Represents the name tree /EmbeddedFiles of the document’s catalog /Names dictionary.
    /// </summary>
    public class PdfEmbeddedFiles : PdfNameTreeNode
    {
        // Reference 2.0: 7.7.4  Name dictionary: Table 32 — Entries in the name dictionary / Page 110

        /// <summary>
        /// Initialize a new instance of this class.
        /// </summary>
        public PdfEmbeddedFiles()
        { }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfEmbeddedFiles(PdfDictionary dict)
            : base(dict)
        {
            InitializeExisting();
        }

        void InitializeExisting()
        {
            // Transform /Names values to PdfFileSpecification.
            var names = Names;
            if (names != null)
            {
                for (int idx = 0; idx < names.Count; idx++)
                {
                    var abc = names[0];
                    var item = abc.Value;
                    if (abc.Value is PdfFileSpecification)
                        continue;

                    //var fc = PdfObjectsHelper.TransformItem<PdfFileSpecification>(item);
                    //var fc = PdfObjectsHelper.TransformArrayItem<PdfFileSpecification>(names, idx * 2 + 1);
#if true
                    // #warning TODO Check transformation in ArrayElements.GetItemInternal(...)
                    var fc = names.Elements.GetValue<PdfFileSpecification>(idx * 2 + 1);
#else
                    var fc = PdfObjectsHelper.TransformArrayItem<PdfFileSpecification>(names, idx * 2 + 1);
#endif
                    // /*v*/ar fc


                    // HACK:
                    var test = names.Elements[1];
                }
            }
        }

        /// <summary>
        /// Gets the number of embedded files.
        /// </summary>
        public int FileCount => Names?.Count ?? 0;

        /// <summary>
        /// Gets the PDF file specification of the embedded file with the specified index.
        /// </summary>
        /// <param name="index">The 0-based index.</param>
        public PdfFileSpecification GetFileSpecification(int index)
        {
            var entry = Names?[index];
            if (entry == null)
                throw new InvalidOperationException("Name tree has no /Names entry.");  // #MSG

            var item = entry.Value;
            PdfReference.Dereference(ref item);
            if (item is PdfFileSpecification fs)
            {
                fs.NamesKey = entry.Key.Value;
                return fs;
            }

            // TODO Should not come here.
            return (PdfFileSpecification)item;
        }

        /// <summary>
        /// Gets the PDF file specification of the embedded file with the specified key,
        /// or null, if no file with the key exists..
        /// </summary>
        /// <param name="key">The name of the file.</param>
        public PdfFileSpecification? GetFileSpecification(string key)
        {
            var item = Names?[key];
            if (item == null)
                return null;
            PdfReference.Dereference(ref item);
            if (item is PdfFileSpecification fs)
                return fs;

            Debugger.Break(); // TODO
            return null;
        }

        /// <summary>
        /// Adds a new embedded file defined by a PDF file specification to the document
        /// </summary>
        /// <param name="key">The name of the file.</param>
        /// <param name="fileSpec">A PDF file specification.</param>
        public void AddFileSpecification(string key, PdfFileSpecification fileSpec)
        {
            PdfDocument owner = OwningDocument;

            if (fileSpec.Reference is null)
            {
                // Make object indirect.
                owner.Internals.AddObject(fileSpec);
            }

            // Add it to the name tree.
            if (String.IsNullOrEmpty(key))
                key = fileSpec.Name;
            AddName(key, fileSpec);

            // Add it to the /AF catalog entry.
            // It’s a PDF 2.0 feature, but some producer apps use it in PDF 1.7 documents.
            var af = owner.Catalog.Elements.GetRequiredArray<PdfArrayOfDictionaries>(PdfCatalog.Keys.AF, VCF.Create);
            af.AddDictionary(fileSpec);
        }
    }
}
