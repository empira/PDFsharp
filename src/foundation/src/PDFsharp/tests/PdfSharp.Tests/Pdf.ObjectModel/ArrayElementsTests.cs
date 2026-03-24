// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Quality.Testing.TestModel;
using Xunit;
using FluentAssertions;

namespace PdfSharp.Tests.Pdf.ObjectModel
{
    [Collection("PDFsharp")]
    public class ArrayElementsTests : ObjectModelTestsBase
    {
        // Keep all tests in sync with DictionaryElementsTests.

        [Fact]
        public void Indexer_Tests()
        {
            // === Test indexer ===
            {
                var array = new TestArrayElements();

                var array1 = array.Elements[(int)TestArrayElements.Index.TestArray1];

                var dict1 = array.Elements[(int)TestArrayElements.Index.TestDict1];
            }
        }

        [Fact]
        public void GetValue_Tests()
        {
            // === GetValue with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfItem? item;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                item = array.Elements.GetValue((int)TestArrayElements.Index.TestArray1);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                item = array.Elements.GetValue((int)TestArrayElements.Index.TestDict1);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                item = array.Elements.GetValue((int)TestArrayElements.Index.TestArray1, VCF.NoTransform, typeof(PdfArray));
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                item = array.Elements.GetValue((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfDictionary));
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => array.Elements.GetValue((int)TestArrayElements.Index.TestArray1, VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => array.Elements.GetValue((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetValue with primitives ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfItem? item;

                // Get PDF integer.
                item = array.Elements.GetValue((int)TestArrayElements.Index.Integer);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(PdfInteger));
            }

            // === GetRequiredValue with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfItem item;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                item = array.Elements.GetRequiredValue((int)TestArrayElements.Index.TestArray1);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                item = array.Elements.GetRequiredValue((int)TestArrayElements.Index.TestDict1);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                item = array.Elements.GetRequiredValue((int)TestArrayElements.Index.TestArray1, VCF.NoTransform, typeof(PdfArray));
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                item = array.Elements.GetRequiredValue((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfDictionary));
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => array.Elements.GetRequiredValue((int)TestArrayElements.Index.TestArray1, VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => array.Elements.GetRequiredValue((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetRequiredValue with primitives ===
            // ...

            // === TryGetValue with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once InlineOutVariableDeclaration
                PdfItem? item;
                // ReSharper disable once JoinDeclarationAndInitializer
                bool success;

                // Get PDF array.
                success = array.Elements.TryGetValue((int)TestArrayElements.Index.TestArray1, out item);
                success.Should().BeTrue();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                success = array.Elements.TryGetValue((int)TestArrayElements.Index.TestDict1, out item);
                success.Should().BeTrue();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                success = array.Elements.TryGetValue((int)TestArrayElements.Index.TestArray1, out item, typeof(PdfArray));
                success.Should().BeTrue();
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                success = array.Elements.TryGetValue((int)TestArrayElements.Index.TestDict1, out item, typeof(PdfDictionary));
                success.Should().BeTrue();
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                success = array.Elements.TryGetValue((int)TestArrayElements.Index.TestArray1, out item, typeof(PdfDictionary));
                success.Should().BeFalse();
                item.Should().BeNull();

                // Get PDF dictionary with unsuitable type.
                success = array.Elements.TryGetValue((int)TestArrayElements.Index.TestDict1, out item, typeof(PdfArray));
                success.Should().BeFalse();
                item.Should().BeNull();
            }

            // === GetValue<T> with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfItem? item;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                item = array.Elements.GetValue<TestArray1>((int)TestArrayElements.Index.TestArray1);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                item = array.Elements.GetValue<TestDict1>((int)TestArrayElements.Index.TestDict1);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                item = array.Elements.GetValue<TestArray1>((int)TestArrayElements.Index.TestArray1, VCF.NoTransform);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                item = array.Elements.GetValue<TestDict1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => array.Elements.GetValue<TestDict1>((int)TestArrayElements.Index.TestArray1, VCF.NoTransform);
                //item.Should().BeNull();
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => array.Elements.GetValue<TestArray1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform);
                //item.Should().BeNull();
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetRequiredValue<T> with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfItem? item;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                item = array.Elements.GetRequiredValue<TestArray1>((int)TestArrayElements.Index.TestArray1);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                item = array.Elements.GetRequiredValue<TestDict1>((int)TestArrayElements.Index.TestDict1);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                item = array.Elements.GetRequiredValue<TestArray1>((int)TestArrayElements.Index.TestArray1, VCF.NoTransform);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                item = array.Elements.GetRequiredValue<TestDict1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => array.Elements.GetRequiredValue<TestDict1>((int)TestArrayElements.Index.TestArray1, VCF.NoTransform);
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => array.Elements.GetRequiredValue<TestArray1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform);
                action.Should().Throw<InvalidOperationException>();
            }

            // === TryGetValue<T> with containers ===
            {
                var array = new TestArrayElements();
                //// ReSharper disable once InlineOutVariableDeclaration
                //PdfItem item;
                // ReSharper disable once JoinDeclarationAndInitializer
                bool success;

                // Get PDF array.
                success = array.Elements.TryGetValue<TestArray1>((int)TestArrayElements.Index.TestArray1, out var resultTestArray1);
                success.Should().BeTrue();
                resultTestArray1!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                success = array.Elements.TryGetValue<TestDict1>((int)TestArrayElements.Index.TestDict1, out var resultTestDict1);
                success.Should().BeTrue();
                resultTestDict1!.GetType().Should().Be(typeof(TestDict1));

                //// Get PDF array with existing type.
                //success = array.Elements.TryGetValue<TestArray1>((int)TestArrayElements.Index.TestArray1, out resultTestArray1, typeof(PdfArray));
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                //// Get PDF dictionary with existing type.
                //success = array.Elements.TryGetValue<TestDict1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfDictionary));
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                success = array.Elements.TryGetValue<TestDict1>((int)TestArrayElements.Index.TestArray1, out resultTestDict1);
                success.Should().BeFalse();
                resultTestDict1.Should().BeNull();

                // Get PDF dictionary with unsuitable type.
                success = array.Elements.TryGetValue<TestArray1>((int)TestArrayElements.Index.TestDict1, out resultTestArray1);
                success.Should().BeFalse();
                resultTestArray1.Should().BeNull();
            }
        }

