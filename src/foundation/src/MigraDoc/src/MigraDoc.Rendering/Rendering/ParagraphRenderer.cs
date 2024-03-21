// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using System.Text;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering.Resources;
using PdfSharp.Pdf.Advanced;

namespace MigraDoc.Rendering
{
    struct TabOffset
    {
        internal TabOffset(TabLeader leader, XUnit offset)
        {
            Leader = leader;
            Offset = offset;
        }
        internal TabLeader Leader;
        internal XUnit Offset;
    }

    /// <summary>
    /// Summary description for ParagraphRenderer.
    /// </summary>
    class ParagraphRenderer : Renderer
    {
        /// <summary>
        /// Process phases of the renderer.
        /// </summary>
        enum Phase
        {
            Formatting,
            Rendering
        }

        /// <summary>
        /// Results that can occur when processing a paragraph element
        /// during formatting.
        /// </summary>
        enum FormatResult
        {
            /// <summary>
            /// Ignore the current element during formatting.
            /// </summary>
            Ignore,

            /// <summary>
            /// Continue with the next element within the same line.
            /// </summary>
            Continue,

            /// <summary>
            /// Start a new line from the current object on.
            /// </summary>
            NewLine,

            /// <summary>
            /// Break formatting and continue in a new area (e.g. a new page).
            /// </summary>
            NewArea
        }

        Phase _phase;

        /// <summary>
        /// Initializes a ParagraphRenderer object for formatting.
        /// </summary>
        /// <param name="gfx">The XGraphics object to do measurements on.</param>
        /// <param name="paragraph">The paragraph to format.</param>
        /// <param name="fieldInfos">The field infos.</param>
        internal ParagraphRenderer(XGraphics gfx, Paragraph paragraph, FieldInfos? fieldInfos)
            : base(gfx, paragraph, fieldInfos)
        {
            _paragraph = paragraph;

            ParagraphRenderInfo parRenderInfo = new()
            {
                DocumentObject = _paragraph
            };
            ((ParagraphFormatInfo)parRenderInfo.FormatInfo)._widowControl = _paragraph.Format.WidowControl;

            _renderInfo = parRenderInfo;
        }

        /// <summary>
        /// Initializes a ParagraphRenderer object for rendering.
        /// </summary>
        /// <param name="gfx">The XGraphics object to render on.</param>
        /// <param name="renderInfo">The render info object containing information necessary for rendering.</param>
        /// <param name="fieldInfos">The field infos.</param>
        internal ParagraphRenderer(XGraphics gfx, RenderInfo renderInfo, FieldInfos? fieldInfos)
            : base(gfx, renderInfo, fieldInfos)
        {
            _paragraph = (Paragraph)renderInfo.DocumentObject;
        }

        /// <summary>
        /// Renders the paragraph.
        /// </summary>
        internal override void Render()
        {
            InitRendering();
            if ((int)_paragraph.Format.OutlineLevel >= 1 && _gfx.PdfPage != null) // Don't call GetOutlineTitle() in vain
                DocumentRenderer.AddOutline((int)_paragraph.Format.OutlineLevel, GetOutlineTitle(), _gfx.PdfPage, GetDestinationPosition());

            RenderShading();
            RenderBorders();

            ParagraphFormatInfo parFormatInfo = (ParagraphFormatInfo)_renderInfo.FormatInfo;
            for (int idx = 0; idx < parFormatInfo.LineCount; ++idx)
            {
                LineInfo lineInfo = parFormatInfo.GetLineInfo(idx);
                _isLastLine = (idx == parFormatInfo.LineCount - 1);

                _lastTabPosition = 0;
                if (lineInfo.ReMeasureLine)
                    ReMeasureLine(ref lineInfo);

                RenderLine(lineInfo);
            }
        }

        static bool IsRenderedField(DocumentObject docObj)
        {
            if (docObj is NumericFieldBase or DocumentInfo or DateField)
                return true;

            return false;
        }

        string GetFieldValue(DocumentObject field)
        {
            if (field is NumericFieldBase numericFieldBase)
            {
                int number = -1;
                if (field is PageRefField refField)
                {
                    var pageRefField = refField;
                    number = _fieldInfos?.GetShownPageNumber(pageRefField.Name) ?? NRT.ThrowOnNull<int, FieldInfos>();
                    if (number <= 0)
                    {
                        if (_phase == Phase.Formatting)
                            return "XX";
                        return Messages2.BookmarkNotDefined(pageRefField.Name);
                    }
                }
                else if (field is SectionField)
                {
                    number = _fieldInfos?.Section ?? NRT.ThrowOnNull<int>();
                    if (number <= 0)
                        return "XX";
                }
                else if (field is PageField)
                {
                    number = _fieldInfos?.DisplayPageNr ?? NRT.ThrowOnNull<int>();
                    if (number <= 0)
                        return "XX";
                }
                else if (field is NumPagesField)
                {
                    number = _fieldInfos?.NumPages ?? NRT.ThrowOnNull<int>();
                    if (number <= 0)
                        return "XXX";
                }
                else if (field is SectionPagesField)
                {
                    number = _fieldInfos?.SectionPages ?? NRT.ThrowOnNull<int>();
                    if (number <= 0)
                        return "XX";
                }
                return NumberFormatter.Format(number, numericFieldBase.Format);
            }
            else
            {
                if (field is DateField dateField)
                {
                    var dt = _fieldInfos?.Date ?? NRT.ThrowOnNull<DateTime>();
                    if (dt == DateTime.MinValue)
                        dt = DateTime.Now;

                    return dt.ToString(GetEffectiveFormat(dateField));
                }

                if (field is InfoField infoField)
                    return GetDocumentInfo(infoField.Name);

                Debug.Assert(false, "Given parameter must be a rendered Field");
            }
            return "";
        }

        static string GetEffectiveFormat(DateField dateField)
        {
            var format = dateField.Format;
            if (!String.IsNullOrEmpty(format))
                return format;

            var culture = dateField.Document!.EffectiveCulture;
            var dtfInfo = culture.DateTimeFormat;

            return dtfInfo.ShortDatePattern + " " + dtfInfo.LongTimePattern;
        }

        string GetOutlineTitle()
        {
            var iter = new ParagraphIterator(_paragraph.Elements);
            iter = iter.GetFirstLeaf();

            var ignoreBlank = true;
            var title = "";
            while (iter != null)
            {
                var current = iter.Current;
                if (!ignoreBlank && (IsBlank(current) || IsTab(current) || IsLineBreak(current)))
                {
                    title += " ";
                    ignoreBlank = true;
                }
                else if (current is Text text && !IsSoftHyphen(text))
                {
                    title += text.Content;
                    ignoreBlank = false;
                }
                else if (IsRenderedField(current))
                {
                    title += GetFieldValue(current);
                    ignoreBlank = false;
                }
                else if (IsSymbol(current))
                {
                    title += GetSymbol((Character)current);
                    ignoreBlank = false;
                }
                iter = iter.GetNextLeaf();
            }
            return title;
        }

        /// <summary>
        /// Gets a layout info with only margin and break information set.
        /// It can be taken before the paragraph is formatted.
        /// </summary>
        /// <remarks>
        /// The following layout information is set properly:<br />
        /// MarginTop, MarginLeft, MarginRight, MarginBottom, KeepTogether, KeepWithNext, PagebreakBefore.
        /// </remarks>
        internal override LayoutInfo InitialLayoutInfo
        {
            get
            {
                LayoutInfo layoutInfo = new()
                {
                    PageBreakBefore = _paragraph.Format.PageBreakBefore,
                    MarginTop = _paragraph.Format.SpaceBefore.Point,
                    MarginBottom = _paragraph.Format.SpaceAfter.Point,
                    //Don't confuse margins with left or right indent.
                    //Indents are invisible for the layouter.
                    MarginRight = 0,
                    MarginLeft = 0,
                    KeepTogether = _paragraph.Format.KeepTogether,
                    KeepWithNext = _paragraph.Format.KeepWithNext
                };
                return layoutInfo;
            }
        }

        /// <summary>
        /// Adjusts the current x position to the given tab stop if possible.
        /// </summary>
        /// <returns>True if the text doesn't fit the line anymore and the tab causes a line break.</returns>
        FormatResult FormatTab()
        {
            // For Tabs in Justified context
            if (_paragraph.Format.Alignment == ParagraphAlignment.Justify)
                _reMeasureLine = true;
            var nextTabStop = GetNextTabStop();
            _savedWordWidth = 0;
            if (nextTabStop == null)
                return FormatResult.NewLine;

            bool notFitting = false;
            XUnit xPositionBeforeTab = _currentXPosition;
            switch (nextTabStop.Alignment)
            {
                case TabAlignment.Left:
                    _currentXPosition = ProbeAfterLeftAlignedTab(nextTabStop.Position.Point, out notFitting);
                    break;

                case TabAlignment.Right:
                    _currentXPosition = ProbeAfterRightAlignedTab(nextTabStop.Position.Point, out notFitting);
                    break;

                case TabAlignment.Center:
                    _currentXPosition = ProbeAfterCenterAlignedTab(nextTabStop.Position.Point, out notFitting);
                    break;

                case TabAlignment.Decimal:
                    _currentXPosition = ProbeAfterDecimalAlignedTab(nextTabStop.Position.Point, out notFitting);
                    break;
            }
            if (!notFitting)
            {
                // For correct right paragraph alignment with tabs
                if (!IgnoreHorizontalGrowth)
                    _currentLineWidth += _currentXPosition - xPositionBeforeTab;

                _tabOffsets.Add(new TabOffset(nextTabStop.Leader, _currentXPosition - xPositionBeforeTab));
                if (_currentLeaf != null)
                    _lastTab = _currentLeaf.Current;
            }

            return notFitting ? FormatResult.NewLine : FormatResult.Continue;
        }

        static bool IsLineBreak(DocumentObject docObj)
        {
            if (docObj is Character { SymbolName: SymbolName.LineBreak })
                return true;
            return false;
        }

        static bool IsBlankOrSoftHyphen(DocumentObject docObj)
        {
            return docObj is Text { Content: " " or "\u00AD" };
        }

        static bool IsBlank(DocumentObject docObj)
        {
            if (docObj is Text { Content: " " })
                return true;
            return false;
        }

        // TODO Make combined predicates for better performance (e.g. IsTabOrLineBreak).
        static bool IsTab(DocumentObject docObj)
        {
            if (docObj is Character { SymbolName: SymbolName.Tab })
                return true;
            return false;
        }

        static bool IsSoftHyphen(DocumentObject docObj)
        {
            if (docObj is Text text)
                return text.Content == "\u00AD";

            return false;
        }

