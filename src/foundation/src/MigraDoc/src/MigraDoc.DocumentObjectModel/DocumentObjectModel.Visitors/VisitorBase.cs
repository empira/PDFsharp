// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Shapes.Charts;

namespace MigraDoc.DocumentObjectModel.Visitors
{
    /// <summary>
    /// Base class of all visitors.
    /// </summary>
    public abstract class VisitorBase : DocumentObjectVisitor
    {
        /// <summary>
        /// Visits a hierarchy of document objects.
        /// </summary>
        /// <param name="documentObject">The root document object.</param>
        public override void Visit(DocumentObject documentObject)
        {
            if (documentObject is IVisitable visitable)
                visitable.AcceptVisitor(this, true);
        }

        /// <summary>
        /// Flattens the the specified document object.
        /// </summary>
        protected void FlattenParagraphFormat(ParagraphFormat format, ParagraphFormat? refFormat)
        {
            if (refFormat != null)
            {
                var refValues = refFormat.Values;
                var values = format.Values;
                if (values.Alignment is null)
                    values.Alignment = refValues.Alignment;

                if (values.FirstLineIndent.IsValueNullOrEmpty())
                    values.FirstLineIndent = refValues.FirstLineIndent;

                if (values.LeftIndent.IsValueNullOrEmpty())
                    values.LeftIndent = refValues.LeftIndent;

                if (values.RightIndent.IsValueNullOrEmpty())
                    values.RightIndent = refValues.RightIndent;

                if (values.SpaceBefore.IsValueNullOrEmpty())
                    values.SpaceBefore = refValues.SpaceBefore;

                if (values.SpaceAfter.IsValueNullOrEmpty())
                    values.SpaceAfter = refValues.SpaceAfter;

                if (values.LineSpacingRule is null)
                    values.LineSpacingRule = refValues.LineSpacingRule;
                if (values.LineSpacing is null)
                    values.LineSpacing = refValues.LineSpacing;

                if (values.WidowControl is null)
                    values.WidowControl = refValues.WidowControl;

                if (values.KeepTogether is null)
                    values.KeepTogether = refValues.KeepTogether;

                if (values.KeepWithNext is null)
                    values.KeepWithNext = refValues.KeepWithNext;

                if (values.PageBreakBefore is null)
                    values.PageBreakBefore = refValues.PageBreakBefore;

                if (values.OutlineLevel is null)
                    values.OutlineLevel = refValues.OutlineLevel;

                if (values.Font is null)
                {
                    if (refValues.Font is not null)
                    {
                        // The font is cloned here to avoid parent problems.
                        values.Font = refValues.Font.Clone();
                        values.Font.Parent = format;
                    }
                }
                else if (refValues.Font is not null)
                    FlattenFont(values.Font, refValues.Font);

                if (values.Shading is null)
                {
                    if (refValues.Shading is not null)
                    {
                        values.Shading = refValues.Shading.Clone();
                        values.Shading.Parent = format;
                    }
                }
                else if (refValues.Shading is not null)
                    FlattenShading(values.Shading, refValues.Shading);

                if (values.Borders is null)
                    values.Borders = refValues.Borders;
                else if (refValues.Borders != null)
                    FlattenBorders(values.Borders, refValues.Borders);

                //if (format.tabStops == null)
                //    format.tabStops = refFormat.tabStops;
                if (refValues.TabStops is not null)
                    FlattenTabStops(format.TabStops, refValues.TabStops);

                if (refValues.ListInfo is not null)
                    FlattenListInfo(format.ListInfo, refValues.ListInfo);
                return;
            }
            throw new ArgumentNullException(nameof(refFormat));
        }

        /// <summary>
        /// Flattens the the specified document object.
        /// </summary>
        protected void FlattenListInfo(ListInfo listInfo, ListInfo refListInfo)
        {
            if (listInfo.Values.ContinuePreviousList is null)
                listInfo.Values.ContinuePreviousList = refListInfo.Values.ContinuePreviousList;
            if (listInfo.Values.ListType is null)
                listInfo.Values.ListType = refListInfo.Values.ListType;
            if (listInfo.Values.NumberPosition is null)
                listInfo.Values.NumberPosition = refListInfo.Values.NumberPosition;
        }

