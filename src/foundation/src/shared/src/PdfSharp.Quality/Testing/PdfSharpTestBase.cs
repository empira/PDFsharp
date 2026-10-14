// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Quality.Testing
{
    public abstract class PdfSharpTestBase : IDisposable
    {
        protected PdfSharpTestBase()
        { }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        { }

        protected static string GetTempRoot(Type type) => $"unit-tests/{type.Namespace}/{type.Name}/";

        protected const string WindowsFontsPath = "C:/Windows/Fonts/";
    }
}
