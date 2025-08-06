// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using MigraDoc.DocumentObjectModel.Shapes;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents a collection of document elements.
    /// </summary>
    public class DocumentElements : DocumentObjectCollection, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the DocumentElements class.
        /// </summary>
        public DocumentElements()
        {
            BaseValues = new DocumentElementsValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the DocumentElements class with the specified parent.
        /// </summary>
        internal DocumentElements(DocumentObject parent) : base(parent)
        {
            BaseValues = new DocumentElementsValues(this);
        }

        /// <summary>
        /// Gets a document object by its index.
        /// </summary>
        public new DocumentObject this[int index] => base[index]!; // May return null in derived classes.

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new DocumentElements Clone() 
            => (DocumentElements)DeepCopy();

        /// <summary>
        /// Adds a new paragraph to the collection.
        /// </summary>
        public Paragraph AddParagraph()
        {
            var paragraph = new Paragraph();
            Add(paragraph);
            return paragraph;
        }

        /// <summary>
        /// Adds a new paragraph with the specified text to the collection.
        /// </summary>
        public Paragraph AddParagraph(string text)
        {
            var paragraph = new Paragraph();
            paragraph.AddText(text);
            Add(paragraph);
            return paragraph;
        }

        /// <summary>
        /// Adds a new paragraph with the specified text and style to the collection.
        /// </summary>
        public Paragraph AddParagraph(string text, string style)
        {
            var paragraph = new Paragraph();
            paragraph.AddText(text);
            paragraph.Style = style;
            Add(paragraph);
            return paragraph;
        }

        /// <summary>
        /// Adds a new table to the collection.
        /// </summary>
        public Table AddTable()
        {
            var tbl = new Table();
            Add(tbl);
            return tbl;
        }

        /// <summary>
        /// Adds a new legend to the collection.
        /// </summary>
        public Legend AddLegend()
        {
            var legend = new Legend();
            Add(legend);
            return legend;
        }

        /// <summary>
        /// Add a manual page break.
        /// </summary>
        public void AddPageBreak()
        {
            var pageBreak = new PageBreak();
            Add(pageBreak);
        }

        /// <summary>
        /// Adds a new barcode to the collection.
        /// </summary>
        public Barcode AddBarcode()
        {
            var barcode = new Barcode();
            Add(barcode);
            return barcode;
        }

        /// <summary>
        /// Adds a new chart with the specified type to the collection.
        /// </summary>
        public Chart AddChart(ChartType type)
        {
            var chart = AddChart();
            chart.Type = type;
            return chart;
        }

        /// <summary>
        /// Adds a new chart with the specified type to the collection.
        /// </summary>
        public Chart AddChart()
        {
            var chart = new Chart
            {
                Type = ChartType.Line
            };
            Add(chart);
            return chart;
        }

        /// <summary>
        /// Adds a new image to the collection.
        /// </summary>
        public Image AddImage(string name)
        {
            var image = new Image
            {
                Name = name
            };
            Add(image);
            return image;
        }

        /// <summary>
        /// Adds a new text frame to the collection.
        /// </summary>
        public TextFrame AddTextFrame()
        {
            var textFrame = new TextFrame();
            Add(textFrame);
            return textFrame;
        }

        /// <summary>
        /// Converts DocumentElements into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            int count = Count;
            if (count == 1 && this[0] is Paragraph)
            {
                // Omit keyword if paragraph has no attributes set.
                var paragraph = (Paragraph)this[0];
                if (paragraph.Style == "" && paragraph.Values.Format.IsValueNullOrEmpty())
                {
                    paragraph.SerializeContentOnly = true;
                    paragraph.Serialize(serializer);
                    paragraph.SerializeContentOnly = false;
                    return;
                }
            }
            for (int index = 0; index < count; index++)
            {
                var documentElement = this[index];
                documentElement.Serialize(serializer);
            }
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitDocumentElements(this);

            foreach (var docObject in this)
            {
                if (docObject is IVisitable visitable)
                    visitable.AcceptVisitor(visitor, visitChildren);
            }
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(DocumentElements));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public DocumentElementsValues Values => (DocumentElementsValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class DocumentElementsValues : Values
        {
            internal DocumentElementsValues(DocumentObject owner) : base(owner)
            { }
        }
    }
}
