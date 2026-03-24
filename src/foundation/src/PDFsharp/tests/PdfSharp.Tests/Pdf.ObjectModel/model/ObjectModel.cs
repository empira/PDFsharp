// DELETE (moved)
//// PDFsharp - A .NET library for processing PDF
//// See the LICENSE file in the solution root for more information.

//using PdfSharp.Pdf;

//namespace PdfSharp.Tests.Pdf.ObjectModel_
//{
//    public class TestDict1 : PdfDictionary
//    {
//        public TestDict1()
//        {
//            Elements.Add(Keys.IAmA, new PdfName('/' + nameof(TestDict1)));
//        }

//        public TestDict1(PdfDocument doc, bool createIndirect) : base(doc, createIndirect)
//        { }

//        protected TestDict1(PdfDictionary dict) : base(dict)
//        { }

//        public T? Foo<T>(int x) => default(T);

//        public sealed class Keys : KeysBase
//        {
//            // ReSharper disable InconsistentNaming

//            [KeyInfo(KeyType.String | KeyType.Optional, FixedValue = nameof(TestDict1))]
//            public const string IAmA = "/I_am_a";

//            /// <summary>
//            /// Gets the KeysMeta for these keys.
//            /// </summary>
//            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
//            static DictionaryMeta? _meta;

//            // ReSharper restore InconsistentNaming
//        }

//        /// <summary>
//        /// Gets the KeysMeta of this dictionary type.
//        /// </summary>
//        internal override DictionaryMeta Meta => Keys.Meta;
//    }

//    public class TestDict2 : PdfDictionary
//    {
//        public TestDict2()
//        {
//            Elements.Add("/I_am_a", new PdfName('/' + nameof(TestDict2)));
//        }

//        /// <summary>
//        /// Initializes a new instance of this class using the elements of the specified dictionary.
//        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
//        /// </summary>
//        internal TestDict2(PdfDictionary dict) : base(dict)
//        { }

//        public sealed class Keys : KeysBase
//        {
//            // ReSharper disable InconsistentNaming

//            /// <summary>
//            /// </summary>
//            [KeyInfo(KeyType.Name | KeyType.Optional, FixedValue = "Outlines")]
//            public const string Type = "/Type";

//            /// <summary>
//            /// Gets the KeysMeta for these keys.
//            /// </summary>
//            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
//            static DictionaryMeta? _meta;

//            // ReSharper restore InconsistentNaming
//        }

//        /// <summary>
//        /// Gets the KeysMeta of this dictionary type.
//        /// </summary>
//        internal override DictionaryMeta Meta => Keys.Meta;
//    }

//    public class TestDict3 : PdfDictionary
//    {
//        public TestDict3(PdfDocument doc) : base(doc)
//        {
//            Elements.Add(Keys.IAmA, new PdfName('/' + nameof(TestDict3)));

//            var dict1 = new TestDict1();
//            Elements.Add(Keys.TestDict1, dict1);

//            var dict2 = new TestDict1();
//            doc.Internals.AddObject(dict2);
//            Elements.Add(Keys.TestDict1Ref, dict2);
//        }

//        TestDict3(PdfDictionary dict) : base(dict)
//        { }

//        public sealed class Keys : KeysBase
//        {
//            // ReSharper disable InconsistentNaming

//            [KeyInfo(KeyType.String | KeyType.Optional, FixedValue = nameof(TestDict3))]
//            public const string IAmA = "/I_am_a";

//            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(TestDict1))]
//            public const string TestDict1 = "/TestDict1";

//            [KeyInfo(KeyType.String | KeyType.Optional, typeof(TestDict1))]
//            public const string TestDict1Ref = "/TestDict1Ref";

//            /// <summary>
//            /// Gets the KeysMeta for these keys.
//            /// </summary>
//            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
//            static DictionaryMeta? _meta;

//            // ReSharper restore InconsistentNaming
//        }

//        /// <summary>
//        /// Gets the KeysMeta of this dictionary type.
//        /// </summary>
//        internal override DictionaryMeta Meta => Keys.Meta;
//    }

//    public class TestBaseDict : PdfDictionary
//    {
//        public TestBaseDict(PdfDocument doc) : base(doc)
//        {
//            Elements.Add(Keys.IAmA, new PdfName('/' + nameof(TestDict3)));

//            var dict1 = new TestDict1();
//            Elements.Add(Keys.TestDict1, dict1);

//            var dict2 = new TestDict1();
//            doc.Internals.AddObject(dict2);
//            Elements.Add(Keys.TestDict1Ref, dict2);
//        }

//        protected TestBaseDict(PdfDictionary dict) : base(dict)
//        { }

//        public /*sealed*/ class Keys : KeysBase
//        {
//            // ReSharper disable InconsistentNaming

//            [KeyInfo(KeyType.String | KeyType.Optional, FixedValue = nameof(TestDict3))]
//            public const string IAmA = "/I_am_a";

//            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(TestDict1))]
//            public const string TestDict1 = "/TestDict1";

//            [KeyInfo(KeyType.String | KeyType.Optional, typeof(TestDict1))]
//            public const string TestDict1Ref = "/TestDict1Ref";

//            /// <summary>
//            /// Gets the KeysMeta for these keys.
//            /// </summary>
//            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
//            static DictionaryMeta? _meta;

//            // ReSharper restore InconsistentNaming
//        }

//        /// <summary>
//        /// Gets the KeysMeta of this dictionary type.
//        /// </summary>
//        internal override DictionaryMeta Meta => Keys.Meta;
//    }

