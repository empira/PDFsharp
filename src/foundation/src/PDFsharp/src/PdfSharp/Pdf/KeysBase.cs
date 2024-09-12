// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Diagnostics.CodeAnalysis;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Base class for all dictionary Keys classes.
    /// </summary>
    public class KeysBase
    {
        internal static DictionaryMeta CreateMeta([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type) => new(type);

        /// <summary>
        /// Creates the DictionaryMeta with the specified default type to return in DictionaryElements.GetValue
        /// if the key is not defined.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="defaultContentKeyType">Default type of the content key.</param>
        /// <param name="defaultContentType">Default type of the content.</param>
        internal static DictionaryMeta CreateMeta([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type, KeyType defaultContentKeyType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type defaultContentType) 
            => new(type, defaultContentKeyType, defaultContentType);
    }
}