        /// <summary>
        /// Flattens the the specified document object.
        /// </summary>
        protected void FlattenFont(Font? font, Font? refFont)  // BUG params must be not-nullable
        {
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            if (refFont == null)
                throw new ArgumentNullException(nameof(refFont));

            font.Values.Name ??= refFont.Values.Name;
            font.Values.Size ??= refFont.Values.Size;  // TODO ??=
            if (font.Values.Color.IsValueNullOrEmpty())
                font.Values.Color = refFont.Values.Color;
            if (font.Values.Underline is null)
                font.Values.Underline = refFont.Values.Underline;

            if (font.Values.Bold is null)
                font.Values.Bold = refFont.Values.Bold;
            if (font.Values.Italic is null)
                font.Values.Italic = refFont.Values.Italic;
            if (font.Values.Superscript is null)
                font.Values.Superscript = refFont.Values.Superscript;
            if (font.Values.Subscript is null)
                font.Values.Subscript = refFont.Values.Subscript;
        }

        /// <summary>
        /// Flattens the the specified document object.
        /// </summary>
        protected void FlattenShading(Shading shading, Shading refShading)
        {
            //fClear?
            if (shading.Values.Visible is null)
                shading.Values.Visible = refShading.Values.Visible;
            if (shading.Values.Color.IsValueNullOrEmpty())
                shading.Values.Color = refShading.Values.Color;
        }

        /// <summary>
        /// Flattens the the specified document object.
        /// </summary>
        protected Border FlattenedBorderFromBorders(Border? border, Borders parentBorders)
        {
            if (border == null)
                border = new Border(parentBorders);

            if (border.Values.Visible is null)
                border.Values.Visible = parentBorders.Values.Visible;

            if (border.Values.Style is null)
                border.Values.Style = parentBorders.Values.Style;

            if (border.Values.Width.IsValueNullOrEmpty())
                border.Values.Width = parentBorders.Values.Width;

            if (border.Values.Color.IsValueNullOrEmpty())
                border.Values.Color = parentBorders.Values.Color;

            return border;
        }

        /// <summary>
        /// Flattens the the specified document object.
        /// </summary>
        protected void FlattenBorders(Borders borders, Borders refBorders)
        {
            borders.Values.Visible ??= refBorders.Values.Visible;
            if (borders.Values.Width.IsValueNullOrEmpty())
            {
                borders.Values.Width = refBorders.Values.Width;
            }
            borders.Values.Style ??= refBorders.Values.Style;
            if (borders.Values.Color.IsValueNullOrEmpty())
            {
                borders.Values.Color = refBorders.Values.Color;
            }

            borders.Values.DistanceFromBottom ??= refBorders.Values.DistanceFromBottom;
            borders.Values.DistanceFromRight ??= refBorders.Values.DistanceFromRight;
            borders.Values.DistanceFromLeft ??= refBorders.Values.DistanceFromLeft;
            borders.Values.DistanceFromTop ??= refBorders.Values.DistanceFromTop;

            if (refBorders.Values.Left != null)
            {
                FlattenBorder(borders.Left, refBorders.Values.Left);
                FlattenedBorderFromBorders(borders.Values.Left, borders);
            }

            if (refBorders.Values.Right != null)
            {
                FlattenBorder(borders.Right, refBorders.Values.Right);
                FlattenedBorderFromBorders(borders.Values.Right, borders);
            }

            if (refBorders.Values.Top != null)
            {
                FlattenBorder(borders.Top, refBorders.Values.Top);
                FlattenedBorderFromBorders(borders.Values.Top, borders);
            }

            if (refBorders.Values.Bottom != null)
            {
                FlattenBorder(borders.Bottom, refBorders.Values.Bottom);
                FlattenedBorderFromBorders(borders.Values.Bottom, borders);
            }
        }

        /// <summary>
        /// Flattens the the specified document object.
        /// </summary>
        protected void FlattenBorder(Border border, Border refBorder)
        {
            border.Values.Visible ??= refBorder.Values.Visible;

            if (border.Values.Width.IsValueNullOrEmpty())
            {
                border.Values.Width = refBorder.Values.Width;
            }

            border.Values.Style ??= refBorder.Values.Style;

            if (border.Values.Color.IsValueNullOrEmpty())
            {
                border.Values.Color = refBorder.Values.Color;
            }
        }

