// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections.Generic;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Base class for all dictionaries that map resource names to objects.
    /// </summary>
    class PdfResourceMap : PdfDictionary //, IEnumerable
    {
        public PdfResourceMap()
        { }

        public PdfResourceMap(PdfDocument document)
            : base(document)
        { }

        protected PdfResourceMap(PdfDictionary dict)
            : base(dict)
        { }

        //    public int Count
        //    {
        //      get {return resources.Count;}
        //    }
        //
        //    public PdfObject this[string key]
        //    {
        //      get {return resources[key] as PdfObject;}
        //      set {resources[key] = value;}
        //    }

        /// <summary>
        /// Adds all imported resource names to the specified hashtable.
        /// </summary>
        internal void CollectResourceNames(Dictionary<string, object?> usedResourceNames)
        {
            // ?TODO: Imported resources (e.g. fonts) can be reused, but I think this is rather difficult. Will be an issue in PDFsharp 2.0.
            PdfName[] names = Elements.KeyNames;
            foreach (PdfName name in names)
                usedResourceNames.Add(name.ToString(), null);
        }
    }
}
