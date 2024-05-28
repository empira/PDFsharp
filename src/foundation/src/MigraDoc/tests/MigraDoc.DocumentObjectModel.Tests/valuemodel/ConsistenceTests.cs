// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Reflection;
using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Tests.Helper;
using Xunit;
using FluentAssertions;

using static MigraDoc.DocumentObjectModel.Border;

namespace MigraDoc.DocumentObjectModel.Tests
{
    [Collection("PDFsharp")]
    public class ConsistenceTests
    {
        public Type[] GetAllDocumentObjectTypes()
        {
            var domTypes = new List<Type>();
            var allTypes = typeof(DocumentObject).Assembly.GetTypes();
            foreach (var type in allTypes)
            {
                if (type.IsSubclassOf(typeof(DocumentObject)) is false)
                    continue;

                if (type.IsAbstract)
                    continue;

                domTypes.Add(type);
            }
            return domTypes.ToArray();
        }

        [Fact]
        public void Check_all_Values_classes()
        {
            var types = GetAllDocumentObjectTypes();
            foreach (var type in types)
            {
                var name = type.Name;
                var valuesClassName = $"{name}Values";
                var valuesType = type.GetNestedType(valuesClassName);
                valuesType.Should().NotBeNull($"the name of the nested type must be by convention '{valuesClassName}'");

                if (valuesType != null)
                {
                    valuesType.IsValueType.Should().BeFalse();
                    valuesType.IsNestedPublic.Should().BeTrue();
                    var xxx = valuesType is { IsAbstract: true, IsSealed: true };
                    (valuesType is { IsAbstract: true, IsSealed: true }).Should().BeFalse();

                }
                else
                {
                    _ = typeof(int);
                }
            }
        }

        [Fact]
        public void Create_all_Metas_and_check_ValueDescriptors()
        {
            var types = GetAllDocumentObjectTypes();
            foreach (var type in types)
            {
                type.IsAbstract.Should().BeFalse();

                var dom = (DocumentObject?)Activator.CreateInstance(type);
                dom.Should().NotBeNull();
                var meta = dom!.Meta;
                meta.Should().NotBeNull();
                var vds = meta.ValueDescriptors;
                vds.Count.Should().BeGreaterThan(3);

                foreach (var vd in vds)
                {
                    Char.IsUpper(vd.ValueName[0]).Should().BeTrue();

                    switch (vd)
                    {
                        case ValueTypeDescriptor vtd:
                            Nullable.GetUnderlyingType(vtd.PropertyInfo.PropertyType).Should().Be(vtd.ValueType,
                                $"the type '{vtd.ValueType.Name}' of property '{vtd.PropertyInfo.Name}' of document object type '{vtd.PropertyInfo.DeclaringType!.Name}' must be nullable");
                            break;

                        case DocumentObjectDescriptor dod:
                        {
                            var constructorInfo = dod.ValueType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                            constructorInfo.Should().NotBeNull();

                            // ??? What can we check here?
                            // We do not check nullability because it is difficult reflection stuff and works anyway.
                            break;
                        }

                        case DocumentObjectCollectionDescriptor docd:
                            // Check element type.
                            break;

                        case ReferenceTypeDescriptor rtd:
                            (rtd.ValueType == typeof(string) || rtd.ValueType == typeof(object)).Should().BeTrue();
                            // We do not check nullability because it is difficult reflection stuff and works anyway.
                            break;

                        default:
                            true.Should().BeFalse("we should not come here");
                            break;
                    }
                }
            }
        }

        [Fact]
        public void Test_DocumentSelfSectionAndParentReferences()
        {
            var doc = TestHelper.CreateTestDocument(TestHelper.TestDocument.ComplexDocument);

            doc.Document.Should().BeNull();
            foreach (var element in doc.GetElementsRecursively())
                doc.Should().Be(element?.Document);

            foreach (var section in doc.Sections.Cast<Section>())
            {
                foreach (var element in section.GetElementsRecursively())
                    section.Should().Be(element?.Section);
            }

            CheckParentReferences(doc);
        }

        void CheckParentReferences(DocumentObject docObj)
        {
            var children = docObj.GetElements();
            foreach (var child in children.Where(x => x is not null).Cast<DocumentObject>())
            {
                docObj.Should().Be(child.Parent?.Parent); // GetElements() skips DocumentObjectCollections like Sections and Cells, therefore we have to compare the children to its grandparents.
                CheckParentReferences(child);
            }
        }
    }
}
