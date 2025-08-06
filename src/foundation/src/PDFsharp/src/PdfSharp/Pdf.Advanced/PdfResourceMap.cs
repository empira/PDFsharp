// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections.Generic;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Base class for all dictionaries that map resource names to objects.
    /// </summary>
    class PdfResourceMap : PdfDictionary
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
            // ?TODO_OLD: Imported resources (e.g. fonts) can be reused, but I think this is rather difficult.
            var names = Elements.KeyNames;
            foreach (var name in names)
            {
                // We found a PDF document where the names of the resources of a page are not different in pairs.
                // The page used the name /R1 for both a font and a graphic state.
                // So we now check first if it already exists in the collection.
                var resName = name.ToString();
                // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd because it is not available in .NET Standard
                if (!usedResourceNames.ContainsKey(resName))
                    usedResourceNames.Add(resName, null);
            }
        }
    }
}