        [Fact]
        public void SetValue_Tests()
        {
            // === GetValue ===
            {
                var array = new TestArrayElements();

                var array1 = array.Elements.GetValue((int)TestArrayElements.Index.TestArray1);

                var dict1 = array.Elements.GetValue((int)TestArrayElements.Index.TestDict1);
            }

            // === SetValue ===
            {
                var array = new TestArrayElements();

                var array1 = array.Elements.GetValue((int)TestArrayElements.Index.TestArray1);

                var dict1 = array.Elements.GetValue((int)TestArrayElements.Index.TestDict1);
            }

            // === GetValue ===
            {
                var array = new TestArrayElements();

                var array1 = array.Elements[(int)TestArrayElements.Index.TestArray1];

                var dict1 = array.Elements[(int)TestArrayElements.Index.TestDict1];
            }
        }


        [Fact]
        public void GetObject_Tests()
        {
            // === GetObject with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfObject? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                //Action action;

                // Get PDF array.
                result = array.Elements.GetObject((int)TestArrayElements.Index.TestArray1);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                result = array.Elements.GetObject((int)TestArrayElements.Index.TestDict1);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                result = array.Elements.GetObject((int)TestArrayElements.Index.TestArray1, VCF.NoTransform, typeof(PdfArray));
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                result = array.Elements.GetObject((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfDictionary));
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                result = array.Elements.GetObject((int)TestArrayElements.Index.TestArray1, VCF.NoTransform, typeof(PdfDictionary));
                result.Should().BeNull();

                // Get PDF dictionary with unsuitable type.
                result = array.Elements.GetObject((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfArray));
                result.Should().BeNull();
            }

