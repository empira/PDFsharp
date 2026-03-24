// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Attachments
{
    /// <summary>
    /// Sums up all relevant information of an embedded file in a PDF file.
    /// </summary>
    public class EmbeddedFileInfo
    {
        /// <summary>
        /// The key of the PdfFileSpecification in the /Names array of the /EmbeddedFiles name tree.
        /// </summary>
        public string NamesKey { get; set; } = "";

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string FileName { get; set; } = "";

        /// <summary>
        /// Gets or sets the file mime type.
        /// </summary>
        public string FileType { get; set; } = "";

        /// <summary>
        /// Gets or sets an optional description of the file.
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Gets or sets the optional creation time of the file.
        /// </summary>
        public DateTimeOffset? CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the optional modification time of the file.
        /// </summary>
        public DateTimeOffset? ModificationTime { get; set; }

        /// <summary>
        /// Gets or sets the bytes of the file.
        /// </summary>
        public byte[] Data { get; set; } = [];

        /// <summary>
        /// Gets or sets the PdfAFRelationship of the file.
        /// See PDF specification for further details.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string AFRelationship { get; set; } = PdfAFRelationship.Unspecified;
    }
}
