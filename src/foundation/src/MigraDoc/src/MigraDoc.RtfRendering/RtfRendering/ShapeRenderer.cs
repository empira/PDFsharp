// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Internals;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;

using Color = MigraDoc.DocumentObjectModel.Color;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Base class for Renderers that render shapes (images, textframes, charts) to RTF.
    /// </summary>
    abstract class ShapeRenderer : RendererBase
    {
        internal ShapeRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _shape = (Shape)domObj;
        }

        /// <summary>
        /// Starts the area for a common shape description in RTF.
        /// </summary>
        protected virtual void StartShapeArea()
        {
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("shp");
            _rtfWriter.StartContent();
            _rtfWriter.WriteControlWithStar("shpinst");
            RenderShapeAttributes();
        }

        /// <summary>
        /// Renders attributes that belong to a shape.
        /// </summary>
        void RenderShapeAttributes()
        {
            RenderTopPosition();
            RenderLeftPosition();

            if (DocumentRelations.HasParentOfType(_shape, typeof(HeaderFooter)))
                _rtfWriter.WriteControl("shpfhdr", "1");
            else
                _rtfWriter.WriteControl("shpfhdr", "0");
            RenderWrapFormat();
            RenderRelativeHorizontal();
            RenderRelativeVertical();
            if (RenderInParagraph())
            {
                _rtfWriter.WriteControl("shplockanchor");
                RenderNameValuePair("fPseudoInline", "1");
            }
            RenderLineFormat();
            RenderFillFormat();
        }

        /// <summary>
        /// Renders the fill format of the shape.
        /// </summary>
        protected void RenderFillFormat()
        {
            var ff = GetValueAsIntended("FillFormat") as FillFormat;
            if (ff != null && (ff.Values.Visible is null || ff.Visible))
            {
                RenderNameValuePair("fFilled", "1");
                TranslateAsNameValuePair("FillFormat.Color", "fillColor", RtfUnit.Undefined, null);
            }
            else
                RenderNameValuePair("fFilled", "0");
        }

        protected Unit GetLineWidth()
        {
            var lf = GetValueAsIntended("LineFormat") as LineFormat;
            if (lf != null && (lf.Values.Visible is null || lf.Visible))
            {
                if (lf.Values.Width.IsValueNullOrEmpty())
                    return 1;
                else
                    return lf.Width;
            }
            return 0;
        }
        /// <summary>
        /// Renders the line format of the shape.
        /// </summary>
        protected void RenderLineFormat()
        {
            var lf = GetValueAsIntended("LineFormat") as LineFormat;
            if (lf != null && (lf.Values.Visible is null || lf.Visible))
            {
                RenderNameValuePair("fLine", "1");
                TranslateAsNameValuePair("LineFormat.Color", "lineColor", RtfUnit.Undefined, "0");
                TranslateAsNameValuePair("LineFormat.Width", "lineWidth", RtfUnit.EMU, ToEmu(1).ToString());
                TranslateAsNameValuePair("LineFormat.DashStyle", "lineDashing", RtfUnit.Undefined, "0");
            }
            else
                RenderNameValuePair("fLine", "0");
        }

        /// <summary>
        /// Renders the shape's Left attribute by setting the \posv, \shptop and \shpbottom RTF controls.
        /// </summary>
        protected void RenderTopPosition()
        {
            RenderTopBottom();
            string topValue = "";
            ShapePosition topShapePosition = _shape.Top.ShapePosition;
            switch (topShapePosition)
            {
                case ShapePosition.Top:
                    //WrapFormat.DistanceTop is used slightly different in the rendering module than in word.
                    //It must be taken into account for the top value.
                    var wrapTop = GetValueAsIntended("WrapFormat.DistanceTop");
                    if (wrapTop == null || ((Unit)wrapTop).Point == 0)
                        topValue = "1";
                    break;

                case ShapePosition.Center:
                    topValue = "2";
                    break;

                case ShapePosition.Bottom:
                    //WrapFormat.DistanceBottom is used slightly different in the rendering module than in word.
                    //It must be taken into account for the bottom value.
                    var wrapBottom = GetValueAsIntended("WrapFormat.DistanceBottom");
                    if (wrapBottom == null || ((Unit)wrapBottom).Point == 0)
                        topValue = "3";
                    break;
            }
            if (topValue != "" && !RenderInParagraph())
                RenderNameValuePair("posv", topValue);
        }

        /// <summary>
        /// Renders the shape's Left attribute by setting the \posh, \shpleft and \shpright RTF controls.
        /// </summary>
        protected void RenderLeftPosition()
        {
            RenderLeftRight();
            ShapePosition leftShapePosition = _shape.Left.ShapePosition;
            string leftValue = "";
            switch (leftShapePosition)
            {
                case ShapePosition.Left:

                    //WrapFormat.DistanceBottom is used slightly different in the rendering module than in word.
                    //It must be taken into account for the left value.
                    var wrapLeft = GetValueAsIntended("WrapFormat.DistanceLeft");
                    if (wrapLeft == null || ((Unit)wrapLeft).Point == 0)
                        leftValue = "1";
                    break;
                case ShapePosition.Center:
                    leftValue = "2";
                    break;
                case ShapePosition.Right:

                    //WrapFormat.DistanceBottom is used slightly different in the rendering module than in word.
                    //It must be taken into account for the right value.
                    var wrapRight = GetValueAsIntended("WrapFormat.DistanceRight");
                    if (wrapRight == null || ((Unit)wrapRight).Point == 0)
                        leftValue = "3";
                    break;
            }
            if (leftValue != "" && !RenderInParagraph())
                RenderNameValuePair("posh", leftValue);
        }

        /// <summary>
        /// Gets the user defined shape height.
        /// </summary>
        protected virtual Unit GetShapeHeight()
        {
            return _shape.Height;
        }

        /// <summary>
        /// Gets the user defined shape width.
        /// </summary>
        protected virtual Unit GetShapeWidth()
        {
            return _shape.Width;
        }

        protected virtual void EndShapeArea()
        {
            _rtfWriter.EndContent();
            _rtfWriter.EndContent();
        }

        /// <summary>
        /// A shape that shall be placed between its predecessor and its successor must be embedded in a paragraph.
        /// </summary>
        protected virtual bool RenderInParagraph()
        {
            if (_shape.Values.RelativeVertical is null || _shape.RelativeVertical == RelativeVertical.Line || _shape.RelativeVertical == RelativeVertical.Paragraph)
            {

                var docObjects = DocumentRelations.GetParent(_shape) as DocumentObjectCollection;
                if (DocumentRelations.GetParent(docObjects) is Paragraph)//don't embed it twice!
                    return false;

                return _shape.Values.WrapFormat.IsValueNullOrEmpty() || _shape.WrapFormat.Values.Style is null || _shape.WrapFormat.Style == WrapStyle.TopBottom;
            }

            return false;
        }

        /// <summary>
        /// Renders the dummy paragraph's attributes.
        /// </summary>
        protected virtual void RenderParagraphAttributes()
        {
            bool isInCell = DocumentRelations.HasParentOfType(_shape, typeof(Cell));
            if (isInCell)
                _rtfWriter.WriteControl("intbl");

            RenderParagraphAlignment();
            RenderParagraphIndents();
            RenderParagraphDistances();
        }

        /// <summary>
        /// Renders the dummy paragraph's space before and space after attributes.
        /// </summary>
        void RenderParagraphDistances()
        {
            Unit spaceAfter = 0;
            Unit spaceBefore = 0;
            TopPosition top = (TopPosition)GetValueOrDefault("Top", new TopPosition());
            if (top.ShapePosition == ShapePosition.Undefined)
            {
                spaceBefore = top.Position + (Unit)GetValueOrDefault("WrapFormat.DistanceTop", (Unit)0);
            }
            spaceAfter = (Unit)GetValueOrDefault("WrapFormat.DistanceBottom", (Unit)0);
            RenderUnit("sa", spaceAfter);
            RenderUnit("sb", spaceBefore);
        }

        /// <summary>
        /// Renders the dummy paragraph's Alignment taking into account the shape's Left property.
        /// </summary>
        void RenderParagraphAlignment()
        {
            if (!_shape.Values.Left.IsValueNullOrEmpty())
            {
                LeftPosition leftPos = _shape.Left.ShapePosition;
                if (leftPos.ShapePosition != ShapePosition.Undefined)
                {
                    switch (leftPos.ShapePosition)
                    {
                        case ShapePosition.Right:
                            _rtfWriter.WriteControl("q", "r");
                            break;

                        case ShapePosition.Center:
                            _rtfWriter.WriteControl("q", "c");
                            break;

                        default:
                            _rtfWriter.WriteControl("q", "l");
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Renders the RelativeHorizontal attribute.
        /// </summary>
        void RenderRelativeHorizontal()
        {
            if (RenderInParagraph())
            {
                _rtfWriter.WriteControl("shpbx", "para");
                _rtfWriter.WriteControl("shpbx", "ignore");
                RenderNameValuePair("posrelh", "3");
            }
            else
            {
                //We need to write both shpbx and posrelh which are almost equivalent.
                Translate("RelativeHorizontal", "shpbx", RtfUnit.Undefined, "margin", false);
                _rtfWriter.WriteControl("shpbx", "ignore");

                var relHorObj = GetValueAsIntended("RelativeHorizontal");
                RelativeHorizontal relHor = relHorObj == null ? RelativeHorizontal.Margin : (RelativeHorizontal)relHorObj;
                switch (relHor)
                {
                    case RelativeHorizontal.Character:
                        RenderNameValuePair("posrelh", "3");
                        break;
                    case RelativeHorizontal.Column:
                        RenderNameValuePair("posrelh", "2");
                        break;
                    case RelativeHorizontal.Margin:
                        RenderNameValuePair("posrelh", "0");
                        break;
                    case RelativeHorizontal.Page:
                        RenderNameValuePair("posrelh", "1");
                        break;

                }
            }
        }

        /// <summary>
        /// Renders the RelativeVerticalattribute.
        /// </summary>
        void RenderRelativeVertical()
        {
            if (RenderInParagraph())
            {
                _rtfWriter.WriteControl("shpby", "para");
                _rtfWriter.WriteControl("shpby", "ignore");
                RenderNameValuePair("posrelv", "3");
            }
            else
            {
                //We need to write both shpby and posrelv which are almost equivalent.
                Translate("RelativeVertical", "shpby", RtfUnit.Undefined, "para", false);
                _rtfWriter.WriteControl("shpby", "ignore");

                var relVrtObj = GetValueAsIntended("RelativeVertical");
                RelativeVertical relVrt = relVrtObj == null ? RelativeVertical.Paragraph : (RelativeVertical)relVrtObj;
                switch (relVrt)
                {
                    case RelativeVertical.Line:
                        RenderNameValuePair("posrelv", "3");
                        break;
                    case RelativeVertical.Margin:
                        RenderNameValuePair("posrelv", "0");
                        break;

                    case RelativeVertical.Page:
                        RenderNameValuePair("posrelv", "1");
                        break;

                    case RelativeVertical.Paragraph:
                        RenderNameValuePair("posrelv", "2");
                        break;
                }
            }
        }

        /// <summary>
        /// Renders the WrapFormat.
        /// </summary>
        void RenderWrapFormat()
        {
            if (!RenderInParagraph())
            {
                Translate("WrapFormat.Style", "shpwr", RtfUnit.Undefined, "3", false);

                //REM: Distances don't work using them like this:
                /*
                TranslateAsNameValuePair("WrapFormat.DistanceTop", "dyWrapDistTop", RtfUnit.EMU, null);
                TranslateAsNameValuePair("WrapFormat.DistanceBottom", "dyWrapDistBottom", RtfUnit.EMU, null);
                TranslateAsNameValuePair("WrapFormat.DistanceLeft", "dxWrapDistLeft", RtfUnit.EMU, null);
                TranslateAsNameValuePair("WrapFormat.DistanceRight", "dxWrapDistRight", RtfUnit.EMU, null);
                */
            }
            else
            {
                //REM: Might not be necessary.
                _rtfWriter.WriteControl("shpwrk", "0");
                _rtfWriter.WriteControl("shpwr", "3");
            }
        }

        /// <summary>
        /// Renders the dummy paragraph's left indent.
        /// </summary>
        void RenderParagraphIndents()
        {
            var relHor = GetValueAsIntended("RelativeHorizontal");
            double leftInd = 0;
            double rightInd = 0;
            if (relHor != null && (RelativeHorizontal)relHor == RelativeHorizontal.Page)
            {
                Section parentSec = (Section)DocumentRelations.GetParentOfType(_shape, typeof(Section))!;
                Unit leftPgMrg = (Unit)parentSec.PageSetup.Values.LeftMargin!; // Re "!": Not null after visiting.
                leftInd = -leftPgMrg.Point;
                Unit rightPgMrg = (Unit)parentSec.PageSetup.Values.RightMargin!;
                rightInd = -rightPgMrg;
            }

            LeftPosition leftPos = (LeftPosition)GetValueOrDefault("Left", new LeftPosition());
            switch (leftPos.ShapePosition)
            {
                case ShapePosition.Undefined:
                    leftInd += leftPos.Position;
                    leftInd += ((Unit)GetValueOrDefault("WrapFormat.DistanceLeft", (Unit)0)).Point;
                    break;

                case ShapePosition.Left:
                    leftInd += ((Unit)GetValueOrDefault("WrapFormat.DistanceLeft", (Unit)0)).Point;
                    break;

                case ShapePosition.Right:
                    rightInd += ((Unit)GetValueOrDefault("WrapFormat.DistanceRight", (Unit)0)).Point;
                    break;
            }
            RenderUnit("li", leftInd);
            RenderUnit("lin", leftInd);
            RenderUnit("ri", rightInd);
            RenderUnit("rin", rightInd);
        }

        /// <summary>
        /// Renders (and calculates) the \shptop and \shpbottom controls in RTF.
        /// </summary>
        void RenderTopBottom()
        {
            Unit height = GetShapeHeight();
            Unit top = 0;
            Unit bottom = height;

            if (!RenderInParagraph())
            {
                RelativeVertical relVert = (RelativeVertical)GetValueOrDefault("RelativeVertical", RelativeVertical.Paragraph);
                TopPosition topPos = (TopPosition)GetValueOrDefault("Top", new TopPosition());
                //REM: Will not work like this in table cells.
                //=>The shape would have to be put in a paragraph there.
                Section sec = (Section)DocumentRelations.GetParentOfType(_shape, typeof(Section))!;

                PageSetup pgStp = sec.PageSetup;
                Unit topMrg = (Unit)pgStp.Values.TopMargin!; // Re "!": Not null after visiting.
                Unit btmMrg = (Unit)pgStp.Values.BottomMargin!;
                Unit pgHeight = pgStp.PageHeight;
                Unit pgWidth = pgStp.PageWidth;

                if (topPos.ShapePosition == ShapePosition.Undefined)
                {
                    top = topPos.Position;
                    bottom = top + height;
                }

                else
                {
                    switch (relVert)
                    {
                        case RelativeVertical.Line:
                            AlignVertically(topPos.ShapePosition, height, out top, out bottom);
                            break;

                        case RelativeVertical.Margin:
                            AlignVertically(topPos.ShapePosition, pgHeight.Point - topMrg.Point - btmMrg.Point, out top, out bottom);
                            break;

                        case RelativeVertical.Page:
                            AlignVertically(topPos.ShapePosition, pgHeight, out top, out bottom);
                            break;
                    }
                }
            }
            RenderUnit("shptop", top);
            RenderUnit("shpbottom", bottom);
        }

        /// <summary>
        /// Aligns the given top and bottom position so that ShapePosition.Top results in top position = 0.
        /// </summary>
        void AlignVertically(ShapePosition shpPos, Unit distanceTopBottom, out Unit topValue, out Unit bottomValue)
        {
            double height = GetShapeHeight().Point;
            topValue = 0;
            bottomValue = height;
            Unit topWrap = (Unit)GetValueOrDefault("WrapFormat.DistanceTop", (Unit)0);
            Unit bottomWrap = (Unit)GetValueOrDefault("WrapFormat.DistanceBottom", (Unit)0);
            switch (shpPos)
            {
                case ShapePosition.Bottom:
                    topValue = distanceTopBottom - height - bottomWrap;
                    bottomValue = distanceTopBottom - bottomWrap;
                    break;

                case ShapePosition.Center:
                    {
                        Unit centerPos = distanceTopBottom / 2.0;
                        topValue = centerPos - height / 2.0;
                        bottomValue = centerPos + height / 2.0;
                    }
                    break;

                case ShapePosition.Top:
                    topValue = topWrap;
                    bottomValue = topWrap + height;
                    break;
            }
        }

        /// <summary>
        /// Renders (and calculates) the \shpleft and \shpright controls in RTF.
        /// </summary>
        void RenderLeftRight()
        {
            Unit width = GetShapeWidth();
            Unit left = 0;
            Unit right = width;

            if (!RenderInParagraph())
            {
                RelativeHorizontal relHor = (RelativeHorizontal)GetValueOrDefault("RelativeHorizontal", RelativeHorizontal.Margin);
                LeftPosition leftPos = (LeftPosition)GetValueOrDefault("Left", new LeftPosition());
                //REM: Will not work like this in table cells.
                //=>The shape would have to be put in a paragraph there.

                Section sec = (Section)DocumentRelations.GetParentOfType(_shape, typeof(Section))!; // Re "!": Not null when used correctly.
                PageSetup pgStp = sec.PageSetup;
                Unit leftMrg = (Unit)pgStp.Values.LeftMargin!; // Re "!": Not null after visiting.
                Unit rgtMrg = (Unit)pgStp.Values.RightMargin!;
                //Unit pgHeight = pgStp.PageHeight;
                Unit pgWidth = pgStp.PageWidth;

                if (leftPos.ShapePosition == ShapePosition.Undefined)
                {
                    left = leftPos.Position;
                    right = left + width;
                }

                else
                {
                    switch (relHor)
                    {
                        case RelativeHorizontal.Column:
                        case RelativeHorizontal.Character:
                        case RelativeHorizontal.Margin:
                            AlignHorizontally(leftPos.ShapePosition, pgWidth.Point - leftMrg.Point - rgtMrg.Point, out left, out right);
                            break;

                        case RelativeHorizontal.Page:
                            AlignHorizontally(leftPos.ShapePosition, pgWidth, out left, out right);
                            break;
                    }
                }
            }
            RenderUnit("shpleft", left);
            RenderUnit("shpright", right);
        }

        /// <summary>
        /// Aligns the given left and right position so that ShapePosition.Left results in left position = 0.
        /// </summary>
        void AlignHorizontally(ShapePosition shpPos, Unit distanceLeftRight, out Unit leftValue, out Unit rightValue)
        {
            double width = GetShapeWidth().Point;
            leftValue = 0;
            rightValue = width;
            Unit leftWrap = (Unit)GetValueOrDefault("WrapFormat.DistanceLeft", (Unit)0);
            Unit rightWrap = (Unit)GetValueOrDefault("WrapFormat.DistanceRight", (Unit)0);
            switch (shpPos)
            {
                case ShapePosition.Right:
                //Positioning the shape Outside seems impossible=>Do the best that's possible.
                case ShapePosition.Outside:
                    leftValue = distanceLeftRight.Point - width - rightWrap;
                    rightValue = distanceLeftRight - rightWrap;
                    break;

                case ShapePosition.Center:
                    {
                        double centerPos = distanceLeftRight.Point / 2;
                        leftValue = centerPos - width / 2.0;
                        rightValue = centerPos + width / 2.0;
                    }
                    break;

                case ShapePosition.Left:
                //Positioning the shape inside seems impossible=>Do the best that's possible.
                case ShapePosition.Inside:
                    leftValue = leftWrap;
                    rightValue = leftWrap + width;
                    break;
            }
        }

        /// <summary>
        /// Renders a name value pair as shape property to RTF.
        /// </summary>
        protected void RenderNameValuePair(string name, string value)
        {
            StartNameValuePair(name);
            _rtfWriter.WriteText(value);
            EndNameValuePair();
        }

        /// <summary>
        /// Renders name as the beginning of a shape's name value pair to RTF.
        /// Used in the order StartNameValuePair &lt;value rendering&gt; EndNameValuePair.
        /// </summary>
        protected void StartNameValuePair(string name)
        {
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("sp");
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("sn");
            _rtfWriter.WriteText(name);
            _rtfWriter.EndContent();
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("sv");
        }

        /// <summary>
        /// Renders the end of a shape's name value pair.
        /// Used in the order StartNameValuePair &lt;value rendering&gt; EndNameValuePair.
        /// </summary>
        protected void EndNameValuePair()
        {
            _rtfWriter.EndContent();
            _rtfWriter.EndContent();
        }

        /// <summary>
        /// Translates a value as a shape's name value pair to RTF.
        /// </summary>
        protected void TranslateAsNameValuePair(string domValueName, string rtfName, RtfUnit unit, string? defaultValue)
        {
            object? val = GetValueAsIntended(domValueName);
            if (val == null && defaultValue == null)
                return;

            string valueStr = "";
            if (val == null)
                valueStr = defaultValue!; // Cannot be null here.
            else
            {
                if (val is Unit val1)
                    valueStr = ToRtfUnit(val1, unit).ToString();
                else if (val is Color col)
                {
                    col = col.GetMixedTransparencyColor();
                    valueStr = (col.R + (col.G * 256) + (col.B * 65536)).ToString();
                }
                else if (val is Enum)
                    valueStr = _enumTranslationTable[val].ToString()!;
                else if (val is bool b)
                    valueStr = b ? "1" : "0";
                else
                    Debug.Assert(false);
            }

            Debug.Assert(valueStr != null, nameof(valueStr) + " != null");
            RenderNameValuePair(rtfName, valueStr);
        }

        /// <summary>
        /// Starts a dummy paragraph to put a shape in, which is wrapped TopBottom style.
        /// </summary>
        protected virtual void StartDummyParagraph()
        {
            _rtfWriter.WriteControl("pard");
            RenderParagraphAttributes();
        }

        /// <summary>
        /// Ends a dummy paragraph to put a shape in, which is wrapped TopBottom style.
        /// </summary>
        protected virtual void EndDummyParagraph()
        {
            _rtfWriter.WriteControl("par");
        }

        readonly Shape _shape;
    }
}
