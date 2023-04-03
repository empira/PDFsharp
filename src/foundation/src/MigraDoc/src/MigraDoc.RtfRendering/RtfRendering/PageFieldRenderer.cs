// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a PageField to RTF.
    /// </summary>
    class PageFieldRenderer : NumericFieldRendererBase
    {
        internal PageFieldRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _pageField = (PageField)domObj;
        }

        /// <summary>
        /// Renders a PageField to RTF.
        /// </summary>
        internal override void Render()
        {
            StartField();
            _rtfWriter.WriteText("PAGE");
            TranslateFormat();
            EndField();
        }

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        PageField _pageField;
    }
}
