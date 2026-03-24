// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Pdf.PdfItemExtensions
{
    /// <summary>
    /// Extension methods for PDF items.
    /// </summary>
    public static class PdfItemExtensions
    {

        /// <summary>
        /// Casts a PDF item into a PDF array, or throws an exception if this is not possible.
        /// </summary>
        public static PdfArray AsArray(this PdfItem? value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value is PdfReference reference)
                value = reference.Value;

            if (value is PdfArray arr)
                return arr;

            throw new InvalidCastException($"PdfItem of type '{value.GetType().FullName}' cannot be casted to PdfArray.");
        }

        /// <summary>
        /// Casts a PDF item into a PDF array of type T, or throws an exception if this is not possible.
        /// </summary>
        public static T AsArray<T>(this PdfItem? value) where T : PdfArray
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value is PdfReference reference)
                value = reference.Value;

            if (value is T arr)
                return arr;

            throw new InvalidCastException($"PdfItem of type '{value.GetType().FullName}' cannot be casted to {typeof(T).FullName}.");
        }

        //public static PdfArray AsArray(this PdfReference? value)
        //{
        //    if (value == null)
        //        throw new ArgumentNullException(nameof(value));

        //    if (value.Value is PdfArray arr)
        //        return arr;

        //    throw new InvalidCastException("value is not a PdfArray.");
        //}

        //public static T AsArray<T>(this PdfReference? value) where T : PdfArray
        //{
        //    if (value == null)
        //        throw new ArgumentNullException(nameof(value));

        //    if (value.Value is T arr)
        //        return arr;

        //    throw new InvalidCastException($"value is not a {typeof(T).Name}.");
        //}

        /// <summary>
        /// Casts a PDF item into a PDF dictionary of type T, or throws an exception if this is not possible.
        /// </summary>
        public static PdfDictionary AsDictionary(this PdfItem? value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value is PdfReference reference)
                value = reference.Value;

            if (value is PdfDictionary dict)
                return dict;

            throw new InvalidCastException($"PdfItem of type '{value.GetType().FullName}' cannot be casted to PdfDictionary.");
        }

        /// <summary>
        /// Casts a PDF item into a PDF dictionary of type T, or throws an exception if this is not possible.
        /// </summary>
        public static T AsDictionary<T>(this PdfItem? value) where T : PdfDictionary
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value is PdfReference reference)
                value = reference.Value;

            if (value is T dict)
                return dict;

            throw new InvalidCastException($"PdfItem of type '{value.GetType().FullName}' cannot be casted to '{typeof(T).FullName}'.");
        }

        //public static PdfDictionary AsDictionary(this PdfReference? value)
        //{
        //    if (value == null)
        //        throw new ArgumentNullException(nameof(value));

        //    if (value.Value is PdfDictionary dict)
        //        return dict;

        //    throw new InvalidCastException("value is not a PdfDictionary.");
        //}

        //public static T AsDictionary<T>(this PdfReference? value) where T : PdfDictionary
        //{
        //    if (value == null)
        //        throw new ArgumentNullException(nameof(value));

        //    if (value.Value is T dict)
        //        return dict;

        //    throw new InvalidCastException($"value is not a {typeof(T).Name}.");
        //}

        /// <summary>
        /// Casts a PDF item into a PDF reference, or throws an exception if this is not possible.
        /// </summary>
        public static PdfReference AsReference(this PdfItem? value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value is PdfReference reference)
                return reference;

            throw new InvalidCastException($"PdfItem of type '{value.GetType().FullName}' cannot be casted to PdfReference.");
        }
    }
}

