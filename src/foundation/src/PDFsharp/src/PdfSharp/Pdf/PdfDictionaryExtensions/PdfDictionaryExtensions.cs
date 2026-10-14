// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//using System.Xml.Linq;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.PdfDictionaryExtensions
{
    /// <summary>
    /// Extension methods for PDF dictionaries.
    /// </summary>
    public static class PdfDictionaryExtensions
    {
        public static PdfDictionary Transform(this PdfDictionary dict,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
            Type type)
        {
            return TransformInternal(dict, type);
        }

        public static T Transform<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
            T>
            (this PdfDictionary dict) where T : PdfDictionary
        {
            return (T)TransformInternal(dict, typeof(T));

        }

        static PdfDictionary TransformInternal(PdfDictionary dict,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
            Type type)
        {
            if (dict.GetType() == type)
                return dict;

            var ctorInfo = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, types: [typeof(PdfDictionary)], null);
            if (ctorInfo == null)
                throw new InvalidOperationException($"Type '{type.FullName}' has no appropriate constructor for object type transformation.");
            dict = (PdfDictionary)ctorInfo.Invoke([dict]);
            return dict;
        }
        
        //public static PdfItem? this[this, string name] => null;

        //public static PdfItem? GetRawValue(this PdfDictionary dict, string key)
        //{
        //    return dict.Elements[key];
        //}

        //public static PdfItem? GetItem(this PdfDictionary dict, string key)
        //{
        //    return dict.Elements[key];
        //}

        //public static void SetItem(this PdfDictionary dict, string key, PdfItem value)
        //{
        //    if (value == null)
        //        throw new ArgumentNullException(nameof(value));

        //    dict.Elements[key] = value;
        //}
    }
}