        /// <summary>
        /// Flattens the the specified document object.
        /// </summary>
        protected void FlattenTabStops(TabStops tabStops, TabStops refTabStops)
        {
            if (!tabStops.TabsCleared)
            {
                foreach (var refTabStop in refTabStops)
                {
                    if (tabStops.GetTabStopAt(refTabStop.Position) == null && refTabStop.AddTab)
                        tabStops.AddTabStop(refTabStop.Position, refTabStop.Alignment, refTabStop.Leader);
                }
            }

            for (int i = 0; i < tabStops.Count; i++)
            {
                TabStop tabStop = tabStops[i];
                if (!tabStop.AddTab)
                    tabStops.RemoveObjectAt(i);
            }

            // The TabStopCollection is complete now.
            // Prevent inheritance of tab stops.
            tabStops.TabsCleared = true;
        }

        /// <summary>
        /// Flattens the the specified document object.
        /// </summary>
        protected void FlattenPageSetup(PageSetup pageSetup, PageSetup refPageSetup)
        {
            if (pageSetup.Values.PageWidth.IsValueNullOrEmpty() && pageSetup.Values.PageHeight is null)
            {
                if (pageSetup.Values.PageFormat is null)
                {
                    pageSetup.Values.PageWidth = refPageSetup.Values.PageWidth;
                    pageSetup.Values.PageHeight = refPageSetup.Values.PageHeight;
                    pageSetup.Values.PageFormat = refPageSetup.Values.PageFormat;
                }
                else
                {
                    // Cannot use properties as out parameters, so use local vars.
                    PageSetup.GetPageSize(pageSetup.PageFormat, out Unit width, out Unit height);
                    pageSetup.Values.PageWidth = width;
                    pageSetup.Values.PageHeight = height;
                }
            }
            else
            {
                if (pageSetup.Values.PageWidth.IsValueNullOrEmpty())
                {
                    if (pageSetup.Values.PageFormat is null)
                        pageSetup.Values.PageHeight = refPageSetup.Values.PageHeight;
                    else
                    {
                        PageSetup.GetPageSize(pageSetup.PageFormat, out _, out Unit height);
                        pageSetup.Values.PageHeight = height;
                    }
                }
                else if (pageSetup.Values.PageHeight.IsValueNullOrEmpty())
                {
                    if (pageSetup.Values.PageFormat is null)
                        pageSetup.Values.PageWidth = refPageSetup.Values.PageWidth;
                    else
                    {
                        PageSetup.GetPageSize(pageSetup.PageFormat, out Unit width, out _);
                        pageSetup.Values.PageWidth = width;
                    }
                }
            }

            //      if (pageSetup.pageWidth.IsNull)
            //        pageSetup.pageWidth = refPageSetup.pageWidth;
            //      if (pageSetup.pageHeight.IsNull)
            //        pageSetup.pageHeight = refPageSetup.pageHeight;
            //      if (pageSetup.pageFormat.IsNull)
            //        pageSetup.pageFormat = refPageSetup.pageFormat;
            if (pageSetup.Values.SectionStart is null)
                pageSetup.Values.SectionStart = refPageSetup.Values.SectionStart;
            if (pageSetup.Values.Orientation is null)
                pageSetup.Values.Orientation = refPageSetup.Values.Orientation;
            if (pageSetup.Values.TopMargin.IsValueNullOrEmpty())
                pageSetup.Values.TopMargin = refPageSetup.Values.TopMargin;
            if (pageSetup.Values.BottomMargin.IsValueNullOrEmpty())
                pageSetup.Values.BottomMargin = refPageSetup.Values.BottomMargin;
            if (pageSetup.Values.LeftMargin.IsValueNullOrEmpty())
                pageSetup.Values.LeftMargin = refPageSetup.Values.LeftMargin;
            if (pageSetup.Values.RightMargin.IsValueNullOrEmpty())
                pageSetup.Values.RightMargin = refPageSetup.Values.RightMargin;
            if (pageSetup.Values.HeaderDistance.IsValueNullOrEmpty())
                pageSetup.Values.HeaderDistance = refPageSetup.Values.HeaderDistance;
            if (pageSetup.Values.FooterDistance.IsValueNullOrEmpty())
                pageSetup.Values.FooterDistance = refPageSetup.Values.FooterDistance;
            if (pageSetup.Values.OddAndEvenPagesHeaderFooter is null)
                pageSetup.Values.OddAndEvenPagesHeaderFooter = refPageSetup.Values.OddAndEvenPagesHeaderFooter;
            if (pageSetup.Values.DifferentFirstPageHeaderFooter is null)
                pageSetup.Values.DifferentFirstPageHeaderFooter = refPageSetup.Values.DifferentFirstPageHeaderFooter;
            if (pageSetup.Values.MirrorMargins is null)
                pageSetup.Values.MirrorMargins = refPageSetup.Values.MirrorMargins;
            if (pageSetup.Values.HorizontalPageBreak is null)
                pageSetup.Values.HorizontalPageBreak = refPageSetup.Values.HorizontalPageBreak;
        }