        /// <summary>
        /// Probes the paragraph elements after a left aligned tab stop and returns the vertical text position to start at.
        /// </summary>
        /// <param name="tabStopPosition">Position of the tab to probe.</param>
        /// <param name="notFitting">Out parameter determining whether the tab causes a line break.</param>
        /// <returns>The new x-position to restart behind the tab.</returns>
        XUnit ProbeAfterLeftAlignedTab(XUnit tabStopPosition, out bool notFitting)
        {
            //--- Save ---------------------------------
            SaveBeforeProbing(out var iter, out var blankCount, out var wordsWidth, out var xPosition, out var lineWidth, out var blankWidth);
            //------------------------------------------

            XUnit xPositionAfterTab = xPosition;
            _currentXPosition = _formattingArea.X + tabStopPosition.Point;

            notFitting = ProbeAfterTab();
            if (!notFitting)
                xPositionAfterTab = _formattingArea.X + tabStopPosition;

            //--- Restore ---------------------------------
            RestoreAfterProbing(iter, blankCount, wordsWidth, xPosition, lineWidth, blankWidth);
            //------------------------------------------
            return xPositionAfterTab;
        }

        /// <summary>
        /// Probes the paragraph elements after a right aligned tab stop and returns the vertical text position to start at.
        /// </summary>
        /// <param name="tabStopPosition">Position of the tab to probe.</param>
        /// <param name="notFitting">Out parameter determining whether the tab causes a line break.</param>
        /// <returns>The new x-position to restart behind the tab.</returns>
        XUnit ProbeAfterRightAlignedTab(XUnit tabStopPosition, out bool notFitting)
        {
            //--- Save ---------------------------------
            SaveBeforeProbing(out var iter, out var blankCount, out var wordsWidth, out var xPosition, out var lineWidth, out var blankWidth);
            //------------------------------------------

            XUnit xPositionAfterTab = xPosition;

            notFitting = ProbeAfterTab();
            if (!notFitting && xPosition + _currentLineWidth <= _formattingArea.X + tabStopPosition)
                xPositionAfterTab = _formattingArea.X + tabStopPosition - _currentLineWidth;

            //--- Restore ------------------------------
            RestoreAfterProbing(iter, blankCount, wordsWidth, xPosition, lineWidth, blankWidth);
            //------------------------------------------
            return xPositionAfterTab;
        }

        Hyperlink? GetHyperlink()
        {
            if (_currentLeaf is not null)
            {
                var elements = DocumentRelations.GetParent(_currentLeaf.Current);
                var parent = DocumentRelations.GetParent(elements);
                while (parent is not Paragraph)
                {
                    if (parent is Hyperlink hyperlink)
                        return hyperlink;
                    elements = DocumentRelations.GetParent(parent);
                    parent = DocumentRelations.GetParent(elements);
                }
                return null;
            }
            NRT.ThrowOnNull();
            return null;
        }

        /// <summary>
        /// Probes the paragraph elements after a right aligned tab stop and returns the vertical text position to start at.
        /// </summary>
        /// <param name="tabStopPosition">Position of the tab to probe.</param>
        /// <param name="notFitting">Out parameter determining whether the tab causes a line break.</param>
        /// <returns>The new x-position to restart behind the tab.</returns>
        XUnit ProbeAfterCenterAlignedTab(XUnit tabStopPosition, out bool notFitting)
        {
            //--- Save ---------------------------------
            SaveBeforeProbing(out var iter, out var blankCount, out var wordsWidth, out var xPosition, out var lineWidth, out var blankWidth);
            //------------------------------------------

            XUnit xPositionAfterTab = xPosition;
            notFitting = ProbeAfterTab();

            if (!notFitting)
            {
                if (xPosition + _currentLineWidth / 2.0 <= _formattingArea.X + tabStopPosition)
                {
                    var rect = _formattingArea.GetFittingRect(_currentYPosition, _currentVerticalInfo.Height);
                    if (rect is null)
                        NRT.ThrowOnNull();
                    if (_formattingArea.X + tabStopPosition + _currentLineWidth / 2.0 > rect.X + rect.Width - RightIndent)
                    {
                        //the text is too long on the right hand side of the tab stop => align to right indent.
                        xPositionAfterTab = rect.X +
                          rect.Width -
                          RightIndent -
                          _currentLineWidth;
                    }
                    else
                        xPositionAfterTab = _formattingArea.X + tabStopPosition - _currentLineWidth / 2;
                }
            }

            //--- Restore ------------------------------
            RestoreAfterProbing(iter, blankCount, wordsWidth, xPosition, lineWidth, blankWidth);
            //------------------------------------------
            return xPositionAfterTab;
        }

        /// <summary>
        /// Probes the paragraph elements after a right aligned tab stop and returns the vertical text position to start at.
        /// </summary>
        /// <param name="tabStopPosition">Position of the tab to probe.</param>
        /// <param name="notFitting">Out parameter determining whether the tab causes a line break.</param>
        /// <returns>The new x-position to restart behind the tab.</returns>
        XUnit ProbeAfterDecimalAlignedTab(XUnit tabStopPosition, out bool notFitting)
        {
            notFitting = false;

            //--- Save ---------------------------------
            SaveBeforeProbing(out var iter, out var blankCount, out var wordsWidth, out var xPosition, out var lineWidth, out var blankWidth);
            //------------------------------------------

            try
            {
                // Extra for auto tab after list symbol.
                if (IsTab(_currentLeaf?.Current ?? NRT.ThrowOnNull<DocumentObject>()))
                    _currentLeaf = _currentLeaf.GetNextLeaf();
                if (_currentLeaf == null)
                {
                    return _currentXPosition + tabStopPosition;
                }
                var newVerticalInfo = CalcCurrentVerticalInfo();
                var fittingRect = _formattingArea.GetFittingRect(_currentYPosition, newVerticalInfo.Height);
                if (fittingRect == null)
                {
                    notFitting = true;
                    return _currentXPosition;
                }

                if (IsPlainText(_currentLeaf.Current))
                {
                    var text = (Text)_currentLeaf.Current;
                    string word = text.Content;

                    var culture = text.Document!.EffectiveCulture;

                    var decimalSeparator = culture.NumberFormat.NumberDecimalSeparator;
                    var decimalSeparatorLength = decimalSeparator.Length;
                    var groupSeparator = culture.NumberFormat.NumberGroupSeparator;
                    var groupSeparatorLength = groupSeparator.Length;

                    // Get the index of the decimal position the word should be aligned at. 
                    var decimalPosIndex = -1;
                    var hasNumber = false;
                    for (var i = 0; i < word.Length;)
                    {
                        var c = word[i];

                        // Number is always accepted before decimal position.
                        if (char.IsNumber(c))
                        {
                            hasNumber = true;
                            i++;
                            continue;
                        }

                        var restLength = word.Length - i;

                        // Decimal Separator always determines decimal position.
                        if (decimalSeparatorLength <= restLength && word.Substring(i, decimalSeparatorLength) == decimalSeparator)
                        {
                            decimalPosIndex = i;
                            break;
                        }

                        // Group Separator is always accepted before decimal position.
                        if (groupSeparatorLength <= restLength && word.Substring(i, groupSeparatorLength) == groupSeparator)
                        {
                            i += groupSeparator.Length;
                            continue;
                        }

                        // Other characters determine decimal position, if word contains numbers by now.
                        if (hasNumber)
                        {
                            decimalPosIndex = i;
                            break;
                        }

                        // Otherwise other characters are accepted before decimal position.
                        i++;
                    }

                    if (decimalPosIndex >= 0)
#if NET6_0_OR_GREATER || USE_INDEX_AND_RANGE
                        word = word[..decimalPosIndex];
#else
                        word = word.Substring(0, decimalPosIndex);
#endif

                    XUnit wordLength = MeasureString(word);
                    notFitting = _currentXPosition + wordLength >= _formattingArea.X + _formattingArea.Width + Tolerance;
                    if (!notFitting)
                        return _formattingArea.X + tabStopPosition - wordLength;

                    return _currentXPosition;
                }
            }
            finally
            {
                //--- Restore ------------------------------
                RestoreAfterProbing(iter, blankCount, wordsWidth, xPosition, lineWidth, blankWidth);
                //------------------------------------------
            }

            return ProbeAfterRightAlignedTab(tabStopPosition, out notFitting);
        }

        void SaveBeforeProbing(out ParagraphIterator? paragraphIter, out int blankCount, out XUnit wordsWidth, out XUnit xPosition, out XUnit lineWidth, out XUnit blankWidth)
        {
            paragraphIter = _currentLeaf;
            blankCount = _currentBlankCount;
            xPosition = _currentXPosition;
            lineWidth = _currentLineWidth;
            wordsWidth = _currentWordsWidth;
            blankWidth = _savedBlankWidth;
        }

        void RestoreAfterProbing(ParagraphIterator? paragraphIter, int blankCount, XUnit wordsWidth, XUnit xPosition, XUnit lineWidth, XUnit blankWidth)
        {
            _currentLeaf = paragraphIter;
            _currentBlankCount = blankCount;
            _currentXPosition = xPosition;
            _currentLineWidth = lineWidth;
            _currentWordsWidth = wordsWidth;
            _savedBlankWidth = blankWidth;
        }

        /// <summary>
        /// Probes the paragraph after a tab.
        /// Caution: This Function resets the word count and line width before doing its work.
        /// </summary>
        /// <returns>True if the tab causes a linebreak.</returns>
        bool ProbeAfterTab()
        {
            _currentLineWidth = 0;
            _currentBlankCount = 0;
            //Extra for auto tab after list symbol.

            //TODO: KLPO4KLPO: Check if this conditional statement is still required.
            if (_currentLeaf != null && IsTab(_currentLeaf.Current))
                _currentLeaf = _currentLeaf.GetNextLeaf();

            bool wordAppeared = false;
            while (_currentLeaf != null && !IsLineBreak(_currentLeaf.Current) && !IsTab(_currentLeaf.Current))
            {
                FormatResult result = FormatElement(_currentLeaf.Current);
                if (result != FormatResult.Continue)
                    break;

                wordAppeared = wordAppeared || IsWordLikeElement(_currentLeaf.Current);
                _currentLeaf = _currentLeaf.GetNextLeaf();
            }
            return _currentLeaf != null && !IsLineBreak(_currentLeaf.Current) &&
              !IsTab(_currentLeaf.Current) && !wordAppeared;
        }

