// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Tests
{
    public static class TestConfig
    {
        public static void CheckManually(DateTime date)
        {
            if (date < new DateTime(2025, 11, 9))
                throw new InvalidOperationException("Check this test manually and update the date.");
        }
    }
}
