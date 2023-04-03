// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.RtfRendering.Resources;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Base Class to render numeric fields.
    /// </summary>
    abstract class NumericFieldRendererBase : FieldRenderer
    {
        internal NumericFieldRendererBase(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _numericField = (NumericFieldBase)domObj;
        }

        /// <summary>
        /// Translates the number format to RTF.
        /// </summary>
        protected void TranslateFormat()
        {
            switch (_numericField.Format)
            {
                case "":
                    break;

                case "ROMAN":
                    _rtfWriter.WriteText(@" \*ROMAN");
                    break;

                case "roman":
                    _rtfWriter.WriteText(@" \*roman");
                    break;

                case "ALPHABETIC":
                    _rtfWriter.WriteText(@" \*ALPHABETIC");
                    break;

                case "alphabetic":
                    _rtfWriter.WriteText(@" \*alphabetic");
                    break;

                default:
                    Debug.WriteLine(Messages2.InvalidNumericFieldFormat(_numericField.Format), "warning");
                    break;
            }
        }

        readonly NumericFieldBase _numericField;
    }
}
