// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Deals with image file names, searches along the image path, checks if images exist etc.
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// Gets the first existing image from the subfolders.
        /// </summary>
        public static string? GetImageName(string root, string filename, string imagePath)
        {
            List<string> subfolders = new(imagePath.Split(new [] { ';' }, StringSplitOptions.RemoveEmptyEntries));
            subfolders.Add("");

            foreach (string subfolder in subfolders)
            {
                string fullname = Path.Combine(Path.Combine(root, subfolder), filename);
                string realFile = ExtractPageNumber(fullname, out _);

                if (File.Exists(realFile))
                    return fullname;
            }
            return null;
        }

        /// <summary>
        /// Gets a value indicating whether the filename given in the referenceFilename exists in the subfolders.
        /// </summary>
        public static bool InSubfolder(string root, string filename, string imagePath, string referenceFilename)
        {
            List<string> subfolders = new List<string>(imagePath.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
            subfolders.Add("");

            foreach (string subfolder in subfolders)
            {
                string fullname = System.IO.Path.Combine(System.IO.Path.Combine(root, subfolder), filename);
                string realFile = ExtractPageNumber(fullname, out _);
                if (System.IO.File.Exists(realFile))
                {
                    if (fullname == referenceFilename)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Extracts the page number if the path has the form 'MyFile.pdf#123' and returns
        /// the actual path without the number sign and the following digits.
        /// </summary>
        public static string ExtractPageNumber(string path, out int pageNumber)
        {
            // Note: duplicated from class XPdfForm.
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            pageNumber = 0;
            int length = path.Length;
            if (length != 0)
            {
                length--;
                if (Char.IsDigit(path, length))
                {
                    while (Char.IsDigit(path, length) && length >= 0)
                        length--;
                    if (length > 0 && path[length] == '#')
                    {
                        // Must have at least one dot left of number sign to distinguish from e.g. '#123'.
                        if (path.IndexOf('.', StringComparison.Ordinal) != -1)
                        {
                            pageNumber = Int32.Parse(path[(length + 1)..]);
                            path = path.Substring(0, length);
                        }
                    }
                }
            }
            return path;
        }
    }
}
