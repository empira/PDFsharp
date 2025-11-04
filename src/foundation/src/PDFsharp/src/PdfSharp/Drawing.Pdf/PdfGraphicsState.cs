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
    sealed class PdfGraphicsState(XGraphicsPdfRenderer renderer) : ICloneable
    {
        public PdfGraphicsState Clone()
        {
            var state = (PdfGraphicsState)MemberwiseClone();
            return state;
        }

        object ICloneable.Clone() => Clone();

        internal int Level;

        internal InternalGraphicsState InternalState = default!;

        public void PushState()
        {
            // BeginGraphic
            renderer.Append("q/n");
        }

        public void PopState()
        {
            //BeginGraphic
            renderer.Append("Q/n");
        }

        #region Stroke

        double _realizedLineWith = -1;
        int _realizedLineCap = -1;
        int _realizedLineJoin = -1;
        double _realizedMiterLimit = -1;
        XDashStyle _realizedDashStyle = (XDashStyle)(-1);
        string _realizedDashPattern = default!;
        XColor _realizedStrokeColor = XColor.Empty;
        bool _realizedStrokeOverPrint;

        public void RealizePen(XPen pen, PdfColorMode colorMode)
        {
            const string format = Config.SignificantDecimalPlaces3;
            const string format2 = Config.SignificantDecimalPlaces2;

            XColor color = pen.Color;
            bool overPrint = pen.Overprint;
            color = ColorSpaceHelper.EnsureColorMode(colorMode, color);

            if (_realizedLineWith != pen.Width)
            {
                renderer.AppendFormatArgs("{0:" + format + "} w\n", pen.Width);
                _realizedLineWith = pen.Width;
            }

            if (_realizedLineCap != (int)pen.LineCap)
            {
                renderer.AppendFormatArgs("{0} J\n", (int)pen.LineCap);
                _realizedLineCap = (int)pen.LineCap;
            }

            if (_realizedLineJoin != (int)pen.LineJoin)
            {
                renderer.AppendFormatArgs("{0} j\n", (int)pen.LineJoin);
                _realizedLineJoin = (int)pen.LineJoin;
            }

            if (_realizedLineCap == (int)XLineJoin.Miter)
            {
                if (_realizedMiterLimit != (int)pen.MiterLimit && (int)pen.MiterLimit != 0)
                {
                    renderer.AppendFormatInt("{0} M\n", (int)pen.MiterLimit);
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
                        renderer.Append("[]0 d\n");
                        break;

                    case XDashStyle.Dash:
                        renderer.AppendFormatArgs("[{0:" + format2 + "} {1:" + format2 + "}]0 d\n", dash, dot);
                        break;

                    case XDashStyle.Dot:
                        renderer.AppendFormatArgs("[{0:" + format2 + "}]0 d\n", dot);
                        break;

                    case XDashStyle.DashDot:
                        renderer.AppendFormatArgs("[{0:" + format2 + "} {1:" + format2 + "} {1:" + format2 + "} {1:" + format2 + "}]0 d\n", dash, dot);
                        break;

                    case XDashStyle.DashDotDot:
                        renderer.AppendFormatArgs("[{0:" + format2 + "} {1:" + format2 + "} {1:" + format2 + "} {1:" + format2 + "} {1:" + format2 + "} {1:" + format2 + "}]0 d\n", dash, dot);
                        break;

                    case XDashStyle.Custom:
                        {
                            var pdf = new StringBuilder("[", 256);
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

                            // IMPROVE
                            // drice2@ageone.de reported a realizing problem.
                            // So we now always render the pattern.
                            //if (_realizedDashPattern != pattern)
                            {
                                _realizedDashPattern = pattern;
                                renderer.Append(pattern);
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
                    renderer.Append(PdfEncoders.ToString(color, PdfColorMode.Rgb));
                    renderer.Append(" RG\n");
                }
            }
            else
            {
                if (!ColorSpaceHelper.IsEqualCmyk(_realizedStrokeColor, color))
                {
                    renderer.Append(PdfEncoders.ToString(color, PdfColorMode.Cmyk));
                    renderer.Append(" K\n");
                }
            }

            if (renderer.Owner.Version >= 14 && (_realizedStrokeColor.A != color.A || _realizedStrokeOverPrint != overPrint))
            {
                PdfExtGState extGState = renderer.Owner.ExtGStateTable.GetExtGStateStroke(color.A, overPrint);
                string gs = renderer.Resources.AddExtGState(extGState);
                renderer.AppendFormatString("{0} gs\n", gs);

                // Must create transparency group.
                if (renderer._page != null! && color.A < 1)
                    renderer._page.TransparencyUsed = true;
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
                
                Debug.Assert(UnrealizedCtm.IsIdentity, "Must realize ctm first.");
                var matrix = renderer.DefaultViewMatrix;
                matrix.Prepend(EffectiveCtm);
                var pattern = new PdfShadingPattern(renderer.Owner);
                
                switch (brush)
                {
                    case XLinearGradientBrush gradientBrush:
                        pattern.SetupFromBrush(gradientBrush, matrix, renderer);
                        break;
                    case XRadialGradientBrush radialGradient:
                        pattern.SetupFromBrush(radialGradient, matrix, renderer);
                        break;
                }
                
                var name = renderer.Resources.AddPattern(pattern);
                renderer.AppendFormatString("/Pattern cs\n", name);
                renderer.AppendFormatString("{0} scn\n", name);

                // Invalidate fill color.
                _realizedFillColor = XColor.Empty;
            }
        }

        void RealizeFillColor(XColor color, bool overPrint, PdfColorMode colorMode)
        {
            color = ColorSpaceHelper.EnsureColorMode(colorMode, color);

            if (colorMode != PdfColorMode.Cmyk)
            {
                if (_realizedFillColor.IsEmpty || _realizedFillColor.Rgb != color.Rgb)
                {
                    renderer.Append(PdfEncoders.ToString(color, PdfColorMode.Rgb));
                    renderer.Append(" rg\n");
                }
            }
            else
            {
                Debug.Assert(colorMode == PdfColorMode.Cmyk);

                if (_realizedFillColor.IsEmpty || !ColorSpaceHelper.IsEqualCmyk(_realizedFillColor, color))
                {
                    renderer.Append(PdfEncoders.ToString(color, PdfColorMode.Cmyk));
                    renderer.Append(" k\n");
                }
            }

            if (renderer.Owner.Version >= 14 && (_realizedFillColor.A != color.A || _realizedNonStrokeOverPrint != overPrint))
            {

                PdfExtGState extGState = renderer.Owner.ExtGStateTable.GetExtGStateNonStroke(color.A, overPrint);
                string gs = renderer.Resources.AddExtGState(extGState);
                renderer.AppendFormatString("{0} gs\n", gs);

                // Must create transparency group.
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (renderer._page != null && color.A < 1)
                    renderer._page.TransparencyUsed = true;
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
        /*internal*/
        PdfFont? _realizedFont;
        string _realizedFontName = "";
        double _realizedFontSize;
        int _realizedRenderingMode; // Reference: TABLE 5.2  Text state operators / Page 398
        double _realizedCharSpace;  // Reference: TABLE 5.2  Text state operators / Page 398

        public void RealizeFont(XGlyphTypeface glyphTypeface, double emSize, XBrush brush, int renderingMode, FontType fontType)
        {
            const string format = Config.SignificantDecimalPlaces3;

            // So far rendering mode 0 (fill text) and 2 (fill, then stroke text) only.
            RealizeBrush(brush, renderer._colorMode, renderingMode, emSize); // _renderer.page.document.Options.ColorMode);

            // Realize rendering mode.
            if (_realizedRenderingMode != renderingMode)
            {
                renderer.AppendFormatInt("{0} Tr\n", renderingMode);
                _realizedRenderingMode = renderingMode;
            }

            // Realize character spacing.
            if (_realizedRenderingMode == 0)
            {
                if (_realizedCharSpace != 0)
                {
                    renderer.Append("0 Tc\n");
                    _realizedCharSpace = 0;
                }
            }
            else  // _realizedRenderingMode is 2.
            {
                double charSpace = emSize * Const.BoldEmphasis;
                if (_realizedCharSpace != charSpace)
                {
                    renderer.AppendFormatDouble("{0:" + format + "} Tc\n", charSpace);
                    _realizedCharSpace = charSpace;
                }
            }

            _realizedFont = null;
            string fontName = renderer.GetFontName(glyphTypeface, fontType, out _realizedFont);
            if (fontName != _realizedFontName || _realizedFontSize != emSize)
            {
                s_formatTf ??= "{0} {1:" + format + "} Tf\n";
                if (renderer.Gfx.PageDirection == XPageDirection.Downwards)
                {
                    // earlier:
                    // renderer.AppendFormatFont("{0} {1:" + format + "} Tf\n", fontName, emSize);
                    renderer.AppendFormatFont(s_formatTf, fontName, emSize);
                }
                else
                {
                    // earlier:
                    // renderer.AppendFormatFont("{0} {1:" + format + "} Tf\n", fontName, emSize);
                    renderer.AppendFormatFont(s_formatTf, fontName, emSize);
                }
                _realizedFontName = fontName;
                _realizedFontSize = emSize;
            }
        }
        // ReSharper disable InconsistentNaming
        static string? s_formatTf;
        // ReSharper restore InconsistentNaming

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
            if (matrixOrder == XMatrixOrder.Append)
                throw new NotImplementedException("XMatrixOrder.Append");

            XMatrix transform = value;
            if (renderer.Gfx.PageDirection == XPageDirection.Downwards)
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

                const string format = Config.SignificantDecimalPlaces7;
                s_formatCtm ??= "{0:" + format + "} {1:" + format + "} {2:" + format + "} {3:" + format + "} {4:"
                                + format + "} {5:" + format + "} cm\n";

                double[] matrix = UnrealizedCtm.GetElements();
                // Use up to six decimal digits to prevent round up problems.
                // earlier:
                // renderer.AppendFormatArgs("{0:" + format + "} {1:" + format + "} {2:" + format + "} {3:" + format + "} {4:" + format + "} {5:" + format + "} cm\n",
                //     matrix[0], matrix[1], matrix[2], matrix[3], matrix[4], matrix[5]);
                renderer.AppendFormatArgs(s_formatCtm, matrix[0], matrix[1], matrix[2], matrix[3], matrix[4], matrix[5]);

                RealizedCtm.Prepend(UnrealizedCtm);
                UnrealizedCtm = new XMatrix();
                EffectiveCtm = RealizedCtm;
                InverseEffectiveCtm = EffectiveCtm;
                InverseEffectiveCtm.Invert();
            }
        }
        // ReSharper disable InconsistentNaming
        static string? s_formatCtm;
        // ReSharper restore InconsistentNaming
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
#if CORE_  // It should work in Core build.
            DiagnosticsHelper.HandleNotImplemented("RealizeClipPath");
#endif
#if GDI
            // Do not render an empty path.
            if (clipPath.GdipPath.PointCount < 0)
                return;
#endif
#if WPF
            // Do not render an empty path.
            if (clipPath.PathGeometry.Bounds.IsEmpty)
                return;
#endif
            renderer.BeginGraphicMode();
            RealizeCtm();
#if CORE
            renderer.AppendPath(clipPath.CorePath);
#endif
#if GDI && !WPF
            renderer.AppendPath(clipPath.GdipPath);
#endif
#if WPF && !GDI
            renderer.AppendPath(clipPath.PathGeometry);
#endif
#if WPF && GDI
            if (renderer.Gfx.TargetContext == XGraphicTargetContext.GDI)
                renderer.AppendPath(clipPath._gdipPath);
            else
                renderer.AppendPath(clipPath._pathGeometry);
#endif
            renderer.Append(clipPath.FillMode == XFillMode.Winding ? "W n\n" : "W* n\n");
        }

        #endregion
    }
}
