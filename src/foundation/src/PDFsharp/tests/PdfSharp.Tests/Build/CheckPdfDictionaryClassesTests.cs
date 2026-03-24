// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Internal;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using PdfSharp.Pdf.Advanced;
using Xunit;

namespace PdfSharp.Tests.Build
{
    [Collection("PDFsharp")]
    public class CheckPdfDictionaryClassesTests

    {
        //[Fact(Skip = "Skip for now.")]
        [Fact]
        public void Check_classes_derived_from_PdfDictionary()
        {
            var classList = FindAllDerivedTypes<PdfDictionary>();

            var sbOK = new StringBuilder();
            var sbPublic = new StringBuilder();
            var sbNotFound = new StringBuilder();
            var sbClassNotPublic = new StringBuilder();

            int notFound = 0;
            int notNotPublic = 0;
            int classNotPublic = 0;

            foreach (var type in classList)
            {
                if (!type.IsPublic)
                {
                    sbClassNotPublic.AppendLine(type.Name);
                    ++classNotPublic;
                }

                // Try to get constructor with signature 'Ctor(PdfDictionary)'.
                var ctorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, [typeof(PdfDictionary)], null);

                var ctors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var found = false;

                if (ctorInfo != null)
                {
                    found = true;
                    if (ctorInfo.IsPublic)
                    {
                        sbPublic.AppendLine(type.Name);
                        ++notNotPublic;
                    }
                    else
                        sbOK.AppendLine(type.Name);
                }

                foreach (var ctor in ctors)
                {
                    var param = ctor.GetParameters();
                    if (param.Length == 1 && param[0].ParameterType == typeof(PdfDictionary))
                    {
                        found = true;
                        if (ctor.IsPublic)
                        {
                            sbPublic.AppendLine(type.Name);
                            ++notNotPublic;
                        }
                        else
                            sbOK.AppendLine(type.Name);
                    }
                }

                if (!found)
                {
                    sbNotFound.AppendLine(type.Name);
                    ++notFound;
                }
            }

            var list1 = sbOK.ToString();
            var list2 = sbPublic.ToString();
            var list3 = sbNotFound.ToString();
            var list4 = sbClassNotPublic.ToString();
            notFound.Should().Be(0, "All classes derived from PdfDictionary should have a c'tor that takes a PdfDictionary.");
            notNotPublic.Should().Be(0, "All classes derived from PdfDictionary should have an internal c'tor that takes a PdfDictionary.");
            classNotPublic.Should().Be(0, "All classes derived from PdfDictionary should be public.");
        }

        public static List<Type> FindAllDerivedTypes<T>()
        {
            return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T))!);
        }

        public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            var baseType = typeof(T);
            return assembly.GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t)).ToList();
        }
    }
}
