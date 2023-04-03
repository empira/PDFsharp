// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a SectionPagesField to RTF.
    /// </summary>
    class SectionPagesFieldRenderer : NumericFieldRendererBase
    {
        internal SectionPagesFieldRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _sectionPagesField = (SectionPagesField)domObj;
        }

        /// <summary>
        /// Renders a SectionPagesField to RTF.
        /// </summary>
        internal override void Render()
        {
            StartField();
            _rtfWriter.WriteText("SECTIONPAGES");
            TranslateFormat();
            EndField();
        }

        SectionPagesField _sectionPagesField;
    }
}