        /// <summary>
        /// Gets the next tab stop following the current x position.
        /// </summary>
        /// <returns>The searched tab stop.</returns>
        TabStop? GetNextTabStop()
        {
            var format = _paragraph.Format;
            var tabStops = format.TabStops;
            XUnit lastPosition = 0;

            foreach (var tabStop in tabStops.Cast<TabStop>())
            {
                if (tabStop is null)
                    NRT.ThrowOnNull();

                if (tabStop.Position.Point > _formattingArea.Width - RightIndent + Tolerance)
                    break;

                // Compare with "Tolerance" to prevent rounding errors from taking us one tab stop too far.
                if (tabStop.Position.Point + _formattingArea.X > _currentXPosition + Tolerance)
                    return tabStop;

                lastPosition = tabStop.Position.Point;
            }
            //Automatic tab stop: FirstLineIndent < 0 => automatic tab stop at LeftIndent.

            if (format.FirstLineIndent < 0 ||
                (format.Values.ListInfo is not null && !format.Values.ListInfo.IsNull() && format.ListInfo.NumberPosition < format.LeftIndent))
            {
                XUnit leftIndent = format.LeftIndent.Point;
                if (_isFirstLine && _currentXPosition < leftIndent + _formattingArea.X)
                    return new TabStop(leftIndent.Point);
            }
            XUnit defaultTabStop = "1.25cm";
            //if (!_paragraph.Document._defaultTabStop.IsNull)
            //if (_paragraph.Document.Values.DefaultTabStop is not null)
            Debug.Assert(_paragraph.Document != null, "_paragraph.Document != null");
            if (!_paragraph.Document.Values.DefaultTabStop.IsValueNullOrEmpty())
                defaultTabStop = _paragraph.Document.DefaultTabStop.Point;

            var currTabPos = defaultTabStop;
            while (currTabPos + _formattingArea.X <= _formattingArea.Width - RightIndent)
            {
                if (currTabPos > lastPosition && currTabPos + _formattingArea.X > _currentXPosition + Tolerance)
                    return new TabStop(currTabPos.Point);

                currTabPos += defaultTabStop;
            }
            return null;
        }

        /// <summary>
        /// Gets the horizontal position to start a new line.
        /// </summary>
        /// <returns>The position to start the line.</returns>
        XUnit StartXPosition
        {
            get
            {
                XUnit xPos = 0;

                if (_phase == Phase.Formatting)
                {
                    xPos = _formattingArea.GetFittingRect(_currentYPosition, _currentVerticalInfo.Height)?.X ?? NRT.ThrowOnNull<XUnit>();
                    xPos += LeftIndent;
                }
                else //if (phase == Phase.Rendering)
                {
                    Area contentArea = _renderInfo.LayoutInfo.ContentArea;
                    //next lines for non-fitting lines that produce an empty fitting rect:
                    XUnit rectX = contentArea.X;
                    XUnit rectWidth = contentArea.Width;

                    var fittingRect = contentArea.GetFittingRect(_currentYPosition, _currentVerticalInfo.Height);
                    if (fittingRect != null)
                    {
                        rectX = fittingRect.X;
                        rectWidth = fittingRect.Width;
                    }
                    switch (_paragraph.Format.Alignment)
                    {
                        case ParagraphAlignment.Left:
                        case ParagraphAlignment.Justify:
                            xPos = rectX;
                            xPos += LeftIndent;
                            break;

                        case ParagraphAlignment.Right:
                            xPos = rectX + rectWidth - RightIndent;
                            xPos -= _currentLineWidth;
                            break;

                        case ParagraphAlignment.Center:
                            xPos = rectX + (rectWidth + LeftIndent - RightIndent - _currentLineWidth) / 2.0;
                            break;
                    }
                }
                return xPos;
            }
        }

        /// <summary>
        /// Renders a single line.
        /// </summary>
        /// <param name="lineInfo"></param>
        void RenderLine(LineInfo lineInfo)
        {
            _currentVerticalInfo = lineInfo.Vertical;
            _currentLeaf = lineInfo.StartIter;
            _startLeaf = lineInfo.StartIter;
            _endLeaf = lineInfo.EndIter;
            _currentBlankCount = lineInfo.BlankCount;
            _currentLineWidth = lineInfo.LineWidth;
            _currentWordsWidth = lineInfo.WordsWidth;
            _currentXPosition = StartXPosition;
            _tabOffsets = lineInfo.TabOffsets;
            _lastTabPassed = lineInfo.LastTab == null;
            _lastTab = lineInfo.LastTab;

            _tabIdx = 0;

            bool ready = _currentLeaf == null;
            if (_isFirstLine)
                RenderListSymbol();

            while (!ready)
            {
                if (_currentLeaf is null)
                    NRT.ThrowOnNull();

                if (_currentLeaf.Current == (lineInfo.EndIter?.Current ?? NRT.ThrowOnNull<DocumentObject>()))
                    ready = true;

                if (_currentLeaf.Current == lineInfo.LastTab)
                    _lastTabPassed = true;

                // If a DocumentObjectCollection is returned as a leaf, it is obviously empty.
                // There is no need to render an empty DocumentObjectCollection. Also, RenderElement() will throw an exception, because it doesn't contain implementations for DocumentObjectCollections. So we better won't call it for them.
                if (_currentLeaf.Current is not DocumentObjectCollection)
                    RenderElement(_currentLeaf.Current);

                _currentLeaf = _currentLeaf.GetNextLeaf();
            }
            _currentYPosition += lineInfo.Vertical.Height;
            _isFirstLine = false;
        }

        void ReMeasureLine(ref LineInfo lineInfo)
        {
            //--- Save ---------------------------------
            SaveBeforeProbing(out var iter, out var blankCount, out var wordsWidth, out var xPosition, out var lineWidth, out var blankWidth);
            bool origLastTabPassed = _lastTabPassed;
            //------------------------------------------
            _currentLeaf = lineInfo.StartIter;
            _startLeaf = lineInfo.StartIter;
            _endLeaf = lineInfo.EndIter;
            _formattingArea = _renderInfo.LayoutInfo.ContentArea;
            _tabOffsets = [];
            _currentLineWidth = 0;
            _currentWordsWidth = 0;

            var fittingRect = _formattingArea.GetFittingRect(_currentYPosition, _currentVerticalInfo.Height);
            if (fittingRect != null)
            {
                _currentXPosition = fittingRect.X + LeftIndent;
                FormatListSymbol();
                bool goOn = true;
                while (goOn && _currentLeaf != null)
                {
                    if (_currentLeaf.Current == lineInfo.LastTab)
                        _lastTabPassed = true;

                    FormatElement(_currentLeaf.Current);

                    if (_endLeaf is null)
                        NRT.ThrowOnNull();
                    goOn = _currentLeaf != null && _currentLeaf.Current != _endLeaf.Current;
                    if (goOn)
                        _currentLeaf = _currentLeaf!.GetNextLeaf();
                }
                lineInfo.LineWidth = _currentLineWidth;
                lineInfo.WordsWidth = _currentWordsWidth;
                lineInfo.BlankCount = _currentBlankCount;
                lineInfo.TabOffsets = _tabOffsets;
                lineInfo.ReMeasureLine = false;
                _lastTabPassed = origLastTabPassed;
            }
            RestoreAfterProbing(iter, blankCount, wordsWidth, xPosition, lineWidth, blankWidth);
        }

        XUnit CurrentWordDistance
        {
            get
            {
                if (_phase == Phase.Rendering &&
                  _paragraph.Format.Alignment == ParagraphAlignment.Justify && _lastTabPassed)
                {
                    if (_currentBlankCount >= 1 && !(_isLastLine && _renderInfo.FormatInfo.IsEnding))
                    {
                        var contentArea = _renderInfo.LayoutInfo.ContentArea;
                        XUnit width = contentArea.GetFittingRect(_currentYPosition, _currentVerticalInfo.Height)?.Width ?? NRT.ThrowOnNull<XUnit>();
                        if (_lastTabPosition > 0)
                        {
                            width -= (_lastTabPosition -
                            contentArea.X);
                        }
                        else
                            width -= LeftIndent;

                        width -= RightIndent;
                        return (width - _currentWordsWidth) / (_currentBlankCount);
                    }
                }
                return MeasureString(" ");
            }
        }

        void RenderElement(DocumentObject docObj)
        {
            switch (docObj)
            {
                case Text obj:
                    if (!IsBlankOrSoftHyphen(obj))
                    {
                        RenderText(obj);
                        break;
                    }
                    if (IsSoftHyphen(obj))
                        RenderSoftHyphen();
                    else
                        RenderBlank();
                    //if (IsBlank(docObj))
                    //    RenderBlank();
                    //else if (IsSoftHyphen(docObj))
                    //    RenderSoftHyphen();
                    //else
                    //    RenderText((Text)docObj);
                    break;

                case Character obj:
                    RenderCharacter(obj);
                    break;

                case DateField obj:
                    RenderDateField(obj);
                    break;

                case InfoField obj:
                    RenderInfoField(obj);
                    break;

                case NumPagesField obj:
                    RenderNumPagesField(obj);
                    break;

                case PageField obj:
                    RenderPageField(obj);
                    break;

                case SectionField obj:
                    RenderSectionField(obj);
                    break;

                case SectionPagesField obj:
                    RenderSectionPagesField(obj);
                    break;

                case BookmarkField obj:
                    RenderBookmarkField(obj);
                    break;

                case PageRefField obj:
                    RenderPageRefField(obj);
                    break;

                case Image obj:
                    RenderImage(obj);
                    break;

                case Footnote:
                    // TODO Not yet implemented.
                    break;

                default:
                    throw new NotImplementedException(docObj.GetType().Name + " is not implemented.");
            }
        }

        // ReSharper disable once UnusedParameter.Local
        void RenderImage(Image _)
        {
            var renderInfo = CurrentImageRenderInfo;
            XUnit top = CurrentBaselinePosition;
            var contentArea = renderInfo?.LayoutInfo.ContentArea ?? NRT.ThrowOnNull<Area>();
            top -= contentArea.Height;
            RenderByInfos(_currentXPosition, top, [renderInfo]);

            RenderUnderline(contentArea.Width, true);
            RealizeHyperlink(contentArea.Width);

            _currentXPosition += contentArea.Width;
        }

        void RenderDateField(DateField dateField)
        {
            if (_fieldInfos is null)
                NRT.ThrowOnNull();
            RenderWord(_fieldInfos.Date.ToString(GetEffectiveFormat(dateField)));
        }

        void RenderInfoField(InfoField infoField)
        {
            RenderWord(GetDocumentInfo(infoField.Name));
        }

        void RenderNumPagesField(NumPagesField numPagesField)
        {
            RenderWord(GetFieldValue(numPagesField));
        }

        void RenderPageField(PageField pageField)
        {
            RenderWord(GetFieldValue(pageField));
        }

        void RenderSectionField(SectionField sectionField)
        {
            RenderWord(GetFieldValue(sectionField));
        }

        void RenderSectionPagesField(SectionPagesField sectionPagesField)
        {
            RenderWord(GetFieldValue(sectionPagesField));
        }

        void RenderBookmarkField(BookmarkField bookmarkField)
        {
            // Add also a named destination, if a PdfDocument is rendered.
            //var pdfDocument = _gfx?.PdfPage?.Owner;
            var pdfDocument = _gfx.PdfPage?.Owner;
            if (pdfDocument != null)
            {
                var pageNr = pdfDocument.PageCount; // Magic: Pages are added while rendering, so the current page number equals pdfDocument.PageCount.
                Debug.Assert(pageNr >= 1);

                var destinationName = bookmarkField.Name;
                var position = GetDestinationPosition();
                pdfDocument.AddNamedDestination(destinationName, pageNr, PdfNamedDestinationParameters.CreatePosition(position));
            }

            RenderUnderline(0, false);
        }