            // === GetObject with primitives ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfObject? result;

                // Get PDF integer.
                result = array.Elements.GetObject((int)TestArrayElements.Index.Integer);
                result.Should().BeNull();
            }

            // === GetRequiredObject with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfObject result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                result = array.Elements.GetRequiredObject((int)TestArrayElements.Index.TestArray1);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                result = array.Elements.GetRequiredObject((int)TestArrayElements.Index.TestDict1);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                result = array.Elements.GetRequiredObject((int)TestArrayElements.Index.TestArray1, VCF.NoTransform, typeof(PdfArray));
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                result = array.Elements.GetRequiredObject((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfDictionary));
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => array.Elements.GetRequiredObject((int)TestArrayElements.Index.TestArray1, VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => array.Elements.GetRequiredObject((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetRequiredValue with primitives ===
            // ...

            // === TryGetObjects with containers ===
            // Not exits.

            // === GetObject<T> with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfObject? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                result = array.Elements.GetObject<TestArray1>((int)TestArrayElements.Index.TestArray1);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                result = array.Elements.GetObject<TestDict1>((int)TestArrayElements.Index.TestDict1);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                result = array.Elements.GetObject<TestArray1>((int)TestArrayElements.Index.TestArray1, VCF.NoTransform);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                result = array.Elements.GetObject<TestDict1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => array.Elements.GetObject<TestDict1>((int)TestArrayElements.Index.TestArray1, VCF.NoTransform);
                //item.Should().BeNull();
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => array.Elements.GetObject<TestArray1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform);
                //item.Should().BeNull();
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetRequiredObject<T> with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfObject? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                result = array.Elements.GetRequiredObject<TestArray1>((int)TestArrayElements.Index.TestArray1);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                result = array.Elements.GetRequiredObject<TestDict1>((int)TestArrayElements.Index.TestDict1);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                result = array.Elements.GetRequiredObject<TestArray1>((int)TestArrayElements.Index.TestArray1, VCF.NoTransform);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                result = array.Elements.GetRequiredObject<TestDict1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => array.Elements.GetRequiredObject<TestDict1>((int)TestArrayElements.Index.TestArray1, VCF.NoTransform);
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => array.Elements.GetRequiredObject<TestArray1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform);
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
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfArray? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                result = array.Elements.GetArray((int)TestArrayElements.Index.TestArray1);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                result = array.Elements.GetArray((int)TestArrayElements.Index.TestDict1);
                result.Should().BeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                result = array.Elements.GetArray((int)TestArrayElements.Index.TestArray1, VCF.NoTransform, typeof(PdfArray));
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                action = () => array.Elements.GetArray((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();
                //item.Should().BeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => array.Elements.GetArray((int)TestArrayElements.Index.TestArray1, VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => array.Elements.GetArray((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetArray with primitives ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfArray? result;

                // Get PDF integer.
                result = array.Elements.GetArray((int)TestArrayElements.Index.Integer);
                result.Should().BeNull();
                //item!.GetType().Should().BeNull();
            }

            // === GetRequiredArray with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfArray result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                result = array.Elements.GetRequiredArray((int)TestArrayElements.Index.TestArray1);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                action = () => array.Elements.GetRequiredArray((int)TestArrayElements.Index.TestDict1);
                action.Should().Throw<InvalidOperationException>();
                //item.Should().BeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                result = array.Elements.GetRequiredArray((int)TestArrayElements.Index.TestArray1, VCF.NoTransform, typeof(PdfArray));
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                action = () => array.Elements.GetRequiredArray((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();
                //item.Should().BeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => array.Elements.GetRequiredArray((int)TestArrayElements.Index.TestArray1, VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => array.Elements.GetRequiredArray((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetRequiredArray with primitives ===
            // ...

            // === TryGetArray with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once InlineOutVariableDeclaration
                PdfArray? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                bool success;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                success = array.Elements.TryGetArray((int)TestArrayElements.Index.TestArray1, out result);
                success.Should().BeTrue();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                success = array.Elements.TryGetArray((int)TestArrayElements.Index.TestDict1, out result);
                success.Should().BeFalse();
                result.Should().BeNull();

                // Get PDF array with existing type.
                success = array.Elements.TryGetArray((int)TestArrayElements.Index.TestArray1, out result, typeof(PdfArray));
                success.Should().BeTrue();
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                action = () => array.Elements.TryGetArray((int)TestArrayElements.Index.TestDict1, out result, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();
                //success.Should().BeFalse();
                //result.Should().BeNull();

                // Get PDF array with unsuitable type.
                action = () => array.Elements.TryGetArray((int)TestArrayElements.Index.TestArray1, out result, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();
                //success.Should().BeFalse();
                //result.Should().BeNull();

                // Get PDF dictionary with unsuitable type.
                success = array.Elements.TryGetArray((int)TestArrayElements.Index.TestDict1, out result, typeof(PdfArray));
                success.Should().BeFalse();
                result.Should().BeNull();
            }

            // === GetArray<T> with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfArray? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                result = array.Elements.GetArray<TestArray1>((int)TestArrayElements.Index.TestArray1);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                //// Get PDF dictionary.
                //item = array.Elements.GetArray<TestDict1>((int)TestArrayElements.Index.TestDict1);
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                result = array.Elements.GetArray<TestArray1>((int)TestArrayElements.Index.TestArray1, VCF.NoTransform);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestArray1));

                //// Get PDF dictionary with existing type.
                //item = array.Elements.GetArray<TestDict1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform);
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                //// Get PDF array with unsuitable type.
                //action = () => array.Elements.GetArray<TestDict1>((int)TestArrayElements.Index.TestArray1, VCF.NoTransform);
                ////item.Should().BeNull();
                //action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => array.Elements.GetArray<TestArray1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform);
                //item.Should().BeNull();
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetRequiredArray<T> with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfArray? item;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                item = array.Elements.GetRequiredArray<TestArray1>((int)TestArrayElements.Index.TestArray1);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                //item = array.Elements.GetRequiredArray<TestDict1>((int)TestArrayElements.Index.TestDict1);
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                item = array.Elements.GetRequiredArray<TestArray1>((int)TestArrayElements.Index.TestArray1, VCF.NoTransform);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestArray1));

                //// Get PDF dictionary with existing type.
                //item = array.Elements.GetRequiredArray<TestDict1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform);
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                //// Get PDF array with unsuitable type.
                //action = () => array.Elements.GetRequiredArray<TestDict1>((int)TestArrayElements.Index.TestArray1, VCF.NoTransform);
                //action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => array.Elements.GetRequiredArray<TestArray1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform);
                action.Should().Throw<InvalidOperationException>();
            }

