// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a NumPagesField to RTF.
    /// </summary>
    class NumPagesFieldRenderer : NumericFieldRendererBase
    {
        internal NumPagesFieldRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _numPagesField = (NumPagesField)domObj;
        }

        /// <summary>
        /// Renders a NumPagesField to RTF.
        /// </summary>
        internal override void Render()
        {
            StartField();
            _rtfWriter.WriteText("NUMPAGES");
            TranslateFormat();
            EndField();
        }

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        NumPagesField _numPagesField;
    }
}