        /// <summary>
        /// Gets the current position for destinations in PDF world space units.
        /// The position is moved by a margin value to leave space between the window and the content that is located at the destination
        /// and it is transformed to PDF world space units.
        /// </summary>
        XPoint GetDestinationPosition()
        {
            //var margin = XUnit.FromCentimeter(0.5);
            var x = _currentXPosition > _margin ? _currentXPosition - _margin : 0;
            var y = _currentYPosition > _margin ? _currentYPosition - _margin : 0;
            var destinationPosition = new XPoint(x, y);

            var pdfPosition = _gfx.Transformer.WorldToDefaultPage(destinationPosition);
            return pdfPosition;
        }
        // ReSharper disable once InconsistentNaming
        static readonly XUnit _margin = XUnit.FromCentimeter(0.5);

        void RenderPageRefField(PageRefField pageRefField)
        {
            RenderWord(GetFieldValue(pageRefField));
        }

        void RenderCharacter(Character character)
        {
            switch (character.SymbolName)
            {
                case SymbolName.Blank:
                case SymbolName.Em:
                case SymbolName.Em4:
                case SymbolName.En:
                    RenderSpace(character);
                    break;
                case SymbolName.LineBreak:
                    RenderLinebreak();
                    break;

                case SymbolName.Tab:
                    RenderTab();
                    break;

                default:
                    RenderSymbol(character);
                    break;
            }
        }

        void RenderSpace(Character character)
        {
            XUnit width = GetSpaceWidth(character);
            RenderUnderline(width, false);
            RealizeHyperlink(width);
            _currentXPosition += width;
        }

        void RenderLinebreak()
        {
            RenderUnderline(0, false);
            RealizeHyperlink(0);
        }

        void RenderSymbol(Character character)
        {
            string sym = GetSymbol(character);
            string completeWord = sym;
            for (int idx = 1; idx < character.Count; ++idx)
                completeWord += sym;

            RenderWord(completeWord);
        }

        void RenderTab()
        {
            var tabOffset = NextTabOffset();
            RenderUnderline(tabOffset.Offset, false);
            RenderTabLeader(tabOffset);
            RealizeHyperlink(tabOffset.Offset);
            _currentXPosition += tabOffset.Offset;
            if (_currentLeaf is null)
                NRT.ThrowOnNull();
            if (_currentLeaf.Current == _lastTab)
                _lastTabPosition = _currentXPosition;
        }

        void RenderTabLeader(TabOffset tabOffset)
        {
            string leaderString;
            switch (tabOffset.Leader)
            {
                case TabLeader.Dashes:
                    leaderString = "-";
                    break;

                case TabLeader.Dots:
                    leaderString = ".";
                    break;

                case TabLeader.Heavy:
                case TabLeader.Lines:
                    leaderString = "_";
                    break;

                case TabLeader.MiddleDot:
                    leaderString = "·";
                    break;

                default:
                    return;
            }
            XUnit leaderWidth = MeasureString(leaderString);
            XUnit xPosition = _currentXPosition;
            string drawString = "";

            while (xPosition + leaderWidth <= _currentXPosition + tabOffset.Offset)
            {
                drawString += leaderString;
                xPosition += leaderWidth;
            }
            Font font = CurrentDomFont;
            XFont xFont = CurrentFont;
            if (font.Subscript || font.Superscript)
                xFont = FontHandler.ToSubSuperFont(xFont);

            if (CurrentBrush != null)
                _gfx.DrawString(drawString, xFont, CurrentBrush, _currentXPosition, CurrentBaselinePosition);
        }

        TabOffset NextTabOffset()
        {
            TabOffset offset = _tabOffsets.Count > _tabIdx ? _tabOffsets[_tabIdx] :
              new TabOffset(0, 0);

            ++_tabIdx;
            return offset;
        }

        int _tabIdx;

        bool IgnoreBlank()
        {
            if (_currentLeaf == _startLeaf)
                return true;

            if (/*_endLeaf is null ||*/ _currentLeaf is null)
                NRT.ThrowOnNull();

            if (_endLeaf != null && _currentLeaf.Current == _endLeaf.Current)
                return true;

            var nextIter = _currentLeaf.GetNextLeaf();
            while (nextIter != null && (IsBlank(nextIter.Current) || nextIter.Current is BookmarkField))
            {
                nextIter = nextIter.GetNextLeaf();
            }
            if (nextIter == null)
                return true;

            if (IsTab(nextIter.Current))
                return true;

            var prevIter = _currentLeaf.GetPreviousLeaf();
            // Can be null if currentLeaf is the first leaf.
            var obj = prevIter?.Current;
            while (obj is BookmarkField)
            {
                prevIter = prevIter?.GetPreviousLeaf();
                obj = prevIter?.Current;
            }
            if (obj == null)
                return true;

            return IsBlank(obj) || IsTab(obj);
        }

        void RenderBlank()
        {
            if (!IgnoreBlank())
            {
                XUnit wordDistance = CurrentWordDistance;
                RenderUnderline(wordDistance, false);
                RealizeHyperlink(wordDistance);
                _currentXPosition += wordDistance;
            }
            else
            {
                RenderUnderline(0, false);
                RealizeHyperlink(0);
            }
        }

        void RenderSoftHyphen()
        {
            if (_currentLeaf is null || _endLeaf is null)
                NRT.ThrowOnNull();

            if (_currentLeaf.Current == _endLeaf.Current)
                RenderWord("-");
        }

        void RenderText(Text text)
        {
            RenderWord(text.Content);
        }

        void RenderWord(string word)
        {
            Font font = CurrentDomFont;
            XFont xFont = CurrentFont;
            if (font.Subscript || font.Superscript)
                xFont = FontHandler.ToSubSuperFont(xFont);

            if (CurrentBrush != null)
                _gfx.DrawString(word, xFont, CurrentBrush, _currentXPosition, CurrentBaselinePosition);
            XUnit wordWidth = MeasureString(word);
            RenderUnderline(wordWidth, true);
            RealizeHyperlink(wordWidth);
            _currentXPosition += wordWidth;
        }

        void StartHyperlink(XUnit left, XUnit top)
        {
            _hyperlinkRect = new XRect(left, top, 0, 0);
        }

        void EndHyperlink(Hyperlink hyperlink, XUnit right, XUnit bottom)
        {
            _hyperlinkRect.Width = right - _hyperlinkRect.X;
            _hyperlinkRect.Height = bottom - _hyperlinkRect.Y;
            var page = _gfx.PdfPage;
            if (page != null)
            {
                XRect rect = _gfx.Transformer.WorldToDefaultPage(_hyperlinkRect);

                switch (hyperlink.Type)
                {
                    case HyperlinkType.Local:

                        // Try to use named destination, if a document is rendered.
                        var pdfDocument = _gfx.PdfPage?.Owner;
                        if (pdfDocument != null)
                        {
                            page.AddDocumentLink(new PdfRectangle(rect), hyperlink.BookmarkName);
                        }
                        // Otherwise use page from bookmark's fieldInfo.
                        else
                        {
                            var pageRef = _fieldInfos?.GetPhysicalPageNumber(hyperlink.BookmarkName) ?? NRT.ThrowOnNull<int>();
                            if (pageRef > 0)
                                page.AddDocumentLink(new PdfRectangle(rect), pageRef);
                        }
                        break;

                    case HyperlinkType.ExternalBookmark:
                        page.AddDocumentLink(new PdfRectangle(rect), hyperlink.Filename, hyperlink.BookmarkName, ConvertHyperlinkTargetWindow(hyperlink.NewWindow));
                        break;

                    case HyperlinkType.EmbeddedDocument:
                        page.AddEmbeddedDocumentLink(new PdfRectangle(rect), hyperlink.Filename, hyperlink.BookmarkName, ConvertHyperlinkTargetWindow(hyperlink.NewWindow));
                        break;

                    case HyperlinkType.Web:
                        page.AddWebLink(new PdfRectangle(rect), hyperlink.Filename);
                        break;

                    case HyperlinkType.File:
                        page.AddFileLink(new PdfRectangle(rect), hyperlink.Filename);
                        break;
                }
                _hyperlinkRect = new XRect();
            }
        }

        static bool? ConvertHyperlinkTargetWindow(HyperlinkTargetWindow hyperlinkTargetWindow)
        {
            return hyperlinkTargetWindow switch
            {
                HyperlinkTargetWindow.NewWindow => true,
                HyperlinkTargetWindow.SameWindow => false,
                HyperlinkTargetWindow.UserPreference => null,
                _ => null
            };
        }

        void RealizeHyperlink(XUnit width)
        {
            XUnit top = _currentYPosition;
            XUnit left = _currentXPosition;
            XUnit bottom = top + _currentVerticalInfo.Height;
            XUnit right = left + width;
            var hyperlink = GetHyperlink();

            bool hyperlinkChanged = _currentHyperlink != hyperlink;

            if (hyperlinkChanged)
            {
                if (_currentHyperlink != null)
                    EndHyperlink(_currentHyperlink, left, bottom);

                if (hyperlink != null)
                    StartHyperlink(left, top);

                _currentHyperlink = hyperlink;
            }

            if (_currentLeaf is null || _endLeaf is null)
                NRT.ThrowOnNull();

            if (_currentLeaf.Current == _endLeaf.Current)
            {
                if (_currentHyperlink != null)
                    EndHyperlink(_currentHyperlink, right, bottom);

                _currentHyperlink = null;
            }
        }
        Hyperlink? _currentHyperlink;
        XRect _hyperlinkRect;

        XUnit CurrentBaselinePosition
        {
            get
            {
                VerticalLineInfo verticalInfo = _currentVerticalInfo;
                XUnit position = _currentYPosition;

                Font font = CurrentDomFont;
                XFont xFont = CurrentFont;

                // $MaOs BUG: For LineSpacingRule.AtLeast the text should be positioned at the line bottom. Maybe verticalInfo.InherentlineSpace does not contain the lineSpacing value in this case.
                double setLineSpace = verticalInfo.InherentLineSpace;
                double standardFontLineSpace = xFont.GetHeight();

                // Set position to bottom of text.
                position += setLineSpace;
                if (font.Subscript)
                {
                    // Move sub-/superscaled descender up.
                    position -= FontHandler.GetSubSuperScaling(xFont) * FontHandler.GetDescent(xFont);
                }
                else if (font.Superscript)
                {
                    // Set position to top of text.
                    position -= standardFontLineSpace;
                    // Move sub-/superscaled LineSpace down and descender up.
                    position += FontHandler.GetSubSuperScaling(xFont) * (standardFontLineSpace - FontHandler.GetDescent(xFont));
                }
                else
                    // Move descender up.
                    position -= verticalInfo.Descent;

                return position;
            }
        }

        XBrush? CurrentBrush
        {
            get
            {
                if (_currentLeaf != null)
                    return FontHandler.FontColorToXBrush(CurrentDomFont);

                return null;
            }
        }

