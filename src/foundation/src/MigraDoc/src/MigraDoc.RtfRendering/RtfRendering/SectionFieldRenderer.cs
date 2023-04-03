// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a SectionField to RTF.
    /// </summary>
    class SectionFieldRenderer : NumericFieldRendererBase
    {
        internal SectionFieldRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _sectionField = (SectionField)domObj;
        }

        /// <summary>
        /// Renders a SectionField to RTF.
        /// </summary>
        internal override void Render()
        {
            StartField();
            _rtfWriter.WriteText("SECTION");
            TranslateFormat();
            EndField();
        }

        SectionField _sectionField;
    }
}
