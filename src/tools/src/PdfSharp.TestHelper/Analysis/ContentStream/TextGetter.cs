// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Events;
using PdfSharp.Fonts.Internal;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.TestHelper.Analysis.ContentStream
{
    /// <summary>
    /// Iterates the text elements of a content stream for easier inspection.
    /// </summary>
    public class TextGetter : ObjectGetterBase<TextGetter.TextInfo>
    {
        public TextGetter(ContentStreamEnumerator contentStreamEnumerator) : base(contentStreamEnumerator)
        { }

        protected override bool IdentifyingElementCheck(string element)
        {
            // Find the identifying Tj element.
            return element == "Tj";
        }

        protected override (bool, TextInfo?) GetObject()
        {
            var tjState = ContentStreamEnumerator.GetState();

            // Find prepending string object.
            if (!ContentStreamEnumerator.MovePrevious())
                return GetObjectFailed();
            var text = ContentStreamEnumerator.Current;
            var textState = ContentStreamEnumerator.GetState();

            // Set isHex and remove the brackets from text.
            bool isHex;
            var firstChar = text?.FirstOrDefault();
            var lastChar = text?.LastOrDefault();
            if (firstChar == null || lastChar == null)
                return GetObjectFailed();
            if (firstChar == '(' && lastChar == ')')
                isHex = false;
            else if (firstChar == '<' && lastChar == '>')
                isHex = true;
            else
                return GetObjectFailed();
            text = text!.Substring(1, text.Length - 2);

            // Load the offsets and calculate the coordinates.
            if (!GetCurrentCoordinates(out var x, out var y, out var xOffset, out var yOffset,
                    out var tdXState, out var tdYState, out var tdState))
                return GetObjectFailed();

            // Move back to the identifying Tj element.
            ContentStreamEnumerator.SetState(tjState);

            var textInfo = new TextInfo(x, y, xOffset, yOffset, text, isHex,
                tdXState!, tdYState!, tdState!, textState, tjState,
                ContentStreamEnumerator);
            return (true, textInfo);
        }

        bool GetCurrentCoordinates(out double x, out double y, out double xOffset, out double yOffset,
            out ContentStreamEnumerator.IState? tdXState, out ContentStreamEnumerator.IState? tdYState, out ContentStreamEnumerator.IState? tdState)
        {
            x = 0;
            y = 0;

            xOffset = 0;
            yOffset = 0;

            tdXState = null;
            tdYState = null;
            tdState = null;

            // Find the previous Td in Text object.
            if (!MoveToPreviousTdInTextObject())
                return false;
            tdState = ContentStreamEnumerator.GetState();

            // Get the offsets of this element and add it to x and y.
            if (!GetCurrentOffset(out xOffset, out yOffset, out tdXState, out tdYState))
                return false;

            x += xOffset;
            y += yOffset;

            // Find all preceding Td’s of Text object and calculate coordinates.
            while (MoveToPreviousTdInTextObject())
            {
                if (!GetCurrentOffset(out var xOffset2, out var yOffset2, out _, out _))
                    return false;

                x += xOffset2;
                y += yOffset2;
            }

            return true;
        }

        bool GetCurrentOffset(out double xOffset, out double yOffset,
            out ContentStreamEnumerator.IState? tdXState, out ContentStreamEnumerator.IState? tdYState)
        {
            xOffset = 0;
            yOffset = 0;

            tdXState = null;
            tdYState = null;

            // Find prepending y offset.
            if (!ContentStreamEnumerator.MovePrevious())
                return false;
            var yOffsetStr = ContentStreamEnumerator.Current;
            if (!PdfFileHelper.TryParseDouble(yOffsetStr, out yOffset))
                return false;
            tdXState = ContentStreamEnumerator.GetState();

            // Find prepending x offset.
            if (!ContentStreamEnumerator.MovePrevious())
                return false;
            var xOffsetStr = ContentStreamEnumerator.Current;
            if (!PdfFileHelper.TryParseDouble(xOffsetStr, out xOffset))
                return false;
            tdYState = ContentStreamEnumerator.GetState();

            return true;
        }

        bool MoveToPreviousTdInTextObject()
        {
            while (true)
            {
                if (!ContentStreamEnumerator.MovePrevious())
                    return false;

                // TD’s before BT belong to another text object and their offsets don’t mind. 
                if (ContentStreamEnumerator.Current == "BT")
                    return false;

                if (ContentStreamEnumerator.Current == "Td")
                    return true;
            }
        }

        /// <summary>
        /// Contains the information for the found text.
        /// </summary>
        public class TextInfo
        {
            /// <summary>
            /// The calculated X position.
            /// </summary>
            public double X { get; }

            /// <summary>
            /// The calculated Y position.
            /// </summary>
            public double Y { get; }

            /// <summary>
            /// The given X offset.
            /// </summary>
            public double XOffset { get; }

            /// <summary>
            /// The given Y offset.
            /// </summary>
            public double YOffset { get; }

            /// <summary>
            /// Contains the text of the text element as it was written to the content stream.
            /// The used encoding (ansi, unicode, hex) is not considered.
            /// </summary>
            public string Text { get; }

            public bool IsHex { get; }

            public ContentStreamEnumerator.IState TdXElement { get; }

            public ContentStreamEnumerator.IState TdYElement { get; }

            public ContentStreamEnumerator.IState TdElement { get; }

            public ContentStreamEnumerator.IState TextElement { get; }

            public ContentStreamEnumerator.IState TjElement { get; }

            const double ComparisonPrecision = 0.01;

            public TextInfo(double x, double y, double xOffset, double yOffset, string text, bool isHex,
                ContentStreamEnumerator.IState tdXElement, ContentStreamEnumerator.IState tdYElement, ContentStreamEnumerator.IState tdElement,
                ContentStreamEnumerator.IState textElement, ContentStreamEnumerator.IState tjElement,
                ContentStreamEnumerator contentStreamEnumerator)
            {
                X = x;
                Y = y;
                XOffset = xOffset;
                YOffset = yOffset;
                Text = text;
                IsHex = isHex;

                TdXElement = tdXElement;
                TdYElement = tdYElement;
                TdElement = tdElement;

                TextElement = textElement;
                TjElement = tjElement;

                _contentStreamEnumerator = contentStreamEnumerator;
            }

            public bool IsAtXPosition(double expectedX)
            {
                return PositionEquals(X, expectedX);
            }

            public bool IsAtYPosition(double expectedY)
            {
                return PositionEquals(Y, expectedY);
            }

            public bool IsAtPosition(double expectedX, double expectedY)
            {
                return IsAtXPosition(expectedX) && IsAtYPosition(expectedY);
            }

            bool PositionEquals(double value, double expected)
            {
                return Math.Abs(value - expected) <= ComparisonPrecision;
            }

            /// <summary>
            /// Gets text as it would be encoded (ansi, unicode, hex) in the contentStream when using font
            /// and checks, if it is equal to the actually saved Text.
            /// </summary>
            public bool TextEquals(string text, XFont font)
            {
                return TextEquals(text, font, out _);
            }

            /// <summary>
            /// Gets text as it would be encoded (ansi, unicode, hex) in the contentStream when using font
            /// and checks, if it is equal to the actually saved Text.
            /// </summary>
            public bool TextEquals(string text, XFont font, out string encodedText)
            {
                encodedText = GetTextWithCorrectEncoding(text, font);
                return Text == encodedText;
            }

            /// <summary>
            /// Gets text as it would be encoded (ansi, unicode, hex) in the contentStream when using font.
            /// </summary>
            public string GetTextWithCorrectEncoding(string text, XFont font)
            {
                // This code should be analog to XGraphicsPdfRenderer.DrawString().

                var pdfDocument = _contentStreamEnumerator.PdfDocument;

                // Invoke TextEvent.
                var args2 = new PrepareTextEventArgs(pdfDocument, font, text);
                pdfDocument.RenderEvents.OnPrepareTextEvent(this, args2);
                text = args2.Text;

                var codePoints = font.IsSymbolFont
                    ? UnicodeHelper.SymbolCodePointsFromString(text, font.OpenTypeDescriptor)
                    : UnicodeHelper.Utf32FromString(text /*, font.AnsiEncoding*/);
                var otDescriptor = font.OpenTypeDescriptor;
                var codePointsWithGlyphIndices = otDescriptor.GlyphIndicesFromCodePoints(codePoints);

                // Invoke RenderEvent.
                var args = new RenderTextEventArgs(pdfDocument, font, codePointsWithGlyphIndices);

                pdfDocument.RenderEvents.OnRenderTextEvent(this, args);
                codePointsWithGlyphIndices = args.CodePointGlyphIndexPairs;
                if (args.ReevaluateGlyphIndices)
                {
                    codePoints = args.CodePointGlyphIndexPairs.Select(x => x.CodePoint).ToArray();
                    codePointsWithGlyphIndices = otDescriptor.GlyphIndicesFromCodePoints(codePoints);
                }

                bool isAnsi;
                if (font.AutoEncoding)
                {
                    // Can we use WinAnsi encoding?
                    isAnsi = AnsiEncoding.IsAnsi(codePoints);
                }
                else
                {
                    var fontType = font.FontTypeFromUnicodeFlag;
                    isAnsi = fontType == FontType.TrueTypeWinAnsi;
                }

                if (isAnsi)
                {
                    // Use ANSI character encoding.
                    var length = codePoints.Length;
                    byte[] bytes = new byte[length];
                    for (int idx = 0; idx < length; idx++)
                    {
                        ref var item = ref codePoints[idx];
                        //Debug.Assert(item.Character == item.Codepoint);
                        var ch = AnsiEncoding.UnicodeToAnsi((char)item);
                        bytes[idx] = (byte)ch;
                    }
                    //bytes = PdfEncoders.WinAnsiEncoding.GetBytes(s);
                    text = PdfEncoders.ToStringLiteral(bytes, false, null);
                }
                else
                {
                    // Use Unicode glyph encoding.
                    int length = codePointsWithGlyphIndices.Length;
                    var bytes = new byte[2 * length];
                    for (int idx = 0; idx < length; idx++)
                    {
                        ref var item = ref codePointsWithGlyphIndices[idx];
                        bytes[idx * 2] = (byte)((item.GlyphIndex & 0xFF00) >>> 8);
                        bytes[idx * 2 + 1] = (byte)(item.GlyphIndex & 0xFF);
                    }

                    text = PdfEncoders.ToHexStringLiteral(bytes, true, false, null);
                }

                // Remove the brackets.
                if (text.Length >= 2)
                {
                    var firstChar = text[0];
                    var lastChar = text[^1];
                    if (firstChar == '(' && lastChar == ')' || firstChar == '<' && lastChar == '>')
                        text = text[1..^1];
                }
                return text;
            }

            readonly ContentStreamEnumerator _contentStreamEnumerator;
        }
    }
}
