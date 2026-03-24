// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Diagnostics.CodeAnalysis;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Specifies the type of the key’s value in a dictionary.
    /// </summary>
    [Flags]
    enum KeyType
    {
        // @@@ Review Flags
        Name = 0x000_00001,
        String = 0x0000_0002,
        Boolean = 0x0000_0003,
        Integer = 0x0000_0004,
        Real = 0x0000_0005,
        Date = 0x0000_0006,
        Rectangle = 0x0000_0007,  // IMPROVE: See #US291
        Array = 0x0000_0008,
        Dictionary = 0x0000_0009,
        Stream = 0x0000_000A,
        NumberTree = 0x0000_000B,
        Function = 0x0000_000C,
        TextString = 0x0000_000D,
        ByteString = 0x0000_000E,
        NameTree = 0x0000_000F,
        FileSpecification = 0x0000_0010,

        NameOrArray = 0x0000_0100,
        NameOrDictionary = 0x0000_0200,
        ArrayOrDictionary = 0x0000_0300,
        StreamOrArray = 0x0000_0400,
        StreamOrName = 0x0000_0500,
        StreamOrDictionary = 0x0000_0600,
        ArrayOrNameOrString = 0x0000_0700,
        FunctionOrName = 0x0000_0800,
        Various = 0x00000_0900,
        ArrayOfDictionaries = 0x00000_0A00,
        NameOrByteStringOrArray = 0x0000_0B00, // #US373: TODO Check String, ByteString, TextString - check if we have duplicates.
        StringOrDictionary = 0x0000_0C00,
        TextStringOrStream = 0x0000_0D00,
        TextStringOrTextStream = 0x0000_0E00,
        BooleanOrDictionary = 0x00000_0F00,

        TypeMask = 0x0000_0FFF,

        Optional = 0x0000_1000,
        Required = 0x0000_2000,
        Inheritable = 0x0000_4000,
        MustBeIndirect = 0x0001_0000,
        MustNotBeIndirect = 0x0002_0000,

        DeprecatedIn20 = 0x000F_0000
    }

    /// <summary>
    /// Summary description for KeyInfo.
    /// </summary>
    class KeyInfoAttribute : Attribute
    {
        public KeyInfoAttribute()
        { }

        public KeyInfoAttribute(KeyType keyType)
        {
            //_version = version;
            KeyType = keyType;
        }

        public KeyInfoAttribute(string version, KeyType keyType)
        {
            _version = version;
            KeyType = keyType;
        }

        public KeyInfoAttribute(KeyType keyType,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
            Type objectType)
        {
            //_version = version;
            KeyType = keyType;
            _objectType = objectType;
        }

        public KeyInfoAttribute(string version,
            KeyType keyType,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
            Type objectType)
        {
            //_version = version;
            KeyType = keyType;
            _objectType = objectType;
        }

        public string Version
        {
            get => _version ?? NRT.ThrowOnNull<string>();
            set => _version = value;
        }
        string? _version = "1.0";

        public KeyType KeyType
        {
            get => _entryType;
            set => _entryType = value;
        }
        KeyType _entryType;

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        // ReSharper disable once ConvertToAutoProperty
        public Type? ObjectType
        {
            get => _objectType!;
            set => _objectType = value;
        }
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        Type? _objectType;

        public string FixedValue
        {
            get => _fixedValue!;
            set => _fixedValue = value;
        }
        string? _fixedValue;
    }
}