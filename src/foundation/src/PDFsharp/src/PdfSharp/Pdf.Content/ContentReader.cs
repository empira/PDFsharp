// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.IO;
using PdfSharp.Pdf.Content.Objects;

namespace PdfSharp.Pdf.Content
{
    /// <summary>
    /// Represents the functionality for reading PDF content streams.
    /// </summary>
    public static class ContentReader
    {
        /// <summary>
        /// Reads the content stream(s) of the specified page.
        /// </summary>
        /// <param name="page">The page.</param>
        static public CSequence ReadContent(PdfPage page)
        {
            CParser parser = new CParser(page);
            CSequence sequence = parser.ReadContent();

            return sequence;
        }

        /// <summary>
        /// Reads the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        static public CSequence ReadContent(byte[] content)
        {
            CParser parser = new CParser(content);
            CSequence sequence = parser.ReadContent();
            return sequence;
        }

        /// <summary>
        /// Reads the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        static public CSequence ReadContent(MemoryStream content)
        {
            CParser parser = new CParser(content);
            CSequence sequence = parser.ReadContent();
            return sequence;
        }
    }
}
