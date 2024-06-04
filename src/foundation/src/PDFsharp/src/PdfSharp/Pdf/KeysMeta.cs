// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Reflection;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Holds information about the value of a key in a dictionary. This information is used to create
    /// and interpret this value.
    /// </summary>
    sealed class KeyDescriptor
    {
        /// <summary>
        /// Initializes a new instance of KeyDescriptor from the specified attribute during a KeysMeta
        /// initializes itself using reflection.
        /// </summary>
        public KeyDescriptor(KeyInfoAttribute attribute)
        {
            Version = attribute.Version;
            KeyType = attribute.KeyType;
            FixedValue = attribute.FixedValue;
            ObjectType = attribute.ObjectType;

            if (Version == "")
                Version = "1.0";
        }

        /// <summary>
        /// Gets or sets the PDF version starting with the availability of the described key.
        /// </summary>
        public string Version { get; set; }

        public KeyType KeyType { get; set; }

        public string KeyValue { get; set; } = default!;

        public string FixedValue { get; }

        public Type ObjectType { get; set; }

        public bool CanBeIndirect => (KeyType & KeyType.MustNotBeIndirect) == 0;

        /// <summary>
        /// Returns the type of the object to be created as value for the described key.
        /// </summary>
        public Type GetValueType()
        {
            var type = ObjectType;
            if (type == null!)
            {
                // If we have no ObjectType specified, use the KeyType enumeration.
                switch (KeyType & KeyType.TypeMask)
                {
                    case KeyType.Name:
                        type = typeof(PdfName);
                        break;

                    case KeyType.String:
                        type = typeof(PdfString);
                        break;

                    case KeyType.Boolean:
                        type = typeof(PdfBoolean);
                        break;

                    case KeyType.Integer:
                        type = typeof(PdfInteger);
                        break;

                    case KeyType.Real:
                        type = typeof(PdfReal);
                        break;

                    case KeyType.Date:
                        type = typeof(PdfDate);
                        break;

                    case KeyType.Rectangle:
                        type = typeof(PdfRectangle);
                        break;

                    case KeyType.Array:
                        type = typeof(PdfArray);
                        break;

                    case KeyType.Dictionary:
                        type = typeof(PdfDictionary);
                        break;

                    case KeyType.Stream:
                        type = typeof(PdfDictionary);
                        break;

                    case KeyType.NumberTree:
                        type = typeof(PdfNumberTreeNode);
                        break;

                    case KeyType.NameTree:
                        type = typeof(PdfNameTreeNode);
                        break;

                    case KeyType.FileSpecification:
                        type = typeof(PdfFileSpecification);
                        break;

                    // The following types are not yet used.

                    case KeyType.NameOrArray:
                        throw new NotImplementedException("KeyType.NameOrArray");

                    case KeyType.ArrayOrDictionary:
                        throw new NotImplementedException("KeyType.ArrayOrDictionary");

                    case KeyType.StreamOrArray:
                        throw new NotImplementedException("KeyType.StreamOrArray");

                    case KeyType.ArrayOrNameOrString:
                        return null!; // HACK: Make PdfOutline work
                                     //throw new NotImplementedException("KeyType.ArrayOrNameOrString");

                    default:
                        Debug.Assert(false, "Invalid KeyType: " + KeyType);
                        break;
                }
            }
            return type;
        }
    }

    /// <summary>
    /// Contains meta information about all keys of a PDF dictionary.
    /// </summary>
    class DictionaryMeta
    {
        public DictionaryMeta(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(typeof(KeyInfoAttribute), false);
                if (attributes.Length == 1)
                {
                    var attribute = (KeyInfoAttribute)attributes[0];
                    var descriptor = new KeyDescriptor(attribute)
                    {
                        KeyValue = (string)field.GetValue(null)!
                    };
                    _keyDescriptors[descriptor.KeyValue] = descriptor;
                }
            }
        }

        /// <summary>
        /// Initializes the DictionaryMeta with the specified default type to return in DictionaryElements.GetValue
        /// if the key is not defined.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="defaultContentKeyType">Default type of the content key.</param>
        /// <param name="defaultContentType">Default type of the content.</param>
        public DictionaryMeta(Type type, KeyType defaultContentKeyType, Type defaultContentType) : this(type)
        {
            _defaultContentKeyDescriptor = new KeyDescriptor(new KeyInfoAttribute(defaultContentKeyType, defaultContentType));
        }

        /// <summary>
        /// Gets the KeyDescriptor of the specified key, or null if no such descriptor exits.
        /// </summary>
        public KeyDescriptor? this[string key]
        {
            get
            {
                _keyDescriptors.TryGetValue(key, out var keyDescriptor);
                return keyDescriptor ?? _defaultContentKeyDescriptor;
            }
        }
        readonly Dictionary<string, KeyDescriptor> _keyDescriptors = new();

        /// <summary>
        /// The default content key descriptor used if no descriptor exists for a given key.
        /// </summary>
        readonly KeyDescriptor? _defaultContentKeyDescriptor;
    }
}
