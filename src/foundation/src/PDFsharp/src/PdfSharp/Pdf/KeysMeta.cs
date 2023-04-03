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
            _version = attribute.Version;
            _keyType = attribute.KeyType;
            _fixedValue = attribute.FixedValue;
            _objectType = attribute.ObjectType;

            if (_version == "")
                _version = "1.0";
        }

        /// <summary>
        /// Gets or sets the PDF version starting with the availability of the described key.
        /// </summary>
        public string Version
        {
            get => _version;
            set => _version = value;
        }

        string _version;

        public KeyType KeyType
        {
            get => _keyType;
            set => _keyType = value;
        }

        KeyType _keyType;

        public string KeyValue
        {
            get => _keyValue;
            set => _keyValue = value;
        }
        string _keyValue = null!; // NRT

        public string FixedValue => _fixedValue;

        readonly string _fixedValue;

        public Type ObjectType
        {
            get => _objectType;
            set => _objectType = value;
        }

        Type _objectType;

        public bool CanBeIndirect => (_keyType & KeyType.MustNotBeIndirect) == 0;

        /// <summary>
        /// Returns the type of the object to be created as value for the described key.
        /// </summary>
        public Type GetValueType()
        {
            var type = _objectType;
            if (type == null!)
            {
                // If we have no ObjectType specified, use the KeyType enumeration.
                switch (_keyType & KeyType.TypeMask)
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

                    // The following types are not yet used

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
                        Debug.Assert(false, "Invalid KeyType: " + _keyType);
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
                    KeyInfoAttribute attribute = (KeyInfoAttribute)attributes[0];
                    KeyDescriptor descriptor = new KeyDescriptor(attribute);
                    descriptor.KeyValue = (string)field.GetValue(null)!;
                    _keyDescriptors[descriptor.KeyValue] = descriptor;
                }
            }
        }

#if UWP
        // Background: The function GetRuntimeFields gets constant fields only for the specified type,
        // not for its base types. So we have to walk recursively through base classes.
        // The documentation says full trust for the immediate caller is required for property BaseClass.
        // TODO: Rewrite this stuff for medium trust.
        void CollectKeyDescriptors(Type type)
        {
            // Get fields of the specified type only.
            var fields = type.GetTypeInfo().DeclaredFields;
            foreach (FieldInfo field in fields)
            {
                var attributes = field.GetCustomAttributes(typeof(KeyInfoAttribute), false);
                foreach (var attribute in attributes)
                {
                    KeyDescriptor descriptor = new KeyDescriptor((KeyInfoAttribute)attribute);
                    descriptor.KeyValue = (string)field.GetValue(null);
                    _keyDescriptors[descriptor.KeyValue] = descriptor;
                }
            }
            type = type.GetTypeInfo().BaseType;
            if (type != typeof(object) && type != typeof(PdfObject))
                CollectKeyDescriptors(type);
        }
#endif

#if (CORE) && true_
        public class A
        {
            public string _a;
            public const string _ca = "x";
        }
        public class B : A
        {
            public string _b;
            public const string _cb = "x";

            void Foo()
            {
                var str = A._ca;
            }
        }
        class Test
        {
            public static void It()
            {
                string s = "Runtime fields of B:";
                foreach (var fieldInfo in typeof(B).GetRuntimeFields()) { s += " " + fieldInfo.Name; }
                Debug.WriteLine(s);

                s = "Declared fields of B:";
                foreach (var fieldInfo in typeof(B).GetTypeInfo().DeclaredFields) { s += " " + fieldInfo.Name; }
                Debug.WriteLine(s);

                s = "Runtime fields of PdfPages.Keys:";
                foreach (var fieldInfo in typeof(PdfPages.Keys).GetRuntimeFields()) { s += " " + fieldInfo.Name; }
                Debug.WriteLine(s);
            }
        }
#endif
        /// <summary>
        /// Gets the KeyDescriptor of the specified key, or null if no such descriptor exits.
        /// </summary>
        public KeyDescriptor? this[string key]
        {
            get
            {
                _keyDescriptors.TryGetValue(key, out var keyDescriptor);
                return keyDescriptor;
            }
        }

        readonly Dictionary<string, KeyDescriptor> _keyDescriptors = new();
    }
}
