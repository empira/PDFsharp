// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Renders an information field to RTF.
    /// </summary>
    class InfoFieldRenderer : FieldRenderer
    {
        internal InfoFieldRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _infoField = (InfoField)domObj;
        }

        /// <summary>
        /// Renders an InfoField to RTF.
        /// </summary>
        internal override void Render()
        {
            StartField();
            _rtfWriter.WriteText("INFO ");
            switch (_infoField.Name.ToLower())
            {
                case "author":
                    _rtfWriter.WriteText("Author");
                    break;

                case "title":
                    _rtfWriter.WriteText("Title");
                    break;

                case "keywords":
                    _rtfWriter.WriteText("Keywords");
                    break;

                case "subject":
                    _rtfWriter.WriteText("Subject");
                    break;
            }
            EndField();
        }

        /// <summary>
        /// Gets the requested document info if available.
        /// </summary>
        protected override string GetFieldResult()
        {
            Document doc = _infoField.Document!;
            if (!doc.Values.Info?.IsNull(_infoField.Name) ?? false)
                return (doc.Info.GetValue(_infoField.Name) as string)!;

            return "";
        }

        readonly InfoField _infoField;
    }
}