            // === TryGetArray<T> with containers ===
            {
                var array = new TestArrayElements();
                //// ReSharper disable once InlineOutVariableDeclaration
                //PdfItem item;
                // ReSharper disable once JoinDeclarationAndInitializer
                bool success;

                // Get PDF array.
                success = array.Elements.TryGetArray<TestArray1>((int)TestArrayElements.Index.TestArray1, out var resultTestArray1);
                success.Should().BeTrue();
                resultTestArray1!.GetType().Should().Be(typeof(TestArray1));

                //// Get PDF dictionary.
                //success = array.Elements.TryGetArray<TestDict1>((int)TestArrayElements.Index.TestDict1, out var resultTestDict1);
                //success.Should().BeTrue();
                //resultTestDict1!.GetType().Should().Be(typeof(TestDict1));

                //// Get PDF array with existing type.
                //success = array.Elements.TryGetValue<TestArray1>((int)TestArrayElements.Index.TestArray1, out resultTestArray1, typeof(PdfArray));
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                //// Get PDF dictionary with existing type.
                //success = array.Elements.TryGetValue<TestDict1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfDictionary));
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                //// Get PDF array with unsuitable type.
                //success = array.Elements.TryGetArray<TestDict1>((int)TestArrayElements.Index.TestArray1, out var resultTestDict1);
                //success.Should().BeFalse();
                //resultTestDict1.Should().BeNull();

                // Get PDF dictionary with unsuitable type.
                success = array.Elements.TryGetArray<TestArray1>((int)TestArrayElements.Index.TestDict1, out resultTestArray1);
                success.Should().BeFalse();
                resultTestArray1.Should().BeNull();
            }
        }

        [Fact]
        public void GetDictionary_Tests()
        {
            // === GetDictionary with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfDictionary? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                result = array.Elements.GetDictionary((int)TestArrayElements.Index.TestArray1);
                result.Should().BeNull();

                // Get PDF dictionary.
                result = array.Elements.GetDictionary((int)TestArrayElements.Index.TestDict1);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => array.Elements.GetDictionary((int)TestArrayElements.Index.TestArray1, VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                result = array.Elements.GetDictionary((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfDictionary));
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => array.Elements.GetDictionary((int)TestArrayElements.Index.TestArray1, VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => array.Elements.GetDictionary((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetArray with primitives ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfDictionary? result;

                // Get PDF integer.
                result = array.Elements.GetDictionary((int)TestArrayElements.Index.Integer);
                result.Should().BeNull();
                //item!.GetType().Should().Be(typeof(PdfInteger));
            }

            // === GetRequiredDictionary with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfDictionary result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                action = () => array.Elements.GetRequiredDictionary((int)TestArrayElements.Index.TestArray1);
                action.Should().Throw<InvalidOperationException>();
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                result = array.Elements.GetRequiredDictionary((int)TestArrayElements.Index.TestDict1);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF dictionary with existing type.
                action = () => array.Elements.GetRequiredDictionary((int)TestArrayElements.Index.TestArray1, VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                result = array.Elements.GetRequiredDictionary((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfDictionary));
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF dictionary with unsuitable type.
                action = () => array.Elements.GetRequiredDictionary((int)TestArrayElements.Index.TestArray1, VCF.NoTransform, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();

                // Get PDF dictionary with unsuitable type.
                action = () => array.Elements.GetRequiredDictionary((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
            }

            // === GetRequiredDictionary with primitives ===
            // ...

            // === TryGetDictionary with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once InlineOutVariableDeclaration
                PdfDictionary? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                bool success;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                success = array.Elements.TryGetDictionary((int)TestArrayElements.Index.TestArray1, out result);
                success.Should().BeFalse();
                result.Should().BeNull();

                // Get PDF dictionary.
                success = array.Elements.TryGetDictionary((int)TestArrayElements.Index.TestDict1, out result);
                success.Should().BeTrue();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with existing type.
                action = () => array.Elements.TryGetDictionary((int)TestArrayElements.Index.TestArray1, out result, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
                //success.Should().BeFalse();
                //result.Should().BeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                success = array.Elements.TryGetDictionary((int)TestArrayElements.Index.TestDict1, out result, typeof(PdfDictionary));
                action.Should().Throw<InvalidOperationException>();
                //success.Should().BeTrue();
                //result.Should().NotBeNull();
                //result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                success = array.Elements.TryGetDictionary((int)TestArrayElements.Index.TestArray1, out result, typeof(PdfDictionary));
                success.Should().BeFalse();
                result.Should().BeNull();

                // Get PDF dictionary with unsuitable type.
                action = () => array.Elements.TryGetDictionary((int)TestArrayElements.Index.TestDict1, out result, typeof(PdfArray));
                action.Should().Throw<InvalidOperationException>();
                //success.Should().BeFalse();
                //result.Should().BeNull();
            }

            // === GetDictionary<T> with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfDictionary? item;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                //// Get PDF array.
                //item = array.Elements.GetDictionary<TestArray1>((int)TestArrayElements.Index.TestArray1);
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                item = array.Elements.GetDictionary<TestDict1>((int)TestArrayElements.Index.TestDict1);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                //// Get PDF array with existing type.
                //item = array.Elements.GetDictionary<TestArray1>((int)TestArrayElements.Index.TestArray1, VCF.NoTransform);
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                item = array.Elements.GetDictionary<TestDict1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform);
                item.Should().NotBeNull();
                item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => array.Elements.GetDictionary<TestDict1>((int)TestArrayElements.Index.TestArray1, VCF.NoTransform);
                //item.Should().BeNull();
                action.Should().Throw<InvalidOperationException>();

                //// Get PDF dictionary with unsuitable type.
                //action = () => array.Elements.GetDictionary<TestArray1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform);
                ////item.Should().BeNull();
                //action.Should().Throw<InvalidOperationException>();
            }

            // === GetRequiredDictionary<T> with containers ===
            {
                var array = new TestArrayElements();
                // ReSharper disable once JoinDeclarationAndInitializer
                PdfDictionary? result;
                // ReSharper disable once JoinDeclarationAndInitializer
                Action action;

                // Get PDF array.
                //item = array.Elements.GetRequiredDictionary<TestArray1>((int)TestArrayElements.Index.TestArray1);
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                result = array.Elements.GetRequiredDictionary<TestDict1>((int)TestArrayElements.Index.TestDict1);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                //// Get PDF array with existing type.
                //item = array.Elements.GetRequiredDictionary<TestArray1>((int)TestArrayElements.Index.TestArray1, VCF.NoTransform);
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary with existing type.
                result = array.Elements.GetRequiredDictionary<TestDict1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform);
                result.Should().NotBeNull();
                result!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                action = () => array.Elements.GetRequiredDictionary<TestDict1>((int)TestArrayElements.Index.TestArray1, VCF.NoTransform);
                action.Should().Throw<InvalidOperationException>();

                //// Get PDF dictionary with unsuitable type.
                //action = () => array.Elements.GetRequiredDictionary<TestArray1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform);
                //action.Should().Throw<InvalidOperationException>();
            }

            // === TryGetDictionary<T> with containers ===
            {
                var array = new TestArrayElements();
                //// ReSharper disable once InlineOutVariableDeclaration
                //PdfDictionary item;
                // ReSharper disable once JoinDeclarationAndInitializer
                bool success;

                //// Get PDF array.
                //success = array.Elements.TryGetDictionary<TestArray1>((int)TestArrayElements.Index.TestArray1, out var resultTestArray1);
                //success.Should().BeTrue();
                //resultTestArray1!.GetType().Should().Be(typeof(TestArray1));

                // Get PDF dictionary.
                success = array.Elements.TryGetDictionary<TestDict1>((int)TestArrayElements.Index.TestDict1, out var resultTestDict1);
                success.Should().BeTrue();
                resultTestDict1!.GetType().Should().Be(typeof(TestDict1));

                //// Get PDF array with existing type.
                //success = array.Elements.TryGetValue<TestArray1>((int)TestArrayElements.Index.TestArray1, out resultTestArray1, typeof(PdfArray));
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestArray1));

                //// Get PDF dictionary with existing type.
                //success = array.Elements.TryGetValue<TestDict1>((int)TestArrayElements.Index.TestDict1, VCF.NoTransform, typeof(PdfDictionary));
                //item.Should().NotBeNull();
                //item!.GetType().Should().Be(typeof(TestDict1));

                // Get PDF array with unsuitable type.
                success = array.Elements.TryGetDictionary<TestDict1>((int)TestArrayElements.Index.TestArray1, out resultTestDict1);
                success.Should().BeFalse();
                resultTestDict1.Should().BeNull();

                //// Get PDF dictionary with unsuitable type.
                //success = array.Elements.TryGetDictionary<TestArray1>((int)TestArrayElements.Index.TestDict1, out var resultTestArray1);
                //success.Should().BeFalse();
                //resultTestArray1.Should().BeNull();
            }
        }
    }
}
