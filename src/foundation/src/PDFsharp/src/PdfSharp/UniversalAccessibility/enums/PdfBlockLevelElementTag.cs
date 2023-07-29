// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.UniversalAccessibility
{
    /// <summary>
    /// PDF Block Level Element tags for Universal Accessibility.
    /// </summary>
    public enum PdfBlockLevelElementTag
    {
        // ReSharper disable InconsistentNaming

        // ----- Paragraph like elements -----

        /// <summary>
        /// (Paragraph) A low-level division of text.
        /// </summary>
        [Obsolete($"Use '{nameof(Paragraph)}'.")]
        P = 0,

        /// <summary>
        /// A low-level division of text.
        /// </summary>
        Paragraph = 0,

        /// <summary>
        /// (Heading) A label for a subdivision of a document’s content. It should be the first child of the division that it heads.
        /// </summary>
        [Obsolete($"Use '{nameof(Heading)}'.")]
        H = 1,

        /// <summary>
        /// A label for a subdivision of a document’s content. It should be the first child of the division that it heads.
        /// </summary>
        Heading = 1,

        /// <summary>
        /// Headings with specific levels, for use in applications that cannot hierarchically nest their sections and thus cannot 
        /// determine the level of a heading from its level of nesting.
        /// Obsolete: Use Heading1 etc. instead.
        /// </summary>
        [Obsolete($"Use '{nameof(Heading1)}'.")]
        H1 = 2,

        /// <summary>
        /// Headings with specific levels, for use in applications that cannot hierarchically nest their sections and thus cannot 
        /// determine the level of a heading from its level of nesting.
        /// Obsolete: Use Heading1 etc. instead.
        /// </summary>
        [Obsolete($"Use '{nameof(Heading2)}'.")]
        H2 = 3,

        /// <summary>
        /// Headings with specific levels, for use in applications that cannot hierarchically nest their sections and thus cannot 
        /// determine the level of a heading from its level of nesting.
        /// Obsolete: Use Heading1 etc. instead.
        /// </summary>
        [Obsolete($"Use '{nameof(Heading3)}'.")]
        H3 = 4,

        /// <summary>
        /// Headings with specific levels, for use in applications that cannot hierarchically nest their sections and thus cannot 
        /// determine the level of a heading from its level of nesting.
        /// Obsolete: Use Heading1 etc. instead.
        /// </summary>
        [Obsolete($"Use '{nameof(Heading4)}'.")]
        H4 = 5,

        /// <summary>
        /// Headings with specific levels, for use in applications that cannot hierarchically nest their sections and thus cannot 
        /// determine the level of a heading from its level of nesting.
        /// Obsolete: Use Heading1 etc. instead.
        /// </summary>
        [Obsolete($"Use '{nameof(Heading5)}'.")]
        H5 = 6,

        /// <summary>
        /// Headings with specific levels, for use in applications that cannot hierarchically nest their sections and thus cannot 
        /// determine the level of a heading from its level of nesting.
        /// Obsolete: Use Heading1 etc. instead.
        /// </summary>
        [Obsolete($"Use '{nameof(Heading6)}'.")]
        H6 = 7, //...

        /// <summary>
        /// Headings with specific level 1, for use in applications that cannot hierarchically nest their sections and thus cannot 
        /// determine the level of a heading from its level of nesting.
        /// </summary>
        Heading1 = 2,

        /// <summary>
        /// Headings with specific level 2, for use in applications that cannot hierarchically nest their sections and thus cannot 
        /// determine the level of a heading from its level of nesting.
        /// </summary>
        Heading2 = 3,

        /// <summary>
        /// Headings with specific level 3, for use in applications that cannot hierarchically nest their sections and thus cannot 
        /// determine the level of a heading from its level of nesting.
        /// </summary>
        Heading3 = 4,

        /// <summary>
        /// Headings with specific level 4, for use in applications that cannot hierarchically nest their sections and thus cannot 
        /// determine the level of a heading from its level of nesting.
        /// </summary>
        Heading4 = 5,

        /// <summary>
        /// Headings with specific level 5, for use in applications that cannot hierarchically nest their sections and thus cannot 
        /// determine the level of a heading from its level of nesting.
        /// </summary>
        Heading5 = 6,

        /// <summary>
        /// Headings with specific level 6, for use in applications that cannot hierarchically nest their sections and thus cannot 
        /// determine the level of a heading from its level of nesting.
        /// </summary>
        Heading6 = 7,

        // ----- List elements -----

        /// <summary>
        /// (List) A sequence of items of like meaning and importance. Its immediate children should be an optional caption (structure type Caption).
        /// </summary>
        [Obsolete($"Use '{nameof(List)}'.")]
        L = 8,

        /// <summary>
        /// A sequence of items of like meaning and importance. Its immediate children should be an optional caption (structure type Caption).
        /// </summary>
        List = 8,

        /// <summary>
        /// (Label) A name or number that distinguishes a given item from others in the same list or other group of like items. In a dictionary list,
        /// for example, it contains the term being defined; in a bulleted or numbered list, it contains the bullet character or the number of the 
        /// list item and associated punctuation.
        /// </summary>
        [Obsolete($"Use '{nameof(Label)}'.")]
        Lbl = 9,

        /// <summary>
        /// A name or number that distinguishes a given item from others in the same list or other group of like items. In a dictionary list,
        /// for example, it contains the term being defined; in a bulleted or numbered list, it contains the bullet character or the number of the 
        /// list item and associated punctuation.
        /// </summary>
        Label = 9,

        /// <summary>
        /// (List item) An individual member of a list. Its children may be one or more labels, list bodies,
        /// or both (structure types Lbl or LBody; see below).
        /// </summary>
        [Obsolete($"Use '{nameof(ListItem)}'.")]
        LI = 10,

        /// <summary>
        /// An individual member of a list. Its children may be one or more labels, list bodies,
        /// or both (structure types Lbl or LBody; see below).
        /// </summary>
        ListItem = 10,

        /// <summary>
        /// (List body) The descriptive content of a list item. In a dictionary list, for example, it contains
        /// the definition of the term. It can either contain the content directly or have other BLSEs, perhaps including nested lists, as children.
        /// </summary>
        [Obsolete($"Use '{nameof(ListBody)}'.")]
        LBody = 11,

        /// <summary>
        /// The descriptive content of a list item. In a dictionary list, for example, it contains
        /// the definition of the term. It can either contain the content directly or have other BLSEs, perhaps including nested lists, as children.
        /// </summary>
        ListBody = 11,

        // ----- Table elements -----

        /// <summary>
        /// A two-dimensional layout of rectangular data cells, possibly having a complex substructure. It contains either one or more
        /// table rows (structure type TR) as children; or an optional table head (structure type THead) followed by one or more table
        /// body elements (structure type TBody) and an optional table footer (structure type TFoot). In addition, a table may have an optional
        /// caption (structure type Caption) as its first or last child.
        /// </summary>
        Table = 12,

        /// <summary>
        /// (Table row) A row of headings or data in a table. It may contain table header cells and table data cells (structure types TH and TD).
        /// </summary>
        [Obsolete($"Use '{nameof (TableRow)}'.")]
        TR = 13,

        /// <summary>
        /// A row of headings or data in a table. It may contain table header cells and table data cells (structure types TH and TD).
        /// </summary>
        TableRow = 13,

        /// <summary>
        /// (Table header cell) A table cell containing header text describing one or more rows or columns of the table.
        /// </summary>
        [Obsolete($"Use '{nameof(TableHeaderCell)}'.")]
        TH = 14,

        /// <summary>
        /// A table cell containing header text describing one or more rows or columns of the table.
        /// </summary>
        TableHeaderCell = 14,

        /// <summary>
        /// (Table data cell) A table cell containing data that is part of the table’s content.
        /// </summary>
        [Obsolete($"Use '{nameof(TableDataCell)}'.")]
        TD = 15,

        /// <summary>
        /// A table cell containing data that is part of the table’s content.
        /// </summary>
        TableDataCell = 15,

        /// <summary>
        /// (Table header row group; PDF 1.5) A group of rows that constitute the header of a table. If the table is split across multiple pages,
        /// these rows may be redrawn at the top of each table fragment (although there is only one THead element).
        /// </summary>
        [Obsolete($"Use '{nameof(TableHeadRowGroup)}'.")]
        THead = 16,

        /// <summary>
        /// (PDF 1.5) A group of rows that constitute the header of a table. If the table is split across multiple pages,
        /// these rows may be redrawn at the top of each table fragment (although there is only one THead element).
        /// </summary>
        TableHeadRowGroup = 16,

        /// <summary>
        /// (Table body row group; PDF 1.5) A group of rows that constitute the main body portion of a table. If the table is split across multiple
        /// pages, the body area may be broken apart on a row boundary. A table may have multiple TBody elements to allow for the drawing of a
        /// border or background for a set of rows.
        /// </summary>
        [Obsolete($"Use '{nameof(TableBodyRowGroup)}'.")]
        TBody = 17,

        /// <summary>
        /// (PDF 1.5) A group of rows that constitute the main body portion of a table. If the table is split across multiple
        /// pages, the body area may be broken apart on a row boundary. A table may have multiple TBody elements to allow for the drawing of a
        /// border or background for a set of rows.
        /// </summary>
        TableBodyRowGroup = 17,

        /// <summary>
        /// (Table footer row group; PDF 1.5) A group of rows that constitute the footer of a table. If the table is split across multiple pages,
        /// these rows may be redrawn at the bottom of each table fragment (although there is only one TFoot element.)
        /// </summary>
        [Obsolete($"Use '{nameof(TableFooterRowGroup)}'.")]
        TFoot = 18,

        /// <summary>
        /// (PDF 1.5) A group of rows that constitute the footer of a table. If the table is split across multiple pages,
        /// these rows may be redrawn at the bottom of each table fragment (although there is only one TFoot element.)
        /// </summary>
        TableFooterRowGroup = 18,

        // ReSharper restore InconsistentNaming
    }
}
