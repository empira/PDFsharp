// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a reference to a PdfDictionary.DictionaryElements or PdfArray.ArrayElements
    /// a direct PdfObject belongs to. Only a direct PdfObject can have a structure parent.
    /// </summary>
    internal class ParentInfo
    {
        /// <summary>
        /// Creates a new ParentInfo instance for a PdfObject that is owned by a PdfDictionary.
        /// </summary>
        /// <param name="elements">The dictionary elements that owns the owned object belongs to.</param>
        /// <param name="key">The key in the owning dictionary.</param>
        public ParentInfo(PdfDictionary.DictionaryElements elements, string key)
        {
            OwningElements = elements;
            Key = key;
        }

        /// <summary>
        /// Creates a new ParentInfo instance for a PdfObject that is owned by a PdfArray.
        /// </summary>
        /// <param name="elements">The array elements that owns the owned object belongs to.</param>
        /// <param name="index">The index in the owning array.</param>
        public ParentInfo(PdfArray.ArrayElements elements, int index)
        {
            OwningElements = elements;
            Index = index;
        }

        /// <summary>
        /// Returns true if this ParentInfo belongs to a PdfArray, false otherwise.
        /// </summary>
        public bool IsArray => Index != -1;

        /// <summary>
        /// Returns true if this ParentInfo belongs to a PdfDictionary, false otherwise.
        /// </summary>
        public bool IsDictionary => Key.Length > 0;

        /// <summary>
        /// Gets the index of the PdfObject in the PDF array if it is owned by an array,
        /// -1 otherwise.
        /// </summary>
        public int Index { get; private set; } = -1;

        /// <summary>
        /// Gets the of the PdfObject in the PDF dictionary if it is owned by a dictionary,
        /// empty string otherwise.
        /// </summary>
        public string Key { get; } = "";

        public ElementsBase OwningElements { get; private set; }

        /// <summary>
        /// Gets the owning PdfArray if the PdfObject is owned by an array,
        /// null otherwise.
        /// </summary>
        public PdfArray OwningArray => IsArray ? (PdfArray)OwningElements.OwningContainer : null!;

        /// <summary>
        /// Gets the owning PdfDictionary if the PdfObject is owned by a dictionary,
        /// null otherwise.
        /// </summary>
        public PdfDictionary OwningDictionary => IsDictionary ? (PdfDictionary)OwningElements.OwningContainer : null!;

        /// <summary>
        /// Adjust the index if the owning array is modified.
        /// This happens when an item is inserted or deleted from an array before this index.
        /// </summary>
        /// <param name="offset">The correction value added to the current index.</param>
        internal void AdjustIndex(int offset)
        {
            Debug.Assert(IsArray);
            Index += offset;
            Debug.Assert(Index >= 0 && Index < ((PdfArray.ArrayElements)OwningElements).Count);
        }
    }
}
