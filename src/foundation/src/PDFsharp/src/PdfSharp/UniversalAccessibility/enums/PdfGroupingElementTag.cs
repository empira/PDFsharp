// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.UniversalAccessibility
{
    /// <summary>
    /// PDF Grouping Element tags for Universal Accessibility.
    /// </summary>
    public enum PdfGroupingElementTag
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// (Document) A complete document. This is the root element of any structure tree containing multiple parts or multiple articles.
        /// </summary>
        Document = 0,

        /// <summary>
        /// (Part) A large-scale division of a document. This type of element is appropriate for grouping articles or sections.
        /// </summary>
        Part = 1,

        /// <summary>
        /// (Article) A relatively self-contained body of text constituting a single narrative or exposition. Articles should be disjoint;
        /// that is, they should not contain other articles as constituent elements.
        /// </summary>
        Article = 2,

        /// <summary>
        /// (Article) A relatively self-contained body of text constituting a single narrative or exposition. Articles should be disjoint;
        /// that is, they should not contain other articles as constituent elements.
        /// </summary>
        [Obsolete($"Use '{nameof(Article)}'.")]
        Art = 2,

        /// <summary>
        /// (Section) A container for grouping related content elements.
        /// For example, a section might contain a heading, several introductory paragraphs,
        /// and two or more other sections nested within it as subsections.
        /// </summary>
        Section = 3,

        /// <summary>
        /// (Section) A container for grouping related content elements.
        /// For example, a section might contain a heading, several introductory paragraphs,
        /// and two or more other sections nested within it as subsections.
        /// </summary>
        [Obsolete($"Use '{nameof(Section)}'.")]
        Sect = 3,

        /// <summary>
        /// (Division) A generic block-level element or group of elements.
        /// </summary>
        Division = 4,

        /// <summary>
        /// (Division) A generic block-level element or group of elements.
        /// </summary>
        [Obsolete($"Use '{nameof(Division)}'.")]
        Div = 4,

        /// <summary>
        /// (Block quotation) A portion of text consisting of one or more paragraphs attributed to someone other than the author of the
        /// surrounding text.
        /// </summary>
        BlockQuotation = 5,

        /// <summary>
        /// (Block quotation) A portion of text consisting of one or more paragraphs attributed to someone other than the author of the
        /// surrounding text.
        /// </summary>
        [Obsolete($"Use '{nameof(BlockQuotation)}'.")]
        BlockQuote = 5,

        /// <summary>
        /// (Caption) A brief portion of text describing a table or figure.
        /// </summary>
        Caption = 6,

        /// <summary>
        /// (Table of contents) A list made up of table of contents item entries (structure type TableOfContentsItem; see below)
        /// and/or other nested table of contents entries (TableOfContents).
        /// A TableOfContents entry that includes only TableOfContentsItem entries represents a flat hierarchy.
        /// A TableOfContents entry that includes other nested TableOfContents entries (and possibly TableOfContentsItem entries)
        /// represents a more complex hierarchy. Ideally, the hierarchy of a top level TableOfContents entry reflects the
        /// structure of the main body of the document.
        /// Note: Lists of figures and tables, as well as bibliographies, can be treated as tables of contents for purposes of the 
        /// standard structure types.
        /// </summary>
        TableOfContents = 7,

        /// <summary>
        /// (Table of contents) A list made up of table of contents item entries (structure type TableOfContentsItem; see below)
        /// and/or other nested table of contents entries (TableOfContents).
        /// A TableOfContents entry that includes only TableOfContentsItem entries represents a flat hierarchy.
        /// A TableOfContents entry that includes other nested TableOfContents entries (and possibly TableOfContentsItem entries)
        /// represents a more complex hierarchy. Ideally, the hierarchy of a top level TableOfContents entry reflects the
        /// structure of the main body of the document.
        /// Note: Lists of figures and tables, as well as bibliographies, can be treated as tables of contents for purposes of the 
        /// standard structure types.
        /// </summary>
        [Obsolete($"Use '{nameof(TableOfContents)}'.")]
        TOC = 7,

        /// <summary>
        /// (Table of contents item) An individual member of a table of contents. This entry’s children can be any of the following structure types:
        /// Label                 A label.
        /// Reference             A reference to the title and the page number.
        /// NonstructuralElement  Non-structure elements for wrapping a leader artifact.
        /// Paragraph             Descriptive text.
        /// TableOfContents       Table of content elements for hierarchical tables of content, as described for the TableOfContents entry.
        /// </summary>
        TableOfContentsItem = 8,

        /// <summary>
        /// (Table of contents item) An individual member of a table of contents. This entry’s children can be any of the following structure types:
        /// Label                 A label.
        /// Reference             A reference to the title and the page number.
        /// NonstructuralElement  Non-structure elements for wrapping a leader artifact.
        /// Paragraph             Descriptive text.
        /// TableOfContents       Table of content elements for hierarchical tables of content, as described for the TableOfContents entry.
        /// </summary>
        [Obsolete($"Use '{nameof(TableOfContentsItem)}'.")]
        TOCI = 8,

        /// <summary>
        /// (Index) A sequence of entries containing identifying text accompanied by reference elements (structure type Reference) that point out
        /// occurrences of the specified text in the main body of a document.
        /// </summary>
        Index = 9,

        /// <summary>
        /// (Nonstructural element) A grouping element having no inherent structural significance; it serves solely for grouping purposes.
        /// This type of element differs from a division (structure type Division; see above) in that it is not interpreted or exported to other
        /// document formats; however, its descendants are to be processed normally.
        /// </summary>
        NonstructuralElement = 10,

        /// <summary>
        /// (Nonstructural element) A grouping element having no inherent structural significance; it serves solely for grouping purposes.
        /// This type of element differs from a division (structure type Division; see above) in that it is not interpreted or exported to other
        /// document formats; however, its descendants are to be processed normally.
        /// </summary>
        [Obsolete($"Use '{nameof(NonstructuralElement)}'.")]
        NonStruct = 10,

        /// <summary>
        /// (Private element) A grouping element containing private content belonging to the application producing it. The structural significance
        /// of this type of element is unspecified and is determined entirely by the producer application. Neither the Private element nor any of
        /// its descendants are to be interpreted or exported to other document formats.
        /// </summary>
        PrivateElement = 11,

        /// <summary>
        /// (Private element) A grouping element containing private content belonging to the application producing it. The structural significance
        /// of this type of element is unspecified and is determined entirely by the producer application. Neither the Private element nor any of
        /// its descendants are to be interpreted or exported to other document formats.
        /// </summary>
        [Obsolete($"Use '{nameof(PrivateElement)}'.")]
        Private = 11

        // ReSharper restore InconsistentNaming
    }
}
