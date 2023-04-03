// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting
{
    /// <summary>
    /// Base class for all chart classes.
    /// </summary>
    public class DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the DocumentObject class.
        /// </summary>
        public DocumentObject()
        { }

        /// <summary>
        /// Initializes a new instance of the DocumentObject class with the specified parent.
        /// </summary>
        public DocumentObject(DocumentObject parent)
        {
            Parent = parent;
        }

        #region Methods
        /// <summary>
        /// Creates a deep copy of the DocumentObject. The parent of the new object is null.
        /// </summary>
        public object Clone() 
            => DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected virtual object DeepCopy()
        {
            var value = (DocumentObject)MemberwiseClone();
            value.Parent = null;
            return value;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the parent object, or null if the object has no parent.
        /// </summary>
        public DocumentObject? Parent { get; internal set; }
        
        //    => _parent;
        //// ReSharper disable once InconsistentNaming because this is old code
        //internal DocumentObject? _parent2;
        #endregion
    }
}
