// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Metadata
{
    /// <summary>
    /// Defines how PDFsharp treads document metadata.
    /// </summary>
    public enum DocumentMetadataStrategy
    {
        /// <summary>
        /// Do not generate metadata.
        /// Keep the existing metadata of an imported PDF file.
        /// </summary>
        KeepExisting,

        /// <summary>
        /// Do not generate metadata.
        /// </summary>
        NoMetadata,

        /// <summary>
        /// Let PDFsharp generate a new metadata object.
        /// </summary>
        AutoGenerate,

        /// <summary>
        /// Let the user code generate the metadata.
        /// </summary>
        UserGenerated,
    }
}
