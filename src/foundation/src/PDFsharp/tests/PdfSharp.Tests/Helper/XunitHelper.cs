// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Xunit
{
    /// <summary>
    /// Helper class used with the Xunit.SkippableFact NuGet package.
    /// </summary>
    public static class SkippableTests
    {
        /// <summary>
        /// Gets a value indicating whether slow unit tests should be skipped.
        /// </summary>
        /// <returns>True if slow tests should be skipped.</returns>
        public static bool SkipSlowTests()
        {
            var env = Environment.GetEnvironmentVariable("PDFsharpTests");
            return String.IsNullOrEmpty(env);
        }

        //            /// <summary>
        //            /// Gets a value indicating whether slow unit tests should be skipped.
        //            /// This method is used for tests that run slow under .NET Framework 4.6.2, but much faster under .NET 6.
        //            /// </summary>
        //            /// <returns>True if slow tests should be skipped.</returns>
        //            public static bool SkipSlowTestsUnderDotNetFramework()
        //            {
        //#if NET6_0_OR_GREATER
        //                return false;
        //#else
        //            return SkipSlowTests();
        //#endif
        //            }
    }
}
