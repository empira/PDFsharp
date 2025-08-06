//// PDFsharp - A .NET library for processing PDF
//// PDFsharp - A .NET library for processing PDF
//// See the LICENSE file in the solution root for more information.

//#pragma warning disable 1591 // Because this is preview code.

//using System.Diagnostics.Contracts;

//namespace PdfSharp.Internal
//{
//    /// <summary>
//    /// (PDFsharp) System message.
//    /// </summary>
//    // ReSharper disable once InconsistentNaming
//    // ReSharper disable once IdentifierTypo
//    public static class SyMgs
//    {
//        public static string IndexOutOfRange3
//            => "Index out of range.";

//        public static SyMsg IndexOutOfRange2<T>(string parameter, T lowerBound, T upperBound)
//            => new(SyMsgId.IndexOutOfRange,
//            $"The value of '{parameter}' is out of range. " +
//                   Invariant($"The value must be between '{lowerBound}' and '{upperBound}'."));
//    }
//}