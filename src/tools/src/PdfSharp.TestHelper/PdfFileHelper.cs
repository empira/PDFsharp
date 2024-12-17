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

        /// <summary>
        /// Gets the font that is used for a text inside a content stream.
        /// </summary>
        /// <param name="pageIdx">The page to look at.</param>
        /// <param name="pdfDocument">The PdfDocument instance.</param>
        /// <param name="moveToText">An Action that moves the ContentStreamEnumerator to the desired text.</param>
        public static string GetCurrentFontName(int pageIdx, PdfDocument pdfDocument, Action<ContentStreamEnumerator> moveToText)
        {
            var streamEnumerator = PdfFileHelper.GetPageContentStreamEnumerator(pdfDocument, pageIdx);

            Exception CreateCouldNotDetermineFontException(Exception? innerException = null)
            {
                return new InvalidOperationException("Could not determine font of the current text. Check implementation.", innerException);
            }

            // Move to the desired text.
            moveToText(streamEnumerator);

            // Move to previous Tf element.
            if (!streamEnumerator.MovePrevious(s => s == "Tf", true))
                throw CreateCouldNotDetermineFontException();

            // Move two elements back, which should start with "/F"
            if (!streamEnumerator.MovePrevious(s => s.StartsWith("/F"), 2, false))
                throw CreateCouldNotDetermineFontException();

            try
            {
                // This is the font identifier.
                var fontId = streamEnumerator.Current!;

                var page = pdfDocument.Pages[0];

                // Get the font dictionary.
                var fontDict = (PdfDictionary)page.Resources.Elements[PdfResources.Keys.Font]!;

                // Get the font object;
                var fontObject = fontDict.Elements[fontId]!;
                PdfReference.Dereference(ref fontObject);

                // Get the font descriptor.
                var fontDescriptor = ((PdfFont)fontObject).FontDescriptor;

                // Get the font name.
                var fontName = fontDescriptor.FontName;

                // Remove the prefix.
                fontName = fontName[(fontName.IndexOf("+", StringComparison.Ordinal) + 1)..];

                return fontName;
            }
            catch (Exception ex)
            {
                throw CreateCouldNotDetermineFontException(ex);
            }
        }
    }
}
