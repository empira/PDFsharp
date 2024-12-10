// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Actions;
using System.Xml.Linq;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents named destinations as specified by the document catalog’s /Dest entry.
    /// </summary>
    public sealed class PdfNamedDestinations : PdfDictionary
    {
        internal PdfNamedDestinations()
        {
        }

        internal PdfNamedDestinations(PdfDictionary dict)
            : base(dict)
        {
        }

        /// <summary>
        /// Gets all the destination names.
        /// </summary>
        public IEnumerable<string> Names => Elements.Keys;

        /// <summary>
        /// Determines whether a destination with the specified name exists.
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <returns>True, if name is found, false otherwise</returns>
        public bool Contains(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            return Elements.ContainsKey(name.StartsWith('/') ? name : '/' + name);
        }

        /// <summary>
        /// Gets the destination with the specified name.
        /// </summary>
        /// <param name="name">The name of the destination</param>
        /// <returns>A <see cref="PdfArray"/> representing the destination
        /// or null if <paramref name="name"/> does not exist.</returns>
        public PdfArray? GetDestination(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            name = name.StartsWith('/') ? name : '/' + name;

            var dict = Elements.GetDictionary(name);
            if (dict != null)
            {
                return dict.Elements.GetArray(PdfGoToAction.Keys.D);
            }
            return Elements.GetArray(name);
        }
    }
}
