// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics.CodeAnalysis;
using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using static MigraDoc.DocumentObjectModel.Tables.Row;
using System.Diagnostics;

namespace MigraDoc.DocumentObjectModel.Tables
{
    /// <summary>
    /// Represents a cell of a table.
    /// </summary>
    public class Cell : DocumentObject, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the Cell class.
        /// </summary>
        public Cell()
        {
            Debug.Assert(BaseValues == null!);
            BaseValues = new CellValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Cell class with the specified parent.
        /// </summary>
        internal Cell(DocumentObject parent) : base(parent)
        {
            Debug.Assert(BaseValues == null!);
            BaseValues = new CellValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Cell Clone()
            => (Cell)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var cell = (Cell)base.DeepCopy();
            cell.ResetCachedValues();

            // Remove all references to the original object hierarchy.
            if (cell.Values.Format != null)
            {
                cell.Values.Format = cell.Values.Format.Clone();
                cell.Values.Format.Parent = cell;
            }
            if (cell.Values.Borders != null)
            {
                cell.Values.Borders = cell.Values.Borders.Clone();
                cell.Values.Borders.Parent = cell;
            }
            if (cell.Values.Shading != null)
            {
                cell.Values.Shading = cell.Values.Shading.Clone();
                cell.Values.Shading.Parent = cell;
            }
            if (cell.Values.Elements != null)
            {
                cell.Values.Elements = cell.Values.Elements.Clone();
                cell.Values.Elements.Parent = cell;
            }
            return cell;
        }

        /// <summary>
        /// Resets the cached values.
        /// </summary>
        internal override void ResetCachedValues()
        {
            base.ResetCachedValues();
            _row = null;
            _clm = null;
            _table = null;

            // Lazy execution makes properties slow. Calculate frequently required property values in advance.
            if (Parent is Cells cells)
            {
                _row = cells.Row;
                _table = cells.Table;
            }
        }

        /// <summary>
        /// Adds a new paragraph to the cell.
        /// </summary>
        public Paragraph AddParagraph()
            => Elements.AddParagraph();

        /// <summary>
        /// Adds a new paragraph with the specified text to the cell.
        /// </summary>
        public Paragraph AddParagraph(string paragraphText, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddParagraph(paragraphText, textRenderOption);

        /// <summary>
        /// Adds a new chart with the specified type to the cell.
        /// </summary>
        public Chart AddChart(ChartType type)
            => Elements.AddChart(type);

        /// <summary>
        /// Adds a new chart to the cell.
        /// </summary>
        public Chart AddChart()
            => Elements.AddChart();

        /// <summary>
        /// Adds a new Image to the cell.
        /// </summary>
        public Image AddImage(string fileName)
            => Elements.AddImage(fileName);

        /// <summary>
        /// Adds a new text frame to the cell.
        /// </summary>
        public TextFrame AddTextFrame()
            => Elements.AddTextFrame();

        /// <summary>
        /// Adds a new paragraph to the cell.
        /// </summary>
        public void Add(Paragraph paragraph)
            => Elements.Add(paragraph);

        /// <summary>
        /// Adds a new chart to the cell.
        /// </summary>
        public void Add(Chart chart)
            => Elements.Add(chart);

        /// <summary>
        /// Adds a new image to the cell.
        /// </summary>
        public void Add(Image image)
            => Elements.Add(image);

        /// <summary>
        /// Adds a new text frame to the cell.
        /// </summary>
        public void Add(TextFrame textFrame)
            => Elements.Add(textFrame);

        /// <summary>
        /// Gets the table the cell belongs to. Can be null if Cell was not added to a table.
        /// </summary>
        public Table Table
        {
            [return: MaybeNull]
            get
            {
                // Set in ResetCachedValues.
                return _table!;
            }
        }
        Table? _table;

        /// <summary>
        /// Gets the column the cell belongs to. Can be null if Cell was not added to a column.
        /// </summary>
        public Column Column
        {
            [return: MaybeNull]
            get
            {
                if (_clm is not null)
                    return _clm;
                if (/*_clm == null &&*/ Parent is Cells cells)
                {
                    for (int index = 0; index < cells.Count; index++)
                    {
                        //if (cells[index] == this)
                        //    _clm = Table.Columns[index];
                        cells[index]._clm = Table.Columns[index];
                    }
                }
                return _clm!;
            }
        }
        internal Column? _clm;

        /// <summary>
        /// Gets the row the cell belongs to. Can be null if Cell was not added to a row.
        /// </summary>
        public Row Row
        {
            [return: MaybeNull]
            get
            {
                // Set in ResetCachedValues.
                return _row!;
            }
        }
        Row? _row;

        /// <summary>
        /// Sets or gets the style name.
        /// </summary>
        public string Style
        {
            get => Values.Style ?? "";
            set => Values.Style = value;
        }

        /// <summary>
        /// Gets the ParagraphFormat object of the paragraph.
        /// </summary>
        public ParagraphFormat Format
        {
            get => Values.Format ??= new(this);
            set
            {
                SetParent(value);
                Values.Format = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertical alignment of the cell.
        /// </summary>
        public VerticalAlignment VerticalAlignment
        {
            get => Values.VerticalAlignment ?? VerticalAlignment.Top;
            set => Values.VerticalAlignment = value;
        }

        /// <summary>
        /// Gets the Borders object.
        /// </summary>
        public Borders Borders
        {
            get => Values.Borders ??= new(this);
            set
            {
                SetParent(value);
                Values.Borders = value;
            }
        }

        /// <summary>
        /// Gets the shading object.
        /// </summary>
        public Shading Shading
        {
            get => Values.Shading ??= new(this);
            set
            {
                SetParent(value);
                Values.Shading = value;
            }
        }

        /// <summary>
        /// Specifies if the Cell should be rendered as a rounded corner.
        /// </summary>
        public RoundedCorner RoundedCorner
        {
            get => Values.RoundedCorner ?? RoundedCorner.None;
            set => Values.RoundedCorner = value;
        }

        /// <summary>
        /// Gets or sets the number of cells to be merged right.
        /// </summary>
        public int MergeRight
        {
            get => Values.MergeRight ?? 0;
            set => Values.MergeRight = value;
        }

        /// <summary>
        /// Gets or sets the number of cells to be merged down.
        /// </summary>
        public int MergeDown
        {
            get => Values.MergeDown ?? 0;
            set => Values.MergeDown = value;
        }

        /// <summary>
        /// Gets the collection of document objects that defines the cell.
        /// </summary>
        public DocumentElements Elements
        {
            get => Values.Elements ??= new(this);
            set
            {
                SetParent(value);
                Values.Elements = value;
            }
        }

        /// <summary>
        /// Gets or sets a comment associated with this object.
        /// </summary>
        public string Comment
        {
            get => Values.Comment ?? "";
            set => Values.Comment = value;
        }

        /// <summary>
        /// Converts Cell into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteComment(Values.Comment);
            serializer.WriteLine("\\cell");

            int pos = serializer.BeginAttributes();

            if (!String.IsNullOrEmpty(Values.Style))
                serializer.WriteSimpleAttribute("Style", Style);

            if (Values.Format is not null)
                Values.Format.Serialize(serializer, "Format", null);

            if (Values.MergeDown is not null)
                serializer.WriteSimpleAttribute("MergeDown", MergeDown);

            if (Values.MergeRight is not null)
                serializer.WriteSimpleAttribute("MergeRight", MergeRight);

            if (Values.VerticalAlignment is not null)
                serializer.WriteSimpleAttribute("VerticalAlignment", VerticalAlignment);

            Values.Borders?.Serialize(serializer, null);

            Values.Shading?.Serialize(serializer);

            if (Values.RoundedCorner is not null and not RoundedCorner.None)
                serializer.WriteSimpleAttribute("RoundedCorner", RoundedCorner);

            serializer.EndAttributes(pos);

            pos = serializer.BeginContent();
            Values.Elements?.Serialize(serializer);
            serializer.EndContent(pos);
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitCell(this);

            if (visitChildren && Values.Elements != null)
                ((IVisitable)Values.Elements).AcceptVisitor(visitor, visitChildren);
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Cell));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public CellValues Values => (CellValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class CellValues : Values
        {
            internal CellValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Style { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public ParagraphFormat? Format { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public VerticalAlignment? VerticalAlignment { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Borders? Borders { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Shading? Shading { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public RoundedCorner? RoundedCorner { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public int? MergeRight { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public int? MergeDown { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public DocumentElements? Elements { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Comment { get; set; }
        }
    }
}
