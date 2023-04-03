// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Internals;

using Font = MigraDoc.DocumentObjectModel.Font;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Renders a font to RTF.
    /// </summary>
    class FontRenderer : RendererBase
    {
        internal FontRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _font = (Font)domObj;
        }

        /// <summary>
        /// Renders a font to RTF.
        /// </summary>
        internal override void Render()
        {
            _useEffectiveValue = true;

            var name = (string?)GetValueAsIntended("Name");
            Debug.Assert(name != "");

            if (name != null)
                _rtfWriter.WriteControl("f", _docRenderer.GetFontIndex(name));

            Translate("Size", "fs", RtfUnit.HalfPts, null, false);
            TranslateBool("Bold", "b", "b0", false);
            Translate("Underline", "ul");
            TranslateBool("Italic", "i", "i0", false);
            Translate("Color", "cf");

            var objectSub = _font.Values.Subscript;
            if (objectSub != null && (bool)objectSub)
                _rtfWriter.WriteControl("sub");

            var objectSuper = _font.Values.Superscript;
            if (objectSuper != null && (bool)objectSuper)
                _rtfWriter.WriteControl("super");
        }

        readonly Font _font;
    }
}
