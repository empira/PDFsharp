// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Xunit;
using FluentAssertions;
using PdfSharp.Internal;
using PdfSharp.Quality;
using System;

// ReSharper disable RedundantArgumentDefaultValue because this is unit test code
// ReSharper disable UnusedVariable because this is unit test code
#pragma warning disable IDE0059

namespace Shared.Tests.System
{
    public class MessagesTests
    {
        [Fact]
        public void IndexOutOfRange_Test()
        {
            var action = () =>
            {
                var array = new int[3];
                //throw new IndexOutOfRangeException()
                var msg = SyMsgs.IndexOutOfRange2("index", 0, array.Length - 1);
            };
            //action.Should().Throw<IOException>();

            var array = new int[3];
            //throw new IndexOutOfRangeException()
            var msg = SyMsgs.IndexOutOfRange2("index", 0, array.Length - 1);
            msg.Message.Should().NotBeEmpty();
            int index = 7;
            var ex = new ArgumentOutOfRangeException(nameof(index), index, SyMsgs.IndexOutOfRange3);
        }
    }
}
