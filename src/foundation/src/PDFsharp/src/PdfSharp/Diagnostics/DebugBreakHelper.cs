// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if PDFSHARP_DEBUG
#if !DEBUG
#warning "PDFSHARP_DEBUG set in a RELEASE build."
#endif

using PdfSharp.Pdf;

// Missing XML comment for publicly visible type or member because
// this is empira internal stuff not contained in a release build.
#pragma warning disable CS1591 // Internal class

namespace PdfSharp.Diagnostics
{
    /// <summary>
    /// Some flags that can be set or test anywhere in the code
    /// to communicate a particular condition from e.g. test code
    /// into PDFsharp. Some kind of code-based conditional break-points.
    /// MUST NOT COME INTO NUGET PACKAGES!
    /// </summary>
    public static class DebugBreakHelper
    {
        /// <summary>
        /// Gets or sets conditional break point 1.
        /// </summary>
        public static bool ShouldBreak1 { get; set; }

        /// <summary>
        /// Gets or sets conditional break point 2.
        /// </summary>
        public static bool ShouldBreak2 { get; set; }

        /// <summary>
        /// Gets or sets conditional break point 3.
        /// </summary>
        public static bool ShouldBreak3 { get; set; }

        /// <summary>
        /// Gets or sets conditional break point 4.
        /// </summary>
        public static bool ShouldBreak4 { get; set; }

        /// <summary>
        /// Gets or sets conditional break point 5.
        /// </summary>
        public static bool ShouldBreak5 { get; set; }

        //public static void BreakOn1()
        //{
        //    if (ShouldBreak1)
        //        Debugger.Break();
        //}
    }

    /// <summary>
    /// An empira internal class for debugging and testing PDFsharp.
    /// </summary>
    public class PdfSharpDebug
    {
        public void AddObjectId(PdfObjectID id)
            => ObjectIdSet.Add(MakeId(id.ObjectNumber, id.GenerationNumber), null);

        public void AddObjectId(int objectNumber, int generationNumber)
            => ObjectIdSet.Add(MakeId(objectNumber, generationNumber), null);

        public void AddItemNumber(PdfItem item)
            => ItemNumberSet.Add(item.ItemNumber, null);

        public void AddItemNumber(int itemNumber)
            => ItemNumberSet.Add(itemNumber, null);

        public void AddDeadContainer(PdfContainer cont)
        {
            DeadContainers.Add(cont, null);
        }

        public bool IsInObjectIdSet(PdfObjectID id) => ObjectIdSet.ContainsKey(MakeId(id));

        public bool IsInObjectIdSet(int objectNumber, int generationNumber = 0) => ObjectIdSet.ContainsKey(MakeId(objectNumber, generationNumber));

        public bool IsInItemNumberSet(PdfItem item) => ItemNumberSet.ContainsKey(item.ItemNumber);

        public bool IsInItemNumberSet(int itemNumber) => ItemNumberSet.ContainsKey(itemNumber);

        public readonly Dictionary<string, object> Stuff = [];

        // ===

        public bool BreakInCrossReferenceTable { get; set; }

        public bool SaveDocumentWithNoPages { get; set; }

        public bool AllowOpenWithUserPasswordOnly { get; set; }

        public bool SaveImportedDocument { get; set; }

        // ===

        /// <summary>
        /// Gets or sets conditional break point 1.
        /// </summary>
        public bool ShouldBreak1 { get; set; }

        /// <summary>
        /// Gets or sets conditional break point 2.
        /// </summary>
        public bool ShouldBreak2 { get; set; }

        /// <summary>
        /// Gets or sets conditional break point 3.
        /// </summary>
        public bool ShouldBreak3 { get; set; }

        /// <summary>
        /// Gets or sets conditional break point 4.
        /// </summary>
        public bool ShouldBreak4 { get; set; }

        /// <summary>
        /// Gets or sets conditional break point 5.
        /// </summary>
        public bool ShouldBreak5 { get; set; }

        // ===

        public static readonly PdfSharpDebug Instance = new();

        // ===

        long MakeId(int objectNumber, int generationNumber) => objectNumber << 16 + generationNumber;

        long MakeId(PdfObjectID id) => MakeId(id.ObjectNumber, id.GenerationNumber);

        static readonly Dictionary<long, object?> ObjectIdSet = [];

        static readonly Dictionary<int, object?> ItemNumberSet = [];

        static readonly Dictionary<PdfContainer, object?> DeadContainers = [];
    }
}
#endif
