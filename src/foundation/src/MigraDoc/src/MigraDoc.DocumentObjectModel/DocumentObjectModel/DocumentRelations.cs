// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Provides relational information between document objects.
    /// </summary>
    public class DocumentRelations
    {
        /// <summary>
        /// Determines whether the specified documentObject has a
        /// parent of the given type somewhere within the document hierarchy.
        /// </summary>
        /// <param name="documentObject">The document object to check.</param>
        /// <param name="type">The parent type to search for.</param>
        public static bool HasParentOfType(DocumentObject documentObject, Type type)
        {
            if (documentObject == null)
                throw new ArgumentNullException(nameof(documentObject));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return GetParentOfType(documentObject, type) != null;
        }

        /// <summary>
        /// Gets the direct parent of the given document object.
        /// </summary>
        /// <param name="documentObject">The document object the parent is searched for.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static DocumentObject? GetParent(DocumentObject? documentObject)
        {
            if (documentObject == null)
                throw new ArgumentNullException(nameof(documentObject));

            return documentObject.Parent;
        }

        /// <summary>
        /// Gets a parent of the document object with the given type somewhere within the document hierarchy.
        /// Returns null if none exists.
        /// </summary>
        /// <param name="documentObject">The document object the parent is searched for.</param>
        /// <param name="type">The parent type to search for.</param>
        public static DocumentObject? GetParentOfType(DocumentObject? documentObject, Type type)
        {
            if (documentObject == null)
                throw new ArgumentNullException(nameof(documentObject));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (documentObject.Parent != null)
            {
                if (documentObject.Parent.GetType() == type)
                    return documentObject.Parent;
                return GetParentOfType(documentObject.Parent, type);
            }
            return null;
        }
    }
}