        void InitRendering()
        {
            _phase = Phase.Rendering;

            var parFormatInfo = (ParagraphFormatInfo)_renderInfo.FormatInfo;
            if (parFormatInfo.LineCount == 0)
                return;
            _isFirstLine = parFormatInfo.IsStarting;

            LineInfo lineInfo = parFormatInfo.GetFirstLineInfo();
            Area contentArea = _renderInfo.LayoutInfo.ContentArea;
            _currentYPosition = contentArea.Y + TopBorderOffset;
            // StL: GetFittingRect liefert manchmal null
            var rect = contentArea.GetFittingRect(_currentYPosition, lineInfo.Vertical.Height);
            if (rect != null)
                _currentXPosition = rect.X;
            _currentLineWidth = 0;
        }

        /// <summary>
        /// Initializes this instance for formatting.
        /// </summary>
        /// <param name="area">The area for formatting.</param>
        /// <param name="previousFormatInfo">A previous format info.</param>
        /// <returns>False if nothing of the paragraph will fit the area anymore.</returns>
        bool InitFormat(Area area, FormatInfo? previousFormatInfo)
        {
            _phase = Phase.Formatting;

            _tabOffsets = [];

            var prevParaFormatInfo = (ParagraphFormatInfo?)previousFormatInfo;
            if (prevParaFormatInfo == null || prevParaFormatInfo.LineCount == 0)
            {
                ((ParagraphFormatInfo)_renderInfo.FormatInfo)._isStarting = true;
                var parIt = new ParagraphIterator(_paragraph.Elements);
                _currentLeaf = parIt.GetFirstLeaf();
                _isFirstLine = true;
            }
            else
            {
                _currentLeaf = prevParaFormatInfo.GetLastLineInfo().EndIter?.GetNextLeaf(); // Do not check here. ?? NRT.ThrowOnNull<ParagraphIterator>();
                _isFirstLine = false;
                ((ParagraphFormatInfo)_renderInfo.FormatInfo)._isStarting = false;
            }

            _startLeaf = _currentLeaf;
            _currentVerticalInfo = CalcCurrentVerticalInfo();
            _currentYPosition = area.Y + TopBorderOffset;
            _formattingArea = area;
            var rect = _formattingArea.GetFittingRect(_currentYPosition, _currentVerticalInfo.Height);
            if (rect == null)
                return false;

            _currentXPosition = rect.X + LeftIndent;
            if (_isFirstLine)
                FormatListSymbol();

            return true;
        }

        /// <summary>
        /// Gets information necessary to render or measure the list symbol.
        /// </summary>
        /// <param name="symbol">The text or list symbol to render or measure.</param>
        /// <param name="font">The font to use for rendering or measuring.</param>
        /// <returns>True if a symbol needs to be rendered.</returns>
        bool GetListSymbol(out string symbol, out XFont font)
        {
            symbol = null!;
            font = null!;
            var formatInfo = (ParagraphFormatInfo)_renderInfo.FormatInfo;
            if (_phase == Phase.Formatting)
            {
                var format = _paragraph.Format;
                if (format.Values.ListInfo is not null && !format.Values.ListInfo.IsNull())
                {
                    var listInfo = format.ListInfo;
                    double size = format.Font.Size;
                    var style = FontHandler.GetXStyle(format.Font);

                    switch (listInfo.ListType)
                    {
                        case ListType.BulletList1:
                            symbol = "·";
                            font = new XFont("Symbol", size, style);
                            break;

                        case ListType.BulletList2:
                            symbol = "o";
                            font = new XFont("Courier New", size, style);
                            break;

                        case ListType.BulletList3:
                            symbol = "§";
                            font = new XFont("Wingdings", size, style);
                            break;

                        case ListType.NumberList1:
                            symbol = _documentRenderer.NextListNumber(listInfo) + ".";
                            font = FontHandler.FontToXFont(format.Font/*, _gfx.M/UH*/);  // CLEANUP
                            break;

                        case ListType.NumberList2:
                            symbol = NumberFormatter.Format(_documentRenderer.NextListNumber(listInfo), "alphabetic") + ".";
                            font = FontHandler.FontToXFont(format.Font/*, _gfx.M/UH*/);
                            break;

                        case ListType.NumberList3:
                            symbol = NumberFormatter.Format(_documentRenderer.NextListNumber(listInfo), "roman") + ".";
                            font = FontHandler.FontToXFont(format.Font/*, _gfx.M/UH*/);
                            break;
                        default:
                            throw new InvalidOperationException("Invalid list type.");
                    }
                    formatInfo.ListFont = font ?? NRT.ThrowOnNull<XFont>();
                    formatInfo.ListSymbol = symbol ?? NRT.ThrowOnNull<string>();
                    return true;
                }
            }
            else
            {
                if (formatInfo.ListFont != null! && formatInfo.ListSymbol != null!)
                {
                    font = formatInfo.ListFont;
                    symbol = formatInfo.ListSymbol;
                    return true;
                }
            }
            return false;
        }

        XUnit LeftIndent
        {
            get
            {
                ParagraphFormat format = _paragraph.Format;
                XUnit leftIndent = format.LeftIndent.Point;
                if (_isFirstLine)
                {
                    if (format.Values.ListInfo is not null && !format.Values.ListInfo.IsNull())
                    {
                        //if (format.ListInfo.Values.NumberPosition is not null)
                        if (!format.ListInfo.Values.NumberPosition.IsValueNullOrEmpty())
                            return format.ListInfo.NumberPosition.Point;
                        if (format.Values.FirstLineIndent.IsValueNullOrEmpty())
                            return 0;
                    }
                    return leftIndent + _paragraph.Format.FirstLineIndent.Point;
                }
                return leftIndent;
            }
        }

        XUnit RightIndent => _paragraph.Format.RightIndent.Point;

        /// <summary>
        /// Formats the paragraph by performing line breaks etc.
        /// </summary>
        /// <param name="area">The area in which to render.</param>
        /// <param name="previousFormatInfo">The format info that was obtained on formatting the same paragraph on a previous area.</param>
        internal override void Format(Area area, FormatInfo? previousFormatInfo)
        {
            var formatInfo = ((ParagraphFormatInfo)_renderInfo.FormatInfo);
            if (!InitFormat(area, previousFormatInfo))
            {
                formatInfo._isStarting = false;
                return;
            }
            formatInfo._isEnding = true;

            FormatResult lastResult = FormatResult.Continue;
            while (_currentLeaf != null)
            {
                FormatResult result = FormatElement(_currentLeaf.Current);
                switch (result)
                {
                    case FormatResult.Ignore:
                        _currentLeaf = _currentLeaf.GetNextLeaf();
                        break;

                    case FormatResult.Continue:
                        lastResult = result;
                        _currentLeaf = _currentLeaf.GetNextLeaf();
                        break;

                    case FormatResult.NewLine:
                        lastResult = result;
                        StoreLineInformation();
                        if (!StartNewLine())
                        {
                            result = FormatResult.NewArea;
                            formatInfo._isEnding = false;
                        }
                        break;
                }
                if (result == FormatResult.NewArea)
                {
                    lastResult = result;
                    formatInfo._isEnding = false;
                    break;
                }
            }
            if (formatInfo.IsEnding && lastResult != FormatResult.NewLine)
                StoreLineInformation();

            if (formatInfo.IsEnding)
                StoreBottomBorderInformation();

            formatInfo.ImageRenderInfos = _imageRenderInfos;
            FinishLayoutInfo();
        }

        /// <summary>
        /// Finishes the layout info by calculating starting and trailing heights.
        /// </summary>
        void FinishLayoutInfo()
        {
            LayoutInfo layoutInfo = _renderInfo.LayoutInfo;
            ParagraphFormat format = _paragraph.Format;
            ParagraphFormatInfo parInfo = (ParagraphFormatInfo)_renderInfo.FormatInfo;
            layoutInfo.MinWidth = _minWidth;
            layoutInfo.KeepTogether = format.KeepTogether;

            if (parInfo.IsComplete)
            {
                int limitOfLines = 1;
                if (parInfo._widowControl)
                    limitOfLines = 3;

                if (parInfo.LineCount <= limitOfLines)
                    layoutInfo.KeepTogether = true;
            }
            if (parInfo.IsStarting)
            {
                layoutInfo.MarginTop = format.SpaceBefore.Point;
                layoutInfo.PageBreakBefore = format.PageBreakBefore;
            }
            else
            {
                layoutInfo.MarginTop = 0;
                layoutInfo.PageBreakBefore = false;
            }

            if (parInfo.IsEnding)
            {
                layoutInfo.MarginBottom = _paragraph.Format.SpaceAfter.Point;
                layoutInfo.KeepWithNext = _paragraph.Format.KeepWithNext;
            }
            else
            {
                layoutInfo.MarginBottom = 0;
                layoutInfo.KeepWithNext = false;
            }
            if (parInfo.LineCount > 0)
            {
                XUnit startingHeight = parInfo.GetFirstLineInfo().Vertical.Height;
                if (parInfo._isStarting && _paragraph.Format.WidowControl && parInfo.LineCount >= 2)
                    startingHeight += parInfo.GetLineInfo(1).Vertical.Height;

                layoutInfo.StartingHeight = startingHeight;

                XUnit trailingHeight = parInfo.GetLastLineInfo().Vertical.Height;

                if (parInfo.IsEnding && _paragraph.Format.WidowControl && parInfo.LineCount >= 2)
                    trailingHeight += parInfo.GetLineInfo(parInfo.LineCount - 2).Vertical.Height;

                layoutInfo.TrailingHeight = trailingHeight;
            }
        }

        XUnit PopSavedBlankWidth()
        {
            XUnit width = _savedBlankWidth;
            _savedBlankWidth = 0;
            return width;
        }

        void SaveBlankWidth(XUnit blankWidth)
        {
            _savedBlankWidth = blankWidth;
        }

        XUnit _savedBlankWidth = 0;

        /// <summary>
        /// Processes the elements when formatting.
        /// </summary>
        /// <param name="docObj"></param>
        /// <returns></returns>
        FormatResult FormatElement(DocumentObject docObj)
        {
            // Check for available space in the area must be made for each element and explicitly for the last paragraph's element, because in formatting phase,
            // BottomBorderOffset contains the actual last line's bottom border offset value only for the last paragraph's object.
            var newVertInfo = CalcVerticalInfo(CurrentFont);
            var fittingRect = _formattingArea.GetFittingRect(_currentYPosition, newVertInfo.Height + BottomBorderOffset);
            if (fittingRect == null)
                return FormatResult.NewArea;

            FormatResult result;
            switch (docObj)
            {
                case Text obj:
                    if (!IsBlankOrSoftHyphen(obj))
                        result = FormatText(obj, fittingRect);
                    else if (IsBlank(obj))
                        result = FormatBlank(fittingRect);
                    else
                        //if (IsSoftHyphen(docObj))
                        result = FormatSoftHyphen();
                    break;

                case Character obj:
                    result = FormatCharacter(obj, fittingRect);
                    break;

                case DateField obj:
                    result = FormatDateField(obj, fittingRect);
                    break;

                case InfoField obj:
                    result = FormatInfoField(obj, fittingRect);
                    break;

                case NumPagesField obj:
                    result = FormatNumPagesField(obj, fittingRect);
                    break;

                case PageField obj:
                    result = FormatPageField(obj, fittingRect);
                    break;

                case SectionField obj:
                    result = FormatSectionField(obj, fittingRect);
                    break;

                case SectionPagesField obj:
                    result = FormatSectionPagesField(obj, fittingRect);
                    break;

                case BookmarkField obj:
                    result = FormatBookmarkField(obj);
                    break;

                case PageRefField obj:
                    result = FormatPageRefField(obj, fittingRect);
                    break;

                case Image obj:
                    result = FormatImage(obj, fittingRect);
                    break;

                default:
                    result = FormatResult.Continue;
                    break;
            }

            if (result == FormatResult.Continue)
                _currentVerticalInfo = newVertInfo;

            return result;
        }

