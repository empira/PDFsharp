// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.C2pa
{
    /// <summary>
    /// Sums up all relevant information of a C2PA content credentials manifest embedded in a PDF file.
    /// </summary>
    public class C2paManifestInfo
    {
        /// <summary>
        /// The key of the file specification in the /Names array of the /EmbeddedFiles name tree,
        /// or the empty string if the manifest was only found via the catalog's /AF array.
        /// </summary>
        public string NamesKey { get; internal set; } = "";

        /// <summary>
        /// Gets the name of the file, taken from the file specification's /UF entry, falling back to /F.
        /// </summary>
        public string FileName { get; internal set; } = "";

        /// <summary>
        /// Gets the optional description of the file specification.
        /// </summary>
        public string Description { get; internal set; } = "";

        /// <summary>
        /// Gets the mime type of the manifest, taken from the file specification's /Subtype entry,
        /// falling back to the /Subtype entry of the embedded file stream.
        /// </summary>
        public string MimeType { get; internal set; } = "";

        /// <summary>
        /// Gets the /AFRelationship value of the file specification.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string AFRelationship { get; internal set; } = "";

        /// <summary>
        /// Gets the raw bytes of the C2PA manifest (JUMBF box), i.e. the unfiltered content
        /// of the embedded file stream.
        /// </summary>
        public byte[] Data { get; internal set; } = [];

        /// <summary>
        /// The object ID of the embedded file stream, used to detect duplicates when the
        /// same manifest is referenced from both the name tree and the /AF array.
        /// </summary>
        internal PdfObjectID ObjectID { get; init; }
    }
}
