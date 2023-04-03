// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Text;

namespace MigraDoc.DocumentObjectModel.Visitors
{
    /// <summary>
    /// Flattens a document for PDF rendering.
    /// </summary>
    public class PdfFlattenVisitor : VisitorBase
    {
        /// <summary>
        /// Initializes a new instance of the PdfFlattenVisitor class.
        /// </summary>
        public PdfFlattenVisitor()
        { }

        internal override void VisitDocumentElements(DocumentElements elements)
        {
#if true
            // New version without sorted list.
            int count = elements.Count;
            for (int idx = 0; idx < count; ++idx)
            {
                if (elements[idx] is Paragraph paragraph)
                {
                    var paragraphs = paragraph.SplitOnParaBreak();
                    if (paragraphs != null)
                    {
                        foreach (var para in paragraphs)
                        {
                            elements.InsertObject(idx++, para);
                            ++count;
                        }
                        elements.RemoveObjectAt(idx--);
                        --count;
                    }
                }
            }
#else
            SortedList splitParaList = new SortedList();

            for (int idx = 0; idx < elements.Count; ++idx)
            {
                Paragraph paragraph = elements[idx] as Paragraph;
                if (paragraph != null)
                {
                    Paragraph[] paragraphs = paragraph.SplitOnParaBreak();
                    if (paragraphs != null)
                        splitParaList.Add(idx, paragraphs);
                }
            }

            int insertedObjects = 0;
            for (int idx = 0; idx < splitParaList.Count; ++idx)
            {
                int insertPosition = (int)splitParaList.GetKey(idx);
                Paragraph[] paragraphs = (Paragraph[])splitParaList.GetByIndex(idx);
                foreach (Paragraph paragraph in paragraphs)
                {
                    elements.InsertObject(insertPosition + insertedObjects, paragraph);
                    ++insertedObjects;
                }
                elements.RemoveObjectAt(insertPosition + insertedObjects);
                --insertedObjects;
            }
#endif
        }

        internal override void VisitDocumentObjectCollection(DocumentObjectCollection elements)
        {
            List<int> textIndices = new List<int>();
            if (elements is ParagraphElements)
            {
                for (int idx = 0; idx < elements.Count; ++idx)
                {
                    if (elements[idx] is Text)
                        textIndices.Add(idx);
                }
            }

            int[] indices = textIndices.ToArray();

            int insertedObjects = 0;
            var currentString = new StringBuilder();
            foreach (int idx in indices)
            {
                Text? text = elements[idx + insertedObjects] as Text;
                string content = text?.Content ?? "";
                currentString.Clear();
                foreach (char ch in content)
                {
                    // TODO Add support for other breaking spaces (en space, em space, &c.).
                    switch (ch)
                    {
                        case ' ':
                        case '\r':
                        case '\n':
                        case '\t':
                            if (currentString.Length > 0)
                            {
                                elements.InsertObject(idx + insertedObjects, new Text(currentString.ToString()));
                                ++insertedObjects;
                                currentString.Clear();
                            }
                            elements.InsertObject(idx + insertedObjects, new Text(" "));
                            ++insertedObjects;
                            break;

                        case '-': // minus.
                            currentString.Append('-');
                            elements.InsertObject(idx + insertedObjects, new Text(currentString.ToString()));
                            ++insertedObjects;
                            currentString.Clear();
                            break;

                        // Characters that allow line breaks without indication.
                        case '\u200B': // zero width space.
                        case '\u200C': // zero width non-joiner.
                            if (currentString.Length > 0)
                            {
                                elements.InsertObject(idx + insertedObjects, new Text(currentString.ToString()));
                                ++insertedObjects;
                                currentString.Clear();
                            }
                            break;

                        case '\u00AD': // soft hyphen.
                            if (currentString.Length > 0)
                            {
                                elements.InsertObject(idx + insertedObjects, new Text(currentString.ToString()));
                                ++insertedObjects;
                                currentString.Clear();
                            }
                            elements.InsertObject(idx + insertedObjects, new Text("\u00AD"));
                            ++insertedObjects;
                            //currentString = "";
                            break;

                        default:
                            currentString.Append(ch);
                            break;
                    }
                }
                if (currentString.Length > 0)
                {
                    elements.InsertObject(idx + insertedObjects, new Text(currentString.ToString()));
                    ++insertedObjects;
                }
                elements.RemoveObjectAt(idx + insertedObjects);
                --insertedObjects;
            }
        }

        internal override void VisitFormattedText(FormattedText formattedText)
        {
            var document = formattedText.Document;
            ParagraphFormat? format = null;

            var style = document.Styles[formattedText.Values.Style]; // BUG??? We get "null" for "null".
            if (style != null)
                format = style.Values.ParagraphFormat;
            else if (!String.IsNullOrEmpty(formattedText.Values.Style) /*!= ""*/) // BUG??? Treat "null" like empty string.
                format = document.Styles[StyleNames.InvalidStyleName]?.Values.ParagraphFormat ?? throw new InvalidOperationException("Style does not exist.");

            if (format != null)
            {
                if (formattedText.Values.Font is null && format.Values.Font is not null)
                    formattedText.Font = format.Values.Font.Clone();
                else if (format.Values.Font is not null)
                    FlattenFont(formattedText.Values.Font, format.Values.Font);
            }

            var parentFont = GetParentFont(formattedText);

            if (formattedText.Values.Font == null && parentFont != null)
                formattedText.Font = parentFont.Clone();
            else if (parentFont != null)
                FlattenFont(formattedText.Values.Font, parentFont);
        }

        internal override void VisitHyperlink(Hyperlink hyperlink)
        {
            // If NoHyperlinkStyle is set to true, the Hyperlink shall look like the surrounding text without using the Hyperlink Style.
            // May be used for text references, e. g. in tables, which shall not be rendered as links.
            if (!hyperlink.NoHyperlinkStyle)
            {
                var styleFont = hyperlink.Document.Styles[StyleNames.Hyperlink]!.Font; // BUG ??? "!"
                if (hyperlink.Values.Font is null)
                    hyperlink.Font = styleFont.Clone();
                else
                    FlattenFont(hyperlink.Values.Font, styleFont);
            }

            var parentFont = GetParentFont(hyperlink);
            if (hyperlink.Values.Font is null && parentFont is not null)
                hyperlink.Font = parentFont.Clone();
            else
                FlattenFont(hyperlink.Values.Font, parentFont);
        }

        /// <summary>
        /// Get the font for the parent of a given object.
        /// </summary>
        /// <param name="obj">The object to start with.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Exception that is thrown of the parent object is neither Paragraph nor Hyperlink or FormattedText.</exception>
        protected Font? GetParentFont(DocumentObject obj)
        {
            DocumentObject parentElements = DocumentRelations.GetParent(obj) ?? NRT.ThrowOnNull<Font, DocumentObject>(); // BUG Throwing on null;
            DocumentObject? parentObject = DocumentRelations.GetParent(parentElements);
            Font? parentFont;
            if (parentObject is Paragraph paragraph)
            {
                var format = paragraph.Format;
                parentFont = format.Values.Font;
            }
            else if (parentObject is Hyperlink hyperlink)
            {
                parentFont = hyperlink.Font;
            }
            else if (parentObject is FormattedText formattedText)
            {
                parentFont = formattedText.Font;
            }
            else
                throw new InvalidOperationException($"Unexpected class {parentObject?.GetType().Name ?? "<null>"} in GetParentFont.");

            return parentFont;
        }
    }
}