        // ReSharper disable once UnusedParameter.Local
#pragma warning disable IDE0060
        FormatResult FormatImage(Image image, Rectangle fittingRect)
#pragma warning restore IDE0060
        {
            XUnit width = CurrentImageRenderInfo?.LayoutInfo.ContentArea.Width ?? NRT.ThrowOnNull<XUnit>();
            return FormatAsWord(width, fittingRect);
        }

        RenderInfo CalcImageRenderInfo(Image image)
        {
            var renderer = Create(_gfx, _documentRenderer, image, _fieldInfos);
            renderer!.Format(new Rectangle(0, 0, double.MaxValue, double.MaxValue), null);

            return renderer.RenderInfo;
        }

        static bool IsPlainText(DocumentObject docObj)
        {
            if (docObj is Text)
                return !IsSoftHyphen(docObj) && !IsBlank(docObj);

            return false;
        }

        static bool IsSymbol(DocumentObject docObj)
        {
            if (docObj is Character)
                return !IsSpaceCharacter(docObj) && !IsTab(docObj) && !IsLineBreak(docObj);

            return false;
        }

        static bool IsSpaceCharacter(DocumentObject docObj)
        {
            if (docObj is Character character)
            {
                switch (character.SymbolName)
                {
                    case SymbolName.Blank:
                    case SymbolName.Em:
                    case SymbolName.Em4:
                    case SymbolName.En:
                        return true;
                }
            }
            return false;
        }

        static bool IsWordLikeElement(DocumentObject docObj)
        {
            if (IsPlainText(docObj))
                return true;

            if (IsRenderedField(docObj))
                return true;

            if (IsSymbol(docObj))
                return true;

            return false;
        }

        FormatResult FormatBookmarkField(BookmarkField bookmarkField)
        {
            if (_fieldInfos is null)
                NRT.ThrowOnNull();

            _fieldInfos.AddBookmark(bookmarkField.Name);

            return FormatResult.Ignore;
        }

        FormatResult FormatPageRefField(PageRefField pageRefField, Rectangle fittingRect)
        {
            _reMeasureLine = true;
            string fieldValue = GetFieldValue(pageRefField);
            return FormatWord(fieldValue, fittingRect);
        }

        FormatResult FormatNumPagesField(NumPagesField numPagesField, Rectangle fittingRect)
        {
            _reMeasureLine = true;
            string fieldValue = GetFieldValue(numPagesField);
            return FormatWord(fieldValue, fittingRect);
        }

        FormatResult FormatPageField(PageField pageField, Rectangle fittingRect)
        {
            _reMeasureLine = true;
            string fieldValue = GetFieldValue(pageField);
            return FormatWord(fieldValue, fittingRect);
        }

        FormatResult FormatSectionField(SectionField sectionField, Rectangle fittingRect)
        {
            _reMeasureLine = true;
            string fieldValue = GetFieldValue(sectionField);
            return FormatWord(fieldValue, fittingRect);
        }

        FormatResult FormatSectionPagesField(SectionPagesField sectionPagesField, Rectangle fittingRect)
        {
            _reMeasureLine = true;
            string fieldValue = GetFieldValue(sectionPagesField);
            return FormatWord(fieldValue, fittingRect);
        }

        /// <summary>
        /// Helper function for formatting word-like elements like text and fields.
        /// </summary>
        FormatResult FormatWord(string word, Rectangle fittingRect)
        {
            XUnit width = MeasureString(word);
            return FormatAsWord(width, fittingRect);
        }

        XUnit _savedWordWidth = 0;

        /// <summary>
        /// When rendering a justified paragraph, only the part after the last tab stop needs remeasuring.
        /// </summary>
        bool IgnoreHorizontalGrowth =>
            _phase == Phase.Rendering && _paragraph.Format.Alignment == ParagraphAlignment.Justify &&
            !_lastTabPassed;

        FormatResult FormatAsWord(XUnit width, Rectangle fittingRect)
        {
            _savedWordWidth = width;

            if (_currentXPosition + width > fittingRect.X + fittingRect.Width - RightIndent + Tolerance)
                return FormatResult.NewLine;

            _currentXPosition += width;
            // For Tabs in justified context.
            if (!IgnoreHorizontalGrowth)
                _currentWordsWidth += width;
            if (_savedBlankWidth > 0)
            {
                // For Tabs in justified context.
                if (!IgnoreHorizontalGrowth)
                    ++_currentBlankCount;
            }
            // For Tabs in justified context.
            if (!IgnoreHorizontalGrowth)
                _currentLineWidth += width + PopSavedBlankWidth();
            _minWidth = Math.Max(_minWidth, width);
            return FormatResult.Continue;
        }

        FormatResult FormatDateField(DateField dateField, Rectangle fittingRect)
        {
            _reMeasureLine = true;
            string estimatedFieldValue = DateTime.Now.ToString(GetEffectiveFormat(dateField));
            return FormatWord(estimatedFieldValue, fittingRect);
        }

        FormatResult FormatInfoField(InfoField infoField, Rectangle fittingRect)
        {
            string fieldValue = GetDocumentInfo(infoField.Name);
            if (fieldValue == "")
                return FormatResult.Continue;

            return FormatWord(fieldValue, fittingRect);

        }

        string GetDocumentInfo(string name)
        {
            Debug.Assert(_paragraph.Document != null, "_paragraph.Document != null");
            return name.ToLower() switch
            {
                "title" => _paragraph.Document.Info.Title,
                "author" => _paragraph.Document.Info.Author,
                "keywords" => _paragraph.Document.Info.Keywords,
                "subject" => _paragraph.Document.Info.Subject,
                _ => ""
            };
        }

        Area GetShadingArea()
        {
            var contentArea = _renderInfo.LayoutInfo.ContentArea;
            var format = _paragraph.Format;
            XUnit left = contentArea.X;
            left += format.LeftIndent;
            if (format.FirstLineIndent < 0)
                left += format.FirstLineIndent;

            XUnit top = contentArea.Y;
            XUnit bottom = contentArea.Y + contentArea.Height;
            XUnit right = contentArea.X + contentArea.Width;
            right -= format.RightIndent;

            if (_paragraph.Format.Values.Borders is not null && !_paragraph.Format.Values.Borders.IsNull())
            {
                Borders borders = format.Borders;
                var bordersRenderer = new BordersRenderer(borders, _gfx);

                if (_renderInfo.FormatInfo.IsStarting)
                    top += bordersRenderer.GetWidth(BorderType.Top);
                if (_renderInfo.FormatInfo.IsEnding)
                    bottom -= bordersRenderer.GetWidth(BorderType.Bottom);

                left -= borders.DistanceFromLeft;
                right += borders.DistanceFromRight;
            }
            return new Rectangle(left, top, right - left, bottom - top);
        }

        void RenderShading()
        {
            if (_paragraph.Format.Values.Shading is null || _paragraph.Format.Values.Shading.IsNull())
                return;

            var shadingRenderer = new ShadingRenderer(_gfx, _paragraph.Format.Shading);
            var area = GetShadingArea();

            shadingRenderer.Render(area.X, area.Y, area.Width, area.Height);
        }

        void RenderBorders()
        {
            if (_paragraph.Format.Values.Borders is null || _paragraph.Format.Values.Borders.IsNull())
                return;

            var shadingArea = GetShadingArea();
            XUnit left = shadingArea.X;
            XUnit top = shadingArea.Y;
            XUnit bottom = shadingArea.Y + shadingArea.Height;
            XUnit right = shadingArea.X + shadingArea.Width;

            var borders = _paragraph.Format.Borders;
            var bordersRenderer = new BordersRenderer(borders, _gfx);
            XUnit borderWidth = bordersRenderer.GetWidth(BorderType.Left);
            if (borderWidth > 0)
            {
                left -= borderWidth;
                bordersRenderer.RenderVertically(BorderType.Left, left, top, bottom - top);
            }

            borderWidth = bordersRenderer.GetWidth(BorderType.Right);
            if (borderWidth > 0)
            {
                bordersRenderer.RenderVertically(BorderType.Right, right, top, bottom - top);
                right += borderWidth;
            }

            borderWidth = bordersRenderer.GetWidth(BorderType.Top);
            if (_renderInfo.FormatInfo.IsStarting && borderWidth > 0)
            {
                top -= borderWidth;
                bordersRenderer.RenderHorizontally(BorderType.Top, left, top, right - left);
            }

            borderWidth = bordersRenderer.GetWidth(BorderType.Bottom);
            if (_renderInfo.FormatInfo.IsEnding && borderWidth > 0)
            {
                bordersRenderer.RenderHorizontally(BorderType.Bottom, left, bottom, right - left);
            }
        }

        XUnit MeasureString(string word)
        {
            var xFont = CurrentFont;
            XUnit width = _gfx.MeasureString(word, xFont, StringFormat).Width;
            var font = CurrentDomFont;

            if (font.Subscript || font.Superscript)
                width *= FontHandler.GetSubSuperScaling(xFont);

            return width;
        }

        XUnit GetSpaceWidth(Character character)
        {
            XUnit width = character.SymbolName switch
            {
                SymbolName.Blank => MeasureString(" "),
                SymbolName.Em => MeasureString("m"),
                SymbolName.Em4 => 0.25 * MeasureString("m"),
                SymbolName.En => MeasureString("n"),
                _ => 0
            };
            return width * character.Count;
        }

        void RenderListSymbol()
        {
            if (GetListSymbol(out var symbol, out var font))
            {
                var brush = FontHandler.FontColorToXBrush(_paragraph.Format.Font);
                _gfx.DrawString(symbol, font, brush, _currentXPosition, CurrentBaselinePosition);
                _currentXPosition += _gfx.MeasureString(symbol, font, StringFormat).Width;
                var tabOffset = NextTabOffset();
                _currentXPosition += tabOffset.Offset;
                _lastTabPosition = _currentXPosition;
            }
        }

        void FormatListSymbol()
        {
            if (GetListSymbol(out var symbol, out var font))
            {
                _currentVerticalInfo = CalcVerticalInfo(font);
                _currentXPosition += _gfx.MeasureString(symbol, font, StringFormat).Width;
                FormatTab();
            }
        }

        FormatResult FormatSpace(Character character, Rectangle fittingRect)
        {
            XUnit width = GetSpaceWidth(character);
            return FormatAsWord(width, fittingRect);
        }

