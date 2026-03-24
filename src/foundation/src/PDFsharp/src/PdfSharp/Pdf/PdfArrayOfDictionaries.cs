// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a PdfArray of PDF dictionaries.
    /// Used e.g. for the /AF entry in the PDF catalog.
    /// </summary>
    public class PdfArrayOfDictionaries : PdfArray
    {
        /// <summary>
        /// Initialize a new instance of this class.
        /// </summary>
        public PdfArrayOfDictionaries()
        { }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public PdfArrayOfDictionaries(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance from an existing object. Used for object type transformation.
        /// </summary>
        public PdfArrayOfDictionaries(PdfArray array)
            : base(array)
        { }

        /// <summary>
        /// Adds a PDF dictionary to the array.
        /// The dictionary must be an indirect object.
        /// </summary>
        /// <param name="dict">The indirect dictionary to add.</param>
        public void AddDictionary(PdfDictionary dict)
        {
            var reference = dict.Reference
                            ?? throw new ArgumentException("Dictionary must be an indirect object.", nameof(dict));
            if (Elements.Contains(reference))
                throw new InvalidOperationException("Dictionary already in array.");
            Elements.Add(reference);
        }

        /// <summary>
        /// Removes a PDF dictionary from the array.
        /// Returns true if the dictionary was successfully removed, false otherwise.
        /// </summary>
        /// <param name="dict"></param>
        public bool RemoveDictionary(PdfDictionary dict)
        {
            var reference = dict.Reference
                            ?? throw new ArgumentException("Dictionary must be an indirect object.", nameof(dict));

            var result = Elements.Remove(reference);
            return result;
        }
    }
}