        /// <summary>
        /// Flattens the the specified document object.
        /// </summary>
        protected void FlattenHeaderFooter(HeaderFooter headerFooter, bool isHeader)
        { }

        /// <summary>
        /// Flattens the the specified document object.
        /// </summary>
        protected void FlattenFillFormat(FillFormat? fillFormat)
        { }

        /// <summary>
        /// Flattens the the specified document object.
        /// </summary>
        protected void FlattenLineFormat(LineFormat? lineFormat, LineFormat? refLineFormat)
        {
            if (refLineFormat != null && lineFormat != null)
            {
                if (lineFormat.Values.Width.IsValueNullOrEmpty())
                    lineFormat.Values.Width = refLineFormat.Values.Width;
            }
        }

        /// <summary>
        /// Flattens the the specified document object.
        /// </summary>
        protected void FlattenAxis(Axis? axis)
        {
            if (axis == null)
                return;

            LineFormat refLineFormat = new()
            {
                Values = { Width = 0.15 }
            };
            if (axis.Values.HasMajorGridlines == true && axis.Values.MajorGridlines is not null)
                FlattenLineFormat(axis.Values.MajorGridlines.Values.LineFormat, refLineFormat);
            if (axis.Values.HasMinorGridlines == true && axis.Values.MinorGridlines is not null)
                FlattenLineFormat(axis.Values.MinorGridlines.Values.LineFormat, refLineFormat);

            refLineFormat.Values.Width = 0.4;
            if (axis.Values.LineFormat is not null)
                FlattenLineFormat(axis.Values.LineFormat, refLineFormat);

            // axis.majorTick;
            // axis.majorTickMark;
            // axis.minorTick;
            // axis.minorTickMark;

            // axis.maximumScale;
            // axis.minimumScale;

            // axis.tickLabels;
            // axis.title;
        }

        /// <summary>
        /// Flattens the the specified document object.
        /// </summary>
        protected void FlattenPlotArea(PlotArea? plotArea)
        {
            // plotArea can be null.
        }

        /// <summary>
        /// Flattens the the specified document object.
        /// </summary>
        protected void FlattenDataLabel(DataLabel? dataLabel)
        { }

        // Chart

        internal override void VisitChart(Chart chart)
        {
            var document = chart.Document;
            if (chart.Values.Style is null)
                chart.Values.Style = Style.DefaultParagraphName;
            var style = document.Styles[chart.Values.Style];
            if (chart.Values.Format == null)
            {
                chart.Values.Format = style?.Values.ParagraphFormat?.Clone() ?? NRT.ThrowOnNull<ParagraphFormat>();
                chart.Values.Format.Parent = chart;
            }
            else
                FlattenParagraphFormat(chart.Values.Format, style?.Values.ParagraphFormat ?? NRT.ThrowOnNull<ParagraphFormat>());


            FlattenLineFormat(chart.Values.LineFormat, null);
            FlattenFillFormat(chart.Values.FillFormat);

            FlattenAxis(chart.Values.XAxis);
            FlattenAxis(chart.Values.YAxis);
            FlattenAxis(chart.Values.ZAxis);

            FlattenPlotArea(chart.Values.PlotArea);

            //      if (this .hasDataLabel.Value)
            FlattenDataLabel(chart.Values.DataLabel);
        }

        // Document

        internal override void VisitDocument(Document document)
        { }

        internal override void VisitDocumentElements(DocumentElements elements)
        { }

        // Format

        internal override void VisitStyle(Style style)
        {
            var baseStyle = style.GetBaseStyle();
            if (baseStyle?.Values.ParagraphFormat != null)
            {
                if (style.Values.ParagraphFormat == null)
                    style.Values.ParagraphFormat = baseStyle.Values.ParagraphFormat;
                else
                    FlattenParagraphFormat(style.Values.ParagraphFormat, baseStyle.Values.ParagraphFormat);
            }
        }

        internal override void VisitStyles(Styles styles)
        { }

        // Paragraph