        static string GetSymbol(Character character)
        {
            char ch;
            switch (character.SymbolName)
            {
                case SymbolName.Euro:
                    ch = '€';
                    break;

                case SymbolName.Copyright:
                    ch = '©';
                    break;

                case SymbolName.Trademark:
                    ch = '™';
                    break;

                case SymbolName.RegisteredTrademark:
                    ch = '®';
                    break;

                case SymbolName.Bullet:
                    ch = '•';
                    break;

                case SymbolName.Not:
                    ch = '¬';
                    break;
                //REM: Non-breakable blanks are still ignored.
                //        case SymbolName.SymbolNonBreakableBlank:
                //          return "\xA0";
                //          break;

                case SymbolName.EmDash:
                    ch = '—';
                    break;

                case SymbolName.EnDash:
                    ch = '–';
                    break;

                default:
                    char c = character.Char;
                    char[] chars = Encoding.UTF8.GetChars([(byte)c]);
                    ch = chars[0];
                    break;
            }

            int count = character.Count;
            if (!(count > 0))
                return ch.ToString(); // Return a single character.

            // Possibly return more characters.
            var returnString = new StringBuilder(ch.ToString());
            while (--count > 0)
                returnString.Append(ch);
            return returnString.ToString();
        }

        FormatResult FormatSymbol(Character character, Rectangle fittingRect)
        {
            return FormatWord(GetSymbol(character), fittingRect);
        }

        /// <summary>
        /// Processes (measures) a special character within text.
        /// </summary>
        /// <param name="character">The character to process.</param>
        /// <param name="fittingRect">The rect defining the space that is still available on the area to render the character.</param>
        /// <returns>True if the character should start at a new line.</returns>
        FormatResult FormatCharacter(Character character, Rectangle fittingRect)
        {
            return character.SymbolName switch
            {
                SymbolName.Blank => FormatSpace(character, fittingRect),
                SymbolName.Em => FormatSpace(character, fittingRect),
                SymbolName.Em4 => FormatSpace(character, fittingRect),
                SymbolName.En => FormatSpace(character, fittingRect),
                SymbolName.LineBreak => FormatLineBreak(),
                SymbolName.Tab => FormatTab(),
                _ => FormatSymbol(character, fittingRect)
            };
        }

        /// <summary>
        /// Processes (measures) a blank.
        /// </summary>
        /// <returns>True if the blank causes a line break.</returns>
        FormatResult FormatBlank(Rectangle fittingRect)
        {

            if (IgnoreBlank())
                return FormatResult.Ignore;

            _savedWordWidth = 0;
            XUnit width;
            var currentFont = CurrentFont;
            if (_lastFont == currentFont)
            {
                width = _blankWidth;
#if DEBUG
                if (MeasureString(" ") != width)
                    throw new InvalidOperationException("Check FormatBlank.");
                //Debug.Assert(MeasureString(" ") == width);
#endif
            }
            else
            {
                _blankWidth = width = MeasureString(" ");
                _lastFont = currentFont;
            }

            if (_currentXPosition + width > fittingRect.X + fittingRect.Width - RightIndent + Tolerance)
                return FormatResult.NewLine;
            
            _currentXPosition += width;
            SaveBlankWidth(width);
            return FormatResult.Continue;
        }

        XUnit _blankWidth;
        XFont? _lastFont;

        FormatResult FormatLineBreak()
        {
            if (_phase != Phase.Rendering)
                _currentLeaf = _currentLeaf?.GetNextLeaf();

            _savedWordWidth = 0;
            return FormatResult.NewLine;
        }

        /// <summary>
        /// Processes a text element during formatting.
        /// </summary>
        /// <param name="text">The text element to measure.</param>
        /// <param name="fittingRect">The available space for the text on the area.</param>
        FormatResult FormatText(Text text, Rectangle fittingRect)
        {
            return FormatWord(text.Content, fittingRect);
        }

        FormatResult FormatSoftHyphen()
        {
            if (_currentLeaf is null || _startLeaf is null)
                NRT.ThrowOnNull();

            if (_currentLeaf.Current == _startLeaf.Current)
                return FormatResult.Continue;

            var nextIter = _currentLeaf.GetNextLeaf();
            var prevIter = _currentLeaf.GetPreviousLeaf() ?? NRT.ThrowOnNull<ParagraphIterator>();
            // nextIter can be null if the soft hyphen is at the end of a paragraph. To prevent a crash, we jump out if nextIter is null.
            if (!IsWordLikeElement(prevIter.Current) || nextIter == null || !IsWordLikeElement(nextIter.Current))
                return FormatResult.Continue;

            //--- Save ---------------------------------
            SaveBeforeProbing(out var iter, out var blankCount, out var wordsWidth, out var xPosition, out var lineWidth, out var blankWidth);
            //------------------------------------------
            _currentLeaf = nextIter;
            var result = FormatElement(nextIter.Current);

            //--- Restore ------------------------------
            RestoreAfterProbing(iter, blankCount, wordsWidth, xPosition, lineWidth, blankWidth);
            //------------------------------------------
            if (result == FormatResult.Continue)
                return FormatResult.Continue;

            RestoreAfterProbing(iter, blankCount, wordsWidth, xPosition, lineWidth, blankWidth);
            var fittingRect = _formattingArea.GetFittingRect(_currentYPosition, _currentVerticalInfo.Height);
            if (fittingRect is null)
                NRT.ThrowOnNull();

            XUnit hyphenWidth = MeasureString("-");
            if (xPosition + hyphenWidth <= fittingRect.X + fittingRect.Width + Tolerance
                // If one word fits, but not the hyphen, the formatting must continue with the next leaf.
                || prevIter.Current == _startLeaf.Current)
            {
                // For Tabs in justified context
                if (!IgnoreHorizontalGrowth)
                {
                    _currentWordsWidth += hyphenWidth;
                    _currentLineWidth += hyphenWidth;
                }
                _currentLeaf = nextIter;
                return FormatResult.NewLine;
            }
            else
            {
                _currentWordsWidth -= _savedWordWidth;
                _currentLineWidth -= _savedWordWidth;
                _currentLineWidth -= GetPreviousBlankWidth(prevIter);
                _currentLeaf = prevIter;
                return FormatResult.NewLine;
            }
        }

        XUnit GetPreviousBlankWidth(ParagraphIterator beforeIter)
        {
            XUnit width = 0;
            var savedIter = _currentLeaf;
            _currentLeaf = beforeIter.GetPreviousLeaf();
            while (_currentLeaf != null)
            {
                if (_currentLeaf.Current is BookmarkField)
                    _currentLeaf = _currentLeaf.GetPreviousLeaf();
                else if (IsBlank(_currentLeaf.Current))
                {
                    if (!IgnoreBlank())
                        width = CurrentWordDistance;

                    break;
                }
                else
                    break;
            }
            _currentLeaf = savedIter;
            return width;
        }

        void HandleNonFittingLine()
        {
            if (_currentLeaf != null)
            {
                if (_savedWordWidth > 0)
                {
                    _currentWordsWidth = _savedWordWidth;
                    _currentLineWidth = _savedWordWidth;
                }
                _currentLeaf = _currentLeaf.GetNextLeaf();
                _currentYPosition += _currentVerticalInfo.Height;
                _currentVerticalInfo = new VerticalLineInfo();
            }
        }

        /// <summary>
        /// Starts a new line by resetting measuring values.
        /// Do not call before the first line is formatted!
        /// </summary>
        /// <returns>True if the new line may fit the formatting area.</returns>
        bool StartNewLine()
        {
            _tabOffsets = [];
            _lastTab = null;
            _lastTabPosition = 0;
            _currentYPosition += _currentVerticalInfo.Height;

            var rect = _formattingArea.GetFittingRect(_currentYPosition, _currentVerticalInfo.Height + BottomBorderOffset);
            if (rect == null)
                return false;

            _isFirstLine = false;
            _currentXPosition = StartXPosition; // Depends on "currentVerticalInfo".
            _currentVerticalInfo = new VerticalLineInfo();
            _currentVerticalInfo = CalcCurrentVerticalInfo();
            _startLeaf = _currentLeaf;
            _currentBlankCount = 0;
            _currentWordsWidth = 0;
            _currentLineWidth = 0;
            return true;
        }

        /// <summary>
        /// Stores all line information.
        /// </summary>
        void StoreLineInformation()
        {
            PopSavedBlankWidth();

            XUnit topBorderOffset = TopBorderOffset;
            var contentArea = _renderInfo.LayoutInfo.ContentArea;
            if (topBorderOffset > 0) // May only occur for the first line.
                contentArea = _formattingArea.GetFittingRect(_formattingArea.Y, topBorderOffset);

            if (contentArea == null)
                contentArea = _formattingArea.GetFittingRect(_currentYPosition, _currentVerticalInfo.Height);
            else
                contentArea = contentArea.Unite(_formattingArea.GetFittingRect(_currentYPosition, _currentVerticalInfo.Height) ?? NRT.ThrowOnNull<Rectangle>());

            var lineInfo = new LineInfo
            {
                Vertical = _currentVerticalInfo
            };

            if (_startLeaf != null && _startLeaf == _currentLeaf)
                HandleNonFittingLine();

            lineInfo.LastTab = _lastTab;
            _renderInfo.LayoutInfo.ContentArea = contentArea ?? NRT.ThrowOnNull<Area>();

            lineInfo.StartIter = _startLeaf;

            if (_currentLeaf == null)
                lineInfo.EndIter = new ParagraphIterator(_paragraph.Elements).GetLastLeaf();
            else
                lineInfo.EndIter = _currentLeaf.GetPreviousLeaf() ?? NRT.ThrowOnNull<ParagraphIterator>();

            lineInfo.BlankCount = _currentBlankCount;

            lineInfo.WordsWidth = _currentWordsWidth;

            lineInfo.LineWidth = _currentLineWidth;
            lineInfo.TabOffsets = _tabOffsets;
            lineInfo.ReMeasureLine = _reMeasureLine;

            _savedWordWidth = 0;
            _reMeasureLine = false;
            ((ParagraphFormatInfo)_renderInfo.FormatInfo).AddLineInfo(lineInfo);
        }

        /// <summary>
        /// Adds the BottomBorderOffset to the ContentArea. This should only be called for the last line of a paragraph.
        /// </summary>
        void StoreBottomBorderInformation()
        {
            var contentArea = _renderInfo.LayoutInfo.ContentArea;

            var bottomBorderOffset = BottomBorderOffset;
            if (bottomBorderOffset > 0)
                contentArea = contentArea.Unite(_formattingArea.GetFittingRect(_currentYPosition + _currentVerticalInfo.Height, bottomBorderOffset) ?? NRT.ThrowOnNull<Rectangle>());

            _renderInfo.LayoutInfo.ContentArea = contentArea;
        }

