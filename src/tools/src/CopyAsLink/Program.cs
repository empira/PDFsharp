// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// Years ago, there was a Visual Studio Add-in called VSCommand
// that added files from one project as links to another project.
// The 'Add/Existing Item/Add/Add as Link' is too simple. It adds
// only one directory in a flat hierarchy. So I wrote this tool.
//
// Such links look like these (taken from PDFsharp-gdi project)
//
// <Compile Include="..\PdfSharp\Drawing\XBrush.cs" Link="Drawing\XBrush.cs" />
// <Compile Include="..\PdfSharp\Drawing\XBrushes.cs" Link="Drawing\XBrushes.cs" />
//

using System.Text;

namespace CopyAsLink
{
    static class Program
    {
        // I didn’t manage to create a top-level program in a single thread apartment.
        // A single thread apartment is needed for COM-based clipboard access.
        // The code GPT 4.0 suggested doesn’t work.
        [STAThread]
        static void Main(string[] _)
        {
            var source = @"…\";      // The full source path.
            var prefix = @"..\…";    // The relative path from destination to source.
            var pattern = "*.cs;…";  // The files to be included, separated by ';'.

            // Root on your PC.
            var root = @"D:\repos\emp\PDFsharp\src\foundation\src\";

            // PdfSharp.Charting
            source = root + @"PDFsharp\src\PdfSharp.Charting\";
            prefix = @"..\PdfSharp.Charting\";

            // MigraDoc.Rendering
            //source = root + @"MigraDoc\src\MigraDoc.Rendering\";
            //prefix = @"..\MigraDoc.Rendering\";

            pattern = "*.cs";

            var infos = Directory.GetFiles(source, pattern, SearchOption.AllDirectories);
            Array.Sort(infos);

            var result = new StringBuilder();
            foreach (var info in infos)
            {
                if (info.Contains(@"\obj\"))
                    continue;

                var link = info[source.Length..];
                var include = prefix + link;

                var res = $"    <Compile Include=\"{include}\" Link=\"{link}\" />";

                result.AppendLine(res);

                Console.WriteLine(res);
            }

            Clipboard.SetText(result.ToString());

            Console.WriteLine("Done.");
        }
    }
}
