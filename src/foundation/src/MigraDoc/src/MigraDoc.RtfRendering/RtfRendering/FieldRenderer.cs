// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;
using MigraDoc.RtfRendering.Resources;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Base class for all classes that render fields to RTF.
    /// </summary>
    abstract class FieldRenderer : RendererBase
    {
        internal FieldRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
        }

        /// <summary>
        /// Starts an RTF field with appropriate control words.
        /// </summary>
        protected void StartField()
        {
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("field");
            _rtfWriter.WriteControl("flddirty");
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("fldinst", true);
        }

        /// <summary>
        /// Ends an RTF field with appropriate control words.
        /// </summary>
        protected void EndField()
        {
            _rtfWriter.EndContent();
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("fldrslt");
            _rtfWriter.WriteText(GetFieldResult());
            _rtfWriter.EndContent();
            _rtfWriter.EndContent();
        }

        /// <summary>
        /// Gets the field result if possible.
        /// </summary>
        protected virtual string GetFieldResult()
        {
            return Messages2.UpdateField;
        }
    }
}
