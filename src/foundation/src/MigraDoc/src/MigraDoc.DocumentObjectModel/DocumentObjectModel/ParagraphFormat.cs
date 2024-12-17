// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// A ParagraphFormat represents the formatting of a paragraph.
    /// </summary>
    public class ParagraphFormat : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the ParagraphFormat class that can be used as a template.
        /// </summary>
        public ParagraphFormat()
        {
            BaseValues = new ParagraphFormatValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the ParagraphFormat class with the specified parent.
        /// </summary>
        internal ParagraphFormat(DocumentObject parent) : base(parent)
        {
            BaseValues = new ParagraphFormatValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new ParagraphFormat Clone() 
            => (ParagraphFormat)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            ParagraphFormat format = (ParagraphFormat)base.DeepCopy();
            if (format.Values.Font != null)
            {
                format.Values.Font = format.Values.Font.Clone();
                format.Values.Font.Parent = format;
            }
            if (format.Values.Shading != null)
            {
                format.Values.Shading = format.Values.Shading.Clone();
                format.Values.Shading.Parent = format;
            }
            if (format.Values.Borders != null)
            {
                format.Values.Borders = format.Values.Borders.Clone();
                format.Values.Borders.Parent = format;
            }
            if (format.Values.TabStops != null)
            {
                format.Values.TabStops = format.Values.TabStops.Clone();
                format.Values.TabStops.Parent = format;
            }
            if (format.Values.ListInfo != null)
            {
                format.Values.ListInfo = format.Values.ListInfo.Clone();
                format.Values.ListInfo.Parent = format;
            }
            return format;
        }

        /// <summary>
        /// Adds a TabStop object to the collection.
        /// </summary>
        public TabStop AddTabStop(Unit position) 
            => TabStops.AddTabStop(position);

        /// <summary>
        /// Adds a TabStop object to the collection and sets its alignment and leader.
        /// </summary>
        public TabStop AddTabStop(Unit position, TabAlignment alignment, TabLeader leader) 
            => TabStops.AddTabStop(position, alignment, leader);

        /// <summary>
        /// Adds a TabStop object to the collection and sets its leader.
        /// </summary>
        public TabStop AddTabStop(Unit position, TabLeader leader) 
            => TabStops.AddTabStop(position, leader);

        /// <summary>
        /// Adds a TabStop object to the collection and sets its alignment.
        /// </summary>
        public TabStop AddTabStop(Unit position, TabAlignment alignment) 
            => TabStops.AddTabStop(position, alignment);

        /// <summary>
        /// Adds a TabStop object to the collection marked to remove the tab stop at
        /// the given position.
        /// </summary>
        public void RemoveTabStop(Unit position) 
            => TabStops.RemoveTabStop(position);

        /// <summary>
        /// Adds a TabStop object to the collection.
        /// </summary>
        public void Add(TabStop tabStop) 
            => TabStops.AddTabStop(tabStop);

        /// <summary>
        /// Clears all TapStop objects from the collection. Additionally, 'TabStops = null'
        /// is written to the DDL stream when serialized.
        /// </summary>
        public void ClearAll() 
            => TabStops.ClearAll();

        /// <summary>
        /// Gets or sets the Alignment of the paragraph.
        /// </summary>
        public ParagraphAlignment Alignment
        {
            get => Values.Alignment ?? ParagraphAlignment.Left;
            set => Values.Alignment = value;
        }

        /// <summary>
        /// Gets the Borders object.
        /// </summary>
        public Borders Borders
        {
            get => Values.Borders ??= new Borders(this);
            set
            {
                SetParentOf(value);
                Values.Borders = value;
            }
        }

        /// <summary>
        /// Gets or sets the indent of the first line in the paragraph.
        /// </summary>
        public Unit FirstLineIndent
        {
            get => Values.FirstLineIndent ?? Unit.Empty;
            set => Values.FirstLineIndent = value;
        }

        /// <summary>
        /// Gets or sets the Font object.
        /// </summary>
        public Font Font
        {
            get => Values.Font ??= new Font(this);
            set
            {
                SetParentOf(value);
                Values.Font = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to keep all the paragraph’s lines on the same page.
        /// </summary>
        public bool KeepTogether
        {
            get => Values.KeepTogether ?? false;
            set => Values.KeepTogether = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this and the next paragraph stay on the same page.
        /// </summary>
        public bool KeepWithNext
        {
            get => Values.KeepWithNext ?? false;
            set => Values.KeepWithNext = value;
        }

        /// <summary>
        /// Gets or sets the left indent of the paragraph.
        /// </summary>
        public Unit LeftIndent
        {
            get => Values.LeftIndent ?? Unit.Empty;
            set => Values.LeftIndent = value;
        }

        /// <summary>
        /// Gets or sets the space between lines on the paragraph.
        /// </summary>
        public Unit LineSpacing
        {
            get => Values.LineSpacing ?? Unit.Empty;
            set => Values.LineSpacing = value;
        }

        /// <summary>
        /// Gets or sets the rule which is used to define the line spacing.
        /// </summary>
        public LineSpacingRule LineSpacingRule
        {
            get => Values.LineSpacingRule ?? LineSpacingRule.Single;
            set => Values.LineSpacingRule = value;
        }

        /// <summary>
        /// Gets or sets the ListInfo object of the paragraph.
        /// </summary>
        public ListInfo ListInfo
        {
            get => Values.ListInfo ??= new ListInfo(this);
            set
            {
                SetParentOf(value);
                Values.ListInfo = value;
            }
        }

        /// <summary>
        /// Gets or sets the outline level of the paragraph.
        /// </summary>
        public OutlineLevel OutlineLevel
        {
            get => Values.OutlineLevel ?? OutlineLevel.BodyText;
            set => Values.OutlineLevel = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether a page break is inserted before the paragraph.
        /// </summary>
        public bool PageBreakBefore
        {
            get => Values.PageBreakBefore ?? false;
            set => Values.PageBreakBefore = value;
        }

        /// <summary>
        /// Gets or sets the right indent of the paragraph.
        /// </summary>
        public Unit RightIndent
        {
            get => Values.RightIndent ?? Unit.Empty;
            set => Values.RightIndent = value;
        }

        /// <summary>
        /// Gets the shading object.
        /// </summary>
        public Shading Shading
        {
            get => Values.Shading ??= new Shading(this);
            set
            {
                SetParentOf(value);
                Values.Shading = value;
            }
        }

        /// <summary>
        /// Gets or sets the space that’s inserted after the paragraph.
        /// </summary>
        public Unit SpaceAfter
        {
            get => Values.SpaceAfter ?? Unit.Empty;
            set => Values.SpaceAfter = value;
        }

        /// <summary>
        /// Gets or sets the space that’s inserted before the paragraph.
        /// </summary>
        public Unit SpaceBefore
        {
            get => Values.SpaceBefore ?? Unit.Empty;
            set => Values.SpaceBefore = value;
        }

        /// <summary>
        /// Indicates whether the ParagraphFormat has a TabStops collection.
        /// </summary>
        public bool HasTabStops 
            => Values.TabStops != null;

        /// <summary>
        /// Indicates whether the ParagraphFormat defines TabStops in its TabStops collection.
        /// </summary>
        public bool DefinesTabStops
            => Values.TabStops?.DefinesTabStops ?? false;

        /// <summary>
        /// Get the TabStops collection.
        /// </summary>
        public TabStops TabStops
        {
            get => Values.TabStops ??= new TabStops(this);
            set
            {
                SetParentOf(value);
                Values.TabStops = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a line from the paragraph stays alone in a page.
        /// </summary>
        public bool WidowControl
        {
            get => Values.WidowControl ?? false;
            set => Values.WidowControl = value;
        }

        /// <summary>
        /// Converts ParagraphFormat into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            if (Parent is Style)
                Serialize(serializer, "ParagraphFormat", null);
            else
                Serialize(serializer, "Format", null);
        }

        /// <summary>
        /// Converts ParagraphFormat into DDL.
        /// </summary>
        internal void Serialize(Serializer serializer, string name, ParagraphFormat? refFormat)
        {
            int pos = serializer.BeginContent(name);

            if (!Values.Font.IsValueNullOrEmpty() && Parent!.GetType() != typeof(Style))
                Font.Serialize(serializer);

            // If a refFormat is specified, it is important to compare the fields and not the properties.
            // Only the fields hold the internal information whether a value is NULL. In contrast to the
            // Efw.Application framework the nullable values and all the meta stuff is kept internal to
            // give the user the illusion of simplicity.

            if (Values.Alignment.HasValue && (refFormat == null || Values.Alignment != refFormat.Values.Alignment))
                serializer.WriteSimpleAttribute("Alignment", Alignment);

            if (!Values.LeftIndent.IsValueNullOrEmpty() && (refFormat == null || Values.LeftIndent != refFormat.Values.LeftIndent))
                serializer.WriteSimpleAttribute("LeftIndent", LeftIndent);

            if (!Values.FirstLineIndent.IsValueNullOrEmpty() && (refFormat == null || Values.FirstLineIndent != refFormat.Values.FirstLineIndent))
                serializer.WriteSimpleAttribute("FirstLineIndent", FirstLineIndent);

            if (!Values.RightIndent.IsValueNullOrEmpty() && (refFormat == null || Values.RightIndent != refFormat.Values.RightIndent))
                serializer.WriteSimpleAttribute("RightIndent", RightIndent);

            if (!Values.SpaceBefore.IsValueNullOrEmpty() && (refFormat == null || Values.SpaceBefore != refFormat.Values.SpaceBefore))
                serializer.WriteSimpleAttribute("SpaceBefore", SpaceBefore);

            if (!Values.SpaceAfter.IsValueNullOrEmpty() && (refFormat == null || Values.SpaceAfter != refFormat.Values.SpaceAfter))
                serializer.WriteSimpleAttribute("SpaceAfter", SpaceAfter);

            if (Values.LineSpacingRule.HasValue && (refFormat == null || Values.LineSpacingRule != refFormat.Values.LineSpacingRule))
                serializer.WriteSimpleAttribute("LineSpacingRule", LineSpacingRule);

            if (!Values.LineSpacing.IsValueNullOrEmpty() && (refFormat == null || Values.LineSpacing != refFormat.Values.LineSpacing))
                serializer.WriteSimpleAttribute("LineSpacing", LineSpacing);

            if (Values.KeepTogether.HasValue && (refFormat == null || Values.KeepTogether != refFormat.Values.KeepTogether))
                serializer.WriteSimpleAttribute("KeepTogether", KeepTogether);

            if (Values.KeepWithNext.HasValue && (refFormat == null || Values.KeepWithNext != refFormat.Values.KeepWithNext))
                serializer.WriteSimpleAttribute("KeepWithNext", KeepWithNext);

            if (Values.WidowControl.HasValue && (refFormat == null || Values.WidowControl != refFormat.Values.WidowControl))
                serializer.WriteSimpleAttribute("WidowControl", WidowControl);

            if (Values.PageBreakBefore.HasValue && (refFormat == null || Values.PageBreakBefore != refFormat.Values.PageBreakBefore))
                serializer.WriteSimpleAttribute("PageBreakBefore", PageBreakBefore);

            if (Values.OutlineLevel.HasValue && (refFormat == null || Values.OutlineLevel != refFormat.Values.OutlineLevel))
                serializer.WriteSimpleAttribute("OutlineLevel", OutlineLevel);

            if (Values.ListInfo is not null)
                ListInfo.Serialize(serializer);

            if (Values.TabStops is not null)
                Values.TabStops.Serialize(serializer);

            if (Values.Borders is not null)
            {
                if (refFormat != null)
                    Values.Borders.Serialize(serializer, refFormat.Borders);
                else
                    Values.Borders.Serialize(serializer, null);
            }

            if (Values.Shading is not null)
                Values.Shading.Serialize(serializer);
            serializer.EndContent(pos);
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(ParagraphFormat));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public ParagraphFormatValues Values => (ParagraphFormatValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class ParagraphFormatValues : Values
        {
            internal ParagraphFormatValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public ParagraphAlignment? Alignment { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Borders? Borders { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? FirstLineIndent { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Font? Font { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? KeepTogether { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? KeepWithNext { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? LeftIndent { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? LineSpacing { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public LineSpacingRule? LineSpacingRule { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public ListInfo? ListInfo { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public OutlineLevel? OutlineLevel { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? PageBreakBefore { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? RightIndent { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Shading? Shading { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? SpaceAfter { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? SpaceBefore { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public TabStops? TabStops { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? WidowControl { get; set; }
        }
    }
}
