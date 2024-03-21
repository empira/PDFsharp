namespace PdfSharp.TestHelper.Analysis.ContentStream
{
    /// <summary>
    /// Iterates the line elements of a content stream for easier inspection.
    /// </summary>
    public class GetLine : GetObjectBase<GetLine.LineInfo>
    {
        public GetLine(ContentStreamEnumerator contentStreamEnumerator) : base(contentStreamEnumerator)
        { }

        protected override bool IdentifyingElementCheck(string element)
        {
            // Find the identifying l element.
            return element == "l";
        }

        protected override (bool, LineInfo?) GetObject()
        {
            var lState = _contentStreamEnumerator.GetState();

            // Y of the l element is y2.
            if (!_contentStreamEnumerator.MovePrevious())
                return GetObjectFailed();
            var y2Str = _contentStreamEnumerator.Current!;

            // X of the l element is x2.
            if (!_contentStreamEnumerator.MovePrevious())
                return GetObjectFailed();
            var x2Str = _contentStreamEnumerator.Current!;

            // Find the previous m or l element.
            if (!_contentStreamEnumerator.MovePrevious())
                return GetObjectFailed();
            if (_contentStreamEnumerator.Current is not ("m" or "l"))
                return GetObjectFailed();

            // Y of the previous m/l element is y1.
            if (!_contentStreamEnumerator.MovePrevious())
                return GetObjectFailed();
            var y1Str = _contentStreamEnumerator.Current!;

            // X of the previous m/l element is x1.
            if (!_contentStreamEnumerator.MovePrevious())
                return GetObjectFailed();
            var x1Str = _contentStreamEnumerator.Current!;

            // Move back to the identifying l element.
            _contentStreamEnumerator.SetState(lState);

            var obj = LineInfo.CreateLineInfo(x1Str, y1Str, x2Str, y2Str);
            return (true, obj);
        }

        /// <summary>
        /// Contains the information for the found line.
        /// </summary>
        public class LineInfo
        {
            /// <summary>
            /// The start X value of the line.
            /// </summary>
            public string X1Str { get; private set; }

            /// <summary>
            /// The start Y value of the line.
            /// </summary>
            public string Y1Str { get; private set; }

            /// <summary>
            /// The end X value of the line.
            /// </summary>
            public string X2Str { get; private set; }

            /// <summary>
            /// The end Y value of the line.
            /// </summary>
            public string Y2Str { get; private set; }

            /// <summary>
            /// The start X value of the line.
            /// </summary>
            public double X1 { get; private set; }

            /// <summary>
            /// The start Y value of the line.
            /// </summary>
            public double Y1 { get; private set; }

            /// <summary>
            /// The end X value of the line.
            /// </summary>
            public double X2 { get; private set; }

            /// <summary>
            /// The end Y value of the line.
            /// </summary>
            public double Y2 { get; private set; }

            LineInfo(string x1Str, string y1Str, string x2Str, string y2Str, double x1, double y1, double x2, double y2)
            {
                X1Str = x1Str;
                Y1Str = y1Str;
                X2Str = x2Str;
                Y2Str = y2Str;
                X1 = x1;
                Y1 = y1;
                X2 = x2;
                Y2 = y2;
            }

            public static LineInfo? CreateLineInfo(string x1Str, string y1Str, string x2Str, string y2Str)
            {
                if (!PdfFileHelper.TryParseDouble(x1Str, out var x1))
                    return null;

                if (!PdfFileHelper.TryParseDouble(y1Str, out var y1))
                    return null;

                if (!PdfFileHelper.TryParseDouble(x2Str, out var x2))
                    return null;

                if (!PdfFileHelper.TryParseDouble(y2Str, out var y2))
                    return null;

                return new LineInfo(x1Str, y1Str, x2Str, y2Str, x1, y1, x2, y2);
            }
        }
    }
}
