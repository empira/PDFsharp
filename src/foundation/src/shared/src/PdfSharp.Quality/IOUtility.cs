// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using System.Reflection;
using System.Runtime.CompilerServices;
using PdfSharp.Internal;

namespace PdfSharp.Quality
{
    /// <summary>
    /// Static utility functions for file IO.
    /// These function are intended for unit test und sample in a solution code only.
    /// </summary>
    public static class IOUtility
    {
        internal const char DirectorySeparatorChar = '\\';
        internal const char AltDirectorySeparatorChar = '/';

        #region Get paths and files

        /// <summary>
        /// True if the given character is a directory separator.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDirectorySeparator(char ch) => ch is DirectorySeparatorChar or AltDirectorySeparatorChar;

        /// <summary>
        /// Replaces all back-slashes with forward slashes in the specified path.
        /// The resulting path works under Windows and Linux if no drive names are
        /// included.
        /// </summary>
        public static void NormalizeDirectorySeparators(ref string? path)
        {
            path = path?.Replace(DirectorySeparatorChar, AltDirectorySeparatorChar);
        }

        /// <summary>
        /// Gets the root path of the current solution, or null, if no parent
        /// directory with a solution file exists.
        /// </summary>
        public static string? GetSolutionRoot()
        {
            if (_solutionRoot is not null)
                return _solutionRoot;

            var path = Directory.GetCurrentDirectory();
            while (true)
            {
                string[] files = Directory.GetFiles(path, "*.sln", SearchOption.TopDirectoryOnly);

                if (files.Length > 0)
                    return _solutionRoot = path;

                var info = Directory.GetParent(path);
                if (info == null)
                    return null;

                path = info.FullName;
            }
        }
        static string? _solutionRoot;

        // R eShar per disable once GrammarMistakeInComment because assets is plural
        /// <summary>
        /// Gets the root path of the current assets directory if no parameter is specified,
        /// or null, if no assets directory exists in the solution root directory.
        /// If a parameter is specified gets the assets root path combined with the specified relative path or file.
        /// If only the root path is returned it always ends with a directory separator.
        /// If a parameter is specified the return value ends literally with value of the parameter.
        /// </summary>
        public static string? GetAssetsPath(string? relativePathOrFileInAsset = null)
        {
            if (_assetsPath is null)
            {
                var pathRoot = GetSolutionRoot();
                if (pathRoot == null)
                    return null;
                _assetsPath = Path.Combine(pathRoot, "assets" + Path.DirectorySeparatorChar);
            }

            if (String.IsNullOrEmpty(relativePathOrFileInAsset))
                return _assetsPath;

            // Just combine what we get. We do not add a directory separator because we
            // cannot know whether it is a path or a file.
            var path = Path.Combine(_assetsPath, relativePathOrFileInAsset);

            return path;
        }
        static string? _assetsPath;

