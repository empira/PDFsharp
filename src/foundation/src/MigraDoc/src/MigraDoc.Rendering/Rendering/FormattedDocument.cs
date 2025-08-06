// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Globalization;
using PdfSharp;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Represents a formatted document.
    /// </summary>
    public class FormattedDocument : IAreaProvider
    {
        enum PagePosition
        {
            First,
            Odd,
            Even
        }

        readonly struct HeaderFooterPosition
        {
            internal HeaderFooterPosition(int sectionNr, PagePosition pagePosition)
            {
                _sectionNr = sectionNr;
                _pagePosition = pagePosition;
            }

            public override bool Equals(object? obj)
            {
                if (obj is HeaderFooterPosition hfp)
                {
                    //HeaderFooterPosition hfp = (HeaderFooterPosition)obj;
                    return _sectionNr == hfp._sectionNr && _pagePosition == hfp._pagePosition;
                }
                return false;
            }

            public override int GetHashCode()
                => _sectionNr.GetHashCode() ^ _pagePosition.GetHashCode();

            readonly int _sectionNr;
            readonly PagePosition _pagePosition;
        }

        internal FormattedDocument(Document document, DocumentRenderer documentRenderer)
        {
            _document = document;
            _documentRenderer = documentRenderer;
        }

        /// <summary>
        /// Formats the document by performing line breaks and page breaks.
        /// </summary>
        internal void Format(XGraphics gfx)
        {
            //_bookmarks = new Dictionary<string, FieldInfos.BookmarkInfo>();
            //_pageRenderInfos = new Dictionary<int, List<RenderInfo>>();
            //_pageInfos = new Dictionary<int, PageInfo>();
            //_pageFieldInfos = new Dictionary<int, FieldInfos>();
            //_formattedHeaders = new Dictionary<HeaderFooterPosition, FormattedHeaderFooter>();
            //_formattedFooters = new Dictionary<HeaderFooterPosition, FormattedHeaderFooter>();
            _gfx = gfx;
            _currentPage = 0;
            _sectionNumber = 0;
            PageCount = 0;
            _shownPageNumber = 0;
            _documentRenderer.ProgressCompleted = 0;
            _documentRenderer.ProgressMaximum = 0;
            if (_documentRenderer.HasPrepareDocumentProgress)
            {
                foreach (var section in _document.Sections.Cast<Section>())
                    _documentRenderer.ProgressMaximum += section.Elements.Count;
            }
            foreach (var section in _document.Sections.Cast<Section>())
            {
                _isNewSection = true;
                _currentSection = section;
                ++_sectionNumber;
                if (NeedsEmptyPage())
                    InsertEmptyPage();

                var formatter = new TopDownFormatter(this, _documentRenderer, section.Elements);
                formatter.FormatOnAreas(gfx, true);
                FillSectionPagesInfo();
                _documentRenderer.ProgressCompleted += section.Elements.Count;
            }
            PageCount = _currentPage;
            FillNumPagesInfo();
        }

        PagePosition CurrentPagePosition
        {
            get
            {
                if (_isNewSection)
                    return PagePosition.First;

                // Choose header and footer based on the shown page number, not the physical page number.
                if (_shownPageNumber % 2 == 0)
                    return PagePosition.Even;
                return PagePosition.Odd;
            }
        }

        void FormatHeadersFooters()
        {
            var headers = _currentSection.Values.Headers;
            if (headers != null)
            {
                var pagePos = CurrentPagePosition;
                var hfp = new HeaderFooterPosition(_sectionNumber, pagePos);
                if (!_formattedHeaders.ContainsKey(hfp))
                    FormatHeader(hfp, ChooseHeaderFooter(headers, pagePos));
            }

            var footers = _currentSection.Values.Footers;
            if (footers != null)
            {
                var pagePos = CurrentPagePosition;
                var hfp = new HeaderFooterPosition(_sectionNumber, pagePos);
                if (!_formattedFooters.ContainsKey(hfp))
                    FormatFooter(hfp, ChooseHeaderFooter(footers, pagePos));
            }
        }

        void FormatHeader(HeaderFooterPosition hfp, HeaderFooter? header)
        {
            if (header != null && !_formattedHeaders.ContainsKey(hfp))
            {
                var formattedHeaderFooter = new FormattedHeaderFooter(header, _documentRenderer, _currentFieldInfos)
                {
                    ContentRect = GetHeaderArea(_currentSection, _currentPage)
                };
                formattedHeaderFooter.Format(_gfx);
                _formattedHeaders.Add(hfp, formattedHeaderFooter);
            }
        }

        void FormatFooter(HeaderFooterPosition hfp, HeaderFooter? footer)
        {
            if (footer != null && !_formattedFooters.ContainsKey(hfp))
            {
                var formattedHeaderFooter = new FormattedHeaderFooter(footer, _documentRenderer, _currentFieldInfos);
                formattedHeaderFooter.ContentRect = GetFooterArea(_currentSection, _currentPage);
                formattedHeaderFooter.Format(_gfx);
                _formattedFooters.Add(hfp, formattedHeaderFooter);
            }
        }

        /// <summary>
        /// Fills the number pages information after formatting the document.
        /// </summary>
        void FillNumPagesInfo()
        {
            for (int page = 1; page <= PageCount; ++page)
            {
                if (IsEmptyPage(page))
                    continue;

                var fieldInfos = _pageFieldInfos[page];
                fieldInfos.NumPages = PageCount;
            }
        }

        /// <summary>
        /// Fills the section pages information after formatting a section.
        /// </summary>
        void FillSectionPagesInfo()
        {
            for (int page = _currentPage; page > 0; --page)
            {
                if (IsEmptyPage(page))
                    continue;

                FieldInfos fieldInfos = _pageFieldInfos[page];
                if (fieldInfos.Section != _sectionNumber)
                    break;

                fieldInfos.SectionPages = _sectionPages;
            }
        }

        Rectangle CalcContentRect(int page)
        {
            var pageSetup = _currentSection.PageSetup;
            XUnitPt width = pageSetup.PageWidth.Point;

            width -= pageSetup.RightMargin.Point;
            width -= pageSetup.LeftMargin.Point;
            
            XUnitPt height = pageSetup.PageHeight.Point;

            height -= pageSetup.TopMargin.Point;
            height -= pageSetup.BottomMargin.Point;
            XUnitPt x;
            XUnitPt y = pageSetup.TopMargin.Point;
            if (pageSetup.MirrorMargins)
                x = page % 2 == 0 ? pageSetup.RightMargin.Point : pageSetup.LeftMargin.Point;
            else
                x = pageSetup.LeftMargin.Point;
            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Gets the rendering information for the page content.
        /// </summary>
        /// <param name="page">The page to render.</param>
        /// <returns>Rendering information for the page content.</returns>
        public RenderInfo[]? GetRenderInfos(int page)
        {
            if (_pageRenderInfos.ContainsKey(page))
                return (_pageRenderInfos[page]).ToArray();
            return null;
        }
        readonly Dictionary<int, List<RenderInfo>> _pageRenderInfos = new();

        /// <summary>
        /// Gets a formatted header/footer object for header of the given page.
        /// </summary>
        /// <param name="page">The physical page the header shall appear on.</param>
        /// <returns>The required header, null if none exists to render.</returns>
        internal FormattedHeaderFooter? GetFormattedHeader(int page)
        {
            var fieldInfos = _pageFieldInfos[page];
            int logicalPage = fieldInfos.DisplayPageNr;

            var pagePos = logicalPage % 2 == 0 ? PagePosition.Even : PagePosition.Odd;

            if (page == 1)
                pagePos = PagePosition.First;
            else //page > 1
            {
                if (IsEmptyPage(page - 1)) // these empty pages only occur between sections.
                    pagePos = PagePosition.First;
                else
                {
                    FieldInfos prevFieldInfos = _pageFieldInfos[page - 1];
                    if (fieldInfos.Section != prevFieldInfos.Section)
                        pagePos = PagePosition.First;
                }
            }

            var hfp = new HeaderFooterPosition(fieldInfos.Section, pagePos);
            if (_formattedHeaders.ContainsKey(hfp))
                return _formattedHeaders[hfp];
            return null;
        }

        /// <summary>
        /// Gets a formatted header/footer object for footer of the given page.
        /// </summary>
        /// <param name="page">The physical page the footer shall appear on.</param>
        /// <returns>The required footer, null if none exists to render.</returns>
        internal FormattedHeaderFooter? GetFormattedFooter(int page)
        {
            var fieldInfos = _pageFieldInfos[page];
            int logicalPage = fieldInfos.DisplayPageNr;

            var pagePos = logicalPage % 2 == 0 ? PagePosition.Even : PagePosition.Odd;

            if (page == 1)
                pagePos = PagePosition.First;
            else //page > 1
            {
                if (IsEmptyPage(page - 1)) // these empty pages only occur between sections.
                    pagePos = PagePosition.First;
                else
                {
                    var prevFieldInfos = _pageFieldInfos[page - 1];
                    if (fieldInfos.Section != prevFieldInfos.Section)
                        pagePos = PagePosition.First;
                }
            }

            var hfp = new HeaderFooterPosition(fieldInfos.Section, pagePos);
            if (_formattedFooters.ContainsKey(hfp))
                return _formattedFooters[hfp];
            return null;
        }

        Rectangle GetHeaderArea(Section section, int page)
        {
            var pageSetup = section.PageSetup;
            XUnitPt xPos;
            if (pageSetup.MirrorMargins && page % 2 == 0)
                xPos = pageSetup.RightMargin.Point;
            else
                xPos = pageSetup.LeftMargin.Point;

            XUnitPt width = pageSetup.PageWidth.Point;
            width -= (pageSetup.LeftMargin + pageSetup.RightMargin).Point;

            XUnitPt yPos = pageSetup.HeaderDistance.Point;
            XUnitPt height = (pageSetup.TopMargin - pageSetup.HeaderDistance).Point;
            return new Rectangle(xPos, yPos, width, height);
        }

        internal Rectangle GetHeaderArea(int page)
        {
            var fieldInfos = _pageFieldInfos[page];
            var section = _document.Sections[fieldInfos.Section - 1];
            return GetHeaderArea(section, page);
        }

        internal Rectangle GetFooterArea(int page)
        {
            var fieldInfos = _pageFieldInfos[page];
            var section = _document.Sections[fieldInfos.Section - 1];
            return GetFooterArea(section, page);
        }

        Rectangle GetFooterArea(Section section, int page)
        {
            var pageSetup = section.PageSetup;
            XUnitPt xPos;
            if (pageSetup.MirrorMargins && page % 2 == 0)
                xPos = pageSetup.RightMargin.Point;
            else
                xPos = pageSetup.LeftMargin.Point;

            XUnitPt width = pageSetup.PageWidth.Point;
            width -= (pageSetup.LeftMargin + pageSetup.RightMargin).Point;
            XUnitPt yPos = pageSetup.PageHeight.Point;

            yPos -= pageSetup.BottomMargin.Point;
            XUnitPt height = (pageSetup.BottomMargin - pageSetup.FooterDistance).Point;
            return new Rectangle(xPos, yPos, width, height);
        }

        HeaderFooter? ChooseHeaderFooter(HeadersFooters? hfs, PagePosition pagePos)
        {
            if (hfs == null)
                return null;

            PageSetup pageSetup = _currentSection.PageSetup;

            if (pagePos == PagePosition.First)
            {
                if (pageSetup.DifferentFirstPageHeaderFooter)
                    return hfs.Values.FirstPage;
            }
            if (pagePos == PagePosition.Even || _shownPageNumber/*_currentPage*/ % 2 == 0)
            {
                if (pageSetup.OddAndEvenPagesHeaderFooter)
                    return hfs.Values.EvenPage;
            }
            return hfs.Values.Primary;
        }

        /// <summary>
        /// Gets the number of pages of the document.
        /// </summary>
        public int PageCount { get; private set; }

        /// <summary>
        /// Gets information about the specified page.
        /// </summary>
        /// <param name="page">The page the information is asked for.</param>
        /// <returns>The page information.</returns>
        public PageInfo GetPageInfo(int page)
        {
            if (page < 1 || page > PageCount)
                throw new ArgumentOutOfRangeException(nameof(page), page, page.ToString(CultureInfo.InvariantCulture));

            return _pageInfos[page];
        }

        #region IAreaProvider Members

        Area IAreaProvider.GetNextArea()
        {
            if (_isNewSection)
                _sectionPages = 0;

            ++_currentPage;
            ++_shownPageNumber;
            ++_sectionPages;
            InitFieldInfos();
            FormatHeadersFooters();
            _isNewSection = false;
            return CalcContentRect(_currentPage);
        }

        int _currentPage;

        Area IAreaProvider.ProbeNextArea()
            => CalcContentRect(_currentPage + 1);

        void InitFieldInfos()
        {
            _currentFieldInfos = new(_bookmarks)
            {
                PhysicalPageNr = _currentPage,
                Section = _sectionNumber
            };

            if (_isNewSection && _currentSection.PageSetup.Values.StartingNumber is not null)
                _shownPageNumber = _currentSection.PageSetup.StartingNumber;

            _currentFieldInfos.DisplayPageNr = _shownPageNumber;
        }

        void IAreaProvider.StoreRenderInfos(List<RenderInfo> renderInfos)
        {
            _pageRenderInfos.Add(_currentPage, renderInfos);
            var pageSize = CalcPageSize(_currentSection.PageSetup);
            var pageOrientation = CalcPageOrientation(_currentSection.PageSetup);
            var pageInfo = new PageInfo(pageSize.Width, pageSize.Height, pageOrientation);
            _pageInfos.Add(_currentPage, pageInfo);
            _pageFieldInfos.Add(_currentPage, _currentFieldInfos);
        }

        PageOrientation CalcPageOrientation(PageSetup _)
        {
            var pageOrientation = PageOrientation.Portrait;
            if (_currentSection.PageSetup.Orientation == Orientation.Landscape)
                pageOrientation = PageOrientation.Landscape;

            return pageOrientation;
        }

        XSize CalcPageSize(PageSetup pageSetup)
            => new(pageSetup.PageWidth.Point, pageSetup.PageHeight.Point);

        bool IAreaProvider.PositionHorizontally(LayoutInfo layoutInfo)
        {
            return layoutInfo.HorizontalReference switch
            {
                HorizontalReference.PageMargin => PositionHorizontallyToMargin(layoutInfo),
                HorizontalReference.AreaBoundary => PositionHorizontallyToMargin(layoutInfo),
                HorizontalReference.Page => PositionHorizontallyToPage(layoutInfo),
                _ => false
            };
        }

        /// <summary>
        /// Gets the alignment depending on the currentPage for the alignments "Outside" and "Inside".
        /// </summary>
        /// <param name="alignment">The original alignment.</param>
        /// <returns>The alignment depending on the currentPage for the alignments "Outside" and "Inside".</returns>
        ElementAlignment GetCurrentAlignment(ElementAlignment alignment)
        {
            var align = alignment;

            align = align switch
            {
                ElementAlignment.Inside => _currentPage % 2 == 0 ? ElementAlignment.Far : ElementAlignment.Near,
                ElementAlignment.Outside => _currentPage % 2 == 0 ? ElementAlignment.Near : ElementAlignment.Far,
                _ => align
            };
            return align;
        }

        bool PositionHorizontallyToMargin(LayoutInfo layoutInfo)
        {
            var rect = CalcContentRect(_currentPage);
            var align = GetCurrentAlignment(layoutInfo.HorizontalAlignment);

            switch (align)
            {
                case ElementAlignment.Near:
                    if (layoutInfo.Left != 0)
                    {
                        layoutInfo.ContentArea.X += layoutInfo.Left;
                        return true;
                    }
                    if (layoutInfo.MarginLeft != 0)
                    {
                        layoutInfo.ContentArea.X += layoutInfo.MarginLeft;
                        return true;
                    }
                    return false;

                case ElementAlignment.Far:
                    XUnitPt xPos = rect.X + rect.Width;
                    xPos -= layoutInfo.ContentArea.Width;
                    xPos -= layoutInfo.MarginRight;
                    layoutInfo.ContentArea.X = xPos;
                    return true;

                case ElementAlignment.Center:
                    xPos = rect.Width;
                    xPos -= layoutInfo.ContentArea.Width;
                    xPos = rect.X + xPos / 2;
                    layoutInfo.ContentArea.X = xPos;
                    return true;
            }
            return false;
        }

        bool PositionHorizontallyToPage(LayoutInfo layoutInfo)
        {
            XUnitPt xPos;
            var align = GetCurrentAlignment(layoutInfo.HorizontalAlignment);
            switch (align)
            {
                case ElementAlignment.Near:
#if true
                    // Attempt to make it compatible with MigraDoc CPP.
                    // Ignore layoutInfo.Left if absolute position is specified in layoutInfo.MarginLeft.
                    // Use layoutInfo.Left if layoutInfo.MarginLeft is 0.
                    // TODO_OLD We would need HasValue for XUnitPt to determine whether a value was assigned.
                    if (layoutInfo.HorizontalReference is HorizontalReference.Page or HorizontalReference.PageMargin)
                        xPos = layoutInfo.MarginLeft != 0 ? layoutInfo.MarginLeft : layoutInfo.Left;
                    else
                        xPos = Math.Max(layoutInfo.MarginLeft, layoutInfo.Left);
#else
                    if (layoutInfo.HorizontalReference == HorizontalReference.Page ||
                      layoutInfo.HorizontalReference == HorizontalReference.PageMargin)
                        xPos = layoutInfo.MarginLeft; // ignore layoutInfo.Left if absolute position is specified
                    else
                        xPos = Math.Max(layoutInfo.MarginLeft, layoutInfo.Left);
#endif
                    layoutInfo.ContentArea.X = xPos;
                    break;

                case ElementAlignment.Far:
                    xPos = _currentSection.PageSetup.PageWidth.Point;
                    xPos -= layoutInfo.ContentArea.Width;
                    xPos -= layoutInfo.MarginRight;
                    layoutInfo.ContentArea.X = xPos;
                    break;

                case ElementAlignment.Center:
                    xPos = _currentSection.PageSetup.PageWidth.Point;
                    xPos -= layoutInfo.ContentArea.Width;
                    xPos /= 2;
                    layoutInfo.ContentArea.X = xPos;
                    break;
            }
            return true;
        }

        bool PositionVerticallyToMargin(LayoutInfo layoutInfo)
        {
            Rectangle rect = CalcContentRect(_currentPage);
            XUnitPt yPos;
            switch (layoutInfo.VerticalAlignment)
            {
                case ElementAlignment.Near:
                    yPos = rect.Y;
                    if (layoutInfo.Top == 0)
                        yPos += layoutInfo.MarginTop;
                    else
                        yPos += layoutInfo.Top;
                    layoutInfo.ContentArea.Y = yPos;
                    break;

                case ElementAlignment.Far:
                    yPos = rect.Y + rect.Height;
                    yPos -= layoutInfo.ContentArea.Height;
                    yPos -= layoutInfo.MarginBottom;
                    layoutInfo.ContentArea.Y = yPos;
                    break;

                case ElementAlignment.Center:
                    yPos = rect.Height;
                    yPos -= layoutInfo.ContentArea.Height;
                    yPos = rect.Y + yPos / 2;
                    layoutInfo.ContentArea.Y = yPos;
                    break;
            }
            return true;
        }

        bool NeedsEmptyPage()
        {
            int nextPage = _currentPage + 1;
            var pageSetup = _currentSection.PageSetup;
            bool startOnEvenPage = pageSetup.SectionStart == BreakType.BreakEvenPage;
            bool startOnOddPage = pageSetup.SectionStart == BreakType.BreakOddPage;

            if (startOnOddPage)
                return nextPage % 2 == 0;
            if (startOnEvenPage)
                return nextPage % 2 == 1;

            return false;
        }

        void InsertEmptyPage()
        {
            ++_currentPage;
            ++_shownPageNumber;
            _emptyPages.Add(_currentPage, null);

            var pageSize = CalcPageSize(_currentSection.PageSetup);
            var pageOrientation = CalcPageOrientation(_currentSection.PageSetup);
            var pageInfo = new PageInfo(pageSize.Width, pageSize.Height, pageOrientation);
            _pageInfos.Add(_currentPage, pageInfo);
        }

        bool PositionVerticallyToPage(LayoutInfo layoutInfo)
        {
            XUnitPt yPos;
            switch (layoutInfo.VerticalAlignment)
            {
                case ElementAlignment.Near:
                    yPos = Math.Max(layoutInfo.MarginTop, layoutInfo.Top);
                    layoutInfo.ContentArea.Y = yPos;
                    break;

                case ElementAlignment.Far:
                    yPos = _currentSection.PageSetup.PageHeight.Point;
                    yPos -= layoutInfo.ContentArea.Height;
                    yPos -= layoutInfo.MarginBottom;
                    layoutInfo.ContentArea.Y = yPos;
                    break;

                case ElementAlignment.Center:
                    yPos = _currentSection.PageSetup.PageHeight.Point;
                    yPos -= layoutInfo.ContentArea.Height;
                    yPos /= 2;
                    layoutInfo.ContentArea.Y = yPos;
                    break;
            }
            return true;
        }

        bool IAreaProvider.PositionVertically(LayoutInfo layoutInfo)
        {
            return layoutInfo.VerticalReference switch
            {
                VerticalReference.PreviousElement => false,
                VerticalReference.AreaBoundary => PositionVerticallyToMargin(layoutInfo),
                VerticalReference.PageMargin => PositionVerticallyToMargin(layoutInfo),
                VerticalReference.Page => PositionVerticallyToPage(layoutInfo),
                _ => false
            };
        }

        internal FieldInfos GetFieldInfos(int page)
            => _pageFieldInfos[page];

        FieldInfos IAreaProvider.AreaFieldInfos => _currentFieldInfos;

        bool IAreaProvider.IsAreaBreakBefore(LayoutInfo layoutInfo)
            => layoutInfo.PageBreakBefore;

        internal bool IsEmptyPage(int page)
            => _emptyPages.ContainsKey(page);

        #endregion

        int _sectionPages;
        int _shownPageNumber;
        int _sectionNumber;
        Section _currentSection = null!;  // Set in foreach loop.
        bool _isNewSection;
        FieldInfos _currentFieldInfos = null!;  // Set in InitFieldInfos.
        readonly Dictionary<string, FieldInfos.BookmarkInfo> _bookmarks = [];
        readonly Dictionary<int, FieldInfos> _pageFieldInfos = [];
        readonly Dictionary<HeaderFooterPosition, FormattedHeaderFooter> _formattedHeaders = [];
        readonly Dictionary<HeaderFooterPosition, FormattedHeaderFooter> _formattedFooters = [];
        readonly DocumentRenderer _documentRenderer;
        readonly Dictionary<int, PageInfo> _pageInfos = [];
        readonly Dictionary<int, object?> _emptyPages = [];
        readonly Document _document;
        XGraphics _gfx = null!;  // Set in Format
    }
}
