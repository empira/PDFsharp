// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Quality.Testing.TestModel;
using Xunit;
using FluentAssertions;

namespace PdfSharp.Tests.Pdf.ObjectModel
{
    [Collection("PDFsharp")]
    public class DictionaryElementsTests : ObjectModelTestsBase
    {
        // Keep all tests in sync with ArrayElementsTests.

        [Fact]
        public void Indexer_Tests()
        {
            // === Test indexer ===
            {
                var dict = new TestDictionaryElements();

                var array1 = dict.Elements["/TestArray1"];

                var dict1 = dict.Elements["/TestDict1"];
            }
        }

        [Fact]
        public void GetValue_Tests()
        {
            // === GetValue with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfItem? item;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                item = dict.Elements.GetValue("/TestArray1");
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                item = dict.Elements.GetValue("/TestDict1");
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                item = dict.Elements.GetValue("/TestArray1", VCF.NoTransform, typeof(PdfArray));
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                item = dict.Elements.GetValue("/TestDict1", VCF.NoTransform, typeof(PdfDictionary));
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => dict.Elements.GetValue("/TestArray1", VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => dict.Elements.GetValue("/TestDict1", VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetValue with primitives ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfItem? item;

                // Get PDF integer.
                item = dict.Elements.GetValue("/Integer");
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(PdfInteger));
            }

            // === GetRequiredValue with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfItem item;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                item = dict.Elements.GetRequiredValue("/TestArray1");
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                item = dict.Elements.GetRequiredValue("/TestDict1");
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                item = dict.Elements.GetRequiredValue("/TestArray1", VCF.NoTransform, typeof(PdfArray));
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                item = dict.Elements.GetRequiredValue("/TestDict1", VCF.NoTransform, typeof(PdfDictionary));
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => dict.Elements.GetRequiredValue("/TestArray1", VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => dict.Elements.GetRequiredValue("/TestDict1", VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetRequiredValue with primitives ===
            // ...

            // === TryGetValue with containers ===
            {
                var array = new TestDictionaryElements();
                // ReSharper disable once InlineOutVariableDeclaration
                PdfItem? item;
                // ReSharper disable once JoinDeclarationAndInitializer
                bool success;

                // Get PDF array.
                success = array.Elements.TryGetValue("/TestArray1", out item);
                success.Should().BeTrue();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                success = array.Elements.TryGetValue("/TestDict1", out item);
                success.Should().BeTrue();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                success = array.Elements.TryGetValue("/TestArray1", out item, typeof(PdfArray));
                success.Should().BeTrue();
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                success = array.Elements.TryGetValue("/TestDict1", out item, typeof(PdfDictionary));
                success.Should().BeTrue();
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                success = array.Elements.TryGetValue("/TestArray1", out item, typeof(PdfDictionary));
                success.Should().BeFalse();
                item.Should().BeNull();

                // Get PDF dictionary with unsuitable type.
                success = array.Elements.TryGetValue("/TestDict1", out item, typeof(PdfArray));
                success.Should().BeFalse();
                item.Should().BeNull();
            }

            // === GetValue<T> with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfItem? item;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                item = dict.Elements.GetValue<TestArray1>("/TestArray1");
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                item = dict.Elements.GetValue<TestDict1>("/TestDict1");
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                item = dict.Elements.GetValue<TestArray1>("/TestArray1", VCF.NoTransform);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                item = dict.Elements.GetValue<TestDict1>("/TestDict1", VCF.NoTransform);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => dict.Elements.GetValue<TestDict1>("/TestArray1", VCF.NoTransform);
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => dict.Elements.GetValue<TestArray1>("/TestDict1", VCF.NoTransform);
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetRequiredValue<T> with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfItem? item;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                item = dict.Elements.GetRequiredValue<TestArray1>("/TestArray1");
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                item = dict.Elements.GetRequiredValue<TestDict1>("/TestDict1");
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                item = dict.Elements.GetRequiredValue<TestArray1>("/TestArray1", VCF.NoTransform);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                item = dict.Elements.GetRequiredValue<TestDict1>("/TestDict1", VCF.NoTransform);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => dict.Elements.GetRequiredValue<TestDict1>("/TestArray1", VCF.NoTransform);
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => dict.Elements.GetRequiredValue<TestArray1>("/TestDict1", VCF.NoTransform);
                action.Should().Throw<InvalidOperationException>();
            }

            // === TryGetValue<T> with containers ===
            {
                var dict = new TestDictionaryElements();
                //// ReSharper disable once InlineOutVariableDeclaration
                //PdfItem item;
                // ReSharper disable once JoinDeclarationAndInitializer
                bool success;

                // Get PDF array.
                success = dict.Elements.TryGetValue<TestArray1>("/TestArray1", out var resultTestArray1);
                success.Should().BeTrue();
                resultTestArray1!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                success = dict.Elements.TryGetValue<TestDict1>("/TestDict1", out var resultTestDict1);
                success.Should().BeTrue();
                resultTestDict1!.GetType().Should().Be(typeof(TestDict1));

                //// Get PDF array with existing type.
                //success = dict.Elements.TryGetValue<TestArray1>("/TestArray1",out resultTestArray1);
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                //// Get PDF dictionary with existing type.
                //success = dict.Elements.TryGetValue("/TestDict1", VCF.NoTransform, typeof(PdfDictionary));
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                success = dict.Elements.TryGetValue<TestArray1>("/TestDict1", out resultTestArray1);
                success.Should().BeFalse();
                resultTestArray1.Should().BeNull();

                // Get PDF dictionary with unsuitable type.
                success = dict.Elements.TryGetValue<TestDict1>("/TestArray1", out resultTestDict1);
                success.Should().BeFalse();
                resultTestDict1.Should().BeNull();
            }
        }

        [Fact]
        public void GetValue_Tests2()
        {


            // === Test array must not be used twice ===
            {
                var array = new TestArray1();
                array.IsIndirect.Should().BeFalse();
                var action = () =>
                {
                    var _ = new TestDict1
                    {
                        Elements =
                        {
                            ["Key1"] = array,
                            ["Key2"] = array
                        }
                    };
                };
                action.Should().Throw<InvalidOperationException>();
            }

        }

        [Fact]
        public void GetObject_Tests()
        {
            // === GetObject with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfObject? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                //Action action;

                // Get PDF array.
                result = dict.Elements.GetObject("/TestArray1");
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                result = dict.Elements.GetObject("/TestDict1");
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                result = dict.Elements.GetObject("/TestArray1", VCF.NoTransform, typeof(PdfArray));
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                result = dict.Elements.GetObject("/TestDict1", VCF.NoTransform, typeof(PdfDictionary));
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                result = dict.Elements.GetObject("/TestArray1", VCF.NoTransform, typeof(PdfDictionary));
                result.Should().BeNull();

                // Get PDF dictionary with unsuitable type.
                result = dict.Elements.GetObject("/TestDict1", VCF.NoTransform, typeof(PdfArray));
                result.Should().BeNull();
            }

            // === GetObject with primitives ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfItem? result;

                // Get PDF integer.
                result = dict.Elements.GetObject("/Integer");
                result.Should().BeNull();
            }

