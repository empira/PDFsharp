// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfSharp.Quality
{
    /// <summary>
    /// Helper class for file paths.
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Builds a path by the following strategy. Get the current directory and find the specified folderName in it.
        /// Then replace the path right of folderName with the specified subPath.
        /// </summary>
        /// <param name="folderName">Name of a parent folder in the path to the current directory.</param>
        /// <param name="subPath">The sub path that substitutes the part right of folderName in the current directory path.</param>
        /// <returns>The new path.</returns>
        public static string FindPath(string folderName, string subPath)
        {
            //System.IO.BinaryWriter x = null;

            string currentDirectory = Directory.GetCurrentDirectory();
            int x = currentDirectory.IndexOf("\\" + folderName + "\\", StringComparison.OrdinalIgnoreCase);
            currentDirectory = Path.Combine(currentDirectory.Substring(0, x + folderName.Length + 1), subPath);
            return currentDirectory;
        }
    }
}
