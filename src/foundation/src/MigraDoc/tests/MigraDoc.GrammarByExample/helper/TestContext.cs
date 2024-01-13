// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GdiGrammarByExample;
using PdfSharp.TestHelper;

namespace MigraDoc.GrammarByExample
{
    public class TestContext
    {
        public TestContext()
        {
            var assembly = GetType().Assembly;
            DeploymentDirectory = Path.GetDirectoryName(assembly.GetOriginalLocation())!;
            //Console.WriteLine($"DeploymentDirectory {DeploymentDirectory})");

            var folder0 = @"D:\GBE-Output\";
            Directory.CreateDirectory(DdlGbeTestBase.WslPathHack(folder0));
            var now = DateTime.Now;
            //Console.WriteLine($"Now {now})");
#if GBE
            const string tag = "GBE";
#elif CORE
            const string tag = "Core";
#elif GDI
            const string tag = "GDI";
#elif WPF
            const string tag = "WPF";
#endif
            var folder = $"Testrun-{tag}-" + now.ToString("yyyy-MM-dd_HH-mm-ss");
            OutputDirectory = folder0 + folder;
            //Console.WriteLine($"OutputDirectory {OutputDirectory})");
            Directory.CreateDirectory(DdlGbeTestBase.WslPathHack(OutputDirectory));
            //TempDirectory = OutputDirectory + @"\Temp";
            TempDirectory = DeploymentDirectory;
            //Console.WriteLine($"TempDirectory {TempDirectory})");
            Directory.CreateDirectory(DdlGbeTestBase.WslPathHack(TempDirectory));

            Singleton = this;
        }

        public static TestContext GetOrCreate()
        {
            if (Singleton != null)
                return Singleton;

            return new TestContext();
        }

        public static TestContext? Singleton
        {
            get;
            internal set;
        }

        /// <summary>
        /// Allows tests to register output files.
        /// </summary>
        /// <param name="file">The name of the file.</param>
        /// <returns>The fully qualified path of the file.</returns>
        public string AddResultFileEx(string file)
        {
            //Console.WriteLine($"AddResultFileEx(string {file})");
            var source = Path.GetFileName(file);
            //Console.WriteLine($"    source {source})");
            int idx = source.LastIndexOf('/');
            if (idx >= 0)
                source = source.Substring(idx + 1);
            var result = Path.Combine(OutputDirectory, source);
            //Console.WriteLine($"    result {result})");
            return result;
        }

        public string DeploymentDirectory
        { get; }

        public string TempDirectory
        { get; }

        public string OutputDirectory
        { get; }
    }
}
