// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if NETSTANDARD

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Classic .net framework does not define ModuleInitializerAttribute.
    /// But declaring it in System.Runtime.CompilerServices is sufficient to make it work.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ModuleInitializerAttribute : Attribute { }
}

#endif
