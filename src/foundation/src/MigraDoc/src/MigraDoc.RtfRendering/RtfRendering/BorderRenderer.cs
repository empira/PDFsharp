// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Renders a single border to RTF.
    /// </summary>
    class BorderRenderer : BorderRendererBase
    {
        public BorderRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _border = (Border)domObj;
        }

        /// <summary>
        /// Renders a single border. A border also needs to be rendered if it is invisible.
        /// </summary>
        internal override void Render()
        {
            _useEffectiveValue = true;
            RenderBorder(GetBorderControl(_borderType));
        }

        /// <summary>
        /// Sets the border type to render. This property is set by the borders renderer.
        /// </summary>
        internal BorderType BorderType
        {
            set { _borderType = value; }
        }

        BorderType _borderType;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        Border _border;
    }
}