//    public class TestDerivedDict : TestBaseDict
//    {
//        public TestDerivedDict(PdfDocument doc) : base(doc)
//        {
//            Elements.Add(Keys.IAmA2, new PdfName('/' + nameof(TestDict3)));

//            var dict1 = new TestDict1();
//            Elements.Add(Keys.TestDict12, dict1);
//            //Elements.Add(TestBaseDict.Keys.TestDict1, dict1);

//            var dict2 = new TestDict1();
//            doc.Internals.AddObject(dict2);
//            Elements.Add(Keys.TestDict1Ref2, dict2);
//        }

//        TestDerivedDict(PdfDictionary dict) : base(dict)
//        { }

//        public new sealed class Keys : TestBaseDict.Keys
//        {
//            // ReSharper disable InconsistentNaming

//            [KeyInfo(KeyType.String | KeyType.Optional, FixedValue = nameof(TestDict3))]
//            public const string IAmA2 = "/I_am_a2";

//            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(TestDict1))]
//            public const string TestDict12 = "/TestDict12";

//            [KeyInfo(KeyType.String | KeyType.Optional, typeof(TestDict1))]
//            public const string TestDict1Ref2 = "/TestDict1Ref2";

//            /// <summary>
//            /// Gets the KeysMeta for these keys.
//            /// </summary>
//            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
//            static DictionaryMeta? _meta;

//            // ReSharper restore InconsistentNaming
//        }

//        /// <summary>
//        /// Gets the KeysMeta of this dictionary type.
//        /// </summary>
//        internal override DictionaryMeta Meta => Keys.Meta;
//    }

//    public class ApiTestDict1 : PdfDictionary
//    {
//        public ApiTestDict1()
//        {
//            Elements.Add(Keys.IAmA, new PdfName('/' + nameof(ApiTestDict1)));
//        }

//        protected ApiTestDict1(PdfDocument doc, bool createIndirect) : base(doc, createIndirect)
//        { }

//        protected ApiTestDict1(PdfDictionary dict) : base(dict)
//        { }

//        public sealed class Keys : KeysBase
//        {
//            // ReSharper disable InconsistentNaming

//            [KeyInfo(KeyType.String | KeyType.Optional, FixedValue = nameof(TestDict1))]
//            public const string IAmA = "/I_am_a";

//            [KeyInfo(KeyType.Boolean | KeyType.Optional)]
//            public const string SomeBoolean = "/SomeBoolean";

//            [KeyInfo(KeyType.Integer | KeyType.Optional)]
//            public const string SomeInteger = "/SomeInteger";

//            // TODO: double, long int, unsigned int?, 

//            [KeyInfo(KeyType.String | KeyType.Optional)]
//            public const string SomeString = "/SomeString";

//            // TODO: XMatrix, etc.

//            [KeyInfo(KeyType.Name | KeyType.Optional)]
//            public const string SomeName = "/SomeName";

//            [KeyInfo(KeyType.Array | KeyType.Optional, typeof(TestArray1))]
//            public const string SomeDirectArray = "/SomeDirectDict";

//            [KeyInfo(KeyType.Array | KeyType.Optional, typeof(TestArray1))]
//            public const string SomeIndirectArray = "/SomeIndirectDict ";

//            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(TestDict1))]
//            public const string SomeDirectDict = "/SomeDirectDict";

//            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(TestDict1))]
//            public const string SomeIndirectDict = "/SomeIndirectDict";

//            /// <summary>
//            /// Gets the KeysMeta for these keys.
//            /// </summary>
//            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
//            static DictionaryMeta? _meta;

//            // ReSharper restore InconsistentNaming
//        }

//        /// <summary>
//        /// Gets the KeysMeta of this dictionary type.
//        /// </summary>
//        internal override DictionaryMeta Meta => Keys.Meta;
//    }

//    public class TestArray1 : PdfArray
//    {
//        public TestArray1()
//        { }

//        public TestArray1(PdfDocument doc, bool createIndirect) : base(doc, createIndirect)
//        { }

//    }

//    public class TestBaseArray : PdfArray
//    {

//    }

//    public class TestDerivedArray : TestBaseArray
//    {
//    }

//    /// <summary>
//    /// Test class for class ArrayElementsTests.
//    /// </summary>
//    public class TestArrayElements : PdfArray
//    {
//        public enum Index
//        {
//            TestArray1,
//            TestDict1,
//            Integer,
//            IntegerObject
//        }

//        public TestArrayElements()
//        {
//            Init();
//        }

//        public TestArrayElements(PdfDocument document, bool createIndirect = false)
//            : base(document, createIndirect)
//        {
//            Init();
//        }

//        void Init()
//        {
//            Elements.Add(new TestArray1());
//            Elements.Add(new TestDict1());
//            Elements.Add(new PdfInteger(-42));

//            if (IsIndirect)
//            {

//            }
//        }
//    }

//    /// <summary>
//    /// Test class for class DictionaryElementsTests.
//    /// </summary>
//    public class TestDictionaryElements : PdfDictionary
//    {
//        public TestDictionaryElements()
//        {
//            Init();
//        }

//        public TestDictionaryElements(PdfDocument document, bool createIndirect = false)
//        : base(document, createIndirect)
//        {
//            Init();
//        }

//        void Init()
//        {
//            Elements.Add("/TestArray1", new TestArray1());
//            Elements.Add("/TestDict1", new TestDict1());
//            Elements.Add("/Integer", new PdfInteger(-42));

//            if (IsIndirect)
//            {

//            }
//        }
//    }
//}
