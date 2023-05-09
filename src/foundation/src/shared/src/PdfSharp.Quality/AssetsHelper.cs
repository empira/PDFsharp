// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Quality
{
    /// <summary>
    /// Static helper functions for fonts.
    /// </summary>
    public static class AssetsHelper
    {
        /// <summary>
        /// Gets the assets folder or null, if no such folder exists.
        /// </summary>
        /// <param name="path">The path or null, if the function should use the current folder.</param>
        public static string? GetAssetsFolder(string? path = null)
        {
            path ??= Directory.GetCurrentDirectory();

            while (true)
            {
                var result = Path.Combine(path, "assets");
                if (Directory.Exists(result))
                    return result;

                var dirInfo = Directory.GetParent(path);
                if (dirInfo == null)
                    break;
            }
            return null;
        }

        /// <summary>
        /// Checks whether the assets folder exists and throws an exception if not.
        /// </summary>
        /// <param name="path">The path or null, if the function should use the current folder.</param>
        public static void EnsureAssets(string? path = null)
        {
            var assets = GetAssetsFolder(path);
            if (assets != null)
            {
                // Check if folder is not empty.
                using var items = Directory.EnumerateFileSystemEntries(assets).GetEnumerator();
                if (items.MoveNext())
                    return;
            }
            throw new InvalidOperationException(
                "The assets folder does not exist or is empty. " +
                @"Maybe you missed to call '.\dev\download-assets.ps1' in the solution root folder.");
        }
    }
}
