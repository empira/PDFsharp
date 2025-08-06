// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents the page setup of a section.
    /// </summary>
    public class PageSetup : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the PageSetup class.
        /// </summary>
        public PageSetup()
        {
            BaseValues = new PageSetupValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the PageSetup class with the specified parent.
        /// </summary>
        internal PageSetup(DocumentObject parent) : base(parent)
        {
            BaseValues = new PageSetupValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new PageSetup Clone()
        {
            var clone = (PageSetup)DeepCopy();
            clone._frozen = false;
            return clone;
        }

        /// <summary>
        /// Freezes this instance.
        /// </summary>
        internal void Freeze() => _frozen = true;

        /// <summary>
        /// Gets the page’s size and height for the given PageFormat.
        /// </summary>
        public static void GetPageSize(PageFormat pageFormat, out Unit pageWidth, out Unit pageHeight)
        {
            // All sizes in mm.

            // ReSharper disable InconsistentNaming
            // Values see here: https://en.wikipedia.org/wiki/Paper_size
            const int A0Height = 1189;
            const int A0Width = 841;
            // ReSharper restore InconsistentNaming

            pageWidth = 0;
            pageHeight = 0;
            int height = 0;
            int width = 0;
            switch (pageFormat)
            {
                case PageFormat.A0:
                    height = A0Height;
                    width = A0Width;
                    break;

                case PageFormat.A1:
                    height = A0Width;
                    width = A0Height / 2;
                    break;

                case PageFormat.A2:
                    height = A0Height / 2;
                    width = A0Width / 2;
                    break;

                case PageFormat.A3:
                    height = A0Width / 2;
                    width = A0Height / 4;
                    break;

                case PageFormat.A4:
                    height = A0Height / 4;
                    width = A0Width / 4;
                    break;

                case PageFormat.A5:
                    height = A0Width / 4;
                    width = A0Height / 8;
                    break;

                case PageFormat.A6:
                    height = A0Height / 8;
                    width = A0Width / 8;
                    break;

                case PageFormat.B5:
                    height = 257;
                    width = 182;
                    break;

                case PageFormat.Letter:
                    pageWidth = Unit.FromPoint(612);
                    pageHeight = Unit.FromPoint(792);
                    break;

                case PageFormat.Legal:
                    pageWidth = Unit.FromPoint(612);
                    pageHeight = Unit.FromPoint(1008);
                    break;

                case PageFormat.Ledger:
                    pageWidth = Unit.FromPoint(1224);
                    pageHeight = Unit.FromPoint(792);
                    break;

                case PageFormat.P11x17:
                    pageWidth = Unit.FromPoint(792);
                    pageHeight = Unit.FromPoint(1224);
                    break;
            }
            if (height > 0)
                pageHeight = Unit.FromMillimeter(height);
            if (width > 0)
                pageWidth = Unit.FromMillimeter(width);
        }

        /// <summary>
        /// Gets or sets a value which defines whether the section starts on next, odd, or even page.
        /// </summary>
        public BreakType SectionStart
        {
            get => Values.SectionStart ?? BreakType.BreakNextPage;
            set
            {
                EnsureNotFrozen();
                Values.SectionStart = value;
            }
        }

        /// <summary>
        /// Gets or sets the page orientation of the section.
        /// If Orientation is set, the PageFormat will be rotated if necessary.
        /// If PageWidth and PageHeight are set, the actual orientation will be defined by width and height.
        /// This applies also if only one of those values is set and the other one is calculated according to PageFormat and Orientation values. 
        /// A quadratic page is considered to be Portrait.
        /// </summary>
        public Orientation Orientation
        {
            get => Values.Orientation ?? Orientation.Portrait;
            set
            {
                EnsureNotFrozen();
                            Values.Orientation = value;
            }
        }

        bool IsLandscape => Orientation == Orientation.Landscape;

        /// <summary>
        /// Gets or sets the page width.
        /// </summary>
        public Unit PageWidth
        {
            get => Values.PageWidth ?? Unit.Empty;
            set
            {
                EnsureNotFrozen();
                Values.PageWidth = value;
            }
        }

        /// <summary>
        /// This is now the same as PageWidth.
        /// </summary>
        [Obsolete("EffectivePageWidth used to return the actual width of the page, while PageWidth returned the width of the non-rotated page format." +
                  "As of now, PageWidth is already the effective page width.")]
        public Unit EffectivePageWidth => PageWidth;

        /// <summary>
        /// Gets or sets the starting number for the first section page.
        /// </summary>
        public int StartingNumber
        {
            get => Values.StartingNumber ?? 0;
            set
            {
                EnsureNotFrozen();
                Values.StartingNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets the page height.
        /// </summary>
        public Unit PageHeight
        {
            get => Values.PageHeight ?? Unit.Empty;
            set
            {
                EnsureNotFrozen();
                Values.PageHeight = value;
            }
        }

        /// <summary>
        /// Resets PageWidth, PageHeight, PageFormat, and Orientation. 
        /// Useful for example before defining a new page size for a cloned PageSetup.
        /// This ensures that no other previously set page size values will interfere with the newly set values.
        /// </summary>
        public void ResetPageSize()
        {
            EnsureNotFrozen();

            Values.PageWidth = null;
            Values.PageHeight = null;
            Values.PageFormat = null;
            Values.Orientation = null;
        }

        /// <summary>
        /// This is now the same as PageHeight.
        /// </summary>
        [Obsolete("EffectivePageHeight used to return the actual height of the page, while PageHeight returned the height of the non-rotated page format." +
                  "As of now, PageHeight is already the effective page height.")]
        public Unit EffectivePageHeight => PageHeight;

        /// <summary>
        /// Gets or sets the top margin of the pages in the section.
        /// </summary>
        public Unit TopMargin
        {
            get => Values.TopMargin ?? Unit.Empty;
            set
            {
                EnsureNotFrozen();
                Values.TopMargin = value;
            }
        }

        /// <summary>
        /// Gets or sets the bottom margin of the pages in the section.
        /// </summary>
        public Unit BottomMargin
        {
            get => Values.BottomMargin ?? Unit.Empty;
            set
            {
                EnsureNotFrozen();
                Values.BottomMargin = value;
            }
        }

        /// <summary>
        /// Gets or sets the left margin of the pages in the section.
        /// </summary>
        public Unit LeftMargin
        {
            get => Values.LeftMargin ?? Unit.Empty;
            set
            {
                EnsureNotFrozen();
                Values.LeftMargin = value;
            }
        }

        /// <summary>
        /// Gets or sets the right margin of the pages in the section.
        /// </summary>
        public Unit RightMargin
        {
            get => Values.RightMargin ?? Unit.Empty;
            set
            {
                EnsureNotFrozen();
                Values.RightMargin = value;
            }
        }

        /// <summary>
        /// Gets or sets a value which defines whether the odd and even pages
        /// of the section have different header and footer.
        /// </summary>
        public bool OddAndEvenPagesHeaderFooter
        {
            get => Values.OddAndEvenPagesHeaderFooter ?? false;
            set
            {
                EnsureNotFrozen();
                Values.OddAndEvenPagesHeaderFooter = value;
            }
        }

        /// <summary>
        /// Gets or sets a value which define whether the section has a different
        /// first page header and footer.
        /// </summary>
        public bool DifferentFirstPageHeaderFooter
        {
            get => Values.DifferentFirstPageHeaderFooter ?? false;
            set
            {
                EnsureNotFrozen();
                Values.DifferentFirstPageHeaderFooter = value;
            }
        }

        /// <summary>
        /// Gets or sets the distance between the header and the page top
        /// of the pages in the section.
        /// </summary>
        public Unit HeaderDistance
        {
            get => Values.HeaderDistance ?? Unit.Empty;
            set
            {
                EnsureNotFrozen();
                Values.HeaderDistance = value;
            }
        }

        /// <summary>
        /// Gets or sets the distance between the footer and the page bottom
        /// of the pages in the section.
        /// </summary>
        public Unit FooterDistance
        {
            get => Values.FooterDistance ?? Unit.Empty;
            set
            {
                EnsureNotFrozen();
                Values.FooterDistance = value;
            }
        }

        /// <summary>
        /// Gets or sets a value which defines whether the odd and even pages
        /// of the section should change left and right margin.
        /// </summary>
        public bool MirrorMargins
        {
            get => Values.MirrorMargins ?? false;
            set
            {
                EnsureNotFrozen();
                Values.MirrorMargins = value;
            }
        }

        /// <summary>
        /// Gets or sets a value which defines whether a page should break horizontally.
        /// Currently only tables are supported.
        /// </summary>
        public bool HorizontalPageBreak
        {
            get => Values.HorizontalPageBreak ?? false;
            set
            {
                EnsureNotFrozen();
                Values.HorizontalPageBreak = value;
            }
        }

        /// <summary>
        /// Gets or sets the page format of the section.
        /// </summary>
        public PageFormat PageFormat
        {
            get => Values.PageFormat ?? PageFormat.A0;
            set
            {
                EnsureNotFrozen();
                Values.PageFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets a comment associated with this object.
        /// </summary>
        public string Comment
        {
            get => Values.Comment ?? "";
            set
            {
                EnsureNotFrozen();
                Values.Comment = value;
            }
        }

        /// <summary>
        /// Gets the PageSetup of the previous section, or null if the page setup belongs 
        /// to the first section.
        /// </summary>
        public PageSetup? PreviousPageSetup()
        {
            if (Parent is Section section)
                return section.PreviousSection()?.PageSetup;
            return null;
        }

        /// <summary>
        /// Gets a PageSetup object with default values for all properties.
        /// User code must not change any value in DefaultPageSetup because these values are the reference used
        /// when persisting DOM objects in MDDDL files.
        /// Create "Clone()" of DefaultPageSetup and make the necessary changes.
        /// </summary>
        public static PageSetup DefaultPageSetup
        {
            get
            {
                if (_defaultPageSetup == null)
                {
#if true
                    // This is the default metric PageSetup. If we ever create a different one
                    // for non-metric countries we have to write this information into MDDDL.
                    // Currently, we only use this default values.
                    _defaultPageSetup = new()
                    {
                        PageFormat = PageFormat.A4,
                        SectionStart = BreakType.BreakNextPage,
                        Orientation = Orientation.Portrait,
                        PageWidth = "21cm",
                        PageHeight = "29.7cm",
                        TopMargin = "2.5cm",
                        BottomMargin = "2cm",
                        LeftMargin = "2.5cm",
                        RightMargin = "2.5cm",
                        HeaderDistance = "1.25cm",
                        FooterDistance = "1.25cm",
                        OddAndEvenPagesHeaderFooter = false,
                        DifferentFirstPageHeaderFooter = false,
                        MirrorMargins = false,
                        HorizontalPageBreak = false
                    };
#else // KEEP for reference if we create a non-metric setup and fix the MDDDL issue.
                    // Check if the current country/region uses the metric system of measurement
                    // and then use page format A4 anyway.
                    var culture = CultureInfo.CurrentCulture;
                    if (culture.IsNeutralCulture)
                        culture = CultureInfo.InvariantCulture;

                    var regionInfo = culture.Name.Length > 0 ? new RegionInfo(culture.Name) : null;
                    var isMetric = regionInfo?.IsMetric ?? true;

                Metric:
                    if (isMetric)
                    {
                        // This is the default metric PageSetup. If we ever create a different one
                        // for non-metric countries we have to write this information into MDDDL.
                        _defaultPageSetup = new()
                        {
                            PageFormat = PageFormat.A4,
                            SectionStart = BreakType.BreakNextPage,
                            Orientation = Orientation.Portrait,
                            PageWidth = "21cm",
                            PageHeight = "29.7cm",
                            TopMargin = "2.5cm",
                            BottomMargin = "2cm",
                            LeftMargin = "2.5cm",
                            RightMargin = "2.5cm",
                            HeaderDistance = "1.25cm",
                            FooterDistance = "1.25cm",
                            OddAndEvenPagesHeaderFooter = false,
                            DifferentFirstPageHeaderFooter = false,
                            MirrorMargins = false,
                            HorizontalPageBreak = false
                        };
                    }
                    else
                    {
                        // TODO_OLD: Maybe useful in the future, but MDDDL now depends on the settings when it was created.
                        // Just go to metric case.
                        isMetric = true;
                        goto Metric;
                    }
#endif
                    _defaultPageSetup.Freeze();
                }
                return _defaultPageSetup;
            }
        }
        static PageSetup? _defaultPageSetup;

        /// <summary>
        /// Converts PageSetup into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteComment(Values.Comment);
            int pos = serializer.BeginContent("PageSetup");
            {
                if (!Values.PageHeight.IsValueNullOrEmpty())
                    serializer.WriteSimpleAttribute("PageHeight", PageHeight);

                if (!Values.PageWidth.IsValueNullOrEmpty())
                    serializer.WriteSimpleAttribute("PageWidth", PageWidth);

                if (Values.Orientation is not null)
                    serializer.WriteSimpleAttribute("Orientation", Orientation);

                if (!Values.LeftMargin.IsValueNullOrEmpty())
                    serializer.WriteSimpleAttribute("LeftMargin", LeftMargin);

                if (!Values.LeftMargin.IsValueNullOrEmpty())
                    serializer.WriteSimpleAttribute("RightMargin", RightMargin);

                if (!Values.TopMargin.IsValueNullOrEmpty())
                    serializer.WriteSimpleAttribute("TopMargin", TopMargin);

                if (!Values.BottomMargin.IsValueNullOrEmpty())
                    serializer.WriteSimpleAttribute("BottomMargin", BottomMargin);

                if (!Values.FooterDistance.IsValueNullOrEmpty())
                    serializer.WriteSimpleAttribute("FooterDistance", FooterDistance);

                if (!Values.HeaderDistance.IsValueNullOrEmpty())
                    serializer.WriteSimpleAttribute("HeaderDistance", HeaderDistance);

                if (Values.OddAndEvenPagesHeaderFooter is not null)
                    serializer.WriteSimpleAttribute("OddAndEvenPagesHeaderFooter", OddAndEvenPagesHeaderFooter);

                if (Values.DifferentFirstPageHeaderFooter is not null)
                    serializer.WriteSimpleAttribute("DifferentFirstPageHeaderFooter", DifferentFirstPageHeaderFooter);

                if (Values.SectionStart is not null)
                    serializer.WriteSimpleAttribute("SectionStart", SectionStart);

                if (Values.PageFormat is not null)
                    serializer.WriteSimpleAttribute("PageFormat", PageFormat);

                if (Values.MirrorMargins is not null)
                    serializer.WriteSimpleAttribute("MirrorMargins", MirrorMargins);

                if (Values.HorizontalPageBreak is not null)
                    serializer.WriteSimpleAttribute("HorizontalPageBreak", HorizontalPageBreak);

                if (Values.StartingNumber is not null)
                    serializer.WriteSimpleAttribute("StartingNumber", StartingNumber);
            }
            serializer.EndContent(pos);
        }

        void EnsureNotFrozen()
        {
            // It is still possible to change DefaultPageSetup directly via Values,
            // but that’s your own bad luck.
            if (_frozen)
                throw new InvalidOperationException("DefaultPageSetup must not be changed. Change the PageSetup member of your Section. You can assign a Clone() of DefaultPageSetup to that PageSetup member as needed.");
        }

        bool _frozen;

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(PageSetup));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public PageSetupValues Values => (PageSetupValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class PageSetupValues : Values
        {
            internal PageSetupValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public BreakType? SectionStart { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Orientation? Orientation { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? PageWidth { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public int? StartingNumber { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? PageHeight { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? TopMargin { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? BottomMargin { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? LeftMargin { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? RightMargin { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? OddAndEvenPagesHeaderFooter { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? DifferentFirstPageHeaderFooter { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? HeaderDistance { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? FooterDistance { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? MirrorMargins { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? HorizontalPageBreak { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public PageFormat? PageFormat { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Comment { get; set; }
        }
    }
}
