// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GdiGrammarByExample;
using MigraDoc.GrammarByExample;

namespace MigraDoc.GBE_Runner
{
    class Program
    {

        static void Main(string[] args)
        {
            // ReSharper disable once LocalizableElement
            Console.WriteLine("Running GBE Tests." + Environment.OSVersion);
            var sw = new Stopwatch();
            sw.Start();

            var fixture = new GbeFixture();

            DdlGbeTestBase test = new DdlGBE_Index(fixture);
            RunTest(test);

            test = new DdlGBE_Attributes_Border(fixture);
            RunTest(test);

            test = new DdlGBE_Attributes_Color(fixture);
            RunTest(test);

            test = new DdlGBE_Attributes_LineAndFillFormat(fixture);
            RunTest(test);

            test = new DdlGBE_Attributes_Shading(fixture);
            RunTest(test);

            test = new DdlGBE_Attributes_Units(fixture);
            RunTest(test);

            test = new DdlGBE_Barcode(fixture);
            RunTest(test);

            test = new DdlGBE_Chart_Layout(fixture);
            RunTest(test);

            test = new DdlGBE_Chart_Types(fixture);
            RunTest(test);

            test = new DdlGBE_Document_Info(fixture);
            RunTest(test);

            test = new DdlGBE_Document_Style(fixture);
            RunTest(test);

            test = new DdlGBE_Document_Styles(fixture);
            RunTest(test);

            test = new DdlGBE_Image_Layout(fixture);
            RunTest(test);

            test = new DdlGBE_Paragraph_Fields(fixture);
            RunTest(test);

            test = new DdlGBE_Paragraph_Font(fixture);
            RunTest(test);

            test = new DdlGBE_Paragraph_Footnotes(fixture);
            RunTest(test);

            test = new DdlGBE_Paragraph_Hyperlinks(fixture);
            RunTest(test);

            test = new DdlGBE_Paragraph_Hyphenation(fixture);
            RunTest(test);

            test = new DdlGBE_Paragraph_Layout(fixture);
            RunTest(test);

            test = new DdlGBE_Paragraph_OutlineLevel(fixture);
            RunTest(test);

            test = new DdlGBE_Paragraph_PagebreakControl(fixture);
            RunTest(test);

            test = new DdlGBE_Paragraph_Spaces(fixture);
            RunTest(test);

            test = new DdlGBE_Paragraph_SpecialCharacters(fixture);
            RunTest(test);

            test = new DdlGBE_Paragraph_Tabs(fixture);
            RunTest(test);

            test = new DdlGBE_Section_HeaderAndFooter(fixture);
            RunTest(test);

            test = new DdlGBE_Section_HorizontalPagebreak(fixture);
            RunTest(test);

            test = new DdlGBE_Section_MirrorMargins(fixture);
            RunTest(test);

            test = new DdlGBE_Section_PageLayout(fixture);
            RunTest(test);

            test = new DdlGBE_Section_PageNumbering(fixture);
            RunTest(test);

            test = new DdlGBE_Shape_Layout(fixture);
            RunTest(test);

            test = new DdlGBE_Table_CellLayout(fixture);
            RunTest(test);

            test = new DdlGBE_Table_Inheritance(fixture);
            RunTest(test);

            test = new DdlGBE_Table_Layout(fixture);
            RunTest(test);

            test = new DdlGBE_TextFrame_Layout(fixture);
            RunTest(test);

            sw.Stop();
            // ReSharper disable once LocalizableElement
            Console.WriteLine($"Total time: {sw.ElapsedMilliseconds / 1000d}");
        }

        static void RunTest(DdlGbeTestBase test)
        {
            ++_counter;
            // ReSharper disable once LocalizableElement
            Console.WriteLine($"Running test {_counter}: {test.GetType().FullName}");
            var sw = new Stopwatch();
            sw.Start();
            test.RunTest();
            sw.Stop();
            // ReSharper disable once LocalizableElement
            Console.WriteLine($"        {sw.ElapsedMilliseconds / 1000d}");
        }

        static int _counter = 0;
    }
}