        internal override void VisitFootnote(Footnote footnote)
        {
            var document = footnote.Document;

            ParagraphFormat? format;

            var style = document.Styles[footnote.Values.Style!]; // BUG??? "!" added.
            if (style != null)
                format = ParagraphFormatFromStyle(style);
            else
            {
                footnote.Style = StyleNames.Footnote;
                format = document.Styles[StyleNames.Footnote]!.Values.ParagraphFormat!; // BUG: Check null
            }

            if (footnote.Values.Format is null)
            {
                footnote.Values.Format = format.Clone();
                footnote.Values.Format.Parent = footnote;
            }
            else
                FlattenParagraphFormat(footnote.Values.Format, format);

        }

        internal override void VisitParagraph(Paragraph paragraph)
        {
            var document = paragraph.Document;

            ParagraphFormat format;
            var currentElementHolder = GetDocumentElementHolder(paragraph);
            var style = document.Styles[paragraph.Values.Style ?? String.Empty];
            if (style != null)
                format = ParagraphFormatFromStyle(style);

            else if (currentElementHolder is Cell cell)
            {
                paragraph.Values.Style = cell.Style;
                format = cell.Format;
            }
            else if (currentElementHolder is HeaderFooter footer)
            {
                var currHeaderFooter = footer;
                if (currHeaderFooter.IsHeader)
                {
                    paragraph.Style = StyleNames.Header;
                    format = document.Styles[StyleNames.Header]?.Values.ParagraphFormat ?? NRT.ThrowOnNull<ParagraphFormat>();
                }
                else
                {
                    paragraph.Style = StyleNames.Footer;
                    format = document.Styles[StyleNames.Footer]?.Values.ParagraphFormat ?? NRT.ThrowOnNull<ParagraphFormat>();
                }

                if (currHeaderFooter.Values.Format != null)
                    FlattenParagraphFormat(paragraph.Format, currHeaderFooter.Values.Format);
            }
            else if (currentElementHolder is Footnote)
            {
                paragraph.Style = StyleNames.Footnote;
                format = document.Styles[StyleNames.Footnote]?.Values.ParagraphFormat ?? NRT.ThrowOnNull<ParagraphFormat>();
            }
            else if (currentElementHolder is TextArea area)
            {
                paragraph.Style = area.Style; // BUG ???
                format = area.Values.Format ?? NRT.ThrowOnNull<ParagraphFormat>();
            }
            else
            {
                if (!String.IsNullOrEmpty(paragraph.Values.Style))  //StL:BUG see old code
                    paragraph.Style = StyleNames.InvalidStyleName;
                else
                    paragraph.Style = StyleNames.Normal;
                format = document.Styles[paragraph.Style]?.Values.ParagraphFormat ?? NRT.ThrowOnNull<ParagraphFormat>();
            }

            if (paragraph.Values.Format == null)
            {
                paragraph.Format = format.Clone(); //StL:BUG see old code
                paragraph.Format.Parent = paragraph;
            }
            else
                FlattenParagraphFormat(paragraph.Format, format);
        }
        // Section

        internal override void VisitHeaderFooter(HeaderFooter headerFooter)
        {
            var document = headerFooter.Document;
            string styleString;
            if (headerFooter.IsHeader)
                styleString = StyleNames.Header;
            else
                styleString = StyleNames.Footer;

            ParagraphFormat format;
            var style = document.Styles[headerFooter.Values.Style];
            if (style != null)
                format = ParagraphFormatFromStyle(style);
            else
            {
                format = document.Styles[styleString]?.Values.ParagraphFormat ?? NRT.ThrowOnNull<ParagraphFormat>();
                headerFooter.Style = styleString;
            }

            if (headerFooter.Values.Format == null)
            {
                headerFooter.Values.Format = format.Clone();
                headerFooter.Values.Format.Parent = headerFooter;
            }
            else
                FlattenParagraphFormat(headerFooter.Values.Format, format);
        }

        internal override void VisitHeadersFooters(HeadersFooters headersFooters)
        { }

