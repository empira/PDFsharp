// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using MigraDoc.DocumentObjectModel.Tables;

namespace MigraDoc.DocumentObjectModel.Visitors
{
    /// <summary>
    /// Represents the base visitor for the DocumentObject.
    /// </summary>
    public abstract class DocumentObjectVisitor
    {
        /// <summary>
        /// Visits the specified document object.
        /// </summary>
        public abstract void Visit(DocumentObject documentObject);

        // Chart
        internal virtual void VisitChart(Chart chart)
        { }

        internal virtual void VisitTextArea(TextArea textArea)
        { }

        internal virtual void VisitLegend(Legend legend)
        { }

        // Document
        internal virtual void VisitDocument(Document document) 
        { }

        internal virtual void VisitDocumentElements(DocumentElements elements)
        { }

        internal virtual void VisitDocumentObjectCollection(DocumentObjectCollection elements)
        { }

        // Fields

        // Format
        internal virtual void VisitFont(Font font) { }

        internal virtual void VisitParagraphFormat(ParagraphFormat paragraphFormat)
        { }

        internal virtual void VisitShading(Shading shading) 
        { }

        internal virtual void VisitStyle(Style style) 
        { }

        internal virtual void VisitStyles(Styles styles)
        { }

        // Paragraph
        internal virtual void VisitFootnote(Footnote footnote) 
        { }

        internal virtual void VisitHyperlink(Hyperlink hyperlink)
        { }

        internal virtual void VisitFormattedText(FormattedText formattedText)
        { }

        internal virtual void VisitParagraph(Paragraph paragraph) 
        { }

        // Section
        internal virtual void VisitHeaderFooter(HeaderFooter headerFooter)
        { }

        internal virtual void VisitHeadersFooters(HeadersFooters headersFooters)
        { }

        internal virtual void VisitSection(Section section) 
        { }

        internal virtual void VisitSections(Sections sections)
        { }

        // Shape
        internal virtual void VisitImage(Image image) 
        { }

        internal virtual void VisitTextFrame(TextFrame textFrame)
        { }

        // Table
        internal virtual void VisitCell(Cell cell)
        { }

        internal virtual void VisitColumns(Columns columns)
        { }

        internal virtual void VisitRow(Row row)
        { }

        internal virtual void VisitRows(Rows rows)
        { }

        internal virtual void VisitTable(Table table)
        { }
    }
}
