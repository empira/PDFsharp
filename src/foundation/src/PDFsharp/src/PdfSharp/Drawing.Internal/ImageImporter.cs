// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing.Internal
{
    /// <summary>
    /// The class that imports images of various formats.
    /// </summary>
    class ImageImporter
    {
        // TODO Make a singleton!
        /// <summary>
        /// Gets the image importer.
        /// </summary>
        public static ImageImporter GetImageImporter()
        {
            return new ImageImporter();
        }

        ImageImporter()
        {
            _importers.Add(new ImageImporterJpeg());
            _importers.Add(new ImageImporterBmp());
            _importers.Add(new ImageImporterPng());
            // TODO: Special importer for PDF? Or dealt with at a higher level?
        }

        /// <summary>
        /// Imports the image.
        /// </summary>
        public ImportedImage? ImportImage(Stream stream)
        {
            using var helper = new StreamReaderHelper(stream);
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

#if GDI || WPF || CORE
        /// <summary>
        /// Imports the image.
        /// </summary>
        public ImportedImage? ImportImage(string filename)
        {
            ImportedImage? ii;
            using (Stream fs = File.OpenRead(filename))
            {
                ii = ImportImage(fs);
            }
            return ii;
        }
#endif

        readonly List<IImageImporter> _importers = new();
    }
}
