// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a shading to RTF.
    /// </summary>
    class ShadingRenderer : RendererBase
    {
        internal ShadingRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _shading = (Shading)domObj;
            _isCellShading = !(DocumentRelations.GetParent(_shading) is ParagraphFormat);
        }

        /// <summary>
        /// Render a shading to RTF.
        /// </summary>
        internal override void Render()
        {
            _useEffectiveValue = true;
            var vsbl = GetValueAsIntended("visible");
            var clr = GetValueAsIntended("Color");

            if (vsbl == null || (bool)vsbl)
            {
                if (clr != null)
                    Translate("Color", _isCellShading ? "clcbpat" : "cbpat");
            }
        }

        readonly Shading _shading;
        readonly bool _isCellShading;
    }
}