        /// <summary>
        /// Gets the root or sub path of the current temp directory.
        /// The directory is created if it does not exist.
        /// If a valid path is returned it always ends with the current directory separator.
        /// </summary>
        public static string? GetTempPath(string? relativeDirectoryInTemp = null)
        {
            if (_tempPath is null)
            {
                // ??? // '/' is important to work correctly under Linux.
                _tempPath = GetAssetsPath() + @"temp" + Path.DirectorySeparatorChar;
                if (!Directory.Exists(_tempPath))
                    Directory.CreateDirectory(_tempPath);
            }

            if (String.IsNullOrEmpty(relativeDirectoryInTemp))
                return _tempPath;

            var path = Path.Combine(_tempPath, relativeDirectoryInTemp);
            // Ensure path ends with valid directory separator.
            switch (path[^1])
            {
                case '/':
                    break;

                case '\\':
                    // We don’t know here if we are under Windows or Linux.
                    if (Path.DirectorySeparatorChar != '\\')
                        path = path[..^1] + Path.DirectorySeparatorChar;
                    break;

                default:
                    path += Path.DirectorySeparatorChar;
                    break;
            }

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
        static string? _tempPath;

        /// <summary>
        /// Gets the viewer watch directory.
        /// Which is currently just a hard-coded directory on drive C or /mnt/c
        /// </summary>
        public static string GetViewerWatchDirectory()
        {
            // Q&D
            if (Capabilities.OperatingSystem.IsWindows)
            {
                return @"c:\PDFsharpViewer";
            }
            else
            {
                return @"/mnt/c/PDFsharpViewer";
            }
        }

        /// <summary>
        /// Creates a temporary file name with the pattern '{namePrefix}-{WIN|WSL|LNX|...}-{...uuid...}_temp.{extension}'.
        /// The name ends with '_temp.' to make it easy to delete it using the pattern '*_temp.{extension}'.
        /// No file is created by this function. The name contains 10 hex characters to make it unique.
        /// It is not tested whether the file exists, because we have no path here.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static string GetTempFileName(string? namePrefix, string? extension, bool addInfo = true)
        {
            var uuid = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
            var os = Capabilities.OperatingSystem.OSAbbreviation;
            var build = Capabilities.Build.BuildName;
            var framework = Capabilities.Build.Framework;

            var info = $"{os}-{build}-{framework}";

            var file = String.IsNullOrEmpty(namePrefix)
                ? addInfo
                    ? $"{info}-{uuid}_temp"
                    : $"{uuid}_temp"
                : addInfo
                    ? $"{namePrefix}-{info}-{uuid}_temp"
                    : $"{namePrefix}-{uuid}_temp";

            if (!String.IsNullOrEmpty(extension))
                file += "." + extension;

            return file;
        }

        /// <summary>
        /// Creates a temporary file and returns the full name. The name pattern is '/.../temp/.../{namePrefix}-{WIN|WSL|LNX|...}-{...uuid...}_temp.{extension}'.
        /// The namePrefix may contain a sub-path relative to the temp directory.
        /// The name ends with '_temp.' to make it easy to delete it using the pattern '*_temp.{extension}'.
        /// The file is created and immediately closed. That ensures the returned full file name can be used.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static string GetTempFullFileName(string? namePrefix, string? extension, bool addOS = true)
        {
            // Does not work with .NET 4.72
            //var pathPart = Path.GetDirectoryName(namePrefix); -> Exception in 4.72 if no path part exists.
            //var namePart = Path.GetFileName(namePrefix);

            string? pathPart = null;
            string? namePart = null;
            int length = namePrefix?.Length ?? 0;
            if (length > 0 && namePrefix != null)  // Interesting: System cannot deduce that namePrefix cannot be null if length is greater than 0.
            {
                int idx = length - 1;
                while (idx >= 0 && !IsDirectorySeparator(namePrefix[idx]))
                    idx--;

                if (idx > 0)
                    pathPart = namePrefix[..idx];
                if (idx < length - 1)
                    namePart = namePrefix[(idx + 1)..];
            }

            var tempPath = GetTempPath() ?? throw new IOException("Cannot localize temp directory. Your current directory may not be part of a solution.");

            if (pathPart != null)
            {
                tempPath = Path.Combine(tempPath, pathPart);
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);
            }

            int retryCount = 3;
        Retry:
            var tempFile = GetTempFileName(namePart, extension, addOS);
            tempFile = Path.Combine(tempPath, tempFile);

            try
            {
                // Create always a new file, do not overwrite existing. Therefore, File.Create cannot be used here.
                using var _ = new FileStream(tempFile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
                _.Close();  // Is superfluous.
            }
            catch
            {
                // Prevent endless loop in case of access violation.
                // Try sometimes is easier than tearing apart the seven possible exception types.
                if (retryCount-- > 0)
                    goto Retry;
                throw;
            }
            return tempFile;
        }

        /// <summary>
        /// Finds the latest temporary file in a directory. The pattern of the file is expected
        /// to be '{namePrefix}*_temp.{extension}'.
        /// </summary>
        /// <param name="namePrefix">The prefix of the file name to search for.</param>
        /// <param name="path">The path to search in.</param>
        /// <param name="extension">The file extension of the file.</param>
        /// <param name="recursive">If set to <c>true</c> subdirectories are included in the search.</param>
        public static string? FindLatestTempFile(string? namePrefix, string path, string extension = "tmp",
            bool recursive = false)
        {
            var searchPattern = String.IsNullOrEmpty(namePrefix)
                ? $"*_temp.{extension}"
                : $"{namePrefix}*_temp.{extension}";

            (String? FileName, DateTime? LastWrite) result = default;
            FindFile(searchPattern, ref result, path, recursive);
            return result.FileName;

            static void FindFile(string searchPattern, ref (string? FileName, DateTime? LastWrite) result,
                string directory, bool recursive)
            {
                var files = Directory.GetFiles(directory, searchPattern);
                foreach (var file in files)
                {
                    var lastWrite = File.GetLastAccessTime(file);
                    if (result.LastWrite == null || lastWrite > result.LastWrite)
                    {
                        result.FileName = file;
                        result.LastWrite = lastWrite;
                    }
                }

                if (recursive)
                {
                    var subdirectories = Directory.GetDirectories(directory);
                    foreach (var subdirectory in subdirectories)
                    {
                        FindFile(searchPattern, ref result, subdirectory, recursive);
                    }
                }
            }
        }

        #endregion

        #region assets folder

        const string AssetsVersionFileName = ".assets-version";

        const string AssetsInfo =
            @"Run '.\dev\download-assets.ps1' in solution root directory to download the assets for the repository in question. " +
            "For more information see 'Download assets' " + UrlLiterals.LinkToAssetsDoc;

        /// <summary>
        /// Ensures the assets folder exists in the solution root and an optional specified file
        /// or directory exists. If relativeFileOrDirectory is specified, it is considered to
        /// be a path to a directory if it ends with a directory separator. Otherwise, it is
        /// considered to ba a path to a file.
        /// </summary>
        /// <param name="relativeFileOrDirectory">A relative path to a file or directory.</param>
        /// <param name="requiredAssetsVersion">The minimum of the required assets version.</param>
        public static void EnsureAssets(string? relativeFileOrDirectory = null, int? requiredAssetsVersion = null)
        {
            var assetsPath = GetAssetsPath();
            if (assetsPath != null && Directory.Exists(assetsPath))
            {
                if (!Directory.Exists(assetsPath + "temp"))
                    Directory.CreateDirectory(assetsPath + "temp");

                if (requiredAssetsVersion != null)
                    EnsureAssetsVersion(requiredAssetsVersion.Value);

                if (relativeFileOrDirectory != null)
                {
                    relativeFileOrDirectory = Path.Combine(assetsPath, relativeFileOrDirectory);
                    var pathType = "file";
                    if (relativeFileOrDirectory[^1] is '/' or '\\')
                    {
                        pathType = "directory";
                        if (Directory.Exists(relativeFileOrDirectory))
                            return;
                        throw new IOException(
                            $"The {pathType} '{relativeFileOrDirectory}' does not exist in the assets folder. " +
                            AssetsInfo);
                    }
                    else if (File.Exists(relativeFileOrDirectory))
                        return;

                    throw new IOException(
                        $"The {pathType} '{relativeFileOrDirectory}' does not exist in the assets folder. " +
                        AssetsInfo);
                }
                else
                {
                    var files = Directory.GetDirectories(assetsPath);
                    if (files.Length > 0 && File.Exists(assetsPath + AssetsVersionFileName))
                        return;
                    throw new IOException("The assets folder is not yet downloaded. " + AssetsInfo);
                }
            }

            throw new IOException(@"The assets folder does not exist. " + AssetsInfo);
        }

        /// <summary>
        /// Ensures the assets directory exists in the solution root and its version is at least the specified version.
        /// </summary>
        /// <param name="requiredAssetsVersion">The minimum of the required assets version.</param>
        public static void EnsureAssetsVersion(int requiredAssetsVersion)
        {
            var assetsPath = GetAssetsPath();
            if (assetsPath != null && Directory.Exists(assetsPath))
            {
                var versionFilePath = Path.Combine(assetsPath, AssetsVersionFileName);
                if (File.Exists(versionFilePath))
                {
                    var versionString = File.ReadAllText(versionFilePath);
                    if (Int32.TryParse(versionString, out var assetsVersion))
                    {
                        if (assetsVersion >= requiredAssetsVersion)
                            return;
                        throw new IOException(
                            Invariant(
                                $"The required assets version is {requiredAssetsVersion}, but the current version is just {assetsVersion}. ") +
                            AssetsInfo);
                    }
                }

                throw new IOException(
                    $"The assets version file '{AssetsVersionFileName}' does not exist in the assets folder. " +
                    AssetsInfo);
            }

            throw new IOException(@"The assets folder does not exist. " + AssetsInfo);
        }

        #endregion

#if true_ // Not needed anymore.
        // Path.Join does not exist in .NET Standard 2.0
        static string Join(string path1, string path2)
        {
            if (path1.Length == 0)
                return path2.ToString();
            if (path2.Length == 0)
                return path1.ToString();

            return JoinInternal(path1, path2);

            static string JoinInternal(string first, string second)
            {
                Debug.Assert(first.Length > 0 && second.Length > 0, "Should have dealt with empty paths.");

                bool hasSeparator = IsDirectorySeparator(first[^1]) || IsDirectorySeparator(second[0]);

                return hasSeparator
                    ? String.Concat(first, second)
                    : String.Concat(first, Path.DirectorySeparatorChar.ToString(), second);
            }
        }
#endif
    }
}