            // === GetRequiredObject with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfItem result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                result = dict.Elements.GetRequiredObject("/TestArray1");
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                result = dict.Elements.GetRequiredObject("/TestDict1");
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                result = dict.Elements.GetRequiredObject("/TestArray1", VCF.NoTransform, typeof(PdfArray));
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                result = dict.Elements.GetRequiredObject("/TestDict1", VCF.NoTransform, typeof(PdfDictionary));
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => dict.Elements.GetRequiredObject("/TestArray1", VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => dict.Elements.GetRequiredObject("/TestDict1", VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetRequiredValue with primitives ===
            // ...

            // === TryGetObjects with containers ===
            // Not exits.

            // === GetObject<T> with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfItem? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                result = dict.Elements.GetObject<TestArray1>("/TestArray1");
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                result = dict.Elements.GetObject<TestDict1>("/TestDict1");
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                result = dict.Elements.GetObject<TestArray1>("/TestArray1", VCF.NoTransform);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                result = dict.Elements.GetObject<TestDict1>("/TestDict1", VCF.NoTransform);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => dict.Elements.GetObject<TestDict1>("/TestArray1", VCF.NoTransform);
                //item.Should().BeNull();
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => dict.Elements.GetObject<TestArray1>("/TestDict1", VCF.NoTransform);
                //item.Should().BeNull();
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetRequiredObject<T> with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfItem? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                result = dict.Elements.GetRequiredObject<TestArray1>("/TestArray1");
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                result = dict.Elements.GetRequiredObject<TestDict1>("/TestDict1");
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                result = dict.Elements.GetRequiredObject<TestArray1>("/TestArray1", VCF.NoTransform);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                result = dict.Elements.GetRequiredObject<TestDict1>("/TestDict1", VCF.NoTransform);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => dict.Elements.GetRequiredObject<TestDict1>("/TestArray1", VCF.NoTransform);
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => dict.Elements.GetRequiredObject<TestArray1>("/TestDict1", VCF.NoTransform);
                action.Should().Throw<InvalidOperationException>();
            }

            // === TryGetValue<T> with containers ===
            // Not exits.
        }


