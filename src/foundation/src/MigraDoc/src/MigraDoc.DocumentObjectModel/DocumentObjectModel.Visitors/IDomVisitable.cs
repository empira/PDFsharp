// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Visitors
{
    interface IVisitable
    {
        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren);
    }
}
