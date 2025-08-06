// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using System.Security.Cryptography;
using PdfSharp.Pdf.Security;
using Xunit;

namespace PdfSharp.Tests.Security
{
    [Collection("PDFsharp")]
    public class SecurityTests
    {
        //[Fact]  // The tests succeeds and is now skipped.
        [Fact(Skip = "Test only once, because it is slow and does not depend on PDFsharp source code.")]
        public void MD5_creation()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < 25_000; i++)
            {
                TestHashCreation(i);
            }
            return;

            void TestHashCreation(int count)
            {
                byte[] bytes = new byte[count];

                rnd.NextBytes(bytes);

                MD5 md5dotNet = MD5.Create();
                MD5Managed md5Managed = MD5Managed.Create();

                byte[] result1 = md5dotNet.ComputeHash(bytes);
                byte[] result2 = md5Managed.ComputeHash(bytes);

                result1.Should().BeEquivalentTo(result2);
                CompareBytes(result1, result2).Should().BeTrue();
            }

            static bool CompareBytes(byte[] bytes1, byte[] bytes2)
            {
                for (int idx = 0; idx < bytes1.Length; idx++)
                {
                    if (bytes1[idx] != bytes2[idx])
                        return false;
                }
                return true;
            }
        }
    }
}