        internal override void VisitSection(Section section)
        {
            var prevSec = section.PreviousSection();
            var prevPageSetup = PageSetup.DefaultPageSetup;
            if (prevSec != null)
            {
                prevPageSetup = prevSec.Values.PageSetup!;

                if (!section.Headers.HasHeaderFooter(section.Headers.Values.Primary))
                    section.Headers.Values.Primary = prevSec.Headers.Values.Primary;
                if (!section.Headers.HasHeaderFooter(section.Headers.Values.EvenPage))
                    section.Headers.Values.EvenPage = prevSec.Headers.Values.EvenPage;
                if (!section.Headers.HasHeaderFooter(section.Headers.Values.FirstPage))
                    section.Headers.Values.FirstPage = prevSec.Headers.Values.FirstPage;

                if (!section.Footers.HasHeaderFooter(section.Footers.Values.Primary))
                    section.Footers.Values.Primary = prevSec.Footers.Values.Primary;
                if (!section.Footers.HasHeaderFooter(section.Footers.Values.EvenPage))
                    section.Footers.Values.EvenPage = prevSec.Footers.Values.EvenPage;
                if (!section.Footers.HasHeaderFooter(section.Footers.Values.FirstPage))
                    section.Footers.Values.FirstPage = prevSec.Footers.Values.FirstPage;

                //if (!section.Headers.HasHeaderFooter(HeaderFooterIndex.Primary))
                //    section.Headers._primary = prevSec.Headers._primary;
                //if (!section.Headers.HasHeaderFooter(HeaderFooterIndex.EvenPage))
                //    section.Headers._evenPage = prevSec.Headers._evenPage;
                //if (!section.Headers.HasHeaderFooter(HeaderFooterIndex.FirstPage))
                //    section.Headers._firstPage = prevSec.Headers._firstPage;

                //if (!section.Footers.HasHeaderFooter(HeaderFooterIndex.Primary))
                //    section.Footers._primary = prevSec.Footers._primary;
                //if (!section.Footers.HasHeaderFooter(HeaderFooterIndex.EvenPage))
                //    section.Footers._evenPage = prevSec.Footers._evenPage;
                //if (!section.Footers.HasHeaderFooter(HeaderFooterIndex.FirstPage))
                //    section.Footers._firstPage = prevSec.Footers._firstPage;
            }

            if (section.Values.PageSetup == null)
                section.Values.PageSetup = prevPageSetup;
            else
                FlattenPageSetup(section.Values.PageSetup, prevPageSetup);
        }

        internal override void VisitSections(Sections sections)
        { }

        // Shape

        internal override void VisitTextFrame(TextFrame textFrame)
        {
            if (textFrame.Values.Height.IsValueNullOrEmpty())
            {
                textFrame.Values.Height = Unit.FromInch(1);
            }

            if (textFrame.Values.Width.IsValueNullOrEmpty())
            {
                textFrame.Values.Width = Unit.FromInch(1);
            }
        }

        // Table

        internal override void VisitCell(Cell cell)
        {
            // format, shading and borders are already processed.
        }

        internal override void VisitColumns(Columns columns)
        {
            foreach (var col in columns.Cast<Column>())
            {
                //if (col.Values.Width is null)
                if (col.Values.Width.IsValueNullOrEmpty())
                    col.Values.Width = columns.Values.Width;

                //if (col.Values.Width is null)
                if (col.Values.Width.IsValueNullOrEmpty())
                    col.Values.Width = "2.5cm";
            }
        }

        internal override void VisitRow(Row row)
        {
            foreach (var cell in row.Cells.Cast<Cell>())
            {
                if (cell.Values.VerticalAlignment is null)
                    cell.Values.VerticalAlignment = row.VerticalAlignment;
            }
        }

        internal override void VisitRows(Rows rows)
        {
            foreach (var row in rows.Cast<Row>())
            {
                //if (row.Values.Height is null)
                if (row.Values.Height.IsValueNullOrEmpty())
                    row.Values.Height = rows.Values.Height;
                if (row.Values.HeightRule is null)
                    row.Values.HeightRule = rows.Values.HeightRule;
                if (row.Values.VerticalAlignment is null)
                    row.Values.VerticalAlignment = rows.Values.VerticalAlignment;
            }
        }

        /// <summary>
        /// Returns a paragraph format object initialized by the given style.
        /// It differs from style.ParagraphFormat if style is a character style.
        /// </summary>
        ParagraphFormat ParagraphFormatFromStyle(Style style)
        {
            if (style.Type == StyleType.Character)
            {
                var doc = style.Document;
                var format = style.Values.ParagraphFormat?.Clone() ?? NRT.ThrowOnNull<ParagraphFormat>();
                FlattenParagraphFormat(format, doc.Styles.Normal.ParagraphFormat);
                return format;
            }
            else
                return style.Values.ParagraphFormat ?? NRT.ThrowOnNull<ParagraphFormat>();
        }