        [Fact]
        public void GetArray_Tests()
        {
            // === GetArray with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfArray? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                result = dict.Elements.GetArray("/TestArray1");
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                result = dict.Elements.GetArray("/TestDict1");
                result.Should().BeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                result = dict.Elements.GetArray("/TestArray1", VCF.NoTransform, typeof(PdfArray));
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                action = () => dict.Elements.GetArray("/TestDict1", VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();
                //item.Should().BeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => dict.Elements.GetArray("/TestArray1", VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => dict.Elements.GetArray("/TestDict1", VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetArray with primitives ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfArray? result;

                // Get PDF integer.
                result = dict.Elements.GetArray("/Integer");
                result.Should().BeNull();
                //item!.GetType().Should().BeNull();
            }

            // === GetRequiredArray with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                var result = dict.Elements.GetRequiredArray("/TestArray1");
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                action = () => dict.Elements.GetRequiredArray("/TestDict1");
                action.Should().Throw<InvalidOperationException>();
                //item.Should().BeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                result = dict.Elements.GetRequiredArray("/TestArray1", VCF.NoTransform, typeof(PdfArray));
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                action = () => dict.Elements.GetRequiredArray("/TestDict1", VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();
                //item.Should().BeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => dict.Elements.GetRequiredArray("/TestArray1", VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => dict.Elements.GetRequiredArray("/TestDict1", VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetRequiredArray with primitives ===
            // ...

            // === TryGetArray with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once InlineOutVariableDeclaration
                PdfArray? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                bool success;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                success = dict.Elements.TryGetArray("/TestArray1", out result);
                success.Should().BeTrue();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                success = dict.Elements.TryGetArray("/TestDict1", out result);
                success.Should().BeFalse();
                result.Should().BeNull();

                // Get PDF array with existing type.
                success = dict.Elements.TryGetArray("/TestArray1", out result, typeof(PdfArray));
                success.Should().BeTrue();
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                action = () => dict.Elements.TryGetArray("/TestDict1", out result, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();
                //success.Should().BeFalse();
                //result.Should().BeNull();

                // Get PDF array with unsuitable type.
                action = () => dict.Elements.TryGetArray("/TestArray1", out result, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();
                //success.Should().BeFalse();
                //result.Should().BeNull();

                // Get PDF dictionary with unsuitable type.
                success = dict.Elements.TryGetArray("/TestDict1", out result, typeof(PdfArray));
                success.Should().BeFalse();
                result.Should().BeNull();
            }

            // === GetArray<T> with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfArray? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                result = dict.Elements.GetArray<TestArray1>("/TestArray1");
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                //// Get PDF dictionary.
                //item = array.Elements.GetArray<TestDict1>("/TestDict1");
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                result = dict.Elements.GetArray<TestArray1>("/TestArray1", VCF.NoTransform);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                //// Get PDF dictionary with existing type.
                //item = array.Elements.GetArray<TestDict1>("/TestDict1", VCF.NoTransform);
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                //// Get PDF array with unsuitable type.
                //action = () => array.Elements.GetArray<TestDict1>("/TestArray1", VCF.NoTransform);
                ////item.Should().BeNull();
                //action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => dict.Elements.GetArray<TestArray1>("/TestDict1", VCF.NoTransform);
                //item.Should().BeNull();
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetRequiredArray<T> with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfArray? item;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                item = dict.Elements.GetRequiredArray<TestArray1>("/TestArray1");
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                //item = array.Elements.GetRequiredArray<TestDict1>("/TestDict1");
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                item = dict.Elements.GetRequiredArray<TestArray1>("/TestArray1", VCF.NoTransform);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                //// Get PDF dictionary with existing type.
                //item = array.Elements.GetRequiredArray<TestDict1>("/TestDict1", VCF.NoTransform);
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                //// Get PDF array with unsuitable type.
                //action = () => array.Elements.GetRequiredArray<TestDict1>("/TestArray1", VCF.NoTransform);
                //action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => dict.Elements.GetRequiredArray<TestArray1>("/TestDict1", VCF.NoTransform);
                action.Should().Throw<InvalidOperationException>();
            }

            // === TryGetArray<T> with containers ===
            {
                var dict = new TestDictionaryElements();
                //// ReSharper disable once InlineOutVariableDeclaration
                //PdfItem item;
                // ReSharper disable once JoinDeclarationAndInitializer
                bool success;

                // Get PDF array.
                success = dict.Elements.TryGetArray<TestArray1>("/TestArray1", out var resultTestArray1);
                success.Should().BeTrue();
                resultTestArray1!.GetType().Should().Be(typeof(TestArray1));

                //// Get PDF dictionary.
                //success = array.Elements.TryGetArray<TestDict1>("/TestDict1", out var resultTestDict1);
                //success.Should().BeTrue();
                //resultTestDict1!.GetType().Should().Be(typeof(TestDict1));

                //// Get PDF array with existing type.
                //success = array.Elements.TryGetValue<TestArray1>("/TestArray1", out resultTestArray1, typeof(PdfArray));
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                //// Get PDF dictionary with existing type.
                //success = array.Elements.TryGetValue<TestDict1>("/TestDict1", VCF.NoTransform, typeof(PdfDictionary));
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                //// Get PDF array with unsuitable type.
                //success = array.Elements.TryGetArray<TestDict1>("/TestArray1", out var resultTestDict1);
                //success.Should().BeFalse();
                //resultTestDict1.Should().BeNull();

                // Get PDF dictionary with unsuitable type.
                success = dict.Elements.TryGetArray<TestArray1>("/TestDict1", out resultTestArray1);
                success.Should().BeFalse();
                resultTestArray1.Should().BeNull();
            }
        }

        [Fact]
        public void GetDictionary_Tests()
        {
            // === GetDictionary with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfDictionary? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                result = dict.Elements.GetDictionary("/TestArray1");
                result.Should().BeNull();

                // Get PDF dictionary.
                result = dict.Elements.GetDictionary("/TestDict1");
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => dict.Elements.GetDictionary("/TestArray1", VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                result = dict.Elements.GetDictionary("/TestDict1", VCF.NoTransform, typeof(PdfDictionary));
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => dict.Elements.GetDictionary("/TestArray1", VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => dict.Elements.GetDictionary("/TestDict1", VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetArray with primitives ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfDictionary? result;

                // Get PDF integer.
                result = dict.Elements.GetDictionary("/Integer");
                result.Should().BeNull();
                //item!.GetType().Should().Be(typeof(PdfInteger));
            }

            // === GetRequiredDictionary with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfDictionary result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                action = () => dict.Elements.GetRequiredDictionary("/TestArray1");
                action.Should().Throw<InvalidOperationException>();
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                result = dict.Elements.GetRequiredDictionary("/TestDict1");
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF dictionary with existing type.
                action = () => dict.Elements.GetRequiredDictionary("/TestArray1", VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                result = dict.Elements.GetRequiredDictionary("/TestDict1", VCF.NoTransform, typeof(PdfDictionary));
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF dictionary with unsuitable type.
                action = () => dict.Elements.GetRequiredDictionary("/TestArray1", VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => dict.Elements.GetRequiredDictionary("/TestDict1", VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetRequiredDictionary with primitives ===
            // ...

            // === TryGetDictionary with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once InlineOutVariableDeclaration
                PdfDictionary? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                bool success;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                success = dict.Elements.TryGetDictionary("/TestArray1", out result);
                success.Should().BeFalse();
                result.Should().BeNull();

                // Get PDF dictionary.
                success = dict.Elements.TryGetDictionary("/TestDict1", out result);
                success.Should().BeTrue();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                action = () => dict.Elements.TryGetDictionary("/TestArray1", out result, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
                //success.Should().BeFalse();
                //result.Should().BeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                success = dict.Elements.TryGetDictionary("/TestDict1", out result, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();
                //success.Should().BeTrue();
                //result.Should().NotBeNull();
                //result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                success = dict.Elements.TryGetDictionary("/TestArray1", out result, typeof(PdfDictionary));
                success.Should().BeFalse();
                result.Should().BeNull();

                // Get PDF dictionary with unsuitable type.
                action = () => dict.Elements.TryGetDictionary("/TestDict1", out result, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
                //success.Should().BeFalse();
                //result.Should().BeNull();
            }

            // === GetDictionary<T> with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfDictionary? item;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                //// Get PDF array.
                //item = array.Elements.GetDictionary<TestArray1>("/TestArray1");
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                item = dict.Elements.GetDictionary<TestDict1>("/TestDict1");
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                //// Get PDF array with existing type.
                //item = array.Elements.GetDictionary<TestArray1>("/TestArray1", VCF.NoTransform);
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                item = dict.Elements.GetDictionary<TestDict1>("/TestDict1", VCF.NoTransform);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => dict.Elements.GetDictionary<TestDict1>("/TestArray1", VCF.NoTransform);
                //item.Should().BeNull();
                action.Should().Throw<InvalidOperationException>();

                //// Get PDF dictionary with unsuitable type.
                //action = () => array.Elements.GetDictionary<TestArray1>("/TestDict1", VCF.NoTransform);
                ////item.Should().BeNull();
                //action.Should().Throw<InvalidOperationException>();
            }

            // === GetRequiredDictionary<T> with containers ===
            {
                var dict = new TestDictionaryElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfDictionary? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                //item = array.Elements.GetRequiredDictionary<TestArray1>("/TestArray1");
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                result = dict.Elements.GetRequiredDictionary<TestDict1>("/TestDict1");
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                //// Get PDF array with existing type.
                //item = array.Elements.GetRequiredDictionary<TestArray1>("/TestArray1", VCF.NoTransform);
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                result = dict.Elements.GetRequiredDictionary<TestDict1>("/TestDict1", VCF.NoTransform);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => dict.Elements.GetRequiredDictionary<TestDict1>("/TestArray1", VCF.NoTransform);
                action.Should().Throw<InvalidOperationException>();

                //// Get PDF dictionary with unsuitable type.
                //action = () => array.Elements.GetRequiredDictionary<TestArray1>("/TestDict1", VCF.NoTransform);
                //action.Should().Throw<InvalidOperationException>();
            }

            // === TryGetDictionary<T> with containers ===
            {
                var dict = new TestDictionaryElements();
                //// ReSharper disable once InlineOutVariableDeclaration
                //PdfDictionary item;
                // ReSharper disable once JoinDeclarationAndInitializer
                bool success;

                //// Get PDF array.
                //success = array.Elements.TryGetDictionary<TestArray1>("/TestArray1", out var resultTestArray1);
                //success.Should().BeTrue();
                //resultTestArray1!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                success = dict.Elements.TryGetDictionary<TestDict1>("/TestDict1", out var resultTestDict1);
                success.Should().BeTrue();
                resultTestDict1!.GetType().Should().Be(typeof(TestDict1));

                //// Get PDF array with existing type.
                //success = array.Elements.TryGetValue<TestArray1>("/TestArray1", out resultTestArray1, typeof(PdfArray));
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                //// Get PDF dictionary with existing type.
                //success = array.Elements.TryGetValue<TestDict1>("/TestDict1", VCF.NoTransform, typeof(PdfDictionary));
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                success = dict.Elements.TryGetDictionary<TestDict1>("/TestArray1", out resultTestDict1);
                success.Should().BeFalse();
                resultTestDict1.Should().BeNull();

                //// Get PDF dictionary with unsuitable type.
                //success = array.Elements.TryGetDictionary<TestArray1>("/TestDict1", out var resultTestArray1);
                //success.Should().BeFalse();
                //resultTestArray1.Should().BeNull();
            }
        }
    }
}
