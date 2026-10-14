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
    [Collection("PDFsharp")]
    public class ObjectModelMetaTests
    {
        [Fact]
        public void Object_transformation_constructor_Test()
        {
            // Not yet a real test. 

            // TODO: Check ctor of all classes derived from PdfDictionary
            // to have the right constructors.
            var assemblies =
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.FullName?.StartsWith("PdfSharp", StringComparison.OrdinalIgnoreCase) ?? false);
            int fromPdfDocumentCounter = 0;
            int fromPdfDictionaryCounter = 0;
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.FullName?.StartsWith("PdfSharp") ?? false)
                {
                    var types = assembly.GetTypes();
                    foreach (Type type in assembly.GetTypes())
                    {
                        ConstructorInfo? ctorInfo;
                        if (type.IsSubclassOf(typeof(PdfDictionary)))
                        {
                            if (type.IsAbstract)
                                continue;

                            ctorInfo = type.GetConstructor(
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                null, [typeof(PdfDocument)], null);
                            if (ctorInfo == null)
                                _ = typeof(int);
                            //ctorInfo.Should().NotBeNull();

                            if (ctorInfo != null)
                                fromPdfDocumentCounter++;

                            ctorInfo = type.GetConstructor(
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                null, [typeof(PdfDictionary)], null);
                            if (ctorInfo == null)
                                _ = typeof(int);
                            //ctorInfo.Should().NotBeNull();

                            if (ctorInfo != null)
                                fromPdfDictionaryCounter++;
                            else
                            {
                                var name = type.FullName;
                                _ = typeof(int);

                            }
                        }
                        else if (type.IsSubclassOf(typeof(PdfArray)))
                        {
                            // TODO
                            _ = typeof(int);

                        }
                    }
                }
            }
        }
    }
}
