// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing.BarCodes
{
    /// <summary>
    /// Implementation of the Code 2 of 5 bar code.
    /// </summary>
    public class Code2of5Interleaved : ThickThinBarCode
    {
        /// <summary>
        /// Initializes a new instance of Interleaved2of5.
        /// </summary>
        public Code2of5Interleaved()
            : base("", XSize.Empty, CodeDirection.LeftToRight)
        { }

        /// <summary>
        /// Initializes a new instance of Interleaved2of5.
        /// </summary>
        public Code2of5Interleaved(string code)
            : base(code, XSize.Empty, CodeDirection.LeftToRight)
        { }

        /// <summary>
        /// Initializes a new instance of Interleaved2of5.
        /// </summary>
        public Code2of5Interleaved(string code, XSize size)
            : base(code, size, CodeDirection.LeftToRight)
        { }

        /// <summary>
        /// Initializes a new instance of Interleaved2of5.
        /// </summary>
        public Code2of5Interleaved(string code, XSize size, CodeDirection direction)
            : base(code, size, direction)
        { }

        /// <summary>
        /// Returns an array of size 5 that represents the thick (true) and thin (false) lines or spaces
        /// representing the specified digit.
        /// </summary>
        /// <param name="digit">The digit to represent.</param>
        static bool[] ThickAndThinLines(int digit)
        {
            return Lines[digit];
        }

        static readonly bool[][] Lines =
        {
            new [] {false, false, true, true, false},
            new [] {true, false, false, false, true},
            new [] {false, true, false, false, true},
            new [] {true, true, false, false, false},
            new [] {false, false, true, false, true},
            new [] {true, false, true, false, false},
            new [] {false, true, true, false, false},
            new [] {false, false, false, true, true},
            new [] {true, false, false, true, false},
            new [] {false, true, false, true, false},
        };

        /// <summary>
        /// Renders the bar code.
        /// </summary>
        protected internal override void Render(XGraphics gfx, XBrush brush, XFont? font, XPoint position)
        {
            XGraphicsState state = gfx.Save();

            BarCodeRenderInfo info = new BarCodeRenderInfo(gfx, brush, font, position);
            InitRendering(info);
            info.CurrPosInString = 0;
            //info.CurrPos = info.Center - Size / 2;
            info.CurrPos = position - CodeBase.CalcDistance(AnchorType.TopLeft, Anchor, Size);

            if (TurboBit)
                RenderTurboBit(info, true);
            RenderStart(info);
            while (info.CurrPosInString < Text.Length)
                RenderNextPair(info);
            RenderStop(info);
            if (TurboBit)
                RenderTurboBit(info, false);
            if (TextLocation != TextLocation.None)
                RenderText(info);

            gfx.Restore(state);
        }

        /// <summary>
        /// Calculates the thick and thin line widths,
        /// taking into account the required rendering size.
        /// </summary>
        internal override void CalcThinBarWidth(BarCodeRenderInfo info)
        {
            /*
             * The total width is the sum of the following parts:
             * Starting lines      = 4 * thin
             *  +
             * Code Representation = (2 * thick + 3 * thin) * code.Length
             *  +
             * Stopping lines      =  1 * thick + 2 * thin
             * 
             * with r = relation ( = thick / thin), this results in
             * 
             * Total width = (6 + r + (2 * r + 3) * text.Length) * thin
             */
            double thinLineAmount = 6 + WideNarrowRatio + (2 * WideNarrowRatio + 3) * Text.Length;
            info.ThinBarWidth = Size.Width / thinLineAmount;
        }

        void RenderStart(BarCodeRenderInfo info)
        {
            RenderBar(info, false);
            RenderGap(info, false);
            RenderBar(info, false);
            RenderGap(info, false);
        }

        void RenderStop(BarCodeRenderInfo info)
        {
            RenderBar(info, true);
            RenderGap(info, false);
            RenderBar(info, false);
        }

        /// <summary>
        /// Renders the next digit pair as bar code element.
        /// </summary>
        void RenderNextPair(BarCodeRenderInfo info)
        {
            int digitForLines = Int32.Parse(Text[info.CurrPosInString].ToString());
            int digitForGaps = Int32.Parse(Text[info.CurrPosInString + 1].ToString());
            bool[] linesArray = Lines[digitForLines];
            bool[] gapsArray = Lines[digitForGaps];
            for (int idx = 0; idx < 5; ++idx)
            {
                RenderBar(info, linesArray[idx]);
                RenderGap(info, gapsArray[idx]);
            }
            info.CurrPosInString += 2;
        }

        /// <summary>
        /// Checks the code to be convertible into an interleaved 2 of 5 bar code.
        /// </summary>
        /// <param name="text">The code to be checked.</param>
        protected override void CheckCode(string text)
        {
#if true_
            if (text == null)
                throw new ArgumentNullException("text");

            if (text == "")
                throw new ArgumentException(BcgSR.Invalid2Of5Code(text));

            if (text.Length % 2 != 0)
                throw new ArgumentException(BcgSR.Invalid2Of5Code(text));

            foreach (char ch in text)
            {
                if (!Char.IsDigit(ch))
                    throw new ArgumentException(BcgSR.Invalid2Of5Code(text));
            }
#endif
        }
    }
}
