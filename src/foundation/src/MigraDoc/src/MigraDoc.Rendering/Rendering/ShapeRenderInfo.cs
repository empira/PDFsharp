// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Rendering information for shapes.
    /// </summary>
    public abstract class ShapeRenderInfo : RenderInfo
    {
        internal ShapeRenderInfo()
        { }

        /// <summary>
        /// Gets the document object to which the layout information applies. Use the Tag property of DocumentObject to identify an object.
        /// </summary>
        public override DocumentObject DocumentObject
        {
            get => _shape ?? NRT.ThrowOnNull<Shape>();
            internal set => _shape = (Shape)value;
        }
        Shape? _shape;
    }
}
