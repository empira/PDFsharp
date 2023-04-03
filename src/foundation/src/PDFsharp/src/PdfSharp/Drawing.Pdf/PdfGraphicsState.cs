// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
#endif
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace PdfSharp.Drawing.Pdf
{
    /// <summary>
    /// Represents the current PDF graphics state.
    /// </summary>
    /// <remarks>
    /// Completely revised for PDFsharp 1.4.
    /// </remarks>
    sealed class PdfGraphicsState : ICloneable
    {
        public PdfGraphicsState(XGraphicsPdfRenderer renderer)
        {
            _renderer = renderer;
        }

        readonly XGraphicsPdfRenderer _renderer;

        public PdfGraphicsState Clone()
        {
            var state = (PdfGraphicsState)MemberwiseClone();
            return state;
        }

        object ICloneable.Clone()
            => Clone();

        internal int Level;

        internal InternalGraphicsState InternalState = null!; // NRT

        public void PushState()
        {
            // BeginGraphic
            _renderer.Append("q/n");
        }

        public void PopState()
        {
            //BeginGraphic
            _renderer.Append("Q/n");
        }

        #region Stroke

        double _realizedLineWith = -1;
        int _realizedLineCap = -1;
        int _realizedLineJoin = -1;
        double _realizedMiterLimit = -1;
        XDashStyle _realizedDashStyle = (XDashStyle)(-1);
        string _realizedDashPattern = null!; // NRT
        XColor _realizedStrokeColor = XColor.Empty;
        bool _realizedStrokeOverPrint;

        public void RealizePen(XPen pen, PdfColorMode colorMode)
        {
            const string frmt2 = Config.SignificantFigures2;
            const string format = Config.SignificantFigures3;
            XColor color = pen.Color;
            bool overPrint = pen.Overprint;
            color = ColorSpaceHelper.EnsureColorMode(colorMode, color);

            if (_realizedLineWith != pen.Width)
            {
                _renderer.AppendFormatArgs("{0:" + format + "} w\n", pen.Width);
                _realizedLineWith = pen.Width;
            }

            if (_realizedLineCap != (int)pen.LineCap)
            {
                _renderer.AppendFormatArgs("{0} J\n", (int)pen.LineCap);
                _realizedLineCap = (int)pen.LineCap;
            }

            if (_realizedLineJoin != (int)pen.LineJoin)
            {
                _renderer.AppendFormatArgs("{0} j\n", (int)pen.LineJoin);
                _realizedLineJoin = (int)pen.LineJoin;
            }

            if (_realizedLineCap == (int)XLineJoin.Miter)
            {
                if (_realizedMiterLimit != (int)pen.MiterLimit && (int)pen.MiterLimit != 0)
                {
                    _renderer.AppendFormatInt("{0} M\n", (int)pen.MiterLimit);
                    _realizedMiterLimit = (int)pen.MiterLimit;
                }
            }

            if (_realizedDashStyle != pen.DashStyle || pen.DashStyle == XDashStyle.Custom)
            {
                double dot = pen.Width;
                double dash = 3 * dot;

                // Line width 0 is not recommended but valid.
                XDashStyle dashStyle = pen.DashStyle;
                if (dot == 0)
                    dashStyle = XDashStyle.Solid;

                switch (dashStyle)
                {
                    case XDashStyle.Solid:
                        _renderer.Append("[]0 d\n");
                        break;

                    case XDashStyle.Dash:
                        _renderer.AppendFormatArgs("[{0:" + frmt2 + "} {1:" + frmt2 + "}]0 d\n", dash, dot);
                        break;

                    case XDashStyle.Dot:
                        _renderer.AppendFormatArgs("[{0:" + frmt2 + "}]0 d\n", dot);
                        break;

                    case XDashStyle.DashDot:
                        _renderer.AppendFormatArgs("[{0:" + frmt2 + "} {1:" + frmt2 + "} {1:" + frmt2 + "} {1:" + frmt2 + "}]0 d\n", dash, dot);
                        break;

                    case XDashStyle.DashDotDot:
                        _renderer.AppendFormatArgs("[{0:" + frmt2 + "} {1:" + frmt2 + "} {1:" + frmt2 + "} {1:" + frmt2 + "} {1:" + frmt2 + "} {1:" + frmt2 + "}]0 d\n", dash, dot);
                        break;

                    case XDashStyle.Custom:
                        {
                            StringBuilder pdf = new StringBuilder("[", 256);
                            int len = pen._dashPattern == null ? 0 : pen.DashPattern.Length;
                            for (int idx = 0; idx < len; idx++)
                            {
                                if (idx > 0)
                                    pdf.Append(' ');
                                pdf.Append(PdfEncoders.ToString(pen.DashPattern[idx] * pen.Width));
                            }
                            // Make an even number of values look like in GDI+
                            if (len > 0 && len % 2 == 1)
                            {
                                pdf.Append(' ');
                                pdf.Append(PdfEncoders.ToString(0.2 * pen.Width));
                            }
                            pdf.AppendFormat(CultureInfo.InvariantCulture, "]{0:" + format + "} d\n", pen.DashOffset * pen.Width);
                            string pattern = pdf.ToString();

                            // BUG: drice2@ageone.de reported a realizing problem
                            // HACK: I remove the if clause
                            //if (_realizedDashPattern != pattern)
                            {
                                _realizedDashPattern = pattern;
                                _renderer.Append(pattern);
                            }
                        }
                        break;
                }
                _realizedDashStyle = dashStyle;
            }

            if (colorMode != PdfColorMode.Cmyk)
            {
                if (_realizedStrokeColor.Rgb != color.Rgb)
                {
                    _renderer.Append(PdfEncoders.ToString(color, PdfColorMode.Rgb));
                    _renderer.Append(" RG\n");
                }
            }
            else
            {
                if (!ColorSpaceHelper.IsEqualCmyk(_realizedStrokeColor, color))
                {
                    _renderer.Append(PdfEncoders.ToString(color, PdfColorMode.Cmyk));
                    _renderer.Append(" K\n");
                }
            }

            if (_renderer.Owner.Version >= 14 && (_realizedStrokeColor.A != color.A || _realizedStrokeOverPrint != overPrint))
            {
                PdfExtGState extGState = _renderer.Owner.ExtGStateTable.GetExtGStateStroke(color.A, overPrint);
                string gs = _renderer.Resources.AddExtGState(extGState);
                _renderer.AppendFormatString("{0} gs\n", gs);

                // Must create transparency group.
                if (_renderer._page != null && color.A < 1)
                    _renderer._page.TransparencyUsed = true;
            }
            _realizedStrokeColor = color;
            _realizedStrokeOverPrint = overPrint;
        }

        #endregion

        #region Fill

        XColor _realizedFillColor = XColor.Empty;
        bool _realizedNonStrokeOverPrint;

        public void RealizeBrush(XBrush brush, PdfColorMode colorMode, int renderingMode, double fontEmSize)
        {
            // Rendering mode 2 is used for bold simulation.
            // Reference: TABLE 5.3  Text rendering modes / Page 402

            if (brush is XSolidBrush solidBrush)
            {
                XColor color = solidBrush.Color;
                bool overPrint = solidBrush.Overprint;

                if (renderingMode == 0)
                {
                    RealizeFillColor(color, overPrint, colorMode);
                }
                else if (renderingMode == 2)
                {
                    // Come here in case of bold simulation.
                    RealizeFillColor(color, false, colorMode);
                    //color = XColors.Green;
                    RealizePen(new XPen(color, fontEmSize * Const.BoldEmphasis), colorMode);
                }
                else
                    throw new InvalidOperationException("Only rendering modes 0 and 2 are currently supported.");
            }
            else
            {
                if (renderingMode != 0)
                    throw new InvalidOperationException("Rendering modes other than 0 can only be used with solid color brushes.");

                if (brush is XLinearGradientBrush gradientBrush)
                {
                    Debug.Assert(UnrealizedCtm.IsIdentity, "Must realize ctm first.");
                    XMatrix matrix = _renderer.DefaultViewMatrix;
                    matrix.Prepend(EffectiveCtm);
                    PdfShadingPattern pattern = new PdfShadingPattern(_renderer.Owner);
                    pattern.SetupFromBrush(gradientBrush, matrix, _renderer);
                    string name = _renderer.Resources.AddPattern(pattern);
                    _renderer.AppendFormatString("/Pattern cs\n", name);
                    _renderer.AppendFormatString("{0} scn\n", name);

                    // Invalidate fill color.
                    _realizedFillColor = XColor.Empty;
                }
            }
        }

        void RealizeFillColor(XColor color, bool overPrint, PdfColorMode colorMode)
        {
            color = ColorSpaceHelper.EnsureColorMode(colorMode, color);

            if (colorMode != PdfColorMode.Cmyk)
            {
                if (_realizedFillColor.IsEmpty || _realizedFillColor.Rgb != color.Rgb)
                {
                    _renderer.Append(PdfEncoders.ToString(color, PdfColorMode.Rgb));
                    _renderer.Append(" rg\n");
                }
            }
            else
            {
                Debug.Assert(colorMode == PdfColorMode.Cmyk);

                if (_realizedFillColor.IsEmpty || !ColorSpaceHelper.IsEqualCmyk(_realizedFillColor, color))
                {
                    _renderer.Append(PdfEncoders.ToString(color, PdfColorMode.Cmyk));
                    _renderer.Append(" k\n");
                }
            }

            if (_renderer.Owner.Version >= 14 && (_realizedFillColor.A != color.A || _realizedNonStrokeOverPrint != overPrint))
            {

                PdfExtGState extGState = _renderer.Owner.ExtGStateTable.GetExtGStateNonStroke(color.A, overPrint);
                string gs = _renderer.Resources.AddExtGState(extGState);
                _renderer.AppendFormatString("{0} gs\n", gs);

                // Must create transparency group.
                if (_renderer._page != null && color.A < 1)
                    _renderer._page.TransparencyUsed = true;
            }
            _realizedFillColor = color;
            _realizedNonStrokeOverPrint = overPrint;
        }

        internal void RealizeNonStrokeTransparency(double transparency, PdfColorMode colorMode)
        {
            XColor color = _realizedFillColor;
            color.A = transparency;
            RealizeFillColor(color, _realizedNonStrokeOverPrint, colorMode);
        }

        #endregion

        #region Text

        internal PdfFont? RealizedFont => _realizedFont;
        /*internal*/ PdfFont? _realizedFont;
        string _realizedFontName = "";
        double _realizedFontSize;
        int _realizedRenderingMode;  // Reference: TABLE 5.2  Text state operators / Page 398
        double _realizedCharSpace;  // Reference: TABLE 5.2  Text state operators / Page 398

        public void RealizeFont(XFont font, XBrush brush, int renderingMode)
        {
            const string format = Config.SignificantFigures3;

            // So far rendering mode 0 (fill text) and 2 (fill, then stroke text) only.
            RealizeBrush(brush, _renderer._colorMode, renderingMode, font.Size); // _renderer.page.document.Options.ColorMode);

            // Realize rendering mode.
            if (_realizedRenderingMode != renderingMode)
            {
                _renderer.AppendFormatInt("{0} Tr\n", renderingMode);
                _realizedRenderingMode = renderingMode;
            }

            // Realize character spacing.
            if (_realizedRenderingMode == 0)
            {
                if (_realizedCharSpace != 0)
                {
                    _renderer.Append("0 Tc\n");
                    _realizedCharSpace = 0;
                }
            }
            else  // _realizedRenderingMode is 2.
            {
                double charSpace = font.Size * Const.BoldEmphasis;
                if (_realizedCharSpace != charSpace)
                {
                    _renderer.AppendFormatDouble("{0:" + format + "} Tc\n", charSpace);
                    _realizedCharSpace = charSpace;
                }
            }

            _realizedFont = null;
            string fontName = _renderer.GetFontName(font, out _realizedFont);
            if (fontName != _realizedFontName || _realizedFontSize != font.Size)
            {
                if (_renderer.Gfx.PageDirection == XPageDirection.Downwards)
                    _renderer.AppendFormatFont("{0} {1:" + format + "} Tf\n", fontName, font.Size);
                else
                    _renderer.AppendFormatFont("{0} {1:" + format + "} Tf\n", fontName, font.Size);
                _realizedFontName = fontName;
                _realizedFontSize = font.Size;
            }
        }

        public XPoint RealizedTextPosition;

        /// <summary>
        /// Indicates that the text transformation matrix currently skews 20° to the right.
        /// </summary>
        public bool ItalicSimulationOn;

        #endregion

        #region Transformation

        /// <summary>
        /// The already realized part of the current transformation matrix.
        /// </summary>
        public XMatrix RealizedCtm;

        /// <summary>
        /// The not yet realized part of the current transformation matrix.
        /// </summary>
        public XMatrix UnrealizedCtm;

        /// <summary>
        /// Product of RealizedCtm and UnrealizedCtm.
        /// </summary>
        public XMatrix EffectiveCtm;

        /// <summary>
        /// Inverse of EffectiveCtm used for transformation.
        /// </summary>
        public XMatrix InverseEffectiveCtm;

        public XMatrix WorldTransform;

        ///// <summary>
        ///// The world transform in PDF world space.
        ///// </summary>
        //public XMatrix EffectiveCtm
        //{
        //  get
        //  {
        //    //if (MustRealizeCtm)
        //    if (!UnrealizedCtm.IsIdentity)
        //    {
        //      XMatrix matrix = RealizedCtm;
        //      matrix.Prepend(UnrealizedCtm);
        //      return matrix;
        //    }
        //    return RealizedCtm;
        //  }
        //  //set
        //  //{
        //  //  XMatrix matrix = realizedCtm;
        //  //  matrix.Invert();
        //  //  matrix.Prepend(value);
        //  //  unrealizedCtm = matrix;
        //  //  MustRealizeCtm = !unrealizedCtm.IsIdentity;
        //  //}
        //}

        public void AddTransform(XMatrix value, XMatrixOrder matrixOrder)
        {
            // TODO: User matrixOrder
#if DEBUG
            if (matrixOrder == XMatrixOrder.Append)
                throw new NotImplementedException("XMatrixOrder.Append");
#endif
            XMatrix transform = value;
            if (_renderer.Gfx.PageDirection == XPageDirection.Downwards)
            {
                // Take chirality into account and
                // invert the direction of rotation.
                transform.M12 = -value.M12;
                transform.M21 = -value.M21;
            }
            UnrealizedCtm.Prepend(transform);

            WorldTransform.Prepend(value);
        }

        /// <summary>
        /// Realizes the CTM.
        /// </summary>
        public void RealizeCtm()
        {
            //if (MustRealizeCtm)
            if (!UnrealizedCtm.IsIdentity)
            {
                Debug.Assert(!UnrealizedCtm.IsIdentity, "mrCtm is unnecessarily set.");

                const string format = Config.SignificantFigures7;

                double[] matrix = UnrealizedCtm.GetElements();
                // Use up to six decimal digits to prevent round up problems.
                _renderer.AppendFormatArgs("{0:" + format + "} {1:" + format + "} {2:" + format + "} {3:" + format + "} {4:" + format + "} {5:" + format + "} cm\n",
                    matrix[0], matrix[1], matrix[2], matrix[3], matrix[4], matrix[5]);

                RealizedCtm.Prepend(UnrealizedCtm);
                UnrealizedCtm = new XMatrix();
                EffectiveCtm = RealizedCtm;
                InverseEffectiveCtm = EffectiveCtm;
                InverseEffectiveCtm.Invert();
            }
        }
        #endregion

        #region Clip Path

        public void SetAndRealizeClipRect(XRect clipRect)
        {
            var clipPath = new XGraphicsPath();
            clipPath.AddRectangle(clipRect);
            RealizeClipPath(clipPath);
        }

        public void SetAndRealizeClipPath(XGraphicsPath clipPath)
        {
            RealizeClipPath(clipPath);
        }

        void RealizeClipPath(XGraphicsPath clipPath)
        {
#if CORE
            DiagnosticsHelper.HandleNotImplemented("RealizeClipPath");
#endif
#if GDI
            // Do not render an empty path.
            if (clipPath._gdipPath.PointCount < 0)
                return;
#endif
#if WPF
            // Do not render an empty path.
            if (clipPath._pathGeometry.Bounds.IsEmpty)
                return;
#endif
            _renderer.BeginGraphicMode();
            RealizeCtm();
#if CORE
            _renderer.AppendPath(clipPath._corePath);
#endif
#if GDI && !WPF
            _renderer.AppendPath(clipPath._gdipPath);
#endif
#if WPF && !GDI
            _renderer.AppendPath(clipPath._pathGeometry);
#endif
#if WPF && GDI
            if (_renderer.Gfx.TargetContext == XGraphicTargetContext.GDI)
                _renderer.AppendPath(clipPath._gdipPath);
            else
                _renderer.AppendPath(clipPath._pathGeometry);
#endif
            _renderer.Append(clipPath.FillMode == XFillMode.Winding ? "W n\n" : "W* n\n");
        }

        #endregion
    }
}
