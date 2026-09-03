// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Attachments;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.C2pa
{
    /// <summary>
    /// Provides functionality to read C2PA content credentials manifests embedded in a PDF document.
    /// </summary>
    public class C2paManager : ManagerBase
    {
        C2paManager(PdfDocument document) : base(document)
        { }

        /// <summary>
        /// Gets a value indicating whether the document contains at least one C2PA manifest.
        /// </summary>
        public bool HasManifests => Manifests.Count > 0;

        /// <summary>
        /// Gets the number of C2PA manifests found in the document.
        /// </summary>
        public int ManifestCount => Manifests.Count;

        /// <summary>
        /// Gets information about all C2PA manifests embedded in the document.
        /// </summary>
        public IReadOnlyList<C2paManifestInfo> Manifests => _manifests ??= ScanManifests();

        List<C2paManifestInfo>? _manifests;

        List<C2paManifestInfo> ScanManifests()
        {
            var result = new List<C2paManifestInfo>();
            var seen = new HashSet<PdfObjectID>();
            var catalog = Document.Catalog;

            // The C2PA spec requires the manifest to be listed in the catalog's /AF array, but some
            // producers only add it to the /Names/EmbeddedFiles name tree. The name tree is the only
            // place we can get the /Names key from, so scan it first.
            if (catalog.HasNames)
            {
                var names = catalog.Names;
                if (names.HasEmbeddedFiles)
                {
                    var namesNode = names.GetEmbeddedFiles()?.Names;
                    if (namesNode != null)
                    {
                        var count = namesNode.Count;
                        for (var idx = 0; idx < count; idx++)
                        {
                            var entry = namesNode[idx];
                            TryAddManifest(entry.Value, entry.Key.Value, result, seen);
                        }
                    }
                }
            }

            // Scan /AF as well to catch manifests that are not listed in the name tree.
            if (catalog.Elements.TryGetArray<PdfArrayOfDictionaries>(PdfCatalog.Keys.AF, out var afArray))
            {
                var count = afArray.Elements.Count;
                for (var idx = 0; idx < count; idx++)
                    TryAddManifest(afArray.Elements[idx], "", result, seen);
            }

            return result;
        }

        static void TryAddManifest(PdfItem item, string namesKey, List<C2paManifestInfo> result, HashSet<PdfObjectID> seen)
        {
            PdfReference.Dereference(ref item);
            if (item is not PdfDictionary fileSpec)
                return;

            if (!fileSpec.Elements.TryGetString(PdfFileSpecification.Keys.AFRelationship, out var relationship))
                return;
            // /AFRelationship is written as a PDF name, whose value includes the leading slash.
            var relationshipName = relationship.Length > 0 && relationship[0] == '/' ? relationship[1..] : relationship;
            if (relationshipName != PdfAFRelationship.C2paManifest)
                return;

            var ef = fileSpec.Elements.GetDictionary(PdfFileSpecification.Keys.EF);
            var stream = ef?.Elements.GetDictionary("/F") ?? ef?.Elements.GetDictionary("/UF");
            if (stream?.Stream == null)
                return;

            // The same manifest can be reachable through both the name tree and /AF.
            if (!seen.Add(stream.ObjectID))
                return;

            if (!fileSpec.Elements.TryGetString(PdfFileSpecification.Keys.UF, out var fileName) || fileName.Length == 0)
                fileSpec.Elements.TryGetString(PdfFileSpecification.Keys.F, out fileName);
            fileName ??= "";

            fileSpec.Elements.TryGetString(PdfFileSpecification.Keys.Desc, out var description);
            description ??= "";

            // /Subtype is not a defined file specification key, but producers put the mime type here anyway.
            if (!fileSpec.Elements.TryGetString("/Subtype", out var mimeType) || mimeType.Length == 0)
                stream.Elements.TryGetString("/Subtype", out mimeType);
            mimeType ??= "";

            result.Add(new C2paManifestInfo
            {
                NamesKey = namesKey,
                FileName = fileName,
                Description = description,
                MimeType = mimeType,
                AFRelationship = relationshipName,
                Data = stream.Stream.UnfilteredValue,
                ObjectID = stream.ObjectID
            });
        }

        /// <summary>
        /// Gets or creates the C2paManager for the specified document.
        /// </summary>
        public static C2paManager ForDocument(PdfDocument document)
            => document.C2paManager ??= new(document);
    }
}