        internal override void VisitTable(Table table)
        {
            var document = table.Document;

            //if (table.Values.LeftPadding is null)
            if (table.Values.LeftPadding.IsValueNullOrEmpty())
                table.Values.LeftPadding = Unit.FromMillimeter(1.2);
            //if (table.Values.RightPadding is null)
            if (table.Values.RightPadding.IsValueNullOrEmpty())
                table.Values.RightPadding = Unit.FromMillimeter(1.2);

            ParagraphFormat format;
            var style = document.Styles[table.Style];
            if (style != null)
                format = ParagraphFormatFromStyle(style);
            else
            {
                table.Style = "Normal";
                format = document.Styles.Normal.ParagraphFormat;
            }

            if (table.Values.Format == null)
            {
                table.Values.Format = format.Clone();
                table.Values.Format.Parent = table;
            }
            else
                FlattenParagraphFormat(table.Values.Format, format);

            int rows = table.Rows.Count;
            int clms = table.Columns.Count;

            for (int idxclm = 0; idxclm < clms; idxclm++)
            {
                var column = table.Columns[idxclm];
                ParagraphFormat colFormat;
                style = document.Styles[column.Values.Style];
                if (style != null)
                    colFormat = ParagraphFormatFromStyle(style);
                else
                {
                    column.Values.Style = table.Values.Style;
                    colFormat = table.Format;
                }

                if (column.Values.Format == null)
                {
                    column.Values.Format = colFormat.Clone();
                    column.Values.Format.Parent = column;
                    if (column.Values.Format.Values.Shading is null && table.Values.Format.Values.Shading is not null)
                        column.Values.Format.Values.Shading = table.Values.Format.Values.Shading;
                }
                else
                    FlattenParagraphFormat(column.Values.Format, colFormat);

                //if (column.Values.LeftPadding is null)
                if (column.Values.LeftPadding.IsValueNullOrEmpty())
                    column.Values.LeftPadding = table.Values.LeftPadding;
                //if (column.Values.RightPadding is null)
                if (column.Values.RightPadding.IsValueNullOrEmpty())
                    column.Values.RightPadding = table.Values.RightPadding;

                if (column.Values.Shading == null)
                    column.Values.Shading = table.Values.Shading;

                else if (table.Values.Shading != null)
                    FlattenShading(column.Values.Shading, table.Values.Shading);

                if (column.Values.Borders == null)
                    column.Values.Borders = table.Values.Borders;
                else if (table.Values.Borders != null)
                    FlattenBorders(column.Values.Borders, table.Values.Borders);
            }

            for (int idxrow = 0; idxrow < rows; idxrow++)
            {
                var row = table.Rows[idxrow];

                ParagraphFormat rowFormat;
                style = document.Styles[row.Values.Style];
                if (style != null)
                {
                    rowFormat = ParagraphFormatFromStyle(style);
                }
                else
                {
                    row.Values.Style = table.Values.Style;
                    rowFormat = table.Format;
                }

                for (int idxclm = 0; idxclm < clms; idxclm++)
                {
                    var column = table.Columns[idxclm];
                    var cell = row[idxclm];

                    var cellStyle = document.Styles[cell.Values.Style];
                    if (cellStyle != null)
                    {
                        var cellFormat = ParagraphFormatFromStyle(cellStyle);

                        if (cell.Values.Format == null)
                            cell.Values.Format = cellFormat;
                        else
                            FlattenParagraphFormat(cell.Values.Format, cellFormat);
                    }
                    else
                    {
                        if (row.Values.Format != null)
                            FlattenParagraphFormat(cell.Format, row.Values.Format);

                        if (style != null)
                        {
                            cell.Values.Style = row.Values.Style;
                            FlattenParagraphFormat(cell.Format, rowFormat);
                        }
                        else
                        {
                            cell.Values.Style = column.Values.Style;
                            FlattenParagraphFormat(cell.Format, column.Values.Format);
                        }
                    }

                    if (cell.Values.Format != null)
                    {
                        if (cell.Values.Format.Values.Shading is null && table.Values.Format.Values.Shading is not null)
                            cell.Values.Format.Values.Shading = table.Values.Format.Values.Shading;
                    }

                    if (cell.Values.Shading == null)
                        cell.Values.Shading = row.Values.Shading;
                    else if (row.Values.Shading != null)
                        FlattenShading(cell.Values.Shading, row.Values.Shading);
                    if (cell.Values.Shading == null)
                        cell.Values.Shading = column.Values.Shading;
                    else if (column.Values.Shading != null)
                        FlattenShading(cell.Values.Shading, column.Values.Shading);
                    if (cell.Values.Borders == null)
                    {
                        //CloneHelper(ref cell.Values.Borders, row._borders); // Cannot pass property as ref
                        // CloneHelper was only used once.
                        if (row.Values.Borders != null)
                        {
                            cell.Values.Borders = row.Borders.Clone();
                            cell.Borders.Parent = row.Borders.Parent;
                        }
                    }
                    else if (row.Values.Borders != null)
                        FlattenBorders(cell.Values.Borders, row.Values.Borders);
                    if (cell.Values.Borders == null)
                        cell.Values.Borders = column.Values.Borders;
                    else if (column.Values.Borders != null)
                        FlattenBorders(cell.Values.Borders, column.Values.Borders);
                }

                if (row.Values.Format == null)
                {
                    row.Values.Format = rowFormat.Clone();
                    row.Values.Format.Parent = row;
                    if (row.Values.Format.Values.Shading is null && table.Values.Format.Values.Shading is not null)
                        row.Values.Format.Values.Shading = table.Values.Format.Values.Shading;
                }
                else
                    FlattenParagraphFormat(row.Values.Format, rowFormat);

                //if (row.Values.TopPadding is null)
                if (row.Values.TopPadding.IsValueNullOrEmpty())
                    row.Values.TopPadding = table.Values.TopPadding;
                //if (row.Values.BottomPadding is null)
                if (row.Values.BottomPadding.IsValueNullOrEmpty())
                    row.Values.BottomPadding = table.Values.BottomPadding;

                if (row.Values.Shading == null)
                    row.Values.Shading = table.Values.Shading;
                else if (table.Values.Shading != null)
                    FlattenShading(row.Values.Shading, table.Values.Shading);

                if (row.Values.Borders == null)
                    row.Values.Borders = table.Values.Borders;
                else if (table.Values.Borders != null)
                    FlattenBorders(row.Values.Borders, table.Values.Borders);
            }
        }

