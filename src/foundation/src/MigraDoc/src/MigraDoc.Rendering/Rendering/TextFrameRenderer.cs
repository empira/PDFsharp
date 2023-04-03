// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel.Shapes;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Renders text frames.
    /// </summary>
    class TextFrameRenderer : ShapeRenderer
    {
        internal TextFrameRenderer(XGraphics gfx, TextFrame textFrame, FieldInfos? fieldInfos)
            : base(gfx, textFrame, fieldInfos)
        {
            _textFrame = textFrame;
            var renderInfo = new TextFrameRenderInfo
            {
                DocumentObject = _shape
            };
            _renderInfo = renderInfo;
        }

        internal TextFrameRenderer(XGraphics gfx, RenderInfo renderInfo, FieldInfos? fieldInfos)
            : base(gfx, renderInfo, fieldInfos)
        {
            _textFrame = (TextFrame)renderInfo.DocumentObject;
        }

        internal override void Format(Area area, FormatInfo? previousFormatInfo)
        {
            FormattedTextFrame formattedTextFrame = new FormattedTextFrame(_textFrame, _documentRenderer, _fieldInfos);
            formattedTextFrame.Format(_gfx);
            ((TextFrameFormatInfo)_renderInfo.FormatInfo).FormattedTextFrame = formattedTextFrame;
            base.Format(area, previousFormatInfo);
        }

        //internal override LayoutInfo InitialLayoutInfo => base.InitialLayoutInfo;  // redundant 

        internal override void Render()
        {
            RenderFilling();
            RenderContent();
            RenderLine();
        }

        void RenderContent()
        {
            FormattedTextFrame formattedTextFrame = ((TextFrameFormatInfo)_renderInfo.FormatInfo).FormattedTextFrame;
            var renderInfos = formattedTextFrame.GetRenderInfos();
            if (renderInfos == null)
                return;

            XGraphicsState state = Transform();
            RenderByInfos(renderInfos);
            ResetTransform(state);
        }

        XGraphicsState Transform()
        {
            Area frameContentArea = _renderInfo.LayoutInfo.ContentArea;
            XGraphicsState state = _gfx.Save();
            XUnit xPosition;
            XUnit yPosition;
            switch (_textFrame.Orientation)
            {
                case TextOrientation.Downward:
                case TextOrientation.Vertical:
                case TextOrientation.VerticalFarEast:
                    xPosition = frameContentArea.X + frameContentArea.Width;
                    yPosition = frameContentArea.Y;
                    _gfx.TranslateTransform(xPosition, yPosition);
                    _gfx.RotateTransform(90);
                    break;

                case TextOrientation.Upward:
                    state = _gfx.Save();
                    xPosition = frameContentArea.X;
                    yPosition = frameContentArea.Y + frameContentArea.Height;
                    _gfx.TranslateTransform(xPosition, yPosition);
                    _gfx.RotateTransform(-90);
                    break;

                default:
                    xPosition = frameContentArea.X;
                    yPosition = frameContentArea.Y;
                    _gfx.TranslateTransform(xPosition, yPosition);
                    break;
            }
            return state;
        }

        void ResetTransform(XGraphicsState? state)
        {
            if (state != null)
                _gfx.Restore(state);
        }

        readonly TextFrame _textFrame;
    }
}
