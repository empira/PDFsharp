// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.IO;

namespace PdfSharp.Drawing.Internal
{
    /// <summary>
    /// The class that imports images of various formats.
    /// </summary>
    class ImageImporter
    {
        // TODO_OLD Make a singleton!
        /// <summary>
        /// Gets the image importer.
        /// </summary>
        public static ImageImporter GetImageImporter()  // StL: To what kind of design pattern this function identifies itself?
        {
            return new ImageImporter();
        }

        ImageImporter()
        {
            _importers.Add(new ImageImporterJpeg());
            _importers.Add(new ImageImporterBmp());
            _importers.Add(new ImageImporterPng());
            // TODO_OLD: Special importer for PDF? Or dealt with at a higher level?
        }

        /// <summary>
        /// Imports the image.
        /// </summary>
        public ImportedImage? ImportImage(Stream stream)
        {
            long length = -1;
            try
            {
                length = stream.Length;
            }
            catch (NotSupportedException)
            {
                // We eat this exception.
                // We can handle streams that do not return their length.
            }
            catch (Exception ex)
            {
                // Unexpected exception.
                throw new InvalidOperationException("Cannot determine the length of the stream. Use a stream that supports the Length property. Consider copying the image to a MemoryStream.", ex);
            }

            if (length < -1 || length > Int32.MaxValue)
                throw new InvalidOperationException($"Image files with a size of {length} bytes are not supported. Use image files smaller than 2 GiB.");

            using var helper = new StreamReaderHelper(stream, (int)length);
            return TryImageImport(helper);
        }

#if GDI || WPF || CORE
        /// <summary>
        /// Imports the image.
        /// </summary>
        public ImportedImage? ImportImage(string filename)
        {
            var data = File.ReadAllBytes(filename);

            using var helper = new StreamReaderHelper(data);
            return TryImageImport(helper);
        }

        ImportedImage? TryImageImport(StreamReaderHelper helper)
        {
            // Try all registered importers to see if any of them can handle the image.
            foreach (var importer in _importers)
            {
                helper.Reset();
                var image = importer.ImportImage(helper);
                if (image != null)
                    return image;
            }
            return null;
        }
#endif
        readonly List<IImageImporter> _importers = [];
    }
}
