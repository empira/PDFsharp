// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Globalization;
using PdfSharp.TestHelper;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
#if CORE
#endif
using Xunit;
using System.Text.RegularExpressions;
using FluentAssertions;
using PdfSharp.Diagnostics;

namespace MigraDoc.Tests
{
    [Collection("PDFsharp")]
    public class CultureAndRegionTests : IDisposable
    {
        public CultureAndRegionTests()
        {
            PdfSharpCore.ResetAll();
#if CORE
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
#endif
        }

        public void Dispose()
        {
            PdfSharpCore.ResetAll();
        }

        [Fact]
        public void Culture_Region_and_metric()
        {
            //var culture1 = CultureInfo.InvariantCulture;
            //var region1 = new RegionInfo(culture1.Name);
            //var isMetric1 = region1.IsMetric;
        }

        [Fact]
        public void DateTimeTest()
        {
            var englishWeekdays = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            var englishMonths = new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            var amPm = new[] { "AM", "PM" };
            var germanWeekdays = new[] { "Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag", "Sonntag" };
            var germanMonths = new[] { "Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember" };

            // filenamePattern with placeholder for cultureInfo.
            var filenamePattern = IOUtility.GetTempFileName("DateTime{0}", "pdf");

            // Create one result file per cultureInfo.
            var cultureInfos = new[] { null, CultureInfo.GetCultureInfo("en-us"), CultureInfo.GetCultureInfo("de-de") };
            foreach (var cultureInfo in cultureInfos)
            {
                var document = new Document { Culture = cultureInfo };

                var section = document.AddSection();

                section.AddParagraph($"123456: {new DateTime(1, 2, 3, 4, 5, 6).ToString(cultureInfo)}");

                section.AddParagraph($"DateTime.Now: {DateTime.Now.ToString(cultureInfo)}");

                var p = section.AddParagraph("p.AddDateField(): ");
                p.AddDateField();

                p = section.AddParagraph("p.AddDateField(\"f\"): ");
                p.AddDateField("f");

                var pdfFilename = String.Format(filenamePattern, "-" + (cultureInfo?.Name ?? "CurrentCulture"));
                var pdfRenderer = new PdfDocumentRenderer { Document = document };
                pdfRenderer.RenderDocument();

                var pdfDocument = pdfRenderer.PdfDocument;
                pdfDocument.Options.CompressContentStreams = false;
                pdfRenderer.Save(pdfFilename);

                PdfFileUtility.ShowDocumentIfDebugging(pdfFilename);

                // Check PDF file content.
                var streamEnumerator = PdfFileHelper.GetPageContentStreamEnumerator(pdfDocument, 0);

                // Get all text content split by whitespace.
                var texts = new List<string>();
                while (streamEnumerator.Text.MoveAndGetNext(true, out var textInfo))
                    texts.AddRange(textInfo!.Text.Split(' '));

                var isEnUs = cultureInfo?.Name.ToLower() == "en-us";
                var isDeDe = cultureInfo?.Name.ToLower() == "de-de";

                var idx = -1;

                if (isEnUs)
                {
                    texts.Count.Should().Be(19);

                    // 123456: 2/3/0001 4:05:06 AM
                    texts[++idx].Should().Be("123456:");
                    Regex.IsMatch(texts[++idx], "[\\d]{1,2}/[\\d]{1,2}/[\\d]{4}").Should().BeTrue();
                    Regex.IsMatch(texts[++idx], "[\\d]{1,2}:[\\d]{2}:[\\d]{2}").Should().BeTrue();
                    amPm.Should().Contain(texts[++idx]);

                    // DateTime.Now: 10/14/2024 10:40:50 AM
                    texts[++idx].Should().Be("DateTime.Now:");
                    Regex.IsMatch(texts[++idx], "[\\d]{1,2}/[\\d]{1,2}/[\\d]{4}").Should().BeTrue();
                    Regex.IsMatch(texts[++idx], "[\\d]{1,2}:[\\d]{2}:[\\d]{2}").Should().BeTrue();
                    amPm.Should().Contain(texts[++idx]);

                    // p.AddDateField(): 10/14/2024 10:40:50 AM
                    texts[++idx].Should().Be("p.AddDateField\\(\\):");
                    Regex.IsMatch(texts[++idx], "[\\d]{1,2}/[\\d]{1,2}/[\\d]{4}").Should().BeTrue();
                    Regex.IsMatch(texts[++idx], "[\\d]{1,2}:[\\d]{2}:[\\d]{2}").Should().BeTrue();
                    amPm.Should().Contain(texts[++idx]);

                    // p.AddDateField("f"): Monday, October 14, 2024 10:40 AM
                    texts[++idx].Should().Be("p.AddDateField\\(\"f\"\\):");
                    englishWeekdays.Should().Contain(texts[++idx].TrimEnd(','));
                    englishMonths.Should().Contain(texts[++idx]);
                    Regex.IsMatch(texts[++idx], "[\\d]{1,2},").Should().BeTrue();
                    Regex.IsMatch(texts[++idx], "[\\d]{4}").Should().BeTrue();
                    Regex.IsMatch(texts[++idx], "[\\d]{1,2}:[\\d]{2}").Should().BeTrue();
                    amPm.Should().Contain(texts[++idx]);
                }
                else if (isDeDe)
                {
                    texts.Count.Should().Be(15);

                    // 123456: 03.02.0001 04:05:06
                    texts[++idx].Should().Be("123456:");
                    Regex.IsMatch(texts[++idx], "[\\d]{2}.[\\d]{2}.[\\d]{4}").Should().BeTrue();
                    Regex.IsMatch(texts[++idx], "[\\d]{2}:[\\d]{2}:[\\d]{2}").Should().BeTrue();

                    // DateTime.Now: 14.10.2024 10:40:50
                    texts[++idx].Should().Be("DateTime.Now:");
                    Regex.IsMatch(texts[++idx], "[\\d]{2}.[\\d]{2}.[\\d]{4}").Should().BeTrue();
                    Regex.IsMatch(texts[++idx], "[\\d]{2}:[\\d]{2}:[\\d]{2}").Should().BeTrue();

                    // p.AddDateField(): 14.10.2024 10:40:50
                    texts[++idx].Should().Be("p.AddDateField\\(\\):");
                    Regex.IsMatch(texts[++idx], "[\\d]{2}.[\\d]{2}.[\\d]{4}").Should().BeTrue();
                    Regex.IsMatch(texts[++idx], "[\\d]{2}:[\\d]{2}:[\\d]{2}").Should().BeTrue();

                    // p.AddDateField("f"): Montag, 14. Oktober 2024 10:40
                    texts[++idx].Should().Be("p.AddDateField\\(\"f\"\\):");
                    germanWeekdays.Should().Contain(texts[++idx].TrimEnd(','));
                    Regex.IsMatch(texts[++idx], "[\\d]{1,2}").Should().BeTrue();
                    germanMonths.Should().Contain(texts[++idx]);
                    Regex.IsMatch(texts[++idx], "[\\d]{4}").Should().BeTrue();
                    Regex.IsMatch(texts[++idx], "[\\d]{1,2}:[\\d]{2}").Should().BeTrue();
                }
            }
        }
    }
}
