﻿// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Reflection;

namespace PdfSharp.Pdf.Security
{
    /// <summary>
    /// Represents the CF dictionary of a security handler.
    /// </summary>
    public class PdfCryptFilters : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfCryptFilters"/> class.
        /// </summary>
        public PdfCryptFilters()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfCryptFilters"/> class.
        /// </summary>
        /// <param name="dict"></param>
        internal PdfCryptFilters(PdfDictionary dict) : base(dict)
        { }

        /// <summary>
        /// Gets the crypt filter with the given name.
        /// </summary>
        public PdfCryptFilter? GetCryptFilter(string name)
        {
            var key = PdfName.AddSlash(name);
            var value = Elements[key];

            return value is null ? null : Convert(value, key);
        }

        /// <summary>
        /// Adds a crypt filter with the given name.
        /// </summary>
        public void AddCryptFilter(string name, PdfCryptFilter cryptFilter)
        {
            var key = PdfName.AddSlash(name);
            Elements[key] = cryptFilter;
        }

        /// <summary>
        /// Removes the crypt filter with the given name.
        /// </summary>
        public bool RemoveCryptFilter(string name)
        {
            var key = PdfName.AddSlash(name);
            return Elements.Remove(key);
        }

        /// <summary>
        /// Enumerates all crypt filters.
        /// </summary>
        public IEnumerable<(string Name, PdfCryptFilter CryptFilter)> GetCryptFilters()
        {
            // Convert() modifies Elements. Avoid "Collection was modified; enumeration operation may not execute" error occuring in net 4.6.2.
            // There is no way to access KeyValuePairs via index natively to use a for loop with.
            // Instead, enumerate Keys and get value via Elements[key], which shall be O(1).
            foreach (var key in Elements.Keys)
            {
                var value = Elements[key]!;
                var name = PdfName.RemoveSlash(key);
                var cryptFilter = Convert(value, key);
                yield return (name, cryptFilter);
            }
        }

        /// <summary>
        /// Returns a dictionary containing all crypt filters.
        /// </summary>
        public Dictionary<string, PdfCryptFilter> GetCryptFiltersAsDictionary()
        {
            return GetCryptFilters().ToDictionary(entry => entry.Name, entry => entry.CryptFilter);
        }

        /// <summary>
        /// Determines whether this instance is empty.
        /// </summary>
        public bool IsEmpty()
        {
            return !Elements.Any();
        }

        /// <summary>
        /// If loaded from file, PdfCryptFilters contains usual PdfDictionaries instead of PdfCryptFilter objects.
        /// This method does the conversion and updates Elements, if desired.
        /// </summary>
        PdfCryptFilter Convert(PdfItem pdfItem, string? keyToUpdate = null)
        {
            if (pdfItem is PdfCryptFilter existingCryptFilter)
                return existingCryptFilter;

            var type = typeof(PdfCryptFilter);

            var ctorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, [typeof(PdfDictionary)], null);
            Debug.Assert(ctorInfo != null, "No appropriate constructor found for type: " + type.Name);
            var cryptFilter = (PdfCryptFilter)ctorInfo.Invoke([pdfItem]);

            if (keyToUpdate is not null)
                Elements[keyToUpdate] = cryptFilter;

            return cryptFilter;
        }
    }
}
