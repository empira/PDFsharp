// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Globalization;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.TestHelper.Analysis.ContentStream;

namespace PdfSharp.TestHelper
{
    public static class PdfFileHelper
    {
        /// <summary>
        /// Gets the content stream of the specified page of the document as string.
        /// </summary>
        public static string GetPageContentStream(PdfDocument pdfDocument, int pageIdx)
        {
            var pdfPage = pdfDocument.Pages[pageIdx];
            var contentReference = (PdfReference)pdfPage.Contents.Elements.Items[0];
            var content = (PdfDictionary)contentReference.Value;
            var contentStream = content.Stream.ToString();

            return contentStream;
        }

        /// <summary>
        /// Gets a ContentStreamEnumerator to inspect the elements inside
        /// the content stream of the specified page of the document.
        /// </summary>
        public static ContentStreamEnumerator GetPageContentStreamEnumerator(PdfDocument pdfDocument, int pageIdx)
        {
            var contentStream = GetPageContentStream(pdfDocument, pageIdx);
            return new ContentStreamEnumerator(contentStream, pdfDocument);
        }

        /// <summary>
        /// Returns the sequence of GlyphIds inside the given hex string.
        /// </summary>
        public static IEnumerable<string> GetHexStringAsGlyphIndices(string hexString)
        {
            // Separate the hex string in its 4 hex digit GlyphIds.
            var glyphIds = hexString.Chunk(4).Select(x =>
            {
                // Remove leading zeros...
                var glyphIndex = new String(x.SkipWhile(c => c == '0').ToArray());
                // ...but remain "0" for "0000".
                if (String.IsNullOrEmpty(glyphIndex))
                    glyphIndex = "0";
                return glyphIndex;
            });
            return glyphIds;
        }

        internal static bool TryParseDouble(string? str, out double result)
        {
            return Double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        }
    }
}
