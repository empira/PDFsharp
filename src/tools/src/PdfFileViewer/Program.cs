
using PdfSharp.Quality;

Console.WriteLine("Started...");

var watchDir = IOUtility.GetViewerWatchDirectory();
var copyDir = Path.Combine(watchDir, "copy");

if (!Directory.Exists(watchDir))
    Directory.CreateDirectory(watchDir);

if (!Directory.Exists(copyDir))
    Directory.CreateDirectory(copyDir);

while (true)
{
    var newFiles = Directory.GetFiles(watchDir, "*.pdf");
    foreach (var file in newFiles)
    {
        var name = Path.GetFileName(file);
        Console.WriteLine($"Copy file {name}");
        var fileTo = Path.Combine(copyDir, name);
        File.Move(file, fileTo);
        PdfFileUtility.ShowDocument(fileTo);
    }
    Thread.Sleep(500);
}
