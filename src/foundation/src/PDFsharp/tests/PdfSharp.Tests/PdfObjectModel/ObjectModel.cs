// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Xunit;
using FluentAssertions;
using System.Security.Cryptography;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using PdfSharp.TestHelper;
using PdfSharp.Quality;
using System.Reflection;

// TODO: DELETE

namespace PdfSharp.Tests.PdfObjectModel
{
    public class TestDict1 : PdfDictionary
    {
        public TestDict1()
        {
            Elements.Add("/I_am_a", new PdfName('/' + nameof(TestDict1)));
        }

        protected TestDict1(PdfDictionary dict) : base(dict)
        { }

        public T? Foo<T>(int x) => default(T);

        public sealed class Keys : KeysBase
        {
            // ReSharper disable InconsistentNaming

            [KeyInfo(KeyType.String | KeyType.Optional, FixedValue = nameof(TestDict1))]
            public const string IAmA = "/I_am_a";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
            static DictionaryMeta? _meta;

            // ReSharper restore InconsistentNaming
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }

    public class TestDict2 : PdfDictionary
    {
        public TestDict2()
        {
            Elements.Add("/I_am_a", new PdfName('/' + nameof(TestDict2)));
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal TestDict2(PdfDictionary dict) : base(dict)
        { }

        public sealed class Keys : KeysBase
        {
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional, FixedValue = "Outlines")]
            public const string Type = "/Type";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
            static DictionaryMeta? _meta;

            // ReSharper restore InconsistentNaming
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }

    public class TestDict3 : PdfDictionary
    {
        public TestDict3(PdfDocument doc) : base(doc)
        {
            Elements.Add(Keys.IAmA, new PdfName('/' + nameof(TestDict3)));

            var dict1 = new TestDict1();
            Elements.Add(Keys.TestDict1, dict1);

            var dict2 = new TestDict1();
            doc.Internals.AddObject(dict2);
            Elements.Add(Keys.TestDict1Ref, dict2);
        }

        TestDict3(PdfDictionary dict) : base(dict)
        { }

        public sealed class Keys : KeysBase
        {
            // ReSharper disable InconsistentNaming

            [KeyInfo(KeyType.String | KeyType.Optional, FixedValue = nameof(TestDict3))]
            public const string IAmA = "/I_am_a";

            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(TestDict1))]
            public const string TestDict1 = "/TestDict1";

            [KeyInfo(KeyType.String | KeyType.Optional, typeof(TestDict1))]
            public const string TestDict1Ref = "/TestDict1Ref";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
            static DictionaryMeta? _meta;

            // ReSharper restore InconsistentNaming
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }

    public class TestArray1 : PdfArray
    {

    }
}
