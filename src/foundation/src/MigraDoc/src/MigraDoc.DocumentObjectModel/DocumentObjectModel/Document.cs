// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics.CodeAnalysis;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Visitors;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents a MigraDoc document.
    /// </summary>
    public sealed class Document : DocumentObject, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the Document class.
        /// </summary>
        public Document()
        {
            BaseValues = new DocumentValues(this);
        }

        /// <summary>
        /// Gets or sets the user-defined culture used for date formatting and decimal tabstop alignment.
        /// Note for RTF rendering: Decimal tabstop alignment is done in the viewing application and always depends on the system regional settings,
        /// as culture or separators cannot be saved in rtf.
        /// </summary>
        public CultureInfo? Culture { get; set; }

        /// <summary>
        /// Gets the actual culture used for date formatting and decimal tabstop alignment.
        /// Note for RTF rendering: Decimal tabstop alignment is done in the viewing application and always depends on the system regional settings,
        /// as culture or separators cannot be saved in rtf.
        /// </summary>
        public CultureInfo EffectiveCulture => Culture ?? CultureInfo.CurrentCulture;

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Document Clone()
                => (Document)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var document = (Document)base.DeepCopy();
            if (document.Values.Info != null)
            {
                document.Values.Info = document.Values.Info.Clone();
                document.Values.Info.Parent = document;
            }

            if (document.Values.Styles != null)
            {
                document.Values.Styles = document.Values.Styles.Clone();
                document.Values.Styles.Parent = document;
            }

            if (document.Values.Sections != null)
            {
                document.Values.Sections = document.Values.Sections.Clone();
                document.Values.Sections.Parent = document;
            }

            return document;
        }

        /// <summary>
        /// Internal function used by a renderer to bind this instance to it.
        /// </summary>
        public void BindToRenderer(object renderer)
        {
            if (_renderer != null && renderer != null && !ReferenceEquals(_renderer, renderer))
            {
                throw new InvalidOperationException("The document is already bound to another renderer. " +
                                                    "A MigraDoc document can be rendered by only one renderer, because the rendering process " +
                                                    "modifies its internal structure. If you want to render a MigraDoc document on different renderers, " +
                                                    "you must create a copy of it using the Clone function.");
            }

            _renderer = renderer;
        }

        object? _renderer;

        /// <summary>
        /// Indicates whether the document is bound to a renderer. A bound document must not be modified anymore.
        /// Modifying it leads to undefined results of the rendering process.
        /// </summary>
        public bool IsBoundToRenderer => _renderer != null;

        /// <summary>
        /// Adds a new section to the document.
        /// </summary>
        public Section AddSection() => Sections.AddSection();

        /// <summary>
        /// Adds a new style to the document styles.
        /// </summary>
        /// <param name="name">Name of the style.</param>
        /// <param name="baseStyle">Name of the base style.</param>
        public Style AddStyle(string name, string baseStyle)
        {
            if (name == null || baseStyle == null)
                throw new ArgumentNullException(name == null ? nameof(name) : nameof(baseStyle));
            if (name.Length == 0 || baseStyle.Length == 0)
                throw new ArgumentException(name == "" ? nameof(name) : nameof(baseStyle));

            return Styles.AddStyle(name, baseStyle);
        }

        /// <summary>
        /// Adds a new section to the document.
        /// </summary>
        public void Add(Section section)
            => Sections.Add(section);

        /// <summary>
        /// Adds a new style to the document styles.
        /// </summary>
        public void Add(Style style)
            => Styles.Add(style);

        /// <summary>
        /// Adds an embedded file to the document.
        /// </summary>
        /// <param name="name">The name used to refer and to entitle the embedded file.</param>
        /// <param name="path">The path of the file to embed.</param>
        public void AddEmbeddedFile(string name, string path)
            => EmbeddedFiles.Add(name, path);

        /// <summary>
        /// Gets the last section of the document. If the document has no sections
        /// a new section is added.
        /// </summary>
        public Section LastSection
        {
            get
            {
                // TODO: LastTable, etc., docu
                var sections = Values.Sections;
                if (sections is { Count: > 0 })
#if NET6_0_OR_GREATER || USE_INDEX_AND_RANGE
                    return sections[^1];
#else
                    return sections[sections.Count - 1];
#endif

                return Capabilities.BackwardCompatibility.DoNotCreateLastSection ? null! : AddSection();
            }
        }

        /// <summary>
        /// Returns the last table in the document, or null if no table exists.
        /// </summary>
        public Table LastTable
        {
            get
            {
                var sections = Values.Sections;
                if (sections is null)
                    return null!;
                for (int idx = sections.Count - 1; idx >= 0; --idx)
                {
                    var section = sections[idx];
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (section.LastTable is not null)
                        return section.LastTable;
                }
                return null!;
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
        /// Gets the document info.
        /// </summary>
        public DocumentInfo Info
        {
            get => Values.Info ??= new DocumentInfo(this);
            set
            {
                SetParent(value);
                Values.Info = value;
            }
        }

        /// <summary>
        /// Gets or sets the styles of the document.
        /// </summary>
        public Styles Styles
        {
            get => Values.Styles ??= new(this);
            set
            {
                SetParent(value);
                Values.Styles = value;
            }
        }

        /// <summary>
        /// Gets or sets the default tab stop position.
        /// </summary>
        public Unit DefaultTabStop
        {
            get => Values.DefaultTabStop ?? Unit.Empty;
            set => Values.DefaultTabStop = value;
        }

        /// <summary>
        /// Gets the default page setup.
        /// </summary>
        public PageSetup DefaultPageSetup => PageSetup.DefaultPageSetup;

        /// <summary>
        /// Gets or sets the location of the Footnote.
        /// </summary>
        public FootnoteLocation FootnoteLocation
        {
            get => Values.FootnoteLocation ?? FootnoteLocation.BottomOfPage;
            set => Values.FootnoteLocation = value;
        }

        /// <summary>
        /// Gets or sets the rule which is used to determine the footnote number on a new page.
        /// </summary>
        public FootnoteNumberingRule FootnoteNumberingRule
        {
            get => Values.FootnoteNumberingRule ?? FootnoteNumberingRule.RestartPage;
            set => Values.FootnoteNumberingRule = value;
        }

        /// <summary>
        /// Gets or sets the type of number which is used for the footnote.
        /// </summary>
        public FootnoteNumberStyle FootnoteNumberStyle
        {
            get => Values.FootnoteNumberStyle ?? FootnoteNumberStyle.Arabic;
            set => Values.FootnoteNumberStyle = value;
        }

        /// <summary>
        /// Gets or sets the starting number of the footnote.
        /// </summary>
        public int FootnoteStartingNumber
        {
            get => Values.FootnoteStartingNumber ?? 0;
            set => Values.FootnoteStartingNumber = value;
        }

        /// <summary>
        /// Gets or sets the path for images used by the document.
        /// </summary>
        public string ImagePath
        {
            get => Values.ImagePath ?? "";
            set => Values.ImagePath = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use the CMYK color model when rendered as PDF.
        /// </summary>
        public bool UseCmykColor
        {
            get => Values.UseCmykColor ?? false;
            set => Values.UseCmykColor = value;
        }

        /// <summary>
        /// Gets the sections of the document.
        /// </summary>
        public Sections Sections
        {
            get => Values.Sections ??= new Sections(this);
            //set
            //{
            //    SetParent(value);
            //    Values.Sections = value;
            //}
        }

        /// <summary>
        /// Gets the embedded documents of the document.
        /// </summary>
        public EmbeddedFiles EmbeddedFiles
        {
            get => Values.EmbeddedFiles ??= new EmbeddedFiles();
            set
            {
                SetParent(value);
                Values.EmbeddedFiles = value;
            }
        }

        /// <summary>
        /// Gets the DDL file name.
        /// </summary>
        public string DdlFile => Values.DdlFile ?? "";

        /// <summary>
        /// Converts Document into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteComment(Values.Comment);
            serializer.WriteLine("\\document");

            int pos = serializer.BeginAttributes();
            //Debug.Assert(IsNull("Info") == Info.IsNull()); // BU/G: Creates Info object.
            Debug.Assert(IsNull("Info") ? Values.Info is null || Info.IsNull() : Values.Info is not null && !Info.IsNull());
            Debug.Assert(IsNull("Info") == Values.Info.IsValueNullOrEmpty());
            if (!Values.Info.IsValueNullOrEmpty())
                Info.Serialize(serializer);
            //if (Values.DefaultTabStop is not null)
            if (!Values.DefaultTabStop.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("DefaultTabStop", DefaultTabStop);
            if (Values.FootnoteLocation is not null)
                serializer.WriteSimpleAttribute("FootnoteLocation", FootnoteLocation);
            if (Values.FootnoteNumberingRule is not null)
                serializer.WriteSimpleAttribute("FootnoteNumberingRule", FootnoteNumberingRule);
            if (Values.FootnoteNumberStyle is not null)
                serializer.WriteSimpleAttribute("FootnoteNumberStyle", FootnoteNumberStyle);
            if (Values.FootnoteStartingNumber is not null)
                serializer.WriteSimpleAttribute("FootnoteStartingNumber", FootnoteStartingNumber);
            if (Values.ImagePath is not null)
                serializer.WriteSimpleAttribute("ImagePath", ImagePath);
            if (Values.UseCmykColor is not null)
                serializer.WriteSimpleAttribute("UseCmykColor", UseCmykColor);
            serializer.EndAttributes(pos);

            serializer.BeginContent();
            if (!Values.EmbeddedFiles.IsValueNullOrEmpty())
                EmbeddedFiles.Serialize(serializer);

            Styles.Serialize(serializer);

            if (!Values.Sections.IsValueNullOrEmpty())
                Sections.Serialize(serializer);
            serializer.EndContent();
            serializer.Flush();
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and all its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitDocument(this);
            if (visitChildren)
            {
                ((IVisitable)Styles).AcceptVisitor(visitor, true);
                ((IVisitable)Sections).AcceptVisitor(visitor, true);
            }
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Document));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public DocumentValues Values => (DocumentValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class DocumentValues : Values
        {
            internal DocumentValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Comment { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public DocumentInfo? Info { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Styles? Styles { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? DefaultTabStop { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public FootnoteLocation? FootnoteLocation { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public FootnoteNumberingRule? FootnoteNumberingRule { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public FootnoteNumberStyle? FootnoteNumberStyle { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public int? FootnoteStartingNumber { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? ImagePath { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? UseCmykColor { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Sections? Sections { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public EmbeddedFiles? EmbeddedFiles { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? DdlFile { get; set; }
        }
    }
}
