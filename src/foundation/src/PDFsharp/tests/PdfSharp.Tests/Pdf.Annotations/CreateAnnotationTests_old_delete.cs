// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Globalization;
using System.IO;
using System.Xml.Linq;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using PdfSharp.Drawing.Layout;
using Xunit;
using FluentAssertions;

namespace PdfSharp.Tests.Pdf
{
    [Collection("PDFsharp")]
    public class CreateAnnotationTests
    {
        const string TempRoot = "unittests/annotations/";
    }
}