        /// <summary>
        /// Gets the top border offset for the first line, else 0.
        /// </summary>
        XUnit TopBorderOffset
        {
            get
            {
                XUnit offset = 0;
                if (_isFirstLine && _paragraph.Format.Values.Borders is not null && !_paragraph.Format.Values.Borders.IsNull())
                {
                    offset += _paragraph.Format.Borders.DistanceFromTop;
                    if (_paragraph.Format.Values.Borders is not null && !_paragraph.Format.Values.Borders.IsNull())
                    {
                        var bordersRenderer = new BordersRenderer(_paragraph.Format.Borders, _gfx);
                        offset += bordersRenderer.GetWidth(BorderType.Top);
                    }
                }
                return offset;
            }
        }

        /// <summary>
        /// Gets the bottom border offset for the last line, else 0.
        /// While formatting, the actual border offset for the last line is only returned for the last paragraph's element.
        /// </summary>
        XUnit BottomBorderOffset
        {
            get
            {
                bool considerAsLastLine;
                // While formatting, it is impossible to determine whether we are in the last line until the last leaf is reached.
                if (_phase == Phase.Formatting)
                    considerAsLastLine = _currentLeaf == null || _currentLeaf.IsLastLeaf;
                else
                    considerAsLastLine = _isLastLine;

                if (!considerAsLastLine)
                    return 0;

                if (_paragraph.Format.Values.Borders is null || _paragraph.Format.Values.Borders.IsNull())
                    return 0;

                var offset = new XUnit(_paragraph.Format.Borders.DistanceFromBottom);
                var bordersRenderer = new BordersRenderer(_paragraph.Format.Borders, _gfx);
                offset += bordersRenderer.GetWidth(BorderType.Bottom);
                return offset;
            }
        }

        VerticalLineInfo CalcCurrentVerticalInfo()
        {
            return CalcVerticalInfo(CurrentFont);
        }

        VerticalLineInfo CalcVerticalInfo(XFont font)
        {
            ParagraphFormat paragraphFormat = _paragraph.Format;
            LineSpacingRule spacingRule = paragraphFormat.LineSpacingRule;
            XUnit lineHeight = 0;

            XUnit descent = FontHandler.GetDescent(font);
            descent = Math.Max(_currentVerticalInfo.Descent, descent);

            XUnit singleLineSpace = font.GetHeight();
            var imageRenderInfo = CurrentImageRenderInfo;
            if (imageRenderInfo != null)
                singleLineSpace = singleLineSpace - FontHandler.GetAscent(font) + imageRenderInfo.LayoutInfo.ContentArea.Height;

            XUnit inherentLineSpace = Math.Max(_currentVerticalInfo.InherentLineSpace, singleLineSpace);
            switch (spacingRule)
            {
                case LineSpacingRule.Single:
                    lineHeight = singleLineSpace;
                    break;

                case LineSpacingRule.OnePtFive:
                    lineHeight = 1.5 * singleLineSpace;
                    break;

                case LineSpacingRule.Double:
                    lineHeight = 2.0 * singleLineSpace;
                    break;

                case LineSpacingRule.Multiple:
                    lineHeight = _paragraph.Format.LineSpacing * singleLineSpace;
                    break;

                case LineSpacingRule.AtLeast:
                    lineHeight = Math.Max(singleLineSpace, _paragraph.Format.LineSpacing);
                    break;

                case LineSpacingRule.Exactly:
                    lineHeight = new XUnit(_paragraph.Format.LineSpacing);
                    inherentLineSpace = _paragraph.Format.LineSpacing.Point;
                    break;
            }
            lineHeight = Math.Max(_currentVerticalInfo.Height, lineHeight);
            if (MaxElementHeight > 0)
                lineHeight = Math.Min(MaxElementHeight - Tolerance, lineHeight);

            return new(lineHeight, descent, inherentLineSpace);
        }

        /// <summary>
        /// The font used for the current paragraph element.
        /// </summary>
        XFont CurrentFont => FontHandler.FontToXFont(CurrentDomFont /*, _documentRenderer.PrivateFonts, _gfx.M/UH*/);  // CLEANUP

        Font CurrentDomFont
        {
            get
            {
                if (_currentDomFont != null && _currentObject == _currentLeaf?.Current)
                {
#if DEBUG
                    var font = CurrentDomFontOld;
                    if (font.Name != _currentDomFont.Name ||
                        font.Bold != _currentDomFont.Bold ||
                        font.Italic != _currentDomFont.Italic ||
                        font.Size != _currentDomFont.Size ||
                        font.Superscript != _currentDomFont.Superscript ||
                        font.Subscript != _currentDomFont.Subscript ||
                        font.Color != _currentDomFont.Color ||
                        font.Underline != _currentDomFont.Underline)
                        throw new InvalidOperationException("Check CurrentDomFont.");
#endif
                    return _currentDomFont;
                }

                if (_currentLeaf != null)
                {
                    var parent = DocumentRelations.GetParent(_currentLeaf.Current);
                    parent = DocumentRelations.GetParent(parent);

                    if (parent is FormattedText formattedText)
                    {
                        _currentObject = _currentLeaf.Current;
                        return _currentDomFont = formattedText.Font;
                    }

                    if (parent is Hyperlink hyperlink)
                    {
                        _currentObject = _currentLeaf.Current;
                        return _currentDomFont = hyperlink.Font;
                    }
                }
                _currentObject = _currentLeaf?.Current;
                return _currentDomFont = _paragraph.Format.Font;
            }
        }
        Font? _currentDomFont;
        DocumentObject? _currentObject;

#if DEBUG
        Font CurrentDomFontOld
        {
            get
            {
                if (_currentLeaf != null)
                {
                    var parent = DocumentRelations.GetParent(_currentLeaf.Current);
                    parent = DocumentRelations.GetParent(parent);

                    if (parent is FormattedText formattedText)
                    {
                        return formattedText.Font;
                    }

                    if (parent is Hyperlink hyperlink)
                    {
                        return hyperlink.Font;
                    }
                }
                return _paragraph.Format.Font;
            }
        }
#endif

        /// <summary>
        /// Help function to receive a line height on empty paragraphs.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="gfx">The GFX.</param>
        /// <param name="renderer">The renderer.</param>
#pragma warning disable IDE0060
        internal static XUnit GetLineHeight(ParagraphFormat format, XGraphics gfx, DocumentRenderer renderer)
#pragma warning restore IDE0060
        {
            var font = FontHandler.FontToXFont(format.Font);
            XUnit singleLineSpace = font.GetHeight();
            return format.LineSpacingRule switch
            {
                LineSpacingRule.Exactly => format.LineSpacing.Point,
                LineSpacingRule.AtLeast => Math.Max(format.LineSpacing.Point, font.GetHeight()),
                LineSpacingRule.Multiple => format.LineSpacing * format.Font.Size,
                LineSpacingRule.OnePtFive => 1.5 * singleLineSpace,
                LineSpacingRule.Double => 2.0 * singleLineSpace,
                LineSpacingRule.Single => singleLineSpace,
                _ => singleLineSpace
            };
        }

        void RenderUnderline(XUnit width, bool isWord)
        {
            var pen = GetUnderlinePen(isWord);

            bool penChanged = UnderlinePenChanged(pen);
            if (penChanged)
            {
                if (_currentUnderlinePen != null)
                    EndUnderline(_currentUnderlinePen, _currentXPosition);

                if (pen != null)
                    StartUnderline(_currentXPosition);

                _currentUnderlinePen = pen;
            }
            if (_currentLeaf is null || _endLeaf is null)
                NRT.ThrowOnNull();
            if (_currentLeaf.Current == _endLeaf.Current)
            {
                if (_currentUnderlinePen != null)
                    EndUnderline(_currentUnderlinePen, _currentXPosition + width);

                _currentUnderlinePen = null;
            }
        }

        void StartUnderline(XUnit xPosition)
        {
            _underlineStartPos = xPosition;
        }

        void EndUnderline(XPen pen, XUnit xPosition)
        {
            XUnit yPosition = CurrentBaselinePosition;
            yPosition += 0.33 * _currentVerticalInfo.Descent;
            _gfx.DrawLine(pen, _underlineStartPos, yPosition, xPosition, yPosition);
        }

        XPen? _currentUnderlinePen;
        XUnit _underlineStartPos;

        bool UnderlinePenChanged(XPen? pen)
        {
            if (pen == null && _currentUnderlinePen == null)
                return false;

            if (pen == null && _currentUnderlinePen != null)
                return true;

            if (pen != null && _currentUnderlinePen == null)
                return true;

            if (_currentUnderlinePen is null)
                NRT.ThrowOnNull();

            if (pen != null && pen.Color != _currentUnderlinePen!.Color)  // BUG in ReSharper:
                return true;

            if (pen is null)
                NRT.ThrowOnNull();

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return pen.Width != _currentUnderlinePen.Width;
        }

        RenderInfo? CurrentImageRenderInfo
        {
            get
            {
                if (_currentLeaf is { Current: Image image })
                {
                    if (_imageRenderInfos != null! && _imageRenderInfos.TryGetValue(image, out var info))
                        return info;

                    _imageRenderInfos ??= [];

                    var renderInfo = CalcImageRenderInfo(image);
                    _imageRenderInfos.Add(image, renderInfo);
                    return renderInfo;
                }
                return null;
            }
        }

        XPen? GetUnderlinePen(bool isWord)
        {
            var font = CurrentDomFont;
            var underlineType = font.Underline;
            if (underlineType == Underline.None)
                return null;

            if (underlineType == Underline.Words && !isWord)
                return null;

#if noCMYK
            XPen pen = new XPen(XColor.FromArgb(font.Color.Argb), font.Size / 16);
#else
            Debug.Assert(_paragraph.Document != null, "_paragraph.Document != null");
            var pen = new XPen(ColorHelper.ToXColor(font.Color, _paragraph.Document.UseCmykColor), font.Size / 16);
#endif
            pen.DashStyle = font.Underline switch
            {
                Underline.DotDash => XDashStyle.DashDot,
                Underline.DotDotDash => XDashStyle.DashDotDot,
                Underline.Dash => XDashStyle.Dash,
                Underline.Dotted => XDashStyle.Dot,
                Underline.Single => XDashStyle.Solid,
                _ => XDashStyle.Solid
            };
            return pen;
        }

        //static XStringFormat StringFormat => _stringFormat ??= XStringFormats.Default;
        //static XStringFormat _stringFormat;
        static XStringFormat StringFormat { get; } = XStringFormats.Default;

        /// <summary>
        /// The paragraph to format or render.
        /// </summary>
        readonly Paragraph _paragraph;

        XUnit _currentWordsWidth;
        int _currentBlankCount;
        XUnit _currentLineWidth;
        bool _isFirstLine;
        bool _isLastLine;
        VerticalLineInfo _currentVerticalInfo;
        Area _formattingArea = default!;
        XUnit _currentYPosition;
        XUnit _currentXPosition;
        ParagraphIterator? _currentLeaf;
        ParagraphIterator? _startLeaf;
        ParagraphIterator? _endLeaf;
        bool _reMeasureLine;
        XUnit _minWidth = 0;
        Dictionary<Image, RenderInfo> _imageRenderInfos = default!;
        List<TabOffset> _tabOffsets = default!;
        DocumentObject? _lastTab;
        bool _lastTabPassed;
        XUnit _lastTabPosition;
    }
}
