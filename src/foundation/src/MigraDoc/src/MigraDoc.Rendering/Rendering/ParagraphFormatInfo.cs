// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Shapes;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Vertical measurements of a paragraph line.
    /// </summary>
    struct VerticalLineInfo
    {
        internal VerticalLineInfo(XUnit height, XUnit descent, XUnit inherentlineSpace)
        {
            Height = height;
            Descent = descent;
            InherentLineSpace = inherentlineSpace;
        }

        public XUnit Height;

        public XUnit Descent;

        public XUnit InherentLineSpace;
    }

    /// <summary>
    /// Line info object used by the paragraph format info.
    /// </summary>
    struct LineInfo
    {
        public ParagraphIterator? StartIter;
        public ParagraphIterator? EndIter;
        public XUnit WordsWidth;
        public XUnit LineWidth;
        public int BlankCount;
        public VerticalLineInfo Vertical;
        public List<TabOffset> TabOffsets;
        public bool ReMeasureLine;
        public DocumentObject? LastTab;
    }

    /// <summary>
    /// Formatting information for a paragraph.
    /// </summary>
    sealed class ParagraphFormatInfo : FormatInfo
    {
        internal ParagraphFormatInfo()
        { }

        internal LineInfo GetLineInfo(int lineIdx)
            => _lineInfos[lineIdx];

        internal LineInfo GetLastLineInfo()
            => _lineInfos[LineCount - 1];

        internal LineInfo GetFirstLineInfo()
            => _lineInfos[0];

        internal void AddLineInfo(LineInfo lineInfo)
            => _lineInfos.Add(lineInfo);

        internal int LineCount
            => _lineInfos.Count;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mergeInfo"></param>
        /// <returns></returns>
        internal void Append(FormatInfo mergeInfo)
        {
            ParagraphFormatInfo formatInfo = (ParagraphFormatInfo)mergeInfo;
            _lineInfos.AddRange(formatInfo._lineInfos);
        }

        /// <summary>
        /// Indicates whether the paragraph is ending.
        /// </summary>
        /// <returns>True if the paragraph is ending.</returns>
        internal override bool IsEnding => _isEnding;

        internal bool _isEnding;  // TODO auto prop

        /// <summary>
        /// Indicates whether the paragraph is starting.
        /// </summary>
        /// <returns>True if the paragraph is starting.</returns>
        internal override bool IsStarting => _isStarting;

        internal bool _isStarting;  // TODO auto prop

        internal override bool IsComplete => _isStarting && _isEnding;

        internal override bool IsEmpty => _lineInfos.Count == 0;

        internal override bool StartingIsComplete
        {
            get
            {
                if (_widowControl)
                    return (IsComplete || (_isStarting && _lineInfos.Count >= 2));
                return _isStarting;
            }
        }
        internal bool _widowControl;  // TODO auto prop

        internal override bool EndingIsComplete
        {
            get
            {
                if (_widowControl)
                    return (IsComplete || (_isEnding && _lineInfos.Count >= 2));
                return _isEnding;
            }
        }

        internal void RemoveEnding()
        {
            if (!IsEmpty)
            {
                if (_widowControl && _isEnding && LineCount >= 2)
                    _lineInfos.RemoveAt(LineCount - 2);
                if (LineCount > 0)
                    _lineInfos.RemoveAt(LineCount - 1);

                _isEnding = false;
            }
        }

        internal string ListSymbol = null!;
        internal XFont ListFont = null!;
        internal Dictionary<Image, RenderInfo> ImageRenderInfos = null!;
        readonly List<LineInfo> _lineInfos = new();
    }
}