        internal override void VisitLegend(Legend legend)
        {
            ParagraphFormat parentFormat;
            if (legend.Values.Style is not null)
            {
                var style = legend.Document.Styles[legend.Style] ?? legend.Document.Styles[StyleNames.InvalidStyleName]!;

                parentFormat = style.Values.ParagraphFormat ?? NRT.ThrowOnNull<ParagraphFormat>();
            }
            else
            {
                var textArea = (TextArea)GetDocumentElementHolder(legend);
                legend.Values.Style = textArea.Values.Style;
                parentFormat = textArea.Values.Format ?? NRT.ThrowOnNull<ParagraphFormat>();
            }

            if (legend.Values.Format is null)
                legend.Format = parentFormat.Clone();
            else
                FlattenParagraphFormat(legend.Values.Format, parentFormat);
        }

        internal override void VisitTextArea(TextArea? textArea)
        {
            if (textArea?.Values.Elements == null)
                return;

            var document = textArea.Document;

            ParagraphFormat parentFormat;

            if (textArea.Values.Style is not null)
            {
                var style = textArea.Document.Styles[textArea.Style];
                if (style == null)
                    style = textArea.Document.Styles[StyleNames.InvalidStyleName] ?? NRT.ThrowOnNull<Style>();

                parentFormat = style.Values.ParagraphFormat ?? NRT.ThrowOnNull<ParagraphFormat>();
            }
            else
            {
                var chart = (Chart?)textArea.Parent ?? NRT.ThrowOnNull<Chart>();
                parentFormat = chart.Values.Format ?? NRT.ThrowOnNull<ParagraphFormat>();
                textArea.Values.Style = chart.Values.Style;
            }

            if (textArea.Values.Format == null)
                textArea.Format = parentFormat.Clone();
            else
                FlattenParagraphFormat(textArea.Values.Format, parentFormat);

            FlattenFillFormat(textArea.Values.FillFormat);
            FlattenLineFormat(textArea.Values.LineFormat, null);
        }

        DocumentObject GetDocumentElementHolder(DocumentObject docObj)
        {
            var docEls = (DocumentElements?)docObj.Parent ?? NRT.ThrowOnNull<DocumentElements>();
            return docEls.Parent ?? NRT.ThrowOnNull<DocumentObject>();
        }
    }
}
