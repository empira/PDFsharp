// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.RtfRendering.Resources;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a TextFrame to RTF.
    /// </summary>
    class TextFrameRenderer : ShapeRenderer
    {
        internal TextFrameRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _textFrame = (TextFrame)domObj;
        }

        /// <summary>
        /// Renders a TextFrame to RTF.
        /// </summary>
        internal override void Render()
        {
            DocumentElements? elms = DocumentRelations.GetParent(_textFrame) as DocumentElements;
            bool renderInParagraph = RenderInParagraph();
            if (renderInParagraph)
                StartDummyParagraph();

            StartShapeArea();

            //Properties
            RenderNameValuePair("shapeType", "202");//202 is TextFrame.

            TranslateAsNameValuePair("MarginLeft", "dxTextLeft", RtfUnit.EMU, "0");
            TranslateAsNameValuePair("MarginTop", "dyTextTop", RtfUnit.EMU, "0");
            TranslateAsNameValuePair("MarginRight", "dxTextRight", RtfUnit.EMU, "0");
            TranslateAsNameValuePair("MarginBottom", "dyTextBottom", RtfUnit.EMU, "0");

            if (_textFrame.Values.Elements.IsValueNullOrEmpty() ||
                !CollectionContainsObjectAssignableTo(_textFrame.Elements,
                   typeof(Shape), typeof(Table)))
                TranslateAsNameValuePair("Orientation", "txflTextFlow", RtfUnit.Undefined, null);
            else
            {
                TextOrientation orient = _textFrame.Orientation;
                if (orient != TextOrientation.Horizontal && orient != TextOrientation.HorizontalRotatedFarEast)
                    Debug.WriteLine(Messages2.TextframeContentsNotTurned, "warning");
            }
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("shptxt");
            _rtfWriter.StartContent();
            foreach (var docObj in _textFrame.Elements)
            {
                Debug.Assert(docObj != null, nameof(docObj) + " != null");
                var rndrr = RendererFactory.CreateRenderer(docObj, _docRenderer);
                //if (rndrr != null)
                {
                    rndrr.Render();
                }
            }
            //Text fields need to close with a paragraph.
            RenderTrailingParagraph(_textFrame.Elements);

            _rtfWriter.EndContent();
            _rtfWriter.EndContent();
            EndShapeArea();
            if (renderInParagraph)
            {
                RenderLayoutPicture();
                EndDummyParagraph();
            }
        }

        /// <summary>
        /// Gets the user defined shape height if given, else 1 inch.
        /// </summary>
        protected override Unit GetShapeHeight()
        {
            if (_textFrame.Values.Height.IsValueNullOrEmpty())
                return Unit.FromInch(1.0);

            return base.GetShapeHeight();
        }

        /// <summary>
        /// Gets the user defined shape width if given, else 1 inch.
        /// </summary>
        protected override Unit GetShapeWidth()
        {
            if (_textFrame.Values.Width.IsValueNullOrEmpty())
                return Unit.FromInch(1.0);

            return base.GetShapeWidth();
        }

        /// <summary>
        /// Renders an empty dummy picture that allows the textframe to be placed in the dummy paragraph.
        /// (A bit obscure, but the only possibility.)
        /// </summary>
        void RenderLayoutPicture()
        {
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("pict");
            _rtfWriter.StartContent();
            _rtfWriter.WriteControlWithStar("picprop");
            _rtfWriter.WriteControl("defshp");
            RenderNameValuePair("shapeType", "75");
            RenderNameValuePair("fPseudoInline", "1");
            RenderNameValuePair("fLockPosition", "1");
            RenderNameValuePair("fLockRotation", "1");
            _rtfWriter.EndContent();
            //The next two lines are needed by Word, whyever.
            _rtfWriter.WriteControl("pich", (int)(GetShapeHeight().Millimeter * 100));
            _rtfWriter.WriteControl("picw", (int)(GetShapeWidth().Millimeter * 100));

            RenderUnit("pichgoal", GetShapeHeight());
            RenderUnit("picwgoal", GetShapeWidth());
            _rtfWriter.WriteControl("wmetafile", 8);

            //It's also not clear why this is needed:
            _rtfWriter.WriteControl("blipupi", 600);
            _rtfWriter.EndContent();
        }

        /// <summary>
        /// Starts a dummy paragraph to put a shape in, which is wrapped TopBottom style.
        /// </summary>
        protected override void StartDummyParagraph()
        {
            base.StartDummyParagraph();
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("field");
            _rtfWriter.WriteControl("fldedit");
            _rtfWriter.WriteControl("fldlock");
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("fldinst", true);
            _rtfWriter.StartContent();
            _rtfWriter.WriteText("SHAPE");
            _rtfWriter.WriteText(@" \*MERGEFORMAT");
            _rtfWriter.EndContent();
            _rtfWriter.EndContent();
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("fldrslt");
        }

        /// <summary>
        /// Ends a dummy paragraph to put a shape in, which is wrapped TopBottom style.
        /// </summary>
        protected override void EndDummyParagraph()
        {
            _rtfWriter.EndContent();
            _rtfWriter.EndContent();
            base.EndDummyParagraph();
        }

        readonly TextFrame _textFrame;
    }
}
