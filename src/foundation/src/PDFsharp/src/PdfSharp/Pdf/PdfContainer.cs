// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// v7.0.0 Ready

namespace PdfSharp.Pdf
{
    // Re/Sharper enable GrammarMistakeInComment

    /// <summary>
    /// Common abstract base class for both PdfArray and PdfDictionary.
    /// For technical purposes only, e.g. as marker class. There is no counterpart of
    /// this class in the PDF specification.
    /// </summary>
    public abstract class PdfContainer : PdfObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfContainer" /> class.
        /// </summary>
        protected internal PdfContainer()
        {
            // Only PdfArray or PdfDictionary are allowed to derive from PdfContainer.
            Debug.Assert(this is PdfArray or PdfDictionary);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfContainer" /> class.
        /// </summary>
        protected internal PdfContainer(PdfDocument doc, bool createIndirect = false)
            : base(doc, createIndirect)
        {
            // Only PdfArray or PdfDictionary are allowed to derive from PdfContainer.
            Debug.Assert(this is PdfArray or PdfDictionary);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfContainer" /> class.
        /// </summary>
        protected internal PdfContainer(PdfContainer obj) : base(obj)
        {
            // Only PdfArray or PdfDictionary are allowed to derive from PdfContainer.
            Debug.Assert(this is PdfArray or PdfDictionary);
        }

        /// <summary>
        /// Transforms a container to a derived. If the container already is of the requested type,
        /// no action is taken.
        /// </summary>
        protected internal T TransformTo<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                        DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
            (PdfContainer obj) where T : PdfContainer
        {
            //if (typeof(T).IsInstanceOfType(obj))//obj.GetType().IsAssignableTo(typeof(T))))
            if (obj is T result)   //obj.GetType().IsAssignableTo(typeof(T))))
                return result;

            Debug.Assert(obj.GetType().IsAssignableFrom(typeof(T)));

            switch (obj)
            {
                case PdfDictionary dict:
                    result = (T)dict.Elements.CreateContainer(typeof(T), obj, dict.IsIndirect);
                    return result;

                case PdfArray array:
                    result = (T)array.Elements.CreateContainer(typeof(T), obj, array.IsIndirect);
                    return result;

                default:
                    throw new InvalidOperationException("Object is neither a PDF array nor a dictionary.");
            }
        }
    }
}
